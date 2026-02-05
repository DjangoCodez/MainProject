using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Business.Tests.Business.Core.OrganizationStructure;
using Soe.Business.Tests.Util;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class AccountStructureScenario : TestBase
    {
        private readonly ParameterObject parameterObject;
        private readonly EAccountStructureCustomerType customerType;
        private readonly AccountStructureAttestRoleType attestRoleType;
        private readonly bool useAllMyAccounts;

        private AccountStructureScenario(
            ParameterObject parameterObject,
            EAccountStructureCustomerType customerType,
            AccountStructureAttestRoleType attestRoleType,
            bool useAllMyAccounts
            )
        {
            this.parameterObject = parameterObject ?? throw new ArgumentNullException(nameof(parameterObject));
            this.customerType = customerType;
            this.attestRoleType = attestRoleType;
            this.useAllMyAccounts = useAllMyAccounts;
        }

        public static AccountStructureScenario CreateScenario(
            ParameterObject parameterObject,
            EAccountStructureCustomerType customerType,
            AccountStructureAttestRoleType attestRoleType,
            bool useAllMyAccounts
            )
        {
            return new AccountStructureScenario(parameterObject, customerType, attestRoleType, useAllMyAccounts);
        }

        public AccountStructureTestResult BuildAndValidateAccountStructure()
        {
            var testConfig = CreateConfig();
            Assert.IsNotNull(testConfig);

            var accountDims = MockFactory.MockAccountDimsWithAccounts(testConfig);
            AssertExt.IsNotNullOrEmpty(accountDims);

            var accountRepositorySettings = AccountStructureMockFactory.MockAccountRepositorySettings(testConfig, accountDims);
            AssertExt.IsNotNullOrEmpty(accountDims);

            var accountRepositoryContext = AccountStructureMockFactory.MockAccountRepositoryContext(accountRepositorySettings, accountDims);
            Assert.IsNotNull(accountRepositoryContext);

            var attestRoleUsers = AccountStructureMockFactory.MockAttestRoleUsers(testConfig, accountRepositoryContext);
            AssertExt.IsNotNullOrEmpty(attestRoleUsers);

            var allEmployees = AccountStructureMockFactory.MockEmployees(testConfig, accountRepositoryContext);
            AssertExt.IsNotNullOrEmpty(allEmployees);

            var userEmployee = allEmployees.Find(e => e.UserId == testConfig.ParameterObject.UserId);
            Assert.IsNotNull(userEmployee);

            var inputAccounts = attestRoleUsers.GetValidAccounts(
                accountRepositoryContext.InternalAccountDims,
                accountRepositoryContext.InternalAccounts,
                testConfig.DateRange.Start,
                testConfig.DateRange.Stop
                );
            AssertExt.IsNotNullOrEmpty(inputAccounts);

            if (!this.useAllMyAccounts)
                inputAccounts = inputAccounts.Take(1).ToList();

            var accountRepository = new AccountRepository(
                attestRoleUsers,
                accountRepositoryContext.InternalAccountDims,
                accountRepositoryContext.InternalAccounts,
                inputAccounts,
                accountRepositoryContext.AccountRepositorySettings
                );
            Assert.IsNotNull(accountRepository);

            var userPermittedAccounts = accountRepository.GetAccountsDict(includeVirtualParented: true);
            AssertExt.IsNotNullOrEmpty(userPermittedAccounts);

            var userPermittedEmployes = FilterUserPermittedEmployees(allEmployees, userEmployee, accountRepository, testConfig.DateRange);
            AssertExt.IsNotNullOrEmpty(userPermittedEmployes);

            return AccountStructureTestResult.Create(userPermittedAccounts.Values?.ToList(), userPermittedEmployes);
        }

        private AccountStructureTestConfiguration CreateConfig()
        {
            switch (this.customerType)
            {
                case EAccountStructureCustomerType.Mathem:
                    return CreateMathemConfig();
                case EAccountStructureCustomerType.Coop:
                    return GetCoopConfig();
                case EAccountStructureCustomerType.Axfood:
                case EAccountStructureCustomerType.MartinServera:
                    throw new NotImplementedException(nameof(this.customerType));
                default:
                    throw new NotImplementedException(nameof(this.customerType));
            }
        }

        private AccountStructureTestConfiguration CreateMathemConfig()
        {
            AccountingMockSettings accountingSettings = new AccountingMockSettings(
                accountDimNames: new List<string> { "Kostnadsställe", "Avdelning", "Område", "Grupp", "Passtyp" },
                accountDimNrWithoutAccounts: null,
                employeeAccountDimNr: 4,                    // Grupp
                selectorAccountDimNr: 4,                    // Grupp
                useLimitedEmployeeAccountDimLevels: true,   // 1 level
                useExtendedEmployeeAccountDimLevels: false  // Not 3 levels
            );

            EmployeeMockSettings employeeSettings = new EmployeeMockSettings(
                nbrOfActiveEmployees: 50,
                nbrOfEndedEmployees: 2,
                nbrOfFutureEmployees: 2,
                nbrOfEmployeesPerAccount: 5,
                nbrOfEmployeeAccountsPerEmployee: 1,
                startLevelOption: EAccountStructureStartLevelOption.Standard,
                subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
            );

            AttestRoleUserMockSettings attestRoleUserSettings;
            switch (this.attestRoleType)
            {
                case AccountStructureAttestRoleType.RegionManager:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                        nbrOfUserAttestRoles: 1,
                        nbrOfAccounts: 1,
                        doSetFirstAttestRoleAsShowAll: false,
                        startLevelOption: EAccountStructureStartLevelOption.Parent,
                        subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
                    );
                    break;
                case AccountStructureAttestRoleType.StoreManager:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                        nbrOfUserAttestRoles: 1,
                        nbrOfAccounts: 1,
                        doSetFirstAttestRoleAsShowAll: false,
                        startLevelOption: EAccountStructureStartLevelOption.Standard,
                        subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
                    );
                    break;
                case AccountStructureAttestRoleType.StoreManager_TwoAccounts:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                         nbrOfUserAttestRoles: 1,
                         nbrOfAccounts: 2,
                         doSetFirstAttestRoleAsShowAll: false,
                         startLevelOption: EAccountStructureStartLevelOption.Standard,
                         subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
                     );
                    break;
                default:
                    return null;
            }

            return AccountStructureTestConfiguration.Create(this.parameterObject, accountingSettings, attestRoleUserSettings, employeeSettings);
        }

        private AccountStructureTestConfiguration GetCoopConfig()
        {
            AccountingMockSettings accountingSettings = new AccountingMockSettings(
                accountDimNames: new List<string> { "Region", "Kostnadsställe", "Flöde", "Avdelning", "Passtyp" },
                accountDimNrWithoutAccounts: null,
                employeeAccountDimNr: 2,                    // Kostnadsställe
                selectorAccountDimNr: 3,                    // Flöde
                useLimitedEmployeeAccountDimLevels: false,  // Not 1 level
                useExtendedEmployeeAccountDimLevels: false  // Not 3 levels
            );
            EmployeeMockSettings employeeSettings = new EmployeeMockSettings(
                nbrOfActiveEmployees: 50,
                nbrOfEndedEmployees: 2,
                nbrOfFutureEmployees: 2,
                nbrOfEmployeesPerAccount: 5,
                nbrOfEmployeeAccountsPerEmployee: 1,
                startLevelOption: EAccountStructureStartLevelOption.Standard,
                subLevelOption: EAccountStructureSubLevelOption.OneChildLevel
            );

            AttestRoleUserMockSettings attestRoleUserSettings;
            switch (this.attestRoleType)
            {
                case AccountStructureAttestRoleType.RegionManager:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                        nbrOfUserAttestRoles: 1,
                        nbrOfAccounts: 1,
                        doSetFirstAttestRoleAsShowAll: false,
                        startLevelOption: EAccountStructureStartLevelOption.Parent,
                        subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
                    );
                    break;
                case AccountStructureAttestRoleType.StoreManager:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                        nbrOfUserAttestRoles: 1,
                        nbrOfAccounts: 1,
                        doSetFirstAttestRoleAsShowAll: false,
                        startLevelOption: EAccountStructureStartLevelOption.Standard,
                        subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
                    );
                    break;
                case AccountStructureAttestRoleType.StoreManager_TwoAccounts:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                         nbrOfUserAttestRoles: 1,
                         nbrOfAccounts: 2,
                         doSetFirstAttestRoleAsShowAll: false,
                         startLevelOption: EAccountStructureStartLevelOption.Standard,
                         subLevelOption: EAccountStructureSubLevelOption.NoSubLevel
                     );
                    break;
                case AccountStructureAttestRoleType.FlowManager:
                    attestRoleUserSettings = AttestRoleUserMockSettings.Create(
                         nbrOfUserAttestRoles: 1,
                         nbrOfAccounts: 1,
                         doSetFirstAttestRoleAsShowAll: false,
                         startLevelOption: EAccountStructureStartLevelOption.Parent,
                         subLevelOption: EAccountStructureSubLevelOption.OneChildLevel
                     );
                    break;
                default:
                    return null;
            }

            return AccountStructureTestConfiguration.Create(this.parameterObject, accountingSettings, attestRoleUserSettings, employeeSettings);
        }

        private List<Employee> FilterUserPermittedEmployees(List<Employee> allEmployees, Employee currentUserEmployee, AccountRepository accountRepository, DateRangeDTO dateRange)
        {
            var currentUserEmployeeAccounts = currentUserEmployee.EmployeeAccount.GetEmployeeAccounts(dateRange.Start, dateRange.Stop);

            var validEmployees = new List<Employee>();
            foreach (var employee in allEmployees)
            {
                var employeeAccountsForEmployee = employee.EmployeeAccount.GetEmployeeAccounts(dateRange.Start, dateRange.Stop);

                if (!accountRepository.IsUserPermittedToSeeEmployee(
                    employee.EmployeeId,
                    employeeAccountsForEmployee,
                    currentUserEmployee,
                    currentUserEmployeeAccounts,
                    dateRange.Start,
                    dateRange.Stop,
                    onlyDefaultAccounts: true
                    )
                )
                    continue; //Not permitted to see employee

                validEmployees.Add(employee);
            }
            return validEmployees;
        }
    }
}
