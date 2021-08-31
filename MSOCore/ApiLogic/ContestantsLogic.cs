using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.ApiLogic
{
    public class ContestantsLogic
    {
        public class ContestantVm
        {
            public bool IsEditable { get; set; }
            public string Title { get; set; }
            public string FullName { get; set; }
            public string Firstname { get; set; }
            public string Initials { get; set; }
            public string Lastname { get; set; }

            public bool IsMale { get; set; }
            // Todo shouldn't be a binary
            public bool IsFemale { get { return !IsMale; } }
            public int ContestantId { get; set; }
            public string Nationality { get; set; }

            public string OnlineNicknames { get; set; }
            public string BgaNickname { get; set; }

            public string Notes { get; set; }
            public IEnumerable<EventVm> Events { get; set; }
            public IEnumerable<string> Nationalities { get; set; }

            public DateTime? DateOfBirth { get; set; }

            public string DisplayDateOfBirth => DateOfBirth?.ToString("dd/MM/yyyy") ?? "";

            public class EventVm
            {
                public int EventId { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
                public decimal Fee { get; set; }
                public string FeeString => (Fee > 0) ? "£" + Fee.ToString("F2") : "";
                public string Medal { get; set; }
                public string MedalStyle => Medal.ToLower();
                public string JuniorMedal { get; set; }
                public string JuniorMedalStyle => JuniorMedal.Replace(" JNR", "").ToLower();
                public string Partner { get; set; }
                public double? Penta { get; set; }
                public string PentaString => (Penta.HasValue) ? Penta.Value.ToString("F2") : "";
                public int Rank { get; set; }
                public string RankString => (Rank > 0) ? Rank.ToString() : "";
                public bool Absent { get; set; }
                public string AbsentString => (Absent) ? "Yes" : "";
            }
        }

        public ContestantVm GetContestant(int id)
        {
            var context = DataEntitiesProvider.Provide();
            var vm = new ContestantVm();

            var contestant = context.Contestants.Single(x => x.Mind_Sport_ID == id);
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);

            vm.ContestantId = id;
            vm.Title = contestant.Title;
            vm.Firstname = contestant.Firstname;
            vm.Initials = contestant.Initials;
            vm.Lastname = contestant.Lastname;
            vm.FullName = contestant.FullName();
            vm.DateOfBirth = contestant.DateofBirth;
            vm.IsMale = contestant.Male;
            vm.OnlineNicknames = contestant.OnlineNicknames;
            vm.BgaNickname = contestant.BgaNickname;
            vm.Nationality = contestant.Nationality;
            vm.Nationalities = context.Nationalities.Select(x => x.Name).OrderBy(x => x);
            vm.Notes = contestant.Notes;

            var entries = contestant.Entrants
                .Join(context.Events, e => e.EventId, g => g.EIN, (e, g) => new { e = e, g = g })
                .Where(x => x.e.OlympiadId == olympiad.Id && x.g.OlympiadId == olympiad.Id)
                .OrderBy(x => x.g.Code).ToList();

            vm.Events = entries.Select(e => new ContestantVm.EventVm()
            {
                EventId = e.e.EventId.Value,
                Absent = e.e.Absent,
                Code = e.g.Code,
                Name = e.g.Mind_Sport,
                Fee = e.e.Fee,
                //StandardFee = (e.g.Entry_Fee != null) ? fees[e.g.Entry_Fee].Value : 0,
                //IncludedInMaxFee = (e.g.incMaxFee.HasValue && e.g.incMaxFee.Value),
                //IsEvent = (e.g.Number > 0),
                Medal = e.e.Medal ?? "",
                JuniorMedal = e.e.JuniorMedal ?? "",
                Partner = e.e.Partner ?? "",
                Penta = e.e.Penta_Score,
                Rank = e.e.Rank.HasValue ? e.e.Rank.Value : 0,
                // Receipt = e.e.Receipt.Value,
                // TieBreak = e.e.Tie_break ?? "",
                //Date = e.e.Event.Start
            }).OrderBy(x => x.Code);

            return vm;
        }

        public void UpdateContestant(ContestantVm model)
        {
            var context = DataEntitiesProvider.Provide();
            var c = context.Contestants.SingleOrDefault(x => x.Mind_Sport_ID == model.ContestantId);
            if (c == null)
                throw new ArgumentOutOfRangeException("Contestant ID " + model.ContestantId + " not recognised");
            if (!context.Nationalities.Any(x => x.Name == model.Nationality))
                throw new ArgumentOutOfRangeException("Nationality " + model.Nationality + " not recognised");

            c.Title = model.Title;
            c.Firstname = model.Firstname;
            c.Initials = model.Initials;
            c.Lastname = model.Lastname;
            c.DateofBirth = model.DateOfBirth;
            c.Male = model.IsMale;
            c.OnlineNicknames = model.OnlineNicknames;
            c.BgaNickname = model.BgaNickname;
            c.Nationality = model.Nationality;
            c.Notes = model.Notes;

            context.SaveChanges();
        }


        public class ContestantForNameVm
        {
            public string Name { get; set; }
            public string Nickname { get; set; }
            public int Id { get; set; }
        }

        public IEnumerable<ContestantForNameVm> GetContestantsForName(string name)
        {
            var context = DataEntitiesProvider.Provide();
            var contestants = context.Contestants.Where(c =>
                (c.Firstname != null && c.Firstname.Contains(name)) || (c.Lastname != null && c.Lastname.Contains(name)) || 
                (c.BgaNickname != null && c.BgaNickname.Contains(name)) || 
                (c.OnlineNicknames != null && c.OnlineNicknames.Contains(name))
                ).ToList();

            foreach (var c in contestants.OrderBy(x => x.Lastname).ThenBy(x => x.Firstname))
                yield return new ContestantForNameVm { Name = c.FullNameWithInitials(), Nickname = c.AllOnlineNicknames ?? "", Id = c.Mind_Sport_ID };
        }

        public void AddContestantToEvent(int contestantId, int eventId)
        {
            var context = DataEntitiesProvider.Provide();
            var existingEntrants = context.Entrants.Where(e => e.Mind_Sport_ID == contestantId && e.EventId == eventId).ToList();
            if (existingEntrants.Any()) return;  // no need

            var evt = context.Events.First(x => x.EIN == eventId);
            var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);

            var newEntrant = new Entrant()
            {
                EventId = eventId,
                OlympiadId = evt.OlympiadId,
                Name = contestant,
                Absent = false
            };

            context.Entrants.Add(newEntrant);
            context.SaveChanges();
        }

        public void AddContestantWithScoreToEvent(int contestantId, int rank, string score, int eventId)
        {
            var context = DataEntitiesProvider.Provide();
            var existingEntrants = context.Entrants.Where(e => e.Mind_Sport_ID == contestantId && e.EventId == eventId).ToList();
            if (existingEntrants.Any()) return;  // no need

            var evt = context.Events.First(x => x.EIN == eventId);
            var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);

            var newEntrant = new Entrant()
            {
                EventId = eventId,
                OlympiadId = evt.OlympiadId,
                Name = contestant,
                Absent = false,
                Rank = rank,
                Score =  score
            };

            context.Entrants.Add(newEntrant);
            context.SaveChanges();
        }

        public void AddContestantWithScoreToEvent(int contestantId, int rank, string score, string tiebreak, int eventId)
        {
            var context = DataEntitiesProvider.Provide();
            var existingEntrants = context.Entrants.Where(e => e.Mind_Sport_ID == contestantId && e.EventId == eventId).ToList();
            if (existingEntrants.Any()) return;  // no need

            var evt = context.Events.First(x => x.EIN == eventId);
            var contestant = context.Contestants.First(x => x.Mind_Sport_ID == contestantId);

            var newEntrant = new Entrant()
            {
                EventId = eventId,
                OlympiadId = evt.OlympiadId,
                Name = contestant,
                Absent = false,
                Rank = rank,
                Score = score,
                Tie_break = tiebreak
            };

            context.Entrants.Add(newEntrant);
            context.SaveChanges();
        }

        public void AddNewContestantToEvent(string firstName, string lastName, int eventId)
        {
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                throw new ArgumentException("You must specify at least one of first name and last name");

            var context = DataEntitiesProvider.Provide();

            var contestant = new Contestant()
            {
                Firstname = firstName ?? "",
                Lastname = lastName ?? "",
                Male = true         // The default, to exclude unknown people from Women's Pentamind until we have confirmed they are eligible
            };

            context.Contestants.Add(contestant);

            var evt = context.Events.First(x => x.EIN == eventId);

            var newEntrant = new Entrant()
            {
                EventId = eventId,
                OlympiadId = evt.OlympiadId,
                Name = contestant,
                Absent = false
            };

            context.Entrants.Add(newEntrant);
            context.SaveChanges();
        }

        public void AddNewContestantWithScoreToEvent(string firstName, string lastName, string onlineNickname, string country, int rank, string score, int eventId)
        {
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                throw new ArgumentException("You must specify at least one of first name and last name");

            var context = DataEntitiesProvider.Provide();

            var contestant = new Contestant()
            {
                Firstname = firstName ?? "",
                Lastname = lastName ?? "",
                Nationality = country ?? "",
                OnlineNicknames = onlineNickname ?? "",
                Male = true         // The default, to exclude unknown people from Women's Pentamind until we have confirmed they are eligible
            };

            context.Contestants.Add(contestant);

            var evt = context.Events.First(x => x.EIN == eventId);

            var newEntrant = new Entrant()
            {
                EventId = eventId,
                OlympiadId = evt.OlympiadId,
                Name = contestant,
                Score = score,
                Rank = rank,
                Absent = false
            };

            context.Entrants.Add(newEntrant);
            context.SaveChanges();
        }

        public void AddNewContestantWithScoreToEvent(string firstName, string lastName, string onlineNickname, string country, int rank, string score, string tiebreak, int eventId)
        {
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                throw new ArgumentException("You must specify at least one of first name and last name");

            var context = DataEntitiesProvider.Provide();

            var contestant = new Contestant()
            {
                Firstname = firstName ?? "",
                Lastname = lastName ?? "",
                Nationality = country ?? "",
                OnlineNicknames = onlineNickname ?? "",
                Male = true         // The default, to exclude unknown people from Women's Pentamind until we have confirmed they are eligible
            };

            context.Contestants.Add(contestant);

            var evt = context.Events.First(x => x.EIN == eventId);

            var newEntrant = new Entrant()
            {
                EventId = eventId,
                OlympiadId = evt.OlympiadId,
                Name = contestant,
                Score = score,
                Rank = rank,
                Absent = false,
                Tie_break =  tiebreak
            };

            context.Entrants.Add(newEntrant);
            context.SaveChanges();
        }
    }
}
