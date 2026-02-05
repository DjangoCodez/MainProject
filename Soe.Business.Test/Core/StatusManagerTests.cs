using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class StatusManagerTests
    {
        [TestMethod()]
        public void GetSoftOneStatusDTOTest()
        {
            StatusManager statusManager = new StatusManager();
            var data = statusManager.GetSoftOneStatusDTO(ServiceType.Unknown);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetPrintSoftOneStatusDTOTest()
        {
            StatusManager statusManager = new StatusManager();
            var data = statusManager.GetPrintSoftOneStatusDTO(ServiceType.Unknown);
            Assert.IsTrue(data != null);
        }
    }
}