using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.DI;
using System.Security.Claims;
using System.Web;

namespace SoftOne.Soe.Web.Util
{
    public class ParameterObjectProvider : IParameterObjectProvider
    {
        private readonly HttpContext _httpContext;
        private ParameterObject _parameterObject;

        public ParameterObjectProvider(HttpContext httpContext)
        {
            this._httpContext = httpContext;
        }

        public ParameterObject CreateParameterObject()
        {
            if (this._parameterObject == null && _httpContext?.User?.Identity is ClaimsIdentity)
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity.IsAuthenticated)
                {
                    var user = SessionCache.GetUserFromCache(identity);
                    var company = SessionCache.GetCompanyFromCache(identity);
                    this._parameterObject = LazyParameterObject.Create(identity, company, user, null, null);
                }
            }
            return this._parameterObject;
        }
    }
}