using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierInvoiceProjectRowDTO
    {
        //General
        public SoeEntityState State { get; set; }
        //TimeCodeTransaction
        public int TimeCodeTransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        //TimeInvoiceTransaction
        public int TimeInvoiceTransactionId { get; set; }
        //SupplierInvoice
        public int SupplierInvoiceId { get; set; }
        //Project
        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        //Order
        public List<int> OrderIds { get; set; }
        public string OrderNr { get; set; }
        //TimeCode
        public int TimeCodeId { get; set; }
        public string TimeCodeCode { get; set; }
        public string TimeCodeName { get; set; }
        public string TimeCodeDescription { get; set; }
        //Employee
        public int? EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDescription { get; set; }
        //TimeBlockDate
        public int? TimeBlockDateId { get; set; }
        public DateTime? Date { get; set; }

        public bool ChargeCostToProject { get; set; }
        public bool IncludeSupplierInvoiceImage { get; set; }

        //Customer invoice - ONLY USED TO KEEP NAME IN GRID BEFORE SAVE
        public int CustomerInvoiceId { get; set; }
        public string CustomerInvoiceNr { get; set; }
        public string CustomerInvoiceCustomerName { get; set; }
        public string CustomerInvoiceNumberName { get; set; }
        public string CustomerInvoiceDescription { get; set; }

        public static decimal CalculateProjectRowsTotalAmount(List<SupplierInvoiceProjectRowDTO> projectRows)
        {
            decimal rowsTotalAmount = Decimal.Zero;
            if (projectRows != null)
                rowsTotalAmount = projectRows.Where(i => i.State == (int)SoeEntityState.Active).Sum(i => i.AmountCurrency);
            return Math.Round(rowsTotalAmount, 2);
        }

        public static bool IsAmountValid(int vatType, decimal totalAmountCurrency, decimal vatAmountCurrency, List<SupplierInvoiceProjectRowDTO> projectRows)
        {
            if (vatType == (int)TermGroup_InvoiceVatType.Contractor || vatType == (int)TermGroup_InvoiceVatType.NoVat)
                vatAmountCurrency = 0;

            return IsAmountValid(totalAmountCurrency, vatAmountCurrency, CalculateProjectRowsTotalAmount(projectRows));
        }

        public static bool IsAmountValid(decimal totalAmountCurrency, decimal vatAmountCurrency, decimal rowsTotalAmount)
        {
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
