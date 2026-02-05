using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeSalaryDistressItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string SSN { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public bool ManualAdded { get; set; }
        public decimal ReservedAmounts { get; set; }
        public string CaseNumber { get; set; }
        public string SeizureAmountType { get; set; }
        public decimal SalaryDistressAmount { get; set; }
        public string Absence { get; set; }
    }
}