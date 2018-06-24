using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOOrganiser
{
    /// <summary>
    /// Yes, we all know classes like this are a bit of a smell. Information stored here
    /// is basically on its way to being stored in the db or managed some other way -
    /// I'm not happy to do schema changes while MSO is actually taking place in case
    /// that brings the whole office crashing down, and quick fixes were necessary.
    /// </summary>
    public class GlobalSettings
    {
        public static string LoggedInUser { get; set; }

        public static bool LoggedInUserIsAdmin { get { return LoggedInUser == "Tony" || LoggedInUser == "Julia"; } }

        public static bool AutomaticallyLoadEntries { get { return LoggedInUser == "Tony" || LoggedInUser == "Julia"; } }
    }
}
