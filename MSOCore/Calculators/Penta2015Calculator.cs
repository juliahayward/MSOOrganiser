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

    public class Penta2015Calculator : PentaCalculator
    {
        public override double Formula(int n, double p)
        {
            // Spread linearly, imaginary rank 0 = 100; last place = 0;
            var myPentaScore = 100 * (n - p) / (n - 1);

            // * All * events - introduce a fudge factor
            myPentaScore = myPentaScore * n / (n + 1);

            return myPentaScore;
        }
    }
}
