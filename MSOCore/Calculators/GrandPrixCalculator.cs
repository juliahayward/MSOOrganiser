using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class GrandPrixCalculator : IPentaCalculator
    {
        public void Calculate(int numberInTeam, IEnumerable<IPentaCalculable> entries, bool isInPentamind = true, float premiumFactor = 1.0f, int overridingNumberOfTeams = 0)
        {
            if (entries.Any(x => x.Absent == false && x.Rank == 0))
                throw new ArgumentException("At least one player was present but has no rank");

            var numberOfTeams = (overridingNumberOfTeams > 0) ? overridingNumberOfTeams : entries.Count(x => !x.Absent) / numberInTeam;

            foreach (var entry in entries)
            {
                if (entry.Absent || !isInPentamind)
                {
                    entry.PentaScore = 0;
                    continue;
                }

                var myRank = entry.Rank;
                var numberOnMyRank = entries.Count(x => x.Rank == myRank);
                var numberOfTeamsOnMyRank = numberOnMyRank / numberInTeam;

                var myPentaScore = Formula(numberOfTeams, myRank, numberOfTeamsOnMyRank);

                entry.PentaScore = (float)myPentaScore * premiumFactor;
            }
        }

        private double Formula(int numberOfEntrants, int myRank, int numberOfTeamsOnMyRank)
        {
            float totalScore = 0;
            for (int i=0; i<numberOfTeamsOnMyRank; i++)
            {
                totalScore += Formula1(numberOfEntrants, myRank + i, myRank);
            }
            return Math.Round(totalScore / numberOfTeamsOnMyRank);
        }

        private int Formula1(int numberOfEntrants, int actualRank, int rankOfFirstInGroup)
        {
            if (actualRank == 1) return 40;
            else if (actualRank == 2) return 28;
            else if (actualRank == 3) return 20;
            else if (actualRank == 4) return 12;
            else if (rankOfFirstInGroup <= numberOfEntrants * 0.05) return 8;
            else if (rankOfFirstInGroup <= numberOfEntrants * 0.10) return 6;
            else if (rankOfFirstInGroup <= numberOfEntrants * 0.25) return 4;
            else if (rankOfFirstInGroup <= numberOfEntrants * 0.50) return 2;
            else return 0;
        }
    }
}
