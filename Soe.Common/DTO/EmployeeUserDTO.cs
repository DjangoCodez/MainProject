using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    [Log]
    public class EmployeeUserDTO : IEmployeeUserBasic
    {
        #region Employee

        //PK
        [LogEmployeeId]
        public int EmployeeId { get; set; }

        //Fields
        public string EmployeeNr { get; set; }
        public string EmployeeNrAndName
        {
            get
            {
                return String.Format("({0}) {1}", this.EmployeeNr, this.Name);
            }
        }
        public bool Vacant { get; set; }
        public bool PortraitConsent { get; set; }
        public DateTime? PortraitConsentDate { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool UseFlexForce { get; set; }
        public string CardNumber { get; set; }
        public TermGroup_EmployeeDisbursementMethod DisbursementMethod { get; set; }
        public string DisbursementClearingNr { get; set; }
        public string DisbursementAccountNr { get; set; }
        public string DisbursementCountryCode { get; set; }
        public string DisbursementBIC { get; set; }
        public string DisbursementIBAN { get; set; }
        public bool DontValidateDisbursementAccountNr { get; set; }
        public string Note { get; set; }
        public bool ShowNote { get; set; }
        [LogIllnessInformationAttribute]
        public bool HighRiskProtection { get; set; }
        [LogIllnessInformationAttribute]
        public DateTime? HighRiskProtectionTo { get; set; }
        [LogIllnessInformationAttribute]
        public bool MedicalCertificateReminder { get; set; }
        [LogIllnessInformationAttribute]
        public int? MedicalCertificateDays { get; set; }
        [LogIllnessInformationAttribute]
        public bool Absence105DaysExcluded { get; set; }
        [LogIllnessInformationAttribute]
        public int? Absence105DaysExcludedDays { get; set; }
        public string ExternalCode { get; set; }
        public bool WantsExtraShifts { get; set; }
        public bool DontNotifyChangeOfDeviations { get; set; }
        public bool DontNotifyChangeOfAttestState { get; set; }

        // Payroll statistics 
        public int? PayrollReportsPersonalCategory { get; set; }
        public int? PayrollReportsWorkTimeCategory { get; set; }
        public int? PayrollReportsSalaryType { get; set; }
        public int? PayrollReportsWorkPlaceNumber { get; set; }
        public int? PayrollReportsCFARNumber { get; set; }

        //Payroll Report settings
        public string WorkPlaceSCB { get; set; }
        public bool PartnerInCloseCompany { get; set; }
        public bool BenefitAsPension { get; set; }
        public int AFACategory { get; set; }
        public int AFASpecialAgreement { get; set; }
        public string AFAWorkplaceNr { get; set; }
        public bool AFAParttimePensionCode { get; set; }
        public int CollectumITPPlan { get; set; }
        public string CollectumAgreedOnProduct { get; set; }
        public string CollectumCostPlace { get; set; }
        public DateTime? CollectumCancellationDate { get; set; }
        public bool CollectumCancellationDateIsLeaveOfAbsence { get; set; }
        public int? KpaRetirementAge { get; set; }
        public int? KpaBelonging { get; set; }
        public int? KpaEndCode { get; set; }
        public int KpaAgreementType { get; set; }
        public int GtpAgreementNumber { get; set; }
        public bool GtpExcluded { get; set; }
        public string AGIPlaceOfEmploymentAddress { get; set; }
        public string AGIPlaceOfEmploymentCity { get; set; }
        public bool AGIPlaceOfEmploymentIgnore { get; set; }
        public int IFAssociationNumber { get; set; }
        public int IFPaymentCode { get; set; }
        public string IFWorkPlace { get; set; }
        public string ParentEmployeeNr { get; set; } = null;
        public int? ParentId { get; set; } = null;

        //FK
        public int ActorCompanyId { get; set; }
        public int? EmployeeTemplateId { get; set; }
        public string EmployeeTemplateName { get; set; }
        public int? TimeCodeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int CurrentEmployeeGroupId { get; set; }
        public List<int> AttestRoleIds { get; set; }
        public int CategoryId { get; set; }

        //Collections
        public List<EmploymentDTO> Employments { get; set; }
        public List<EmployeeAccountDTO> Accounts { get; set; }
        public List<CompanyCategoryRecordDTO> CategoryRecords { get; set; }
        public List<TimeScheduleTemplateGroupEmployeeDTO> TemplateGroups { get; set; }
        public List<EmployeeSkillDTO> EmployeeSkills { get; set; }
        public List<EmployeeChildDTO> EmployeeChilds { get; set; }
        public List<EmployeeChildDTO> ParentalLeaves { get; set; }
        public List<EmployeeChildCareDTO> ChildCares { get; set; }
        public List<EmployeeFactorDTO> Factors { get; set; }
        public EmployeeVacationSEDTO EmployeeVacationSE { get; set; }
        public List<EmployeeMeetingDTO> EmployeeMeetings { get; set; }
        public List<EmployeeUnionFeeDTO> UnionFees { get; set; }
        public List<UserRolesDTO> UserRoles { get; set; }
        public List<EmployeeCalculatedCostDTO> CalculatedCosts { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFieldRecords { get; set; }
        public List<EmployeeTimeWorkAccountDTO> EmployeeTimeWorkAccounts { get; set; }
        public List<EmployeeSettingDTO> EmployeeSettings { get; set; }
        [TsIgnore]
        public EmployeeTaxSEDTO EmployeeTax { get; set; }   // Only used througt the API with employee templates
        [TsIgnore]
        public List<EmployeeChangeDTO> CurrentChanges { get; set; }

        public List<EmployeeEmployeerDTO> EmployeeEmployeers { get; set; } = null;

        //Flags
        public bool IsEmploymentsChanged { get; set; }
        public bool IsEmployeeMeetingsChanged { get; set; }
        public bool IsPayrollUpdated { get; set; }
        public bool IsTemplateGroupsChanged { get; set; }
        public bool ExcludeFromPayroll { get; set; }
        public bool HasNotAttestRoleToSeeEmployee { get; set; }
        [TsIgnore]
        public bool IsNew { get; set; }

        //End Schedule automatically on date if terminated
        public DateTime? ClearScheduleFrom { get; set; }

        #endregion

        #region User

        //PK
        public int UserId { get; set; }

        //Fields
        public int? DefaultActorCompanyId { get; set; }
        public int? LangId { get; set; }
        public string EstatusLoginId { get; set; }
        public string LoginName { get; set; }
        public bool ChangePassword { get; set; }
        public byte[] Password { get; set; }
        public string NewPassword { get; set; }
        public string PasswordHomePage { get; set; }
        public string Email { get; set; }
        public bool EmailCopy { get; set; }
        public bool IsMobileUser { get; set; }
        public DateTime? BlockedFromDate { get; set; }

        //FK
        public int LicenseId { get; set; }

        #endregion

        #region ContactPerson

        //PK
        public int ActorContactPersonId { get; set; }

        //Fields
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name
        {
            get { return FirstName + " " + LastName; }
        }
        [LogSocSec]
        public string SocialSec { get; set; }
        public TermGroup_Sex Sex { get; set; }

        #endregion

        #region Common

        //Fields
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Flags
        public bool Found { get; set; }
        public bool SaveEmployee { get; set; }
        public bool SaveUser { get; set; }
        public bool DisconnectExistingEmployee { get; set; }
        public bool DisconnectExistingUser { get; set; }

        public string ExternalAuthId { get; set; }
        public bool ExternalAuthIdModified { get; set; }
        public int LifetimeSeconds { get; set; }
        public bool LifetimeSecondsModified { get; set; }
        public string BygglosenAgreementArea { get; set; }
        public string BygglosenAllocationNumber { get; set; }
        public string BygglosenMunicipalCode { get; set; }
        public int BygglosenSalaryFormula { get; set; }
        public string BygglosenSalaryFormulaName { get; set; }
        public string BygglosenProfessionCategory { get; set; }
        public string BygglosenWorkPlaceNumber { get; set; }
        public string BygglosenLendedToOrgNr { get; set; }
        public int BygglosenSalaryType { get; set; }
        public decimal BygglosenAgreedHourlyPayLevel { get; set; }
        public string UserLinkConnectionKey { get; set; }
        public int TempTaxRate { get; set; }

        [TsIgnore]
        public bool SavingFromApi { get; set; } = false;



        #endregion

        #region Public methods

        public bool HasChanges(bool excludeError = false)
        {
            List<EmployeeChangeDTO> employeeCurrentChanges = this.CurrentChanges;
            if (!employeeCurrentChanges.IsNullOrEmpty() && excludeError)
                employeeCurrentChanges = employeeCurrentChanges.Where(i => i.FieldType > 0).ToList();

            if (!employeeCurrentChanges.IsNullOrEmpty() && (!excludeError || employeeCurrentChanges.Any(i => !i.HasError)))
                return true;
            if (!this.GetEmploymentCurrentChanges().IsNullOrEmpty())
                return true;
            return false;
        }

        public List<EmploymentChangeDTO> GetEmploymentCurrentChanges()
        {
            return this.Employments?.SelectMany(i => i.CurrentChanges).ToList() ?? new List<EmploymentChangeDTO>();
        }

        public void AddCurrentChange(int fieldType, DateTime? fromValue, DateTime? toValue, string fromValueName = null, string toValueName = null, string valuePrefix = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            string fromValueStr = fromValue?.ToShortDateString();
            string toValueStr = toValue?.ToShortDateString();

            AddCurrentChange(fieldType, fromValueStr, toValueStr, fromValueName, toValueName, valuePrefix, fromDate, toDate);
        }

        public void AddCurrentChange<T>(int fieldType, T fromValue, T toValue, string fromValueName = null, string toValueName = null, string valuePrefix = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (this.CurrentChanges == null)
                this.CurrentChanges = new List<EmployeeChangeDTO>();

            string fromValueFormatted = null;
            if (fromValue != null)
                fromValueFormatted = !string.IsNullOrEmpty(valuePrefix) ? $"{valuePrefix} {fromValue}" : fromValue.ToString();

            string toValueFormatted = null;
            if (toValue != null)
                toValueFormatted = !string.IsNullOrEmpty(valuePrefix) ? $"{valuePrefix} {toValue}" : toValue.ToString();

            this.CurrentChanges.Add(new EmployeeChangeDTO()
            {
                FieldType = fieldType,
                FromDate = fromDate?.Date,
                ToDate = toDate?.Date,
                FromValue = fromValueFormatted,
                ToValue = toValueFormatted,
                FromValueName = fromValueName,
                ToValueName = toValueName,
            });
        }

        public void AddErrorChange(int fieldType, string error)
        {
            if (this.CurrentChanges == null)
                this.CurrentChanges = new List<EmployeeChangeDTO>();

            this.CurrentChanges.Add(new EmployeeChangeDTO()
            {
                FieldType = fieldType,
                Error = error,
            });
        }

        public void RemoveChange<T>(int fieldType, T toValue)
        {
            EmployeeChangeDTO change = this.CurrentChanges?.FirstOrDefault(i => i.FieldType == fieldType && i.ToValue == toValue.ToString());
            if (change != null)
                this.CurrentChanges.Remove(change);
        }

        public void TryAddUserRoles(List<UserRolesDTO> userRoles)
        {
            if (userRoles == null)
                userRoles = new List<UserRolesDTO>();
            if (!userRoles.Any(ur => ur.ActorCompanyId == this.ActorCompanyId))
                userRoles.Add(new UserRolesDTO() { ActorCompanyId = this.ActorCompanyId });

            this.UserRoles = userRoles;
        }

        public bool DoSaveRoles()
        {
            return this.UserRoles?.Any(ur => ur.Roles != null && ur.Roles.Any(r => r.IsModified)) ?? false;
        }

        public bool DoSaveAttestRoles()
        {
            return this.UserRoles?.Any(ur => ur.AttestRoles != null && ur.AttestRoles.Any(r => r.IsModified)) ?? false;
        }

        #endregion
    }

    public class EmployeeUserImportBatch
    {
        #region Properties

        public int NrOfReceivedEmployees { get; private set; }
        public List<EmployeeUserImport> Imports { get; private set; }

        public ActionResult Result { get; set; }
        public StringBuilder StackTrace { get; set; }
        public bool IsLegacyMode { get; set; }

        public List<string> ValidationErrors { get; set; }
        public bool HasValidationErrors
        {
            get
            {
                return !this.ValidationErrors.IsNullOrEmpty();
            }
        }
        public bool IsAlreadyUpdated
        {
            get
            {
                return this.Imports?.TrueForAll(import => import.IsAlreadyUpdated) ?? false;
            }
        }

        #endregion

        #region Ctor

        public EmployeeUserImportBatch(int nrOfReceivedEmployees = 0)
        {
            this.NrOfReceivedEmployees = nrOfReceivedEmployees;
            this.Result = new ActionResult(true);
            this.Init();
        }

        public EmployeeUserImportBatch(ActionResult result)
        {
            this.NrOfReceivedEmployees = 0;
            this.Result = result;
            this.Init();
        }

        #endregion

        #region Public methods

        public bool HasValidChanges()
        {
            List<EmployeeUserImport> imports = this.Imports ?? new List<EmployeeUserImport>();
            foreach (EmployeeUserImport import in imports)
            {
                bool hasChanges = import?.EmployeeUser?.HasChanges(excludeError: true) ?? false;
                if (hasChanges)
                    return true;
            }

            return false;
        }

        #endregion

        #region Help-methods

        private void Init()
        {
            this.Imports = new List<EmployeeUserImport>();
            this.ValidationErrors = new List<string>();
            this.StackTrace = new StringBuilder();
        }

        #endregion
    }

    public class EmployeeUserImport
    {
        #region Contants

        public const int NOTHING_UPDATED = -1;
        public const int SAVE_FAILED = -2;

        #endregion

        #region Properties

        public EmployeeUserDTO EmployeeUser { get; set; }
        public bool IsNewEmployee
        {
            get
            {
                return this.EmployeeUser?.IsNew ?? false;
            }
        }
        public bool IsAlreadyUpdated
        {
            get
            {
                return this.EmployeeUser?.CurrentChanges?.Count == 1 && this.EmployeeUser.CurrentChanges[0].FieldType == EmployeeUserImport.NOTHING_UPDATED;
            }
        }
        public ActionResult Result { get; set; }

        #endregion

        #region Ctor

        public EmployeeUserImport(EmployeeUserDTO employeeUser)
        {
            this.EmployeeUser = employeeUser;
            this.Result = new ActionResult(true);
        }

        #endregion

        #region Public methods

        public List<EmployeeChangeDTO> GetEmployeeChangesToLog()
        {
            if (this.EmployeeUser == null || this.EmployeeUser.CurrentChanges.IsNullOrEmpty())
                return new List<EmployeeChangeDTO>();

            //If not saved successfully - return only error rows. Nothing saved so success rows was not committed
            if (!this.Result.Success)
                return this.EmployeeUser.CurrentChanges.Where(i => i.HasError).ToList();

            return this.EmployeeUser.CurrentChanges;
        }

        public List<EmploymentChangeDTO> GetEmploymentChangesToLog()
        {
            if (this.EmployeeUser == null)
                return new List<EmploymentChangeDTO>();

            //If not saved successfully - return empty collection. No support for error rows on Employment
            if (!this.Result.Success)
                return new List<EmploymentChangeDTO>();

            return this.EmployeeUser.GetEmploymentCurrentChanges();
        }

        public bool HasChanges()
        {
            return this.IsNewEmployee || (this.EmployeeUser?.HasChanges(excludeError: true) ?? false);
        }

        #endregion
    }

    public class CreateVacantEmployeeDTO
    {
        public string EmployeeNr { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime EmploymentDateFrom { get; set; }
        public int EmployeeGroupId { get; set; }
        public int WorkTimeWeek { get; set; }
        public decimal Percent { get; set; }
        public List<CompanyCategoryRecordDTO> Categories { get; set; }
        public List<EmployeeAccountDTO> Accounts { get; set; }
    }

    [TSInclude]
    public class EmployeeAccountDTO
    {
        public int EmployeeAccountId { get; set; }
        public int? ParentEmployeeAccountId { get; set; }
        public int EmployeeId { get; set; }
        public int? AccountId { get; set; }
        public bool Default { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool AddedOtherEmployeeAccount { get; set; }
        public bool MainAllocation { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<EmployeeAccountDTO> Children { get; set; }
    }

    public class EmployeeAccountSmallDTO
    {
        public int EmployeeAccountId { get; set; }
        public int? ParentEmployeeAccountId { get; set; }
        public int AccountId { get; set; }
        public int EmployeeId { get; set; }
        public bool Default { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<EmployeeAccountDTO> Children { get; set; }
    }

    public class EmployeeAccountFlattenedDTO
    {
        public int EmployeeAccountId { get; set; }
        public int? ParentEmployeeAccountId { get; set; }
        public int? AccountId { get; set; }
        public int Level { get; set; }
        public bool MainAllocation { get; set; }
        public bool Default { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class ValidatePossibleDeleteOfEmployeeAccountDTO
    {
        public int EmployeeId { get; set; }
        public List<ValidatePossibleDeleteOfEmployeeAccountRowDTO> Rows { get; set; }
    }

    public class ValidatePossibleDeleteOfEmployeeAccountRowDTO
    {
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int EmployeeAccountId { get; set; }
    }

    public class EmployeeUserApplyFeaturesResult
    {
        public EmployeeUserDTO EmployeeUserDTO { get; set; }
        public int LicenseId { get; }
        public int ActorCompanyId { get; }
        public int RoleId { get; }
        public int UserId { get; set; }
        public bool UserIsMySelf { get; }
        public List<Feature> Features { get; set; }
        private Dictionary<int, bool> permissions;

        private bool hasBlankedSocialSec;
        public bool HasBlankedSocialSec { get { return this.hasBlankedSocialSec; } }
        private bool hasBlankedCardNumber;
        public bool HasBlankedCardNumber { get { return this.hasBlankedCardNumber; } }
        private bool hasBlankedDisbursement;
        public bool HasBlankedDisbursement { get { return this.hasBlankedDisbursement; } }
        private bool hasBlankedPriceTypes;
        public bool HasBlankedPriceTypes { get { return this.hasBlankedPriceTypes; } }
        private bool hasBlankedAccounts;
        public bool HasBlankedAccounts { get { return this.hasBlankedAccounts; } }
        private bool hasBlankedUnionFees;
        public bool HasBlankedUnionFees { get { return this.hasBlankedUnionFees; } }
        private bool hasBlankedTimeWorkAccounts;
        public bool HasBlankedTimeWorkAccounts { get { return this.hasBlankedTimeWorkAccounts; } }
        private bool hasBlankedEmployeeVacationSE;
        public bool HasBlankedEmployeeVacationSE { get { return this.hasBlankedEmployeeVacationSE; } }
        private bool hasBlankedHighRiskProtection;
        public bool HasBlankedHighRiskProtection { get { return this.hasBlankedHighRiskProtection; } }
        private bool hasBlankedMedicalCertificateReminder;
        public bool HasBlankedMedicalCertificateReminder { get { return this.hasBlankedMedicalCertificateReminder; } }
        private bool hasBlankedChildCare;
        public bool HasBlankedChildCare { get { return this.hasBlankedChildCare; } }

        private bool hasBlankedEmployeeSkills;
        public bool HasBlankedEmployeeSkills { get { return this.hasBlankedEmployeeSkills; } }
        private bool hasBlankedTemplateGroups;
        public bool HasBlankedTemplateGroups { get { return this.hasBlankedTemplateGroups; } }
        private bool hasBlankedEmployeeMeetings;
        public bool HasBlankedEmployeeMeetings { get { return this.hasBlankedEmployeeMeetings; } }
        private bool hasBlankedNote;
        public bool HasBlankedNote { get { return this.hasBlankedNote; } }

        #region Ctor

        public EmployeeUserApplyFeaturesResult(EmployeeUserDTO employeeUser, int licenseId, int actorCompanyId, int roleId, int userId)
        {
            this.EmployeeUserDTO = employeeUser;
            this.LicenseId = licenseId;
            this.ActorCompanyId = actorCompanyId;
            this.RoleId = roleId;
            this.UserIsMySelf = employeeUser != null && employeeUser.UserId == userId;
            this.Features = new List<Feature>()
            {
                //Personal
                Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber,
                Feature.Time_Employee_Employees_Edit_MySelf_Contact,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact,
                Feature.Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount,
                Feature.Time_Employee_Employees_Edit_MySelf_User,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_User,
                Feature.Manage_Users_Edit_UserMapping,
                Feature.Manage_Users_Edit_AttestRoleMapping,
                Feature.Manage_Users_Edit_AttestReplacementMapping,

                //Employment
                Feature.Time_Employee_Employees_Edit_MySelf_Employments,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments,
                Feature.Time_Employee_Employees_Edit_MySelf_Employments_Employment,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment,
                Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary,
                Feature.Time_Employee_Employees_Edit_MySelf_Employments_Accounts,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts,
                Feature.Time_Employee_Employees_Edit_MySelf_Tax,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Tax,
                Feature.Time_Employee_Employees_Edit_MySelf_UnionFee,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_UnionFee,
                Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation,
                Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence,
                Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children,
                Feature.Time_Employee_Employees_Edit_MySelf_Reports,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports,
                Feature.Time_Employee_Employees_Edit_MySelf_Categories,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories,
                Feature.Time_Employee_Employees_Edit_MySelf_Time,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Time,
                Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour,
                Feature.Time_Employee_Employees_Edit_MySelf_WorkTimeAccount,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount,

                //HR
                Feature.Time_Employee_Employees_Edit_MySelf_Skills,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills,
                Feature.Time_Employee_Employees_Edit_MySelf_EmployeeMeeting,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting,
                Feature.Time_Employee_Employees_Edit_MySelf_Note,
                Feature.Time_Employee_Employees_Edit_OtherEmployees_Note,
            };
        }

        #endregion

        #region Public methods

        public void SetPermissions(Dictionary<int, bool> permissions)
        {
            this.permissions = permissions;
        }

        public bool IsMySelfOrHasPermission(Feature feature)
        {
            return this.UserIsMySelf || ContainsPermissionAndTrue(feature);
        }

        public bool HasPermission(Feature feature)
        {
            return ContainsPermissionAndTrue(feature);
        }

        public bool HasPermission(Feature mySelfFeature, Feature othersFeature)
        {
            return this.UserIsMySelf ? ContainsPermissionAndTrue(mySelfFeature) : ContainsPermissionAndTrue(othersFeature);
        }

        public void BlankSocialSec()
        {
            this.EmployeeUserDTO.SocialSec = null;
            this.EmployeeUserDTO.Sex = TermGroup_Sex.Unknown;
            this.hasBlankedSocialSec = true;
        }

        public void SetSocialSec(string socialSec, TermGroup_Sex sex)
        {
            if (!this.hasBlankedSocialSec)
                return;

            this.EmployeeUserDTO.SocialSec = socialSec;
            this.EmployeeUserDTO.Sex = TermGroup_Sex.Unknown;
            this.hasBlankedSocialSec = false;
        }

        public void BlankCardNumber()
        {
            this.EmployeeUserDTO.CardNumber = null;
            this.hasBlankedCardNumber = true;
        }

        public void SetCardNumber(string cardNumber)
        {
            if (!this.hasBlankedCardNumber)
                return;

            this.EmployeeUserDTO.CardNumber = cardNumber;
            this.hasBlankedCardNumber = false;
        }

        public void BlankDisbursement()
        {
            this.EmployeeUserDTO.DisbursementClearingNr = null;
            this.EmployeeUserDTO.DisbursementAccountNr = null;
            this.EmployeeUserDTO.DontValidateDisbursementAccountNr = false;
            this.EmployeeUserDTO.DisbursementCountryCode = null;
            this.EmployeeUserDTO.DisbursementBIC = null;
            this.EmployeeUserDTO.DisbursementIBAN = null;
            this.hasBlankedDisbursement = true;
        }

        public void SetDisbursement(TermGroup_EmployeeDisbursementMethod disbursementMethod, string disbursementClearingNr, string disbursementAccountNr, bool dontValidateDisbursementAccountNr)
        {
            if (!this.hasBlankedDisbursement)
                return;

            this.EmployeeUserDTO.DisbursementClearingNr = disbursementClearingNr;
            this.EmployeeUserDTO.DisbursementAccountNr = disbursementAccountNr;
            this.EmployeeUserDTO.DontValidateDisbursementAccountNr = dontValidateDisbursementAccountNr;
            this.hasBlankedDisbursement = false;
        }

        public void BlankHighRiskProtection()
        {
            this.EmployeeUserDTO.HighRiskProtection = false;
            this.EmployeeUserDTO.HighRiskProtectionTo = null;
            this.hasBlankedHighRiskProtection = true;
        }

        public void SetHighRiskProtection(bool highRiskProtection, DateTime? highRiskProtectionTo)
        {
            if (!this.hasBlankedHighRiskProtection)
                return;

            this.EmployeeUserDTO.HighRiskProtection = highRiskProtection;
            this.EmployeeUserDTO.HighRiskProtectionTo = highRiskProtectionTo;
            this.hasBlankedHighRiskProtection = false;
        }

        public void BlankMedicalCertificateReminder()
        {
            this.EmployeeUserDTO.MedicalCertificateReminder = false;
            this.EmployeeUserDTO.MedicalCertificateDays = null;
            this.hasBlankedMedicalCertificateReminder = true;
        }

        public void BlankMedicalCertificateReminder(bool medicalCertificateReminder, int? medicalCertificateDays)
        {
            if (!this.hasBlankedMedicalCertificateReminder)
                return;

            this.EmployeeUserDTO.MedicalCertificateReminder = medicalCertificateReminder;
            this.EmployeeUserDTO.MedicalCertificateDays = medicalCertificateDays;
            this.hasBlankedMedicalCertificateReminder = false;
        }

        public void BlankNote()
        {
            this.EmployeeUserDTO.Note = null;
            this.hasBlankedNote = true;
            this.EmployeeUserDTO.ShowNote = false;
        }

        public void SetNote(string note, bool showNote)
        {
            if (!this.hasBlankedNote)
                return;

            this.EmployeeUserDTO.Note = note;
            this.EmployeeUserDTO.ShowNote = showNote;
            this.hasBlankedNote = false;
        }

        public void BlankPriceTypes(EmploymentDTO employment)
        {
            if (employment == null)
                return;

            employment.PriceTypes = null;
            this.hasBlankedPriceTypes = true;
        }

        public void SetPriceTypes(EmploymentDTO employment, List<EmploymentPriceTypeDTO> priceTypes)
        {
            if (!this.hasBlankedPriceTypes || employment == null)
                return;

            employment.PriceTypes = priceTypes;
            this.hasBlankedPriceTypes = false;
        }

        public void BlankAccounts(EmploymentDTO employment)
        {
            if (employment == null)
                return;

            employment.CostAccounts = null;
            employment.IncomeAccounts = null;
            employment.Fixed1Accounts = null;
            employment.Fixed2Accounts = null;
            employment.Fixed3Accounts = null;
            employment.Fixed4Accounts = null;
            employment.Fixed5Accounts = null;
            employment.Fixed6Accounts = null;
            employment.Fixed7Accounts = null;
            employment.Fixed8Accounts = null;
            employment.AccountingSettings = null;
            this.hasBlankedAccounts = true;
        }

        public void BlankUnionFees()
        {
            this.EmployeeUserDTO.UnionFees = null;
            this.hasBlankedUnionFees = true;
        }
        public void SetUnionFees(List<EmployeeUnionFeeDTO> unionFees)
        {
            if (!this.hasBlankedUnionFees)
                return;

            this.EmployeeUserDTO.UnionFees = unionFees;
            this.hasBlankedUnionFees = false;
        }
        public void BlankTimeWorkAccounts()
        {
            this.EmployeeUserDTO.EmployeeTimeWorkAccounts = null;
            this.hasBlankedTimeWorkAccounts = true;

        }
        public void SetEmployeeTimeWorkAccount(List<EmployeeTimeWorkAccountDTO> employeeTimeWorkAccounts)
        {
            if (!this.hasBlankedTimeWorkAccounts)
                return;

            this.EmployeeUserDTO.EmployeeTimeWorkAccounts = employeeTimeWorkAccounts;
            this.hasBlankedTimeWorkAccounts = false;
        }
        public void BlankEmployeeVacationSE()
        {
            this.EmployeeUserDTO.EmployeeVacationSE = null;
            this.hasBlankedEmployeeVacationSE = true;
        }

        public void SetEmployeeVacationSE(EmployeeVacationSEDTO employeeVacationSE)
        {
            if (!this.hasBlankedEmployeeVacationSE)
                return;

            this.EmployeeUserDTO.EmployeeVacationSE = employeeVacationSE;
            this.hasBlankedEmployeeVacationSE = false;
        }

        public void BlankChildCare()
        {
            this.EmployeeUserDTO.ChildCares = null;
            this.EmployeeUserDTO.EmployeeChilds = null;
            this.hasBlankedChildCare = true;
        }

        public void SetChildCare(List<EmployeeChildCareDTO> childCares, List<EmployeeChildDTO> employeeChilds)
        {
            if (!this.hasBlankedChildCare)
                return;

            this.EmployeeUserDTO.ChildCares = childCares;
            this.EmployeeUserDTO.EmployeeChilds = employeeChilds;
            this.hasBlankedChildCare = false;
        }

        public void BlankEmployeeSkills()
        {
            this.EmployeeUserDTO.EmployeeSkills = null;
            this.hasBlankedEmployeeSkills = true;
        }

        public void SetmployeeSkills(List<EmployeeSkillDTO> employeeSkills)
        {
            if (!this.hasBlankedEmployeeSkills)
                return;

            this.EmployeeUserDTO.EmployeeSkills = employeeSkills;
            this.hasBlankedEmployeeSkills = false;
        }

        public void BlankTemplateGroups()
        {
            this.EmployeeUserDTO.TemplateGroups = null;
            this.hasBlankedTemplateGroups = true;
        }

        public void SetTemplateGroups(List<TimeScheduleTemplateGroupEmployeeDTO> templateGroups)
        {
            if (!this.hasBlankedTemplateGroups)
                return;

            this.EmployeeUserDTO.TemplateGroups = templateGroups;
            this.hasBlankedTemplateGroups = false;
        }

        public void BlankEmployeeMeetings()
        {
            this.EmployeeUserDTO.EmployeeMeetings = null;
            this.hasBlankedEmployeeMeetings = true;
        }

        public void SetEmployeeMeetings(List<EmployeeMeetingDTO> employeeMeetings)
        {
            if (!this.hasBlankedEmployeeMeetings)
                return;

            this.EmployeeUserDTO.EmployeeMeetings = employeeMeetings;
            this.hasBlankedEmployeeMeetings = false;
        }

        #endregion

        #region Private methods

        private bool ContainsPermissionAndTrue(Feature feature)
        {
            if (this.permissions == null)
                return false;
            return this.permissions.ContainsKey((int)feature) && this.permissions[(int)feature];
        }

        #endregion
    }

    public class DeleteEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public DeleteEmployeeAction Action { get; set; }

        public bool RemoveInfoSalaryDistress { get; set; }
        public bool RemoveInfoUnionFee { get; set; }
        public bool RemoveInfoAbsenceSick { get; set; }
        public bool RemoveInfoAbsenceParentalLeave { get; set; }
        public bool RemoveInfoMeeting { get; set; }
        public bool RemoveInfoNote { get; set; }
        public bool RemoveInfoAddress { get; set; }
        public bool RemoveInfoPhone { get; set; }
        public bool RemoveInfoEmail { get; set; }
        public bool RemoveInfoClosestRelative { get; set; }
        public bool RemoveInfoOtherContactInfo { get; set; }
        public bool RemoveInfoImage { get; set; }
        public bool RemoveInfoBankAccount { get; set; }
        public bool RemoveInfoSkill { get; set; }
    }

    public class DeleteUserDTO
    {
        public int UserId { get; set; }
        public DeleteUserAction Action { get; set; }

        public bool RemoveInfoAddress { get; set; }
        public bool RemoveInfoPhone { get; set; }
        public bool RemoveInfoEmail { get; set; }
        public bool RemoveInfoClosestRelative { get; set; }
        public bool RemoveInfoOtherContactInfo { get; set; }
        public bool DisconnectEmployee { get; set; }
    }

    public class UserRolesDTO
    {
        public int ActorCompanyId { get; set; }
        public bool DefaultCompany { get; set; }
        public string CompanyName { get; set; }
        [TsIgnore]
        public bool IsDeltaChange { get; set; }
        public List<UserCompanyRoleDTO> Roles { get; set; }
        public List<UserAttestRoleDTO> AttestRoles { get; set; }
    }

    public class UserCompanyRoleDTO : IUserCompanyRole
    {
        public int UserCompanyRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ActorCompanyId { get; set; }
        public bool Default { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public SoeEntityState State { get; set; }
        [TsIgnore]
        public int StateId { get { return (int)this.State; } } //Needed for IUserCompanyRole

        // Extensions
        public string Name { get; set; }
        public bool IsModified { get; set; }
        public bool IsDelegated { get; set; }
        public int RoleSort { get; set; }
    }

    public class UserAttestRoleDTO
    {
        public int AttestRoleUserId { get; set; }
        public int? ParentAttestRoleUserId { get; set; }
        public int AttestRoleId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string ModuleName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal MaxAmount { get; set; }
        public int? AccountId { get; set; }
        public int? PrevAccountId { get; set; }
        public string AccountName { get; set; }
        public int? AccountDimId { get; set; }
        public string AccountDimName { get; set; }
        public int? RoleId { get; set; }
        public SoeEntityState State { get; set; }

        public bool IsExecutive { get; set; }
        public bool IsNearestManager { get; set; }
        public TermGroup_AttestRoleUserAccountPermissionType AccountPermissionType { get; set; }
        public string AccountPermissionTypeName { get; set; }
        public List<UserAttestRoleDTO> Children { get; set; }
        public bool IsModified { get; set; }
        public bool IsDelegated { get; set; }
        public int? AttestRoleSort { get; set; }
    }

    public class CompanyRolesDTO
    {
        public int ActorCompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<UserCompanyRoleDTO> Roles { get; set; }
        public List<CompanyAttestRoleDTO> AttestRoles { get; set; }
    }

    public class CompanyAttestRoleDTO
    {
        public int AttestRoleId { get; set; }
        public string Name { get; set; }
        public string ModuleName { get; set; }
        public decimal DefaultMaxAmount { get; set; }
        public bool ShowAllCategories { get; set; }
        public bool ShowUncategorized { get; set; }
        public bool ShowTemplateSchedule { get; set; }
        public bool AlsoAttestAdditionsFromTime { get; set; }
        public bool HumanResourcesPrivacy { get; set; }
        public bool IsExecutive { get; set; }
        public bool IsNearestManager { get; set; }
        public bool AttestByEmployeeAccount { get; set; }
        public bool StaffingByEmployeeAccount { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class EmployeeEmployeerDTO
    {
        public string EmployerRegistrationNumber { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; } = null;
        public bool Deteled { get; set; } = false;
        public SoeEntityState State { get; set; }
    }
}
