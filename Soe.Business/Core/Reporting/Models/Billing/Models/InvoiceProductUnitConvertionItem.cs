using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models
{
    public class InvoiceProductUnitConvertItem
    {
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductUnitName { get; set; }
        public string ProductConvertUnitName { get; set; }
        public decimal? ConvertFactor { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
    }
}
