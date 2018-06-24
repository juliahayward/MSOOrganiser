using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore;
using MSOCore.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCoreTests.Calculators
{
    [TestClass]
    public class EventIndexerTests
    {
        [TestMethod]
        public void EventIndexer_IntegrationTest()
        {
            var eventIndexer = new EventIndexer();
            eventIndexer.IndexEvents(9);            // 2014 in test data

            var context = DataEntitiesProvider.Provide();
            var events = context.Events.Where(x => x.OlympiadId == 9).ToList();
            // This depends on the sessions not being altered - unlikely now
            Assert.AreEqual(70, events.Max(e => e.Number));
            Assert.AreEqual(5, events.Count(e => e.Number == 0));
        }
    }
}
