using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public interface IPentaCalculable
    {
        int Rank { get; set; }
        float PentaScore { get; set; }
        bool Absent { get; set; }
    }

    public class Penta2015Calculator
    {
        public void Calculate(int numberInTeam, IEnumerable<IPentaCalculable> entries)
        {
            if (entries.Any(x => x.Absent == false && x.Rank == 0))
                throw new ArgumentException("At least one player was present but has no rank");

            var numberOfTeams = entries.Count(x => !x.Absent) / numberInTeam;

            foreach (var entry in entries)
            {
                if (entry.Absent) {
                    entry.PentaScore = 0;
                    continue;
                }

                var myRank = entry.Rank;
                var numberOnMyRank = entries.Count(x => x.Rank == myRank);
                var numberOfTeamsOnMyRank = numberOnMyRank / numberInTeam;

                var myEffectiveRank = myRank + (numberOfTeamsOnMyRank - 1) / 2.0;

                // Spread linearly, imaginary rank 0 = 100; last place = 0;
                var myPentaScore = 100 * (numberOfTeams - myEffectiveRank) / numberOfTeams;

                entry.PentaScore = (float)myPentaScore;
            }
        }
    }
}
