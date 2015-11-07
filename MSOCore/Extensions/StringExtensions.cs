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
            return string.Format("http://www.boardability.com/images/flags/{0}.jpg", nationality)
                        .Replace(" ", "_").ToLower();
        }

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
