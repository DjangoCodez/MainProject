using SoftOne.Soe.Business.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Data;
using System;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class AttestManagerTests : TestBase
    {
        [TestMethod()]
        public void GetPayrollCalculationProductsTest()
        {
            TimeTreePayrollManager am = new TimeTreePayrollManager(null);
            am.GetPayrollCalculationProducts(291, 264, 239);

            Assert.Fail();
        }

        [TestMethod()]
        public void ResendAndExtendIfErrorTest()
        {
            ConfigurationSetupUtil.Init();

            //Setup language cache
          //  TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            AttestManager am = new AttestManager(GetParameterObject(110807));
            using (CompEntities entities = new CompEntities())
            {
                am.ResendAndExtendIfError(entities,1, 1108079, 1242, DateTime.Today.AddDays(-200), DateTime.Today, false);
            }
        }
    }
}