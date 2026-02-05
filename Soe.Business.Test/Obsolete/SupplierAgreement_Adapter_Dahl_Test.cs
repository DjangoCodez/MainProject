using SoftOne.Soe.Business.Util.SupplierAgreement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    /// <summary>
    ///This is a test class for DahlDiscountTest and is intended
    ///to contain all DahlDiscountTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SupplierAgreement_Adapter_Dahl_Test
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
        ///A test for DahlDiscount Constructor
        ///</summary>
        [TestMethod()]
        public void SupplierAgreement_WhenDahlDicount_ParseToObject()
        {
            //Arrange
            string item = "028202   RFR LIVSM-RÖRD F SVETSNING SMS37.0"; //Magic string is an example taken from actual file

            var expected = new DahlDiscount()
            {
                Discount = 37.0M,
                MaterialClass = "028202",
                Name = "RFR LIVSM-RÖRD F SVETSNING SMS"
            };

            //Act
            DahlDiscount actual = new DahlDiscount(item);

            //Assert
            Assert.AreEqual(expected.Discount, actual.Discount);
            Assert.AreEqual(expected.MaterialClass, actual.MaterialClass);
            Assert.AreEqual(expected.Name, actual.Name);
        }
    }
}
