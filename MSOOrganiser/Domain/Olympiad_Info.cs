using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOOrganiser
{
    public partial class Olympiad_Info
    {
        public string FullTitle()
        {
            return Number + " " + Title + " (" + YearOf + ")";
        }
    }
}
