using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCoreTests.Calculators
{
    [TestClass]
    public class NumberListContractorTests
    {
        [TestMethod]
        public void TestContractor()
        {
            var numbers = new int[] { };
            var output = NumberListContractor.Contract(numbers);
            Assert.AreEqual("", output);

            numbers = new int[] { 1 };
            output = NumberListContractor.Contract(numbers);
            Assert.AreEqual("1", output);

            numbers = new int[] { 1, 4, 5 };
            output = NumberListContractor.Contract(numbers);
            Assert.AreEqual("1, 4-5", output);

            numbers = new int[] { 1, 4, 9 };
            output = NumberListContractor.Contract(numbers);
            Assert.AreEqual("1, 4, 9", output);

            numbers = new int[] { 1, 2, 3 };
            output = NumberListContractor.Contract(numbers);
            Assert.AreEqual("1-3", output);

            numbers = new int[] { 1, 2, 3, 6, 9, 10, 11 };
            output = NumberListContractor.Contract(numbers);
            Assert.AreEqual("1-3, 6, 9-11", output);
        }
    }
}
