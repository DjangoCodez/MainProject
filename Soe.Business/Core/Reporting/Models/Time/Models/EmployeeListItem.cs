using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeListItem
    {

        public EmployeeListItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }

        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SocialSec { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }
        public string UserName { get; set; }
        public string EmployeeExternalCode { get; set; }
        public string ExternalAuthId { get; set; }
        public string NearestExecutiveUserName { get; set; }
        public string NearestExecutiveName { get; set; }
        public string NearestExecutiveEmail { get; set; }
        public string NearestExecutiveSocialSec { get; set; }
        public string NearestExecutiveCellPhone { get; set; }
        public string Language { get; set; }
        public string DefaultCompany { get; set; }
        public string DefaultRole { get; set; }
        public bool IsMobileUser { get; set; }
        public bool IsSysUser { get; set; }
        public decimal EmployeeCalculatedCostPerHour { get; set; }
        public string Note { get; set; }
        public DateTime EmploymentDate { get; set; }
        public DateTime FirstEmploymentDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal MonthlySalary { get; set; }
        public decimal HourlySalary { get; set; }
        public decimal Salary { get; set; }
        public int LASDays { get; set; }
        public string EmploymentTypeName { get; set; }
        public string PayrollGroupName { get; set; }
        public string EmployeeGroupName { get; set; }
        public string VacationGroupName { get; set; }
        public int WorkTimeWeekMinutes { get; set; }
        public decimal WorkTimeWeekPercent { get; set; }
        public string WorkPlace { get; set; }
        public bool HasSecondaryEmployment { get; set; }
        public string DisbursementMethodText { get; set; }
        public string DisbursementClearingNr { get; set; }
        public string DisbursementAccountNr { get; set; }
        public string DisbursementCountryCode { get; set; }
        public string DisbursementBIC { get; set; }
        public string DisbursementIBAN { get; set; }
        public string SSYKCode { get; set; }
        public string SSYKName { get; set; }
        public string PayrollStatisticsPersonalCategory { get; set; }
        public string PayrollStatisticsWorkTimeCategory { get; set; }
        public string PayrollStatisticsSalaryType { get; set; }
        public string PayrollStatisticsWorkPlaceNumber { get; set; }
        public string PayrollStatisticsCFARNumber { get; set; }
        public string WorkPlaceSCB { get; set; }
        public bool PartnerInCloseCompany { get; set; }
        public bool BenefitAsPension { get; set; }
        public string AFACategory { get; set; }
        public string AFASpecialAgreement { get; set; }
        public string AFAWorkplaceNr { get; set; }
        public bool AFAParttimePensionCode { get; set; }
        public string CollectumITPPlan { get; set; }
        public string CollectumAgreedOnProduct { get; set; }
        public string CollectumCostPlace { get; set; }
        public DateTime? CollectumCancellationDate { get; set; }
        public bool CollectumCancellationDateIsLeaveOfAbsence { get; set; }
        public int KPARetirementAge { get; set; }
        public string KPABelonging { get; set; }
        public string KPAEndCode { get; set; }
        public string KPAAgreementType { get; set; }
        public string BygglosenAgreementArea { get; set; }
        public string BygglosenAllocationNumber { get; set; }
        public string BygglosenMunicipalCode { get; set; }
        public string BygglosenSalaryFormula { get; set; }
        public string BygglosenProfessionCategory { get; set; }
        public string BygglosenSalaryType { get; set; }
        public string BygglosenWorkPlaceNumber { get; set; }
        public string BygglosenLendedToOrgNr { get; set; }
        public decimal BygglosenAgreedHourlyPayLevel { get; set; }
        public string GTPAgreementNumber { get; set; }
        public bool GTPExcluded { get; set; }
        public string DistributionAddress { get; set; }
        public string Email { get; set; }
        public string CellPhone { get; set; }
        public string HomePhone { get; set; }
        public string ClosestRelative { get; set; }
        public bool ExcludeFromPayroll { get; set; }
        public bool Vacant { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }
        public string AccountInternalNr1 { get; set; }
        public string AccountInternalName1 { get; set; }
        public string AccountInternalNr2 { get; set; }
        public string AccountInternalName2 { get; set; }
        public string AccountInternalNr3 { get; set; }
        public string AccountInternalName3 { get; set; }
        public string AccountInternalNr4 { get; set; }
        public string AccountInternalName4 { get; set; }
        public string AccountInternalNr5 { get; set; }
        public string AccountInternalName5 { get; set; }
        public string CategoryName { get; set; }
        public string AGIPlaceOfEmploymentAddress { get; set; }
        public string AGIPlaceOfEmploymentCity { get; set; }
        public bool AGIPlaceOfEmploymentIgnore { get; set; }
        public string AddressRow { get; set; }
        public string AddressRow2 { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public Position Position { get; set; }
        public string EmploymentTypeOnSecondaryEmployment { get; set; }
        public string SecondaryEmploymentExcludeFromWorkTimeWeekCalculation { get; set; }
        public string SecondaryEmploymentExcludeFromWorkTimeWeekCalculationEmploymentType { get; set; }

        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
        public string IFPaymentCode { get; set; }
        public int IFAssociationNumber { get; set; }
        public string IFWorkPlace { get; set; }
        public string EmploymentExternalCode { get; set; }

        //Matrix
        public AccountAndInternalAccountComboDTO AccountAndInternalAccountCombo { get; set; }
        public decimal VacationDaysPaidByLaw { get; set; }
       
    }
}
