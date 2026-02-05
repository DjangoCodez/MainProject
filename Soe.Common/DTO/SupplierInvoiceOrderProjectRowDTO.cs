using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class SupplierInvoiceCostTransferForGridDTO
    {
        public SupplierInvoiceCostLinkType Type;
        public int RecordId;
        public string OrderNumber;
        public string OrderName;
        public string ProjectNumber;
        public string ProjectName;
        public string TimeCodeName;
        public decimal Amount;
        public decimal SupplementCharge;
        public decimal SumAmount;
    }
    public class SupplierInvoiceCostTransferDTO
    {
        //General
        public SupplierInvoiceCostLinkType Type { get; set; }
        public int RecordId { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsModified { get; set; }
        public SoeEntityState State { get; set; }

        public int CustomerInvoiceRowId { get; set; }

        public int SupplierInvoiceId { get; set; }

        public int InvoiceProductId { get; set; }

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


        public decimal SupplementCharge { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }

        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal SumAmountLedgerCurrency { get; set; }
        public decimal SumAmountEntCurrency { get; set; }

        //TimeCodeTransaction
        public int TimeCodeTransactionId { get; set; }
        //TimeInvoiceTransaction
        public int TimeInvoiceTransactionId { get; set; }
        //Order
        public List<int> OrderIds { get; set; }
        public string OrderNr { get; set; }
        //TimeCode
        public int TimeCodeId { get; set; }
        public string TimeCodeCode { get; set; }
        public string TimeCodeName { get; set; }
        public string TimeCodeDescription { get; set; }
        //Employee
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDescription { get; set; }
        //TimeBlockDate
        public int? TimeBlockDateId { get; set; }
        public DateTime? Date { get; set; }

        public bool ChargeCostToProject { get; set; }
    }
}
