using System;
using System.Security.Cryptography.X509Certificates;

namespace SoftOne.Common.KeyVault.Helpers
{
    internal static class CertificateHelper
    {
        public static X509Certificate2 FindBySubjectDistinguishedName(string distinguishedName, string storeLocation)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
                throw new System.ArgumentException("Distinguished name must be provided when retrieving a certificate.", nameof(distinguishedName));

            StoreLocation location = StoreLocation.LocalMachine;

            if (storeLocation.Equals("CurrentUser", StringComparison.OrdinalIgnoreCase))
                location = StoreLocation.CurrentUser;

            if (storeLocation.Equals("LocalMachine", StringComparison.OrdinalIgnoreCase))
                location = StoreLocation.LocalMachine;


            using (var store = new X509Store(StoreName.My, location))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection col = store.Certificates.Find(
                    X509FindType.FindBySubjectDistinguishedName,
                    distinguishedName,
                    false); // Validate cert, turn off for testing purposes.

                if (col == null || col.Count == 0)
                {
                    X509Certificate2Collection col2 = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    "CN=" + distinguishedName,
                    false); // Validate cert, turn off for testing purposes

                    if (col2 == null || col2.Count == 0)
                    {
                        X509Certificate2Collection col3 = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        distinguishedName,
                        false); // Validate cert, turn off for testing purposes

                        if (col3 == null || col3.Count == 0)
                            return null;

                        return col3[0];
                    }

                    return col2[0];
                }

                return col[0];
            }
        }
    }
}
