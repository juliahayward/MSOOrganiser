using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class GamePlanReportGenerator
    {
        public class GamePlanReportVm
        {
            public class EventVm
            {
                public string Code { get; set; }
                public string Name { get; set; }
                public int GameId { get; set; }
                public int SequenceNumber { get; set; }
                public string SequenceNumberStr { get { return SequenceNumber.ToString(); } }
                public bool InPentamind { get; set; }
                public string InPentamindStr { get { return (InPentamind) ? "Yes" : "No"; } }
                public int NumberInTeam { get; set; }
                public string NumberInTeamStr { get { return NumberInTeam.ToString(); } }
                public IEnumerable<string> Arbiters { get; set; }
                public decimal PrizeFund { get; set; }
                public string PrizeGiving { get; set; }
                public string Notes { get; set; }
                public string EntryFeeCode { get; set; }
                public string Location { get; set; }
                public int Participants { get; set; }
                public int NumSessions { get; set; }
                public DateTime? StartDate { get; set; }
                public TimeSpan? StartTime { get; set; }
                public DateTime? EndDate { get; set; }
                public TimeSpan? EndTime { get; set; }

                public string DatesString
                {
                    get
                    {
                        if (!StartDate.HasValue) return "";
                        return StartDate.Value.Add(StartTime.Value).ToString("dd MMM HH:mm")
                            + "-" + EndDate.Value.Add(EndTime.Value).ToString("HH:mm");
                    }
                }
            }

            public class GameVm
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string Contacts { get; set; }
                public string Equipment { get; set; }
                public string Rules { get; set; }
            }

            public class FeeVm
            {
                public string Code { get; set; }
                public decimal Fee { get; set; }
                public decimal Concession { get; set; }
            }

            public string OlympiadName { get; set; }
            public Dictionary<string, GameVm> Games { get; set; }
            public List<EventVm> Events { get; set; }
            public Dictionary<string, FeeVm> Fees { get; set; }
        }

        public GamePlanReportVm GetItemsForLatest()
        {
            var vm = new GamePlanReportVm();
            var context = new DataEntities();
            var currentOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            vm.OlympiadName = currentOlympiad.FullTitle();

            vm.Fees = context.Fees.ToDictionary(x => x.Code, x => new GamePlanReportVm.FeeVm()
            {
                Code = x.Code, 
                Fee = x.Adult.HasValue ? x.Adult.Value : 0m,
                Concession = x.Concession.HasValue ? x.Concession.Value : 0m
            });

            var query = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id)
                .Where(x => x.Number > 0)
                .Select(e => new GamePlanReportVm.EventVm
                {
                    Code = e.Code,
                    Name = e.Mind_Sport,
                    SequenceNumber = e.Number,
                    GameId = e.Game.Id,
                    InPentamind = e.Pentamind.HasValue && e.Pentamind.Value,
                    NumberInTeam = e.Number_in_Team.HasValue ? e.Number_in_Team.Value : 0,
                    Arbiters = e.Arbiters.Select(x => x.Name.Firstname + " " + x.Name.Lastname),
                    PrizeGiving = e.Prize_Giving,
                    PrizeFund = e.Prize_fund.HasValue ? e.Prize_fund.Value : 0m,
                    Notes = e.Notes,
                    EntryFeeCode = e.Entry_Fee,
                    Location = e.Location,
                    NumSessions = e.Event_Sess.Count(),
                    StartDate = e.Event_Sess.Min(sess => sess.Date.Value),
                    StartTime = e.Event_Sess.Min(sess => sess.Session1.StartTime.Value),
                    EndDate = e.Event_Sess.Max(sess => sess.Date.Value),
                    EndTime = e.Event_Sess.Max(sess => sess.Session1.FinishTime.Value),
                    Participants = e.Entrants.Count()
                });
                
            vm.Events = query.ToList();

            vm.Games = context.Games
                .Where(x => x.Events.Any(e => e.OlympiadId == currentOlympiad.Id && e.Number > 0))
                .ToDictionary(g => g.Code, g => new GamePlanReportVm.GameVm
                {
                    Id = g.Id,
                    Name = g.Mind_Sport,
                    Contacts = g.Contacts,
                    Rules = g.Rules,
                    Equipment = g.Equipment
                });

            return vm;
        }
    }
}
