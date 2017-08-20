using MSOCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class EventsWithPrizesReportGenerator
    {
        public class EventsWithPrizesVm
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Gold { get; set; }
            public string Silver { get; set; }
            public string Bronze { get; set; }
            public string JnrGold { get; set; }
            public string JnrSilver { get; set; }
            public string JnrBronze { get; set; }
        }

        public IEnumerable<EventsWithPrizesVm> GetItemsForLatest()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            return GetItems(olympiad.Id);
        }

        public IEnumerable<EventsWithPrizesVm> GetItems(int olympiadId)
        {
            var context = DataEntitiesProvider.Provide();
            var events = context.Events.Where(x => x.OlympiadId == olympiadId)
                .Where(x => (x.C1st_Prize != null && x.C1st_Prize != "0"))
                .ToList()
                .Select(e => new EventsWithPrizesVm 
                    { 
                        Code = e.Code,
                        Name = e.ShortName(),
                        Gold = (e.C1st_Prize != "0" ? e.C1st_Prize : ""),
                        Silver = (e.C2nd_Prize != "0" ? e.C2nd_Prize : ""),
                        Bronze = (e.C3rd_Prize != "0" ? e.C3rd_Prize : ""),
                        JnrGold = (e.JNR_1st_Prize != "0" ? e.JNR_1st_Prize : ""),
                        JnrSilver = (e.JNR_2nd_Prize != "0" ? e.JNR_2nd_Prize : ""),
                        JnrBronze = (e.JNR_3rd_Prize != "0" ? e.JNR_3rd_Prize : ""),
                    })
                .ToList();

            return events.OrderBy(r => r.Code);
        }
    }
}
