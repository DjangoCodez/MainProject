using System;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class AccountingMockSettings
    {
        public List<string> AccountDimNames { get; private set; } = new List<string>();
        public int? AccountDimNrWithoutAccounts { get; private set; }
        public int EmployeeAccountDimNr { get; private set; }
        public int SelectorAccountDimNr { get; private set; }
        public bool UseLimitedEmployeeAccountDimLevels { get; private set; }
        public bool UseExtendedEmployeeAccountDimLevels { get; private set; }

        public AccountingMockSettings(
            List<string> accountDimNames,
            int? accountDimNrWithoutAccounts,
            int employeeAccountDimNr,
            int selectorAccountDimNr,
            bool useLimitedEmployeeAccountDimLevels = false,
            bool useExtendedEmployeeAccountDimLevels = false
            )
        {
            this.AccountDimNames = accountDimNames ?? throw new ArgumentNullException(nameof(accountDimNames));
            if (this.AccountDimNames.Count < 1 || this.AccountDimNames.Count > 5)
                throw new ArgumentException("AccountDimNames must contain between 1 and 5 names.", nameof(accountDimNames));

            // Handle that standard dim has 1, so all internal dims actually have + 1
            this.AccountDimNrWithoutAccounts = accountDimNrWithoutAccounts.HasValue ? accountDimNrWithoutAccounts.Value + 1 : (int?)null;
            this.EmployeeAccountDimNr = employeeAccountDimNr + 1;
            this.SelectorAccountDimNr = selectorAccountDimNr + 1;
            this.UseLimitedEmployeeAccountDimLevels = useLimitedEmployeeAccountDimLevels;
            this.UseExtendedEmployeeAccountDimLevels = useExtendedEmployeeAccountDimLevels;
        }
    }
}
