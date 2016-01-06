using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class EventIncomeReportGenerator
    {
        public class EventIncomeReportVm
        {
            public class EventVm {
                public string Name { get; set; }
                public int SequenceNumber { get; set; }
            }
            public class FeeVm {
                public int Entrants { get; set; }
                public decimal TotalFees { get; set; }
            }

            public string OlympiadName { get; set; }
            public Dictionary<string, string> Games { get; set; }
            public Dictionary<string, EventVm> Events { get; set; }
            public Dictionary<string, FeeVm> Fees { get; set; }
        }

        public EventIncomeReportVm GetItemsForLatest(bool actualEvents)
        {
            Expression<Func<Event, bool>> eventSelector;
            if (actualEvents)
                eventSelector = (x => x.Number > 0);
            else
                eventSelector = (x => x.Number == 0);


            var vm = new EventIncomeReportVm();
            var context = new DataEntities();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            vm.OlympiadName = currentOlympiad.FullTitle();

            vm.Events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id)
                .Where(eventSelector)
                .ToDictionary(e => e.Code, e => new EventIncomeReportVm.EventVm { Name = e.Mind_Sport, SequenceNumber = e.Number });

            vm.Fees = context.Entrants.Where(x => x.OlympiadId == currentOlympiad.Id)
                .GroupBy(x => x.Game_Code)
                .ToDictionary(gp => gp.Key, gp => new EventIncomeReportVm.FeeVm { Entrants = gp.Count(), TotalFees = gp.Sum(x => x.Fee) });

            vm.Games = context.Games.ToDictionary(g => g.Code, g => g.Mind_Sport);

            return vm;
        }
    }
}
