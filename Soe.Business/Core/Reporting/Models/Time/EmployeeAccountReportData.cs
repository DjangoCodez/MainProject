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
    public class EmployeeAccountReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeAccountReportDataInput _reportDataInput;
        private readonly EmployeeAccountReportDataOutput _reportDataOutput;

        private bool LoadAccountInternal
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
            }
        }
        private bool LoadExtraFields
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeAccountMatrixColumns.ExtraFieldEmployee);
            }
        }
        private bool LoadCategory
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_EmployeeAccountMatrixColumns.CategoryName);
            }
        }

        public EmployeeAccountReportData(ParameterObject parameterObject, EmployeeAccountReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeAccountReportDataOutput(reportDataInput);
        }

        public static List<EmployeeAccountReportDataField> GetPossibleDataFields()
        {
            List<EmployeeAccountReportDataField> possibleFields = new List<EmployeeAccountReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeAccountMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeAccountReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeAccountReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out _, "filterOnAccounting");
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);

                    if (selectionEmployeeIds.Count == 0)
                        employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: false, loadContact: false);
                    else
                        employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: false, loadContact: false);

                    #region Content
                    if (employees != null)
                    {
                        #region Collections

                        List<Account> companyAccounts = AccountManager.GetAccountsByCompany(entities, base.ActorCompanyId, false, true, true, true);
                        List<AccountDTO> companyAccountDTOs = companyAccounts.ToDTOs();
                        Dictionary<int, string> accountTypeDict = new Dictionary<int, string>
                        {
                            {(int)EmploymentAccountType.Income,  GetText(10452, 1004)},
                            {(int)EmploymentAccountType.Cost, GetText(10451, 1004)},
                        };
                        List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                        List<CompanyCategoryRecord> categoryRecords;
                        List<CategoryAccount> categoryAccounts = new List<CategoryAccount>();
                        List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();
                        Dictionary<string, List<CompanyCategoryRecord>> categoryRecordsFullKeyDict = new Dictionary<string, List<CompanyCategoryRecord>>();

                        if (LoadCategory && !useAccountHierarchy)
                        {
                            categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                            categoryRecordsFullKeyDict = CategoryManager.GetCompanyCategoryRecordsFullKeyDict(categoryRecords, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                        }

                        if (LoadAccountInternal)
                        {
                            companyAccountDTOs = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs();
                            companyAccountDTOs.ForEach(f => f.ParentAccounts = f.GetParentAccounts(companyAccountDTOs));
                            if (useAccountHierarchy)
                                employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                            else
                                categoryAccounts = base.GetCategoryAccountsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));
                        }
                        else if (useAccountHierarchy)
                            employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                        if (LoadExtraFields)
                        {
                            extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();
                        }

                        #endregion                        

                        foreach (Employee employee in employees)
                        {
                            List<EmployeeAccount> selectedEmployeeAccountRecords = null;
                            List<CompanyCategoryRecord> selectedEmployeeCategoryRecords = null;

                            List<Employment> employments = employee.GetEmployments(selectionDateFrom, selectionDateTo, includeSecondary: true);
                            List<EmploymentDTO> employmentsDTOs = employments.ToSplittedDTOs();
                            EmploymentDTO lastEmployment = employmentsDTOs.OrderBy(o => o.DateFrom).LastOrDefault();
                            if (lastEmployment != null && !employmentsDTOs.Any(e => e.EmploymentId == lastEmployment.EmploymentId))
                            {
                                employmentsDTOs.Add(lastEmployment);
                                employmentsDTOs = employmentsDTOs.OrderBy(o => o.DateFrom).ToList();
                            }

                            if (employmentsDTOs.IsNullOrEmpty())
                                continue;

                            List<CompanyCategoryRecord> employeeCategoryRecords = null;
                            List<EmploymentAccountStd> employmentAccountStdsForEmployee = EmployeeManager.GetEmploymentAccounts(entities, employee.EmployeeId);

                            foreach (Employment employment in employments)
                            {
                                int fixedCounter = 1;

                                List<EmploymentAccountStd> employmentAccountingStds = employmentAccountStdsForEmployee.Where(a => a.EmploymentId == employment.EmploymentId).OrderByDescending(a => a.Percent).ToList();

                                foreach (EmploymentAccountStd employmentAccountingStd in employmentAccountingStds)
                                {
                                    EmployeeAccountItem employeeAccountItem = new EmployeeAccountItem();

                                    #region Accounting

                                    if (LoadAccountInternal)
                                    {
                                        string key = useAccountHierarchy ? string.Empty : CompanyCategoryRecord.ConstructKey((int)SoeCategoryRecordEntity.Employee, employee.EmployeeId);
                                        if (!string.IsNullOrEmpty(key) && categoryRecordsFullKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records))
                                            employeeCategoryRecords = records.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true);

                                        employeeAccountItem.AccountAnalysisFields = employment.AccountAnalysisFields(employeeAccounts.FindAll(r => r.EmployeeId == employee.EmployeeId), employeeCategoryRecords, categoryAccounts, companyAccountDTOs, selectionDateFrom, selectionDateTo, employmentAccountingStd.EmploymentAccountStdId);
                                    }

                                    #endregion

                                    string accountType = employmentAccountingStd.IsFixedAccounting ? GetText(3605, 1002) + " " + fixedCounter : GetValueFromDict(employmentAccountingStd.Type, accountTypeDict);

                                    employeeAccountItem.EmployeeId = employee.EmployeeId;
                                    employeeAccountItem.EmployeeNr = employee.EmployeeNr;
                                    employeeAccountItem.EmployeeName = employee.Name;
                                    employeeAccountItem.FirstName = employee.FirstName;
                                    employeeAccountItem.LastName = employee.LastName;
                                    employeeAccountItem.FixedAccounting = employmentAccountingStd.IsFixedAccounting;
                                    employeeAccountItem.DateFrom = employment.DateFrom;
                                    employeeAccountItem.Type = accountType;
                                    employeeAccountItem.AccountInternalStd = employmentAccountingStd.AccountStd?.Account?.AccountNrPlusName ?? string.Empty;
                                    employeeAccountItem.CategoryName = !selectedEmployeeCategoryRecords.IsNullOrEmpty() ? string.Join(", ", selectedEmployeeCategoryRecords.Select(c => c.Category?.Name)) : string.Empty;
                                    employeeAccountItem.AccountStdName = !selectedEmployeeAccountRecords.IsNullOrEmpty() ? string.Join(", ", selectedEmployeeAccountRecords.Select(a => a.Account?.AccountNrPlusName)) : string.Empty;

                                    if (employmentAccountingStd.Percent > 0)
                                        employeeAccountItem.Percent = employmentAccountingStd.Percent;

                                    #region EmployeeExtraFeilds

                                    Dictionary<int, string> yesNoDictionary = base.GetTermGroupDict(TermGroup.YesNo, base.GetLangId());
                                    if (LoadExtraFields && extraFieldRecords.Any())
                                    {
                                        var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == employee.EmployeeId).ToList();

                                        foreach (var column in _reportDataInput.Columns.Where(w => w.Column == TermGroup_EmployeeAccountMatrixColumns.ExtraFieldEmployee))
                                        {
                                            if (column.Selection?.Options?.Key != null && int.TryParse(column.Selection.Options.Key, out int recordId))
                                            {
                                                var matchOnEmployee = extraFieldRecordsOnEmployee.FirstOrDefault(f => f.ExtraFieldId == recordId);
                                                if (matchOnEmployee != null)
                                                {
                                                    employeeAccountItem.ExtraFieldAnalysisFields.Add(new ExtraFieldAnalysisField(matchOnEmployee, yesNoDictionary));
                                                    continue;
                                                }
                                            }
                                            employeeAccountItem.ExtraFieldAnalysisFields.Add(new ExtraFieldAnalysisField(null));
                                        }
                                    }

                                    #endregion

                                    _reportDataOutput.EmployeeAccountItems.Add(employeeAccountItem);

                                    fixedCounter++;
                                }

                            }

                        }
                    }
                    #endregion
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
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

    public class EmployeeAccountReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeAccountMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }
        public string OptionKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeAccountReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.OptionKey = Selection?.Options?.Key;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeAccountMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_EmployeeAccountMatrixColumns.Unknown;
        }
    }

    public class EmployeeAccountReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeAccountReportDataField> Columns { get; set; }

        public EmployeeAccountReportDataInput(CreateReportResult reportResult, List<EmployeeAccountReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeAccountReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeeAccountReportDataInput Input { get; set; }
        public List<EmployeeAccountItem> EmployeeAccountItems { get; set; }

        public EmployeeAccountReportDataOutput(EmployeeAccountReportDataInput input)
        {
            this.Input = input;
            this.EmployeeAccountItems = new List<EmployeeAccountItem>();
        }
    }
}

