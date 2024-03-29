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
    public class GrandPrixStandingsGenerator
    {
        public class GrandPrixStandingsReportVm
        {
            public class EventScore
            {
                public double Score { get; set; }

                public int Rank { get; set; }
                public string GameCode { get; set; }

                public int? GameVariantCode { get; set; }
                public string EventCode { get; set; }
                public string GPCategory { get; set; }
                public override string ToString()
                {
                    return string.Format("{0}: {1:0.00}", EventCode, Score);
                }

                public EventScore Clone()
                {
                    return new EventScore()
                    {
                        Score = Score,
                        GameCode = GameCode,
                        GameVariantCode = GameVariantCode,
                        EventCode = EventCode,
                        GPCategory = GPCategory,
                        Rank = Rank
                    };
                }
            }

            public class ContestantStanding : IContestantStanding
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

                public string TieBreak { get; set; }
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

        public GrandPrixStandingsReportVm GetStandings(int? year, DateTime? date = null)
        {
            DateTime? endOfDay = (date.HasValue) ? date.Value.AddDays(1) : (DateTime?)null;

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.Ruleset == "GrandPrix" && x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);

            var vm = new GrandPrixStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue && x.Event.Pentamind)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .ToList() // TODO This was inserted for Etan's stuff - remove with the subsequent Where
                .Where(ec => (endOfDay == null || ec.e.Event.End < endOfDay))
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var excludedWomen = context.WomenNotInWomensPentaminds
                .Where(x => x.OlympiadId == currentOlympiad.Id)
                .Select(x => x.ContestantId);

            var calc = new GrandPrixMetaScoreCalculator();

            var standings = new List<GrandPrixStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new GrandPrixStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = !r.First().c.Male && !excludedWomen.Contains(r.First().c.Mind_Sport_ID),
                    IsJunior = r.First().c.IsJuniorForOlympiad(currentOlympiad),
                    IsSenior = r.First().c.IsSeniorForOlympiad(currentOlympiad)
                };

                standing.Scores = r.Select(x => new GrandPrixStandingsReportVm.EventScore()
                {
                    EventCode = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    GPCategory = x.e.Event.GPCategory,
                    GameVariantCode = x.e.Event.GameVariantId,
                    Score = (double)x.e.Penta_Score,
                    Rank = x.e.Rank.Value
                }).ToList();

                standing.Scores = calc.SelectEligibleScores(standing.Scores);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Any());
                standing.TieBreak = EvaluateTiebreak(standing.Scores);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore).ThenByDescending(x => x.TieBreak);
            return vm;
        }

        private string EvaluateTiebreak(IEnumerable<GrandPrixStandingsReportVm.EventScore> scores)
        {
            return scores.Count(x => x.Rank == 1).ToString("00")
                + scores.Count(x => x.Rank == 2).ToString("00")
                + scores.Count(x => x.Rank == 3).ToString("00")
                + scores.Count(x => x.Rank == 4).ToString("00");
        }

        public GrandPrixStandingsReportVm GetGPCategoryStandings(string category, int? year = null, DateTime? date = null)
        {
            DateTime? endOfDay = (date.HasValue) ? date.Value.AddDays(1) : (DateTime?)null;

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.Ruleset == "GrandPrix" && x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.First(x => x.Current);

            var vm = new GrandPrixStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .ToList() // TODO This was inserted for Etan's stuff - remove with the subsequent Where
                .Where(ec => ec.e.Event.GPCategory == category)
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var excludedWomen = context.WomenNotInWomensPentaminds
                .Where(x => x.OlympiadId == currentOlympiad.Id)
                .Select(x => x.ContestantId);

            var calc = new GrandPrixMetaScoreCalculator();

            var standings = new List<GrandPrixStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new GrandPrixStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    Nationality = r.First().c.Nationality,
                    IsInWomensPenta = !r.First().c.Male && !excludedWomen.Contains(r.First().c.Mind_Sport_ID),
                    IsJunior = r.First().c.IsJuniorForOlympiad(currentOlympiad),
                    IsSenior = r.First().c.IsSeniorForOlympiad(currentOlympiad)
                };

                standing.Scores = r.Select(x => new GrandPrixStandingsReportVm.EventScore()
                {
                    EventCode = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    GPCategory = x.e.Event.GPCategory,
                    Score = (double)x.e.Penta_Score,
                    Rank = x.e.Rank.Value
                }).ToList();

                standing.Scores = calc.SelectEligibleCategoryScores(standing.Scores);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Any());
                standing.TieBreak = EvaluateTiebreak(standing.Scores);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore).ThenByDescending(x => x.TieBreak);
            return vm;
        }
    }
}
