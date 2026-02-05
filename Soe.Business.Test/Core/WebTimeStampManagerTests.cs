using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class WebTimeStampManagerTests
    {
        [TestMethod()]
        public void AdjustForTimeZoneTest()
        {
            WebTimeStampManager mm = new WebTimeStampManager(null);
            mm.AdjustForTimeZone(DateTime.Now, 1);
            Assert.IsTrue(true);
        }
    }
}