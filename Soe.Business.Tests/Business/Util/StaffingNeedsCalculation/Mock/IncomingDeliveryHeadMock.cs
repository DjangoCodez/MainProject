using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class IncomingDeliveryHeadMock
    {
        public static List<IncomingDeliveryHeadDTO> GetIncomingDeliveryHeads(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetIncomingDeliveryHeads();
                default:
                    return GetIncomingDeliveryHeads();
            }
        }
        private static List<IncomingDeliveryHeadDTO> GetIncomingDeliveryHeads()
        {
            return new List<IncomingDeliveryHeadDTO>();
        }
    }
}
