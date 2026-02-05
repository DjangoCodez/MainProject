
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Util.Communicator.Kivra
{
    public static class KivraUtil
    {
        public static CommunicatorMessage CreateMessage(Employee employee, string subject, byte[] pdf, string kivraTenentKey)
        {
            return CommunicatorMessageHelper.CreateKivraPayrollMessage(kivraTenentKey, StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec), employee.Name, subject, subject + " " + employee.Name + ".pdf", pdf);
        }
    }
}
