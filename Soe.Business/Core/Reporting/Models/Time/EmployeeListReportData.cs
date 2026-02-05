using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class EmployeeListReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeListReportDataInput _reportDataInput;
        private readonly EmployeeListReportDataOutput _reportDataOutput;

        private bool LoadRoles
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.DefaultRole);

            }
        }
        private bool LoadUser
        {
            get
            {
                if (LoadRoles)
                    return true;

                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.UserName ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.IsSysUser ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.IsSysUser);

            }
        }
        private bool LoadContactInfo
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.Email ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.CellPhone ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.HomePhone ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.ClosestRelative ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.DistributionAddress ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.DistributionAddressRow ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.DistributionAddressRow2 ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.DistributionCity ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.DistributionZipCode);
            }
        }
        private bool LoadSalary
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.Salary ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.MonthlySalary ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.HourlySalary);
            }
        }
        private bool LoadEmployments
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.EmploymentDate ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.EndDate ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.LASDays ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.EmploymentTypeName ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.PayrollGroupName ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.EmployeeGroupName ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.VacationGroupName ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.WorkTimeWeekMinutes ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.WorkTimeWeekPercent ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.WorkPlace ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.HasSecondaryEmployment ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.ExternalCode);
            }
        }
        private bool LoadVacationGroup
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_EmployeeListMatrixColumns.VacationGroupName ||
                        a.Column == TermGroup_EmployeeListMatrixColumns.VacationDaysPaidByLaw);
            }
        }
        private bool LoadVacationsDayByLaw
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_EmployeeListMatrixColumns.VacationDaysPaidByLaw);
            }
        }
        private bool LoadSecondaryEmployment
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.HasSecondaryEmployment ||
                       a.Column == TermGroup_EmployeeListMatrixColumns.EmploymentTypeOnSecondaryEmployment);
            }
        }
        private bool LoadAccountInternal
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
            }
        }
        private bool LoadNearestExecutive
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveEmail ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveName ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveUserName ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveSocialSec ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveCellphone);
            }
        }
        private bool LoadNearestExecutiveSocialSec
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveSocialSec);
            }
        }
        private bool LoadNearestExecutiveCellPhone
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_EmployeeListMatrixColumns.NearestExecutiveCellphone);
            }
        }
        private bool LoadCategory
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_EmployeeListMatrixColumns.CategoryName);
            }
        }
        private bool LoadPositions
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                    a.Column == TermGroup_EmployeeListMatrixColumns.SSYKCode ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.SSYKName ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.PositionCode ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.PositionName ||
                    a.Column == TermGroup_EmployeeListMatrixColumns.PositionSysName);
            }
        }
        private bool LoadReportInformation
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                  a.Column == TermGroup_EmployeeListMatrixColumns.PayrollStatisticsPersonalCategory ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.PayrollStatisticsWorkTimeCategory ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.PayrollStatisticsSalaryType ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.PayrollStatisticsWorkPlaceNumber ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.PayrollStatisticsCFARNumber ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.WorkPlaceSCB ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.PartnerInCloseCompany ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BenefitAsPension ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.AFACategory ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.AFASpecialAgreement ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.AFAWorkplaceNr ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.AFAParttimePensionCode ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.CollectumITPPlan ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.CollectumAgreedOnProduct ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.CollectumCostPlace ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.CollectumCancellationDate ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.CollectumCancellationDateIsLeaveOfAbsence ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.KPARetirementAge ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.KPABelonging ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.KPAEndCode ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.KPAAgreementType ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenAgreementArea ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenAllocationNumber ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenMunicipalCode ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenSalaryFormula ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenProfessionCategory ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenSalaryType ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenWorkPlaceNumber ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenLendedToOrgNr ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.BygglosenAgreedHourlyPayLevel ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.GTPAgreementNumber ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.GTPExcluded ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.IFAssociationNumber ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.IFPaymentCode ||
                  a.Column == TermGroup_EmployeeListMatrixColumns.IFWorkPlace);
            }
        }
        private bool LoadExternalAuthId
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                                   a.Column == TermGroup_EmployeeListMatrixColumns.ExternalAuthId);
            }
        }
        private bool LoadLasDays
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeListMatrixColumns.LASDays);
            }
        }
        private bool LoadEmployeeCalculatedCostPerHour
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeListMatrixColumns.EmployeeCalculatedCostPerHour);
            }
        }
        private bool LoadExtraFields
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeListMatrixColumns.ExtraFieldEmployee);
            }
        }
        private bool loadEmploymentTypeSetting
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeListMatrixColumns.EmplymentTypeExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment);
            }
        }

        public EmployeeListReportData(ParameterObject parameterObject, EmployeeListReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeListReportDataOutput(reportDataInput);
        }

        public static List<EmployeeListReportDataReportDataField> GetPossibleDataFields()
        {
            List<EmployeeListReportDataReportDataField> possibleFields = new List<EmployeeListReportDataReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeListMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeListReportDataReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeListReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out _))
                return new ActionResult(false);

            TryGetDateFromSelection(reportResult, out DateTime createdAfter, "createdAfter");
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            if (selectionDateTo == DateTime.MaxValue.Date)
                selectionDateTo = selectionDateFrom;

            DateTime selectionDate = selectionDateFrom.Date == selectionDateTo.Date || selectionDateTo == DateTime.MaxValue ? selectionDateFrom.Date : selectionDateTo.Date;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                if (selectionEmployeeIds.Any())
                {
                    #region Prereq

                    #region Permissions

                    int mySelfEmployeeId = EmployeeManager.GetEmployeeIdForUser(reportResult.ActorCompanyId, reportResult.UserId);

                    bool socialSecPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool notePermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Note, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool employmentPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool disbursementPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollSalaryPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool contactPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool userPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool calculatedCostPerHourMySelfPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool calculatedCostPerHourOtherEmployeesPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool reportsPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);



                    #endregion

                    #region Company settings

                    bool useVacant = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeUseVacant, 0, ActorCompanyId, 0);
                    Company company = CompanyManager.GetCompany(entities, reportResult.ActorCompanyId);
                    List<Company> companiesOnLicense = CompanyManager.GetCompaniesByLicense(entities, company.LicenseId);
                    List<SysLanguageDTO> languages = LanguageManager.GetSysLanguages();
                    List<Role> roles = LoadRoles ? RoleManager.GetAllRolesByCompany(entities, reportResult.ActorCompanyId) : new List<Role>();

                    #endregion

                    #region Terms and dictionaries

                    int langId = GetLangId();
                    Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);
                    Dictionary<int, string> secondaryEmploymentExcludeFromCalculationDict = base.GetTermGroupDict(TermGroup.ExcludeFromWorkTimeWeekCalculationItems, langId);
                    Dictionary<int, string> disbursementMethodsDict = EmployeeManager.GetEmployeeDisbursementMethods(langId, true);
                    Dictionary<int, string> reportsPersonalCategoryDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsSalaryTypeDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsAFACategoryDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsPayrollReportsAFASpecialAgreementDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsPayrollReportsCollectumITPplanDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsWorkTimeCategoryDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsKPABelongingDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsKPAEndCodeDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsKPAAgreementTypeDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsBygglosenSalaryFormulaDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsBygglosenSalaryTypeDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsGTPAgreementNumberDict = new Dictionary<int, string>();
                    Dictionary<int, string> reportsIFPaymentCodeDict = new Dictionary<int, string>();
                    Dictionary<string, List<CompanyCategoryRecord>> categoryRecordsFullKeyDict = new Dictionary<string, List<CompanyCategoryRecord>>();
                    List<AccountDTO> companyAccounts = new List<AccountDTO>();
                    List<CategoryAccount> categoryAccounts = new List<CategoryAccount>();
                    List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                    List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();
                    List<AttestRole> attestRoles = new List<AttestRole>();
                    Dictionary<int, List<GetAttestTransitionLogsForEmployeeResult>> attestTransitionLogDict = null;
                    Dictionary<int, List<TimeScheduleTemplateBlock>> timeScheduleTemplateBlockDict = null;
                    List<User> users = new List<User>();
                    if (LoadCategory && !useAccountHierarchy)
                    {
                        var categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                        categoryRecordsFullKeyDict = CategoryManager.GetCompanyCategoryRecordsFullKeyDict(categoryRecords, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                    }

                    if (LoadExtraFields)
                    {
                        extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();
                    }

                    if (LoadReportInformation && reportsPermission)
                    {
                        reportsPersonalCategoryDict = base.GetTermGroupDict(TermGroup.PayrollReportsPersonalCategory, langId, includeKey: true);
                        reportsSalaryTypeDict = base.GetTermGroupDict(TermGroup.PayrollReportsSalaryType, langId, includeKey: true);
                        reportsAFACategoryDict = base.GetTermGroupDict(TermGroup.PayrollReportsAFACategory, langId, includeKey: true);
                        reportsPayrollReportsAFASpecialAgreementDict = base.GetTermGroupDict(TermGroup.PayrollReportsAFASpecialAgreement, langId, includeKey: true);
                        reportsPayrollReportsCollectumITPplanDict = base.GetTermGroupDict(TermGroup.PayrollReportsCollectumITPplan, langId, includeKey: true);
                        reportsWorkTimeCategoryDict = base.GetTermGroupDict(TermGroup.PayrollReportsWorkTimeCategory, langId, includeKey: true);
                        reportsKPABelongingDict = base.GetTermGroupDict(TermGroup.KPABelonging, langId, includeKey: true);
                        reportsKPAEndCodeDict = base.GetTermGroupDict(TermGroup.KPAEndCode, langId, includeKey: true);
                        reportsKPAAgreementTypeDict = base.GetTermGroupDict(TermGroup.KPAAgreementType, langId, includeKey: true);
                        reportsBygglosenSalaryFormulaDict = PayrollManager.GetPayrollPriceFormulasDict(entities, base.ActorCompanyId, false);
                        reportsBygglosenSalaryTypeDict = base.GetTermGroupDict(TermGroup.BygglosenSalaryType, langId, includeKey: true);
                        reportsGTPAgreementNumberDict = base.GetTermGroupDict(TermGroup.GTPAgreementNumber, langId, includeKey: true);
                        reportsIFPaymentCodeDict = base.GetTermGroupDict(TermGroup.IFPaymentCode, langId, includeKey: true);
                    }

                    if (LoadAccountInternal)
                    {
                        companyAccounts = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs();
                        companyAccounts.ForEach(f => f.ParentAccounts = f.GetParentAccounts(companyAccounts));
                        if (useAccountHierarchy)
                            employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                        else
                            categoryAccounts = base.GetCategoryAccountsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));
                    }

                    if (LoadNearestExecutive)
                    {
                        if (useAccountHierarchy)
                            employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                        var startDate = selectionDate.Date.AddDays(-1);
                        var stopDate = selectionDate.Date.AddDays(1);
                        attestTransitionLogDict = AttestManager.GetLimitedAttestTransitionLogsForEmployees(entities, selectionEmployeeIds, startDate, stopDate);
                        var timeScheduleTemplateBlocks = entities.TimeScheduleTemplateBlock.Where(w => w.EmployeeId.HasValue && selectionEmployeeIds.Contains(w.EmployeeId.Value) && w.Date.HasValue && w.Date >= startDate && w.Date <= stopDate && w.State == (int)SoeEntityState.Active).ToList();
                        timeScheduleTemplateBlockDict = timeScheduleTemplateBlocks.GroupBy(g => g.EmployeeId.Value).ToDictionary(k => k.Key, v => v.ToList());
                    }

                    if (LoadNearestExecutiveSocialSec)
                        attestRoles = AttestManager.GetAttestRolesForUser(entities, base.UserId, reportResult.ActorCompanyId, selectionDate, SoeModule.Time, false);

                    #endregion

                    #region Collections

                    List<EmploymentTypeDTO> employmentTypes = new List<EmploymentTypeDTO>();
                    List<EmploymentTypeDTO> employmentTypesSettingOnly = new List<EmploymentTypeDTO>();
                    List<PayrollGroup> payrollGroups = new List<PayrollGroup>();
                    List<EmployeeGroup> employeeGroups = new List<EmployeeGroup>();
                    List<VacationGroup> vacationGroups = new List<VacationGroup>();
                    List<EmployeePosition> allEmployeePositions = new List<EmployeePosition>();
                    List<Contact> contacts = new List<Contact>();

                    if (employmentPermission && (LoadSalary || LoadEmployments))
                    {
                        employmentTypes = EmployeeManager.GetEmploymentTypes(entities, reportResult.ActorCompanyId, (TermGroup_Languages)langId);
                        employmentTypesSettingOnly = employmentTypes.Where(t => t.SettingOnly).ToList();
                        payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId), loadSettings: LoadSalary);
                        employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                        vacationGroups = GetVacationGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    }
                    if (LoadPositions)
                        allEmployeePositions = EmployeeManager.GetEmployeePositions(entities, selectionEmployeeIds, false);

                    if (LoadUser && userPermission)
                    {
                        List<int> userIds = employees.Where(w => w.UserId.HasValue).Select(s => s.UserId.Value).Distinct().ToList();
                        users = entities.User.Where(w => userIds.Contains(w.UserId)).ToList();
                    }

                    #endregion

                    #endregion

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: LoadEmployments && employmentPermission, loadUser: LoadUser && userPermission, loadContact: LoadContactInfo, loadEmploymentVacactionGroup: LoadEmployments && employmentPermission, loadEmploymentAccounts: LoadAccountInternal);

                    if (createdAfter != CalendarUtility.DATETIME_DEFAULT)
                        employees = employees.Where(w => w.Created.HasValue && w.Created >= createdAfter).ToList();



                    if (LoadContactInfo || contactPermission)
                    {
                        List<int> actorIds = employees.Select(s => s.ContactPerson?.ActorContactPersonId ?? 0).ToList();
                        if (!LoadNearestExecutiveCellPhone)

                            contacts = ContactManager.GetContactsFromActors(entitiesReadOnly, actorIds, loadActor: true, loadAddresses: true);
                        else

                            contacts = ContactManager.GetContactsFromActors(entitiesReadOnly, entities.Employee.Where(w => w.ActorCompanyId == reportResult.ActorCompanyId && w.State == (int)SoeEntityState.Active).Select(s => s.ContactPersonId).ToList(), loadActor: true, loadAddresses: true);
                    }

                    PayrollPriceFormula hourlySalaryFormula = null;
                    PayrollPriceFormula monthlySalaryFormula = null;
                    EvaluatePayrollPriceFormulaInputDTO inputDTO = null;
                    if (LoadSalary)
                    {
                        var hourlySalaryFormulaKey = _reportDataInput.Columns.FirstOrDefault(w => w.Column == TermGroup_EmployeeListMatrixColumns.HourlySalary)?.OptionKey;
                        var monthlySalaryFormulaKey = _reportDataInput.Columns.FirstOrDefault(w => w.Column == TermGroup_EmployeeListMatrixColumns.MonthlySalary)?.OptionKey;

                        if (!string.IsNullOrEmpty(hourlySalaryFormulaKey))
                            hourlySalaryFormula = PayrollManager.GetPayrollPriceFormula(entities, reportResult.ActorCompanyId, hourlySalaryFormulaKey);

                        if (!string.IsNullOrEmpty(monthlySalaryFormulaKey))
                            monthlySalaryFormula = PayrollManager.GetPayrollPriceFormula(entities, reportResult.ActorCompanyId, monthlySalaryFormulaKey);

                        inputDTO = new EvaluatePayrollPriceFormulaInputDTO()
                        {

                            SysCountryId = base.GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId),

                            EmployeeGroups = employeeGroups ?? base.GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                            PayrollGroups = payrollGroups ?? base.GetPayrollGroupsFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                            PayrollPriceTypes = base.GetPayrollPriceTypesFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                            EmploymentPriceTypesDict = base.GetEmploymentPriceTypeFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId), selectionEmployeeIds).GroupBy(k => k.EmploymentId).ToDictionary(k => k.Key, v => v.ToList()),
                            PayrollGroupPriceTypes = base.GetPayrollGroupPriceTypesForCompanyFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                            PayrollProductPriceTypes = base.GetPayrollProductPriceTypesForCompanyFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                            PayrollProductPriceFormulas = ProductManager.GetPayrollProductPriceFormulas(entitiesReadOnly, this.ActorCompanyId),
                            PayrollPriceFormulas = base.GetPayrollPriceFormulasFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                            SysPayrollPriceViews = SysDbCache.Instance.SysPayrollPriceViewDTOs,
                            TimePeriods = TimePeriodManager.GetTimePeriods(entitiesReadOnly, TermGroup_TimePeriodType.Payroll, this.ActorCompanyId, addTimePeriodHeadName: false),
                            EmployeeFactorsDict = base.GetEmployeeFactorsFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId)).GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList()),
                            PayrollProducts = base.GetPayrollProductsFromCache(entitiesReadOnly, CacheConfig.Company(this.ActorCompanyId)),
                        };
                    }
                    Guid idProviderGuid = Guid.Empty;
                    if (LoadExternalAuthId)
                    {

                        string value = SettingManager.GetStringSetting(SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, company.LicenseId);
                        if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out idProviderGuid)) { }
                    }

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;
                        List<CompanyCategoryRecord> employeeCategoryRecords = null;
                        List<Employment> employments = employee.GetActiveEmploymentsDesc(includeSecondary: LoadSecondaryEmployment);
                        if (employments.IsNullOrEmpty())
                            continue;

                        Employment firstEmployment = employments.GetFirstEmployment();
                        Employment lastEmployment = employments.GetLastEmployment();
                        Employment currentEmployment = employments.GetEmployment(selectionDate) ?? firstEmployment.GetNearestEmployment(lastEmployment, selectionDate);
                        Employment secondaryEmployment = employments.GetSecondaryEmployment(selectionDate);
                        if (currentEmployment == null)
                            continue;

                        EmployeeListItem employeeItem = new EmployeeListItem();

                        #region Accounting

                        if (LoadAccountInternal)
                        {
                            string key = useAccountHierarchy ? string.Empty : CompanyCategoryRecord.ConstructKey((int)SoeCategoryRecordEntity.Employee, employee.EmployeeId);
                            if (!string.IsNullOrEmpty(key) && categoryRecordsFullKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records))
                                employeeCategoryRecords = records.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true);

                            employeeItem.AccountAnalysisFields = currentEmployment.AccountAnalysisFields(employeeAccounts.FindAll(r => r.EmployeeId == employee.EmployeeId), employeeCategoryRecords, categoryAccounts, companyAccounts, selectionDateFrom, selectionDateTo);
                        }

                        #endregion

                        #region Permissions

                        bool calculatedCostPerHourPermission = (employee.EmployeeId == mySelfEmployeeId && calculatedCostPerHourMySelfPermission) ||
                            (employee.EmployeeId != mySelfEmployeeId && calculatedCostPerHourOtherEmployeesPermission);

                        #endregion

                        #region Salary

                        if (LoadSalary)
                        {
                            var salaryType = EmployeeManager.GetEmployeeSalaryType(employee, selectionDateFrom, selectionDateTo);
                            var payrollGroup = currentEmployment.GetPayrollGroup(selectionDateTo, payrollGroups);
                            if (payrollGroup != null)
                            {
                                if (hourlySalaryFormula != null && salaryType == TermGroup_PayrollExportSalaryType.Hourly)
                                {
                                    employeeItem.Salary = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, currentEmployment, null, selectionDateTo, null, null, hourlySalaryFormula.PayrollPriceFormulaId, null, inputDTO)?.Amount ?? 0;
                                    employeeItem.HourlySalary = employeeItem.Salary;
                                }
                                else if (monthlySalaryFormula != null && salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                                {
                                    employeeItem.Salary = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, currentEmployment, null, selectionDateTo, null, null, monthlySalaryFormula.PayrollPriceFormulaId, null, inputDTO)?.Amount ?? 0;
                                    employeeItem.MonthlySalary = employeeItem.Salary;
                                }
                                else
                                {
                                    var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                                    if (setting != null)
                                    {
                                        employeeItem.Salary = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, currentEmployment, null, selectionDateTo, null, null, setting.IntData, null, inputDTO)?.Amount ?? 0;

                                        if (salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                                            employeeItem.MonthlySalary = employeeItem.Salary;
                                        else if (salaryType == TermGroup_PayrollExportSalaryType.Hourly)
                                            employeeItem.HourlySalary = employeeItem.Salary;
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Content
                        User user = null;

                        if (LoadUser && userPermission && !employee.UserReference.IsLoaded)
                        {
                            user = users.FirstOrDefault(f => f.UserId == employee.UserId);

                            if (user == null)
                            {
                                employee.UserReference.Load();

                                if (employee.User != null)
                                    user = employee.User;
                            }
                        }

                        Company defaultCompany = user != null && user.DefaultActorCompanyId.HasValue ? companiesOnLicense.FirstOrDefault(f => f.ActorCompanyId == user.DefaultActorCompanyId.Value) ?? CompanyManager.GetCompany(entities, user.DefaultActorCompanyId.Value) : null;
                        SysLanguageDTO sysLanguage = user != null && user.LangId.HasValue ? languages.FirstOrDefault(f => f.SysLanguageId == user.LangId) ?? LanguageManager.GetSysLanguage(user.LangId.Value) : null;

                        List<ContactAddressItem> contactItems = LoadContactInfo && contactPermission ? ContactManager.GetContactAddressItems(entities, employee.ContactPerson.ActorContactPersonId, preLoadedContacts: contacts) : new List<ContactAddressItem>();
                        // TODO: Check permission for seeing secret addresses
                        // For now, do not show them for anyone
                        if (!contactPermission)
                            contactItems = contactItems.Where(c => !c.IsSecret).ToList();

                        EmployeePosition defaultEmployeePosition = null;
                        if (LoadPositions)
                        {
                            List<EmployeePosition> employeePositions = allEmployeePositions.Where(w => w.EmployeeId == employeeId).ToList();
                            defaultEmployeePosition = employeePositions.FirstOrDefault(f => f.Default);
                            employeeItem.Position = defaultEmployeePosition?.Position;
                        }

                        Dictionary<int, string> yesNoDictionary = base.GetTermGroupDict(TermGroup.YesNo, base.GetLangId());
                        if (LoadExtraFields && extraFieldRecords.Any())
                        {
                            var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == employeeId).ToList();

                            foreach (var column in _reportDataInput.Columns.Where(w => w.Column == TermGroup_EmployeeListMatrixColumns.ExtraFieldEmployee))
                            {
                                if (column.Selection?.Options?.Key != null && int.TryParse(column.Selection.Options.Key, out int recordId))
                                {
                                    var matchOnEmployee = extraFieldRecordsOnEmployee.FirstOrDefault(f => f.ExtraFieldId == recordId);
                                    if (matchOnEmployee != null)
                                    {
                                        employeeItem.ExtraFieldAnalysisFields.Add(new ExtraFieldAnalysisField(matchOnEmployee, yesNoDictionary));
                                        continue;
                                    }
                                }
                                employeeItem.ExtraFieldAnalysisFields.Add(new ExtraFieldAnalysisField(null));
                            }
                        }

                        if (LoadCategory)
                        {
                            string key = CompanyCategoryRecord.ConstructKey((int)SoeCategoryRecordEntity.Employee, employee.EmployeeId);
                            if (employeeCategoryRecords.IsNullOrEmpty() && categoryRecordsFullKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records))
                                employeeCategoryRecords = records.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true);
                        }

                        if (LoadNearestExecutive)
                        {
                            List<GetAttestTransitionLogsForEmployeeResult> attestTransitions = attestTransitionLogDict != null && attestTransitionLogDict.TryGetValue(employeeId, out var transitions) ? transitions : new List<GetAttestTransitionLogsForEmployeeResult>();
                            List<TimeScheduleTemplateBlock> timeScheduleBlocks = timeScheduleTemplateBlockDict != null && timeScheduleTemplateBlockDict.TryGetValue(employeeId, out var blocks) ? blocks : new List<TimeScheduleTemplateBlock>();

                            UserDTO nearest = UserManager.GetEmployeeNearestExecutive(entities, employee, selectionDate, selectionDate, reportResult.ActorCompanyId, employeeAccounts.IsNullOrEmpty() ? null : employeeAccounts, attestTransitions, timeScheduleBlocks);

                            if (nearest != null)
                            {
                                employeeItem.NearestExecutiveEmail = nearest.Email;
                                employeeItem.NearestExecutiveUserName = nearest.LoginName;
                                employeeItem.NearestExecutiveName = nearest.Name;
                                employeeItem.NearestExecutiveCellPhone = "";

                                if (LoadNearestExecutiveSocialSec && socialSecPermission)
                                {
                                    var matchingEmployee = employees.FirstOrDefault(w => w.UserId == nearest.UserId && w.UserId != employee.UserId);

                                    if (matchingEmployee != null)
                                    {
                                        employeeItem.NearestExecutiveSocialSec = matchingEmployee.SocialSec;
                                    }
                                    else
                                    {
                                        if (attestRoles.Any(a => a.ShowAllCategories))
                                        {
                                            var matchingEmployeeUser = entities.Employee.Include("ContactPerson").FirstOrDefault(w => w.UserId == nearest.UserId && w.ActorCompanyId == reportResult.ActorCompanyId && w.UserId != employee.UserId);

                                            if (matchingEmployeeUser?.ContactPerson != null)
                                                employeeItem.NearestExecutiveSocialSec = matchingEmployeeUser.ContactPerson.SocialSec;
                                            else
                                            {
                                                var matchingUser = entities.User.Include("ContactPerson").FirstOrDefault(f => f.UserId == nearest.UserId && f.UserId != employee.UserId);

                                                if (matchingUser?.ContactPerson != null)
                                                    employeeItem.NearestExecutiveSocialSec = matchingUser.ContactPerson.SocialSec;
                                            }

                                        }
                                    }
                                }

                                if (LoadNearestExecutiveCellPhone)
                                {
                                    var matchingEmployee = employees.FirstOrDefault(w => w.UserId == nearest.UserId && w.UserId != employee.UserId);

                                    if (matchingEmployee == null && attestRoles.Any(a => a.ShowAllCategories))
                                        matchingEmployee = entities.Employee.FirstOrDefault(w => w.UserId == nearest.UserId && w.ActorCompanyId == reportResult.ActorCompanyId && w.UserId != employee.UserId);

                                    if (matchingEmployee?.ContactPerson != null)
                                    {
                                        var executiveContactItems = ContactManager.GetContactAddressItems(entities, matchingEmployee.ContactPersonId, preLoadedContacts: contacts);
                                        employeeItem.NearestExecutiveCellPhone = executiveContactItems?.Where(w => !w.IsAddress && w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile)?.Select(s => s.DisplayAddress)?.JoinToString<string>(", ") ?? String.Empty;
                                    }
                                }
                            }
                        }
                        #endregion

                        employeeItem.EmployeeId = employee.EmployeeId;
                        employeeItem.EmployeeNr = employee.EmployeeNr;
                        employeeItem.EmployeeName = employee.Name;
                        employeeItem.FirstName = employee.FirstName;
                        employeeItem.LastName = employee.LastName;
                        employeeItem.EmployeeExternalCode = employee.ExternalCode;
                        employeeItem.SocialSec = socialSecPermission ? employee.SocialSec : string.Empty;
                        employeeItem.Gender = GetValueFromDict((int)employee.Sex, sexDict);
                        employeeItem.Age = EmployeeManager.GetEmployeeAge(employee);
                        employeeItem.BirthDate = EmployeeManager.GetEmployeeBirthDate(employee);
                        if (payrollPermission && payrollSalaryPermission)
                            employeeItem.ExcludeFromPayroll = employee.ExcludeFromPayroll;
                        if (useVacant)
                            employeeItem.Vacant = employee.Vacant;

                        if (LoadExternalAuthId && idProviderGuid != Guid.Empty && user.idLoginGuid.HasValue && user.idLoginGuid != Guid.Empty)
                            employeeItem.ExternalAuthId = SoftOneIdConnector.GetExternalAuthId(user.idLoginGuid.Value, idProviderGuid);

                        if (LoadContactInfo && contactPermission)
                        {
                            employeeItem.DistributionAddress = contactItems?.Where(w => w.IsAddress && w.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution)?.Select(s => s.DisplayAddress)?.JoinToString<string>("; ") ?? String.Empty;
                            employeeItem.Email = contactItems?.Where(w => !w.IsAddress && w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)?.Select(s => s.DisplayAddress)?.JoinToString<string>(", ") ?? String.Empty;
                            employeeItem.CellPhone = contactItems?.Where(w => !w.IsAddress && w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile)?.Select(s => s.DisplayAddress)?.JoinToString<string>(", ") ?? String.Empty;
                            employeeItem.HomePhone = contactItems?.Where(w => !w.IsAddress && w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneHome)?.Select(s => s.DisplayAddress)?.JoinToString<string>(", ") ?? String.Empty;
                            employeeItem.ClosestRelative = contactItems?.Where(w => !w.IsAddress && w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.ClosestRelative)?.Select(s => s.DisplayAddress)?.JoinToString<string>("; ") ?? String.Empty;

                            employeeItem.AddressRow = contactItems.FirstOrDefault(f => f.IsAddress)?.Address ?? "";
                            employeeItem.AddressRow2 = contactItems.FirstOrDefault(f => f.IsAddress)?.AddressCO ?? "";
                            employeeItem.ZipCode = contactItems.FirstOrDefault(f => f.IsAddress)?.PostalCode ?? "";
                            employeeItem.City = contactItems.FirstOrDefault(f => f.IsAddress)?.PostalAddress ?? "";
                        }

                        if (userPermission)
                        {
                            employeeItem.UserName = user?.LoginName;
                            employeeItem.Language = sysLanguage?.Name ?? string.Empty;
                            employeeItem.DefaultCompany = defaultCompany?.Name ?? string.Empty;

                            Role defaultRole = null;
                            if (LoadRoles && user != null)
                                defaultRole = roles.FirstOrDefault(f => f.RoleId == user.DefaultRoleId) ?? RoleManager.GetRole(entities, user.DefaultRoleId, reportResult.ActorCompanyId);
                            employeeItem.DefaultRole = defaultRole != null ? defaultRole.Name : string.Empty;
                            employeeItem.IsMobileUser = user?.IsMobileUser ?? false;
                            employeeItem.IsSysUser = user?.SysUser ?? false;
                        }

                        if (calculatedCostPerHourPermission && LoadEmployeeCalculatedCostPerHour)
                            employeeItem.EmployeeCalculatedCostPerHour = EmployeeManager.GetEmployeeCalculatedCost(entities, employee, selectionDate, null);

                        if (notePermission)
                            employeeItem.Note = employee.Note;

                        if (LoadEmployments && employmentPermission)
                        {
                            employeeItem.EmploymentDate = currentEmployment.DateFrom.Value;
                            employeeItem.FirstEmploymentDate = employments.OrderBy(o => o.DateFrom.Value).First().DateFrom.Value;
                            employeeItem.EndDate = currentEmployment.DateTo;
                            if (LoadLasDays)
                                employeeItem.LASDays = EmployeeManager.GetLasDays(entities, reportResult.ActorCompanyId, employee, selectionDate);
                            employeeItem.EmploymentTypeName = currentEmployment.GetEmploymentTypeName(employmentTypes, selectionDate);
                            employeeItem.PayrollGroupName = currentEmployment.GetPayrollGroup(selectionDate, payrollGroups)?.Name;
                            employeeItem.EmployeeGroupName = employee.GetEmployeeGroup(selectionDate, employeeGroups: employeeGroups)?.Name;
                            employeeItem.VacationGroupName = LoadVacationGroup ? currentEmployment.GetCurrentVacationGroup(selectionDate, vacationGroups)?.Name : string.Empty;
                            employeeItem.WorkTimeWeekMinutes = currentEmployment.GetWorkTimeWeek(selectionDate);
                            employeeItem.WorkTimeWeekPercent = currentEmployment.GetPercent(selectionDate);
                            employeeItem.WorkPlace = currentEmployment.GetWorkPlace(selectionDate);
                            employeeItem.EmploymentExternalCode = currentEmployment.GetExternalCode(selectionDate);
                            if (loadEmploymentTypeSetting && secondaryEmployment != null)
                            {
                                bool excludeFromCalculationEmploymentType = employmentTypesSettingOnly.FirstOrDefault(t => t.Type == secondaryEmployment.GetEmploymentType(selectionDate))?.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment ?? false;
                                employeeItem.SecondaryEmploymentExcludeFromWorkTimeWeekCalculationEmploymentType = GetSecondaryEmploymentExcludeFromCalculationValueFromDict(excludeFromCalculationEmploymentType, secondaryEmploymentExcludeFromCalculationDict);
                            }
                            if (LoadSecondaryEmployment && secondaryEmployment != null)
                            {
                                employeeItem.HasSecondaryEmployment = true;
                                bool? excludeFromCalculation = secondaryEmployment.GetExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(selectionDate);
                                employeeItem.SecondaryEmploymentExcludeFromWorkTimeWeekCalculation = GetSecondaryEmploymentExcludeFromCalculationValueFromDict(excludeFromCalculation, secondaryEmploymentExcludeFromCalculationDict);
                                employeeItem.EmploymentTypeOnSecondaryEmployment = secondaryEmployment.GetEmploymentTypeName(employmentTypes, selectionDate);
                            }
                        }
                        if (LoadVacationsDayByLaw)
                            employeeItem.VacationDaysPaidByLaw = PayrollManager.GetVacationDaysPaidByLaw(entities, ActorCompanyId, employee, currentEmployment.GetCurrentVacationGroup(selectionDate, vacationGroups), selectionDate)?.Value ?? 0;

                        if (disbursementPermission)
                        {
                            employeeItem.DisbursementMethodText = GetValueFromDict(employee.DisbursementMethod, disbursementMethodsDict);
                            employeeItem.DisbursementClearingNr = employee.DisbursementClearingNr;
                            employeeItem.DisbursementAccountNr = employee.DisbursementAccountNr;
                            employeeItem.DisbursementCountryCode = employee.DisbursementCountryCode;
                            employeeItem.DisbursementIBAN = employee.DisbursementIBAN;
                            employeeItem.DisbursementBIC = employee.DisbursementBIC;
                        }

                        employeeItem.SSYKCode = defaultEmployeePosition?.Position?.SysPositionCode ?? string.Empty;
                        employeeItem.SSYKName = defaultEmployeePosition?.Position?.Name ?? string.Empty;

                        if (reportsPermission)
                        {
                            employeeItem.PayrollStatisticsPersonalCategory = GetValueFromDict(employee.PayrollStatisticsPersonalCategory, reportsPersonalCategoryDict);
                            employeeItem.PayrollStatisticsWorkTimeCategory = GetValueFromDict(employee.PayrollStatisticsWorkTimeCategory, reportsWorkTimeCategoryDict);
                            employeeItem.PayrollStatisticsSalaryType = GetValueFromDict(employee.PayrollStatisticsSalaryType, reportsSalaryTypeDict);
                            employeeItem.PayrollStatisticsWorkPlaceNumber = employee.PayrollStatisticsWorkPlaceNumber?.ToString() ?? string.Empty;
                            employeeItem.PayrollStatisticsCFARNumber = employee.PayrollStatisticsCFARNumber?.ToString() ?? string.Empty;
                            employeeItem.WorkPlaceSCB = employee.WorkPlaceSCB;
                            employeeItem.PartnerInCloseCompany = employee.PartnerInCloseCompany;
                            employeeItem.BenefitAsPension = employee.BenefitAsPension;
                            employeeItem.AFACategory = GetValueFromDict(employee.AFACategory, reportsAFACategoryDict);
                            employeeItem.AFASpecialAgreement = GetValueFromDict(employee.AFASpecialAgreement, reportsPayrollReportsAFASpecialAgreementDict);
                            employeeItem.AFAWorkplaceNr = employee.AFAWorkplaceNr;
                            employeeItem.AFAParttimePensionCode = employee.AFAParttimePensionCode;
                            employeeItem.CollectumITPPlan = GetValueFromDict(employee.CollectumITPPlan, reportsPayrollReportsCollectumITPplanDict);
                            employeeItem.CollectumCostPlace = employee.CollectumCostPlace;
                            employeeItem.CollectumAgreedOnProduct = employee.CollectumAgreedOnProduct;
                            employeeItem.CollectumCancellationDate = employee.CollectumCancellationDate;
                            employeeItem.CollectumCancellationDateIsLeaveOfAbsence = employee.CollectumCancellationDateIsLeaveOfAbsence;
                            employeeItem.KPARetirementAge = employee.KPARetirementAge ?? 0;
                            employeeItem.KPABelonging = GetValueFromDict(employee.KPABelonging, reportsKPABelongingDict);
                            employeeItem.KPAEndCode = GetValueFromDict(employee.KPAEndCode, reportsKPAEndCodeDict);
                            employeeItem.KPAAgreementType = GetValueFromDict(employee.KPAAgreementType, reportsKPAAgreementTypeDict);
                            employeeItem.BygglosenAgreementArea = employee.BygglosenAgreementArea;
                            employeeItem.BygglosenAllocationNumber = employee.BygglosenAllocationNumber;
                            employeeItem.BygglosenMunicipalCode = employee.BygglosenMunicipalCode;
                            employeeItem.BygglosenSalaryFormula = GetValueFromDict(employee.BygglosenSalaryFormula, reportsBygglosenSalaryFormulaDict);
                            employeeItem.BygglosenSalaryType = GetValueFromDict(employee.BygglosenSalaryType, reportsBygglosenSalaryTypeDict);
                            employeeItem.BygglosenProfessionCategory = employee.BygglosenProfessionCategory;
                            employeeItem.BygglosenWorkPlaceNumber = employee.BygglosenWorkPlaceNumber;
                            employeeItem.BygglosenLendedToOrgNr = employee.BygglosenLendedToOrgNr;
                            employeeItem.BygglosenAgreedHourlyPayLevel = employee.BygglosenAgreedHourlyPayLevel ?? decimal.Zero;
                            employeeItem.GTPAgreementNumber = GetValueFromDict(employee.GTPAgreementNumber, reportsGTPAgreementNumberDict);
                            employeeItem.GTPExcluded = employee.GTPExcluded;
                            employeeItem.IFPaymentCode = GetValueFromDict(employee.IFPaymentCode, reportsIFPaymentCodeDict);
                            employeeItem.IFAssociationNumber = employee.IFAssociationNumber;
                            employeeItem.IFWorkPlace = employee.IFWorkPlace != "" ? employee.IFWorkPlace : string.Empty;
                            employeeItem.AGIPlaceOfEmploymentAddress = employee.AGIPlaceOfEmploymentAddress;
                            employeeItem.AGIPlaceOfEmploymentCity = employee.AGIPlaceOfEmploymentCity;
                            employeeItem.AGIPlaceOfEmploymentIgnore = employee.AGIPlaceOfEmploymentIgnore;
                        }

                        employeeItem.Created = employee.Created;
                        employeeItem.CreatedBy = employee.CreatedBy;
                        employeeItem.Modified = employee.Modified;
                        employeeItem.ModifiedBy = employee.ModifiedBy;

                        if (LoadCategory && !employeeCategoryRecords.IsNullOrEmpty())
                            employeeItem.CategoryName = employeeCategoryRecords.GetCategoryNamesString();

                        _reportDataOutput.Employees.Add(employeeItem);
                        #endregion
                    }
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.IsNullOrEmpty())
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);
            return value ?? string.Empty;
        }

        private string GetSecondaryEmploymentExcludeFromCalculationValueFromDict(bool? excludeFromCalculation, Dictionary<int, string> dict)
        {
            string outValue = string.Empty;
            switch (excludeFromCalculation)
            {
                case null:
                    dict.TryGetValue((int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.UseSettingOnEmploymentType, out outValue);
                    break;
                case true:
                    dict.TryGetValue((int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.Yes, out outValue);
                    break;
                case false:
                    dict.TryGetValue((int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.No, out outValue);
                    break;
            }
            return outValue;
        }
    }

    public class EmployeeListReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeListMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }
        public string OptionKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeListReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.OptionKey = Selection?.Options?.Key;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeListMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_EmployeeListMatrixColumns.Unknown;
        }
    }

    public class EmployeeListReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AccountDimDTO> AccountDims { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        public List<EmployeeListReportDataReportDataField> Columns { get; set; }

        public EmployeeListReportDataInput(CreateReportResult reportResult, List<EmployeeListReportDataReportDataField> columns, List<AccountDimDTO> accountDims, List<AccountDTO> accountInternals = null)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.AccountDims = accountDims;
            this.AccountInternals = accountInternals;
        }
    }

    public class EmployeeListReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeListItem> Employees { get; set; }
        public EmployeeListReportDataInput Input { get; set; }

        public EmployeeListReportDataOutput(EmployeeListReportDataInput input)
        {
            this.Employees = new List<EmployeeListItem>();
            this.Input = input;
        }
    }
}