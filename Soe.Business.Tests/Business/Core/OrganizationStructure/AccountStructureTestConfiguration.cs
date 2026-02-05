using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class AccountStructureTestConfiguration
    {
        public ParameterObject ParameterObject { get; private set; }
        public DateRangeDTO DateRange { get; private set; }
        public AccountingMockSettings AccountingMockSettings { get; private set; }
        public AttestRoleUserMockSettings AttestRoleUserMockSettings { get; private set; }
        public EmployeeMockSettings EmployeeMockSettings { get; private set; }

        private AccountStructureTestConfiguration() { }

        public static AccountStructureTestConfiguration Create(
            ParameterObject parameterObject,
            AccountingMockSettings accountingSettings,
            AttestRoleUserMockSettings attestRoleUserSettings,
            EmployeeMockSettings employeeSettings
            )
        {
            return new AccountStructureTestConfiguration
            {
                ParameterObject = parameterObject,
                DateRange = new DateRangeDTO(CalendarUtility.GetBeginningOfYear(), CalendarUtility.GetEndOfYear()),
                AccountingMockSettings = accountingSettings,
                AttestRoleUserMockSettings = attestRoleUserSettings,
                EmployeeMockSettings = employeeSettings
            };
        }
    }
}
