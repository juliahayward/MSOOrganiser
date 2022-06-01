using MSOCore;
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
            public int NumberOfMales { get; set; }
            public string Males { get { return (NumberOfMales > 0 ? NumberOfMales.ToString() : ""); } }
            public int NumberOfFemales { get; set; }
            public string Females { get { return (NumberOfFemales > 0 ? NumberOfFemales.ToString() : ""); } }
            public string Total { get; set; }
        }

        public IEnumerable<NationalityVm> GetItemsForLatest()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);

            return GetItems(olympiad.Id);
        }

        public IEnumerable<NationalityVm> GetItems(int olympiadId)
        {
            var context = DataEntitiesProvider.Provide();
            var contIds = context.Entrants.Where(x => x.OlympiadId == olympiadId) 
                .Select(x => x.Mind_Sport_ID).Distinct().ToList();
            var conts = context.Contestants.Where(x => contIds.Contains(x.Mind_Sport_ID))
                .ToList();

            var totals = conts.GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            var males = conts.Where(x => x.Male).GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            var females = conts.Where(x => !x.Male).GroupBy(x => x.Nationality ?? "Other")
                .ToDictionary(x => x.Key, x => x.Count());
            foreach (var key in totals.Keys.OrderBy(x => x))
                yield return new NationalityVm()
                {
                    Nationality = (string.IsNullOrEmpty(key) ? "Unknown" : key),
                    NumberOfMales = males.ContainsKey(key) ? males[key] : 0,
                    NumberOfFemales = females.ContainsKey(key) ? females[key] : 0,
                    Total = totals[key].ToString()
                };

        }
    }
}
