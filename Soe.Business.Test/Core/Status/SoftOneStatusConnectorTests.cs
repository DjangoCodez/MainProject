using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Status.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Status.Tests
{
    [TestClass()]
    public class SoftOneStatusConnectorTests
    {
        [TestMethod()]
        public void IsServerLiveTest()
        {
            var d1 = SoftOneStatusConnector.IsServerLive("s1s1d1");
            var d2 = SoftOneStatusConnector.IsServerLive("s1s1d2");
            var d7 = SoftOneStatusConnector.IsServerLive("s1s1d7");
            var xe = SoftOneStatusConnector.IsServerLive("xe");
            Assert.IsTrue(d1 && d2 && d7 && xe);
        }

        [TestMethod()]
        public void GetStatusServiceGroupUpTimesTest()
        {
            var result = SoftOneStatusConnector.GetStatusServiceGroupUpTimes(new DateTime(2018, 3, 5), new DateTime(2018, 3, 5));
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void Test()
        {
            var groups = SoftOneStatusConnector.GetStatusServiceTypes();
            List<StatusResultAggregatedDTO> statusResultAggregatedDTOs = new List<StatusResultAggregatedDTO>();
            foreach (var group in groups)
            {
                statusResultAggregatedDTOs.AddRange(SoftOneStatusConnector.GetStatusResultAggregates(group.StatusServiceTypeId, DateTime.Now.AddHours(-10), DateTime.Now));
            }
            Assert.IsTrue(statusResultAggregatedDTOs.Any());
        }

        [TestMethod()]
        public void TestSeleniumTypes()
        {
            var seleniumTypes = SoftOneStatusConnector.GetStatusServiceTypes(SoftOne.Status.Shared.ServiceType.Selenium);
            List<StatusResultAggregatedDTO> statusResultAggregatedDTOs = new List<StatusResultAggregatedDTO>();
            List<string> seleniumList = new List<string>();
            foreach (var type in seleniumTypes)
            {
                seleniumList.Add($"{type.StatusActionJobSetting.Domain} {type.StatusActionJobSetting.SeleniumType}");
            }
            foreach (var type in seleniumTypes)
            {
                statusResultAggregatedDTOs.AddRange(SoftOneStatusConnector.GetStatusResultAggregates(type.StatusServiceTypeId, DateTime.Now.AddHours(-10), DateTime.Now));
            }
            Assert.IsTrue(statusResultAggregatedDTOs.Any());
        }

        [TestMethod()]
        public void GetTestCasesTest()
        {
            var data = SoftOneStatusConnector.GetTestCases(TestCaseType.Selenium);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetTestCaseGroupTest()
        {
            var groups = SoftOneStatusConnector.GetTestCaseGroups();
            var group = SoftOneStatusConnector.GetTestCaseGroup(2);
            group.Description = group.Description + " qw123";
            var result = SoftOneStatusConnector.SaveTestCaseGroup(group);
            Assert.IsTrue(groups != null && group != null && result.Success);
        }
    }
}