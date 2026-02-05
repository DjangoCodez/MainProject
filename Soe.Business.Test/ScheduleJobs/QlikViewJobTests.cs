using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.ScheduledJobs.Tests
{
    [TestClass()]
    public class QlikViewJobTests
    {
        [TestMethod()]
        public void ExecuteTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            var job = new QlikViewJob();
            job.Execute(new SysScheduledJobDTO() { SysJobSettings = new List<Common.DTO.SysJobSettingDTO>() }, 111);
            Assert.IsTrue(true);
        }
    }
}