using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.ScheduledJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.ScheduledJobs.Tests
{
    [TestClass()]
    public class TimeStampConversionJobTests
    {
        [TestMethod()]
        public void ExecuteTest()
        {
            ZEntityFrameworkExtensionUtil.SetUpZEntityFrameworkExtension();
            SysServiceManager ssm = new SysServiceManager(null);

            //Setup language cache
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            var job = new TimeStampConversionJob();
            job.Execute(new Common.DTO.SysScheduledJobDTO() { SysJobSettings = new List<Common.DTO.SysJobSettingDTO>() }, 111);
        }
    }
}