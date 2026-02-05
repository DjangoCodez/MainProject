using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class OpeningHoursMock
    {
        public static List<OpeningHoursDTO> GetOpeningHours(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetOpeningHours();
                default:
                    return GetOpeningHours();
            }
        }

        private static List<OpeningHoursDTO> GetOpeningHours()
        {

            var listOfOpeningHoursDTOs = new List<OpeningHoursDTO>
            {
            };

            return listOfOpeningHoursDTOs;
        }
    }
}
