using SoftOne.Common.KeyVault.Models;
using SoftOne.Common.KeyVaultSecrets;
using System;
using System.Threading.Tasks;

namespace SoftOne.Common.KeyVault
{
    public static class KeyVaultSecretsFetcher
    {
        public static string GetSecret(string secretName)
        {
            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            if (vaultsettings != null)
            {
                return GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, secretName, vaultsettings.StoreLocation, vaultsettings.TenantId);
            }
            else
            {
                return null;
            }
        }

        public static string GetSecret(string authenticationCertificateName, string clientId, string keyVaultUrl, string secretName, string storeLocation, string tenantId)
        {
            return KeyVaultSecretsFetcherV2.GetSecret(authenticationCertificateName, clientId, keyVaultUrl, secretName, storeLocation, tenantId);
        }

        public static string GetSecret(KeyVaultSettings keyVaultSettings, string secretName, string storeLocation)
        {
            return GetSecret(keyVaultSettings, secretName, storeLocation, retry: 0);
        }

        public static string GetSecret(KeyVaultSettings keyVaultSettings, string secretName, string storeLocation, int retry = 0)
        {
            if (keyVaultSettings == null)
                throw new ArgumentNullException(nameof(keyVaultSettings), "Key vault settings cannot be null when fetching secret.");

            if (string.IsNullOrWhiteSpace(keyVaultSettings.ClientId))
                throw new ArgumentException("Client ID for vault access must be provided.", nameof(keyVaultSettings.ClientId));

            if (string.IsNullOrWhiteSpace(keyVaultSettings.KeyVaultUrl))
                throw new ArgumentException("URL to key vault must be provided.", nameof(keyVaultSettings.KeyVaultUrl));

            if (string.IsNullOrWhiteSpace(keyVaultSettings.CertificateDistinguishedName))
                throw new ArgumentException("The name of the certificate to be used for authentication must be provided.", nameof(keyVaultSettings.CertificateDistinguishedName));

            if (string.IsNullOrWhiteSpace(secretName))
                throw new ArgumentException("The name of the secret you wish to fetch must be provided.", nameof(secretName));

            if (string.IsNullOrWhiteSpace(keyVaultSettings.TenantId))
                throw new ArgumentException("Tenant Id for vault access must be provided.", nameof(keyVaultSettings.TenantId));

            try
            {
                return KeyVaultSecretsFetcherV2.GetSecret(keyVaultSettings.CertificateDistinguishedName, keyVaultSettings.ClientId, keyVaultSettings.KeyVaultUrl, secretName, storeLocation, keyVaultSettings.TenantId);
            }
            catch (Exception ex)
            {
                Task.Delay(1000);
                if (retry < 3)
                {
                    return GetSecret(keyVaultSettings, secretName, storeLocation, retry + 1);
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}