using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeSalaryUnionFeesItem
    {
        public int TransactionId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public string UnionName { get; set; }
        public string PayrollPriceTypeIdPercentName { get; set; }
        public string PayrollPriceTypeIdPercentCeilingName { get; set; }
        public string PayrollPriceTypeIdFixedAmountName { get; set; }
        public int? UnionFeeId { get; set; }
        public bool ManualAdded { get; set; }
        public bool CentRounding { get; set; }
    }
}