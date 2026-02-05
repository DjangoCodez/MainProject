using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class AccountDistributionManagerTests
    {
        [TestMethod()]
        public void GetAccountDistributionTraceViewsTest()
        {
            var am = new AccountDistributionManager(null);
            //am.GetPayrollCalculationProducts(291, 264, 239, false,false); 
            var data = am.GetAccountDistributionTraceViews(34);

            Assert.IsTrue(data != null);
        }
    }
}