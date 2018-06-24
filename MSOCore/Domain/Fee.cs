using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Fee
    {
        public string DropdownText
        {
            get
            {
                if (Adult.HasValue && Concession.HasValue)
                    return Code + " (" + Adult.Value.ToString("C") + "/" + Concession.Value.ToString("C") + ")";
                else
                    return Code;
            }
        }
    }
}
