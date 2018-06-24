using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class LocationUseReportGenerator
    {
        public class LocationUseReportVm
        {
            public class LocationVm
            {
                public string Name { get; set; }
            }

            public class LocationNameOnlyComparer : IEqualityComparer<LocationVm>
            {
                public bool Equals(LocationVm x, LocationVm y)
                {
                    return x.Name == y.Name;
                }

                public int GetHashCode(LocationVm obj)
                {
                    return obj.Name.GetHashCode();
                }
            }

            public class SessionVm
            {
                public string Code { get; set; }
                public TimeSpan StartTime { get; set; }
                public TimeSpan EndTime { get; set; }

                public string Text
                {
                    get
                    {
                        return "Session " + Code + ": " +
                            StartTime.ToStandardString() + "-" + EndTime.ToStandardString();
                    }
                }
            }

            public class SessionVmCodeOnlyComparer : IEqualityComparer<SessionVm>
            {
                public bool Equals(SessionVm x, SessionVm y)
                {
                    return x.Code == y.Code;
                }

                public int GetHashCode(SessionVm obj)
                {
                    return obj.Text.GetHashCode();
                }
            }

            public class EventVm
            {
                public string Name { get; set; }
                public string Code { get; set; }
                public int NumParticipants { get; set; }
                public DateTime Date { get; set; }
                public IEnumerable<string> SessionCodes { get; set; }
                public string Location { get; set; }
                public int SequenceNumber { get; set; }
            }

            public string OlympiadName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            public IEnumerable<LocationVm> Locations { get; set; }
            public IEnumerable<SessionVm> Sessions { get; set; }
            public IEnumerable<EventVm> Events { get; set; }
        }

        public LocationUseReportVm GetItemsForLatest()
        {
            var vm = new LocationUseReportVm();
            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            vm.OlympiadName = currentOlympiad.FullTitle();
            vm.StartDate = currentOlympiad.StartDate.Value;
            vm.EndDate = currentOlympiad.FinishDate.Value;

            vm.Locations = currentOlympiad.Events
                .Where(x => x.Number > 0 && x.Location != null && x.Event_Sess.Any())
                .Select(x => new LocationUseReportVm.LocationVm { Name = x.Location })
                .Distinct(new LocationUseReportVm.LocationNameOnlyComparer())
                .OrderBy(x => x.Name);

            vm.Sessions = currentOlympiad.Events
                .Where(x => x.Number > 0 && x.Location != null && x.Event_Sess.Any())
                .SelectMany(x => x.Event_Sess)
                .Select(x => new LocationUseReportVm.SessionVm
                {
                    Code = x.Session1.Session1,
                    StartTime = x.Session1.StartTime.Value,
                    EndTime = x.Session1.FinishTime.Value
                })
                .Distinct(new LocationUseReportVm.SessionVmCodeOnlyComparer())
                .OrderBy(x => x.StartTime).ThenBy(x => x.EndTime);

            vm.Events = currentOlympiad.Events
                .Where(x => x.Number > 0 && x.Location != null && x.Event_Sess.Any())
                .Select(x => new LocationUseReportVm.EventVm
                {
                    Code = x.Code,
                    Name = x.Mind_Sport,
                    Location = x.Location,
                    Date = x.Event_Sess.Min(s => s.Date.Value),
                    SessionCodes = x.Event_Sess.Select(s => s.Session1.Session1),
                    NumParticipants = x.Entrants.Count(),
                    SequenceNumber = x.Number
                })
                .ToList();

            return vm;
        }
    }
}
