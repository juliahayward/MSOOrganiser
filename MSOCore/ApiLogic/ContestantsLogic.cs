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
            public string FullName { get; set; }
            public int ContestantId { get; set; }

            public IEnumerable<EventVm> Events { get; set; }

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
            vm.FullName = contestant.FullName();

            var entries = contestant.Entrants
                .Join(context.Events, e => e.Game_Code, g => g.Code, (e, g) => new { e = e, g = g })
                .Where(x => x.e.OlympiadId == olympiad.Id && x.g.OlympiadId == olympiad.Id)
                .OrderBy(x => x.e.Game_Code).ToList();

            vm.Events = entries.Select(e => new ContestantVm.EventVm()
                {
                    EventId = e.e.EventId.Value,
                    Absent = e.e.Absent,
                    Code = e.e.Game_Code,
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
    }
}
