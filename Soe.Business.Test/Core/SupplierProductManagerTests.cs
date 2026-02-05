using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class SupplierProductManagerTests
    {
        [TestMethod()]
        public void GetSupplierProductPricelistsTest()
        {
            var sim = new SupplierProductManager(null);
            var result = sim.GetSupplierProductPricelists(7, 491);
            Assert.IsTrue(result.Any());
        }
    }
}
