using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class GoTimeStampManagerTests
    {
        [TestMethod()]
        public void GetCurrentScheduleTest()
        {
            GoTimeStampManager goTimeStampManager = new GoTimeStampManager(null);
            var result = goTimeStampManager.GetCurrentSchedule(30449, 1205, 2028);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void GetScheduleForEmployeesTest()
        {
            GoTimeStampManager goTimeStampManager = new GoTimeStampManager(null);
            var result = goTimeStampManager.GetScheduleForEmployees(30449, 2028);
            Assert.IsTrue(result != null);
        }
    }
}