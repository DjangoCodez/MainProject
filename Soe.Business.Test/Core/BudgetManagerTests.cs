using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Configuration;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class BudgetManagerTests
    {
        [TestMethod()]
        public void CreateBudgetFromSalesTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);

            Company company = new CompanyManager(null).GetCompany(759, true);
            User user = new UserManager(null).GetUser(4);

            var param = ParameterObject.Create(user: user.ToDTO(),
                                               company: company.ToCompanyDTO(),
                                               thread: "");

            BudgetManager m = new BudgetManager(param);
            ActionResult result = m.CreateBudgetFromSales(company.ActorCompanyId, new DateTime(2018, 9, 1), new DateTime(2018, 9, 26));
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CreateBudgetTimeFromScheduleTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);

            Company company = new CompanyManager(null).GetCompany(759, true);
            User user = new UserManager(null).GetUser(4);

            var param = ParameterObject.Create(user: user.ToDTO(),
                                               company: company.ToCompanyDTO(),
                                               thread: "");

            BudgetManager m = new BudgetManager(param);
            ActionResult result = m.CreateBudgetTimeFromSchedule(company.ActorCompanyId, new DateTime(2018, 9, 1), new DateTime(2018, 9, 26));
            Assert.IsTrue(result.Success);
        }
    }
}