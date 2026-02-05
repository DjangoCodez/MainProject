using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class SupplierInvoiceArrivalHallTests
    {
        [TestMethod()]
        public void GetSupplierInvoiceArrivalHall()
        {
            var sim = new SupplierInvoiceManager(null);
            var result = sim.GetSupplierInvoiceIncomingHallGrid(7);
            Assert.IsTrue(result.Any());
        }
    }
}
