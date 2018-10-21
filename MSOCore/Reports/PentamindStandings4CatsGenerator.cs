using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class PentamindStandings4CatsGenerator
    {
        public class PentamindStandings4CatsReportVm
        {
            public class EventScore 
            {
                internal bool Selected { get; set; }
                public double Score { get; set; }
                public string GameCode { get; set; }
                public string Code { get; set; }
                public int CategoryId { get; set; }
                public override string ToString()
                {
                    return string.Format("{0}({1}): {2:0.00}", Code, CategoryId, Score);
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

        public PentamindStandings4CatsReportVm GetStandings(int? year)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = (year.HasValue)
                ? context.Olympiad_Infoes.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == year.Value).First()
                : context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var vm = new PentamindStandings4CatsReportVm();
            vm.OlympiadTitle = currentOlympiad.FullTitle();
            int pentaTotal = currentOlympiad.PentaTotal.Value;
            int pentaLong = currentOlympiad.PentaLong.Value;
            var longSessionEvents = currentOlympiad.Events.Where(x => x.No_Sessions > 1)
                .Select(x => x.Code)
                .ToList();

            var results = context.Entrants
                .Where(x => x.OlympiadId == currentOlympiad.Id && !x.Absent && x.Rank.HasValue && x.Penta_Score.HasValue)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Where(x => x.e.Game_Code != "EGWC" && x.e.Game_Code != "AGOC" && x.e.Game_Code != "SFOC"
                && x.e.Game_Code != "LPOC")
                .GroupBy(x => x.c.Mind_Sport_ID)
                .ToList();

            var standings = new List<PentamindStandings4CatsReportVm.ContestantStanding>();
            foreach (var r in results)
            {
                var standing = new PentamindStandings4CatsReportVm.ContestantStanding()
                {
                    ContestantId = r.Key,
                    Name = r.First().c.FullName(),
                    IsMale = r.First().c.Male
                };

                standing.Scores = r.Select(x => new PentamindStandings4CatsReportVm.EventScore()
                {
                    Code = x.e.Game_Code,
                    GameCode = x.e.Event.Game.Code,
                    Score = (double)x.e.Penta_Score,
                    CategoryId = x.e.Event.Game.CategoryId.Value
                })
                .OrderByDescending(x => x.Score)
                .ToList();

                standing.Scores = SelectBestScores(standing.Scores, pentaLong, pentaTotal);
                standing.TotalScore = standing.Scores.Sum(x => x.Score);
                standings.Add(standing);
            }

            vm.Standings = standings.OrderByDescending(x => x.TotalScore);
            return vm;
        }

        /// <summary>
        /// This solution relies on the fact that all events for a Game have the same Category.
        /// </summary>
        public List<PentamindStandings4CatsReportVm.EventScore>
            SelectBestScores(List<PentamindStandings4CatsReportVm.EventScore> allScores, int pentaLong, int pentaTotal)
        {
            var selected = new List<PentamindStandings4CatsReportVm.EventScore>();
            // First four picked are different Categories - this also ensures differnt Games
            foreach (var score in allScores)
            {
                if (selected.Count == 4) break;

                if (selected.Any(x => x.CategoryId == score.CategoryId)) continue;
                if (selected.Any(x => x.GameCode == score.GameCode)) continue;
                selected.Add(score);
                score.Selected = true;
            }
            // Fifth is any one not already used in a game not already used
            var fifthEvent = allScores.FirstOrDefault(x => !x.Selected && !selected.Any(s => x.GameCode == s.GameCode));
            if (fifthEvent != null)
                selected.Add(fifthEvent);

            return selected;
        }

    }
}
