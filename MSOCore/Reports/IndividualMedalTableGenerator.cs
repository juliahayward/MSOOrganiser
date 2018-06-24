using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOCore.Extensions;

namespace MSOCore.Reports
{
    public class IndividualMedalTableGenerator
    {
        public class MedalTableVm
        {
            public int Page { get; set; }
            public bool HasPrevious { get; set; }
            public bool HasNext { get; set; }
            public int FirstIndex { get; set; }
            public IEnumerable<MedalTableEntryVm> Entries { get; set; }
            public int PreviousPage { get { return Page - 1; } }
            public int NextPage { get { return Page + 1; } }

            public class MedalTableEntryVm
            {
                public int ContestantId { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Name { get { return string.Format("{0} {1}", FirstName, LastName); } }
                public string Nationality { get; set; }
                public string Flag
                {
                    get
                    {
                        return Nationality.GetFlag();
                    }
                }
                public int Golds { get; set; }
                public int Silvers { get; set; }
                public int Bronzes { get; set; }
                public int Total { get { return Golds + Silvers + Bronzes; } }
            }
        }

        public MedalTableVm GetItems(int page, int pageSize)
        {
            var medals = new[] { "Gold", "Silver", "Bronze" };

            var context = DataEntitiesProvider.Provide();
            var medalCounts = context.Entrants.Where(x => medals.Contains(x.Medal))
                .GroupBy(x => x.Mind_Sport_ID.Value, x => x)
                .Select(x => new 
                { 
                    ContestantId = x.Key, 
                    Golds = x.Count(e => e.Medal == "Gold"),
                    Silvers = x.Count(e => e.Medal == "Silver"),
                    Bronzes = x.Count(e => e.Medal == "Bronze")
                });

            var entries = medalCounts.Join(context.Contestants, 
                mc => mc.ContestantId, c => c.Mind_Sport_ID,
                (mc, c) => new MedalTableVm.MedalTableEntryVm
                {
                    ContestantId = mc.ContestantId,
                    Golds = mc.Golds,
                    Silvers = mc.Silvers,
                    Bronzes = mc.Bronzes,
                    FirstName = c.Firstname,
                    LastName = c.Lastname,
                    Nationality = c.Nationality ?? "default"
                })
                .ToList()
                .OrderByDescending(m => m.Golds).ThenByDescending(x => x.Silvers).ThenByDescending(x => x.Bronzes);

            return new MedalTableVm()
            {
                Entries = entries.Skip((page - 1) * pageSize).Take(pageSize),
                FirstIndex = 1 + (page - 1) * pageSize,
                HasNext = (entries.Count() > page * pageSize),
                HasPrevious = (page > 1),
                Page = page
            };
        }
    }
}
