using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Olympiad_Info
    {
        public string FullTitle()
        {
            return Number + " " + Title + " (" + YearOf + ")";
        }


        public DateTime FirstDateOfBirthForJunior()
        {
            // If JnrAge is 18, then we can be 19 years minus a day (hence >, not >=) on the AgeDate
            return AgeDate.Value.AddYears(-JnrAge.Value -1);
        }

        public DateTime LastDateOfBirthForSenior()
        {
            // If JnrAge is 18, then we can be 19 years minus a day (hence >, not >=) on the AgeDate
            return AgeDate.Value.AddYears(-SnrAge.Value);
        }
    }
}
