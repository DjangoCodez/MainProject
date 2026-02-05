using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public static class MockFactory
    {
        public static List<AccountDim> MockAccountDimsWithAccounts(AccountStructureTestConfiguration config)
        {
            return new[]
            {
                MockAccountDimStd(config.ParameterObject)
            }
            .Concat(MockAccountDimInternals(config.ParameterObject, config.AccountingMockSettings))
            .ToList();
        }

        public static AccountDim MockAccountDimStd(ParameterObject parameterObject)
        {
            var accountDimStd = MockAccountDim(parameterObject, Constants.ACCOUNTDIM_STANDARD, "Kontoplan", "Standard");
            accountDimStd.Account.AddRange(new List<Account>()
            {
                MockAccount(parameterObject, accountDimStd, "1910", "Kassa", "Kassa och bank"),
                MockAccount(parameterObject, accountDimStd, "1920", "Plusgiro", "Plusgirokonto"),
                MockAccount(parameterObject, accountDimStd, "1930", "Bank", "Bankkonto"),
                MockAccount(parameterObject, accountDimStd, "1940", "Företagskonto", "Företagskonto"),
                MockAccount(parameterObject, accountDimStd, "2010", "Eget kapital", "Eget kapital"),
                MockAccount(parameterObject, accountDimStd, "2610", "Utgående moms", "Utgående moms"),
                MockAccount(parameterObject, accountDimStd, "3010", "Försäljning", "Försäljning av varor"),
                MockAccount(parameterObject, accountDimStd, "4010", "Inköp", "Inköp av varor"),
                MockAccount(parameterObject, accountDimStd, "5010", "Lokalhyra", "Lokalhyra"),
                MockAccount(parameterObject, accountDimStd, "7010", "Löner", "Löner till anställda")
            });
            return accountDimStd;
        }

        public static List<AccountDim> MockAccountDimInternals(
            ParameterObject parameterObject,
            AccountingMockSettings settings
            )
        {
            var accountDims = new List<AccountDim>();

            // Internal (AccountDimNr != ACCOUNTDIM_STANDARD)
            int startAccountDimNr = Constants.ACCOUNTDIM_STANDARD + 1;
            int stopAccountDimNr = startAccountDimNr + settings.AccountDimNames.Count;

            AccountDim prevAccountDim = null;
            int accountId = 1;

            for (int dimIdx = 0, dimId = startAccountDimNr; dimId < stopAccountDimNr; dimId++, dimIdx++)
            {
                var accountDim = MockAccountDim(
                    parameterObject,
                    dimNr: dimId,
                    name: settings.AccountDimNames[dimIdx],
                    shortName: $"Dim-{dimId} (InternalDim-{dimId - 1})",
                    parentAccountDim: prevAccountDim,
                    id: dimId
                    );

                int nbrOfAccounts = dimId == startAccountDimNr ? 1 : (int)Math.Pow(4, dimId - startAccountDimNr); // 1, 4, 16, 64, ...
                for (int accIdx = 0; accIdx < nbrOfAccounts; accIdx++)
                {
                    var account = MockAccount(
                        parameterObject,
                        accountDim,
                        accountNr: $"{dimId}00{accountId}",
                        name: $"{accountDim.Name}-{accountId}",
                        id: accountId
                        );

                    if (dimId == startAccountDimNr)
                        account.ParentAccountId = null;
                    else if (dimId == settings.AccountDimNrWithoutAccounts)
                        account.ParentAccountId = null;
                    else if (prevAccountDim?.Account != null && prevAccountDim.Account.Count > accIdx / 4)
                        account.ParentAccountId = (prevAccountDim.Account.ToList()[accIdx / 4].AccountId);

                    accountDim.Account.Add(account);
                    accountId++;
                }

                accountDims.Add(accountDim);
                prevAccountDim = accountDim;
            }


            return accountDims;
        }

        public static AccountDim MockAccountDim(
            ParameterObject parameterObject,
            int dimNr,
            string name,
            string shortName,
            AccountDim parentAccountDim = null,
            int? id = null
            )
        {
            var accountDim = new AccountDim
            {
                ActorCompanyId = parameterObject.ActorCompanyId,
                AccountDimNr = dimNr,
                Name = name,
                ShortName = shortName,
                Parent = parentAccountDim
            };
            accountDim.MockId(nameof(AccountDim.AccountDimId), id);
            return accountDim;
        }

        public static Account MockAccount(
            ParameterObject parameterObject,
            AccountDim accountDim,
            string accountNr,
            string name,
            string description = null,
            int? id = null
            )
        {
            Account account = new Account
            {
                ActorCompanyId = parameterObject.ActorCompanyId,
                AccountDim = accountDim,
                AccountDimId = accountDim.AccountDimId,
                AccountNr = accountNr,
                Name = name,
                Description = description
            };
            account.MockId(nameof(Account.AccountId), id);
            return account;
        }

        public static AttestRole MockAttestRole(
            ParameterObject parameterObject,
            SoeModule module,
            string name,
            string description = null,
            bool doShowAll = false
            )
        {
            var attestRole = new AttestRole
            {
                ActorCompanyId = parameterObject.ActorCompanyId,
                Module = (int)module,
                Name = name,
                Description = description,
                ShowAllCategories = doShowAll,
            };
            attestRole.MockId(nameof(AttestRole.AttestRoleId));
            return attestRole;
        }

        public static AttestRoleUser MockAttestRoleUser(
            ParameterObject parameterObject,
            AttestRole attestRole,
            DateRangeDTO dateRange,
            AccountDTO account,
            AttestRoleUser parent = null,
            params AttestRoleUser[] children
            )
        {
            var attestRoleUser = new AttestRoleUser
            {
                AttestRole = attestRole,
                UserId = parameterObject.UserId,
                AccountId = account?.AccountId,
                DateFrom = dateRange.Start,
                DateTo = dateRange.Stop,
            };

            if (parent != null)
                attestRoleUser.SetParent(parent);
            else if (!children.IsNullOrEmpty())
                attestRoleUser.AddChildren(children);
            attestRoleUser.MockId(nameof(AttestRoleUser.AttestRoleUserId));

            return attestRoleUser;
        }

        public static Employee MockEmployee(
            ParameterObject parameterObject,
            int? userId,
            int employeeId,
            string firstName,
            string lastName,
            DateRangeDTO dateRange,
            IEnumerable<EmployeeAccountNode> employeeAccountNodes
        )
        {
            string ssn = DataGenerationHelper.GenerateSwedishSSN(out var sex);

            var actor = new Actor()
            {
                ActorType = (int)SoeActorType.ContactPerson,
            };
            actor.MockId(nameof(Actor.ActorId));

            var contactPerson = new ContactPerson
            {
                ActorContactPersonId = actor.ActorId,
                FirstName = firstName,
                LastName = lastName,
                SocialSec = ssn,
                Sex = (int)sex
            };

            var employee = new Employee
            {
                UserId = userId,
                EmployeeNr = employeeId.ToString(),
                ContactPerson = contactPerson
            };
            employee.MockId(nameof(Employee.EmployeeId), employeeId);

            int baseWorkTimeWeek = DataGenerationHelper.GenerateRandomInt(10, maxExclusive: 41);
            int workTimeWeek = DataGenerationHelper.GenerateRandomInt(10, maxExclusive: baseWorkTimeWeek + 1);

            var employment = new Employment
            {
                Employee = employee,
                DateFrom = dateRange.Start,
                DateTo = dateRange.Stop,
                OriginalEmployeeGroupId = 1,
                OriginalPayrollGroupId = null,
                OriginalBaseWorkTimeWeek = baseWorkTimeWeek,
                OriginalWorkTimeWeek = workTimeWeek
            };
            employee.Employment.Add(employment);

            foreach (var employeeAccountNode in employeeAccountNodes)
            {
                var employeeAccount = new EmployeeAccount
                {
                    ActorCompanyId = parameterObject.ActorCompanyId,
                    EmployeeId = employee.EmployeeId,
                    AccountId = employeeAccountNode.AccountId,
                    DateFrom = dateRange.Start,
                    DateTo = dateRange.Stop,
                    Default = employeeAccountNode.IsDefault,
                    MainAllocation = employeeAccountNode.IsMainAllocation,
                    AddedOtherEmployeeAccount = false

                };
                employee.EmployeeAccount.Add(employeeAccount);
            }

            return employee;
        }

        public static void MockId<T>(this T obj, string idPropertyName, int? id = null)
            where T : EntityObject
        {
            var prop = typeof(T).GetProperty(idPropertyName) ?? throw new ArgumentException($"Property '{idPropertyName}' not found on type '{typeof(T).Name}'.");

            var value = prop.GetValue(obj);
            if (value is int intValue && intValue == 0)
                prop.SetValue(obj, id ?? new Random().Next(1, 1000));
        }
    }
}


