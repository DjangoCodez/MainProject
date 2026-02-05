using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.Cache;

namespace Soe.Business.Tests
{
    public class TestBase
    {

        protected void Init()
        {
            ConfigurationSetupUtil.Init();
            InitDatabase();
        }
        protected void InitDatabase()
        {
            SysServiceManager ssm = new SysServiceManager(null);
            SoeCache.RedisConnectionString = CompDbCache.Instance.RedisCacheConnectionString;
        }

        protected ParameterObject GetParameterObject(int actorCompanyId = 0, int userId = 0)
        {
            CompanyDTO company = null;
            UserDTO user = null;
            int defaultRoleId = 0;

            if (actorCompanyId > 0 || userId > 0)
            {
                SoeCache.RedisConnectionString = CompDbCache.Instance.RedisCacheConnectionString;

                if (actorCompanyId > 0)
                    company = new CompanyManager(null).GetSoeCompany(actorCompanyId);

                if (userId > 0)
                    user = new UserManager(null).GetSoeUser(actorCompanyId, userId);
                else
                    user = new UserDTO { LoginName = this.ToString() };

                defaultRoleId = new UserManager(null).GetDefaultRoleId(actorCompanyId, userId);
            }

            return ParameterObject.Create(
                user: user,
                company: company,
                thread: "Soe.Business.Tests",
                roleId: defaultRoleId
                );
        }
    }
}
