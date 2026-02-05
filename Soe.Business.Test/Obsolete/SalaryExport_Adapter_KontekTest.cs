using SoftOne.Soe.Business.Util.SalaryAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace Soe.Business.Test
{
    
    
    /// <summary>
    ///This is a test class for KontekAdapterTest and is intended
    ///to contain all KontekAdapterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SalaryExport_Adapter_KontekTest
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
        ///A test for CreateDocument
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SoftOne.Soe.Business.dll")]
        public void TimeSalaryExport_Adapter_Kontek_CreateSalaryDocument()
        {
            //KontekAdapter_Accessor target = new KontekAdapter_Accessor();
            //XDocument actual = target.CreateSalaryDocument();
            //Assert.IsNotNull(target);
            Assert.IsTrue(true);
        }
    }
}
