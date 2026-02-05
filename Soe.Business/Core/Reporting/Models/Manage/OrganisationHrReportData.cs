using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage
{
    public class OrganisationHrReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly OrganisationHrReportDataInput _reportDataInput;
        private readonly OrganisationHrReportDataOutput _reportDataOutput;

        private bool loadAccountInternal => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
        private bool loadExtraFields => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("xtraField"));

        public OrganisationHrReportData(ParameterObject parameterObject, OrganisationHrReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new OrganisationHrReportDataOutput(reportDataInput);
        }

        public OrganisationHrReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out bool selectionOnlyInactive, out _);

            List<int> selectionCategoryIds = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees")?.CategoryIds;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {

                    #region Init variables

                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);

                    List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                    List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
                    List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();
                    List<AccountDTO> companyAccountsDTO = new List<AccountDTO>();


                    #endregion

                    #region Load Accounts and Categories

                    if (!useAccountHierarchy)
                    {
                        companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, base.ActorCompanyId);
                    }

                    #endregion

                    #region Load Employees

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: (selectionIncludeInactive || !selectionOnlyInactive), orderByName: false, loadEmployment: false, loadUser: true, loadEmploymentAccounts: true);

                    if (selectionIncludeInactive)
                    {
                        List<Employee> employeesInactive = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: false, orderByName: false, loadEmployment: false, loadUser: true, loadEmploymentAccounts: true);
                        if (!employeesInactive.IsNullOrEmpty())
                        {
                            employees = employees.Concat(employeesInactive).ToList();
                        }
                    }

                    #endregion

                    if (loadAccountInternal)
                    {
                        companyAccountsDTO = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs();
                        companyAccountsDTO.ForEach(f => f.ParentAccounts = f.GetParentAccounts(companyAccountsDTO));
                        if (useAccountHierarchy)
                            employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                    }

                    if (loadExtraFields)
                    {
                        var accountIds = companyAccountsDTO.Select(s => s.AccountId).ToList();
                        extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(entities, accountIds, (int)SoeEntityType.Account, reportResult.ActorCompanyId, true, true).ToDTOs();
                    }

                    foreach (var employee in employees)
                    {
                        if (useAccountHierarchy)
                        {
                            var connectedToAccounts = employeeAccounts.FindAll(r => r.EmployeeId == employee.EmployeeId);
                            var isAllAccountHasParent = false;
                            if (!connectedToAccounts.IsNullOrEmpty())
                            {
                                var connectedAccountIds = connectedToAccounts.Select(s => s.AccountId).ToList();
                                var accountDTOParents = companyAccountsDTO.Where(a => connectedAccountIds.Contains(a.AccountId)).Select(s => s.ParentAccounts).ToList();
                                if (accountDTOParents.TrueForAll(a => a.Count > 0))
                                    isAllAccountHasParent = true;

                                var connectedToAccountOrganisationHrItemList = new List<OrganisationHrItem>();
                                foreach (var connectedToAccount in connectedToAccounts)
                                {
                                    bool filteredOk = true;

                                    if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeAccount && !filteredOk)
                                        continue;

                                    #region Add OrganisationHrItem

                                    var orgItem = new OrganisationHrItem();

                                    orgItem.EmployeeNr = employee.EmployeeNr ?? String.Empty;
                                    orgItem.Name = employee.Name;
                                    orgItem.FirstName = employee.FirstName;
                                    orgItem.LastName = employee.LastName;
                                    orgItem.DateFrom = connectedToAccount.DateFrom;
                                    orgItem.DateTo = connectedToAccount.DateTo;
                                    orgItem.CategoryIsDefault = false;
                                    orgItem.CategoryName = string.Empty;
                                    orgItem.CategoryGroup = string.Empty;
                                    orgItem.SubCategory = string.Empty;
                                    orgItem.AccountIsPrimary = connectedToAccount.Default;
                                    AddAccountAnalysisFieldsAndExtraFields(orgItem, companyAccountsDTO, connectedToAccount, extraFieldRecords);
                                    connectedToAccountOrganisationHrItemList.Add(orgItem);

                                    #endregion
                                }
                                if (!isAllAccountHasParent)
                                {
                                    if (connectedToAccounts.Count > 1)
                                    {
                                        //Use for Hierarchy Exist in Stora Coop
                                        _reportDataOutput.OrganisationHrItems.Add(SetupDistinctOrganisationHrItemRow(connectedToAccountOrganisationHrItemList));
                                    }
                                }
                                else
                                {
                                    _reportDataOutput.OrganisationHrItems.AddRange(connectedToAccountOrganisationHrItemList);
                                }

                            }
                        }
                        else
                        {
                            var selectedEmployeeCategoryRecords = companyCategoryRecords.FindAll(r => r.RecordId == employee.EmployeeId); //Category

                            //If category filter is applied
                            if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeCategory && !selectionCategoryIds.IsNullOrEmpty())
                            {
                                selectedEmployeeCategoryRecords = selectedEmployeeCategoryRecords.Where(r => selectionCategoryIds.Contains(r.CategoryId)).ToList();
                            }

                            if (!selectedEmployeeCategoryRecords.IsNullOrEmpty())
                            {
                                foreach (var selectedEmployeeCategoryRecord in selectedEmployeeCategoryRecords)
                                {

                                    if (!selectedEmployeeCategoryRecord.CategoryReference.IsLoaded)
                                        selectedEmployeeCategoryRecord.CategoryReference.Load();

                                    if (!selectedEmployeeCategoryRecord.Category.CategoryGroupReference.IsLoaded)
                                        selectedEmployeeCategoryRecord.Category.CategoryGroupReference.Load();

                                    List<Category> subCategories = new List<Category>();
                                    if (!selectedEmployeeCategoryRecord.Category.Children.IsNullOrEmpty())
                                        subCategories = selectedEmployeeCategoryRecord.Category.Children.ToList();

                                    #region Item

                                    var orgItem = new OrganisationHrItem();

                                    orgItem.EmployeeNr = employee.EmployeeNr ?? String.Empty;
                                    orgItem.Name = employee.Name;
                                    orgItem.FirstName = employee.FirstName;
                                    orgItem.LastName = employee.LastName;
                                    orgItem.DateFrom = selectedEmployeeCategoryRecord?.DateFrom ?? null;
                                    orgItem.DateTo = selectedEmployeeCategoryRecord?.DateTo ?? null;
                                    orgItem.CategoryIsDefault = selectedEmployeeCategoryRecord?.Default ?? false;
                                    orgItem.CategoryName = selectedEmployeeCategoryRecord?.Category?.Name ?? string.Empty;
                                    orgItem.CategoryGroup = selectedEmployeeCategoryRecord?.Category?.CategoryGroup?.Name ?? string.Empty;
                                    orgItem.SubCategory = !subCategories.IsNullOrEmpty() ? string.Join(", ", subCategories.Select(t => t.Name)) : string.Empty;
                                    orgItem.AccountIsPrimary = false;
                                    AddAccountAnalysisFieldsAndExtraFields(orgItem, companyAccountsDTO, null, extraFieldRecords);

                                    _reportDataOutput.OrganisationHrItems.Add(orgItem);

                                    #endregion
                                }
                            }
                        }
                    }

                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        public void AddAccountAnalysisFieldsAndExtraFields(OrganisationHrItem orgItem, List<AccountDTO> companyAccountsDTO, EmployeeAccount connectedToAccount, List<ExtraFieldRecordDTO> extraFieldRecords)
        {
            if (connectedToAccount != null)
            {
                if (loadAccountInternal && companyAccountsDTO.Any(a => a.AccountId == connectedToAccount.AccountId))
                {
                    var accountDTO = companyAccountsDTO.FirstOrDefault(a => a.AccountId == connectedToAccount.AccountId);
                    if (accountDTO != null)
                    {
                        foreach (var parentAccount in accountDTO.ParentAccounts)
                        {
                            orgItem.AccountAnalysisFields.Add(new AccountAnalysisField(parentAccount));
                            if (loadExtraFields && extraFieldRecords.Any())
                            {
                                orgItem.ExtraFieldAnalysisFields.AddRange(GetExtraFieldAnalysisField(extraFieldRecords, parentAccount.AccountId));
                            }

                        }

                        orgItem.AccountAnalysisFields.Add(new AccountAnalysisField(accountDTO));
                        if (loadExtraFields && extraFieldRecords.Any())
                            orgItem.ExtraFieldAnalysisFields.AddRange(GetExtraFieldAnalysisField(extraFieldRecords, accountDTO.AccountId));
                    }
                }
            }
            else
            {
                orgItem.AccountAnalysisFields = new List<AccountAnalysisField>();
                orgItem.ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            }

        }

        public OrganisationHrItem SetupDistinctOrganisationHrItemRow(List<OrganisationHrItem> connectedToAccountOrganisationHrItemList)
        {
            var item = connectedToAccountOrganisationHrItemList.FirstOrDefault();
            if (item == null)
                return null;

            var organisationHrItemObject = new OrganisationHrItem
            {
                EmployeeNr = item.EmployeeNr,
                Name = item.Name,
                FirstName = item.FirstName,
                LastName = item.LastName,
                DateFrom = item.DateFrom,
                DateTo = item.DateTo,
                CategoryIsDefault = false,
                CategoryName = string.Empty,
                CategoryGroup = string.Empty,
                SubCategory = string.Empty,
                AccountIsPrimary = item.AccountIsPrimary,
                AccountAnalysisFields = item.AccountAnalysisFields,
                ExtraFieldAnalysisFields = item.ExtraFieldAnalysisFields
            };

            foreach (var listItem in connectedToAccountOrganisationHrItemList)
            {
                if (connectedToAccountOrganisationHrItemList.IndexOf(listItem) != 0)
                {
                    organisationHrItemObject.AccountAnalysisFields = organisationHrItemObject.AccountAnalysisFields.Union(listItem.AccountAnalysisFields).ToList();
                    organisationHrItemObject.ExtraFieldAnalysisFields = organisationHrItemObject.ExtraFieldAnalysisFields.Union(listItem.ExtraFieldAnalysisFields).ToList();
                }
            }

            return organisationHrItemObject;
        }

        private List<ExtraFieldAnalysisField> GetExtraFieldAnalysisField(List<ExtraFieldRecordDTO> extraFieldRecords, int accountId)
        {
            var extraFieldList = new List<ExtraFieldAnalysisField>();
            var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == accountId).ToList();
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

    public class OrganisationHrReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_OrganisationHrMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public OrganisationHrReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_OrganisationHrMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_OrganisationHrMatrixColumns.Unknown;
        }
    }

    public class OrganisationHrReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<OrganisationHrReportDataReportDataField> Columns { get; set; }

        public OrganisationHrReportDataInput(CreateReportResult reportResult, List<OrganisationHrReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class OrganisationHrReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<OrganisationHrItem> OrganisationHrItems { get; set; }
        public OrganisationHrReportDataInput Input { get; set; }
        public List<Account> AccountInternals { get; set; }

        public OrganisationHrReportDataOutput(OrganisationHrReportDataInput input)
        {
            this.OrganisationHrItems = new List<OrganisationHrItem>();
            this.Input = input;
            this.AccountInternals = new List<Account>();
        }
    }
}
