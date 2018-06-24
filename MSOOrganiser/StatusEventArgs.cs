using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOOrganiser
{
    public class StatusEventArgs : EventArgs
    {
        public string Status { get; private set; }

        public StatusEventArgs(string status)
        {
            Status = status;
        }
    }
}
