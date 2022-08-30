using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    // A trivial Calculator that assigns 0 to each entry. Used for events that aren't counted
    // for pentamind / GP purposes (eg casual, demonstrations, beginners')
    public class CasualEventCalculator : IPentaCalculator
    {
        public void Calculate(int numberInTeam, IEnumerable<IPentaCalculable> entries, bool isInPentamind = true, float premiumFactor = 1.0f, int overridingNumberOfTeams = 0)
        {
            foreach (var entry in entries)
            {
                entry.PentaScore = 0;
            }
        }
    }
}
