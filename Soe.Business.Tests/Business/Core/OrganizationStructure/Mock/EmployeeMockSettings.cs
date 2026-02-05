using System;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class EmployeeMockSettings
    {
        public int NbrOfActiveEmployees { get; private set; }
        public int NbrOfEndedEmployees { get; private set; }
        public int NbrOfFutureEmployees { get; private set; }
        public int NbrOfEmployeesPerAccount { get; private set; }
        public int NbrOfEmployeeAccountsPerEmployee { get; private set; }
        public bool SetFirstEmployeeToCurrentUser { get; set; }
        public EAccountStructureStartLevelOption StartLevelOption { get; set; }
        public EAccountStructureSubLevelOption SubLevelOption { get; set; }

        public EmployeeMockSettings(
            int nbrOfActiveEmployees = 5,
            int nbrOfEndedEmployees = 0,
            int nbrOfFutureEmployees = 0,
            int nbrOfEmployeesPerAccount = 1,
            int nbrOfEmployeeAccountsPerEmployee = 1,
            bool setFirstEmployeeToCurrentUser = true,
            EAccountStructureStartLevelOption startLevelOption = EAccountStructureStartLevelOption.Standard,
            EAccountStructureSubLevelOption subLevelOption = EAccountStructureSubLevelOption.NoSubLevel
        )
        {
            if (nbrOfActiveEmployees < 1)
                throw new ArgumentOutOfRangeException(nameof(nbrOfActiveEmployees), "Value must be greater than or equal to 1.");
            if (nbrOfEmployeeAccountsPerEmployee < 1 || nbrOfEmployeeAccountsPerEmployee > 3)
                throw new ArgumentOutOfRangeException(nameof(nbrOfEmployeeAccountsPerEmployee), "Value must be between 1 and 3.");

            this.NbrOfActiveEmployees = nbrOfActiveEmployees;
            this.NbrOfEndedEmployees = nbrOfEndedEmployees;
            this.NbrOfFutureEmployees = nbrOfFutureEmployees;
            this.NbrOfEmployeesPerAccount = nbrOfEmployeesPerAccount;
            this.NbrOfEmployeeAccountsPerEmployee = nbrOfEmployeeAccountsPerEmployee;
            this.SetFirstEmployeeToCurrentUser = setFirstEmployeeToCurrentUser;
            this.StartLevelOption = startLevelOption;
            this.SubLevelOption = subLevelOption;
        }
    }

}
