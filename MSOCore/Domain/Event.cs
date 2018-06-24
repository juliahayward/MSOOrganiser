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

        public DateTime? End
        {
            get
            {
                if (Event_Sess.Any())
                    return Event_Sess.OrderBy(x => x.ActualStart).Last().ActualEnd;
                else
                    return null;
            }
        }

        public string ShortName()
        {
            return this.Mind_Sport.Replace("Olympiad Championship", "")
                .Replace("World Championship", "");
        }

        public string Status()
        {
            var hasNoScoredEntrants = !Entrants.Any(x => x.Rank.HasValue && x.Rank > 0);
            var hasAllScoredEntrants = Entrants.All(x => (x.Rank.HasValue && x.Rank > 0) || x.Absent);
            if (!Entrants.Any())
            {
                if (!End.HasValue || DateTime.Now < End.Value)
                    return "In database, empty";
                else return "Complete"; // noone entered, and the end is passed
            }
            else if (hasNoScoredEntrants && Event_Sess.All(s => s.ActualEnd < DateTime.Now))
                return "Awaiting results";
            else if (hasNoScoredEntrants && Event_Sess.Any(s => s.ActualStart < DateTime.Now))
                return "In progress";
            else if (hasNoScoredEntrants)
                return "Taking entries";
            else if (hasAllScoredEntrants)
                return "Complete";
            else
                return "Results being entered";
        }

        public double FractionDone()
        {
            var hasNoScoredEntrants = !Entrants.Any(x => x.Rank.HasValue && x.Rank > 0);
            var hasAllScoredEntrants = Entrants.All(x => (x.Rank.HasValue && x.Rank > 0) || x.Absent);
            if (!Entrants.Any())
                return 0.0;
            else if (hasNoScoredEntrants && Event_Sess.All(s => s.ActualEnd < DateTime.Now))
                return 0.666;
            else if (hasNoScoredEntrants && Event_Sess.Any(s => s.ActualStart < DateTime.Now))
                return 0.333;
            else if (hasNoScoredEntrants)
                return 0.0;
            else if (hasAllScoredEntrants)
                return 1.0;
            else return 0.9;
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
                ConsistentWithBoardability = true
            };

            evt.Arbiters = this.Arbiters.Select(x => x.CopyTo(evt)).ToList();
            evt.Event_Sess = this.Event_Sess.Select(x => x.CopyTo(evt)).ToList();
            return evt;
        }
    }
}
