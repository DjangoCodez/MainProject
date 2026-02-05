using System;

namespace SoftOne.Soe.Common.DTO
{
    public class ProjectInvoiceDayInvoiceRowMappingDTO
    {
        public Guid ProjectInvoiceWeekTempId { get; set; }
        public Guid ProjectInvoiceDayTempId { get; set; }
        public int InvoiceRowTempId { get; set; }
        public int PreviousRowTempId { get; set; }
        public int QuantityDifference { get; set; }
    }
}
