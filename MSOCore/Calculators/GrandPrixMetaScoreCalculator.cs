using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;
using MSOCore.Reports;

namespace MSOCore.Calculators
{
    public class GrandPrixMetaScoreCalculator
    {
        public List<GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore> 
            SelectEligibleScores(List<GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore> allScores)
        {
            var alreadyCountedVariants = new List<int>();
            var alreadyCountedGames = new List<string>();
            var alreadyCountedCategories = new List<string>();

            var sortedScores = allScores.OrderByDescending(x => x.Score);
            var eligibleScores = new List<GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore>();
            foreach (var score in sortedScores)
            {
                // You can't have more than one of each GameVariant
                if (score.GameVariantCode.HasValue)
                {
                    if (alreadyCountedVariants.Contains(score.GameVariantCode.Value)) continue;
                    alreadyCountedVariants.Add(score.GameVariantCode.Value);
                }
                else // Some games have no Variants, in which case the whole Game counts as a variant
                {
                    if (alreadyCountedGames.Contains(score.GameCode)) continue;
                    // Will be added to the list later
                }

                var s = score.Clone();
                // If this is the best in a category, double the points, 
                if (!alreadyCountedCategories.Contains(score.GPCategory))
                {
                    s.Score *= 2;
                    alreadyCountedCategories.Add(score.GPCategory);
                }

                // If this is not the best in a Game, then halve the points
                if (alreadyCountedGames.Contains(score.GameCode))
                {
                    s.Score = Math.Round(s.Score / 2, MidpointRounding.AwayFromZero);
                }
                else 
                { 
                    alreadyCountedGames.Add(score.GameCode);
                }

                eligibleScores.Add(s);
            }

            return eligibleScores.OrderByDescending(x => x.Score).ToList();
        }

        public List<GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore>
            SelectEligibleCategoryScores(List<GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore> allScores)
        {
            return allScores.OrderByDescending(x => x.Score).ToList();
        }
    }
}
