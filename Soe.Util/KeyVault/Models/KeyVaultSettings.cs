using Newtonsoft.Json;
using System.Configuration;

namespace SoftOne.Common.KeyVault.Models
{
    public class KeyVaultSettings
    {
        public string CertificateDistinguishedName { get; set; }
        public string KeyVaultUrl { get; set; }
        public string ClientId { get; set; }
        public string StoreLocation { get; set; }
        public string TenantId { get; set; }
    }

    public static class KeyVaultSettingsHelper
    {
        public static KeyVaultSettings GetKeyVaultSettings()
        {
            var value = ConfigurationManager.AppSettings["KeyVaultSettings"];

            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<KeyVaultSettings>(value);
            }
            catch
            {
                return null;
            }
        }
    }
}
