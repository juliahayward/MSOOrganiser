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
                public string GameCode { get; set; }
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
                    GameCode = x.e.Game_Code.Substring(0, 2),
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

        /// <summary>
        /// This solution relies on there only ever being two long events required - otherwise
        /// complexity quickly rises up and kills you! The situation we code around is where either
        /// the best or second-best long event is beaten by a short event in the same skill.
        /// </summary>
        public List<PentamindStandingsReportVm.EventScore>
            SelectBestScores(List<PentamindStandingsReportVm.EventScore> allScores, int pentaLong, int pentaTotal)
        {
            var options = new List<List<PentamindStandingsReportVm.EventScore>>();
            var plainScores = SelectBestScores1(allScores, pentaLong, pentaTotal, new string[0]);
            options.Add(plainScores);

            if (pentaLong >= 1)
            {
                var bestBigEvent = allScores.Where(x => x.IsLongSession).OrderByDescending(x => x.Score)
                    .FirstOrDefault();
                if (bestBigEvent != null)
                {
                    options.Add(SelectBestScores1(allScores, pentaLong, pentaTotal, new[] { bestBigEvent.GameCode }));
                }

                if (pentaLong >= 2 && bestBigEvent != null)
                {
                    var secondBest = allScores.Where(x => x.IsLongSession && x.GameCode != bestBigEvent.GameCode)
                        .OrderByDescending(x => x.Score).FirstOrDefault();
                    if (secondBest != null)
                    {
                        options.Add(SelectBestScores1(allScores, pentaLong, pentaTotal, 
                            new[] { secondBest.GameCode }));
                        options.Add(SelectBestScores1(allScores, pentaLong, pentaTotal,
                            new[] { bestBigEvent.GameCode, secondBest.GameCode }));
                    }
                }
            }

            return options.OrderByDescending(x => x.Count).ThenByDescending(x => x.Sum(l => l.Score)).First();
        }

        public List<PentamindStandingsReportVm.EventScore>
            SelectBestScores1(List<PentamindStandingsReportVm.EventScore> allScores, int pentaLong, int pentaTotal,
                IEnumerable<string> skillsNotToUseOnLongSessions)
        {
            List<string> skills = new List<string>();
            var allScoresClone = new List<PentamindStandingsReportVm.EventScore>();
            allScoresClone.AddRange(allScores);

            var selected = new List<PentamindStandingsReportVm.EventScore>();
            for (int i = 0; i < pentaLong; i++)
            {
                var best = allScoresClone.Where(x => x.IsLongSession 
                            && !skills.Contains(x.GameCode)
                            && !skillsNotToUseOnLongSessions.Contains(x.GameCode))
                    .OrderByDescending(x => x.Score).FirstOrDefault();
                if (best != null)
                {
                    allScoresClone.Remove(best);
                    selected.Add(best);
                    skills.Add(best.GameCode);
                }
            }
            var alreadyPicked = selected.Count();
            for (int i = 0; i < pentaTotal - pentaLong; i++)
            {
                var best = allScoresClone.Where(x => !skills.Contains(x.GameCode))
                    .OrderByDescending(x => x.Score).FirstOrDefault();
                if (best != null)
                {
                    allScoresClone.Remove(best);
                    selected.Add(best);
                    skills.Add(best.GameCode);
                }
            }
            return selected.OrderByDescending(x => x.Score).ToList();
        }
    }
}
