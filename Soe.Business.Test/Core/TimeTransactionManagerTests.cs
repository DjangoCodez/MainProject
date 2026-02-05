using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class TimeTransactionManagerTests
    {
        [TestMethod()]
        public void GetTimePayrollStatisticsDTOs_newTest()
        {
            using (CompEntities entities = new CompEntities())
            {
                TimeTransactionManager ttm = new TimeTransactionManager(null);
                EmployeeManager em = new EmployeeManager(null);
                int actorCompanyId = 701609;
                var employees = em.GetAllEmployees(entities, actorCompanyId, loadEmployment: true);
                employees = employees.OrderBy(e => e.EmployeeId).Take(20).ToList();
                TimePeriodManager tpm = new TimePeriodManager(null);
                var timePeriods = tpm.GetTimePeriods(entities, TermGroup_TimePeriodType.Payroll, actorCompanyId).Where(p => p.StartDate > new DateTime(2017, 12, 31)).ToList();
                var transFull = ttm.GetTimePayrollStatisticsDTOs(entities, actorCompanyId, employees, timePeriods.Select(t => t.TimePeriodId).ToList());
                var transMini = ttm.GetTimePayrollStatisticsSmallDTOs_new(entities, actorCompanyId, employees, timePeriods.Select(t => t.TimePeriodId).ToList());
            };

            Assert.Fail();
        }
    }
}