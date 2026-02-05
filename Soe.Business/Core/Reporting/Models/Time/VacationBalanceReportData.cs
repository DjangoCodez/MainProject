using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class VacationBalanceReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly VacationBalanceReportDataInput _reportDataInput;
        private readonly VacationBalanceReportDataOutput _reportDataOutput;

        private DateTime selectionDateFrom, selectionDateTo;

        public VacationBalanceReportData(ParameterObject parameterObject, VacationBalanceReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new VacationBalanceReportDataOutput(reportDataInput);
        }

        public VacationBalanceReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private bool loadAccountInternal => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
        private bool loadCategories => _reportDataInput.Columns.Any(a => a.Column == TermGroup_VacationBalanceMatrixColumns.Categories);
        private bool loadExtraFields => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("xtraField"));

        public ActionResult LoadData()
        {
            ActionResult result = new ActionResult();

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            if (!TryGetDatesFromSelection(reportResult, out selectionDateFrom, out selectionDateTo))
                return new ActionResult(true);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(true);

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            var selectionCategoryIds = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees")?.CategoryIds;
            var includePrelUsedDays = true;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Build Data

            using (CompEntities entities = new CompEntities())
            {
                int langId = GetLangId();
                Dictionary<int, string> fieldTypesDict = base.GetTermGroupDict(TermGroup.EmploymentChangeFieldType, langId);
                Dictionary<int, string> payrollPriceTypesDict = PayrollManager.GetPayrollPriceTypesDict(reportResult.ActorCompanyId, null);
                Dictionary<int, string> employmentEndReasonsDict = EmployeeManager.GetSystemEndReasons(reportResult.ActorCompanyId, includeCompanyEndReasons: true);
                List<EmploymentTypeDTO> employmentTypeList = EmployeeManager.GetEmploymentTypes(reportResult.ActorCompanyId, (TermGroup_Languages)langId);
                List<EmployeeGroupDTO> employeeGroups = EmployeeManager.GetEmployeeGroups(reportResult.ActorCompanyId, onlyActive: false).ToDTOs().ToList();
                List<PayrollGroupDTO> payrollGroups = PayrollManager.GetPayrollGroups(reportResult.ActorCompanyId, onlyActive: true).ToDTOs().ToList();
                List<AnnualLeaveGroupDTO> annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(reportResult.ActorCompanyId).ToDTOs().ToList();
                Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);

                using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);
                List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
                List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();

                List<AccountDTO> companyAccountsDTO = new List<AccountDTO>();
                if (!useAccountHierarchy)
                {
                    companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, base.ActorCompanyId);
                }

                if (loadAccountInternal)
                {
                    companyAccountsDTO = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs();
                    companyAccountsDTO.ForEach(f => f.ParentAccounts = f.GetParentAccounts(companyAccountsDTO));
                }

                if (useAccountHierarchy)
                    employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true, loadEmploymentPriceType: true);

                if (loadExtraFields)
                {
                    extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(entities, selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();
                }

                #region Content

                foreach (var employee in employees)
                {
                    string gender = GetValueFromDict((int)employee.Sex, sexDict);
                    var age = CalendarUtility.GetYearsBetweenDates(EmployeeManager.GetEmployeeBirthDate(employee).ToValueOrToday(), selectionDateTo);
                    var birthMonth = employee.SocialSec != "" ? employee.SocialSec.Substring(0, 4) + '-' + employee.SocialSec.Substring(4, 2) : string.Empty;
                    var employmentsEntities = employee.GetEmployments(selectionDateFrom, selectionDateTo, includeSecondary: false);
                    List<EmploymentDTO> employments = SetUpEmploymentDTO(employmentsEntities, fieldTypesDict, employeeGroups, payrollGroups, employmentTypeList, employmentEndReasonsDict, payrollPriceTypesDict, annualLeaveGroups);

                    EmploymentDTO lastEmployment = employments.OrderBy(o => o.DateFrom).LastOrDefault();
                    var userRoles = employee.UserId.HasValue ? UserManager.GetUserRolesDTO(entities, employee.UserId.Value, false) : new List<UserRolesDTO>();

                    if (lastEmployment != null && !employments.Any(e => e.EmploymentId == lastEmployment.EmploymentId))
                    {
                        employments.Add(lastEmployment);
                        employments = employments.OrderBy(o => o.DateFrom).ToList();
                    }
                    string employeeCategoryString = null;
                    string employeeRolesString = null;
                    var extraFields = GetExtraFieldAnalysisField(extraFieldRecords, employee.EmployeeId);
                    if (!useAccountHierarchy && loadCategories)
                    {
                        var selectedEmployeeCategoryRecords = companyCategoryRecords.Where(r => r.RecordId == employee.EmployeeId && r.Default) ?? null;
                        if (!selectedEmployeeCategoryRecords.IsNullOrEmpty())
                            employeeCategoryString = string.Join(", ", selectedEmployeeCategoryRecords.Select(t => t.Category?.Name ?? string.Empty));
                    }

                    if (!userRoles.IsNullOrEmpty())
                    {
                        var companyRole = userRoles.FirstOrDefault(s => s.ActorCompanyId == reportResult.ActorCompanyId);
                        employeeRolesString = string.Join(", ", companyRole?.Roles.Select(t => t.Name ?? string.Empty));
                    }

                    foreach (var employeeEmployment in employments)
                    {

                        #region EmployeeVacation

                        EmployeeVacationSE employeeVacationSE = null;
                        List<EmployeeTimePeriod> employeeTimePeriods = null;
                        var userrrId = employee.UserId;
                        employeeVacationSE = employeeVacationSE ?? EmployeeManager.GetLatestEmployeeVacationSE(employee.EmployeeId, null);
                        decimal preliminaryUsedDays = includePrelUsedDays ? PayrollManager.GetEmployeeVacationPrelUsedDays(reportResult.ActorCompanyId, employee.EmployeeId, DateTime.Today, employee: employee, employeeTimePeriods: employeeTimePeriods, lastEmployeeVacation: employeeVacationSE).Sum : 0;
                        preliminaryUsedDays = -preliminaryUsedDays;

                        #endregion

                        if (useAccountHierarchy)
                        {
                            var connectedToAccounts = employeeAccounts.Where(r => r.EmployeeId == employee.EmployeeId && r.Default).ToList();
                            var isAllAccountHasParent = false;
                            if (!connectedToAccounts.IsNullOrEmpty())
                            {
                                var connectedAccountIds = connectedToAccounts.Select(s => s.AccountId).ToList();
                                var accountDTOParents = companyAccountsDTO.Where(a => connectedAccountIds.Contains(a.AccountId)).Select(s => s.ParentAccounts).ToList();
                                if (accountDTOParents.Count > 0 && accountDTOParents.TrueForAll(a => a.Count > 0))
                                    isAllAccountHasParent = true;

                                var connectedToAccountVacationBalanceItemList = new List<VacationBalanceItem>();
                                foreach (var connectedToAccount in connectedToAccounts)
                                {
                                    #region Vacation Balance Item

                                    var vacationBalanceItem = SetUpVacationBalanceItem(employee, birthMonth, age, gender, employeeRolesString, employeeCategoryString, employeeEmployment, employmentsEntities, employeeVacationSE, preliminaryUsedDays, connectedToAccount.AccountId.Value, extraFields, companyAccountsDTO);
                                    connectedToAccountVacationBalanceItemList.Add(vacationBalanceItem);

                                    #endregion
                                }
                                if (!isAllAccountHasParent)
                                {
                                    if (connectedToAccounts.Count > 1)
                                    {
                                        //Use for Hierarchy Exist in Stora Coop or not have account dims
                                        _reportDataOutput.VacationBalanceItems.Add(SetupDistinctOrganisationHrItemRow(connectedToAccountVacationBalanceItemList));
                                    }

                                    //when not select account dims
                                    if (connectedToAccounts.Count == 1 && !loadAccountInternal && companyAccountsDTO.Count == 0)
                                        _reportDataOutput.VacationBalanceItems.AddRange(connectedToAccountVacationBalanceItemList);

                                }
                                else
                                {
                                    _reportDataOutput.VacationBalanceItems.AddRange(connectedToAccountVacationBalanceItemList);
                                }
                            }

                        }
                        else
                        {
                            if (loadCategories)
                            {
                                var selectedEmployeeCategoryRecords = companyCategoryRecords.Where(r => r.RecordId == employee.EmployeeId && r.Default) ?? null; //Category

                                //If category filter is applied
                                if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeCategory && !selectionCategoryIds.IsNullOrEmpty())
                                {
                                    selectedEmployeeCategoryRecords = selectedEmployeeCategoryRecords.Where(r => selectionCategoryIds.Contains(r.CategoryId)).ToList();
                                }
                                if (!selectedEmployeeCategoryRecords.IsNullOrEmpty())
                                    employeeCategoryString = string.Join(", ", selectedEmployeeCategoryRecords.Select(t => t.Category?.Name ?? string.Empty));
                            }

                            #region ------ Create item ------

                            var vacationBalanceItem = SetUpVacationBalanceItem(employee, birthMonth, age, gender, employeeRolesString, employeeCategoryString, employeeEmployment, employmentsEntities, employeeVacationSE, preliminaryUsedDays, null, extraFields, companyAccountsDTO);

                            _reportDataOutput.VacationBalanceItems.Add(vacationBalanceItem);

                            #endregion ------ Create item ------

                        }
                    }
                }

                #endregion
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        public List<EmploymentDTO> SetUpEmploymentDTO(List<Employment> employmentsEntities, Dictionary<int, string> fieldTypesDict, List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<EmploymentTypeDTO> employmentTypeList, Dictionary<int, string> employmentEndReasonsDict, Dictionary<int, string> payrollPriceTypesDict, List<AnnualLeaveGroupDTO> annualLeaveGroups)
        {
            List<EmploymentDTO> employments = employmentsEntities.ToSplittedDTOs();
            employments.ApplyEmploymentHistory(fieldTypesDict, employeeGroups, payrollGroups, employmentTypeList, employmentEndReasonsDict, payrollPriceTypesDict, annualLeaveGroups);
            employments.ApplyEmploymentChanges(selectionDateFrom, employeeGroups, payrollGroups, annualLeaveGroups, employmentTypeList, employmentEndReasonsDict);
            employments.SetEmploymentTypeNames(employmentTypeList);
            employments.SetHibernatingPeriods();
            return employments;
        }

        public VacationBalanceItem SetupDistinctOrganisationHrItemRow(List<VacationBalanceItem> connectedToAccountVacationBalanceItemList)
        {
            var firstConnectedAccountObject = connectedToAccountVacationBalanceItemList.FirstOrDefault();
            if (firstConnectedAccountObject == null)
                return null;

            foreach (var listItem in connectedToAccountVacationBalanceItemList)
            {
                if (connectedToAccountVacationBalanceItemList.IndexOf(listItem) != 0)
                {
                    firstConnectedAccountObject.AccountAnalysisFields = firstConnectedAccountObject.AccountAnalysisFields.Union(listItem.AccountAnalysisFields).ToList();
                    firstConnectedAccountObject.ExtraFieldAnalysisFields = firstConnectedAccountObject.ExtraFieldAnalysisFields.Union(listItem.ExtraFieldAnalysisFields).ToList();
                }
            }

            return firstConnectedAccountObject;
        }

        private VacationBalanceItem SetUpVacationBalanceItem(Employee employee, string birthMonth, int age, string gender, string roles, string categories, EmploymentDTO employeeEmployment, List<Employment> employmentsEntities, EmployeeVacationSE employeeVacationSE, decimal preliminaryUsedDays, int? connectedAccountId, List<ExtraFieldAnalysisField> extraFields, List<AccountDTO> companyAccountsDTO)
        {
            var currentEmployment = employmentsEntities.FirstOrDefault(s => s.EmploymentId == employeeEmployment.EmploymentId);
            var vacationAgreement = currentEmployment.GetCurrentVacationGroup();

            #region Item

            var vacationBalanceItem = new VacationBalanceItem();
            vacationBalanceItem.EmploymentNr = employee.EmployeeNr;
            vacationBalanceItem.FirstName = employee.FirstName;
            vacationBalanceItem.LastName = employee.LastName;
            vacationBalanceItem.SocialSecurityNumber = employee.SocialSec;
            vacationBalanceItem.Active = employee.State == (int)SoeEntityState.Active;
            vacationBalanceItem.Categories = categories;
            vacationBalanceItem.BirthYearMonth = birthMonth;
            vacationBalanceItem.Age = age;
            vacationBalanceItem.Gender = gender;
            vacationBalanceItem.Roles = roles;
            vacationBalanceItem.EmploymentPosition = employeeEmployment.EmploymentTypeName;
            vacationBalanceItem.PayrollAgreement = employeeEmployment.PayrollGroupName;
            vacationBalanceItem.ContractGroup = employeeEmployment.EmployeeGroupName;
            vacationBalanceItem.VacationAgreement = vacationAgreement != null ? vacationAgreement.Name : "";
            vacationBalanceItem.WeeklyWorkingHours = employeeEmployment.WorkTimeWeek;
            vacationBalanceItem.EmploymentRate = employeeEmployment.Percent;
            vacationBalanceItem.BasicWeeklyWorkingHours = employeeEmployment.BaseWorkTimeWeek;
            vacationBalanceItem.PaidEarnDays = employeeVacationSE != null ? employeeVacationSE.EarnedDaysPaid ?? 0 : 0;
            vacationBalanceItem.PaidSelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysPaid ?? 0 : 0;
            vacationBalanceItem.PaidRemainingDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysPaid ?? 0 : 0;
            vacationBalanceItem.PaidSysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRatePaid ?? 0 : 0;
            vacationBalanceItem.PaidHolidayAllowance = employeeVacationSE != null ? employeeVacationSE.PaidVacationAllowance ?? 0 : 0;
            vacationBalanceItem.PaidVariableVacationSupplementsSelectedDays = employeeVacationSE != null ? employeeVacationSE.PaidVacationVariableAllowance ?? 0 : 0;
            vacationBalanceItem.UnpaidEarnedDays = employeeVacationSE != null ? employeeVacationSE.EarnedDaysUnpaid ?? 0 : 0;
            vacationBalanceItem.UnpaidSelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysUnpaid ?? 0 : 0;
            vacationBalanceItem.UnpaidRemainingDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysUnpaid ?? 0 : 0;
            vacationBalanceItem.AdvanceEarnedDays = employeeVacationSE != null ? employeeVacationSE.EarnedDaysAdvance ?? 0 : 0;
            vacationBalanceItem.AdvanceSelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysAdvance ?? 0 : 0;
            vacationBalanceItem.AdvanceRemaininDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysAdvance ?? 0 : 0;
            vacationBalanceItem.DebtCashAdvancesAmount = employeeVacationSE != null ? employeeVacationSE.DebtInAdvanceAmount ?? 0 : 0;
            vacationBalanceItem.DebtCashAdvancesDecay = employeeVacationSE != null && employeeVacationSE.DebtInAdvanceDueDate.HasValue
                ? employeeVacationSE.DebtInAdvanceDueDate.Value.ToShortDateString() : String.Empty;
            vacationBalanceItem.SavedYear1EarnedDays = employeeVacationSE != null ? employeeVacationSE.SavedDaysYear1 ?? 0 : 0;
            vacationBalanceItem.SavedYear1SelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysYear1 ?? 0 : 0;
            vacationBalanceItem.SavedYear1RemaininDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear1 ?? 0 : 0;
            vacationBalanceItem.SavedYear1SysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear1 ?? 0 : 0;
            vacationBalanceItem.SavedYear2EarnedDays = employeeVacationSE != null ? employeeVacationSE.SavedDaysYear2 ?? 0 : 0;
            vacationBalanceItem.SavedYear2SelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysYear2 ?? 0 : 0;
            vacationBalanceItem.SavedYear2RemaininDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear2 ?? 0 : 0;
            vacationBalanceItem.SavedYear2SysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear2 ?? 0 : 0;
            vacationBalanceItem.SavedYear3EarnedDays = employeeVacationSE != null ? employeeVacationSE.SavedDaysYear3 ?? 0 : 0;
            vacationBalanceItem.SavedYear3SelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysYear3 ?? 0 : 0;
            vacationBalanceItem.SavedYear3RemaininDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear3 ?? 0 : 0;
            vacationBalanceItem.SavedYear3SysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear3 ?? 0 : 0;
            vacationBalanceItem.SavedYear4EarnedDays = employeeVacationSE != null ? employeeVacationSE.SavedDaysYear4 ?? 0 : 0;
            vacationBalanceItem.SavedYear4SelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysYear4 ?? 0 : 0;
            vacationBalanceItem.SavedYear4RemaininDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear4 ?? 0 : 0;
            vacationBalanceItem.SavedYear4SysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear4 ?? 0 : 0;
            vacationBalanceItem.SavedYear5EarnedDays = employeeVacationSE != null ? employeeVacationSE.SavedDaysYear5 ?? 0 : 0;
            vacationBalanceItem.SavedYear5SelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysYear5 ?? 0 : 0;
            vacationBalanceItem.SavedYear5RemaininDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear5 ?? 0 : 0;
            vacationBalanceItem.SavedYear5SysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear5 ?? 0 : 0;
            vacationBalanceItem.OverdueDaysEarnedDays = employeeVacationSE != null ? employeeVacationSE.SavedDaysOverdue ?? 0 : 0;
            vacationBalanceItem.OverdueDaysSelectedDays = employeeVacationSE != null ? employeeVacationSE.UsedDaysOverdue ?? 0 : 0;
            vacationBalanceItem.OverdueDaysRemainingDays = employeeVacationSE != null ? employeeVacationSE.RemainingDaysOverdue ?? 0 : 0;
            vacationBalanceItem.OverdueDaysSysDegreeEarned = employeeVacationSE != null ? employeeVacationSE.EmploymentRateOverdue ?? 0 : 0;
            vacationBalanceItem.PreliminaryWithdrawnRemaininDays = preliminaryUsedDays;
            vacationBalanceItem.RemainingSelectedDays = CalculateRemainingDaysPrelSum(employeeVacationSE);
            vacationBalanceItem.RemainingRemainingDays = CalculateRemainingDaysSum(employeeVacationSE, preliminaryUsedDays);
            vacationBalanceItem.Created = employeeVacationSE != null && employeeVacationSE.Created.HasValue ? employeeVacationSE.Created.Value : CalendarUtility.DATETIME_DEFAULT;
            vacationBalanceItem.CreatedBy = employeeVacationSE != null ? employeeVacationSE.CreatedBy : String.Empty;
            vacationBalanceItem.Modified = employeeVacationSE != null && employeeVacationSE.Modified.HasValue ? employeeVacationSE.Modified.Value : CalendarUtility.DATETIME_DEFAULT;
            vacationBalanceItem.ModifiedBy = employeeVacationSE != null ? employeeVacationSE.ModifiedBy : String.Empty;
            vacationBalanceItem.AccountAnalysisFields = connectedAccountId.HasValue ? GetAccountAnalysisFields(companyAccountsDTO, connectedAccountId) : new List<AccountAnalysisField>();
            vacationBalanceItem.ExtraFieldAnalysisFields = extraFields;

            #endregion

            return vacationBalanceItem;
        }

        private decimal CalculateRemainingDaysSum(EmployeeVacationSE employeeVacationSE, decimal prelUsedDays)
        {
            var defaultDecimalValue = (decimal)0.00;

            var remainingDaysSum = employeeVacationSE != null ?
                 (employeeVacationSE.RemainingDaysPaid ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysUnpaid ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysAdvance ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysYear1 ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysYear2 ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysYear3 ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysYear4 ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysYear5 ?? defaultDecimalValue) +
                 (employeeVacationSE.RemainingDaysOverdue ?? defaultDecimalValue) +
                 (prelUsedDays) : defaultDecimalValue;

            return remainingDaysSum;
        }

        private decimal CalculateRemainingDaysPrelSum(EmployeeVacationSE employeeVacationSE)
        {
            var defaultDecimalValue = (decimal)0.00;

            var usedDaysPrelSum = employeeVacationSE != null ?
                (employeeVacationSE.UsedDaysPaid ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysUnpaid ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysAdvance ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysYear1 ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysYear2 ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysYear3 ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysYear4 ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysYear5 ?? defaultDecimalValue) +
                (employeeVacationSE.UsedDaysOverdue ?? defaultDecimalValue) : defaultDecimalValue;
            return usedDaysPrelSum;
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }

        private List<AccountAnalysisField> GetAccountAnalysisFields(List<AccountDTO> companyAccountsDTO, int? accountId)
        {
            var accountAnalysisFieldsList = new List<AccountAnalysisField>();
            if (loadAccountInternal && companyAccountsDTO.Any(a => a.AccountId == accountId))
            {
                var accountDTO = companyAccountsDTO.FirstOrDefault(a => a.AccountId == accountId);
                if (accountDTO != null)
                {
                    foreach (var parentAccount in accountDTO.ParentAccounts)
                    {
                        accountAnalysisFieldsList.Add(new AccountAnalysisField(parentAccount));
                    }
                }
                accountAnalysisFieldsList.Add(new AccountAnalysisField(accountDTO));
            }
            return accountAnalysisFieldsList;
        }

        private List<ExtraFieldAnalysisField> GetExtraFieldAnalysisField(List<ExtraFieldRecordDTO> extraFieldRecords, int employeeId)
        {
            var extraFieldList = new List<ExtraFieldAnalysisField>();
            var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == employeeId).ToList();
            Dictionary<int, string> yesNoDictionary = base.GetTermGroupDict(TermGroup.YesNo, base.GetLangId());

            foreach (var column in _reportDataInput.Columns.Where(w => w.ColumnKey.Contains("xtraField")))
            {
                if (column.Selection?.Options?.Key != null && int.TryParse(column.Selection.Options.Key, out int recordId))
                {
                    var matchOnEmployee = extraFieldRecordsOnEmployee.FirstOrDefault(f => f.ExtraFieldId == recordId);
                    if (matchOnEmployee != null)
                    {
                        extraFieldList.Add(new ExtraFieldAnalysisField(matchOnEmployee, yesNoDictionary));
                    }
                }
            }

            if (extraFieldList.Count == 0)
                extraFieldList.Add(new ExtraFieldAnalysisField(null));
            return extraFieldList;
        }
    }

    public class VacationBalanceReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_VacationBalanceMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public VacationBalanceReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_VacationBalanceMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_VacationBalanceMatrixColumns.Unknown;
        }
    }

    public class VacationBalanceReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<VacationBalanceReportDataField> Columns { get; set; }

        public VacationBalanceReportDataInput(CreateReportResult reportResult, List<VacationBalanceReportDataField> columns, List<AccountDimDTO> accountDims, List<AccountDTO> accountInternals)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class VacationBalanceReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<VacationBalanceItem> VacationBalanceItems { get; set; }
        public VacationBalanceReportDataInput Input { get; set; }

        public VacationBalanceReportDataOutput(VacationBalanceReportDataInput input)
        {
            this.VacationBalanceItems = new List<VacationBalanceItem>();
            this.Input = input;
        }
    }
}
