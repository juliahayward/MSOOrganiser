using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class YearMedalsGenerator
    {
        public class YearMedalsVm
        {
            public int Year { get; set; }
            public IEnumerable<MedalVm> Medals { get; set; }

            public class MedalVm
            {
                public string GameCode { get; set; }
                public string GameName { get; set; }
                public string EventCode { get; set; }
                public string EventName { get; set; }
                public string Medal { get; set; }
                public string JuniorMedal { get; set; }
                public string Medals { 
                    get 
                    { 
                        var won = new [] { Medal, JuniorMedal };
                        return string.Join(",<br>", won.Where(x => !string.IsNullOrEmpty(x))); 
                    }
                }
                public int Rank { get; set; }
                public int ContestantId { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Name { get { return FirstName + " " + LastName; } }
                public string Nationality { get; set; }
                public string Flag { get { return Nationality.GetFlag(); } }
            }
        }

        public YearMedalsVm GetModel(int year)
        {
            var context = DataEntitiesProvider.Provide();

            // Warning - this will go wonky when I undo the 2007/7002 hack
            var olympiad = context.Olympiad_Infoes.FirstOrDefault(x => x.YearOf == year);
            if (olympiad == null)
                throw new ArgumentException($"No olympiad was held in {year}");
            var olympiadId = olympiad.Id;

            var retval = new YearMedalsVm { Year = year };

            retval.Medals = context.Entrants.Where(x => x.OlympiadId.Value == olympiadId
                    && (x.Medal != null || x.JuniorMedal != null))
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Join(context.Events.Where(x => x.OlympiadId == olympiadId),
                        ec => ec.e.EventId, ev => ev.EIN, (ec, ev) => new { ec, ev })
                .Select(ecv => new YearMedalsVm.MedalVm()
                {
                    EventCode = ecv.ev.Code,
                    GameCode = ecv.ev.Game.Code,
                    GameName = ecv.ev.Game.Mind_Sport,
                    EventName = ecv.ev.Mind_Sport,
                    Medal = ecv.ec.e.Medal,
                    JuniorMedal = ecv.ec.e.JuniorMedal,
                    Rank = ecv.ec.e.Rank.Value,
                    ContestantId = ecv.ec.c.Mind_Sport_ID,
                    FirstName = ecv.ec.c.Firstname,
                    LastName = ecv.ec.c.Lastname,
                    Nationality = ecv.ec.c.Nationality ?? "default"
                })
                .ToList();

            retval.Medals = retval.Medals.OrderBy(x => x.GameName)
                .ThenBy(x => x.EventName)
                .ThenBy(x => x.Rank)
                .ThenBy(x => x.Medal.MedalRank())
                .ThenBy(x => x.JuniorMedal.MedalRank());


            return retval;
        }

    }
}
