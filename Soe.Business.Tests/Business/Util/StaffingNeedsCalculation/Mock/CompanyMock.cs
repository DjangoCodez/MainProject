using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class CompanyMock
    {
        public static CompanyDTO GetCompany(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetCompany();
                default:
                    return GetCompany();
            }
        }
        private static CompanyDTO GetCompany()
        {
            var companyDTO = new CompanyDTO
            {
                ActorCompanyId = 451,
                Language = TermGroup_Languages.Swedish,
                Name = "Dotter AB",
                ShortName = "Elia",
                OrgNr = "5560640120",
                VatNr = "SE5560640120",
                AllowSupportLogin = true,
                AllowSupportLoginTo = DateTime.ParseExact("2022-12-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Template = false,
                Global = false,
                LicenseId = 14,
                LicenseNr = "9005",
                LicenseSupport = false,
                SysCountryId = 1,
                TimeSpotId = null
            };

            return companyDTO;
        }
    }
}
