using SoftOne.Soe.Business.Util.SupplierAgreement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    
    
    /// <summary>
    ///This is a test class for StorelDiscountTest and is intended
    ///to contain all StorelDiscountTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SupplierAgreement_Adapter_Storel_Test
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
        ///A test for StorelDiscount Constructor
        ///</summary>
        [Ignore]
        public void SupplierAgreement_WhenStorelDiscount_Dicount_ParseToObject()
        {
            //Arrange
            string item = "    V   0820               KABELSKOR ISOLERADE             48,0                       "; //Magic string is an example taken from actual file

            var expected = new SolarDiscount()
            {
                Discount = 48.0M,
                MaterialClass = "0820",
                Name = "KABELSKOR ISOLERADE           ",
                Type = "V"
            };

            //Act
            SolarDiscount actual = new SolarDiscount(item);

            //Assert
            Assert.AreEqual(expected.Discount, actual.Discount);
            Assert.AreEqual(expected.MaterialClass, actual.MaterialClass);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Type, actual.Type);
        }
    }
}
