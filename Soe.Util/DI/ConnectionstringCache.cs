using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SoftOne.Common.KeyVault.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace SoftOne.Soe.Util.DI
{

    public class ConnectionStringCache : IConnectionStringCache
    {
        private IDictionary<string, IConnectionStringProvider> _connectionStrings = new Dictionary<string, IConnectionStringProvider>();

        public string GetConnectionString(string name)
        {
            if (!_connectionStrings.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", "Could not find a ConnectionString with the given name");

            return _connectionStrings[name].GetConnectionString();
        }

        /// <summary>
        /// Registers a connection string from ConfigurationManager.ConnectionStrings
        /// </summary>
        /// <param name="name"></param>
        public ConnectionStringCache RegisterConnectionString(string name)
        {
            _connectionStrings.Add(name, new ConfigurationConnectionStringProvider(name));
            return this;
        }

        public ConnectionStringCache RegisterConnectionStringsInConfig()
        {
            for (int i=0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                RegisterConnectionString(ConfigurationManager.ConnectionStrings[i].Name, ConfigurationManager.ConnectionStrings[i].ConnectionString);
            }
            return this;
        }

        public ConnectionStringCache RegisterConnectionString(string name, string connectionString)
        {
            _connectionStrings.Add(name, new StringConnectionStringProvider(connectionString));
            return this;
        }

        public ConnectionStringCache RegisterConnectionStringProvider(string name, IConnectionStringProvider connectionstringProvider) {
            _connectionStrings.Add(name, connectionstringProvider);
            return this;
        }

        private class StringConnectionStringProvider : IConnectionStringProvider
        {
            private readonly string _connectionString;

            public StringConnectionStringProvider(string connectionString)
            {
                this._connectionString = connectionString;
            }

            public string GetConnectionString()
            {
                return _connectionString;
            }
        }

        private class ConfigurationConnectionStringProvider : IConnectionStringProvider
        {
            private string _connstring;

            public ConfigurationConnectionStringProvider(string connectionStringName)
            {
                if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
                {
                    throw new ArgumentOutOfRangeException("ConnectionStringName", "Could not find a ConnectionString with the given name");
                }

                _connstring = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            }

            public string GetConnectionString()
            {
                return _connstring;
            }
        }
    }

    public interface IConnectionStringProvider
    {
        string GetConnectionString();
    }

    public class KeyVaultConnectionStringProviderFactory
    {
        private readonly string _keyVaultUrl;
        private readonly ClientAssertionCertificate _keyvaultCert;

        public KeyVaultConnectionStringProviderFactory(string keyVaultUrl, string clientId, string certificateName, string storeLocation)
        {
            if (string.IsNullOrWhiteSpace(keyVaultUrl))
                throw new ArgumentException("URL to key vault must be provided.", nameof(keyVaultUrl));

            if (string.IsNullOrWhiteSpace(certificateName))
                throw new ArgumentException("The name of the certificate to be used for authentication must be provided.", nameof(certificateName));

            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client id for vault access must be provided.", nameof(clientId));

            if (string.IsNullOrWhiteSpace(storeLocation))
                throw new ArgumentException("Store location for vault access must be provided.", nameof(storeLocation));

            var clientAssertionCertPfx = CertificateHelper.FindBySubjectDistinguishedName(certificateName, storeLocation);
            _keyvaultCert = new ClientAssertionCertificate(clientId, clientAssertionCertPfx);
            _keyVaultUrl = keyVaultUrl;
        }

        public IConnectionStringProvider GetProvider(string secretName)
        {
            return new KeyVaultConnectionStringProvider(_keyVaultUrl, _keyvaultCert, secretName);
        }

        private class KeyVaultConnectionStringProvider : IConnectionStringProvider
        {
            private readonly string _keyVaultUrl;
            private readonly ClientAssertionCertificate _assertionCertificate;
            private readonly string _secretName;

            public KeyVaultConnectionStringProvider(string keyVaultUrl, ClientAssertionCertificate assertionCertificate, string secretName)
            {
                this._keyVaultUrl = keyVaultUrl;
                this._assertionCertificate = assertionCertificate;
                this._secretName = secretName;
            }

            private async Task<string> GetAccessToken(string authority, string resource, string scope)
            {
                var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
                var result = await context.AcquireTokenAsync(resource, _assertionCertificate);
                return result.AccessToken;
            }

            public string GetConnectionString()
            {
                using (var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(KeyVaultAuthenticationHelper.GetAccessToken)))
                {
                    return SyncAsync(() => client.GetSecretAsync(_keyVaultUrl, _secretName)).Value;
                }
            }

            private T SyncAsync<T>(Func<Task<T>> workload)
            {
                Task<T> task = Task.Run(workload);
                task.Wait();
                return task.Result;
            }
        }
    }
}
