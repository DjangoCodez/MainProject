using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierInvoiceCostAllocationDTO
    {
        public int CustomerInvoiceRowId { get; set; }
        public int TimeCodeTransactionId { get; set; }
        public int SupplierInvoiceId { get; set; }
        public int ProjectId { get; set; }
        public int OrderId { get; set; }
        public int AttestStateId { get; set; }
        public int? TimeInvoiceTransactionId { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal ProjectAmount { get; set; }
        public decimal ProjectAmountCurrency { get; set; }
        public decimal RowAmount { get; set; }
        public decimal RowAmountCurrency { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal OrderAmountCurrency { get; set; }
        public decimal? SupplementCharge { get; set; }

        public bool ChargeCostToProject { get; set; }
        public bool IncludeSupplierInvoiceImage { get; set; }
        public bool IsReadOnly { get; set; }

        public int? ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }

        public int TimeCodeId { get; set; }
        public string TimeCodeCode { get; set; }
        public string TimeCodeName { get; set; }
        public string TimeCodeDescription { get; set; }

        public int? EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDescription { get; set; }

        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }

        public string OrderNr { get; set; }
        public string CustomerInvoiceNumberName { get; set; }

        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        public SoeEntityState State { get; set; }

        public bool IsTransferToOrderRow { get; set; }
        public bool IsConnectToProjectRow { get; set; }
    }

    public static class SupplierInvoiceCostAllocationHelper
    {
        public static bool AmountsAreValid(decimal totalAmountCurrency, decimal vatAmountCurrency, List<SupplierInvoiceCostAllocationDTO> rows)
        {
            var rowsTotalAmount = rows.Sum(r =>
            {
                if (r.State != SoeEntityState.Active)
                    return 0;
                if (r.IsConnectToProjectRow && r.ChargeCostToProject)
                    return r.ProjectAmountCurrency;
                if (r.IsTransferToOrderRow)
                    return r.RowAmountCurrency;
                return 0;
            });

            if (rowsTotalAmount == 0)
                return true;

            //Amounts passed as parameters have negative values if credit, project rows has always positive values
            if (totalAmountCurrency < 0)
            {
                totalAmountCurrency = Decimal.Negate(totalAmountCurrency);
                vatAmountCurrency = Decimal.Negate(vatAmountCurrency);
            }

            //Amount for project rows cannot exceed total amount for invoice
            return (totalAmountCurrency - vatAmountCurrency) >= rowsTotalAmount;
        }
    }
}
