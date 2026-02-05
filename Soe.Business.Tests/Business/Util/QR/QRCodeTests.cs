using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.Tests;

namespace SoftOne.Soe.Business.Util.QR.Tests
{
    [TestClass()]
    public class QRCodeTests : TestUtilBase
    {
        [TestInitialize()]
        public void Initialize() { base.Init(); }

        [TestCleanup()]
        public void Cleanup() { }

        [TestMethod()]
        public void CreateQRTest()
        {
            QRCode qRCode = new QRCode();
           Assert.IsNotNull(qRCode.CreateQR("Test"));
        }
    }
}