using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Extensions;

namespace MSOCoreTests.Extensions
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void CheckUnicodeConversion()
        {
            string encoded = "\u041a\u0430\u043b\u043e\u044f\u043d \u041a\u0430loyan";
            Assert.AreEqual("Калоян Каloyan", encoded.DecodeEncodedNonAsciiCharacters());
        }
    }
}
