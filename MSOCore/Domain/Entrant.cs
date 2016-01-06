using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Entrant
    {
        public static Entrant NewEntrant(string eventCode, string olympiadId, Contestant contestant)
        {
            return new Entrant()
                    {
                        Absent = false,
                        Comment = null,
                        Date = null,
                        // EntryNumber = id
                        Fee = 0m,
                        Game_Code = eventCode,
                        Medal = null,
                        Mind_Sport_ID = contestant.Mind_Sport_ID,
                        MustUse = null,
                        Name = contestant,
                        OlympiadId = int.Parse(olympiadId),
                        Partner = null,
                        Penta_Score = null,
                        PIN = null,
                        Rank = null,
                        Receipt = false,
                        Score = null,
                        Tie_break = null,
                        Year = null
                    };
        }
    }
}
