using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.ApiLogic
{
    public class OlympiadEventsApiLogic
    {
        public class OlympiadEventsVm
        {
            public class EventVm 
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public decimal Cost { get; set; }
                public decimal ConcessionCost { get; set; }
            }

            public int Year { get; set; }
            public string OlympiadName { get; set; }
            public decimal MaximumCost { get; set; }
            public decimal MaximumConcessionCost { get; set; }
            public IEnumerable<EventVm> Events { get; set; }
        }

        public OlympiadEventsVm GetOlympiadEvents()
        {
            var context = DataEntitiesProvider.Provide();
            var vm = new OlympiadEventsVm();

            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var entryFees = context.Fees.ToDictionary(x => x.Code, x => x);

            vm.OlympiadName = currentOlympiad.FullTitle();
            vm.Year = currentOlympiad.YearOf.Value;
            vm.MaximumCost = currentOlympiad.MaxFee.Value;
            vm.MaximumConcessionCost = currentOlympiad.MaxCon.Value;
            vm.Events = currentOlympiad.Events
                .Where(x => x.No_Sessions > 0)
                .Select(e =>
                new OlympiadEventsVm.EventVm()
                {
                    Code = e.Code,
                    Name = e.Mind_Sport,
                    Cost = entryFees[e.Entry_Fee].Adult.Value,
                    ConcessionCost = entryFees[e.Entry_Fee].Concession.Value
                }).ToList();

            return vm;
        }

        public void AddEventEntry(string json)
        {
            var context = DataEntitiesProvider.Provide();

            var entryJson = new EntryJson()
            {
                JsonText = json
            };

            context.EntryJsons.Add(entryJson);
            context.SaveChanges();
        }
    }
}
