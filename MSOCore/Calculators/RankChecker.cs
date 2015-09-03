using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class RankChecker
    {
        public void Check(int numberInTeam, IEnumerable<IPentaCalculable> entries)
        {
            var ranks = entries.Where(x => !x.Absent).Select(x => x.Rank).Distinct().OrderBy(x => x).ToArray();
            if (ranks.First() == 0)
                throw new Exception("There is a missing rank");

            for (int i = 0; i < ranks.Count(); i++)
            {
                var rank = ranks[i];
                var nextRank = (i < ranks.Count() - 1) 
                    ? ranks[i+1]    // next in the array
                    : 1 + entries.Count(x => !x.Absent) / numberInTeam;
                
                var numberOnThisRank = entries.Count(x => x.Rank == rank);
                var expectedNumberOnThisRank = (nextRank - rank) * numberInTeam;

                if (numberOnThisRank != expectedNumberOnThisRank)
                    throw new Exception(string.Format("Wrong number on rank {0}: expected {1}, found {2}",
                        rank, expectedNumberOnThisRank, numberOnThisRank));
            }
        }
    }
}
