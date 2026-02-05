using Microsoft.IdentityModel.Tokens;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Soe.WebApi.Middlewares
{
    public class SOEAuthorizeAttribute : AuthorizeAttribute
    {

        public static string SigningCertificateName { get; set; }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            HttpRequestMessage request = actionContext.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            // Handle double posts made by the browser
            if (actionContext.Request.Method == HttpMethod.Post)
            {
                string postGuid = null;
                IEnumerable<string> values = null;
                request.Headers.TryGetValues("PostGuid", out values);
                if (values != null && values.Any())
                    postGuid = values.FirstOrDefault();

                if (!string.IsNullOrEmpty(postGuid) && postGuid != Guid.Empty.ToString())
                {
                    var exist = BusinessMemoryCache<string>.Get(postGuid);

                    if (!string.IsNullOrEmpty(exist))
                    {
                        try
                        {
                            var parameters = request.Headers.GetValues("soeparameters").First();
                            var decryptedParameters = (new StringEncryption("TestingNewEncryptionToWorkBothIn48AND7")).Decrypt(parameters);
                            SysLogConnector.LogErrorString($"PostGuid {HttpUtility.UrlDecode(request.RequestUri.ToString())} {decryptedParameters}");
                        }
                        catch
                        {
                            // Intentionally ignored, safe to continue
                            // NOSONAR
                        }
                        HandleUnauthorizedRequest(actionContext);
                        return;
                    }
                    else
                    {
                        BusinessMemoryCache<string>.Set(postGuid, postGuid, 10 * 60);
                    }
                }
            }

            // Check if cookie authentication is enabled
            bool isCookieAuthenticationEnabled = IsCookieAuthenticationEnabled();

            if (isCookieAuthenticationEnabled)
            {
                base.OnAuthorization(actionContext);
                return;
            }

            if (authorization == null || authorization.Scheme != "Bearer")
            {
                // If cookie authentication is enabled and there is no bearer token, try cookie authentication
                if (isCookieAuthenticationEnabled)
                {
                    // Perform cookie authentication logic here
                    if (AuthenticateWithCookie(request))
                        return;
                }

                HandleUnauthorizedRequest(actionContext);
                return;
            }

            string token = authorization.Parameter;

            if (!ValidateToken(token))
            {
                // If cookie authentication is enabled and the token validation fails, try cookie authentication
                if (isCookieAuthenticationEnabled)
                {
                    // Perform cookie authentication logic here
                    if (AuthenticateWithCookie(request))
                        return;
                }

                HandleUnauthorizedRequest(actionContext);
                return;
            }
        }

        private bool IsCookieAuthenticationEnabled()
        {
            // Add your logic to determine if cookie authentication is enabled or not
            return true; // Change this based on your implementation
        }

        private bool AuthenticateWithCookie(HttpRequestMessage request)
        {
            // Extract the cookie string from the request headers
            IEnumerable<string> cookieValues;
            if (!request.Headers.TryGetValues("Cookie", out cookieValues))
                return false;

            string cookieString = cookieValues.FirstOrDefault();
            if (string.IsNullOrEmpty(cookieString))
                return false;

            // Split the cookie string into individual cookies
            string[] cookies = cookieString.Split(';');

            // Find the authentication cookie (e.g., ".AspNet.Cookies")
            string authCookieName = ".AspNet.Cookies";
            string authCookie = cookies.FirstOrDefault(c => c.Trim().StartsWith(authCookieName));

            if (string.IsNullOrEmpty(authCookie))
                return false;

            // Extract the authentication token from the authentication cookie
            string token = authCookie.Replace(authCookieName + "=", "").Trim();

            // Validate the token (optional)
            if (!ValidateToken(token))
                return false;

            // Perform any additional cookie-based authentication logic if needed

            // If the authentication is successful, set the principal and return true
            // Example: request.GetRequestContext().Principal = CreatePrincipalFromToken(token);

            return true;
        }


        private bool ValidateToken(string authToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = "SoftOneIdP",
                ValidAudience = "SoftOne",
                IssuerSigningKey = GetSigningKey() // The same key as the one that generate the token
            };
        }

        private X509SecurityKey GetSigningKey()
        {
            return new X509SecurityKey(GetSigningCertificate());
        }
        private X509Certificate2 GetSigningCertificate()
        {
#if RELEASE
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
#else
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
#endif

            {
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, SigningCertificateName, false).Count > 0)
                    return store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, SigningCertificateName, false)[0];
                else
                    return store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, "CN=" + SigningCertificateName, false)[0];

            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
        }
    }

}