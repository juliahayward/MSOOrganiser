using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MSOCore.ApiLogic
{
    public class OlympiadEventsApiLogic
    {
        private readonly Random _random = new Random();

        public class OlympiadEventsVm
        {
            public class EventVm 
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public decimal Cost { get; set; }
                public decimal ConcessionCost { get; set; }
            }

            public int Year { get; set; }
            public string OlympiadName { get; set; }
            public decimal MaximumCost { get; set; }
            public decimal MaximumConcessionCost { get; set; }
            public IEnumerable<EventVm> Events { get; set; }
        }

        public OlympiadEventsVm GetOlympiadEvents()
        {
            var context = DataEntitiesProvider.Provide();
            var vm = new OlympiadEventsVm();

            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var entryFees = context.Fees.ToDictionary(x => x.Code, x => x);

            vm.OlympiadName = currentOlympiad.FullTitle();
            vm.Year = currentOlympiad.YearOf.Value;
            vm.MaximumCost = currentOlympiad.MaxFee.Value;
            vm.MaximumConcessionCost = currentOlympiad.MaxCon.Value;
            vm.Events = currentOlympiad.Events
                .Where(x => x.No_Sessions > 0)
                .Select(e =>
                new OlympiadEventsVm.EventVm()
                {
                    Code = e.Code,
                    Name = e.Mind_Sport,
                    Cost = entryFees[e.Entry_Fee].Adult.Value,
                    ConcessionCost = entryFees[e.Entry_Fee].Concession.Value
                }).ToList();

            return vm;
        }

        [XmlType(TypeName = "Event")]  
        public class EventContestantsVm
        {
            [XmlType(TypeName = "Contestant")]  
            public class ContestantVm
            {
                public int ContestantId { get; set; }
                public string Name { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Email { get; set; }
                public string Phone { get; set; }
                public string OnlineNicknames { get; set; }
                public string BgaNickname { get; set; }

                public string AllOnlineNicknames { get; set; }
                public string DiscordNickname { get; set; }
                public string Whatsapp { get; set; }
                public bool IsJunior { get; set; }
                public bool IsSenior { get; set; }
                public int SeedingPoints { get; set; }
                public int RatingPoints { get; set; }
                public string Nationality { get; set; }
                
                public int EntryId { get; set; }
            }

            public string EventCode { get; set; }
            public string EventName { get; set; }
            public List<ContestantVm> Contestants { get; set; }
        }

        [XmlType(TypeName = "Players")]
        public class SwissManagerEventContestantsVm : List<SwissManagerEventContestantsVm.ContestantVm>
        {
            [XmlType(TypeName = "Player")]
            public class ContestantVm
            {
                [XmlAttribute]
                public int NatId { get; set; }
                [XmlAttribute]
                public string Firstname { get; set; }
                [XmlAttribute]
                public string Lastname { get; set; }
                [XmlAttribute]
                public int PlayerUniqueId { get; set; }
            }
        }


        public EventContestantsVm GetEventContestants(string eventCode)
        {
            var context = DataEntitiesProvider.Provide();
            var vm = new EventContestantsVm();

            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var evt = currentOlympiad.Events.SingleOrDefault(x => x.Code == eventCode);
            var elos = context.Ratings.Where(s => s.EventCode == eventCode).ToDictionary(s => s.ContestantId, s => s.QuasiEloRating);
            var seedings = context.Seedings.Where(s => s.EventCode == eventCode).ToDictionary(s => s.ContestantId, s => s.Score);

            if (evt == null) throw new ArgumentOutOfRangeException("Unrecognised event");

            vm.EventCode = eventCode;
            vm.EventName = evt.Mind_Sport;
            vm.Contestants = evt.Entrants.Select(e =>
                new EventContestantsVm.ContestantVm()
                {
                    ContestantId = e.Name.Mind_Sport_ID,
                    Name = e.Name.FullName(),
                    FirstName = e.Name.Firstname,
                    LastName = e.Name.Lastname,
                    Email = e.Name.email,
                    Phone = $"Day: {e.Name.DayPhone} Evening: {e.Name.EvePhone}",
                    OnlineNicknames = e.Name.OnlineNicknames,
                    BgaNickname = e.Name.BgaNickname,
                    AllOnlineNicknames = e.Name.AllOnlineNicknames,
                    DiscordNickname = e.Name.DiscordNickname,
                    Whatsapp = e.Name.Whatsapp ? "yes" : "no",
                    IsJunior = e.Name.IsJuniorForOlympiad(currentOlympiad),
                    IsSenior = e.Name.IsSeniorForOlympiad(currentOlympiad),
                    EntryId = e.EntryNumber,
                    Nationality = e.Name.Nationality ?? ""
                }).ToList();

            foreach (var c in vm.Contestants)
            {
                c.SeedingPoints = GetSeeding(seedings, c.ContestantId);
                c.RatingPoints = GetElo(elos, c.ContestantId);
            }

            vm.Contestants.Sort(new Comparison<EventContestantsVm.ContestantVm>((first, second) 
                    => second.RatingPoints.CompareTo(first.RatingPoints)));
            return vm;
        }

        private int GetSeeding(Dictionary<int, int?> seedings, int contestantId)
        {
            return (seedings.ContainsKey(contestantId) && seedings[contestantId].HasValue)
                ? seedings[contestantId].Value
                : 0;
        }

        private int GetElo(Dictionary<int, int> elos, int contestantId)
        {
            return (elos.ContainsKey(contestantId))
                ? elos[contestantId]
                : 1500 + _random.Next(50);
        }


        public SwissManagerEventContestantsVm GetSwissManagerEventContestants(string eventCode)
        {
            var context = DataEntitiesProvider.Provide();
            var vm = new SwissManagerEventContestantsVm();

            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var evt = currentOlympiad.Events.SingleOrDefault(x => x.Code == eventCode);

            if (evt == null) throw new ArgumentOutOfRangeException("Unrecognised event");

            int i = 1;
            vm.AddRange(evt.Entrants.Select(e =>
                new SwissManagerEventContestantsVm.ContestantVm()
                {
                    NatId = e.Name.Mind_Sport_ID,
                    Firstname = e.Name.Firstname,
                    Lastname = e.Name.Lastname,
                    PlayerUniqueId = i++
                }).ToList());

            return vm;
        }

        public void AddEventEntry(string json)
        {
            var context = DataEntitiesProvider.Provide();

            var entryJson = new EntryJson()
            {
                JsonText = json,
                SubmittedDate = DateTime.UtcNow
            };

            context.EntryJsons.Add(entryJson);
            context.SaveChanges();
        }
    }
}
