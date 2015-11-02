using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Reports
{
    public class ContestantMedalsGenerator
    {
        public class ContestantVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public ContestantVm GetModel(int id)
        {
            var context = new DataEntities();

            var contestant = context.Contestants.First(x => x.Mind_Sport_ID == id);

            var model = new ContestantVm
            {
                Id = id,
                Name = string.Format("{0} {1}", contestant.Firstname, contestant.Lastname)
            };

            return model;
        }
    }
}
