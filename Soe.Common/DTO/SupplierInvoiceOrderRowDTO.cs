using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierInvoiceOrderRowDTO
    {
        //General
        public bool IsReadOnly { get; set; }
        public bool IsModified { get; set; }
        public SoeEntityState State { get; set; }

        public int CustomerInvoiceRowId { get; set; }

        public int SupplierInvoiceId { get; set; }

        public int InvoiceProductId { get; set; }
        public string InvoiceProductName { get; set; }

        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool IncludeSupplierInvoiceImage { get; set; }

        //Project
        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }

        //Customer invoice
        public int CustomerInvoiceId { get; set; }
        public string CustomerInvoiceNr { get; set; }
        public string CustomerInvoiceCustomerName { get; set; }
        public string CustomerInvoiceNumberName { get; set; }
        public string CustomerInvoiceDescription { get; set; }


        public decimal? SupplementCharge { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }

        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal SumAmountLedgerCurrency { get; set; }
        public decimal SumAmountEntCurrency { get; set; }
    }

    public static class SupplierInvoiceOrderRowDTOExtensions
    {
        public static SupplierInvoiceOrderRowDTO ToSupplierInvoiceOrderRow(this SupplierInvoiceCostAllocationDTO row, (int ProductId, string ProductName, int SupplierInvoceId) fallbacks)
        {
            return new SupplierInvoiceOrderRowDTO()
            {
                State = row.State,
                InvoiceProductId = row.ProductId.HasValue && row.ProductId != 0 ? row.ProductId.Value : fallbacks.ProductId,
                InvoiceProductName = row.ProductId.HasValue && row.ProductId != 0 ? row.ProductName : fallbacks.ProductName,
                SupplierInvoiceId = row.SupplierInvoiceId > 0 ? row.SupplierInvoiceId : fallbacks.SupplierInvoceId,
                Amount = row.RowAmount,
                AmountCurrency = row.RowAmountCurrency,
                SumAmount = row.OrderAmount,
                SumAmountCurrency = row.OrderAmountCurrency,
                SupplementCharge = row.SupplementCharge,
                CustomerInvoiceId = row.OrderId,
                CustomerInvoiceRowId = row.CustomerInvoiceRowId,
                IncludeSupplierInvoiceImage = row.IncludeSupplierInvoiceImage,
                ProjectId = row.ProjectId,
            };
        }
    }
}
