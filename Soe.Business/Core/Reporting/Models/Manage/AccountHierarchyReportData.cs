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
    public class AccountHierarchyReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly AccountHierarchyReportDataOutput _reportDataOutput;
        private readonly AccountHierarchyReportDataInput _reportDataInput;

        private bool fillHolesInDim;
        private bool addEmptyAccounts;
        private AccountDim defaultAccountDim;

        private bool loadExecutive => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveUsername ||
            a.Column == TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveName ||
            a.Column == TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveEmail
        );

        public AccountHierarchyReportData(ParameterObject parameterObject, AccountHierarchyReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new AccountHierarchyReportDataOutput(reportDataInput);
        }

        public AccountHierarchyReportDataOutput CreateOutput(CreateReportResult reportResult, bool fillHolesInDim = false, bool addEmptyAccounts = false)
        {
            base.reportResult = reportResult;
            this.fillHolesInDim = fillHolesInDim;
            this.addEmptyAccounts = addEmptyAccounts;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }       

        private void RecursiveHierachy(CompEntities entities, AccountDimDTO childDim, List<AccountDimDTO> dims, List<AccountDTO> accounts, AccountHierarchyItem item, List<int> executiveAccIds, bool addEmptyAccounts = false)
        {
            executiveAccIds.Add(item.AccountField.AccountId);
            if (loadExecutive)
            {
                var users = UserManager.GetExecutiveForAccount(entities, reportResult.ActorCompanyId, executiveAccIds, DateTime.Today, DateTime.Today);
                var user = users.FirstOrDefault();
                if (user != null)
                {
                    item.AccountField.ExecutiveUserName = user.LoginName;
                    item.AccountField.ExecutiveName = user.Name;
                    item.AccountField.ExecutiveEmail = user.Email;
                }
            }

            if (childDim == null)
                return;

            if (childDim.AccountDimId == defaultAccountDim.AccountDimId)
                executiveAccIds = new List<int>() { item.AccountField.AccountId };

            var dimAccounts = accounts.Where(w => w.AccountDimId == childDim.AccountDimId && (!w.ParentAccountId.HasValue || w.ParentAccountId == item.AccountField.AccountId)).ToList();

            bool noParent = false;
            if (!dimAccounts.Any())
            {
                noParent = true;
                dimAccounts = accounts.Where(w => w.AccountDimId == childDim.AccountDimId && !w.ParentAccountId.HasValue).ToList();
            }

            if (addEmptyAccounts)
            {
                var emptyAccount = new AccountDTO() { AccountId = childDim.AccountDimId * -1, Name = "", AccountDimId = childDim.AccountDimId, AccountNr = "0", ParentAccountId = (noParent ? (int?)null : item.AccountField.AccountId) };
                dimAccounts.Add(emptyAccount);
                accounts.Add(emptyAccount);
            }

            foreach (var child in dimAccounts)
            {
                var childItem = new AccountHierarchyItem() { AccountField = new AccountAnalysisField(child) };
                var childDimension = dims.FirstOrDefault(w => w.ParentAccountDimId == child.AccountDimId);

                if (childDim != null)
                    RecursiveHierachy(entities, childDimension, dims, accounts, childItem, executiveAccIds, addEmptyAccounts);

                item.ChildrenAccountHierarchyItems.Add(childItem);
            }
        }

        public ActionResult LoadData()
        {
            #region Prereq

            TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            #endregion  

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();
            int langId = GetLangId();
            Dictionary<int, string> sysReport = base.GetTermGroupDict(TermGroup.SysReportTemplateType, langId);
            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            bool isInsight = matrixColumnsSelection.InsightId != 0;
            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var dims = base.GetAccountDimsFromCache(entitiesReadonly, DataCache.CacheConfig.Company(reportResult.ActorCompanyId));
                    dims.CalculateLevels();

                    defaultAccountDim = AccountManager.GetDefaultEmployeeAccountDim(reportResult.ActorCompanyId);
                    List<AccountDTO> accounts = this._reportDataInput.AccountInternals ?? AccountManager.GetAccountsByCompany(reportResult.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs(true, true);
                    List<AccountHierarchyItem> items = new List<AccountHierarchyItem>();
                    List<int> selectionDimIds = new List<int>();
                    List<AccountDimDTO> selectedDims = new List<AccountDimDTO>();

                    foreach (var col in matrixColumnsSelection.Columns)
                    {
                        var idValue = GetIdValueFromColumn(col);

                        if (idValue != 0 && col.Options?.Key != null && int.TryParse(col.Options.Key, out int value) && !selectionDimIds.Any(a => a == value))
                            selectionDimIds.Add(value);
                    }

                    if (selectionDimIds.Any())
                    {
                        if (!fillHolesInDim)
                        {
                            selectedDims = dims.Where(w => selectionDimIds.Contains(w.AccountDimId)).ToList();
                        }
                        else
                        {
                            var selDims = dims.Where(w => selectionDimIds.Contains(w.AccountDimId)).ToList();
                            var attachedDims = new List<AccountDimDTO>();

                            foreach (var dim in dims.Where(w => w.IsInternal).OrderBy(o => o.Level))
                            {
                                bool attached = false;
                                if (selectionDimIds.Contains(dim.AccountDimId))
                                {
                                    attachedDims.Add(dim);
                                    attached = true;
                                }

                                if (!attachedDims.Any())
                                    continue;

                                if (!attached && attachedDims.Count == selectionDimIds.Count)
                                    continue;

                                selectedDims.Add(dim);
                            }
                        }
                    }

                    bool stopDim = false;
                    foreach (var dim in selectedDims.Where(w => w.IsInternal).OrderBy(o => o.Level))
                    {
                        if (stopDim)
                            continue;
                        var accountsOnTopLevel = accounts.Where(w => w.AccountDimId == dim.AccountDimId).ToList();
                        var filter = _reportDataInput.FilterAccountIds ?? accountsOnTopLevel.Select(s => s.AccountId).ToList();
                        accountsOnTopLevel = accountsOnTopLevel.Where(w => filter.Contains(w.AccountId) || accountsOnTopLevel.Count == 1).ToList();

                        foreach (var account in accountsOnTopLevel)
                        {
                            stopDim = true;
                            var item = new AccountHierarchyItem();
                            item.AccountField = new AccountAnalysisField(account);
                            var childDim = selectedDims.FirstOrDefault(w => w.ParentAccountDimId == dim.AccountDimId);
                            RecursiveHierachy(entities, childDim, selectedDims, accounts.ToList(), item, new List<int>(), addEmptyAccounts);
                            items.Add(item);
                        }

                        List<AccountHierarchyItem> connectedItems = new List<AccountHierarchyItem>();
                        int rowNumber = 0;
                        foreach (var item in items)
                            connectedItems.AddRange(item.GetHierarchyItems(ref rowNumber, null));

                        foreach (var key in connectedItems.GroupBy(g => g.RowNumber))
                            _reportDataOutput.AccountHierarchyItems.Add(Tuple.Create(key.Key, key.ToList()));
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
    }

    public class AccountHierarchyReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_AccountHierarchyMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public AccountHierarchyReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_AccountHierarchyMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_AccountHierarchyMatrixColumns.Unknown;
        }
    }

    public class AccountHierarchyReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<Tuple<int, List<AccountHierarchyItem>>> AccountHierarchyItems { get; set; }
        public AccountHierarchyReportDataInput Input { get; set; }

        public AccountHierarchyReportDataOutput(AccountHierarchyReportDataInput input)
        {
            this.AccountHierarchyItems = new List<Tuple<int, List<AccountHierarchyItem>>>();
            this.Input = input;
        }
    }

    public class AccountHierarchyReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AccountHierarchyReportDataField> Columns { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        public List<int> FilterAccountIds { get; set; }

        public AccountHierarchyReportDataInput(CreateReportResult reportResult, List<AccountHierarchyReportDataField> columns, List<AccountDimDTO> accountDims, List<AccountDTO> accountInternals, List<int> filterAccountIds = null)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.AccountInternals = accountInternals;
            this.FilterAccountIds = filterAccountIds;
        }
    }
}
