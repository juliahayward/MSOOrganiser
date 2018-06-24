using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class DonationReportGenerator
    {
        public class DonationReportVm
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }
        }

        public IEnumerable<DonationReportVm> GetItemsForLatest()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            return GetItems(olympiad.Id);
        }

        public IEnumerable<DonationReportVm> GetItems(int olympiadId)
        {
            var context = DataEntitiesProvider.Provide();
            var donors = context.Entrants.Where(x => x.OlympiadId == olympiadId && x.Game_Code.StartsWith("DO"))
                .Join(context.Contestants, x => x.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Select(ec => new DonationReportVm
                    {
                        FirstName = ec.c.Firstname,
                        LastName = ec.c.Lastname
                    })
                .ToList()
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName);

            return donors;
        }
    }
}
