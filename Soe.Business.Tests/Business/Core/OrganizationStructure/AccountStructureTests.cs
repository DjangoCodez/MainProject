using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Soe.Business.Tests.Business.Core.OrganizationStructure;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.IO;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    [TestClass()]
    public class AccountStructureTests : TestBase
    {
        private const int LicenseId = 2;
        private const int ActorCompanyId = 2;
        private const int UserId = 1;
        private const int RoleId = 1;
        private const string _directory = @"C:\temp\TestResult";
        private const string _fileNamePrefix = "AccountStructureResult";
        private const string _system = "SOE";

        private static ParameterObject CreateParameterObject()
        {
            return ParameterObject.Create(
                user: new UserDTO
                {
                    UserId = UserId,
                },
                company: new CompanyDTO
                {
                    ActorCompanyId = ActorCompanyId,
                    LicenseId = LicenseId
                },
                roleId: RoleId,
                thread: "Soe.Business.Tests"
                );
        }

        [TestMethod()]
        public void AccountStructure_Mathem_RegionManager() =>
            RunTest(EAccountStructureCustomerType.Mathem, AccountStructureAttestRoleType.RegionManager);
        [TestMethod()]
        public void AccountStructure_Mathem_StoreManager() =>
            RunTest(EAccountStructureCustomerType.Mathem, AccountStructureAttestRoleType.StoreManager);
        [TestMethod()]
        public void AccountStructure_Mathem_StoreManager_CurrentAccount() =>
            RunTest(EAccountStructureCustomerType.Mathem, AccountStructureAttestRoleType.StoreManager, useAllMyAccounts: false);
        [TestMethod()]
        public void AccountStructure_Mathem_StoreManager_TwoAccounts() =>
            RunTest(EAccountStructureCustomerType.Mathem, AccountStructureAttestRoleType.StoreManager_TwoAccounts);

        [TestMethod()]
        public void AccountStructure_Coop_RegionManager() =>
            RunTest(EAccountStructureCustomerType.Coop, AccountStructureAttestRoleType.RegionManager);
        [TestMethod()]
        public void AccountStructure_Coop_StoreManager() =>
            RunTest(EAccountStructureCustomerType.Coop, AccountStructureAttestRoleType.StoreManager);
        [TestMethod()]
        public void AccountStructure_Coop_StoreManger_CurrentAccount() =>
            RunTest(EAccountStructureCustomerType.Coop, AccountStructureAttestRoleType.StoreManager, useAllMyAccounts: false);
        [TestMethod()]
        public void AccountStructure_Coop_StoreManager_TwoAccounts() =>
            RunTest(EAccountStructureCustomerType.Coop, AccountStructureAttestRoleType.StoreManager_TwoAccounts);
        [TestMethod()]
        public void AccountStructure_Coop_StoreManager_TwoAccounts_CurrentAccount() =>
            RunTest(EAccountStructureCustomerType.Coop, AccountStructureAttestRoleType.StoreManager_TwoAccounts, useAllMyAccounts: false);
        [TestMethod()]
        public void AccountStructure_Coop_FlowManager() =>
            RunTest(EAccountStructureCustomerType.Coop, AccountStructureAttestRoleType.FlowManager);

        private void RunTest(
            EAccountStructureCustomerType customerType,
            AccountStructureAttestRoleType attestRoleType,
            bool useAllMyAccounts = true
            )
        {
            var scenario = AccountStructureScenario.CreateScenario(CreateParameterObject(), customerType, attestRoleType, useAllMyAccounts);
            Assert.IsNotNull(scenario);

            var result = scenario.BuildAndValidateAccountStructure();
            Assert.IsNotNull(result.ValidAccounts);
            Assert.IsTrue(result.ValidAccounts.Any());
            Assert.IsNotNull(result.ValidEmployees);
            Assert.IsTrue(result.ValidEmployees.Any());

            CreateResultFile(customerType, attestRoleType, useAllMyAccounts, result);
        }

        private static void CreateResultFile(
            EAccountStructureCustomerType customerType,
            AccountStructureAttestRoleType attestRoleType,
            bool useAllMyAccounts,
            AccountStructureTestResult result
            )
        {
            var json = JsonConvert.SerializeObject(new
            {
                ValidAccounts = result.ValidAccounts
                    .Select(a => new { a.AccountDim?.AccountDimNr, a.AccountNr, a.Name })
                    .ToList(),
                ValidEmployees = result.ValidEmployees
                    .Select(e => new { e.EmployeeNr, e.Name })
                    .ToList()
            }, Formatting.Indented);

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            var filePath = Path.Combine(_directory, $"{_fileNamePrefix}_{customerType}_{attestRoleType}_{(useAllMyAccounts ? "AllAccounts" : "CurrentAccount")}_{_system}.json");
            File.WriteAllText(filePath, json);
        }
    }
}
