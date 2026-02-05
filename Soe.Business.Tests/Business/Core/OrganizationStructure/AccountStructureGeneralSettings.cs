using SoftOne.Soe.Common.DTO;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class AccountStructureGeneralSettings
    {
        public DateRangeDTO DateRange { get; private set; }

        public AccountStructureGeneralSettings(DateRangeDTO dateRange)
        {
            this.DateRange = dateRange;
        }
    }
}
