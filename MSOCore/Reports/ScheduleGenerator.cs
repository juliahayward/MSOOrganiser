using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class ScheduleGenerator
    {
        public class DayScheduleVm
        {
            public DateTime DateValue { get; set; }
            public string Date { get { return DateValue.ToString("dddd dd MMMM"); } }
            public List<ScheduleEventVm> Events { get; set; }
        }

        public class ScheduleEventVm
        {
            public string Name { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public bool HasAMBadge { get; set; }
            public bool HasPMBadge { get; set; }
            public bool HasEveBadge { get; set; }
            public string Times { get { return Start.ToString("HH:mm") + "-" + End.ToString("HH:mm"); } }
        }

        public IList<DayScheduleVm> GetCompleteSchedule()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);

            var dates = olympiad.Events.SelectMany(e => e.Event_Sess)
                .Select(es => es.Date)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new DayScheduleVm() { DateValue = x.Value })
                .ToList();

            var Mornings = new[] { "2017AM", "2017D", "20171Hyb" };
            var Afternoons = new[] { "2017PM", "2017D", "2017LD", "20171Hyb", "20172Hyb" };
            var Evenings = new[] { "2017E", "2017LD", "20172Hyb" };

            foreach (var date in dates)
            {
                date.Events = olympiad.Events
                    .Where(x => x.Event_Sess.Any(s => s.Date == date.DateValue))
                    .OrderBy(x => x.Start).ThenBy(x => x.End)
                    .Select(e => new ScheduleEventVm()
                    {
                        Name = e.Mind_Sport,
                        HasAMBadge = e.Event_Sess.Any(s => Mornings.Contains(s.Session)),
                        HasPMBadge = e.Event_Sess.Any(s => Afternoons.Contains(s.Session)),
                        HasEveBadge = e.Event_Sess.Any(s => Evenings.Contains(s.Session))
                    })
                    .ToList();
            }

            return dates;
        }

        public IList<ScheduleEventVm> GetDaySchedule(DateTime today)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);

            return olympiad.Events
                    .Where(x => x.Event_Sess.Any(s => s.Date == today))
                    .OrderBy(x => x.Start).ThenBy(x => x.End)
                    .Select(e => new ScheduleEventVm()
                    {
                        Name = e.Mind_Sport,
                        Start = e.Start.Value,
                        End = e.End.Value
                    })
                    .ToList();
        }
    }
}
