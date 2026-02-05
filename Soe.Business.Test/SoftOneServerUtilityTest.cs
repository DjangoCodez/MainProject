using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using System;

namespace Soe.Business.Test
{
    [TestClass]
    public class SoftOneServerUtilityTest
    {
        [TestMethod]
        public void TestTimeStamp()
        {
            TimeStampManager tsm = new TimeStampManager(null);
            var result = tsm.SyncEmployeeSchedule(529075, 2529, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(3), null, 1110);
            Assert.IsTrue(result != null);
        }
    }
}
