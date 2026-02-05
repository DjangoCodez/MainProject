using SoftOne.Soe.Business.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class TimeStampManagerTests : TestBase
    {
        [TestMethod()]
        public void SyncEmployeeTest()
        {
            TimeStampManager tsm = new TimeStampManager(null);
            var result = tsm.SyncEmployee(90, DateTime.Now, null, true);
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void CreateFakeTimeStampsTest()
        {
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            var param = GetParameterObject(1750, 1211);
            TimeStampManager m = new TimeStampManager(param);
            var result = m.CreateFakeTimeStamps(1750, new DateTime(2021, 9, 14), DateTime.Now.Date);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void ConvertTimeStampsToTimeBlocksTest()
        {

            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            TimeStampManager tsm = new TimeStampManager(null);
            List<int> companyIds = tsm.GetCompanyIdsWithNewEntries(new DateTime(2022, 5, 15));
            foreach (var id in companyIds)
            {
                TimeRuleManager trm = new TimeRuleManager(null);
                trm.FlushTimeRulesFromCache(id);
                trm.GetTimeRulesFromCache(new CompEntities(), id);
                tsm.RemoveDuplicateTimeStampEntries(id);
                tsm.ConvertTimeStampsToTimeBlocks(id, new DateTime(2020,9,18));
                Debug.WriteLine(id.ToString());
            }
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void GetTimeAccumulatorTest()
        {    
            ConfigurationSetupUtil.Init();
            TermCacheManager.Instance.SetupSysTermCacheTS(Environment.MachineName, "Application_Start", true);
            TimeStampManager tsm = new TimeStampManager(null);
            var data = tsm.GetTimeAccumulator(628935, 43087, 2843, new DateTime(DateTime.Today.Year,1,1), DateTime.Now);
            Assert.IsTrue(data != null);
        }
    }
}