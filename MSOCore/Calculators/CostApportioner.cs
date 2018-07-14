using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore.Calculators
{
    /// <summary>
    /// When someone enters a lot of events and pays the all-you-can-eat fee, we split the costs
    /// across all the events so that we can report what each event "earnt". This should add up
    /// exactly, avoiding rounding errors. However, we shouldn't include items which aren't
    /// real events (eg. T-shirt sales). One day we'll refactor these out of the Entrants table.
    /// </summary>
    public class CostApportioner<T>
    {
        private readonly Func<T, decimal> _costGetter;
        private readonly Action<T, decimal> _costSetter;
        private readonly Func<T, bool> _isApportionableGetter;

        public CostApportioner(Func<T, decimal> costGetter,
            Action<T, decimal> costSetter, Func<T, bool> isApportionableGetter)
        {
            _costGetter = costGetter;
            _costSetter = costSetter;
            _isApportionableGetter = isApportionableGetter;
        }

        public void ApportionCost(IEnumerable<T> events, decimal apportionableCost)
        {
            var rawCosts = events.Where(_isApportionableGetter).Sum(_costGetter);
            if (apportionableCost >= rawCosts || rawCosts == 0)
                return;

            foreach (var evt in events.Where(_isApportionableGetter))
            {
                var factor = apportionableCost / rawCosts;
                var oldValue = _costGetter(evt);
                var newValue = Math.Round(oldValue * factor, 2);
                rawCosts -= oldValue;
                apportionableCost -= newValue;
                _costSetter(evt, newValue);
            }
        }
    }
}
