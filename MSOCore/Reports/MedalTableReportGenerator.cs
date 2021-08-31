using MSOCore;
using MSOCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class MedalTableReportGenerator
    {
        public class MedalTableVm
        {
            public string Nationality { get; set; }
            public int Golds { get; set; }
            public int Silvers { get; set; }
            public int Bronzes { get; set; }
            public int JnrGolds { get; set; }
            public int JnrSilvers { get; set; }
            public int JnrBronzes { get; set; }
            public string GoldsStr { get { return (Golds > 0) ? Golds.ToString() : ""; } }
            public string SilversStr { get { return (Silvers > 0) ? Silvers.ToString() : ""; } }
            public string BronzesStr { get { return (Bronzes > 0) ? Bronzes.ToString() : ""; } }
            public string JnrGoldsStr { get { return (JnrGolds > 0) ? JnrGolds.ToString() : ""; } }
            public string JnrSilversStr { get { return (JnrSilvers > 0) ? JnrSilvers.ToString() : ""; } }
            public string JnrBronzesStr { get { return (JnrBronzes > 0) ? JnrBronzes.ToString() : ""; } }
            public bool HasAMedal
            {
                get
                {
                    return Golds > 0 || Silvers > 0 || Bronzes > 0
                        || JnrGolds > 0 || JnrSilvers > 0 || JnrBronzes > 0;
                }
            }

            public string Flag
            {
                get { return Nationality.GetFlag(); }
            }
        }

        public IEnumerable<MedalTableVm> GetItemsForLatest()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.Single(x => x.Current);

            return GetItems(olympiad.Id);
        }

        public IEnumerable<MedalTableVm> GetItems(int olympiadId)
        {
            var context = DataEntitiesProvider.Provide();
            var entrants = context.Entrants.Where(x => x.OlympiadId == olympiadId)
                .Join(context.Contestants, x => x.Mind_Sport_ID, c => c.Mind_Sport_ID, (e, c) => new { e, c })
                .Select(ec => new { Nationality = ec.c.Nationality ?? "Other", Medal = ec.e.Medal, JuniorMedal = ec.e.JuniorMedal  })
                .ToList();

            var g = entrants.Where(x => x.Medal == Medals.Gold).GroupBy(x => CanonicalNationality(x.Nationality))
                .ToDictionary(x => x.Key, x => x.Count());
            var s = entrants.Where(x => x.Medal == Medals.Silver).GroupBy(x => CanonicalNationality(x.Nationality))
                 .ToDictionary(x => x.Key, x => x.Count());
            var b = entrants.Where(x => x.Medal == Medals.Bronze).GroupBy(x => CanonicalNationality(x.Nationality))
                .ToDictionary(x => x.Key, x => x.Count());
            var jg = entrants.Where(x => x.JuniorMedal == Medals.JnrGold).GroupBy(x => CanonicalNationality(x.Nationality))
                .ToDictionary(x => x.Key, x => x.Count());
            var js = entrants.Where(x => x.JuniorMedal == Medals.JnrSilver).GroupBy(x => CanonicalNationality(x.Nationality))
                .ToDictionary(x => x.Key, x => x.Count());
            var jb = entrants.Where(x => x.JuniorMedal == Medals.JnrBronze).GroupBy(x => CanonicalNationality(x.Nationality))
                .ToDictionary(x => x.Key, x => x.Count());

            List<MedalTableVm> results = new List<MedalTableVm>();

            foreach (var n in entrants.Select(x => CanonicalNationality(x.Nationality)).Distinct())
            {
                results.Add(new MedalTableVm()
                {
                    Nationality = n,
                    Golds = g.ContainsKey(n) ? g[n] : 0,
                    Silvers = s.ContainsKey(n) ? s[n] : 0,
                    Bronzes = b.ContainsKey(n) ? b[n] : 0,
                    JnrGolds = jg.ContainsKey(n) ? jg[n] : 0,
                    JnrSilvers = js.ContainsKey(n) ? js[n] : 0,
                    JnrBronzes = jb.ContainsKey(n) ? jb[n] : 0,
                });
            }
            return results.Where(r => r.HasAMedal)
                .OrderByDescending(x => x.Golds)
                .ThenByDescending(x => x.Silvers)
                .ThenByDescending(x => x.Bronzes)
                .ThenBy(x => x.Nationality);
        }

        // Fix for 2021 medal table
        private string CanonicalNationality(string nat)
        {
            List<string> nats = new List<string>() {"England", "Scotland", "Wales", "Great Britain", "Northern Ireland"};
            if (nats.Contains(nat))
                return "Great Britain";
            return nat;
        }
    }
}
