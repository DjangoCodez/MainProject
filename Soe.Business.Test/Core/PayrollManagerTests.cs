using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class PayrollManagerTests :TestBase
    {
        [TestMethod()]
        public void SaveTimeRegistrationInformationTest()
        {
            ConfigurationSetupUtil.Init();
            SysServiceManager ssm = new SysServiceManager(null);

            PayrollManager pm = new PayrollManager(GetParameterObject(291, 193));
            TimeRegistrationInformation information = new TimeRegistrationInformation()
            {
                EmployeeNr = "10",
                Code = "ÖT2",
                Date = new DateTime(2021, 8, 1),
                Comment = "Testar öt 2121",
                Minutes = 120,
            };

            var result = pm.SaveTimeRegistrationInformation(new List<TimeRegistrationInformation>() { information }, 291, true);
            Assert.IsTrue(result.Success);
        }
    }
}