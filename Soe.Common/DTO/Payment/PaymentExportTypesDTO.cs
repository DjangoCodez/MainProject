using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    public class PaymentExportTypesDTO
    {
        public TermGroup_SysPaymentMethod PaymentMethod { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_SysPaymentType SysPaymentType { get; set; }
    }
}
