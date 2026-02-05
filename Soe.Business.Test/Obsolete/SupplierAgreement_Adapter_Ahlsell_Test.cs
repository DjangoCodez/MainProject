using SoftOne.Soe.Business.Util.SupplierAgreement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    
    
    /// <summary>
    ///This is a test class for AhlsellDiscountTest and is intended
    ///to contain all AhlsellDiscountTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SupplierAgreement_Adapter_Ahlsell_Test
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
        ///A test for AhlsellDiscount Constructor
        ///</summary>
        [TestMethod()]
        [Ignore] //todo implement
        public void SupplierAgreement_WhenAhlsellDicount_ParseToObject()
        {
            //Arrange
            string item = " 1N635635         0236028000000000000000000GODSSLANG T-40                041231"; //Magic string is an example taken from actual file

            var expected = new AhlsellDiscount()
            {
                Discount = 48.0M,
                MaterialClass = "1N635635",
                Name = "0236028000000000000000000GODSSLANG T-40",
                
            };

            //Act
            AhlsellDiscount actual = new AhlsellDiscount(item);

            //Assert
            Assert.AreEqual(expected.Discount, actual.Discount);
            Assert.AreEqual(expected.MaterialClass, actual.MaterialClass);
            Assert.AreEqual(expected.Name, actual.Name);
        }
    }
}
