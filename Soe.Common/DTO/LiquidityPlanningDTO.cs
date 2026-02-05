using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class LiquidityPlanningDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public SoeOriginType OriginType { get; set; }
        public int? LiquidityPlanningTransactionId { get; set; }
        public DateTime Date { get; set; }
        public LiquidityPlanningTransactionType TransactionType { get; set; }
        public string TransactionTypeName { get; set; }
        public string Specification { get; set; }
        public decimal ValueIn { get; set; }
        public decimal ValueOut { get; set; }
        public decimal Total { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
}
