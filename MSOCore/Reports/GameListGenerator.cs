using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class GameListGenerator
    {
        public class GameVm
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public IEnumerable<GameVm> GetItems()
        {
            var context = DataEntitiesProvider.Provide();

            var games = context.Games
                .OrderBy(x => x.Mind_Sport)
                .Select(g => new GameVm() { Name = g.Mind_Sport, Code = g.Code }).ToList();

            return games;
        }
    }
}
