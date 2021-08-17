using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
   

    public class Penta2021Calculator : PentaCalculator
    {

        public override double Formula(int n, double p)
        {
            // Source: https://mindsportsolympiad.com/pentamind-world-championship/
            if (n <= 100)
            {
                // Spread linearly, rank 1 = 100; last place = 0;
                var myPentaScore = 100 * (n - p) / (n - 1);

                // * All * events - introduce a fudge factor
                return myPentaScore * (n + 5) / (n + 6);
            }
            else // n > 100
            {
                // Effectively capping off at 100
                if (p <= 10)
                    return 100.0 * (105.0 / 99.0 / 106.0) * (100.0 - p);
                else
                    return 100.0 * (105.0 * 90.0 / 99.0 / 106.0) * (n - p) / (n - 10);
            }
        }
    }

}
