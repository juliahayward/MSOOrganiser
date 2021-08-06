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

    [TestClass]
    public class PaymentProcessor2021Tests
    {
        [TestMethod]
        public void TestParseWordpressFile()
        {
            var file = @"c:\src\juliahayward\MSOOrganiser\RawData\2021-test\bookings-0e36a55b72764f7233c971c06d9f67ad.csv";
            var processor = new PaymentProcessor2021();
            var orders = processor.ParseCsvFile(file);
            Assert.AreEqual(1812, orders.Count());
        }
    }
}
