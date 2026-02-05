using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class EmployeeSalaryReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly EmployeeSalaryReportDataInput _reportDataInput;
        private readonly EmployeeSalaryReportDataOutput _reportDataOutput;

        public EmployeeSalaryReportData(ParameterObject parameterObject, EmployeeSalaryReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeSalaryReportDataOutput(reportDataInput);
        }

        private bool isInsightSalaryTypeGender;
        private bool isInsightSalaryType;

        private bool loadAccountInternal => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
        private bool loadCategories => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeSalaryMatrixColumns.CategoryName);
        private bool loadExtraFields => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("xtraField"));
        private bool loadExperience => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeSalaryMatrixColumns.ExperienceTot);
        private bool changeItems => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_EmployeeSalaryMatrixColumns.SalaryDateFrom ||
            a.Column == TermGroup_EmployeeSalaryMatrixColumns.SalaryAmount ||
            a.Column == TermGroup_EmployeeSalaryMatrixColumns.AccordingToPayrollGroup ||
            a.Column == TermGroup_EmployeeSalaryMatrixColumns.IsSecondaryEmployment ||
            a.Column == TermGroup_EmployeeSalaryMatrixColumns.PayrollLevel ||
            a.Column == TermGroup_EmployeeSalaryMatrixColumns.SalaryFromPayrollGroup
        );

        public EmployeeSalaryReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Prereq

            string employmentTypeNames = "";

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            isInsightSalaryType = matrixColumnsSelection.InsightId == (int)TermGroup_FixedInsights.EmployeeSalary_PayrollType;
            isInsightSalaryTypeGender = matrixColumnsSelection.InsightId == (int)TermGroup_FixedInsights.EmployeeSalary_GenderWithPayrollType;
            List<string> logInsight = new List<string>();

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);

            TryGetPayrollPriceTypeSelection(reportResult, out List<int> payrollPriceTypes, "payrollPriceTypes");
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    Dictionary<string, List<CompanyCategoryRecord>> categoryRecordsFullKeyDict = new Dictionary<string, List<CompanyCategoryRecord>>();
                    List<CategoryAccount> categoryAccounts = new List<CategoryAccount>();

                    int langId = GetLangId();
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);
                    Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);
                    Dictionary<int, string> employmentPriceTypes = base.GetTermGroupDict(TermGroup.PayrollPriceTypes, langId);
                    List<PayrollPriceType> companyPayrollPriceTypes = PayrollManager.GetPayrollPriceTypes(base.ActorCompanyId, null);
                    List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                    List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
                    List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();
                    List<AccountDTO> companyAccountsDTO = new List<AccountDTO>();
                    Dictionary<int, List<EmploymentPriceTypeChangeDTO>> employmentPayrollPriceTypeChanges;
                    Dictionary<int, string> fieldTypesDict = base.GetTermGroupDict(TermGroup.EmploymentChangeFieldType, langId);
                    Dictionary<int, string> payrollPriceTypesDict = PayrollManager.GetPayrollPriceTypesDict(reportResult.ActorCompanyId, null);
                    Dictionary<int, string> employmentEndReasonsDict = EmployeeManager.GetSystemEndReasons(reportResult.ActorCompanyId, includeCompanyEndReasons: true);
                    List<EmploymentTypeDTO> employmentTypeList = EmployeeManager.GetEmploymentTypes(reportResult.ActorCompanyId, (TermGroup_Languages)langId);
                    List<EmployeeGroupDTO> employeeGroups = EmployeeManager.GetEmployeeGroups(reportResult.ActorCompanyId, onlyActive: false).ToDTOs().ToList();
                    List<PayrollGroupDTO> payrollGroups = PayrollManager.GetPayrollGroups(reportResult.ActorCompanyId, onlyActive: true).ToDTOs().ToList();
                    List<AnnualLeaveGroupDTO> annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(reportResult.ActorCompanyId).ToDTOs().ToList();

                    bool useExperienceMonthsOnEmploymentAsStartValue = false;
                    if (loadExperience)
                        useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, Company.ActorCompanyId, 0);

                    if (!useAccountHierarchy)
                    {
                        companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, base.ActorCompanyId);
                        var categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                        categoryRecordsFullKeyDict = CategoryManager.GetCompanyCategoryRecordsFullKeyDict(categoryRecords, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                        categoryAccounts = base.GetCategoryAccountsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                    }
                    else
                        employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                    if (loadAccountInternal)
                    {
                        companyAccountsDTO = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs();
                        companyAccountsDTO.ForEach(f => f.ParentAccounts = f.GetParentAccounts(companyAccountsDTO));
                    }

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true, loadEmploymentPriceType: true);

                    if (changeItems)
                        employmentPayrollPriceTypeChanges = EmployeeManager.GetEmploymentPriceTypeChangesForEmployees(base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                    else
                    {
                        employmentPayrollPriceTypeChanges = new Dictionary<int, List<EmploymentPriceTypeChangeDTO>>();
                        EmploymentPriceTypeChangeDTO emptyChange = new EmploymentPriceTypeChangeDTO
                        {
                            PayrollPriceTypeId = 0,
                            IsPayrollGroupPriceType = false,
                            IsSecondaryEmployment = false,
                            Amount = 0,
                            PayrollLevelName = "",
                            Code = "",
                            Name = "",
                            PayrollPriceType = TermGroup_SoePayrollPriceType.Misc,
                            PayrollGroupAmount = 0

                        };

                        foreach (int employeeId in selectionEmployeeIds)
                            employmentPayrollPriceTypeChanges.Add(employeeId, new List<EmploymentPriceTypeChangeDTO> { emptyChange });

                    }
                    if (loadExtraFields)
                    {
                        extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(entities, selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();
                    }

                    foreach (Employee employee in employees)
                    {
                        List<CompanyCategoryRecord> employeeCategoryRecords = null;
                        var acccountAnalysisFields = new List<AccountAnalysisField>();

                        string employeeCategoryString = null;
                        string gender = GetValueFromDict((int)employee.Sex, sexDict);
                        var age = CalendarUtility.GetYearsBetweenDates(EmployeeManager.GetEmployeeBirthDate(employee).ToValueOrToday(), selectionDateTo);
                        var birthMonth = employee.SocialSec != "" ? employee.SocialSec.Substring(0, 4) + '-' + employee.SocialSec.Substring(4, 2) : string.Empty;
                        List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true).DistinctBy(i => i.PositionId).ToList();
                        var employmentsEntities = employee.GetEmployments(selectionDateFrom, selectionDateTo, includeSecondary: true);
                        List<EmploymentDTO> employments = employmentsEntities.ToSplittedDTOs();
                        employments.ApplyEmploymentHistory(fieldTypesDict, employeeGroups, payrollGroups, employmentTypeList, employmentEndReasonsDict, payrollPriceTypesDict, annualLeaveGroups);
                        employments.ApplyEmploymentChanges(selectionDateFrom, employeeGroups, payrollGroups, annualLeaveGroups, employmentTypeList, employmentEndReasonsDict);
                        employments.SetEmploymentTypeNames(employmentTypeList);
                        employments.SetHibernatingPeriods();
                        EmploymentDTO lastEmployment = employments.OrderBy(o => o.DateFrom).LastOrDefault();

                        List<Employment> employmentx = employee.GetActiveEmploymentsDesc(includeSecondary: true);
                        if (employmentx.IsNullOrEmpty())
                            continue;

                        Employment currentEmployment = employmentx.GetEmployment(selectionDateTo) ?? employmentx.GetFirstEmployment().GetNearestEmployment(employmentx.GetLastEmployment(), selectionDateTo);

                        if (loadAccountInternal)
                        {
                            string key = useAccountHierarchy ? string.Empty : CompanyCategoryRecord.ConstructKey((int)SoeCategoryRecordEntity.Employee, employee.EmployeeId);
                            if (!string.IsNullOrEmpty(key) && categoryRecordsFullKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records))
                                employeeCategoryRecords = records.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true);

                            acccountAnalysisFields = currentEmployment.AccountAnalysisFields(employeeAccounts.FindAll(r => r.EmployeeId == employee.EmployeeId), employeeCategoryRecords, categoryAccounts, companyAccountsDTO, selectionDateFrom, selectionDateTo);
                        }

                        if (lastEmployment != null && !employments.Any(e => e.EmploymentId == lastEmployment.EmploymentId))
                        {
                            employments.Add(lastEmployment);
                            employments = employments.OrderBy(o => o.DateFrom).ToList();
                        }

                        if (!employmentsEntities.IsNullOrEmpty())
                        {
                            if (!useAccountHierarchy && loadCategories)
                            {
                                var selectedEmployeeCategoryRecords = companyCategoryRecords.Where(r => r.RecordId == employee.EmployeeId && r.Default) ?? null; //Category
                                if (!selectedEmployeeCategoryRecords.IsNullOrEmpty())
                                    employeeCategoryString = string.Join(", ", selectedEmployeeCategoryRecords.Select(t => t.Category?.Name ?? string.Empty));
                            }

                            employmentTypeNames = "";
                            foreach (EmploymentDTO employment in employments)
                            {
                                if (employmentTypeNames == "")
                                    employmentTypeNames = $"{employment.EmploymentTypeName}";
                                else
                                {
                                    if (!employmentTypeNames.Contains(employment.EmploymentTypeName))
                                        employmentTypeNames += $", {employment.EmploymentTypeName}";
                                }
                            }
                            List<ExtraFieldAnalysisField> extraFields = GetExtraFieldAnalysisField(extraFieldRecords, employee.EmployeeId);

                            if (useAccountHierarchy)
                            {
                                var connectedToAccountOrganisationHrItemList = new List<EmployeeSalaryItem>();
                                var connectedToAccounts = employeeAccounts.Where(r => r.EmployeeId == employee.EmployeeId).ToList();
                                var isAllAccountHasParent = false;
                                if (!connectedToAccounts.IsNullOrEmpty())
                                {
                                    var connectedAccountIds = connectedToAccounts.Select(s => s.AccountId).ToList();
                                    var accountDTOParents = companyAccountsDTO.Where(a => connectedAccountIds.Contains(a.AccountId)).Select(s => s.ParentAccounts).ToList();
                                    if (accountDTOParents.Count > 0 && accountDTOParents.TrueForAll(a => a.Count > 0))
                                        isAllAccountHasParent = true;
                                    connectedToAccountOrganisationHrItemList.Clear();

                                    var changeListEmployeeSalaryItems = new List<EmployeeSalaryItem>();

                                    foreach (var connectedToAccount in connectedToAccounts)
                                    {
                                        bool filteredOk = true;

                                        if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeAccount && !filteredOk)
                                            continue;

                                        var rebuildAccount = new List<AccountAnalysisField>();
                                        var account = acccountAnalysisFields.FirstOrDefault(w => connectedToAccount.AccountId == w.AccountId);
                                        if (account != null)
                                        {
                                            rebuildAccount.Add(account);
                                            rebuildAccount.AddRange(acccountAnalysisFields.Where(w => connectedToAccount.AccountId != w.AccountId));
                                            acccountAnalysisFields = rebuildAccount;
                                        }

                                        List<EmploymentPriceTypeChangeDTO> changeList = employmentPayrollPriceTypeChanges.ContainsKey(employee.EmployeeId) ? employmentPayrollPriceTypeChanges[employee.EmployeeId] : new List<EmploymentPriceTypeChangeDTO>();

                                        if (!payrollPriceTypes.IsNullOrEmpty())
                                            changeList = changeList.Where(l => payrollPriceTypes.Contains(l.PayrollPriceTypeId)).ToList();

                                        changeListEmployeeSalaryItems.Clear();
                                        connectedToAccountOrganisationHrItemList.Clear();

                                        if (changeList.Count > 0)
                                        {
                                            changeListEmployeeSalaryItems = ChangeListEmployeeSalaryItem(entities, changeList, employmentsEntities, lastEmployment, companyPayrollPriceTypes,
                                                employee, useExperienceMonthsOnEmploymentAsStartValue, logInsight, employmentPriceTypes, gender, employeeCategoryString,
                                                employmentTypeNames, birthMonth, age, employeePositions, extraFields, acccountAnalysisFields);

                                        }
                                        else
                                        {
                                            var salaryItem = SetUpEmployeeSalaryItem(employee, gender, employeeCategoryString, employmentTypeNames, birthMonth, age, employeePositions, extraFields, acccountAnalysisFields);
                                            changeListEmployeeSalaryItems.Add(salaryItem);
                                        }

                                        connectedToAccountOrganisationHrItemList.AddRange(changeListEmployeeSalaryItems);
                                        if (isAllAccountHasParent)
                                        {
                                            _reportDataOutput.EmployeeSalaryItems.AddRange(connectedToAccountOrganisationHrItemList);
                                        }
                                        else
                                        {
                                            //when any of accounts don't have parent
                                            if (EmployeeSalaryItemsAddingCheckingSublevelOfHierarchy(connectedToAccounts, loadAccountInternal, companyAccountsDTO))
                                                _reportDataOutput.EmployeeSalaryItems.AddRange(changeListEmployeeSalaryItems);
                                        }
                                    }

                                }

                                if ((!isAllAccountHasParent || isAllAccountHasParent) && connectedToAccounts.Count > 1)
                                {
                                    List<EmployeeSalaryItem> merged = new List<EmployeeSalaryItem>();
                                    List<EmployeeSalaryItem> finalMerged = new List<EmployeeSalaryItem>();

                                    foreach (var salaries in _reportDataOutput.EmployeeSalaryItems.Select(s => s).GroupBy(g => g.GroupOn(_reportDataInput.Columns.Select(s => s).ToList(), false)))
                                    {
                                        merged.Add(MergeEmployeeSalaryRecords(salaries));
                                    }
                                    foreach (var salaries in merged.Select(s => s).GroupBy(g => g.GroupOn(_reportDataInput.Columns.Select(s => s).ToList(), false)))
                                    {
                                        finalMerged.Add(MergeEmployeeSalaryRecords(salaries, true));
                                    }

                                    _reportDataOutput.EmployeeSalaryItems = finalMerged.OrderBy(o => o.EmployeeId).ToList();
                                }
                            }
                            else
                            {
                                List<EmploymentPriceTypeChangeDTO> changeList = employmentPayrollPriceTypeChanges.ContainsKey(employee.EmployeeId) ? employmentPayrollPriceTypeChanges[employee.EmployeeId] : new List<EmploymentPriceTypeChangeDTO>();

                                if (!payrollPriceTypes.IsNullOrEmpty() && changeItems)
                                    changeList = changeList.Where(l => payrollPriceTypes.Contains(l.PayrollPriceTypeId)).ToList();

                                if (changeList.Count > 0)
                                {
                                    _reportDataOutput.EmployeeSalaryItems.AddRange(ChangeListEmployeeSalaryItem(entities, changeList, employmentsEntities, lastEmployment, companyPayrollPriceTypes,
                                        employee, useExperienceMonthsOnEmploymentAsStartValue, logInsight, employmentPriceTypes, gender, employeeCategoryString,
                                        employmentTypeNames, birthMonth, age, employeePositions, extraFields, acccountAnalysisFields));
                                }
                                else
                                {
                                    var salaryItem = SetUpEmployeeSalaryItem(employee, gender, employeeCategoryString, employmentTypeNames, birthMonth, age, employeePositions, extraFields, new List<AccountAnalysisField>());
                                    _reportDataOutput.EmployeeSalaryItems.Add(salaryItem);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(ex);
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        public bool EmployeeSalaryItemsAddingCheckingSublevelOfHierarchy(List<EmployeeAccount> connectedToAccounts, bool loadAccountInternal, List<AccountDTO> companyAccountsDTO)
        {
            return (connectedToAccounts.Count == 1 && !loadAccountInternal && companyAccountsDTO.Count == 0) ||
                                                (connectedToAccounts.Count > 0 && loadAccountInternal && companyAccountsDTO.Count > 0) ||
                                                (connectedToAccounts.Count > 0 && !loadAccountInternal && companyAccountsDTO.Count == 0);
        }

        public List<EmployeeSalaryItem> ChangeListEmployeeSalaryItem(CompEntities entities, List<EmploymentPriceTypeChangeDTO> changeList, List<Employment> employmentsEntities,
            EmploymentDTO lastEmployment, List<PayrollPriceType> companyPayrollPriceTypes, Employee employee, bool useExperienceMonthsOnEmploymentAsStartValue
            , List<string> logInsight, Dictionary<int, string> employmentPriceTypes, string gender, string employeeCategoryString, string employmentTypeNames,
            string birthMonth, int age, List<EmployeePosition> employeePositions, List<ExtraFieldAnalysisField> extraFields, List<AccountAnalysisField> acccountAnalysisFields)
        {
            var changeListEmployeeSalaryItems = new List<EmployeeSalaryItem>();
            var changeDate = changeList.Max(w => w.FromDate);
            var experienceTot = 0;
            foreach (var changeItem in changeList)
            {
                PayrollPriceType payrollPriceType = companyPayrollPriceTypes.FirstOrDefault(t => t.PayrollPriceTypeId == changeItem.PayrollPriceTypeId);

                if (loadExperience)
                {
                    var lastEmpl = employmentsEntities.FirstOrDefault(f => f.EmploymentId == lastEmployment.EmploymentId);
                    if (experienceTot == 0)
                        experienceTot = EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, lastEmpl, useExperienceMonthsOnEmploymentAsStartValue, changeDate);
                }

                string salaryType = GetValueFromDict(payrollPriceType?.Type ?? 0, employmentPriceTypes);

                if (isInsightSalaryTypeGender)
                {
                    if (logInsight.Contains(employee.EmployeeNr + "-" + salaryType + "-" + gender))
                        continue;

                    logInsight.Add(employee.EmployeeNr + "-" + salaryType + "-" + gender);
                }
                else if (isInsightSalaryType)
                {
                    if (logInsight.Contains(employee.EmployeeNr + "-" + salaryType))
                        continue;

                    logInsight.Add(employee.EmployeeNr + "-" + salaryType);
                }

                #region Item

                var salaryItem = SetUpEmployeeSalaryItem(employee, gender, employeeCategoryString, employmentTypeNames, birthMonth, age, employeePositions, extraFields, acccountAnalysisFields, payrollPriceType, salaryType, changeItem, experienceTot);
                changeListEmployeeSalaryItems.Add(salaryItem);

                #endregion
            }

            return changeListEmployeeSalaryItems;

        }

        public EmployeeSalaryItem MergeEmployeeSalaryRecords(IGrouping<string, EmployeeSalaryItem> transaction, bool doCalculation = false)
        {
            var sorted = transaction.ToList();
            var first = sorted.First();
            var last = sorted.Last();

            EmployeeSalaryItem mergedSalary = first;
            if (doCalculation)
            {
                mergedSalary.SalaryAmount = transaction.Sum(s => s.SalaryAmount);
                mergedSalary.SalaryDiff = transaction.Sum(s => s.SalaryDiff);
                mergedSalary.SalaryFromPayrollGroup = transaction.Sum(s => s.SalaryFromPayrollGroup);
                mergedSalary.ExperienceTot = transaction.Sum(s => s.ExperienceTot);
            }
            return mergedSalary;
        }

        public EmployeeSalaryItem SetUpEmployeeSalaryItem(Employee employee, string gender, string employeeCategoryString, string employmentTypeNames, string birthMonth, int age, List<EmployeePosition> employeePositions, List<ExtraFieldAnalysisField> extraFields,
            List<AccountAnalysisField> accountAnalysisField, PayrollPriceType payrollPriceType = null, string salaryType = "", EmploymentPriceTypeChangeDTO wageItem = null, int totalExperience = 0)
        {
            var salaryItem = new EmployeeSalaryItem();
            salaryItem.EmployeeId = employee.EmployeeId;
            salaryItem.EmployeeNr = employee.EmployeeNr;
            salaryItem.EmployeeName = employee.Name;
            salaryItem.FirstName = employee.FirstName;
            salaryItem.LastName = employee.LastName;
            salaryItem.Gender = gender;
            salaryItem.EmploymentType = employmentTypeNames;
            salaryItem.Position = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.Name ?? string.Empty)) : string.Empty;
            salaryItem.CategoryName = !string.IsNullOrEmpty(employeeCategoryString) ? employeeCategoryString : string.Empty;
            salaryItem.BirthYearMonth = birthMonth;
            salaryItem.Age = age;
            salaryItem.ExtraFieldAnalysisFields = extraFields;
            salaryItem.AccountAnalysisFields = accountAnalysisField;

            if (payrollPriceType != null)
            {
                salaryItem.SalaryTypeName = payrollPriceType?.Name ?? string.Empty;
                salaryItem.SalaryTypeCode = payrollPriceType?.Code ?? string.Empty;
                salaryItem.SalaryTypeDesc = payrollPriceType?.Description ?? string.Empty;
            }
            if (wageItem != null)
            {
                salaryItem.SalaryDateFrom = wageItem.FromDate;
                salaryItem.SalaryAmount = wageItem.Amount;
                salaryItem.AccordingToPayrollGroup = wageItem.IsPayrollGroupPriceType;
                salaryItem.IsSecondaryEmployment = wageItem.IsSecondaryEmployment;
                salaryItem.PayrollLevel = wageItem.PayrollLevelName ?? string.Empty;
                salaryItem.SalaryFromPayrollGroup = wageItem.PayrollGroupAmount;
                salaryItem.SalaryDiff = wageItem.Amount - wageItem.PayrollGroupAmount;
            }
            salaryItem.SalaryType = salaryType;
            salaryItem.ExperienceTot = totalExperience;

            return salaryItem;
        }

        private List<ExtraFieldAnalysisField> GetExtraFieldAnalysisField(List<ExtraFieldRecordDTO> extraFieldRecords, int employeeId)
        {
            Dictionary<int, string> yesNoDictionary = base.GetTermGroupDict(TermGroup.YesNo, base.GetLangId());
            var extraFieldList = new List<ExtraFieldAnalysisField>();
            var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == employeeId).ToList();

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

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class EmployeeSalaryReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeSalaryMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeSalaryReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeSalaryMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_EmployeeSalaryMatrixColumns.Unknown;
        }
    }

    public class EmployeeSalaryReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeSalaryReportDataField> Columns { get; set; }

        public EmployeeSalaryReportDataInput(CreateReportResult reportResult, List<EmployeeSalaryReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeSalaryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeSalaryItem> EmployeeSalaryItems { get; set; }
        public EmployeeSalaryReportDataInput Input { get; set; }

        public EmployeeSalaryReportDataOutput(EmployeeSalaryReportDataInput input)
        {
            this.EmployeeSalaryItems = new List<EmployeeSalaryItem>();
            this.Input = input;
        }
    }
}
