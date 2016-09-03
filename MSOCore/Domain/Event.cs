using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class Event
    {
        public DateTime? Start
        {
            get
            {
                if (Event_Sess.Any())
                    return Event_Sess.OrderBy(x => x.ActualStart).First().ActualStart;
                else
                    return null;
            }
        }

        /// <summary>
        /// Make a copy of this event, but with ID 0 ready to be created, and no Entrants.
        /// </summary>
        /// <returns></returns>
        public Event CopyTo(Olympiad_Info olympiad)
        {
            var evt = new Event()
            {
                // Arbiters see later
                C1st_Prize = this.C1st_Prize,
                C2nd_Prize = this.C2nd_Prize,
                C3rd_Prize = this.C3rd_Prize,
                Code = this.Code,
                Display = this.Display,
                EIN = 0,
                Entrants = new List<Entrant>(),
                Entry_Fee = this.Entry_Fee,
                // Event_Sess see later 
                Game = this.Game,
                GameId = this.GameId,
                incMaxFee = this.incMaxFee,
                JNR_1st_Prize = this.JNR_1st_Prize,
                JNR_2nd_Prize = this.JNR_2nd_Prize,
                JNR_3rd_Prize = this.JNR_3rd_Prize,
                JNR_Medals = this.JNR_Medals,
                JNR_Other_Prizes = this.JNR_Other_Prizes,
                Location = this.Location,
                MAX_Number = this.MAX_Number,
                Mind_Sport = this.Mind_Sport,
                No_Sessions = this.No_Sessions,
                Notes = this.Notes,
                Number = this.Number,
                Number_in_Team = this.Number_in_Team,
                OlympiadId = olympiad.Id,
                Olympiad_Info = olympiad,
                Other_Prizes = this.Other_Prizes,
                Pentamind = this.Pentamind,
                Prize_fund = this.Prize_fund,
                Prize_Giving = this.Prize_Giving,
                Type = this.Type,
                X_Num = this.X_Num,
                Year = olympiad.YearOf,
            };

            evt.Arbiters = this.Arbiters.Select(x => x.CopyTo(evt)).ToList();
            evt.Event_Sess = this.Event_Sess.Select(x => x.CopyTo(evt)).ToList();
            return evt;
        }
    }
}
