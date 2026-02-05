using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class PaymentInformationIODTO
    {
        public int invoiceId { get; set; }
        public bool FullyPaid { get; set; }
        public decimal PaidAmount { get; set; }
        public List<PaymentRowImportIODTO> PaymentRowImportIODTOs { get; set; }
    }
}
