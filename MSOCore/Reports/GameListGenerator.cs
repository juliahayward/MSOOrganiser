using System.Collections.Generic;
using System.Linq;

namespace MSOCore.Reports
{
    public class GameListGenerator
    {
        public class GameVm
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string FindOutMoreLink { get; set; }
            public string OnlineSiteName { get; set; }
            public string OnlineSiteUrl { get; set; }
        }

        public IEnumerable<GameVm> GetItems()
        {
            var context = DataEntitiesProvider.Provide();

            var games = context.Games
                .OrderBy(x => x.Mind_Sport)
                .Select(g => new GameVm() {
                    Name = g.Mind_Sport,
                    Code = g.Code,
                    FindOutMoreLink = g.FindOutMoreLink,
                    OnlineSiteName = g.OnlineSiteName,
                    OnlineSiteUrl = g.OnlineSiteUrl
                }).ToList();

            return games;
        }
    }

    public class EventListGenerator
    {
        public class EventVm
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public IEnumerable<EventVm> GetItems()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.First(x => x.Current);

            var games = context.Events
                .Where(e => e.OlympiadId == olympiad.Id)
                .OrderBy(x => x.Code)
                .Select(g => new EventVm() { Name = g.Mind_Sport, Code = g.Code }).ToList();

            return games;
        }

        // This excludes meta-events
        public IEnumerable<EventVm> GetNonMetaItems()
        {
            var context = DataEntitiesProvider.Provide();
            var olympiad = context.Olympiad_Infoes.FirstOrDefault(x => x.Current);
            if (olympiad == null)
                throw new NoCurrentOlympiadException();

            var games = context.Events
                .Where(e => e.OlympiadId == olympiad.Id && e.Pentamind)
                .OrderBy(x => x.Code)
                .Select(g => new EventVm() { Name = g.Mind_Sport, Code = g.Code }).ToList();

            return games;
        }
    }
}
