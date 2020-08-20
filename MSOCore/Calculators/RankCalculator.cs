using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class RankCalculatorFactory
    {
        public static IRankCalculator Get(Event ev)
        {
            if (ev.OlympiadId == 25 /* 2010 online */ && ev.Code.StartsWith("PO"))
                return new PokerstarsRankCalculator();

            return new RankCalculator();
        }
    }

    public interface IRankCalculator
    {
        bool CanCalculate(IEnumerable<IPentaCalculable> entries);
        void Calculate(int numberInTeam, bool highScoreIsBest, IEnumerable<IPentaCalculable> entries);
    }

    /// <summary>
    /// Given a set of entries with scores and absence-flags, calculate ranks
    /// </summary>
    public class RankCalculator : IRankCalculator
    {
        public bool CanCalculate(IEnumerable<IPentaCalculable> entries)
        {
            // The website posts scores as blanks; this lets us work out if we're ready to do ranks yet.
            return !entries.Any(x => !x.Absent && x.Score == "");
        }

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

    /// <summary>
    /// Given a set of entries with scores and absence-flags, calculate ranks
    /// This is an unusual case in that I don't want to enter thousands of unknown people. So we use
    /// the score for a contestant as a proxy for their rank.
    /// </summary>
    public class PokerstarsRankCalculator : IRankCalculator
    {
        public bool CanCalculate(IEnumerable<IPentaCalculable> entries)
        {
            // The website posts scores as blanks; this lets us work out if we're ready to do ranks yet.
            return !entries.Any(x => !x.Absent && x.Score == "");
        }

        public void Calculate(int numberInTeam, bool highScoreIsBest, IEnumerable<IPentaCalculable> entries)
        {
            if (!entries.Any()) return;

            var entriesToRank = (highScoreIsBest)
                ? entries.Where(x => !x.Absent && x.Score != null).OrderByDescending(x => double.Parse(x.Score))
                : entries.Where(x => !x.Absent && x.Score != null).OrderBy(x => double.Parse(x.Score));

            foreach (var entry in entriesToRank)
            {
                entry.Rank = int.Parse(entry.Score);
            }

        }
    }
}
