using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Extensions
{
    public static class StringExtensions
    {
        public static string GetFlag(this string nationality)
        {
            if (nationality == "") nationality = "default";

            return string.Format("/content/images/flags/{0}.jpg", nationality)
                        .Replace(" ", "_").ToLower();
        }

        public static string Ordinal(this string number)
        {
            if (number.EndsWith("1")) return number + "st";
            if (number.EndsWith("2") && !number.EndsWith("12")) return number + "nd";
            if (number.EndsWith("3") && !number.EndsWith("13")) return number + "rd";
            return number + "th";
        }

        /// <summary>
        /// Note that both medals and juniors start with G / S / B
        /// </summary>
        /// <param name="medal"></param>
        /// <returns></returns>
        public static int MedalRank(this string medal)
        {
            if (medal == null)
                return 4;
            else if (medal.StartsWith("G"))
                return 1;
            else if (medal.StartsWith("S"))
                return 2;
            else if (medal.StartsWith("B"))
                return 3;
            else
                return 4;
        }
    }
}
