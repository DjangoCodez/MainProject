using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace SoftOne.Common.KeyVault.Helpers
{
    internal class KeyVaultAuthenticationHelper
    {
        // I don't think it's thread safe to to store the certificate in a static variable.
        // We should fix this, but don't want to do too many things at once.
        internal static ClientAssertionCertificate AssertionCertificate { get; set; }

        internal static void GetClientAssertionCertificate(string clientId, string distinguishedName, string storeLocation)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new System.ArgumentException("Client ID must be provided when retrieving client assertion certificate.", nameof(clientId));

            if (string.IsNullOrWhiteSpace(distinguishedName))
                throw new System.ArgumentException("Distinguished name must be provided when retrieving client assertion certificate..", nameof(distinguishedName));

            var clientAssertionCertPfx = CertificateHelper.FindBySubjectDistinguishedName(distinguishedName, storeLocation);
            AssertionCertificate = new ClientAssertionCertificate(clientId, clientAssertionCertPfx);
        }

        public static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, AssertionCertificate);
            return result.AccessToken;
        }
    }
}