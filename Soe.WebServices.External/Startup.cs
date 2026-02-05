using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Mobile.Objects;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Web.Middleware;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.Cache;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

[assembly: OwinStartup("Owin.WebServicesExternal", typeof(Soe.WebServices.External.Startup))]


namespace Soe.WebServices.External
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
     
            if (HttpContext.Current?.Server != null)
                ConfigSettings.SetCurrentDirectory(HttpContext.Current.Server.MapPath("~"));
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ConfigurationSetupUtil.Init();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogInfo($"WSX start from machine {Environment.MachineName} from  {HttpContext.Current.Server.MapPath("~")}");
            SoeCache.RedisConnectionString = CompDbCache.Instance.RedisCacheConnectionString;

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            app.UseForwardedHeaders();
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationType = "Bearer",
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new string[] { SoftOneIdUtil.ScopesMobile },
                IssuerSecurityKeyProviders = new X509CertificateSecurityKeyProvider[]
                {
                    new X509CertificateSecurityKeyProvider("SoftOneIdP", GetCertificate())
                }
            });

            var excludedPaths = new List<string>() { "/", "/edi/ediservice.asmx", "/tele2/tele2.asmx", "/io/connect.asmx" };

            app.Use(async (ctx, next) =>
            {
                try
                {
                    bool skipAuthOnNonSoapRequest = true;

#if !DEBUG
                        skipAuthOnNonSoapRequest  =  ctx.Request.Body == null || ctx.Request.Body.Length == 0;
#endif


                    if (skipAuthOnNonSoapRequest || excludedPaths.Contains(ctx.Request.Path.ToString().ToLower()))
                    {
                        await next();
                        return;
                    }

                    if ((!string.IsNullOrEmpty(ctx.Request.Headers["Authorization"]) || !string.IsNullOrEmpty(ctx.Request.Headers["authorization"])) && !ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        SetHttpStatusCode(ctx, HttpStatusCode.Unauthorized, "Token Invalid");
                        return;
                    }

                    UserParameters userParameters = new UserParameters(0, 0, 0);
                    bool userParametersSet = false;

                    if (RequireAuthorization && !ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        SetHttpStatusCode(ctx, HttpStatusCode.Unauthorized, "Token Invalid");
                        return;
                    }
                    else if (ctx.Authentication.User.Identity.IsAuthenticated)
                    {
                        userParameters = MapUserGuid(ctx.Authentication.User.GetUserGuid());
                        if (userParameters == null)
                        {
                            try
                            {
                                var xml = XDocument.Load(ctx.Request.Body).ToString().ToLower();

                                if (xml.Contains("key") && xml.Contains("status") && xml.Contains("guid"))
                                {
                                    await next();
                                    return;
                                }

                            }
                            catch
                            {
                                //always continue
                            }

                            ssm.LogInfo("App: Invalid userguid");
                            SetHttpStatusCode(ctx, HttpStatusCode.NotAcceptable, "Invalid userguid");
                            return;
                        }

                        userParametersSet = TryOverrideUserParametersWithPostedValues(userParameters, ctx, ssm);

                        if (!HasAccess(ctx.Authentication.User.GetUserGuid(), userParameters.UserId, userParameters.RoleId, userParameters.CompanyId))
                        {
                            ssm.LogInfo("App: Invalid user");
                            SetHttpStatusCode(ctx, HttpStatusCode.NotAcceptable, "Invalid user");
                            userParametersSet = false;
                            return;
                        }
                    }

                    if (userParametersSet)
                    {
                        var user = GetUser(userParameters.UserId);
                        if (user == null)
                        {
                            SetHttpStatusCode(ctx, HttpStatusCode.Forbidden, "Not a valid user");
                            return;
                        }

                        var company = GetCompany(userParameters.CompanyId);
                        if (company == null)
                        {
                            SetHttpStatusCode(ctx, HttpStatusCode.Forbidden, "Not a valid company");
                            return;
                        }

                        ctx.Environment["Soe.UserParameters"] = userParameters;
                        ctx.Environment["Soe.ParameterObject"] = ParameterObject.Create(user: user.ToDTO(),
                                                                                        company: company.ToCompanyDTO(), 
                                                                                        thread: "WebService", 
                                                                                        roleId: userParameters.RoleId);
                    }

                    await next();
                }
                catch (Exception ex)
                {
                    ssm.LogError(ex.ToString());
                }

            });
            app.Use<CorrelationIdMiddleware>();
            app.UseClaimsTransformation(incoming =>
            {
                try
                {
                    var environment = HttpContext.Current.GetOwinContext().Environment;
                    if (incoming.Identity.IsAuthenticated)
                    {
                        var identity = incoming.Identities?.FirstOrDefault();

                        if (identity != null && environment.ContainsKey("Soe.UserParameters"))
                        {
                            var userParams = (UserParameters)environment["Soe.UserParameters"];
                            if (userParams != null)
                            {
                                AddOrReplaceClaim(identity, "urn:soe:user_id", userParams.UserId.ToString());
                                AddOrReplaceClaim(identity, "urn:soe:company_id", userParams.CompanyId.ToString());
                                AddOrReplaceClaim(identity, "urn:soe:role_id", userParams.RoleId.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ssm.LogError("UseClaimsTransformation " + ex.ToString());
                }

                return Task.FromResult(new ClaimsPrincipal(incoming));
            });


        }

        private User GetUser(Guid idLoginGuid)
        {
            var key = $"UserCacheGuid_{idLoginGuid}";
            if (MemoryCache.Default.Contains(key))
                return (User)MemoryCache.Default.Get(key);

            var user = new UserManager(null).GetUserForMobileLogin(idLoginGuid);
            if (user != null)
            {
                var slidingExp = new UserManager(null).HasIdLoginGuidsWithMultipleUsers(idLoginGuid) ? 10 : 5 * 60;
                MemoryCache.Default.Add(key, user, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(slidingExp) });
            }
            return user;
        }

        private User GetUser(int userId)
        {
            var key = $"UserCacheUserid_{userId}";
            if (MemoryCache.Default.Contains(key))
                return (User)MemoryCache.Default.Get(key);
            var user = new UserManager(null).GetUser(userId);
            if (user != null)
                MemoryCache.Default.Add(key, user, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return user;
        }

        private Company GetCompany(int companyId)
        {
            var key = $"companyCachecompanyid_{companyId}";
            if (MemoryCache.Default.Contains(key))
                return (Company)MemoryCache.Default.Get(key);

            var company = new CompanyManager(null).GetCompany(companyId);
            if (company != null)
                MemoryCache.Default.Add(key, company, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return company;
        }

        private UserParameters MapUserGuid(Guid idLoginGuid)
        {
            var key = "UserParameters_" + idLoginGuid.ToString();
            if (MemoryCache.Default.Contains(key))
            {
                var cached = (UserParameters)MemoryCache.Default[key];

                if (cached != null && cached.UserId != 0)
                    return cached;
            }

            UserManager userManager = new UserManager(null);
            var user = GetUser(idLoginGuid);
            if (user != null && user.DefaultActorCompanyId.HasValue)
            {
                int defaultRoleId = userManager.GetDefaultRoleId(user.DefaultActorCompanyId.Value, user);
                if (ValidateMobileUserCompanyRole(user.LicenseId, user.UserId, defaultRoleId, user.DefaultActorCompanyId.Value))
                {
                    var userParameters = new UserParameters(user.UserId, defaultRoleId, user.DefaultActorCompanyId.Value);
                    var slidingExp = userManager.HasIdLoginGuidsWithMultipleUsers(idLoginGuid) ? 5 : 5 * 60;
                    MemoryCache.Default.Add(key, userParameters, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(slidingExp) });
                    return userParameters;
                }
            }

            return null;
        }

        private bool ValidateMobileUserCompanyRole(int licenseId, int userId, int roleId, int actorCompanyId)
        {
            var key = $"licenseid_{licenseId}_userid_{userId}_roleid_{roleId}_actorCompanyid{actorCompanyId}";
            if (MemoryCache.Default.Contains(key))
                return true;

            UserManager userManager = new UserManager(null);
            if (userManager.ValidateMobileUserCompanyRole(licenseId, userId, roleId, actorCompanyId))
            {
                var x = new UserParameters(userId, roleId, actorCompanyId); // TODO: Implement
                MemoryCache.Default.Add(key, x, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(30) });
                return true;
            }

            return false;
        }

        private bool HasAccess(Guid userGuid, int requestedUserId, int roleId, int actorCompanyId)
        {
            User fromGuid = GetUser(userGuid);
            User fromId = GetUser(requestedUserId);

            if (fromGuid == null || fromId == null)
                return false;

            if (fromGuid.UserId != fromId.UserId)
                return false;

            return ValidateMobileUserCompanyRole(fromId.LicenseId, fromId.UserId, roleId, actorCompanyId);
        }

        private bool RequireAuthorization
        {
            get
            {

#if !DEBUG
                bool authorize = false;
                var value = ConfigurationManager.AppSettings["Authorize"];
                if (!string.IsNullOrEmpty(value))
                    if (bool.TryParse(value, out authorize))
                        return authorize;
#endif
                return false;
            }
        }

        private X509Certificate2 GetCertificate()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                string signingCertificateName = SoftOneIdUtil.GetCertificateName(CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test);
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, signingCertificateName, false).Count > 0)
                    return store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, signingCertificateName, false)[0];
                else
                    return store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, "CN=" + signingCertificateName, false)[0];
            }
        }

        private bool TryOverrideUserParametersWithPostedValues(UserParameters userParameters, IOwinContext ctx, SysServiceManager ssm = null)
        {
            try
            {
                var xml = XDocument.Load(ctx.Request.Body);
                if (ssm != null && CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test)
                    ssm.LogInfo("App-Startup - body: " + xml.ToString());

                ctx.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
                //if (ssm != null && CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test)
                //{
                //    var log = new StringBuilder(ctx.Request.Headers.Count);
                //    foreach (var header in ctx.Request.Headers)
                //    {
                //        log.AppendLine(header.Key + ": " + string.Join(",", header.Value));
                //    }
                //    ssm.LogInfo("App-Startup - headers: " + log.ToString());
                //}

                //var ns = xml.Root
                //            .Descendants().First()  // soap:Envelope
                //            .Descendants().First()  // soap:Body
                //            .Descendants().First()  // Method element
                //            .Name.NamespaceName;    // Namespace of method and arguments

                var body = (from e in xml.Root.Elements()
                            where e.Name.LocalName == "Body"
                            select e).FirstOrDefault(); //Get Body element

                var ns = body.Descendants().First().Name.NamespaceName; //Namespace of method and arguments

                if (ssm != null && CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test)
                    ssm.LogInfo("App-Startup - ns: " + ns);

                var userIdElement = xml.Descendants(XName.Get("userId", ns)).FirstOrDefault();
                userParameters.UserId = userIdElement != null ? int.Parse(userIdElement.Value) : userParameters.UserId;

                var roleIdElement = xml.Descendants(XName.Get("roleId", ns)).FirstOrDefault();
                userParameters.RoleId = roleIdElement != null ? int.Parse(roleIdElement.Value) : userParameters.RoleId;

                var actorCompanyIdElement = xml.Descendants(XName.Get("companyId", ns)).FirstOrDefault();
                userParameters.CompanyId = actorCompanyIdElement != null ? int.Parse(actorCompanyIdElement.Value) : userParameters.CompanyId;

                var deviceType = ctx.Request.Headers.FirstOrDefault(x => x.Key == "DeviceType");
                if (deviceType.Key != null && deviceType.Value != null && deviceType.Value.Length > 0)
                {
                    userParameters.MobileDeviceTypeFromString(deviceType.Value[0]);
                }

                return userParameters.UserId != 0;
            }
            catch
            {
                //Always continuie;
            }

            //#if DEBUG
            //userParameters.MobileDeviceType = MobileDeviceType.Android;
            //return true;
            //#else
            return userParameters.UserId != 0;
            //#endif
        }

        private void AddOrReplaceClaim(ClaimsIdentity identity, string claimName, string value)
        {
            if (string.IsNullOrEmpty(claimName) || string.IsNullOrEmpty(value))
                return;

            var claim = identity.FindFirst(claimName);
            if (claim != null)
                identity.TryRemoveClaim(claim);

            identity.AddClaim(new Claim(claimName, value));
        }

        private void SetHttpStatusCode(IOwinContext ctx, HttpStatusCode statusCode, string message)
        {
            ctx.Response.StatusCode = (int)statusCode;
            ctx.Response.Write(MobileMessages.GetErrorMessageDocument(message).ToString());
            Thread.Sleep(1000);
        }


    }
}
