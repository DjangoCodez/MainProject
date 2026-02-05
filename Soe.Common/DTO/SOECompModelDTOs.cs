using Newtonsoft.Json;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    #region Tables

    #region BaseClasses

    public abstract class SettingsDTO<T, J>
        where T : struct
        where J : struct
    {
        public SettingsDTO()
        {
        }

        public int Id { get; set; }
        public int TimeTerminalId { get; set; }

        [TsIgnore]
        public T Type { get; set; }
        [TsIgnore]
        public J DataType { get; set; }
        public string Name { get; set; }

        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Account

    [TSInclude]
    public class AccountDTO
    {
        public static readonly char HIERARCHYDELIMETER = '-';

        public int AccountId { get; set; }
        public int AccountDimId { get; set; }
        public string AccountNr { get; set; }
        public int? ParentAccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalCode { get; set; }
        public bool HierarchyOnly { get; set; }
        public int AccountTypeSysTermId { get; set; }
        public bool HierarchyNotOnSchedule { get; set; }

        // Extensions
        public AccountDimDTO AccountDim { get; set; }
        public int AccountDimNr { get; set; }
        public string NumberName
        {
            get
            {
                return $"{StringUtility.NullToEmpty(AccountNr).Trim()} {StringUtility.NullToEmpty(Name).Trim()}";
            }
        }
        [TsIgnore]
        public string DimNameNumberAndName
        {
            get
            {
                if (this.AccountDim != null)
                    return $"{AccountDim.Name} {this.NumberName}";
                else
                    return this.NumberName;
            }
        }
        public int AmountStop { get; set; }
        public bool UnitStop { get; set; }
        public string Unit { get; set; }
        public bool RowTextStop { get; set; }
        public List<int?> GrossProfitCode { get; set; }
        public int? AttestWorkFlowHeadId { get; set; }
        public SoeEntityState State { get; set; }
        public bool IsAccrualAccount { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }

        #region Account Hierarchy

        [TsIgnore]
        public string AccountIdWithDelimeter
        {
            get
            {
                return $"{this.AccountId}{HIERARCHYDELIMETER}";
            }
        }
        public bool IsAbstract { get; set; }
        public bool HasVirtualParent { get; set; }
        public int? VirtualParentAccountId { get; set; }
        [TsIgnore]
        public List<AccountDTO> ParentAccounts { get; set; }

        private Dictionary<string, AccountHierarchyRow> hierarchyRows = null;
        private Dictionary<string, AccountHierarchyRow> hierarchyRowsByUniqueId = null;
        private AccountHierarchyRow hierarchyRow = null;
        [TsIgnore]
        public Dictionary<int, string> ParentHierachy
        {
            get
            {
                return this.hierarchyRow?.ParentHierachy;
            }
        }
        [TsIgnore]
        public int NoOParentHierachys
        {
            get
            {
                return this.ParentHierachy?.Count ?? 0;
            }
        }
        [TsIgnore]
        public string HierachyId
        {
            get
            {
                return this.hierarchyRow?.HierarchyId;
            }
        }
        [TsIgnore]
        public string HierachyName
        {
            get
            {
                return this.hierarchyRow?.HierarchyName;
            }
        }
        [TsIgnore]
        public string AccountHierarchyUniqueId
        {
            get
            {
                return this.hierarchyRow?.UniqueId;
            }
        }

        private AccountHierarchyRow GetHierarchyRowByUniqueId(string uniqueId)
        {
            return this.hierarchyRowsByUniqueId?.GetValue(uniqueId);
        }
        private AccountHierarchyRow GetHierarchyRowByHierarchyId(string hierarchyId)
        {
            return this.hierarchyRows?.GetValue(hierarchyId);
        }
        private AccountHierarchyRow GetHierarchyRowEndingWithHierarchyId(string hierarchyId)
        {
            return this.hierarchyRows?.FirstOrDefault(r => r.Key.EndsWith(hierarchyId)).Value;
        }
        public void SetAccountHierarchy(AccountDTO parentAccount)
        {
            Dictionary<int, string> parentHierarchy = this.BuildParentHierarchy(parentAccount);
            string hierarchyId = String.Join(HIERARCHYDELIMETER.ToString(), parentHierarchy.Keys);
            string hierarchyName = String.Join(HIERARCHYDELIMETER.ToString(), parentHierarchy.Values);

            AccountHierarchyRow row = new AccountHierarchyRow(hierarchyId, hierarchyName, parentHierarchy, this.ParentAccountId);
            AddAccountHierarchy(row);
            ResetAccountHierarchy(row);

        }
        public bool ContainsHiearchy(string hierarchyId)
        {
            return this.NoOParentHierachys > 1 ? this.hierarchyRows.ContainsKey(hierarchyId) : this.HierachyId == hierarchyId;
        }
        public void AddAccountHierarchy(string hierarchyId, string hierarchyName, Dictionary<int, string> parentHierarchy, int? parentAccountId)
        {
            AddAccountHierarchy(new AccountHierarchyRow(hierarchyId, hierarchyName, parentHierarchy, parentAccountId));
        }
        private void AddAccountHierarchy(AccountHierarchyRow row)
        {
            if (row == null)
                return;
            if (this.hierarchyRows == null)
            {
                this.hierarchyRows = new Dictionary<string, AccountHierarchyRow>();
                this.hierarchyRowsByUniqueId = new Dictionary<string, AccountHierarchyRow>();
            }

            if (!this.hierarchyRows.ContainsKey(row.HierarchyId))
                this.hierarchyRows.Add(row.HierarchyId, row);
            if (!this.hierarchyRowsByUniqueId.ContainsKey(row.UniqueId))
                this.hierarchyRowsByUniqueId.Add(row.UniqueId, row);
        }
        public Dictionary<string, string> GetHierarchys()
        {
            if (this.hierarchyRows?.Count > 0)
                return this.hierarchyRows.Values.ToDictionary(k => k.HierarchyId, v => v.HierarchyName);
            else
                return new Dictionary<string, string>() { { this.HierachyId, this.HierachyName } };
        }
        public void ResetAccountHierarchy(IEnumerable<AccountDTO> parentAccounts)
        {
            if (this.hierarchyRows == null || this.hierarchyRows.Count <= 1)
                return;

            List<int> parentAccountIds = parentAccounts?.Select(a => a.AccountId).ToList();
            if (parentAccountIds.IsNullOrEmpty())
                return;

            string hierarchyId = String.Join(HIERARCHYDELIMETER.ToString(), parentAccountIds.Select(id => id.ToString())) + HIERARCHYDELIMETER + this.AccountId;
            ResetAccountHierarchy(this.GetHierarchyRowEndingWithHierarchyId(hierarchyId));
        }
        public void ResetAccountHierarchy(string uniqueId)
        {
            AccountHierarchyRow row = this.GetHierarchyRowByUniqueId(uniqueId);
            if (row != null)
                ResetAccountHierarchy(row);
        }
        private void ResetAccountHierarchy(AccountHierarchyRow row)
        {
            if (row != null)
            {
                this.hierarchyRow = row;
                this.ParentAccountId = row.ParentAccountId;
            }
        }
        public bool HasParentAndReset(AccountDTO parentAccount)
        {
            if (this.AccountDim?.Level != parentAccount.AccountDim?.Level + 1)
                return false;

            string hierarchyId = BuildHierarchyId(parentAccount);
            AccountHierarchyRow row = this.GetHierarchyRowByHierarchyId(hierarchyId);
            if (row != null)
                ResetAccountHierarchy(row);
            return row != null;
        }
        public bool IsParent(AccountDTO parentAccount, int level)
        {
            return this.NoOParentHierachys == level && this.HierachyId == this.BuildHierarchyId(parentAccount);
        }

        private Dictionary<int, string> BuildParentHierarchy(AccountDTO parentAccount = null)
        {
            Dictionary<int, string> parentHierarchy = new Dictionary<int, string>();

            if (parentAccount?.ParentHierachy != null)
                parentHierarchy.AddRange(parentAccount.ParentHierachy);
            else if (parentAccount != null && (this.ParentHierachy == null || !this.ParentHierachy.ContainsKey(parentAccount.AccountId)))
                parentHierarchy.Add(parentAccount.AccountId, parentAccount.Name);

            if (!parentHierarchy.ContainsKey(this.AccountId))
                parentHierarchy.Add(this.AccountId, this.Name);

            return parentHierarchy;
        }
        private string BuildHierarchyId(AccountDTO parentAccount)
        {
            if (parentAccount == null)
                return string.Empty;
            return parentAccount.HierachyId + HIERARCHYDELIMETER + this.AccountId;
        }

        #endregion
    }

    public class AccountHierarchyRow
    {
        public string UniqueId { get; }
        public string HierarchyId { get; }
        public string HierarchyName { get; }
        public Dictionary<int, string> ParentHierachy { get; }
        public int? ParentAccountId { get; set; }

        public AccountHierarchyRow(string hierarchyId, string hierarchyName, Dictionary<int, string> parentHierachy, int? parentAccountId)
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.HierarchyId = hierarchyId;
            this.HierarchyName = hierarchyName;
            this.ParentHierachy = parentHierachy;
            this.ParentAccountId = parentAccountId;
        }
    }

    [TSInclude]
    public class AccountGridDTO
    {
        public int AccountId { get; set; }
        public int AccountDimId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public int? AccountTypeSysTermId { get; set; }
        public string Type { get; set; }
        public int? SysVatAccountId { get; set; }
        public string VatType { get; set; }
        public string ExternalCode { get; set; }
        public string ParentAccountName { get; set; }
        public decimal Balance { get; set; }
        public SoeEntityState State { get; set; }

        public bool IsLinkedToShiftType { get; set; }
        public string Categories { get; set; }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value == true ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    [TSInclude]
    public class AccountEditDTO
    {
        // Account
        public int AccountId { get; set; }
        public int AccountDimId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AttestWorkFlowHeadId { get; set; }
        public int? ParentAccountId { get; set; }
        public bool UseVatDeductionDim { get; set; }
        public bool UseVatDeduction { get; set; }
        public decimal VatDeduction { get; set; }
        public string ExternalCode { get; set; }
        public string AccountHierachyPayrollExportExternalCode { get; set; }
        public string AccountHierachyPayrollExportUnitExternalCode { get; set; }
        public bool HierarchyOnly { get; set; }
        public bool isAccrualAccount { get; set; }
        public bool HierarchyNotOnSchedule { get; set; }

        // AccountStd
        public int? SysAccountSruCode1Id { get; set; }
        public int? SysAccountSruCode2Id { get; set; }
        public int? SysVatAccountId { get; set; }
        public int AccountTypeSysTermId { get; set; }
        public int AmountStop { get; set; }
        public string Unit { get; set; }
        public bool UnitStop { get; set; }
        public string SieKpTyp { get; set; }
        public bool? ExcludeVatVerification { get; set; }
        public bool RowTextStop { get; set; }

        //References
        public List<AccountMappingDTO> AccountMappings { get; set; }

        // Extensions
        public bool Active { get; set; }
        public bool IsStdAccount { get; set; }

        public List<AccountInternalDTO> AccountInternals { get; set; }
    }

    public class AccountIODTO
    {
        public int AccountDimNr { get; set; }
        public int AccountDimSieNr { get; set; }
        public string AccountDimName { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public string SysVatAccountNr { get; set; }
        public bool AmountStop { get; set; }
        public string QuantityUnit { get; set; }
        public bool QuantityStop { get; set; }
        public string SieKpTyp { get; set; }
        public bool ExcludeVatVerification { get; set; }
        public string ExternalCode { get; set; }
        public string ParentAccountNr { get; set; }

        public string SruCode1 { get; set; }
        public string SruCode2 { get; set; }
        public string SruCode3 { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }

        public bool AccountDim2Mandatory { get; set; }
        public bool AccountDim3Mandatory { get; set; }
        public bool AccountDim4Mandatory { get; set; }
        public bool AccountDim5Mandatory { get; set; }
        public bool AccountDim6Mandatory { get; set; }

        public bool AccountDim2Stop { get; set; }
        public bool AccountDim3Stop { get; set; }
        public bool AccountDim4Stop { get; set; }
        public bool AccountDim5Stop { get; set; }
        public bool AccountDim6Stop { get; set; }
        public string AccountDim2Default { get; set; }
        public string AccountDim3Default { get; set; }
        public string AccountDim4Default { get; set; }
        public string AccountDim5Default { get; set; }
        public string AccountDim6Default { get; set; }

    }
    [TSInclude]
    public class AccountSmallDTO
    {
        public int AccountId { get; set; }
        public int AccountDimId { get; set; }
        public int? ParentAccountId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description
        {
            get
            {
                return Number + ". " + Name;
            }
        }
        public decimal Percent { get; set; }
    }

    [TSInclude]
    public class AccountNumberNameDTO
    {
        public int AccountId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string NumberName { get; set; }
    }

    [TSInclude]
    public class AccountInternalDTO : IAccountId
    {
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public int AccountDimId { get; set; }
        public int AccountDimNr { get; set; }
        public int? SysSieDimNr { get; set; }
        public int SysSieDimNrOrAccountDimNr { get; set; }
        public int MandatoryLevel { get; set; }
        public bool UseVatDeduction { get; set; }
        public decimal VatDeduction { get; set; }

        // Extensions
        public AccountDTO Account { get; set; }
    }

    [TSInclude]
    public class AccountingSettingsRowDTO
    {
        public int Type { get; set; }

        public int AccountDim1Nr { get; set; }
        public int Account1Id { get; set; }
        public string Account1Nr { get; set; }
        public string Account1Name { get; set; }
        public int AccountDim2Nr { get; set; }
        public int Account2Id { get; set; }
        public string Account2Nr { get; set; }
        public string Account2Name { get; set; }
        public int AccountDim3Nr { get; set; }
        public int Account3Id { get; set; }
        public string Account3Nr { get; set; }
        public string Account3Name { get; set; }
        public int AccountDim4Nr { get; set; }
        public int Account4Id { get; set; }
        public string Account4Nr { get; set; }
        public string Account4Name { get; set; }
        public int AccountDim5Nr { get; set; }
        public int Account5Id { get; set; }
        public string Account5Nr { get; set; }
        public string Account5Name { get; set; }
        public int AccountDim6Nr { get; set; }
        public int Account6Id { get; set; }
        public string Account6Nr { get; set; }
        public string Account6Name { get; set; }

        public decimal Percent { get; set; }

        public void SetAccountValues(int position, int accountDimNr, int accountId, string accountNr, string accountName)
        {
            if (position == 2)
            {
                this.AccountDim2Nr = accountDimNr;
                this.Account2Id = accountId;
                this.Account2Nr = accountNr;
                this.Account2Name = accountName;
            }
            else if (position == 3)
            {
                this.AccountDim3Nr = accountDimNr;
                this.Account3Id = accountId;
                this.Account3Nr = accountNr;
                this.Account3Name = accountName;
            }
            else if (position == 4)
            {
                this.AccountDim4Nr = accountDimNr;
                this.Account4Id = accountId;
                this.Account4Nr = accountNr;
                this.Account4Name = accountName;
            }
            else if (position == 5)
            {
                this.AccountDim5Nr = accountDimNr;
                this.Account5Id = accountId;
                this.Account5Nr = accountNr;
                this.Account5Name = accountName;
            }
            else if (position == 6)
            {
                this.AccountDim6Nr = accountDimNr;
                this.Account6Id = accountId;
                this.Account6Nr = accountNr;
                this.Account6Name = accountName;
            }
        }

        public TimeIntervalAccountingDTO CreateTimeIntervalAccountingDTO(DateTime startTime, DateTime stopTime)
        {
            List<int> accountIds = new List<int>();

            TryAddAccountId(this.Account2Id);
            TryAddAccountId(this.Account3Id);
            TryAddAccountId(this.Account4Id);
            TryAddAccountId(this.Account5Id);
            TryAddAccountId(this.Account6Id);

            return new TimeIntervalAccountingDTO
            {
                StartTime = startTime,
                StopTime = stopTime,
                AccountInternalIds = accountIds
            };

            void TryAddAccountId(int? accountId)
            {
                if (accountId.HasValue && accountId.Value > 0)
                    accountIds.Add(accountId.Value);
            }
        }
    }

    public class TimeIntervalAccountingDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public IEnumerable<int> AccountInternalIds { get; set; }

        public bool IsInInterval(DateTime startTime, DateTime stopTime) => startTime >= this.StartTime && stopTime <= this.StopTime;
    }

    #endregion

    #region AccountBalance
    [TSInclude]
    public class AccountBalanceDTO
    {
        public int AccountYearId { get; set; }
        public int AccountId { get; set; }
        public decimal Balance { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public string AccountYearStr { get; set; }
        public string BalanceStr { get; set; }
        public string ModifiedCreatedStr { get; set; }
    }

    #endregion

    #region AccountDim

    [TSInclude]
    public class AccountDimDTO
    {
        public int AccountDimId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? SysAccountStdTypeParentId { get; set; }
        public int? ParentAccountDimId { get; set; }

        public int? SysSieDimNr { get; set; }
        public int AccountDimNr { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? MinChar { get; set; }
        public int? MaxChar { get; set; }
        public bool LinkedToProject { get; set; }
        public bool LinkedToShiftType { get; set; }
        public string ParentAccountDimName { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool UseInSchedulePlanning { get; set; }
        public bool ExcludeinAccountingExport { get; set; }
        public bool ExcludeinSalaryReport { get; set; }
        public bool UseVatDeduction { get; set; }
        public bool MandatoryInOrder { get; set; }
        public bool MandatoryInCustomerInvoice { get; set; }
        public bool OnlyAllowAccountsWithParent { get; set; }
        [TsIgnore]
        public bool AllowAccountsWithoutParent => !OnlyAllowAccountsWithParent;

        public List<AccountDTO> Accounts { get; set; }

        // Extensions
        public bool IsStandard
        {
            get
            {
                return AccountDimNr == Constants.ACCOUNTDIM_STANDARD;
            }
        }
        public bool IsInternal
        {
            get
            {
                return AccountDimNr != Constants.ACCOUNTDIM_STANDARD;
            }
        }
        public int Level { get; set; }
        [TsIgnore]
        public bool IsSelected { get; set; }
    }

    [TSInclude]
    public class AccountDimSmallDTO
    {
        public int AccountDimId { get; set; }
        public int AccountDimNr { get; set; }
        public string Name { get; set; }
        public int? ParentAccountDimId { get; set; }
        public bool LinkedToShiftType { get; set; }
        public bool MandatoryInOrder { get; set; }
        public bool MandatoryInCustomerInvoice { get; set; }
        public List<AccountDTO> Accounts { get; set; }
        public int Level { get; set; }
        public bool IsAboveCompanyStdSetting { get; set; }

        [TsIgnore]
        public List<AccountDTO> CurrentSelectableAccounts { get; set; }
    }

    [TSInclude]
    public class AccountDimGridDTO
    {
        public int AccountDimId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AccountDimNr { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string ParentAccountDimName { get; set; }
        public int? SysSieDimNr { get; set; }
        public bool UseInSchedulePlanning { get; set; }
        public bool ExcludeinAccountingExport { get; set; }
        public bool ExcludeinSalaryReport { get; set; }
        public bool OnlyAllowAccountsWithParent { get; set; }

        public SoeEntityState State { get; set; }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    #endregion

    #region AccountDistribution

    #region AccountDistributionEntry

    [TSInclude]
    public class AccountDistributionEntryDTO
    {
        public int AccountDistributionEntryId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? AccountDistributionHeadId { get; set; }
        public string AccountDistributionHeadName { get; set; }
        public TermGroup_AccountDistributionTriggerType TriggerType { get; set; }
        public TermGroup_AccountDistributionPeriodType PeriodType { get; set; }
        public DateTime Date { get; set; }
        public int? VoucherHeadId { get; set; }
        public int? SupplierInvoiceId { get; set; }
        public int? InventoryId { get; set; }
        public int State { get; set; }
        public TermGroup_AccountDistributionRegistrationType RegistrationType { get; set; }
        public int? SourceVoucherHeadId { get; set; }
        public long? SourceVoucherNr { get; set; }
        public int? SourceSupplierInvoiceId { get; set; }
        public int? SourceSupplierInvoiceSeqNr { get; set; }
        public int? SourceCustomerInvoiceId { get; set; }
        public int? SourceCustomerInvoiceSeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public int? SourceRowId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public bool IsSelected { get; set; }
        public bool IsSelectEnable { get; set; }
        public bool PeriodError { get; set; }
        public int? VoucherSeriesTypeId { get; set; }
        public string TypeName { get; set; }
        public string InventoryName { get; set; }
        public string InventoryNr { get; set; }
        public string Status { get; set; }
        public long? VoucherNr { get; set; }
        public int? AccountYearId { get; set; }
        public decimal Amount { get; set; }
        public decimal WriteOffAmount { get; set; }
        public decimal WriteOffYear { get; set; }
        public decimal WriteOffTotal { get; set; }
        public decimal WriteOffSum { get; set; }
        public decimal CurrentAmount { get; set; }
        public bool DetailVisible { get; set; }
        public int RowId { get; set; }
        public List<AccountDistributionEntryRowDTO> AccountDistributionEntryRowDTO { get; set; }
        public string Categories { get; set; }
        public DateTime? InventoryPurchaseDate { get; set; }
        public DateTime? InventoryWriteOffDate { get; set; }
        public string InventoryDescription { get; set; }
        public string InventoryNotes { get; set; }
        public bool IsReversal { get; set; } = false;
    }

    #endregion

    #region AccountDistributionEntryRow

    [TSInclude]
    public class AccountDistributionEntryRowDTO
    {
        public int AccountDistributionEntryRowId { get; set; }
        public int AccountDistributionEntryId { get; set; }

        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmountCurrency { get; set; }
        public decimal CreditAmountCurrency { get; set; }
        public decimal DebitAmountEntCurrency { get; set; }
        public decimal CreditAmountEntCurrency { get; set; }
        public decimal DebitAmountLedgerCurrency { get; set; }
        public decimal CreditAmountLedgerCurrency { get; set; }

        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public string Dim1DimName { get; set; }

        // Extensions
        public decimal SameBalance { get; set; }
        public decimal OppositeBalance { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public string Dim2DimName { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public string Dim3DimName { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public string Dim4DimName { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public string Dim5DimName { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public string Dim6DimName { get; set; }
    }

    public class AccountDistributionEntryRowIODTO
    {
        public int AccountDistributionEntryRowId { get; set; }
        public int AccountDistributionEntryId { get; set; }

        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmountCurrency { get; set; }
        public decimal CreditAmountCurrency { get; set; }
        public decimal DebitAmountEntCurrency { get; set; }
        public decimal CreditAmountEntCurrency { get; set; }
        public decimal DebitAmountLedgerCurrency { get; set; }
        public decimal CreditAmountLedgerCurrency { get; set; }

        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public string Dim1DimName { get; set; }

        // Extensions
        public decimal SameBalance { get; set; }
        public decimal OppositeBalance { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public string Dim2DimName { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public string Dim3DimName { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public string Dim4DimName { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public string Dim5DimName { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public string Dim6DimName { get; set; }
        public string DimNrSieDim1 { get; set; }
        public string DimNrSieDim6 { get; set; }

        //Entry Information
        public int TriggerType { get; set; }
        public DateTime Date { get; set; }
        public string SupplierInvoiceNr { get; set; }
        public string SupplierNr { get; set; }
        public string InventoryNr { get; set; }
        public string VoucherNr { get; set; }
    }

    #endregion

    #region AccountDistributionHead

    [TSInclude]
    public class AccountDistributionHeadDTO
    {
        public int AccountDistributionHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? VoucherSeriesTypeId { get; set; }
        public int Type { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_AccountDistributionTriggerType TriggerType { get; set; }
        public TermGroup_AccountDistributionCalculationType CalculationType { get; set; }
        public decimal Calculate { get; set; }
        public TermGroup_AccountDistributionPeriodType PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public int Sort { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DayNumber { get; set; }
        public decimal Amount { get; set; }
        public int AmountOperator { get; set; }
        public bool KeepRow { get; set; }
        public bool UseInVoucher { get; set; }
        public bool UseInSupplierInvoice { get; set; }
        public bool UseInCustomerInvoice { get; set; }
        public bool UseInImport { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public int Dim1Id { get; set; }
        public string Dim1Expression { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Expression { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Expression { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Expression { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Expression { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Expression { get; set; }

        public List<AccountDistributionRowDTO> Rows { get; set; }
        public bool UseInPayrollVacationVoucher { get; set; }
        public bool UseInPayrollVoucher { get; set; }
    }

    public class AccountDistributionGridDTO
    {
        public int AccountDistributionHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
    }

    [TSInclude]
    public class AccountDistributionHeadSmallDTO
    {
        public int AccountDistributionHeadId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_AccountDistributionCalculationType CalculationType { get; set; }
        public TermGroup_AccountDistributionTriggerType TriggerType { get; set; }
        public int PeriodValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DayNumber { get; set; }
        public decimal Amount { get; set; }
        public int AmountOperator { get; set; }
        public bool KeepRow { get; set; }
        public int Sort { get; set; }
        public string Dim1Expression { get; set; }
        public string Dim2Expression { get; set; }
        public string Dim3Expression { get; set; }
        public string Dim4Expression { get; set; }
        public string Dim5Expression { get; set; }
        public string Dim6Expression { get; set; }
        public int? EntryTotalCount { get; set; }
        public int? EntryTransferredCount { get; set; }
        public DateTime? EntryLatestTransferDate { get; set; }
        public decimal? EntryTotalAmount { get; set; }
        public decimal? EntryTransferredAmount { get; set; }
        public decimal? EntryPeriodAmount { get; set; }
        public bool UseInVoucher { get; set; }
        public bool UseInImport { get; set; }
    }

    #endregion

    #region AccountDistributionRow

    [TSInclude]
    public class AccountDistributionRowDTO
    {
        public int AccountDistributionRowId { get; set; }
        public int AccountDistributionHeadId { get; set; }
        public int RowNbr { get; set; }
        public int CalculateRowNbr { get; set; }
        public decimal SameBalance { get; set; }
        public decimal OppositeBalance { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }

        public int? Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1Disabled { get; set; }
        public bool Dim1Mandatory { get; set; }

        // Extensions
        public int PreviousRowNbr { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public bool Dim2Disabled { get; set; }
        public bool Dim2Mandatory { get; set; }
        public bool Dim2KeepSourceRowAccount { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public bool Dim3Disabled { get; set; }
        public bool Dim3Mandatory { get; set; }
        public bool Dim3KeepSourceRowAccount { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public bool Dim4Disabled { get; set; }
        public bool Dim4Mandatory { get; set; }
        public bool Dim4KeepSourceRowAccount { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public bool Dim5Disabled { get; set; }
        public bool Dim5Mandatory { get; set; }
        public bool Dim5KeepSourceRowAccount { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public bool Dim6Disabled { get; set; }
        public bool Dim6Mandatory { get; set; }
        public bool Dim6KeepSourceRowAccount { get; set; }
    }

    #endregion

    #region AccountDistributionHeadIO

    public class AccountDistributionHeadIODTO
    {
        public int ActorCompanyId { get; set; }
        public string VoucherSeriesType { get; set; }
        public int Type { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int TriggerType { get; set; }
        public int CalculationType { get; set; }
        public decimal Calculate { get; set; }
        public int PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public int Sort { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DayNumber { get; set; }
        public decimal Amount { get; set; }
        public int AmountOperator { get; set; }
        public bool KeepRow { get; set; }
        public bool UseInVoucher { get; set; }
        public bool UseInSupplierInvoice { get; set; }
        public bool UseInCustomerInvoice { get; set; }
        public bool UseInImport { get; set; }

        public int Dim1Nr { get; set; }
        public string Dim1Expression { get; set; }
        public int Dim2Nr { get; set; }
        public string Dim2Expression { get; set; }
        public int Dim3Nr { get; set; }
        public string Dim3Expression { get; set; }
        public int Dim4Nr { get; set; }
        public string Dim4Expression { get; set; }
        public int Dim5Nr { get; set; }
        public string Dim5Expression { get; set; }
        public int Dim6Nr { get; set; }
        public string Dim6Expression { get; set; }
        public int DimSieDim1 { get; set; }
        public int DimSieDim6 { get; set; }
        public string DimExpressionSieDim1 { get; set; }
        public string DimExpressionSieDim6 { get; set; }

        public List<AccountDistributionRowIODTO> Rows { get; set; }
    }

    public class AccountDistributionRowIODTO
    {
        public int AccountDistributionHeadId { get; set; }
        public int RowNbr { get; set; }
        public int CalculateRowNbr { get; set; }
        public decimal SameBalance { get; set; }
        public decimal OppositeBalance { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }

        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1Disabled { get; set; }
        public bool Dim1Mandatory { get; set; }

        // Extensions
        public int PreviousRowNbr { get; set; }

        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public bool Dim2Disabled { get; set; }
        public bool Dim2Mandatory { get; set; }
        public bool Dim2KeepSourceRowAccount { get; set; }

        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public bool Dim3Disabled { get; set; }
        public bool Dim3Mandatory { get; set; }
        public bool Dim3KeepSourceRowAccount { get; set; }

        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public bool Dim4Disabled { get; set; }
        public bool Dim4Mandatory { get; set; }
        public bool Dim4KeepSourceRowAccount { get; set; }

        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public bool Dim5Disabled { get; set; }
        public bool Dim5Mandatory { get; set; }
        public bool Dim5KeepSourceRowAccount { get; set; }

        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public bool Dim6Disabled { get; set; }
        public bool Dim6Mandatory { get; set; }
        public bool Dim6KeepSourceRowAccount { get; set; }

        public string DimSieDim1 { get; set; }
        public string DimNameSieDim1 { get; set; }
        public bool DimDisabledSieDim1 { get; set; }
        public bool DimMandatorySieDim1 { get; set; }
        public bool DimKeepSourceRowAccountSieDim1 { get; set; }
        public string DimSieDim6 { get; set; }
        public string DimNameSieDim6 { get; set; }
        public bool DimDisabledSieDim6 { get; set; }
        public bool DimMandatorySieDim6 { get; set; }
        public bool DimKeepSourceRowAccountSieDim6 { get; set; }
    }


    #endregion

    #endregion

    #region AccountHistory

    public class AccountHistoryDTO
    {
        public int AccountHistoryId { get; set; }
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public int? SysAccountStdTypeId { get; set; }
        public DateTime Date { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public string SieKpTyp { get; set; }

        // Extensions
        public string UserName { get; set; }
        public string SysAccountStdTypeName { get; set; }
    }

    #endregion

    #region AccountMapping

    public class AccrualAccountMappingDTO
    {
        public int AccrualAccountMappingId { get; set; }
        public int ActorCompanyId { get; set; }
        public int SourceAccountId { get; set; }
        public int SourceAccountTypeSysTermId { get; set; }
        public int TargetAccrualAccountId { get; set; }
        // Extensions
        public string AccrualTypeName { get; set; }
        public string SourceAccountNr { get; set; }
        public string TargetAccrualAccountNr { get; set; }
    }

    #endregion

    #region AccountMapping

    [TSInclude]
    public class AccountMappingDTO
    {
        public int AccountId { get; set; }
        public int AccountDimId { get; set; }
        public int? DefaultAccountId { get; set; }
        public TermGroup_AccountMandatoryLevel MandatoryLevel { get; set; }
        public string AccountDimName { get; set; }
        public List<AccountDTO> Accounts { get; set; }
        public List<GenericType> MandatoryLevels { get; set; }
    }

    #endregion

    #region AccountPeriod

    [TSInclude]
    public class AccountPeriodDTO
    {
        public int AccountPeriodId { get; set; }
        public int AccountYearId { get; set; }
        public int PeriodNr { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TermGroup_AccountStatus Status { get; set; }
        public string StartValue { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public bool IsDeleted { get; set; }
        public bool HasExistingVouchers { get; set; }
    }

    public class AccountPeriodIODTO
    {
        public int AccountPeriodId { get; set; }
        public int PeriodNr { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Status { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #region AccountYear
    [TSInclude]
    public class AccountYearDTO
    {
        public int AccountYearId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TermGroup_AccountStatus Status { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public List<AccountPeriodDTO> Periods { get; set; }

        // Extension
        public int NoOfPeriods { get; set; }
        public string StatusText { get; set; }
        public string YearFromTo { get; set; }
    }

    public class AccountYearIODTO
    {
        public int AccountYearId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Status { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }

        public List<AccountPeriodIODTO> AccountPeriods = new List<AccountPeriodIODTO>();
        public List<VoucherSeriesIODTO> VoucherSeries = new List<VoucherSeriesIODTO>();

    }

    public class AccountYearBalanceHeadDTO
    {
        public int AccountYearBalanceHeadId { get; set; }
        public int AccountYearId { get; set; }
        public int AccountId { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceEntCurrency { get; set; }
        public decimal? Quantity { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public List<AccountYearBalanceRowDTO> rows { get; set; }
    }

    [TSInclude]
    public class AccountYearBalanceFlatDTO
    {
        public int AccountYearBalanceHeadId { get; set; }
        public int AccountYearId { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceEntCurrency { get; set; }
        public decimal? Quantity { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public string Dim1TypeName { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }

        // Extensions
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public int RowNr { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool isDiffRow { get; set; }

        public void SetAccountValues(int position, int accountId, string accountNr, string accountName)
        {
            switch (position)
            {
                case 2:
                    this.Dim2Id = accountId;
                    this.Dim2Nr = accountNr;
                    this.Dim2Name = accountName;
                    break;
                case 3:
                    this.Dim3Id = accountId;
                    this.Dim3Nr = accountNr;
                    this.Dim3Name = accountName;
                    break;
                case 4:
                    this.Dim4Id = accountId;
                    this.Dim4Nr = accountNr;
                    this.Dim4Name = accountName;
                    break;
                case 5:
                    this.Dim5Id = accountId;
                    this.Dim5Nr = accountNr;
                    this.Dim5Name = accountName;
                    break;
                case 6:
                    this.Dim6Id = accountId;
                    this.Dim6Nr = accountNr;
                    this.Dim6Name = accountName;
                    break;
            }
        }
    }

    public class AccountYearBalanceRowDTO
    {
        public int AccountYearBalanceRowId { get; set; }
        public int AccountId { get; set; }
    }

    public class AccountYearBalanceIODTO
    {
        public int ActorCompanyId { get; set; }
        public DateTime AccountYearStartDate { get; set; }
        public int? AccountYearId { get; set; }
        public string AccountNr { get; set; }
        public decimal Balance { get; set; }
        public decimal Quantity { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountNrSieDim1 { get; set; }
        public string AccountNrSieDim6 { get; set; }
    }

    #endregion

    #region Actor

    public class ExternalActorDTO
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string OrgNumber { get; set; }

        public int ExternalId { get; set; }

        public string StreetName { get; set; }
        public string PostBox { get; set; }
        public string PostCode { get; set; }
        public string PostArea { get; set; }

        public string ProxyHomepage { get; set; }
        public string EniroInfoPage { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public string Phonenumber1 { get; set; }
        public string Phonenumber2 { get; set; }
        public string Phonenumber3 { get; set; }
    }

    #endregion

    #region Annual leave

    #region AnnualLeaveGroup

    [TSInclude]
    public class AnnualLeaveGroupDTO
    {
        public int AnnualLeaveGroupId { get; set; }

        public TermGroup_AnnualLeaveGroupType Type { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? QualifyingDays { get; set; }
        public int? QualifyingMonths { get; set; }
        public int GapDays { get; set; }
        public int RuleRestTimeMinimum { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? TimeDeviationCauseId { get; set; }
    }

    [TSInclude]
    public class AnnualLeaveGroupGridDTO
    {
        public int AnnualLeaveGroupId { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? QualifyingDays { get; set; }
        public int? QualifyingMonths { get; set; }
        public int GapDays { get; set; }
        public SoeEntityState State { get; set; }
        public string TimeDeviationCauseName { get; set; }
    }

    [TSInclude]
    public class AnnualLeaveGroupLimitDTO
    {
        public int WorkedMinutes { get; set; }
        public int NbrOfDaysAnnualLeave { get; set; }
        public int NbrOfMinutesAnnualLeave { get; set; }
    }

    #endregion

    #region AnnualLeaveTransactions

    [TSInclude]
    public class AnnualLeaveTransactionEditDTO
    {
        public int AnnualLeaveTransactionId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? DateEarned { get; set; }
        public int MinutesEarned { get; set; }
        public DateTime? DateSpent { get; set; }
        public int MinutesSpent { get; set; }
        public int AccumulatedMinutes { get; set; }
        public int LevelEarned { get; set; }
        public bool ManuallyAdded { get; set; }
        public bool ManuallyEarned { get; set; }
        public bool ManuallySpent { get; set; }
        public TermGroup_AnnualLeaveTransactionType Type { get; set; }
        public int DayBalance { get; set; }
        public int MinuteBalance { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string EmployeeNrAndName { get; set; }

        // Extensions
        public string TypeColor { get; set; }

    }

    [TSInclude]
    public class AnnualLeaveTransactionGridDTO
    {
        public string EmployeeNrAndName { get; set; }
        public int AnnualLeaveTransactionId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? DateEarned { get; set; }
        public int MinutesEarned { get; set; }
        public DateTime? DateSpent { get; set; }
        public int MinutesSpent { get; set; }
        public int AccumulatedMinutes { get; set; }
        public int LevelEarned { get; set; }
        public bool ManuallyAdded { get; set; }
        public TermGroup_AnnualLeaveTransactionType Type { get; set; }
        public string TypeName { get; set; }
        public string TypeColor { get; set; }
        public int DayBalance { get; set; }
        public int MinuteBalance { get; set; }
        public bool ManuallyEarned { get; set; }
        public bool ManuallySpent { get; set; }
    }

    #endregion

    #endregion

    #region AttestRole

    [TSInclude]
    public class AttestRoleDTO
    {
        public int AttestRoleId { get; set; }
        public int ActorCompanyId { get; set; }
        public SoeModule Module { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DefaultMaxAmount { get; set; }
        public bool ShowUncategorized { get; set; }
        public bool ShowAllCategories { get; set; }
        public bool AttestByEmployeeAccount { get; set; }
        public bool StaffingByEmployeeAccount { get; set; }
        public bool ShowAllSecondaryCategories { get; set; }
        public bool ShowTemplateSchedule { get; set; }
        public bool AlsoAttestAdditionsFromTime { get; set; }
        public bool HumanResourcesPrivacy { get; set; }
        public bool IsExecutive { get; set; }
        public bool AllowToAddOtherEmployeeAccounts { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
        public int? ReminderAttestStateId { get; set; }
        public int? ReminderNoOfDays { get; set; }
        public int? ReminderPeriodType { get; set; }
        public int Sort { get; set; }
        public bool Active { get; set; }

        //Extensions
        public List<int> TransitionIds { get; set; }
        public List<CompanyCategoryRecordDTO> PrimaryCategoryRecords { get; set; }
        public List<CompanyCategoryRecordDTO> SecondaryCategoryRecords { get; set; }
        public List<AttestRoleMappingDTO> AttestRoleMapping { get; set; }

        [TsIgnore]
        public int AccountId { get; set; }
    }

    public class AttestRoleGridDTO
    {
        public int AttestRoleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalCodesString { get; set; }
        public decimal DefaultMaxAmount { get; set; }
        public string ShowUncategorizedText { get; set; }
        public string ShowAllCategoriesText { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }
    }

    [TSInclude]
    public class UpdateAttestRoleModel
    {
        public Dictionary<int, bool> Dict { get; set; }
        public SoeModule Module { get; set; }
    }

    #endregion

    #region AttestRoleMapping

    [TSInclude]
    public class AttestRoleMappingDTO
    {
        public int AttestRoleMappingId { get; set; }
        public int ParentAttestRoleId { get; set; }
        public int ChildtAttestRoleId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? Created { get; set; }
        public TermGroup_AttestEntity Entity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public string ParentAttestRoleName { get; set; }
        public string ChildtAttestRoleName { get; set; }
    }


    #endregion

    #region AttestRule

    public class AttestRuleHeadDTO
    {
        public int AttestRuleHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? DayTypeId { get; set; }
        public int? ScheduledJobHeadId { get; set; }
        public SoeModule Module { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<AttestRuleRowDTO> AttestRuleRows { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public bool IsSelected { get; set; }
        public string DayTypeName { get; set; }
        public int DayTypeCompanyId { get; set; }
        public string DayTypeCompanyName { get; set; }
    }

    public class AttestRuleHeadGridDTO
    {
        public int AttestRuleHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }
        public string DayTypeName { get; set; }
        public string EmployeeGroupNames { get; set; }
    }

    public class AttestRuleRowDTO
    {
        public int AttestRuleRowId { get; set; }
        public int AttestRuleHeadId { get; set; }

        public TermGroup_AttestRuleRowLeftValueType? LeftValueType { get; set; }
        public int? LeftValueId { get; set; }
        public TermGroup_AttestRuleRowRightValueType? RightValueType { get; set; }
        public int? RightValueId { get; set; }
        public int Minutes { get; set; }
        public WildCard ComparisonOperator { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string LeftValueTypeName { get; set; }
        public string LeftValueIdName { get; set; }
        public bool ShowLeftValueId { get; set; }
        public string RightValueTypeName { get; set; }
        public string RightValueIdName { get; set; }
        public bool ShowRightValueId { get; set; }

        public string ComparisonOperatorString { get; set; }
    }

    #endregion

    #region AttestState

    [TSInclude]
    public class AttestStateDTO
    {
        public int AttestStateId { get; set; }
        public int ActorCompanyId { get; set; }

        public TermGroup_AttestEntity Entity { get; set; }
        public SoeModule Module { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string ImageSource { get; set; }

        public int Sort { get; set; }
        public bool Initial { get; set; }
        public bool Closed { get; set; }
        public bool Hidden { get; set; }
        public bool Locked { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public int? LangId { get; set; }
        public string EntityName { get; set; }
    }

    public class AttestStateSmallDTO
    {
        public int AttestStateId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string ImageSource { get; set; }

        public int Sort { get; set; }
        public bool Initial { get; set; }
        public bool Closed { get; set; }
    }

    #endregion

    #region AttestTransition

    [TSInclude]
    public class AttestTransitionDTO
    {
        public int AttestTransitionId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AttestStateFromId { get; set; }
        public int AttestStateToId { get; set; }
        public SoeModule Module { get; set; }
        public string Name { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public bool NotifyChangeOfAttestState { get; set; }

        // Extensions
        public AttestStateDTO AttestStateFrom { get; set; }
        public AttestStateDTO AttestStateTo { get; set; }
    }

    public class AttestTransitionGridDTO
    {
        public int AttestTransitionId { get; set; }
        public string Name { get; set; }

        // Extensions
        public string EntityName { get; set; }
        public string AttestStateFrom { get; set; }
        public string AttestStateTo { get; set; }
    }

    #endregion

    #region AttestWorkFlow

    #region AttestWorkFlowHead

    [TSInclude]
    public class AttestWorkFlowHeadGridDTO
    {
        public int AttestWorkFlowHeadId { get; set; }
        public int? AttestWorkFlowTemplateHeadId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Attestants { get; set; }
    }

    [TSInclude]
    public class AttestGroupGridDTO
    {
        public int AttestWorkFlowHeadId { get; set; }
        public int? AttestWorkFlowTemplateHeadId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    [TSInclude]
    [KnownType(typeof(AttestGroupDTO))]
    public class AttestWorkFlowHeadDTO
    {
        public int AttestWorkFlowHeadId { get; set; }
        public int? AttestWorkFlowTemplateHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_AttestWorkFlowType Type { get; set; }
        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }
        public string Name { get; set; }
        public bool SendMessage { get; set; }
        public string AdminInformation { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public bool IsDeleted { get; set; }
        public string TypeName { get; set; }
        public string TemplateName { get; set; }
        public List<AttestWorkFlowRowDTO> Rows { get; set; }
        public int? AttestWorkFlowGroupId { get; set; } // Used when saving to update field in invoice
        public bool SignInitial { get; set; }   // Used when initiating a sign process to sign initial state directly

        public virtual bool IsAttestGroup
        {
            get
            {
                return false;
            }
        }
    }

    [TSInclude]

    public class AttestGroupDTO : AttestWorkFlowHeadDTO
    {
        public string AttestGroupCode { get; set; }
        public string AttestGroupName { get; set; }
        public override bool IsAttestGroup
        {
            get
            {
                return true;
            }
        }
    }

    #endregion

    #region AttestWorkFlowRow

    [TSInclude]
    public class AttestWorkFlowRowDTO
    {
        public int AttestWorkFlowRowId { get; set; }
        public int AttestWorkFlowHeadId { get; set; }
        public int AttestTransitionId { get; set; }
        public int? AttestRoleId { get; set; }
        public int? UserId { get; set; }
        public int? OriginateFromRowId { get; set; }
        public TermGroup_AttestWorkFlowRowProcessType ProcessType { get; set; }
        public bool? Answer { get; set; }
        public DateTime? AnswerDate { get; set; }
        public string AnswerText { get; set; }
        public string Comment { get; set; }
        public string CommentUser { get; set; }
        public DateTime? CommentDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public TermGroup_AttestFlowRowState State { get; set; }
        public TermGroup_AttestWorkFlowType? Type { get; set; }

        // Extensions
        public int AttestStateFromId { get; set; }
        public string AttestStateFromName { get; set; }
        public string AttestStateToName { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestTransitionName { get; set; }
        public string AttestRoleName { get; set; }
        public string ProcessTypeName { get; set; }
        public int ProcessTypeSort { get; set; }
        public string TypeName { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool IsDeleted { get; set; }
        public int WorkFlowRowIdToReplace { get; set; }
    }

    #endregion

    #region AttestWorkFlowTemplateHead

    [TSInclude]

    public class AttestWorkFlowTemplateHeadGridDTO
    {
        public int AttestWorkFlowTemplateHeadId { get; set; }
        public TermGroup_AttestWorkFlowType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [TSInclude]
    public class AttestWorkFlowTemplateHeadDTO
    {
        public int AttestWorkFlowTemplateHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_AttestWorkFlowType Type { get; set; }
        public TermGroup_AttestEntity AttestEntity { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region AttestWorkFlowTemplateRow

    [TSInclude]
    public class AttestWorkFlowTemplateRowDTO
    {
        public int AttestWorkFlowTemplateRowId { get; set; }
        public int AttestWorkFlowTemplateHeadId { get; set; }
        public int AttestTransitionId { get; set; }
        public int Sort { get; set; }
        public int? Type { get; set; }

        // Extensions
        public string AttestTransitionName { get; set; }
        public string AttestStateFromName { get; set; }
        public string AttestStateToName { get; set; }
        public string AttestStateToColor { get; set; }
        public string TypeName { get; set; }
        public bool Initial { get; set; }
        public bool Closed { get; set; }
    }

    #endregion

    #region Gauges

    public class AttestFlowGaugeDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Amount { get; set; }
        public string SupplierName { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
    }

    #endregion

    #region Overview

    public class AttestWorkFlowOverviewGridDTO
    {
        public int OwnerActorId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string ProjectNr { get; set; }
        public string ReferenceOur { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public bool FullyPaid { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? PayDate { get; set; }
        public bool AttestFlowOverdued { get; set; }
        public int? CostCentreId { get; set; }
        public string CostCentreName { get; set; }
        public int? AttestStateId { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
        public string AttestComments { get; set; }
        public bool ShowAttestCommentIcon { get; set; }
        public bool Selected { get; set; }
        public int? OrderNr { get; set; }
        public string InternalDescription { get; set; }
        public string Currency { get; set; }
        public bool hasPicture { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public bool BlockPayment { get; set; }
        public string BlockReason { get; set; }
        // Internal accounts
        public int? DefaultDim2Id { get; set; }
        public string DefaultDim2Name { get; set; }
        public int? DefaultDim3Id { get; set; }
        public string DefaultDim3Name { get; set; }
        public int? DefaultDim4Id { get; set; }
        public string DefaultDim4Name { get; set; }
        public int? DefaultDim5Id { get; set; }
        public string DefaultDim5Name { get; set; }
        public int? DefaultDim6Id { get; set; }
        public string DefaultDim6Name { get; set; }
    }

    #endregion

    #endregion

    #region BalanceRuleSetting

    public class BalanceRuleSettingDTO
    {
        public int BalanceRuleSettingId { get; set; }
        public int TimeRuleGroupId { get; set; }
        public int TimeCodeId { get; set; }
        public int ReplacementTimeCodeId { get; set; }

        // Extensions
        public Guid BalanceRuleSettingTempId { get; set; }  // Used when saving time rules connected to BalanceRuleSetting
    }

    #endregion

    #region Budget

    [TSInclude]
    public class BudgetHeadGridDTO
    {
        public int BudgetHeadId { get; set; }
        public string Name { get; set; }
        public int AccountYearId { get; set; }
        public string AccountingYear { get; set; }
        public string Created { get; set; }
        public string NoOfPeriods { get; set; }
        public int BudgetTypeId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    [TSInclude]
    public class BudgetHeadDTO
    {
        public int BudgetHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Type { get; set; }
        public int? AccountYearId { get; set; }
        public int? DistributionCodeHeadId { get; set; }
        public int NoOfPeriods { get; set; }
        public int Status { get; set; }
        public int? ProjectId { get; set; }
        public string AccountYearText { get; set; }
        public string StatusName { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public bool? UseDim2 { get; set; }
        public bool? UseDim3 { get; set; }
        public int Dim2Id { get; set; }
        public int Dim3Id { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public List<BudgetRowDTO> Rows { get; set; }
    }

    [TSInclude]
    public class BudgetHeadFlattenedDTO
    {
        public int BudgetHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Type { get; set; }
        public int DistributionCodeSubType { get; set; }
        public int? AccountYearId { get; set; }
        public int? DistributionCodeHeadId { get; set; }
        public int NoOfPeriods { get; set; }
        public int Status { get; set; }
        public int? ProjectId { get; set; }
        public string AccountYearText { get; set; }
        public string StatusName { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }

        public bool? UseDim2 { get; set; }
        public bool? UseDim3 { get; set; }
        public int Dim2Id { get; set; }
        public int Dim3Id { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public List<BudgetRowFlattenedDTO> Rows { get; set; }
    }

    [TSInclude]
    public class BudgetHeadProjectDTO
    {
        public int BudgetHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Type { get; set; }
        public int NoOfPeriods { get; set; }
        public int Status { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public DateTime? ProjectFromDate { get; set; }
        public DateTime? ProjectToDate { get; set; }
        public string StatusName { get; set; }
        public string Name { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PeriodType { get; set; }
        public string CreatedDate { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string ResultModified { get; set; }

        public int? ParentBudgetHeadId { get; set; }
        public string ParentBudgetHeadName { get; set; }

        public List<BudgetRowProjectDTO> Rows { get; set; }
    }

    public class BudgetHeadSalesDTO
    {
        public int BudgetHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Type { get; set; }
        public int? DistributionCodeSubType { get; set; }
        public int? DistributionCodeHeadId { get; set; }
        public int NoOfPeriods { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Name { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public List<BudgetRowSalesDTO> Rows { get; set; }
    }

    [TSInclude]
    public class BudgetRowDTO
    {
        public int BudgetRowId { get; set; }
        public int BudgetHeadId { get; set; }
        public int AccountId { get; set; }
        public int TimeCodeId { get; set; }
        public string Name { get; set; }
        public int? DistributionCodeHeadId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int BudgetRowNr { get; set; }
        public int? Type { get; set; }
        public int? ModifiedUserId { get; set; }
        public bool IsAdded { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public string Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string DistributionCodeHeadName { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal TotalQuantity { get; set; }

        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }

        //Extensions
        public BudgetHeadDTO BudgetHead { get; set; }
        public List<BudgetPeriodDTO> Periods { get; set; }
    }

    [TSInclude]
    public class BudgetRowProjectDTO
    {
        public int BudgetRowId { get; set; }
        public int BudgetHeadId { get; set; }
        public int? ParentBudgetRowId { get; set; }
        public int TimeCodeId { get; set; }
        public string TypeCodeName { get; set; }
        public string Name { get; set; }
        public int BudgetRowNr { get; set; }
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public bool IsAdded { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public bool HasLogPosts { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal TotalQuantity { get; set; }
        public Decimal TotalAmountResult { get; set; }
        public Decimal TotalQuantityResult { get; set; }
        public Decimal TotalDiffResult { get; set; }
        public Decimal TotalAmountCompBudget { get; set; }
        public Decimal TotalQuantityCompBudget { get; set; }
        public Decimal TotalDiffCompBudget { get; set; }
        public string Comment { get; set; }
        public List<BudgetRowProjectChangeLogDTO> ChangeLogItems { get; set; }
        public List<BudgetPeriodProjectDTO> Periods { get; set; }
    }

    [TSInclude]
    public class BudgetRowProjectChangeLogDTO
    {
        public int BudgetRowChangeLogId { get; set; }
        public int BudgetRowId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public Decimal FromTotalAmount { get; set; }
        public Decimal FromTotalQuantity { get; set; }
        public Decimal ToTotalAmount { get; set; }
        public Decimal ToTotalQuantity { get; set; }
        public Decimal TotalAmountDiff { get; set; }
        public Decimal TotalQuantityDiff { get; set; }
        public string Comment { get; set; }
    }

    public class BudgetRowSalesDTO
    {
        public int BudgetRowId { get; set; }
        public int BudgetHeadId { get; set; }
        public int AccountId { get; set; }
        public int BudgetRowNr { get; set; }
        public int? Type { get; set; }
        public int? ModifiedUserId { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public string Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string DistributionCodeHeadName { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal TotalQuantity { get; set; }

        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }

        public List<BudgetPeriodSalesDTO> Periods { get; set; }
    }

    [TSInclude]
    public class BudgetRowFlattenedDTO
    {
        public int BudgetRowId { get; set; }
        public int BudgetHeadId { get; set; }
        public int AccountId { get; set; }
        public int? DistributionCodeHeadId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int BudgetRowNr { get; set; }
        public int? Type { get; set; }
        public int? ModifiedUserId { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public string Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string DistributionCodeHeadName { get; set; }
        public Decimal TotalAmount { get; set; }
        public Decimal TotalQuantity { get; set; }

        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }

        #region Period (Maximum 18 in angular)

        public int BudgetRowPeriodId1 { get; set; }
        public int PeriodNr1 { get; set; }
        public DateTime? StartDate1 { get; set; }
        public Decimal Amount1 { get; set; }
        public Decimal Quantity1 { get; set; }

        public int BudgetRowPeriodId2 { get; set; }
        public int PeriodNr2 { get; set; }
        public DateTime? StartDate2 { get; set; }
        public Decimal Amount2 { get; set; }
        public Decimal Quantity2 { get; set; }

        public int BudgetRowPeriodId3 { get; set; }
        public int PeriodNr3 { get; set; }
        public DateTime? StartDate3 { get; set; }
        public Decimal Amount3 { get; set; }
        public Decimal Quantity3 { get; set; }

        public int BudgetRowPeriodId4 { get; set; }
        public int PeriodNr4 { get; set; }
        public DateTime? StartDate4 { get; set; }
        public Decimal Amount4 { get; set; }
        public Decimal Quantity4 { get; set; }

        public int BudgetRowPeriodId5 { get; set; }
        public int PeriodNr5 { get; set; }
        public DateTime? StartDate5 { get; set; }
        public Decimal Amount5 { get; set; }
        public Decimal Quantity5 { get; set; }

        public int BudgetRowPeriodId6 { get; set; }
        public int PeriodNr6 { get; set; }
        public DateTime? StartDate6 { get; set; }
        public Decimal Amount6 { get; set; }
        public Decimal Quantity6 { get; set; }

        public int BudgetRowPeriodId7 { get; set; }
        public int PeriodNr7 { get; set; }
        public DateTime? StartDate7 { get; set; }
        public Decimal Amount7 { get; set; }
        public Decimal Quantity7 { get; set; }

        public int BudgetRowPeriodId8 { get; set; }
        public int PeriodNr8 { get; set; }
        public DateTime? StartDate8 { get; set; }
        public Decimal Amount8 { get; set; }
        public Decimal Quantity8 { get; set; }

        public int BudgetRowPeriodId9 { get; set; }
        public int PeriodNr9 { get; set; }
        public DateTime? StartDate9 { get; set; }
        public Decimal Amount9 { get; set; }
        public Decimal Quantity9 { get; set; }

        public int BudgetRowPeriodId10 { get; set; }
        public int PeriodNr10 { get; set; }
        public DateTime? StartDate10 { get; set; }
        public Decimal Amount10 { get; set; }
        public Decimal Quantity10 { get; set; }

        public int BudgetRowPeriodId11 { get; set; }
        public int PeriodNr11 { get; set; }
        public DateTime? StartDate11 { get; set; }
        public Decimal Amount11 { get; set; }
        public Decimal Quantity11 { get; set; }

        public int BudgetRowPeriodId12 { get; set; }
        public int PeriodNr12 { get; set; }
        public DateTime? StartDate12 { get; set; }
        public Decimal Amount12 { get; set; }
        public Decimal Quantity12 { get; set; }

        public int BudgetRowPeriodId13 { get; set; }
        public int PeriodNr13 { get; set; }
        public DateTime? StartDate13 { get; set; }
        public Decimal Amount13 { get; set; }
        public Decimal Quantity13 { get; set; }

        public int BudgetRowPeriodId14 { get; set; }
        public int PeriodNr14 { get; set; }
        public DateTime? StartDate14 { get; set; }
        public Decimal Amount14 { get; set; }
        public Decimal Quantity14 { get; set; }

        public int BudgetRowPeriodId15 { get; set; }
        public int PeriodNr15 { get; set; }
        public DateTime? StartDate15 { get; set; }
        public Decimal Amount15 { get; set; }
        public Decimal Quantity15 { get; set; }

        public int BudgetRowPeriodId16 { get; set; }
        public int PeriodNr16 { get; set; }
        public DateTime? StartDate16 { get; set; }
        public Decimal Amount16 { get; set; }
        public Decimal Quantity16 { get; set; }

        public int BudgetRowPeriodId17 { get; set; }
        public int PeriodNr17 { get; set; }
        public DateTime? StartDate17 { get; set; }
        public Decimal Amount17 { get; set; }
        public Decimal Quantity17 { get; set; }

        public int BudgetRowPeriodId18 { get; set; }
        public int PeriodNr18 { get; set; }
        public DateTime? StartDate18 { get; set; }
        public Decimal Amount18 { get; set; }
        public Decimal Quantity18 { get; set; }

        //Max 31 in sales budget
        public int BudgetRowPeriodId19 { get; set; }
        public int PeriodNr19 { get; set; }
        public DateTime? StartDate19 { get; set; }
        public Decimal Amount19 { get; set; }
        public Decimal Quantity19 { get; set; }

        public int BudgetRowPeriodId20 { get; set; }
        public int PeriodNr20 { get; set; }
        public DateTime? StartDate20 { get; set; }
        public Decimal Amount20 { get; set; }
        public Decimal Quantity20 { get; set; }

        public int BudgetRowPeriodId21 { get; set; }
        public int PeriodNr21 { get; set; }
        public DateTime? StartDate21 { get; set; }
        public Decimal Amount21 { get; set; }
        public Decimal Quantity21 { get; set; }

        public int BudgetRowPeriodId22 { get; set; }
        public int PeriodNr22 { get; set; }
        public DateTime? StartDate22 { get; set; }
        public Decimal Amount22 { get; set; }
        public Decimal Quantity22 { get; set; }

        public int BudgetRowPeriodId23 { get; set; }
        public int PeriodNr23 { get; set; }
        public DateTime? StartDate23 { get; set; }
        public Decimal Amount23 { get; set; }
        public Decimal Quantity23 { get; set; }

        public int BudgetRowPeriodId24 { get; set; }
        public int PeriodNr24 { get; set; }
        public DateTime? StartDate24 { get; set; }
        public Decimal Amount24 { get; set; }
        public Decimal Quantity24 { get; set; }

        public int BudgetRowPeriodId25 { get; set; }
        public int PeriodNr25 { get; set; }
        public DateTime? StartDate25 { get; set; }
        public Decimal Amount25 { get; set; }
        public Decimal Quantity25 { get; set; }

        public int BudgetRowPeriodId26 { get; set; }
        public int PeriodNr26 { get; set; }
        public DateTime? StartDate26 { get; set; }
        public Decimal Amount26 { get; set; }
        public Decimal Quantity26 { get; set; }

        public int BudgetRowPeriodId27 { get; set; }
        public int PeriodNr27 { get; set; }
        public DateTime? StartDate27 { get; set; }
        public Decimal Amount27 { get; set; }
        public Decimal Quantity27 { get; set; }

        public int BudgetRowPeriodId28 { get; set; }
        public int PeriodNr28 { get; set; }
        public DateTime? StartDate28 { get; set; }
        public Decimal Amount28 { get; set; }
        public Decimal Quantity28 { get; set; }

        public int BudgetRowPeriodId29 { get; set; }
        public int PeriodNr29 { get; set; }
        public DateTime? StartDate29 { get; set; }
        public Decimal Amount29 { get; set; }
        public Decimal Quantity29 { get; set; }

        public int BudgetRowPeriodId30 { get; set; }
        public int PeriodNr30 { get; set; }
        public DateTime? StartDate30 { get; set; }
        public Decimal Amount30 { get; set; }
        public Decimal Quantity30 { get; set; }

        public int BudgetRowPeriodId31 { get; set; }
        public int PeriodNr31 { get; set; }
        public DateTime? StartDate31 { get; set; }
        public Decimal Amount31 { get; set; }
        public Decimal Quantity31 { get; set; }

        #endregion
    }

    [TSInclude]
    public class BudgetPeriodDTO
    {
        public int BudgetRowPeriodId { get; set; }
        public int BudgetRowId { get; set; }
        public int? DistributionCodeHeadId { get; set; }
        public int PeriodNr { get; set; }
        public int? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsModified { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        //Extensions
        public BudgetRowDTO BudgetRow { get; set; }
    }

    [TSInclude]
    public class BudgetPeriodProjectDTO
    {
        public int BudgetRowPeriodId { get; set; }
        public int BudgetRowId { get; set; }
        public int PeriodNr { get; set; }
        public int? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsModified { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
    }

    public class BudgetPeriodSalesDTO
    {
        public int BudgetRowPeriodId { get; set; }
        public int BudgetRowId { get; set; }
        public int BudgetRowNr { get; set; }
        public int DistributionCodeHeadId { get; set; }
        public int PeriodNr { get; set; }
        public int? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsModified { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public decimal Percent { get; set; }
        public int StartHour { get; set; }
        public int ClosingHour { get; set; }

        // Guid
        public Guid Guid { get; set; }
        public Guid ParentGuid { get; set; }

        //Extensions
        public List<BudgetPeriodSalesDTO> Periods { get; set; }
    }

    public class BudgetBalanceDTO
    {
        public int AccountId { get; set; }
        public int Dim2Id { get; set; }
        public int Dim3Id { get; set; }
        public int Dim4Id { get; set; }
        public int Dim5Id { get; set; }
        public int Dim6Id { get; set; }

        public decimal RowSum { get; set; }

        public List<BalanceItemDTO> BalancePeriods { get; set; }
    }

    public class BudgetHeadIODTO
    {
        public DateTime AccountYearStartDate { get; set; }
        public string DistributionCodeHeadName { get; set; }
        public int NoOfPeriods { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }

        public int DimNr2 { get; set; }
        public int DimNr3 { get; set; }

        public bool UseDimNr2 { get; set; }
        public bool UseDimNr3 { get; set; }

        public List<BudgetRowIODTO> Rows { get; set; }
    }

    public class BudgetRowIODTO
    {
        public int BudgetRowNr { get; set; }
        public string AccountNr { get; set; }
        public string AccountDIm2Nr { get; set; }
        public string AccountDIm3Nr { get; set; }
        public string AccountDIm4Nr { get; set; }
        public string AccountDIm5Nr { get; set; }
        public string AccountDIm6Nr { get; set; }
        public string AccountSieDim1 { get; set; }
        public string AccountSieDim6 { get; set; }
        public string DistributionCodeHeadName { get; set; }
        public Decimal TotalAmount { get; set; }
        public string ShiftTypeCode { get; set; }
        public Decimal TotalQuantity { get; set; }
        public Decimal Period1Amount { get; set; }
        public Decimal Period2Amount { get; set; }
        public Decimal Period3Amount { get; set; }
        public Decimal Period4Amount { get; set; }
        public Decimal Period5Amount { get; set; }
        public Decimal Period6Amount { get; set; }
        public Decimal Period7Amount { get; set; }
        public Decimal Period8Amount { get; set; }
        public Decimal Period9Amount { get; set; }
        public Decimal Period10Amount { get; set; }
        public Decimal Period11Amount { get; set; }
        public Decimal Period12Amount { get; set; }
        public Decimal Period13Amount { get; set; }
        public Decimal Period14Amount { get; set; }
        public Decimal Period15Amount { get; set; }
        public Decimal Period16Amount { get; set; }
        public Decimal Period17Amount { get; set; }
        public Decimal Period18Amount { get; set; }
        public Decimal Period19Amount { get; set; }
        public Decimal Period20Amount { get; set; }
        public Decimal Period21Amount { get; set; }
        public Decimal Period22Amount { get; set; }
        public Decimal Period23Amount { get; set; }
        public Decimal Period24Amount { get; set; }
        public Decimal Period1Quantity { get; set; }
        public Decimal Period2Quantity { get; set; }
        public Decimal Period3Quantity { get; set; }
        public Decimal Period4Quantity { get; set; }
        public Decimal Period5Quantity { get; set; }
        public Decimal Period6Quantity { get; set; }
        public Decimal Period7Quantity { get; set; }
        public Decimal Period8Quantity { get; set; }
        public Decimal Period9Quantity { get; set; }
        public Decimal Period10Quantity { get; set; }
        public Decimal Period11Quantity { get; set; }
        public Decimal Period12Quantity { get; set; }
        public Decimal Period13Quantity { get; set; }
        public Decimal Period14Quantity { get; set; }
        public Decimal Period15Quantity { get; set; }
        public Decimal Period16Quantity { get; set; }
        public Decimal Period17Quantity { get; set; }
        public Decimal Period18Quantity { get; set; }
        public Decimal Period19Quantity { get; set; }
        public Decimal Period20Quantity { get; set; }
        public Decimal Period21Quantity { get; set; }
        public Decimal Period22Quantity { get; set; }
        public Decimal Period23Quantity { get; set; }
        public Decimal Period24Quantity { get; set; }
        public string AccountString { get; set; }

        //If one row is one period
        public int PeriodNr { get; set; }
        public decimal PeriodAmount { get; set; }
        public decimal PeriodQuantity { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }


        //HeadInformation
        public DateTime AccountYearStartDate { get; set; }
        public int NoOfPeriods { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }

        public int DimNr2 { get; set; }
        public int DimNr3 { get; set; }

        // Extras
        public int AccountYearId { get; set; }
    }

    #endregion

    #region CardNumber
    [TSInclude]
    public class CardNumberGridDTO
    {
        public int EmployeeId { get; set; }
        public string CardNumber { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeeName { get; set; }

        // Extensions
        public string EmployeeNrSort
        {
            get
            {
                return EmployeeNumber.PadLeft(50, '0');
            }
        }
    }

    #endregion

    #region Category

    [TSInclude]
    public class CategoryGridDTO
    {
        public int CategoryId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        // Extensions
        public string ChildrenNamesString { get; set; }
        public string CategoryGroupName { get; set; }
    }

    [TSInclude]
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ParentId { get; set; }

        public SoeCategoryType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }
        public int? CategoryGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public List<CompanyCategoryRecordDTO> CompanyCategoryRecords { get; set; }
        public List<CategoryDTO> Children { get; set; }
        public string ChildrenNamesString { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public string CategoryGroupName { get; set; }
    }

    [TSInclude]
    public class CompanyCategoryRecordDTO
    {
        #region Properties

        //General
        public string UniqueId { get; set; }

        public int CompanyCategoryId { get; set; }
        public int ActorCompanyId { get; set; }
        public int CategoryId { get; set; }
        public SoeCategoryRecordEntity Entity { get; set; }
        public int RecordId { get; set; }
        public bool Default { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool IsExecutive { get; set; }

        // Extensions
        public CategoryDTO Category { get; set; }

        #endregion

        #region Ctor

        public CompanyCategoryRecordDTO()
        {
            this.UniqueId = Guid.NewGuid().ToString();
        }

        #endregion
    }

    public class CategoryGroupDTO
    {
        public int CategoryGroupId { get; set; }
        public int ActorCompanyId { get; set; }

        public SoeCategoryType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public string TypeName { get; set; }
    }

    #endregion

    #region CategoryAccount

    [TSInclude]
    public class CategoryAccountDTO
    {
        public int CategoryAccountId { get; set; }
        public int CategoryId { get; set; }
        public int AccountId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public SoeEntityState State { get; set; }

    }

    #endregion

    #region Checklist

    public class ChecklistHeadDTO
    {
        public int ChecklistHeadId { get; set; }
        public int ChecklistHeadRecordId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ReportId { get; set; }

        public TermGroup_ChecklistHeadType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public bool AddAttachementsToEInvoice { get; set; }
        public bool DefaultInOrder { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public List<ChecklistRowDTO> ChecklistRows { get; set; }
    }

    public class ChecklistHeadGridDTO
    {
        public int ChecklistHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TypeName { get; set; }
        public bool DefaultInOrder { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChecklistHeadRecordDTO
    {
        public Guid TempHeadId { get; set; }
        public int ChecklistHeadRecordId { get; set; }
        public int ChecklistHeadId { get; set; }
        public int ActorCompanyId { get; set; }

        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public ChecklistHeadDTO ChecklistHead { get; set; }
    }
    [TSInclude]
    public class ChecklistHeadRecordCompactDTO
    {
        public Guid TempHeadId { get; set; }
        public int ChecklistHeadRecordId { get; set; }
        public int ChecklistHeadId { get; set; }
        public int RecordId { get; set; }
        public string ChecklistHeadName { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public DateTime? Created { get; set; }
        public int RowNr { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public List<ChecklistExtendedRowDTO> ChecklistRowRecords { get; set; }
        public List<ImagesDTO> Signatures { get; set; }
    }

    public class ChecklistRowDTO
    {
        public int ChecklistRowId { get; set; }
        public int ChecklistHeadId { get; set; }

        public TermGroup_ChecklistRowType Type { get; set; }
        public int RowNr { get; set; }
        public bool Mandatory { get; set; }
        public string Text { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public Guid Guid { get; set; }
        public string TypeName { get; set; }
        public string MandatoryName { get; set; }
        public bool IsModified { get; set; }
        public ChecklistHeadDTO ChecklistHead { get; set; }
        public int? CheckListMultipleChoiceAnswerHeadId { get; set; }
    }
    [TSInclude]
    public class ChecklistExtendedRowDTO
    {
        #region Common

        public Guid Guid { get; set; }
        public bool IsHeadline { get; set; }

        #endregion

        #region ChecklistHead

        public int HeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CheckListMultipleChoiceAnswerHeadId { get; set; }

        #endregion

        #region ChecklistRow (Question)

        public int RowId { get; set; }
        public int RowNr { get; set; }
        public string Text { get; set; }
        public TermGroup_ChecklistRowType Type { get; set; }
        public bool Mandatory { get; set; }

        #endregion

        #region ChecklistHeadRecord

        public int HeadRecordId { get; set; }

        #endregion

        #region ChecklistRowRecord (Answer)

        public int RowRecordId { get; set; }
        public string Comment { get; set; }
        public DateTime? Date { get; set; }
        public int DataTypeId { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public decimal? DecimalData { get; set; }
        public List<FileUploadDTO> FileUploads { get; set; }
        public string Value
        {
            get
            {
                string value = "";

                switch (this.DataTypeId)
                {
                    case (int)SettingDataType.Boolean:
                        if (this.BoolData.HasValue)
                            value = this.BoolData.Value.ToString();
                        break;
                    case (int)SettingDataType.Date:
                    case (int)SettingDataType.Time:
                        if (this.DateData.HasValue)
                            value = this.DateData.Value.ToString("yyyyMMdd");
                        break;
                    case (int)SettingDataType.Decimal:
                        if (this.DecimalData.HasValue)
                            value = this.DecimalData.Value.ToString();
                        break;
                    case (int)SettingDataType.Integer:
                        if (this.IntData.HasValue)
                            value = this.IntData.Value.ToString();
                        break;
                    case (int)SettingDataType.String:
                        if (!String.IsNullOrEmpty(this.StrData))
                            value = this.StrData.ToString();
                        break;
                    case (int)SettingDataType.Image:
                        if (this.BoolData.GetValueOrDefault())
                        {
                            value = "Bild";
                        }
                        break;
                    default:
                        break;
                }

                return value;
            }
        }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        #endregion
    }

    public class CheckListMultipleChoiceAnswerHeadDTO
    {
        public int CheckListMultipleChoiceAnswerHeadId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Title { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public List<ChecklistRowDTO> ChecklistRows { get; set; }

    }

    public class CheckListMultipleChoiceAnswerRowDTO
    {
        public int CheckListMultipleChoiceAnswerRowId { get; set; }
        public int CheckListMultipleChoiceAnswerHeadId { get; set; }

        public string Question { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public List<ChecklistRowDTO> ChecklistRows { get; set; }

    }

    public class ChecklistRecordDTO
    {
    }

    public class ChecklistHeadIODTO
    {
        public int ReportNr { get; set; }

        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<ChecklistRowIODTO> ChecklistRows { get; set; }
    }

    public class ChecklistRowIODTO
    {
        public int Type { get; set; }
        public int RowNr { get; set; }
        public bool Mandatory { get; set; }
        public string Text { get; set; }
    }

    #endregion

    #region Company
    public class CompanySearchResultDTO
    {
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string CompanyGuid { get; set; }
    }

    public class CompanySearchFilterDTO
    {
        public string OrgNr { get; set; }
        public string BankAccountBIC { get; set; }
        public string BankAccountNr { get; set; }
        public TermGroup_SysPaymentType BankAccountType { get; set; }
        public string NameOrLicense { get; set; }
        public bool? Demo { get; set; }
        public bool? BankConnected { get; set; }
    }

    [TSInclude]
    public class CompanyDTO
    {
        public int ActorCompanyId { get; set; }
        public Guid _companyGuid { get; set; }
        public TermGroup_Languages Language { get; set; }
        public int? Number { get; set; }
        public string Name { get; set; }

        public string ShortName { get; set; }
        public string OrgNr { get; set; }
        public string VatNr { get; set; }
        public bool AllowSupportLogin { get; set; }
        public DateTime? AllowSupportLoginTo { get; set; }

        public bool Template { get; set; }
        public bool Global { get; set; }
        public bool Demo { get; set; }

        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public Guid _licenseGuid { get; set; }
        public bool LicenseSupport { get; set; }

        public int? SysCountryId { get; set; }
        public int? TimeSpotId { get; set; }

        public CompanyDTO() { }

        public CompanyDTO(Guid companyGuid, Guid licenseGuid)
        {
            _companyGuid = companyGuid;
            _licenseGuid = licenseGuid;
        }

        // Protective measure to not expose these to client unnecessarily
        public Guid? GetCompanyGuid() => _companyGuid == Guid.Empty ? (Guid?)null : _companyGuid;
        public Guid? GetLicenseGuid() => _licenseGuid == Guid.Empty ? (Guid?)null : _licenseGuid;
    }

    public class CompanyEditDTO : CompanyDTO
    {
        public bool CompanyTaxSupport { get; set; }
        public int MaxNrOfSMS { get; set; }
        public int BaseSysCurrencyId { get; set; }
        public int BaseEntCurrencyId { get; set; }
        public string CompanyApiKey { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        //EDI
        public string EdiUsername { get; set; }
        public string EdiPassword { get; set; }
        public bool IsEdiActivated { get; set; }
        public bool IsEdiGOActivated { get; set; }
        public DateTime? EdiActivated { get; set; }
        public string EdiActivatedBy { get; set; }
        public DateTime? EdiModified { get; set; }
        public string EdiModifiedBy { get; set; }

        // Payment information
        public int DefaultSysPaymentTypeId { get; set; }
        public PaymentInformationDTO PaymentInformation { get; set; }
        public List<ContactAddressItem> ContactAddresses { get; set; }
    }

    public class UserCompanySettingIODTO
    {
        public int? UserId { get; set; }
        public int SettingTypeId { get; set; }
        public int DataTypeId { get; set; }
        public string SettingName { get; set; }
        public string UserName { get; set; }
        public string EmployeeNr { get; set; }
        public string StringData { get; set; }
        public DateTime? DateData { get; set; }
        public int? IntData { get; set; }
        public bool? BoolData { get; set; }
        public decimal? DecimalData { get; set; }
    }

    public class CompanyInformationIODTO
    {
        public string CompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyShortName { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string EntCurrencyCode { get; set; }
        public string Country { get; set; }
        public string OrgNr { get; set; }
        public string VatNr { get; set; }
        public string InvoiceReference { get; set; }

        public string BgNumber { get; set; }
        public string PgNumber { get; set; }
        public string BankNumber { get; set; }
        public string CfpNumber { get; set; }
        public string SepaNumber { get; set; }
        public string NetsDirectNumber { get; set; }
        public string IbanNumber { get; set; }
        public string BicNumber { get; set; }

        public string DistributionAddress { get; set; }
        public string DistributionCoAddress { get; set; }
        public string DistributionPostalCode { get; set; }
        public string DistributionPostalAddress { get; set; }
        public string DistributionCountry { get; set; }

        public string BillingAddress { get; set; }
        public string BillingCoAddress { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingPostalAddress { get; set; }
        public string BillingCountry { get; set; }

        public string BoardHQAddress { get; set; }
        public string BoardHQCountry { get; set; }

        public string VisitingAddress { get; set; }
        public string VisitingCoAddress { get; set; }
        public string VisitingPostalCode { get; set; }
        public string VisitingPostalAddress { get; set; }
        public string VisitingCountry { get; set; }

        public string DeliveryAddress { get; set; }
        public string DeliveryCoAddress { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string DeliveryPostalAddress { get; set; }
        public string DeliveryCountry { get; set; }

        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string PhoneHome { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneJob { get; set; }
        public string Fax { get; set; }
        public string Webpage { get; set; }
    }

    #endregion

    #region CompanyExternalCode
    public class CompanyExternalCodeDTO
    {
        public int CompanyExternalCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string ExternalCode { get; set; }
        public TermGroup_CompanyExternalCodeEntity Entity { get; set; }
        public int RecordId { get; set; }
    }
    public class CompanyExternalCodeGridDTO
    {
        public int CompanyExternalCodeId { get; set; }
        public string ExternalCode { get; set; }
        public string EntityName { get; set; }
        public string RecordName { get; set; }
        public TermGroup_CompanyExternalCodeEntity Entity { get; set; }
        public int RecordId { get; set; }
    }
    #endregion

    #region CompanyGroup

    [TSInclude]
    public class CompanyGroupAdministrationDTO
    {
        public int CompanyGroupAdministrationId { get; set; }
        public int GroupCompanyActorCompanyId { get; set; }
        public int ChildActorCompanyId { get; set; }
        public string ChildActorCompanyName { get; set; }
        public int ChildActorCompanyNr { get; set; }
        public int CompanyGroupMappingHeadId { get; set; }
        public int AccountId { get; set; }
        public decimal? Conversionfactor { get; set; }
        public string Note { get; set; }
        public bool MatchInternalAccountOnNr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class CompanyGroupAdministrationGridDTO
    {
        public int CompanyGroupAdministrationId { get; set; }
        public int GroupCompanyActorCompanyId { get; set; }
        public int ChildActorCompanyId { get; set; }
        public string ChildCompanyName { get; set; }
        public int ChildCompanyNr { get; set; }
        public int CompanyGroupMappingHeadId { get; set; }
        public string MappingHeadName { get; set; }
        public int AccountId { get; set; }
        public decimal? Conversionfactor { get; set; }
        public string Note { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class CompanyGroupMappingHeadDTO
    {
        public int CompanyGroupMappingHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extension
        public List<CompanyGroupMappingRowDTO> Rows { get; set; }
    }

    [TSInclude]
    public class CompanyGroupMappingRowDTO
    {
        public int CompanyGroupMappingRowId { get; set; }
        public int CompanyGroupMappingHeadId { get; set; }
        public int ChildAccountFrom { get; set; }
        public int? ChildAccountTo { get; set; }
        public int GroupCompanyAccount { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extension
        public int RowNr { get; set; }

        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsProcessed { get; set; }

        public string ChildAccountFromName { get; set; }
        public string ChildAccountToName { get; set; }
        public string GroupCompanyAccountName { get; set; }
    }

    [TSInclude]
    public class CompanyGroupTransferHeadDTO
    {
        public int? CompanyGroupTransferHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? AccountYearId { get; set; }
        public string AccountYearText { get; set; }
        public int? FromAccountPeriodId { get; set; }
        public string FromAccountPeriodText { get; set; }
        public int? ToAccountPeriodId { get; set; }
        public string ToAccountPeriodText { get; set; }
        public CompanyGroupTransferType TransferType { get; set; }
        public string TransferTypeName { get; set; }
        public CompanyGroupTransferStatus TransferStatus { get; set; }
        public string TransferStatusName { get; set; }
        public DateTime? TransferDate { get; set; }
        public bool IsOnlyVoucher { get; set; }
        public List<CompanyGroupTransferRowDTO> CompanyGroupTransferRows { get; set; }
    }

    [TSInclude]
    public class CompanyGroupTransferRowDTO
    {
        public int? CompanyGroupTransferRowId { get; set; }
        public int? VoucherHeadId { get; set; }
        public int? BudgetHeadId { get; set; }
        public string BudgetName { get; set; }
        public long VoucherNr { get; set; }
        public string VoucherText { get; set; }
        public int VoucherSeriesId { get; set; }
        public string VoucherSeriesName { get; set; }
        public int ChildActorCompanyId { get; set; }
        public string ChildActorCompanyNrName { get; set; }
        public int AccountPeriodId { get; set; }
        public string AccountPeriodText { get; set; }
        public DateTime? Created { get; set; }
        public string Status { get; set; }
        public decimal? ConversionFactor { get; set; }

    }

    #endregion

    #region CompanyNews

    // TODO: Remove after TimeStampService is deleted

    public class CompanyNewsBaseDTO
    {
        public int CompanyNewsId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string SimpleText { get; set; }
    }

    #endregion

    #region CompTerms

    [TSInclude]
    public class CompTermDTO
    {
        public int CompTermId { get; set; }
        public int RecordId { get; set; }
        public SoeEntityState State { get; set; }
        public CompTermsRecordType RecordType { get; set; }
        public string Name { get; set; }
        public TermGroup_Languages Lang { get; set; }
        public string LangName { get; set; }
    }

    #endregion

    #region Connect

    #region ConnectHistory

    public class ConnectHistoryDTO
    {
        public int ConnectHistoryId { get; set; }
        public TermGroup_IOImportHeadType Type { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public string ImportedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int NumberOfItemsInImport { get; set; }
        public int NumberOfItemsImported { get; set; }
        public string Message { get; set; }
        public int Duration { get; set; }
        public bool Success { get; set; }
        public List<ConnectHistoryRowDTO> ConnectHistoryRowDTOs { get; set; }
    }

    public class ConnectHistoryRowDTO
    {
        public int ConnectHistoryRowId { get; set; }
        public int ConnectHistoryId { get; set; }
        public int XeId { get; set; }
        public int ParentXEId { get; set; }
        public string ExternalKey { get; set; }
        public string ExternalValue { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }

    #endregion

    #endregion

    #region ContactPerson

    [LogAttribute]
    [TSInclude]

    public class ContactPersonDTO
    {
        public int ActorContactPersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstAndLastName
        {
            get
            {
                return $"{this.FirstName} {this.LastName}";
            }
        }
        public int Position { get; set; }
        public string Description { get; set; }
        [LogSocSec]
        public string SocialSec { get; set; }
        public TermGroup_Sex Sex { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string PositionName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<CompanyCategoryRecordDTO> CategoryRecords;


        public bool HasConsent { get; set; }
        public DateTime? ConsentDate { get; set; }
        public DateTime? ConsentModified { get; set; }
        public string ConsentModifiedBy { get; set; }
        public List<int> CategoryIds { get; set; }
    }

    [TSInclude]
    public class ContactPersonGridDTO
    {
        public int ActorContactPersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstAndLastName { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }
        [LogSocSec]
        public string SocialSec { get; set; }
        public TermGroup_Sex Sex { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string PositionName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CategoryString { get; set; }

        // Actor relations
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }

        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNr { get; set; }


        public bool HasConsent { get; set; }
        public DateTime? ConsentDate { get; set; }
        public DateTime? ConsentModified { get; set; }
        public string ConsentModifiedBy { get; set; }
    }

    #endregion

    #region ContractGroup

    [TSInclude]
    public class ContractGroupDTO
    {
        public int ContractGroupId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_ContractGroupPeriod Period { get; set; }
        public int Interval { get; set; }
        public int DayInMonth { get; set; }
        public TermGroup_ContractGroupPriceManagement PriceManagement { get; set; }

        public string InvoiceText { get; set; }
        public string InvoiceTextRow { get; set; }

        public int? OrderTemplate { get; set; }
        public int? InvoiceTemplate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class ContractGroupGridDTO
    {
        public int ContractGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [TSInclude]
    public class ContractGroupExtendedGridDTO : ContractGroupGridDTO
    {
        public int PeriodId { get; set; }
        public string PeriodText { get; set; }
        public int Interval { get; set; }
        public string PriceManagementText { get; set; }
        public int DayInMonth { get; set; }
    }

    #endregion

    #region CustomerCentralOriginsGrid

    public class CustomerCentralOriginsGridDTO
    {
        public int OriginId { get; set; }
        public int? SeqNr { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Number { get; set; }
        public string Text { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PayAmount { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? PayDate { get; set; }
    }

    #endregion

    #region CustomerUser

    [TSInclude]
    public class CustomerUserDTO
    {
        public int CustomerUserId { get; set; }
        public int ActorCustomerId { get; set; }
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public bool Main { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string LoginName { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region DataStorage

    [TSInclude]
    public class DataStorageDTO
    {
        public int DataStorageId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ParentDataStorageId { get; set; }
        public int? EmployeeId { get; set; }
        public int? TimePeriodId { get; set; }
        public int? UserId { get; set; }

        public SoeDataStorageRecordType Type { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string Folder { get; set; }
        public byte[] Data { get; set; }
        public string Xml { get; set; }
        public int FileSize { get; set; }
        public SoeDataStorageOriginType OriginType { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool NeedsConfirmation { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public int SeqNr { get; set; }
        public string Information { get; set; }
        public DateTime? ExportDate { get; set; }

        public string DownloadURL { get; set; }

        public List<DataStorageRecipientDTO> DataStorageRecipients { get; set; }
        public List<DataStorageRecordDTO> DataStorageRecords { get; set; }

        public DateTime CreatedOrModified
        {
            get
            {
                List<DateTime> dates = new List<DateTime>();

                dates.Add(CalendarUtility.DATETIME_DEFAULT);
                if (Created.HasValue)
                    dates.Add(Created.Value);
                if (Modified.HasValue)
                    dates.Add(Modified.Value);
                if (ValidFrom.HasValue && ValidFrom.Value < DateTime.Now)
                    dates.Add(ValidFrom.Value);

                return dates.Max();
            }
        }

        [TsIgnore]
        public List<SoeEntityType> EntityTypes { get; set; }
    }

    public class DataStorageSmallDTO
    {
        public int DataStorageId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ParentDataStorageId { get; set; }
        public int? EmployeeId { get; set; }
        public int? TimePeriodId { get; set; }

        public SoeDataStorageRecordType Type { get; set; }
        public string Description { get; set; }
        public int NoOfChildrens { get; set; }
        public string TimePeriodName { get; set; }
        public DateTime TimePeriodStartDate { get; set; }
        public string TimePeriodStartDateString
        {
            get
            {
                return TimePeriodStartDate.ToString(CalendarUtility.SHORTDATEMASK);
            }
        }
        public DateTime TimePeriodStopDate { get; set; }
        public string TimePeriodStopDateString
        {
            get
            {
                return TimePeriodStopDate.ToString(CalendarUtility.SHORTDATEMASK);
            }
        }
        public DateTime? TimePeriodPaymentDate { get; set; }
        public int? Year { get; set; }
        public decimal? NetSalary { get; set; }

        //XML and placeholders for specific data in the XML. Depends on which type the DataStorage has. Add placeholders when needed
        public string XML { get; set; }
        public string XMLValue1 { get; set; }
    }

    public class DataStorageAllDTO
    {
        public int DataStorageId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ParentDataStorageId { get; set; }
        public int? EmployeeId { get; set; }
        public int? TimePeriodId { get; set; }

        public SoeDataStorageRecordType Type { get; set; }
        public string Description { get; set; }
        public int NoOfChildrens { get; set; }
        public string TimePeriodName { get; set; }
        public DateTime TimePeriodStartDate { get; set; }
        public string TimePeriodStartDateString
        {
            get
            {
                return TimePeriodStartDate.ToString(CalendarUtility.SHORTDATEMASK);
            }
        }
        public DateTime TimePeriodStopDate { get; set; }
        public DateTime? TimePeriodPaymentDate { get; set; }
        public string TimePeriodStopDateString
        {
            get
            {
                return TimePeriodStopDate.ToString(CalendarUtility.SHORTDATEMASK);
            }
        }

        //XML and placeholders for specific data in the XML. Depends on which type the DataStorage has. Add placeholders when needed
        public string XML { get; set; }
        public byte[] Data { get; set; }
        public string XMLValue1 { get; set; }

    }

    [TSInclude]
    public class DataStorageRecordDTO
    {
        public int DataStorageRecordId { get; set; }
        public int RecordId { get; set; }
        public SoeEntityType Entity { get; set; }
        public int? AttestStateId { get; set; }
        public string CurrentAttestUsers { get; set; }
        public TermGroup_DataStorageRecordAttestStatus AttestStatus { get; set; }

        // Extensions
        public byte[] Data { get; set; }
        public SoeDataStorageRecordType Type { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        // Permissions
        public List<int> RoleIds { get; set; }
    }

    public class DataStorageRecordExtendedDTO : DataStorageRecordDTO
    {
        public string RecordNumber { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
    }

    [TSInclude]
    public class DataStorageRecipientDTO
    {
        public int DataStorageRecipientId { get; set; }
        public int DataStorageId { get; set; }
        public int UserId { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string UserName { get; set; }
        public string EmployeeNrAndName { get; set; }
    }

    #endregion

    #region Dashboard

    #region UserGauge
    public class UserGaugeHeadDTO
    {
        public int UserGaugeHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public SoeModule Module { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Priority { get; set; }
        public List<UserGaugeDTO> UserGauges { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

    }
    public class UserGaugeDTO
    {
        public int UserGaugeId { get; set; }
        public int? UserGaugeHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public int SysGaugeId { get; set; }

        public SoeModule Module { get; set; }
        public int Sort { get; set; }
        public int WindowState { get; set; }

        // Extensions
        public string SysGaugeName { get; set; }
        public List<UserGaugeSettingDTO> UserGaugeSettings { get; set; }
    }

    public class UserGaugeSettingDTO
    {
        public int UserGaugeSettingId { get; set; }
        public int UserGaugeId { get; set; }

        public int DataType { get; set; }
        public string Name { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }
    }

    #endregion

    #region Gauges

    #region PerformanceMonitorGauge

    public class PerformanceMonitorGaugeDTO
    {
        public TermGroup_SysPerformanceMonitorTask Task { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public int Duration1 { get; set; }
        public int Duration2 { get; set; }
        public int Duration3 { get; set; }
        public int Duration4 { get; set; }
        public int Duration5 { get; set; }
        public int Duration6 { get; set; }
        public int Duration7 { get; set; }
        public int Duration8 { get; set; }
        public int NbrOfRecords1 { get; set; }
        public int NbrOfRecords2 { get; set; }
        public int NbrOfRecords3 { get; set; }
        public int NbrOfRecords4 { get; set; }
        public int NbrOfRecords5 { get; set; }
        public int NbrOfRecords6 { get; set; }
        public int NbrOfRecords7 { get; set; }
        public int NbrOfRecords8 { get; set; }
        public int NbrOfSubRecords1 { get; set; }
        public int NbrOfSubRecords2 { get; set; }
        public int NbrOfSubRecords3 { get; set; }
        public int NbrOfSubRecords4 { get; set; }
        public int NbrOfSubRecords5 { get; set; }
        public int NbrOfSubRecords6 { get; set; }
        public int NbrOfSubRecords7 { get; set; }
        public int NbrOfSubRecords8 { get; set; }
        public string Color1 { get; set; }
        public string Color2 { get; set; }
        public string Color3 { get; set; }
        public string Color4 { get; set; }
        public string Color5 { get; set; }
        public string Color6 { get; set; }
        public string Color7 { get; set; }
        public string Color8 { get; set; }
    }

    #endregion

    #endregion

    #endregion

    #region DayType

    [TSInclude]
    public class DayTypeDTO
    {
        public int DayTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? SysDayTypeId { get; set; }
        public TermGroup_SysDayType Type { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? StandardWeekdayFrom { get; set; }
        public int? StandardWeekdayTo { get; set; }
        public bool WeekendSalary { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        [TSIgnore]
        public bool Import { get; set; }

        [TSIgnore]
        public List<TimeHalfdayDTO> TimeHalfdays { get; set; }

        [TSIgnore]
        public List<EmployeeGroupDTO> EmployeeGroups { get; set; }

        public DayTypeDTO()
        {
            this.TimeHalfdays = new List<TimeHalfdayDTO>();
        }
    }

    [TSInclude]
    public class DayTypeGridDTO
    {
        public int DayTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]

    public class DayTypeAndWeekdayDTO
    {
        public int? DayTypeId { get; set; }
        public int? WeekdayNr { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region DeliveryCondition

    [TSInclude]

    public class DeliveryConditionDTO
    {
        public int DeliveryConditionId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    [TSInclude]

    public class DeliveryConditionGridDTO
    {
        public int DeliveryConditionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region DeliveryType
    [TSInclude]
    public class DeliveryTypeDTO
    {
        public int DeliveryTypeId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
    [TSInclude]
    public class DeliveryTypeGridDTO
    {
        public int DeliveryTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region Document

    [TSInclude]
    public class DocumentDTO
    {
        public int DataStorageId { get; set; }
        public int? MessageId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int? FileSize { get; set; }
        public string Folder { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public DateTime? ReadDate { get; set; }
        [TsIgnore]
        public DateTime? ConfirmedDate { get; set; }
        public bool NeedsConfirmation { get; set; }
        public XEMailAnswerType AnswerType { get; set; }
        public DateTime? AnswerDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Relations
        public List<DataStorageRecordDTO> Records { get; set; }
        public List<DataStorageRecipientDTO> Recipients { get; set; }

        // Extensions
        public List<int> MessageGroupIds { get; set; }

        [TsIgnore]
        public TermGroup_DataStorageRecordAttestStatus AttestStatus { get; set; }
        [TsIgnore]
        public string CurrentAttestUsers { get; set; }
        [TsIgnore]
        public int? AttestStateId { get; set; }

        public string DisplayName
        {
            get
            {
                string name = Name;
                if (String.IsNullOrEmpty(name))
                    name = Description;
                if (String.IsNullOrEmpty(name))
                    name = FileName;
                return name;
            }
        }
    }

    #endregion

    #region DistributionCode
    [TSInclude]
    public class DistributionCodeHeadDTO
    {
        public int DistributionCodeHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TypeId { get; set; }
        public String Type { get; set; }
        public int NoOfPeriods { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public int? SubType { get; set; }
        public string TypeOfPeriod { get; set; }
        public int? OpeningHoursId { get; set; }
        public int? AccountDimId { get; set; }
        public string AccountDim { get; set; }
        public string OpeningHour { get; set; }
        public bool IsInUse { get; set; }
        public DateTime? FromDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public List<DistributionCodePeriodDTO> Periods { get; set; }
    }

    [TSInclude]
    public class DistributionCodePeriodDTO
    {
        public int DistributionCodePeriodId { get; set; }
        public int? ParentToDistributionCodePeriodId { get; set; }
        public int Number { get; set; }
        public bool IsAdded { get; set; }
        public bool IsModified { get; set; }
        public Decimal Percent { get; set; }
        public string Comment { get; set; }
        public string PeriodSubTypeName { get; set; }
    }

    [TSInclude]
    public class DistributionCodeGridDTO
    {
        public int DistributionCodeHeadId { get; set; }
        public int TypeId { get; set; }
        public int TypeOfPeriodId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeOfPeriod { get; set; }
        public string SubLevel { get; set; }
        public string OpeningHour { get; set; }
        public string AccountDim { get; set; }
        public int NoOfPeriods { get; set; }
        public DateTime? FromDate { get; set; }
    }

    public class DistributionCodeHeadIODTO
    {
        public int Type { get; set; }
        public int NoOfPeriods { get; set; }
        public string Name { get; set; }

        public List<DistributionCodePeriodIODTO> Periods { get; set; }
    }

    public class DistributionCodePeriodIODTO
    {
        public int Number { get; set; }
        public Decimal Percent { get; set; }
        public string Comment { get; set; }
    }

    #endregion

    #region DrilldownReport

    public class DrilldownReportGridDTO
    {
        public int ReportId { get; set; }
        public int ReportNr { get; set; }
        public string ReportName { get; set; }
        public int? sysReportTemplateTypeId { get; set; }
        public List<ReportGroupsDrilldownDTO> ReportGroups { get; set; }
    }

    public class ReportGroupsDrilldownDTO
    {
        public int ReportGroupOrder { get; set; }
        public string ReportGroupName { get; set; }
        public decimal PeriodAmount { get; set; }
        public decimal YearAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal PrevPeriodAmount { get; set; }
        public decimal PrevYearAmount { get; set; }
        public decimal BudgetPeriodAmount { get; set; }
        public decimal BudgetToPeriodEndAmount { get; set; }
        public decimal PeriodPrevPeriodDiff { get; set; }
        public decimal YearPrevYearDiff { get; set; }
        public decimal PeriodBudgetDiff { get; set; }
        public decimal YearBudgetDiff { get; set; }
        public List<ReportHeadersDrilldownDTO> ReportHeaders { get; set; }
    }

    public class ReportHeadersDrilldownDTO
    {
        public int ReportHeaderOrder { get; set; }
        public string ReportHeaderName { get; set; }
        public decimal PeriodAmount { get; set; }
        public decimal YearAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal PrevPeriodAmount { get; set; }
        public decimal PrevYearAmount { get; set; }
        public decimal BudgetPeriodAmount { get; set; }
        public decimal BudgetToPeriodEndAmount { get; set; }
        public decimal PeriodPrevPeriodDiff { get; set; }
        public decimal YearPrevYearDiff { get; set; }
        public decimal PeriodBudgetDiff { get; set; }
        public decimal YearBudgetDiff { get; set; }
        public List<AccountsDrilldownDTO> Accounts { get; set; }
    }

    public class AccountsDrilldownDTO
    {
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public decimal PeriodAmount { get; set; }
        public decimal YearAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal PrevPeriodAmount { get; set; }
        public decimal PrevYearAmount { get; set; }
        public decimal BudgetPeriodAmount { get; set; }
        public decimal BudgetToPeriodEndAmount { get; set; }
        public decimal PeriodPrevPeriodDiff { get; set; }
        public decimal YearPrevYearDiff { get; set; }
        public decimal PeriodBudgetDiff { get; set; }
        public decimal YearBudgetDiff { get; set; }
    }

    [TSInclude]
    public class DrilldownReportGridFlattenedDTO
    {
        // ReportGroup
        public int ReportGroupOrder { get; set; }
        public string ReportGroupName { get; set; }

        // ReportHeader
        public int ReportHeaderOrder { get; set; }
        public string ReportHeaderName { get; set; }

        // Account
        public string AccountNr { get; set; }
        public string AccountNrCount { get; set; }
        public string AccountName { get; set; }
        public decimal PeriodAmount { get; set; }
        public decimal YearAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal PrevPeriodAmount { get; set; }
        public decimal PrevYearAmount { get; set; }
        public decimal BudgetPeriodAmount { get; set; }
        public decimal BudgetToPeriodEndAmount { get; set; }
        public decimal PeriodPrevPeriodDiff { get; set; }
        public decimal YearPrevYearDiff { get; set; }
        public decimal PeriodBudgetDiff { get; set; }
        public decimal YearBudgetDiff { get; set; }
    }

    #endregion

    #region EDI/Scanning

    public class CompanyEdiDTO
    {
        public enum SourceTypeEnum
        {
            FTP = 0,
            File = 1,
            Xml = 2,
        }

        public int CompanyEdiId { get; set; }
        public int ActorCompanyId { get; set; }
        public string CompanyName { get; set; }

        public int Type { get; set; }
        public string Source { get; set; }
        public SourceTypeEnum SourceType { get; set; }
        public string FileName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class CompanyEdiEntryDTO
    {
        public int ActorCompanyId { get; set; }
        public int SysWholesellerId { get; set; }
        public string WholesellerName { get; set; }
        public string LicenseNr { get; set; }
        public string CompanyName { get; set; }
        public int? CompanyNr { get; set; }
        public string OrgNr { get; set; }
        public string MessageTypeName { get; set; }
    }

    [TSInclude]
    public class EdiEntryDTO
    {
        //Edi
        public int EdiEntryId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_EDISourceType Type { get; set; }
        public TermGroup_EDIStatus Status { get; set; }
        public TermGroup_EdiMessageType MessageType { get; set; }
        public int SysWholesellerId { get; set; }
        public string WholesellerName { get; set; }
        public string BuyerId { get; set; }
        public string BuyerReference { get; set; }
        public decimal? VatRate { get; set; }
        public string PostalGiro { get; set; }
        public string BankGiro { get; set; }
        public string OCR { get; set; }
        public string IBAN { get; set; }
        public TermGroup_BillingType? BillingType { get; set; }
        public string XML { get; set; }
        public byte[] PDF { get; set; }
        public string FileName { get; set; }
        public int ErrorCode { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Scanning
        public int? ScanningEntryArrivalId { get; set; }
        public int? ScanningEntryInvoiceId { get; set; }

        //Dates
        public DateTime? Date { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        //Sum
        public decimal Sum { get; set; }
        public decimal SumCurrency { get; set; }
        public decimal SumVat { get; set; }
        public decimal SumVatCurrency { get; set; }

        //Currency
        public int CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }

        //Order
        public int? OrderId { get; set; }
        public TermGroup_EDIOrderStatus OrderStatus { get; set; }
        public string OrderNr { get; set; }

        //Invoice
        public int? InvoiceId { get; set; }
        public TermGroup_EDIInvoiceStatus InvoiceStatus { get; set; }
        public int? SeqNr { get; set; }
        public string InvoiceNr { get; set; }

        //Supplier
        public int? ActorSupplierId { get; set; }
        public string SellerOrderNr { get; set; }
        public bool HasPDF { get; set; }

        //Extensions
        public ScanningEntryDTO ScanningEntryInvoice { get; set; }
    }

    public class EdiEntryImageDTO
    {
        public int EdiEntryId { get; set; }
        public TermGroup_EDISourceType Type { get; set; }
        /// <summary>
        /// Image can be either PDF or and actual Image
        /// </summary>
        public byte[] Image { get; set; }
        /// <summary>
        /// HasImage applies to both a PDF and image
        /// </summary>
        public bool? HasImage { get; set; }
    }

    [TSInclude]
    public class ScanningEntryDTO
    {
        public int ScanningEntryId { get; set; }
        public int ActorCompanyId { get; set; }
        public string BatchId { get; set; }
        public string CompanyId { get; set; }

        public int Type { get; set; }
        public TermGroup_ScanningMessageType MessageType { get; set; }
        public TermGroup_ScanningStatus Status { get; set; }

        public byte[] Image { get; set; }
        public string XML { get; set; }

        public int ErrorCode { get; set; }
        public string OperatorMessage { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public IEnumerable<ScanningEntryRowDTO> ScanningEntryRow { get; set; }
    }

    [TSInclude]
    public class ScanningEntryRowDTO
    {
        public ScanningEntryRowType Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Format { get; set; }
        public string ValidationError { get; set; }
        public string Position { get; set; }
        public string PageNumber { get; set; }
        public string NewText { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Email
    [TSInclude]
    public class EmailTemplateDTO
    {
        public int EmailTemplateId { get; set; }
        public int ActorCompanyId { get; set; }

        public int Type { get; set; }   // TODO: Not used?
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Typename { get; set; }
        public bool BodyIsHTML { get; set; }
    }

    [TSInclude]
    public class EmailDocumentsRequestDTO
    {
        public List<int> FileRecordIds { get; set; } // List of DataStorageRecordId values
        public List<int> RecipientUserIds { get; set; }
        public string SingleRecipient { get; set; }
        public string[] EmailAddresses { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    #endregion

    #region Employee
    [LogAttribute]
    public class EmployeeDTO : IEmployee
    {
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int ContactPersonId { get; set; }
        public int? UserId { get; set; }
        public int? TimeCodeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int ProjectDefaultTimeCodeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeNrSort { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [TsIgnore]
        public string NumberAndName
        {
            get
            {
                return $"({this.EmployeeNr}) {this.Name}";
            }
        }
        [LogSocSec]
        public string SocialSec { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? CalculatedCostPerHour { get; set; }
        public string CardNumber { get; set; }
        public bool Hidden { get; set; }
        public bool Vacant { get; set; }
        public bool UseFlexForce { get; set; }
        public TermGroup_EmployeeDisbursementMethod DisbursementMethod { get; set; }
        public string DisbursementMethodName { get; set; }
        public string DisbursementClearingNr { get; set; }
        public string DisbursementAccountNr { get; set; }
        public string DisbursementCountryCode { get; set; }
        public string DisbursementBIC { get; set; }
        public string DisbursementIBAN { get; set; }
        public bool DisbursementAccountNrIsMissing
        {
            get
            {
                return this.DisbursementMethod == TermGroup_EmployeeDisbursementMethod.SE_AccountDeposit && string.IsNullOrEmpty(this.DisbursementAccountNr);
            }
        }
        public bool DisbursementMethodIsCash
        {
            get
            {
                return this.DisbursementMethod == TermGroup_EmployeeDisbursementMethod.SE_CashDeposit;
            }
        }
        public bool DisbursementMethodIsUnknown
        {
            get
            {
                return this.DisbursementMethod == TermGroup_EmployeeDisbursementMethod.Unknown;
            }
        }
        public bool TaxSettingsAreMissing
        {
            get
            {
                return (this.EmployeeTaxSE == null) || (this.EmployeeTaxSE.Type == TermGroup_EmployeeTaxType.NotSelected);
            }
        }
        public bool DontValidateDisbursementAccountNr { get; set; }
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
        public string Note { get; set; }
        public bool ShowNote { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Deleted { get; set; }
        public string DeletedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string ActiveString { get; set; }
        public bool ExcludeFromPayroll { get; set; }

        // Relations
        public List<EmploymentDTO> Employments { get; set; }
        public List<EmployeeFactorDTO> Factors { get; set; }
        public EmployeeVacationSEDTO EmployeeVacationSE { get; set; }
        public EmployeeTaxSEDTO EmployeeTaxSE { get; set; }

        // Extensions
        public int CurrentEmployeeGroupId { get; set; }
        public string CurrentEmployeeGroupName { get; set; }
        public int? CurrentEmployeeGroupTimeDeviationCauseId { get; set; }
        public List<string> EmployeeGroupNames { get; set; }
        public string EmployeeGroupNamesString { get; set; }
        public int CurrentPayrollGroupId { get; set; }
        public string CurrentPayrollGroupName { get; set; }
        public List<string> PayrollGroupNames { get; set; }
        public string PayrollGroupNamesString { get; set; }
        public int CurrentVacationGroupId { get; set; }
        public string CurrentVacationGroupName { get; set; }
        public decimal? CurrentEmploymentPercent { get; set; }
        public List<string> RoleNames { get; set; }
        public string RoleNamesString { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CategoryNamesString { get; set; }
        public string TimeCodeName { get; set; }
        public DateTime? FinalSalaryEndDate { get; set; }
        public DateTime? FinalSalaryEndDateApplied { get; set; }
        public int? FinalSalaryAppliedTimePeriodId { get; set; }
        public string CurrentEmploymentTypeString { get; set; }
        public string CurrentEmploymentDateFromString { get; set; }
        public string CurrentEmploymentDateToString { get; set; }
        public DateTime OrderDate { get; set; }
        [TsIgnore]
        public int OriginalEmployeeId { get; set; }

    }

    [TSInclude]
    public class EmployeeSmallDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
    }

    [TSInclude]
    public class EmployeeTimeCodeDTO : EmployeeSmallDTO
    {
        public int DefaultTimeCodeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int EmployeeGroupId { get; set; }
        public bool AutoGenTimeAndBreakForProject { get; set; }
    }

    [TSInclude]
    [Log]
    public class EmployeeGridDTO
    {
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        [LogSocSec]
        public string SocialSec { get; set; }
        public TermGroup_Sex Sex { get; set; }
        public string SexString { get; set; }
        public int Age { get; set; }
        public bool Vacant { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeGroupNamesString { get; set; }
        public string EmploymentTypeString { get; set; }
        public string PayrollGroupNamesString { get; set; }
        public string AnnualLeaveGroupNamesString { get; set; }
        public string CurrentVacationGroupName { get; set; }
        public string RoleNamesString { get; set; }
        public string CategoryNamesString { get; set; }
        public string AccountNamesString { get; set; }
        public int WorkTimeWeek { get; set; }
        public decimal Percent { get; set; }
        public DateTime? EmploymentStart { get; set; }
        public DateTime? EmploymentStop { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public DateTime? UserBlockedFromDate { get; set; }
    }

    [TSInclude]
    public class AvailableEmployeesDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        [TsIgnore]
        public string EmployeeName { get; set; }
        public bool WantsExtraShifts { get; set; }
        public int ScheduleMinutes { get; set; }
        public int? EmploymentDays { get; set; }
        public int? Age { get; set; }
    }

    #region EmployeeIO

    public class EmployeeIODTO
    {
        #region Ctor

        public EmployeeIODTO()
        {
            //Should not be used. Only for serialization and webservice calls
        }

        public EmployeeIODTO(TermGroup_IOSource source, TermGroup_IOType ioType, TermGroup_IOImportHeadType headType, int actorCompanyId, int? employeeId = null)
        {
            Configure(source, ioType, headType, actorCompanyId, employeeId);
        }

        #endregion

        #region Keys

        public int EmployeeIOId { get; set; }
        public int? EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }

        #endregion

        #region Core

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public TermGroup_IOType IOType { get; set; }
        public TermGroup_IOImportHeadType HeadType { get; set; }
        public TermGroup_IOStatus Status { get; set; }

        #endregion

        #region Personal information

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SocialSec { get; set; }
        public int? Sex { get; set; }
        public string Email { get; set; }
        public string DistributionAddress { get; set; }
        public string DistributionCoAddress { get; set; }
        public string DistributionPostalCode { get; set; }
        public string DistributionPostalAddress { get; set; }
        public string DistributionCountry { get; set; }
        public string PhoneHome { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneJob { get; set; }
        public string ClosestRelativeNr { get; set; }
        public string ClosestRelativeName { get; set; }
        public string ClosestRelativeRelation { get; set; }
        public string ClosestRelativeNr2 { get; set; }
        public string ClosestRelativeName2 { get; set; }
        public string ClosestRelativeRelation2 { get; set; }
        public int? DisbursementMethod { get; set; }
        public string DisbursementClearingNr { get; set; }
        public string DisbursementAccountNr { get; set; }
        public bool? DontValidateDisbursementAccountNr { get; set; }
        public string LoginName { get; set; }
        public string LangId { get; set; }
        public string DefaultCompanyName { get; set; }
        public string RoleName1 { get; set; }
        public string RoleName2 { get; set; }
        public string RoleName3 { get; set; }
        public string AttestRoleName1 { get; set; }
        public string AttestRoleName2 { get; set; }
        public string AttestRoleName3 { get; set; }
        public string AttestRoleName4 { get; set; }
        public string AttestRoleName5 { get; set; }
        public string AttestRoleAccount1 { get; set; }
        public string AttestRoleAccount2 { get; set; }
        public string AttestRoleAccount3 { get; set; }
        public string AttestRoleAccount4 { get; set; }
        public string AttestRoleAccount5 { get; set; }

        #endregion

        #region Employment information

        public string EmployeeNr { get; set; }
        public int? EmploymentType { get; set; }
        public string EmploymentTypeCode { get; set; }
        public string EmployeeGroupName { get; set; }
        public string PayrollGroupName { get; set; }
        public string VacationGroupName { get; set; }
        public DateTime? EmploymentDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? WorkTimeWeek { get; set; }
        public decimal? WorkPercentage { get; set; }
        public string EmploymentPriceTypeCode { get; set; }
        public string EmploymentPayrollLevelCode { get; set; }
        public DateTime? EmploymentPriceTypeFromDate { get; set; }
        public decimal? EmploymentPriceTypeAmount { get; set; }
        public string CostAccountStd { get; set; }
        public string CostAccountInternal1 { get; set; }
        public string CostAccountInternal2 { get; set; }
        public string CostAccountInternal3 { get; set; }
        public string CostAccountInternal4 { get; set; }
        public string CostAccountInternal5 { get; set; }
        public string IncomeAccountStd { get; set; }
        public string IncomeAccountInternal1 { get; set; }
        public string IncomeAccountInternal2 { get; set; }
        public string IncomeAccountInternal3 { get; set; }
        public string IncomeAccountInternal4 { get; set; }
        public string IncomeAccountInternal5 { get; set; }
        public decimal? EarnedDaysPaid { get; set; }
        public decimal? UsedDaysPaid { get; set; }
        public decimal? RemainingDaysPaid { get; set; }
        public decimal? EmploymentRatePaid { get; set; }
        public decimal? PaidVacationAllowance { get; set; }
        public decimal? PaidVacationVariableAllowance { get; set; }
        public decimal? EarnedDaysUnpaid { get; set; }
        public decimal? UsedDaysUnpaid { get; set; }
        public decimal? RemainingDaysUnpaid { get; set; }
        public decimal? EarnedDaysAdvance { get; set; }
        public decimal? UsedDaysAdvance { get; set; }
        public decimal? RemainingDaysAdvance { get; set; }
        public decimal? DebtInAdvanceAmount { get; set; }
        public DateTime? DebtInAdvanceDueDate { get; set; }
        public decimal? SavedDaysYear1 { get; set; }
        public decimal? UsedDaysYear1 { get; set; }
        public decimal? RemainingDaysYear1 { get; set; }
        public decimal? EmploymentRateYear1 { get; set; }
        public decimal? SavedDaysYear2 { get; set; }
        public decimal? UsedDaysYear2 { get; set; }
        public decimal? RemainingDaysYear2 { get; set; }
        public decimal? EmploymentRateYear2 { get; set; }
        public decimal? SavedDaysYear3 { get; set; }
        public decimal? UsedDaysYear3 { get; set; }
        public decimal? RemainingDaysYear3 { get; set; }
        public decimal? EmploymentRateYear3 { get; set; }
        public decimal? SavedDaysYear4 { get; set; }
        public decimal? UsedDaysYear4 { get; set; }
        public decimal? RemainingDaysYear4 { get; set; }
        public decimal? EmploymentRateYear4 { get; set; }
        public decimal? SavedDaysYear5 { get; set; }
        public decimal? UsedDaysYear5 { get; set; }
        public decimal? RemainingDaysYear5 { get; set; }
        public decimal? EmploymentRateYear5 { get; set; }
        public decimal? SavedDaysOverdue { get; set; }
        public decimal? UsedDaysOverdue { get; set; }
        public decimal? RemainingDaysOverdue { get; set; }
        public decimal? EmploymentRateOverdue { get; set; }
        public bool? HighRiskProtection { get; set; }
        public DateTime? HighRiskProtectionTo { get; set; }
        public bool? MedicalCertificateReminder { get; set; }
        public int? MedicalCertificateDays { get; set; }
        public bool? Absence105DaysExcluded { get; set; }
        public int? Absence105DaysExcludedDays { get; set; }
        public int? EmployeeFactorType { get; set; }
        public DateTime? EmployeeFactorFromDate { get; set; }
        public decimal? EmployeeFactorFactor { get; set; }
        public int? PayrollStatisticsPersonalCategory { get; set; }
        public int? PayrollStatisticsWorkTimeCategory { get; set; }
        public int? PayrollStatisticsSalaryType { get; set; }
        public int? PayrollStatisticsWorkPlaceNumber { get; set; }
        public int? PayrollStatisticsCFARNumber { get; set; }
        public string WorkPlaceSCB { get; set; }
        public int? AFACategory { get; set; }
        public int? AFASpecialAgreement { get; set; }
        public string AFAWorkplaceNr { get; set; }
        public int? CollectumITPPlan { get; set; }
        public string CollectumCostPlace { get; set; }
        public string CollectumAgreedOnProduct { get; set; }
        public string CategoryCode1 { get; set; }
        public string CategoryCode2 { get; set; }
        public string CategoryCode3 { get; set; }
        public string CategoryCode4 { get; set; }
        public string CategoryCode5 { get; set; }
        public string SecondaryCategoryCode1 { get; set; }
        public string SecondaryCategoryCode2 { get; set; }
        public string SecondaryCategoryCode3 { get; set; }
        public string SecondaryCategoryCode4 { get; set; }
        public string SecondaryCategoryCode5 { get; set; }
        public string EmployeeAccount1 { get; set; }
        public string EmployeeAccount2 { get; set; }
        public string EmployeeAccount3 { get; set; }
        public bool? EmployeeAccountDefault1 { get; set; }
        public bool? EmployeeAccountDefault2 { get; set; }
        public bool? EmployeeAccountDefault3 { get; set; }
        public DateTime? EmployeeAccountStartDate1 { get; set; }
        public DateTime? EmployeeAccountStartDate2 { get; set; }
        public DateTime? EmployeeAccountStartDate3 { get; set; }
        public string DefaultTimeDeviationCauseName { get; set; }
        public string DefaultTimeCodeName { get; set; }
        public int ExperienceMonths { get; set; }
        public bool ExperienceAgreedOrEstablished { get; set; }
        public string WorkPlace { get; set; }

        #endregion

        #region HR

        public string EmployeePositionCode { get; set; }
        public string Note { get; set; }
        public bool? ShowNote { get; set; }

        #endregion

        #region Public methods

        public void Configure(TermGroup_IOSource source, TermGroup_IOType ioType, TermGroup_IOImportHeadType headType, int actorCompanyId, int? employeeId = null)
        {
            this.Source = source;
            this.IOType = ioType;
            this.HeadType = headType;
            this.Status = TermGroup_IOStatus.Unprocessed;
            this.ActorCompanyId = actorCompanyId;
            this.EmployeeId = employeeId;
        }

        public bool HasAnyVacationFieldsValue()
        {
            return NumberUtility.HasAnyValue(
                EarnedDaysPaid, EarnedDaysUnpaid, EarnedDaysAdvance,
                SavedDaysYear1, SavedDaysYear2, SavedDaysYear3, SavedDaysYear4, SavedDaysYear5, SavedDaysOverdue,
                UsedDaysPaid, PaidVacationAllowance, UsedDaysUnpaid, UsedDaysAdvance, UsedDaysYear1, UsedDaysYear2, UsedDaysYear3, UsedDaysYear4, UsedDaysYear5, UsedDaysOverdue,
                RemainingDaysPaid, RemainingDaysUnpaid, RemainingDaysAdvance, RemainingDaysYear1, RemainingDaysYear2, RemainingDaysYear3, RemainingDaysYear4, RemainingDaysYear5, RemainingDaysOverdue,
                EmploymentRatePaid, EmploymentRateYear1, EmploymentRateYear2, EmploymentRateYear3, EmploymentRateYear4, EmploymentRateYear5, EmploymentRateOverdue,
                DebtInAdvanceAmount, PaidVacationVariableAllowance
            );
        }

        public void SetPrimaryCategories(List<string> categoryNames)
        {
            if (categoryNames.IsNullOrEmpty())
                return;

            if (categoryNames.Count >= 1)
                this.CategoryCode1 = categoryNames[0];
            if (categoryNames.Count >= 2)
                this.CategoryCode2 = categoryNames[1];
            if (categoryNames.Count >= 3)
                this.CategoryCode3 = categoryNames[2];
            if (categoryNames.Count >= 4)
                this.CategoryCode4 = categoryNames[3];
            if (categoryNames.Count >= 5)
                this.CategoryCode5 = categoryNames[4];
        }
        public void SetSecondaryCategories(List<string> categoryNames)
        {
            if (categoryNames.IsNullOrEmpty())
                return;

            if (categoryNames.Count >= 1)
                this.SecondaryCategoryCode1 = categoryNames[0];
            if (categoryNames.Count >= 2)
                this.SecondaryCategoryCode2 = categoryNames[1];
            if (categoryNames.Count >= 3)
                this.SecondaryCategoryCode3 = categoryNames[2];
            if (categoryNames.Count >= 4)
                this.SecondaryCategoryCode4 = categoryNames[3];
            if (categoryNames.Count >= 5)
                this.SecondaryCategoryCode5 = categoryNames[4];
        }

        public void SetCostAccounting((string AccountNr, List<string> AccountInternals) cost)
        {
            this.CostAccountStd = cost.AccountNr;

            if (cost.AccountInternals.IsNullOrEmpty())
                return;

            if (cost.AccountInternals.Count >= 1)
                this.CostAccountInternal1 = cost.AccountInternals[0];
            if (cost.AccountInternals.Count >= 2)
                this.CostAccountInternal2 = cost.AccountInternals[1];
            if (cost.AccountInternals.Count >= 3)
                this.CostAccountInternal3 = cost.AccountInternals[2];
            if (cost.AccountInternals.Count >= 4)
                this.CostAccountInternal4 = cost.AccountInternals[3];
            if (cost.AccountInternals.Count >= 5)
                this.CostAccountInternal5 = cost.AccountInternals[4];
        }
        public void SetIncomeAccounting((string AccountNr, List<string> AccountInternals) income)
        {
            this.IncomeAccountStd = income.AccountNr;

            if (income.AccountInternals.IsNullOrEmpty())
                return;

            if (income.AccountInternals.Count >= 1)
                this.IncomeAccountInternal1 = income.AccountInternals[0];
            if (income.AccountInternals.Count >= 2)
                this.IncomeAccountInternal2 = income.AccountInternals[1];
            if (income.AccountInternals.Count >= 3)
                this.IncomeAccountInternal3 = income.AccountInternals[2];
            if (income.AccountInternals.Count >= 4)
                this.IncomeAccountInternal4 = income.AccountInternals[3];
            if (income.AccountInternals.Count >= 5)
                this.IncomeAccountInternal5 = income.AccountInternals[4];
        }

        public void SetUserRoles(List<string> roleNames)
        {
            if (roleNames.IsNullOrEmpty())
                return;

            if (roleNames.Count >= 1)
                this.RoleName1 = roleNames[0];
            if (roleNames.Count >= 2)
                this.RoleName2 = roleNames[1];
            if (roleNames.Count >= 3)
                this.RoleName3 = roleNames[2];
        }

        public void SetAttestRoles(List<string> attestRoleNames)
        {
            if (attestRoleNames.IsNullOrEmpty())
                return;

            if (attestRoleNames.Count >= 1)
                this.AttestRoleName1 = attestRoleNames[0];
            if (attestRoleNames.Count >= 2)
                this.AttestRoleName2 = attestRoleNames[1];
            if (attestRoleNames.Count >= 3)
                this.AttestRoleName3 = attestRoleNames[2];
            if (attestRoleNames.Count >= 4)
                this.AttestRoleName4 = attestRoleNames[3];
            if (attestRoleNames.Count >= 5)
                this.AttestRoleName5 = attestRoleNames[4];
        }

        #endregion
    }

    #endregion

    #endregion

    #region EmployeeCalculationVacationResultHead

    public class CalculateVacationResultContainer
    {
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public List<EmployeeCalculateVacationResultDTO> Results { get; set; }
    }

    public class EmployeeCalculateVacationResultHeadDTO
    {
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<CalculateVacationResultContainer> EmployeeContainer { get; set; }
    }

    #endregion

    #region EmployeeCalculateVacationResult

    public class EmployeeCalculateVacationResultDTO
    {
        public int EmployeeCalculateVacationResultId { get; set; }
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        public int EmployeeId { get; set; }
        public bool Success { get; set; }

        public int Type { get; set; }

        public string Name { get; set; }
        public decimal? Value { get; set; }
        public string FormulaPlain { get; set; }
        public string FormulaExtracted { get; set; }
        public string FormulaNames { get; set; }
        public string FormulaOrigin { get; set; }
        public string Error { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public bool isDeleted
        {
            get { return State != SoeEntityState.Active; }
        }

    }

    #endregion

    #region EmploymentVacationGroup

    public class EmployeeCalculatedCostDTO
    {
        public int EmployeeCalculatedCostId { get; set; }
        public int EmployeeId { get; set; }
        public decimal CalculatedCostPerHour { get; set; }
        public DateTime? fromDate { get; set; }

        public int? ProjectId { get; set; }

        //GUI flags
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
    }
    #endregion

    #region EmployeeChild
    [Log]
    public class EmployeeChildDTO
    {
        [LogParentalLeaveAndChild]
        public int EmployeeChildId { get; set; }
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public string Name
        {
            get
            {
                return $"{this.FirstName} {this.LastName}";
            }
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool SingleCustody { get; set; }
        [TsIgnore]
        public int NbrOfDays
        {
            get
            {
                return this.SingleCustody ? 180 : 120;
            }
        }
        public int OpeningBalanceUsedDays { get; set; }
        public int UsedDays { get; set; }
        [TsIgnore]
        public int DaysLeft
        {
            get
            {
                int count = this.NbrOfDays - OpeningBalanceUsedDays - UsedDays;
                return count < 0 ? 0 : count;
            }

        }
        public int UsedDaysPayroll;
        public string UsedDaysText
        {
            get
            {
                return "(" + this.UsedDaysPayroll + ")" + UsedDays;
            }
        }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region EmployeeChildCare

    public class EmployeeChildCareDTO
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int NbrOfDays { get; set; }
        public int OpeningBalanceUsedDays { get; set; }
        public int UsedDays { get; set; }
        public int DaysLeft
        {
            get
            {
                return this.NbrOfDays - OpeningBalanceUsedDays - UsedDays;
            }
        }
        public int UsedDaysPayroll;
        public string UsedDaysText
        {
            get
            {
                return $"({this.UsedDaysPayroll}){this.UsedDays}";
            }
        }
        public string Name { get; set; }

    }

    #endregion

    #region EmployeeCSRExportDTO

    [Log]
    [TSInclude]
    public class EmployeeCSRExportDTO
    {
        public int? EmployeeTaxId { get; set; }
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public string EmployeeNr { get; set; }
        [LogSocSec]
        public string EmployeeSocialSec { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? CsrExportDate { get; set; }
        public DateTime? CsrImportDate { get; set; }

    }

    #endregion

    #region EmployeeFactor

    public class EmployeeFactorDTO
    {
        public int EmployeeFactorId { get; set; }
        public TermGroup_EmployeeFactorType Type { get; set; }
        public int? VacationGroupId { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal Factor { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string VacationGroupName { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsCurrent { get; set; }
    }

    #endregion

    #region EmployeeGroup

    [TSInclude]
    public class EmployeeGroupDTO
    {
        public int EmployeeGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? TimeCodeId { get; set; }
        public string Name { get; set; }
        public int DeviationAxelStartHours { get; set; }
        public int DeviationAxelStopHours { get; set; }
        public string PayrollProductAccountingPrio { get; set; }
        public string InvoiceProductAccountingPrio { get; set; }
        public bool AutogenTimeblocks { get; set; }
        public bool AutogenBreakOnStamping { get; set; }
        public bool AlwaysDiscardBreakEvaluation { get; set; }
        public bool MergeScheduleBreaksOnDay { get; set; }
        public int BreakDayMinutesAfterMidnight { get; set; }
        public int KeepStampsTogetherWithinMinutes { get; set; }
        public int RuleWorkTimeWeek { get; set; }
        public int RuleWorkTimeYear { get; set; }
        public int RuleRestTimeDay { get; set; }
        public int RuleRestTimeWeek { get; set; }
        public int MaxScheduleTimeFullTime { get; set; }
        public int MinScheduleTimeFullTime { get; set; }
        public int MaxScheduleTimePartTime { get; set; }
        public int MinScheduleTimePartTime { get; set; }
        public int MaxScheduleTimeWithoutBreaks { get; set; }
        public int RuleWorkTimeDayMinimum { get; set; }
        public int RuleWorkTimeDayMaximumWorkDay { get; set; }
        public int RuleWorkTimeDayMaximumWeekend { get; set; }
        public TermGroup_QualifyingDayCalculationRule QualifyingDayCalculationRule { get; set; }
        public bool QualifyingDayCalculationRuleLimitFirstDay { get; set; }
        public bool ExtraShiftAsDefault { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public bool AlsoAttestAdditionsFromTime { get; set; }
        public int BreakRoundingUp { get; set; }
        public int BreakRoundingDown { get; set; }
        public bool RuleRestDayIncludePresence { get; set; }
        public bool RuleRestWeekIncludePresence { get; set; }
        public int RuleScheduleFreeWeekendsMinimumYear { get; set; }
        public int RuleScheduledDaysMaximumWeek { get; set; }
        public DateTime? RuleRestTimeWeekStartTime { get; set; }
        public DateTime? RuleRestTimeDayStartTime { get; set; }
        public int? RuleRestTimeWeekStartDayNumber { get; set; }
        public bool NotifyChangeOfDeviations { get; set; }
        public bool CandidateForOvertimeOnZeroDayExcluded { get; set; }
        public bool AutoGenTimeAndBreakForProject { get; set; }
        public int? ReminderAttestStateId { get; set; }
        public int? ReminderNoOfDays { get; set; }
        public int? ReminderPeriodType { get; set; }
        public bool AllowShiftsWithoutAccount { get; set; }
        public TermGroup_TimeWorkReductionCalculationRule TimeWorkReductionCalculationRule { get; set; }
        public string SwapShiftToShorterText { get; set; }
        public string SwapShiftToLongerText { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public TimeDeviationCauseDTO TimeDeviationCause { get; set; }
        [TsIgnore]
        public List<EmployeeGroupTimeDeviationCauseTimeCodeDTO> EmployeeGroupTimeDeviationCauseTimeCode { get; set; }
        public List<EmployeeGroupDayTypeDTO> EmployeeGroupDayType { get; set; }
        public List<int> DayTypeIds { get; set; }
        public string TimeDeviationCausesNames { get; set; }
        public string DayTypesNames { get; set; }
        public List<string> ExternalCodes { get; set; }
        public int TimeReportType { get; set; }
        public string TimeReportTypeName { get; set; }
        public string ExternalCodesString { get; set; }
        public List<int> TimeAccumulatorIds { get; set; }
        public List<int> TimeDeviationCauseRequestIds { get; set; }
        public List<int> TimeDeviationCauseAbsenceAnnouncementIds { get; set; }
        public List<int> TimeCodeIds { get; set; }
        public List<EmployeeGroupAttestTransitionDTO> AttestTransition { get; set; }
        public List<EmployeeGroupTimeDeviationCauseDTO> TimeDeviationCauses { get; set; }
        public int RoundInNeg { get; set; }
        public int RoundInPos { get; set; }
        public int RoundOutNeg { get; set; }
        public int RoundOutPos { get; set; }
        public List<EmployeeGroupRuleWorkTimePeriodDTO> RuleWorkTimePeriods { get; set; }
        public List<TimeAccumulatorEmployeeGroupRuleDTO> TimeAccumulatorEmployeeGroupRules { get; set; }
        public int? DefaultDim1CostAccountId { get; set; }
        public int? DefaultDim2CostAccountId { get; set; }
        public int? DefaultDim3CostAccountId { get; set; }
        public int? DefaultDim4CostAccountId { get; set; }
        public int? DefaultDim5CostAccountId { get; set; }
        public int? DefaultDim6CostAccountId { get; set; }
        public int? DefaultDim1IncomeAccountId { get; set; }
        public int? DefaultDim2IncomeAccountId { get; set; }
        public int? DefaultDim3IncomeAccountId { get; set; }
        public int? DefaultDim4IncomeAccountId { get; set; }
        public int? DefaultDim5IncomeAccountId { get; set; }
        public int? DefaultDim6IncomeAccountId { get; set; }
    }

    [TSInclude]
    public class EmployeeGroupGridDTO
    {
        public int EmployeeGroupId { get; set; }
        public string Name { get; set; }
        public string TimeDeviationCausesNames { get; set; }
        public string DayTypesNames { get; set; }
        public int TimeReportType { get; set; }
        public string TimeReportTypeName { get; set; }
        public SoeEntityState State { get; set; }

    }

    [TSInclude]
    public class EmployeeGroupSmallDTO
    {
        public int EmployeeGroupId { get; set; }
        public string Name { get; set; }
        public int RuleWorkTimeWeek { get; set; }
        public bool AutogenTimeblocks { get; set; }
    }
    #endregion

    #region EmployeeGroupTimeDeviationCauseTimeCode

    [TSInclude]
    public class EmployeeGroupTimeDeviationCauseTimeCodeDTO
    {
        public int EmployeeGroupId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int TimeCodeId { get; set; }

        // Extensions
        public TimeCodeDTO TimeCode { get; set; }
    }

    #endregion

    #region EmployeeGroupTimeDeviationCause

    [TSInclude]
    public class EmployeeGroupTimeDeviationCauseDTO
    {
        public int EmployeeGroupTimeDeviationCauseId { get; set; }
        public int EmployeeGroupId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public bool UseInTimeTerminal { get; set; }

    }
    #endregion

    #region EmployeeGroupDayType

    [TSInclude]
    public class EmployeeGroupDayTypeDTO
    {
        public int EmployeeGroupDayTypeId { get; set; }
        public int DayTypeId { get; set; }
        public int EmployeeGroupId { get; set; }
        public bool IsHolidaySalary { get; set; }
    }

    #endregion

    #region EmployeeGroupAttestTransition
    [TSInclude]
    public class EmployeeGroupAttestTransitionDTO
    {
        public int AttestTransitionId { get; set; }
        public int Entity { get; set; }
    }
    #endregion

    #region EmployeeGroupRuleWorkTimePeriod
    [TSInclude]
    public class EmployeeGroupRuleWorkTimePeriodDTO
    {
        public int EmployeeGroupRuleWorkTimePeriodId { get; set; }
        public int EmployeeGroupId { get; set; }
        public int TimePeriodId { get; set; }
        public int RuleWorkTime { get; set; }
    }
    #endregion

    #region EmployeeGroupTimeLeisureCode

    [TSInclude]
    public class EmployeeGroupTimeLeisureCodeDTO
    {
        public int EmployeeGroupTimeLeisureCodeId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public int TimeLeisureCodeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public IEnumerable<EmployeeGroupTimeLeisureCodeSettingDTO> Settings { get; set; }
    }

    [TSInclude]
    public class EmployeeGroupTimeLeisureCodeGridDTO
    {
        public int EmployeeGroupTimeLeisureCodeId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public int TimeLeisureCodeId { get; set; }
        public string TimeLeisureCodeName { get; set; }
        public string EmployeeGroupName { get; set; }
        public DateTime? DateFrom { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region EmployeeGroupTimeLeisureCodeSetting

    [TSInclude]
    public class EmployeeGroupTimeLeisureCodeSettingDTO
    {
        public int EmployeeGroupTimeLeisureCodeSettingId { get; set; }
        public TermGroup_TimeLeisureCodeSettingType Type { get; set; }
        public SettingDataType DataType { get; set; }
        public string Name { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string SettingValue { get; set; }
    }

    [TSInclude]
    public class EmployeeGroupTimeLeisureCodeSettingGridDTO
    {
        public int EmployeeGroupTimeLeisureCodeSettingId { get; set; }
        public TermGroup_TimeLeisureCodeSettingType Type { get; set; }
        public string TypeName { get; set; }
        public SettingDataType DataType { get; set; }
        public string SettingValue { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region EmployeeMeeting
    [Log]
    public class EmployeeMeetingDTO
    {
        [LogEmployeeMeetingId]
        public int EmployeeMeetingId { get; set; }
        [TsIgnore]
        public int ActorCompanyId { get; set; }
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public int FollowUpTypeId { get; set; }
        public DateTime? StartTime { get; set; }
        public bool Reminder { get; set; }
        public bool EmployeeCanEdit { get; set; }
        public string Note { get; set; }
        public string OtherParticipants { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public bool Completed { get; set; }
        public List<int> AttestRoleIds { get; set; }
        public List<int> ParticipantIds { get; set; }
        public string FollowUpTypeName { get; set; }
        public string ParticipantNames { get; set; }
        [TsIgnore]
        public string MeetingDateString { get; set; }
    }

    #endregion

    #region EmployeePosition

    public class EmployeePositionDTO
    {
        public int EmployeePositionId { get; set; }
        public int EmployeeId { get; set; }
        public int PositionId { get; set; }
        public string EmployeePositionName { get; set; }
        public bool Default { get; set; }
        public string SysPositionCode { get; set; }
        public string SysPositionName { get; set; }
        public string SysPositionDescription { get; set; }
    }

    #endregion

    #region EmployeePost

    public class EmployeePostDTO
    {
        #region Propertys

        public int EmployeePostId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public int? ScheduleCycleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int WorkTimeWeek { get; set; }
        public decimal WorkTimePercent { get; set; }
        public string DayOfWeeks { get; set; }
        public List<SmallGenericType> DayOfWeeksGenericType { get; set; }
        public List<int> OverWriteDayOfWeekIds { get; set; }
        private List<int> dayOfWeekIds { get; set; }
        public List<int> DayOfWeekIds
        {
            get
            {
                if (OverWriteDayOfWeekIds != null)
                    return OverWriteDayOfWeekIds;
                else
                    return dayOfWeekIds;
            }
            set
            {
                dayOfWeekIds = value;
            }
        }
        public string DayOfWeeksGridString { get; set; }
        public int WorkDaysWeek { get; set; }
        private int remainingWorkDaysWeek { get; set; }
        public int RemainingWorkDaysWeek
        {

            get
            {
                if (this.remainingWorkDaysWeek != 0)
                    return this.remainingWorkDaysWeek;
                else
                    return this.WorkDaysWeek;
            }
            set
            {
                this.remainingWorkDaysWeek = value;
            }
        }
        public List<DayOfWeek> FreeDays
        {
            get
            {
                return GetFreeDays();
            }
        }
        public SoeEmployeePostStatus Status { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public TermGroup_EmployeePostWeekendType EmployeePostWeekendType { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }

        // Extensions
        public string EmployeeGroupName { get; set; }
        public List<EmployeePostSkillDTO> EmployeePostSkillDTOs { get; set; }
        public ScheduleCycleDTO ScheduleCycleDTO { get; set; }
        public EmployeeGroupDTO EmployeeGroupDTO { get; set; }
        public bool IgnoreDaysOfWeekIds { get; set; }
        public bool HasMinMaxTimeSpan
        {
            get
            {
                return this.WorkTimeWeekMax != this.WorkTimeWeekMin;
            }
        }
        public string AccountName { get; set; }
        public int WorkTimePerDay
        {
            get
            {
                if (this.WorkTimeWeek != 0 && this.WorkDaysWeek != 0)
                    return (int)decimal.Divide(this.WorkTimeWeek, this.WorkDaysWeek);
                else if (this.WorkTimeWeek != 0)
                    return (int)decimal.Divide(this.WorkTimeWeek, 5);
                else
                    return 0;
            }
        }
        public int WorkTimeCycle
        {
            get
            {
                if (this.EmployeeGroupDTO != null)
                {
                    return this.WorkTimeWeek * GetNumberOfWeeks();
                }
                else
                    return 0;
            }
        }
        public int WorkTimeWeekMax
        {
            get
            {
                if (this.EmployeeGroupDTO != null)
                {
                    if (IsFullTime())
                        return this.WorkTimeWeek + this.EmployeeGroupDTO.MaxScheduleTimeFullTime;
                    else
                        return this.WorkTimeWeek + this.EmployeeGroupDTO.MaxScheduleTimePartTime;
                }
                else
                    return this.WorkDaysWeek;
            }
        }
        public int WorkTimeWeekMin
        {
            get
            {
                if (this.EmployeeGroupDTO != null)
                {
                    if (IsFullTime())
                        return this.WorkTimeWeek + this.EmployeeGroupDTO.MinScheduleTimeFullTime;
                    else
                        return this.WorkTimeWeek + this.EmployeeGroupDTO.MinScheduleTimePartTime;
                }
                else
                    return this.WorkDaysWeek;
            }
        }
        public List<ShiftTypeDTO> ValidShiftTypes { get; set; }
        public string SkillNames { get; set; }

        #endregion

        #region Ctor

        public EmployeePostDTO()
        {
            this.ValidShiftTypes = new List<ShiftTypeDTO>();
        }

        #endregion

        #region Public methods

        public List<ScheduleCycleRuleDTO> GetScheduleCycleRuleDTOs()
        {
            if (this.ScheduleCycleDTO == null || this.ScheduleCycleDTO.ScheduleCycleRuleDTOs.IsNullOrEmpty())
                return new List<ScheduleCycleRuleDTO>();
            return this.ScheduleCycleDTO.ScheduleCycleRuleDTOs;
        }

        public bool SkillMatch(ShiftTypeDTO shiftTypeDTO)
        {
            if (shiftTypeDTO == null)
                return false;

            if (shiftTypeDTO.ShiftTypeSkills.IsNullOrEmpty() && this.EmployeePostSkillDTOs.IsNullOrEmpty())
                return true;
            else if (this.EmployeePostSkillDTOs.IsNullOrEmpty())
                return false;
            else if (shiftTypeDTO.ShiftTypeSkills.IsNullOrEmpty())
                return true;

            foreach (var skill in shiftTypeDTO.ShiftTypeSkills)
            {
                var employeePostSkill = this.EmployeePostSkillDTOs.FirstOrDefault(t => t.SkillId == skill.SkillId);
                if (employeePostSkill == null || employeePostSkill.SkillLevel < skill.SkillLevel)
                    return false;
            }

            return true;
        }

        public string GetKey()
        {
            string key = string.Empty;
            if (this.EmployeePostSkillDTOs != null)
            {
                int count = this.EmployeePostSkillDTOs.Count;
                int idSum = this.EmployeePostSkillDTOs.Sum(i => i.SkillId);
                int skillSum = this.EmployeePostSkillDTOs.Sum(i => i.SkillLevel);
                key = $"{count}#{idSum}#{skillSum}";
            }
            return key;
        }

        public int GetNumberOfWeeks()
        {
            return this.ScheduleCycleDTO?.NbrOfWeeks ?? 0;
        }

        public bool WorksOnlyWeekend()
        {
            if (this.ScheduleCycleDTO?.ScheduleCycleRuleDTOs?.FirstOrDefault()?.ScheduleCycleRuleTypeDTO == null)
                return false;

            foreach (var ruleType in this.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Select(rule => rule.ScheduleCycleRuleTypeDTO))
            {
                if (ruleType?.DayOfWeekIds == null || (ruleType.DayOfWeekIds.Any(day => day != (int)DayOfWeek.Saturday && day != (int)DayOfWeek.Sunday)))
                    return false;
            }

            return true;
        }

        public bool DayOfWeekValid(DayOfWeek dayOfWeek, bool ignoreFreeDays)
        {
            if (this.DayOfWeekIds == null || !this.DayOfWeekIds.Contains((int)dayOfWeek) || ignoreFreeDays || IgnoreDaysOfWeekIds)
            {
                if (this.ScheduleCycleDTO == null)
                    return false;
                if (this.ScheduleCycleDTO.ScheduleCycleRuleDTOs.IsNullOrEmpty())
                    return true;

                foreach (var rule in this.ScheduleCycleDTO.ScheduleCycleRuleDTOs)
                {
                    if (rule.ScheduleCycleRuleTypeDTO != null && rule.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)dayOfWeek) && rule.MaxOccurrences > 0)
                        return true;
                }
            }

            return false;
        }

        public int GetWorkTimeWeek(int interval)
        {
            return CalendarUtility.AdjustAccordingToInterval(this.WorkTimeWeek, interval, alwaysReduce: true);
        }

        public int GetSortOrder()
        {
            return this.WorkTimeWeek / (this.WorkDaysWeek != 0 ? this.WorkDaysWeek : 1);
        }

        public bool IsFullTime()
        {
            if (EmployeeGroupDTO != null)
                return this.EmployeeGroupDTO.RuleWorkTimeWeek == this.WorkTimeWeek;
            else
                return true;
        }

        #endregion

        #region Help-methods

        private List<DayOfWeek> GetFreeDays()
        {
            return this.DayOfWeekIds.Select(w => (DayOfWeek)w).ToList();
        }

        #endregion
    }

    #region EmployeeSkill

    [TSInclude]
    public class EmployeePostSkillDTO
    {
        public int EmployeePostSkillId { get; set; }
        public int EmployeePostId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }
        public DateTime? DateTo { get; set; }

        // Extensions
        public double SkillLevelStars { get; set; }
        public bool SkillLevelUnreached { get; set; }
        public string SkillName { get; set; }
        public string SkillTypeName { get; set; }
        public SkillDTO SkillDTO { get; set; }
    }

    #endregion

    #endregion

    #region EmployeeRequest
    [TSInclude]
    public class EmployeeRequestDTO
    {
        public int EmployeeRequestId { set; get; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }

        public TermGroup_EmployeeRequestType Type { set; get; }
        public TermGroup_EmployeeRequestStatus Status { get; set; }
        public TermGroup_EmployeeRequestResultStatus ResultStatus { get; set; }
        public DateTime Start { set; get; }
        public DateTime Stop { set; get; }
        public string StartString { set; get; }
        public string StopString { set; get; }
        public string Comment { set; get; }
        public bool ReActivate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedString { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeName { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string StatusName { get; set; }
        public string ResultStatusName { get; set; }
        public ExtendedAbsenceSettingDTO ExtendedSettings { get; set; }
        public bool RequestIntersectsWithCurrent { get; set; }
        public string IntersectMessage { get; set; }
        public int? EmployeeChildId { get; set; }
        public string EmployeeChildName { get; set; }
        public string CategoryNamesString { get; set; }
        public string AccountNamesString { get; set; }
        public bool IsSelected { get; set; }

        public EmployeeRequestDTO()
        {

        }

        public EmployeeRequestDTO(int employeeId, int timeDeviationCauseId, int? employeeChildId = null, string comment = null, decimal? ratio = null)
        {
            this.EmployeeId = employeeId;
            this.TimeDeviationCauseId = timeDeviationCauseId;
            this.EmployeeChildId = employeeChildId.ToNullable();
            this.Comment = comment ?? string.Empty;
            this.ExtendedSettings = new ExtendedAbsenceSettingDTO
            {
                PercentalAbsence = ratio.HasValue,
                PercentalValue = ratio,
            };
        }
    }

    [TSInclude]
    public class EmployeeRequestGridDTO
    {
        public int EmployeeRequestId { set; get; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }

        public TermGroup_EmployeeRequestType Type { set; get; }
        public TermGroup_EmployeeRequestStatus Status { get; set; }
        public TermGroup_EmployeeRequestResultStatus ResultStatus { get; set; }
        public DateTime Start { set; get; }
        public DateTime Stop { set; get; }
        //public string StartString { set; get; }
        //public string StopString { set; get; }
        public string Comment { set; get; }
        //public bool ReActivate { get; set; }

        public DateTime? Created { get; set; }
        //public string CreatedString { get; set; }
        //public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        //public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeName { get; set; }
        //public string TimeDeviationCauseName { get; set; }
        //public string StatusName { get; set; }
        //public string ResultStatusName { get; set; }
        //public ExtendedAbsenceSettingDTO ExtendedSettings { get; set; }
        //public bool RequestIntersectsWithCurrent { get; set; }
        //public string IntersectMessage { get; set; }
        //public int? EmployeeChildId { get; set; }
        //public string EmployeeChildName { get; set; }
        public List<string> AccountNames { get; set; }
        public List<string> CategoryNames { get; set; }
        //public string CategoryNamesString { get; set; }
        //public string AccountNamesString { get; set; }
        //public bool IsSelected { get; set; }
    }

    #endregion

    #region EmployeeSchedule

    public class EmployeeScheduleDTO
    {
        public int EmployeeScheduleId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int EmployeeId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int StartDayNumber { get; set; }
        public bool IsPreliminary { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class ExtendedAbsenceSettingDTO
    {
        public int ExtendedAbsenceSettingId { get; set; }
        public bool AbsenceFirstAndLastDay { get; set; }
        public bool AbsenceWholeFirstDay { get; set; }
        public DateTime? AbsenceFirstDayStart { get; set; }
        public bool AbsenceWholeLastDay { get; set; }
        public DateTime? AbsenceLastDayStart { get; set; }
        public bool PercentalAbsence { get; set; }
        public decimal? PercentalValue { get; set; }
        public bool? PercentalAbsenceOccursStartOfDay { get; set; }
        public bool? PercentalAbsenceOccursEndOfDay { get; set; }
        public bool AdjustAbsencePerWeekDay { get; set; }
        public DateTime? AdjustAbsenceAllDaysStart { get; set; }
        public DateTime? AdjustAbsenceAllDaysStop { get; set; }
        public DateTime? AdjustAbsenceMonStart { get; set; }
        public DateTime? AdjustAbsenceMonStop { get; set; }
        public DateTime? AdjustAbsenceTueStart { get; set; }
        public DateTime? AdjustAbsenceTueStop { get; set; }
        public DateTime? AdjustAbsenceWedStart { get; set; }
        public DateTime? AdjustAbsenceWedStop { get; set; }
        public DateTime? AdjustAbsenceThuStart { get; set; }
        public DateTime? AdjustAbsenceThuStop { get; set; }
        public DateTime? AdjustAbsenceFriStart { get; set; }
        public DateTime? AdjustAbsenceFriStop { get; set; }
        public DateTime? AdjustAbsenceSatStart { get; set; }
        public DateTime? AdjustAbsenceSatStop { get; set; }
        public DateTime? AdjustAbsenceSunStart { get; set; }
        public DateTime? AdjustAbsenceSunStop { get; set; }
    }

    [TSInclude]
    public class ActivateScheduleControlDTO
    {
        //Up
        public List<ActivateScheduleControlHeadDTO> Heads { get; set; }
        public string Key { get; set; }
        public bool HasWarnings
        {
            get
            {
                return !this.Heads.IsNullOrEmpty();
            }
        }
        //Down
        public List<ActivateScheduleControlHeadResultDTO> ResultHeads { get; set; }
        public bool DiscardCheckesAll { get; set; }
        public bool DiscardCheckesForAbsence { get; set; }
        public bool DiscardCheckesForManuallyAdjusted { get; set; }
        public ActivateScheduleControlDTO()
        {
            this.Key = Guid.NewGuid().ToString();
        }
        public void AddHead(ActivateScheduleControlHeadDTO head)
        {
            if (head == null)
                return;

            if (this.Heads == null)
                this.Heads = new List<ActivateScheduleControlHeadDTO>();
            this.Heads.Add(head);
        }
        public void Sort()
        {
            if (!this.Heads.IsNullOrEmpty())
                this.Heads = this.Heads.OrderBy(i => i.Type).ThenBy(i => i.StartDate).ThenBy(i => i.StopDate).ThenBy(i => i.TimeDeviationCauseName).ToList();
        }
    }

    [TSInclude]
    public class ActivateScheduleControlHeadResultDTO
    {
        public TermGroup_ControlEmployeeSchedulePlacementType Type { get; set; }
        public int EmployeeId { get; set; }
        public int? EmployeeRequestId { get; set; }
        public bool ReActivateAbsenceRequest { get; set; }
    }

    [TSInclude]
    public class ActivateScheduleControlHeadDTO
    {
        public TermGroup_ControlEmployeeSchedulePlacementType Type { get; set; }
        public List<ActivateScheduleControlRowDTO> Rows { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNrAndName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int? EmployeeRequestId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string TypeName { get; set; }
        public string StatusName { get; set; }
        public string ResultStatusName { get; set; }
        public string Comment { get; set; }
        public bool ReActivateAbsenceRequest { get; set; }

        public ActivateScheduleControlHeadDTO(TermGroup_ControlEmployeeSchedulePlacementType type, List<ActivateScheduleControlRowDTO> rows, EmployeeDTO employee, DateTime startDate, DateTime stopDate, string typeName, string statusName, string resultStatusName, string comment, int? employeeRequestId = null, int? timeDeviationCauseId = null, string timeDeviationCauseName = null, bool reActivateAbsenceRequest = false)
        {
            this.Type = type;
            this.Rows = rows;
            this.EmployeeId = employee?.EmployeeId ?? 0;
            this.EmployeeNrAndName = employee?.NumberAndName;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.EmployeeRequestId = employeeRequestId;
            this.TimeDeviationCauseId = timeDeviationCauseId;
            this.TimeDeviationCauseName = timeDeviationCauseName;
            this.TypeName = typeName;
            this.StatusName = statusName;
            this.ResultStatusName = resultStatusName;
            this.Comment = comment;
            this.ReActivateAbsenceRequest = reActivateAbsenceRequest;
        }
    }

    [TSInclude]
    public class ActivateScheduleControlRowDTO
    {
        public TermGroup_ControlEmployeeSchedulePlacementType Type { get; set; }
        public DateTime Date { get; set; }
        public DateTime ScheduleStart { get; set; }
        public DateTime ScheduleStop { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public bool IsWholeDayAbsence { get; set; }
        public int? TimeScheduleTemplateBlockId { get; set; }
        public int? TimeScheduleTemplateBlockType { get; set; }
        [TsIgnore]
        public int? EmployeeRequestId { get; set; }
        [TsIgnore]
        public int? TimeDeviationCauseId { get; set; }

        public ActivateScheduleControlRowDTO(TermGroup_ControlEmployeeSchedulePlacementType type, DateTime date, DateTime scheduleStart, DateTime scheduleStop, DateTime start, DateTime stop, bool isWholeDayAbsence, int? timeScheduleTemplateBlockId, int? timeScheduleTemplateBlockType, int? employeeRequestId = null, int? timeDeviationCauseId = null)
        {
            this.Type = type;
            this.Date = date;
            this.ScheduleStart = scheduleStart;
            this.ScheduleStop = scheduleStop;
            this.Start = start;
            this.Stop = stop;
            this.IsWholeDayAbsence = isWholeDayAbsence;
            this.TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId;
            this.TimeScheduleTemplateBlockType = timeScheduleTemplateBlockType;
            this.EmployeeRequestId = employeeRequestId;
            this.TimeDeviationCauseId = timeDeviationCauseId;
        }
    }

    #endregion

    #region EmployeeSetting

    public class EmployeeSettingDTO
    {
        public int EmployeeSettingId { get; set; }
        public int EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }

        public TermGroup_EmployeeSettingType EmployeeSettingAreaType { get; set; }
        public TermGroup_EmployeeSettingType EmployeeSettingGroupType { get; set; }
        public int EmployeeSettingType { get; set; }

        public SettingDataType DataType { get; set; }
        public DateTime? ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class EmployeeSettingTypeDTO
    {
        public TermGroup_EmployeeSettingType EmployeeSettingAreaType { get; set; }
        public TermGroup_EmployeeSettingType EmployeeSettingGroupType { get; set; }
        public int EmployeeSettingType { get; set; }

        public SettingDataType DataType { get; set; }
        public string Name { get; set; }

        public List<SmallGenericType> Options { get; set; }
        public int? MaxLength { get; set; }

        public EmployeeSettingTypeDTO(TermGroup_EmployeeSettingType areaType, TermGroup_EmployeeSettingType groupType, int type, SettingDataType dataType, string name = null)
        {
            EmployeeSettingAreaType = areaType;
            EmployeeSettingGroupType = groupType;
            EmployeeSettingType = type;
            DataType = dataType;
            Name = name;
        }
    }

    #endregion

    #region EmployeeSkill

    [TSInclude]
    public class EmployeeSkillDTO
    {
        public int EmployeeSkillId { get; set; }
        public int EmployeeId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }
        public DateTime? DateTo { get; set; }

        // Extensions
        public double SkillLevelStars { get; set; }
        public bool SkillLevelUnreached { get; set; }
        public string SkillName { get; set; }
        public string SkillTypeName { get; set; }
    }

    public class SearchEmployeeSkillDTO
    {
        public int EmployeeId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }
        public double SkillLevelStars { get; set; }
        public int SkillLevelPosition { get; set; }
        public double SkillLevelPositionStars { get; set; }
        public int SkillLevelDifference { get; set; }
        public double SkillLevelDifferenceStars { get; set; }
        public string EmployeeName { get; set; }
        public string SkillName { get; set; }
        public string Positions { get; set; }
        public DateTime? EndDate { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region EmployeeTaxSE
    [Log]
    public class EmployeeTaxSEDTO
    {
        public int EmployeeTaxId { get; set; }
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public bool MainEmployer { get; set; }

        public TermGroup_EmployeeTaxType Type { get; set; }

        public int? TaxRate { get; set; }
        public int? TaxRateColumn { get; set; }
        public decimal? OneTimeTaxPercent { get; set; }
        public decimal? EstimatedAnnualSalary { get; set; }

        public TermGroup_EmployeeTaxAdjustmentType AdjustmentType { get; set; }
        public decimal? AdjustmentValue { get; set; }
        public DateTime? AdjustmentPeriodFrom { get; set; }
        public DateTime? AdjustmentPeriodTo { get; set; }
        public decimal? SchoolYouthLimitInitial { get; set; }

        public TermGroup_EmployeeTaxSinkType SinkType { get; set; }

        public TermGroup_EmployeeTaxEmploymentTaxType EmploymentTaxType { get; set; }
        public TermGroup_EmployeeTaxEmploymentAbroadCode EmploymentAbroadCode { get; set; }
        public bool RegionalSupport { get; set; }


        [LogSalaryDistress]
        public decimal? SalaryDistressAmount { get; set; }
        public TermGroup_EmployeeTaxSalaryDistressAmountType SalaryDistressAmountType { get; set; }
        [LogSalaryDistress]
        public decimal? SalaryDistressReservedAmount { get; set; }
        public string SalaryDistressCase { get; set; }

        public DateTime? CsrExportDate { get; set; }
        public DateTime? CsrImportDate { get; set; }

        public string TinNumber { get; set; }
        public string CountryCode { get; set; }
        public bool ApplyEmploymentTaxMinimumRule { get; set; }

        public bool FirstEmployee { get; set; }
        public bool SecondEmployee { get; set; }
        public string BirthPlace { get; set; }
        public string CountryCodeBirthPlace { get; set; }
        public string CountryCodeCitizen { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public string TypeName { get; set; }
    }

    #endregion

    #region EmployeeTimeWorkAccount

    public class EmployeeTimeWorkAccountDTO
    {
        public int EmployeeTimeWorkAccountId { get; set; }
        public int TimeWorkAccountId { get; set; }
        public string TimeWorkAccountName { get; set; }
        public int EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public SoeEntityState State { get; set; }
        [TsIgnore]
        public Guid Key { get; set; }
    }

    #endregion

    #region EmployeeUnionFee
    [Log]
    public class EmployeeUnionFeeDTO
    {
        [LogEmployeeUnionFee]
        public int EmployeeUnionFeeId { get; set; }
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public int UnionFeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string UnionFeeName { get; set; }
    }

    #endregion

    #region EmployeeVacationSE

    public class EmployeeVacationSEDTO
    {
        public int EmployeeVacationSEId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? AdjustmentDate { get; set; }

        public decimal? EarnedDaysPaid { get; set; }                    // Intjänade dagar, betald
        public decimal? EarnedDaysUnpaid { get; set; }                  // Intjänade dagar, obetalda dagar
        public decimal? EarnedDaysAdvance { get; set; }                 // Intjänade dagar, förskott
        public decimal? PrelPayedDaysYear1 { get; set; }                // Dagar som är betalda av semesterårsskiftet (skapat transaktioner)
        public decimal? SavedDaysYear1 { get; set; }                    // Sparade dagar år 1
        public decimal? SavedDaysYear2 { get; set; }                    // Sparade dagar år 2
        public decimal? SavedDaysYear3 { get; set; }                    // Sparade dagar år 3
        public decimal? SavedDaysYear4 { get; set; }                    // Sparade dagar år 4
        public decimal? SavedDaysYear5 { get; set; }                    // Sparade dagar år 5
        public decimal? SavedDaysOverdue { get; set; }                  // Sparade dagar, förfallna

        public decimal? UsedDaysPaid { get; set; }                      // Uttagna dagar, betald
        public decimal? PaidVacationAllowance { get; set; }         // Uttagna dagar, utbetalda semestertillägg
        public decimal? PaidVacationVariableAllowance { get; set; }         // Utbetalda rörliga semestertillägg
        public decimal? UsedDaysUnpaid { get; set; }                    // Uttagna dagar, obetalda dagar
        public decimal? UsedDaysAdvance { get; set; }                   // Uttagna dagar, förskott
        public decimal? UsedDaysYear1 { get; set; }                     // Uttagna dagar år 1
        public decimal? UsedDaysYear2 { get; set; }                     // Uttagna dagar år 2
        public decimal? UsedDaysYear3 { get; set; }                     // Uttagna dagar år 3
        public decimal? UsedDaysYear4 { get; set; }                     // Uttagna dagar år 4
        public decimal? UsedDaysYear5 { get; set; }                     // Uttagna dagar år 5
        public decimal? UsedDaysOverdue { get; set; }                   // Uttagna dagar, förfallna

        public decimal? RemainingDaysPaid { get; set; }                 // Återstående dagar, betald
        public decimal? RemainingDaysUnpaid { get; set; }               // Återstående dagar, obetalda dagar
        public decimal? RemainingDaysAdvance { get; set; }              // Återstående dagar, förskott
        public decimal? RemainingDaysYear1 { get; set; }                // Återstående dagar år 1
        public decimal? RemainingDaysYear2 { get; set; }                // Återstående dagar år 2
        public decimal? RemainingDaysYear3 { get; set; }                // Återstående dagar år 3
        public decimal? RemainingDaysYear4 { get; set; }                // Återstående dagar år 4
        public decimal? RemainingDaysYear5 { get; set; }                // Återstående dagar år 5
        public decimal? RemainingDaysOverdue { get; set; }              // Återstående dagar, förfallna

        public decimal? EarnedDaysRemainingHoursPaid { get; set; }      // Återstående timmar, betald
        public decimal? EarnedDaysRemainingHoursUnpaid { get; set; }    // Återstående timmar, obetalda dagar
        public decimal? EarnedDaysRemainingHoursAdvance { get; set; }   // Återstående timmar, förskott
        public decimal? EarnedDaysRemainingHoursYear1 { get; set; }     // Återstående timmar år 1
        public decimal? EarnedDaysRemainingHoursYear2 { get; set; }     // Återstående timmar år 2
        public decimal? EarnedDaysRemainingHoursYear3 { get; set; }     // Återstående timmar år 3
        public decimal? EarnedDaysRemainingHoursYear4 { get; set; }     // Återstående timmar år 4
        public decimal? EarnedDaysRemainingHoursYear5 { get; set; }     // Återstående timmar år 5
        public decimal? EarnedDaysRemainingHoursOverdue { get; set; }   // Återstående timmar, förfallna

        public decimal? EmploymentRatePaid { get; set; }            // Sysselsättningsgrad (intjänad)
        public decimal? EmploymentRateYear1 { get; set; }           // Sysselsättningsgrad (intjänad) år 1
        public decimal? EmploymentRateYear2 { get; set; }           // Sysselsättningsgrad (intjänad) år 2
        public decimal? EmploymentRateYear3 { get; set; }           // Sysselsättningsgrad (intjänad) år 3
        public decimal? EmploymentRateYear4 { get; set; }           // Sysselsättningsgrad (intjänad) år 4
        public decimal? EmploymentRateYear5 { get; set; }           // Sysselsättningsgrad (intjänad) år 5
        public decimal? EmploymentRateOverdue { get; set; }         // Sysselsättningsgrad (intjänad), förfallna

        public decimal? RemainingDaysAllowanceYear1 { get; set; }  // Återstående tillägg
        public decimal? RemainingDaysAllowanceYear2 { get; set; }
        public decimal? RemainingDaysAllowanceYear3 { get; set; }
        public decimal? RemainingDaysAllowanceYear4 { get; set; }
        public decimal? RemainingDaysAllowanceYear5 { get; set; }
        public decimal? RemainingDaysAllowanceYearOverdue { get; set; }
        public decimal? RemainingDaysVariableAllowanceYear1 { get; set; } // Återstående rörliga tillägg
        public decimal? RemainingDaysVariableAllowanceYear2 { get; set; }
        public decimal? RemainingDaysVariableAllowanceYear3 { get; set; }
        public decimal? RemainingDaysVariableAllowanceYear4 { get; set; }
        public decimal? RemainingDaysVariableAllowanceYear5 { get; set; }
        public decimal? RemainingDaysVariableAllowanceYearOverdue { get; set; }

        public decimal? DebtInAdvanceAmount { get; set; }           // Skuld förskott (belopp)
        public DateTime? DebtInAdvanceDueDate { get; set; }         // Skuld förskott förfaller
        public bool DebtInAdvanceDelete { get; set; }               // Skuld förskott, ta bort

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public decimal TotalRemainingDays
        {
            get { return (RemainingDaysPaid ?? 0) + (RemainingDaysUnpaid ?? 0) + (RemainingDaysAdvance ?? 0) + (RemainingDaysYear1 ?? 0) + (RemainingDaysYear2 ?? 0) + (RemainingDaysYear3 ?? 0) + (RemainingDaysYear4 ?? 0) + (RemainingDaysYear5 ?? 0) + (RemainingDaysOverdue ?? 0); }
            set { }//NOSONAR
        }

        public decimal TotalRemainingHours
        {
            get { return (EarnedDaysRemainingHoursPaid ?? 0) + (EarnedDaysRemainingHoursUnpaid ?? 0) + (EarnedDaysRemainingHoursAdvance ?? 0) + (EarnedDaysRemainingHoursYear1 ?? 0) + (EarnedDaysRemainingHoursYear2 ?? 0) + (EarnedDaysRemainingHoursYear3 ?? 0) + (EarnedDaysRemainingHoursYear4 ?? 0) + (EarnedDaysRemainingHoursYear5 ?? 0) + (EarnedDaysRemainingHoursOverdue ?? 0); }
            set { }//NOSONAR
        }

    }

    public class EmployeeVacationSE2DTO
    {
        public int VacationGroupId { get; set; }                    // Semesteravtal
        public int VacationRight { get; set; }                      // Semesterrätt (dagar)
        public decimal VacationCoefficient { get; set; }            // Semesterkoefficient
        public int AverageWorkTimeWeek { get; set; }                // Snittarbetstid per vecka (minuter)
        public int AverageWorkTimeShift { get; set; }               // Snittarbetstid per arbetspass (minuter)
        public decimal VacationSalaryDay { get; set; }              // Semesterlön per dag (belopp)
        public decimal VacationSalaryHour { get; set; }             // Semesterlön per timme (belopp)
        public decimal VacationAllowancesVariable { get; set; }     // Semetertillägg rörligt (belopp)
    }

    public class EmployeeVacationSEIODTO
    {
        public string EmployeeNr { get; set; }

        public decimal TotalDaysVacation { get; set; }
        public string VacationGroupCode { get; set; }
        public decimal EarnedDaysPaid { get; set; }                    // Intjänade dagar, betald
        public decimal EarnedDaysUnpaid { get; set; }                  // Intjänade dagar, obetalda dagar
        public decimal EarnedDaysAdvance { get; set; }                 // Intjänade dagar, förskott
        public decimal SavedDaysYear1 { get; set; }                    // Sparade dagar år 1
        public decimal SavedDaysYear2 { get; set; }                    // Sparade dagar år 2
        public decimal SavedDaysYear3 { get; set; }                    // Sparade dagar år 3
        public decimal SavedDaysYear4 { get; set; }                    // Sparade dagar år 4
        public decimal SavedDaysYear5 { get; set; }                    // Sparade dagar år 5
        public decimal SavedDaysOverdue { get; set; }                  // Sparade dagar, förfallna

        public decimal UsedDaysPaid { get; set; }                      // Uttagna dagar, betald
        public decimal PaidVacationAllowance { get; set; }             // Uttagna dagar, utbetalda semestertillägg

        public decimal PaidVacationVariableAllowance { get; set; }     // Rörliga semestertillägg
        public decimal UsedDaysUnpaid { get; set; }                    // Uttagna dagar, obetalda dagar
        public decimal UsedDaysAdvance { get; set; }                   // Uttagna dagar, förskott
        public decimal UsedDaysYear1 { get; set; }                     // Uttagna dagar år 1
        public decimal UsedDaysYear2 { get; set; }                     // Uttagna dagar år 2
        public decimal UsedDaysYear3 { get; set; }                     // Uttagna dagar år 3
        public decimal UsedDaysYear4 { get; set; }                     // Uttagna dagar år 4
        public decimal UsedDaysYear5 { get; set; }                     // Uttagna dagar år 5
        public decimal UsedDaysOverdue { get; set; }                   // Uttagna dagar, förfallna

        public decimal RemainingDaysPaid { get; set; }                 // Återstående dagar, betald
        public decimal RemainingDaysUnpaid { get; set; }               // Återstående dagar, obetalda dagar
        public decimal RemainingDaysAdvance { get; set; }              // Återstående dagar, förskott
        public decimal RemainingDaysYear1 { get; set; }                // Återstående dagar år 1
        public decimal RemainingDaysYear2 { get; set; }                // Återstående dagar år 2
        public decimal RemainingDaysYear3 { get; set; }                // Återstående dagar år 3
        public decimal RemainingDaysYear4 { get; set; }                // Återstående dagar år 4
        public decimal RemainingDaysYear5 { get; set; }                // Återstående dagar år 5
        public decimal RemainingDaysOverdue { get; set; }              // Återstående dagar, förfallna

        public decimal EarnedDaysRemainingHoursPaid { get; set; }      // Återstående timmar, betald
        public decimal EarnedDaysRemainingHoursUnpaid { get; set; }    // Återstående timmar, obetalda dagar
        public decimal EarnedDaysRemainingHoursAdvance { get; set; }   // Återstående timmar, förskott
        public decimal EarnedDaysRemainingHoursYear1 { get; set; }     // Återstående timmar år 1
        public decimal EarnedDaysRemainingHoursYear2 { get; set; }     // Återstående timmar år 2
        public decimal EarnedDaysRemainingHoursYear3 { get; set; }     // Återstående timmar år 3
        public decimal EarnedDaysRemainingHoursYear4 { get; set; }     // Återstående timmar år 4
        public decimal EarnedDaysRemainingHoursYear5 { get; set; }     // Återstående timmar år 5
        public decimal EarnedDaysRemainingHoursOverdue { get; set; }   // Återstående timmar, förfallna

        public decimal EmploymentRatePaid { get; set; }            // Sysselsättningsgrad (intjänad)
        public decimal EmploymentRateYear1 { get; set; }           // Sysselsättningsgrad (intjänad) år 1
        public decimal EmploymentRateYear2 { get; set; }           // Sysselsättningsgrad (intjänad) år 2
        public decimal EmploymentRateYear3 { get; set; }           // Sysselsättningsgrad (intjänad) år 3
        public decimal EmploymentRateYear4 { get; set; }           // Sysselsättningsgrad (intjänad) år 4
        public decimal EmploymentRateYear5 { get; set; }           // Sysselsättningsgrad (intjänad) år 5
        public decimal EmploymentRateOverdue { get; set; }         // Sysselsättningsgrad (intjänad), förfallna

        public decimal SavedDaysAmountYear1 { get; set; }                    // Sparade Semesterdaglön år 1
        public decimal SavedDaysAmountYear2 { get; set; }                    // Sparade Semesterdaglön år 2
        public decimal SavedDaysAmountYear3 { get; set; }                    // Sparade Semesterdaglön år 3
        public decimal SavedDaysAmountYear4 { get; set; }                    // Sparade Semesterdaglön år 4
        public decimal SavedDaysAmountYear5 { get; set; }                    // Sparade Semesterdaglön år 5ac

        public decimal DebtInAdvanceAmount { get; set; }           // Skuld förskott (belopp)
        public DateTime DebtInAdvanceDueDate { get; set; }         // Skuld förskott förfaller
        public bool DebtInAdvanceDelete { get; set; }               // Skuld förskott, ta bort
    }
    #endregion

    #region EmployeeVehicle
    [Log]
    public class EmployeeVehicleDTO
    {
        [LogVehiclenformation]
        public int EmployeeVehicleId { get; set; }
        public int ActorCompanyId { get; set; }
        [LogEmployeeIdAttribute]
        public int EmployeeId { get; set; }

        public int Year { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int? SysVehicleTypeId { get; set; }
        public TermGroup_VehicleType Type { get; set; }

        public string LicensePlateNumber { get; set; }
        public string ModelCode { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public decimal Price { get; set; }

        public TermGroup_SysVehicleFuelType FuelType { get; set; }
        public string CodeForComparableModel { get; set; }
        public decimal ComparablePrice { get; set; }
        public decimal PriceAdjustment { get; set; }

        public bool HasExtensiveDriving { get; set; }
        public decimal TaxableValue { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public decimal BenefitValueAdjustment { get; set; }

        // Relations
        public List<EmployeeVehicleDeductionDTO> Deduction { get; set; }
        public List<EmployeeVehicleEquipmentDTO> Equipment { get; set; }
        public List<EmployeeVehicleTaxDTO> Tax { get; set; }
    }

    public class EmployeeVehicleDeductionDTO
    {
        public int EmployeeVehicleDeductionId { get; set; }
        public int EmployeeVehicleId { get; set; }

        public DateTime? FromDate { get; set; }
        public decimal Price { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class EmployeeVehicleEquipmentDTO
    {
        public int EmployeeVehicleEquipmentId { get; set; }
        public int EmployeeVehicleId { get; set; }

        public string Description { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Price { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class EmployeeVehicleTaxDTO
    {
        public int EmployeeVehicleTaxId { get; set; }
        public int EmployeeVehicleId { get; set; }

        public DateTime? FromDate { get; set; }
        public decimal Amount { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [Log]
    public class EmployeeVehicleGridDTO
    {
        [LogVehiclenformation]
        public int EmployeeVehicleId { get; set; }

        [LogEmployeeId]
        public int EmployeeId { get; set; }

        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }

        public string LicensePlateNumber { get; set; }
        public string VehicleMakeAndModel { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Price { get; set; }
        public decimal EquipmentSum { get; set; }
        public decimal NetSalaryDeduction { get; set; }
        public decimal TaxableValue { get; set; }
    }

    public class EmployeeVehiclePayrollCalculationDTO
    {
        public int EmployeeVehicleId { get; set; }
        public decimal NetSalaryDeduction { get; set; }
        public decimal TaxableValue { get; set; }
    }

    #endregion

    #region Employment

    [LogAttribute]
    public class EmploymentDTO : IEmployment
    {
        #region Propertys

        //General
        public string UniqueId { get; set; }

        public int EmploymentId { get; set; }
        [LogEmployeeIdAttribute]
        public int EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmploymentType { get; set; }
        public string EmploymentTypeName { get; set; }
        public string Name { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int WorkTimeWeek { get; set; }
        public int FullTimeWorkTimeWeek { get; set; }
        public bool? ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment { get; set; }
        public decimal Percent { get; set; }
        public int ExperienceMonths { get; set; }
        public bool UpdateExperienceMonthsReminder { get; set; }
        public bool ExperienceAgreedOrEstablished { get; set; }
        public string WorkPlace { get; set; }
        public string SpecialConditions { get; set; }
        public string WorkTasks { get; set; }
        public string Comment { get; set; }
        public SoeEntityState State { get; set; }
        [TsIgnore]
        public int StateId
        {
            get
            {
                return (int)this.State;
            }
        }
        public int EmploymentEndReason { get; set; }
        public string EmploymentEndReasonName { get; set; }
        public int BaseWorkTimeWeek { get; set; }
        public string SubstituteFor { get; set; }
        public string SubstituteForDueTo { get; set; }
        public SoeEmploymentFinalSalaryStatus FinalSalaryStatus { get; set; }
        [TsIgnore]
        public int FinalSalaryStatusId
        {
            get
            {
                return (int)this.FinalSalaryStatus;
            }
        }
        public bool IsSecondaryEmployment { get; set; }
        public bool IsTemporaryPrimary { get; set; }
        public int? HibernatingTimeDeviationCauseId { get; set; }
        public string ExternalCode { get; set; }
        public List<EmploymentPriceTypeDTO> PriceTypes { get; set; }
        public List<EmploymentVacationGroupDTO> EmploymentVacationGroup { get; set; }
        [TsIgnore]
        public List<DateRangeDTO> HibernatingPeriods { get; set; }

        //EmploymentChange
        public DateTime? CurrentApplyChangeDate { get; set; }
        public DateTime? CurrentChangeDateFrom { get; set; }
        public DateTime? CurrentChangeDateTo { get; set; }
        public List<EmploymentChangeDTO> Changes { get; set; }
        public List<EmploymentChangeDTO> CurrentChanges { get; set; }
        [TsIgnore]
        public Dictionary<TermGroup_EmploymentChangeFieldType, string> OriginalValues { get; set; }

        //EmployeeGroup
        public int EmployeeGroupId { get; set; }
        public string EmployeeGroupName { get; set; }
        public int EmployeeGroupWorkTimeWeek { get; set; }
        public int? PayrollGroupId { get; set; }
        public string PayrollGroupName { get; set; }
        public int? AnnualLeaveGroupId { get; set; }
        public string AnnualLeaveGroupName { get; set; }
        public string EmployeeName { get; set; }
        public int CalculatedExperienceMonths { get; set; }
        public List<int> EmployeeGroupTimeCodes { get; set; }

        //Accounting
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }
        public bool FixedAccounting { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> CostAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> IncomeAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed1Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed2Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed3Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed4Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed5Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed6Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed7Accounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> Fixed8Accounts { get; set; }
        [TsIgnore]
        public List<AccountingSettingDTO> AccountSettings { get; set; }

        //Current flags
        public bool IsChangingEmployment { get; set; }
        public bool IsChangingEmploymentDates { get; set; }
        public bool IsChangingToNotTemporary { get; set; }
        public bool IsAddingEmployment { get; set; }
        public bool IsDeletingEmployment { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsNewFromCopy { get; set; }
        [TsIgnore]
        public bool IsEditing
        {
            get
            {
                return this.IsChangingEmployment || this.IsChangingEmploymentDates || this.IsChangingToNotTemporary || this.IsAddingEmployment || this.IsDeletingEmployment;
            }
        }

        #endregion

        #region Ctor

        public EmploymentDTO()
        {
            //General
            this.UniqueId = Guid.NewGuid().ToString();
            this.Changes = new List<EmploymentChangeDTO>();
            this.CurrentChanges = new List<EmploymentChangeDTO>();
            this.OriginalValues = new Dictionary<TermGroup_EmploymentChangeFieldType, string>();
        }

        #endregion

        #region Public methods

        public bool UpdateEmployeeGroup(int toValue, List<EmployeeGroupDTO> employeeGroups)
        {
            return UpdateEmployeeGroup(toValue, employeeGroups.ToDictionary(k => k.EmployeeGroupId, n => n.Name));
        }

        public bool UpdateEmployeeGroup(int toValue, Dictionary<int, string> employeeGroupsDict)
        {
            if (this.EmployeeGroupId == toValue && !String.IsNullOrEmpty(this.EmployeeGroupName))
                return false;

            //Add EmploymentChange
            var from = this.EmployeeGroupId;
            var to = toValue;
            var fromValueName = StringUtility.GetDictStringValue(employeeGroupsDict, from);
            var toValueName = StringUtility.GetDictStringValue(employeeGroupsDict, to);
            AddChange(TermGroup_EmploymentChangeFieldType.EmployeeGroupId, from, to, fromValueName, toValueName);

            //Update Employment
            this.EmployeeGroupId = toValue;
            this.EmployeeGroupName = toValueName;

            return true;
        }

        public bool UpdatePayrollGroup(int? toValue, List<PayrollGroupDTO> payrollGroups)
        {
            return UpdatePayrollGroup(toValue, payrollGroups.ToDictionary(k => k.PayrollGroupId, n => n.Name));
        }

        public bool UpdatePayrollGroup(int? toValue, Dictionary<int, string> payrollGroupsDict)
        {
            toValue = toValue.ToNullable();
            if (this.PayrollGroupId == toValue && !String.IsNullOrEmpty(this.PayrollGroupName))
                return false;
            if (!this.PayrollGroupId.HasValue && !toValue.HasValue)
                return false;

            //Add EmploymentChange
            var from = this.PayrollGroupId.FromNullable();
            var to = toValue.FromNullable();
            var fromValueName = StringUtility.GetDictStringValue(payrollGroupsDict, from);
            var toValueName = StringUtility.GetDictStringValue(payrollGroupsDict, to);
            AddChange(TermGroup_EmploymentChangeFieldType.PayrollGroupId, from, to, fromValueName, toValueName);

            //Update Employment
            this.PayrollGroupId = toValue;
            this.PayrollGroupName = toValueName;

            return true;
        }

        public bool UpdateAnnualLeaveGroup(int? toValue, List<AnnualLeaveGroupDTO> annualLeaveGroups)
        {
            return UpdateAnnualLeaveGroup(toValue, annualLeaveGroups.ToDictionary(k => k.AnnualLeaveGroupId, n => n.Name));
        }

        public bool UpdateAnnualLeaveGroup(int? toValue, Dictionary<int, string> annualLeaveGroupsDict)
        {
            toValue = toValue.ToNullable();
            if (this.AnnualLeaveGroupId == toValue && !String.IsNullOrEmpty(this.AnnualLeaveGroupName))
                return false;
            if (!this.AnnualLeaveGroupId.HasValue && !toValue.HasValue)
                return false;

            //Add EmploymentChange
            var from = this.AnnualLeaveGroupId.FromNullable();
            var to = toValue.FromNullable();
            var fromValueName = StringUtility.GetDictStringValue(annualLeaveGroupsDict, from);
            var toValueName = StringUtility.GetDictStringValue(annualLeaveGroupsDict, to);
            AddChange(TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId, from, to, fromValueName, toValueName);

            //Update Employment
            this.AnnualLeaveGroupId = toValue;
            this.AnnualLeaveGroupName = toValueName;

            return true;
        }

        public bool UpdateState(SoeEntityState toValue, string activeValueName, string deletedValueName)
        {
            if (this.State == toValue)
                return false;

            if (toValue == SoeEntityState.Deleted)
            {
                //Clear all changes made earlier
                this.CurrentChanges.Clear();
            }

            //Add EmploymentChange
            var from = (int)this.State;
            var to = (int)toValue;

            string fromValueName = (from == (int)SoeEntityState.Active) ? activeValueName : deletedValueName;
            string toValueName = (to == (int)SoeEntityState.Active) ? activeValueName : deletedValueName;

            AddChange(TermGroup_EmploymentChangeFieldType.State, from, to, fromValueName, toValueName);

            //Update Employment
            this.State = toValue;

            return true;
        }

        public bool UpdateDateFrom(DateTime? toValue)
        {
            if (this.DateFrom == toValue)
                return false;

            //Add EmploymentChange
            var from = this.DateFrom;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.DateFrom, from, to);

            //Update Employment
            this.DateFrom = toValue;
            this.CurrentChangeDateFrom = toValue;

            return true;
        }

        public bool UpdateDateTo(DateTime? toValue)
        {
            if (this.DateTo == toValue)
                return false;

            //Add EmploymentChange
            var from = this.DateTo;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.DateTo, from, to);

            //Update Employment
            this.DateTo = toValue;
            this.CurrentChangeDateTo = toValue;

            return true;
        }

        public bool UpdateBaseWorkTimeWeek(int toValue)
        {
            if (this.BaseWorkTimeWeek == toValue)
                return false;

            //Add EmploymentChange
            var from = this.BaseWorkTimeWeek;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek, from, to);

            //Update Employment
            this.BaseWorkTimeWeek = toValue;

            return true;
        }

        public bool UpdateFullTimeWorkTimeWeek(int toValue)
        {
            if (this.FullTimeWorkTimeWeek == toValue)
                return false;

            //Add EmploymentChange
            var from = this.FullTimeWorkTimeWeek;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek, from, to);

            //Update Employment
            this.FullTimeWorkTimeWeek = toValue;

            return true;
        }

        public bool UpdateExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(bool? toValue)
        {
            if (this.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment == toValue)
                return false;

            //Add EmploymentChange
            var from = this.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment, from, to);

            //Update Employment
            this.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = toValue;

            return true;
        }

        public bool UpdateEmploymentEndReason(int toValue, Dictionary<int, string> employmentEndReasonDict)
        {
            if (this.EmploymentEndReason == toValue)
                return false;

            //Add EmploymentChange
            var from = this.EmploymentEndReason;
            var to = toValue;
            var fromValueName = StringUtility.GetDictStringValue(employmentEndReasonDict, from);
            var toValueName = StringUtility.GetDictStringValue(employmentEndReasonDict, to);
            AddChange(TermGroup_EmploymentChangeFieldType.EmploymentEndReason, from, to, fromValueName, toValueName);

            //Update Employment
            this.EmploymentEndReason = toValue;
            this.EmploymentEndReasonName = toValueName;

            return true;
        }

        public bool UpdateEmploymentType(int toValue, List<EmploymentTypeDTO> employmentTypes)
        {
            if (this.EmploymentType == toValue)
                return false;

            //Add EmploymentChange            
            var from = this.EmploymentType;
            var to = toValue;
            var fromValueName = employmentTypes.GetName(from);
            var toValueName = employmentTypes.GetName(to);
            AddChange(TermGroup_EmploymentChangeFieldType.EmploymentType, from, to, fromValueName, toValueName);

            //Update Employment
            this.EmploymentType = toValue;
            this.EmploymentTypeName = toValueName;

            return true;
        }

        public bool UpdateExperienceMonths(int toValue)
        {
            if (this.ExperienceMonths == toValue)
                return false;

            //Add EmploymentChange
            var from = this.ExperienceMonths;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.ExperienceMonths, from, to);

            //Update Employment
            this.ExperienceMonths = toValue;

            return true;
        }

        public bool UpdateExperienceAgreedOrEstablished(bool toValue, string trueValueName, string falseValueName)
        {
            if (this.ExperienceAgreedOrEstablished == toValue)
                return false;

            //Add EmploymentChange
            var from = this.ExperienceAgreedOrEstablished;
            var to = toValue;

            string fromValueName = from ? trueValueName : falseValueName;
            string toValueName = to ? trueValueName : falseValueName;
            AddChange(TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished, from, to, fromValueName, toValueName);

            //Update Employment
            this.ExperienceAgreedOrEstablished = toValue;

            return true;
        }

        public bool UpdateIsSecondaryEmployment(bool toValue)
        {
            if (this.IsSecondaryEmployment == toValue)
                return false;

            //Add EmploymentChange
            var from = this.IsSecondaryEmployment;
            var to = toValue;


            AddChange(TermGroup_EmploymentChangeFieldType.IsSecondaryEmployment, from, to);

            //Update Employment
            this.IsSecondaryEmployment = toValue;

            return true;
        }

        public bool UpdateExternalCode(string toValue)
        {
            if (StringUtility.NullToEmpty(this.ExternalCode) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.ExternalCode;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.ExternalCode, from, to);

            //Update Employment
            this.ExternalCode = toValue;

            return true;
        }

        public bool UpdateName(string toValue)
        {
            if (StringUtility.NullToEmpty(this.Name) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.Name;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.Name, from, to);

            //Update Employment
            this.Name = toValue;

            return true;
        }

        public bool UpdateSpecialConditions(string toValue)
        {
            if (StringUtility.NullToEmpty(this.SpecialConditions) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.SpecialConditions;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.SpecialConditions, from, to);

            //Update Employment
            this.SpecialConditions = toValue;

            return true;
        }

        public bool UpdateSubstituteFor(string toValue)
        {
            if (StringUtility.NullToEmpty(this.SubstituteFor) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.SubstituteFor;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.SubstituteFor, from, to);

            //Update Employment
            this.SubstituteFor = toValue;

            return true;
        }

        public bool UpdateSubstituteForDueTo(string toValue)
        {
            if (StringUtility.NullToEmpty(this.SubstituteForDueTo) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.SubstituteForDueTo;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.SubstituteForDueTo, from, to);

            //Update Employment
            this.SubstituteForDueTo = toValue;

            return true;
        }

        public bool UpdateWorkTasks(string toValue)
        {
            if (StringUtility.NullToEmpty(this.WorkTasks) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.WorkTasks;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.WorkTasks, from, to);

            //Update Employment
            this.WorkTasks = toValue;

            return true;
        }

        public bool UpdatePercent(decimal toValue)
        {
            toValue = Math.Round(toValue, 2);
            if (Math.Round(this.Percent, 2) == toValue)
                return false;

            //Add EmploymentChange
            var from = this.Percent;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.Percent, from, to);

            //Update Employment
            this.Percent = toValue;

            return true;
        }

        public bool UpdateWorkTimeWeek(int toValue, EmployeeGroupDTO employeeGroup, bool updatePercent = false, decimal? forceFromPercentIfUnchanged = null)
        {
            if (this.WorkTimeWeek == toValue && forceFromPercentIfUnchanged.HasValue)
                toValue = GetWorkTimeWeekFromPercent(employeeGroup.RuleWorkTimeWeek, forceFromPercentIfUnchanged.Value);
            if (this.WorkTimeWeek == toValue)
                return false;

            //Add EmploymentChange
            var from = this.WorkTimeWeek;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, from, to);

            //Update Employment
            this.WorkTimeWeek = toValue;

            //Related updates
            if (updatePercent)
                UpdatePercent(employeeGroup != null && employeeGroup.RuleWorkTimeWeek > 0 ? Decimal.Divide(this.WorkTimeWeek, employeeGroup.RuleWorkTimeWeek) * 100 : 0);

            return true;
        }

        public int GetWorkTimeWeekFromPercent(int fulltimeWorkTimeWeekMinutes, decimal percent)
        {
            return fulltimeWorkTimeWeekMinutes == 0 ? 0 : Convert.ToInt32(decimal.Multiply(NumberUtility.DividePercentIfAboveOne(percent), Convert.ToDecimal(fulltimeWorkTimeWeekMinutes)));
        }

        public bool UpdateWorkPlace(string toValue)
        {
            if (StringUtility.NullToEmpty(this.WorkPlace) == StringUtility.NullToEmpty(toValue))
                return false;

            //Add EmploymentChange
            var from = this.WorkPlace;
            var to = toValue;
            AddChange(TermGroup_EmploymentChangeFieldType.WorkPlace, from, to);

            //Update Employment
            this.WorkPlace = toValue;

            return true;
        }

        public void AddCurrentChange(TermGroup_EmploymentChangeFieldType fieldType, DateTime? fromDate, DateTime? toDate, string fromValue, string toValue, string fromValueName = "", string toValueName = "")
        {
            if (this.CurrentChanges == null)
                this.CurrentChanges = new List<EmploymentChangeDTO>();

            this.CurrentChanges.Add(new EmploymentChangeDTO()
            {
                Type = GetEmploymentChangeType(fieldType),
                FieldType = fieldType,
                Created = DateTime.Now,

                FromDate = fromDate?.Date,
                ToDate = toDate?.Date,
                FromValue = fromValue,
                ToValue = toValue,
                FromValueName = fromValueName,
                ToValueName = toValueName,
            });
        }

        public void AddChange(TermGroup_EmploymentChangeFieldType fieldType, int fromValue, int toValue, string fromValueName = "", string toValueName = "")
        {
            AddChange(fieldType, fromValue.ToString(), toValue.ToString(), fromValueName, toValueName);
        }

        public void AddChange(TermGroup_EmploymentChangeFieldType fieldType, decimal fromValue, decimal toValue, string fromValueName = "", string toValueName = "")
        {
            AddChange(fieldType, fromValue.ToString(), toValue.ToString(), fromValueName, toValueName);
        }

        public void AddChange(TermGroup_EmploymentChangeFieldType fieldType, bool fromValue, bool toValue, string fromValueName = "", string toValueName = "")
        {
            AddChange(fieldType, fromValue.ToString(), toValue.ToString(), fromValueName, toValueName);
        }

        public void AddChange(TermGroup_EmploymentChangeFieldType fieldType, bool? fromValue, bool? toValue, string fromValueName = "", string toValueName = "")
        {
            AddChange(fieldType, fromValue.ToString(), toValue.ToString(), fromValueName, toValueName);
        }

        public void AddChange(TermGroup_EmploymentChangeFieldType fieldType, DateTime? fromValue, DateTime? toValue, string fromValueName = "", string toValueName = "")
        {
            AddChange(fieldType, CalendarUtility.ToShortDateString(fromValue), CalendarUtility.ToShortDateString(toValue), fromValueName, toValueName);
        }

        public void AddChange(TermGroup_EmploymentChangeFieldType fieldType, string fromValue, string toValue, string fromValueName = "", string toValueName = "")
        {
            if (!this.IsEditing)
                return;
            if (!this.CurrentChangeDateFrom.HasValue && EmploymentChangeDTO.DoFieldRequireDate(fieldType))
                return;

            var change = this.CurrentChanges.FirstOrDefault(i => i.FieldType == fieldType);
            if (change == null)
            {
                #region Add

                DateTime? changeDateFrom = this.CurrentChangeDateFrom;
                DateTime? changeDateTo = this.CurrentChangeDateTo;

                if (fieldType == TermGroup_EmploymentChangeFieldType.EmploymentEndReason)
                {
                    changeDateFrom = null;
                    changeDateTo = null;
                }

                this.CurrentChanges.Add(new EmploymentChangeDTO()
                {
                    Type = GetEmploymentChangeType(fieldType),
                    FieldType = fieldType,
                    FromValue = fromValue,
                    ToValue = toValue,
                    FromValueName = fromValueName,
                    ToValueName = toValueName,
                    FromDate = changeDateFrom,
                    ToDate = changeDateTo,
                    Comment = this.Comment,
                });

                #endregion
            }
            else
            {
                #region Update/Delete

                if (change.FromValue == toValue)
                {
                    //Check if changed back to original value. Delete it.
                    this.CurrentChanges.Remove(change);
                }
                else
                {
                    //Update
                    change.ToValue = toValue;
                    change.ToValueName = toValueName;
                }

                #endregion
            }
        }

        public EmploymentChangeDTO GetCurrentChange(TermGroup_EmploymentChangeFieldType fieldType, DateTime changesForDate)
        {
            TermGroup_EmploymentChangeType type = GetEmploymentChangeType(fieldType);
            List<EmploymentChangeDTO> changes = this.CurrentChanges.FilterEmploymentChanges(changesForDate, type, fieldType);
            return changes.OrderBy(i => i.FromDate).LastOrDefault();
        }

        public int GetCurrentChangeValue(TermGroup_EmploymentChangeFieldType fieldType, DateTime changesForDate, int defaultValue)
        {
            int value = 0;

            EmploymentChangeDTO change = GetCurrentChange(fieldType, changesForDate);
            if (change != null)
                int.TryParse(change.ToValue, out value);

            return value > 0 ? value : defaultValue;
        }

        public string GetEmploymentTypeName(List<EmploymentTypeDTO> employmentTypes)
        {
            if (employmentTypes.IsNullOrEmpty())
                return string.Empty;
            return employmentTypes.GetName(this.EmploymentType);
        }

        public string GetOriginalValue(TermGroup_EmploymentChangeFieldType fieldType)
        {
            if (this.OriginalValues == null || !this.OriginalValues.ContainsKey(fieldType))
                return string.Empty;
            return this.OriginalValues[fieldType];
        }

        public void RefreshRelations(List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<EmploymentTypeDTO> employmentTypes)
        {
            UpdateEmployeeGroup(this.EmployeeGroupId, employeeGroups);
            UpdatePayrollGroup(this.PayrollGroupId, payrollGroups);
            UpdateEmploymentType(this.EmploymentType, employmentTypes);
        }

        #endregion

        #region Help-methods

        private TermGroup_EmploymentChangeType GetEmploymentChangeType(TermGroup_EmploymentChangeFieldType fieldType)
        {
            TermGroup_EmploymentChangeType type = TermGroup_EmploymentChangeType.Unknown;

            int fieldTypeValue = (int)fieldType;
            if (fieldTypeValue <= 100) //Information
                type = TermGroup_EmploymentChangeType.Information;
            else if (fieldTypeValue <= 200) //Relation changes
                type = TermGroup_EmploymentChangeType.DataChange;
            else if (fieldTypeValue <= 300) //Field changes
                type = TermGroup_EmploymentChangeType.DataChange;

            return type;
        }

        #endregion
    }

    public class EmploymentIODTO
    {

        public EmploymentIODTO()
        {
            IsSecondaryEmployment = false;
        }
        public string EmployeeNr { get; set; }
        public int EmploymentType { get; set; }
        public string EmploymentTypeName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int WorkTimeWeek { get; set; }
        public decimal Percent { get; set; }
        public int ExperienceMonths { get; set; }
        public bool ExperienceAgreedOrEstablished { get; set; }
        public string WorkPlace { get; set; }
        public string SpecialConditions { get; set; }
        public string Comment { get; set; }
        public int EmploymentEndReason { get; set; }
        public string EmploymentEndReasonName { get; set; }
        public int BaseWorkTimeWeek { get; set; }
        public string SubstituteFor { get; set; }
        public string EmployeeGroupName { get; set; }
        public string PayrollGroupName { get; set; }
        public string VacationGroupName { get; set; }
        public string WorkTasks { get; set; }
        public string ExternalCode { get; set; }
        public bool IsSecondaryEmployment { get; set; }
        public string SubstituteForDueTo { get; set; }
        public bool UpdateExperienceMonthsReminder { get; set; }
    }

    #endregion

    #region EmployeeChange

    public class EmployeeChangeDTO
    {
        public int FieldType { get; set; }
        public string Description { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public string FromValueName { get; set; }
        public string ToValueName { get; set; }
        public string Error { get; set; }
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this.Error);
            }
        }
    }

    #endregion

    #region EmploymentChange

    public class EmploymentChangeDTO
    {
        public int EmploymentChangeId { get; set; }
        public int EmploymentId { get; set; }
        public int EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_EmploymentChangeType Type { get; set; }
        public TermGroup_EmploymentChangeFieldType FieldType { get; set; }
        public string FieldTypeName { get; set; }
        [TsIgnore]
        public string FieldTypeNameSuffix { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [TsIgnore]
        public string FromToDescription
        {
            get
            {
                return $"{(FromDate.HasValue ? FromDate.Value.ToString("yyyy-MM-dd") : "NA")}-{(ToDate.HasValue ? ToDate.Value.ToString("yyyy-MM-dd") : "NA")}";
            }
        }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public string FromValueName { get; set; }
        public string ToValueName { get; set; }
        public string Comment { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool IsDeleted
        {
            get
            {
                return this.State != SoeEntityState.Active;
            }
        }

        public bool DoRequireDate()
        {
            return DoFieldRequireDate(this.FieldType);
        }

        public static bool DoFieldRequireDate(TermGroup_EmploymentChangeFieldType fieldType)
        {
            switch (fieldType)
            {
                case TermGroup_EmploymentChangeFieldType.DateTo:
                case TermGroup_EmploymentChangeFieldType.State:
                case TermGroup_EmploymentChangeFieldType.EmploymentEndReason:
                    return false;
                default:
                    return true;
            }
        }
    }

    #endregion

    #region EmploymentPriceType

    public class EmploymentPriceTypeDTO
    {
        #region Propertys

        public int EmploymentPriceTypeId { get; set; }
        public int EmploymentId { get; set; }
        public int EmployeeId { get; set; }
        public int PayrollPriceTypeId { get; set; }

        // Extensions
        public string Code { get; set; }
        public string Name { get; set; }
        public TermGroup_SoePayrollPriceType PayrollPriceType { get; set; }
        public int Sort { get; set; }
        public decimal? PayrollGroupAmount { get; set; }
        public DateTime? PayrollGroupAmountDate { get; set; }
        public bool ReadOnly { get; set; }
        public bool IsPayrollGroupPriceType { get; set; }

        [TsIgnore]
        public int? CurrentPayrollLevelId { get; set; }//Depens on date, not always set
        [TsIgnore]
        public string CurrentPayrollLevelName { get; set; }//Depens on date, not always set

        // Relations
        public PayrollPriceTypeDTO Type { get; set; }
        public List<EmploymentPriceTypePeriodDTO> Periods { get; set; }

        #endregion

        #region Public methods

        public EmploymentPriceTypePeriodDTO GetPeriod(DateTime? date)
        {
            if (this.Periods == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return this.Periods.Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public decimal? GetAmount(DateTime? date)
        {
            if (this.PayrollGroupAmount.HasValue && this.IsPayrollGroupPriceType)
                return this.PayrollGroupAmount.Value;

            EmploymentPriceTypePeriodDTO period = GetPeriod(date);
            return period?.Amount ?? this.PayrollGroupAmount;
        }
        public int? GetPayrollLevel(DateTime? date)
        {
            return GetPeriod(date)?.PayrollLevelId;
        }

        public Tuple<decimal, DateTime?, bool> GetAmountAndDateAndIsIsPayrollGroupPriceType(DateTime? date)
        {
            if (this.PayrollGroupAmount.HasValue && this.IsPayrollGroupPriceType)
                return Tuple.Create(this.PayrollGroupAmount.Value, this.PayrollGroupAmountDate, true);

            EmploymentPriceTypePeriodDTO period = GetPeriod(date);
            if (period != null)
                return Tuple.Create(period.Amount, period.FromDate, false);

            return Tuple.Create(decimal.Zero, (DateTime?)null, false);
        }

        #endregion
    }

    public class EmploymentPriceTypePeriodDTO
    {
        #region Propertys

        public int EmploymentPriceTypePeriodId { get; set; }
        public int EmploymentPriceTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal Amount { get; set; }
        public int? PayrollLevelId { get; set; }
        public string PayrollLevelName { get; set; }

        // Extensions
        public bool Hidden { get; set; }

        #endregion
    }

    public class EmploymentPriceTypeChangeDTO
    {
        #region Propertys

        public int PayrollPriceTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPayrollGroupPriceType { get; set; }
        public bool IsSecondaryEmployment { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public TermGroup_SoePayrollPriceType PayrollPriceType { get; set; }
        public int? PayrollLevelId { get; set; }
        public string PayrollLevelName { get; set; }
        public decimal PayrollGroupAmount { get; set; }

        public bool IsEqual(EmploymentPriceTypeChangeDTO item)
        {
            //Check unique fields except FromDate and ToDate because more than one type of PayrollPriceType can exist on the same date and must be handled separately.
            return (this.PayrollPriceTypeId == item.PayrollPriceTypeId && this.Amount == item.Amount && this.IsPayrollGroupPriceType == item.IsPayrollGroupPriceType);
        }
        #endregion
    }

    #endregion

    #region EmploymentVacationGroup

    public class EmploymentVacationGroupDTO : IEmploymentVacationGroup
    {
        #region Propertys

        public int EmploymentVacationGroupId { get; set; }
        public int EmploymentId { get; set; }
        public int VacationGroupId { get; set; }
        public DateTime? FromDate { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        [TsIgnore]
        public bool IsAddedAsDefault { get; set; }
        public TermGroup_VacationGroupCalculationType CalculationType { get; set; }
        public TermGroup_VacationGroupVacationHandleRule VacationHandleRule { get; set; }
        public TermGroup_VacationGroupVacationDaysHandleRule VacationDaysHandleRule { get; set; }

        #endregion
    }

    #endregion

    #region EmployeeTimePeriodProductSetting

    public class EmployeeTimePeriodProductSettingDTO
    {
        public int EmployeeTimePeriodProductSettingId { get; set; }
        public int EmployeeTimePeriodId { get; set; }
        public int PayrollProductId { get; set; }
        public TermGroup_PayrollProductTaxCalculationType TaxCalculationType { get; set; }
        public bool PrintOnSalarySpecification { get; set; }
        public bool UseSettings { get; set; }
        public string Note { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #region EmployeeTimePeriod

    public class EmployeeTimePeriodDTO
    {
        public int EmployeeTimePeriodId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int TimePeriodId { get; set; }
        public SoeEmployeeTimePeriodStatus Status { get; set; }
        public DateTime? SalarySpecificationPublishDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #endregion

    #region EndReason

    [TSInclude]
    public class EndReasonDTO
    {
        public int EndReasonId { get; set; }
        public bool SystemEndReson { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool IsActive { get; set; }

        public EndReasonDTO()
        {

        }
        public EndReasonDTO(int endReasonId, string name, string code, bool systemEndReson)
        {
            this.EndReasonId = endReasonId;
            this.Name = name;
            this.Code = code;
            this.SystemEndReson = systemEndReson;
            this.State = SoeEntityState.Active;
            this.IsActive = true;
        }
    }

    [TSInclude]
    public class EndReasonGridDTO
    {
        public int EndReasonId { get; set; }
        public string Name { get; set; }
        public bool SystemEndReson { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region EvacuationList
    public class EvacuationListDTO
    {
        public EvacuationListDTO() { }

        public EvacuationListDTO(int actorCompanyId, int userId, string name)
        {
            ActorCompanyId = actorCompanyId;
            UserId = userId;
            Name = name;
            EvacuationListRow = new List<EvacuationListRowDTO>();

        }
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }

        public List<EvacuationListRowDTO> EvacuationListRow { get; set; }
    }
    public class EvacuationListRowDTO
    {
        public EvacuationListRowDTO() { }
        public EvacuationListRowDTO(int headId, int employeeId, bool marked)
        {
            EvacuationListHeadId = headId;
            EmployeeId = employeeId;
            State = SoeEntityState.Active;
            Marked = marked;
        }
        public EvacuationListRowDTO(int employeeId, int userId, int headId, string headName, SoeEntityState state, DateTime? created, DateTime? modified)
        {
            EmployeeId = employeeId;
            UserId = userId;
            Created = created;
            Modified = modified;
            HeadName = headName;
            State = state;
            EvacuationListHeadId = headId;
        }

        public int EvacuationListHeadId { get; set; }
        public int EmployeeId { get; set; }
        public bool Marked { get; set; }
        public SoeEntityState State { get; set; }
        public int UserId { get; set; }
        public string HeadName { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }

    }
    #endregion 

    #region EventHistory

    public class EventHistoryDTO
    {
        public EventHistoryDTO() { }

        public EventHistoryDTO(int actorCompanyId, TermGroup_EventHistoryType type, SoeEntityType entity, int recordId, int? userId = null, string stringValue = null, int? integerValue = null, decimal? decimalValue = null, bool? booleanValue = null, DateTime? dateValue = null)
        {
            ActorCompanyId = actorCompanyId;
            Type = type;
            Entity = entity;
            RecordId = recordId;
            UserId = userId;
            StringValue = stringValue;
            IntegerValue = integerValue;
            DecimalValue = decimalValue;
            BooleanValue = booleanValue;
            DateValue = dateValue;
        }

        public int EventHistoryId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_EventHistoryType Type { get; set; }
        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }
        public int BatchId { get; set; }
        public int? UserId { get; set; }

        public string StringValue { get; set; }
        public int? IntegerValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public bool? BooleanValue { get; set; }
        public DateTime? DateValue { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string EntityName { get; set; }
        public string RecordName { get; set; }
    }

    #endregion

    #region FileRecord

    [TSInclude]
    public class FileRecordDTO
    {
        public FileRecordDTO() { }

        public int FileRecordId { get; set; }
        public int RecordId { get; set; }
        public SoeEntityType Entity { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Extension { get; set; }
        public string EntityTypeName { get; set; }
        public int? FileSize { get; set; }
        public int ActorCompanyId { get; set; }
        public int IdentifierId { get; set; }
        public string IdentifierNumber { get; set; }
        public string IdentifierName { get; set; }
        public SoeDataStorageRecordType Type { get; set; }
        public byte[] Data { get; set; }
    }

    [TSInclude]
    public class ZipFileRequestDTO
    {
        public List<int> Ids { get; set; }
        public string prefixName { get; set; }
    }

    #endregion

    #region FixedPayrollRow

    public class FixedPayrollRowDTO
    {
        public int FixedPayrollRowId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public bool Distribute { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public SoeEntityState State { get; set; }

        //Product
        public int ProductId { get; set; }
        [TsIgnore]
        public string ProductNr { get; set; }
        [TsIgnore]
        public string ProductName { get; set; }
        [TsIgnore]
        public bool IsFromPreviousPayrollGroup { get; set; }
        [TsIgnore]
        public DateTime IsFromPreviousPayrollGroupDate { get; set; }
        public string PayrollProductNrAndName
        {
            get { return $"{this.ProductNr} {this.ProductName}"; }
        }
    }

    public class EmployeeFixedPayrollRowChangesDTO
    {
        #region Propertys
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string PayrollGroupName { get; set; }
        public int FixedPayrollRowId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public bool Distribute { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        public string EmploymentTypeName { get; set; }
        public bool FromPayrollGroup { get; set; }

        #endregion
    }

    public class FixedPayrollRowIODTO
    {
        public string EmployeeNr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public string ProductNr { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Quantity { get; set; }

        public bool IsSpecifiedUnitPrice { get; set; }

        public decimal Amount { get; set; }

        public decimal VatAmount { get; set; }
    }

    #endregion

    #region FollowUpType


    [TSInclude]
    public class FollowUpTypeDTO
    {
        public int FollowUpTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_FollowUpTypeType Type { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }


    [TSInclude]
    public class FollowUpTypeGridDTO
    {
        public int FollowUpTypeId { get; set; }
        public TermGroup_FollowUpTypeType Type { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    #endregion

    #region GrossProfitCode

    [TSInclude]
    public class GrossProfitCodeGridDTO
    {
        public int GrossProfitCodeId { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AccountYearId { get; set; }
        public DateTime AccountDateFrom { get; set; }
        public DateTime AccountDateTo { get; set; }
        public string AccountYear
        {
            get
            {
                return string.Format("{0} - {1}", AccountDateFrom.ToShortDateString(), AccountDateTo.ToShortDateString());
            }
        }
    }

    [TSInclude]
    public class GrossProfitCodeDTO
    {
        public int GrossProfitCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AccountYearId { get; set; }
        public DateTime AccountDateFrom { get; set; }
        public DateTime AccountDateTo { get; set; }
        public int? AccountDimId { get; set; }
        public int? AccountId { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AccountYear
        {
            get
            {
                return string.Format("{0} - {1}", AccountDateFrom.ToShortDateString(), AccountDateTo.ToShortDateString());
            }
        }

        public decimal OpeningBalance { get; set; }
        public decimal Period1 { get; set; }
        public decimal Period2 { get; set; }
        public decimal Period3 { get; set; }
        public decimal Period4 { get; set; }
        public decimal Period5 { get; set; }
        public decimal Period6 { get; set; }
        public decimal Period7 { get; set; }
        public decimal Period8 { get; set; }
        public decimal Period9 { get; set; }
        public decimal Period10 { get; set; }
        public decimal Period11 { get; set; }
        public decimal Period12 { get; set; }
        public decimal Period13 { get; set; }
        public decimal Period14 { get; set; }
        public decimal Period15 { get; set; }
        public decimal Period16 { get; set; }
        public decimal Period17 { get; set; }
        public decimal Period18 { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class GrossProfitCodeIODTO
    {
        public DateTime AccountYearDateFrom { get; set; }
        public int AccountDimNr { get; set; }
        public string AccountNr { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal Period1 { get; set; }
        public decimal Period2 { get; set; }
        public decimal Period3 { get; set; }
        public decimal Period4 { get; set; }
        public decimal Period5 { get; set; }
        public decimal Period6 { get; set; }
        public decimal Period7 { get; set; }
        public decimal Period8 { get; set; }
        public decimal Period9 { get; set; }
        public decimal Period10 { get; set; }
        public decimal Period11 { get; set; }
        public decimal Period12 { get; set; }
    }

    public class GrossProfitCodePeriodIODTO
    {
        public int PeriodNr { get; set; }
        public decimal Value { get; set; }
    }

    #endregion

    #region Help

    public class SLFormDTO
    {
        public int SLFormId { get; set; }
        public int? HelpId { get; set; }
        public string Name { get; set; }

        // Extensions
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
    }

    public class SLFieldDTO
    {
        public int SLFieldId { get; set; }
        public int SLFormId { get; set; }
        public string Name { get; set; }

        // Extensions
        public string FormName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }

        //Caution, not always set! Therefore it could be empty.
        public List<HelpDTO> Help { get; set; }

        public bool HasShortHelp
        {
            get
            {
                return Help.Any(h => h.Type == HelpType.ShortHelp);
            }
        }
        public SLFieldDTO()
        {
            this.Help = new List<HelpDTO>();
        }
    }

    public class HelpDTO
    {
        public int HelpId { get; set; }
        public HelpType Type { get; set; }

        // Extensions
        public string TypeName { get; set; }

        //forms that is directly connected to this Help entity, not through fields.
        public List<SLFormDTO> Forms { get; set; }
        public List<SLFieldDTO> Fields { get; set; }
        public List<HelpGroupDTO> HelpGroups { get; set; }

        public HelpDTO()
        {
            this.Forms = new List<SLFormDTO>();
            this.Fields = new List<SLFieldDTO>();
            this.HelpGroups = new List<HelpGroupDTO>();
        }
    }

    public class HelpGroupDTOBase
    {
        public int HelpGroupId { get; set; }

        // Extensions
        public String HelpGroupName { get; set; }
    }

    public class HelpGroupDTO : HelpGroupDTOBase
    {
        public int HelpGroupNameId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
    }

    public class HelpGroupTreeDTO : HelpGroupDTOBase
    {
        public int? ParentHelpGroupId { get; set; }
        public IEnumerable<HelpGroupTreeDTO> Children { get; set; }
        public IEnumerable<HelpTextDTOBase> HelpTexts { get; set; }
    }

    public class HelpTextDTOBase
    {
        public int HelpTextId { get; set; }
        public String Description { get; set; }
        public String Title { get; set; }
        public String PlainText { get; set; }
    }

    public class HelpTextDTOSearch : HelpTextDTOBase
    {
        public bool TitleHit { get; set; }
        public bool ContentHit { get; set; }
        public SearchLevel Level { get; set; }
    }

    public class HelpTextDTO : HelpTextDTOBase
    {
        public String Text { get; set; }
        public int SysLanguageId { get; set; }
        public int? ActorCompanyId { get; set; }
        public int? RoleId { get; set; }

        public DateTime? Created { get; set; }
        public String CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public String ModifiedBy { get; set; }

        // Extensions
        public String RoleName { get; set; }
        public String CompanyName { get; set; }
        public HelpDTO Help { get; set; }

        //FormNames, FieldNames and GroupNames are only set and used in GUI, to make binding to Grid easier - just for now. In the future we may want a more advanced Grid.
        public String FormNames { get; set; }
        public String FieldNames { get; set; }
        public String GroupNames { get; set; }

        public List<MessageAttachmentDTO> Attachments { get; set; }

        //public TermGroup_MessageTextType TextType { get; set; }

        public bool IsStandard
        {
            get { return (ActorCompanyId == null && RoleId == null); }
        }
        public bool IsForAllRoles
        {
            get { return (RoleId == null); }
        }
        public int HelpTypeId
        {
            get { return Help != null ? (int)Help.Type : 0; }
        }
        public int HelpId
        {
            get { return Help != null ? Help.HelpId : 0; }
        }
    }

    public class HelpTextSmallDTO
    {
        public String Title { get; set; }
        public String Text { get; set; }
        public String PlainText { get; set; }
    }

    #endregion

    #region Holiday

    [TSInclude]
    public class HolidayDTO
    {
        public int HolidayId { get; set; }
        public int ActorCompanyId { get; set; }
        public int DayTypeId { get; set; }
        public int? SysHolidayId { get; set; }
        public int? SysHolidayTypeId { get; set; }
        public string SysHolidayTypeName { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public bool IsRedDay { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string DayTypeName { get; set; }
        public bool Import { get; set; }

        [TSIgnore]
        public DayTypeDTO DayType { get; set; }
        [TSIgnore]
        public bool IsSaturday => this.Date.DayOfWeek == DayOfWeek.Saturday;
    }

    [TSInclude]
    public class HolidaySmallDTO
    {
        public int HolidayId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRedDay { get; set; }
    }

    [TSInclude]
    public class HolidayGridDTO
    {
        public int HolidayId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string DayTypeName { get; set; }
        public string SysHolidayTypeName { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Household tax deduction

    [Log]
    [TSInclude]
    public class HouseholdTaxDeductionApplicantDTO
    {
        //Flags
        public bool Hidden { get; set; }
        public bool ShowButton { get; set; }

        //Properties
        [LogHouseholdDeductionApplicantId]
        public int HouseholdTaxDeductionApplicantId { get; set; }
        [LogSocSec]
        public string SocialSecNr { get; set; }
        public string ApartmentNr { get; set; }
        public string Name { get; set; }
        public string Property { get; set; }
        public string CooperativeOrgNr { get; set; }
        public string IdentifierString { get; set; }
        public decimal Share { get; set; }
        public string Comment { get; set; }
        public SoeEntityState State { get; set; }

        // Applicants from row
        public int? CustomerInvoiceRowId { get; set; }
    }

    public class HouseholdTaxDeductionRowDTO
    {
        public string Property { get; set; }
        public string ApartmentNr { get; set; }
        public string CooperativeOrgNr { get; set; }
        public string SocialSecNr { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion

    #region Images
    [TSInclude]
    public class ImagesDTO
    {
        public int ImageId { get; set; }
        public int? InvoiceAttachmentId { get; set; }
        public ImageFormatType FormatType { get; set; }
        public byte[] Image { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string ConnectedTypeName { get; set; }
        public DateTime? Created { get; set; }
        public SoeEntityImageType Type { get; set; }
        public bool NeedsConfirmation { get; set; }
        public bool? IncludeWhenDistributed { get; set; }
        public bool? IncludeWhenTransfered { get; set; }
        public SoeDataStorageRecordType? DataStorageRecordType { get; set; }
        public InvoiceAttachmentSourceType SourceType { get; set; }
        public DateTime? LastSentDate { get; set; }

        public bool Confirmed { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public bool CanDelete { get; set; }

        // Document signing
        public int? AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public string CurrentAttestUsers { get; set; }
        public TermGroup_DataStorageRecordAttestStatus AttestStatus { get; set; }
        public int? RecordId { get; set; }
    }

    public class SaveSingleImagesDTO
    {
        public int ActorCompanyId { get; set; }
        public SoeEntityImageType Type { get; set; }
        public ImageFormatType FormatType { get; set; }
        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
    }

    public class SaveImagesDTO
    {
        public int ActorCompanyId { get; set; }
        public SoeEntityImageType Type { get; set; }
        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }

        public List<ImagesDTO> NewImages { get; set; }
        public Dictionary<int, string> UpdatedDescriptions { get; set; }
        public List<int> DeletedImages { get; set; }
    }

    public class SaveGalleryItemsDTO : SaveImagesDTO
    {
        public int UserId { get; set; }
        public Dictionary<int, string> UpdatedFileDescriptions { get; set; }
        public List<int> DeletedFiles { get; set; }
        public List<DataStorageRecordExtendedDTO> NewFiles { get; set; }
    }

    public class ImageViewerItemDTO
    {
        public int NrOfPages { get; set; }
        public Dictionary<int, byte[]> pages { get; set; }
        public int ScanningEntryId { get; set; }
    }

    #endregion

    #region IncomingDelivery

    [TSInclude]
    public class IncomingDeliveryHeadDTO
    {
        public int IncomingDeliveryHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int? NbrOfOccurrences { get; set; }
        public string RecurrencePattern { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }

        // Relations
        public string RecurrencePatternDescription { get; set; }
        public string RecurrenceStartsOnDescription { get; set; }
        public string RecurrenceEndsOnDescription { get; set; }
        public List<IncomingDeliveryRowDTO> Rows { get; set; }

        // Extensions
        public DailyRecurrenceDatesOutput RecurringDates { get; set; }
        public List<DateTime> ExcludedDates { get; set; }
        public string AccountName { get; set; }
    }

    [TSInclude]
    public class IncomingDeliveryGridDTO
    {
        public int IncomingDeliveryHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int? NbrOfOccurrences { get; set; }

        public string RecurrencePatternDescription { get; set; }
        public string RecurrenceStartsOnDescription { get; set; }
        public string RecurrenceEndsOnDescription { get; set; }
        public bool HasRows { get; set; }
    }

    [TSInclude]
    public class IncomingDeliveryRowDTO
    {
        public int IncomingDeliveryRowId { get; set; }
        public int IncomingDeliveryHeadId { get; set; }
        public int IncomingDeliveryTypeId { get; set; }
        public int? ShiftTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NbrOfPackages { get; set; }
        public int NbrOfPersons { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int Length { get; set; }
        public int OffsetDays { get; set; }
        public int MinSplitLength { get; set; }
        public bool OnlyOneEmployee { get; set; }
        public bool DontAssignBreakLeftovers { get; set; }
        public bool AllowOverlapping { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public IncomingDeliveryTypeDTO IncomingDeliveryTypeDTO { get; set; }

        // Relations
        public int? Account2Id { get; set; }
        public int? Account3Id { get; set; }
        public int? Account4Id { get; set; }
        public int? Account5Id { get; set; }
        public int? Account6Id { get; set; }

        // Extensions
        public string ShiftTypeName { get; set; }
        public string TypeName { get; set; }

        public int? HeadAccountId { get; set; }
        public string HeadAccountName { get; set; }
    }

    [TSInclude]
    public class IncomingDeliveryTypeGridDTO
    {
        public int IncomingDeliveryTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public int NbrOfPersons { get; set; }
        public string AccountName { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class IncomingDeliveryTypeDTO
    {
        public int IncomingDeliveryTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public int NbrOfPersons { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    [TSInclude]
    public class IncomingDeliveryTypeSmallDTO
    {
        public int IncomingDeliveryTypeId { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public int NbrOfPersons { get; set; }
    }

    #endregion

    #region Information

    public class InformationDTO
    {
        public int InformationId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? LicenseId { get; set; }
        public int SysLanguageId { get; set; }
        public SoeInformationSourceType SourceType { get; set; }
        public SoeInformationType Type { get; set; }
        public TermGroup_InformationSeverity Severity { get; set; }
        public string Subject { get; set; }
        public string ShortText { get; set; }
        public string Text { get; set; }
        public string Folder { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public TermGroup_InformationStickyType StickyType { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool NeedsConfirmation { get; set; }
        public XEMailAnswerType AnswerType { get; set; }
        public DateTime? AnswerDate { get; set; }

        public bool ShowInWeb { get; set; }
        public bool ShowInMobile { get; set; }
        public bool ShowInTerminal { get; set; }
        public bool Notify { get; set; }
        public DateTime? NotificationSent { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Relations
        public List<int> MessageGroupIds { get; set; }
        public List<int> SysFeatureIds { get; set; }
        public List<int> SysCompDbIds { get; set; }
        public List<SysInformationSysCompDbDTO> SysInformationSysCompDbs { get; set; }
        public List<InformationRecipientDTO> Recipients { get; set; }

        // Extensions
        public string SeverityName { get; set; }
        public bool HasText { get; set; }

        public DateTime CreatedOrModified
        {
            get
            {
                List<DateTime> dates = new List<DateTime>();

                dates.Add(CalendarUtility.DATETIME_DEFAULT);
                if (Created.HasValue)
                    dates.Add(Created.Value);
                if (Modified.HasValue)
                    dates.Add(Modified.Value);
                if (ValidFrom.HasValue && ValidFrom.Value < DateTime.Now)
                    dates.Add(ValidFrom.Value);

                return dates.Max();
            }
        }

        public DateTime? DisplayDate
        {
            get
            {
                return ValidFrom.HasValue ? ValidFrom : Created;
            }
        }
    }

    public class InformationGridDTO
    {
        public int InformationId { get; set; }
        public TermGroup_InformationSeverity Severity { get; set; }
        public string Subject { get; set; }
        public string ShortText { get; set; }
        public string Folder { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public bool NeedsConfirmation { get; set; }

        public bool ShowInWeb { get; set; }
        public bool ShowInMobile { get; set; }
        public bool ShowInTerminal { get; set; }
        public bool Notify { get; set; }
        public DateTime? NotificationSent { get; set; }

        // Extensions
        public string SeverityName { get; set; }
    }

    public class InformationRecipientDTO
    {
        public int InformationRecipientId { get; set; }
        public int? InformationId { get; set; }
        public int? SysInformationId { get; set; }
        public int UserId { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? HideDate { get; set; }

        // Extensions
        public string UserName { get; set; }
        public string EmployeeNrAndName { get; set; }
        public string CompanyName { get; set; }
    }

    public class SysInformationSysCompDbDTO
    {
        public int SysCompDbId { get; set; }
        public DateTime? NotificationSent { get; set; }

        // Extensions
        public string SiteName { get; set; }
    }

    #endregion

    #region Inventory

    #region Inventory

    [TSInclude]
    public class InventoryDTO
    {
        public int InventoryId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ParentId { get; set; }
        public int InventoryWriteOffMethodId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public int? SupplierInvoiceId { get; set; }
        public int? CustomerInvoiceId { get; set; }

        public string InventoryNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public TermGroup_InventoryStatus Status { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WriteOffDate { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal WriteOffAmount { get; set; }
        public decimal WriteOffSum { get; set; }
        public decimal WriteOffRemainingAmount { get; set; }
        public decimal EndAmount { get; set; }

        public TermGroup_InventoryWriteOffMethodPeriodType PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public int WriteOffPeriods { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> InventoryAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> AccWriteOffAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> WriteOffAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> AccOverWriteOffAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> OverWriteOffAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> AccWriteDownAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> WriteDownAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> AccWriteUpAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> WriteUpAccounts { get; set; }
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }
        public List<int> CategoryIds { get; set; }
        public string ParentName { get; set; }
        public string StatusName { get; set; }
        public string SupplierInvoiceInfo { get; set; }
        public string CustomerInvoiceInfo { get; set; }
        public List<FileUploadDTO> InventoryFiles { get; set; }
    }

    public class InventoryIODTO
    {
        public string InventoryNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }

        public int Status { get; set; }
        public string InventoryWriteOffMethodName { get; set; }
        public string VoucherSeriesTypeNr { get; set; }
        public string SupplierInvoiceNr { get; set; }
        public string CustomerInvoiceNr { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WriteOffDate { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal WriteOffAmount { get; set; }
        public decimal WriteOffSum { get; set; }
        public decimal WriteOffRemainingAmount { get; set; }
        public decimal EndAmount { get; set; }

        public int PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public int WriteOffPeriods { get; set; }

        public int State { get; set; }

        public string InventoryAccountNr { get; set; }
        public string InventoryAccountDim2Nr { get; set; }
        public string InventoryAccountDim3Nr { get; set; }
        public string InventoryAccountDim4Nr { get; set; }
        public string InventoryAccountDim5Nr { get; set; }
        public string InventoryAccountDim6Nr { get; set; }
        public string InventoryAccountSieDim1 { get; set; }
        public string InventoryAccountSieDim6 { get; set; }

        public string AccWriteOffAccountNr { get; set; }
        public string AccWriteOffAccountDim2Nr { get; set; }
        public string AccWriteOffAccountDim3Nr { get; set; }
        public string AccWriteOffAccountDim4Nr { get; set; }
        public string AccWriteOffAccountDim5Nr { get; set; }
        public string AccWriteOffAccountDim6Nr { get; set; }
        public string AccWriteOffAccountSieDim1 { get; set; }
        public string AccWriteOffAccountSieDim6 { get; set; }

        public string WriteOffAccountNr { get; set; }
        public string WriteOffAccountDim2Nr { get; set; }
        public string WriteOffAccountDim3Nr { get; set; }
        public string WriteOffAccountDim4Nr { get; set; }
        public string WriteOffAccountDim5Nr { get; set; }
        public string WriteOffAccountDim6Nr { get; set; }
        public string WriteOffAccountSieDim1 { get; set; }
        public string WriteOffAccountSieDim6 { get; set; }

        public string AccOverWriteOffAccountNr { get; set; }
        public string AccOverWriteOffAccountDim2Nr { get; set; }
        public string AccOverWriteOffAccountDim3Nr { get; set; }
        public string AccOverWriteOffAccountDim4Nr { get; set; }
        public string AccOverWriteOffAccountDim5Nr { get; set; }
        public string AccOverWriteOffAccountDim6Nr { get; set; }
        public string AccOverWriteOffAccountSieDim1 { get; set; }
        public string AccOverWriteOffAccountSieDim6 { get; set; }

        public string OverWriteOffAccountNr { get; set; }
        public string OverWriteOffAccountDim2Nr { get; set; }
        public string OverWriteOffAccountDim3Nr { get; set; }
        public string OverWriteOffAccountDim4Nr { get; set; }
        public string OverWriteOffAccountDim5Nr { get; set; }
        public string OverWriteOffAccountDim6Nr { get; set; }
        public string OverWriteOffAccountSieDim1 { get; set; }
        public string OverWriteOffAccountSieDim6 { get; set; }

        public string AccWriteDownAccountNr { get; set; }
        public string AccWriteDownAccountDim2Nr { get; set; }
        public string AccWriteDownAccountDim3Nr { get; set; }
        public string AccWriteDownAccountDim4Nr { get; set; }
        public string AccWriteDownAccountDim5Nr { get; set; }
        public string AccWriteDownAccountDim6Nr { get; set; }
        public string AccWriteDownAccountSieDim1 { get; set; }
        public string AccWriteDownAccountSieDim6 { get; set; }

        public string WriteDownAccountNr { get; set; }
        public string WriteDownAccountDim2Nr { get; set; }
        public string WriteDownAccountDim3Nr { get; set; }
        public string WriteDownAccountDim4Nr { get; set; }
        public string WriteDownAccountDim5Nr { get; set; }
        public string WriteDownAccountDim6Nr { get; set; }
        public string WriteDownAccountSieDim1 { get; set; }
        public string WriteDownAccountSieDim6 { get; set; }

        public string AccWriteUpAccountNr { get; set; }
        public string AccWriteUpAccountDim2Nr { get; set; }
        public string AccWriteUpAccountDim3Nr { get; set; }
        public string AccWriteUpAccountDim4Nr { get; set; }
        public string AccWriteUpAccountDim5Nr { get; set; }
        public string AccWriteUpAccountDim6Nr { get; set; }
        public string AccWriteUpAccountSieDim1 { get; set; }
        public string AccWriteUpAccountSieDim6 { get; set; }

        public string WriteUpAccountNr { get; set; }
        public string WriteUpAccountDim2Nr { get; set; }
        public string WriteUpAccountDim3Nr { get; set; }
        public string WriteUpAccountDim4Nr { get; set; }
        public string WriteUpAccountDim5Nr { get; set; }
        public string WriteUpAccountDim6Nr { get; set; }
        public string WriteUpAccountSieDim1 { get; set; }
        public string WriteUpAccountSieDim6 { get; set; }

        public string ParentName { get; set; }

        public string InventoryWriteOffMethodDescription { get; set; }
        public int InventoryWriteOffMethodPeriodType { get; set; }
        public int InventoryWriteOffMethodPeriodValue { get; set; }
        public int InventoryWriteOffMethodType { get; set; }

        public List<AccountDistributionEntryRowIODTO> accountDistributionEntryRowIODTOs { get; set; }
    }

    [TSInclude]
    public class InventoryGridDTO
    {
        public int InventoryId { get; set; }
        public string InventoryNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal WriteOffAmount { get; set; }
        public decimal WriteOffRemainingAmount { get; set; }
        public decimal WriteOffSum { get; set; }
        public decimal EndAmount { get; set; }
        public string InventoryWriteOffMethod { get; set; }
        public int InventoryWriteOffMethodId { get; set; }
        public string InventoryAccountNr { get; set; }
        public string InventoryAccountName { get; set; }
        public string Categories { get; set; }

        public decimal AccWriteOffAmount => WriteOffAmount - WriteOffRemainingAmount;
        public string InventoryAccountNumberName => $"{InventoryAccountNr} {InventoryAccountName}";
    }

    public class InventoryAccountDTO
    {
        public int InventoryAccountId { get; set; }
        public string InventoryAccountNr { get; set; }
        public string InventoryAccountName { get; set; }
    }

    #endregion

    #region InventoryWriteOffMethod

    [TSInclude]
    public class InventoryWriteOffMethodDTO
    {
        public int InventoryWriteOffMethodId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_InventoryWriteOffMethodType Type { get; set; }
        public TermGroup_InventoryWriteOffMethodPeriodType PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public decimal YearPercent { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool HasAcitveWirteOffs { get; set; }
    }

    [TSInclude]
    public class InventoryWriteOffMethodGridDTO
    {
        public int InventoryWriteOffMethodId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_InventoryWriteOffMethodType Type { get; set; }
        public string TypeName { get; set; }
        public TermGroup_InventoryWriteOffMethodPeriodType PeriodType { get; set; }
        public string PeriodTypeName { get; set; }
        public int PeriodValue { get; set; }
        public SoeEntityState State { get; set; }
        public decimal YearPercent { get; set; }
    }

    #endregion

    #region InventoryWriteOffTemplate

    [TSInclude]
    public class InventoryWriteOffTemplateDTO
    {
        public int InventoryWriteOffTemplateId { get; set; }
        public int ActorCompanyId { get; set; }
        public int InventoryWriteOffMethodId { get; set; }
        public int VoucherSeriesTypeId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<AccountingSettingDTO> AccountingSettings { get; set; }

        // Extensions
        // TODO: Can remove these after Shared/Economy/Inventory/Inventories/EditController.ts references are migrated to Angular.
        // (AccountingRowsDirective.ts in Supplier Invoice EditController, TraceRows.ts)
        public Dictionary<int, AccountSmallDTO> InventoryAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> AccWriteOffAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> WriteOffAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> AccOverWriteOffAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> OverWriteOffAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> AccWriteDownAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> WriteDownAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> AccWriteUpAccounts { get; set; }
        public Dictionary<int, AccountSmallDTO> WriteUpAccounts { get; set; }
    }

    [TSInclude]
    public class InventoryWriteOffTemplateGridDTO
    {
        public int InventoryWriteOffTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int InventoryWriteOffMethodId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public string InventoryWriteOffName { get; set; }
        public string VoucherSeriesName { get; set; }
    }

    #endregion

    #endregion

    #region Invoice

    #region Template

    public class InvoiceTemplateDTO
    {
        public int InvoiceId { get; set; }
        public int? ActorId { get; set; }
        public int? SortingNbr { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string ActorName { get; set; }
        public SoeOriginType OriginType { get; set; }
    }

    #endregion

    #region Invoice
    [TSInclude]
    public class InvoiceDTO
    {
        // Keys
        public int InvoiceId { get; set; }
        public int? ActorId { get; set; }
        public int? ContactEComId { get; set; }
        public int? VoucheHeadId { get; set; }
        public int? VoucheHead2Id { get; set; }
        public int? SysPaymentTypeId { get; set; }
        public int? ProjectId { get; set; }
        public int? VatCodeId { get; set; }
        public int? DeliveryCustomerId { get; set; }

        // Types
        public SoeInvoiceType Type { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }

        // Numbers
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public string OCR { get; set; }

        // Currency
        public int CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }

        // Dates
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }

        //Time discount
        public DateTime? TimeDiscountDate { get; set; }
        public decimal? TimeDiscountPercent { get; set; }

        // References
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }

        // Amounts
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal TotalAmountEntCurrency { get; set; }
        public decimal TotalAmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public decimal PaidAmountEntCurrency { get; set; }
        public decimal PaidAmountLedgerCurrency { get; set; }
        public decimal? RemainingAmount { get; set; }
        public decimal? RemainingAmountExVat { get; set; }

        // Flags
        public bool FullyPayed { get; set; }
        public bool OnlyPayment { get; set; }
        public string PaymentNr { get; set; }
        public bool ManuallyAdjustedAccounting { get; set; }
        public bool IsTemplate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public SoeStatusIcon StatusIcon { get; set; }

        // Extensions
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string OriginDescription { get; set; }
        public List<OriginUserDTO> OriginUsers { get; set; }
        public int VoucherSeriesId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public int ClaimAccountId { get; set; }

        //Dims
        public int? DefaultDim1AccountId { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }

        public int? ContactGLNId { get; set; }
    }

    #endregion

    #region Import
    [TSInclude]
    public class ImportBatchDTO
    {
        public int RecordId { get; set; }
        public TermGroup_IOType Type { get; set; }
        public string TypeName { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public string SourceName { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public string ImportHeadTypeName { get; set; }
        public List<TermGroup_IOStatus> Status { get; set; }
        public List<string> StatusName { get; set; }

        public string BatchId { get; set; }
        public DateTime? Created { get; set; }
    }

    [TSInclude]
    public class ImportDTO
    {
        // Keys
        public int ImportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int ImportDefinitionId { get; set; }
        public int? AccountYearId { get; set; }
        public int? VoucherSeriesId { get; set; }

        public int Module { get; set; }
        public string Name { get; set; }
        public string HeadName { get; set; }
        public SoeEntityState State { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public TermGroup_SysImportDefinitionType Type { get; set; }
        public string TypeText { get; set; }
        public bool UseAccountDistribution { get; set; }
        public bool UseAccountDimensions { get; set; }
        public bool UpdateExistingInvoice { get; set; }
        public Guid Guid { get; set; }
        public string SpecialFunctionality { get; set; }

        public int? Dim1AccountId { get; set; }
        public int? Dim2AccountId { get; set; }
        public int? Dim3AccountId { get; set; }
        public int? Dim4AccountId { get; set; }
        public int? Dim5AccountId { get; set; }
        public int? Dim6AccountId { get; set; }

        //Flags
        public bool IsStandard { get; set; }
        public string IsStandardText { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
    [TSInclude]
    public class ImportGridColumnDTO
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string Headername { get; set; }
    }

    #endregion

    #region Export

    [TSInclude]
    public class InvoiceExportDTO
    {
        public int InvoiceExportId { get; set; }
        public int? ActorCompanyId { get; set; }
        public int BatchId { get; set; }
        public TermGroup_SysPaymentService SysPaymentServiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfInvoices { get; set; }
        public DateTime ExportDate { get; set; }
        public string Filename { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extension
        public string ServiceName { get; set; }
    }

    [TSInclude]
    public class InvoiceExportIODTO
    {
        public int InvoiceExportIOId { get; set; }
        public int InvoiceExportId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public TermGroup_BillingType InvoiceType { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string InvoiceSeqnr { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public string Currency { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string BankAccount { get; set; }
        public string PayerId { get; set; }
        public SoeEntityState State { get; set; }


        /* Extension */
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public string TypeName { get; set; }
        public string StateName { get; set; }


    }

    #endregion Export

    #region CustomerInvoice

    [TSInclude]
    public class HandleBillingRowDTO
    {
        // CustoomerInvoiceRow
        public int CustomerInvoiceRowId { get; set; }
        public int RowNr { get; set; }
        public SoeInvoiceRowType Type { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? InvoiceQuantity { get; set; }
        public decimal? PreviouslyInvoicedQuantity { get; set; }
        public int DiscountType { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime? Created { get; set; }
        public int? EdiEntryId { get; set; }
        public string EdiTextValue { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmountCurrency { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal PurchasePriceCurrency { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        public decimal MarginalIncomeRatio { get; set; }
        public decimal MarginalIncomeLimit { get; set; }
        public bool IsTimeProjectRow { get; set; }
        public bool TimeManuallyChanged { get; set; }
        public int? HouseholdDeductionType { get; set; }
        public DateTime? Date { get; set; }
        public bool IsStockRow { get; set; }
        public SoeProductRowType ProductRowType { get; set; }

        // Invoice
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }

        // Customer
        public int ActorCustomerId { get; set; }
        public string Customer { get; set; }

        // Project
        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string Project { get; set; }

        // AttestState
        public int? AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        // Product
        public int? ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public TermGroup_InvoiceProductCalculationType ProductCalculationType { get; set; }

        // ProductUnit
        public int? ProductUnitId { get; set; }
        public string ProductUnitCode { get; set; }

        // Extentions
        public bool ValidForInvoice { get; set; }
    }

    public class CustomerInvoiceInterestDTO
    {
        public int CustomerInvoiceInterestId { get; set; }
        public int CustomerInvoiceOriginId { get; set; }
        public int InvoiceProductId { get; set; }
        public string BatchId { get; set; }

        public SoeInvoiceInterestHandlingType Type { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PayDate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        // Extensions
        public string CustomerName { get; set; }
    }

    public class CustomerInvoiceReminderDTO
    {
        public int CustomerInvoiceReminderId { get; set; }
        public int CustomerInvoiceOriginId { get; set; }
        public int InvoiceProductId { get; set; }
        public string BatchId { get; set; }

        public SoeInvoiceReminderHandlingType Type { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime DueDate { get; set; }
        public int NoOfReminder { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        // Extensions
        public string CustomerName { get; set; }
    }

    public class CustomerInvoicePrintedReminderDTO
    {
        public int CustomerInvoiceReminderId { get; set; }
        public int ActorCompanyId { get; set; }
        public int CustomerInvoiceOriginId { get; set; }
        public string InvoiceNr { get; set; }
        public decimal Amount { get; set; }
        public DateTime ReminderDate { get; set; }
        public DateTime DueDate { get; set; }
        public int NoOfReminder { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }

    #region Order

    public class OrderDTO
    {
        // Keys
        public int InvoiceId { get; set; }
        public int? MainInvoiceId { get; set; }
        public int? ActorId { get; set; }
        public int? DeliveryCustomerId { get; set; }
        public int? PayingCustomerId { get; set; }
        public int? ContactEComId { get; set; }
        public int? ContactGLNId { get; set; }
        //public int? VoucheHeadId { get; set; }
        //public int? VoucheHead2Id { get; set; }
        //public int? SysPaymentTypeId { get; set; }
        public int? ProjectId { get; set; }
        //public int? VatCodeId { get; set; }
        //public int OriginateFrom { get; set; }
        public int? PaymentConditionId { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? DeliveryConditionId { get; set; }
        public int DeliveryAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public int? PriceListTypeId { get; set; }
        public int? SysWholeSellerId { get; set; }

        // Account dims
        public int? DefaultDim1AccountId { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }

        // Types
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public TermGroup_OrderType OrderType { get; set; }

        // Numbers
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public string MainInvoiceNr { get; set; }
        //public string OCR { get; set; }

        // Text
        public string InvoiceText { get; set; }
        public string InvoiceHeadText { get; set; }
        public string InvoiceLabel { get; set; }
        //public string InternalDescription { get; set; }
        //public string ExternalDescription { get; set; }
        public string WorkingDescription { get; set; }
        public string BillingAdressText { get; set; }
        public string DeliveryDateText { get; set; }
        public string OrderReference { get; set; }
        public string MainInvoice { get; set; }
        public string Note { get; set; }

        // Currency
        public int CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }

        // Dates
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Time discount
        //public DateTime? TimeDiscountDate { get; set; }
        //public decimal? TimeDiscountPercent { get; set; }

        // References
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public string ContractNr { get; set; }

        // Amounts
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        //public decimal TotalAmountEntCurrency { get; set; }
        //public decimal TotalAmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        //public decimal VatAmountEntCurrency { get; set; }
        //public decimal VatAmountLedgerCurrency { get; set; }
        //public decimal PaidAmount { get; set; }
        //public decimal PaidAmountCurrency { get; set; }
        //public decimal PaidAmountEntCurrency { get; set; }
        //public decimal PaidAmountLedgerCurrency { get; set; }
        public decimal? RemainingAmount { get; set; }
        //public decimal? RemainingAmountVat { get; set; }
        public decimal? RemainingAmountExVat { get; set; }
        public decimal CentRounding { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal FreightAmountCurrency { get; set; }
        //public decimal FreightAmountEntCurrency { get; set; }
        //public decimal FreightAmountLedgerCurrency { get; set; }
        public decimal InvoiceFee { get; set; }
        public decimal InvoiceFeeCurrency { get; set; }
        //public decimal InvoiceFeeEntCurrency { get; set; }
        //public decimal InvoiceFeeLedgerCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        //public decimal SumAmountEntCurrency { get; set; }
        //public decimal SumAmountLedgerCurrency { get; set; }
        //public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        public decimal? MarginalIncomeRatio { get; set; }

        // Flags
        public bool IsTemplate { get; set; }
        public bool FixedPriceOrder { get; set; }
        public bool PrintTimeReport { get; set; }
        public bool HasManuallyDeletedTimeProjectRows { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public bool AddSupplierInvoicesToEInvoice { get; set; }
        public SoeStatusIcon StatusIcon { get; set; }
        public bool CheckConflictsOnSave { get; set; }
        public bool ForceSave { get; set; }
        public bool TransferAttachments { get; set; }
        public bool IsMainInvoice { get; set; }
        public bool ProjectIsActive { get; set; }
        public bool ShowNote { get; set; }
        public TermGroup_OrderEdiTransferMode EdiTransferMode { get; set; }

        // Contract
        public int? ContractGroupId { get; set; }
        public int NextContractPeriodYear { get; set; }
        public int NextContractPeriodValue { get; set; }
        public DateTime? NextContractPeriodDate { get; set; }
        public int? OrderInvoiceTemplateId { get; set; }

        // Created/Modified
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        //public SoeEntityState State { get; set; }

        // Order planning
        public int? ShiftTypeId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedStopDate { get; set; }
        public int EstimatedTime { get; set; }
        public int RemainingTime { get; set; }
        public int? Priority { get; set; }
        public bool KeepAsPlanned { get; set; }

        //public bool? HasOrder { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? InvoicePaymentService { get; set; }
        public bool IncludeOnInvoice { get; set; }
        public bool IncludeOnlyInvoicedTime { get; set; }
        public TermGroup_IncludeExpenseInReportType includeExpenseInReport { get; set; }
        //public string OrderNumbers { get; set; }
        //public int ExportStatus { get; set; }
        //public int? CustomerInvoicePaymentService { get; set; }
        public bool TriangulationSales { get; set; }

        // Cash sales - one time customer
        public string CustomerName { get; set; }
        public string CustomerPhoneNr { get; set; }
        public string CustomerEmail { get; set; }

        // Extensions
        public int PrevInvoiceId { get; set; }
        public int NbrOfChecklists { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string OriginDescription { get; set; }
        public List<OriginUserSmallDTO> OriginUsers { get; set; }
        public int VoucherSeriesId { get; set; }
        public string ProjectNr { get; set; }
        //public int ClaimAccountId { get; set; }
        public string CustomerBlockNote { get; set; }
        public List<ProductRowDTO> CustomerInvoiceRows { get; set; }
        public List<int> CategoryIds { get; set; }
    }

    public class ProductRowDTO
    {
        // Keys
        public int CustomerInvoiceRowId { get; set; }
        public int TempRowId { get; set; }
        public int? ParentRowId { get; set; }
        public int? ProductId { get; set; }
        public int? ProductUnitId { get; set; }
        public int? AttestStateId { get; set; }
        public int? VatCodeId { get; set; }
        public int? VatAccountId { get; set; }
        public int? EdiEntryId { get; set; }
        public int? StockId { get; set; }
        public string StockCode { get; set; }
        public int? SupplierInvoiceId { get; set; }

        public int? MergeToId { get; set; }
        public int RowNr { get; set; }
        public SoeInvoiceRowType Type { get; set; }
        public TermGroup_HouseHoldTaxDeductionType HouseholdTaxDeductionType { get; set; }
        public int? prevCustomerInvoiceRowId { get; set; }
        public int? CustomerInvoiceInterestId { get; set; }
        public int? CustomerInvoiceReminderId { get; set; }

        [JsonProperty(PropertyName = "_quantity")]
        public decimal? Quantity { get; set; }
        public decimal? InvoiceQuantity { get; set; }
        public decimal? PreviouslyInvoicedQuantity { get; set; }
        public string Text { get; set; }
        public string DeliveryDateText { get; set; }
        public string SysWholesellerName { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmountCurrency { get; set; }
        public int Discount2Type { get; set; }
        public decimal Discount2Percent { get; set; }
        public decimal Discount2Amount { get; set; }
        public decimal Discount2AmountCurrency { get; set; }
        public decimal Discount2Value { get; set; }


        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal PurchasePrice { get; set; }
        [JsonProperty(PropertyName = "_purchasePriceCurrency")]
        public decimal PurchasePriceCurrency { get; set; }
        //public decimal PurchasePriceEntCurrency { get; set; }
        //public decimal PurchasePriceLedgerCurrency { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        //public decimal MarginalIncomeEntCurrency { get; set; }
        //public decimal MarginalIncomeLedgerCurrency { get; set; }
        public decimal? MarginalIncomeRatio { get; set; }

        // Dates
        public DateTime? Date { get; set; }
        public DateTime? DateTo { get; set; }

        // Flags
        public bool IsFreightAmountRow { get; set; }
        public bool IsInvoiceFeeRow { get; set; }
        public bool IsCentRoundingRow { get; set; }
        public bool IsInterestRow { get; set; }
        public bool IsReminderRow { get; set; }
        public bool IsTimeProjectRow { get; set; }
        public bool IsStockRow { get; set; }
        public bool IsLiftProduct { get; set; }
        public bool IsClearingProduct { get; set; }
        public bool IsContractProduct { get; set; }
        public bool IsFixedPriceProduct { get; set; }
        public bool IsSupplementChargeProduct { get; set; }
        public bool IsExpenseRow { get; set; }
        public bool IsTimeBillingRow { get; set; }

        public bool TimeManuallyChanged { get; set; }


        // Created/Modified
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // HouseholdTaxDeduction
        public int? HouseholdDeductionType { get; set; }
        public string HouseholdProperty { get; set; }
        [LogSocSec]
        public string HouseholdSocialSecNbr { get; set; }
        public string HouseholdName { get; set; }
        public decimal HouseholdAmount { get; set; }
        public decimal HouseholdAmountCurrency { get; set; }
        public string HouseholdApartmentNbr { get; set; }
        public string HouseholdCooperativeOrgNbr { get; set; }

        // Intrastat
        public int? IntrastatTransactionId { get; set; }
        public int? IntrastatCodeId { get; set; }
        public int? SysCountryId { get; set; }

        //Split accounting 
        public List<SplitAccountingRowDTO> SplitAccountingRows { get; set; }
    }

    public class CustomerInvoiceAccountRowDTO
    {
        // Keys
        public AccountingRowType Type { get; set; }
        public int InvoiceRowId { get; set; }
        public int InvoiceAccountRowId { get; set; }
        public int TempRowId { get; set; }
        public int TempInvoiceRowId { get; set; }

        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1Disabled { get; set; }
        public bool Dim1Mandatory { get; set; }
        public bool Dim1Stop { get; set; }
        public bool Dim1ManuallyChanged { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public bool Dim2Disabled { get; set; }
        public bool Dim2Mandatory { get; set; }
        public bool Dim2Stop { get; set; }
        public bool Dim2ManuallyChanged { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public bool Dim3Disabled { get; set; }
        public bool Dim3Mandatory { get; set; }
        public bool Dim3Stop { get; set; }
        public bool Dim3ManuallyChanged { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public bool Dim4Disabled { get; set; }
        public bool Dim4Mandatory { get; set; }
        public bool Dim4Stop { get; set; }
        public bool Dim4ManuallyChanged { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public bool Dim5Disabled { get; set; }
        public bool Dim5Mandatory { get; set; }
        public bool Dim5Stop { get; set; }
        public bool Dim5ManuallyChanged { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public bool Dim6Disabled { get; set; }
        public bool Dim6Mandatory { get; set; }
        public bool Dim6Stop { get; set; }
        public bool Dim6ManuallyChanged { get; set; }

        // Row
        public int RowNr { get; set; }
        public decimal? Quantity { get; set; }
        public string Text { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal DebitAmountCurrency { get; set; }
        public decimal DebitAmountEntCurrency { get; set; }
        public decimal DebitAmountLedgerCurrency { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal CreditAmountCurrency { get; set; }
        public decimal CreditAmountEntCurrency { get; set; }
        public decimal CreditAmountLedgerCurrency { get; set; }
        public decimal Balance { get; set; }

        // Split
        public int SplitType { get; set; }
        public decimal SplitValue { get; set; }
        public decimal SplitPercent { get; set; }

        // Account distribution / inventory
        //public int AccountDistributionHeadId { get; set; }
        //public int AccountDistributionNbrOfPeriods { get; set; }
        //public DateTime? AccountDistributionStartDate { get; set; }

        //public int InventoryId { get; set; }

        // Flags
        public bool IsCreditRow { get; set; }
        public bool IsDebitRow { get; set; }
        public bool IsVatRow { get; set; }
        public bool IsContractorVatRow { get; set; }
        //public bool IsCentRoundingRow { get; set; }
        //public bool IsClaimRow { get; set; }
        //public bool IsHouseholdRow { get; set; }

        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        //public bool IsManuallyAdjusted { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public int ParentRowId { get; set; }
    }

    public class SplitAccountingRowDTO
    {
        // Keys
        public int InvoiceAccountRowId { get; set; }

        // Accounts
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public bool Dim1Disabled { get; set; }
        public bool Dim1Mandatory { get; set; }
        public bool Dim1Stop { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public bool Dim2Disabled { get; set; }
        public bool Dim2Mandatory { get; set; }
        public bool Dim2Stop { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public bool Dim3Disabled { get; set; }
        public bool Dim3Mandatory { get; set; }
        public bool Dim3Stop { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public bool Dim4Disabled { get; set; }
        public bool Dim4Mandatory { get; set; }
        public bool Dim4Stop { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public bool Dim5Disabled { get; set; }
        public bool Dim5Mandatory { get; set; }
        public bool Dim5Stop { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public bool Dim6Disabled { get; set; }
        public bool Dim6Mandatory { get; set; }
        public bool Dim6Stop { get; set; }

        // Split
        public int SplitType { get; set; }
        public decimal SplitValue { get; set; }
        public decimal SplitPercent { get; set; }

        // Amounts
        public decimal AmountCurrency { get; set; }
        public decimal DebitAmountCurrency { get; set; }
        public decimal CreditAmountCurrency { get; set; }

        // Flags
        public bool IsCreditRow { get; set; }
        public bool IsDebitRow { get; set; }
    }

    #endregion

    #region BillingInvoice

    public class BillingInvoiceDTO
    {
        // Keys
        public int InvoiceId { get; set; }
        public int? ActorId { get; set; }
        public int? DeliveryCustomerId { get; set; }

        public int? ContactEComId { get; set; }
        public int? ContactGLNId { get; set; }
        public int? ProjectId { get; set; }

        public int? PaymentConditionId { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? DeliveryConditionId { get; set; }
        public int DeliveryAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public int? PriceListTypeId { get; set; }
        public int? SysWholeSellerId { get; set; }
        public bool TriangulationSales { get; set; }

        // Account dims
        public int? DefaultDim1AccountId { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }

        // Types
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public TermGroup_OrderType OrderType { get; set; }

        // Numbers
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public string ExternalInvoiceNr { get; set; }

        // Text
        public string InvoiceText { get; set; }
        public string InvoiceHeadText { get; set; }
        public string InvoiceLabel { get; set; }
        //public string InternalDescription { get; set; }
        //public string ExternalDescription { get; set; }
        public string WorkingDescription { get; set; }
        public string BillingAdressText { get; set; }
        public string DeliveryDateText { get; set; }
        public string OrderReference { get; set; }
        public string Note { get; set; }

        // Currency
        public int CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }

        // Dates
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Time discount
        //public DateTime? TimeDiscountDate { get; set; }
        //public decimal? TimeDiscountPercent { get; set; }

        // References
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public string ContractNr { get; set; }

        // Amounts
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        //public decimal TotalAmountEntCurrency { get; set; }
        //public decimal TotalAmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        //public decimal VatAmountEntCurrency { get; set; }
        //public decimal VatAmountLedgerCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        //public decimal PaidAmountEntCurrency { get; set; }
        //public decimal PaidAmountLedgerCurrency { get; set; }
        public decimal? RemainingAmount { get; set; }
        //public decimal? RemainingAmountVat { get; set; }
        public decimal? RemainingAmountExVat { get; set; }
        public decimal CentRounding { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal FreightAmountCurrency { get; set; }
        //public decimal FreightAmountEntCurrency { get; set; }
        //public decimal FreightAmountLedgerCurrency { get; set; }
        public decimal InvoiceFee { get; set; }
        public decimal InvoiceFeeCurrency { get; set; }
        //public decimal InvoiceFeeEntCurrency { get; set; }
        //public decimal InvoiceFeeLedgerCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        //public decimal SumAmountEntCurrency { get; set; }
        //public decimal SumAmountLedgerCurrency { get; set; }
        //public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        //public decimal MarginalIncomeEntCurrency { get; set; }
        //public decimal MarginalIncomeLedgerCurrency { get; set; }
        public decimal? MarginalIncomeRatio { get; set; }

        // Flags
        public bool IsTemplate { get; set; }
        public bool ManuallyAdjustedAccounting { get; set; }
        //public bool HasHouseholdTaxDeduction { get; set; }
        public bool FixedPriceOrder { get; set; }
        //public bool MultipleAssetRows { get; set; }
        public bool InsecureDebt { get; set; }
        public bool PrintTimeReport { get; set; }
        public bool BillingInvoicePrinted { get; set; }
        public bool CashSale { get; set; }
        public bool HasManuallyDeletedTimeProjectRows { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public bool AddSupplierInvoicesToEInvoice { get; set; }
        public SoeStatusIcon StatusIcon { get; set; }
        public bool CheckConflictsOnSave { get; set; }
        public bool ForceSave { get; set; }

        // Created/Modified
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        //public SoeEntityState State { get; set; }

        // Order planning
        public int? ShiftTypeId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedStopDate { get; set; }
        public int EstimatedTime { get; set; }
        public int RemainingTime { get; set; }
        public int? Priority { get; set; }

        public bool? HasOrder { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? InvoiceDeliveryProvider { get; set; }
        public int? InvoicePaymentService { get; set; }
        public bool IncludeOnInvoice { get; set; }
        public bool IncludeOnlyInvoicedTime { get; set; }
        public string OrderNumbers { get; set; }
        //public int ExportStatus { get; set; }
        //public int? CustomerInvoicePaymentService { get; set; }

        // Cash sales - one time customer
        public string CustomerName { get; set; }
        public string CustomerPhoneNr { get; set; }
        public string CustomerEmail { get; set; }

        // Extensions
        public int PrevInvoiceId { get; set; }
        public int NbrOfChecklists { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string OriginDescription { get; set; }
        public List<OriginUserSmallDTO> OriginUsers { get; set; }
        public int VoucherSeriesId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public string ProjectNr { get; set; }
        //public int ClaimAccountId { get; set; }
        public string CustomerBlockNote { get; set; }
        public bool TransferedFromOrder { get; set; }
        public bool TransferedFromOffer { get; set; }
        public SoeOriginType TransferedFromOriginType { get; set; }
        public List<ProductRowDTO> CustomerInvoiceRows { get; set; }
        public List<int> CategoryIds { get; set; }
    }

    #endregion

    #endregion

    #region SupplierInvoice

    [TSInclude]
    public class FileUploadDTO
    {
        public int? Id { get; set; }
        public int? ImageId { get; set; }
        public int? InvoiceAttachmentId { get; set; }
        public bool IsSupplierInvoice { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool IncludeWhenDistributed { get; set; }
        public bool IncludeWhenTransfered { get; set; }
        public SoeDataStorageRecordType? DataStorageRecordType { get; set; }
        public InvoiceAttachmentSourceType? SourceType { get; set; }
        public int? RecordId { get; set; }
    }

    #endregion

    #endregion

    #region InExchange

    public class InExchangeDTO
    {
        public int InExchangeId { get; set; }
        public int InvoiceId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Guid { get; set; }
        public string Message { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int InvoiceState { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
    }

    [TSInclude]
    public class InvoiceDistributionDTO
    {
        public int InvoiceDistributionId { get; set; }
        public int OriginId { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }

        public string Message { get; set; }

        public string Guid { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        public string SeqNr { get; set; }

        public string CustomerName { get; set; }
        public string CustomerNr { get; set; }
        public int? OriginTypeId { get; set; }
        public string OriginTypeName { get; set; }
        public string TypeName { get; set; }
        public string StatusName { get; set; }
    }

    [TSInclude]
    public class SalesEUDTO
    {
        public int ActorId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNr { get; set; }
        public string VATNr { get; set; }

        public decimal SumGoodsSale { get; set; }
        public decimal SumServiceSale { get; set; }
        public decimal SumTriangulationSales { get; set; }
    }

    [TSInclude]
    public class SalesEUDetailDTO
    {
        public int ActorId { get; set; }
        public int CustomerInvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public decimal SumGoodsSale { get; set; }
        public decimal SumServiceSale { get; set; }
        public bool TriangulationSales { get; set; }
    }
    #endregion

    #region License

    public class LicenseDTO
    {
        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public bool Support { get; set; }
        public int? NrOfCompanies { get; set; }
        public int MaxNrOfUsers { get; set; }
        public int MaxNrOfEmployees { get; set; }
        public int MaxNrOfMobileUsers { get; set; }
        public int? ConcurrentUsers { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public bool AllowDuplicateUserLogin { get; set; }
        public string LegalName { get; set; }
        public bool IsAccountingOffice { get; set; }
        public int AccountingOfficeId { get; set; }
        public string AccountingOfficeName { get; set; }
        public int? SysServerId { get; set; }
        public Guid? LicenseGuid { get; set; }
    }

    #endregion

    #region LicenseArticle

    public class LicenseArticleDTO
    {
        public int LicenseArticleId { get; set; }
        public int LicenseId { get; set; }
        public int SysXEArticleId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #region Markup

    [TSInclude]
    public class MarkupDTO
    {
        public int MarkupId { get; set; }
        public int ActorCompanyId { get; set; }
        public int SysWholesellerId { get; set; }
        public int? ActorCustomerId { get; set; }

        public string Code { get; set; }
        public string ProductIdFilter { get; set; }

        public decimal MarkupPercent { get; set; }
        public decimal? DiscountPercent { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string WholesellerName { get; set; }
        public decimal WholesellerDiscountPercent { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string customerName { get; set; }
    }

    #endregion

    #region MassRegistrationTemplate

    public class MassRegistrationGridDTO
    {
        public int MassRegistrationTemplateHeadId { get; set; }
        public string Name { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? RecurringDateTo { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class MassRegistrationTemplateHeadDTO
    {
        public int MassRegistrationTemplateHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? RecurringDateTo { get; set; }
        public TermGroup_MassRegistrationInputType InputType { get; set; }
        public string Comment { get; set; }
        public bool StopOnProduct { get; set; }
        public bool StopOnDateFrom { get; set; }
        public bool StopOnDateTo { get; set; }
        public bool StopOnQuantity { get; set; }
        public bool StopOnIsSpecifiedUnitPrice { get; set; }
        public bool StopOnUnitPrice { get; set; }
        public bool StopOnPaymentDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? PayrollProductId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public int? Dim1Id { get; set; }
        [TsIgnore]
        public string Dim1Nr { get; set; }
        [TsIgnore]
        public string Dim1Name { get; set; }
        public int? Dim2Id { get; set; }
        public int? Dim3Id { get; set; }
        public int? Dim4Id { get; set; }
        public int? Dim5Id { get; set; }
        public int? Dim6Id { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<MassRegistrationTemplateRowDTO> Rows { get; set; }

        [TsIgnore]
        public List<AccountInternalDTO> AccountInternals { get; set; }
        //Used only for save in Silverlight
        [TsIgnore]
        public List<AccountingSettingDTO> AccountSettings { get; set; }

        public bool HasCreatedTransactions { get; set; }
    }

    public class MassRegistrationTemplateRowDTO
    {
        public int MassRegistrationTemplateRowId { get; set; }
        public int MassRegistrationTemplateHeadId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNrSort { get; set; }
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public int Dim2DimNr { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int Dim3DimNr { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int Dim4DimNr { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int Dim5DimNr { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int Dim6DimNr { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string Warnings { get; set; }

        [TsIgnore]
        public List<AccountInternalDTO> AccountInternals { get; set; }

        // Extensions
        public string ErrorMessage { get; set; }
    }

    #endregion

    #region MatchCode
    [TSInclude]
    public class MatchCodeGridDTO
    {
        public int MatchCodeId { get; set; }

        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string AccountNr { get; set; }
        public string VatAccountNr { get; set; }
        public SoeEntityState State { get; set; }

        [TSIgnore]
        public int TypeId { get; set; }
    }

    [TSInclude]
    public class MatchCodeDTO
    {
        public int MatchCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AccountId { get; set; }
        public int? VatAccountId { get; set; }

        public SoeInvoiceMatchingType Type { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string AccountNr { get; set; }
        public string VatAccountNr { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Message

    /***
     * This class is intent to work as a container for entities: Message and MessageRecipient. 
     * It will be used to represent messages in the grid (Icoming, Outgoing, Sent and Deleted)
     * */
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public int MessageTextId { get; set; }
        public TermGroup_MessageType MessageType { get; set; }
        public int Priority { get; set; }
        public int? ActorCompanyId { get; set; }

        public String SenderName { get; set; }
        public String Subject { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? Created { get; set; }

        // Extensions
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public bool IsHandledByJob { get; set; }
        public string HasBeenRead { get; set; }
        public String RecieversName { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public List<MessageRecipientDTO> RecipientList { get; set; }
        public bool NeedsConfirmation { get; set; }
        public bool NeedsConfirmationAnswer { get; set; }
        public string HasBeenConfirmed { get; set; }
        public DateTime? AnswerDate { get; set; }
        public bool HasAttachment { get; set; }

        public bool IsUnRead
        {
            get { return !ReadDate.HasValue; }
        }

        public bool IsExpired
        {
            get { return ExpirationDate.HasValue && ExpirationDate.Value > DateTime.Now; }
        }

        public MessageDTO()
        {
            IsVisible = true;
            IsSelected = false;
            RecipientList = new List<MessageRecipientDTO>();
        }
    }

    public class MessageGridDTO
    {
        public int MessageId { get; set; }
        public TermGroup_MessageType MessageType { get; set; }
        public String SenderName { get; set; }
        public String Subject { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool NeedsConfirmation { get; set; }
        public DateTime? AnswerDate { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? ForwardDate { get; set; }
        public String RecieversName { get; set; }
        public DateTime? SentDate { get; set; }
        public string HasBeenRead { get; set; }
        public string HasBeenConfirmed { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool HasAttachment { get; set; }
        public String FirstTextRow { get; set; }
    }

    //public class MailMessageDTO
    //{
    //    public MailMessageDTO()
    //    {

    //    }
    //    public MailMessageDTO(String from, String to, List<string> cc, String subject, String body, bool emailcontentIsHtml, bool convert)
    //    {
    //        List<string> recievers = new List<string>();
    //        recievers.Add(to);

    //        this.SenderEmail = from;
    //        this.SenderName = from;
    //        this.subject = subject;
    //        this.recievers = recievers;
    //        this.cc = cc;
    //        this.subject = subject;
    //        this.body = body;
    //        this.EmailcontentIsHtml = emailcontentIsHtml;
    //    }
    //    public string SenderName { get; set; }
    //    public string SenderEmail { get; set; }
    //    public List<string> recievers { get; set; }
    //    public List<string> cc { get; set; }
    //    public string subject { get; set; }
    //    public string body { get; set; }

    //    public bool EmailcontentIsHtml { get; set; }

    //    public List<MessageAttachmentDTO> MessageAttachmentDTOs { get; set; }
    //}

    //public class MessageAttachmentDTO
    //{
    //    public int MessageAttachmentId { get; set; }
    //    public long Filesize { get; set; }
    //    public string Name { get; set; }
    //    public byte[] Data { get; set; }
    //    public int? DataStorageId { get; set; }
    //    public bool IsUploadedAsImage { get; set; } // TODO: Not needed when we get rid of Images table
    //}

    /***     
     * This class will be used when the user saves or sends a mail
     * */
    public class MessageEditDTO
    {
        public int LicenseId { get; set; }
        public int MessageId { get; set; }
        public int MessageTextId { get; set; }
        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }
        public int? ActorCompanyId { get; set; }
        public int? ParentId { get; set; }
        public int? RoleId { get; set; }
        public int? SenderUserId { get; set; }
        public int? AbsenceRequestEmployeeId { get; set; }
        public int? AbsenceRequestEmployeeUserId { get; set; }
        public String Subject { get; set; }
        public String Text { get; set; }
        public String ShortText { get; set; }
        public String SenderName { get; set; }
        public String SenderEmail { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public DateTime? Created { get; set; }
        public bool MarkAsOutgoing { get; set; }
        public bool CopyToSMS { get; set; }
        public XEMailAnswerType AnswerType { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? ForwardDate { get; set; }
        public TermGroup_MessagePriority MessagePriority { get; set; }
        public TermGroup_MessageType MessageType { get; set; }
        public TermGroup_MessageDeliveryType MessageDeliveryType { get; set; }
        public TermGroup_MessageTextType MessageTextType { get; set; }
        public List<MessageRecipientDTO> Recievers { get; set; }
        [TsIgnore]
        public bool ForceSendToReceiver { get; set; }
        [TsIgnore]
        public bool ForceSendToEmailReceiver { get; set; }
        public List<MessageAttachmentDTO> Attachments { get; set; }

        public MessageEditDTO()
        {
            this.Recievers = new List<MessageRecipientDTO>();
            this.Attachments = new List<MessageAttachmentDTO>();
        }

        public List<int> RecipientsIDList()
        {
            return Recievers.Select(r => r.UserId).ToList();
        }
    }

    public class MessageGroupDTO
    {
        public int MessageGroupId { get; set; }
        public int LicenseId { get; set; }
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public bool NoUserValidation { get; set; }

        // Extensions
        public List<MessageGroupMemberDTO> GroupMembers { get; set; }
    }

    public class MessageGroupGridDTO
    {
        public int MessageGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
    }

    public class MessageGroupMemberDTO
    {
        public int MessageGroupId { get; set; }
        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }

        // Extensions
        public string Name { get; set; }
        public string Username { get; set; }
    }

    public class MessageRecipientDTO
    {
        public int RecipientId { get; set; }
        public int UserId { get; set; }

        public XEMailRecipientType Type { get; set; }
        public bool IsCC { get; set; }
        public bool SendCopyAsEmail { get; set; }
        public DateTime? ReadDate { get; set; }
        public XEMailAnswerType? AnswerType { get; set; }
        public DateTime? AnswerDate { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? ForwardDate { get; set; }

        // Extensions
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public int CreatedById { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? DeletedDate { get; set; }
        public TermGroup_EmployeeRequestTypeFlags EmployeeRequestTypeFlags { get; set; }
        public TermGroup_EmployeeRequestType EmployeeRequestType
        {
            get
            {
                return (TermGroup_EmployeeRequestType)((int)EmployeeRequestTypeFlags & (int)~TermGroup_EmployeeRequestTypeFlags.PartyDefined);
            }
            set
            {
                EmployeeRequestTypeFlags = (TermGroup_EmployeeRequestTypeFlags)value;
            }
        }

        public int ExternalId { get; set; } // Example: InvoiceId, AttestWorkFlowRowId for attest and signing
        public string SigneeKey { get; set; }
    }

    #endregion

    #region OpeningHours
    [TSInclude]
    public class OpeningHoursDTO
    {
        public int OpeningHoursId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StandardWeekDay { get; set; }
        public DateTime? SpecificDate { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    [TSInclude]
    public class OpeningHoursGridDTO
    {
        public int OpeningHoursId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StandardWeekDay { get; set; }
        public DateTime? SpecificDate { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
        public DateTime? FromDate { get; set; }
        public int State { get; set; }
    }

    #endregion

    #region Origin

    public class OriginDTO
    {
        public int OriginId { get; set; }
        public int? ActorCompanyId { get; set; }
        public int VoucherSeriesId { get; set; }

        public SoeOriginType Type { get; set; }
        public string Description { get; set; }
        public SoeOriginStatus Status { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    #endregion

    #region OriginUser
    [TSInclude]
    public class OriginUserDTO
    {
        public int OriginUserId { get; set; }
        public int OriginId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public bool Main { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public DateTime? ReadyDate { get; set; }


        // Extensions
        public string LoginName { get; set; }
        public string Name { get; set; }
    }
    [TSInclude]
    public class OriginUserSmallDTO
    {
        public int OriginUserId { get; set; }
        public int UserId { get; set; }
        public bool Main { get; set; }
        public string Name { get; set; }
        public bool IsReady { get; set; }
    }

    #endregion

    #region Payment

    #region Supplier payment

    public class SupplierPaymentGridDTO
    {
        public int SupplierPaymentId { get; set; }
        public int SupplierInvoiceId { get; set; }
        public string InvoiceSeqNr { get; set; }
        public int PaymentSeqNr { get; set; }
        public int SequenceNumber { get; set; }
        public int SequenceNumberRecordId { get; set; }
        public string InvoiceNr { get; set; }
        public string OCR { get; set; }
        public string PaymentStatus { get; set; }
        public int BillingTypeId { get; set; }
        public string BillingTypeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public decimal TotalAmountExVatCurrency { get; set; }
        public decimal VATAmount { get; set; }
        public decimal VATAmountCurrency { get; set; }
        public decimal PayAmount { get; set; }
        public decimal PayAmountCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public decimal VatRate { get; set; }
        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PayDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public int? AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string CurrentAttestUserName { get; set; }
        public int OwnerActorId { get; set; }
        public bool FullyPaid { get; set; }
        public string PaymentStatuses { get; set; }
        public int SysPaymentMethodId { get; set; }
        public int SysPaymentTypeId { get; set; }
        public string PaymentMethodName { get; set; }
        public int PaymentRowId { get; set; }
        public string PaymentNr { get; set; }
        public string PaymentNrString { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal PaymentAmountCurrency { get; set; }
        public decimal PaymentAmountDiff { get; set; }
        public decimal BankFee { get; set; }
        public DateTime? TimeDiscountDate { get; set; }
        public decimal? TimeDiscountPercent { get; set; }
        public bool BlockPayment { get; set; }
        public bool SupplierBlockPayment { get; set; }
        public int StatusIcon { get; set; }
        public bool MultipleDebtRows { get; set; }
        public bool HasVoucher { get; set; }
        public bool HasAttestComment { get; set; }
        public bool IsModified { get; set; }
        public Guid Guid { get; set; }
        public string BlockReason { get; set; }
        public string Description { get; set; } //Internal text...
    }

    #endregion

    #region CashPayment
    public class CashPaymentDTO
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal AmountCurrency { get; set; }
        public bool InUse { get; set; }
    }

    #endregion

    #endregion

    #region PaymentCondition

    [TSInclude]
    public class PaymentConditionDTO
    {
        public int PaymentConditionId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public int Days { get; set; }

        public int? DiscountDays { get; set; }
        public decimal? DiscountPercent { get; set; }

        public bool StartOfNextMonth { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    [TSInclude]
    public class PaymentConditionGridDTO
    {
        public int PaymentConditionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Days { get; set; }
    }

    #endregion

    #region PaymentExport

    public class PaymentExportDTO
    {
        public int ActorCompanyId { get; set; }
        public int PaymentExportId { get; set; }
        public TermGroup_SysPaymentType Type { get; set; }

        public DateTime ExportDate { get; set; }
        public string Filename { get; set; }
        public string CustomerNr { get; set; }
        public int? NumberOfPayments { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int TransferStatus { get; set; }
        public string TransferMsg { get; set; }

        // Extensions
        public List<PaymentRowDTO> PaymentRows { get; set; }
        public bool Foreign { get; set; }
        public int CancelledState { get; set; }
        public bool DetailVisible { get; set; }
        public bool DownloadVisible { get; set; }
    }

    #endregion

    #region PaymentImport
    [TSInclude]
    public class PaymentImportDTO
    {
        public int PaymentImportId { get; set; }
        public int? ActorCompanyId { get; set; }
        public int BatchId { get; set; }
        public TermGroup_SysPaymentType SysPaymentTypeId { get; set; }
        public int Type { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfPayments { get; set; }
        public DateTime ImportDate { get; set; }
        public string Filename { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public ImportPaymentType ImportType { get; set; }
        public TermGroup_ImportPaymentType ImportPaymentTypeTermId { get; set; }
        public string StatusName { get; set; }
        public int? TransferStatus { get; set; }

        public string PaymentLabel { get; set; }

        // Extension
        public string TypeName { get; set; }
        public string PaymentMethodName { get; set; }
    }

    #endregion

    #region PaymentInformation

    [TSInclude]
    public class PaymentInformationDTO
    {
        public int PaymentInformationId { get; set; }
        public int ActorId { get; set; }
        public int DefaultSysPaymentTypeId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<PaymentInformationRowDTO> Rows { get; set; }
    }

    #endregion

    #region PaymentInformationRow
    [TSInclude]
    public class PaymentInformationRowDTO
    {
        public int PaymentInformationRowId { get; set; }
        public int PaymentInformationId { get; set; }
        public int SysPaymentTypeId { get; set; }
        public string PaymentNr { get; set; }
        public bool Default { get; set; }
        public bool ShownInInvoice { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string BIC { get; set; }
        public string ClearingCode { get; set; }
        public string PaymentCode { get; set; }
        public int? PaymentMethodCode { get; set; }
        public int? PaymentForm { get; set; }
        public int? ChargeCode { get; set; }
        public int? IntermediaryCode { get; set; }
        public string CurrencyAccount { get; set; }
        public string PayerBankId { get; set; }
        public bool BankConnected { get; set; }
        public int? CurrencyId { get; set; }


        // Extensions
        public string SysPaymentTypeName { get; set; }
        public string PaymentMethodCodeName { get; set; }
        public string PaymentFormName { get; set; }
        public string ChargeCodeName { get; set; }
        public string IntermediaryCodeName { get; set; }
        public string CurrencyCode { get; set; }
        public string PaymentNrDisplay { get; set; }
        public TermGroup_BillingType BillingType { get; set; }

        public TermGroup_TimeSalaryPaymentExportBank GetPaymentExportBank()
        {
            if (this.IsPaymentTypeBIC() && (this.BIC == "NDEASESS" || this.BIC == "NDEANOKK"))
                return TermGroup_TimeSalaryPaymentExportBank.Nordea;
            else if (this.IsPaymentTypeBIC() && this.BIC == "FTSBSESS")
                return TermGroup_TimeSalaryPaymentExportBank.BNP;
            else if (this.IsPaymentTypeBIC() && this.BIC == "DNBASESX")
                return TermGroup_TimeSalaryPaymentExportBank.DNB;
            else
                return TermGroup_TimeSalaryPaymentExportBank.Handelsbanken;
        }

        public bool IsPaymentTypeBIC()
        {
            return this.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC;
        }
        public bool IsPaymentTypeBANK()
        {
            return this.SysPaymentTypeId == (int)TermGroup_SysPaymentType.Bank;
        }
        public bool IsValidForISO20022SalaryPayment()
        {
            if (!this.IsPaymentTypeBIC() && this.PaymentNr.Contains("/"))
                return true;
            if (this.IsPaymentTypeBIC())
            {
                if (this.BIC.IsNullOrEmpty() && this.PaymentNr.Contains("/"))
                    return true;
                if (!this.BIC.IsNullOrEmpty() && !this.PaymentNr.Contains("/"))
                    return true;
            }
            return false;
        }

        public string GetBICOrClearingNrForSalaryPayment()
        {
            if (this.IsPaymentTypeBIC() && !this.BIC.IsNullOrEmpty())
                return this.BIC.RemoveWhiteSpaceAndHyphen();

            string str;
            if (this.PaymentNr.Contains("/"))
                str = this.PaymentNr.Split('/')[0];
            else
                str = "wrong format";

            return str.RemoveWhiteSpaceAndHyphen();

        }

        public string GetIBANOrBBANForSalaryPayment()
        {
            if (this.IsPaymentTypeBIC() && !this.PaymentNr.Contains("/"))
                return this.PaymentNr.RemoveWhiteSpaceAndHyphen();

            string str;
            if (this.PaymentNr.Contains("/"))
                str = this.PaymentNr.Split('/')[1];
            else
                str = "wrong format";

            return str.RemoveWhiteSpaceAndHyphen();
        }

        public string GetCountryCodeFromBic()
        {
            return this.BIC.Substring(4, 2);
        }
        public string GetBankFromBic()
        {
            return this.BIC.Left(4);
        }
    }

    public class PaymentInformationDistributionRowDTO
    {
        public TermGroup_SysPaymentType SysPaymentTypeId { get; set; }
        public string PaymentNr { get; set; }
        public string BIC { get; set; }
        public bool Default { get; set; }
    }

    #endregion

    #region PaymentMethod
    [TSInclude]

    public class PaymentMethodSupplierGridDTO
    {
        public int PaymentMethodId { get; set; }
        public string Name { get; set; }
        public int SysPaymentMethodId { get; set; }
        public string AccountNr { get; set; }
        public string CustomerNr { get; set; }
        public int AccountId { get; set; }
        public string PayerBankId { get; set; }
        public string PaymentNr { get; set; }
        public string SysPaymentMethodName { get; set; }
        public string CurrencyCode { get; set; }
    }


    [TSInclude]
    public class PaymentMethodCustomerGridDTO
    {
        public int PaymentMethodId { get; set; }
        public string Name { get; set; }
        public int SysPaymentMethodId { get; set; }
        public string CustomerNr { get; set; }
        public bool UseInCashSales { get; set; }
        public int? PaymentInformationRowId { get; set; }
        public int AccountId { get; set; }
        public bool UseRoundingInCashSales { get; set; }
        public int? TransactionCode { get; set; }
        public string PaymentNr { get; set; }
        public string SysPaymentMethodName { get; set; }
        public string AccountNr { get; set; }
        public string CurrencyCode { get; set; }
    }

    [TSInclude]
    public class PaymentMethodDTO
    {
        public int PaymentMethodId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AccountId { get; set; }
        public int? PaymentInformationRowId { get; set; }
        public int SysPaymentMethodId { get; set; }

        public SoeOriginType PaymentType { get; set; }
        public string Name { get; set; }
        public string CustomerNr { get; set; }
        public bool UseInCashSales { get; set; }
        public bool UseRoundingInCashSales { get; set; }
        public string CurrencyCode { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public string PaymentNr { get; set; }
        public string PayerBankId { get; set; }
        public string SysPaymentMethodName { get; set; }
        public int? SysPaymentTypeId { get; set; }
        public PaymentInformationRowDTO PaymentInformationRow { get; set; }
        public string AccountNr { get; set; }
        public int? TransactionCode { get; set; }

    }
    [TSInclude]
    public class PaymentMethodSmallDTO
    {
        public int PaymentMethodId { get; set; }
        public TermGroup_SysPaymentMethod PaymentMethod { get; set; }
        public int AccountStdAccountId { get; set; }
    }

    #endregion

    #region PayrollControlFunctionOutcome
    public class PayrollControlFunctionOutcomeDTO
    {
        public int PayrollControlFunctionOutcomeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int EmployeeTimePeriodId { get; set; }
        public TermGroup_PayrollControlFunctionType Type { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public decimal? DecimalValue1 { get; set; }
        public decimal? DecimalValue2 { get; set; }
        public string Key { get; set; }
        public TermGroup_PayrollControlFunctionStatus Status { get; set; }
        public string StatusName { get; set; }
        public string Comment { get; set; }
        public bool hasChanges { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<PayrollControlFunctionOutcomeChangeDTO> Changes { get; set; }
        public bool IsStoppingPayrollWarning { get; set; }
    }

    #endregion

    #region PayrollControlFunctionOutcomeChange
    public class PayrollControlFunctionOutcomeChangeDTO
    {
        public int PayrollControlFunctionOutcomeChangeId { get; set; }
        public int PayrollControlFunctionOutcomeId { get; set; }
        public int EmployeeId { get; set; }
        public int EmployeeTimePeriodId { get; set; }
        public TermGroup_PayrollControlFunctionOutcomeChangeType Type { get; set; }
        public string TypeName { get; set; }
        public TermGroup_PayrollControlFunctionOutcomeChangeFieldType FieldType { get; set; }
        public string FieldTypeName { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }


        public List<PayrollControlFunctionOutcomeChangeDTO> Changes { get; set; }
    }

    #endregion

    #region PayrollProductDistributionRuleHead
    [TSInclude]
    public class PayrollProductDistributionRuleHeadDTO
    {
        public int PayrollProductDistributionRuleHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<PayrollProductDistributionRuleDTO> Rules { get; set; }
    }

    #endregion

    #region PayrollProductDistributionRule
    [TSInclude]
    public class PayrollProductDistributionRuleDTO
    {
        public int PayrollProductDistributionRuleId { get; set; }
        public int PayrollProductDistributionRuleHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int PayrollProductId { get; set; }
        public int Type { get; set; }
        public decimal Start { get; set; }
        public decimal Stop { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }
    [TSInclude]
    public class PayrollProductDistributionRuleHeadGridDTO
    {
        public int PayrollProductDistributionRuleHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region PayrollProductSettingLookup

    public class PayrollProductSettingLookupDTO
    {
        public int? PayrollGroupId { get; set; }
        public int PayrollProductId { get; set; }
        public SoeEntityType Entity { get; set; }
        public SettingDataType Type { get; set; }
        public string Value { get; set; }

        // extensions
        public SysExtraFieldType SysExtraFieldType { get; set; }
        public bool GetBoolValue()
        {
            if (Type == SettingDataType.Boolean && !string.IsNullOrEmpty(Value))
            {
                return Value == "1" || Value.ToLower() == "true";
            }
            return false;
        }

        public decimal? GetDecimalValue()
        {
            if (Type == SettingDataType.Decimal && !string.IsNullOrEmpty(Value))
            {
                if (decimal.TryParse(Value, out decimal result))
                {
                    return result;
                }
            }
            return null;
        }

        public int? GetIntValue()
        {
            if (Type == SettingDataType.Integer && !string.IsNullOrEmpty(Value))
            {
                if (int.TryParse(Value, out int result))
                {
                    return result;
                }
            }
            return null;
        }

        public string GetStringValue()
        {
            if (Type == SettingDataType.String)
            {
                return Value;
            }
            return null;
        }

        public DateTime? GetDateValue()
        {
            if (Type == SettingDataType.Date && !string.IsNullOrEmpty(Value))
            {
                if (DateTime.TryParse(Value, out DateTime result))
                {
                    return result;
                }
            }
            return null;
        }
    }

    #endregion

    #region PayrollGroup

    public class PayrollGroupBaseDTO
    {
        public int PayrollGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int? TimePeriodHeadId { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class PayrollGroupDTO : PayrollGroupBaseDTO
    {
        public int? OneTimeTaxFormulaId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Relations
        public List<PayrollGroupPriceTypeDTO> PriceTypes { get; set; }
        public List<PayrollGroupPriceFormulaDTO> PriceFormulas { get; set; }
        public List<PayrollGroupAccountsDTO> Accounts;
        [TsIgnore]
        public List<PayrollGroupSettingDTO> Settings { get; set; }
        public List<PayrollGroupReportDTO> Reports { get; set; }
        public List<int> ReportIds { get; set; }
        public List<PayrollGroupVacationGroupDTO> Vacations { get; set; }
        public List<PayrollGroupPayrollProductDTO> PayrollProducts { get; set; }
        public TimePeriodHeadDTO TimePeriodHead { get; set; }

        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
    }

    public class PayrollGroupSmallDTO
    {
        public int PayrollGroupId { get; set; }
        public string Name { get; set; }

        // Relations
        public List<PayrollGroupPriceTypeDTO> PriceTypes { get; set; }
        public List<PriceTypeLevelDTO> PriceTypeLevels { get; set; }
    }

    public class PayrollGroupGridDTO
    {
        public int PayrollGroupId { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }

        public string TimePeriodHeadName { get; set; }
    }

    #endregion

    #region PayrollGroupAccounts

    public class PayrollGroupAccountsDTO
    {
        #region Propertys

        public decimal? FromInterval { get; set; }
        public decimal? ToInterval { get; set; }

        public decimal? EmploymentTaxPercent { get; set; }
        public int? EmploymentTaxAccountId { get; set; }
        public string EmploymentTaxAccountNr { get; set; }
        public string EmploymentTaxAccountName { get; set; }

        public decimal? PayrollTaxPercent { get; set; }
        public int? PayrollTaxAccountId { get; set; }
        public string PayrollTaxAccountNr { get; set; }
        public string PayrollTaxAccountName { get; set; }

        public decimal? OwnSupplementChargePercent { get; set; }
        public int? OwnSupplementChargeAccountId { get; set; }
        public string OwnSupplementChargeAccountNr { get; set; }
        public string OwnSupplementChargeAccountName { get; set; }

        public bool IsModified { get; set; }

        #endregion
    }

    #endregion

    #region PayrollGroupPayrollProduct

    public class PayrollGroupPayrollProductDTO
    {
        public int PayrollGroupPayrollProductId { get; set; }
        public int PayrollGroupId { get; set; }
        public int ProductId { get; set; }
        public bool Distribute { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string ProductName { get; set; }
        public string ProductNr { get; set; }
    }

    #endregion

    #region PayrollGroupPriceFormula

    public class PayrollGroupPriceFormulaDTO
    {
        #region Propertys

        public int PayrollGroupPriceFormulaId { get; set; }
        public int PayrollGroupId { get; set; }
        public int PayrollPriceFormulaId { get; set; }
        public bool ShowOnEmployee { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Extensions
        public string FormulaName { get; set; }
        public string FormulaPlain { get; set; }
        public string FormulaNames { get; set; }
        public string FormulaExtracted { get; set; }
        public decimal Result { get; set; }

        #endregion
    }

    #endregion

    #region PayrollGroupPriceType

    public class PayrollGroupPriceTypeDTO
    {
        #region Propertys

        public int PayrollGroupPriceTypeId { get; set; }
        public int PayrollGroupId { get; set; }
        public int PayrollPriceTypeId { get; set; }
        public int Sort { get; set; }
        public decimal? CurrentAmount { get { return GetAmount(null); } }
        public decimal? PayrollPriceTypeCurrentAmount { get; set; }
        public bool ShowOnEmployee { get; set; }
        public bool ReadOnlyOnEmployee { get; set; }
        public List<PayrollGroupPriceTypePeriodDTO> Periods { get; set; }
        public PayrollPriceTypeDTO PayrollPriceType { get; set; }

        public int? PayrollLevelId { get; set; }

        // Extensions
        public string PriceTypeCode { get; set; }
        public string PriceTypeName { get; set; }
        public string PayrollLevelName { get; set; }

        public PriceTypeLevelDTO PriceTypeLevel { get; set; }

        #endregion

        #region Public methods

        public PayrollGroupPriceTypePeriodDTO GetPeriod(DateTime? date = null)
        {
            if (this.Periods == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return this.Periods.Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public decimal? GetAmount(DateTime? date = null)
        {
            PayrollGroupPriceTypePeriodDTO period = GetPeriod(date);
            return period != null ? period.Amount : PayrollPriceTypeCurrentAmount;
        }

        #endregion
    }

    public class PayrollGroupPriceTypePeriodDTO
    {
        public int PayrollGroupPriceTypePeriodId { get; set; }
        public int PayrollGroupPriceTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion

    #region PayrollGroupReport

    public class PayrollGroupReportDTO
    {
        public int PayrollGroupReportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int PayrollGroupId { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public int ReportNr { get; set; }
        public string ReportDescription { get; set; }
        public int SysReportTemplateTypeId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int EmployeeTemplateId { get; set; }

        public string ReportNameDesc
        {
            get
            {
                return this.ReportName + (string.IsNullOrEmpty(this.ReportDescription) ? string.Empty : " (" + this.ReportDescription + ")");
            }
        }

    }

    #endregion

    #region PayrollGroupSetting

    public class PayrollGroupSettingDTO : SettingsDTO<PayrollGroupSettingType, SettingDataType>
    {
        public int PayrollGroupId { get; set; }
    }

    #endregion

    #region PayrollGroupVacationGroup

    public class PayrollGroupVacationGroupDTO
    {
        #region Propertys

        public int PayrollGroupVacationGroupId { get; set; }
        public int PayrollGroupId { get; set; }
        public int VacationGroupId { get; set; }
        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public TermGroup_VacationGroupCalculationType CalculationType { get; set; }
        public TermGroup_VacationGroupVacationHandleRule VacationHandleRule { get; set; }
        public TermGroup_VacationGroupVacationDaysHandleRule VacationDaysHandleRule { get; set; }

        #endregion
    }

    #endregion

    #region PayrollImportHead

    public class PayrollImportHeadDTO
    {
        public PayrollImportHeadDTO()
        {
            Employees = new List<PayrollImportEmployeeDTO>();
        }
        public int PayrollImportHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime? PaymentDate { get; set; }
        public TermGroup_PayrollImportHeadType Type { get; set; }
        public string TypeName { get; set; }
        public TermGroup_PayrollImportHeadFileType FileType { get; set; }
        public string FileTypeName { get; set; }
        public byte[] File { get; set; }
        public string Comment { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string Checksum { get; set; }

        // Relations
        public List<PayrollImportEmployeeDTO> Employees { get; set; }

        //Extensions
        public string Name { get; set; }
        public int? NrOfEmployees { get; set; }
        public string ErrorMessage { get; set; }
        public TermGroup_PayrollImportHeadStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    #endregion

    #region PayrollImportEmployee

    public class PayrollImportEmployeeDTO
    {
        public PayrollImportEmployeeDTO()
        {
            Schedule = new List<PayrollImportEmployeeScheduleDTO>();
            Transactions = new List<PayrollImportEmployeeTransactionDTO>();
        }
        public int PayrollImportEmployeeId { get; set; }
        public int PayrollImportHeadId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeInfo { get; set; }
        public List<PayrollImportEmployeeScheduleDTO> Schedule { get; set; }
        public List<PayrollImportEmployeeTransactionDTO> Transactions { get; set; }
        public SoeEntityState State { get; set; }
        public TermGroup_PayrollImportEmployeeStatus Status { get; set; }
        public string StatusName { get; set; }

        //Schedule
        public int ScheduleRowCount { get; set; }
        public TimeSpan ScheduleQuantity { get; set; }
        public TimeSpan ScheduleBreakQuantity { get; set; }

        //Transaction
        public int TransactionRowCount { get; set; }
        public decimal TransactionQuantity { get; set; }
        public decimal TransactionAmount { get; set; }


        // Methods
        public DateTime? FirstDateSchedule()
        {
            if (!this.Schedule.IsNullOrEmpty())
                return this.Schedule.Where(w => w.State == SoeEntityState.Active).OrderBy(o => o.Date).FirstOrDefault()?.Date;

            return (DateTime?)null;
        }

        public DateTime? LastDateSchedule()
        {
            if (!this.Schedule.IsNullOrEmpty())
                return this.Schedule.Where(w => w.State == SoeEntityState.Active).OrderBy(o => o.Date).LastOrDefault()?.Date;

            return (DateTime?)null;
        }

        public DateTime? FirstDateTransactions()
        {
            if (!this.Transactions.IsNullOrEmpty())
                return this.Transactions.Where(w => w.State == SoeEntityState.Active).OrderBy(o => o.Date).FirstOrDefault()?.Date;

            return (DateTime?)null;
        }

        public DateTime? LastDateTransactions()
        {
            if (!this.Transactions.IsNullOrEmpty())
                return this.Transactions.Where(w => w.State == SoeEntityState.Active).OrderBy(o => o.Date).LastOrDefault()?.Date;

            return (DateTime?)null;
        }
    }

    #endregion

    #region PayrollImportEmployeeSchedule

    public class PayrollImportEmployeeScheduleDTO
    {
        public int PayrollImportEmployeeScheduleId { get; set; }
        public int PayrollImportEmployeeId { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsBreak { get; set; }
        public decimal Quantity { get; set; }
        public string ErrorMessage { get; set; }
        public string StatusName { get; set; }
        public TermGroup_PayrollImportEmployeeScheduleStatus Status { get; set; }
        public SoeEntityState State { get; set; }

        public TimeScheduleTemplateBlockDTO CreateScheduleBlock(int employeeId, int timeCodeId, Guid link)
        {
            var dto = new TimeScheduleTemplateBlockDTO()
            {
                ActualDate = Date,
                Date = Date,
                IsBreak = IsBreak,
                StartTime = StartTime,
                StopTime = StopTime,
                TimeScheduleTemplatePeriodId = 0,
                EmployeeId = employeeId,
                TimeCodeId = timeCodeId,
                Link = link
            };

            return dto;
        }
    }

    #endregion

    #region PayrollImportEmployeeTransaction

    public class PayrollImportEmployeeTransactionDTO
    {
        public PayrollImportEmployeeTransactionDTO()
        {
            AccountInternals = new List<PayrollImportEmployeeTransactionAccountInternalDTO>();
            PayrollImportEmployeeTransactionLinks = new List<PayrollImportEmployeeTransactionLinkDTO>();
        }
        public int PayrollImportEmployeeTransactionId { get; set; }
        public int PayrollImportEmployeeId { get; set; }
        public int? PayrollProductId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? TimeCodeAdditionDeductionId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; }
        public string AccountCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Note { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string StatusName { get; set; }
        public TermGroup_PayrollImportEmployeeTransactionType Type { get; set; }
        public TermGroup_PayrollImportEmployeeTransactionStatus Status { get; set; }
        public SoeEntityState State { get; set; }

        public int? AccountStdId { get; set; }
        public int? AccountStdDimNr { get; set; }
        public string AccountStdNr { get; set; }
        public string AccountStdName { get; set; }
        public List<PayrollImportEmployeeTransactionAccountInternalDTO> AccountInternals { get; set; }

        public List<PayrollImportEmployeeTransactionLinkDTO> PayrollImportEmployeeTransactionLinks { get; set; }
    }

    public class PayrollImportEmployeeTransactionLinkDTO
    {
        public int PayrollImportEmployeeTransactionLinkId { get; set; }
        public int PayrollImportEmployeeTransactionId { get; set; }
        public int? TimePayrollTransactionId { get; set; }
        public int? TimeBlockId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public decimal Quantity { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
    }

    #endregion

    #region PayrollImportEmployeeTransactionAccountInternal

    public class PayrollImportEmployeeTransactionAccountInternalDTO
    {
        public int PayrollImportEmployeeTransactionAccountInternalId { get; set; }
        public int PayrollImportEmployeeTransactionId { get; set; }
        public int AccountSIEDimNr { get; set; }
        public string AccountCode { get; set; }

        public int? AccountId { get; set; }
        public int? AccountDimNr { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region PayrollLevel

    [TSInclude]
    public class PayrollLevelDTO
    {
        public int PayrollLevelId { get; set; }
        public int ActorCompanyId { get; set; }

        public string ExternalCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public SoeEntityState State { get; set; }


        public string NameAndDesc
        {
            get
            {
                if (this.PayrollLevelId != 0)
                    return (this.Name + "(" + this.Description + ")");
                else
                    return "";
            }
        }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    [TSInclude]
    public class PayrollLevelGridDTO
    {
        public int PayrollLevelId { get; set; }

        public string ExternalCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public SoeEntityState State { get; set; }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    #endregion

    #region PayrollPriceFormula

    public class PayrollPriceFormulaDTO
    {
        public int PayrollPriceFormulaId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Formula { get; set; }
        public string FormulaPlain { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    public class PayrollPriceFormulaSmallDTO
    {
        public int PayrollPriceFormulaId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PayrollPriceFormulaResultDTO
    {
        public int? PayrollPriceFormulaId { get; set; }
        public int? PayrollPriceTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Formula { get; set; }             // As stored in field Formula (with ID's)
        public string FormulaPlain { get; set; }        // As stored in field FormulaPlain (with codes)
        public string FormulaExtracted { get; set; }    // Formula extracted with values
        public string FormulaNames { get; set; }        // FormulaPlain with codes replaced with names
        public string FormulaOrigin { get; set; }       // FormulaPlain with origin (what entity did the value come from)

        public PayrollPriceFormulaResultDTO Clone()
        {
            return new PayrollPriceFormulaResultDTO()
            {
                PayrollPriceFormulaId = this.PayrollPriceFormulaId,
                PayrollPriceTypeId = this.PayrollPriceTypeId,
                Amount = this.Amount,
                Formula = this.Formula,
                FormulaPlain = this.FormulaPlain,
                FormulaExtracted = this.FormulaExtracted,
                FormulaNames = this.FormulaNames,
                FormulaOrigin = this.FormulaOrigin,
            };
        }
    }

    #endregion

    #region PayrollPriceType

    [TSInclude]
    public class PayrollPriceTypeDTO
    {
        public int PayrollPriceTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int ConditionEmployeedMonths { get; set; }
        public int ConditionExperienceMonths { get; set; }
        public int ConditionAgeYears { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }

        // Relations
        public List<PayrollPriceTypePeriodDTO> Periods { get; set; }

        public PayrollPriceTypePeriodDTO GetPeriod(DateTime? date = null)
        {
            if (this.Periods == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return this.Periods.Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public decimal? GetAmount(DateTime? date = null)
        {
            PayrollPriceTypePeriodDTO period = GetPeriod(date);
            return period != null ? period.Amount : (decimal?)null;
        }
    }

    [TSInclude]
    public class PayrollPriceTypeGridDTO
    {
        public int PayrollPriceTypeId { get; set; }
        public string TypeName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class PayrollPriceTypeSmallDTO
    {
        public int PayrollPriceTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    [TSInclude]
    public class PayrollPriceTypePeriodDTO
    {
        public int PayrollPriceTypePeriodId { get; set; }
        public int PayrollPriceTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion

    #region PayrollReview

    public class PayrollReviewHeadDTO
    {
        public int PayrollReviewHeadId { get; set; }
        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public TermGroup_PayrollReviewStatus Status { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<int> PayrollGroupIds { get; set; }
        public List<int> PayrollPriceTypeIds { get; set; }
        public List<int?> PayrollLevelIds { get; set; }
        public List<PayrollReviewRowDTO> Rows { get; set; }

        // Extensions
        public string StatusName { get; set; }
        public string PayrollGroupNames { get; set; }
        public string PayrollLevelNames { get; set; }
        public string PayrollPriceTypeNames { get; set; }
    }

    public class PayrollReviewRowDTO
    {
        public int PayrollReviewRowId { get; set; }
        public int PayrollReviewHeadId { get; set; }
        public int EmployeeId { get; set; }
        public int? PayrollGroupId { get; set; }
        public int? PayrollPriceTypeId { get; set; }
        public int? PayrollLevelId { get; set; }
        public decimal? PayrollGroupAmount { get; set; }
        public decimal? EmploymentAmount { get; set; }
        public decimal Adjustment { get; set; }
        public decimal Amount { get; set; }

        // Extensions
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string PayrollGroupName { get; set; }
        public string PayrollPriceTypeName { get; set; }
        public string PayrollLevelName { get; set; }
        public bool ReadOnly { get; set; }
        public bool IsModified { get; set; }
        public string WarningMessage { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PayrollReviewEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public int PayrollGroupId { get; set; }
        public int PayrollPriceTypeId { get; set; }
        public int? PayrollLevelId { get; set; }
        public decimal PayrollGroupAmount { get; set; }
        public decimal EmploymentAmount { get; set; }
        public bool ReadOnly { get; set; }
        public List<PayrollReviewSelectableLevelDTO> SelectableLevels { get; set; }
    }

    public class PayrollReviewSelectableLevelDTO
    {
        public int PayrollLevelId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime FromDate { get; set; }
    }

    #endregion

    #region PayrollPeriodChangeRow

    public class PayrollPeriodChangeRowDTO
    {
        public int PayrollPeriodChangeRowId { get; set; }
        public int PayrollPeriodChangeHeadId { get; set; }
        public PayrollPeriodChangeRowField Field { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public decimal ChangeDecimalValue { get; set; }

        public PayrollPeriodChangeRowDTO()
        {

        }

        public PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField field, string fromValue, string toValue, decimal changeValue)
        {
            this.Field = field;
            this.FromValue = fromValue;
            this.ToValue = toValue;
            this.ChangeDecimalValue = changeValue;
        }
    }

    #endregion

    #region PayrollStartValueHead

    public class PayrollStartValueHeadDTO
    {
        public int PayrollStartValueHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string ImportedFrom { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        // Extensions
        public List<PayrollStartValueRowDTO> Rows { get; set; }
    }

    #endregion

    #region PayrollStartValueRow

    public class PayrollStartValueRowDTO
    {
        public int PayrollStartValueRowId { get; set; }
        public int PayrollStartValueHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public int ProductId { get; set; }
        public string Appellation { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductNrAndName
        {
            get
            {
                return $"{this.ProductNr} {this.ProductName}";
            }
        }
        public int? SysPayrollStartValueId { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel1 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel2 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel3 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel4 { get; set; }
        public int? ScheduleTimeMinutes { get; set; }
        public int? AbsenceTimeMinutes { get; set; }
        public SoeEntityState State { get; set; }

        //TimePayrollTransaction
        public int TimePayrollTransactionId { get; set; }
        public int TransactionProductId { get; set; }
        public string TransactionProductNr { get; set; }
        public string TransactionProductName { get; set; }
        public string TransactionProductNrAndName
        {
            get
            {
                return $"{this.TransactionProductNr} {this.TransactionProductName}";
            }
        }
        public decimal TransactionQuantity { get; set; }
        public decimal? TransactionAmount { get; set; }
        public decimal TransactionUnitPrice { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionComment { get; set; }
        public TermGroup_SysPayrollType TransactionLevel1 { get; set; }
        public TermGroup_SysPayrollType TransactionLevel2 { get; set; }
        public TermGroup_SysPayrollType TransactionLevel3 { get; set; }
        public TermGroup_SysPayrollType TransactionLevel4 { get; set; }
        public bool DoCreateTransaction { get; set; }
    }

    public class PayrollStartValueRowIODTO
    {
        public string EmployeeNr { get; set; }
        public string Appellation { get; set; }
        public string ProductNr { get; set; }
        public int? SysPayrollStartValueId { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public int? ScheduleTimeMinutes { get; set; }
        public int? AbsenceTimeMinutes { get; set; }
    }

    #endregion

    #region PlanningPeriod

    public class PlanningPeriodHead
    {
        public int TimePeriodHeadId { get; set; }
        public string Name { get; set; }
        public int? ChildId { get; set; }
        public string ChildName { get; set; }
        public List<PlanningPeriod> ParentPeriods { get; set; }
        public List<PlanningPeriod> ChildPeriods { get; set; }
    }

    public class PlanningPeriod
    {
        public int TimePeriodId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
    }

    #endregion

    #region Position
    [TSInclude]
    public class PositionDTO
    {
        public int PositionId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? SysPositionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAndCode
        {
            get
            {
                return $"{this.Name} ({this.Code})";
            }
        }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<PositionSkillDTO> PositionSkills { get; set; }
    }

    [TSInclude]
    public class PositionGridDTO
    {
        public int PositionId { get; set; }
        public int? SysPositionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region PositionSkill
    [TSInclude]
    public class PositionSkillDTO
    {
        public int PositionSkillId { get; set; }
        public int PositionId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }

        // Extensions
        public string PositionName { get; set; }
        public double SkillLevelStars { get; set; }
        public bool SkillLevelUnreached { get; set; }
        public string SkillName { get; set; }
        public string SkillTypeName { get; set; }
        public bool Missing { get; set; }
    }

    #endregion

    #region PriceBasedMarkup

    [TSInclude]
    public class PriceBasedMarkupDTO
    {
        public int PriceBasedMarkupId { get; set; }
        public int? PriceListTypeId { get; set; }
        public string PriceListName { get; set; }

        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public decimal MarkupPercent { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }


    #endregion

    #region PriceRule

    public class PriceRuleDTO
    {
        public int RuleId { get; set; }
        public int PriceListTypeId { get; set; }
        public int? CompanyWholesellerPriceListId { get; set; }
        public int? PriceListImportedHeadId { get; set; }

        public int? LRuleId { get; set; }
        public int? LValueType { get; set; }
        public decimal? LValue { get; set; }
        public int? RRuleId { get; set; }
        public int? RValueType { get; set; }
        public decimal? RValue { get; set; }
        public int OperatorType { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public bool UseNetPrice { get; set; }

        // Extensions
        public PriceRuleDTO LRule { get; set; }
        public PriceRuleDTO RRule { get; set; }

        public int lExampleType { get; set; }
        public int rExampleType { get; set; }
    }

    #endregion

    #region Product

    #region PayrollProduct

    public class PayrollProductDTO : ProductDTO, IPayrollProduct, IPayrollType
    {
        public int? SysPayrollProductId { get; set; }

        public int PayrollType { get; set; }
        public int ResultType { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public decimal Factor { get; set; }
        public string ShortName { get; set; }
        public string ExternalNumber { get; set; }
        public bool AverageCalculated { get; set; }
        public bool DontUseFixedAccounting { get; set; }
        public bool ExcludeInWorkTimeSummary { get; set; }
        public bool Export { get; set; }
        public bool IncludeAmountInExport { get; set; }
        public bool Payed { get; set; }
        public bool UseInPayroll { get; set; }

        public bool IsAbsence
        {
            get
            {
                return PayrollRulesUtil.IsAbsence(this.SysPayrollTypeLevel1, this.SysPayrollTypeLevel2, this.SysPayrollTypeLevel3, this.SysPayrollTypeLevel4);
            }
        }

        // Relations
        public List<PayrollProductSettingDTO> Settings { get; set; }
    }

    [TSInclude]
    public class PayrollProductGridDTO
    {
        public int ProductId { get; set; }
        public string Number { get; set; }
        public string NumberSort { get; set; }
        public string ExternalNumber { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int SysPayrollTypeLevel1 { get; set; }
        public string SysPayrollTypeLevel1Name { get; set; }
        public int SysPayrollTypeLevel2 { get; set; }
        public string SysPayrollTypeLevel2Name { get; set; }
        public int SysPayrollTypeLevel3 { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public int SysPayrollTypeLevel4 { get; set; }
        public string SysPayrollTypeLevel4Name { get; set; }
        public decimal Factor { get; set; }
        public TermGroup_PayrollResultType ResultType { get; set; }

        public bool Payed { get; set; }
        public bool ExcludeInWorkTimeSummary { get; set; }
        public bool AverageCalculated { get; set; }
        public bool Export { get; set; }
        public bool UseInPayroll { get; set; }
        public bool IncludeAmountInExport { get; set; }

        // Extensions
        public TermGroup_PayrollType PayrollType { get; set; }
        public string SysPayrollTypeName { get; set; }
        public string ResultTypeText { get; set; }

        public SoeEntityState State { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }

        public bool IsAbsence
        {
            get
            {
                return PayrollRulesUtil.IsAbsence(this.SysPayrollTypeLevel1, this.SysPayrollTypeLevel2, this.SysPayrollTypeLevel3, this.SysPayrollTypeLevel4);
            }
        }
    }

    #region PayrollProductPriceFormula

    public class PayrollProductPriceFormulaDTO
    {
        public int PayrollProductPriceFormulaId { get; set; }
        public int PayrollProductSettingId { get; set; }
        public int PayrollPriceFormulaId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Extensions
        public string FormulaName { get; set; }
    }

    #endregion

    #region PayrollProductPriceType

    public class PayrollProductPriceTypeDTO
    {
        public int PayrollProductPriceTypeId { get; set; }
        public int PayrollProductSettingId { get; set; }
        public int PayrollPriceTypeId { get; set; }

        // Relations
        public List<PayrollProductPriceTypePeriodDTO> Periods { get; set; }
        public List<PayrollPriceTypePeriodDTO> PriceTypePeriods { get; set; }

        // Extensions
        public string PriceTypeName { get; set; }

        public PayrollProductPriceTypePeriodDTO GetPeriod(DateTime? date)
        {
            if (this.Periods == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return this.Periods.Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public PayrollPriceTypePeriodDTO GetPriceTypePeriod(DateTime? date)
        {
            if (this.PriceTypePeriods == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return this.PriceTypePeriods.Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public decimal? GetAmount(DateTime? date)
        {
            if (!Periods.IsNullOrEmpty())
            {
                var period = GetPeriod(date);

                if (period.Amount.HasValue && period.Amount.Value != 0)
                    return period.Amount;

                var priceTypePeriod = GetPriceTypePeriod(date);

                if (priceTypePeriod != null)
                    return priceTypePeriod.Amount;

                return null;
            }

            return null;
        }
    }

    public class PayrollProductPriceTypePeriodDTO
    {
        public int PayrollProductPriceTypePeriodId { get; set; }
        public int PayrollProductPriceTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal? Amount { get; set; }
    }

    #endregion

    public class PayrollProductPriceTypeAndFormulaDTO
    {
        public int? PayrollProductPriceTypeId { get; set; }
        public int? PayrollPriceTypeId { get; set; }
        public int? PayrollProductPriceTypePeriodId { get; set; }
        public int? PayrollProductPriceFormulaId { get; set; }
        public int? PayrollPriceFormulaId { get; set; }
        public string Name { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal? Amount { get; set; }
    }

    public class PayrollPriceTypeAndFormulaDTO
    {
        public int ID { get; set; }
        public int? PayrollPriceTypeId { get; set; }
        public int? PayrollPriceFormulaId { get; set; }
        public string Name { get; set; }
    }

    #region PayrollProductSetting

    public class PayrollProductSettingDTO : IPayrollProductSetting
    {
        public int PayrollProductSettingId { get; set; }
        public int ProductId { get; set; }
        public int? ChildProductId { get; set; }
        public int? PayrollGroupId { get; set; }

        public int QuantityRoundingMinutes { get; set; }
        public int QuantityRoundingType { get; set; }
        public int CentRoundingType { get; set; }
        public int CentRoundingLevel { get; set; }
        public int TaxCalculationType { get; set; }
        public int TimeUnit { get; set; }
        public int PensionCompany { get; set; }
        public string AccountingPrio { get; set; }
        public bool CalculateSicknessSalary { get; set; }
        public bool CalculateSupplementCharge { get; set; }
        public bool DontPrintOnSalarySpecificationWhenZeroAmount { get; set; }
        public bool DontIncludeInRetroactivePayroll { get; set; }
        public bool DontIncludeInAbsenceCost { get; set; }
        public bool PrintOnSalarySpecification { get; set; }
        public bool PrintDate { get; set; }
        public bool UnionFeePromoted { get; set; }
        public bool VacationSalaryPromoted { get; set; }
        public bool WorkingTimePromoted { get; set; }

        // Relations
        public List<PayrollProductPriceTypeDTO> PriceTypes { get; set; }
        public List<PayrollProductPriceFormulaDTO> PriceFormulas { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> PurchaseAccounts { get; set; }
        [TsIgnore]
        public List<AccountingSettingDTO> AccountSettings { get; set; }
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }
        [TsIgnore]
        public List<CompanyCategoryRecordDTO> CategoryRecords { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFields { get; set; }

        // Extensions
        public bool IsReadOnly { get; set; }
        public string PayrollGroupName { get; set; }
        public bool IsSelected { get; set; }

        public List<PayrollProductPriceTypeDTO> ClonePriceTypes()
        {
            List<PayrollProductPriceTypeDTO> clonedPriceTypes = new List<PayrollProductPriceTypeDTO>();

            if (this.PriceTypes != null && this.PriceTypes.Count > 0)
            {
                foreach (var priceType in this.PriceTypes)
                {
                    PayrollProductPriceTypeDTO clonedPriceType = new PayrollProductPriceTypeDTO()
                    {
                        PayrollProductSettingId = priceType.PayrollProductSettingId,
                        PayrollPriceTypeId = priceType.PayrollPriceTypeId,
                        PriceTypeName = priceType.PriceTypeName
                    };

                    if (priceType.Periods != null && priceType.Periods.Count > 0)
                    {
                        clonedPriceType.Periods = new List<PayrollProductPriceTypePeriodDTO>();
                        foreach (var period in priceType.Periods)
                        {
                            PayrollProductPriceTypePeriodDTO clonedPeriod = new PayrollProductPriceTypePeriodDTO()
                            {
                                FromDate = period.FromDate,
                                Amount = period.Amount
                            };
                            clonedPriceType.Periods.Add(clonedPeriod);
                        }
                    }
                    clonedPriceTypes.Add(clonedPriceType);
                }
            }

            return clonedPriceTypes;
        }

        public List<PayrollProductPriceFormulaDTO> ClonePriceFormulas()
        {
            List<PayrollProductPriceFormulaDTO> clonedPriceFormulas = new List<PayrollProductPriceFormulaDTO>();

            if (this.PriceFormulas != null && this.PriceFormulas.Count > 0)
            {
                foreach (var priceFormula in this.PriceFormulas)
                {
                    PayrollProductPriceFormulaDTO clonedPriceFormula = new PayrollProductPriceFormulaDTO()
                    {
                        PayrollProductSettingId = priceFormula.PayrollProductSettingId,
                        PayrollPriceFormulaId = priceFormula.PayrollPriceFormulaId,
                        FromDate = priceFormula.FromDate,
                        ToDate = priceFormula.ToDate,
                        FormulaName = priceFormula.FormulaName
                    };
                    clonedPriceFormulas.Add(clonedPriceFormula);
                }
            }

            return clonedPriceFormulas;
        }

        public Dictionary<int, AccountSmallDTO> ClonePurchaseAccounts()
        {
            Dictionary<int, AccountSmallDTO> clonedAccounts = new Dictionary<int, AccountSmallDTO>();

            if (this.PurchaseAccounts != null && this.PurchaseAccounts.Count > 0)
            {
                foreach (var account in this.PurchaseAccounts)
                {
                    AccountSmallDTO clonedAccount = new AccountSmallDTO()
                    {
                        AccountId = account.Value.AccountId,
                        Name = account.Value.Name,
                        Number = account.Value.Number,
                        Percent = account.Value.Percent
                    };
                    clonedAccounts.Add(account.Key, clonedAccount);
                }
            }

            return clonedAccounts;
        }

        public List<CompanyCategoryRecordDTO> CloneCategoryRecords()
        {
            List<CompanyCategoryRecordDTO> clonedCategories = new List<CompanyCategoryRecordDTO>();

            if (this.CategoryRecords != null && this.CategoryRecords.Count > 0)
            {
                foreach (var categoryRecord in this.CategoryRecords)
                {
                    CompanyCategoryRecordDTO clonedCategory = new CompanyCategoryRecordDTO()
                    {
                        ActorCompanyId = categoryRecord.ActorCompanyId,
                        CategoryId = categoryRecord.CategoryId,
                        Entity = categoryRecord.Entity,
                        RecordId = categoryRecord.RecordId,
                        Default = categoryRecord.Default,
                        DateFrom = categoryRecord.DateFrom,
                        DateTo = categoryRecord.DateTo,
                        IsExecutive = categoryRecord.IsExecutive,
                        Category = categoryRecord.Category
                    };
                    clonedCategories.Add(clonedCategory);
                }
            }

            return clonedCategories;
        }

        public void UpdateAccounting(List<AccountingSettingDTO> accountSettings)
        {
            if (accountSettings == null)
                return;

            this.AccountSettings = AccountingSettingDTO.Copy(accountSettings).ToList();
            if (this.AccountSettings == null)
                return;

            AccountingSettingDTO accountingSettingStd = this.AccountSettings.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);
            List<AccountingSettingDTO> accountingSettingInternals = this.AccountSettings.Where(a => a.DimNr != Constants.ACCOUNTDIM_STANDARD).ToList();

            foreach (ProductAccountType accountStdType in EnumUtility.GetPayrollProductAccountTypes())
            {
                #region AccountStd

                if (accountStdType == ProductAccountType.Purchase)
                    this.SetPurchaseAccount(accountingSettingStd, Constants.ACCOUNTDIM_STANDARD);

                #endregion

                #region AccountInternal

                foreach (AccountingSettingDTO accountingSettingInternal in accountingSettingInternals)
                {
                    if (accountStdType == ProductAccountType.Purchase)
                        this.SetPurchaseAccount(accountingSettingInternal, accountingSettingInternal.DimNr);
                }

                #endregion
            }
        }

        private void SetPurchaseAccount(AccountingSettingDTO accountSetting, int dimNr)
        {
            if (accountSetting == null)
                return;

            AccountSmallDTO account = null;
            if (this.PurchaseAccounts == null)
                this.PurchaseAccounts = new Dictionary<int, AccountSmallDTO>();
            if (this.PurchaseAccounts.ContainsKey(dimNr))
            {
                account = this.PurchaseAccounts[dimNr];
            }
            else
            {
                //Dont add null accounts if not existed previously
                if (accountSetting.Account1Id == 0)
                    return;

                account = new AccountSmallDTO();
            }

            account.AccountId = accountSetting.Account1Id;
            account.Number = accountSetting.Account1Nr;
            account.Name = accountSetting.Account1Name;
            this.PurchaseAccounts[dimNr] = account;
        }
    }

    #endregion

    #endregion

    #region CustomerProduct

    [TSInclude]
    public class CustomerProductPriceSmallDTO
    {
        public int CustomerProductId { get; set; }
        public int ProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    [TSInclude]
    public class CustomerProductPriceIODTO
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
    }
    #endregion

    #endregion

    #region Product accounts

    public class ProductAccountStdDTO
    {
        public int ProductAccountStdId { get; set; }
        public int ProductId { get; set; }
        public int? AccountId { get; set; }
        public ProductAccountType Type { get; set; }
        public int? Percent { get; set; }

        // Extensions
        public AccountDTO AccountStd { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }
    }

    #endregion

    #region ProductGroup
    [TSInclude]
    public class ProductGroupDTO
    {
        public int ProductGroupId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    [TSInclude]
    public class ProductGroupGridDTO
    {
        public int ProductGroupId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region ProductUnit

    [TSInclude]
    public class ProductUnitDTO
    {
        public int ProductUnitId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    [TSInclude]
    public class ProductUnitSmallDTO
    {
        public int ProductUnitId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    [TSInclude]
    public class ProductUnitConvertDTO
    {
        public int ProductUnitConvertId { get; set; }
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public int ProductUnitId { get; set; }
        public string ProductUnitName { get; set; }
        public decimal ConvertFactor { get; set; }

        // Extras
        public int? BaseProductUnitId { get; set; }
        public string BaseProductUnitName { get; set; }

        //Extensions
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ProductUnitConvertExDTO : ProductUnitConvertDTO
    {
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
    }

    #endregion

    #region Project

    #region CaseProject

    public class CaseProjectDTO : ProjectDTO
    {
        public TermGroup_CaseProjectApplication Application { get; set; }
        public TermGroup_CaseProjectType CaseProjectType { get; set; }
        public int? LicenseId { get; set; }
        public int? CustomerCompanyId { get; set; }
        public TermGroup_CaseProjectChannel Channel { get; set; }
        public TermGroup_CaseProjectPriority Priority { get; set; }
        public TermGroup_CaseProjectArea Area { get; set; }
        public TermGroup_CaseProjectResult Result { get; set; }
        public int AttestStateId { get; set; }
        public int ReportedByUserId { get; set; }
        public int? ResponsibleUserId { get; set; }
        public int? ClosedByUserId { get; set; }
        public int? WorkItemNr { get; set; }
        public int? SprintId { get; set; }
        public int ElapsedTime { get; set; }
        public bool StopwatchDisabled { get; set; }

        // Extensions
        public List<CaseProjectNoteDTO> Notes { get; set; }
    }

    public class CaseProjectNoteDTO
    {
        public int CaseProjectNoteId { get; set; }
        public int ProjectId { get; set; }
        public CaseProjectNoteType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public string Note { get; set; }
    }

    public class CaseProjectGridDTO
    {
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string LicenseName { get; set; }
        public string CustomerName { get; set; }
        public string Name { get; set; }
        public TermGroup_CaseProjectType CaseProjectType { get; set; }
        public TermGroup_CaseProjectPriority Priority { get; set; }
        public int AttestStateId { get; set; }
        public string StatusName { get; set; }
        public string StatusImageSource { get; set; }
    }

    #endregion

    #region TimeProject

    [TSInclude]
    public class TimeProjectDTO : ProjectDTO
    {
        public string PayrollProductAccountingPrio { get; set; }
        public string InvoiceProductAccountingPrio { get; set; }
        public bool HasInvoices { get; set; }
        public int NumberOfInvoices { get; set; }

        public string ParentProjectNr { get; set; }
        public string ParentProjectName { get; set; }

        public int? OrderTemplateId { get; set; }
        public List<ProjectWeekTotal> ProjectWeekTotals { get; set; }
    }

    #endregion

    #endregion

    #region ProjectUser

    [TSInclude]
    public class ProjectUserDTO
    {
        public int ProjectUserId { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public int? TimeCodeId { get; set; }
        public TermGroup_ProjectUserType Type { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public decimal EmployeeCalculatedCost { get; set; }

        // Extensions
        public int InternalId { get; set; }
        public string Name { get; set; }
        public string TimeCodeName { get; set; }
        public string TypeName { get; set; }
    }

    #endregion

    #region Quinyx (FleXForce)

    public class CompanyFlexForceDTO
    {
        public int CompanyFlexForceId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? CostCentreAccountDimId { get; set; }
        public int? SectionAccountDimId { get; set; }
        public int? AccountNoAccountDimId { get; set; }
        public int? ProjectAccountDimId { get; set; }
        public int? CategoryAccountDimId { get; set; }

        public string ApiKeys { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class TimeScheduleSyncBatchDTO
    {
        public int TimeScheduleSyncBatchId { get; set; }
        public int ActorCompanyId { get; set; }

        public TermGroup_IOSource Source { get; set; }
        public TermGroup_TimeScheduleSyncBatchType Type { get; set; }
        public bool ConditionalSync { get; set; }
        public DateTime SyncDate { get; set; }

        public int PresenceDaysSynced { get; set; }
        public int AbsenceDaysSynced { get; set; }
        public int LeaveApplicationsSynced { get; set; }
        public int EmployeesSynced { get; set; }

        public TermGroup_TimeScheduleSyncBatchStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string StatusName { get; set; }
    }

    public class TimeScheduleSyncEntryDTO
    {
        public int TimeScheduleSyncEntryId { get; set; }
        public int TimeScheduleSyncBatchId { get; set; }
        public int ScheduleId { get; set; }

        public TermGroup_TimeScheduleSyncEntryType Type { get; set; }

        public string EmployeeNr { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleStartTime { get; set; }
        public DateTime ScheduleStopDate { get; set; }
        public DateTime ScheduleStopTime { get; set; }
        public DateTime? Break1Start { get; set; }
        public DateTime? Break1Stop { get; set; }
        public DateTime? Break2Start { get; set; }
        public DateTime? Break2Stop { get; set; }
        public DateTime? Break3Start { get; set; }
        public DateTime? Break3Stop { get; set; }
        public DateTime? Break4Start { get; set; }
        public DateTime? Break4Stop { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CostCentre { get; set; }
        public string CostCentreExtCode { get; set; }
        public int Section { get; set; }
        public string SectionName { get; set; }
        public int SalaryType { get; set; }
        public string SalaryTypeExtCode { get; set; }
        public int AccountNo { get; set; }
        public string AccountNoExtCode { get; set; }
        public int ProjectNo { get; set; }
        public string ProjectNoExtCode { get; set; }
        public string LeaveReason { get; set; }

        public DateTime TimeStamp { get; set; }
        public TermGroup_TimeScheduleSyncEntryStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string StatusName { get; set; }
        public string EmployeeNrSort { get; set; }
        public DateTime ScheduleStart { get; set; }
        public DateTime ScheduleStop { get; set; }
        public bool IsSelected { get; set; }
    }

    public class TimeScheduleSyncLeaveApplicationDTO
    {
        public int TimeScheduleSyncLeaveApplicationId { get; set; }
        public int TimeScheduleSyncBatchId { get; set; }

        public string EmployeeNr { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime ToTime { get; set; }

        public bool IsPreliminary { get; set; }
        public string ExtCode { get; set; }
        public string LeaveReason { get; set; }
        public string BodyText { get; set; }
        public decimal? SickLevel { get; set; }

        public DateTime TimeStamp { get; set; }

        public TermGroup_TimeScheduleSyncLeaveApplicationStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        // Extensions
        public string EmployeeNrSort { get; set; }
        public string StatusName { get; set; }
        public bool IsSelected { get; set; }
    }

    #endregion

    #region RecalculateTimeHead
    [TSInclude]
    public class RecalculateTimeHeadDTO
    {
        public int RecalculateTimeHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? UserId { get; set; }

        public SoeRecalculateTimeHeadAction Action { get; set; }
        public TermGroup_RecalculateTimeHeadStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? ExcecutedStartTime { get; set; }
        public DateTime? ExcecutedStopTime { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        // Extensions
        public string StatusName { get; set; }
        public List<RecalculateTimeRecordDTO> Records { get; set; }
    }

    #endregion

    #region RecalculateTimeRecord
    [TSInclude]
    public class RecalculateTimeRecordDTO
    {
        public int RecalculateTimeRecordId { get; set; }
        public int RecalculateTimeHeadId { get; set; }
        public int EmployeeId { get; set; }
        public TermGroup_RecalculateTimeRecordStatus RecalculateTimeRecordStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string ErrorMsg { get; set; }
        public string WarningMsg { get; set; }

        // Extensions
        public string EmployeeName { get; set; }
        public string StatusName { get; set; }
    }

    #endregion

    #region Report
    [TSInclude]
    public class ReportDTO
    {
        public int ReportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int ReportTemplateId { get; set; }
        public int? ReportSelectionId { get; set; }

        public int ReportNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IncludeAllHistoricalData { get; set; }
        public bool IncludeBudget { get; set; }
        public bool DetailedInformation { get; set; }
        public int NoOfYearsBackinPreviousYear { get; set; }
        public bool Standard { get; set; }
        public bool Original { get; set; }
        public SoeModule Module { get; set; }
        public string FilePath { get; set; }
        public TermGroup_ReportExportType ExportType { get; set; }
        public TermGroup_ReportExportFileType ExportFileType { get; set; }
        public bool ShowInAccountingReports { get; set; }

        public int? NrOfDecimals { get; set; }
        public bool ShowRowsByAccount { get; set; }

        public TermGroup_ReportGroupAndSortingTypes SortByLevel1 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel2 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel3 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel4 { get; set; }
        public bool IsSortAscending { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel1 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel2 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel3 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel4 { get; set; }
        public string Special { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //SysReportTemplateType
        public int? SysReportTemplateTypeId { get; set; }
        public int? SysReportTemplateTypeSelectionType { get; set; }

        //ReportSelection
        public string ReportSelectionText { get; set; }
        public List<ReportSelectionDateDTO> ReportSelectionDate { get; set; }
        public List<ReportSelectionIntDTO> ReportSelectionInt { get; set; }
        public List<ReportSelectionStrDTO> ReportSelectionStr { get; set; }

        //Permissions
        public List<int> RoleIds { get; set; }
        public bool IsNewGroupsAndHeaders { get; set; }
        public int ImportCompanyId { get; set; }
        public int ImportReportId { get; set; }

        //Relations
        public List<ReportSettingDTO> Settings { get; set; }
        public List<ReportTemplateSettingDTO> ReportTemplateSettings { get; set; }
    }

    public class ReportSmallDTO
    {
        public int ReportId { get; set; }
        public int ReportNr { get; set; }
        public string Name { get; set; }
    }

    public class ReportAbstractionDTO
    {
        //Combines SysReportTemplate & Report
        public int ReportId { get; set; }
        public int ReportTemplateId { get; set; }
        public int SysTemplateTypeId { get; set; }
        public string Name { get; set; }
        public bool IsSys { get; set; }
        public bool IsComp
        {
            get
            {
                return !IsSys;
            }
        }
    }

    public class ReportGroupMappingDTO
    {
        public int ReportId { get; set; }
        public int ReportGroupId { get; set; }
        public int Order { get; set; }
        public ReportGroupDTO ReportGroup { get; set; }
    }

    #endregion

    #region ReportGroup

    public class ReportGroupDTO
    {
        public int ReportGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TemplateTypeId { get; set; }

        public SoeModule Module { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string NameAndDescription
        {
            get
            {
                return $"{Name} {Description}";
            }
        }

        public bool ShowLabel { get; set; }
        public bool ShowSum { get; set; }
        public bool InvertRow { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TemplateType { get; set; }
        public bool IsSelected { get; set; }

        public List<ReportGroupHeaderMappingDTO> ReportGroupHeaderMappings { get; set; }
        public List<ReportHeaderDTO> ReportHeaders { get; set; }
    }

    #endregion

    #region ReportHeader

    public class ReportHeaderDTO
    {
        public int ReportHeaderId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TemplateTypeId { get; set; }

        public SoeModule Module { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string NameAndDescription
        {
            get
            {
                return $"{Name} {Description}";
            }
        }

        public bool ShowLabel { get; set; }
        public bool ShowSum { get; set; }
        public bool ShowRow { get; set; }
        public bool ShowZeroRow { get; set; }
        public bool InvertRow { get; set; }
        public bool DoNotSummarizeOnGroup { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TemplateType { get; set; }
        public bool IsSelected { get; set; }

        public List<ReportGroupHeaderMappingDTO> ReportGroupHeaderMappings { get; set; }
        public List<ReportHeaderIntervalDTO> ReportHeaderIntervals { get; set; }
    }

    public class ReportGroupHeaderMappingDTO
    {
        public int ReportHeaderId { get; set; }
        public ReportHeaderDTO ReportHeader { get; set; }
        public int ReportGroupId { get; set; }
        public ReportGroupDTO ReportGroup { get; set; }
        public int Order { get; set; }
    }

    public interface IReportHeaderInterval
    {
        string IntervalFrom { get; set; }
        string IntervalTo { get; set; }
        int? SelectValue { get; set; }
    }

    public class ReportHeaderIntervalDTO : IReportHeaderInterval
    {
        public int ReportHeaderIntervalId { get; set; }
        public int ReportHeaderId { get; set; }
        public string IntervalFrom { get; set; }
        public string IntervalTo { get; set; }
        public int? SelectValue { get; set; }
    }


    #endregion

    #region ReportPermission

    public class ReportRolePermissionDTO
    {
        public int ActorCompanyId { get; set; }
        public int RoleId { get; set; }
        public int ReportId { get; set; }

    }

    #endregion

    #region ReportPackage

    public class ReportPackageGridDTO
    {
        public int ReportPackageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ReportPackageDTO
    {
        public int ReportPackageId { get; set; }
        public int ActorCompanyId { get; set; }

        public SoeModule Module { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region ReportSelection

    #region ReportSelectionDate
    [TSInclude]
    public class ReportSelectionDateDTO
    {
        public int ReportSelectionDateId { get; set; }
        public int ReportSelectionId { get; set; }
        public SoeSelectionData ReportSelectionType { get; set; }
        public DateTime SelectFrom { get; set; }
        public DateTime SelectTo { get; set; }
        public int? SelectGroup { get; set; }
        public int? Order { get; set; }
    }

    #endregion

    #region ReportSelectionInt
    [TSInclude]
    public class ReportSelectionIntDTO
    {
        public int ReportSelectionIntId { get; set; }
        public int ReportSelectionId { get; set; }
        public SoeSelectionData ReportSelectionType { get; set; }
        public int SelectFrom { get; set; }
        public int SelectTo { get; set; }
        public int? SelectGroup { get; set; }
        public int? Order { get; set; }
    }

    #endregion

    #region ReportSelectionStr
    [TSInclude]
    public class ReportSelectionStrDTO
    {
        public int ReportSelectionStrId { get; set; }
        public int ReportSelectionId { get; set; }
        public SoeSelectionData ReportSelectionType { get; set; }
        public string SelectFrom { get; set; }
        public string SelectTo { get; set; }
        public int? SelectGroup { get; set; }
        public int? Order { get; set; }
    }

    #endregion

    #endregion

    #region ReportSetting
    [TSInclude]
    public class ReportSettingDTO
    {
        public int ReportSettingId { get; set; }
        public int ReportId { get; set; }
        public TermGroup_ReportSettingType Type { get; set; }
        public string Value { get; set; }
        public SettingDataType DataTypeId { get; set; }
        public bool? BoolData { get; set; }
        public int? IntData { get; set; }
        public string StrData { get; set; }
    }

    #endregion

    #region ReportTemplate

    public class ReportTemplateDTO
    {
        public int ReportTemplateId { get; set; }
        public int? ActorCompanyId { get; set; }

        public int? SysReportTemplateTypeId { get; set; }
        public int? SysReportTypeId { get; set; }
        public List<int> SysCountryIds { get; set; }
        public bool IsSystem { get; set; }
        public SoeModule Module { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? ReportNr { get; set; }
        public string FileName { get; set; }
        public int GroupByLevel1 { get; set; }
        public int GroupByLevel2 { get; set; }
        public int GroupByLevel3 { get; set; }
        public int GroupByLevel4 { get; set; }
        public int SortByLevel1 { get; set; }
        public int SortByLevel2 { get; set; }
        public int SortByLevel3 { get; set; }
        public int SortByLevel4 { get; set; }
        public string Special { get; set; }
        public bool IsSortAscending { get; set; }
        public bool ShowGroupingAndSorting { get; set; }
        public bool ShowOnlyTotals { get; set; }
        public List<int> ValidExportTypes { get; set; }
        public bool IsSystemReport { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<ReportTemplateSettingDTO> ReportTemplateSettings { get; set; }

        //Extensions
        public string SysReportTemplateTypeName { get; set; }

        public static List<int> GetValidExportTypes(string exportTypes, SoeReportType sysReportType = SoeReportType.CrystalReport)
        {
            List<int> validExportTypes = new List<int>();

            if (sysReportType == SoeReportType.CrystalReport)
            {
                validExportTypes.Add((int)TermGroup_ReportExportType.Pdf);
                validExportTypes.Add((int)TermGroup_ReportExportType.Xml);
            }

            if (sysReportType == SoeReportType.Analysis)
            {
                validExportTypes.Add((int)TermGroup_ReportExportType.MatrixGrid);
                validExportTypes.Add((int)TermGroup_ReportExportType.MatrixExcel);
            }

            string[] exportTypeIds = exportTypes?.Split(',');
            if (exportTypeIds != null)
            {
                foreach (string exportTypeId in exportTypeIds)
                {
                    if (Int32.TryParse(exportTypeId, out int id) && !validExportTypes.Contains(id) && Enum.IsDefined(typeof(TermGroup_ReportExportType), id))
                        validExportTypes.Add(id);
                }
            }

            return validExportTypes;
        }
    }

    public class ReportTemplateGridDTO
    {
        public int ReportTemplateId { get; set; }
        public int? ReportNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //Extensions
        public string SysReportTemplateGroupName { get; set; }
        public string SysReportTemplateTypeName { get; set; }
        public string CombinedDisplayName { get; set; }

    }

    [TSInclude]
    public class ReportTemplateSettingDTO
    {
        public int ReportTemplateSettingId { get; set; }
        public int ReportTemplateId { get; set; }
        public int SettingField { get; set; }
        public int SettingType { get; set; }
        public string SettingValue { get; set; }
        public bool IsModified { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region ReportPrintout

    public class ReportPrintoutDTO
    {
        public int ReportPrintoutId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? ReportId { get; set; }
        public int? ReportPackageId { get; set; }
        public int? ReportUrlId { get; set; }
        public int? ReportTemplateId { get; set; }
        public int? SysReportTemplateTypeId { get; set; }
        [TsIgnore]
        public SoeReportTemplateType? SysReportTemplateType
        {
            get
            {
                return this.SysReportTemplateTypeId.HasValue ? (SoeReportTemplateType)this.SysReportTemplateTypeId : (SoeReportTemplateType?)null;
            }
        }
        public TermGroup_ReportExportType ExportType { get; set; }
        public SoeExportFormat ExportFormat { get; set; }
        public TermGroup_ReportPrintoutDeliveryType DeliveryType { get; set; }
        public int Status { get; set; }
        public int ResultMessage { get; set; }
        public string ResultMessageDetails { get; set; }
        public string EmailMessage { get; set; }
        public string ReportName { get; set; }
        public string Selection { get; set; }
        public DateTime? OrderedDeliveryTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? CleanedTime { get; set; }
        public string XML { get; set; }
        public byte[] XMLCompressed { get; set; }
        public byte[] Data { get; set; }
        public List<byte[]> Datas { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }

        #region Extensions

        public string Json { get; set; }

        public bool ForceValidation { get; set; }
        public string ReportFileName { get; set; }
        public string ReportFileType { get; set; }

        //Email
        public int? EmailTemplateId { get; set; }
        public string EmailFileName { get; set; }
        public List<int> EmailRecipients { get; set; }
        public string SingleRecipient { get; set; }
        public bool IsEmailValid
        {
            get
            {
                return this.EmailTemplateId.HasValue && this.EmailTemplateId.Value > 0 && ((this.EmailRecipients != null && this.EmailRecipients.Count > 0) || !this.SingleRecipient.IsNullOrEmpty());
            }
        }

        #endregion
    }

    public class ReportPrintoutGridDTO
    {
        public int ReportPrintoutId { get; set; }
        public int ActorCompanyId { get; set; }
        public string ReportName { get; set; }
        public TermGroup_ReportExportType ExportType { get; set; }
        public SoeExportFormat ExportFormat { get; set; }
        public TermGroup_ReportPrintoutDeliveryType DeliveryType { get; set; }

        public string Selection { get; set; }
        public DateTime? OrderedDeliveryTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public DateTime? CleanedTime { get; set; }

    }



    #endregion

    #region ReportUserSelection
    [TSInclude]
    public class ReportUserSelectionDTO
    {
        public int ReportUserSelectionId { get; set; }
        public int ReportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? UserId { get; set; }
        public ReportUserSelectionType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ScheduledJobHeadId { get; set; }
        public bool ValidForScheduledJobHead { get; set; }
        public ICollection<ReportDataSelectionDTO> Selections { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<ReportUserSelectionAccessDTO> Access { get; set; }
    }

    #endregion

    #region ReportUserSelectionAccess
    [TSInclude]
    public class ReportUserSelectionAccessDTO
    {
        public int ReportUserSelectionAccessId { get; set; }
        public int ReportUserSelectionId { get; set; }
        public TermGroup_ReportUserSelectionAccessType Type { get; set; }
        public int? RoleId { get; set; }
        public int? MessageGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region RetroactivePayroll

    public class RetroactivePayrollDTO
    {
        public int RetroactivePayrollId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimePeriodId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public TermGroup_SoeRetroactivePayrollStatus Status { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public string TimePeriodName { get; set; }
        public DateTime? TimePeriodPaymentDate { get; set; }
        public int TimePeriodHeadId { get; set; }
        public string TimePeriodHeadName { get; set; }
        public string StatusName { get; set; }
        public int NrOfEmployees { get; set; }
        public decimal TotalAmount { get; set; }

        public List<RetroactivePayrollEmployeeDTO> RetroactivePayrollEmployees { get; set; }
        public List<RetroactivePayrollAccountDTO> RetroactivePayrollAccounts { get; set; }
    }

    public class RetroactivePayrollAccountDTO
    {
        public int RetroactivePayrollAccountId { get; set; }
        public int RetroactivePayrollId { get; set; }
        public int AccountDimId { get; set; }
        public int? AccountId { get; set; }
        public TermGroup_RetroactivePayrollAccountType Type { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public AccountDimDTO AccountDim { get; set; }
    }

    public class RetroactivePayrollEmployeeDTO
    {
        public int RetroactivePayrollEmployeeId { get; set; }
        public int RetroactivePayrollId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public string Note { get; set; }
        public TermGroup_SoeRetroactivePayrollEmployeeStatus Status { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public int PayrollGroupId { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<RetroactivePayrollOutcomeDTO> RetroactivePayrollOutcomes { get; set; }
        public bool HasOutcomes { get; set; }
        public bool HasTransactions { get; set; }
        public string StatusName { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class RetroactivePayrollOutcomeDTO
    {
        public int RetroactivePayrollOutcomeId { get; set; }
        public int RetroactivePayrolIEmployeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TransactionUnitPrice { get; set; }
        public decimal? RetroUnitPrice { get; set; }
        public decimal? SpecifiedUnitPrice { get; set; }
        public decimal Amount { get; set; }
        public bool IsRetroCalculated { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public TermGroup_SoeRetroactivePayrollOutcomeErrorCode ErrorCode { get; set; }
        public TermGroup_PayrollResultType ResultType { get; set; }
        public bool IsReversed { get; set; }
        public List<AttestPayrollTransactionDTO> Transactions { get; set; }

        //Extensions
        public bool IsReadOnly
        {
            get
            {
                return (this.HasTransactions || this.IsReversed);
            }
        }
        public bool HasTransactions { get; set; }
        public string ErrorCodeText { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductNumberSort
        {
            get
            {
                return !String.IsNullOrEmpty(this.PayrollProductNumber) ? this.PayrollProductNumber.PadLeft(100, '0') : String.Empty;
            }
        }
        public string PayrollProductName { get; set; }
        public string PayrollProductString
        {
            get
            {
                string text = "";
                if (!String.IsNullOrEmpty(this.PayrollProductNumber) && !String.IsNullOrEmpty(this.PayrollProductName))
                    text = String.Format("{0}, {1}", PayrollProductNumber, PayrollProductName);
                return text;
            }
        }
        public bool IsQuantity
        {
            get
            {
                return (this.ResultType == TermGroup_PayrollResultType.Quantity);
            }
        }
        public string QuantityString
        {
            get
            {
                return IsQuantity ? this.Quantity.ToString("F") : CalendarUtility.GetHoursAndMinutesString(Convert.ToInt32(this.Quantity), false);
            }
        }

    }

    #endregion

    #region Role

    [TSInclude]
    public class RoleDTO
    {
        public int RoleId { get; set; }
        public int? ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string ActualName { get; set; }
        public int? TermId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<String> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
        public int Sort { get; set; }
    }

    public class RoleEditDTO
    {
        public int RoleId { get; set; }
        public string Name { get; set; }

        public string ExternalCodesString { get; set; }
        public int FavoriteOption { get; set; }

        public bool IsAdmin { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool Active { get; set; }
        public int Sort { get; set; }
        // Extensions for save
        public int TemplateRoleId { get; set; }
        public bool UpdateStartPage { get; set; }
    }

    public class RoleGridDTO
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public string ExternalCodesString { get; set; }
        public int Sort { get; set; }
        public SoeEntityState State { get; set; }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    #endregion

    #region ScheduleCycle
    [TSInclude]
    public class ScheduleCycleDTO
    {
        public int ScheduleCycleId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NbrOfWeeks { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }

        //Extensions

        public List<ScheduleCycleRuleDTO> ScheduleCycleRuleDTOs { get; set; }

        #region Methods

        public bool Valid(DayOfWeek dayOfWeek, DateTime startTime, DateTime stopTime)
        {
            if (this.ScheduleCycleRuleDTOs == null)
                return false;

            foreach (var rule in this.ScheduleCycleRuleDTOs)
            {
                if (rule.Valid(dayOfWeek, startTime, stopTime))
                    return true;
            }

            return false;
        }

        public bool HasWeekends()
        {
            var weeksDays = GetValidWeekDays();
            return weeksDays.Any(a => a == DayOfWeek.Saturday || a == DayOfWeek.Sunday);
        }

        public bool OnlyHasOneWeekEndDayPerWeek(EmployeePostDTO employeePost)
        {
            var rules = ScheduleCycleRuleDTOs.Where(w => (w.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)DayOfWeek.Saturday) || w.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)DayOfWeek.Sunday))).ToList();
            var valid = rules.SelectMany(s => s.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Where(w => w == (int)DayOfWeek.Sunday || w == (int)DayOfWeek.Saturday)).Count() == 1;

            if (!valid && !employeePost.FreeDays.IsNullOrEmpty() && employeePost.FreeDays.Any(a => a == DayOfWeek.Saturday || a == DayOfWeek.Sunday))
                valid = true;

            return valid;
        }

        public List<DayOfWeek> GetValidWeekDays()
        {
            List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>();
            List<int> dayOfWeekIds = GetValidWeekDayIds();

            foreach (var id in dayOfWeekIds)
            {
                if (id <= 6)
                    dayOfWeeks.Add((DayOfWeek)id);
            }

            return dayOfWeeks;

        }

        public List<int> GetValidWeekDayIds()
        {
            List<int> dayOfWeekIds = new List<int>();
            if (this.ScheduleCycleRuleDTOs != null)
            {
                foreach (ScheduleCycleRuleDTO scheduleCycleRule in this.ScheduleCycleRuleDTOs)
                {
                    dayOfWeekIds.AddRange(scheduleCycleRule.ScheduleCycleRuleTypeDTO.DayOfWeekIds);
                }
                dayOfWeekIds = dayOfWeekIds.Distinct().ToList();
            }
            return dayOfWeekIds;
        }

        public int GetNumberOfWeekEnds()
        {
            if (this.ScheduleCycleRuleDTOs != null)
            {
                int saturdayCount = 0;
                int sundayCount = 0;
                int bothCount = 0;

                foreach (ScheduleCycleRuleDTO scheduleCycleRule in this.ScheduleCycleRuleDTOs)
                {
                    if (scheduleCycleRule.ScheduleCycleRuleTypeDTO != null && (scheduleCycleRule.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)DayOfWeek.Saturday) && scheduleCycleRule.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)DayOfWeek.Sunday)))
                        bothCount += scheduleCycleRule.MinOccurrences;
                    else
                    {
                        if (scheduleCycleRule.ScheduleCycleRuleTypeDTO != null && scheduleCycleRule.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)DayOfWeek.Saturday))
                            saturdayCount += scheduleCycleRule.MinOccurrences;

                        if (scheduleCycleRule.ScheduleCycleRuleTypeDTO != null && scheduleCycleRule.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)DayOfWeek.Sunday))
                            sundayCount += scheduleCycleRule.MinOccurrences;
                    }
                }

                if (bothCount > 0 && sundayCount == 0 && saturdayCount == 0)
                    return bothCount;

                if (saturdayCount > 0 && sundayCount > 0 && saturdayCount == sundayCount)
                    return saturdayCount;

                if (sundayCount > saturdayCount && sundayCount > 0)
                    return sundayCount;

                if (saturdayCount > sundayCount && saturdayCount > 0)
                    return saturdayCount;

                if (bothCount > 0)
                    return bothCount;

                return saturdayCount + sundayCount;
            }
            else
                return 0;
        }

        #endregion
    }

    [TSInclude]
    public class ScheduleCycleGridDTO
    {
        public int ScheduleCycleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NbrOfWeeks { get; set; }
    }
    #endregion

    #region ScheduleCycleRule
    [TSInclude]
    public class ScheduleCycleRuleDTO
    {
        public int ScheduleCycleRuleId { get; set; }
        public int ScheduleCycleId { get; set; }
        public int ScheduleCycleRuleTypeId { get; set; }
        public int MinOccurrences { get; set; }
        public int MaxOccurrences { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }


        //Extension
        public ScheduleCycleRuleTypeDTO ScheduleCycleRuleTypeDTO { get; set; }

        //Methods
        public bool Valid(DayOfWeek dayOfWeek, DateTime startTime, DateTime stopTime)
        {
            if (this.ScheduleCycleRuleTypeDTO == null)
                return false;

            return this.ScheduleCycleRuleTypeDTO.Valid(dayOfWeek, startTime, stopTime);
        }

        public int GetOverlappingMinutes(DateTime startTime, DateTime stopTime)
        {
            if (this.ScheduleCycleRuleTypeDTO == null)
                return 0;

            return this.ScheduleCycleRuleTypeDTO.GetOverlappingMinutes(startTime, stopTime);
        }
    }

    #endregion

    #region ScheduleCycleRuleType

    [TSInclude]
    public class ScheduleCycleRuleTypeDTO
    {
        public int ScheduleCycleRuleTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string DayOfWeeks { get; set; }
        public List<int> DayOfWeekIds { get; set; }
        public string DayOfWeeksGridString { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }

        public int Lenght
        {
            get
            {
                return Convert.ToInt32((this.StopTime - this.StartTime).TotalMinutes);
            }
        }

        public decimal Hours
        {
            get
            {
                return decimal.Divide(Lenght, 60);
            }
        }

        public bool IsWeekEndOnly
        {
            get
            {
                return !DayOfWeekIds.Any(d => d != (int)DayOfWeek.Sunday && d != (int)DayOfWeek.Saturday);
            }
        }

        public bool IsEvening(DateTime eveningStarts)
        {
            bool valid = false;
            if (eveningStarts < StopTime)
            {
                if (Hours < 12)
                    valid = true;
            }

            return valid;

        }

        public bool Valid(DayOfWeek dayOfWeek, DateTime startTimeInput, DateTime stopTimeInput)
        {
            DateTime startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, startTimeInput);
            DateTime stopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT.AddDays((stopTimeInput - startTimeInput).Days), stopTimeInput);

            if (this.DayOfWeekIds == null)
                return false;

            if (!this.DayOfWeekIds.Contains((int)dayOfWeek))
                return false;

            if (CalendarUtility.IsEndTimeInRange(stopTime, this.StartTime, this.StopTime) || CalendarUtility.GetOverlappingMinutes(startTime, stopTime, this.StartTime, this.StopTime) > ((stopTime - startTime).TotalMinutes * 0.99))
                return true;

            return false;
        }

        public int GetOverlappingMinutes(DateTime startTimeInput, DateTime stopTimeInput)
        {
            DateTime startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, startTimeInput);
            DateTime stopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT.AddDays((stopTimeInput - startTimeInput).Days), stopTimeInput);
            return CalendarUtility.GetOverlappingMinutes(startTime, stopTime, this.StartTime, this.StopTime);
        }

    }

    [TSInclude]
    public class ScheduleCycleRuleTypeGridDTO
    {
        public int ScheduleCycleRuleTypeId { get; set; }
        public string Name { get; set; }
        public string DayOfWeeksGridString { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }

    #endregion

    #region ScheduledJob

    #region ScheduledJobHead

    public class ScheduledJobHeadDTO
    {
        public ScheduledJobHeadDTO()
        {
            Rows = new List<ScheduledJobRowDTO>();
            Logs = new List<ScheduledJobLogDTO>();
            Settings = new List<ScheduledJobSettingDTO>();
        }
        public int ScheduledJobHeadId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public bool SharedOnLicense { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<ScheduledJobRowDTO> Rows { get; set; }
        public List<ScheduledJobLogDTO> Logs { get; set; }
        public List<ScheduledJobSettingDTO> Settings { get; set; }
    }

    public class ScheduledJobHeadGridDTO
    {
        public int ScheduledJobHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public bool SharedOnLicense { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region ScheduledJobRow

    public class ScheduledJobRowDTO
    {
        public int ScheduledJobRowId { get; set; }

        public int ScheduledJobHeadId { get; set; }
        public string RecurrenceInterval { get; set; }
        public int? SysTimeIntervalId { get; set; }
        public DateTime NextExecutionTime { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string RecurrenceIntervalText { get; set; }
        public string TimeIntervalText { get; set; }
    }

    #endregion

    #region ScheduledJobLog

    public class ScheduledJobLogDTO
    {
        public int ScheduledJobLogId { get; set; }
        public int ScheduledJobHeadId { get; set; }
        public int ScheduledJobRowId { get; set; }
        public int BatchNr { get; set; }
        public TermGroup_ScheduledJobLogStatus Status { get; set; }
        public TermGroup_ScheduledJobLogLevel LogLevel { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }

        // Extensions
        public string LogLevelName { get; set; }
        public string StatusName { get; set; }
    }

    #endregion

    #region ScheduledJobSetting

    public class ScheduledJobSettingDTO
    {
        public int ScheduledJobSettingId { get; set; }
        public int ScheduledJobHeadId { get; set; }
        public TermGroup_ScheduledJobSettingType Type { get; set; }

        public SettingDataType DataType { get; set; }
        public string Name { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }
        public SoeEntityState State { get; set; }

        public List<SmallGenericType> Options { get; set; }
    }

    #endregion

    #endregion

    #region SchoolHoliday
    [TSInclude]
    public class SchoolHolidayDTO
    {
        public int SchoolHolidayId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsSummerHoliday { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    [TSInclude]
    public class SchoolHolidayGridDTO
    {
        public int SchoolHolidayId { get; set; }
        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region ShiftType
    [TSInclude]
    public class ShiftTypeDTO
    {
        public int ShiftTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        [TsIgnore]
        public string TimeScheduleTypeName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockType? TimeScheduleTemplateBlockType { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string NeedsCode { get; set; }
        public int? ExternalId { get; set; }
        public string ExternalCode { get; set; }
        public int DefaultLength { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public bool HandlingMoney { get; set; }
        public int? AccountId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> AccountInternals { get; set; }
        public List<int> AccountInternalIds { get; set; }

        public AccountingSettingsRowDTO AccountingSettings { get; set; }

        public List<ShiftTypeSkillDTO> ShiftTypeSkills { get; set; }
        public List<ShiftTypeEmployeeStatisticsTargetDTO> EmployeeStatisticsTargets { get; set; }
        public List<int> LinkedShiftTypeIds { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<ShiftTypeHierarchyAccountDTO> HierarchyAccounts { get; set; }
        public List<int> ChildHierarchyAccountIds { get; set; }
        public bool AccountIsNotActive { get; set; }
        public string AccountNrAndName { get; set; }

        //Methods

        public bool IsSkillsSame(List<ShiftTypeSkillDTO> ShiftTypeSkills)
        {
            if (this.ShiftTypeSkills.Count != ShiftTypeSkills.Count)
                return false;

            if (this.ShiftTypeSkills.Sum(s => s.SkillLevel) != ShiftTypeSkills.Sum(s => s.SkillLevel))
                return false;

            if (this.ShiftTypeSkills.Sum(s => s.SkillId) != ShiftTypeSkills.Sum(s => s.SkillId))
                return false;

            return true;
        }

        public string GetKey()
        {
            string key = string.Empty;

            if (this.ShiftTypeSkills != null)
            {
                int count = this.ShiftTypeSkills.Count;
                int idSum = this.ShiftTypeSkills.Sum(i => i.SkillId);
                int skillSum = this.ShiftTypeSkills.Sum(i => i.SkillLevel);

                key = $"{count}#{idSum}#{skillSum}";
            }
            else
            {
                key = string.Empty;
            }
            return key;
        }
    }

    [TSInclude]
    public class ShiftTypeGridDTO
    {
        #region Propertys

        public int ShiftTypeId { get; set; }
        public TermGroup_TimeScheduleTemplateBlockType? TimeScheduleTemplateBlockType { get; set; }
        public string TimeScheduleTemplateBlockTypeName { get; set; }
        public string Name { get; set; }
        public string NeedsCode { get; set; }
        public string NeedsCodeName { get; set; }
        public string Description { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public string CategoryNames { get; set; }
        public string Color { get; set; }
        public int DefaultLength { get; set; }
        public int? AccountId { get; set; }
        public string AccountingStringAccountNames { get; set; }
        public string SkillNames { get; set; }
        public string ExternalCode { get; set; }
        public bool AccountIsNotActive { get; set; }

        #endregion
    }

    #endregion

    #region ShiftTypeHierarchyAccount
    [TSInclude]
    public class ShiftTypeHierarchyAccountDTO
    {
        public int ShiftTypeHierarchyAccountId { get; set; }
        public int AccountId { get; set; }
        public TermGroup_AttestRoleUserAccountPermissionType AccountPermissionType { get; set; }
    }

    #endregion

    #region ShiftTypeLink

    public class ShiftTypeLinkDTO
    {
        public int ActorCompanyId { get; set; }
        public string Guid { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
    }

    #endregion

    #region ShiftTypeEmployeeStatisticsTarget
    [TSInclude]
    public class ShiftTypeEmployeeStatisticsTargetDTO
    {
        public int ShiftTypeEmployeeStatisticsTargetId { get; set; }
        public int ShiftTypeId { get; set; }
        public TermGroup_EmployeeStatisticsType EmployeeStatisticsType { get; set; }
        public decimal TargetValue { get; set; }
        public DateTime? FromDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeStatisticsTypeName { get; set; }
    }

    #endregion

    #region ShiftTypeSkill
    [TSInclude]
    public class ShiftTypeSkillDTO
    {
        public int ShiftTypeSkillId { get; set; }
        public int ShiftTypeId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }

        // Extensions
        public string SkillName { get; set; }
        public string SkillTypeName { get; set; }
        public double SkillLevelStars { get; set; }
        public bool Missing { get; set; }
    }

    #endregion

    #region Skill

    [TSInclude]
    public class SkillDTO
    {
        public int SkillId { get; set; }
        public int SkillTypeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public SkillTypeDTO SkillTypeDTO { get; set; }
        public string SkillTypeName { get; set; }
    }

    [TSInclude]
    public class SkillGridDTO
    {
        public int SkillId { get; set; }
        public int SkillTypeId { get; set; }
        public string SkillTypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region SkillType

    [TSInclude]
    public class SkillTypeDTO
    {
        public int SkillTypeId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class SkillTypeGridDTO
    {
        public int SkillTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    #endregion

    #region Sprint

    public class SprintDTO
    {
        public int SprintId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public DateTime? CodeStopDate { get; set; }
        public DateTime? BetaReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Staffing needs

    #region StaffingNeedsHead

    public class StaffingNeedsHeadSmallDTO
    {
        public int StaffingNeedsHeadId { get; set; }
        public StaffingNeedsHeadType Type { get; set; }
        public int Interval { get; set; }
        public string Name { get; set; }
        public int? DayTypeId { get; set; }
        public DayOfWeek? Weekday { get; set; }
        public DateTime? Date { get; set; }
        public TermGroup_StaffingNeedsHeadStatus Status { get; set; }
    }

    public class StaffingNeedsHeadDTO
    {
        public StaffingNeedsHeadDTO()
        {
            Rows = new List<StaffingNeedsRowDTO>();
        }
        public int StaffingNeedsHeadId { get; set; }
        public int? ParentId { get; set; }
        public StaffingNeedsHeadType Type { get; set; }
        public int Interval { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? FromDate { get; set; }
        public Guid PeriodGuid { get; set; }

        public int? DayTypeId { get; set; }
        public DayOfWeek? Weekday { get; set; }
        public DateTime? Date { get; set; }
        public int? AccountId { get; set; }

        public TermGroup_StaffingNeedsHeadStatus Status { get; set; }

        [TsIgnore]
        public DateTime? Created { get; set; }
        [TsIgnore]
        public string CreatedBy { get; set; }
        [TsIgnore]
        public DateTime? Modified { get; set; }
        [TsIgnore]
        public string ModifiedBy { get; set; }
        [TsIgnore]
        public SoeEntityState State { get; set; }

        // Relations
        public List<StaffingNeedsRowDTO> Rows { get; set; }
        public List<StaffingNeedsHeadUserDTO> StaffingNeedsHeadUsers { get; set; }
    }

    #endregion

    #region StaffingNeedsHeadUser

    public class StaffingNeedsHeadUserDTO
    {
        public int StaffingNeedsHeadUserId { get; set; }
        public int StaffingNeedsHeadId { get; set; }
        public int UserId { get; set; }
        public bool Main { get; set; }

        [TsIgnore]
        public DateTime? Created { get; set; }
        [TsIgnore]
        public string CreatedBy { get; set; }
        [TsIgnore]
        public DateTime? Modified { get; set; }
        [TsIgnore]
        public string ModifiedBy { get; set; }
        [TsIgnore]
        public SoeEntityState State { get; set; }

        // Extensions
        public string LoginName { get; set; }
        public string Name { get; set; }
    }
    #endregion

    #region StaffingNeedsRow

    public class StaffingNeedsRowDTO
    {
        public StaffingNeedsRowDTO()
        {
            Periods = new List<StaffingNeedsRowPeriodDTO>();
        }

        #region Propertys

        public int TempId { get; set; } //not always set
        public int StaffingNeedsRowId { get; set; }
        public int StaffingNeedsHeadId { get; set; }
        public int? ShiftTypeId { get; set; }
        public StaffingNeedsRowType Type { get; set; }
        public StaffingNeedsRowOriginType OriginType { get; set; }
        public string Name { get; set; }

        [TsIgnore]
        public DateTime? Created { get; set; }
        [TsIgnore]
        public string CreatedBy { get; set; }
        [TsIgnore]
        public DateTime? Modified { get; set; }
        [TsIgnore]
        public string ModifiedBy { get; set; }
        [TsIgnore]
        public SoeEntityState State { get; set; }

        // Relations
        public List<StaffingNeedsRowPeriodDTO> Periods { get; set; }
        public List<StaffingNeedsRowFrequencyDTO> RowFrequencys { get; set; }
        public List<StaffingNeedsRowTaskDTO> Tasks { get; set; }

        // Extensions
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }
        [TsIgnore]
        public TimeSpan DayStart { get; set; }
        [TsIgnore]
        public TimeSpan DayEnd { get; set; }
        public int RowNr { get; set; }
        [TsIgnore]
        public bool IsAdded { get; set; }
        public string ToolTip { get; set; }
        public int? StaffingNeedsLocationGroupId { get; set; } //Only for StaffingNeedsCalculationItem

        // From head
        [TsIgnore]
        public int? DayTypeId { get; set; }
        [TsIgnore]
        public DayOfWeek? Weekday { get; set; }
        [TsIgnore]
        public DateTime? Date { get; set; }

        // How many weeks that are visible in ScheduleView
        [TsIgnore]
        public int NbrOfWeeks { get; set; }
        // How many times this row is planned in ScheduleView
        [TsIgnore]
        public int NbrOfPlanned { get; set; }
        // If number of planned has reached number of weeks
        [TsIgnore]
        public bool FullyPlanned { get; set; }

        #endregion

        #region Public methods

        public List<TimeSchedulePlanningDayDTO> GetShifts()
        {
            List<TimeSchedulePlanningDayDTO> shifts = new List<TimeSchedulePlanningDayDTO>();

            if (this.Periods.Count > 0)
            {
                StaffingNeedsRowPeriodDTO firstPeriod = null;   // First period in each block
                StaffingNeedsRowPeriodDTO prevPeriod = null;    // Previous period in loop
                foreach (StaffingNeedsRowPeriodDTO period in this.Periods.OrderBy(p => p.StartTime))
                {
                    if (firstPeriod == null)
                        firstPeriod = period;

                    if (prevPeriod != null && (prevPeriod.ShiftTypeId != period.ShiftTypeId || prevPeriod.StartTime.AddMinutes(period.Interval) != period.StartTime))
                    {
                        // Shift type has changed or there is a hole in schedule, create shift
                        shifts.Add(GetShift(firstPeriod, prevPeriod));
                        firstPeriod = period;
                    }

                    prevPeriod = period;
                }

                // Add last shift
                shifts.Add(GetShift(firstPeriod, prevPeriod));
            }

            return shifts;
        }

        private TimeSchedulePlanningDayDTO GetShift(StaffingNeedsRowPeriodDTO firstPeriod, StaffingNeedsRowPeriodDTO lastPeriod)
        {
            if (firstPeriod == null)
                firstPeriod = lastPeriod;

            TimeSchedulePlanningDayDTO shift = new TimeSchedulePlanningDayDTO()
            {
                Type = TermGroup_TimeScheduleTemplateBlockType.Need,
                ShiftTypeId = firstPeriod.ShiftTypeId ?? 0,
                ShiftTypeName = firstPeriod.ShiftTypeName,
                StartTime = firstPeriod.StartTime,
                StopTime = lastPeriod.StartTime.AddMinutes(lastPeriod.Interval),
                Link = Guid.NewGuid(),
                StaffingNeedsRowId = firstPeriod.StaffingNeedsRowId,
                StaffingNeedsRowPeriodId = firstPeriod.StaffingNeedsRowPeriodId
            };

            return shift;
        }

        public TimeSpan GetBeginningOfDay()
        {
            var period = this.Periods.OrderBy(p => p.StartTime).FirstOrDefault();
            return period != null ? period.StartTime.TimeOfDay : new TimeSpan();
        }

        public TimeSpan GetEndOfDay()
        {
            var period = this.Periods.OrderByDescending(p => p.StartTime).FirstOrDefault();
            return period != null ? period.StartTime.AddMinutes(period.Interval).TimeOfDay : new TimeSpan(23, 59, 0);
        }

        #endregion
    }

    #endregion

    #region StaffingNeedsRowPeriod

    public class StaffingNeedsRowPeriodDTO
    {
        public int StaffingNeedsRowPeriodId { get; set; }
        public int StaffingNeedsRowId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTaskId { get; set; }
        public int? IncomingDeliveryRowId { get; set; }
        public Guid PeriodGuid { get; set; }
        public int Interval { get; set; }
        public DateTime? Date { get; set; }
        public DateTime StartTime { get; set; }
        public decimal Value { get; set; }
        public int Length { get; set; }
        public bool IsBreak { get; set; }
        public StaffingNeedsCalculationTimeSlot TimeSlot { get; set; }
        public int? ParentId { get; set; }
        public bool IsSpecificNeed { get; set; }
        public bool IsRemovedNeed { get; set; }
        public bool IsBaseNeed
        {
            get
            {
                return !this.IsSpecificNeed;
            }
        }

        [TsIgnore]
        public DateTime? Created { get; set; }
        [TsIgnore]
        public string CreatedBy { get; set; }
        [TsIgnore]
        public DateTime? Modified { get; set; }
        [TsIgnore]
        public string ModifiedBy { get; set; }
        [TsIgnore]
        public SoeEntityState State { get; set; }

        // Extensions
        public string ShiftTypeName { get; set; }
        public string ShiftTypeNeedsCode { get; set; }
        public string ShiftTypeColor { get; set; }
    }

    #endregion

    #region StaffingNeedsRowFrequency

    public class StaffingNeedsRowFrequencyDTO
    {
        public int StaffingNeedsRowFrequencyId { get; set; }
        public int StaffingNeedsRowId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int Interval { get; set; }
        public DateTime StartTime { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }

        public DateTime ActualStartTime
        {
            get
            {
                if (StartTime > DateTime.Now.AddYears(-10))
                    return StartTime;

                if (Date > DateTime.Now.AddYears(-10))
                    return CalendarUtility.MergeDateAndTime(Date, StartTime);

                return StartTime;
            }
        }

        public DateTime ActualStopTime
        {
            get
            {
                if (StartTime > DateTime.Now.AddYears(-10))
                    return StartTime.AddMinutes(Interval);

                if (Date > DateTime.Now.AddYears(-10))
                    return CalendarUtility.MergeDateAndTime(Date, StartTime).AddMinutes(Interval);

                return StartTime;
            }
        }

        [TsIgnore]
        public DateTime? Created { get; set; }
        [TsIgnore]
        public string CreatedBy { get; set; }
        [TsIgnore]
        public DateTime? Modified { get; set; }
        [TsIgnore]
        public string ModifiedBy { get; set; }
        [TsIgnore]
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region StaffingNeedsTask

    public class StaffingNeedsTaskDTO
    {
        public int Id { get; set; }
        public SoeStaffingNeedsTaskType Type { get; set; }
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int Length { get; set; }
        public bool IsFixed { get; set; }
        public string Color { get; set; }
        public string RecurrencePattern { get; set; }
        public int? Account2Id { get; set; }
        public int? Account3Id { get; set; }
        public int? Account4Id { get; set; }
        public int? Account5Id { get; set; }
        public int? Account6Id { get; set; }

        // Extensions
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region StaffingNeedsRowTask

    public class StaffingNeedsRowTaskDTO
    {
        public int StaffingNeedsRowTaskId { get; set; }
        public int StaffingNeedsRowId { get; set; }
        public string Task { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }

        [TsIgnore]
        public DateTime? Created { get; set; }
        [TsIgnore]
        public string CreatedBy { get; set; }
        [TsIgnore]
        public DateTime? Modified { get; set; }
        [TsIgnore]
        public string ModifiedBy { get; set; }
        [TsIgnore]
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region StaffingNeedsFrequency

    public class StaffingNeedsFrequencyDTO
    {
        public int StaffingNeedsFrequencyId { get; set; }
        public int ActorCompanyId { get; set; }

        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public int Minutes
        {
            get
            {
                TimeSpan time = TimeTo.Subtract(TimeFrom);
                return (int)decimal.Round((decimal)time.TotalMinutes);
            }
        }
        public decimal AmountPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(Amount, Minutes);
                return 0;
            }
        }

        public decimal CostPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(Cost, Minutes);
                return 0;
            }
        }

        public decimal MinutesPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(NbrOfMinutes, Minutes);
                return 0;
            }
        }

        public decimal ItemsPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(NbrOfItems, Minutes);
                return 0;
            }
        }
        public decimal NbrOfItems { get; set; }
        public decimal NbrOfCustomers { get; set; }
        public int NbrOfMinutes { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }

        public string ExternalCode { get; set; }
        public int? AccountId { get; set; }
        public int? ParentAccountId { get; set; }

        // Extensions
        public int? Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int? Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int? Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int? Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int? Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public bool TempUsed { get; set; }
        public string CompareKey
        {
            get
            {
                return $"{this.AccountId}#{this.TimeFrom}#{this.TimeTo}#{this.ExternalCode}#{this.FrequencyType}#{this.ParentAccountId}";
            }
        }

        public string CompareKeyWithValues
        {
            get
            {
                return CompareKey + $"{this.Amount}#{this.NbrOfCustomers}#{this.NbrOfItems}#{this.NbrOfMinutes}#{this.Cost}";
            }
        }
    }

    #endregion

    #region StaffingNeedsFrequencyIO

    public class StaffingNeedsFrequencyIODTO
    {
        public int StaffingNeedsFrequencyId { get; set; }
        public int ActorCompanyId { get; set; }

        public string DateFrom { get; set; }
        public string TimeFrom { get; set; }
        public string DateTo { get; set; }
        public string TimeTo { get; set; }
        public decimal NbrOfItems { get; set; }
        public decimal NbrOfCustomers { get; set; }
        public decimal NbrOfMinutes { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }

        public string ExternalCode { get; set; }
        public string ParentExternalCode { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public string RowKey { get; set; }
    }

    #endregion

    #region StaffingNeedsLocation

    [TSInclude]
    public class StaffingNeedsLocationDTO
    {
        public int StaffingNeedsLocationId { get; set; }
        public int StaffingNeedsLocationGroupId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalCode { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class StaffingNeedsLocationGridDTO
    {
        public int StaffingNeedsLocationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalCode { get; set; }

        // Extensions
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int GroupAccountId { get; set; }
        public string GroupAccountName { get; set; }
    }

    #endregion

    #region StaffingNeedsLocationGroup

    [TSInclude]
    public class StaffingNeedsLocationGroupDTO
    {
        public int StaffingNeedsLocationGroupId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public int? TimeScheduleTaskId { get; set; }

        // Extensions
        public List<int> ShiftTypeIds { get; set; }
        public string SelectedShiftTypeNames { get; set; }

        public List<StaffingNeedsLocationDTO> StaffingNeedsLocations { get; set; }

        public TimeScheduleTaskDTO TimeScheduleTask { get; set; }

        private Dictionary<string, int?> externalCodeAccountIdDict { get; set; }

        public int? accountId { get; set; }
        public string AccountName { get; set; }
        // Methods

        public int? GetAccountId(List<ShiftTypeDTO> shiftTypes, List<TimeScheduleTaskDTO> timeScheduleTasks, int dimNr)
        {
            if (accountId.HasValue && accountId > 0)
                return accountId;

            TimeScheduleTaskDTO timeScheduleTask = timeScheduleTasks?.FirstOrDefault(w => w.TimeScheduleTaskId == this.TimeScheduleTaskId);
            if (timeScheduleTask != null && timeScheduleTask.ShiftTypeId.HasValue && this.StaffingNeedsLocations != null)
            {
                ShiftTypeDTO shiftType = shiftTypes?.FirstOrDefault(w => w.ShiftTypeId == timeScheduleTask.ShiftTypeId);
                if (shiftType != null && !shiftType.AccountInternals.IsNullOrEmpty())
                {
                    int id = shiftType.AccountInternals.GetAccountId(dimNr);
                    if (id != 0)
                    {
                        this.accountId = id;
                        return id;
                    }
                }
            }

            return null;
        }

        public List<string> GetExternalCodes()
        {
            List<string> codes = new List<string>();

            if (this.StaffingNeedsLocations != null)
            {
                foreach (var item in this.StaffingNeedsLocations.Where(w => !string.IsNullOrEmpty(w.ExternalCode)))
                    codes.Add(item.ExternalCode.ToLower());
            }

            return codes;
        }
    }

    [TSInclude]
    public class StaffingNeedsLocationGroupGridDTO
    {
        public int StaffingNeedsLocationGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SelectedShiftTypeNames { get; set; }
        public int? TimeScheduleTaskId { get; set; }
        public string TimeScheduleTaskName { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region StaffingNeedsRule

    [TSInclude]
    public class StaffingNeedsRuleDTO
    {
        public int StaffingNeedsRuleId { get; set; }
        public int StaffingNeedsLocationGroupId { get; set; }
        public string Name { get; set; }
        public TermGroup_StaffingNeedsRuleUnit Unit { get; set; }
        public int MaxQuantity { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }

        // Extensions
        public List<StaffingNeedsRuleRowDTO> Rows { get; set; }
    }

    [TSInclude]
    public class StaffingNeedsRuleGridDTO
    {
        public int StaffingNeedsRuleId { get; set; }
        public string Name { get; set; }
        public int MaxQuantity { get; set; }

        // Extensions
        public string GroupName { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region StaffingNeedsRuleRow

    [TSInclude]
    public class StaffingNeedsRuleRowDTO
    {
        #region Propertys

        public int StaffingNeedsRuleRowId { get; set; }
        public int StaffingNeedsRuleId { get; set; }
        public int Sort { get; set; }
        public int DayId { get; set; } //Use in Angular
        public int? DayTypeId { get; set; }
        public DayOfWeek? Weekday { get; set; }
        public decimal Value { get; set; }

        // Extensions
        public string DayName { get; set; }

        #endregion
    }

    #endregion

    #region StaffingNeedsCalculationTimeSlot

    public class StaffingNeedsCalculationTimeSlot
    {
        #region Properties

        public DateTime MinFrom { get; set; }
        public DateTime MaxTo { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int? ShiftTypeId { get; set; }
        public bool IsFixed
        {
            get
            {
                return this.From == this.MinFrom && this.To == this.MaxTo;
            }

        }
        public int Minutes
        {
            get
            {
                return Convert.ToInt32((this.To - this.From).TotalMinutes);
            }
        }
        public int TimeSlotLength
        {
            get
            {
                return Convert.ToInt32((this.MaxTo - this.MinFrom).TotalMinutes);
            }
        }
        public DateTime Middle
        {
            get
            {
                return this.MinFrom.AddMinutes(Convert.ToInt32((this.MaxTo - this.MinFrom).TotalMinutes / 2));
            }
        }

        #region TempProp

        public Guid CalculationGuid { get; set; }
        public bool IsBreak { get; set; }

        #endregion

        #endregion

        #region Ctor

        public StaffingNeedsCalculationTimeSlot()
        {
            this.MinFrom = CalendarUtility.DATETIME_DEFAULT;
            this.MaxTo = CalendarUtility.DATETIME_DEFAULT;
            this.From = CalendarUtility.DATETIME_DEFAULT;
            this.To = CalendarUtility.DATETIME_DEFAULT;
        }


        public StaffingNeedsCalculationTimeSlot(DateTime minFrom, DateTime maxTo, DateTime from, DateTime to)
        {
            this.MinFrom = minFrom;
            this.MaxTo = maxTo;
            this.From = from;
            this.To = to;
        }

        public StaffingNeedsCalculationTimeSlot(DateTime minFrom, DateTime maxTo, int lenght)
        {
            this.MinFrom = CalendarUtility.ClearSeconds(minFrom);
            this.MaxTo = CalendarUtility.ClearSeconds(maxTo);
            this.MinFrom = CalendarUtility.AdjustAccordingToInterval(this.MinFrom, lenght, 15, alwaysReduce: false);
            this.MaxTo = CalendarUtility.AdjustAccordingToInterval(this.MaxTo, lenght, 15, alwaysReduce: false);


            if (Convert.ToInt32((this.MaxTo - this.MinFrom).TotalMinutes) == lenght)
            {
                this.From = this.MinFrom;
                this.To = this.MaxTo;
            }
            else // Flexible slot, Put it in the beginning of the slot.
            {
                var from = minFrom; //.AddMinutes((((this.MaxTo - this.MinFrom).TotalMinutes) / 2) - (lenght / 2));
                this.From = this.MinFrom;
                this.To = this.From.AddMinutes(lenght);
            }
        }

        #endregion

        #region Public methods


        public void MoveToEndOfSlot()
        {
            if (!this.IsFixed)
            {
                int length = this.Minutes;
                this.To = this.MaxTo;
                this.From = this.To.AddMinutes(-length);
            }
        }

        public void MoveToStartOfSlot()
        {
            if (!this.IsFixed)
            {
                int length = this.Minutes;
                this.From = this.MinFrom;
                this.To = this.From.AddMinutes(length);
            }
        }

        public void MoveToMiddleOfSlot(int interval)
        {
            if (!this.IsFixed)
            {
                int length = this.Minutes;
                this.From = this.Middle.AddMinutes(-(length / 2));
                this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                this.To = this.From.AddMinutes(length);
            }
        }

        public void MoveForward(int Minutes)
        {
            this.From = this.From.AddMinutes(Minutes);
            this.To = this.To.AddMinutes(Minutes);
        }

        public void MoveBackward(int Minutes)
        {
            this.From = this.From.AddMinutes(-Minutes);
            this.To = this.To.AddMinutes(-Minutes);
        }

        public bool TrySetCloseToMiddleTime(DateTime start, DateTime stop, int interval)
        {
            if (start >= this.MinFrom && start <= this.MaxTo && start.AddMinutes(this.Minutes) <= this.MaxTo)
            {
                int length = this.Minutes;
                var middle = this.MinFrom.AddMinutes(Convert.ToInt32((stop - start).TotalMinutes / 2));

                if (middle >= this.MinFrom && middle <= this.MaxTo && middle.AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else if (middle.AddMinutes(-interval) >= this.MinFrom && middle.AddMinutes(-interval) <= this.MaxTo && middle.AddMinutes(-interval).AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-interval).AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else if (middle.AddMinutes(interval) >= this.MinFrom && middle.AddMinutes(interval) <= this.MaxTo && middle.AddMinutes(interval).AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-interval).AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else if (middle.AddMinutes(Convert.ToInt32(decimal.Multiply(-interval, 2))) >= this.MinFrom && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(-interval, 2))) <= this.MaxTo && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(-interval, 2))).AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-interval).AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else if (middle.AddMinutes(Convert.ToInt32(decimal.Multiply(interval, 2))) >= this.MinFrom && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(interval, 2))) <= this.MaxTo && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(interval, 2))).AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-interval).AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else if (middle.AddMinutes(Convert.ToInt32(decimal.Multiply(-interval, 3))) >= this.MinFrom && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(-interval, 3))) <= this.MaxTo && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(-interval, 3))).AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-interval).AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else if (middle.AddMinutes(Convert.ToInt32(decimal.Multiply(interval, 3))) >= this.MinFrom && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(interval, 3))) <= this.MaxTo && middle.AddMinutes(Convert.ToInt32(decimal.Multiply(interval, 3))).AddMinutes(this.Minutes) <= this.MaxTo)
                {
                    this.From = middle.AddMinutes(-interval).AddMinutes(-(length / 2));
                    this.From = CalendarUtility.AdjustAccordingToInterval(this.From, length, interval, alwaysReduce: true);
                    this.To = this.From.AddMinutes(length);
                }
                else
                    return false;
            }

            return false;
        }

        public bool TrySetStartTime(DateTime start)
        {
            if (start >= this.MinFrom && start <= this.MaxTo && start.AddMinutes(this.Minutes) <= this.MaxTo)
            {
                if (start == this.From)
                    return true;

                int length = this.Minutes;

                this.From = start;
                this.To = start.AddMinutes(length);

                return true;
            }

            return false;
        }

        public bool TrySetStopTime(DateTime stop)
        {
            if (stop >= this.MinFrom && stop <= this.MaxTo && stop.AddMinutes(-this.Minutes) >= this.MinFrom)
            {
                if (stop == this.To)
                    return true;

                int length = this.Minutes;

                this.To = stop;
                this.From = stop.AddMinutes(-length);

                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion

    #endregion

    #region Supplier

    [TSInclude]
    [Log]
    public class SupplierDTO
    {
        [LogActorId]
        public int ActorSupplierId { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public int? PaymentConditionId { get; set; }
        public int? FactoringSupplierId { get; set; }
        public int CurrencyId { get; set; }
        public int? SysCountryId { get; set; }
        public int? SysLanguageId { get; set; }
        public int? VatCodeId { get; set; }
        public int? IntrastatCodeId { get; set; }

        public string SupplierNr { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public string VatNr { get; set; }
        public string InvoiceReference { get; set; }
        public string BIC { get; set; }
        public string OurCustomerNr { get; set; }
        public bool CopyInvoiceNrToOcr { get; set; }
        public bool Interim { get; set; }
        public bool ManualAccounting { get; set; }
        public bool BlockPayment { get; set; }
        public bool IsEDISupplier { get; set; }
        public string RiksbanksCode { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string Note { get; set; }
        public bool ShowNote { get; set; }
        public SoeEntityState State { get; set; }
        public string OurReference { get; set; }
        public int? SysWholeSellerId { get; set; }
        public int? AttestWorkFlowGroupId { get; set; }

        public int? DeliveryConditionId { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? ContactEcomId { get; set; }
        public bool IsEUCountryBased { get; set; }

        // Extensions
        public bool Active { get; set; }
        [LogPrivateSupplier]
        public bool IsPrivatePerson { get; set; }

        public bool HasConsent { get; set; }
        public DateTime? ConsentDate { get; set; }
        public DateTime? ConsentModified { get; set; }
        public string ConsentModifiedBy { get; set; }

        public List<ContactAddressItem> ContactAddresses { get; set; }
        public List<int> ContactPersons { get; set; }
        public List<int> CategoryIds { get; set; }

        public PaymentInformationDTO PaymentInformationForegin { get; set; }
        public PaymentInformationDTO PaymentInformationDomestic { get; set; }

        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> DebitAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> CreditAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> VatAccounts { get; set; }
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> InterimAccounts { get; set; }

        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }

        public AttestWorkFlowHeadDTO TemplateAttestHead { get; set; }
    }

    public class SupplierSmallDTO
    {
        public int ActorSupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string Name { get; set; }
        public int CurrencyId { get; set; }
        public SoeEntityState State { get; set; }
    }
    [TSInclude]
    public class SupplierGridDTO
    {
        public int ActorSupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }

        public List<ContactAddressItem> ContactAddresses { get; set; }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }

        public bool? IsPrivatePerson { get; set; }
    }

    [TSInclude]
    public class SupplierExtendedGridDTO : SupplierGridDTO
    {
        public string OrgNr { get; set; }
        public string Categories { get; set; }
        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string PayToAccount { get; set; }
        public string PaymentCondition { get; set; }
    }
    [TSInclude]
    public class SupplierIODTO
    {
        public int SupplierIOId { get; set; }
        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public TermGroup_IOType Type { get; set; }
        public TermGroup_IOStatus Status { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public string BatchId { get; set; }
        public string ErrorMessage { get; set; }

        public int? SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public string VatNr { get; set; }
        public int? VatType { get; set; }
        public string RiksbanksCode { get; set; }
        public string OurCustomerNr { get; set; }
        public string FactoringSupplierNr { get; set; }
        public string SysCountry { get; set; }
        public string SysLanguage { get; set; }
        public string Currency { get; set; }
        public List<int> CategoryIds { get; set; }

        public List<string> ExternalNbrs { get; set; }

        public int? StandardPaymentType { get; set; }
        public string BankGiroNr { get; set; }
        public string PlusGiroNr { get; set; }
        public string BankNr { get; set; }
        public string BIC { get; set; }
        public string IBAN { get; set; }

        public string DistributionAddress { get; set; }
        public string DistributionCoAddress { get; set; }
        public string DistributionPostalCode { get; set; }
        public string DistributionPostalAddress { get; set; }
        public string DistributionCountry { get; set; }

        public string BillingAddress { get; set; }
        public string BillingCoAddress { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingPostalAddress { get; set; }
        public string BillingCountry { get; set; }

        public string BoardHQAddress { get; set; }
        public string BoardHQCountry { get; set; }

        public string VisitingAddress { get; set; }
        public string VisitingCoAddress { get; set; }
        public string VisitingPostalCode { get; set; }
        public string VisitingPostalAddress { get; set; }
        public string VisitingCountry { get; set; }

        public string DeliveryAddress { get; set; }
        public string DeliveryCoAddress { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string DeliveryPostalAddress { get; set; }
        public string DeliveryCountry { get; set; }

        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string PhoneHome { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneJob { get; set; }
        public string Fax { get; set; }
        public string Webpage { get; set; }

        public string PaymentConditionCode { get; set; }
        public string DeliveryTypeCode { get; set; }
        public string DeliveryConditionCode { get; set; }
        public bool? CopyInvoiceNrToOcr { get; set; }
        public bool? BlockPayment { get; set; }
        public bool? ManualAccounting { get; set; }

        public string AccountsPayableAccountNr { get; set; }
        public string AccountsPayableAccountInternal1 { get; set; }
        public string AccountsPayableAccountInternal2 { get; set; }
        public string AccountsPayableAccountInternal3 { get; set; }
        public string AccountsPayableAccountInternal4 { get; set; }
        public string AccountsPayableAccountInternal5 { get; set; }
        public string AccountsPayableAccountSieDim1 { get; set; }
        public string AccountsPayableAccountSieDim6 { get; set; }

        public string PurchaseAccountNr { get; set; }
        public string PurchaseAccountInternal1 { get; set; }
        public string PurchaseAccountInternal2 { get; set; }
        public string PurchaseAccountInternal3 { get; set; }
        public string PurchaseAccountInternal4 { get; set; }
        public string PurchaseAccountInternal5 { get; set; }
        public string PurchaseAccountSieDim1 { get; set; }
        public string PurchaseAccountSieDim6 { get; set; }

        public string VATAccountNr { get; set; }
        public string VATCodeNr { get; set; }
        public string IntrastatCode { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string StatusName { get; set; }
        public string VatTypeName { get; set; }
        public string StandardPaymentTypeName { get; set; }

        // Flags
        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
    }

    public class SupplierDistributionDTO
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string OrgNr { get; set; }
        public List<PaymentInformationDistributionRowDTO> PaymentInformationRows { get; set; }
    }

    #endregion

    #region Stock

    [TSInclude]
    public class StockDTO
    {
        public int StockId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extension for product edit
        public int Saldo { get; set; }
        public decimal AvgPrice { get; set; }

        public int? StockProductId { get; set; }
        public decimal PurchaseTriggerQuantity { get; set; }
        public decimal PurchaseQuantity { get; set; }
        public int DeliveryLeadTimeDays { get; set; }

        public int StockShelfId { get; set; }
        public string StockShelfName { get; set; }

        public List<StockShelfDTO> StockShelves { get; set; }
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }

        public bool IsExternal { get; set; }

        public int? DeliveryAddressId { get; set; }
        public List<StockProductDTO> StockProducts { get; set; }
    }
    [TSInclude]
    public class StockGridDTO
    {
        public int StockId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsExternal { get; set; }
    }

    #endregion

    #region StockInventory

    [TSInclude]
    public class StockInventoryGridDTO
    {
        public int StockInventoryHeadId { get; set; }
        public string HeaderText { get; set; }
        public DateTime? InventoryStart { get; set; }
        public DateTime? InventoryStop { get; set; }
        public string StockName { get; set; }
        public string CreatedBy { get; set; }
    }

    [TSInclude]
    public class StockInventoryHeadDTO
    {
        public int StockInventoryHeadId { get; set; }

        public DateTime? InventoryStart { get; set; }
        public DateTime? InventoryStop { get; set; }
        public string HeaderText { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int StockId { get; set; }
        public string StockName { get; set; }
        public string StockCode { get; set; }

        //Added this to work in the migrated Angular page
        public List<StockInventoryRowDTO> StockInventoryRows { get; set; }
    }

    [TSInclude]
    public class StockInventoryRowDTO
    {
        public int StockInventoryRowId { get; set; }
        public int StockInventoryHeadId { get; set; }
        public int StockProductId { get; set; }

        public decimal StartingSaldo { get; set; }
        public decimal InventorySaldo { get; set; }
        public decimal Difference { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        //Extensions for productname and number
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductGroupName { get; set; }
        public string Unit { get; set; }
        public decimal AvgPrice { get; set; }
        public int ShelfId { get; set; }
        public string ShelfCode { get; set; }
        public string ShelfName { get; set; }

        public decimal OrderedQuantity { get; set; }
        public decimal ReservedQuantity { get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    [TSInclude]
    public class StockInventoryFilterDTO
    {
        public int StockId { get; set; }
        public int[] ShelfIds { get; set; }
        public int[] ProductGroupIds { get; set; }
        public string ProductNrFrom { get; set; }
        public string ProductNrTo { get; set; }
    }

    #endregion

    #region StockProduct

    [TSInclude]
    public class StockProductSmallDTO
    {
        public int StockProductId { get; set; }
        public int StockId { get; set; }
        public int InvoiceProductId { get; set; }

        public decimal Quantity { get; set; }

        public int? StockShelfId { get; set; }
        public string StockShelfName { get; set; }

        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
    }

    [TSInclude]
    public class StockProductAvgPriceDTO
    {
        public int StockProductId { get; set; }
        public int InvoiceProductId { get; set; }
        public decimal AvgPrice { get; set; }
    }

    [TSInclude]
    public class StockProductDTO
    {
        public int StockProductId { get; set; }
        public int StockId { get; set; }
        public int InvoiceProductId { get; set; }

        public decimal Quantity { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ReservedQuantity { get; set; }

        public bool IsInInventory { get; set; }
        public int? WarningLevel { get; set; }

        public decimal AvgPrice { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int? StockShelfId { get; set; }

        //Extensions
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string ProductUnit { get; set; }
        public string StockName { get; set; }
        public string StockShelfCode { get; set; }
        public string StockShelfName { get; set; }
        public decimal StockValue { get; set; }
        public decimal PurchaseQuantity { get; set; }
        public decimal PurchaseTriggerQuantity { get; set; }
        public decimal PurchasedQuantity { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductGroupName { get; set; }
        public int ProductGroupId { get; set; }
        public decimal TransactionPrice { get; set; }
        public int DeliveryLeadTimeDays { get; set; }
        public string NumberSort
        {
            get
            {
                return StringUtility.IsNumeric(ProductNumber) ? ProductNumber.PadLeft(100, '0') : ProductNumber;
            }
        }
        public int ProductState { get; set; }
    }

    #endregion

    #region StockShelf
    [TSInclude]
    public class StockShelfDTO
    {
        public int StockShelfId { get; set; }
        public int StockId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        //Extensions
        public string StockName { get; set; }
        public bool IsDelete { get; set; }
    }

    #endregion

    #region StockTransaction

    public class StockTransactionSmallDTO
    {
        public int StockTransactionId { get; set; }
        public int StockProductId { get; set; }
        public TermGroup_StockTransactionType ActionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal AvgPrice { get; set; }
      
        public DateTime? TransactionDate { get; set; }
    }

    [TSInclude]
    public class StockTransactionDTO
    {
        public int StockTransactionId { get; set; }
        public int StockProductId { get; set; }
        public int? InvoiceRowId { get; set; }
        public int? InvoiceId { get; set; }
        public TermGroup_StockTransactionType ActionType { get; set; }

        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal AvgPrice { get; set; }
        public string Note { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public int? VoucherId { get; set; }
        public string VoucherNr { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int TargetStockId { get; set; }
        public int? ParentStockTransactionId { get; set; }
        public string ChildStockTransaction { get; set; }

        public string ActionTypeName { get; set; }
        public decimal ReservedQuantity { get; set; }
        public int? ProductUnitConvertId { get; set; }
        public int ProductId { get; set; }
        public int StockId { get; set; }
        public int StockShelfId { get; set; }
        public int? PurchaseId { get; set; }
        public int? StockInventoryHeadId { get; set; }
        public string SourceLabel { get; set; }
        public string SourceNr { get; set; }
        public int? OriginType { get; set; }
    }

    public class StockTransactionExDTO : StockTransactionDTO
    {
        public int? DeliveryNr { get; set; }
        public string StockInventoryNr { get; set; }
        public string InvoiceNr { get; set; }
        public string PurchaseNr { get; set; }
    }
    #endregion

    #region StockPurchase
    [TSInclude]
    public class GenerateStockPurchaseSuggestionDTO
    {
        public TermGroup_StockPurchaseGenerationOptions PurchaseGenerationType { get; set; }
        public string ProductNrFrom { get; set; }
        public string ProductNrTo { get; set; }
        public decimal TriggerQuantityPercent { get; set; }
        public bool ExcludeMissingTriggerQuantity { get; set; }
        public bool ExcludeMissingPurchaseQuantity { get; set; }
        public bool ExcludePurchaseQuantityZero { get; set; }
        public List<int> StockPlaceIds { get; set; }
        public string DefaultDeliveryAddress { get; set; }
        public string Purchaser { get; set; }
    }
    #endregion

    #region SystemInfoLog

    public class SystemInfoLogDTO
    {
        public int SystemInfoLogId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? EmployeeId { get; set; }

        public SystemInfoLogLevel LogLevel { get; set; }
        public SystemInfoType Type { get; set; }

        public SoeEntityType Entity { get; set; }
        public int RecordId { get; set; }

        public DateTime Date { get; set; }
        public string Text { get; set; }
        public bool DeleteManually { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Textblock
    [TSInclude]
    public class TextblockDTOBase
    {
        public int TextblockId { get; set; }
        public string Text { get; set; }

        // Extensions
        public bool IsModified { get; set; }
    }
    [TSInclude]
    public class TextblockDTO : TextblockDTOBase
    {
        public int ActorCompanyId { get; set; }

        public string Headline { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int Type { get; set; }
        public bool ShowInContract { get; set; }
        public bool ShowInOffer { get; set; }
        public bool ShowInOrder { get; set; }
        public bool ShowInInvoice { get; set; }
        public bool ShowInPurchase { get; set; }
    }

    public class TextBlockIODTO
    {
        public int Entity { get; set; }
        public string Text { get; set; }
        public bool IsCustomerInvoice { get; set; }
        public bool IsOrder { get; set; }
        public bool IsContract { get; set; }
        public string LanguageCode { get; set; }
    }

    #endregion

    #region TimeAbsenceRule

    #region TimeAbsenceRuleHead

    public class TimeAbsenceRuleHeadDTO
    {
        #region Propertys

        public int TimeAbsenceRuleHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimeCodeId { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public TermGroup_TimeAbsenceRuleType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public TimeCodeDTO TimeCode { get; set; }
        public string TimeCodeName { get; set; }
        public string EmployeeGroupNames { get; set; }
        public List<TimeAbsenceRuleRowDTO> TimeAbsenceRuleRows { get; set; }
        public string CompanyName { get; set; }

        #endregion
    }

    public class TimeAbsenceRuleHeadGridDTO
    {
        public int TimeAbsenceRuleHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public string EmployeeGroupNames { get; set; }
        public TermGroup_TimeAbsenceRuleType Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region TimeAbsenceRuleRow

    public class TimeAbsenceRuleRowDTO
    {
        public int TimeAbsenceRuleRowId { get; set; }
        public int TimeAbsenceRuleHeadId { get; set; }
        public int? PayrollProductId { get; set; }
        public string PayrollProductNr { get; set; }
        public string PayrollProductName { get; set; }
        public bool HasMultiplePayrollProducts { get; set; }
        public TermGroup_TimeAbsenceRuleRowType Type { get; set; }
        public TermGroup_TimeAbsenceRuleRowScope Scope { get; set; }
        public string TypeName { get; set; }
        public string ScopeName { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeAbsenceRuleRowPayrollProductsDTO> PayrollProductRows { get; set; }
    }

    public class TimeAbsenceRuleRowPayrollProductsDTO
    {
        public int TimeAbsenceRuleRowPayrollProductsId { get; set; }

        public int SourcePayrollProductId { get; set; }
        public string SourcePayrollProductNr { get; set; }
        public string SourcePayrollProductName { get; set; }

        public int? TargetPayrollProductId { get; set; }
        public string TargetPayrollProductNr { get; set; }
        public string TargetPayrollProductName { get; set; }
    }

    #endregion

    #endregion

    #region TimeAccumulator

    public class TimeAccumulatorDTO
    {
        public int TimeAccumulatorId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? TimePeriodHeadId { get; set; }
        public int? TimeCodeId { get; set; }

        public TermGroup_TimeAccumulatorType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool ShowInTimeReports { get; set; }
        public bool FinalSalary { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public bool UseTimeWorkAccount { get; set; }
        public bool UseTimeWorkReductionWithdrawal { get; set; }
        public int? TimeWorkReductionEarningId { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string TimePeriodHeadName { get; set; }

        public List<TimeAccumulatorInvoiceProductDTO> InvoiceProducts { get; set; }
        public List<TimeAccumulatorPayrollProductDTO> PayrollProducts { get; set; }
        public List<TimeAccumulatorTimeCodeDTO> TimeCodes { get; set; }
        public List<TimeAccumulatorEmployeeGroupRuleDTO> EmployeeGroupRules { get; set; }
        public TimeWorkReductionEarningDTO TimeWorkReductionEarning { get; set; }
    }

    public class TimeAccumulatorInvoiceProductDTO
    {
        public int InvoiceProductId { get; set; }
        public decimal Factor { get; set; }
    }

    public class TimeAccumulatorPayrollProductDTO
    {
        public int PayrollProductId { get; set; }
        public decimal Factor { get; set; }
    }

    public class TimeAccumulatorTimeCodeDTO
    {
        public int TimeCodeId { get; set; }
        public decimal Factor { get; set; }
        public bool IsHeadTimeCode { get; set; }
        public bool ImportDefault { get; set; }
    }
    [TSInclude]
    public class TimeAccumulatorEmployeeGroupRuleDTO
    {
        public int EmployeeGroupId { get; set; }
        public TermGroup_AccumulatorTimePeriodType Type { get; set; }
        public int? MinMinutes { get; set; }
        public int? MinTimeCodeId { get; set; }
        public int? MaxMinutes { get; set; }
        public int? MaxTimeCodeId { get; set; }
        public bool ShowOnPayrollSlip { get; set; }
        public int? MinMinutesWarning { get; set; }
        public int? MaxMinutesWarning { get; set; }
        public int? ScheduledJobHeadId { get; set; }
        public int TimeAccumulatorEmployeeGroupRuleId { get; set; }
        public int TimeAccumulatorId { get; set; }
        public SoeEntityState State { get; set; }
        public int? ThresholdMinutes { get; set; }
    }
    [TSInclude]
    public class TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO
    {
        public int TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId { get; set; }
        public int EmployeeGroupId { get; set; }
        public int TimeWorkReductionEarningId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public EmployeeGroupDTO EmployeeGroup { get; set; }
        public TimeWorkReductionEarningDTO TimeWorkReductionEarning { get; set; }
    }
    [TSInclude]
    public class TimeWorkReductionEarningDTO
    {
        public int TimeWorkReductionEarningId { get; set; }
        public int MinutesWeight { get; set; }
        public int PeriodType { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO> TimeAccumulatorTimeWorkReductionEarningEmployeeGroup { get; set; }
        public String TimeAccumulatorName { get; set; }
    }

    public class TimeAccumulatorGridDTO
    {
        public int TimeAccumulatorId { get; set; }
        public TermGroup_TimeAccumulatorType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool ShowInTimeReports { get; set; }
        public string TimePeriodHeadName { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class TimeAccumulatorEmloyeeValue
    {
        public int TimeAccumulatorId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public decimal Sum { get; set; }
    }

    #endregion

    #region TimeBlock

    public class TimeBlockDTO : ITimeBlockObject
    {
        public int TimeBlockId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int? TimeDeviationCauseStartId { get; set; }
        public int? TimeDeviationCauseStopId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeScheduleTemplateBlockBreakId { get; set; }
        public int? TimeBlockGroupId { get; set; }
        public int? AccountId { get; set; }
        public int? EmployeeChildId { get; set; }

        public bool ManuallyAdjusted { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsBreak { get; set; }
        public bool IsPreliminary { get; set; }
        public string Comment { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeChildName { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public List<TimeCodeDTO> TimeCodes { get; set; }
        public List<TimeCodeTransactionDTO> TimeCodeTransactions { get; set; }
        public List<TimeInvoiceTransactionDTO> TimeInvoiceTransactions { get; set; }
        public List<TimePayrollTransactionDTO> TimePayrollTransactions { get; set; }
        public List<int> TransactionTimeCodeIds { get; set; }
        public TimeDeviationCauseDTO TimeDeviationCauseStart { get; set; }
        public TimeDeviationCauseDTO TimeDeviationCauseStop { get; set; }
        public Guid? Guid { get; set; }
        public bool IsAttested { get; set; }
        public bool IsTransferedToSalary { get; set; }
        public bool CopyCommentToExcessBlockIfCreated { get; set; }
        public int? NoOfPresenceTimeCodes { get; set; }
        public int? NoOfAbsenceTimeCodes { get; set; }
        public int? NoOfBreakTimeCodes { get; set; }
        public bool IsGeneratedFromBreak
        {
            get { return TimeScheduleTemplateBlockBreakId.HasValue; }
        }

        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTypeId { get; set; }


        public DateTime? Date { get; set; }
        public DateTime? ActualStartTime
        {
            get { return Date.HasValue ? CalendarUtility.MergeDateAndTime(this.Date.Value.AddDays((this.StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days), this.StartTime) : (DateTime?)null; }
        }

        public DateTime? ActualStopTime
        {
            get { return ActualStartTime.HasValue ? ActualStartTime.Value.AddMinutes((this.StopTime - this.StartTime).TotalMinutes) : (DateTime?)null; }
        }
    }

    public class TimeBlockSmallDTO : ITimeBlockObject
    {
        public int TimeBlockId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int? TimeDeviationCauseStartId { get; set; }
        public int? TimeDeviationCauseStopId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }

    #endregion

    #region TimeBlockDate

    public class TimeBlockDateDTO
    {
        public int TimeBlockDateId { get; set; }
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }
        public SoeTimeBlockDateStatus Status { get; set; }
        public TermGroup_TimeBlockDateStampingStatus StampingStatus { get; set; }

        public bool DiscardedBreakEvaluation { get; set; }
    }

    #endregion

    #region TimeCode
    [TSInclude]
    public class TimeCodeGridDTO
    {
        public int TimeCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateText { get; set; }
        public string TimeCodeBreakGroupName { get; set; }
        public string TimeCodeBreakEmployeeGroupNames { get; set; }
        public string ClassificationText { get; set; }
        public SoeEntityState State { get; set; }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }

        public string PayrollProductNames { get; set; }
    }

    [TSInclude]
    public class TimeCodeDTO : ITimeCodeDTO
    {
        public int ActorCompanyId { get; set; }
        public int TimeCodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Payed { get; set; }
        public bool FactorBasedOnWorkPercentage { get; set; }
        public int MinutesByConstantRules { get; set; }
        public SoeTimeCodeType Type { get; set; }
        public TermGroup_TimeCodeRegistrationType RegistrationType { get; set; }

        public TermGroup_TimeCodeRoundingType RoundingType { get; set; }
        public int RoundingValue { get; set; }
        public int? RoundingTimeCodeId { get; set; }
        public int? RoundingInterruptionTimeCodeId { get; set; }
        public string RoundingGroupKey { get; set; }
        public bool RoundStartTime { get; set; }

        public TermGroup_TimeCodeClassification Classification { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Work
        public bool IsWorkOutsideSchedule { get; set; }

        // Absence
        public int? KontekId { get; set; }
        public bool IsAbsence { get; set; }

        // Break
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public int DefaultMinutes { get; set; }
        public int StartType { get; set; }
        public int StopType { get; set; }
        public DateTime? StartTime { get; set; }
        public int StartTimeMinutes { get; set; }
        public int StopTimeMinutes { get; set; }
        public bool Template { get; set; }
        public int TimeCodeBreakGroupId { get; set; }
        public string TimeCodeBreakGroupName { get; set; }
        public string TimeCodeBreakEmployeeGroupNames { get; set; }

        // Material
        public string Note { get; set; }

        //Relations
        public IEnumerable<TimeCodeRuleDTO> TimeCodeRules { get; set; }
        public List<PayrollProductGridDTO> PayrollProducts { get; set; }
        public string PayrollProductNames { get; set; }

        //Extensions
        public string CompanyName { get; set; }
    }

    [TSInclude]
    public abstract class TimeCodeBaseDTO : ITimeCodeDTO
    {
        public int ActorCompanyId { get; set; }
        public int TimeCodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Payed { get; set; }
        public bool FactorBasedOnWorkPercentage { get; set; }
        public int MinutesByConstantRules { get; set; }
        public SoeTimeCodeType Type { get; set; }
        public TermGroup_TimeCodeRegistrationType RegistrationType { get; set; }
        public TermGroup_TimeCodeRoundingType RoundingType { get; set; }
        public int RoundingValue { get; set; }
        public int? RoundingTimeCodeId { get; set; }
        public int? RoundingInterruptionTimeCodeId { get; set; }
        public string RoundingGroupKey { get; set; }
        public bool RoundStartTime { get; set; }
        public int? TimeCodeRuleType { get; set; }
        public DateTime? TimeCodeRuleTime { get; set; }
        public int? TimeCodeRuleValue { get; set; }

        public TermGroup_TimeCodeClassification Classification { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<TimeCodeInvoiceProductDTO> InvoiceProducts { get; set; }
        public List<TimeCodePayrollProductDTO> PayrollProducts { get; set; }
    }
    [TSInclude]
    public class TimeCodeInvoiceProductDTO
    {
        public int TimeCodeInvoiceProductId { get; set; }
        public int TimeCodeId { get; set; }
        public int InvoiceProductId { get; set; }
        public decimal Factor { get; set; }
        public decimal invoiceProductPrice { get; set; }
    }
    [TSInclude]
    public class TimeCodePayrollProductDTO
    {
        public int TimeCodePayrollProductId { get; set; }
        public int TimeCodeId { get; set; }
        public int PayrollProductId { get; set; }
        public decimal Factor { get; set; }
    }

    public class TimeCodeAbsenceDTO : TimeCodeBaseDTO, ITimeCodeAdjustQuantity
    {
        public int? KontekId { get; set; }
        public bool IsAbsence { get; set; }
        public TermGroup_AdjustQuantityByBreakTime AdjustQuantityByBreakTime { get; set; }
        public int? AdjustQuantityTimeCodeId { get; set; }
        public int? AdjustQuantityTimeScheduleTypeId { get; set; }
    }

    public class TimeCodeAdditionDeductionDTO : TimeCodeBaseDTO
    {
        public TermGroup_ExpenseType ExpenseType { get; set; }
        public string Comment { get; set; }
        public int MinutesByConstantRule { get; set; }
        public bool StopAtDateStart { get; set; }
        public bool StopAtDateStop { get; set; }
        public bool StopAtPrice { get; set; }
        public bool StopAtVat { get; set; }
        public bool StopAtAccounting { get; set; }
        public bool StopAtComment { get; set; }
        public bool CommentMandatory { get; set; }
        public bool HideForEmployee { get; set; }
        public bool ShowInTerminal { get; set; }
        public decimal? FixedQuantity { get; set; }
        // Extensions
        public bool HasInvoiceProducts { get; set; }
    }

    public class TimeCodeBreakDTO : TimeCodeBaseDTO
    {
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public int DefaultMinutes { get; set; }
        public int StartType { get; set; }
        public int StopType { get; set; }
        public DateTime? StartTime { get; set; }
        public int StartTimeMinutes { get; set; }
        public int StopTimeMinutes { get; set; }
        public bool Template { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }

        public List<TimeCodeRuleDTO> TimeCodeRules { get; set; }
        public List<TimeCodeBreakTimeCodeDeviationCauseDTO> TimeCodeDeviationCauses { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
    }

    [TSInclude]
    public class TimeCodeBreakSmallDTO
    {
        public int TimeCodeId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public int DefaultMinutes { get; set; }
        public DateTime? StartTime { get; set; }
        public int StartTimeMinutes { get; set; }
        public int StopTimeMinutes { get; set; }
    }
    [TSInclude]
    public class TimeCodeBreakTimeCodeDeviationCauseDTO
    {
        public int TimeCodeBreakTimeCodeDeviationCauseId { get; set; }
        public int TimeCodeBreakId { get; set; }
        public int TimeCodeDeviationCauseId { get; set; }
        public int TimeCodeId { get; set; }

    }
    [TSInclude]
    public class TimeCodeMaterialDTO : TimeCodeBaseDTO
    {
        public string Note { get; set; }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    public class TimeCodeWorkDTO : TimeCodeBaseDTO, ITimeCodeAdjustQuantity
    {
        public bool IsWorkOutsideSchedule { get; set; }
        public TermGroup_AdjustQuantityByBreakTime AdjustQuantityByBreakTime { get; set; }
        public int? AdjustQuantityTimeCodeId { get; set; }
        public int? AdjustQuantityTimeScheduleTypeId { get; set; }
    }
    [TSInclude]
    public class TimeCodeSaveDTO
    {
        // Base
        public int TimeCodeId { get; set; }
        public SoeTimeCodeType Type { get; set; }
        public TermGroup_TimeCodeRegistrationType RegistrationType { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_TimeCodeRoundingType RoundingType { get; set; }
        public int RoundingValue { get; set; }
        public int? RoundingTimeCodeId { get; set; }
        public int? RoundingInterruptionTimeCodeId { get; set; }
        public string RoundingGroupKey { get; set; }
        public bool RoundStartTime { get; set; }
        public int MinutesByConstantRules { get; set; }
        public bool FactorBasedOnWorkPercentage { get; set; }
        public bool Payed { get; set; }
        public TermGroup_AdjustQuantityByBreakTime AdjustQuantityByBreakTime { get; set; }
        public int? AdjustQuantityTimeCodeId { get; set; }
        public int? AdjustQuantityTimeScheduleTypeId { get; set; }
        public TermGroup_TimeCodeRuleType TimeCodeRuleType { get; set; }
        public DateTime? TimeCodeRuleTime { get; set; }
        public int? TimeCodeRuleValue { get; set; }

        public TermGroup_TimeCodeClassification Classification { get; set; }

        public SoeEntityState State { get; set; }

        public List<TimeCodeInvoiceProductDTO> InvoiceProducts { get; set; }
        public List<TimeCodePayrollProductDTO> PayrollProducts { get; set; }

        // Absence
        public int? KontekId { get; set; }
        public bool IsAbsence { get; set; }

        // AdditionDeduction
        public TermGroup_ExpenseType ExpenseType { get; set; }
        public string Comment { get; set; }
        public bool StopAtDateStart { get; set; }
        public bool StopAtDateStop { get; set; }
        public bool StopAtPrice { get; set; }
        public bool StopAtVat { get; set; }
        public bool StopAtAccounting { get; set; }
        public bool StopAtComment { get; set; }
        public bool CommentMandatory { get; set; }
        public bool HideForEmployee { get; set; }
        public bool ShowInTerminal { get; set; }
        public decimal? FixedQuantity { get; set; }

        // Break
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public int DefaultMinutes { get; set; }
        public int StartType { get; set; }
        public int StopType { get; set; }
        public DateTime? StartTime { get; set; }
        public int StartTimeMinutes { get; set; }
        public int StopTimeMinutes { get; set; }
        public bool Template { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }

        public List<TimeCodeRuleDTO> TimeCodeRules { get; set; }
        public List<TimeCodeBreakTimeCodeDeviationCauseDTO> TimeCodeDeviationCauses { get; set; }
        public List<int> EmployeeGroupIds { get; set; }

        // Material
        public string Note { get; set; }

        // Work
        public bool IsWorkOutsideSchedule { get; set; }
    }

    #region TimeCodeRankning
    [TSInclude]
    public class TimeCodeRankingGroupGridDTO
    {
        public int TimeCodeRankingGroupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }

    }
    [TSInclude]
    public class TimeCodeRankingGroupDTO
    {
        public int TimeCodeRankingGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string Description { get; set; }
        public SoeEntityState State { get; set; }
        // Extensions
        public List<TimeCodeRankingDTO> TimeCodeRankings { get; set; }
    }
    [TSInclude]
    public class TimeCodeRankingDTO
    {
        public int TimeCodeRankingId { get; set; }
        public int ActorCompanyId { get; set; }
        public int LeftTimeCodeId { get; set; }
        public List<int> RightTimeCodeIds { get; set; }
        public string LeftTimeCodeName { get; set; }
        public List<string> RightTimeCodeNames { get; set; }
        public TermGroup_TimeCodeRankingOperatorType OperatorType { get; set; }
    }

    #endregion

    [TSInclude]
    public class TimeCodeRuleDTO : ITimeCodeRule
    {
        public int Type { get; set; }
        public int Value { get; set; }
        public DateTime? Time { get; set; }

        public TimeCodeRuleDTO()
        {

        }

        public TimeCodeRuleDTO(TermGroup_TimeCodeRuleType type, int value, DateTime? time = null)
        {
            this.Type = (int)type;
            this.Value = value;
            this.Time = time;
        }
    }

    #endregion

    #region TimeBreakTemplateDTO

    public class TimeBreakTemplateGridDTO
    {
        public int TimeBreakTemplateId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        public List<DayTypeDTO> DayTypes { get; set; }
        public List<SmallGenericType> DayOfWeeks { get; set; }
        public bool UseMaxWorkTimeBetweenBreaks { get; set; }
        public int ShiftLength { get; set; }
        public int ShiftStartFromTimeMinutes { get; set; }
        public int MinTimeBetweenBreaks { get; set; }
        public int MajorNbrOfBreaks { get; set; }
        public int? MajorTimeCodeBreakGroupId { get; set; }
        public string MajorTimeCodeBreakGroupName { get; set; }
        public int MajorMinTimeAfterStart { get; set; }
        public int MajorMinTimeBeforeEnd { get; set; }
        public int MinorNbrOfBreaks { get; set; }
        public int? MinorTimeCodeBreakGroupId { get; set; }
        public string MinorTimeCodeBreakGroupName { get; set; }
        public int MinorMinTimeAfterStart { get; set; }
        public int MinorMinTimeBeforeEnd { get; set; }

        public int RowNr { get; set; }
        public ActionResult ValidationResult { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class TimeBreakTemplateGridDTONew
    {
        public int TimeBreakTemplateId { get; set; }
        public int RowNr { get; set; }  
        public string ShiftTypeNames { get; set; } 
        public string DayTypeNames { get; set; }  
        public string DayOfWeekNames { get; set; }  
        public int ShiftLength { get; set; }
        public int ShiftStartFromTimeMinutes { get; set; }

        
        public int MajorNbrOfBreaks { get; set; }
        public string MajorTimeCodeBreakGroupName { get; set; }
        public int MajorMinTimeAfterStart { get; set; }
        public int MajorMinTimeBeforeEnd { get; set; }

        
        public int MinorNbrOfBreaks { get; set; }
        public string MinorTimeCodeBreakGroupName { get; set; }
        public int MinorMinTimeAfterStart { get; set; }
        public int MinorMinTimeBeforeEnd { get; set; }

        public int MinTimeBetweenBreaks { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class TimeBreakTemplateDTONew
    {
        public int TimeBreakTemplateId { get; set; }
        public int ActorCompanyId { get; set; }

        
        public List<int> ShiftTypeIds { get; set; }
        public List<int> DayTypeIds { get; set; }
        public List<DayOfWeek> DayOfWeeks { get; set; }

       
        public int ShiftStartFromTime { get; set; }
        public int ShiftLength { get; set; }
        public bool UseMaxWorkTimeBetweenBreaks { get; set; }
        public int MinTimeBetweenBreaks { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }

        
        public int MajorNbrOfBreaks { get; set; }
        public int MajorTimeCodeBreakGroupId { get; set; }
        public int MajorMinTimeAfterStart { get; set; }
        public int MajorMinTimeBeforeEnd { get; set; }

        
        public int MinorNbrOfBreaks { get; set; }
        public int MinorTimeCodeBreakGroupId { get; set; }
        public int MinorMinTimeAfterStart { get; set; }
        public int MinorMinTimeBeforeEnd { get; set; }

       
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class TimeBreakTemplateDTO
    {
        public int TimeBreakTemplateId { get; set; }
        public int ActorCompanyId { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public int NrOfShiftTypeIds { get { return this.ShiftTypeIds != null ? this.ShiftTypeIds.Count : 0; } }
        public List<int> DayTypeIds { get; set; }
        public List<DayOfWeek> DayOfWeeks { get; set; }
        public DateTime? ShiftStartFromTime { get; set; }
        public int ShiftLength { get; set; }
        public int ShiftLengthNet
        {
            get
            {
                return this.ShiftLength - this.BreakLength;
            }
        }
        public int BreakLength
        {
            get
            {
                return this.GetTemplateRows().Sum(i => i.Length);
            }
        }
        public bool UseMaxWorkTimeBetweenBreaks { get; set; }
        public int MinTimeBetweenBreaks { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeBreakTemplateRowDTO> TimeBreakTemplateRows { get; set; }
    }

    #endregion

    #region TimeBreakTemplateRowDTO

    public class TimeBreakTemplateRowDTO
    {
        public int TimeBreakTemplateRowId { get; set; }
        public int TimeBreakTemplateId { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }
        public SoeTimeBreakTemplateType Type { get; set; }
        public int MinTimeAfterStart { get; set; }
        public int MinTimeBeforeEnd { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extension
        public Guid Guid { get; set; }
        public int Length { get; set; }
    }

    #endregion

    #region TimeCodeBreakGroup
    [TSInclude]
    public class TimeCodeBreakGroupGridDTO
    {
        public int TimeCodeBreakGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    [TSInclude]
    public class TimeCodeBreakGroupDTO
    {
        public int TimeCodeBreakGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        [TSIgnore]
        public List<TimeCodeDTO> TimeCodeBreaks { get; set; }
    }

    #endregion

    #region TimeCodeTransaction

    public class TimeCodeTransactionDTO
    {
        #region Propertys

        public int TimeCodeTransactionId { get; set; }
        public int? TimeBlockId { get; set; }
        public int? TimeRuleId { get; set; }
        public int TimeCodeId { get; set; }
        public int? CustomerInvoiceRowId { get; set; }
        public int? ProjectId { get; set; }
        public int? ProjectInvoiceDayId { get; set; }
        public int? SupplierInvoiceId { get; set; }
        public int? TimeBlockDateId { get; set; }
        public SoeTimeCodeType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal Vat { get; set; }
        public decimal VatCurrency { get; set; }
        public decimal VatEntCurrency { get; set; }
        public decimal VatLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public string Comment { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public string TimeCodeName { get; set; }
        public string TimeCodeTypeName { get; set; }
        public string QuantityText { get; set; }
        public string TypeName { get; set; }
        public int? EmployeeId { get; set; }

        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public int AttestStateId { get; set; }

        public bool IsRegistrationTypeQuantity { get; set; }
        public bool IsRegistrationTypeTime { get; set; }
        public bool IsAddition { get; set; }
        public bool IsProvision { get; set; }
        public bool IsEarnedHoliday { get; set; }
        public bool IsReadOnly { get; set; }

        public List<TimePayrollTransactionDTO> TimePayrollTransactionItems { get; set; }
        public List<TimeInvoiceTransactionDTO> TimeInvoiceTransactionItems { get; set; }
        public List<TimeCodeTransactionDetailsDTO> TimeCodeTransactionDetailsDTOItems { get; set; }

        #endregion
    }

    public class TimeCodeTransactionDetailsDTO
    {
        public string PayrollProductString { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityText { get; set; }
        public decimal? Amount { get; set; }
        public decimal? VatAmount { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public int AttestStateId { get; set; }
    }

    public class TimeCodeTransactionIODTO
    {
        public string TimeCodeCode { get; set; }
        public string ProductNr { get; set; }
        public string PayrollProductNr { get; set; }
        public string ProductName { get; set; }
        public string PayrollProductName { get; set; }
        public string CustomerInvoiceNr { get; set; }
        public string ProjectNr { get; set; }
        public string EmployeeNr { get; set; }
        public int? EmployeeId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierInvoiceNr { get; set; }
        public SoeTimeCodeType Type { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal Vat { get; set; }
        public decimal VatCurrency { get; set; }
        public decimal VatEntCurrency { get; set; }
        public decimal VatLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }
        public decimal InvoiceQuantity { get; set; }

        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public string Comment { get; set; }
        public string ExternalComment { get; set; }

        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountNrSieDim1 { get; set; }
        public string AccountNrSieDim6 { get; set; }

        public string TimeCodeName { get; set; }

        public string TimeCodeTypeName { get; set; }
        public string QuantityText { get; set; }

        public string PayrollAttestStateName { get; set; }

        public bool IsRegistrationTypeQuantity { get; set; }
        public bool IsRegistrationTypeTime { get; set; }
        public bool IsAddition { get; set; }
        public bool IsReadOnly { get; set; }

        public bool ManuallyAdded { get; set; }
        public bool Exported { get; set; }

        public string Formula { get; set; }             // As stored in field Formula (with ID's)
        public string FormulaPlain { get; set; }        // As stored in field FormulaPlain (with codes)
        public string FormulaExtracted { get; set; }    // Formula extracted with values
        public string FormulaNames { get; set; }        // FormulaPlain with codes replaced with names
        public string FormulaOrigin { get; set; }       // Descriptive text where the values of the formula was fetched from

        public int TimeCodeTransactionId { get; set; }

        public List<TimePayrollTransactionIODTO> TimePayrollTransactionIOItems { get; set; }
        public List<TimeInvoiceTransactionIODTO> TimeInvoiceTransactionIOItems { get; set; }
    }


    #endregion

    #region TimeDeviationCause
    [TSInclude]
    public class TimeDeviationCauseDTO
    {
        public int TimeDeviationCauseId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? TimeCodeId { get; set; }
        public TermGroup_TimeDeviationCauseType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExtCode { get; set; }
        public string ImageSource { get; set; }
        public int EmployeeRequestPolicyNbrOfDaysBefore { get; set; }
        public bool EmployeeRequestPolicyNbrOfDaysBeforeCanOverride { get; set; }
        public int AttachZeroDaysNbrOfDaysBefore { get; set; }
        public int AttachZeroDaysNbrOfDaysAfter { get; set; }
        public int ChangeCauseOutsideOfPlannedAbsence { get; set; }
        public int ChangeCauseInsideOfPlannedAbsence { get; set; }
        public bool ChangeDeviationCauseAccordingToPlannedAbsence { get; set; }
        public int AdjustTimeOutsideOfPlannedAbsence { get; set; }
        public int AdjustTimeInsideOfPlannedAbsence { get; set; }
        public bool AllowGapToPlannedAbsence { get; set; }
        public bool ShowZeroDaysInAbsencePlanning { get; set; }
        public bool IsVacation { get; set; }
        public bool Payed { get; set; }
        public bool NotChargeable { get; set; }
        public bool OnlyWholeDay { get; set; }
        public bool SpecifyChild { get; set; }
        public bool ExcludeFromPresenceWorkRules { get; set; }
        public bool ExcludeFromScheduleWorkRules { get; set; }
        public bool ValidForStandby { get; set; }
        public bool ValidForHibernating { get; set; }
        public bool CandidateForOvertime { get; set; }
        public bool CalculateAsOtherTimeInSales { get; set; }
        public bool MandatoryNote { get; set; }
        public bool MandatoryTime { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string TimeCodeName { get; set; }
        public bool IsAbsence { get; set; }
        public bool IsPresence { get; set; }
        public TimeCodeDTO TimeCode { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public List<string> ExternalCodes
        {
            get
            {
                return this.GetExternalCodes();
            }
        }

        private List<string> GetExternalCodes()
        {
            if (string.IsNullOrEmpty(this.ExtCode))
                return new List<string>();
            return this.ExtCode.Split('#').ToList();
        }
    }
    [TSInclude]
    public class TimeDeviationCauseGridDTO
    {
        public int TimeDeviationCauseId { get; set; }
        public TermGroup_TimeDeviationCauseType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageSource { get; set; }
        public bool SpecifyChild { get; set; }
        public bool ValidForHibernating { get; set; }
        public bool ValidForStandby { get; set; }
        public bool MandatoryNote { get; set; }
        public bool CandidateForOvertime { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string TimeCodeName { get; set; }
    }

    #endregion

    #region TimeHalfday

    public class TimeHalfdayDTO
    {
        public int TimeHalfdayId { get; set; }
        public int DayTypeId { get; set; }

        public SoeTimeHalfdayType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeCodeDTO> TimeCodeBreaks { get; set; }

        // Extensions
        public string DayTypeName { get; set; }
        public string TypeName { get; set; }
    }

    [TSInclude]
    public class TimeHalfdayGridDTO
    {
        public int TimeHalfdayId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public string DayTypeName { get; set; }
        public string TypeName { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class TimeHalfdayEditDTO
    {
        public int TimeHalfdayId { get; set; }
        public int DayTypeId { get; set; }

        public SoeTimeHalfdayType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<int> TimeCodeBreakIds { get; set; }
    }

    #endregion

    #region TimeHibernatingAbsenceHead

    public class TimeHibernatingAbsenceHeadDTO
    {
        public int TimeHibernatingAbsenceHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int EmploymentId { get; set; }
        public int? TimeDeviationCauseId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extension
        public string EmployeeName { get; set; }
        public string EmployeeNr { get; set; }
        public EmploymentDTO Employment { get; set; }
        public List<TimeHibernatingAbsenceRowDTO> Rows { get; set; }
    }

    #endregion

    #region TimeHibernatingAbsenceRow

    public class TimeHibernatingAbsenceRowDTO
    {
        public int TimeHibernatingAbsenceRowId { get; set; }
        public int TimeHibernatingAbsenceHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? EmploymeeChildId { get; set; }
        public DateTime Date { get; set; }
        public int ScheduleTimeMinutes { get; set; }
        public int AbsenceTimeMinutes { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public TimeHibernatingAbsenceHeadDTO TimeHibernatingAbsenceHead { get; set; }
    }


    #endregion

    #region TimeInvoiceTransaction

    public class TimeInvoiceTransactionDTO
    {
        public int TimeInvoiceTransactionId { get; set; }
        public int? TimeCodeTransactionId { get; set; }
        public int? TimeBlockId { get; set; }
        public int? EmployeeId { get; set; }
        public int? TimeBlockDateId { get; set; }
        public int InvoiceProductId { get; set; }
        public int AccountId { get; set; }
        public int? AttestStateId { get; set; }
        public int? CustomerInvoiceRowId { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }
        public decimal InvoiceQuantity { get; set; }

        public bool Invoice { get; set; }
        public bool ManuallyAdded { get; set; }
        public bool Exported { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public AttestStateDTO AttestState { get; set; }
        public AccountDTO AccountStd { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }
    }

    public class TimeInvoiceTransactionIODTO
    {
        public string ProductNr { get; set; }
        public string InvoiceNr { get; set; }
        public DateTime Date { get; set; }
        public string EmployeeNr { get; set; }
        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }
        public decimal InvoiceQuantity { get; set; }

        public bool Invoice { get; set; }
        public bool ManuallyAdded { get; set; }
        public bool Exported { get; set; }

        public string AttestStateName { get; set; }

        public int TimeInvoiceTransactionId { get; set; }
    }

    #endregion

    #region TimeLeisureCode

    [TSInclude]
    public class TimeLeisureCodeDTO
    {
        public int TimeLeisureCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_TimeLeisureCodeType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class TimeLeisureCodeSmallDTO
    {
        public int TimeLeisureCodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    [TSInclude]
    public class TimeLeisureCodeGridDTO
    {
        public int TimeLeisureCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_TimeLeisureCodeType Type { get; set; }
        public string TypeName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Lesiure Automatic Allocation

    public class AutomaticAllocationResultDTO
    {
        public List<AutomaticAllocationEmployeeResultDTO> EmployeeResults = new List<AutomaticAllocationEmployeeResultDTO>();
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class AutomaticAllocationEmployeeResultDTO
    {
        public int EmployeeId { get; set; }
        public List<AutomaticAllocationEmployeeDayResultDTO> DayResults = new List<AutomaticAllocationEmployeeDayResultDTO>();
        public LeisureCodeAllocationEmployeeStatus Status { get; set; }
        public string Message { get; set; }
    }

    public class AutomaticAllocationEmployeeDayResultDTO
    {
        public DateTime Date { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    #endregion

    #region TimePayrollTransaction

    public class TimePayrollTypeSumDTO : IPayrollType
    {
        #region Variables

        public int? VacationYearEndRowId { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public decimal Quantity { get; set; }
        public int? InvoiceQuantity { get; }
        public decimal? Amount { get; set; }
        public bool PayrollProductPayed { get; set; }
        public bool IsAdded { get; set; }
        public bool IsAdditionOrDeduction { get; set; }

        #endregion

        #region Ctor

        public TimePayrollTypeSumDTO()
        {

        }

        public TimePayrollTypeSumDTO(IPayrollTransactionProc transactionItem)
        {
            if (transactionItem != null)
            {
                this.Quantity = transactionItem.Quantity;
                this.InvoiceQuantity = transactionItem.InvoiceQuantity;
                this.Amount = transactionItem.Amount;
                this.SysPayrollTypeLevel1 = transactionItem.SysPayrollTypeLevel1;
                this.SysPayrollTypeLevel2 = transactionItem.SysPayrollTypeLevel2;
                this.SysPayrollTypeLevel3 = transactionItem.SysPayrollTypeLevel3;
                this.SysPayrollTypeLevel4 = transactionItem.SysPayrollTypeLevel4;
                this.PayrollProductPayed = transactionItem.PayrollProductPayed;
                this.IsAdded = transactionItem.IsAdded;
                this.IsAdditionOrDeduction = transactionItem.IsAdditionOrDeduction;
            }
        }

        #endregion
    }

    public class TimePayrollTransactionDTO : IPayrollType
    {
        public int TimePayrollTransactionId { get; set; }
        public int? TimeCodeTransactionId { get; set; }
        public int? TimeBlockId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int PayrollProductId { get; set; }
        public int AccountId { get; set; }
        public int AttestStateId { get; set; }
        public int? PayrollStartValueRowId { get; set; }

        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }

        public DateTime? Date { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceCurrency { get; set; }
        public decimal UnitPriceEntCurrency { get; set; }
        public decimal UnitPriceLedgerCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }

        public bool ManuallyAdded { get; set; }
        public bool AutoAttestFailed { get; set; }
        public bool Exported { get; set; }
        public bool IsPreliminary { get; set; }
        public bool IsFixed { get; set; }
        public bool IsExtended { get; set; }
        public bool IsAdded { get; set; }
        public string Comment { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public TimePayrollTransactionExtendedDTO Extended { get; set; }
        public TimeCodeTransactionDTO TimeCodeTransaction { get; set; }

        // Extensions
        public AttestStateDTO AttestState { get; set; }
        public AccountDTO AccountStd { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }
        /// <summary>
        /// TimeBlockStartTime
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// TimeBlockStopTime
        /// </summary>
        public DateTime? StopTime { get; set; }
        public DateTime? TimeCodeTransactionStartTime { get; set; }
        public DateTime? TimeCodeTransactionStopTime { get; set; }
        public int? TimeDeviationCauseStartId { get; set; }
        public int? TimeDeviationCauseStopId { get; set; }
        public bool TempUsed { get; set; }

        //Matrix
        public AccountAndInternalAccountComboDTO AccountAndInternalAccountCombo { get; set; }
        public decimal AbsenceRatio { get; set; }
        public bool? ScheduleTransaction { get; set; }
        public SoeTimePayrollScheduleTransactionType? ScheduleTransactionType { get; set; }

        //Methods

        public string GetAccountingIdString()
        {
            string str = this.AccountId.ToString();

            if (this.AccountInternals != null)

                foreach (var ai in this.AccountInternals)
                    str += $"|{ai.AccountId}";

            return str;
        }

        private List<int> accountInternalIds { get; set; }
        private string accountInternalIdsString { get; set; }
        public decimal TempAmountEmploymentTax { get; set; }
        public decimal TempAmountSupplementCharge { get; set; }
        public bool ForceTempAmountSupplementCharge { get; set; } = false;

        public string GetAccountInternalIdsString()
        {
            if (accountInternalIdsString != null)
                return accountInternalIdsString;
            if (accountInternalIds == null)
                GetAccountInternalIds();
            accountInternalIdsString = accountInternalIds.JoinToString("_");
            return accountInternalIdsString;
        }

        public List<int> GetAccountInternalIds()
        {
            if (accountInternalIds != null)
                return accountInternalIds;

            accountInternalIds = this.AccountInternals?.OrderBy(o => o.AccountId).Select(ai => ai.AccountId).ToList() ?? new List<int>();

            return accountInternalIds;
        }
    }

    public class TimePayrollTransactionExtendedDTO
    {
        public int TimePayrollTransactionId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? PayrollPriceFormulaId { get; set; }

        public string Formula { get; set; }             // As stored in field Formula (with ID's)
        public string FormulaPlain { get; set; }        // As stored in field FormulaPlain (with codes)
        public string FormulaExtracted { get; set; }    // Formula extracted with values
        public string FormulaNames { get; set; }        // FormulaPlain with codes replaced with names
        public string FormulaOrigin { get; set; }       // Descriptive text where the values of the formula was fetched from

        public decimal QuantityWorkDays { get; set; }
        public decimal QuantityCalendarDays { get; set; }
        public decimal CalenderDayFactor { get; set; }
        public int TimeUnit { get; set; }

        public bool PayrollCalculationPerformed { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class TimePayrollTransactionCompactDTO
    {
        public int TimePayrollTransactionId { get; set; }
        public DateTime Date { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductNrAndName
        {
            get { return String.Format("{0} {1}", this.ProductNr, this.ProductName); }
        }
        public decimal Quantity { get; set; }
        public decimal Unitprice { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
    }

    #endregion

    #region TimePayrollStatistics

    [SysTermGroupAttribute(TermGroup.General)]
    public class TimePayrollStatisticsDTO : EntryWithAttachedStuff, IPayrollType
    {
        //TimePayrollTransaction
        public int TimePayrollTransactionId { get; set; }
        public int EmployeeId { get; set; }
        public int? RetroactivePayrollOutcomeId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceCurrency { get; set; }
        public decimal UnitPriceEntCurrency { get; set; }
        public decimal UnitPriceLedgerCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public string SysPayrollTypeLevel1Name { get; set; }
        public string SysPayrollTypeLevel2Name { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public string SysPayrollTypeLevel4Name { get; set; }
        public bool ManuallyAdded { get; set; }
        public bool AutoAttestFailed { get; set; }
        public bool Exported { get; set; }
        public bool IsPreliminary { get; set; }
        public bool IsScheduleTransaction { get; set; }
        public bool IsCentrounding { get; set; }
        public SoeTimePayrollScheduleTransactionType? ScheduleTransactionType { get; set; }
        public bool IsEmploymentTaxBelowLimitHidden { get; set; }
        public string Comment { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public decimal AbsenceRatio { get; set; }
        //TimePayrollTrasactionExtended
        public decimal QuantityWorkDays { get; set; }
        public decimal QuantityCalendarDays { get; set; }
        public decimal CalenderDayFactor { get; set; }
        public int TimeUnit { get; set; }
        public string Formula { get; set; }             // As stored in field Formula (with ID's)
        public string FormulaPlain { get; set; }        // As stored in field FormulaPlain (with codes)
        public string FormulaExtracted { get; set; }    // Formula extracted with values
        public string FormulaNames { get; set; }        // FormulaPlain with codes replaced with names
        public string FormulaOrigin { get; set; }       // Descriptive text where the values of the formula was fetched from
        public bool PayrollCalculationPerformed { get; set; }
        public bool IsDistributed { get; set; }

        public bool IsPayrollStartValues { get; set; }

        //PayrollProduct
        public int PayrollProductId { get; set; }
        [SysTermAttribute(-1, "Löneartsnamn")]
        public string PayrollProductName { get; set; }
        [SysTermAttribute(-1, "Löneartsnummer")]
        public string PayrollProductNumber { get; set; }
        [SysTermAttribute(-1, "Beskrivning")]
        public string PayrollProductDescription { get; set; }

        public TermGroup_PensionCompany PensionCompany { get; set; }

        //TimeCodeTransaction
        public int? TimeCodeTransactionId { get; set; }
        [SysTermAttribute(-1, "Tidkod")]
        public string TimeCodeNumber { get; set; }
        [SysTermAttribute(-1, "Tidskodsnamn")]
        public string TimeCodeName { get; set; }
        public string TimeCodeDescription { get; set; }

        //AttestState
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }

        //TimeBlockDate
        public int TimeBlockDateId { get; set; }
        [DatabaseFieldAttribute("TimeBlockDate.Date")]
        [SysTermAttribute(-1, "Datum")]
        public DateTime TimeBlockDate { get; set; }

        public bool IsFromOtherPeriod { get; set; }

        //TimeBlock
        public int TimeBlockId { get; set; }
        [SysTermAttribute(-1, "Starttid")]
        public DateTime TimeBlockStartTime { get; set; }
        [SysTermAttribute(-1, "Stopptid")]
        public DateTime TimeBlockStopTime { get; set; }
        public DateTime? PaymentDate { get; set; }

        //Employment
        public int WorkTimeWeek { get; set; }

        //EmployeeGroup
        [SysTermAttribute(-1, "Anställningsgrupp")]
        public string EmployeeGroupName { get; set; }
        public int EmployeeGroupWorkTimeWeek { get; set; }



        //PayrollGroup
        public int PayrollGroupId { get; set; }
        public string PayrollGroupName { get; set; }

        //Accounting
        public int AccountId { get; set; }
        public int? Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public int? Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int? Dim2SIENr { get; set; }
        public int? Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int? Dim3SIENr { get; set; }
        public int? Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int? Dim4SIENr { get; set; }
        public int? Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int? Dim5SIENr { get; set; }
        public int? Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public int? Dim6SIENr { get; set; }
        public string AccountString { get; set; }
        public TermGroup_PayrollResultType ResultType { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }

    }

    public class TimePayrollStatisticsSmallDTO : EntryWithAttachedStuff, IPayrollType
    {
        //TimePayrollTransaction
        public int TransactionId { get; set; }
        public bool IsScheduleTransaction { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public int? TimePeriodId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }
        public bool IsBelowEmploymentTaxLimitRuleHidden { get; set; }
        public bool IsBelowEmploymentTaxLimitRuleFromPreviousPeriods { get; set; }
        public bool IsEmploymentTaxAndHidden { get; set; }
        public bool IsPayrollStartValues { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public string SysPayrollTypeLevel1Name { get; set; }
        public string SysPayrollTypeLevel2Name { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public string SysPayrollTypeLevel4Name { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int ForaCollectiveAgreementId { get; set; }
        public string KPAAgreementNumber { get; set; }
        public string KPAAgreementType { get; set; }
        public int GTPAgreementNumber { get; set; }
        public int PayrollGroupId { get; set; }

        //TimePayrollTrasactionExtended
        public decimal QuantityWorkDays { get; set; }
        public decimal QuantityCalendarDays { get; set; }
        public decimal CalenderDayFactor { get; set; }

        //PayrollProduct
        public int PayrollProductId { get; set; }
        [SysTermAttribute(-1, "Löneartsnamn")]
        public string PayrollProductName { get; set; }
        [SysTermAttribute(-1, "Löneartsnummer")]
        public string PayrollProductNumber { get; set; }
        [SysTermAttribute(-1, "Beskrivning")]
        public string PayrollProductDescription { get; set; }
        public TermGroup_PensionCompany PensionCompany { get; set; }
        public string TimePeriodName { get; set; }
        public TermGroup_PayrollResultType ResultType { get; set; }
        public DateTime Date { get; set; }
        public EmploymentTypeDTO EmploymentTypeDTO { get; set; }

        //Report

        public string GetUniqueId(List<EmploymentDTO> mergedEmployments)
        {
            if (!mergedEmployments.IsNullOrEmpty())
            {
                var matchedOnDate = mergedEmployments.FirstOrDefault(w => (w.DateFrom <= Date && !w.DateTo.HasValue) || (w.DateTo.HasValue && w.DateTo.Value >= Date));

                if (matchedOnDate != null)
                    return matchedOnDate.UniqueId;
                return "uq";
            }
            return string.Empty;
        }
    }

    public class TimePayrollTransactionIODTO
    {
        public string PayrollProductNr { get; set; }
        public string TimeCodeCode { get; set; }
        public DateTime Date { get; set; }
        public string AttestStateName { get; set; }
        public string EmployeeNr { get; set; }
        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal UnitPriceCurrency { get; set; }
        public decimal UnitPriceEntCurrency { get; set; }
        public decimal UnitPriceLedgerCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity { get; set; }

        public bool ManuallyAdded { get; set; }
        public bool Exported { get; set; }
        public string Comment { get; set; }

        public string Formula { get; set; }             // As stored in field Formula (with ID's)
        public string FormulaPlain { get; set; }        // As stored in field FormulaPlain (with codes)
        public string FormulaExtracted { get; set; }    // Formula extracted with values
        public string FormulaNames { get; set; }        // FormulaPlain with codes replaced with names
        public string FormulaOrigin { get; set; }       // Descriptive text where the values of the formula was fetched from

        public int TimePayrollTransactionId { get; set; }
    }

    #endregion

    #region PayrollVoucher

    #region PayrollVoucherHead

    public class PayrollVoucherHeadDTO
    {
        public PayrollVoucherHeadDTO()
        {
            Rows = new List<PayrollVoucherRowDTO>();
        }
        public int VoucherNr { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public bool Template { get; set; }
        public bool TypeBalance { get; set; }
        public bool VatVoucher { get; set; }
        public TermGroup_AccountStatus Status { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string Note { get; set; }

        // Extensions
        public int VoucherSeriesTypeId { get; set; }
        public string VoucherSeriesTypeName { get; set; }
        public List<PayrollVoucherRowDTO> Rows { get; set; }
    }

    #endregion

    #region PayrollVoucherRow

    public class PayrollVoucherRowDTO : IVoucherRowDTO
    {
        public PayrollVoucherRowDTO()
        {
            PayrollVoucherRowOriginDTOs = new List<PayrollVoucherRowOriginDTO>();
            AccountInternalDTO_forReports = new List<AccountInternalDTO>();
            AccountInternals = new List<AccountInternalDTO>();
        }
        public int TimePayrollTransactionAccountId { get; set; }
        public int TimePayrollTransactionId { get; set; }
        public int TimePayrollScheduleTransactionId { get; set; }
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        public int EmployeeCalculateVacationResultId { get; set; }
        public int PreviousEmployeeCalculateVacationResultHeadId { get; set; }
        public int PreviousEmployeeCalculateVacationResultId { get; set; }
        public DateTime? Date { get; set; }
        public string Text { get; set; }
        public decimal? Quantity { get; set; }
        public bool SkipQuantity { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public string accountString { get; set; }
        public int SignGroup
        {
            get
            {
                return this.Amount > 0 ? 1 : 0;
            }
        }
        public bool Merged { get; set; }


        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int Dim2SIENr { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int Dim3SIENr { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int Dim4SIENr { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int Dim5SIENr { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public int Dim6SIENr { get; set; }

        public string AccountDistributionName { get; set; }
        public List<PayrollVoucherRowOriginDTO> PayrollVoucherRowOriginDTOs { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }
        public int? AccountDistributionHeadId { get; set; }
        public List<AccountInternalDTO> AccountInternalDTO_forReports { get; set; }
        public int Dim1AmountStop { get; set; }
        public bool Dim1UnitStop { get; set; }
        public int? ParentRowId { get; set; }
        public int? RowNr { get; set; }
        public SoeEntityState State { get; set; }
        public int TempRowId { get; set; }
        public int VoucherHeadId { get; set; }
        public long VoucherNr { get; set; }
        public int VoucherRowId { get; set; }
        public string VoucherSeriesTypeName { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public int? TimeCodeTransactionId { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }

    }

    public class PayrollVoucherRowOriginDTO
    {
        public int TimePayrollTransactionAccountId { get; set; }
        public int TimePayrollTransactionId { get; set; }
        public int TimePayrollScheduleTransactionId { get; set; }
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        public int EmployeeCalculateVacationResultId { get; set; }
        public int PreviousEmployeeCalculateVacationResultHeadId { get; set; }
        public int PreviousEmployeeCalculateVacationResultId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DateTime { get; set; }
        public string EmployeeNr { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        new public string ToString()
        {
            return $"EmployeeId:{EmployeeId}Date:{DateTime.ToShortDateString()}Code:{Code}Amount:{Amount}Quantity{Quantity}";
        }
    }

    #endregion

    #endregion

    #region TimePeriod
    [TSInclude]
    public class TimePeriodDTO
    {
        public int TimePeriodId { get; set; }
        public int TimePeriodHeadId { get; set; }
        public TimePeriodHeadDTO TimePeriodHead { get; set; }

        public int RowNr { get; set; }
        public string Name { get; set; }
        public bool ExtraPeriod { get; set; }
        public string Comment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime? PayrollStartDate { get; set; }
        public DateTime? PayrollStopDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime SortDate
        {
            get
            {
                return PaymentDate ?? StopDate;
            }
        }
        public bool HasRequiredPayrollProperties
        {
            get
            {
                return this.PaymentDate.HasValue && this.PayrollStartDate.HasValue && this.PayrollStopDate.HasValue;
            }
        }
        public string PaymentDateString
        {
            get
            {
                if (PaymentDate.HasValue)
                    return (this.ExtraPeriod ? "* " : string.Empty) + (PaymentDate.Value.ToShortDateString() + " (" + this.Name + ")");
                else
                    return string.Empty;
            }
        }
        public string NameAndPaymentDate
        {
            get
            {
                string text = this.Name;
                if (this.ExtraPeriod)
                    text += " *";

                if (PaymentDate.HasValue)
                    text += " (" + PaymentDate.Value.ToShortDateString() + ")";

                return text;
            }
        }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public bool IsModified { get; set; }
        public bool ShowAsDefault { get; set; }
    }

    #endregion

    #region TimePeriodHead
    [TSInclude]
    public class TimePeriodHeadDTO
    {
        public int TimePeriodHeadId { get; set; }
        public int ActorCompanyId { get; set; }

        public TermGroup_TimePeriodType TimePeriodType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public int? AccountId { get; set; }
        public int? ChildId { get; set; }
        public int? PayrollProductDistributionRuleHeadId { get; set; }

        // Extensions
        public string TimePeriodTypeName { get; set; }
        public List<TimePeriodDTO> TimePeriods { get; set; }
    }
    [TSInclude]
    public class TimePeriodHeadGridDTO
    {
        public int TimePeriodHeadId { get; set; }
        public TermGroup_TimePeriodType TimePeriodType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Extensions
        public string TimePeriodTypeName { get; set; }
        public string AccountName { get; set; }
        public string ChildName { get; set; }
        public int? PayrollProductDistributionRuleHeadId { get; set; }
    }

    #endregion

    #region TimePeriodAccountValue
    [TSInclude]
    public class AccountProvisionBaseDTO
    {
        public int TimePeriodAccountValueId { get; set; }   // TimePeriodAccountValueId of Period12
        public int TimePeriodId { get; set; }               // TimePeriodId of Period12

        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public string AccountDescription { get; set; }

        public decimal Period1Value { get; set; }
        public decimal Period2Value { get; set; }
        public decimal Period3Value { get; set; }
        public decimal Period4Value { get; set; }
        public decimal Period5Value { get; set; }
        public decimal Period6Value { get; set; }
        public decimal Period7Value { get; set; }
        public decimal Period8Value { get; set; }
        public decimal Period9Value { get; set; }
        public decimal Period10Value { get; set; }
        public decimal Period11Value { get; set; }
        public decimal Period12Value { get; set; }

        public bool IsLocked { get; set; }
        public bool IsModified { get; set; }

        public void SetValue(int position, decimal value)
        {
            switch (position)
            {
                case 1:
                    this.Period1Value = value;
                    break;
                case 2:
                    this.Period2Value = value;
                    break;
                case 3:
                    this.Period3Value = value;
                    break;
                case 4:
                    this.Period4Value = value;
                    break;
                case 5:
                    this.Period5Value = value;
                    break;
                case 6:
                    this.Period6Value = value;
                    break;
                case 7:
                    this.Period7Value = value;
                    break;
                case 8:
                    this.Period8Value = value;
                    break;
                case 9:
                    this.Period9Value = value;
                    break;
                case 10:
                    this.Period10Value = value;
                    break;
                case 11:
                    this.Period11Value = value;
                    break;
                case 12:
                    this.Period12Value = value;
                    break;
            }
        }
    }

    public class TimePeriodAccountValueGridDTO
    {
        public int TimePeriodAccountValueId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimePeriodId { get; set; }
        public int AccountId { get; set; }
        public SoeTimePeriodAccountValueType Type { get; set; }
        public SoeTimePeriodAccountValueStatus Status { get; set; }
        public decimal Value { get; set; }
        public bool IsModified { get; set; }
    }

    public class TimePeriodAccountValueDTO
    {
        public int TimePeriodAccountValueId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimePeriodId { get; set; }
        public int AccountId { get; set; }
        public SoeTimePeriodAccountValueType Type { get; set; }
        public SoeTimePeriodAccountValueStatus Status { get; set; }
        public decimal Value { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #region TimePeriodAccountValue
    [TSInclude]
    public class AccountProvisionTransactionGridDTO
    {
        public int TimePayrollTransactionId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public string AccountDescription { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public string Comment { get; set; }
        public string Formula { get; set; }
        public string FormulaPlain { get; set; }
        public string FormulaNames { get; set; }
        public string FormulaExtracted { get; set; }
        public string FormulaOrigin { get; set; }
        public string IsModified { get; set; }
    }

    #endregion


    #endregion

    #region TimeRule

    public class TimeRuleDTO
    {
        #region Propertys

        public int TimeRuleId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimeCodeId { get; set; }
        public int? BalanceRuleSettingId { get; set; }
        public SoeTimeRuleType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public int RuleStartDirection { get; set; }
        public int RuleStopDirection { get; set; }
        public int? TimeCodeMaxLength { get; set; }
        public bool TimeCodeMaxPerDay { get; set; }
        public bool BelongsToGroup { get; set; }
        public bool Internal { get; set; }
        public bool IsInconvenientWorkHours { get; set; }
        public bool IsStandby { get; set; }
        public decimal Factor { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool Imported { get; set; }
        public int? StandardMinutes { get; set; }
        public bool BreakIfAnyFailed { get; set; }
        public bool AdjustStartToTimeBlockStart { get; set; }
        public int SourceId { get; set; }
        public string SourceName { get; set; }
        public string CompanyName { get; set; }
        public string TimeCodeName { get; set; }
        public List<int> TimeDeviationCauseIds { get; set; }
        public string TimeDeviationCauseNames { get; set; }
        public List<int> DayTypeIds { get; set; }
        public string DayTypeNames { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public string EmployeeGroupNames { get; set; }
        public List<int> TimeScheduleTypeIds { get; set; }
        public string TimeScheduleTypeNames { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public TimeCodeDTO TimeCode { get; set; }
        public Dictionary<int, string> TimeDeviationCausesDict = new Dictionary<int, string>();
        public Dictionary<int, string> DayTypesDict = new Dictionary<int, string>();
        public List<TimeRuleExpressionDTO> TimeRuleExpressions { get; set; }
        public BalanceRuleSettingDTO BalanceRuleSetting { get; set; }
        public List<TimeCodeDTO> UsedTimeCodesByRule = new List<TimeCodeDTO>();
        public Guid BalanceRuleSettingTempId { get; set; }  // Used when saving time rules connected to BalanceRuleSetting

        #endregion
    }

    public class TimeRuleRowDTO
    {
        #region Propertys

        public int TimeRuleRowId { get; set; }
        public int TimeRuleId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int? DayTypeId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public int? TimeScheduleTypeId { get; set; }

        #endregion
    }

    public class TimeRuleExpressionDTO : ITimeRuleExpression
    {
        public int TimeRuleExpressionId { get; set; }
        public int TimeRuleId { get; set; }
        public bool IsStart { get; set; }
        public List<TimeRuleOperandDTO> TimeRuleOperands { get; set; }
    }

    public class TimeRuleOperandDTO : ITimeRuleOperand
    {
        public int TimeRuleOperandId { get; set; }
        public int TimeRuleExpressionId { get; set; }
        public int? TimeRuleExpressionRecursiveId { get; set; }
        public SoeTimeRuleOperatorType OperatorType { get; set; }
        public SoeTimeRuleValueType LeftValueType { get; set; }
        public int? LeftValueId { get; set; }
        public SoeTimeRuleValueType RightValueType { get; set; }
        public int? RightValueId { get; set; }
        public int Minutes { get; set; }
        public SoeTimeRuleComparisonOperator ComparisonOperator { get; set; }
        public int OrderNbr { get; set; }

        // Extensions
        public TimeRuleExpressionDTO TimeRuleExpressionRecursive { get; set; }
    }

    public class TimeRuleEditDTO
    {
        public int TimeRuleId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public bool IsInconvenientWorkHours { get; set; }
        public bool IsStandby { get; set; }

        public SoeTimeRuleType Type { get; set; }
        public int RuleStartDirection { get; set; }
        public int RuleStopDirection { get; set; }
        public decimal Factor { get; set; }
        public int TimeCodeId { get; set; }
        public int? TimeCodeMaxLength { get; set; }
        public bool TimeCodeMaxPerDay { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool Imported { get; set; }
        public int? StandardMinutes { get; set; }
        public bool BreakIfAnyFailed { get; set; }
        public bool AdjustStartToTimeBlockStart { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<int> EmployeeGroupIds { get; set; }
        public List<int> TimeScheduleTypeIds { get; set; }
        public List<int> TimeDeviationCauseIds { get; set; }
        public List<int> DayTypeIds { get; set; }

        public List<TimeRuleExpressionDTO> TimeRuleExpressions { get; set; }
        public TimeRuleEditIwhDTO InconvenientWorkHourRule { get; set; }

        public List<TimeRuleExportImportUnmatchedDTO> ExportImportUnmatched { get; set; }
        public string ExportStartExpression { get; set; }
        public string ExportStopExpression { get; set; }
        public string ImportStartExpression { get; set; }
        public string ImportStopExpression { get; set; }
    }

    public class TimeRuleImportedDetailsDTO
    {
        public string CompanyName { get; set; }
        public DateTime? Imported { get; set; }
        public string ImportedBy { get; set; }
        public string Json { get; set; }
        public string OriginalJson { get; set; }
    }

    public class TimeRuleGridDTO
    {
        public int TimeRuleId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int Sort { get; set; }
        public SoeTimeRuleType Type { get; set; }
        public string TypeName { get; set; }
        public SoeTimeRuleDirection StartDirection { get; set; }
        public string StartDirectionName { get; set; }
        public bool Internal { get; set; }
        public string IsInconvenientWorkHours { get; set; }
        public string IsStandby { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int? TimeCodeMaxLength { get; set; }
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public string EmployeeGroupNames { get; set; }
        public string DayTypeNames { get; set; }
        public string TimeScheduleTypesNames { get; set; }
        public string TimeDeviationCauseNames { get; set; }
        public string StartExpression { get; set; }
        public string StopExpression { get; set; }
        public bool Imported { get; set; }
        public string StandardMinutes { get; set; }
        public string BreakIfAnyFailed { get; set; }
        public string AdjustStartToTimeBlockStart { get; set; }
    }

    public class TimeRuleFormulaWidget
    {
        public SoeTimeRuleOperatorType TimeRuleType { get; set; }
        public int Sort { get; set; }
        public bool IsOperator { get; set; }
        public bool IsExpression { get; set; }

        // Data
        public int? Minutes { get; set; }
        public SoeTimeRuleComparisonOperator? ComparisonOperator { get; set; }
        public int? LeftValueId { get; set; }
        public int? RightValueId { get; set; }
    }

    #endregion

    #region TimeRuleIwhDTO

    public class TimeRuleIwhDTO
    {
        public int ActorCompanyId { get; set; }
        public int TimeRuleId { get; set; }
        public string Name { get; set; }
        public DateTime? RuleStartDate { get; set; }
        public DateTime? RuleStopDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string Length { get; set; }
        public List<int> TimeDeviationCauseIds { get; set; }
        public List<int?> DayTypeIds { get; set; }
        public List<int?> EmployeeGroupIds { get; set; }
        public List<int?> TimeScheduleTypeIds { get; set; }
        public int PayrollProductId { get; set; }
        public string PayrollProductName { get; set; }
        public decimal PayrollProductFactor { get; set; }
        public int NrOfTimeCodePayrollProducts { get; set; }
        public string Information { get; set; }
        public string PayrollProductExternalCode { get; set; }
    }

    public class TimeRuleEditIwhDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string Length { get; set; }
        public string PayrollProductName { get; set; }
        public decimal PayrollProductFactor { get; set; }
        public string Information { get; set; }
    }

    #endregion

    #region TimeRuleExportImport

    public class TimeRuleExportImportDTO
    {
        // Actual time rules exported
        public List<TimeRuleEditDTO> TimeRules { get; set; }

        // Relational data needed to match on import
        public List<TimeRuleExportImportTimeCodeDTO> TimeCodes { get; set; }
        public List<TimeRuleExportImportEmployeeGroupDTO> EmployeeGroups { get; set; }
        public List<TimeRuleExportImportTimeScheduleTypeDTO> TimeScheduleTypes { get; set; }
        public List<TimeRuleExportImportTimeDeviationCauseDTO> TimeDeviationCauses { get; set; }
        public List<TimeRuleExportImportDayTypeDTO> DayTypes { get; set; }

        // Information needed for storage
        public string OriginalJson { get; set; }
        public string Filename { get; set; }
        public string ExportedFromCompany { get; set; }

    }

    public class TimeRuleExportImportTimeCodeDTO
    {
        public int TimeCodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int MatchedTimeCodeId { get; set; }
    }

    public class TimeRuleExportImportEmployeeGroupDTO
    {
        public int EmployeeGroupId { get; set; }
        public string Name { get; set; }
        public int MatchedEmployeeGroupId { get; set; }
    }

    public class TimeRuleExportImportTimeScheduleTypeDTO
    {
        public int TimeScheduleTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int MatchedTimeScheduleTypeId { get; set; }
    }

    public class TimeRuleExportImportTimeDeviationCauseDTO
    {
        public int TimeDeviationCauseId { get; set; }
        public TermGroup_TimeDeviationCauseType Type { get; set; }
        public string Name { get; set; }
        public int MatchedTimeDeviationCauseId { get; set; }
    }

    public class TimeRuleExportImportDayTypeDTO
    {
        public int DayTypeId { get; set; }
        public int? SysDayTypeId { get; set; }
        public TermGroup_SysDayType Type { get; set; }
        public string Name { get; set; }
        public int MatchedDayTypeId { get; set; }
    }

    public class TimeRuleExportImportUnmatchedDTO
    {
        public TimeRuleExportImportUnmatchedType Type { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public TimeRuleExportImportUnmatchedDTO(TimeRuleExportImportUnmatchedType type, int id, string code, string name)
        {
            this.Type = type;
            this.Id = id;
            this.Code = code;
            this.Name = name;
        }
    }

    #endregion

    #region TimeSalaryExport

    public class TimeSalaryExportDTO
    {
        public int TimeSalaryExportId { get; set; }
        public int ActorCompanyId { get; set; }

        public DateTime StartInterval { get; set; }
        public DateTime StopInterval { get; set; }
        public DateTime ExportDate { get; set; }
        public SoeTimeSalaryExportTarget ExportTarget { get; set; }
        public SoeTimeSalaryExportFormat ExportFormat { get; set; }

        public byte[] File1 { get; set; }
        public byte[] File2 { get; set; }
        public string Extension { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public string Comment { get; set; }

        public bool IsPreliminary { get; set; }

        // Extensions
        public string TargetName { get; set; }
        public string IsPreliminaryText { get; set; }
    }

    #endregion

    #region TimeSalaryPaymentExport

    public class TimeSalaryPaymentExportDTO
    {
        #region Propertys

        public int TimeSalaryPaymentExportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimePeriodId { get; set; }
        public DateTime ExportDate { get; set; }
        public TermGroup_TimeSalaryPaymentExportType ExportType { get; set; }
        public SoeTimeSalaryPaymentExportFormat ExportFormat { get; set; }
        public byte[] File { get; set; }
        public string Extension { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        [TsIgnore]
        public bool EmployeeDetailsVisible { get; set; }
        public List<TimeSalaryPaymentExportEmployeeDTO> TimeSalaryPaymentExportEmployees { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string TimePeriodName { get; set; }
        public string TimePeriodHeadName { get; set; }
        public DateTime PaymentDate { get; set; }
        public String PaymentDateString { get; set; }
        public String PayrollDateInterval { get; set; }
        public bool IsSelected { get; set; }
        public bool HasEmployeeDetails
        {
            get
            {
                return this.TimeSalaryPaymentExportEmployees != null && this.TimeSalaryPaymentExportEmployees.Any();
            }
        }
        public decimal CashDepositNetAmount
        {
            get
            {
                return this.TimeSalaryPaymentExportEmployees.Where(x => x.IsSECashDeposit).Sum(x => x.NetAmount);
            }
        }
        public decimal AccountDepositNetAmount
        {
            get
            {
                return this.TimeSalaryPaymentExportEmployees.Where(x => x.DisbursementMethod.HasValue && !x.IsSECashDeposit).Sum(x => x.NetAmount);
            }
        }
        public decimal AccountDepositNetAmountCurrency
        {
            get
            {
                return this.TimeSalaryPaymentExportEmployees.Where(x => x.DisbursementMethod.HasValue && x.IsSEExtendedSelection).Sum(x => x.NetAmountCurrency);
            }
        }

        #endregion
    }

    public class TimeSalaryPaymentExportGridDTO
    {
        public int TimeSalaryPaymentExportId { get; set; }
        public DateTime ExportDate { get; set; }
        public string TimePeriodHeadName { get; set; }
        public string TimePeriodName { get; set; }
        public String PayrollDateInterval { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? DebitDate { get; set; }
        public TermGroup_TimeSalaryPaymentExportType ExportType { get; set; }
        public string TypeName { get; set; }
        public string MsgKey { get; set; }
        public string PaymentKey { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime? CurrencyDate { get; set; }
        public decimal? CurrencyRate { get; set; }

        public DateTime? SalarySpecificationPublishDate { get; set; }
        public decimal CashDepositNetAmount
        {
            get { return this.Employees.Where(x => x.IsSECashDeposit).Sum(x => x.NetAmount); }
        }

        public decimal AccountDepositNetAmount
        {
            get { return this.Employees.Where(x => x.DisbursementMethod.HasValue && !x.IsSECashDeposit).Sum(x => x.NetAmount); }
        }
        public decimal AccountDepositNetAmountCurrency
        {
            get
            {
                return this.Employees.Where(x => x.DisbursementMethod.HasValue && x.IsSEExtendedSelection).Sum(x => x.NetAmountCurrency);
            }
        }
        public List<TimeSalaryPaymentExportEmployeeDTO> Employees { get; set; }
    }

    public class TimeSalaryPaymentExportEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string AccountNr { get; set; }
        public string PaymentRowKey { get; set; }
        public string AccountNrGridStr
        {
            get
            {
                return this.IsSECashDeposit ? this.DisbursementMethodName : this.AccountNr;
            }
        }
        public decimal NetAmount { get; set; }
        public decimal NetAmountCurrency { get; set; }
        public int? DisbursementMethod { get; set; }
        public bool IsSECashDeposit
        {
            get
            {
                return (this.DisbursementMethod.HasValue && this.DisbursementMethod.Value == (int)TermGroup_EmployeeDisbursementMethod.SE_CashDeposit);
            }
        }
        public bool IsSEExtendedSelection
        {
            get
            {
                return (this.DisbursementMethod.HasValue && this.DisbursementMethod.Value == (int)TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount);
            }
        }
        //Extensions
        public string Name { get; set; }
        public string EmployeeNr { get; set; }
        public string DisbursementMethodName { get; set; }
    }

    #endregion

    #region TimeScheduleCopy

    public class TimeScheduleCopyHeadDTO
    {
        public int TimeScheduleCopyHeadId { get; set; }
        public TermGroup_TimeScheduleCopyHeadType Type { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int UserId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<TimeScheduleCopyRowDTO> Rows { get; set; }
    }

    public class TimeScheduleCopyRowDTO
    {
        public int TimeScheduleCopyRowId { get; set; }
        public int EmployeeId { get; set; }
        public TermGroup_TimeScheduleCopyRowType Type { get; set; }
        public string JsonData { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class TimeScheduleCopyRowJsonDataDTO
    {
        public List<TimeScheduleCopyRowJsonDataShiftDTO> Shifts { get; set; }
    }

    public class TimeScheduleCopyRowJsonDataShiftDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public TimeScheduleBlockType Type { get; set; }
        public DateTime? Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int? ShiftTypeId { get; set; }
        public bool IsBreak { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? AccountId { get; set; }
        public int? TimeScheduleEmployeePeriodDetailId { get; set; }
        public int? TimeLeisureCodeId { get; set; }
        public TermGroup_TimeScheduleTemplateBlockAbsenceType AbsenceType { get; set; }
    }

    #endregion

    #region TimeScheduleEmployeePeriodDetail

    public class TimeScheduleEmployeePeriodDetailDTO
    {
        public int TimeScheduleEmployeePeriodDetailId { get; set; }
        public int TimeScheduleEmployeePeriodId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public SoeTimeScheduleEmployeePeriodDetailType Type { get; set; }
        public int? TimeLeisureCodeId { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
    }

    #endregion

    #region TimeScheduleEvent
    [TSInclude]
    public class TimeScheduleEventDTO
    {
        public int TimeScheduleEventId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeScheduleEventMessageGroupDTO> TimeScheduleEventMessageGroups { get; set; }
    }

    public class TimeScheduleEventForPlanningDTO
    {
        public int TimeScheduleEventId { get; set; }
        public int OpeningHoursId { get; set; }

        public DateTime Date { get; set; }
        public DateTime? OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
    }

    [TSInclude]
    public class TimeScheduleEventGridDTO
    {
        public int TimeScheduleEventId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RecipientGroupNames { get; set; }
    }

    #endregion

    #region TimeScheduleEventMessageGroup
    [TSInclude]
    public class TimeScheduleEventMessageGroupDTO
    {
        public int TimeScheduleEventMessageGroupId { get; set; }
        public int TimeScheduleEventId { get; set; }
        public int MessageGroupId { get; set; }
    }

    #endregion

    #region TimeScheduleScenarioAccount

    public class TimeScheduleScenarioAccountDTO
    {
        public int TimeScheduleScenarioAccountId { get; set; }
        public int TimeScheduleScenarioHeadId { get; set; }
        public int AccountId { get; set; }

        //Extensions
        public string AccountName { get; set; }
    }

    #endregion

    #region TimeScheduleScenarioEmployee

    public class TimeScheduleScenarioEmployeeDTO
    {
        public int TimeScheduleScenarioEmployeeId { get; set; }
        public int TimeScheduleScenarioHeadId { get; set; }
        public int EmployeeId { get; set; }


        //Extensions
        public string EmployeeName { get; set; }
        public string EmployeeNumberAndName { get; set; }

        public bool NeedsReplacement { get; set; }
        public int? ReplacementEmployeeId { get; set; }
        public string ReplacementEmployeeNumberAndName { get; set; }
    }

    #endregion

    #region TimeScheduleScenarioHead

    public class TimeScheduleScenarioHeadDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public TermGroup_TimeScheduleScenarioHeadSourceType SourceType { get; set; }
        public DateTime? SourceDateFrom { get; set; }
        public DateTime? SourceDateTo { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeScheduleScenarioEmployeeDTO> Employees { get; set; }
        public List<TimeScheduleScenarioAccountDTO> Accounts { get; set; }

    }


    #endregion

    #region ActivateScenario

    public class PreviewActivateScenarioDTO
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string ShiftTextSchedule { get; set; }
        public string ShiftTextScenario { get; set; }
        public SoeScheduleWorkRules WorkRule { get; set; }
        public string WorkRuleName { get; set; }
        public string WorkRuleText { get; set; }
        public bool WorkRuleCanOverride { get; set; }
        public string StatusName { get; set; }
        public string StatusMessage { get; set; }
    }

    public class ActivateScenarioDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public DateTime? PreliminaryDateFrom { get; set; }
        public bool SendMessage { get; set; }
        public List<ActivateScenarioRowDTO> Rows { get; set; }
        public string Key
        {
            get
            {

                return $"{TimeScheduleScenarioHeadId}#{PreliminaryDateFrom}#{SendMessage}";
            }
        }
    }

    public class ActivateScenarioRowDTO
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string Key
        {
            get
            {

                return $"{EmployeeId}";
            }
        }
    }

    public class PreviewCreateTemplateFromScenarioDTO
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public DateTime? TemplateDateFrom { get; set; }
        public DateTime? TemplateDateTo { get; set; }
        public string ShiftTextScenario { get; set; }
        public SoeScheduleWorkRules WorkRule { get; set; }
        public string WorkRuleName { get; set; }
        public string WorkRuleText { get; set; }
        public bool WorkRuleCanOverride { get; set; }
    }

    public class CreateTemplateFromScenarioDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int WeekInCycle { get; set; }
        public List<CreateTemplateFromScenarioRowDTO> Rows { get; set; }
    }

    public class CreateTemplateFromScenarioRowDTO
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
    }

    #endregion

    #region TimeScheduleSwap
    public class TimeScheduleSwapApproveViewDTO
    {
        public string InitiatorEmployeeName { get; set; }
        public string InitiatorEmployeeNumber { get; set; }
        public int InitiatorEmployeeId { get; set; }
        public string SwapWithEmployeeName { get; set; }
        public string SwapWithEmployeeNumber { get; set; }
        public int SwapWithEmployeeId { get; set; }
        public bool ValidSkills { get; set; }
        public string ValidSkillsMessage { get; set; }
        public int InitiatorUserId { get; set; }
        public string Comment { get; set; }
        public TermGroup_TimeScheduleSwapRequestStatus Status { get; set; }
        public SoeEntityState State { get; set; }
        public bool ShiftsHasChanged { get; set; }
        public bool Admin { get; set; }
        public bool DifferentLength { get; set; }
        public string DifferentLengthMessage { get; set; }

        //TimeScheduelSwapRows
        public List<TimeScheduleSwapRequestRowDTO> InitiatorEmployeeRows { get; set; }
        public List<TimeScheduleSwapRequestRowDTO> SwapWithEmployeeRows { get; set; }
        public List<TimeScheduleSwapRequestRowDTO> CurrentInitiatorEmployeeRows { get; set; }
        public List<TimeScheduleSwapRequestRowDTO> CurrentSwapWithEmployeeRows { get; set; }

        public TimeScheduleSwapApproveViewDTO()
        {
            this.InitiatorEmployeeRows = new List<TimeScheduleSwapRequestRowDTO>();
            this.SwapWithEmployeeRows = new List<TimeScheduleSwapRequestRowDTO>();
            this.CurrentInitiatorEmployeeRows = new List<TimeScheduleSwapRequestRowDTO>();
            this.CurrentSwapWithEmployeeRows = new List<TimeScheduleSwapRequestRowDTO>();
        }

        public bool IsSwapWithEmployeeRowsApprovedByEmployee()
        {
            return this.SwapWithEmployeeRows.All(w => w.Status == TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByEmployee);
        }
    }
    #endregion

    #region TimeScheduleTask

    [TSInclude]
    public class TimeScheduleTaskDTO
    {
        public int TimeScheduleTaskId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTaskTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int Length { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int? NbrOfOccurrences { get; set; }
        public string RecurrencePattern { get; set; }

        public bool OnlyOneEmployee { get; set; }
        public bool DontAssignBreakLeftovers { get; set; }
        public bool AllowOverlapping { get; set; }
        public int MinSplitLength { get; set; }
        public int NbrOfPersons { get; set; }
        public bool IsStaffingNeedsFrequency { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }

        // Relations
        public int? Account2Id { get; set; }
        public int? Account3Id { get; set; }
        public int? Account4Id { get; set; }
        public int? Account5Id { get; set; }
        public int? Account6Id { get; set; }

        // Extensions
        public string RecurrencePatternDescription { get; set; }
        public string RecurrenceStartsOnDescription { get; set; }
        public string RecurrenceEndsOnDescription { get; set; }
        public DailyRecurrenceDatesOutput RecurringDates { get; set; }
        public List<DateTime> ExcludedDates { get; set; }
        public string AccountName { get; set; }

        public void SetAccountId(int position, int accountId)
        {
            if (position == 2)
                this.Account2Id = accountId;
            else if (position == 3)
                this.Account3Id = accountId;
            else if (position == 4)
                this.Account4Id = accountId;
            else if (position == 5)
                this.Account5Id = accountId;
            else if (position == 6)
                this.Account6Id = accountId;
        }
    }

    [TSInclude]
    public class TimeScheduleTaskGeneratedNeedDTO
    {
        public int StaffingNeedsRowPeriodId { get; set; }
        public int StaffingNeedsRowId { get; set; }
        public DayOfWeek? WeekDay { get; set; }
        public DateTime? Date { get; set; }
        public string Type { get; set; }
        public string Occurs { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

    }

    [TSInclude]
    public class TimeScheduleTaskGridDTO
    {
        public int TimeScheduleTaskId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int Length { get; set; }
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
        public bool OnlyOneEmployee { get; set; }
        public bool DontAssignBreakLeftovers { get; set; }
        public bool AllowOverlapping { get; set; }
        public int NbrOfPersons { get; set; }
        public bool IsStaffingNeedsFrequency { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string RecurrencePatternDescription { get; set; }
        public string RecurrenceStartsOnDescription { get; set; }
        public string RecurrenceEndsOnDescription { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region TimeScheduleTaskType

    [TSInclude]
    public class TimeScheduleTaskTypeDTO
    {
        public int TimeScheduleTaskTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    [TSInclude]
    public class TimeScheduleTaskTypeGridDTO
    {
        public int TimeScheduleTaskTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region TimeScheduleTemplateBlock

    [TSInclude]
    public class TimeScheduleTemplateBlockDTO
    {
        #region Constants

        public const int MIN_BREAK = 1;
        public const int MAX_BREAK = 4;
        public const int MIN_ACCOUNTDIM = 2;
        public const int MAX_ACCOUNTDIM = 6;

        #endregion

        #region Propertys

        public int TimeScheduleTemplateBlockId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeScheduleEmployeePeriodId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? StaffingNeedsRowPeriodId { get; set; }
        public SoeTimeScheduleDeviationCauseStatus TimeDeviationCauseStatus { get; set; }
        public int? EmployeeId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }
        public int DayNumber { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public TimeSpan Length { get; set; }
        public DateTime? Date { get; set; }
        public DateTime ActualDate { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftStatus ShiftStatus { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus ShiftUserStatus { get; set; }
        public int? CustomerInvoiceId { get; set; }
        public int? ProjectId { get; set; }
        public bool IsPreliminary { get; set; }
        public int? PlannedTime { get; set; }
        public SoeEntityState State { get; set; }

        //Midnight secure
        public DateTime? ActualStartTime
        {
            get { return Date.HasValue ? CalendarUtility.MergeDateAndTime(this.Date.Value.AddDays((this.StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days), this.StartTime) : (DateTime?)null; }
        }
        //Midnight secure
        public DateTime? ActualStopTime
        {
            get { return ActualStartTime.HasValue ? ActualStartTime.Value.AddMinutes((this.StopTime - this.StartTime).TotalMinutes) : (DateTime?)null; }
        }

        //RecalculateTimeRecord
        public int? RecalculateTimeRecordId { get; set; }
        public TermGroup_RecalculateTimeRecordStatus RecalculateTimeRecordStatus { get; set; }

        //Schedule only
        public string DayName { get; set; }
        public string HolidayName { get; set; }
        public bool IsHoliday { get; set; }
        public bool HasAttestedTransactions { get; set; }

        //Breaks
        public int Break1Id { get; set; }
        public DateTime Break1StartTime { get; set; }
        public int Break1Minutes { get; set; }
        public Guid? Break1Link { get; set; }
        public int Break2Id { get; set; }
        public DateTime Break2StartTime { get; set; }
        public int Break2Minutes { get; set; }
        public Guid? Break2Link { get; set; }
        public int Break3Id { get; set; }
        public DateTime Break3StartTime { get; set; }
        public int Break3Minutes { get; set; }
        public Guid? Break3Link { get; set; }
        public int Break4Id { get; set; }
        public DateTime Break4StartTime { get; set; }
        public int Break4Minutes { get; set; }
        public Guid? Break4Link { get; set; }
        public bool HasBreakTimes { get; set; }
        public bool IsBreak { get; set; }
        public SoeTimeScheduleTemplateBlockBreakType BreakType { get; set; }
        public int TotalMinutes
        {
            get { return (int)StopTime.Subtract(StartTime).TotalMinutes; }
        }
        public bool BelongsToPreviousDay
        {
            get { return StartTime.Date.AddDays(-1) == CalendarUtility.DATETIME_DEFAULT; }
        }
        public bool BelongsToNextDay
        {
            get { return StartTime.Date.AddDays(1) == CalendarUtility.DATETIME_DEFAULT; }
        }

        //TimeCodes
        public TimeCodeDTO TimeCode { get; set; }
        public int TimeCodeId { get; set; }
        public int Break1TimeCodeId { get; set; }
        public string Break1TimeCodeName { get; set; }
        public string Break1TimeCodeDescription { get; set; }
        public int Break1TimeCodeDefaultMinutes { get; set; }
        public bool Break1IsPreliminary { get; set; }
        public int Break2TimeCodeId { get; set; }
        public string Break2TimeCodeName { get; set; }
        public string Break2TimeCodeDescription { get; set; }
        public int Break2TimeCodeDefaultMinutes { get; set; }
        public bool Break2IsPreliminary { get; set; }
        public int Break3TimeCodeId { get; set; }
        public string Break3TimeCodeName { get; set; }
        public string Break3TimeCodeDescription { get; set; }
        public int Break3TimeCodeDefaultMinutes { get; set; }
        public bool Break3IsPreliminary { get; set; }
        public int Break4TimeCodeId { get; set; }
        public string Break4TimeCodeName { get; set; }
        public string Break4TimeCodeDescription { get; set; }
        public int Break4TimeCodeDefaultMinutes { get; set; }
        public bool Break4IsPreliminary { get; set; }

        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public string Dim2Description { get; set; }

        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public string Dim3Description { get; set; }

        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public string Dim4Description { get; set; }

        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public string Dim5Description { get; set; }

        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public string Dim6Description { get; set; }

        public List<int> DimIds
        {
            get
            {
                List<int> dimIds = new List<int>();
                for (int i = MIN_ACCOUNTDIM; i <= MAX_ACCOUNTDIM; i++)
                {
                    int dimId = GetDimIds(i);
                    if (dimId > 0)
                        dimIds.Add(dimId);
                }
                return dimIds;
            }
        }
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        [TsIgnore]
        public List<int> AccountInternalIds { get; set; }

        //Shift
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeDescription { get; set; }
        public int? ShiftTypeTimeScheduleTypeId { get; set; }
        public Guid? Link { get; set; }
        public bool ExtraShift { get; set; }
        public bool SubstituteShift { get; set; }
        public int? StaffingNeedsRowId { get; set; }

        //TimeScheduleTemplateBlockTasks
        public List<TimeScheduleTemplateBlockTaskDTO> Tasks { get; set; }

        //Flags
        public bool IsAdded { get; set; }
        public bool IsModified { get; set; }
        public bool Overlapping { get; set; }
        public bool OverlappingBreaks { get; set; }

        #endregion

        #region Public methods

        public int GetMinutes() => (int)this.StopTime.Subtract(this.StartTime).TotalMinutes;

        public void TryAddAccountInternal(List<AccountDTO> accounts, TermGroup_SieAccountDim sie, string externalCodeOrNumber)
        {
            var account = accounts.GetAccount(externalCodeOrNumber, sie);
            if (account != null)
                AddAccountInternal(account);
        }

        public void AddAccountInternal(AccountDTO account)
        {
            if (this.AccountInternals == null)
                this.AccountInternals = new List<AccountDTO>();
            this.AccountInternals.Add(account);
        }

        public int GetDimIds(int dimNr)
        {
            int accountDimId = 0;
            switch (dimNr)
            {
                case 2:
                    accountDimId = Dim2Id;
                    break;
                case 3:
                    accountDimId = Dim3Id;
                    break;
                case 4:
                    accountDimId = Dim4Id;
                    break;
                case 5:
                    accountDimId = Dim5Id;
                    break;
                case 6:
                    accountDimId = Dim6Id;
                    break;
            }
            return accountDimId;
        }

        public int GetBreakId(int breakNr)
        {
            int breakId = 0;
            switch (breakNr)
            {
                case 1:
                    breakId = Break1Id;
                    break;
                case 2:
                    breakId = Break2Id;
                    break;
                case 3:
                    breakId = Break3Id;
                    break;
                case 4:
                    breakId = Break4Id;
                    break;
            }
            return breakId;
        }

        public DateTime GetBreakStartTime(int breakNr)
        {
            DateTime breakStartTime = CalendarUtility.DATETIME_DEFAULT;
            if (HasBreakTimes)
            {
                switch (breakNr)
                {
                    case 1:
                        breakStartTime = Break1StartTime;
                        break;
                    case 2:
                        breakStartTime = Break2StartTime;
                        break;
                    case 3:
                        breakStartTime = Break3StartTime;
                        break;
                    case 4:
                        breakStartTime = Break4StartTime;
                        break;
                }
            }
            return breakStartTime;
        }

        public int GetBreakMinutes(int breakNr)
        {
            int breakMinutes = 0;
            if (HasBreakTimes)
            {
                switch (breakNr)
                {
                    case 1:
                        breakMinutes = Break1Minutes;
                        break;
                    case 2:
                        breakMinutes = Break2Minutes;
                        break;
                    case 3:
                        breakMinutes = Break3Minutes;
                        break;
                    case 4:
                        breakMinutes = Break4Minutes;
                        break;
                }
            }
            return breakMinutes;
        }

        public int GetTimeCodeId(int breakNr)
        {
            int timeCodeId = 0;
            switch (breakNr)
            {
                case 0:
                    timeCodeId = TimeCodeId;
                    break;
                case 1:
                    timeCodeId = Break1TimeCodeId;
                    break;
                case 2:
                    timeCodeId = Break2TimeCodeId;
                    break;
                case 3:
                    timeCodeId = Break3TimeCodeId;
                    break;
                case 4:
                    timeCodeId = Break4TimeCodeId;
                    break;
            }
            return timeCodeId;
        }

        public DateTime? GetBreakDate(int breakNr)
        {
            DateTime? date = null;
            switch (breakNr)
            {
                case 1:
                    date = Break1StartTime.Date;
                    break;
                case 2:
                    date = Break2StartTime.Date;
                    break;
                case 3:
                    date = Break3StartTime.Date;
                    break;
                case 4:
                    date = Break4StartTime.Date;
                    break;
            }
            return date;
        }

        #endregion
    }

    public class TimeScheduleTemplateBlockSmallDTO : IScheduleBlockAccounting
    {
        public int? EmployeeId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsBreak { get; set; }
        public int Type { get; set; }
        public int TimeCodeId { get; set; }
        public int? AccountId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public TimeSpan Length
        {
            get
            {
                return this.StopTime.Subtract(this.StartTime);
            }
        }
    }

    public class TimeScheduleBlockIODTO
    {
        #region Propertys

        public bool IsTemplate { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeScheduleEmployeePeriodId { get; set; }

        public string EmployeeNr { get; set; }
        public int TimeScheduleTypeId { get; set; }
        public int DayNumber { get; set; }
        public string Description { get; set; }

        public DateTime StartTime;
        public DateTime StopTime;
        public int LengthMinutes;
        public DateTime Date { get; set; }
        public SoeEntityState State { get; set; }

        //Breaks
        public int Break1Id { get; set; }
        public DateTime Break1StartTime { get; set; }
        public int Break1Minutes { get; set; }
        public string Break1Link { get; set; }
        public bool Break1IsPreliminary { get; set; }
        public int Break2Id { get; set; }
        public DateTime Break2StartTime { get; set; }
        public int Break2Minutes { get; set; }
        public string Break2Link { get; set; }
        public bool Break2IsPreliminary { get; set; }
        public int Break3Id { get; set; }
        public DateTime Break3StartTime { get; set; }
        public int Break3Minutes { get; set; }
        public string Break3Link { get; set; }
        public bool Break3IsPreliminary { get; set; }
        public int Break4Id { get; set; }
        public DateTime Break4StartTime { get; set; }
        public int Break4Minutes { get; set; }
        public string Break4Link { get; set; }
        public bool Break4IsPreliminary { get; set; }
        public bool HasBreakTimes { get; set; }
        public bool IsBreak { get; set; }

        //Shift
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeDescription { get; set; }
        public int? ShiftTypeTimeScheduleTypeId { get; set; }
        public string Link { get; set; }

        //Accounting
        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }

        public int TimeScheduleTemplateBlockId { get; set; }
        public int TimeDeviationCauseId { get; set; }

        // Hierachy
        public int? HierachyAccountId { get; set; }
        #endregion
    }

    public class TimeScheduleTemplateBlockHistoryDTO
    {
        //Keys
        public int TimeScheduleTemplateBlockHistoryId { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? CurrentEmployeeId { get; set; }

        //Common
        public string BatchId { get; set; }
        public string Note { get; set; }
        public TermGroup_ShiftHistoryType Type { get; set; }
        public bool IsBreak { get; set; }
        public int? RecordId { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        //Dates
        public DateTime? FromStart { get; set; }
        public DateTime? ToStart { get; set; }
        public DateTime? FromStop { get; set; }
        public DateTime? ToStop { get; set; }
        public DateTime? OriginDate { get; set; }

        //Employee
        public int? FromEmployeeId { get; set; }
        public string FromEmployeeNr { get; set; }
        public int? ToEmployeeId { get; set; }
        public string ToEmployeeNr { get; set; }
        public int? OriginEmployeeId { get; set; }
        public string OriginEmployeeNr { get; set; }

        //Shift
        public int? FromShiftTypeId { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftStatus? FromShiftStatus { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus? FromShiftUserStatus { get; set; }
        public string FromShiftTypeExtCode { get; set; }
        public int? ToShiftTypeId { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftStatus? ToShiftStatus { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus? ToShiftUserStatus { get; set; }
        public string ToShiftTypeExtCode { get; set; }

        //TimeDeviationCause
        public int? FromTimeDeviationCauseId { get; set; }
        public string FromTimeDeviationCauseName { get; set; }
        public int? ToTimeDeviationCauseId { get; set; }
        public string ToTimeDeviationCauseName { get; set; }

        // Flags
        public bool IsNew { get; set; }
        public bool IsChanged { get; set; }
        public bool IsSwapped { get; set; }
        public bool IsDeleted { get; set; }
        public bool Invalid { get; set; }
    }

    [TSInclude]
    public class ShiftHistoryDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public string TypeName { get; set; }
        public string FromShiftStatus { get; set; }
        public string ToShiftStatus { get; set; }
        public bool ShiftStatusChanged { get; set; }
        public string FromShiftUserStatus { get; set; }
        public string ToShiftUserStatus { get; set; }
        public bool ShiftUserStatusChanged { get; set; }
        public string FromEmployeeName { get; set; }
        public string ToEmployeeName { get; set; }
        public string FromEmployeeNr { get; set; }
        public string ToEmployeeNr { get; set; }
        public bool EmployeeChanged { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool TimeChanged { get; set; }
        public string FromDateAndTime { get; set; }
        public string ToDateAndTime { get; set; }
        public bool DateAndTimeChanged { get; set; }
        public string FromShiftType { get; set; }
        public string ToShiftType { get; set; }
        public bool ShiftTypeChanged { get; set; }
        public string FromTimeDeviationCause { get; set; }
        public string ToTimeDeviationCause { get; set; }
        public bool TimeDeviationCauseChanged { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public string AbsenceRequestApprovedText { get; set; }
        public string FromStart { get; set; }
        public string FromStop { get; set; }
        public string ToStart { get; set; }
        public string ToStop { get; set; }
        public string OriginEmployeeNr { get; set; }
        public string OriginEmployeeName { get; set; }
        public int? FromEmployeeId { get; set; }
        public int? ToEmployeeId { get; set; }
        public string FromExtraShift { get; set; }
        public string ToExtraShift { get; set; }
        public bool ExtraShiftChanged { get; set; }
    }

    #endregion

    #region TimeScheduleTemplateBlockTask

    [TSInclude]
    public class TimeScheduleTemplateBlockTaskDTO
    {
        public int TimeScheduleTemplateBlockTaskId { get; set; }
        public int? TimeScheduleTemplateBlockId { get; set; }
        public int? TimeScheduleTaskId { get; set; }
        public int? IncomingDeliveryRowId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        //Extensions
        public bool IsTimeScheduleTask
        {
            get { return this.TimeScheduleTaskId.HasValue; }
        }
        public bool IsIncomingDeliveryRow
        {
            get { return this.IncomingDeliveryRowId.HasValue; }
        }

        [TsIgnore]
        public string TaskKey
        {
            get
            {
                return $"t{TimeScheduleTaskId}#d{IncomingDeliveryRowId}";
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }

    }

    #endregion

    #region TimeScheduleTemplateGroup

    public class TimeScheduleTemplateGroupDTO
    {
        public int TimeScheduleTemplateGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TemplateNames { get; set; }
        public List<TimeScheduleTemplateGroupRowDTO> Rows { get; set; }
        public List<TimeScheduleTemplateGroupEmployeeDTO> Employees { get; set; }
    }

    public class TimeScheduleTemplateGroupGridDTO
    {
        public int TimeScheduleTemplateGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Extensions
        public int NbrOfRows { get; set; }
        public int NbrOfEmployees { get; set; }
    }

    public class TimeScheduleTemplateGroupRowDTO
    {
        public int TimeScheduleTemplateGroupRowId { get; set; }
        public int TimeScheduleTemplateGroupId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string RecurrencePattern { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public DateTime? NextStartDate { get; set; }
    }

    public class TimeScheduleTemplateGroupEmployeeDTO
    {
        public int TimeScheduleTemplateGroupEmployeeId { get; set; }
        public int TimeScheduleTemplateGroupId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeName { get; set; }
        public string EmployeeNr { get; set; }

        public TimeScheduleTemplateGroupDTO Group { get; set; }
    }

    public class TimeScheduleTemplateHeadsRangeDTO
    {
        public List<TimeScheduleTemplateHeadRangeDTO> Heads { get; set; }
    }

    public class TimeScheduleTemplateHeadRangeDTO
    {
        public int EmployeeId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public string TemplateName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int? TimeScheduleTemplateGroupId { get; set; }
        public string TimeScheduleTemplateGroupName { get; set; }
        public int NoOfDays { get; set; }
        public DateTime? FirstMondayOfCycle { get; set; }
        public int? EmployeeScheduleId { get; set; }
        public DateTime? EmployeeScheduleStartDate { get; set; }
        public DateTime? EmployeeScheduleStopDate { get; set; }
    }

    #endregion

    #region TimeScheduleTemplateHead

    public class TimeScheduleTemplateHeadDTO
    {
        public int TimeScheduleTemplateHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? EmployeeId { get; set; }
        public int? EmployeePostId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NoOfDays { get; set; }
        public bool StartOnFirstDayOfWeek { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? FirstMondayOfCycle { get; set; }
        public bool SimpleSchedule { get; set; }
        public bool FlexForceSchedule { get; set; }
        public bool Locked { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<TimeScheduleTemplatePeriodDTO> TimeScheduleTemplatePeriods { get; set; }
        public List<EmployeeScheduleDTO> EmployeeSchedules { get; set; }
        public string EmployeeName { get; set; }
        public DateTime LastPlacementStartDate { get; set; }
        public DateTime LastPlacementStopDate { get; set; }
    }

    // Used in TimeSchedulePlanning for performance, do not add properties!
    [TSInclude]
    public class TimeScheduleTemplateHeadSmallDTO
    {
        public int TimeScheduleTemplateHeadId { get; set; }
        public int? TimeScheduleTemplateGroupId { get; set; }
        public string TimeScheduleTemplateGroupName { get; set; }
        public string Name { get; set; }
        public int NoOfDays { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? VirtualStopDate { get; set; }
        public DateTime? FirstMondayOfCycle { get; set; }
        public bool SimpleSchedule { get; set; }
        public bool Locked { get; set; }

        // Extensions
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
    }

    #endregion

    #region TimeScheduleTemplatePeriod

    [TSInclude]
    public class TimeScheduleTemplatePeriodDTO
    {
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }

        public int DayNumber { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<TimeScheduleTemplateBlockDTO> TimeScheduleTemplateBlocks { get; set; }
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public string HolidayName { get; set; }
        public bool IsHoliday { get; set; }
        public bool HasAttestedTransactions { get; set; }
    }

    public class TimeScheduleTemplatePeriodSmallDTO
    {
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }

        public int DayNumber { get; set; }
    }

    #endregion

    #region TimeEmployeeScheduleDataSmall

    public class TimeEmployeeScheduleDataSmallDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public int EmployeeId { get; set; }
        public int EmployeeGroupId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? AccountId { get; set; }
        public int TimeCodeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceCurrency { get; set; }
        public decimal UnitPriceEntCurrency { get; set; }
        public decimal UnitPriceLedgerCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal GrossAmountCurrency { get; set; }
        public decimal GrossAmountEntCurrency { get; set; }
        public decimal GrossAmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity
        {
            get { return Convert.ToDecimal((StopTime - StartTime).TotalMinutes); }
        }
        public decimal GrossQuantity { get; set; }
        public decimal NetQuantity
        {
            get { return this.IsBreak ? this.Quantity : this._netQuantity; }
            set { this._netQuantity = value; }
        }
        public bool IsBreak { get; set; }
        public bool SubstituteShift { get; set; }
        public bool ExtraShift { get; set; }
        public bool IsPreliminary { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public DateTime? LastChanged
        {
            get
            {
                if (this.Modified.HasValue)
                    return this.Modified;
                else
                    return this.Created;
            }
        }

        // Extensions
        public List<AccountInternalDTO> AccountInternals { get; set; }
        public int? ScheduleTypeId { get; set; }
        public List<GrossTimeRule> GrossTimeRules { get; set; }
        public List<GrossTimeCalc> GrossTimeCalcs { get; set; } = new List<GrossTimeCalc>();

        public string GroupName { get; set; }
        public string EmployeeNrSort { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public double NetLength
        {
            get { return this.IsBreak ? this.Length : this._netLength; }
            set { this._netLength = value; }
        }
        public double Length
        {
            get { return (this.StopTime - this.StartTime).TotalMinutes; }
            set { }//NOSONAR
        }

        public bool IsZeroSchedule
        {
            get { return this.StartTime == this.StopTime; }
            set { }//NOSONAR
        }

        public decimal QuantityHours
        {
            get
            {
                return decimal.Round(decimal.Divide(Quantity, 60), 2);

            }
        }

        public SoeTimeScheduleTemplateBlockLendedType LendedType { get; set; }

        //Private variables
        private double _netLength { get; set; }
        private decimal _netQuantity { get; set; }

        //Matrix
        public AccountAndInternalAccountComboDTO AccountAndInternalAccountCombo { get; set; }
        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }

        //Methods
        public string GetAccountingIdString()
        {
            string str = string.Empty;
            if (this.AccountId.HasValue)
                str = this.AccountId.ToString();
            else
                str = "0";

            if (this.AccountInternals != null)

                foreach (var ai in this.AccountInternals)
                    str += $"|{ai.AccountId}";

            return str;
        }

        public string GroupOn(List<TermGroup_ScheduleTransactionMatrixColumns> columns)
        {
            string value = string.Empty;

            foreach (var column in columns)
            {
                switch (column)
                {
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeNr:
                        value += $"#{this.EmployeeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeName:
                        value += $"#{this.EmployeeId}{this.FirstName}{this.LastName}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Date:
                    case TermGroup_ScheduleTransactionMatrixColumns.EmploymentPercent:
                        value += $"#{this.Date}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.StartTime:
                        value += $"#{this.StartTime}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.StopTime:
                        value += $"#{this.StopTime}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.NetMinutes:
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossMinutes:
                    case TermGroup_ScheduleTransactionMatrixColumns.NetHours:
                    case TermGroup_ScheduleTransactionMatrixColumns.NetHoursString:
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossHours:
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossHoursString:
                        value += $"#";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.NetCost:
                        value += $"#{this.Amount}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossCost:
                        value += $"#{this.GrossAmount}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.IsBreak:
                        value += $"#{this.IsBreak}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ExtraShift:
                        value += $"#{this.ExtraShift}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.SubstituteShift:
                        value += $"#{this.SubstituteShift}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.SubstituteShiftCalculated:
                        value += $"#{this.SubstituteShift}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.IsPreliminary:
                        value += $"#{this.IsPreliminary}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Description:
                        value += $"#{this.Description}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeName:
                    case TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeScheduleTypeName:
                    case TermGroup_ScheduleTransactionMatrixColumns.ScheduleTypeName:
                        value += $"#{this.ShiftTypeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Created:
                        value += $"#{this.Created}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Modified:
                        value += $"#{this.Modified}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.CreatedBy:
                        value += $"#{this.CreatedBy}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ModifiedBy:
                        value += $"#{this.ShiftTypeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeCodeName:
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeCodeCode:
                        value += $"#{this.TimeCodeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeRuleName:
                        value += $"#{this.GrossTimeRules?.Count}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountNr:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountName:
                        value += $"#{this.AccountId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNr1:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalName1:
                        value += $"#{this.AccountAndInternalAccountCombo?.Dim2Nr}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNr2:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalName2:
                        value += $"#{this.AccountAndInternalAccountCombo?.Dim3Nr}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNr3:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalName3:
                        value += $"#{this.AccountAndInternalAccountCombo?.Dim3Nr}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNr4:
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalName4:
                        value += $"#{this.AccountAndInternalAccountCombo?.Dim5Nr}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNr5:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalName5:
                        value += $"#{this.AccountAndInternalAccountCombo?.Dim6Nr}";
                        break;
                    default:
                        break;
                }
            }

            return value;
        }
    }

    public class GrossTimeCalc
    {
        public GrossTimeRule Rule { get; set; }

        public decimal AddedGrossQuantity { get; set; }
        public decimal AddedGrossAmount { get; set; }
        public int AddedOverlappingMinutes { get; set; }

    }

    public class TimeEmployeeScheduleDataSmallDTOIWHInfo
    {
        public int Productid { get; set; }
        public string ExternalCode { get; set; }
        public string ProductNr { get; set; }
    }


    #endregion

    #region TimeScheduleSwapLengthComparisonDTO

    public class TimeScheduleSwapLengthComparisonDTO
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public ScheduleSwapLengthComparisonType Type { get; set; }
    }

    #endregion

    #region TimeScheduleSwapRequest

    public class TimeScheduleSwapRequestDTO
    {
        public int TimeScheduleSwapRequestId { get; set; }
        public int InitiatorUserId { get; set; }
        public int? InitiatorEmployeeId { get; set; }
        public string Comment { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public TermGroup_TimeScheduleSwapRequestStatus Status { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? InitiatedDate { get; set; }
        public int? AcceptorUserId { get; set; }
        public string AcceptorUserName { get; set; }
        public int? AcceptorEmployeeId { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }

        public List<TimeScheduleSwapRequestRowDTO> Rows;
        public bool IsInitiator(int employeeid)
        {
            return this.InitiatorEmployeeId == employeeid;
        }
        public bool IsSwapWith(int employeeid)
        {
            return this.InitiatorEmployeeId != employeeid && this.Rows.Any(x => x.EmployeeId == employeeid);
        }
        public bool IsShiftAffectedBySwap(TimeSchedulePlanningDayDTO shift)
        {
            return this.Rows.Any(w => w.EmployeeId == shift.EmployeeId && CalendarUtility.IsDatesOverlapping(shift.StartTime, shift.StopTime, w.ScheduleStart, w.ScheduleStop));
        }
    }

    #endregion

    #region TimeScheduleSwapRequestRow
    public class TimeScheduleSwapRequestRowDTO
    {
        public int TimeScheduleSwapRequestRowId { get; set; }
        public int TimeScheduleSwapRequestId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string ShiftsInfo { get; set; }
        public DateTime ScheduleStart { get; set; }
        public DateTime ScheduleStop { get; set; }
        public TermGroup_TimeScheduleSwapRequestRowStatus Status { get; set; }
        public SoeEntityState State { get; set; }
        public bool SkillsMatch { get; set; }
        public int ShiftLength { get; set; }
    }

    #endregion

    #region TimeScheduleType
    [TSInclude]
    public class TimeScheduleTypeDTO
    {
        public int TimeScheduleTypeId { get; set; }
        public int ActorCompanyId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsAll { get; set; }
        public bool IsNotScheduleTime { get; set; }
        public bool UseScheduleTimeFactor { get; set; }
        public bool IsBilagaJ { get; set; }
        public bool ShowInTerminal { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public bool IgnoreIfExtraShift { get; set; }

        // Relations
        public List<TimeScheduleTypeFactorDTO> Factors { get; set; }

        public List<TimeScheduleTypeFactorSmallDTO> ToSmallFactors()
        {
            List<TimeScheduleTypeFactorSmallDTO> dtos = new List<TimeScheduleTypeFactorSmallDTO>();

            if (this.Factors != null && this.Factors.Count > 0)
            {
                foreach (TimeScheduleTypeFactorDTO factor in this.Factors)
                {
                    dtos.Add(factor.ToSmallDTO());
                }
            }

            return dtos;
        }

        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    [TSInclude]
    public class TimeScheduleTypeGridDTO
    {
        public int TimeScheduleTypeId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsAll { get; set; }

        public bool IsBilagaJ { get; set; }
        public bool ShowInTerminal { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public SoeEntityState State { get; set; }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }

    }


    [TSInclude]
    public class TimeScheduleTypeSmallDTO
    {
        public int TimeScheduleTypeId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        // Relations
        public List<TimeScheduleTypeFactorSmallDTO> Factors { get; set; }
    }
    [TSInclude]
    public class TimeScheduleTypeFactorDTO
    {
        public int TimeScheduleTypeFactorId { get; set; }
        public int TimeScheduleTypeId { get; set; }

        public decimal Factor { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public TimeScheduleTypeFactorSmallDTO ToSmallDTO()
        {
            return new TimeScheduleTypeFactorSmallDTO()
            {
                Factor = this.Factor,
                FromTime = this.FromTime,
                ToTime = this.ToTime
            };
        }
    }
    [TSInclude]
    public class TimeScheduleTypeFactorSmallDTO
    {
        public decimal Factor { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
    }

    #endregion

    #region TimeSpot

    #region TimeSpotEmployeeView

    public class TimeSpotEmployeeViewDTO
    {
        public int ActorCompanyId { get; set; }
        public string CardNumber { get; set; }
        public string Company { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Dialog { get; set; }
        public string CostPlace { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? modified { get; set; }
    }

    #endregion

    #region TimeSpotTimeCodeView

    public class TimeSpotTimeCodeViewDTO
    {
        public int ActorCompanyId { get; set; }
        public int id { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }
    #endregion

    #region TimeSpotTimeCodeViewForEmployee

    public class TimeSpotTimeCodeViewForEmployeeDTO
    {
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public int id { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }

    #endregion

    #region TimeSpotTimeStampView

    public class TimeSpotTimeStampViewDTO
    {
        public int ActorCompanyId { get; set; }
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public DateTime Time { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Type { get; set; }
        public string TimeTerminalName { get; set; }
        public int TimeTerminalId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public DateTime? Changed { get; set; }
    }

    #endregion

    #endregion

    #region TimeStampEntry
    [TSInclude]
    public class TimeStampEntryDTO
    {
        public int TimeStampEntryId { get; set; }
        public int? TimeTerminalId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? AccountId { get; set; }
        public int? TimeTerminalAccountId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeBlockDateId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public TermGroup_TimeStampEntryOriginType OriginType { get; set; }
        public TimeStampEntryType Type { get; set; }
        public string TerminalStampData { get; set; }
        public string Note { get; set; }
        public DateTime Time { get; set; }
        public DateTime? OriginalTime { get; set; }
        public bool ManuallyAdjusted { get; set; }
        public bool EmployeeManuallyAdjusted { get; set; }
        public TermGroup_TimeStampEntryStatus Status { get; set; }
        public bool IsBreak { get; set; }
        public bool IsPaidBreak { get; set; }
        public bool IsDistanceWork { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<TimeStampEntryExtendedDTO> Extended { get; set; }

        // Extensions
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public DateTime? TimeBlockDateDate { get; set; }
        public DateTime? AdjustedTimeBlockDateDate { get; set; }
        public DateTime? AdjustedTime { get; set; }
        public DateTime Date { get; set; }
        public string TypeName { get; set; }
        public string TimeTerminalName { get; set; }
        public bool IsModified { get; set; }
        public DateTime? DateFromTimeBlockDate { get; set; }
        [TSIgnore]
        public long Sort
        {
            get
            {
                int inOrOut = this.Type == TimeStampEntryType.Out ? 0 : 1;
                var time = $"{this.Time:yyyyMMddHHmm}{inOrOut}";
                return Convert.ToInt64(time);
            }
        }
        [TSIgnore]
        public DateTime? ScheduleStartTime { get; set; }

        public List<int> GetAccountIds()
        {
            List<int> accountIds = new List<int>();
            if (this.AccountId.HasValue)
                accountIds.Add(this.AccountId.Value);
            if (!this.Extended.IsNullOrEmpty())
                accountIds.AddRange(Extended.Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value));

            return accountIds;
        }
    }

    public class TimeStampEntryRawDTO
    {
        public int TimeStampEntryRawId { get; set; }
        public int? TimeStampEntryId { get; set; }
        public int TimeTerminalRecordId { get; set; }
        public int ActorCompanyRecordId { get; set; }
        public string EmployeeNr { get; set; }
        public int? TimeDeviationCauseRecordId { get; set; }
        public int? AccountRecordId { get; set; }
        public TimeStampEntryType Type { get; set; }
        public string TerminalStampData { get; set; }
        public DateTime Time { get; set; }
        public TermGroup_TimeStampEntryStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Created { get; set; }
        public int? EmployeeChildRecordId { get; set; }
        public bool IsBreak { get; set; }
    }

    #endregion

    #region TimeStampEntryExtended
    [TSInclude]
    public class TimeStampEntryExtendedDTO
    {
        public int TimeStampEntryExtendedId { get; set; }
        public int TimeStampEntryId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public int? TimeCodeId { get; set; }
        public int? AccountId { get; set; }
        public decimal? Quantity { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class TimeStampAdditionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeStampAdditionType Type { get; set; }
        public decimal? FixedQuantity { get; set; }
    }

    public class TimeStampEntryExtendedDetailsDTO
    {
        public int? TimeScheduleTypeId { get; set; }
        public int? AccountId { get; set; }
        public int? TimeCodeId { get; set; }
    }

    #endregion

    #region TimeTerminal

    public class TimeTerminalDTO
    {
        public int TimeTerminalId { get; set; }
        public int ActorCompanyId { get; set; }
        public TimeTerminalType Type { get; set; }
        [TsIgnore]
        public bool IsWebTimeStamp
        {
            get
            {
                return Type == TimeTerminalType.WebTimeStamp;
            }
        }

        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string MacName { get; set; }
        public int? MacNumber { get; set; }

        public bool Registered { get; set; }
        public DateTime? LastSync { get; set; }

        public int? TerminalDbSchemaVersion { get; set; }
        public string TerminalVersion { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        public int SysCompDBId { get; set; }
        public string Uri { get; set; }

        // Extensions
        public string CompanyName { get; set; }
        public string TypeName { get; set; }
        public string CompanyApiKey { get; set; }
        public List<TimeTerminalSettingDTO> TimeTerminalSettings { get; set; }
        public List<int> CategoryIds { get; set; }
        public Guid TimeTerminalGuid { get; set; }

        public void SetUri()
        {
            this.Uri = $"https://terminal.softone.se/LogIn?c={this.ActorCompanyId}&t={this.TimeTerminalId}";
        }
    }

    public class TimeTerminalSettingDTO : SettingsDTO<TimeTerminalSettingType, TimeTerminalSettingDataType>
    {
        public int TimeTerminalSettingId { get { return this.Id; } set { this.Id = value; } }
        public int? ParentId { get; set; }
        public List<TimeTerminalSettingDTO> Children { get; set; }

        [TsIgnore]
        public bool HasChildren
        {
            get { return Children != null && Children.Any(); }
        }
    }

    #endregion

    #region TimeWorkAccount

    #region Grid

    [TSInclude]
    public class TimeWorkAccountGridDTO
    {
        public int TimeWorkAccountId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool UsePensionDeposit { get; set; }
        public bool UsePaidLeave { get; set; }
        public bool UseDirectPayment { get; set; }
        public TermGroup_TimeWorkAccountWithdrawalMethod DefaultWithdrawalMethod { get; set; }
        public TermGroup_TimeWorkAccountWithdrawalMethod DefaultPaidLeaveNotUsed { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Edit DTO

    [TSInclude]
    public class TimeWorkAccountDTO
    {
        public int TimeWorkAccountId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool UsePensionDeposit { get; set; }
        public bool UsePaidLeave { get; set; }
        public bool UseDirectPayment { get; set; }
        public TermGroup_TimeWorkAccountWithdrawalMethod DefaultWithdrawalMethod { get; set; }
        public TermGroup_TimeWorkAccountWithdrawalMethod DefaultPaidLeaveNotUsed { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<TimeWorkAccountYearDTO> TimeWorkAccountYears { get; set; }
        [TsIgnore]
        public bool IsNew
        {
            get
            {
                return !this.TimeWorkAccountId.ToNullable().HasValue;
            }
        }
    }

    [TSInclude]
    public class TimeWorkAccountWorkTimeWeekDTO
    {
        public int TimeWorkAccountWorkTimeWeekId { get; set; }
        public int TimeWorkAccountYearId { get; set; }
        public int WorkTimeWeekFrom { get; set; }
        public int WorkTimeWeekTo { get; set; }
        public int PaidLeaveTime { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

    }

    [TSInclude]
    public class TimeWorkAccountYearDTO
    {
        public int TimeWorkAccountYearId { get; set; }
        public int TimeWorkAccountId { get; set; }
        public DateTime EarningStart { get; set; }
        public DateTime EarningStop { get; set; }
        public DateTime WithdrawalStart { get; set; }
        public DateTime WithdrawalStop { get; set; }
        public decimal? PensionDepositPercent { get; set; }
        public decimal? PaidLeavePercent { get; set; }
        public decimal? DirectPaymentPercent { get; set; }
        public DateTime EmployeeLastDecidedDate { get; set; }
        public DateTime PaidAbsenceStopDate { get; set; }
        public DateTime DirectPaymentLastDate { get; set; }
        public int? PensionDepositPayrollProductId { get; set; }
        public int? DirectPaymentPayrollProductId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<TimeWorkAccountYearEmployeeDTO> TimeWorkAccountYearEmployees { get; set; }
        public List<TimeWorkAccountWorkTimeWeekDTO> TimeWorkAccountWorkTimeWeeks { get; set; }
        public int? TimeAccumulatorId { get; set; }
        [TsIgnore]
        public bool IsNew
        {
            get
            {
                return !this.TimeWorkAccountYearId.ToNullable().HasValue;
            }
        }
    }

    [TSInclude]
    public class TimeWorkAccountYearEmployeeDTO
    {
        public int TimeWorkAccountYearEmployeeId { get; set; }
        public int TimeWorkAccountId { get; set; }
        public int EmployeeId { get; set; }
        public TermGroup_TimeWorkAccountYearEmployeeStatus Status { get; set; }
        public DateTime EarningStart { get; set; }
        public DateTime EarningStop { get; set; }
        public TermGroup_TimeWorkAccountWithdrawalMethod SelectedWithdrawalMethod { get; set; }
        public DateTime? SelectedDate { get; set; }
        public int CalculatedPaidLeaveMinutes { get; set; }
        public decimal CalculatedPaidLeaveAmount { get; set; }
        public decimal CalculatedPensionDepositAmount { get; set; }
        public decimal CalculatedDirectPaymentAmount { get; set; }
        public decimal CalculatedWorkingTimePromoted { get; set; }
        public decimal? SpecifiedWorkingTimePromoted { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? SentDate { get; set; }

        // Extensions
        public string EmployeeName { get; set; }
        public string EmployeeNumber { get; set; }
    }

    [TSInclude]
    public class TimeWorkAccountExportPensionDTO
    {
        public int TimeWorkAccountId { get; set; }
        public int TimeWorkAccountYearId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNrAndName { get; set; }
        public string EmployeeSocialSec { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public bool Ended { get; set; }
    }

    [TSInclude]
    public class TimeWorkAccountYearEmployeeBasisDTO
    {
        public DateTime Date { get; set; }
        public int? TimePeriodId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? RuleWorkTimeWeek { get; set; }
        public int? WorkTimeWeek { get; set; }
        public decimal? EmploymentPercent { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PaidLeaveMinutes { get; set; }
        public string TptIds { get; set; }
        public string TpstIds { get; set; }
        public decimal UnpaidAbsenceRatio { get; set; }
        public int YearEarningDays { get; set; }

        public static TimeWorkAccountYearEmployeeBasisDTO Create(DateTime date, TimePeriodDTO timePeriod, int ruleWorkTimeWeek, int workTimeWeek, decimal employmentPercent, decimal unpaidAbsenceRatio, int yearEarningDays, decimal amount, decimal paidLeaveMinutes, List<int> tptIds, List<int> tpstIds)
        {
            return new TimeWorkAccountYearEmployeeBasisDTO
            {
                Date = date,
                TimePeriodId = timePeriod?.TimePeriodId,
                PaymentDate = timePeriod?.PaymentDate,
                WorkTimeWeek = workTimeWeek,
                EmploymentPercent = employmentPercent,
                Amount = amount,
                PaidLeaveMinutes = paidLeaveMinutes,
                TptIds = tptIds.ToCommaSeparated(),
                TpstIds = tpstIds.ToCommaSeparated(),
                RuleWorkTimeWeek = ruleWorkTimeWeek,
                UnpaidAbsenceRatio = unpaidAbsenceRatio,
                YearEarningDays = yearEarningDays,
            };
        }
    }

    #endregion

    #region Edit Model

    [TSInclude]
    public class TimeWorkAccountYearEmployeeModel
    {
        public int TimeWorkAccountId { get; set; }
        public int TimeWorkAccountYearId { get; set; }
        public List<int> TimeWorkAccountYearEmployeeIds { get; set; }
        public List<int> EmployeeIds { get; set; }
    }

    [TSInclude]
    public class TimeWorkAccountGenerateOutcomeModel
    {
        public int TimeWorkAccountId { get; set; }
        public int TimeWorkAccountYearId { get; set; }
        public int PaymentDateId { get; set; }
        public DateTime PaymentDate { get; set; }
        public List<int> TimeWorkAccountYearEmployeeIds { get; set; }
        public bool OverrideChoosen { get; set; }
    }

    #endregion

    #region Result DTO

    [TSInclude]
    public class TimeWorkAccountYearEmployeeResultDTO
    {
        public ActionResult Result { get; set; }
        public List<TimeWorkAccountYearEmployeeResultRowDTO> Rows { get; set; }

        public TimeWorkAccountYearEmployeeResultDTO(ActionResult result = null)
        {
            this.Result = result ?? new ActionResult(true);
            this.Rows = new List<TimeWorkAccountYearEmployeeResultRowDTO>();
        }
    }

    [TSInclude]
    public class TimeWorkAccountYearEmployeeResultRowDTO
    {
        public int TimeWorkAccountId { get; private set; }
        public int TimeWorkAccountYearId { get; private set; }
        public int EmployeeId { get; private set; }
        public string EmployeeNrAndName { get; private set; }
        public DateTime? EmployeeDateFrom { get; private set; }
        public DateTime? EmployeeDateTo { get; private set; }
        public TermGroup_TimeWorkAccountYearResultCode Code { get; private set; }
        public string CodeName { get; private set; }
        public TermGroup_TimeWorkAccountYearEmployeeStatus EmployeeStatus { get; private set; }
        public string EmployeeStatusName { get; private set; }

        [TSIgnore]
        public bool HasFailed => (int)Code > 10;

        public static TimeWorkAccountYearEmployeeResultRowDTO Create(
            int timeWorkAccountId,
            int timeWorkAccountYearId,
            int employeeId,
            string employeeNrAndName,
            DateTime? employeeDateFrom,
            DateTime? employeeDateTo
            )
        {
            return new TimeWorkAccountYearEmployeeResultRowDTO
            {
                TimeWorkAccountId = timeWorkAccountId,
                TimeWorkAccountYearId = timeWorkAccountYearId,
                EmployeeId = employeeId,
                EmployeeNrAndName = employeeNrAndName,
                EmployeeDateFrom = employeeDateFrom,
                EmployeeDateTo = employeeDateTo,
            };
        }
        public TimeWorkAccountYearEmployeeResultRowDTO Failed(
            TermGroup_TimeWorkAccountYearResultCode code,
            TermGroup_TimeWorkAccountYearEmployeeStatus employeeStatus = TermGroup_TimeWorkAccountYearEmployeeStatus.NotCalculated
            )
        {
            this.Code = code;
            this.EmployeeStatus = employeeStatus;
            return this;
        }
        public TimeWorkAccountYearEmployeeResultRowDTO Succeeded()
        {
            this.Code = TermGroup_TimeWorkAccountYearResultCode.Succeeded;
            this.EmployeeStatus = TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated;
            return this;
        }
        public TimeWorkAccountYearEmployeeResultRowDTO Deleted()
        {
            this.Code = TermGroup_TimeWorkAccountYearResultCode.Deleted;
            this.EmployeeStatus = TermGroup_TimeWorkAccountYearEmployeeStatus.NotCalculated;
            return this;
        }
        public void UpdateEmployeeStatusName(TermGroup_TimeWorkAccountYearEmployeeStatus employeeStatus, string name = null)
        {
            this.EmployeeStatus = employeeStatus;
            if (name != null)
                this.EmployeeStatusName = name;
        }
        public void UpdateCode(TermGroup_TimeWorkAccountYearResultCode code, string name = null)
        {
            this.Code = code;
            if (name != null)
                this.CodeName = name;
        }
    }

    [TSInclude]
    public class TimeWorkAccountGenerateOutcomeResultDTO
    {
        public ActionResult Result { get; set; }
        public List<TimeWorkAccountTransactionResultRowDTO> Rows { get; set; }

        public TimeWorkAccountGenerateOutcomeResultDTO(ActionResult result = null)
        {
            this.Result = result ?? new ActionResult(true);
            this.Rows = new List<TimeWorkAccountTransactionResultRowDTO>();
        }
    }

    [TSInclude]
    public class TimeWorkAccountTransactionResultRowDTO
    {
        public int TimeWorkAccountId { get; private set; }
        public int TimeWorkAccountYearId { get; private set; }
        public int EmployeeId { get; private set; }
        public string EmployeeNrAndName { get; private set; }
        public TermGroup_TimeWorkAccountTransactionActionCode Code { get; private set; }
        public string CodeName { get; private set; }
        public TermGroup_TimeWorkAccountWithdrawalMethod Method { get; private set; }
        public string MethodName { get; private set; }

        [TSIgnore]
        public bool HasFailed =>
            Code == TermGroup_TimeWorkAccountTransactionActionCode.EmployeeNotFound ||
            Code == TermGroup_TimeWorkAccountTransactionActionCode.SaveFailed ||
            Code == TermGroup_TimeWorkAccountTransactionActionCode.GenerationFailed ||
            Code == TermGroup_TimeWorkAccountTransactionActionCode.TimePeriodNotFound ||
            Code == TermGroup_TimeWorkAccountTransactionActionCode.TimePeriodLocked;

        public static TimeWorkAccountTransactionResultRowDTO Create(int timeWorkAccountId, int timeWorkAccountYearId, int employeeId, string employeeNrAndName, TermGroup_TimeWorkAccountWithdrawalMethod Method)
        {
            return new TimeWorkAccountTransactionResultRowDTO
            {
                TimeWorkAccountId = timeWorkAccountId,
                TimeWorkAccountYearId = timeWorkAccountYearId,
                EmployeeId = employeeId,
                EmployeeNrAndName = employeeNrAndName,
                Method = Method,
            };
        }
        public TimeWorkAccountTransactionResultRowDTO Failed(TermGroup_TimeWorkAccountTransactionActionCode code)
        {
            this.Code = code;
            return this;
        }
        public TimeWorkAccountTransactionResultRowDTO CreateSucess()
        {
            this.Code = TermGroup_TimeWorkAccountTransactionActionCode.CreateSucess;
            return this;
        }
        public TimeWorkAccountTransactionResultRowDTO DeleteSuccess()
        {
            this.Code = TermGroup_TimeWorkAccountTransactionActionCode.DeleteSuccess;
            return this;
        }
        public TimeWorkAccountTransactionResultRowDTO BalanceSuccess()
        {
            this.Code = TermGroup_TimeWorkAccountTransactionActionCode.BalanceSuccess;
            return this;
        }
        public bool IsBalanceSuccess()
        {
            return this.Code == TermGroup_TimeWorkAccountTransactionActionCode.BalanceSuccess;
        }
        public void UpdateMethod(TermGroup_TimeWorkAccountWithdrawalMethod method, string name = null)
        {
            this.Method = method;
            if (name != null)
                this.MethodName = name;
        }
        public void UpdateCode(TermGroup_TimeWorkAccountTransactionActionCode code, string name = null)
        {
            this.Code = code;
            if (name != null)
                this.CodeName = name;
        }
    }

    [TSInclude]
    public class TimeWorkAccountChoiceResultDTO
    {
        public ActionResult Result { get; set; }
        public List<TimeWorkAccountChoiceResultRowDTO> Rows { get; set; }

        public TimeWorkAccountChoiceResultDTO(ActionResult result = null)
        {
            this.Result = result ?? new ActionResult(true);
            this.Rows = new List<TimeWorkAccountChoiceResultRowDTO>();
        }
    }

    [TSInclude]
    public class TimeWorkAccountChoiceResultRowDTO
    {
        public int TimeWorkAccountId { get; set; }
        public int TimeWorkAccountYearId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeWorkAccountYearEmployeeId { get; set; }
        public string EmployeeNrAndName { get; set; }
        public TermGroup_TimeWorkAccountYearSendMailCode Code { get; set; }
        public string CodeName { get; set; }

        public static TimeWorkAccountChoiceResultRowDTO Create(int timeWorkAccountId, int timeWorkAccountYearId, int timeWorkAccountYearEmployeeId, int employeeId, string employeeNrAndName, TermGroup_TimeWorkAccountYearSendMailCode code)
        {
            return new TimeWorkAccountChoiceResultRowDTO
            {
                TimeWorkAccountId = timeWorkAccountId,
                TimeWorkAccountYearId = timeWorkAccountYearId,
                TimeWorkAccountYearEmployeeId = timeWorkAccountYearEmployeeId,
                EmployeeId = employeeId,
                EmployeeNrAndName = employeeNrAndName,
                Code = code,
            };
        }
        public void UpdateCode(TermGroup_TimeWorkAccountYearSendMailCode code, string name = null)
        {
            this.Code = code;
            if (name != null)
                this.CodeName = name;
        }
    }

    #endregion

    #endregion

    #region TimeWorkReduction

    #region Edit DTO

    [TSInclude]
    public class TimeWorkReductionReconciliationDTO
    {
        public int TimeWorkReductionReconciliationId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimeAccumulatorId { get; set; }
        public string Description { get; set; }
        public bool UsePensionDeposit { get; set; }
        public bool UseDirectPayment { get; set; }
        public TermGroup_TimeWorkReductionWithdrawalMethod DefaultWithdrawalMethod { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeWorkReductionReconciliationYearDTO> TimeWorkReductionReconciliationYearDTO { get; set; }

    }

    #endregion

    #region Edit Model

    [TSInclude]
    public class TimeWorkReductionReconciliationEmployeeModel
    {
        public int TimeWorkReductionReconciliationId { get; set; }
        public int TimeWorkReductionReconciliationYearId { get; set; }
        public List<int> TimeWorkReductionReconciliationEmployeeIds { get; set; }
        public List<int> EmployeeIds { get; set; }
    }

    [TSInclude]
    public class TimeWorkReductionReconciliationGenerateOutcomeModel
    {
        public int TimeWorkReductionReconciliationId { get; set; }
        public int TimeWorkReductionReconciliationYearId { get; set; }
        public int PaymentDateId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public List<int> TimeWorkReductionReconciliationEmployeeIds { get; set; }
        public bool OverrideChoosen { get; set; }
    }
    #endregion

    #region Result DTO

    [TSInclude]
    public class TimeWorkReductionReconciliationYearEmployeeResultDTO
    {
        public ActionResult Result { get; set; }
        public List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO> Rows { get; set; }

        public TimeWorkReductionReconciliationYearEmployeeResultDTO(ActionResult result = null)
        {
            this.Result = result ?? new ActionResult(true);
            this.Rows = new List<TimeWorkReductionReconciliationYearEmployeeResultRowDTO>();
        }
    }

    [TSInclude]
    public class TimeWorkReductionReconciliationYearEmployeeResultRowDTO
    {
        public int TimeWorkReductionReconciliationId { get; private set; }
        public int TimeWorkReductionReconciliationYearId { get; private set; }
        public int EmployeeId { get; private set; }
        public string EmployeeNrAndName { get; private set; }
        public TermGroup_TimeWorkReductionReconciliationResultCode Code { get; private set; }
        public string CodeName { get; private set; }
        public TermGroup_TimeWorkReductionReconciliationEmployeeStatus EmployeeStatus { get; private set; }
        public string EmployeeStatusName { get; private set; }
        public TermGroup_TimeWorkReductionWithdrawalMethod Method { get; private set; }
        public string MethodName { get; private set; }

        [TSIgnore]
        public bool HasFailed => (int)Code > 10;

        public static TimeWorkReductionReconciliationYearEmployeeResultRowDTO Create(
           int timeWorkReductionReconciliationId,
           int timeWorkReductionReconciliationYearId,
           int employeeId,
           string employeeNrAndName,
           TermGroup_TimeWorkReductionWithdrawalMethod? method = null
           )
        {
            return new TimeWorkReductionReconciliationYearEmployeeResultRowDTO
            {
                TimeWorkReductionReconciliationId = timeWorkReductionReconciliationId,
                TimeWorkReductionReconciliationYearId = timeWorkReductionReconciliationYearId,
                EmployeeId = employeeId,
                EmployeeNrAndName = employeeNrAndName,
                Method = method ?? TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed
            };
        }
        public TimeWorkReductionReconciliationYearEmployeeResultRowDTO CreateSucess()
        {
            this.Code = TermGroup_TimeWorkReductionReconciliationResultCode.Succeeded;
            return this;
        }

        public void Failed(
            TermGroup_TimeWorkReductionReconciliationResultCode code,
            TermGroup_TimeWorkReductionReconciliationEmployeeStatus employeeStatus = TermGroup_TimeWorkReductionReconciliationEmployeeStatus.NotCalculated
            )
        {
            this.Code = code;
            this.EmployeeStatus = employeeStatus;
        }
        public void Succeeded()
        {
            this.Code = TermGroup_TimeWorkReductionReconciliationResultCode.Succeeded;
            this.EmployeeStatus = TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated;
        }
        public void Deleted()
        {
            this.Code = TermGroup_TimeWorkReductionReconciliationResultCode.Deleted;
            this.EmployeeStatus = TermGroup_TimeWorkReductionReconciliationEmployeeStatus.NotCalculated;
        }
        public void UpdateEmployeeStatusName(TermGroup_TimeWorkReductionReconciliationEmployeeStatus employeeStatus, string name = null)
        {
            this.EmployeeStatus = employeeStatus;
            if (name != null)
                this.EmployeeStatusName = name;
        }
        public void UpdateCode(TermGroup_TimeWorkReductionReconciliationResultCode code, string name = null)
        {
            this.Code = code;
            if (name != null)
                this.CodeName = name;
        }
        public void UpdateMethod(TermGroup_TimeWorkReductionWithdrawalMethod method, string name = null)
        {
            this.Method = method;
            if (name != null)
                this.MethodName = name;
        }
        public TimeWorkReductionReconciliationYearEmployeeResultRowDTO DeleteSuccess()
        {
            this.Code = TermGroup_TimeWorkReductionReconciliationResultCode.DeleteSuccess;
            return this;
        }
    }

    #endregion

    #region Pension Export
    [TSInclude]
    public class TimeWorkReductionExportPensionDTO
    {
        public int TimeWorkReductionReconciliationId { get; set; }
        public int TimeWorkReductionReconciliationYearId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeSocialSec { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion 

    #endregion

    #region Grid

    [TSInclude]
    public class TimeWorkReductionReconciliationGridDTO
    {
        public int TimeWorkReductionReconciliationId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimeAccumulatorId { get; set; }
        public string Description { get; set; }
        public bool UsePensionDeposit { get; set; }
        public bool UseDirectPayment { get; set; }
        public TermGroup_TimeWorkReductionWithdrawalMethod DefaultWithdrawalMethod { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region TimeWorkReductionReconciliationYear
    [TSInclude]
    public class TimeWorkReductionReconciliationYearDTO
    {
        public int TimeWorkReductionReconciliationYearId { get; set; }
        public int TimeWorkReductionReconciliationId { get; set; }
        public DateTime Stop { get; set; }
        public DateTime EmployeeLastDecidedDate { get; set; }
        public int? PensionDepositPayrollProductId { get; set; }
        public int? DirectPaymentPayrollProductId { get; set; }
        public SoeEntityState State { get; set; }

        public List<TimeWorkReductionReconciliationEmployeeDTO> TimeWorkReductionReconciliationEmployeeDTO { get; set; }
    }


    #endregion

    #region TimeWorkReductionReconciliationEmployee
    [TSInclude]
    public class TimeWorkReductionReconciliationEmployeeDTO
    {
        public int TimeWorkReductionReconciliationEmployeeId { get; set; }
        public int TimeWorkReductionReconciliationYearId { get; set; }
        public int TimeWorkReductionReconciliationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeNrAndName { get; set; }
        public int MinutesOverThreshold { get; set; }
        public DateTime? SentDate { get; set; }
        public TermGroup_TimeWorkReductionWithdrawalMethod SelectedWithdrawalMethod { get; set; }
        public DateTime? SelectedDate { get; set; }
        public int Status { get; set; }
        public SoeEntityState State { get; set; }
        public int AccEarningMinutes { get; set; }
        public int Threshold { get; set; }

    }

    #endregion

    #endregion

    #region TrackChanges

    public class TrackChangesDTO
    {
        public int TrackChangesId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? TopRecordId { get; set; }
        public int RecordId { get; set; }
        public int? ParentRecordId { get; set; }
        public SoeEntityType TopEntity { get; set; }
        public SoeEntityType Entity { get; set; }
        public SoeEntityType ParentEntity { get; set; }
        public TermGroup_TrackChangesAction Action { get; set; }
        public TermGroup_TrackChangesActionMethod ActionMethod { get; set; }
        public TermGroup_TrackChangesColumnType ColumnType { get; set; }
        public SettingDataType DataType { get; set; }
        public string ColumnName { get; set; }
        public string Role { get; set; }
        public Guid Batch { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public string FromValueName { get; set; }
        public string ToValueName { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        public TrackChangesDTO()
        {

        }

        public TrackChangesDTO(
            int actorCompanyId, int? topRecordId, int recordId, int? parentRecordId,
            SoeEntityType topEntity, SoeEntityType entity, SoeEntityType parentEntity,
            TermGroup_TrackChangesAction action, TermGroup_TrackChangesActionMethod actionMethod, TermGroup_TrackChangesColumnType columnType, SettingDataType dataType,
            string columnName, string role, Guid batch,
            string fromValue, string toValue, string fromValueName, string toValueName)
        {
            this.ActorCompanyId = actorCompanyId;
            this.TopRecordId = topRecordId;
            this.RecordId = recordId;
            this.ParentRecordId = parentRecordId;
            this.TopEntity = topEntity;
            this.Entity = entity;
            this.ParentEntity = parentEntity;
            this.Action = action;
            this.ActionMethod = actionMethod;
            this.ColumnType = columnType;
            this.DataType = dataType;
            this.ColumnName = columnName.EmptyToNull();
            this.Role = role;
            this.Batch = batch;
            this.FromValue = fromValue.EmptyToNull();
            this.FromValueName = fromValueName.EmptyToNull();
            this.ToValue = toValue.EmptyToNull();
            this.ToValueName = toValueName.EmptyToNull();
        }
    }

    [TSInclude]
    public class TrackChangesLogDTO
    {
        public int TrackChangesId { get; set; }
        public Guid Batch { get; set; }
        public int BatchNbr { get; set; }
        public string ActionMethodText { get; set; }
        public SoeEntityType Entity { get; set; }
        public string EntityText { get; set; }
        public string ColumnText { get; set; }
        public string ActionText { get; set; }
        public string FromValueText { get; set; }
        public string ToValueText { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public string Role { get; set; }
        public int RecordId { get; set; }
        public string RecordName { get; set; }
        public string TopRecordName { get; set; }
    }

    #endregion

    #region UnionFee

    [TSInclude]
    public class UnionFeeDTO
    {
        public int UnionFeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int? PayrollPriceTypeIdPercent { get; set; }
        public int? PayrollPriceTypeIdPercentCeiling { get; set; }
        public int? PayrollPriceTypeIdFixedAmount { get; set; }
        public int? PayrollProductId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public int Association { get; set; }
    }

    [TSInclude]
    public class UnionFeeGridDTO
    {
        public int UnionFeeId { get; set; }
        public string Name { get; set; }
        public string PayrollPriceTypeIdPercentName { get; set; }
        public string PayrollPriceTypeIdPercentCeilingName { get; set; }
        public string PayrollPriceTypeIdFixedAmountName { get; set; }
        public string PayrollProductName { get; set; }
        public SoeEntityState State { get; set; }
        public int Association { get; set; }
    }

    #endregion

    #region UploadedFile

    [TSInclude]
    public class FileDTO
    {
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
    }

    public class UploadedFileDTO
    {
        public int UploadedFileId { get; set; }
        public int ActorCompanyId { get; set; }
        public int DataStorageId { get; set; }

        public string FileName { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string Folder { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public DataStorageDTO DataStorage { get; set; }
        [TsIgnore]
        public bool IsSelected { get; set; }
    }

    #endregion

    #region User

    public class AttestRoleExtendedUserDTO
    {
        public UserDTO User { get; set; }
        public List<UserAttestRoleDTO> UserAttestRoles { get; set; } = new List<UserAttestRoleDTO>();
        public List<UserCompanyRoleDTO> UserCompanyRoles { get; set; } = new List<UserCompanyRoleDTO>();
    }

    public class UserDTO : IUser
    {
        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public Guid LicenseGuid { get; set; }
        public int? DefaultActorCompanyId { get; set; }
        [TsIgnore]
        public int DefaultRoleId { get; set; }
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public SoeEntityState State { get; set; }
        public bool ChangePassword { get; set; }
        public int? LangId { get; set; }
        public DateTime? BlockedFromDate { get; set; }
        public string EstatusLoginId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsMobileUser { get; set; }
        public Guid? idLoginGuid { get; set; }
        public bool HasUserVerifiedEmail { get; set; }
        public bool EmailCopy { get; set; }

        public static UserDTO Create(int userId, int licenseId, int roleId, string loginName, bool isAdmin, bool hasVerifiedEmail, Guid? idLoginGuid = null)
        {
            return new UserDTO
            {
                LicenseId = licenseId,
                DefaultRoleId = roleId,
                UserId = userId,
                LoginName = loginName,
                IsAdmin = isAdmin,
                idLoginGuid = idLoginGuid,
            };
        }
    }

    [TSInclude]
    public class UserSmallDTO
    {
        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public int? DefaultActorCompanyId { get; set; }
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? LangId { get; set; }
        public DateTime? BlockedFromDate { get; set; }
        public bool IdLoginActive { get; set; }
        public SoeEntityState State { get; set; }
        public bool HideEditButton { get; set; }
        public bool AllowSupportLogin { get; set; }
        public bool ChangePassword { get; set; }

        // Extensions
        public string DefaultRoleName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSelectedForEmail { get; set; }
        public bool Main { get; set; }
        public bool AttestFlowIsRequired { get; set; }
        public bool AttestFlowHasAnswered { get; set; }
        public int AttestFlowRowId { get; set; }
        public int AttestRoleId { get; set; }
        public string Categories { get; set; }

    }

    public class UserGridDTO
    {
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string DefaultRoleName { get; set; }
        public string Email { get; set; }
        public bool IdLoginActive { get; set; }
        public string ExternalAuthId { get; set; }
        public string SoftOneIdLoginName { get; set; }
        public SoeEntityState State { get; set; }

        // Support login
        public int? DefaultActorCompanyId { get; set; }
    }

    public class UserRequestTypeDTO : UserSmallDTO
    {
        public TermGroup_EmployeeRequestTypeFlags EmployeeRequestTypes { get; set; }
    }

    [TSInclude]
    public class UserWithNameAndLoginDTO
    {
        public string LoginName { get; set; }
        public string UserNameAndLogin { get; set; }
    }

    [TSInclude]
    public class UserForOriginDTO
    {
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool Main { get; set; }
        public string Categories { get; set; }
    }

    #endregion

    #region UserCompanyRoleDelegateHistory

    public class UserCompanyRoleDelegateHistoryHeadDTO
    {
        public int UserCompanyRoleDelegateHistoryHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int ByUserId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<UserCompanyRoleDelegateHistoryRowDTO> Rows { get; set; }
    }

    public class UserCompanyRoleDelegateHistoryRowDTO
    {
        public int UserCompanyRoleDelegateHistoryRowId { get; set; }
        public int UserCompanyRoleDelegateHistoryHeadId { get; set; }
        public int? ParentId { get; set; }
        public int? RoleId { get; set; }
        public int? AttestRoleId { get; set; }
        public int? AccountId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class UserCompanyRoleDelegateHistoryGridDTO
    {
        public int UserCompanyRoleDelegateHistoryHeadId { get; set; }

        public int FromUserId { get; set; }
        public string FromUserName { get; set; }
        public int ToUserId { get; set; }
        public string ToUserName { get; set; }
        public int ByUserId { get; set; }
        public string ByUserName { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string RoleNames { get; set; }
        public string AttestRoleNames { get; set; }

        public DateTime? Created { get; set; }
        public SoeEntityState State { get; set; }

        public bool ShowDelete { get; set; }
    }

    public class UserCompanyRoleDelegateHistoryUserDTO
    {
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public List<UserCompanyRoleDTO> TargetRoles { get; set; }
        public List<UserAttestRoleDTO> TargetAttestRoles { get; set; }
        public List<UserCompanyRoleDTO> PossibleTargetRoles { get; set; }
        public List<UserAttestRoleDTO> PossibleTargetAttestRoles { get; set; }
    }

    #endregion

    #region UserCompanySetting

    public class UserCompanySettingEditDTO
    {
        public int UserCompanySettingId { get; set; }
        public SettingMainType SettingMainType { get; set; }
        public int SettingTypeId { get; set; }

        // Data
        public SettingDataType DataType { get; set; }
        public string StringValue { get; set; }
        public int? IntegerValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public bool? BooleanValue { get; set; }
        public DateTime? DateValue { get; set; }

        public List<SmallGenericType> Options { get; set; }

        // Grouping
        public string GroupLevel1 { get; set; }
        public string GroupLevel2 { get; set; }
        public string GroupLevel3 { get; set; }
        public string Name { get; set; }

        // Flags
        public bool IsModified { get; set; }
        public bool VisibleOnlyForSupportAdmin { get; set; }

        public UserCompanySettingEditDTO(SettingMainType settingMainType, int settingType)
        {
            this.SettingMainType = settingMainType;
            this.SettingTypeId = settingType;
        }
    }

    #endregion

    #region UserReplacement

    public class UserReplacementDTO
    {
        public int UserReplacementId { get; set; }
        public int ActorCompanyId { get; set; }

        public UserReplacementType Type { get; set; }
        public int OriginUserId { get; set; }
        public int ReplacementUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }

        public SoeEntityState State { get; set; }
    }

    #endregion

    #region UserSelection

    [TSInclude]
    public class UserSelectionDTO
    {
        public int UserSelectionId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? UserId { get; set; }
        public UserSelectionType Type { get; set; }
        public bool Default { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<ReportDataSelectionDTO> Selections { get; set; }
        public string Selection { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public List<UserSelectionAccessDTO> Access { get; set; }
    }

    #endregion

    #region UserSelectionAccess

    [TSInclude]
    public class UserSelectionAccessDTO
    {
        public int UserSelectionAccessId { get; set; }
        public int UserSelectionId { get; set; }
        public TermGroup_ReportUserSelectionAccessType Type { get; set; }
        public int? RoleId { get; set; }
        public int? MessageGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    #endregion

    #region UserSession

    public class UserSessionDTO
    {
        public int UserSessionId { get; set; }
        public DateTime Login { get; set; }
        public DateTime? Logout { get; set; }
        public bool RemoteLogin { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public bool MobileLogin { get; set; }
        public string Browser { get; set; }
        public string Screen { get; set; }
        public string Silverlight { get; set; }
        public string Platform { get; set; }
        public string ClientIP { get; set; }
        public string Host { get; set; }
        public string CacheCredentials { get; set; }
        public string Token { get; set; }
    }

    #endregion

    #region Vacation

    public class CalculateVacationResultContainerDTO
    {
        public List<CalculateVacationResultDTO> FormulaResult { get; set; }
        public Dictionary<string, string> FormulasUsed { get; set; }

        public CalculateVacationResultContainerDTO()
        {
            FormulaResult = new List<CalculateVacationResultDTO>();
            FormulasUsed = new Dictionary<string, string>();
        }
    }

    public class CalculateVacationResultDTO
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string FormulaPlain { get; set; }        // As stored in field FormulaPlain (with codes)
        public string FormulaExtracted { get; set; }    // Formula extracted with values
        public string FormulaNames { get; set; }        // FormulaPlain with codes replaced with names
        public string FormulaOrigin { get; set; }       // FormulaPlain with origin (what entity did the value come from)
        public string Error { get; set; }

        private List<CalculateVacationResultDTO> children;
        public bool HasChildren
        {
            get
            {
                return this.children != null && this.children.Count > 0;
            }
        }

        public int EmployeeId { get; set; }
        public bool SRTDAdded { get; set; } = false;

        #region Constructors

        public CalculateVacationResultDTO()
        {
            this.Success = true;
            this.Name = "";
        }

        public CalculateVacationResultDTO(string name)
        {
            this.Success = true;
            this.Name = name;
        }

        public CalculateVacationResultDTO(string error, string name)
        {
            this.SetError(error, name);
        }

        #endregion

        #region Public methods

        public void AddChild(CalculateVacationResultDTO child)
        {
            if (this.children == null)
                this.children = new List<CalculateVacationResultDTO>();
            this.children.Add(child);
        }

        public List<CalculateVacationResultDTO> GetChildren()
        {
            return this.children ?? new List<CalculateVacationResultDTO>();
        }

        public List<CalculateVacationResultDTO> GetAllChildren()
        {
            var allChildren = new List<CalculateVacationResultDTO>();
            foreach (var child in GetChildren())
            {
                allChildren.Add(child);
                allChildren.AddRange(child.GetAllChildren());
            }
            return allChildren;
        }

        public void SetError(string error, string name)
        {
            this.Success = false;
            this.Error = error;
            this.Name = name;
        }

        #endregion
    }

    #endregion

    #region VacationGroup

    public class VacationGroupGridDTO
    {
        public int VacationGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public TermGroup_VacationGroupType Type { get; set; }
        public DateTime FromDate { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string FromDateName { get; set; }
    }

    public class VacationGroupDTO
    {
        public int VacationGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_VacationGroupType Type { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public int? VacationDaysPaidByLaw { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Relations
        public VacationGroupSEDTO VacationGroupSE { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public DateTime RealDateFrom { get; set; }
        public DateTime RealDateTo
        {
            get
            {
                return RealDateFrom.AddYears(1).AddDays(-1);
            }
        }

        public DateTime? LatesVacationYearEnd { get; set; }

        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
    }

    public class VacationGroupSEDTO
    {
        public int VacationGroupSEId { get; set; }
        public int VacationGroupId { get; set; }

        public TermGroup_VacationGroupCalculationType CalculationType { get; set; }

        public bool UseAdditionalVacationDays { get; set; }
        public int NbrOfAdditionalVacationDays { get; set; }
        public int? AdditionalVacationDaysFromAge1 { get; set; }
        public int? AdditionalVacationDays1 { get; set; }
        public int? AdditionalVacationDaysFromAge2 { get; set; }
        public int? AdditionalVacationDays2 { get; set; }
        public int? AdditionalVacationDaysFromAge3 { get; set; }
        public int? AdditionalVacationDays3 { get; set; }

        public TermGroup_VacationGroupVacationHandleRule VacationHandleRule { get; set; }
        public TermGroup_VacationGroupVacationDaysHandleRule VacationDaysHandleRule { get; set; }
        public bool VacationDaysGrossUseFiveDaysPerWeek { get; set; }

        public TermGroup_VacationGroupRemainingDaysRule RemainingDaysRule { get; set; }
        public bool UseMaxRemainingDays { get; set; }
        public int? MaxRemainingDays { get; set; }
        public int? RemainingDaysPayoutMonth { get; set; }

        public DateTime EarningYearAmountFromDate { get; set; }
        public DateTime? EarningYearVariableAmountFromDate { get; set; }

        public int? MonthlySalaryFormulaId { get; set; }
        public int? HourlySalaryFormulaId { get; set; }

        public decimal? VacationDayPercent { get; set; }
        public decimal? VacationDayAdditionPercent { get; set; }
        public decimal? VacationVariablePercent { get; set; }
        public int? VacationDayPercentPriceTypeId { get; set; }
        public int? VacationDayAdditionPercentPriceTypeId { get; set; }
        public int? VacationVariablePercentPriceTypeId { get; set; }

        public bool UseGuaranteeAmount { get; set; }
        public bool GuaranteeAmountAccordingToHandels { get; set; }
        public TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule GuaranteeAmountMaxNbrOfDaysRule { get; set; }
        public int? GuaranteeAmountEmployedNbrOfYears { get; set; }
        public int? GuaranteeAmountPerDayPriceTypeId { get; set; }
        public bool GuaranteeAmountJuvenile { get; set; }
        public int? GuaranteeAmountJuvenileAgeLimit { get; set; }
        public int? GuaranteeAmountJuvenilePerDayPriceTypeId { get; set; }

        public bool UseFillUpToVacationDaysPaidByLawRule { get; set; }
        public bool UseOwnGuaranteeAmount { get; set; }
        public decimal? OwnGuaranteeAmount { get; set; }

        public TermGroup_VacationGroupVacationAbsenceCalculationRule VacationAbsenceCalculationRule { get; set; }

        public TermGroup_VacationGroupVacationSalaryPayoutRule VacationSalaryPayoutRule { get; set; }
        public int? VacationSalaryPayoutDays { get; set; }
        public int? VacationSalaryPayoutMonth { get; set; }

        public TermGroup_VacationGroupVacationSalaryPayoutRule VacationVariablePayoutRule { get; set; }
        public int? VacationVariablePayoutDays { get; set; }
        public int? VacationVariablePayoutMonth { get; set; }

        public TermGroup_VacationGroupYearEndRemainingDaysRule YearEndRemainingDaysRule { get; set; }
        public TermGroup_VacationGroupYearEndOverdueDaysRule YearEndOverdueDaysRule { get; set; }
        public TermGroup_VacationGroupYearEndVacationVariableRule YearEndVacationVariableRule { get; set; }

        public int? ReplacementTimeDeviationCauseId { get; set; }

        public int? ValueDaysDebitAccountId { get; set; }
        public int? ValueDaysCreditAccountId { get; set; }
        public bool ValueDaysAccountInternalOnDebit { get; set; }
        public bool ValueDaysAccountInternalOnCredit { get; set; }
        public bool UseEmploymentTaxAcccount { get; set; }
        public int? EmploymentTaxDebitAccountId { get; set; }
        public int? EmploymentTaxCredidAccountId { get; set; }
        public bool EmploymentTaxAccountInternalOnDebit { get; set; }
        public bool EmploymentTaxAccountInternalOnCredit { get; set; }
        public bool UseSupplementChargeAccount { get; set; }
        public int? SupplementChargeDebitAccountId { get; set; }
        public int? SupplementChargeCreditAccountId { get; set; }
        public bool SupplementChargeAccountInternalOnDebit { get; set; }
        public bool SupplementChargeAccountInternalOnCredit { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public List<VacationGroupSEDayTypeDTO> VacationGroupSEDayTypes { get; set; }
        public bool ShowHours
        {
            get { return (this.VacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Hours || this.VacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Shifts); }
            set { }//NOSONAR
        }


    }
    public class VacationGroupSEDayTypeDTO
    {
        public int VacationGroupSEDayTypeId { get; set; }
        public int DayTypeId { get; set; }
        public int VacationGroupSEId { get; set; }
        public SoeVacationGroupDayType Type { get; set; }
    }

    #endregion

    #region VacationYearEnd

    public class VacationYearEndHeadDTO
    {
        public int VacationYearEndHeadId { get; set; }
        public DateTime Date { get; set; }
        public TermGroup_VacationYearEndHeadContentType ContentType { get; set; }
        public string ContentTypeName { get; set; }
        public string Content { get; set; }
        public string EmployeesFailed { get; set; }
        public int? DataStorageId { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }

        // Relations
        public List<VacationYearEndRowDTO> Rows { get; set; }
    }

    public class VacationYearEndRowDTO
    {
        public int EmployeeId { get; set; }
        public int? EmployeeVacationSEId { get; set; }
    }

    #endregion

    #region VatCode

    [TSInclude]
    public class VatCodeDTO
    {
        public int VatCodeId { get; set; }
        public int AccountId { get; set; }
        public int? PurchaseVATAccountId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Percent { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string AccountNr { get; set; }
        public string PurchaseVATAccountNr { get; set; }
    }

    [TSInclude]
    public class VatCodeGridDTO
    {
        public int VatCodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string PurchaseVATAccount { get; set; }
        public decimal Percent { get; set; }
    }

    #endregion

    #region WorkRuleBypassLog

    [TSInclude]
    public class WorkRuleBypassLogGridDTO
    {
        public int WorkRuleBypassLogId { get; set; }
        public DateTime Date { get; set; }
        public string EmployeeNrAndName { get; set; }
        public string Message { get; set; }
        public string ActionText { get; set; }
        public string CreatedBy { get; set; }
    }

    [TSInclude]
    public class WorkRuleBypassLogDTO
    {
        public int WorkRuleBypassLogId { get; set; }
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public SoeScheduleWorkRules WorkRule { get; set; }
        public TermGroup_ShiftHistoryType Action { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string EmployeeName { get; set; }
        public string ActionText { get; set; }
        public string EmployeeNrAndName { get; set; }
    }

    #endregion

    #region POCO

    public class XEWallDTO
    {
        public int XEWallId { get; set; }
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }
    }

    public class SideDictionaryIODTO
    {
        public int Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Account1Nr { get; set; }
        public string Account2Nr { get; set; }
        public string GroupName { get; set; }
        public decimal Percent { get; set; }
        public bool VatIncluded { get; set; }
        public decimal Quantity { get; set; }
        public decimal Quantity2 { get; set; }
        public decimal Discount { get; set; }
        public int DictionaryType { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
    }

    public class AttestStateIODTO
    {
        public string Name { get; set; }
        public string EntityName { get; set; }
        public string ModuleName { get; set; }
        public int Sort { get; set; }
        public bool Initial { get; set; }
        public bool Closed { get; set; }
        public bool Hidden { get; set; }
        public bool Locked { get; set; }
    }

    public class EmployeeCalculateVacationResultFlattenedDTO
    {
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime Date { get; set; }
        public string DateStr { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNrAndName { get; set; }

        public decimal VIDValue { get; set; }
        public decimal VBDValue { get; set; }
        public decimal VSDValue { get; set; }
        public decimal VBSTRValue { get; set; }
        public decimal BSTRAValue { get; set; }
        public decimal VSSTRValue { get; set; }
        public decimal SSTRAValue { get; set; }
        public decimal VISTRValue { get; set; }
        public decimal VFDValue { get; set; }
        public decimal TotVSTRValue { get; set; }
        public decimal TotValue { get; set; }

        public DateTime? Created { get; set; }

    }
    #endregion

    #region Views

    #region Account

    public class AccountGridViewDTO
    {
        public int AccountId { get; set; }
        public int ActorCompanyId { get; set; }
        public int? LangId { get; set; }
        public int AccountDimId { get; set; }
        public int? AccountTypeSysTermId { get; set; }
        public int? SysVatAccountId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public SoeEntityState State { get; set; }
        public string VatType { get; set; }
        public decimal Balance { get; set; }
        // Extensions
        public bool IsSelected { get; set; }
    }

    public class AccountVatRateViewDTO
    {
        public int ActorCompanyId { get; set; }
        public int AccountDimId { get; set; }
        public int AccountId { get; set; }
        public int? SysVatAccountId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public decimal? VatRate { get; set; }
    }

    public class AccountVatRateViewSmallDTO
    {
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public decimal? VatRate { get; set; }
    }

    #endregion

    #region Currency

    [TSInclude]
    public class CompCurrencyDTO
    {
        public int CurrencyId { get; set; }
        public int SysCurrencyId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public DateTime Date { get; set; }
        public decimal RateToBase { get; set; }

        // Extensions
        public List<CompCurrencyRateDTO> CompCurrencyRates { get; set; }
    }

    //Should raplce CompCurrencyDTO in the future (as that is based on a WebForms approach).
    [TSInclude]
    public class CurrencyDTO
    {
        public int CurrencyId { get; set; }
        public int SysCurrencyId { get; set; }

        public TermGroup_CurrencyIntervalType IntervalType { get; set; }
        public string IntervalName { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public List<CurrencyRateDTO> CurrencyRates { get; set; }
    }

    [TSInclude]
    public class CurrencyRateDTO
    {
        public int CurrencyRateId { get; set; }
        public int CurrencyId { get; set; }
        public decimal RateToBase { get; set; }
        public decimal RateFromBase { get; set; }
        public TermGroup_CurrencySource Source { get; set; }
        public string SourceName { get; set; }
        public DateTime Date { get; set; }
        public bool DoDelete { get; set; }
        public bool IsModified { get; set; }
    }

    [TSInclude]
    public class CurrencyGridDTO
    {
        public int CurrencyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string IntervalName { get; set; }
    }


    public class CompCurrencyGridDTO
    {
        public int CurrencyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        // Extensions
        public List<CompCurrencyRateDTO> CompCurrencyRates { get; set; }

        [TsIgnore]
        public bool DetailVisible { get; set; }
    }

    [TSInclude]
    public class CompCurrencySmallDTO
    {
        public int CurrencyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    [TSInclude]
    public class CompCurrencyRateDTO
    {
        public int CurrencyId { get; set; }
        public int? CurrencyRateId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public TermGroup_CurrencyIntervalType IntervalType { get; set; }
        public TermGroup_CurrencySource Source { get; set; }
        public DateTime? Date { get; set; }
        public decimal RateToBase { get; set; }

        // Extensions
        public string IntervalTypeName { get; set; }
        public string SourceName { get; set; }
    }

    public class CompCurrencyRateGridDTO
    {
        public int? CurrencyRateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public decimal RateToBase { get; set; }
        // Extensions
        public string IntervalTypeName { get; set; }
        public string SourceName { get; set; }
    }

    public class CalculateCurrencyResultDTO
    {
        public decimal BaseCurrencyAmount { get; set; }
        public decimal TransactionCurrencyAmount { get; set; }
        public decimal EnterpriseCurrencyAmount { get; set; }
        public decimal LedgerCurrencyAmount { get; set; }
    }

    #endregion

    #region CustomerInvoiceRowAttestStateView

    [TSInclude]
    public class CustomerInvoiceRowAttestStateViewDTO
    {
        public int InvoiceId { get; set; }
        public int ActorCompanyId { get; set; }
        public int AttestStateId { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public string Color { get; set; }
    }

    #endregion

    #region EDI/Scanning

    [TSInclude]

    public class EdiEntryViewDTO
    {
        //Edi
        public int EdiEntryId { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_EDISourceType Type { get; set; }
        public TermGroup_EDIStatus Status { get; set; }
        public string StatusName { get; set; }
        public TermGroup_EdiMessageType EdiMessageType { get; set; }
        public string EdiMessageTypeName { get; set; }
        public TermGroup_ScanningMessageType ScanningMessageType { get; set; }
        public string ScanningMessageTypeName { get; set; }
        public string SourceTypeName { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public string BillingTypeName { get; set; }
        public int WholesellerId { get; set; }
        public string WholesellerName { get; set; }
        public string BuyerId { get; set; }
        public string BuyerReference { get; set; }
        public bool HasPdf { get; set; }
        public int ErrorCode { get; set; }
        public DateTime? Created { get; set; }
        public SoeEntityState State { get; set; }
        public string ErrorMessage { get; set; }
        public EdiImportSource ImportSource { get; set; }

        //Scanning
        public int? ScanningEntryId { get; set; }
        public int NrOfPages { get; set; }
        public int NrOfInvoices { get; set; }
        public string OperatorMessage { get; set; }
        public TermGroup_ScanningStatus ScanningStatus { get; set; }

        //Dates
        public DateTime? Date { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        //Sum
        public decimal Sum { get; set; }
        public decimal SumCurrency { get; set; }
        public decimal SumVat { get; set; }
        public decimal SumVatCurrency { get; set; }

        //Currency
        public int SysCurrencyId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        //Order
        public int? OrderId { get; set; }
        public TermGroup_EDIOrderStatus OrderStatus { get; set; }
        public string OrderStatusName { get; set; }
        public string OrderNr { get; set; }
        public string SellerOrderNr { get; set; }

        //Invoice
        public int? InvoiceId { get; set; }
        public TermGroup_EDIInvoiceStatus InvoiceStatus { get; set; }
        public string InvoiceStatusName { get; set; }
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }

        //Customer
        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }

        //Supplier
        public int? SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }

        //Lang
        public int LangId { get; set; }

        // Extensions
        public int? SupplierAttestGroupId { get; set; }
        public string SupplierAttestGroupName { get; set; }
        public decimal RoundedInterpretation { get; set; }
        public bool IsVisible { get; set; }
        public bool IsModified { get; set; }
        public bool IsSelectDisabled { get; set; }
        public bool IsSelected { get; set; }
    }

    [TSInclude]
    public class UpdateEdiEntryDTO
    {
        public int EdiEntryId { get; set; }
        public int? SupplierId { get; set; }
        public int? AttestGroupId { get; set; }
        public int? ScanningEntryId { get; set; }
        public string OrderNr { get; set; }
    }

    #endregion

    #region EmployeeSchedulePlacement

    public class EmployeeSchedulePlacementGridViewDTO
    {
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public DateTime? EmployeeEndDate { get; set; }
        public decimal EmployeeWorkPercentage { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public int EmployeePosition { get; set; }
        public bool IsPreliminary { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public string TimeScheduleTemplateHeadName { get; set; }
        public int TimeScheduleTemplateHeadNoOfDays { get; set; }
        public int? TemplateEmployeeId { get; set; }
        public DateTime? TemplateStartDate { get; set; }
        public int EmployeeScheduleId { get; set; }
        public DateTime? EmployeeScheduleStartDate { get; set; }
        public DateTime? EmployeeScheduleStopDate { get; set; }
        public int EmployeeScheduleStartDayNumber { get; set; }

        //EmployeeGroup
        public int EmployeeGroupId { get; set; }
        public string EmployeeGroupName { get; set; }
        public int EmployeeGroupWorkTimeWeek { get; set; }
        public int BreakDayMinutesAfterMidnight { get; set; }
        public int KeepStampsTogetherWithinMinutes { get; set; }
        public bool AutogenTimeblocks { get; set; }
        public bool AutogenBreakOnStamping { get; set; }
        public bool MergeScheduleBreaksOnDay { get; set; }
        public bool AlwaysDiscardBreakEvaluation { get; set; }

        // Extensions
        public bool IsPlaced { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public bool IsModified { get; set; }
        public List<EmploymentDTO> Employments { get; set; }

        public string EmployeeNrSort
        {
            get { return EmployeeNr.PadLeft(50, '0'); }
        }
        public string EmployeeName
        {
            get { return EmployeeFirstName + " " + EmployeeLastName; }
        }
        public string EmployeeInfo
        {
            get { return String.Format("({0}) {1}", EmployeeNr, EmployeeName); }
        }
        public bool IsPersonalTemplate
        {
            get { return this.TemplateEmployeeId.HasValue && this.TemplateEmployeeId.Value > 0 && this.TemplateStartDate.HasValue; }
        }
    }
    [TSInclude]
    public class ActivateScheduleGridDTO
    {
        public int EmployeeScheduleId { get; set; }
        public bool IsPlaced { get; set; }
        public bool IsPreliminary { get; set; }
        public DateTime? EmployeeScheduleStartDate { get; set; }
        public DateTime? EmployeeScheduleStopDate { get; set; }
        public int EmployeeScheduleStartDayNumber { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        [TsIgnore]
        public string EmployeNrAndName
        {
            get
            {
                return $"({this.EmployeeNr}) {this.EmployeeName}";
            }
        }
        public DateTime? EmploymentEndDate { get; set; }
        public bool EmployeeHidden { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public string TimeScheduleTemplateHeadName { get; set; }
        public int? TemplateEmployeeId { get; set; }
        public DateTime? TemplateStartDate { get; set; }
        public bool SimpleSchedule { get; set; }
        public int EmployeeGroupId { get; set; }
        public string EmployeeGroupName { get; set; }
        public string CategoryNamesString { get; set; }
        public string AccountNamesString { get; set; }

        public static ActivateScheduleGridDTO Create(int employeeId, DateTime? employeeScheduleStartDate, DateTime? employeeScheduleStopDate, bool isPlaced = true, bool isPreliminary = false)
        {
            return new ActivateScheduleGridDTO
            {
                EmployeeId = employeeId,
                EmployeeScheduleStartDate = employeeScheduleStartDate,
                EmployeeScheduleStopDate = employeeScheduleStopDate,
                IsPlaced = isPlaced,
                IsPreliminary = isPreliminary,
            };
        }

        // Extensions
        [TsIgnore]
        public string EmployeeInfo
        {
            get { return String.Format("({0}) {1}", EmployeeNr, EmployeeName); }
        }
        public bool IsPersonalTemplate
        {
            get { return this.TemplateEmployeeId.HasValue && this.TemplateEmployeeId.Value > 0 && this.TemplateStartDate.HasValue; }
        }
    }

    #endregion

    #region Household tax deduction

    [Log]
    [TSInclude]
    public class HouseholdTaxDeductionGridViewDTO
    {
        public int? ActorCompanyId { get; set; }
        public int CustomerInvoiceRowId { get; set; }
        public int? VoucherHeadId { get; set; }

        public SoeOriginStatus OriginStatus { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }

        public string Property { get; set; }
        [LogSocSec]
        public string SocialSecNr { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal? ApprovedAmount { get; set; }

        public DateTime? PayDate { get; set; }
        public bool FullyPayed { get; set; }

        public int? SeqNr { get; set; }
        public string HouseholdStatus { get; set; }

        public string VoucherNr { get; set; }

        public bool Applied { get; set; }
        public DateTime? AppliedDate { get; set; }
        public bool Received { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public bool Denied { get; set; }
        public DateTime? DeniedDate { get; set; }

        public SoeEntityState State { get; set; }
        public int HouseHoldTaxDeductionType { get; set; }
        public string HouseHoldTaxDeductionTypeName { get; set; }
        public string HouseHoldTaxDeductionPercent { get; set; }
        public int ProductId { get; set; }
        public string Comment { get; set; }

        // Extensions
        public int Status { get; set; }
        public string StatusIcon { get; set; }
        public string StatusIconClass { get; set; }
    }

    #endregion

    #region PaymentInformation

    [TSInclude]
    public class PaymentInformationViewDTO
    {
        public int LangId { get; set; }
        public int ActorId { get; set; }
        public int SysPaymentTypeId { get; set; }
        public int DefaultSysPaymentTypeId { get; set; }
        public int PaymentInformationRowId { get; set; }
        public int? CurrencyId { get; set; }

        public string PaymentNr { get; set; }
        public string PaymentNrDisplay { get; set; }
        public string Name { get; set; }
        public bool Default { get; set; }
    }
    [TSInclude]
    public class PaymentInformationViewDTOSmall
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
    }

    #endregion

    #region PriceList

    public class CompanyWholesellerPriceListSmallDTO
    {
        public int SysPriceListHeadId { get; set; }
        public int SysWholesellerId { get; set; }
        public int Provider { get; set; }
    }

    public class CompanyWholesellerPriceListViewDTO
    {
        public int SysPriceListHeadId { get; set; }
        public int SysWholesellerId { get; set; }
        public TermGroup_Country SysWholesellerCountry { get; set; }
        public int? CompanyWholesellerPriceListId { get; set; }
        public int? PriceListImportedHeadId { get; set; }
        public int? CompanyWholesellerId { get; set; }
        public int? ActorCompanyId { get; set; }

        public string SysWholesellerName { get; set; }
        public string PriceListName { get; set; }

        public int Provider { get; set; }
        public int? Version { get; set; }
        public DateTime Date { get; set; }

        public bool IsUsed { get; set; }
        public PriceListOrigin PriceListOrigin { get; set; }
        public bool HasNewerVersion { get; set; }
        public string TypeName { get; set; }
    }

    [TSInclude]
    public class SysWholsesellerPriceSearchDTO
    {
        public int SysProductId { get; set; }
        public int SysWholesellerId { get; set; }
        public decimal GNP { get; set; }
        public string Code { get; set; }
        public string ProductNr { get; set; }
    }

    #endregion

    #region PriceRule

    public class CompanyPriceRuleDTO
    {
        public int RuleId { get; set; }
        public int PriceListTypeId { get; set; }
        public int CompanyWholesellerPriceListId { get; set; }
        public int? RuleCompanyWholesellerPriceListId { get; set; }

        public string SysWholesellerName { get; set; }

        public DateTime Date { get; set; }

        public string PriceListTypeName { get; set; }
        public string PriceListTypeDescription { get; set; }

        public int PriceListImportedHeadId { get; set; }
    }

    #endregion

    #region Product

    [TSInclude]
    public class InvoiceProductSearchViewDTO
    {
        public string Number { get; set; }
        public PriceListOrigin PriceListOrigin { get; set; }
        public List<int> ProductIds { get; set; }
        public string Name { get; set; }
        public string ExtendedInfo { get; set; }
        public string Manufacturer { get; set; }
        public int? ExternalId { get; set; }
        public string ImageUrl { get; set; }
        public SoeSysPriceListProviderType Type { get; set; }
        public string ExternalUrl { get; set; }
        public DateTime? EndAt { get; set; }
        public string EndAtTooltip { get; set; }
        public string EndAtIcon { get; set; }
    }

    [TSInclude]
    [DebuggerDisplay("Name = {Name}")]
    public class InvoiceProductPriceSearchViewDTO
    {
        public int ProductId { get; set; }
        public int? CompanyWholesellerPriceListId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public decimal GNP { get; set; }
        public decimal? NettoNettoPrice { get; set; }
        public decimal? CustomerPrice { get; set; }
        public decimal? SalesPrice { get; set; }
        public int PriceStatus { get; set; }

        public int SysPriceListHeadId { get; set; }
        public int SysWholesellerId { get; set; }
        public string Wholeseller { get; set; }

        public int WholsellerNetPriceId { get; set; }

        public string PriceListType { get; set; }
        public int PriceListOrigin { get; set; }

        public string PurchaseUnit { get; set; }
        public string SalesUnit { get; set; }

        public int Type { get; set; }
        public int ProductType { get; set; }

        public SoeSysPriceListProviderType ProductProviderType { get; set; }

        // Extensions
        public string PriceFormula { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeRatio { get; set; }
    }

    [DebuggerDisplay("ProductNr:{ProductNr}, ProductName:{ProductName}")]
    public class InvoiceProductPriceSearchDTO
    {
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductUnitCode { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public string SysWholesellerName { get; set; }
        public int SysWholesellerId { get; set; }
        public int SysPriceListHeadId { get; set; }
        public bool External { get; set; }
        public string ExternalUrl { get; set; }
        public int ExternalId { get; set; }
        public string ExtendedInfo { get; set; }
        public string ImageFileName { get; set; }
    }

    public class InvoiceProductPriceSearchExDTO : InvoiceProductPriceSearchDTO
    {
        public int? ExternalProductId { get; set; }
        public int NumberIndexOf { get; set; }
        public int NameIndexOf { get; set; }
    }

    public class InvoiceProductSearchResultDTO
    {
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductUnitCode { get; set; }

        public string Description { get; set; }
        public string EAN { get; set; }
        public bool External { get; set; }
        public int VatType { get; set; }
        public string VatCodeNr { get; set; }
        public string ProductGroupCode { get; set; }
        public int State { get; set; }
        public bool DontUseDiscountPercent { get; set; }
        public bool IsStockProduct { get; set; }
    }

    #endregion

    #region Project

    public class ProjectInvoiceAccountDTO
    {
        public string AccountTypeName { get; set; }
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }

        public int Dim1Id { get; set; }
        public string Dim1Nr { get; set; }
        public string Dim1Name { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
    }

    [TSInclude]
    public class ProjectTimeMatrixSaveDTO
    {
        public int EmployeeId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int ProjectId { get; set; }
        public int CustomerInvoiceId { get; set; }
        public int TimeCodeId { get; set; }
        public DateTime WeekDate { get; set; }
        public int ProjectInvoiceWeekId { get; set; }
        public bool IsDeleted { get; set; }
        public List<ProjectTimeMatrixSaveRowDTO> Rows { get; set; }
    }

    [TSInclude]
    public class ProjectTimeMatrixSaveRowDTO
    {
        public int ProjectTimeBlockId { get; set; }
        public int WeekDay { get; set; }
        public decimal PayrollQuantity { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public string InternalNote { get; set; }
        public string ExternalNote { get; set; }
        public bool IsPayrollEditable { get; set; }
        public bool IsInvoiceEditable { get; set; }
        public int EmployeeChildId { get; set; }
        public string PayrollStateColor { get; set; }
        public string InvoiceStateColor { get; set; }
    }

    [TSInclude]
    public class ProjectTimeMatrixDTO
    {

        public int EmployeeId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }

        //Project
        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public int CustomerInvoiceId { get; set; }
        public string InvoiceNr { get; set; }

        //TimeCodeTransaction
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }

        public int ProjectInvoiceWeekId { get; set; }
        public List<ProjectTimeMatrixSaveRowDTO> Rows { get; set; }
    }

    [TSInclude]
    public class ProjectTimeBlockDTO
    {
        public int ProjectTimeBlockId { get; set; }
        public int TimeSheetWeekId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public int EmployeeChildId { get; set; }
        public string EmployeeChildName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNr { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime Date { get; set; }
        public string YearMonth { get; set; }
        public string YearWeek { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Week { get; set; }
        public string WeekDay { get; set; }
        public bool HasComment { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public string InternalNote { get; set; }
        public string ExternalNote { get; set; }
        public bool EmployeeIsInactive { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        //Extensions
        public bool ShowInvoiceRowAttestState { get; set; }
        public bool ShowPayrollAttestState { get; set; }
        public bool IsEditable { get; set; }
        public bool IsPayrollEditable { get; set; }
        public bool OrderClosed { get; set; }

        //Project
        public int ProjectId { get; set; }
        public int ProjectInvoiceWeekId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public TermGroup_ProjectAllocationType AllocationType { get; set; }

        //CustomerInvoice
        public OrderInvoiceRegistrationType RegistrationType { get; set; }
        public int CustomerInvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string ReferenceOur { get; set; }
        public string InternOrderText { get; set; }

        //TimeCodeTransaction
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }

        //TimePayrollTransaction
        public List<int> TimePayrollTransactionIds { get; set; }
        public decimal TimePayrollQuantity { get; set; }
        public int TimePayrollAttestStateId { get; set; }
        public string TimePayrollAttestStateName { get; set; }
        public string TimePayrollAttestStateColor { get; set; }

        public bool AdditionalTime { get; set; }

        public bool IsSalaryPayrollType { get; set; }
        public decimal ScheduledQuantity { get; set; }

        public bool MandatoryTime { get; set; }

        //TimeInvoiceTransactions
        public int TimeInvoiceTransactionId { get; set; }

        //CustomerInvoiceRow
        public int CustomerInvoiceRowAttestStateId { get; set; }
        public string CustomerInvoiceRowAttestStateName { get; set; }
        public string CustomerInvoiceRowAttestStateColor { get; set; }
    }

    [TSInclude]
    public class ValidateProjectTimeBlockSaveDTO
    {
        public int EmployeeId { get; set; }
        public bool AutoGenTimeAndBreakForProject { get; set; }
        public List<ValidateProjectTimeBlockSaveRowDTO> Rows { get; set; }
    }
    [TSInclude]
    public class ValidateProjectTimeBlockSaveRowDTO
    {
        public int Id { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public DateTime WorkDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime? OriginalStartTime { get; set; }
        public DateTime? OriginalStopTime { get; set; }
        public int? OriginalTimeDeviationCauseId { get; set; }
        public int? EmployeeChildId { get; set; }
    }

    [TSInclude]
    public class ProjectTimeBlockSaveDTO
    {
        public int ProjectTimeBlockId { get; set; }
        public bool isFromTimeSheet { get; set; }
        public int? TimeBlockDateId { get; set; }
        public DateTime? Date { get; set; }
        public int? TimeSheetWeekId { get; set; }
        public int? ProjectInvoiceWeekId { get; set; }
        public int? ProjectInvoiceDayId { get; set; }
        public int? TimeDeviationCauseId { get; set; }

        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? ProjectId { get; set; }
        public int TimeCodeId { get; set; }
        public int? CustomerInvoiceId { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public decimal TimePayrollQuantity { get; set; }
        public string InternalNote { get; set; }
        public string ExternalNote { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool AutoGenTimeAndBreakForProject { get; set; }
        public int? EmployeeChildId { get; set; }

        public bool MandatoryTime { get; set; }
        public bool AdditionalTime { get; set; }
    }

    [TSInclude]
    public class EmployeeScheduleTransactionInfoDTO
    {
        public int EmployeeId { get; set; }
        public int EmployeeGroupId { get; set; }
        public DateTime Date { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public bool AutoGenTimeAndBreakForProject { get; set; }
        public List<ProjectTimeBlockDTO> TimeBlocks { get; set; }
        public List<ProjectTimeBlockDTO> ScheduleBlocks { get; set; }
    }

    #endregion

    #region ReportView
    [TSInclude]
    public class ReportViewDTO
    {
        public int ActorCompanyId { get; set; }
        public int ReportId { set; get; }
        public int ExportType { get; set; }
        public string ReportName { set; get; }
        public int ReportNr { set; get; }
        public int? ReportSelectionId { set; get; }
        public string ReportDescription { set; get; }
        public int SysReportTemplateTypeId { set; get; }
        public bool ShowInAccountingReports { get; set; }

        public string SysReportTypeName { get; set; }
        public bool IsSystemReport { get; set; }

        public string ReportNameDesc
        {
            get
            {
                return this.ReportName + (string.IsNullOrEmpty(this.ReportDescription) ? string.Empty : " (" + this.ReportDescription + ")");
            }
        }
    }

    public class ReportViewGridDTO : ReportViewDTO
    {
        public bool Standard { get; set; }
        public string ReportSelectionText { set; get; }
        public string RoleNames { set; get; }
        public string ExportTypeName { set; get; }
        public bool Original { set; get; }
        public SoeSelectionType SelectionType { set; get; }
        public bool IsMigrated { get; set; }
    }

    #endregion

    #region Statistics

    public class EntryWithAttachedStuff
    {
        public int Id { get; set; }
        public bool Linked { get; set; }
        public IEnumerable<int> DataStorageRecordIds { get; set; }
    }

    #region Contract
    public class StatisticsContractDTO : EntryWithAttachedStuff
    {
        private DateTime? _startdate;
        private DateTime? _enddate;
        public int InvoiceId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractCategory { get; set; }
        public string ContractType { get; set; }
        public string CustomerCategory { get; set; }
        public string CustomerName { get; set; }
        public string InternalText { get; set; }
        public DateTime? StartDate
        {
            get { return this._startdate; }

            set
            {
                if (value.HasValue)
                {
                    this._startdate = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._startdate = value;
                }
            }
        }
        public DateTime? EndDate
        {
            get { return this._enddate; }

            set
            {
                if (value.HasValue)
                {
                    this._enddate = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._enddate = value;
                }
            }
        }

        public string NextPeriod { get; set; }

        public decimal Amount { get; set; }

        public decimal Cost { get; set; }

        public string CategoryType { get; set; }

        public decimal? MarginalIncome { get; set; }

        public IEnumerable<StatisticsContractRowDTO> Rows { get; set; }

        public string StatusName { get; set; }

    }

    public class StatisticsContractRowDTO
    {
        public int RowNr { get; set; }
    }

    #endregion

    #region Order

    public class StatisticsOrderDTO : EntryWithAttachedStuff
    {
        private DateTime? _date;
        private DateTime? _duedate;
        private DateTime? _deliverydate;
        // Add properties and fields
        public int ActorCompanyId { get; set; }
        public string ArticleDescription { get; set; }
        //public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public string InternalText { get; set; }
        public DateTime? Date
        {
            get { return this._date; }

            set
            {
                if (value.HasValue)
                {
                    this._date = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._date = value;
                }
            }
        }
        public string Number { get; set; }
        public int? SeqNr { get; set; }
        public string CustomerNr { get; set; }
        public string Customer { get; set; }
        public decimal Amount { get; set; }
        public decimal InvoicePaidAmount { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal VATAmount { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }

        public string OrderNr { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? DueDate
        {
            get { return this._duedate; }

            set
            {
                if (value.HasValue)
                {
                    this._duedate = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._duedate = value;
                }
            }
        }

        public DateTime? DeliveryDate
        {
            get { return this._deliverydate; }

            set
            {
                if (value.HasValue)
                {
                    this._deliverydate = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._deliverydate = value;
                }
            }
        }
        //public bool Linked { get; set; }

        //public IEnumerable<int> DataStorageRecordIds { get; set; }

        public IEnumerable<StatisticsOrderRowDTO> Rows { get; set; }

        public string CurrencyCode { get; set; }

        public string UserName { get; set; }
        public decimal? MarginalIncome { get; set; }

        public decimal? MarginalIncomeRatio { get; set; }

        public string Dim2Name { get; set; }
        public string Dim3Name { get; set; }
        public string Dim4Name { get; set; }
        public string Dim5Name { get; set; }
        public string Dim6Name { get; set; }

    }

    public class StatisticsOrderRowDTO
    {
        // Add properties and fields
        public int RowNr { get; set; }

        public string Text { get; set; }
        public decimal Amount { get; set; }
        public decimal? Quantity { get; set; }
        public decimal SumAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int Id { get; set; }

        public string Number { get; set; }
    }

    #endregion

    #region SupplierInvoice

    public class StatisticsSupplierInvoiceDTO : EntryWithAttachedStuff
    {
        private DateTime? _invoiceDate;
        private DateTime? _dueDate;
        // Add properties and fields
        public int ActorCompanyId { get; set; }
        //public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProjectId { get; set; }
        public string InternalText { get; set; }

        public DateTime? InvoiceDate
        {
            get { return this._invoiceDate; }

            set
            {
                if (value.HasValue)
                {
                    this._invoiceDate = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._invoiceDate = value;
                }
            }
        }

        public DateTime? DueDate
        {
            get { return this._dueDate; }

            set
            {
                if (value.HasValue)
                {
                    this._dueDate = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                }
                else
                {
                    this._dueDate = value;
                }
            }
        }

        public string Number { get; set; }
        public int? SeqNr { get; set; }
        public string SupplierNr { get; set; }
        public string Supplier { get; set; }
        public decimal Amount { get; set; }
        public decimal InvoicePaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal VATAmount { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int BillingTypeId { get; set; }
        public string BillingTypeName { get; set; }

        public IEnumerable<StatisticsSupplierInvoiceRowDTO> Rows { get; set; }
    }

    public class StatisticsSupplierInvoiceRowDTO
    {
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? Quantity { get; set; }
        public bool DebitRow { get; set; }
        public bool CreditRow { get; set; }
        public bool InterimRow { get; set; }
        public bool ContractorVatRow { get; set; }

        public string AccountDim1Nr { get; set; }
        public string AccountDim1Name { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim2Name { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim3Name { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim4Name { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim5Name { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountDim6Name { get; set; }
    }


    public class StatisticsTimeProjectEmployeeDTO : EntryWithAttachedStuff
    {
        private DateTime _date;
        //public int Id { get; set; }
        public DateTime Date
        {
            get { return this._date; }
            set { this._date = new DateTime(value.Ticks, DateTimeKind.Utc); }
        }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int SumWorkTimeInMinutes { get; set; }
        public int SumInvoiceTimeInMinutes { get; set; }

        public List<StatisticsTimeProjectEmployeeRowDTO> Rows { get; set; }
    }


    public class StatisticsTimeProjectEmployeeRowDTO
    {
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public int OrderId { get; set; }
        public string OrderNr { get; set; }
        public int WorkTimeInMinutes { get; set; }
        public int InvoiceTimeInMinutes { get; set; }

    }

    #endregion

    #region Voucher

    public class StatisticsVoucherDTO : EntryWithAttachedStuff
    {
        private DateTime _date;
        public int ActorCompanyId { get; set; }
        public int VoucherHeadId { get; set; }
        public int VoucherNr { get; set; }
        public DateTime Date
        {
            get { return this._date; }
            set { this._date = new DateTime(value.Ticks, DateTimeKind.Utc); }
        }
        public string Text { get; set; }
        public string Note { get; set; }

        public bool Template { get; set; }
        public bool TypeBalance { get; set; }
        public bool? VatVoucher { get; set; }

        public string VoucherSeriesTypeName { get; set; }
        public string AccountPeriodName { get; set; }
        public TermGroup_AccountStatus Status { get; set; }
        public string StatusName { get; set; }

        public List<StatisticsVoucherRowDTO> Rows { get; set; }
    }

    public class StatisticsVoucherRowDTO
    {
        public int VoucherHeadId { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public decimal? Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public DateTime? Date { get; set; }
        public bool Merged { get; set; }

        public string AccountDim1Nr { get; set; }
        public string AccountDim1Name { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim2Name { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim3Name { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim4Name { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim5Name { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountDim6Name { get; set; }
    }

    #endregion

    public class ExportReportSettingsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Data { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsOwner { get; set; }
    }

    #endregion

    #region TimeStampAttendanceView

    public class TimeStampAttendanceViewDTO
    {
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public DateTime Time { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public TimeStampEntryType Type { get; set; }
        public string TimeTerminalName { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string AccountName { get; set; }

        // Extensions
        public string TypeName { get; set; }
        public string Name { get; set; }
    }

    public class TimeStampAttendanceGaugeDTO
    {
        public int EmployeeId { get; set; }
        public DateTime Time { get; set; }
        public TimeStampEntryType Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string TimeTerminalName { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string AccountName { get; set; }
        public string EmployeeNr { get; set; }
        public bool IsBreak { get; set; }
        public bool IsPaidBreak { get; set; }
        public bool IsDistanceWork { get; set; }
        public DateTime? ScheduleStartTime { get; set; }
        public bool IsMissing { get; set; }
        public string TimeStr
        {
            get
            {
                if (this.IsMissing)
                    return ScheduleStartTime.Value.ToShortTimeString();
                else
                    return this.Time.ToShortTimeString();
            }
        }
    }

    #endregion

    #region TraceViews

    #region AccountDistribution
    [TSInclude]
    public class AccountDistributionTraceViewDTO
    {
        public int AccountDistributionHeadId { get; set; }
        public bool IsVoucher { get; set; }
        public int VoucherHeadId { get; set; }
        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public int LangId { get; set; }
        public SoeEntityState State { get; set; }

    }

    #endregion

    #region Base
    [TSInclude]
    public abstract class EDITraceViewBase
    {
        public EDITraceViewBase() { }

        public bool IsEdi { get; set; }
        public int EdiEntryId { get; set; }
        public bool EdiHasPdf { get; set; }
    }

    #endregion

    #region Contract
    [TSInclude]
    public class ContractTraceViewDTO
    {
        public int LangId { get; set; }

        public int ContractId { get; set; }

        public bool IsOrder { get; set; }
        public int OrderId { get; set; }

        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }

        public bool IsProject { get; set; }
        public int ProjectId { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public TermGroup_BillingType BillingType { get; set; }
        public string BillingTypeName { get; set; }
        public string Number { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }

    #endregion

    #region Inventory

    [TSInclude]
    public class InventoryTraceViewDTO
    {
        public int LangId { get; set; }

        public int InventoryId { get; set; }
        public int InventoryLogId { get; set; }

        public int? VoucherHeadId { get; set; }
        public string VoucherNr { get; set; }

        public int? InvoiceId { get; set; }
        public string InvoiceNr { get; set; }

        public int? AccountDistributionEntryId { get; set; }

        public TermGroup_InventoryLogType Type { get; set; }
        public string TypeName { get; set; }

        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion

    #region Invoice

    [TSInclude]
    public class InvoiceTraceViewDTO : EDITraceViewBase
    {
        public int LangId { get; set; }

        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }

        public bool IsContract { get; set; }
        public int ContractId { get; set; }

        public bool IsOffer { get; set; }
        public int OfferId { get; set; }

        public bool IsOrder { get; set; }
        public int OrderId { get; set; }

        public int MappedInvoiceId { get; set; }

        public bool IsReminderInvoice { get; set; }
        public int ReminderInvoiceId { get; set; }

        public bool IsInterestInvoice { get; set; }
        public int InterestInvoiceId { get; set; }

        public bool IsPayment { get; set; }
        public int PaymentRowId { get; set; }
        public SoePaymentStatus PaymentStatusId { get; set; }
        public string PaymentStatusName { get; set; }


        public bool IsStockVoucher { get; set; }
        public bool IsVoucher { get; set; }
        public int VoucherHeadId { get; set; }

        public bool IsInventory { get; set; }
        public int InventoryId { get; set; }
        public string InventoryName { get; set; }
        public string InventoryDescription { get; set; }
        public string InventoryTypeName { get; set; }
        public TermGroup_InventoryStatus InventoryStatusId { get; set; }
        public string InventoryStatusName { get; set; }

        public bool IsAccountDistribution { get; set; }
        public int AccountDistributionHeadId { get; set; }
        public string AccountDistributionName { get; set; }
        public int TriggerType { get; set; }
        public string TriggerTypeName { get; set; }

        public bool IsProject { get; set; }
        public int ProjectId { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public TermGroup_BillingType BillingType { get; set; }
        public string BillingTypeName { get; set; }
        public string Number { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public bool IsScanning { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }

    #endregion

    #region Offer
    [TSInclude]
    public class OfferTraceViewDTO
    {
        public int LangId { get; set; }

        public int OfferId { get; set; }

        public bool IsOrder { get; set; }
        public int OrderId { get; set; }

        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }

        public bool IsProject { get; set; }
        public int ProjectId { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public TermGroup_BillingType BillingType { get; set; }
        public string BillingTypeName { get; set; }
        public string Number { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }

    #endregion

    #region Order
    [TSInclude]
    public class OrderTraceViewDTO : EDITraceViewBase
    {
        public int LangId { get; set; }

        public int OrderId { get; set; }

        public bool IsContract { get; set; }
        public int ContractId { get; set; }

        public bool IsOffer { get; set; }
        public int OfferId { get; set; }

        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }

        public bool IsProject { get; set; }
        public int ProjectId { get; set; }

        public bool IsSupplierInvoice { get; set; }
        public int SupplierInvoiceId { get; set; }

        public bool IsPurchase { get; set; }
        public int PurchaseId { get; set; }

        public bool IsStockVoucher { get; set; }
        public int VoucherHeadId { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public TermGroup_BillingType BillingType { get; set; }
        public string BillingTypeName { get; set; }
        public string Number { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }

    #endregion

    #region Payment
    [TSInclude]
    public class PaymentTraceViewDTO
    {
        public int LangId { get; set; }

        public int PaymentRowId { get; set; }

        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }

        public bool IsVoucher { get; set; }
        public int VoucherHeadId { get; set; }

        public bool IsProject { get; set; }
        public int ProjectId { get; set; }
        public bool IsImport { get; set; }
        public int PaymentImportId { get; set; }
        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public string Number { get; set; }
        public int SequenceNumber { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }
        public OrderInvoiceRegistrationType RegistrationType { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }
    [TSInclude]
    public class PaymentImportIODTO
    {
        public int PaymentImportIOId { get; set; }
        public int ActorCompanyId { get; set; }
        public int BatchNr { get; set; }
        public TermGroup_BillingType Type { get; set; }
        public int? CustomerId { get; set; }
        public string Customer { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? RestAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public string Currency { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? MatchCodeId { get; set; }
        public ImportPaymentIOStatus Status { get; set; }
        public ImportPaymentIOState State { get; set; }
        public string InvoiceSeqnr { get; set; }
        public decimal? PaidAmountCurrency { get; set; }
        public ImportPaymentType? ImportType { get; set; }
        public int? PaymentRowId { get; set; }
        public int? PaymentRowSeqNr { get; set; }
        public string OCR { get; set; }
        public string Comment { get; set; }


        /* Extension */
        public bool IsSelected { get; set; }
        public bool IsFullyPaid { get; set; }
        public bool IsVisible { get; set; }
        public decimal AmountDiff { get; set; }
        public string TypeName { get; set; }
        public string StatusName { get; set; }
        public string StateName { get; set; }
        public string PaymentTypeName { get; set; }
        public string MatchCodeName { get; set; }
        public int TempRowId { get; set; }

    }

    #endregion

    #region Project
    [TSInclude]
    public class ProjectTraceViewDTO
    {
        public int LangId { get; set; }

        public int ProjectId { get; set; }

        public bool IsContract { get; set; }
        public int ContractId { get; set; }

        public bool IsOffer { get; set; }
        public int OfferId { get; set; }

        public bool IsOrder { get; set; }
        public int OrderId { get; set; }

        public bool IsCustomerInvoice { get; set; }
        public int CustomerInvoiceId { get; set; }

        public bool IsSupplierInvoice { get; set; }
        public int SupplierInvoiceId { get; set; }

        public bool IsPayment { get; set; }
        public int PaymentRowId { get; set; }
        public SoePaymentStatus PaymentStatusId { get; set; }
        public string PaymentStatusName { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public TermGroup_BillingType BillingType { get; set; }
        public string BillingTypeName { get; set; }
        public string Number { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }
    }

    #endregion

    #region Purchase

    [TSInclude]
    public class PurchaseTraceViewDTO
    {
        public int LangId { get; set; }

        public int PurchaseId { get; set; }

        public bool IsOrder { get; set; }
        public int OrderId { get; set; }

        public bool IsDelivery { get; set; }
        public int PurchaseDeliveryId { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public int Number { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }

    #endregion

    #region Price Optimization

    [TSInclude]
    public class PriceOptimizationTraceDTO
    {
        public int Number { get; set; }
        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public string OriginStatusName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }
    }

    #endregion

    #region Voucher
    [TSInclude]
    public class VoucherTraceViewDTO
    {
        public int LangId { get; set; }

        public int VoucherHeadId { get; set; }

        public bool IsInvoice { get; set; }
        public int InvoiceId { get; set; }

        public bool IsPayment { get; set; }
        public int PaymentRowId { get; set; }
        public SoePaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusName { get; set; }

        public bool IsInventory { get; set; }
        public int InventoryId { get; set; }
        public string InventoryName { get; set; }
        public string InventoryDescription { get; set; }
        public string InventoryTypeName { get; set; }
        public TermGroup_InventoryStatus InventoryStatusId { get; set; }
        public string InventoryStatusName { get; set; }

        public bool IsAccountDistribution { get; set; }
        public int AccountDistributionHeadId { get; set; }
        public string AccountDistributionName { get; set; }

        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginStatusName { get; set; }
        public string Description { get; set; }

        public string Number { get; set; }

        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }

        public DateTime? Date { get; set; }

        public SoeEntityState State { get; set; }
        public OrderInvoiceRegistrationType RegistrationType { get; set; }

        // Extensions
        public bool Foreign { get; set; }
    }

    #endregion

    #endregion

    #region Wholeseller

    public class WholesellerDTO
    {
        public int SysWholesellerId { get; set; }
        public string Name { get; set; }
    }

    public class CompanyWholesellerListDTO : WholesellerDTO
    {
        public int CompanySysWholesellerDtoId { get; set; }
        public bool Active { get; set; }
    }

    public class CompanyWholesellerDTO : CompanyWholesellerListDTO
    {
        public bool UseEdi { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public int SysWholesellerEdiId { get; set; }
        public SysWholesellerDTO Wholeseller { get; set; }
        public IEnumerable<EdiConnectionDTO> EdiConnections { get; set; }

        public string MessageTypes { get; set; }

        public bool HasEdiFeature { get; set; }

        public string EdiWholesellerSenderNrs { get; set; }
    }

    public class EdiConnectionDTO
    {
        public int EdiConnectionId { get; set; }
        public string WholesellerCustomerNr { get; set; }
    }

    #endregion

    #endregion

    #region Stored Procedures

    #region CustomerInvoiceSearch

    public class CustomerInvoiceSearchResultDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
    }

    #endregion

    #region Product statistics

    public class StatisticsMostSoldProductsByAmountResultDTO
    {
        public int ProductId { get; set; }
        [ExcelExportAttribute(titleText: "Nummer")]
        public string Number { get; set; }
        [ExcelExportAttribute(titleText: "Namn")]
        public string Name { get; set; }
        [ExcelExportAttribute(titleText: "Summa")]
        public decimal? SumAmount { get; set; }
        [ExcelExportAttribute(titleText: "Inköpspris(summa)")]
        public decimal? SumPurchasePrice { get; set; }
        [ExcelExportAttribute(titleText: "Täckningsbidrag")]
        public decimal ContributionMargin
        {
            get
            {
                if (!this.SumAmount.HasValue || !this.SumPurchasePrice.HasValue)
                    return 0;

                return this.SumAmount.Value - this.SumPurchasePrice.Value;
            }
        }
        [ExcelExportAttribute(titleText: "Täckningsbidrag (%)")]
        public decimal? ContributionMarginPercent
        {
            get
            {
                if (!this.SumPurchasePrice.HasValue || !this.SumAmount.HasValue || this.SumAmount == 0)
                    return null;

                return Math.Round(((1 - (this.SumPurchasePrice.Value / this.SumAmount.Value)) * 100), 2);
            }
        }

        public override string ToString()
        {
            // Needed for the tooltip
            return this.Name;
        }
    }

    public class StatisticsMostSoldProductsByQuantityResultDTO
    {
        public int ProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }

        public override string ToString()
        {
            // Needed for the tooltip
            return this.Name;
        }
    }

    #endregion

    #endregion
}