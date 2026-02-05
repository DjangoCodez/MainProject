using SoftOne.Soe.Business.Util.SupplierAgreement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    
    
    /// <summary>
    ///This is a test class for OnninenDiscountTest and is intended
    ///to contain all OnninenDiscountTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SupplierAgreement_Adapter_Onninen_Test
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for OnninenDiscount Constructor
        ///</summary>
        [TestMethod()]
        public void SupplierAgreement_WhenOnninenDicount_ParseToObject()
        {
            //Arrange
            //string item = ";H05570;Markavloppsrördel.-Pragma;65.10;.00;.00;.00;20071231"; //Magic string is an example taken from actual file
            string mc = "H05570";
            string reb = "65.10";

            var expected = new OnninenDiscount()
            {
                Discount = 65.1M,
                MaterialClass = "H05570",
            };

            //Act
            OnninenDiscount actual = new OnninenDiscount(mc, string.Empty,reb);

            //Assert
            Assert.AreEqual(expected.Discount, actual.Discount);
            Assert.AreEqual(expected.MaterialClass, actual.MaterialClass);
            
        }
    }
}
