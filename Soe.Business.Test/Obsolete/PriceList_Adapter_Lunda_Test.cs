using SoftOne.Soe.Business.Util.PricelistProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    [TestClass()]
    public class PriceList_Adapter_Lunda_Test
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

        [TestMethod()]
        public void PriceList_WhenLundaPost_ParseToObject()
        {
            //Arrange
            string [] item = new string[5];// "8342083;FMM 9210 DUSCH 160 c/c NED FKR;140500;139700;187300;A0002;ST;4"
            var expected = new Lunda.LundaPost()
            {
                ProductId = "8342083",
                Name = "FMM 9210 DUSCH 160 c/c NED FKR",
                PriceStyckNetto = 1405.00M,
                PriceBrutto = 1873.00M,
                MaterialCode = "A0002",
                StorageUnit = "ST",
            };

            //Act
            Lunda.LundaPost actual = new Lunda.LundaPost(item);

            //Assert
            Assert.AreEqual(expected.MaterialCode, actual.MaterialCode);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.PriceStyckNetto, actual.PriceStyckNetto);
            Assert.AreEqual(expected.ProductId, actual.ProductId);
            Assert.AreEqual(expected.StorageUnit, actual.StorageUnit);
        }
    }
}
