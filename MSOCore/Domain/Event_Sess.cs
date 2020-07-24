using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Event_Sess
    {
        /// <summary>
        /// Event_Sess's are NOT shared, so need to be copied when we copy an Event 
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Event_Sess CopyTo(Event evt)
        {
            var es = new Event_Sess()
            {
                Date = this.Date,
                EIN = evt.EIN,
                Event = evt,
                INDEX = this.INDEX,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                Session = this.Session,
                Session1 = this.Session1    // Sessions ARE shared
            };

            return es;
        }

        public DateTime ActualStart
        {
            get
            {
                if (StartTime != null)
                    return this.Date.Value.Add(StartTime.Value);
                return this.Date.Value.Add(Session1.StartTime.Value);
            }
        }
        public DateTime ActualEnd
        {
            get
            {
                if (EndTime != null)
                    return this.Date.Value.Add(EndTime.Value);
                return this.Date.Value.Add(Session1.FinishTime.Value);
            }
        }
    }
}
