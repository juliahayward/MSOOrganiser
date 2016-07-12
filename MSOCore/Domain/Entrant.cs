using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Entrant
    {
        public static Entrant NewEntrant(int evtId, string evtCode, int olympiadId, Contestant contestant, decimal fee)
        {
            return new Entrant()
                    {
                        Absent = false,
                        Comment = null,
                        Date = null,
                        // EntryNumber = id
                        Fee = fee,
                        EventId = evtId,
                        Game_Code = evtCode,
                        Medal = null,
                        JuniorMedal = null,
                        Mind_Sport_ID = contestant.Mind_Sport_ID,
                        MustUse = null,
                        Name = contestant,
                        OlympiadId = olympiadId,
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
