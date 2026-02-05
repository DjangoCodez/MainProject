using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class TimeAccumulatorManagerTests
    {
        [TestMethod()]
        public void GetBreakTimeAccumulatorItemTest()
        {
            TimeAccumulatorManager timeAccumulatorManager = new TimeAccumulatorManager(null);
            var result = timeAccumulatorManager.GetBreakTimeAccumulatorItem(291, DateTime.Now, 237, 37);
            Assert.IsTrue(result != null);

        }
    }
}