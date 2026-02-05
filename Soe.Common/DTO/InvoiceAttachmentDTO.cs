using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class InvoiceAttachmentDTO
    {
        public int InvoiceAttachmentId { get; set; }
        public int InvoiceId { get; set; }
        public int? DataStorageRecordId { get; set; }
        public int? EdiEntryId { get; set; }
        public bool AddAttachmentsOnTransfer { get; set; }
        public bool AddAttachmentsOnEInvoice { get; set; }
        public DateTime? LastDistributedDate { get; set; }
    }
}
