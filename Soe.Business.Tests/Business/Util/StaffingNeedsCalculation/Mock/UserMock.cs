using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class UserMock
    {
        public static UserDTO GetUser(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetUser();
                default:
                    return GetUser();
            }
        }

        public static UserDTO GetUser()
        {
            return new UserDTO
            {
                LicenseId = 14,
                LicenseNr = "",
                DefaultActorCompanyId = 451,
                DefaultRoleId = 108,
                UserId = 322,
                LoginName = "sys",
                Name = "Admin",
                Email = null,
                State = SoeEntityState.Active,
                ChangePassword = true,
                LangId = 1,
                BlockedFromDate = null,
                EstatusLoginId = null,
                IsAdmin = true,
                IsSuperAdmin = false,
                IsMobileUser = true,
                idLoginGuid = new Guid("4423fc4f-dbdf-4238-900f-0bd31a62ee5f"),
                HasUserVerifiedEmail = false
            };
        }
    }
}
