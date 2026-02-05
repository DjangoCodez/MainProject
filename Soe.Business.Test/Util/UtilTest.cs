using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace Soe.Business.Test.Util
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<int> list = new List<int>();
            list.Add(7);
            list.Add(28);
            list.Add(63);

            int lcm = NumberUtility.LestCommonMultiple(list.ToArray());
            Assert.IsTrue(lcm > 0);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var settings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            string secret = KeyVaultSecretsFetcher.GetSecret("cn=SoftOneKeyVault", "a6abfa56-d362-4ac5-84fc-484fb9b207dc", "https://softone.vault.azure.net/", "SoftOneId-InternalIdP-ClientId", "localmachine", "24b68b1d-c072-4e48-908e-2afc34b7b1ca");
            string secret2 = KeyVaultSecretsFetcher.GetSecret("cn=SoftOneOnlineTest", "33555af2-20b6-478f-ad71-f79453ade1b8", "https://softoneonlinetest.vault.azure.net/", "SoftOneId-InternalIdP-ClientId", "", "24b68b1d-c072-4e48-908e-2afc34b7b1ca");
            Assert.IsTrue(settings != null && secret != null && secret2 != null);
        }

        [TestMethod]
        public void TestMethod_NewKeyFetcher()
        {
            var settings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            string secret = KeyVaultSecretsFetcher.GetSecret(settings.CertificateDistinguishedName, settings.ClientId, settings.KeyVaultUrl, "SoftOneId-InternalIdP-ClientId", settings.StoreLocation, settings.TenantId);
            Assert.IsTrue(settings != null && secret != null);
        }

        [TestMethod]
        public void TestCleanPhone()
        {
            var phone = "?#012 33-4324";
            var value = StringUtility.CleanPhoneNumber(phone);
            Assert.IsTrue(value != null);
        }

        [TestMethod]
        public void GetNumberEtc()
        {
            var text = "1234567890";
            var value1 = text.SafeSubstring(1,2);
            var value2 = text.SafeSubstring(0, 2);
            var value3 = text.SafeSubstring(0, 9);
            var value4 = text.SafeSubstring(0, 15);
            var value5 = text.SafeSubstring(15, 1);
            Assert.IsTrue(value1 != null && value2 != null && value3 != null && value4 != null && value5 != null);
        }
    }
}
