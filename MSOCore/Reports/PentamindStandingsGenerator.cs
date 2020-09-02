using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;
using MSOCore.Calculators;
using System.Linq.Expressions;

namespace MSOCore.Reports
{
    public class PentamindStandingsGenerator
    {
        public class PentamindStandingsReportVm
        {
            public class EventScore
            {
                public double Score { get; set; }
                public string GameCode { get; set; }
                public string Code { get; set; }
                public bool IsLongSession { get; set; }
                public bool IsEuroGame { get; set; }
                public bool IsModernAbstract { get; set; }
                public override string ToString()
                {
                    return string.Format("{0}{1}: {2:0.00}", Code, (IsLongSession ? "*" : ""), Score);
                }
            }

            public class ContestantStanding
            {
                public int ContestantId { get; set; }
                public string Name { get; set; }
                public string Nationality { get; set; }
                public string Flag { get { return Nationality.GetFlag(); } }
                public bool IsInWomensPenta { get; set; }
                public bool IsJunior { get; set; }
                public bool IsSenior { get; set; }
                public string FemaleFlag { get { return (IsInWomensPenta) ? "W" : ""; } }
                public string JuniorFlag { get { return (IsJunior) ? "Jnr" : ""; } }
                public string SeniorFlag { get { return (IsSenior) ? "Snr" : ""; } }
                public double TotalScore { get; set; }
                public string TotalScoreStr { get { return string.Format("{0:0.00}", TotalScore); } }
                public List<EventScore> Scores { get; set; }
                public bool IsValid { get; set; }
                public string ScoreStr(int place)
                {
                    var score = Scores.ElementAtOrDefault(place);
                    return (score != null) ? score.ToString() : "";
                }
                public string GameStr(int place)
                {
                    var score = Scores.ElementAtOrDefault(place);
                    return (score != null) ? score.ToString().Substring(0, 4) : "";
                }
            }

            public string OlympiadTitle { get; set; }
            public IEnumerable<ContestantStanding> Standings { get; set; }

            public bool HeaderRequired { get; set; }
            public int TopNRequired { get; set; }
            public Func<ContestantStanding, bool> StandingsFilter { get; set; }

            public IEnumerable<ContestantStanding> TopNStandings
            {
                get
                {
                    if (StandingsFilter != null)    
                        return Standings.Where(StandingsFilter).Take(TopNRequired);
                    else
                        return Standings.Take(TopNRequired);
                }
            }
            public int EventsNeeded { get; set; }
        }

        public PentamindStandingsReportVm GetStandings(int? year, DateTime? date = null)
        {
            DateTime? endOfDay = (date.HasValue) ? date.Value.AddDays(1) : (DateTime?)null;

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .ToList() // TODO This was inserted for Etan's stuff - remove with the subsequent Where
                .Where(ec => (endOfDay == null || ec.e.Event.End < endOfDay))
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var excludedWomen = context.WomenNotInWomensPentaminds
                .Where(x => x.OlympiadId == currentOlympiad.Id)
                .Select(x => x.ContestantId);

            var calc = new PentamindMetaScoreCalculator();

            var standings = new List<PentamindStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = !r.First().c.Male && !excludedWomen.Contains(r.First().c.Mind_Sport_ID),
                    IsJunior = r.First().c.IsJuniorForOlympiad(currentOlympiad),
                    IsSenior = r.First().c.IsSeniorForOlympiad(currentOlympiad)
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Event.Code),
                    IsEuroGame = (x.e.Event.Game.GameCategory.Id == 3)
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Count() == pentaTotal);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        // Potential for de-duping this code
        public PentamindStandingsReportVm GetJuniorStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);
            var firstDobForJunior = currentOlympiad.FirstDateOfBirthForJunior();

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Where(x => x.c.DateofBirth.HasValue && x.c.DateofBirth.Value >= firstDobForJunior)
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var calc = new PentamindMetaScoreCalculator();

            var standings = new List<PentamindStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = false // irrelevant for junior
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Event.Code),
                    IsEuroGame = (x.e.Event.Game.GameCategory.Id == 3)
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        // Potential for de-duping this code
        public PentamindStandingsReportVm GetSeniorStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);
            var lastDobForSenior = currentOlympiad.LastDateOfBirthForSenior();

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Where(x => x.c.DateofBirth.HasValue && x.c.DateofBirth.Value <= lastDobForSenior)
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var calc = new PentamindMetaScoreCalculator();

            var standings = new List<PentamindStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = false // irrelevant for Senior
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Event.Code),
                    IsEuroGame = (x.e.Event.Game.GameCategory.Id == 3)
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        // Potential for de-duping this code
        public PentamindStandingsReportVm GetEuroStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);

            var def = context.MetaGameDefinitions.First(x =>
                x.Type == "Eurogames" && x.OlympiadId == currentOlympiad.Id);
            var codes = def.SubEvents.Split(',');

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue
                    && codes.Contains(x.Event.Code))
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var calc = new EurogameMetaScoreCalculator();
            bool allDifferentGames = ((year ?? currentOlympiad.YearOf) == 2020);

            var standings = new List<PentamindStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = false // irrelevant for Euro
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = true, // No long game rule in eurogames // longSessionEvents.Contains(x.e.Game_Code),
                    IsEuroGame = (codes.Contains(x.e.Event.Code))
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year, allDifferentGames);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Count() == pentaTotal);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        public PentamindStandingsReportVm GetModernAbstractStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);

            var def = context.MetaGameDefinitions.First(x =>
                x.Type == "ModernAbstract" && x.OlympiadId == currentOlympiad.Id);
            var codes = def.SubEvents.Split(',');

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue
                    && codes.Contains(x.Event.Code))
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var calc = new EurogameMetaScoreCalculator();
            bool allDifferentGames = ((year ?? currentOlympiad.YearOf) == 2020);

            var standings = new List<PentamindStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = false // irrelevant for Modern Abstract
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = true, // No long game rule in eurogames // longSessionEvents.Contains(x.e.Game_Code),
                    IsEuroGame = (x.e.Event.Game.GameCategory.Id == 3),
                    IsModernAbstract = (codes.Contains(x.e.Event.Code))
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year, allDifferentGames);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Count() == pentaTotal);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        public PentamindStandingsReportVm GetPokerStandings(int? year)
        {
            int pentaTotal = (year.HasValue && year < 2020) ? 4 : 5; // Todo read off event
            int pentaLong = 0;  // length irrelevant for poker
            return GetMetaStandings(year, "Poker", pentaTotal, pentaLong);
        }

        public PentamindStandingsReportVm GetChessStandings(int? year)
        {
            if (year < 2020) throw new ArgumentOutOfRangeException("No Chess meta-events before 2020");

            int pentaTotal = 5; // OK for 2020, doesn't apply to other years
            int pentaLong = 0;  // length irrelevant
            return GetMetaStandings(year, "Chess", pentaTotal, pentaLong);
        }

        public PentamindStandingsReportVm GetBackgammonStandings(int? year)
        {
            if (year < 2020) throw new ArgumentOutOfRangeException("No Backgammon meta-events before 2020");

            int pentaTotal = 5; // OK for 2020, doesn't apply to other years
            int pentaLong = 0;  // length irrelevant
            return GetMetaStandings(year, "Backgammon", pentaTotal, pentaLong);
        }

        private PentamindStandingsReportVm GetMetaStandings(int? year, string type, int pentaTotal, int pentaLong)
        { 
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);

            var def = context.MetaGameDefinitions.First(x =>
                x.Type == type && x.OlympiadId == currentOlympiad.Id);
            var codes = def.SubEvents.Split(',');

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            vm.EventsNeeded = pentaTotal;

            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue
                    && codes.Contains(x.Event.Code))
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var calc = new EurogameMetaScoreCalculator();

            var standings = new List<PentamindStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = false // irrelevant for Modern Abstract
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = true, // No long game rule in poker 
                    IsEuroGame = (x.e.Event.Game.GameCategory.Id == 3),
                    IsModernAbstract = (codes.Contains(x.e.Event.Code))
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Count() == pentaTotal);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

    }



}
