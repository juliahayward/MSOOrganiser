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
            public TitleVm Titles { get; set; }

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

            public class TitleVm  
            {
                public IList<string> Grandmasters { get; }
                public IList<string> Masters { get; }
                public IList<string> CandidateMasters { get; }

                public TitleVm()
                {
                    Grandmasters = new List<string>();
                    Masters = new List<string>();
                    CandidateMasters = new List<string>();
                }
            }

            public GameMedalsVm()
            {
                Titles = new TitleVm();
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
                .Where(x => x.Game_Code.StartsWith(gameCode) && (x.Medal != null || x.JuniorMedal != null))
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Join(context.Events,
                        ec => new { Code = ec.e.Game_Code, OlympiadId = ec.e.OlympiadId.Value }, ev => new { ev.Code, ev.OlympiadId }, 
                        (ec, ev) => new { ec, ev })
                .Select(ecv => new GameMedalsVm.MedalVm()
                {
                    Year = ecv.ev.Olympiad_Info.YearOf.Value,
                    EventCode = ecv.ev.Code,
                    EventName = ecv.ev.Mind_Sport,
                    Medal = ecv.ec.e.Medal ?? ecv.ec.e.JuniorMedal,
                    ContestantId = ecv.ec.c.Mind_Sport_ID,
                    FirstName = ecv.ec.c.Firstname,
                    LastName = ecv.ec.c.Lastname,
                    Nationality = ecv.ec.c.Nationality ?? "default"
                })
                .ToList()
                .Where(x => x.Year < 7002)
                .OrderByDescending(x => x.Year).ThenBy(x => x.EventCode).ThenBy(x => x.Rank).ThenBy(x => x.Medal.MedalRank());

            var titleLookup = retval.Medals.GroupBy(x => new { x.EventCode, x.ContestantId, x.Name });
            foreach (var possibleTitle in titleLookup)
            {
                var golds = possibleTitle.Count(x => x.Medal == "Gold");
                var silvers = possibleTitle.Count(x => x.Medal == "Silver");
                var bronzes = possibleTitle.Count(x => x.Medal == "Bronze");
                if (golds >= 2 || (golds == 1 && silvers >= 2))
                    retval.Titles.Grandmasters.Add(possibleTitle.Key.Name);
                else if ((golds == 1 && silvers + bronzes > 0)
                    || silvers >= 2
                    || (silvers == 1 && bronzes >= 2))
                    retval.Titles.Masters.Add(possibleTitle.Key.Name);
                else if ((silvers == 1 && bronzes > 0)
                    || bronzes >= 2)
                    retval.Titles.CandidateMasters.Add(possibleTitle.Key.Name);
            }

            return retval;
        }

    }
}
