using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoftOne.Soe.Business.Core.SysService.Tests
{
    [TestClass()]
    public class SysEdiConnectorTests
    {
        [TestMethod()]
        public void RunFlowTest()
        {
            SysServiceManager  ssm = new SysServiceManager(null);
            var result = ssm.RunFlow();
            Assert.IsTrue(result.Success);
        }
    }
}