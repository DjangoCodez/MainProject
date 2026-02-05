using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Soe.Api.Internal.Middlewares
{
    public class SOEAuthorizeAttribute : AuthorizeAttribute
    {

        public static string SigningCertificateName { get; set; }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            HttpRequestMessage request = actionContext.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            if (authorization == null || authorization.Scheme != "Bearer")
            {
                HandleUnauthorizedRequest(actionContext);
                return;
            }

            string token = authorization.Parameter;

            if (!ValidateToken(token))
                HandleUnauthorizedRequest(actionContext);
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
                ValidAudience = "SoftOne.Internal",
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