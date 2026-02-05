using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class BatchUpdateManagerTests : TestBase
    {
        [TestMethod()]
        public void GetBatchUpdateForEntityTest()
        {
            var cm = new BatchUpdateManager(null);
            var result = cm.GetBatchUpdate(SoeEntityType.Customer);
            Assert.IsTrue(result.Any());    
        }
    }
}