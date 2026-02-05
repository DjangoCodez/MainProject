using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftOne.Soe.Business.Util.API.Tests
{
    [TestClass()]
    public class CalabrioConnectorTests
    {
        [TestMethod()]
        public void GetTimeScheduleEmployeeIOsTest()
        {
            CalabrioConnector connector = new CalabrioConnector("https://mtdemousce01.teleopticloud.com/api/");
            connector.GetTimeScheduleEmployeeIOs("928DD0BC-BF40-412E-B970-9B5E015AADEA", new DateTime(2020, 4, 3), new DateTime(2020, 4, 3), null, null, null);
            Assert.IsTrue(true);
        }
    }
}