using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs.Tests
{
    [TestClass()]
    public class ExampleDataJobTests
    {
        [TestMethod()]
        public void ExecuteTest()
        {
            ConfigurationSetupUtil.Init();
            SysServiceManager ssm = new SysServiceManager(null);

            using (SOESysEntities ent = new SOESysEntities())
            {
                var jobs = ent.SysScheduledJob.Include("SysJobSettingScheduledJob").FirstOrDefault(f => f.SysScheduledJobId == 171).ToDTO(true, true, true);
                TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
                var job = new ExampleDataJob();
                job.Execute(jobs, 121);
                Assert.IsTrue(true);
            }
        }
    }
}