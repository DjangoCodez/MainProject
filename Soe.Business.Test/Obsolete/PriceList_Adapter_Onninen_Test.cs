using SoftOne.Soe.Business.Util.PricelistProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Soe.Business.Test
{
    
    
    /// <summary>
    ///This is a test class for OnninenPostTest and is intended
    ///to contain all OnninenPostTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PriceList_Adapter_Onninen_Test
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
        ///A test for OnninenPost Constructor
        ///</summary>
        [TestMethod()]
        [Ignore] //not implemented
        public void PriceList_WhenOnninnenProduct_ParseToObject()
        {
            //Arrange
            string source = "AL22X20-50;H07400;136309;ISOLERING ARMALOK AL-22X20-50,22X20 L=1,0 METER;99,00;M;1,00;60,00;11;;;;0;L;;http://handla.onninen.se/catalogue/default.asp?artno=AL22X20-50"; //Magic string is an example taken from actual file
            string[] items = source.Split(",".ToCharArray());
            var expected = new OnninenPost()
            {
                EAN = "",
                Name = "",
                Price = 1.0M,
                ProductGroup = "",
                ProductId = "",
                Unit = "",
                MaterialCode = "",
            };

            //Act
            OnninenPost actual = new OnninenPost(items);

            //Assert
            Assert.AreEqual(expected.MaterialCode, actual.MaterialCode);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Price, actual.Price);
            Assert.AreEqual(expected.ProductId, actual.ProductId);
            Assert.AreEqual(expected.EAN, actual.EAN);
            Assert.AreEqual(expected.Unit, actual.Unit);
            Assert.AreEqual(expected.ProductGroup, actual.ProductGroup);
        }
    }
}
