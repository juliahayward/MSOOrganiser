using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class IndividualMedalTableGenerator
    {
        public class MedalTableVm
        {
            public int ContestantId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Name { get { return FirstName + " " + LastName; } }
            public string Nationality { get; set; }
            public int Golds { get; set; }
            public int Silvers { get; set; }
            public int Bronzes { get; set; }
        }

        public IEnumerable<MedalTableVm> GetItems()
        {
            var medals = new[] { "Gold", "Silver", "Bronze" };

            var context = new DataEntities();
            var medalCounts = context.Entrants.Where(x => medals.Contains(x.Medal))
                .GroupBy(x => x.Mind_Sport_ID.Value, x => x)
                .Select(x => new 
                { 
                    ContestantId = x.Key, 
                    Golds = x.Count(e => e.Medal == "Gold"),
                    Silvers = x.Count(e => e.Medal == "Silver"),
                    Bronzes = x.Count(e => e.Medal == "Bronze")
                });

            return medalCounts.Join(context.Contestants, 
                mc => mc.ContestantId, c => c.Mind_Sport_ID,
                (mc, c) => new MedalTableVm
                {
                    ContestantId = mc.ContestantId,
                    Golds = mc.Golds,
                    Silvers = mc.Silvers,
                    Bronzes = mc.Bronzes,
                    FirstName = c.Firstname,
                    LastName = c.Lastname,
                    Nationality = c.Nationality
                })
                .ToList()
                .OrderByDescending(m => m.Golds).ThenByDescending(x => x.Silvers).ThenByDescending(x => x.Bronzes);
        }
    }
}
