using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class VacationBalanceItem
    {
        public VacationBalanceItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }
        public string EmploymentNr { get; set; }
        public string EmployeeName { get { return $"{FirstName} {LastName}"; } }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SocialSecurityNumber { get; set; }
        public bool Active { get; set; }
        public string Categories { get; set; }
        public string BirthYearMonth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Roles { get; set; }
        public string EmploymentPosition { get; set; }
        public string PayrollAgreement { get; set; }
        public string ContractGroup { get; set; }
        public string VacationAgreement { get; set; }
        public int WeeklyWorkingHours { get; set; }
        public decimal EmploymentRate { get; set; }
        public int BasicWeeklyWorkingHours { get; set; }
        public decimal PaidEarnDays { get; set; }
        public decimal PaidSelectedDays { get; set; }
        public decimal PaidRemainingDays { get; set; }
        public decimal PaidSysDegreeEarned { get; set; }
        public decimal PaidHolidayAllowance { get; set; }
        public decimal PaidVariableVacationSupplementsSelectedDays { get; set; }
        public decimal UnpaidEarnedDays { get; set; }
        public decimal UnpaidSelectedDays { get; set; }
        public decimal UnpaidRemainingDays { get; set; }
        public decimal AdvanceEarnedDays { get; set; }
        public decimal AdvanceSelectedDays { get; set; }
        public decimal AdvanceRemaininDays { get; set; }
        public decimal DebtCashAdvancesAmount { get; set; }
        public string DebtCashAdvancesDecay { get; set; }
        public decimal SavedYear1EarnedDays { get; set; }
        public decimal SavedYear1SelectedDays { get; set; }
        public decimal SavedYear1RemaininDays { get; set; }
        public decimal SavedYear1SysDegreeEarned { get; set; }
        public decimal SavedYear2EarnedDays { get; set; }
        public decimal SavedYear2SelectedDays { get; set; }
        public decimal SavedYear2RemaininDays { get; set; }
        public decimal SavedYear2SysDegreeEarned { get; set; }
        public decimal SavedYear3EarnedDays { get; set; }
        public decimal SavedYear3SelectedDays { get; set; }
        public decimal SavedYear3RemaininDays { get; set; }
        public decimal SavedYear3SysDegreeEarned { get; set; }
        public decimal SavedYear4EarnedDays { get; set; }
        public decimal SavedYear4SelectedDays { get; set; }
        public decimal SavedYear4RemaininDays { get; set; }
        public decimal SavedYear4SysDegreeEarned { get; set; }
        public decimal SavedYear5EarnedDays { get; set; }
        public decimal SavedYear5SelectedDays { get; set; }
        public decimal SavedYear5RemaininDays { get; set; }
        public decimal SavedYear5SysDegreeEarned { get; set; }
        public decimal OverdueDaysEarnedDays { get; set; }
        public decimal OverdueDaysSelectedDays { get; set; }
        public decimal OverdueDaysRemainingDays { get; set; }
        public decimal OverdueDaysSysDegreeEarned { get; set; }
        public decimal PreliminaryWithdrawnRemaininDays { get; set; }
        public decimal RemainingSelectedDays { get; set; }
        public decimal RemainingRemainingDays { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
    }
}
