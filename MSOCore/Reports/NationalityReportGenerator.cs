using MSOOrganiser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class NationalityReportGenerator
    {
        public class NationalityVm
        {
            public string Nationality { get; set; }
            public string Males { get; set; }
            public string Females { get; set; }
            public string Total { get; set; }
        }

        public IEnumerable<NationalityVm> GetItems()
        {
            var context = new DataEntities();
            var contIds = context.Entrants.Where(x => x.Year == 2014) // TODO current olympiad
                .Select(x => x.Mind_Sport_ID).Distinct().ToList();
            var conts = context.Contestants.Where(x => contIds.Contains(x.Mind_Sport_ID))
                .ToList();

            var totals = conts.GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            var males = conts.Where(x => !x.Male.HasValue || x.Male.Value).GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            var females = conts.Where(x => x.Male.HasValue && !x.Male.Value).GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            foreach (var key in totals.Keys.OrderBy(x => x))
                yield return new NationalityVm()
                {
                    Nationality = key,
                    Males = males.ContainsKey(key) ? males[key].ToString() : "",
                    Females = females.ContainsKey(key) ? females[key].ToString() : "",
                    Total = totals[key].ToString()
                };

        }
    }
}
