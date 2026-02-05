using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class StaffingneedsFrequencyReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly StaffingneedsFrequencyReportDataOutput _reportDataOutput;
        private readonly StaffingneedsFrequencyReportDataInput _reportDataInput;

        public StaffingneedsFrequencyReportData(ParameterObject parameterObject, StaffingneedsFrequencyReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new StaffingneedsFrequencyReportDataOutput(reportDataInput);
        }

        public StaffingneedsFrequencyReportDataOutput CreateOutput(CreateReportResult reportResult)
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
                return new ActionResult(true);

            List<int> accountIds = null;
            selectionDateTo = CalendarUtility.GetEndOfDay(selectionDateTo);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (useAccountHierarchy)
            {
                AccountRepository accountRepository = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entitiesReadOnly, base.ActorCompanyId, base.RoleId, base.UserId, selectionDateFrom, selectionDateTo);
                List<AccountDTO> allAccountInternals = accountRepository?.GetAccounts() ?? new List<AccountDTO>();

                if (accountIds == null)
                    accountIds = allAccountInternals.Select(s => s.AccountId).ToList();
                else
                    accountIds = accountIds.Where(w => allAccountInternals.Select(s => s.AccountId).Contains(w)).ToList();
            }

            if (FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_Dashboard, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId, entitiesReadOnly))
            {
                var frequencies = TimeScheduleManager.GetStaffingNeedsFrequencys(entitiesReadOnly, base.ActorCompanyId, selectionDateFrom, selectionDateTo);
                if (!frequencies.IsNullOrEmpty())
                {
                    if (accountIds != null)
                        frequencies = frequencies.Where(w => w.Amount != 0 && ((w.AccountId.HasValue && accountIds.Contains(w.AccountId.Value)) || (w.ParentAccountId.HasValue && accountIds.Contains(w.ParentAccountId.Value)))).ToList();

                    foreach (var freq in frequencies)
                    {
                        var account = freq.AccountId.HasValue ? _reportDataInput.AccountInternals.FirstOrDefault(f => f.AccountId == freq.AccountId.Value) : null;
                        var parentAccount = freq.ParentAccountId.HasValue ? _reportDataInput.AccountInternals.FirstOrDefault(f => f.AccountId == freq.ParentAccountId.Value) : null;

                        StaffingneedsFrequencyItem item = new StaffingneedsFrequencyItem()
                        {
                            Amount = freq.Amount,
                            AccountNumber = account != null ? account.AccountNr : string.Empty,
                            AccountName = account != null ? account.Name : string.Empty,
                            AccountParentNumber = parentAccount != null ? parentAccount.AccountNr : string.Empty,
                            AccountParentName = parentAccount != null ? parentAccount.Name : string.Empty,
                            Cost = freq.Cost,
                            ExternalCode = freq.ExternalCode,
                            ParentExternalCode = freq.ParentExternalCode,
                            NbrOfCustomers = freq.NbrOfCustomers,
                            NbrOfItems = freq.NbrOfItems,
                            NbrOfMinutes = freq.NbrOfMinutes,
                            TimeFrom = freq.TimeFrom,
                            TimeTo = freq.TimeTo,
                            FrequencyType = (FrequencyType)freq.FrequencyType
                        };

                        _reportDataOutput.StaffingneedsFrequencyItems.Add(item);
                    }
                }
            }

            return new ActionResult();
        }
    }

    public class StaffingneedsFrequencyReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_StaffingneedsFrequencyMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public StaffingneedsFrequencyReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_StaffingneedsFrequencyMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_StaffingneedsFrequencyMatrixColumns.Unknown;
        }
    }

    public class StaffingneedsFrequencyReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<StaffingneedsFrequencyItem> StaffingneedsFrequencyItems { get; set; }
        public StaffingneedsFrequencyReportDataInput Input { get; set; }

        public StaffingneedsFrequencyReportDataOutput(StaffingneedsFrequencyReportDataInput input)
        {
            this.StaffingneedsFrequencyItems = new List<StaffingneedsFrequencyItem>();
            this.Input = input;
        }
    }

    public class StaffingneedsFrequencyReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<StaffingneedsFrequencyReportDataReportDataField> Columns { get; set; }
        public List<AccountDimDTO> AccountDims { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }

        public StaffingneedsFrequencyReportDataInput(CreateReportResult reportResult, List<StaffingneedsFrequencyReportDataReportDataField> columns, List<AccountDimDTO> accountDims, List<AccountDTO> accountInternals = null)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.AccountDims = accountDims;
            this.AccountInternals = accountInternals;
        }
    }
}
