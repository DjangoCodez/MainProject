using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class CompanyManagerTests: TestBase
    {
        [TestMethod()]
        public void CopyCustomerInvoicesFromTemplateCompanyTest()
        {
            CompanyManager cm = new CompanyManager(null);
            ActionResult result = cm.CopyCustomerInvoicesFromTemplateCompany(18312, 35897, 644, false, Common.Util.SoeOriginType.Order);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetSysCountry()
        {
            var m = new CountryCurrencyManager(null);
            int? data = (int?)m.GetSysCountry("UK")?.SysCountryId;
            if (data != null)
                data = (int?)m.GetSysCountry("UK")?.SysCountryId;
            if (data != null)
                data = (int?)m.GetSysCountry("GB")?.SysCountryId;
            if (data != null)
                data = (int?)m.GetSysCountry(247)?.SysCountryId;
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetCompanySysCountry()
        {
            var cm = new CompanyManager(GetParameterObject(2092129));

            int sysCountryId;

            using (CompEntities entities = new CompEntities())
            {
                sysCountryId = 0;
                //sysCountryId = cm.GetCompanySysCountryIdFromCache(entities, 7);
                //sysCountryId = cm.GetCompanySysCountryIdFromCache(entities, 2092129);
            }

            Assert.IsTrue(sysCountryId != 0);
        }

        [TestMethod()]
        public void GetCompanySysCountryDateFormat()
        {
            var cm = new CountryCurrencyManager(GetParameterObject(2092129));

            var dateString = cm.GetDateFormatedForCountry(3,new System.DateTime(2024, 1, 14));
            Assert.IsTrue(!string.IsNullOrEmpty(dateString));
        }

        [TestMethod()]
        public void SearchCompany()
        {
            var c = new CompanyManager(null);

            var x = c.GetCompaniesBySearch(new Common.DTO.CompanySearchFilterDTO {Demo=false, BankAccountBIC = "HANDSESS", BankAccountNr = "2255-5555", BankAccountType = TermGroup_SysPaymentType.BG });

            Assert.IsTrue(x != null);
        }


        [TestMethod()]
        public void CopyCompanyCollectiveAgreementsAndEmployeeTemplatesTest()
        {
            using (CompEntities entities = new CompEntities())
            {
                var actorCompanyIds = entities.Report.Where(w => w.ReportTemplateId == 121).Select(s => s.ActorCompanyId).Distinct().ToList();

                foreach (var company in actorCompanyIds)
                {
                    CompanyManager cm = new CompanyManager(GetParameterObject(company, entities.User.FirstOrDefault(f => f.DefaultActorCompanyId == company)?.UserId ?? 0));
                    cm.CopyCompanyCollectiveAgreements(company, 55487, null, null, null);
                    cm.CopyEmployeeTemplates(company, 55487, true, null);
                }

                Assert.IsTrue(true);
            }
        }
    }
}