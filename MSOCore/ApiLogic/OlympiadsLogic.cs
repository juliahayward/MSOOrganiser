using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public int EventId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

            public IEnumerable<EntrantVm> Entrants { get; set; }

            public class EntrantVm
            {
                public int EntrantId { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Medal { get; set; }
                public string JuniorMedal { get; set; }
                public int? Rank { get; set; }
                public string Score { get; set; }
                public bool Absent { get; set; }
                public string Tiebreak { get; set; }
                public double Pentamind { get; set; }

                public string FullName() { return FirstName + " " + LastName.ToUpper(); }
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

        public EventVm GetEvent(int id)
        {
            var context = DataEntitiesProvider.Provide();

            return context.Events.Where(x => x.EIN == id).Select(e => new EventVm()
            {
                EventId = id,
                Code = e.Code,
                Name = e.Mind_Sport,
                Entrants = e.Entrants.Select(en => new EventVm.EntrantVm()
                {
                    EntrantId = en.Mind_Sport_ID.Value,
                    FirstName = en.Name.Firstname,
                    LastName = en.Name.Lastname,
                    Medal = en.Medal,
                    JuniorMedal = en.JuniorMedal,
                    Rank = en.Rank,
                    Score = en.Score,
                    Absent = en.Absent,
                    Tiebreak = en.Tie_break,
                    Pentamind = en.Penta_Score.Value
                }).OrderBy(x => x.Rank).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
            }).First();
        }
    }
}
