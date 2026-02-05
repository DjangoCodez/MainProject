using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.Tests
{
    [TestClass()]
    public class Hogia214002AdapterTests
    {
        [TestMethod()]
        public void FillWithBlanksTest()
        {
            Hogia214002Adapter adapter = new Hogia214002Adapter(null, false);
            var test1 = adapter.FillWithBlanks(6, "häj", true);
            var test2 = adapter.FillWithBlanks(6, string.Empty, true);
            Assert.IsTrue(test1 != null && test2 != null);           
        }
    }
}