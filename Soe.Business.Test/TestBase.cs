using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.Cache;

namespace SoftOne.Soe.Business.Core.Tests
{
    public class TestBase
    {
        protected const string THREAD = "UnitTest";

        protected ParameterObject GetParameterObject(int actorCompanyId = 0, int userId = 0, int roleId = 0)
        {
            new SysServiceManager(null);
            SoeCache.RedisConnectionString = CompDbCache.Instance.RedisCacheConnectionString;

            return ParameterObject.Create(company: actorCompanyId > 0 ? new CompanyManager(null).GetCompany(actorCompanyId)?.ToCompanyDTO() : null,
                                          user: (userId > 0 ? new UserManager(null).GetUser(userId) : new User() { LoginName = this.ToString() })?.ToDTO(),
                                          roleId: roleId > 0 ? new RoleManager(null).GetRole(roleId, actorCompanyId)?.RoleId : (int?)null,
                                          thread: "test");
        }
    }
}
