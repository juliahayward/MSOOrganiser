using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOOrganiser.Events
{
    public class EventEventArgs : EventArgs
    {
        public int OlympiadId { get; set; }
        public string EventCode { get; set; }
    }
}
