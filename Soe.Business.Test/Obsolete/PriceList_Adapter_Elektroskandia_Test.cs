//using SoftOne.Soe.Business.Util.PricelistProvider;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//namespace Soe.Business.Test
//{
    
    
//    /// <summary>
//    ///This is a test class for Elektroskandia_ElektroskandiaPostTest and is intended
//    ///to contain all Elektroskandia_ElektroskandiaPostTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class PriceList_Adapter_Elektroskandia_Test
//    {


//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion


//        /// <summary>
//        ///A test for ElektroskandiaPost Constructor
//        ///</summary>
//        [TestMethod()]
//        public void PriceList_WhenElektroskandiaProduct_ParseToObject()
//        {
//            //Arrange
//            string item = "E0000065EKKJ 2X2,5/2,5 1 KV      M 00000500N20050110ACA 000004430"; //Magic string is an example taken from actual file
//            var expected = new Elektroskandia.ElektroskandiaPost()
//            {
//                MaterialCode = "ACA", //pos 52+3
//                Name = "EKKJ 2X2,5/2,5 1 KV      ", //pos 8+25
//                Price = 44.3M, //pos 56+9
//                ProductId = "0000065", //pos 1+7
//                StorageUnit = "M ", //33+2
//            };

//            //Act
//            Elektroskandia.ElektroskandiaPost actual= new Elektroskandia.ElektroskandiaPost(item);

//            //Assert
//            Assert.AreEqual(expected.MaterialCode, actual.MaterialCode);
//            Assert.AreEqual(expected.Name, actual.Name);
//            Assert.AreEqual(expected.Price, actual.Price);
//            Assert.AreEqual(expected.ProductId, actual.ProductId);
//            Assert.AreEqual(expected.StorageUnit, actual.StorageUnit);
//        }
//    }
//}
