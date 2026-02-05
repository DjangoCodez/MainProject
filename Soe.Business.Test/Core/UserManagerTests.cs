using SoftOne.Soe.Business.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class UserManagerTests
    {
        [TestMethod()]
        public void GetUserInfoDTOTest()
        {
            UserManager userManager = new UserManager(null);
            var dto = userManager.GetUserInfoDTO(Guid.Parse("4A72ACB2-D23F-4578-A6CF-640996EE4091"), true);
            Assert.IsTrue(dto != null);
        }

        [TestMethod()]
        public void SaveMandatoryInformationFromUserInfoDTOTest()
        {
            UserManager userManager = new UserManager(null);
            var infoDTO = userManager.GetUserInfoDTO(Guid.Parse("5D4A888E-6678-4320-997B-5078C19E2B24"), true);
            foreach (var item in infoDTO.MissingMandatoryInformation)
            {
                item.Value = item.Type;
            }
            var result = userManager.SaveMandatoryInformationFromUserInfoDTO(infoDTO);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetUserForMobileLoginTest()
        {
            UserManager userManager = new UserManager(null);
            var test = userManager.GetUserForMobileLogin(Guid.Parse("5D4A888E-6678-4320-997B-5078C19E2B24"));
            Assert.IsTrue(test != null);
        }
    }
}