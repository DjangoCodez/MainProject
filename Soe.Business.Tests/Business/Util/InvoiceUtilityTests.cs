using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util.Tests
{
    [TestClass()]
    public class InvoiceUtilityTests
    {
        [TestMethod()]
        public void ValidSwedishOCRTest()
        {
            Assert.IsTrue( InvoiceUtility.ValidateSwedishOCRNumber("6200817968431") );
            Assert.IsTrue( InvoiceUtility.ValidateSwedishOCRNumber("21951395710") );
            Assert.IsTrue( InvoiceUtility.ValidateSwedishOCRNumber("0498609100132") );
        }

        [TestMethod()]
        public void InValidSwedishOCRTest()
        {
            Assert.IsFalse(InvoiceUtility.ValidateSwedishOCRNumber("454564654564"));
            Assert.IsFalse( InvoiceUtility.ValidateSwedishOCRNumber("85456454"));
        }

        
    }
}