using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class EventResultsGenerator
    {
        public class EventResultsVm
        {
            public int Year { get; set; }
            public string EventCode { get; set; }
            public string EventName { get; set; }
            public string OlympiadName { get; set; }
            public IEnumerable<EntrantVm> Entrants { get; set; }

            public class EntrantVm
            {
                public string Medal { get; set; }
                public string JuniorMedal { get; set; }
                public int Rank { get; set; }
                public string Score { get; set; }
                public double PentaScore { get; set; }
                public int ContestantId { get; set; }
                public string Name { get; set; }
                public string Nationality { get; set; }
                public string Flag { get { return Nationality.GetFlag(); } }
            }
        }

        public EventResultsVm GetModel(int year, string eventCode)
        {
            var context = DataEntitiesProvider.Provide();

            // Warning - this will go wonky when I undo the 2007/7002 hack
            var olympiad = context.Olympiad_Infoes.First(x => x.YearOf == year);

            var retval = new EventResultsVm { Year = year, EventCode = eventCode, OlympiadName = olympiad.FullTitle() };

            var evt = context.Events.FirstOrDefault(x => x.OlympiadId == olympiad.Id && x.Code == eventCode);

            retval.EventName = evt.Mind_Sport;
            retval.Entrants = evt.Entrants
                .Where(e => e.Rank.HasValue && e.Rank.Value > 0)
                .Select(e => new EventResultsVm.EntrantVm()
                {                
                    ContestantId = e.Name.Mind_Sport_ID,
                    Medal = e.Medal,
                    JuniorMedal = e.JuniorMedal,
                    Score = e.Score,
                    PentaScore = (double)e.Penta_Score,
                    Rank = e.Rank.Value,
                    Name = e.Name.FullName(),
                    Nationality = e.Name.Nationality ?? "default",
                })
                .ToList()
                .OrderBy(x => x.Rank)
                    .ThenBy(x => x.Medal.MedalRank()).ThenBy(x => x.JuniorMedal.MedalRank());

            return retval;
        }

    }
}
