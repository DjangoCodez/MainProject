using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using System;
using System.Threading;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class SysScheduledJobManagerTests
    {
        [TestMethod()]
        public void RunJobsTest()
        {
            ActionResult result;
            using (SysScheduledJobManager jm = new SysScheduledJobManager(null))
            {
                result = jm.RunJobs(); //105
            }
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ResurrectScheduledJobsInLimboTest()
        {
            using (SysScheduledJobManager jm = new SysScheduledJobManager(null))
            {
                jm.ResurrectScheduledJobsInLimbo();
            }
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void CheckJobsTest()
        {
            using (SysScheduledJobManager jm = new SysScheduledJobManager(null))
            {
                jm.CheckJobs();
                Thread.Sleep(100000);
            }
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void LastStartOfJobTest()
        {
            DateTime? data;
            using (SysScheduledJobManager jm = new SysScheduledJobManager(null))
            {
                data = jm.LastStartOfJob(254,"Schemalagt jobb avbrutet");
                Thread.Sleep(100000);
            }
            Assert.IsTrue(data != null);
        }
    }
}