using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Contestant
    {
        public string FullName()
        {
            return Firstname + " " + Lastname;
        }

        public string FullNameWithInitials()
        {
            return Firstname + " " 
                + (string.IsNullOrEmpty(Initials) ? "" : Initials + " ")
                + Lastname;
        }

        public static bool IsJuniorForOlympiad(DateTime? dateofBirth, Olympiad_Info olympiad)
        {
            // If JnrAge is 18, then we can be 19 years minus a day (hence >, not >=) on the AgeDate
            var requiredDob = olympiad.AgeDate.Value.AddYears(- olympiad.JnrAge.Value - 1);
            return (dateofBirth.HasValue && dateofBirth > requiredDob);
        }

        public bool IsJuniorForOlympiad(Olympiad_Info olympiad)
        {
            return IsJuniorForOlympiad(this.DateofBirth, olympiad);
        }

        public static bool IsSeniorForOlympiad(DateTime? dateofBirth, Olympiad_Info olympiad)
        {
            // If JnrAge is 18, then we can be 19 years minus a day (hence >, not >=) on the AgeDate
            var requiredDob = olympiad.AgeDate.Value.AddYears(-olympiad.SnrAge.Value);
            return (dateofBirth.HasValue && dateofBirth <= requiredDob);
        }

        public bool IsSeniorForOlympiad(Olympiad_Info olympiad)
        {
            return IsSeniorForOlympiad(this.DateofBirth, olympiad);
        }

        public string JuniorFlagForOlympiad(Olympiad_Info olympiad)
        {
            return (IsJuniorForOlympiad(olympiad)) ? "JNR" : "";
        }
    }
}
