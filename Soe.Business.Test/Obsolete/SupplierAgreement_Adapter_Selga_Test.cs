using SoftOne.Soe.Business.Util.SupplierAgreement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    /// <summary>
    ///This is a test class for SelgaDiscountTest and is intended
    ///to contain all SelgaDiscountTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SupplierAgreement_Adapter_Selga_Test
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod()]
        [Ignore] //not implemented
        public void TestSupplierAgreementSelga()
        {
            Assert.IsTrue(1 + 1 == 2);
        }
    }
}
