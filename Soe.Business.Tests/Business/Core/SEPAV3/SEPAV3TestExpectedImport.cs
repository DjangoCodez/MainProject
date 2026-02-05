namespace SoftOne.Soe.Business.Core.Tests
{
    public class SEPAV3TestExpectedImport
    {
        public int Status { get; set; }
        public int State { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int? CustomerId { get; set; }
        public string Customer { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
    }
}
