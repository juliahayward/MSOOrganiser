using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class PentamindStandingsGenerator
    {
        public class PentamindStandingsReportVm
        {
            public class EventScore 
            {
                public double Score { get; set; }
                public string Code { get; set; }
                public bool IsLongSession { get; set; }
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

        public PentamindStandingsReportVm GetStandings()
        {
            var context = new DataEntities();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var vm = new PentamindStandingsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && x.Penta_Score.HasValue && x.Penta_Score.Value > 0)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

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
                    Score = (double)x.e.Penta_Score,
                    IsLongSession = longSessionEvents.Contains(x.e.Game_Code)
                }).ToList();

                standing.Scores = SelectBestScores(standing.Scores, pentaLong, pentaTotal);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        public List<PentamindStandingsReportVm.EventScore>
            SelectBestScores(List<PentamindStandingsReportVm.EventScore> allScores, int pentaLong, int pentaTotal)
        {
            var selected = new List<PentamindStandingsReportVm.EventScore>();
            for (int i = 0; i < pentaLong; i++)
            {
                var best = allScores.Where(x => x.IsLongSession).OrderByDescending(x => x.Score).FirstOrDefault();
                if (best != null)
                {
                    allScores.Remove(best);
                    selected.Add(best);
                }
            }
            var alreadyPicked = selected.Count();
            for (int i = 0; i < pentaTotal - alreadyPicked; i++)
            {
                var best = allScores.OrderByDescending(x => x.Score).FirstOrDefault();
                if (best != null)
                {
                    allScores.Remove(best);
                    selected.Add(best);
                }
            }
            return selected.OrderByDescending(x => x.Score).ToList();
        }
    }
}
