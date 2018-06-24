using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class ContestantMedalsGenerator
    {
        public class ContestantVm
        {
            public class MedalVm
            {
                public int Year { get; set; }
                public string Name { get; set; }
                public string Medal { get; set; }
                public string Code { get; set; }
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public string Nationality { get; set; }
            public string Flag
            {
                get
                {
                    return Nationality.GetFlag();
                }
            }

            public IEnumerable<MedalVm> Medals { get; set; }
            public int Golds { get { return Medals.Count(x => x.Medal.StartsWith("Gold")); } }
            public int Silvers { get { return Medals.Count(x => x.Medal.StartsWith("Silver")); } }
            public int Bronzes { get { return Medals.Count(x => x.Medal.StartsWith("Bronze")); } }
            public IEnumerable<string> Grandmasters { get; set; }
            public IEnumerable<string> InternationalMasters { get; set; }
            public IEnumerable<string> CandidateMasters { get; set; }

            public bool HasTitles()
            {
                return (Grandmasters.Any() || InternationalMasters.Any() || CandidateMasters.Any());
            }
        }

        public ContestantVm GetModel(int id)
        {
            var context = DataEntitiesProvider.Provide();

            var contestant = context.Contestants.First(x => x.Mind_Sport_ID == id);

            var medalEvents1 = context.Entrants.Where(x => x.Mind_Sport_ID == id &&
                        (x.Medal != null || x.JuniorMedal != null) && x.OlympiadId != null)
                .Join(context.Events,
                x => new { code = x.Game_Code, year = x.OlympiadId.Value },
                e => new { code = e.Code, year = e.OlympiadId }, (x, e) => new { x, e })
                .Select(xe => new ContestantVm.MedalVm
                {
                    Year = xe.e.Olympiad_Info.YearOf.Value,
                    Code = xe.x.Game_Code,
                    Name = xe.e.Mind_Sport,
                    Medal = xe.x.Medal ?? xe.x.JuniorMedal
                })
                .OrderByDescending(x => x.Year);
            var medalEvents = medalEvents1.ToList();

            var gmCodes = new List<string>();
            var imCodes = new List<string>();
            var cmCodes = new List<string>();
            foreach (var code in medalEvents.Select(x => x.Code).Distinct())
            {
                var name = medalEvents.First(x => x.Code == code).Name;

                var golds = medalEvents.Count(x => x.Code == code && x.Medal == "Gold");
                var silvers = medalEvents.Count(x => x.Code == code && x.Medal == "Silver");
                var bronzes = medalEvents.Count(x => x.Code == code && x.Medal == "Bronze");
                if (golds >= 2 || (golds == 1 && silvers >= 2))
                    gmCodes.Add(name);
                else if ((golds == 1 && silvers + bronzes > 0)
                    || silvers >= 2
                    || (silvers == 1 && bronzes >= 2))
                    imCodes.Add(name);
                else if ((silvers == 1 && bronzes > 0)
                    || bronzes >= 2)
                    cmCodes.Add(name);
            }

            var model = new ContestantVm
            {
                Id = id,
                Name = string.Format("{0} {1}", contestant.Firstname, contestant.Lastname),
                Nationality = contestant.Nationality ?? "default",
                Medals = medalEvents,
                Grandmasters = gmCodes,
                InternationalMasters = imCodes,
                CandidateMasters = cmCodes
            };

            return model;
        }
    }
}
