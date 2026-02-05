using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Tests;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;

namespace SoftOne.Soe.Business.Util.ExportFiles.Tests
{
    [TestClass()]
    public class KPATests : TestBase
    {
        [TestMethod()]
        public void CreateEmployeeKPADirektTest()
        {
            Kpa kPA = new Kpa(null, null);

            using (CompEntities entities = new CompEntities())
            {
                int actorCompanyId = 146132;
                var param = GetParameterObject(actorCompanyId, 9478, 0);               
                EmployeeManager em = new EmployeeManager(param);
                var employees = em.GetAllEmployees(actorCompanyId, loadEmployment: true, loadContact: true, loadEmployeeVactionSE: true, loadEmploymentPriceType: true);
                TimePeriodManager tpm = new TimePeriodManager(param);
                var timePeriods = tpm.GetTimePeriods(TermGroup_TimePeriodType.Payroll, actorCompanyId);
                var kpa =  kPA.CreateEmployeeKPADirekt(entities, actorCompanyId, 9478, 1508, timePeriods.Where(w => w.StartDate.Year == 2019 && w.StartDate.Month == 12).Select(s => s.TimePeriodId).ToList(),2019, employees, true, null);
                Assert.IsTrue(kpa != null);
            }
        }
    }
}