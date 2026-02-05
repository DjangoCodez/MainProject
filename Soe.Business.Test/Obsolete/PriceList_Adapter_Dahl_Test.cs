//using SoftOne.Soe.Business.Util.PricelistProvider;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//namespace Soe.Business.Test
//{
    
    
//    /// <summary>
//    ///This is a test class for Dahl_DahlProductPostTest and is intended
//    ///to contain all Dahl_DahlProductPostTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class PriceList_Adapter_Dahl_Test
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
//        ///A test for DahlProductPost Constructor
//        ///</summary>
//        [TestMethod()]
//        public void PriceList_WhenDahlProduct_ParseToObject()
//        {
//            //Arrange
//            string item = "24211298        RHB LOCK M LINA T BAJONETTKPL.STK 084012GJ0000204.00000000001 N                      "; //Magic string is an example taken from actual file

//            var expected = new Dahl.DahlProductPost()
//            {
//                EAN = 0,
//                MinPackageSize = 1.0M,
//                Name = "RHB LOCK M LINA T BAJONETTKPL.",
//                NetPrice = false,
//                PostType = Dahl.DahlPostType.ProductPost,
//                Price = 204.0M,
//                ProductId = "4211298        ", //pos 1+15
//                RebateGroup = "084012",
//                Storage = true,
//                Unit = "STK ",
//            };
 
//            //Act
//            Dahl.DahlProductPost actual = new Dahl.DahlProductPost(item);

//            //Assert
//            Assert.AreEqual(expected.EAN, actual.EAN);
//            Assert.AreEqual(expected.MinPackageSize, actual.MinPackageSize);
//            Assert.AreEqual(expected.Name, actual.Name);
//            Assert.AreEqual(expected.NetPrice, actual.NetPrice);
//            Assert.AreEqual(expected.PostType, actual.PostType);
//            Assert.AreEqual(expected.Price, actual.Price);
//            Assert.AreEqual(expected.ProductId, actual.ProductId);
//            Assert.AreEqual(expected.RebateGroup, actual.RebateGroup);
//            Assert.AreEqual(expected.Storage, actual.Storage);
//            Assert.AreEqual(expected.Unit, actual.Unit);
//        }

//        /// <summary>
//        ///A test for DahlOpeningPost Constructor
//        ///</summary>
//        [TestMethod()]
//        public void PriceList_WhenDahlOpeningPost_ParseToObject()
//        {
//            //Arrange
//            string item = "1080201 DAHL"; //Magic string is an example taken from actual file
//            var expected = new Dahl.DahlOpeningPost()
//            {
//                Date = new DateTime(2008, 2, 1),
//                PostType = Dahl.DahlPostType.OpeningPost,
//                Provider = "DAHL",
//            };

//            //Act
//            Dahl.DahlOpeningPost actual = new Dahl.DahlOpeningPost(item);

//            //Assert
//            Assert.AreEqual(expected.Date, actual.Date);
//            Assert.AreEqual(expected.PostType, actual.PostType);
//            Assert.AreEqual(expected.Provider, actual.Provider);
//        }
//    }
//}
