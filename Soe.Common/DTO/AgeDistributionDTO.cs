using System;

namespace SoftOne.Soe.Common.DTO
{
    public class AgeDistributionDTO
    {
        public int InvoiceId { get; set; }
        public int ActorId { get; set; }
        public string ActorNr { get; set; }
        public string ActorNrSort { get; set; }
        public string ActorName { get; set; }
        public int SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Amount1 { get; set; }
        public decimal Amount2 { get; set; }
        public decimal Amount3 { get; set; }
        public decimal Amount4 { get; set; }
        public decimal Amount5 { get; set; }
        public decimal Amount6 { get; set; }
        public decimal SumAmount { get; set; }

        public int RegistrationType { get; set; }
    }
}
