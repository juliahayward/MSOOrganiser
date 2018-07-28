using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Calculators;
using System.Linq;

namespace MSOCoreTests.Calculators
{
    [TestClass]
    public class PaymentProcessor2018Tests
    {
        [TestMethod]
        public void TestParseVikasFile()
        {
            var file = @"c:\users\jhayward\desktop\Events.json";
            var processor = new PaymentProcessor2018();
            var order = processor.ParseJsonFile(file);
            Assert.IsTrue(order.Any());
            Assert.IsFalse(order.Any(o => o.Attendees.Count > 1));
        }
    }
}
