using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    /// <summary>
    /// Represents the request for fetching employee information.
    /// </summary>
    ///         [LogEmployeeId]
    public class FetchEmployeeInformation
    {

        public FetchEmployeeInformation()
        {
            AddVirtualHierarchyAccounts = false;
            OnePerChangeCalenderDayInfo = false;
        }

        /// <summary>
        /// List of employee numbers. Use this or EmployeeExternalCodes, will add employees to the list.
        /// </summary>
        public List<string> EmployeeNrs { get; set; }

        /// <summary>
        /// List of employee external codes. Use this or EmployeeNrs, will add employees to the list.
        /// </summary>
        public List<string> EmployeeExternalCodes { get; set; }

        /// <summary>
        /// The start date for filtering the information.
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// The end date for filtering the information.
        /// </summary>
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Indicates whether to load contact information.
        /// </summary>
        public bool LoadContactInformation { get; set; }

        /// <summary>
        /// Indicates whether to load employment information.
        /// </summary>
        public bool LoadEmployments { get; set; }

        /// <summary>
        /// Indicates whether to load the accounts connected to the employment. Will only load if LoadEmployments is set to true.
        /// </summary>
        public bool LoadEmploymentAccounts { get; set; }


        // <summary>
        /// Indicates whether to load employment changes.
        ///  </summary>
        public bool LoadEmploymentChanges { get; set; }

        // <summary>
        /// Indicates whether to set values according to start date on employment. (instead of current or last date)
        ///  </summary>
        public bool SetInitialValuesOnEmployment { get; set; }

        /// <summary>
        /// Indicates whether to load calendar information.
        /// </summary>
        public bool LoadCalenderInfo { get; set; }

        /// <summary>
        /// If this is true, then the date will be the startdate and all dates after are the same until the next change. Use this to limit the size of the list.
        /// </summary>
        public bool OnePerChangeCalenderDayInfo { get; set; }
        /// <summary>
        /// Indicates whether to load vacation information.
        /// </summary>
        public bool LoadVacationInfo { get; set; }

        /// <summary>
        /// Indicates whether to load vacation information history.
        /// </summary>
        public bool LoadVacationInfoHistory { get; set; }

        /// <summary>
        /// Indicates whether to load hierarchy accounts.
        /// </summary>
        public bool LoadHierarchyAccounts { get; set; }

        /// <summary>
        /// Indicates whether to load executives.
        /// </summary>
        public bool LoadExecutives { get; set; }

        /// <summary>
        /// Indicates whether to load nearest executive.
        /// </summary>
        public bool LoadNearestExecutive { get; set; }

        /// <summary>
        /// Indicates whether to add virtual hierarchy accounts. Default is false.
        /// </summary>
        public bool AddVirtualHierarchyAccounts { get; set; }

        /// <summary>
        /// Indicates whether to load positions.
        /// </summary>
        public bool LoadPositions { get; set; }

        /// <summary>
        /// Indicates whether to load report settings.
        /// </summary>
        public bool LoadReportSettings { get; set; }

        ///<summary>
        /// Indicates whether to load extra fields
        /// </summary>
        public bool LoadExtraFields { get; set; }

        /// <summary>
        /// Indicates whether to load skills.
        /// </summary>
        public bool LoadSkills { get; set; }

        /// <summary>
        /// Indicates whether to load user information.
        /// </summary>
        public bool LoadUser { get; set; }

        /// <summary>
        /// Indicates whether to load user roles.
        /// </summary>
        public bool LoadUserRoles { get; set; }

        /// <summary>
        /// Indicates whether to load social security number.
        /// </summary>
        public bool LoadSocialSec { get; set; }

        /// <summary>
        /// Filters employees changed or added after the specified date and time in UTC.
        /// </summary>
        public DateTime? EmployeesChangedOrAddedAfterUtc { get; set; }

        /// <summary
        /// Load payroll information about the employee
        /// </summary>
        public bool LoadPayrollInformation { get; set; }
        // / <summary>
        /// Indicates whether to load payroll formula result. (LoadPayrollInformation must be true aswell) This is very performance intensive and should be used only when really needed.
        /// </summary>
        public bool LoadPayrollFormulaResult { get; set; } = false;
    }


    /// <summary>
    /// Represents detailed information about an employee.
    /// </summary>
    [Log]
    public class EmployeeInformation
    {
        /// <summary>
        /// Employee's Id
        /// </summary>
        [LogEmployeeId]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Employee's social security number.
        /// </summary>
        [LogSocSec]
        public string SocialSecurityNumber { get; set; }

        /// <summary>
        /// Employee number.
        /// </summary>
        public string EmployeeNr { get; set; }

        /// <summary>
        /// Employee's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Employee's last name.
        /// </summary>
        public string LastName { get; set; }


        /// <summary>
        /// Employee's external code.
        /// </summary>
        public string EmployeeExternalCode { get; set; }

        /// <summary>
        /// Indicates whether the employee is excluded from payroll.
        /// </summary>
        public bool ExcludeFromPayroll { get; set; }

        /// <summary>
        /// The date and time when the employee was created.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The date and time when the employee was last modified.
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// Indicates whether the employee is vacant.
        /// </summary>
        public bool Vacant { get; set; }

        /// <summary>
        /// Employee's contact information.
        /// </summary>
        public EmployeeContactInformation ContactInformation { get; set; } = new EmployeeContactInformation();

        /// <summary>
        /// List of employments related to the employee.
        /// Is only populated if the FetchEmployeeInformation.LoadEmployments property is set to true.
        /// </summary>
        public List<EmployeeInformationEmployment> Employments { get; set; }

        /// <summary>
        /// List of calendar information entries related to the employee.
        /// Is only populated if the FetchEmployeeInformation.LoadCalenderInfo property is set to true.
        /// </summary>
        public List<CalenderInfo> CalenderInfos { get; set; }

        /// <summary>
        /// Employee's vacation information.
        /// Is only populated if the FetchEmployeeInformation.LoadVacationInfo property is set to true.
        /// </summary>
        public EmployeeInformationVacation InformationVacation { get; set; }

        /// <summary>
        /// Employee's hierarchy accounts.
        /// </summary>
        public List<HierarchyAccount> HierarchyAccounts { get; set; }

        /// <summary>
        /// Employee's executives.
        /// </summary>
        public List<EmployeeInformationExecutive> Executives { get; set; }

        /// <summary>
        /// Employee's positions.
        /// </summary>
        public List<EmployeeInformationPosition> Positions { get; set; }

        /// <summary>
        /// Employee's report settings.
        /// </summary>
        public List<EmployeeInformationReportSetting> ReportSettings { get; set; }

        /// <summary>
        /// Employee's extra fields.
        /// </summary>
        public List<EmployeeInformationExtraField> ExtraFields { get; set; }

        /// <summary>
        /// Employee's skills.
        /// </summary>
        public List<EmployeeInformationSkill> Skills { get; set; }

        /// <summary>
        /// Employee's User information.
        /// </summary>  
        public EmployeeInformationUser UserInformation { get; set; }
    }

    /// <summary>
    /// Represents contact information of an employee.
    /// </summary>
    public class EmployeeContactInformation
    {
        /// <summary>
        /// List of email addresses.
        /// </summary>
        public List<EmployeeInformationEmail> Emails { get; set; } = new List<EmployeeInformationEmail>();

        /// <summary>
        /// List of phone numbers.
        /// </summary>
        public List<EmployeeInformationPhone> Phones { get; set; } = new List<EmployeeInformationPhone>();

        /// <summary>
        /// List of addresses.
        /// </summary>
        public List<EmployeeInformationAddress> Addresses { get; set; } = new List<EmployeeInformationAddress>();
    }

    /// <summary>
    /// Represents an email address of an employee.
    /// </summary>
    public class EmployeeInformationEmail
    {
        /// <summary>
        /// The email address.
        /// </summary>
        public string Email { get; set; }
    }

    /// <summary>
    /// Represents a phone number of an employee.
    /// </summary>
    public class EmployeeInformationPhone
    {
        /// <summary>
        /// The type of phone number (e.g., mobile, home).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The phone number.
        /// </summary>
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// Represents an address of an employee.
    /// </summary>
    public class EmployeeInformationAddress
    {
        /// <summary>
        /// The type of address (e.g., home, office).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Care of address (c/o).
        /// </summary>
        public string AddressCO { get; set; }

        /// <summary>
        /// Postal code.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// City.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country.
        /// </summary>
        public string Country { get; set; }
    }

    /// <summary>
    /// Represents employment information of an employee.
    /// </summary>
    public class EmployeeInformationEmployment
    {
        /// <summary>
        /// The type of employment.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The start date of the employment.
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// The end date of the employment, if applicable.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Work time per week in hours.
        /// </summary>
        public int WorkTimeWeek { get; set; }

        /// <summary>
        /// Employment percentage.
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Number of months of experience.
        /// </summary>
        public int ExperienceMonths { get; set; }

        /// <summary>
        /// Indicates whether the experience is agreed upon or established.
        /// </summary>
        public bool ExperienceAgreedOrEstablished { get; set; }

        /// <summary>
        /// The workplace of the employee.
        /// </summary>
        public string WorkPlace { get; set; }

        /// <summary>
        /// Special conditions related to the employment.
        /// </summary>
        public string SpecialConditions { get; set; }


        /// <summary>
        /// Reason for the end of employment.
        /// </summary>
        public int EmploymentEndReason { get; set; }

        /// <summary>
        /// Name of the reason for the end of employment.
        /// </summary>
        public string EmploymentEndReasonName { get; set; }

        /// <summary>
        /// Base work time per week in hours.
        /// </summary>
        public int BaseWorkTimeWeek { get; set; }

        /// <summary>
        /// Substitute for another employee.
        /// </summary>
        public string SubstituteFor { get; set; }

        /// <summary>
        /// Name of the employee group.
        /// </summary>
        public string EmployeeGroupName { get; set; }

        /// <summary>
        /// Name of the payroll group.
        /// </summary>
        public string PayrollGroupName { get; set; }

        /// <summary>
        /// Name of the vacation group.
        /// </summary>
        public string VacationGroupName { get; set; }

        /// <summary>
        /// Work tasks of the employee.
        /// </summary>
        public string WorkTasks { get; set; }

        /// <summary>
        /// External code associated with the employment.
        /// </summary>
        public string ExternalCode { get; set; }

        /// <summary>
        /// Indicates if the employment is temporarily primary.
        /// </summary>
        public bool IsTemporaryPrimary { get; set; }

        /// <summary>
        /// Indicates if the employment is secondary.(not primary)
        /// </summary>
        public bool IsSecondaryEmployment { get; set; }

        /// <summary>
        /// Substitute due to a specific reason.
        /// </summary>
        public string SubstituteForDueTo { get; set; }

        /// <summary>
        /// Indicates if experience months reminder should be updated.
        /// </summary>
        public bool UpdateExperienceMonthsReminder { get; set; }

        /// <summary>
        /// Type of employment (integer representation).
        /// </summary>
        public int EmploymentType { get; set; }

        /// <summary>
        /// Name of the employment type.
        /// </summary>
        public string EmploymentTypeName { get; set; }

        // <summary>
        /// Unique identifier of the employment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Collection of accounts connected to the employment.
        /// </summary>
        public List<EmployeeInformationEmploymentAccount> EmploymentAccounts { get; set; }

        /// <summary>
        /// Collection of changes to the employment.
        /// </summary>
        public List<EmployeeInformationEmploymentChange> EmploymentChanges { get; set; }

        /// <summary>
        /// Connected Pricetypes
        /// </summary>
        public List<EmployeeInformationEmploymentPriceType> EmploymentPriceTypes { get; set; } = new List<EmployeeInformationEmploymentPriceType>();

        /// <summary>
        /// Calulated Pricetypes (formulas)
        /// </summary>  
        public List<EmployeeInformationEmploymentPriceFormulaResult> EmploymentPriceFormulaResults { get; set; } = new List<EmployeeInformationEmploymentPriceFormulaResult>();
    }

    /// <summary>
    /// Represents a change in employment information for an employee.
    /// </summary>
    public class EmployeeInformationEmploymentChange
    {
        /// <summary>
        /// The start date of the change.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// The end date of the change.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// The type of field that was changed.
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// The name of the field type.
        /// </summary>
        public string FieldTypeName { get; set; }

        /// <summary>
        /// The previous value of the field.
        /// </summary>
        public string FromValue { get; set; }

        /// <summary>
        /// The new value of the field.
        /// </summary>
        public string ToValue { get; set; }

        /// <summary>
        /// The name of the new value.
        /// </summary>
        public string ToValueName { get; set; }

        /// <summary>
        /// The name of the previous value.
        /// </summary>
        public string FromValueName { get; set; }

    }

    /// <summary>
    /// Represents vacation information of an employee.
    /// </summary>
    public class EmployeeInformationVacation
    {
        /// <summary>
        /// Remaining paid vacation days.
        /// </summary>
        public decimal? RemainingDaysPaid { get; set; }

        /// <summary>
        /// Remaining unpaid vacation days.
        /// </summary>
        public decimal? RemainingDaysUnpaid { get; set; }

        /// <summary>
        /// Remaining advance vacation days.
        /// </summary>
        public decimal? RemainingDaysAdvance { get; set; }

        /// <summary>
        /// Remaining vacation days for the first year.
        /// </summary>
        public decimal? RemainingDaysYear1 { get; set; }

        /// <summary>
        /// Remaining vacation days for the second year.
        /// </summary>
        public decimal? RemainingDaysYear2 { get; set; }

        /// <summary>
        /// Remaining vacation days for the third year.
        /// </summary>
        public decimal? RemainingDaysYear3 { get; set; }

        /// <summary>
        /// Remaining vacation days for the fourth year.
        /// </summary>
        public decimal? RemainingDaysYear4 { get; set; }

        /// <summary>
        /// Remaining vacation days for the fifth year.
        /// </summary>
        public decimal? RemainingDaysYear5 { get; set; }

        /// <summary>
        /// Remaining overdue vacation days.
        /// </summary>
        public decimal? RemainingDaysOverdue { get; set; }

        /// <summary>
        /// Date of adjustment for the vacation information.
        /// </summary>
        public DateTime? AdjustmentDate { get; set; }

        /// <summary>
        /// Date of last update for the vacation information.
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// History of vacation information entries.
        /// </summary>
        public List<EmployeeInformationVacation> EmployeeInformationVacationHistory { get; set; }
    }

    /// <summary>
    /// Represents a hierarchy account.
    /// </summary>
    public class HierarchyAccount
    {
        /// <summary>
        /// The account number.
        /// </summary>
        public string AccountNr { get; set; }

        /// <summary>
        /// The account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// The dimension number.
        /// </summary>
        public int DimNr { get; set; }

        /// <summary>
        /// The Sie dimension number. https://sie.se/
        /// </summary>
        public int SieDimNr { get; set; }

        /// <summary>
        /// Indicates if it is the default account.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Indicates if it is the main allocation account.
        /// </summary>
        public bool MainAllocation { get; set; }

        /// <summary>
        /// The start date of the account.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// The end date of the account.
        /// </summary>
        public DateTime? DateTo { get; set; }
        /// <summary>
        /// If virtual account, the account is calculated from the account not by the employees hierarchy.
        /// </summary>
        public bool Virtual { get; set; }

        /// <summary>
        /// The list of child hierarchy accounts.
        /// </summary>
        public List<HierarchyAccount> Children { get; set; } = new List<HierarchyAccount>();

    }
    /// <summary>
    /// Represents the position information of an employee returned from the API.
    /// </summary>
    public class EmployeeInformationPosition
    {
        /// <summary>
        /// The code of the position.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The name of the position.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The system code associated with the position.
        /// </summary>
        public string SysCode { get; set; }

        /// <summary>
        /// If position is set as default for the employee.
        /// </summary>
        public bool Default { get; set; }
    }

    /// <summary>
    /// Represents the report setting information for an employee.
    /// </summary>
    public class EmployeeInformationReportSetting
    {
        /// <summary>
        /// The name of the  setting.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the  setting.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The start date of the setting.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// The end date of the setting.
        /// </summary>
        public DateTime? DateTo { get; set; }
    }

    /// <summary>
    /// Represents additional field information for an employee returned from the API.
    /// </summary>
    public class EmployeeInformationExtraField
    {
        /// <summary>
        /// The name of the extra field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the extra field.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The external codes associated with the extra field.
        /// </summary>
        public List<string> ExternalCodes { get; set; }

        /// <summary>
        /// The unique identifier of the extra field. (per database) ExternalCodes are preferred.
        /// </summary>
        public int ExtraFieldId { get; set; }
    }

    /// <summary>
    /// Represents user information for an employee.
    /// </summary>
    public class EmployeeInformationUser
    {
        /// <summary>
        /// The username of the employee.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The roles associated with the employee.
        /// </summary>
        public List<EmployeeInformationUserRole> UserRoles { get; set; }

        /// <summary>
        /// The attestroles associated with the employee.
        /// </summary>
        public List<EmployeeInformationUserAttestRole> UserAttestRoles { get; set; }
    }

    /// <summary>
    /// Represents a user role for an employee.
    /// </summary>
    public class EmployeeInformationUserRole
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The start date of the connection to the role.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// The end date of the connection to the role.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Indicates whether this role is the default role for employee/user on this company
        /// </summary>
        public bool Default { get; set; }
    }

    /// <summary>
    /// Represents an attest role for an employee/user returned
    /// </summary>
    public class EmployeeInformationUserAttestRole
    {
        /// <summary>
        /// The name of the attest role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The start date of theconnection to the attest role.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// The end date of the connection to the attest role.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// The account number associated with the attest role.
        /// </summary>
        public string AccountNr { get; set; }

        /// <summary>
        /// The account name associated with the attest role.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Indicates whether the attest role user is the nearest manager.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? DefinedManager { get; set; }
    }

    /// <summary>
    /// Represents an account connected to an employment.
    /// </summary>
    public class EmployeeInformationEmploymentAccount
    {
        /// <summary>
        /// The connection type of the account. For example, Cost, Income, or Fixed.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The account number.
        /// </summary>
        public string AccountNr { get; set; }

        /// <summary>
        /// The account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// The dimension number.
        /// </summary>
        public int DimNr { get; set; }

        /// <summary>
        /// The Sie dimension number.
        /// </summary>
        public int SieDimNr { get; set; }

        /// <summary>
        /// The percentage associated with the account. Only applicable for fixed accounts.
        /// </summary>
        public decimal Percent { get; set; }
    }

    /// <summary>
    /// Employee skill
    /// </summary>
    public class EmployeeInformationSkill
    {
        /// <summary>
        /// Skill name
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// Skill level 0-100
        /// </summary>
        public int SkillLevel { get; set; }

        /// <summary>
        /// Skill is valid to
        /// </summary>  
        public DateTime? DateTo { get; set; }
    }

    /// <summary>
    /// Executive information for an employee.
    /// </summary>

    public class EmployeeInformationExecutive
    {
        /// <summary>
        /// Name Of Executive
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Email of Executive
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Social Security Number of Executive
        /// </summary>        
        public string SocialSec { get; set; }
        /// <summary>
        /// Is Nearest Executive
        /// </summary>     
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNearest { get; set; }
        /// <summary>
        /// Intervals with dates of which the executive is valid
        /// </summary>
        public List<EmployeeInformationExecutiveInterval> Intervals { get; set; } = new List<EmployeeInformationExecutiveInterval>();
    }

    /// <summary>
    /// Interval with dates of which the executive is valid
    /// </summary>
    public class EmployeeInformationExecutiveInterval
    {
        /// <summary>
        /// Name of Role
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// Date from which the executive is valid (if not set it is set to 1900-01-01)
        /// </summary>
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Date to which the executive is valid. If null then it is valid until the end of time.
        /// </summary>
        public DateTime? DateTo { get; set; }
    }

    /// <summary>
    /// Contains payroll information about the employee
    /// </summary>
    /// <summary>
    /// Represents payroll price type information for an employee's employment.
    /// </summary>
    public class EmployeeInformationEmploymentPriceType
    {
        /// <summary>
        /// The code of the price type.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The name of the price type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The integer value representing the type of the price type.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// The name of the price type type.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The current amount for the price type.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The start date for the price type.
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Indicates if this is a payroll group price type (0 or 1).
        /// </summary>
        public int IsPayrollGroupPriceType { get; set; }
    }
    /// <summary>
    /// Represents the result of evaluating a payroll price formula for an employment.
    /// </summary>
    public class EmployeeInformationEmploymentPriceFormulaResult
    {
        /// <summary>
        /// The names of the formulas used in the evaluation.
        /// </summary>
        public string FormulaName { get; set; }

        /// <summary>
        /// The plain (unparsed) formula string.
        /// </summary>
        public string FormulaPlain { get; set; }

        /// <summary>
        /// The extracted (parsed or calculated) formula string.
        /// </summary>
        public string FormulaExtracted { get; set; }

        /// <summary>
        /// The amount calculated by the formula.
        /// </summary>
        public decimal FormulaAmount { get; set; }
    }

}
