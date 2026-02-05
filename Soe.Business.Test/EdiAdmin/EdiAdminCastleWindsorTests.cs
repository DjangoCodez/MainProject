//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SoftOne.EdiAdmin.Business;
//using SoftOne.EdiAdmin.Business.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Soe.Business.Test.EdiAdmin
//{
//    [TestClass]
//    public class EdiAdminCastleWindsorTests
//    {
//        [TestMethod]
//        public void TestEdiAdmin()
//        {
//            var instance = EdiAdminFactory.Instance.GetEdiAdminManager<EdiAdminManager>();

//            Assert.IsTrue(instance.GetType().Equals(typeof(EdiAdminManager)));
//        }

//        [TestMethod]
//        public void TestFileWatcherManager()
//        {
//            var instance = EdiAdminFactory.Instance.GetEdiAdminManager<EdiAdminFolderWatcherManager>();

//            Assert.IsTrue(instance.GetType().Equals(typeof(EdiAdminFolderWatcherManager)));
//        }

//    }
//}
