using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class PokerStandingsGenerator
    {
        public class PokerStandingsReportVm
        {
            public class EventScore 
            {
                public double Score { get; set; }
                public string GameCode { get; set; }
                public string Code { get; set; }

                public override string ToString()
                {
                    return string.Format("{0}: {1:0.00}", Code, Score);
                }
            }

            public class ContestantStanding
            {
                public int ContestantId { get; set; }
                public string Name { get; set; }
                public bool IsMale { get; set; }
                public string FemaleFlag { get { return (IsMale) ? "" : "W"; } }
                public double TotalScore { get; set; }
                public bool IsValid { get; set; }
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

        public PokerStandingsReportVm GetStandings()
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            // We had five events before the move to JW3, four after that.
            int numAllowed = (currentOlympiad.YearOf < 2013) ? 5 : 4;
            // in 2019 the rules were changed to include POHU


            var vm = new PokerStandingsReportVm();
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

            var standings = new List<PokerStandingsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PokerStandingsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    IsMale = r.First().c.Male
                };

                standing.Scores = r.Select(x => new PokerStandingsReportVm.EventScore()
                {
                    Code = x.e.Event.Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score
                }).ToList();

                standing.Scores = SelectBestScores(currentOlympiad.YearOf.Value, standing.Scores, numAllowed);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standing.IsValid = (standing.Scores.Count() == numAllowed);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        public List<PokerStandingsReportVm.EventScore>
            SelectBestScores(int year, List<PokerStandingsReportVm.EventScore> allScores, int numAllowed)
        {
            // TODO - unpick this logic in the same way as we unpicked the MBWC/EGWC
            var excluded = new List<string>() { "POWC", "POTU" };
            if (year < 2019) excluded.Add("POHU");

            var pokerScores = allScores.Where(x => x.GameCode.StartsWith("PO") && !excluded.Contains(x.GameCode));

            return pokerScores.OrderByDescending(x => x.Score).Take(numAllowed).ToList();
        
        }
    }
}
