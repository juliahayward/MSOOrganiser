using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class MedalFormsGenerator
    {
        public class MedalFormsVm
        {
            public class EntrantVm
            {
                public string Name { get; set; }
                public string Nationality { get; set; }
                public string Junior { get; set; }
            }

            public class EventVm
            {
                public string Title { get; set; }
                public string Code { get; set; }
                public string SequenceNumber { get; set; }
                public string Location { get; set; }
                public string PrizeGiving { get; set; }
                public string Prize1 { get; set; }
                public string Prize2 { get; set; }
                public string Prize3 { get; set; }
                public string JuniorPrizes { get; set; }
                public string OtherPrizes { get; set; }
                public IEnumerable<EntrantVm> Entrants { get; set; }

                public DateTime? StartDate { get; set; }
                public TimeSpan? StartTime { get; set; }
                public DateTime? EndDate { get; set; }
                public TimeSpan? EndTime { get; set; }

                public string StartDateString
                {
                    get
                    {
                        if (!StartDate.HasValue) return "";
                        return StartDate.Value.Add(StartTime.Value).ToString("dd MMM yyyy HH:mm");
                    }
                }

                public string EndDateString
                {
                    get
                    {
                        if (!EndDate.HasValue) return "";
                        return EndDate.Value.Add(EndTime.Value).ToString("dd MMM yyyy HH:mm");
                    }
                }
            }

            public string OlympiadTitle { get; set; }
            public IEnumerable<EventVm> Events { get; set; }
        }

        public MedalFormsVm GetItemsForLatest(string eventCode)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            var events = currentOlympiad.Events.Where(x => x.Code == eventCode);

            var items = GetItemsForLatest(context, currentOlympiad, events); 
            items.OlympiadTitle = currentOlympiad.FullTitle();
            return items;
        }

        public MedalFormsVm GetItemsForLatest(DateTime startDate, DateTime endDate)
        {
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            var events = currentOlympiad.Events
                .Where(x => x.Event_Sess.Any(sess => sess.Date >= startDate && sess.Date <= endDate));

            var items = GetItemsForLatest(context, currentOlympiad, events);
            items.OlympiadTitle = currentOlympiad.FullTitle();
            return items;
        }

        private MedalFormsVm GetItemsForLatest(DataEntities context, Olympiad_Info currentOlympiad, IEnumerable<Event> events)
        {
            var vm = new MedalFormsVm()
            {
                Events = events.Select(x => new MedalFormsVm.EventVm()
                    {
                        Title = x.Mind_Sport,
                        Code = x.Code,
                        SequenceNumber = x.Number.ToString(),
                        StartDate = x.Event_Sess.Min(s => s.Date.Value),
                        EndDate = x.Event_Sess.Max(s => s.Date.Value),
                        StartTime = x.Event_Sess.Min(s => s.Session1.StartTime),
                        EndTime = x.Event_Sess.Max(s => s.Session1.FinishTime),
                        Location = x.Location,
                        PrizeGiving = x.Prize_Giving,
                        Prize1 = x.C1st_Prize == null ? "" : "£" + x.C1st_Prize,
                        Prize2 = x.C2nd_Prize == null ? "" : "£" + x.C2nd_Prize,
                        Prize3 = x.C3rd_Prize == null ? "" : "£" + x.C3rd_Prize,
                        OtherPrizes = x.Other_Prizes == null ? "" : x.Other_Prizes,
                        JuniorPrizes = string.Join(", ", (new string[] { x.JNR_1st_Prize, x.JNR_2nd_Prize, x.JNR_3rd_Prize, x.JNR_Other_Prizes}).Where(p => p != null)),
                    })
                    .ToList()
            };

            var juniorDate = DateTime.Now.AddYears(-currentOlympiad.JnrAge.Value - 1);

            foreach (var evt in vm.Events)
            {
                var entrants = context.Entrants.Where(x => x.OlympiadId == currentOlympiad.Id && x.Game_Code == evt.Code)
                    .Join(context.Contestants, e => e.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                    .ToList()
                    .Select(ec => new MedalFormsVm.EntrantVm()
                    {
                        Name = ec.c.Firstname + " " + ec.c.Lastname,
                        Nationality = ec.c.Nationality,
                        Junior = (ec.c.DateofBirth.HasValue && ec.c.DateofBirth > juniorDate) ? "JNR" : ""
                    })
                    .OrderBy(x => x.Name);

                evt.Entrants = entrants;
            }

            return vm;
        }
    }
}
