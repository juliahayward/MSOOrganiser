using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class TotalEventEntriesGenerator
    {
        public class TotalEventEntriesVm
        {
            public IEnumerable<EventVm> Events { get; set; }
            public string LastLoadedCaption { get; set; }

            public class EventVm
            {
                public string Name { get; set; }
                public string Code { get; set; }
                public int EventId { get; set; }
                public int Entrants { get; set; }
                public bool IsMetaEvent { get; set; }
            }
        }

        // Warning - this will go wonky in 2007 when I undo the 7002 hack
        public TotalEventEntriesVm GetModel(int? year)
        {
            var vm = new TotalEventEntriesVm();
            var context = DataEntitiesProvider.Provide();

            var param = context.Parameters.First(x => x.Id == 1);
            vm.LastLoadedCaption = "Last load from WooCommerce: " + param.Value;

            var olympiad = (year.HasValue)
                ? context.Olympiad_Infoes.First(x => x.YearOf == year.Value)
                : context.Olympiad_Infoes.FirstOrDefault(x => x.Current);

            var events = context.Events.Where(x => x.OlympiadId == olympiad.Id)
                .Select(x => new TotalEventEntriesVm.EventVm()
                {
                    EventId = x.EIN,
                    Code = x.Code,
                    Name = x.Mind_Sport,
                    IsMetaEvent = !x.Pentamind
                })
                .OrderBy(x => x.Code).ToList();

            var entrants = context.Entrants.Where(x => x.OlympiadId == olympiad.Id)
                .ToList()
                .GroupBy(x => x.EventId)
                .ToDictionary(x => x.Key, x => x.Count());

            foreach (var ev in events)
            {
                ev.Entrants = (entrants.ContainsKey(ev.EventId)) ? entrants[ev.EventId] : 0;
            }

            vm.Events = events;
                
            return vm;
        }
    }
}
