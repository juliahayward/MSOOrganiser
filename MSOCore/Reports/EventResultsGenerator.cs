using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class EventResultsGenerator
    {
        public class EventResultsVm
        {
            public int Year { get; set; }
            public string EventCode { get; set; }
            public string EventName { get; set; }
            public string OlympiadName { get; set; }

            public string PointsType { get; set; }
            public IEnumerable<EntrantVm> Entrants { get; set; }

            public class EntrantVm
            {
                public string Medal { get; set; }
                public string JuniorMedal { get; set; }
                public int Rank { get; set; }
                public string Score { get; set; }
                public double PentaScore { get; set; }
                public int ContestantId { get; set; }

                public string Name { get; set; }
                public string Nationality { get; set; }
                public string Flag { get { return Nationality.GetFlag(); } }
            }
        }

        public class EventsIndexEventVm
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public bool HasResults { get; set; }
            public bool Started { get; set; }
            public bool Finished { get; set; }
        }

        public IEnumerable<EventsIndexEventVm> GetEventsIndex()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.FirstOrDefault(x => x.Current);
            if (olympiad == null)
                throw new ArgumentNullException("No current olympiad");

            return olympiad.Events.Where(x => x.Entrants.Any())
                .Select(x => new EventsIndexEventVm()
                {
                    Code = x.Code,
                    Name = x.Mind_Sport,
                    HasResults = x.Entrants.Any(e => e.Penta_Score.HasValue && e.Penta_Score.Value > 0),
                    Started = (x.Start < DateTime.Now),
                    Finished = (x.End < DateTime.Now)
                })
                .OrderBy(x => x.Code)
                .ToList();
        }

        public EventResultsVm GetModel(int? year, string eventCode)
        {
            var context = DataEntitiesProvider.Provide();

            // Warning - this will go wonky when I undo the 2007/7002 hack
            var olympiad = (year == null) 
                ? context.Olympiad_Infoes.FirstOrDefault(x => x.Current)
                : context.Olympiad_Infoes.FirstOrDefault(x => x.YearOf == year);
            if (olympiad == null)
                throw new ArgumentOutOfRangeException("Year " + year + " is not an olympiad year");

            var retval = new EventResultsVm { Year = olympiad.YearOf.Value, EventCode = eventCode, OlympiadName = olympiad.FullTitle(), PointsType = olympiad.Ruleset };

            var evt = context.Events.FirstOrDefault(x => x.OlympiadId == olympiad.Id && x.Code == eventCode);
            if (evt == null)
                throw new ArgumentOutOfRangeException("Event " + eventCode + " did not take place in " + year);

            var gpMetaEvents = new[] { "GPC", "WGPC", "JGPC", "SGPC", "DRGPC", "MPGPC", "POGPC", "CHGPC", "BAGPC", "ABGPC", "IIGPC" };
            var metaEvents = context.MetaGameDefinitions.Select(x => x.Event.Code).Distinct();
            bool isPentamind = eventCode.StartsWith("PE") || metaEvents.Contains(eventCode) || gpMetaEvents.Contains(eventCode);

            retval.EventName = evt.Mind_Sport;
            retval.Entrants = evt.Entrants
                .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Where(ec => ec.e.Rank.HasValue && ec.e.Rank.Value > 0)
                .Select(ec => new EventResultsVm.EntrantVm()
                {                
                    ContestantId = ec.c.Mind_Sport_ID,
                    Medal = ec.e.Medal ?? "",
                    JuniorMedal = ec.e.JuniorMedal ?? "",
                    Score = (isPentamind) ? "" : ec.e.Score,
                    PentaScore = (isPentamind) ? double.Parse(ec.e.Score) : (double)ec.e.Penta_Score,
                    Rank = ec.e.Rank.Value,
                    Name = ec.c.FullName(),
                    Nationality = ec.c.Nationality ?? "default",
                })
                .ToList()
                .OrderBy(x => x.Rank)
                    .ThenBy(x => x.Medal.MedalRank()).ThenBy(x => x.JuniorMedal.MedalRank());

            return retval;
        }

        public EventResultsVm GetEntrantsModel(string eventCode)
        {
            var context = DataEntitiesProvider.Provide();

            var olympiad = context.Olympiad_Infoes.FirstOrDefault(x => x.Current);
            if (olympiad == null)
                throw new ArgumentOutOfRangeException("No current olympiad");

            var retval = new EventResultsVm { Year = olympiad.YearOf.Value, EventCode = eventCode, OlympiadName = olympiad.FullTitle() };

            var evt = context.Events.FirstOrDefault(x => x.OlympiadId == olympiad.Id && x.Code == eventCode);
            if (evt == null)
                throw new ArgumentOutOfRangeException("Unknown event code " + eventCode);

            retval.EventName = evt.Mind_Sport;
            retval.Entrants = evt.Entrants
                .Select(e => new EventResultsVm.EntrantVm()
                {
                    ContestantId = e.Name.Mind_Sport_ID,
                    Name = e.Name.FullName(),
                    Nationality = e.Name.Nationality ?? "default",
                })
                .ToList()
                .OrderBy(x => x.Name);

            return retval;
        }
    }
}
