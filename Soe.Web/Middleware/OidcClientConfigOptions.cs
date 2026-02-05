using Microsoft.Owin;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Util;
using System;
using System.Web;

namespace SoftOne.Soe.Web.Middleware
{
    public class OidcClientConfigOptions
    {
        public OidcClientConfigOptions()
        {
            EndpointPath = new PathString("/cssjs/oidc/oidc-client-config.js");
            Authority = SoftOneIdConnector.GetUri().RemoveTrailingSlash().ToString();
            ClientId = "Testing_Cookie";
            GetPostLogoutRedirectUri = x => new Uri(ConfigurationSetupUtil.GetCurrentUrl()).EnsureTrailingSlash();
            SilentTokenRenewalRedirectPath = "silentsignin.aspx";
            Scopes = "openid profile SoftOne";
        }

        public PathString EndpointPath { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri
        {
            get
            {
                var request = HttpContext.Current.Request;
                var scheme = request.Headers["X-Forwarded-Proto"] ?? request.Url.Scheme;
                var host = request.Headers["X-Forwarded-Host"] ?? request.Url.Host;
                var portString = request.Headers["X-Forwarded-Port"] ?? request.Url.Port.ToString();

                if (int.TryParse(portString, out int port))
                {
                    var builder = new UriBuilder(scheme, host, port);
                    return builder.Uri.EnsureTrailingSlash().ToString() + "login.aspx";
                }
                else
                {
                    var builder = new UriBuilder(scheme, host);
                    return builder.Uri.EnsureTrailingSlash().ToString() + "login.aspx";
                }
            }
        }
        public Func<IOwinContext, string> GetPostLogoutRedirectUri { get; set; }
        public string SilentTokenRenewalRedirectPath { get; set; }
        public string Scopes { get; set; }

    }
}