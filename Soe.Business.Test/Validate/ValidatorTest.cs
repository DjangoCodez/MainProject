using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;

namespace Soe.Business.Test.Validate
{
    [TestClass]
    public class ValidatorTest
    {
        [TestMethod]
        public void TestValidBankNumberSE()
        {
            var result = Validator.IsValidBankNumberSE((int)TermGroup_SysPaymentType.Bank, "9023", "398-7T187");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestValidEmail()
        {
            var result = Validator.IsValidEmailAddress("ekonomi@nojdhselektronik.se ");
            Assert.IsTrue(result);
        }
    }
}
