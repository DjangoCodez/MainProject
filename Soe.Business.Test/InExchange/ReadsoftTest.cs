using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.Business.Test.InExchange
{
    [TestClass]
    public class ReadsoftTest
    {
        public ParameterObject GetParamsObject(int userId = 72, int companyId = 7, int roleId = 0)
        {
            //Defaults are Anders Svensson, Hantverkardemo (demo db)
            Company company = new CompanyManager(null).GetCompany(companyId, true);
            User user = new UserManager(null).GetUser(userId);

            return ParameterObject.Create(company: company.ToCompanyDTO(),
                                          user: user.ToDTO(),
                                          roleId: roleId,
                                          thread: "");
        }

        [TestMethod]
        public void TestFetchFromAPI()
        {
            ActionResult result = new ActionResult();
            var em = new EdiManager(GetParamsObject());

            result = em.AddScanningEntrysFromWebService(7);

            Assert.IsTrue(result.Success);
        }

    }
}
