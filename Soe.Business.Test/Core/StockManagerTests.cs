using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class StockManagerTests : TestBase
    {
        [TestMethod()]
        public void GetStockTransactionsTest()
        {
            var cm = new StockManager(null);
            var result = cm.GetStockTransactionDTOs(9, new System.DateTime(2025,1,23));
            Assert.IsTrue(result.Any());
        }
    }
}