using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;
using MSOCore.Reports;

namespace MSOCore.Calculators
{
    public class EurogameMetaScoreCalculator
    {
        public List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore> 
            SelectBestScores(List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore> allScores, 
            int pentaLong, int pentaTotal, int year)
        {
            if (pentaTotal == 0) return new List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore>();

            var best = SelectBestScores1(allScores, pentaLong, pentaTotal, year);
            if (best != null) return best.ToList();

            int reducedLongRequirement = Math.Max(pentaLong - 1, 0);
            return SelectBestScores(allScores, reducedLongRequirement, pentaTotal - 1, year);
        }


        private IEnumerable<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore> 
            SelectBestScores1(List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore> allScores, 
            int pentaLong, int pentaTotal, int year)
        {
            int counted = 0;
            double bestScore = 0;
            IEnumerable<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore> bestCombination = null;

            foreach (var combination in allScores.Combinations(pentaTotal))
            {
                counted++;
                // Must have 2 long sessions or more
                if (combination.Where(x => x.IsLongSession).Count() < pentaLong) continue;

                if (combination.Sum(x => x.Score) > bestScore)
                {
                    bestScore = combination.Sum(x => x.Score);
                    bestCombination = combination;
                }
            }
            return bestCombination;
        }
    }
}
