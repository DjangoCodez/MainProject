using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.AltInn;
using SoftOne.Soe.Common.DTO;
using System.IO;
using System.Linq;

namespace Soe.Business.Test
{
    [TestClass]
    public class AltInnTests
    {
        [TestMethod]
        public void TestAuthenticationChallengeTest()
        {
            var altInn = new AltInn();
            var user = new AltInnUser()
            {
                UserSSN = "24076800983",
                EndUserSystemPassword = "SoftOne123",
                LogInMethod = AltInnAuthMethods.SMSPin,
                OrginizationNumber = "910232592",
                EndUserSystemId = "2984",
            };
            var response = altInn.GetAuthenticationChallenge(user);
            Assert.IsTrue(response.GetAuthenticationChallengeResult.Status == SoftOne.Soe.Business.Altinn.SystemAuthentication.ChallengeRequestResult.Ok, response.GetAuthenticationChallengeResult.Message);

            // Pin code comes from mobile
            string pinCode = string.Empty;
            var response2 = altInn.GetPrefillData(user, user.OrginizationNumber);

            Assert.IsTrue(response2.Body.GetPrefillDataBasicResult.Status == SoftOne.Soe.Business.Altinn.PreFillEUSExternalBasic.PrefillStatus.PREFILLDATAFOUND);
        }

        [TestMethod]
        [Ignore]
        public void GetServicesTest()
        {
            var altInn = new AltInn();
            var response = altInn.GetAvailableServices(new AltInnUser());
            Assert.IsTrue(response.GetAvailableServicesBasicResult.Count() > 0);

            var stream = File.Open("GetServices.txt", FileMode.OpenOrCreate);
            TextWriter writer = new StreamWriter(stream);

            foreach (var item in response.GetAvailableServicesBasicResult)
            {
                writer.WriteLine(item.ServiceName + " " + item.ExternalServiceCode + " " + item.ExternalServiceEditionCode);
            }

            writer.Flush();
            writer.Close();
        }

        [TestMethod]
        [Ignore]
        public void GetSchemaDefinitionTest()
        {
            var altInn = new AltInn();
            var response = altInn.GetFormTaskSchemaDefinitions(new AltInnUser());

            var stream = File.Open("GetFormTaskSchemaDefinitions.txt", FileMode.OpenOrCreate);
            TextWriter writer = new StreamWriter(stream);

            foreach (var item in response.GetFormTaskSchemaDefinitionsBasicResult)
            {
                writer.WriteLine(item.DataFormatXsd);
            }

            writer.Flush();
            writer.Close();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SerializeToXML()
        {
            RF002_VatDeclaration vat = new RF002_VatDeclaration();
            Assert.IsTrue(true);
        }
    }
}