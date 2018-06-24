using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class TodaysEventsGenerator
    {
        public class TodaysEventsVm
        {
            public class SessionVm
            {
                public string IndexLetter { get { return EventName.Substring(0, 1); } }
                public string EventName { get; set; }
                public string Location { get; set; }
                public TimeSpan Start { get; set; }
                public TimeSpan End { get; set; }
            }

            public string OlympiadName { get; set; }

            public Dictionary<string, List<SessionVm>> Sessions { get; set; }
        }

        public TodaysEventsVm GetEvents(DateTime date)
        {
            var vm = new TodaysEventsVm();
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.Where(x => x.StartDate <= date && x.FinishDate >= date).First();
            vm.OlympiadName = currentOlympiad.FullTitle();

            vm.Sessions = context.Event_Sesses.Where(x => x.Date == date).Select(x => new TodaysEventsVm.SessionVm()
            {
                EventName = x.Event.Mind_Sport,
                Location = x.Event.Location,
                Start = x.Session1.StartTime.Value,
                End = x.Session1.FinishTime.Value
            })
            .ToList()
            .OrderBy(x => x.IndexLetter).ThenBy(x => x.Start)
            .GroupBy(x => x.IndexLetter)
            .ToDictionary(x => x.Key, x => x.ToList()); 

            return vm;
        }
    }
}
