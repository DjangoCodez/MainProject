using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeTimePeriodItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string SocialSec { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime PayrollStartDate { get; set; }
        public DateTime PayrollStopDate { get; set; }

        public decimal Tax { get; set; }
        public decimal TableTax { get; set; }
        public decimal OneTimeTax { get; set; }
        public decimal EmploymentTaxCredit { get; set; }
        public decimal SupplementChargeCredit { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal VacationCompensation { get; set; }
        public decimal Benefit { get; set; }
        public decimal Compensation { get; set; }
        public decimal Deduction { get; set; }
        public decimal UnionFee { get; set; }
        public decimal OptionalTax { get; set; }
        public decimal SINKTax { get; set; }
        public decimal ASINKTax { get; set; }
        public decimal EmploymentTaxBasis { get; set; }
    }
}
