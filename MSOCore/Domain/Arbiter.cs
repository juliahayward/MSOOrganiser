using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Arbiter
    {
        /// <summary>
        /// Arbiters are NOT shared, so we want a new one when we copy an Event
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public Arbiter CopyTo(Event evt)
        {
            return new Arbiter()
            {
                Arbiter_ID = this.Arbiter_ID,
                EIN = evt.EIN,
                Event = evt,
                Id = 0,
                Name = this.Name
            };
        }
    }
}
