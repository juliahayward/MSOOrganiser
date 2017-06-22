using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class EventEntrantsGenerator
    {
        public class EventEntrantsVm
        {
            public int EventYear { get; set; }
            public string EventName { get; set; }
            public IEnumerable<EntrantVm> Entrants { get; set; }

            public class EntrantVm
            {
                public string Name { get; set; }
            }
        }

        // Warning - this will go wonky in 2007 when I undo the 7002 hack
        public EventEntrantsVm GetModel(int? year, string gameCode)
        {
            var vm = new EventEntrantsVm();
            var context = DataEntitiesProvider.Provide();

            var olympiad = (year.HasValue)
                ? context.Olympiad_Infoes.First(x => x.YearOf == year.Value)
                : context.Olympiad_Infoes.OrderByDescending(x => x.YearOf).First();

            vm.EventYear = olympiad.YearOf.Value;

            var evt = context.Events.First(x => x.OlympiadId == olympiad.Id && x.Code == gameCode);
            vm.EventName = evt.Mind_Sport;
            vm.Entrants = evt.Entrants
                .Select(x => new EventEntrantsVm.EntrantVm() { Name = x.Name.FullName() })
                .OrderBy(x => x.Name);
            return vm;
        }
    }
}
