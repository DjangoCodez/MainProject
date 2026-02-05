using System;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class ProductUnitIODTO
    {
        public int ProductUnitId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ProductGroupIODTO
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class PaymentSearchIODTO
    {
        public int? InvoiceId { get; set; }
        public DateTime? PayDateFrom { get; set; }
        public DateTime? PayDateTo { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool IncludeAccountInformation { get; set; }
    }
}
