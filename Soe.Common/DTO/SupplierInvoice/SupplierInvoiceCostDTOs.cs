using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ProjectCentralSupplierInvoiceCostRowDTO
    {
        public int InvoiceId { get; set; }
        public int ProjectId { get; set; }
        public decimal? Amount { get; set; }
    }

    public class SupplierInvoiceCostOverviewDTO
    {
        public int SupplierInvoiceId { get; set; }
        public string SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string InternalText { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public decimal VATAmountCurrency { get; set; }
        public decimal DiffAmount { get; set; }
        public decimal DiffPercent { get; set; }
        public decimal? ProjectAmount { get; set; }
        public decimal? OrderAmount { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string OrderNr { get; set; }
        public string AttestGroupName { get; set; }

        public List<int> OrderIds { get; set; }
        public List<int> ProjectIds { get; set; }
    }
}
