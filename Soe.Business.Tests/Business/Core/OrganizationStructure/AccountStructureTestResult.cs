using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.Core.OrganizationStructure
{
    public class AccountStructureTestResult
    {
        private readonly List<AccountDTO> validAccounts;
        public IReadOnlyList<AccountDTO> ValidAccounts => validAccounts;

        private readonly List<Employee> validEmployees;
        public IReadOnlyList<Employee> ValidEmployees => validEmployees;

        private AccountStructureTestResult(List<AccountDTO> validAccounts, List<Employee> validEmployees)
        {
            this.validAccounts = validAccounts;
            this.validEmployees = validEmployees;
        }

        public static AccountStructureTestResult Create(List<AccountDTO> validAccounts, List<Employee> validEmployees)
        {
            return new AccountStructureTestResult(validAccounts, validEmployees);
        }
    }
}
