using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class RankCalculator
    {
        public void Calculate(int numberInTeam, bool highScoreIsBest, IEnumerable<IPentaCalculable> entries)
        {
            if (!entries.Any()) return;

            var entriesToRank = (highScoreIsBest) 
                ? entries.Where(x => !x.Absent && x.Score != null).OrderByDescending(x => double.Parse(x.Score))
                : entries.Where(x => !x.Absent && x.Score != null).OrderBy(x => double.Parse(x.Score));

            var currentScore = "";
            var currentRank = 0;
            var currentEffectiveRank = 0;
            var currentInTeam = numberInTeam;
            // We're relying on people in the same team having the same score;
            foreach (var entry in entriesToRank)
            {
                if (currentInTeam == numberInTeam)
                {
                    currentRank += 1;
                    if (entry.Score != currentScore) currentEffectiveRank = currentRank;
                    currentInTeam = 1;
                }
                else
                    currentInTeam += 1;

                entry.Rank = currentEffectiveRank;
                currentScore = entry.Score;
            }

        }
    }
}
