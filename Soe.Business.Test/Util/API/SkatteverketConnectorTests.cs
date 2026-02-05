using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.API.OAuth;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SoftOne.Soe.Business.Util.API.Tests
{
    [TestClass()]
    public class SkatteverketConnectorTests
    {
        [TestMethod()]
        public void GetCsrReponseTest()
        {
            var csrResponse = SkatteverketConnector.GetCsrReponse("165781006753", "196605253225", 2018);
            string[] arr1 = new string[] { "196605253225", "197605159404", "198504292387" };
            var csrResponses = SkatteverketConnector.GetCsrReponses("165781006753", arr1.ToList(), 2018);
            Assert.IsTrue(csrResponse != null && csrResponses != null);
        }

        [TestMethod()]
        public void GetSkatteAvdragTest()
        {
            var csrResponse = SkatteverketConnector.GetSkatteAvdrag("165781006753", "196605253225", 2019);
            SkatteAvdragFleraPersoner request = new SkatteAvdragFleraPersoner();
            request.personnummer = new List<string> { "196605253225", "197605159404", "198504292387" };
            var csrResponses = SkatteverketConnector.GetFleraSkatteavdrag("165781006753", request, 2019);
            Assert.IsTrue(csrResponse != null && csrResponses != null);
        }

        [TestMethod()]
        public void GetFOS()
        {
            var fos = SkatteverketConnector.GetFOS("165781006753", "196207243046", 2024);
        }

        [TestMethod()]
        public void GetMultipleFOS()
        {
            SkatteAvdragFleraPersoner request = new SkatteAvdragFleraPersoner();
            request.personnummer = new List<string> { "196605253225", "197605159404", "198504292387" };
            var fos = SkatteverketConnector.GetFleraFOS("165781006753", request, 2024);
        }

        [TestMethod()]
        public void GetTokenTest2()
        {
            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", "fos" },
                { "client_id", "bee10941de6997363c9b5ea46e820023e9bab0dd4914d05c" },
                { "client_secret", "078f946aea27f28d80870791da7c0023e9bab0dd4914d05c" }
            };

            HttpClient httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://sysoauth2.test.skatteverket.se/oauth2/v1/sys/token");
            request.Content = new FormUrlEncodedContent(body);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded;charset=UTF-8");
            var response = httpClient.SendAsync(request).Result;
        }
   }
}