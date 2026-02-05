using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Tests
{
    [TestClass()]
    public class TimeReportDataManagerTests
    {
        [TestMethod()]
        public void SendPayrollSlipTest()
        {
            TimeReportDataManager timeReportDataManager = new TimeReportDataManager(null);
            using (CompEntities entities = new CompEntities())
            {
                //181	1717
                timeReportDataManager.SendPayrollSlip(entities, 291, entities.TimePeriod.FirstOrDefault(t => t.TimePeriodId == 1717), new List<Employee>() { entities.Employee.Include("ContactPerson").FirstOrDefault(f => f.EmployeeId == 181) });
            }
            Assert.IsTrue(true);
        }
    }
}