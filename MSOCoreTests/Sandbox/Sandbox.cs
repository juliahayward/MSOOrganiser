using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace MSOCoreTests.Sandbox
{
    [TestClass]
    public class Sandbox
    {
        [TestMethod]
        public void DateTimesParseTheWayYouExpect()
        {
            DateTime dob;

            var success = DateTime.TryParseExact("", "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dob);
            Assert.IsFalse(success);

            success = DateTime.TryParseExact("1976-02-01", "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dob);
            Assert.IsTrue(success);
            Assert.AreEqual(dob.Year, 1976);
            Assert.AreEqual(dob.Month, 2);
            Assert.AreEqual(dob.Day, 1);
        }
    }
}
