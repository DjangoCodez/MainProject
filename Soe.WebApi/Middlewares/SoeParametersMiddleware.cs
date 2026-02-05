using Microsoft.Owin;
using Newtonsoft.Json;
using Soe.WebApi.Security;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Soe.WebApi.Middlewares
{
    public class SoeParametersMiddleware
    {
        private AppFunc _next;

        public SoeParametersMiddleware(AppFunc next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var ctx = new OwinContext(environment);

            if (!ctx.Request.Path.ToString().StartsWith("/translation/", StringComparison.OrdinalIgnoreCase) && !ctx.Request.Path.ToString().ToLower().Contains("softonestatus/getsoftonestatus/"))
            {
                ParameterObject parameterObject = null;

                ClaimsIdentity identity = ctx.Request.User?.Identity as ClaimsIdentity;
                if (identity.TryGetInt(SoeClaimType.UserId, out int userId) && identity.TryGetInt(SoeClaimType.ActorCompanyId, out int actorCompanyId) && identity.TryGetInt(SoeClaimType.RoleId, out int roleId))
                {
                    identity.TryGetInt(SoeClaimType.SupportUserId, out int supportUserId);
                    identity.TryGetInt(SoeClaimType.SupportActorCompanyId, out int supportCompanyId);
                    identity.TryGetBool(SoeClaimType.IsSuperAdminMode, out bool isSuperAdminMode);
                    identity.TryGetBool(SoeClaimType.IsSupportLoggedInByCompany, out bool isSupportLoggedInByCompany);
                    identity.TryGetBool(SoeClaimType.IsSupportLoggedInByCompany, out bool isSupportLoggedInByUser);
                    identity.TryGetBool(SoeClaimType.IncludeInactiveAccounts, out bool includeInactiveAccounts);

                    try
                    {
                        var parameters = ctx.Request.Headers.GetValues("soeparameters").First();
                        if (parameters != null)
                        {
                            var decryptedParameters = (new StringEncryption("TestingNewEncryptionToWorkBothIn48AND7")).Decrypt(parameters);
                            var soeParams = JsonConvert.DeserializeObject<SoeParameters>(decryptedParameters) ?? new SoeParameters();
                            roleId = soeParams.RoleId;
                            actorCompanyId = soeParams.ActorCompanyId;
                        }
                    }
                    catch
                    {
                        LogCollector.LogError("SoeParametersMiddleware Error in reading soeparameters from header");
                    }

                    parameterObject = CreateParameterObject(
                        actorCompanyId,
                        roleId,
                        userId,
                        supportUserId,
                        supportCompanyId,
                        isSuperAdminMode,
                        isSupportLoggedInByCompany,
                        isSupportLoggedInByUser,
                        includeInactiveAccounts);
                }

                ctx.Environment["Soe.ParameterObject"] = parameterObject;
            }

            return _next(environment);
        }

        private ParameterObject CreateParameterObject(
            int actorCompanyId,
            int roleId,
            int userId,
            int? supportUserId,
            int? supportCompanyId,
            bool isSuperAdminMode,
            bool isSupportLoggedInByCompany,
            bool isSupportLoggedInByUser,
            bool includeInactiveAccounts)
        {
            var parameterObject = ParameterObject.Create(
                SessionCache.GetCompanyFromCache(actorCompanyId), 
                SessionCache.GetUserFromCache(userId, actorCompanyId),
                supportCompanyId.HasValidValue() ? SessionCache.GetCompanyFromCache(supportCompanyId.Value) : null,
                supportCompanyId.HasValidValue() && supportUserId.HasValue ? SessionCache.GetUserFromCache(supportUserId.Value, supportCompanyId.Value) : null,
                roleId,
                isSupportLoggedInByCompany,
                isSupportLoggedInByUser, 
                isSuperAdminMode, 
                includeInactiveAccounts);

            parameterObject.SetExtendedUserParams(ExtendedUserParams.Create(GetCurrentDirectory(), GetClientIP(), GetRequestUrl(), GetUserEnvironmentInfo()));
            return parameterObject;
        }     

        private string GetUserEnvironmentInfo()
        {
            return GetHostInfo() + Constants.SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR + GetClientIP();
        }

        private string GetHostIP()
        {
            string ipNr = "";

            IPHostEntry hostEntry = Dns.GetHostEntry(GetHostName());
            if (hostEntry != null)
            {
                IPAddress[] ipHostEntry = hostEntry.AddressList;
                ipNr = ipHostEntry[ipHostEntry.Length - 1].ToString();
            }
            return ipNr;
        }

        private string GetHostName()
        {
            return Dns.GetHostName();
        }

        private string GetHostInfo()
        {
            return $"{GetHostIP()}_{GetHostName()}";
        }

        private string GetCurrentDirectory()
        {
            return HttpContext.Current.Server.MapPath("~");
        }

        private static string GetClientIP()
        {
            string ipNr = "";

            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    ipNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipNr))
                        ipNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch
            {
                //Continue on error on IP
            }

            return ipNr;
        }

        private String GetRequestUrl()
        {
            string url = String.Empty;
            if (HttpContext.Current?.Request?.Url != null)
                HttpContext.Current.Request.Url.ToString();
            return url;
        }
    }
}