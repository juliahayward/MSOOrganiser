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

            var olympiadId = context.Olympiad_Infoes.First(x => x.YearOf == year).Id;

            var retval = new YearMedalsVm { Year = year };

            var games = context.Games.ToDictionary(x => x.Code, x => x);

            retval.Medals = context.Entrants.Where(x => x.OlympiadId.Value == olympiadId && x.Medal != null)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Join(context.Events.Where(x => x.OlympiadId == olympiadId),
                        ec => ec.e.Game_Code, ev => ev.Code, (ec, ev) => new { ec, ev })
                .Select(ecv => new YearMedalsVm.MedalVm()
                {
                    EventCode = ecv.ev.Code,
                    EventName = ecv.ev.Mind_Sport,
                    Medal = ecv.ec.e.Medal,
                    Rank = ecv.ec.e.Rank.Value,
                    ContestantId = ecv.ec.c.Mind_Sport_ID,
                    FirstName = ecv.ec.c.Firstname,
                    LastName = ecv.ec.c.Lastname,
                    Nationality = ecv.ec.c.Nationality
                })
                .ToList()
                .OrderBy(x => x.EventCode).ThenBy(x => x.Rank).ThenBy(x => x.Medal.MedalRank());

            foreach (var medal in retval.Medals)
            {
                medal.GameCode = medal.EventCode.Substring(0, 2);
                medal.GameName = games[medal.GameCode].Mind_Sport;
            }

            return retval;
        }

    }
}
