using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.API.Intrum;
using SoftOne.Soe.Business.Util.API.Zetes;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Util.API.Tests
{
    [TestClass()]
    public class ConnectorTests
    {
        [TestMethod()]
        public void IntrumLogin()
        {
            var connector = new IntrumConnector();
            var token = connector.GetLoginToken(true,new SoftoneIntrumConfiguration { });
            Assert.IsTrue(token != null);
        }

        [TestMethod()]
        public void ZetesLogin()
        {
            var connector = new ZetesConnector();
            var message = connector.GetLoginToken(connector.GetTestOauthConfig(new SoftoneZetesConfiguration { },false));
            Assert.IsTrue(string.IsNullOrEmpty(message));
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
    }

}