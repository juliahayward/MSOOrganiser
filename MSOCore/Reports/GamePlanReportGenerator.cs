using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class GamePlanReportGenerator
    {
        public class GamePlanReportVm
        {
            public class EventVm
            {
                public string Name { get; set; }
                public int SequenceNumber { get; set; }
            }

            public class GameVm
            {
                public string Name { get; set; }
                public string Contacts { get; set; }
                public string Equipment { get; set; }
                public string Rules { get; set; }
            }

            public string OlympiadName { get; set; }
            public Dictionary<string, GameVm> Games { get; set; }
            public Dictionary<string, EventVm> Events { get; set; }
        }

        public GamePlanReportVm GetItemsForLatest()
        {
            var vm = new GamePlanReportVm();
            var context = new DataEntities();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            vm.OlympiadName = currentOlympiad.FullTitle();

            vm.Events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id)
                .Where(x => x.Number > 0)
                .ToDictionary(e => e.Code, e => new GamePlanReportVm.EventVm { Name = e.Mind_Sport, SequenceNumber = e.Number });

            vm.Games = context.Games
                .ToDictionary(g => g.Code, g => new GamePlanReportVm.GameVm
                {
                    Name = g.Mind_Sport,
                    Contacts = g.Contacts,
                    Rules = g.Rules,
                    Equipment = g.Equipment
                });

            return vm;
        }
    }
}
