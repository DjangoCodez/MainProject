using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class AccountRepositoryContext
    {
        public AccountRepositorySettings AccountRepositorySettings { get; private set; }
        public List<AccountDimDTO> InternalAccountDims { get; private set; }
        public AccountDimDTO EmployeeAccountDim { get; private set; }
        public List<AccountDTO> InternalAccounts { get; private set; }

        private AccountRepositoryContext(
            AccountRepositorySettings accountRepositorySettings,
            List<AccountDim> accountDims
        )
        {
            this.AccountRepositorySettings = accountRepositorySettings;
            this.InternalAccountDims = accountDims
                .GetInternals()
                .ToDTOs(includeAccounts: true);
            this.InternalAccounts = accountDims
                .GetInternals()
                .SelectMany(ad => ad.Account?.ToList() ?? new List<Account>())
                .ToDTOs(includeAccountDim: true);
            this.EmployeeAccountDim = (accountRepositorySettings.EmployeeAccountDimId.HasValue
                ? this.InternalAccountDims.Find(ad => ad.AccountDimId == accountRepositorySettings.EmployeeAccountDimId.Value)
                : null)
                ?? throw new ArgumentException("Cannot find EmployeeAccountDim in mocked AccountDims");
        }

        public static AccountRepositoryContext Create(
            AccountRepositorySettings accountRepositorySettings,
            List<AccountDim> accountDims
        )
        {
            return new AccountRepositoryContext(
                accountRepositorySettings,
                accountDims
            );
        }
    }
}
