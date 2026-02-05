using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class InvoicePaymentMatchAndVoucherDTO
    {
        public VoucherHeadDTO VoucherHead { get; set; }
        public List<AccountingRowDTO> AccoutningsRows { get; set; }
        public List<InvoiceMatchingDTO> Matchings { get; set; }
        public int MatchCodeId { get; set; }
    }
}
