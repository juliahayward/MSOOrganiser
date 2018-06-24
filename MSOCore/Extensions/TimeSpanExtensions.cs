using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToStandardString(this TimeSpan t)
        {
            return t.ToString(@"hh\:mm");
        }
    }
}
