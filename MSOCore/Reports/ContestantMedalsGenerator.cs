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

            public class EventVm
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public string Medal { get; set; }
                public string MedalStyle => Medal?.ToLower() ?? "";
                public string JuniorMedal { get; set; }
                public string JuniorMedalStyle => JuniorMedal?.Replace(" JNR", "").ToLower() ?? "";
                public string Partner { get; set; }
                public double? Penta { get; set; }
                public string PentaString => (Penta.HasValue) ? Penta.Value.ToString("F2") : "";
                public int? Rank { get; set; }
                public string RankString => (Rank != null && Rank > 0) ? Rank.ToString() : "";
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

            public string PointsType { get; set; }

            public IEnumerable<MedalVm> Medals { get; set; }
            public IEnumerable<EventVm> Events { get; set; }
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

            var currentOlympiad = context.Olympiad_Infoes.First(x => x.Current);
            var contestant = context.Contestants.FirstOrDefault(x => x.Mind_Sport_ID == id);
            if (contestant == null)
                throw new ArgumentOutOfRangeException($"No contestant with id {id}");

            var medalEvents1 = context.Entrants.Where(x => x.Mind_Sport_ID == id &&
                        (x.Medal != null || x.JuniorMedal != null) && x.OlympiadId != null)
                .Select(x => new ContestantVm.MedalVm
                {
                    Year = x.Event.Olympiad_Info.YearOf.Value,
                    Code = x.Event.Code,
                    Name = x.Event.Mind_Sport,
                    Medal = x.Medal ?? x.JuniorMedal
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

            var events = context.Entrants
                .Where(x => x.Mind_Sport_ID == id && x.OlympiadId == currentOlympiad.Id && !x.Absent)
                .Select(x => new ContestantVm.EventVm() { Code = x.Event.Code, Name = x.Event.Mind_Sport, Rank = x.Rank, Medal = x.Medal, JuniorMedal = x.JuniorMedal, Partner = x.Partner, Penta = x.Penta_Score})
                .OrderBy(evt => evt.Name);

            var model = new ContestantVm
            {
                Id = id,
                Name = string.Format("{0} {1}", contestant.Firstname, contestant.Lastname),
                Nationality = contestant.Nationality ?? "default",
                Medals = medalEvents,
                Grandmasters = gmCodes,
                InternationalMasters = imCodes,
                CandidateMasters = cmCodes,
                Events = events,
                PointsType = currentOlympiad.Ruleset
            };

            return model;
        }
    }
}
