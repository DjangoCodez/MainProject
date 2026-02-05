//using SoftOne.Soe.Business.Core;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using SoftOne.Soe.Util;
//using System.Collections.Generic;
//using SoftOne.Soe.Business.Util;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Data.Entity.Core.EntityClient;
//using SoftOne.Soe.Data;


//namespace Soe.Business.Test
//{
    
    
//    /// <summary>
//    ///This is a test class for EmailManagerTest and is intended
//    ///to contain all EmailManagerTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class EmailManagerTest
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
//        ///A test for SendEmail
//        ///</summary>
//        [TestMethod()]        
//        public void SendEmailTest()
//        {
//            using (EntityConnection conn = new EntityConnection(TestUtil.GetSoeCompConnection()))
//            {
//                //Arrange
//                var entities = new CompEntities(conn);

//                ParameterObject parameterObject = null;
//                EmailManager target = new EmailManager(parameterObject);
//                int actorCompanyId = 17;
//                int emailTemplateId = 3;
//                Dictionary<int, List<string>> emailRecipients = new Dictionary<int, List<string>>();

//                var attachements = new List<string>();
//                attachements.Add("39a37195-b658-49bf-b7eb-b5a636860d05");
//                emailRecipients.Add(20, attachements);

//                ActionResult expected = null; // TODO: Initialize to an appropriate value
//                ActionResult actual;

//                actual = target.SendEmail(entities,actorCompanyId, emailTemplateId, emailRecipients);


//                Assert.AreEqual(expected, actual);
//                Assert.Inconclusive("Verify the correctness of this test method.");
//            }
//        }
//    }
//}
