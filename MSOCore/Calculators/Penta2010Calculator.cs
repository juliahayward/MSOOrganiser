using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    public class PentaCalculatorFactory
    {
        public static IPentaCalculator Get(Event evt)
        {
            // From 2023 - events can be out of the pendamind, but still count for category meta-events. Now, we assing them points
            // and do the filtering at the point of calculating the meta-event total.
            if (!evt.Pentamind && string.IsNullOrEmpty(evt.GPCategory))
                return new CasualEventCalculator();
            else if (evt.Olympiad_Info.Ruleset == "GrandPrix")
                return new GrandPrixCalculator();
            else
                return new Penta2021Calculator();
        }
    }
    public class Penta2010Calculator : PentaCalculator
    {
        public override double Formula(int n, double p)
        {
            // Spread linearly; top = 100, bottom = 0 
            var myPentaScore = 100 * (n - p) / (n - 1);
            // Small events - introduce a fudge factor
            if (n < 10)
                myPentaScore = myPentaScore * n / (n + 1);
            return myPentaScore;
        }
    }
        

    public interface IPentaCalculator
    {
        void Calculate(int numberInTeam, IEnumerable<IPentaCalculable> entries, float premiumFactor = 1.0f, int overridingNumberOfTeams = 0);

    }

    public abstract class PentaCalculator : IPentaCalculator
    {
        public abstract double Formula(int numberOfTeams, double myPosition);

        public void Calculate(int numberInTeam, IEnumerable<IPentaCalculable> entries, float premiumFactor = 1.0f, int overridingNumberOfTeams=0)
        {
            if (entries.Any(x => x.Absent == false && x.Rank == 0))
                throw new ArgumentException("At least one player was present but has no rank");

            var numberOfTeams = (overridingNumberOfTeams>0) ? overridingNumberOfTeams:  entries.Count(x => !x.Absent) / numberInTeam;

            foreach (var entry in entries)
            {
                if (entry.Absent || entry.Withdrawn)
                {
                    entry.PentaScore = 0;
                    continue;
                }

                var myRank = entry.Rank;
                var numberOnMyRank = entries.Count(x => x.Rank == myRank);
                var numberOfTeamsOnMyRank = numberOnMyRank / numberInTeam;

                var myEffectiveRank = myRank + (numberOfTeamsOnMyRank - 1) / 2.0;

                var myPentaScore = Formula(numberOfTeams, myEffectiveRank);               

                entry.PentaScore = (float)myPentaScore * premiumFactor;
            }
        }
    }
}
