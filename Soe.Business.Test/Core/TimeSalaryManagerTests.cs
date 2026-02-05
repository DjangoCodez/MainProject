using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class TimeSalaryManagerTests
    {
        [TestMethod()]
        public void GetTimeSalaryExportSelectionTest()
        {
            TimeSalaryManager salaryManager = new TimeSalaryManager(null);
            var data = salaryManager.GetTimeSalaryExportSelection(1300, new DateTime(2020, 1, 1), new DateTime(2020,1, 31), 104);
            Assert.IsTrue(data != null);
        }
    }
}