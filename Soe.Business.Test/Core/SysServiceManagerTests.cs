using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Sys.Common.DTO;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class SysServiceManagerTests
    {
        [TestMethod()]
        public void PopulateAzureSearchTest()
        {
            SysServiceManager smm = new SysServiceManager(null);
            var result = smm.PopulateAzureSearch();
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetSysCompanyTest()
        {
            SysServiceManager smm = new SysServiceManager(null);
            var company = smm.GetSysCompany(2201, false);
            Assert.IsTrue(company != null);
            company = smm.GetSysCompany(2201, true);
            Assert.IsTrue(company != null);
        }

        [TestMethod()]
        public void SearchCompanyTest()
        {
            SysServiceManager smm = new SysServiceManager(null);
            var filter = new SearchSysCompanyDTO
            {
                UsesBankIntegration = true,
                BankAccount = new SearchSysCompanyBankAccountDTO()
                {
                    BIC = "HANDSESS",
                    PaymentNr = "12-123XX",
                    PaymentType = Common.Util.TermGroup_SysPaymentType.Bank
                }
            };
            var companies1 = smm.SearchSysCompanies(filter);
            Assert.IsTrue(companies1.Count == 1);

            filter = new SearchSysCompanyDTO
            {
                UsesBankIntegration = true,
                FinvoiceAddress = "XXX123456789"
            };
            var companies2 = smm.SearchSysCompanies(filter);
            Assert.IsTrue(companies2.Count == 1);
        }
    }
}