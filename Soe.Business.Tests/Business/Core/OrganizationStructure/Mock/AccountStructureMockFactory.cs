using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public static partial class AccountStructureMockFactory
    {
        public static AccountRepositorySettings MockAccountRepositorySettings(
            AccountStructureTestConfiguration testConfig,
            IEnumerable<AccountDim> accountDims
            )
        {
            return new AccountRepositorySettings(
                selectorAccountDimId: accountDims.GetByNr(testConfig.AccountingMockSettings.SelectorAccountDimNr)?.AccountDimId,
                employeeAccountDimId: accountDims.GetByNr(testConfig.AccountingMockSettings.EmployeeAccountDimNr)?.AccountDimId,
                useLimitedEmployeeAccountDimLevels: false,
                useExtendedEmployeeAccountDimLevels: false,
                includeOnlyChildrenOneLevel: false
            );
        }
        public static AccountRepositoryContext MockAccountRepositoryContext(
            AccountRepositorySettings accountRepositorySettings,
            List<AccountDim> accountDims
            )
        {
            return AccountRepositoryContext.Create(
                accountRepositorySettings,
                accountDims
                );
        }

        public static List<AttestRoleUser> MockAttestRoleUsers(
            AccountStructureTestConfiguration testConfig,
            AccountRepositoryContext accountRepositoryContext
            )
        {
            if (accountRepositoryContext == null)
                throw new ArgumentNullException(nameof(accountRepositoryContext), $"{nameof(accountRepositoryContext)} cannot be null.");
            if (accountRepositoryContext.EmployeeAccountDim == null)
                throw new ArgumentNullException(nameof(accountRepositoryContext), $"{nameof(accountRepositoryContext.EmployeeAccountDim.Accounts)} cannot be null.");

            if (testConfig.AttestRoleUserMockSettings.NbrOfUserAttestRoles < 1)
                throw new ArgumentException("NbrOfUserAttestRoles must be at least 1.");

            var accountDim = GetStartAccountDim(testConfig.AttestRoleUserMockSettings.StartLevelOption, accountRepositoryContext.EmployeeAccountDim, accountRepositoryContext.InternalAccountDims) ??
                throw new ArgumentException("Cannot determine AccountDim for AttestRoleUser mocking.");

            if (accountDim.Accounts == null || accountDim.Accounts.Count < testConfig.AttestRoleUserMockSettings.NbrOfAccounts)
                throw new ArgumentException("Not enough accounts in AccountDim for AttestRoleUser");

            var attestRoleUsers = new List<AttestRoleUser>();

            for (int attestRoleIdx = 1; attestRoleIdx <= testConfig.AttestRoleUserMockSettings.NbrOfUserAttestRoles; attestRoleIdx++)
            {
                var attestRole = MockFactory.MockAttestRole(
                    testConfig.ParameterObject,
                    SoeModule.Time,
                    name: $"AttestRole-{accountDim.Name}-{attestRoleIdx}",
                    description: $"AttestRole-{accountDim.Name}-{attestRoleIdx}",
                    doShowAll: testConfig.AttestRoleUserMockSettings.DoSetFirstAttestRoleAsShowAll && attestRoleIdx == 1
                );

                for (int accountIdx = 0; accountIdx < testConfig.AttestRoleUserMockSettings.NbrOfAccounts; accountIdx++)
                {
                    var account = GetAccountForDim(accountDim, accountIdx);
                    var attestRoleUser = AddAttestRoleUser(attestRole, account, parent: null);

                    switch (testConfig.AttestRoleUserMockSettings.SubLevelOption)
                    {
                        case EAccountStructureSubLevelOption.OneParentLevel:
                            var parentDim = GetParentDim(accountDim, accountRepositoryContext.InternalAccountDims);
                            if (parentDim != null)
                            {
                                var parentDimAccount = GetAccountForDim(parentDim, accountIdx);
                                AddAttestRoleUser(attestRole, parentDimAccount, parent: null, children: attestRoleUser);
                            }
                            break;
                        case EAccountStructureSubLevelOption.OneChildLevel:
                        case EAccountStructureSubLevelOption.TwoChildLevels:
                            var childDim = GetChildDim(accountDim, accountRepositoryContext.InternalAccountDims);
                            if (childDim != null)
                            {
                                var childAccount = GetAccountForDim(childDim, accountIdx);
                                var childAttestRoleUser = AddAttestRoleUser(attestRole, childAccount, parent: attestRoleUser);

                                if (testConfig.AttestRoleUserMockSettings.SubLevelOption == EAccountStructureSubLevelOption.TwoChildLevels)
                                {
                                    var grandChildDim = GetChildDim(childDim, accountRepositoryContext.InternalAccountDims);
                                    if (grandChildDim != null)
                                    {
                                        var grandChildAccount = GetAccountForDim(grandChildDim, accountIdx);
                                        AddAttestRoleUser(attestRole, grandChildAccount, parent: childAttestRoleUser);
                                    }
                                }
                            }
                            break;

                    }
                }
            }

            AccountDTO GetAccountForDim(AccountDimDTO dim, int accountIdx)
            {
                if (dim?.Accounts == null || dim.Accounts.IsNullOrEmpty())
                    throw new ArgumentException("Cannot determine Account for AttestRoleUser");
                return dim.Accounts.ElementAt(accountIdx % dim.Accounts.Count);
            }
            AttestRoleUser AddAttestRoleUser(AttestRole attestRole, AccountDTO account, AttestRoleUser parent, params AttestRoleUser[] children)
            {
                if (account == null)
                    throw new ArgumentException("Account cannot be null when adding AttestRoleUser.");

                var attestRoleUser = MockFactory.MockAttestRoleUser(
                    testConfig.ParameterObject,
                    attestRole,
                    testConfig.DateRange,
                    account,
                    parent,
                    children
                );
                attestRoleUsers.Add(attestRoleUser);
                return attestRoleUser;
            }

            return attestRoleUsers;
        }

        public static List<Employee> MockEmployees(
            AccountStructureTestConfiguration testConfig,
            AccountRepositoryContext accountRepositoryContext
            )
        {
             if (testConfig.EmployeeMockSettings.NbrOfActiveEmployees < 1)
                throw new ArgumentException("NbrOfActiveEmployees must be at least 1.");

            if (accountRepositoryContext.InternalAccountDims.Any(ad => ad.ActorCompanyId != testConfig.ParameterObject.ActorCompanyId) || accountRepositoryContext.InternalAccountDims.Any(ad => ad.Accounts.IsNullOrEmpty()))
                throw new ArgumentException("All AccountDims must belong to the same CompanyId and have Accounts.");

            var employees = new List<Employee>();

            for (int employeeIdx = 1; employeeIdx <= testConfig.EmployeeMockSettings.NbrOfActiveEmployees; employeeIdx++)
            {
                var employeeAccountNodes = MockEmployeeAccountNodes(
                    employeeIdx,
                    testConfig,
                    accountRepositoryContext
                );

                var employee = MockFactory.MockEmployee(
                    testConfig.ParameterObject,
                    userId: testConfig.EmployeeMockSettings.SetFirstEmployeeToCurrentUser && employeeIdx == 1 ? testConfig.ParameterObject.UserId : (int?)null,
                    employeeId: employeeIdx,
                    firstName: "Employee",
                    lastName: employeeIdx.ToString(),
                    dateRange: testConfig.DateRange,
                    employeeAccountNodes: employeeAccountNodes
                );
                employees.Add(employee);
            }
            return employees;
        }

        public static IEnumerable<EmployeeAccountNode> MockEmployeeAccountNodes(
            int employeeIdx,
            AccountStructureTestConfiguration testConfig,
            AccountRepositoryContext accountRepositoryContext
            )
        {
            if (testConfig.EmployeeMockSettings.NbrOfEmployeeAccountsPerEmployee < 1)
                throw new ArgumentException("NbrOfEmployeeAccountsPerEmployee must be at least 1.");

            var accountDim = GetStartAccountDim(testConfig.EmployeeMockSettings.StartLevelOption, accountRepositoryContext.EmployeeAccountDim, accountRepositoryContext.InternalAccountDims)
                ?? throw new ArgumentException("Cannot determine AccountDim for Employee mocking.");

            if (accountDim.Accounts == null || accountDim.Accounts.Count < testConfig.EmployeeMockSettings.NbrOfEmployeeAccountsPerEmployee)
                throw new ArgumentException("Not enough accounts in AccountDim for EmployeeAccount");

            var accountNodes = new List<EmployeeAccountNode>();

            for (int employeeAccountIdx = 1; employeeAccountIdx <= testConfig.EmployeeMockSettings.NbrOfEmployeeAccountsPerEmployee; employeeAccountIdx++)
            {
                var account = GetAccountForDim();
                var employeeAccount = AddEmployeeAccount(account, parent: null);

                switch (testConfig.EmployeeMockSettings.SubLevelOption)
                {
                    case EAccountStructureSubLevelOption.OneParentLevel:
                        var parentDim = GetParentDim(accountDim, accountRepositoryContext.InternalAccountDims);
                        if (parentDim != null)
                        {
                            var parentDimAccount = GetAccountForDim();
                            AddEmployeeAccount(parentDimAccount, parent: null, children: employeeAccount);
                        }
                        break;
                    case EAccountStructureSubLevelOption.OneChildLevel:
                    case EAccountStructureSubLevelOption.TwoChildLevels:
                        var childDim = GetChildDim(accountDim, accountRepositoryContext.InternalAccountDims);
                        if (childDim != null)
                        {
                            var childAccount = GetAccountForDim();
                            var childEmployeeAccount = AddEmployeeAccount(childAccount, parent: employeeAccount);

                            if (testConfig.EmployeeMockSettings.SubLevelOption == EAccountStructureSubLevelOption.TwoChildLevels)
                            {
                                var grandChildDim = GetChildDim(childDim, accountRepositoryContext.InternalAccountDims);
                                if (grandChildDim != null)
                                {
                                    var grandChildAccount = GetAccountForDim();
                                    AddEmployeeAccount(grandChildAccount, parent: childEmployeeAccount);
                                }
                            }
                        }
                        break;
                }
            }

            AccountDTO GetAccountForDim()
            {
                int accountIdx = ((employeeIdx - 1) / testConfig.EmployeeMockSettings.NbrOfEmployeesPerAccount) % (accountDim?.Accounts?.Count ?? 1);
                return accountDim?.Accounts?.ElementAt(accountIdx) ??
                    throw new InvalidOperationException("Account not found for current AccountDim.");
            }
            EmployeeAccountNode AddEmployeeAccount(AccountDTO account, EmployeeAccountNode parent, params EmployeeAccountNode[] children)
            {
                var employeeAccount = EmployeeAccountNode.Create(
                    account,
                    testConfig.DateRange,
                    isDefault: true,
                    isMainAllocation: true,
                    parent: parent,
                    children: children
                );
                accountNodes.Add(employeeAccount);
                return employeeAccount;
            }

            return accountNodes;
        }

        private static AccountDimDTO GetStartAccountDim(
            EAccountStructureStartLevelOption startLevelOption,
            AccountDimDTO standardAccountDim,
            List<AccountDimDTO> accountDims
            )
        {
            AccountDimDTO accountDim = null;
            switch (startLevelOption)
            {
                case EAccountStructureStartLevelOption.Standard:
                    accountDim = standardAccountDim;
                    break;
                case EAccountStructureStartLevelOption.Parent:
                    accountDim = GetParentDim(standardAccountDim, accountDims);
                    break;
                case EAccountStructureStartLevelOption.Child:
                    accountDim = GetChildDim(standardAccountDim, accountDims);
                    break;
            }

            return accountDim;
        }

        private static AccountDimDTO GetChildDim(AccountDimDTO dim, IEnumerable<AccountDimDTO> accountDims)
        {
            return dim == null ? null : accountDims.FirstOrDefault(ad => ad.ParentAccountDimId == dim.AccountDimId);
        }

        private static AccountDimDTO GetParentDim(AccountDimDTO dim, IEnumerable<AccountDimDTO> accountDims)
        {
            return dim == null ? null : accountDims.FirstOrDefault(ad => ad.AccountDimId == dim.ParentAccountDimId);
        }
    }
}
