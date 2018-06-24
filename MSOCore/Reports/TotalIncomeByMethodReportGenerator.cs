using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class TotalIncomeByMethodReportGenerator
    {
        public class TotalIncomeByMethodReportVm
        {
            public class MethodVm {
                public string Method { get; set; }
                public decimal TotalFees { get; set; }
            }

            public string OlympiadName { get; set; }
            public List<MethodVm> Fees { get; set; }
        }

        public TotalIncomeByMethodReportVm GetItemsForLatest()
        {
            var vm = new TotalIncomeByMethodReportVm();
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            vm.OlympiadName = currentOlympiad.FullTitle();

            vm.Fees = context.Payments.Where(x => x.OlympiadId == currentOlympiad.Id)
                .GroupBy(x => x.Payment_Method)
                .Select(x => new TotalIncomeByMethodReportVm.MethodVm()
                {
                    Method = x.Key,
                    TotalFees = x.Sum(p => p.Payment1.Value)
                })
                .ToList();

            return vm;
        }
    }
}
