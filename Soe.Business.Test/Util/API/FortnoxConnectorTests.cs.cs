using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.API.Fortnox;
using SoftOne.Soe.Business.Util.API.VismaEAccounting;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util.API.Tests
{
    [TestClass()]
    public class FortnoxConnectorTests
    {
        [TestMethod()]
        public void Login()
        {
            var connector = new FortnoxConnector();
            connector.SetAuthFromRefreshToken("13666d9db7fcad417f22c1168dfa7f831404447c");
            var token = connector.GetRefreshToken();
            Assert.IsTrue(token != null);
        }

        public ParameterObject GetParameterObject(int actorCompanyId, int userId)
        {
            Company company = null;
            if (actorCompanyId > 0)
                company = new CompanyManager(null).GetCompany(actorCompanyId);

            User user = null;
            if (userId > 0)
                user = new UserManager(null).GetUser(userId);
            else
                user = new User() { LoginName = this.ToString() };

            return ParameterObject.Create(company: company.ToCompanyDTO(),
                                          user: user.ToDTO(),
                                          thread: "test");
        }


        [TestMethod()]
        public void TestEndpoints()
        {
            var param = GetParameterObject(7, 72);
            var settingManager = new SettingManager(param);
            var refreshToken = settingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingVismaEAccountingRefreshToken, 72, 7, 0);
            var connector = new VismaEAccountingIntegrationManager();
            connector.SetAuthFromRefreshToken(refreshToken);
            var token = connector.GetRefreshToken();
            settingManager.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingVismaEAccountingRefreshToken, token, 72, 7, 0);
            Assert.IsTrue(token != null);
        }
    }
}