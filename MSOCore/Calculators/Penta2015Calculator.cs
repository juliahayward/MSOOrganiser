using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public interface IPentaCalculable
    {
        // TODO for v2, we need to make scores more sensible
        string Score { get; set; }
        int Rank { get; set; }
        float PentaScore { get; set; }
        bool Absent { get; set; }
    }

    public class Penta2015Calculator : PentaCalculator
    {
        /// <summary>
        /// See mail from Matthew H.
        /// </summary>
        public override double Formula(int n, double p)
        {
            // Spread linearly, rank 1 = 100; last place = 0;
            var myPentaScore = 100 * (n - p) / (n - 1);

            // * All * events - introduce a fudge factor
            myPentaScore = myPentaScore * n / (n + 1);

            return myPentaScore;
        }
    }
}
