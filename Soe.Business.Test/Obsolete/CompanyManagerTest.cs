//using System.Data.Entity.Core.EntityClient;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SoftOne.Soe.Business.Core;
//using SoftOne.Soe.Data;
//using SoftOne.Soe.Util;

//namespace Soe.Business.Test
//{
//    /// <summary>
//    ///This is a test class for CompanyManagerTest and is intended
//    ///to contain all CompanyManagerTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class CompanyManagerTest
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
//        ///A test for GetCompany
//        ///</summary>
//        [TestMethod()]
//        public void GetCompany_WhenValid_ReturnsCompany() //integration test
//        {
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);
//                ParameterObject parameter = null; 
//                CompanyManager target = new CompanyManager(parameter);
//                int actorCompanyId = 2;

//                //Act
//                Company actual = target.GetCompany(entities, actorCompanyId, false);
                
//                //Assert
//                Assert.AreEqual(actorCompanyId, actual.ActorCompanyId);
//            }
//        }
//    }
//}
