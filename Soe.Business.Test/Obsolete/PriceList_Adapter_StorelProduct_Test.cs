//using SoftOne.Soe.Business.Util.PricelistProvider;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SoftOne.Soe.Business.Util;

//namespace Soe.Business.Test
//{   
//    /// <summary>
//    ///This is a test class for Storel_StorelProductTest and is intended
//    ///to contain all Storel_StorelProductTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class PriceList_Adapter_StorelProduct_Test
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

//        /// <summary>
//        ///A test for StorelProduct Constructor
//        ///</summary>
//        [TestMethod()]
//        public void PriceList_WhenStorel7Product_ParseToObject()
//        {
//            //Arrange
//            string item = "00000137EKKJ 3x6/6 T1500/K12          m  01AA00092401"; //Magic string is an example taken from actual file
//            SoeSysPriceListProvider providerType = SoeSysPriceListProvider.Storel7;
//            var expected = new Storel.StorelProduct()
//            {
//                MaterialCode = "01AA",
//                Name = "EKKJ 3x6/6 T1500/K12          ",
//                Price = 92.4M,
//                ProductId = "0000137",
//                Unit = "m  ",
//            };
        
//            //Act
//            Storel.StorelProduct actual = new Storel.StorelProduct(providerType, item);

//            //Assert
//            Assert.AreEqual(expected.MaterialCode, actual.MaterialCode);
//            Assert.AreEqual(expected.Name, actual.Name);
//            Assert.AreEqual(expected.Price, actual.Price);
//            Assert.AreEqual(expected.ProductId, actual.ProductId);
//            Assert.AreEqual(expected.Unit, actual.Unit);
//        }

//        /// <summary>
//        ///A test for StorelProduct Constructor
//        ///</summary>
//        [TestMethod()]
//        public void PriceList_WhenStorel8Product_ParseToObject()
//        {
//            //Arrange
//            string item = "00000137EKKJ 3x6/6 T1500/K12          m  01AA00092401"; //Magic string is an example taken from actual file
//            SoeSysPriceListProvider providerType = SoeSysPriceListProvider.Storel8;
//            var expected = new Storel.StorelProduct()
//            {
//                MaterialCode = "01AA",
//                Name = "EKKJ 3x6/6 T1500/K12          ",
//                Price = 92.4M,
//                ProductId = "00000137",
//                Unit = "m  ",
//            };

//            //Act
//            Storel.StorelProduct actual = new Storel.StorelProduct(providerType, item);

//            //Assert
//            Assert.AreEqual(expected.MaterialCode, actual.MaterialCode);
//            Assert.AreEqual(expected.Name, actual.Name);
//            Assert.AreEqual(expected.Price, actual.Price);
//            Assert.AreEqual(expected.ProductId, actual.ProductId);
//            Assert.AreEqual(expected.Unit, actual.Unit);
//        }
//    }
//}
