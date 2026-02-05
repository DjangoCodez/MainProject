using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.WebApiInternal;

namespace Soe.Business.Test.Util.WebApiInternal
{
    [TestClass]
    public class ReportConnectorTests
    {
        //[TestMethod]
        public void ReportConnector_can_authenticate_against_web_api()
        {
            var connector = new ReportConnector();

            var loads = connector.GetNrOfLoads();
        }

        [TestMethod()]
        [Ignore] //not implemented
        public void TestReportConnector()
        {
            Assert.IsTrue(1 + 1 == 2);
        }
    }
}
