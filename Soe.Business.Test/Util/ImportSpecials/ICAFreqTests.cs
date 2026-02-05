using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SoftOne.Soe.Business.Util.ImportSpecials.Tests
{
    [TestClass()]
    public class ICAFreqTests
    {
        [TestMethod()]
        public void ApplyTest()
        {
            string s = File.ReadAllText(@"C:\Users\mrickardk.SOFTONE\Downloads\WFM_ICA_SalesData_BI_02431_20170303_110234.txt");
            ICAFreq freq = new ICAFreq(null);
            var dtos = freq.GetStaffingNeedsFrequencyIODTOs(s);
            Assert.IsTrue(dtos != null);
        }
    }
}