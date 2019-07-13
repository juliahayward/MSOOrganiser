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
                public int Entrants { get; set; }
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
                : context.Olympiad_Infoes.First(x => x.Current);

            var events = context.Events.Where(x => x.OlympiadId == olympiad.Id).ToList();

            var entrants = context.Entrants.Where(x => x.OlympiadId == olympiad.Id)
                .GroupBy(x => x.EventId)
                .ToDictionary(x => x.Key, x => x.Count());

            vm.Events = events
                .Select(x => new TotalEventEntriesVm.EventVm()
                {
                    Code = x.Code,
                    Name = x.Mind_Sport,
                    Entrants = (entrants.ContainsKey(x.EIN)) ? entrants[x.EIN] : 0
                })
                .OrderBy(x => x.Code);
            return vm;
        }
    }
}
