using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MSOCore.Extensions
{
    public static class StringExtensions
    {
        public static string GetFlag(this string nationality)
        {
            if (string.IsNullOrEmpty(nationality)) nationality = "default";

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

        /// <summary>
        /// Turns ASCII characters into their \u1234 equivalent
        /// </summary>
        public static string EncodeNonAsciiCharacters(this string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Turns ASCII characters into their \u1234 equivalent
        /// </summary>
        public static string DecodeEncodedNonAsciiCharacters(this string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
    }
}
