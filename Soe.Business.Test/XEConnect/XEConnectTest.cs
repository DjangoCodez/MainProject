using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System;

namespace Soe.Business.Test.XEConnect
{
    [TestClass]
    public class XEConnectTest
    {
        [TestMethod]
        public void TestImport()
        {
            ImportExportManager iem = new ImportExportManager(null);
            iem.TestImport();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void CreateSelectXML()
        {
            ImportExportManager iem = new ImportExportManager(null);
            iem.CreateSelectXML(typeof(EmploymentIODTO));
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetTimeCodeTrans()
        {
            ImportExportManager iem = new ImportExportManager(null);
            var result = iem.GetTimeCodeTransactions(17, new DateTime(2016,1,1), DateTime.MaxValue, null);
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        public void GetProjects()
        {
            ImportExportManager iem = new ImportExportManager(null);
            var result = iem.GetProjectIODTOs(17);
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        public void GetCustomersAndSuppliers()
        {
            int actorCompanyId = 17;
            ImportExportManager iem = new ImportExportManager(null);
            var customers =  iem.GetCustomerIODTOs(actorCompanyId,"","",null);
            var suppliers = iem.GetSupplierIODTOs(actorCompanyId, "", "", null);
            Assert.IsTrue(customers != null && suppliers != null);
        }
    }
}
