using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.BlobStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.BlobStorage.Tests
{
    [TestClass()]
    public class EvoDataStorageConnectorTests
    {
        [TestMethod()]
        public void GetDataStorageTest()
        {
            var ds = EvoDataStorageConnector.GetDataStorage(6595, 9, 90, "f1c4a27e-4f8f-468c-8588-cb2ecc02b0f7");
            Assert.IsNotNull(ds);
            var xml = ds.GetXml();
            Assert.IsNotNull(xml);
            Assert.IsTrue(xml.Length > 0);
            var xdoc = System.Xml.Linq.XDocument.Parse(xml);
            Assert.IsNotNull(xdoc);
        }
    }
}