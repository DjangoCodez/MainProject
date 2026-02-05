using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class EmployeeManagerTests : TestBase
    {
        [TestMethod()]
        public void GetEmployeeOrganisationInformationsTest()
        {
            EmployeeManager ex = new EmployeeManager(GetParameterObject(2101239, 56633));
            ConfigurationSetupUtil.Init();

            var result = ex.GetEmployeeOrganisationInformations(2101239, DateTime.Today, DateTime.Today);
            Assert.IsTrue(result != null);
        }


        [TestMethod()]
        public void TestPostEmployeeChanges()
        {
            var apim = new ApiManager((GetParameterObject(864, 107, 84))); // Admin M&S, company 50002
            ConfigurationSetupUtil.Init();
            string rawJson = @"
                [
                  {
                    ""EmployeeNr"": ""50012"",
                    ""EmployeeChangeRowIOs"": [
                      {
                        ""EmployeeChangeType"": ""FirstName"",
                        ""Value"": ""Test""
                      },
                      {
                        ""EmployeeChangeType"": ""LastName"",
                        ""Value"": ""User ID""
                      },
                      {
                        ""EmployeeChangeType"": ""Email"",
                        ""Value"": ""ValidForGoTest.testing@martinservera.se""
                      },
                      {
                        ""EmployeeChangeType"": ""ExternalAuthId"",
                        ""Value"": ""ValidForGoTest.testing@martinservera.se""
                      }
                    ]
                  }
                ]
            ";

            var ioDtos = JsonConvert.DeserializeObject<List<EmployeeChangeIODTO>>(rawJson);
            var apiMessageDto = new ApiMessageDTO(SoeEntityType.Employee, TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.API);

            apim.ImportEmployeeChangesExtensive(apiMessageDto, ioDtos, out ActionResult result);
            Assert.IsTrue(result.Success);
        }
    }
}