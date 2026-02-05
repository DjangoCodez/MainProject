using SoftOne.Soe.Business.Util.PricelistProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace Soe.Business.Test
{
    
    
    /// <summary>
    ///This is a test class for Selga_SelgaPostTest and is intended
    ///to contain all Selga_SelgaPostTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PriceList_Adapter_Selga_Test
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
        ///A test for SelgaPost Constructor
        ///</summary>
        [TestMethod()]
        [Ignore] //not implemented
        public void PriceList_WhenSelgaProduct_ParseToObject()
        {
            //Arrange
            string item = "0547005NOVOFLEX NCY-2 PLUS 4G1,5   116000514090302"; //Magic string is an example taken from actual file
            var expected = new Rexel.RexelPost()
            {
                MaterialCode = "",
                Name = "",
                Price = 1.0M,
                PriceChangeDate = new DateTime(2009,1,1),
                ProductId = "",
            };

            //Act
            Rexel.RexelPost actual = new Rexel.RexelPost(item);

            //Assert
            Assert.AreEqual(expected.MaterialCode, actual.MaterialCode);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Price, actual.Price);
            Assert.AreEqual(expected.ProductId, actual.ProductId);
            Assert.AreEqual(expected.PriceChangeDate, actual.PriceChangeDate);
        }
    }
}
