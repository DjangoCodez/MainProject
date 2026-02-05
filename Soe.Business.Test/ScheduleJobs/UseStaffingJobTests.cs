using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.ScheduledJobs.Tests
{
    [TestClass()]
    public class UseStaffingJobTests
    {
        [TestMethod()]
        public void ExecuteTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            var job = new UseStaffingJob();
            job.Execute(new Common.DTO.SysScheduledJobDTO() { SysJobSettings = new List<Common.DTO.SysJobSettingDTO>() }, 111);
            Assert.IsTrue(true);
        }
    }
}