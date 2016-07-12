using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class PmindScores
    {
        [TestMethod]
        public void CheckBestOptions()
        {
            var dt = DateTime.Now;

            var evs = new[]
            {
                new Event() {Code = "A1", Score = 95, Type = "A", IsLong = false, IsEuro = true},
                new Event() {Code = "A2", Score = 93, Type = "A", IsLong = false, IsEuro = true},
                new Event() {Code = "B1", Score = 92, Type = "B", IsLong = true, IsEuro = false},
                new Event() {Code = "B2", Score = 85, Type = "B", IsLong = false, IsEuro = false},
                new Event() {Code = "C1", Score = 82, Type = "C", IsLong = false, IsEuro = true},
                new Event() {Code = "E1", Score = 77, Type = "E", IsLong = true, IsEuro = true},
                new Event() {Code = "D1", Score = 77, Type = "D", IsLong = false, IsEuro = true},
                new Event() {Code = "C2", Score = 65, Type = "C", IsLong = false, IsEuro = true},
                new Event() {Code = "E2", Score = 60, Type = "E", IsLong = false, IsEuro = false},
                new Event() {Code = "A3", Score = 52, Type = "A", IsLong = true, IsEuro = true},
                new Event() {Code = "A4", Score = 51, Type = "A", IsLong = false, IsEuro = true},
                new Event() {Code = "F", Score = 45, Type = "F", IsLong = false, IsEuro = true},
                new Event() {Code = "G", Score = 38, Type = "G", IsLong = true, IsEuro = true},
                new Event() {Code = "H", Score = 33, Type = "H", IsLong = false, IsEuro = false},
                new Event() {Code = "G1", Score = 23, Type = "G", IsLong = true, IsEuro = false},
                new Event() {Code = "F1", Score = 21, Type = "F", IsLong = false, IsEuro = true},
                new Event() {Code = "G2", Score = 10, Type = "G", IsLong = true, IsEuro = true},
                new Event() {Code = "A5", Score = 0, Type = "A", IsLong = false, IsEuro = true},
                new Event() {Code = "B3", Score = 0, Type = "B", IsLong = false, IsEuro = false},
            };

            int counted = 0;
            double bestScore = 0;
            IEnumerable<Event> bestCombination = null;
            foreach (var combination in evs.Combinations(5))
            {
                counted++;
                if (combination.Select(x => x.Type).Distinct().Count() < 5) continue;
                if (combination.Where(x => x.IsLong).Count() < 2) continue;
                if (combination.Where(x => x.IsEuro).Count() > 3) continue;
                if (combination.Sum(x => x.Score) > bestScore)
                {
                    bestScore = combination.Sum(x => x.Score);
                    bestCombination = combination;
                }
            }

            var dt2 = DateTime.Now;
            Assert.IsTrue(dt2.Subtract(dt).TotalMilliseconds < 1000);
        }

 
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0
                ? new[] {new T[0]}
                : elements.SelectMany((e, i) =>
                    elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] {e}).Concat(c)));
        }
    }

    public class Event
    {
        public string Code { get; set; }
        public double Score { get; set; }
        public string Type { get; set; }
        public bool IsLong { get; set; }
        public bool IsEuro { get; set; }
    }
}
