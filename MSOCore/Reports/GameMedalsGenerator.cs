using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class GameMedalsGenerator
    {
        public class GameMedalsVm
        {
            public string GameName { get; set; }
            public IEnumerable<MedalVm> Medals { get; set; }

            public class MedalVm
            {
                public int Year { get; set; }
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

        public GameMedalsVm GetModel(string gameCode)
        {
            var context = DataEntitiesProvider.Provide();

            var game = context.Games.FirstOrDefault(x => x.Code == gameCode);
            if (game == null)
                throw new ArgumentException("Game code " + (gameCode ?? "(null)") + " not recognised");

            var retval = new GameMedalsVm { GameName = game.Mind_Sport };

            retval.Medals = context.Entrants
                .Where(x => x.Game_Code.StartsWith(gameCode) && x.Medal != null  && x.Year < 7000)
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Join(context.Events,
                        ec => new { Code = ec.e.Game_Code, ec.e.Year }, ev => new { ev.Code, ev.Year }, 
                        (ec, ev) => new { ec, ev })
                .Select(ecv => new GameMedalsVm.MedalVm()
                {
                    Year = ecv.ec.e.Year.Value,
                    EventCode = ecv.ev.Code,
                    EventName = ecv.ev.Mind_Sport,
                    Medal = ecv.ec.e.Medal,
                  //  Rank = ecv.ec.e.Rank.Value,
                    ContestantId = ecv.ec.c.Mind_Sport_ID,
                    FirstName = ecv.ec.c.Firstname,
                    LastName = ecv.ec.c.Lastname,
                    Nationality = ecv.ec.c.Nationality ?? "default"
                })
                .ToList()
                .OrderByDescending(x => x.Year).ThenBy(x => x.EventCode).ThenBy(x => x.Rank).ThenBy(x => x.Medal.MedalRank());

            return retval;
        }

    }
}
