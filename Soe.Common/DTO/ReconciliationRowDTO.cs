using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ReconciliationRowDTO
    {
        public int Type { get; set; }
        public int ActorCompanyId { get; set; }
        public int AccountId { get; set; }
        public int AssociatedId { get; set; }
        public int RowStatus { get; set; }
        public int OriginType { get; set; }
        public int VoucherSeriesId { get; set; }

        public bool ShowInfo { get; set; }

        public string Account { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string TypeName { get; set; }
        public string VoucherSeriesTypeName { get; set; }

        public decimal CustomerAmount { get; set; }
        public decimal SupplierAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal LedgerAmount { get; set; }
        public decimal DiffAmount { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime Date { get; set; }
    }
}
