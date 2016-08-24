using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class PeopleOwingMoneyReportGenerator
    {
        public class PeopleOwingMoneyReportVm
        {
            public class AmountVm {
                public string Name { get; set; }
                public decimal Paid { get; set; }
                public decimal Owed { get; set; }
            }

            public string OlympiadName { get; set; }
            public IEnumerable<AmountVm> Fees { get; set; }
        }

        public PeopleOwingMoneyReportVm GetItemsForLatest()
        {
            var vm = new PeopleOwingMoneyReportVm();

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);

            var payments = context.Payments.Where(p => p.OlympiadId == currentOlympiad.Id)
                .GroupBy(x => x.MindSportsID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Payment1));
            var fees = context.Entrants.Where(p => p.OlympiadId == currentOlympiad.Id)
                .GroupBy(x => x.Mind_Sport_ID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Fee));
            var contestants = fees.Where(x => !payments.Keys.Contains(x.Key) || x.Value > payments[x.Key])
                .Select(x => x.Key);

            // Warning - can't do comparison inside SQL as Sum() can be NULL
            vm.OlympiadName = currentOlympiad.FullTitle();
            vm.Fees = context.Contestants.Where(x => contestants.Contains(x.Mind_Sport_ID))
                .OrderBy(x => x.Lastname)
                .ThenBy(x => x.Firstname)
                .ToList()
                .Select(x => new PeopleOwingMoneyReportVm.AmountVm() { 
                    Name = x.FullName(),
                     Owed = fees[x.Mind_Sport_ID],
                     Paid = (payments.Keys.Contains(x.Mind_Sport_ID) ? payments[x.Mind_Sport_ID].Value : 0m)
                });
 
            return vm;
        }
    }
}
