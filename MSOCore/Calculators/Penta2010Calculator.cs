using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class Penta2010Calculator
    {
        public void Calculate(int numberInTeam, IEnumerable<IPentaCalculable> entries)
        {
            if (entries.Any(x => x.Absent == false && x.Rank == 0))
                throw new ArgumentException("At least one player was present but has no rank");

            var numberOfTeams = entries.Count(x => !x.Absent) / numberInTeam;

            foreach (var entry in entries)
            {
                if (entry.Absent)
                {
                    entry.PentaScore = "";
                    continue;
                }

                var myRank = entry.Rank;
                var numberOnMyRank = entries.Count(x => x.Rank == myRank);
                var numberOfTeamsOnMyRank = numberOnMyRank / numberInTeam;

                var myEffectiveRank = myRank + (numberOfTeamsOnMyRank - 1) / 2.0;

                // Spread linearly; top = 100, bottom = 0 
                var myPentaScore = 100 * (numberOfTeams - myEffectiveRank) / (numberOfTeams - 1);
                
                // Small events - introduce a fudge factor
                if (numberOfTeams < 10)
                    myPentaScore = myPentaScore * numberOfTeams / (numberOfTeams + 1);

                entry.PentaScore = myPentaScore.ToString();
            }
        }
    }
}
