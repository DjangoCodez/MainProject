//using SoftOne.Soe.Business.Util.PricelistProvider;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//namespace Soe.Business.Test
//{
    
    
//    /// <summary>
//    ///This is a test class for AhlsellEl_AhlsellElPostTest and is intended
//    ///to contain all AhlsellEl_AhlsellElPostTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class PriceList_Adapter_AhlsellEl_Test
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
//        ///A test for AhlsellElPost Constructor
//        ///</summary>
//        [TestMethod()]
//        public void PriceList_WhenAhlsellElProduct_ParseToObject()
//        {
//            //Arrange
//            string item = "8634600J00003150V102FRP              XMONTAGESKRUV EJOFIX-M2 4.2X14 (100ST)         "; //Magic string is an example taken from actual file

//            var expected = new AhlsellEl.AhlsellElPost()
//            {
//                EnvironmentFee = false,
//                MaterialClass = "V102", //pos 16+4
//                Name = "MONTAGESKRUV EJOFIX-M2 4.2X14 (100ST)         ",
//                NetPrice = 31.50M, // pos 8+8
//                ProductId = "8634600J", //pos 0+8
//                State = SoftOne.Soe.Business.Util.SoeProductPriceStatus.PriceChange,
//                Storage = false,
//                StorageUnit = "FRP", //pos 20+3
//            };

//            //Act
//            AhlsellEl.AhlsellElPost actual = new AhlsellEl.AhlsellElPost(item);

//            //Assert
//            Assert.AreEqual(expected.EnvironmentFee, actual.EnvironmentFee);
//            Assert.AreEqual(expected.MaterialClass, actual.MaterialClass);
//            Assert.AreEqual(expected.Name, actual.Name);
//            Assert.AreEqual(expected.NetPrice, actual.NetPrice);
//            Assert.AreEqual(expected.ProductId, actual.ProductId);
//            Assert.AreEqual(expected.State, actual.State);
//            Assert.AreEqual(expected.Storage, actual.Storage);
//            Assert.AreEqual(expected.StorageUnit, actual.StorageUnit);
//       }
//    }
//}
