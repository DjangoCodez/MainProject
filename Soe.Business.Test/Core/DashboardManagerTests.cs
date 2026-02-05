using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class DashboardManagerTests
    {
        [TestMethod()]
        public void GetPerformanceTestResultsTest()
        {
            DashboardManager ex = new DashboardManager(null);
            ConfigurationSetupUtil.Init();
            foreach (var type in ex.GetDashboardStatisticTypes(SoftOne.Status.Shared.ServiceType.Selenium))
                ex.GetPerformanceTestResults(type.Key, DateTime.Now.AddDays(-1), DateTime.Now, Common.Util.TermGroup_PerformanceTestInterval.Hour);
            Assert.IsTrue(true);
        }
    }
}