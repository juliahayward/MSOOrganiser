using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class NumberListContractor
    {
        // Turn a list 3, 4, 5, 6, 8, 9 into a string "3-6, 8-9"
        public static string Contract(IEnumerable<int> numbers)
        {
            string output = "";
            int lastAdded = int.MinValue;
            foreach (var number in numbers)
            {
                if (number == lastAdded + 1)
                {
                    // Contract this one
                    if (!output.EndsWith("-")) output += "-";
                }
                else
                {
                    if (output.EndsWith("-")) output += lastAdded;
                    if (output != "") output += ", ";
                    output += number;
                }
                lastAdded = number;
            }
            if (output.EndsWith("-")) output += lastAdded;
            return output;
        }
    }
}
