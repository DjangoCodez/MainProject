using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeFixedPayLinesItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public int? BirthYear { get; set; }
        public string Position { get; set; }
        public string SSYKCode { get; set; }
        public string EmploymentTypeName { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        public string Payrollgroup { get; set; }
        public string ProductNr { get; set; }
        public string ProuctName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public bool Distribute { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VatAmount { get; set; }
        public bool FromPayrollGroup { get; set; }
        public decimal Amount { get; set; }
    }
}
