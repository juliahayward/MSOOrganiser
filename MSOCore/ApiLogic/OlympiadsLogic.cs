using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Calculators;

namespace MSOCore.ApiLogic
{
    public class OlympiadsLogic
    {
        public class OlympiadListVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Venue { get; set; }
            public DateTime StartDate { get; set; }
        }

        public class OlympiadVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Venue { get; set; }
            public DateTime StartDate { get; set; }

            public IEnumerable<EventVm> Events { get; set; }

            public class EventVm
            {
                public int EventId { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
            }
        }

        public class EventVm
        {
            public int OlympiadId { get; set; }
            public int OlympiadYear { get; set; }
            public bool Editable { get; set; }
            public int EventId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public int NumberInTeam { get; set; }

            public IEnumerable<EntrantVm> Entrants { get; set; }

            public int TotalEntrants { get { return Entrants.Count(); } }
            public int ResultEntrants { get { return Entrants.Count(x => !string.IsNullOrEmpty(x.Score)); } }
            public int NoResultEntrants {  get { return Entrants.Count(x => !x.Absent && string.IsNullOrEmpty(x.Score)); } }
            public int AbsentEntrants { get { return Entrants.Count(x => x.Absent); } }

            public class EntrantVm
            {
                public int EntryNumber { get; set; }
                public int ContestantId { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Partner { get; set; }
                public string Medal { get; set; }
                public string MedalStyle => Medal.ToLower();
                public string GoldSelected { get { return Medal == "Gold" ? "selected" : ""; } }
                public string SilverSelected { get { return Medal == "Silver" ? "selected" : ""; } }
                public string BronzeSelected { get { return Medal == "Bronze" ? "selected" : ""; } }
                public string JuniorMedal { get; set; }
                public string JuniorMedalStyle => JuniorMedal.Replace(" JNR", "").ToLower();
                public string JGoldSelected { get { return JuniorMedal == "Gold JNR" ? "selected" : ""; } }
                public string JSilverSelected { get { return JuniorMedal == "Silver JNR" ? "selected" : ""; } }
                public string JBronzeSelected { get { return JuniorMedal == "Bronze JNR" ? "selected" : ""; } }
                public int? Rank { get; set; }
                public string Score { get; set; }
                public bool Absent { get; set; }
                public string AbsentChecked { get { return Absent ? "checked" : ""; } }
                public string Tiebreak { get; set; }
                public double? Pentamind { get; set; }
                public string PentamindString { get { return Pentamind.HasValue ? Pentamind.Value.ToString("F2") : ""; } }
                public bool IsJunior { get; set; }

                public string Junior { get { return IsJunior ? "JNR" : ""; } }

                public string FullName() { return FirstName + " " + LastName.ToUpper(); }

                public string OnlineNicknames { get; set; }
            }
        }

        public class UpdateEventModel
        {
            public int EventId { get; set; }

            public IList<EntrantVm> Entrants { get; set; }

            public class EntrantVm : IPentaCalculable
            {
                public int EntryNumber { get; set; }
                public string Medal { get; set; }
                public string JuniorMedal { get; set; }
                public string Score { get; set; }
                public bool Absent { get; set; }
                public string Tiebreak { get; set; }
                public int Rank { get; set; }
                public float PentaScore { get; set; }
            }

            public void Validate()
            {
                foreach (var entrant in Entrants)
                {
                    if (entrant.Absent && !(string.IsNullOrEmpty(entrant.Score)))
                        throw new InvalidOperationException($"Entrants cannot be both absent and have a score");
                    if (entrant.Absent && !(string.IsNullOrEmpty(entrant.Tiebreak)))
                        throw new InvalidOperationException($"Entrants cannot be both absent and have a tiebreak");
                }
            }
        }

        public IEnumerable<OlympiadListVm> GetOlympiads()
        {
            var context = DataEntitiesProvider.Provide();

            return context.Olympiad_Infoes.Select(x => new OlympiadListVm()
            {
                Id = x.Id,
                Name = x.Number + " " + x.Title,
                Venue = x.Venue,
                StartDate = x.StartDate.Value
            }).OrderByDescending(x => x.StartDate);
        }

        public int GetCurrentOlympiadId()
        {
            var context = DataEntitiesProvider.Provide();
            return context.Olympiad_Infoes.Single(x => x.Current).Id;
        }

        public OlympiadVm GetOlympiad(int id)
        {
            var context = DataEntitiesProvider.Provide();

            return context.Olympiad_Infoes.Where(x => x.Id == id).Select(x => new OlympiadVm()
            {
                Id = x.Id,
                Name = x.Number + " " + x.Title,
                Venue = x.Venue,
                StartDate = x.StartDate.Value,
                Events = x.Events.Select(e => new OlympiadVm.EventVm()
                {
                    Code = e.Code,
                    Name = e.Mind_Sport,
                    EventId = e.EIN
                }).OrderBy(e => e.Code)
            }).First();
        }

        /// <summary>
        /// Retrieve all the data about an event needed to display it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EventVm GetEvent(int id)
        {
            var context = DataEntitiesProvider.Provide();

            var e = context.Events.Single(x => x.EIN == id);
            var olympiad = e.Olympiad_Info;

            return new EventVm()
            {
                OlympiadId=e.OlympiadId,
                OlympiadYear = e.Olympiad_Info.YearOf.Value,
                EventId = id,
                Code = e.Code,
                Name = e.Mind_Sport,
                NumberInTeam = e.Number_in_Team,
                Entrants = e.Entrants.Select(en => new EventVm.EntrantVm()
                {
                    EntryNumber = en.EntryNumber,
                    ContestantId = en.Name.Mind_Sport_ID,
                    IsJunior = en.Name.IsJuniorForOlympiad(olympiad),
                    FirstName = en.Name.Firstname,
                    LastName = en.Name.Lastname,
                    Partner = en.Partner,
                    Medal = en.Medal ?? "",
                    JuniorMedal = en.JuniorMedal ?? "",
                    Rank = en.Rank,
                    Score = en.Score,
                    Absent = en.Absent,
                    Tiebreak = en.Tie_break,
                    Pentamind = en.Penta_Score,
                    OnlineNicknames = en.Name.OnlineNicknames
                }).OrderBy(x => x.Absent).ThenBy(x => string.IsNullOrEmpty(x.Score) ? 0 : 1)
                    .ThenBy(x => x.Rank).ThenByDescending(x=>x.Score)
                    .ThenByDescending(x=>x.Tiebreak)
                    .ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
            };
        }

        public void UpdateEvent(UpdateEventModel model)
        {
            var context = DataEntitiesProvider.Provide();
            var evt = context.Events.SingleOrDefault(x => x.EIN == model.EventId);
            if (evt == null) throw new ArgumentOutOfRangeException("Event ID " + model.EventId + " not recognised");

            // TODO - valiudate

            // Calculate ranks and pentamind points
            var rankCalculator = RankCalculatorFactory.Get(evt);
            if (rankCalculator.CanCalculate(model.Entrants))
            {
                // TODO high-score-is-best
                rankCalculator.Calculate(evt.Number_in_Team, true, model.Entrants);

                // TODO some nasty hard coding - also these big events can't be done via the desktop
                int overridingNumberOfEntrants = 0;
                if (evt.OlympiadId == 25 && evt.Code == "POTH") overridingNumberOfEntrants = 1049;
                if (evt.OlympiadId == 25 && evt.Code == "POOM") overridingNumberOfEntrants = 499;
                if (evt.OlympiadId == 25 && evt.Code == "PO7S") overridingNumberOfEntrants = 339;
                if (evt.OlympiadId == 25 && evt.Code == "POHU") overridingNumberOfEntrants = 704;
                if (evt.OlympiadId == 25 && evt.Code == "PO5D") overridingNumberOfEntrants = 258;
                if (evt.OlympiadId == 25 && evt.Code == "POOH") overridingNumberOfEntrants = 332;

                var pentaCalculator = new Penta2018Calculator();
                pentaCalculator.Calculate(evt.Number_in_Team, model.Entrants, evt.Pentamind, evt.PentamindFactor, overridingNumberOfEntrants);
            }
            // TODO - events with partners

            // Do the saving - see EventPanel. We'remissing bits such as the EventIndexer if only updating results
            // Event properties
            // Entrants
            // Entrants are *not* added here but their ranks etc are
            foreach (var e in model.Entrants)
            {
                var entrant = context.Entrants.First(x => x.EntryNumber == e.EntryNumber);
                entrant.Absent = e.Absent;
                entrant.Medal = (string.IsNullOrEmpty(e.Medal)) ? null : e.Medal;
                entrant.JuniorMedal = (string.IsNullOrEmpty(e.JuniorMedal)) ? null : e.JuniorMedal;
                entrant.Rank = e.Rank;
                entrant.Score = (string.IsNullOrWhiteSpace(e.Score)) ? null : e.Score;
                if (e.Rank > 0) entrant.Penta_Score = e.PentaScore; else entrant.Penta_Score = null;
                // zero length constraint
                entrant.Tie_break = (e.Tiebreak == "") ? null : e.Tiebreak;
                // TODO entrant.Partner = e.TeamOrPair;
            }

            context.SaveChanges();
        }
    }
}
