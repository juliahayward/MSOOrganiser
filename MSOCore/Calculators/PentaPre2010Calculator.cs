using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class PentaPre2010Calculator : PentaCalculator
    {
        public override double Formula(int n, double p)
        {
            // Spread linearly; top = 100, bottom = 0 
            var myPentaScore = 100 * (n - p) / (n - 1);

            return myPentaScore;
        }
    }
}
