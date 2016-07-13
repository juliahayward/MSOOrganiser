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
                public override string ToString()
                {
                    return string.Format("{0}{1}: {2:0.00}", Code, (IsLongSession ? "*" : ""), Score);
                }
            }

            public class ContestantStanding
            {
                public int ContestantId { get; set; }
                public string Name { get; set; }
                public bool IsMale { get; set; }
                public string FemaleFlag { get { return (IsMale) ? "" : "W"; } }
                public double TotalScore { get; set; }
                public string TotalScoreStr { get { return string.Format("{0:0.00}", TotalScore); } }
                public List<EventScore> Scores { get; set; }
                public string ScoreStr(int place)
                {
                    var score = Scores.ElementAtOrDefault(place);
                    return (score != null) ? score.ToString() : "";
                }
            }

            public string OlympiadTitle { get; set; }
            public IEnumerable<ContestantStanding> Standings { get; set; }
        }

        public PentamindStandingsReportVm GetStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

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
                    IsMale = r.First().c.Male
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Game_Code,
                    GameCode = x.e.Game_Code.Substring(0, 2),
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Game_Code),
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
        public PentamindStandingsReportVm GetJuniorStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
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
                    IsMale = r.First().c.Male
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Game_Code,
                    GameCode = x.e.Game_Code.Substring(0, 2),
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Game_Code),
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
                : context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
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
                    IsMale = r.First().c.Male
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Game_Code,
                    GameCode = x.e.Game_Code.Substring(0, 2),
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Game_Code),
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
            year = 2015;

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var lastDobForSenior = currentOlympiad.LastDateOfBirthForSenior();

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue
                    && x.Event.Game.GameCategory.Id == 3)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Where(x => x.c.DateofBirth.HasValue && x.c.DateofBirth.Value <= lastDobForSenior)
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
                    IsMale = r.First().c.Male
                };

                standing.Scores = r.Select(x => new PentamindStandingsReportVm.EventScore()
                {
                    Code = x.e.Game_Code,
                    GameCode = x.e.Game_Code.Substring(0, 2),
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Game_Code),
                    IsEuroGame = (x.e.Event.Game.GameCategory.Id == 3)
                }).ToList();

                standing.Scores = calc.SelectBestScores(standing.Scores, pentaLong, pentaTotal, currentOlympiad.StartDate.Value.Year);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

    }


    
}
