using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Owin;
using RestSharp;
using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;

namespace SoftOne.Soe.Business.Security
{
    class TokenResponse
    {
        public string access_token { get; set; }
        public string id_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }

    }

    public static class IAppBuilderExtensions
    {
        public class MyCookieManager : ICookieManager
        {

            private readonly SystemWebChunkingCookieManager chunkingCookieManager;
            public MyCookieManager()
            {
                chunkingCookieManager = new SystemWebChunkingCookieManager();
            }
            public string GetRequestCookie(IOwinContext context, string key)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                var requestCookies = webContext.Request.Cookies;
                var escapedKey = Uri.EscapeDataString(key);
                var cookie = requestCookies[escapedKey];
                
                if (cookie == null)                
                    return null;
                
                int chunksCount = ParseChunksCount(cookie.Value);
                if (chunksCount > 0)
                {
                    string[] chunks = new string[chunksCount];
                    chunks[0] = requestCookies[escapedKey].Value;
                    for (int chunkId = 1; chunkId <= chunksCount; chunkId++)
                    {
                        cookie = requestCookies[escapedKey + "C" + chunkId.ToString(CultureInfo.InvariantCulture)];
                        string chunk = cookie.Value;
                        chunks[chunkId - 1] = chunk;
                    }
                    string merged = string.Join(string.Empty, chunks);
                    return Uri.UnescapeDataString(merged);
                }

                return Uri.UnescapeDataString(requestCookies[escapedKey].Value);
            }

            public void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options)
            {
                chunkingCookieManager.AppendResponseCookie(context, key, value, options);
            }

            public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
            {
                chunkingCookieManager.DeleteCookie(context, key, options);
            }

            private int ParseChunksCount(string value)
            {
                if (value != null && value.StartsWith("chunks-", StringComparison.Ordinal))
                {
                    string chunksCountString = value.Substring("chunks-".Length);
                    int chunksCount;
                    if (int.TryParse(chunksCountString, NumberStyles.None, CultureInfo.InvariantCulture, out chunksCount))
                    {
                        return chunksCount;
                    }
                }
                return 0;
            }
        }

        public static IAppBuilder ConfigureAuthentication(this IAppBuilder app, string sysDbConnectionstring, KeyVaultSettings keyVaultSettings)
        {
            
            var dataProtectionProvider = CreateDataProtectionProvider(sysDbConnectionstring, "SO.Web", keyVaultSettings);
            var dataProtector = dataProtectionProvider.CreateProtector(
                            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                            // Must match the Scheme name used in the ASP.NET Core app, i.e. CookieAuthenticationDefaults.ApplicationScheme
                            CookieAuthenticationDefaults.AuthenticationType,
                            "v2"
                        );
#if DEBUG
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif

            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.UseCookieAuthentication(new Microsoft.Owin.Security.Cookies.CookieAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                CookieName = ".AspNetCore.Cookies",
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(240),
                LoginPath = new PathString("/login.aspx"),
                TicketDataFormat = new AspNetTicketDataFormat(new DataProtectorShim(dataProtector)),
                CookieManager = new MyCookieManager(),
            });



            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = SoftOneIdConnector.GetUri().ToString(),
                ClientId = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, "SoftOneId-WebCookieIdP-ClientId", keyVaultSettings.StoreLocation),
                ClientSecret = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, "SoftOneId-WebCookieIdP-Secret", keyVaultSettings.StoreLocation),
                RedirectUri = new Uri(ConfigurationSetupUtil.GetCurrentUrl()).EnsureTrailingSlash(),
                ResponseMode = OpenIdConnectResponseMode.FormPost,
                ResponseType = "code",
                Scope = "openid profile SoftOne",
                SaveTokens = false,
                UsePkce = false,
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = (context) =>
                    {
                        context.ProtocolMessage.RedirectUri = new Uri(ConfigurationSetupUtil.GetCurrentUrl()).EnsureTrailingSlash();
                        return Task.FromResult(0);
                    },
                    AuthorizationCodeReceived = (context) =>
                    {
                        try
                        {
                            var clientId = context.TokenEndpointRequest.ClientId;
                            var clientSecret = context.TokenEndpointRequest.ClientSecret;
                            var code = context.TokenEndpointRequest.Code;
                            var redirectUri = context.TokenEndpointRequest.RedirectUri;

                            var response = GetTokenFromSoftOneId(clientId, clientSecret, code, redirectUri);
                            var idToken = response.id_token;
                            var accessToken = response.access_token;

                            // Handle code redemption will ensure that the response is valid and will set the claims principal
                            context.HandleCodeRedemption(accessToken, idToken);
                        }
                        catch (Exception ex)
                        {
                            LogCollector.LogWithTrace($"[AuthorizationCodeReceived] {ex}");
                            context.HandleResponse();
                        }
                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = (context) =>
                    {
                        LogCollector.LogWithTrace($"[AuthenticationFailed] {context.Exception.Message}");
                        return Task.FromResult(0);
                    },
                    SecurityTokenValidated = (context) =>
                    {
                        var claims = context.AuthenticationTicket.Identity.Claims;

                        var claimsJson = JsonConvert.SerializeObject(claims, 
                            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); // Each claim has a reference to the claims, causing a loop
                        LogCollector.LogWithTrace($"[SecurityTokenValidated] Claims: {claimsJson}");

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType);
                        var userGuid = identity.FindFirst(SoeClaimType.UserGuid);

                        if (userGuid?.Value != null && Guid.Parse(userGuid.Value) != Guid.Empty)
                            EvoDistributionCacheConnector.UpsertCachedValue($"IdToken{userGuid.Value}", context.ProtocolMessage.IdToken, TimeSpan.FromHours(20), true);

                        return Task.FromResult(0);
                    },
                    MessageReceived = (context) =>
                    {
                        return Task.FromResult(0);
                    },
                    SecurityTokenReceived = (context) =>
                    {
                        LogCollector.LogWithTrace($"[SecurityTokenReceived] Token: {context.ProtocolMessage.IdToken}");
                        return Task.FromResult(0);
                    },
                    TokenResponseReceived = (context) =>
                    {
                        LogCollector.LogWithTrace($"[TokenResponseReceived] Token: {context.ProtocolMessage.AccessToken}");
                        return Task.FromResult(0);
                    },
                }

            });
            return app;
        }

        public static Guid? GetGuidClaim(HttpContext context, string type)
        {
            var identity = (ClaimsIdentity)context.User.Identity;
            var claim = identity.FindFirst(type);
            if (claim != null && Guid.TryParse(claim.Value, out Guid ret))
            {
                if (ret == Guid.Empty)
                    return null;
                return ret;
            }
            return null;
        }

        private static TokenResponse GetTokenFromSoftOneId(string clientId, string clientSecret, string code, string redirectUri)
        {
            var tokenBase = SoftOneIdConnector.GetUri().RemoveTrailingSlash().ToString();
            var httpClient = new GoRestClient(tokenBase);

            var request = new RestRequest("/connect/token", Method.Post)
                .AddHeader("Accept", "application/json")
                .AddParameter("grant_type", "authorization_code")
                .AddParameter("client_id", clientId)
                .AddParameter("client_secret", clientSecret)
                .AddParameter("code", code)
                .AddParameter("redirect_uri", redirectUri);

            var response = httpClient.ExecuteAsync(request)
                .GetAwaiter()
                .GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"[GetTokenFromSoftOneId] ResponseCode: {response.StatusCode}, Resp: {response.Content}");
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(response.Content);

            if (string.IsNullOrEmpty(token?.id_token))
                throw new Exception("[GetTokenFromSoftOneId] Missing id_token in response.");

            return token;
        }

        private static IEnumerable<Claim> GetClaimsFromIdToken(string idToken) {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);
            return jwtToken.Claims;
        }

        private static IDataProtectionProvider CreateDataProtectionProvider(string sysDbConnectionstring, string applicationName, KeyVaultSettings keyVaultSettings)
        {
            var certicate = GetSigningCertificate(keyVaultSettings.CertificateDistinguishedName);
            var services = new ServiceCollection();

            services.AddDataProtection()
                .PersistKeysToSysDb(sysDbConnectionstring)
                .SetApplicationName(applicationName)
                .ProtectKeysWithCertificate(certicate);

            return services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();
        }

        private static X509Certificate2 GetSigningCertificate(string certificateName)
        {
#if RELEASE
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
#else
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
#endif

            {
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, certificateName, false).Count > 0)
                    return store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, certificateName, false)[0];
                else
                    return store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, "CN=" + certificateName, false)[0];
            }
        }
    }
}