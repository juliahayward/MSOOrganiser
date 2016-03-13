using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class EventIndexer
    {
        /// <summary>
        /// Reindex the events in this olympiad. Those with sessions get numbers in sequence, in
        /// order of start time. Those without (pentamind, non-events) get 0 and don't appear in 
        /// event-type reports.
        /// </summary>
        /// <param name="olympiadId"></param>
        public void IndexEvents(int olympiadId)
        {
            var context = new DataEntities();
            var events = context.Events.Where(x => x.OlympiadId == olympiadId && x.Event_Sess.Any())
                .OrderBy(x => x.Event_Sess.Min(es => es.Date))
                .ThenBy(x => x.Event_Sess.Min(es => es.Session1.StartTime))
                .ThenBy(x => x.Code)
                .ToList();

            var index = 1;
            foreach (var evt in events)
            {
                evt.Number = index;
                index++;
            }

            var nonevents = context.Events.Where(x => x.OlympiadId == olympiadId && !x.Event_Sess.Any());
            foreach (var evt in nonevents)
            {
                evt.Number = 0;
            }

            context.SaveChanges();
        }
    }
}
