using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Tests;
using SoftOne.Soe.Common.DTO;
using System;

namespace SoftOne.Soe.Business.Util.Tests
{
    [TestClass()]
    public class ConnectUtilTests : TestBase
    {
        [TestMethod()]
        [Ignore] // Not used atm
        public void GenerateTokenTest()
        {
            //ConnectUtil c = new ConnectUtil(null);
            //int id = 1223456;
            //var st = c.GenerateToken("token", ref id);
            //c.ValidateToken("token", st, ref id);
            //Assert.Fail();
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void GetTokenTest()
        {
            Z.EntityFramework.Extensions.LicenseManager.AddLicense("836;101-SoftOne", "808e16a2-f300-1dd0-5be5-bc37afb71327");
            LoginDTO loginDTO = new LoginDTO()
            {
                License = "8000",
                UserName = "apitest@8000",
                Password = "Sommar2019"

            };
            ConnectUtil c = new ConnectUtil(null);
            c.GetToken(Guid.Parse("5ed49f81-881b-4c8c-a14d-90ebeef70a79"), Guid.Parse("2EBF8ABC-8542-AB30-84DA-94CED7B8E4E0"), loginDTO);

            ApiManager apiManager = new ApiManager(null);
            apiManager.ValidateToken(Guid.Parse("ee9c2f7c-fc1e-4ec7-bb91-e066215452d5"), Guid.Parse("2EBF8ABC-8542-AB30-84DA-94CED7B8E4E0"), loginDTO.Token, out string validation);

            Assert.Fail();
        }

        [TestMethod()]
        public void GetTokenTestLicense()
        {
            LoginDTO loginDTO = new LoginDTO()
            {
                License = "8000",
                UserName = "Apitest@8000",
                Password = "Sommar2019"
            };

            ConnectUtil c = new ConnectUtil(null);
            var apikey = Guid.Parse(ConnectUtil.AXFOODLICENSEAPIKEY);
            var connectApiKey = Guid.Parse("2EBF8ABC-8542-AB30-84DA-94CED7B8E4E0");
            loginDTO = c.GetToken(apikey, connectApiKey, loginDTO);

            apikey = Guid.Parse("9241f643-b8ba-427e-84e6-42e2e6d468e0");
            ApiManager apiManager = new ApiManager(null);
            apiManager.ValidateToken(apikey, connectApiKey, loginDTO.Token, out string validation);

            Assert.Fail();
        }

        [TestMethod()]
        public void GetTokenTestLicenseHRM()
        {
            ConfigurationSetupUtil.Init(); 

            LoginDTO loginDTO = new LoginDTO()
            {
                License = "101",
                UserName = "apiTest",
                Password = "chimeraTestarApi3781#"
            };

            ConnectUtil c = new ConnectUtil(null);
            var apikey = Guid.Parse("2514f1b4-4a3f-46a9-aa43-f1664b205f1c");
            var connectApiKey = Guid.Parse("e8d7bf57-fd1b-44a6-8468-9cfce813f783");
            loginDTO = c.GetToken(apikey, connectApiKey, loginDTO);            
            Assert.IsNotNull(loginDTO.Token);
        }

        [TestMethod()]
        public void ValidateTokenTest()
        {
            var connectApiKey = Guid.Parse("2EBF8ABC-8542-AB30-84DA-94CED7B8E4E0");
            var apikey = Guid.Parse("9241f643-b8ba-427e-84e6-42e2e6d468e0");
            LoginDTO dto = new LoginDTO();
            dto.Token = "SDRzSUFBYXhqMXdBL3czUFI0S3FNQUFBMEFPeGtDQkZGbjlCUjNwSHN3UEN5Q0FTbFZDUzA4OS9OM2hyY0p4VmlUT2JIeTh3YzBRR1ZIdHl5ZXN4OThaODRCWGJtOHNjS3VCTUNMZEdlWkF4Smxha044dm11M2xSKzUzOWZxNE5IZndXRlVnTExLY1Q5UEx0V3loRXdpVFBpSGMyZE1WejYra1kzQTJKNUJGUHpPL1BjRnI5RXNKMnVZYkRBbVdKc3EvSWVEQi9WNXBSbXdNYUlhUFU2WkczUlpnbE5kTE1FV0FObmNwTFZiYnFnd0JUd1JtdDVZNUY4S2xFei9tRG00VHJ1K1VUVkRlWUd6Szl4RGZEdFY0NXRhRlhaUHpaWG5ZVnY0NWNiU1g5ODZGcjNORzlEM0FxT3ZuWVo4MWw5RWpZU2g4MTI0ZWduMkNvaG5iZlNKSG1ESlgwbWxiRlZsVEJQZmxWa2h5cnpDQlgwUmo0YVdkVzNLNk5RWHRub1hDUkZXaWRrU0F5b1JDRldWQnkxeFpEQ0pMZ3VFR3ZZNXpVUUpqS3lYemJyN3JHL2dlTWhYRnM4YVpmcDkxNXA0N1Q5R1dWdXo0djhqZkV3Q29icXY2NnhuZ2ZqK0xFRStaMkorTTVVVzB1Si9lcWJCYjIrdUdSYzFKeHpKT3EveDdBUzhGd0xoOU4yZmFNUFFhUjNBbHdOUGtGOVhjdGhmLytBRk9rSHluc0FRQUE=";
            ApiManager apiManager = new ApiManager(null);
            var result = apiManager.ValidateToken(apikey, connectApiKey, dto.Token, out _);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void ValidateTokenTest2()
        {
            var connectApiKey = Guid.Parse("2EBF8ABC-8542-AB30-84DA-94CED7B8E4E0");
            var apikey = Guid.Parse("9241f643-b8ba-427e-84e6-42e2e6d468e0");
            LoginDTO dto = new LoginDTO();
            dto.Token = "SDRzSUFPOWV6VjBBL3dYQlNhSkRNQUFBMEFOWkZLVmlHYUlJRVdxb2I0ZlNtdWRTcC8vdnhmZklDWTlMOVpoMlZCTmZNTVJDNENMRDlyR3VlODU5KzJhQmg3Tk1SdjIyS2lia3JWUGh5WWlVSHZESkZMM0JpL3Rsc3RUWi9iU29LQkhwWndtanRtWDlrVlZkVXJIQitsSEFST3JtTWN5RmcrWXhFaDNPOGdRTVNpWXRwT2RwRmh4L3UxVXR3UFhjL0VXcFc2Qy94bVhxc2RuV0QxdGRRUjk4NDQ4Z3A2MWJDdjM1ZmlVeVZPblF3U0pZU2RLV3p2d0xWN0hUTlQ2a1J1NzYvZ2xZajluNjNpZUZBSUpwWHQyZ0V1NUt4UnR3RlNDUER6WlJxSWttSUxWYURmV25qTXpTNXBYdncwcHZBcDQ2VWNNZmlLNmEwV1hHM2hXTXNLZnFhV1Y2Ym5KdHNsL1U5M1VKeEx3dUlUMnE1Q1NjZEZscnVnR3hYZko0WVdmR3dkc3pQZGhLcnR2ZWxFSitISWlsaW5GM3RhaDU1STI1eWtqcm5WRDAzV3NUUDlTSytJTnJPTisrUEdjR1J2WmVTK0ptZ1NkSmdYNG5ha2gvMmtCcnIzMmRET3ZldENIQ2w0bkQrUjVsU1cvSGl5M3IvcS9ya2orNmZBdjZPa05ZaithRzZPK2RDS0lOMlh1R3VWc1JIcU1xVzlTVHJORjBUNFNMaW15OHJzU01IdjhEU1lhZTlRQUNBQUE9";
            ApiManager apiManager = new ApiManager(null);
            var result = apiManager.ValidateToken(apikey, connectApiKey, dto.Token, out _);
            Assert.IsTrue(result);
        }
    }
}