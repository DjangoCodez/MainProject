using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util
{
    public static class SoftOneIdUtil
    {
        public const string RedirectUri = "/login.aspx";
        public const string ClientId = "Soe.Web_PKCE";
        public const string Scopes = "SoftOne";
        public const string ScopesMobile = "SoftOne.Mobile";
        public const string SoftOneIdSecuritySecret = "SoftOneId-Security-Secret";

        private static string _oidcClientRedirectUri;
        public static string OidcClientRedirectUri
        {
            get
            {
                var url = new Uri(ConfigurationSetupUtil.GetCurrentUrl()).RemoveTrailingSlash() + RedirectUri;
                if (!string.IsNullOrEmpty(url))
                {
                    _oidcClientRedirectUri = url;
                }
                return _oidcClientRedirectUri;
            }
        }

        public static string GetCertificateName(bool isTest)
        {
            return isTest ? "SoftOneIdentityServer" : "SoftOneId";
        }
    }
}
