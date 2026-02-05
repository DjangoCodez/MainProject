using System;
using System.ServiceModel;

namespace SoftOne.Soe.Common.Interfaces
{
    [ServiceContract(Name = "IVoucherChannel")]
    public interface IVoucherChannelAsync
    {
        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginHeartbeat(AsyncCallback callback, object asyncState);

        bool EndHeartbeat(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccounts(int accountDimId, int actorCompanyId, bool loadAccountDim, bool loadInternalAccounts, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDTO> EndGetAccounts(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountsDict(int accountDimId, int actorCompanyId, bool loadAccountDim, bool loadInternalAccounts, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetAccountsDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDimStds(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDTO> EndGetAccountDimStds(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountGridView(int accountDimId, int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountGridViewDTO> EndGetAccountGridView(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountTypeSysTerms(bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetAccountTypeSysTerms(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginChangeAccountStates(System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountGridViewDTO> accounts, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndChangeAccountStates(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveAccount(string accountNr, string name, int accountTypeId, int vatAccountId, int sruCode1Id, int actorCompanyId, int userId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountDTO EndSaveAccount(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteAccounts(System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountGridViewDTO> accounts, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteAccounts(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountHistory(int accountId, int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountHistoryDTO> EndGetAccountHistory(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountVatRates(int accountDimId, int actorCompanyId, bool addVatFreeRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountVatRateViewDTO> EndGetAccountVatRates(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetSysAccountStd(int sysAccountStdTypeId, string accountNr, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.SysAccountStdDTO EndGetSysAccountStd(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginCopyAccountStdFromSys(int actorCompanyId, SoftOne.Soe.Common.DTO.SysAccountStdDTO sysAccountStd, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountDTO EndCopyAccountStdFromSys(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetSysAccountSruCodesDict(bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetSysAccountSruCodesDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetSysVatRateValue(int accountId, AsyncCallback callback, object asyncState);

        decimal EndGetSysVatRateValue(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountBalance(int actorCompanyId, int accountId, int accountYearId, AsyncCallback callback, object asyncState);

        decimal EndGetAccountBalance(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountBalances(int accountYearId, int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, decimal> EndGetAccountBalances(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginCalculateAccountBalanceForAccountsFromVoucher(int actorCompanyId, int accountYearId, int savedMinutesBack, AsyncCallback callback, object asyncState);

        void EndCalculateAccountBalanceForAccountsFromVoucher(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDim(int accountDimId, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountDimDTO EndGetAccountDim(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDimStdId(int actorCompanyId, AsyncCallback callback, object asyncState);

        int EndGetAccountDimStdId(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDims(int actorCompanyId, bool onlyStandard, bool onlyInternal, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDimDTO> EndGetAccountDims(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDimsDict(int actorCompanyId, bool onlyAccountDimStd, bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetAccountDimsDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInternalAccountDims(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDimDTO> EndGetInternalAccountDims(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteAccountDims(System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountDimDTO> accountDims, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteAccountDims(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDistributionHead(int accountDistributionHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountDistributionHeadDTO EndGetAccountDistributionHead(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDistributionHeads(int actorCompanyId, SoftOne.Soe.Common.Util.SoeAccountDistributionType type, bool loadRows, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDistributionHeadDTO> EndGetAccountDistributionHeads(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDistributionHeadsUsedIn(int actorCompanyId, SoftOne.Soe.Common.Util.SoeAccountDistributionType? type, System.DateTime? date, bool? useInVoucher, bool? useInSupplierInvoice, bool? useInCustomerInvoice, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDistributionHeadSmallDTO> EndGetAccountDistributionHeadsUsedIn(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDistributionRows(int accountDistributionHeadId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDistributionRowDTO> EndGetAccountDistributionRows(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveAccountDistribution(SoftOne.Soe.Common.DTO.AccountDistributionHeadDTO distributionHeadDTOInput, System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountDistributionRowDTO> distributionRowsDTOInput, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveAccountDistribution(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteAccountDistribution(int accountDistributionHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteAccountDistribution(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginTransferToAccountDistributionEntry(int actorCompanyId, System.DateTime periodDate, SoftOne.Soe.Common.Util.SoeAccountDistributionType accountDistributionType, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndTransferToAccountDistributionEntry(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountDistributionEntries(int actorCompanyId, System.DateTime periodDate, SoftOne.Soe.Common.Util.SoeAccountDistributionType accountDistributionType, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountDistributionEntryDTO> EndGetAccountDistributionEntries(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginTransferAccountDistributionEntryToVoucher(System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountDistributionEntryDTO> entriesDTO, int actorCompanyId, SoftOne.Soe.Common.Util.SoeAccountDistributionType accountDistributionType, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndTransferAccountDistributionEntryToVoucher(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteAccountDistributionEntries(System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountDistributionEntryDTO> entriesDTO, int actorCompanyId, SoftOne.Soe.Common.Util.SoeAccountDistributionType accountDistributionType, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteAccountDistributionEntries(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginRestoreAccountDistributionEntries(SoftOne.Soe.Common.DTO.AccountDistributionEntryDTO entryDTO, int actorCompanyId, SoftOne.Soe.Common.Util.SoeAccountDistributionType accountDistributionType, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndRestoreAccountDistributionEntries(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteAccountDistributionEntriesPermanently(SoftOne.Soe.Common.DTO.AccountDistributionEntryDTO entryDTO, int actorCompanyId, SoftOne.Soe.Common.Util.SoeAccountDistributionType accountDistributionType, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteAccountDistributionEntriesPermanently(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountPeriod(int accountYearId, int actorCompanyId, System.DateTime date, bool includeAccountYear, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountPeriodDTO EndGetAccountPeriod(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountPeriodId(int accountYearId, int actorCompanyId, System.DateTime date, AsyncCallback callback, object asyncState);

        int EndGetAccountPeriodId(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountPeriodDict(int accountYearId, bool addEmptyItem, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetAccountPeriodDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginUpdateAccountPeriodStatus(int accountPeriodId, SoftOne.Soe.Common.Util.TermGroup_AccountStatus status, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndUpdateAccountPeriodStatus(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountYear(System.DateTime date, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountYearDTO EndGetAccountYear(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountYearById(int accountYearId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.AccountYearDTO EndGetAccountYearById(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountYearId(System.DateTime date, int actorCompanyId, AsyncCallback callback, object asyncState);

        int EndGetAccountYearId(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountYearsDict(int actorCompanyId, bool onlyOpen, bool excludeNew, bool includeStatusText, bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetAccountYearsDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetAccountYears(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.AccountYearDTO> EndGetAccountYears(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginIsDateWithinCurrentAccountYear(int actorCompanyId, System.DateTime date, AsyncCallback callback, object asyncState);

        bool EndIsDateWithinCurrentAccountYear(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetProcessInfo(System.Guid key, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.SoeProgressInfoSmall EndGetProcessInfo(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetResult(System.Guid key, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.BudgetBalanceDTO> EndGetResult(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetBudgetHeads(int actorCompanyId, int budgetType, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.BudgetHeadDTO> EndGetBudgetHeads(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetBudgetHeadsDict(int actorCompanyId, int budgetType, bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetBudgetHeadsDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetBudgetHead(int budgetHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.BudgetHeadDTO EndGetBudgetHead(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveBudget(SoftOne.Soe.Common.DTO.BudgetHeadDTO dto, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveBudget(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteBudget(int budgetHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteBudget(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetBalanceChangePerYear(int accountYearId, int accountId, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.BalanceItemDTO EndGetBalanceChangePerYear(IAsyncResult result);


        [OperationContract(
            IsOneWay = true,
            AsyncPattern = true)]
        IAsyncResult BeginGetBalanceChangesPerPeriod(System.Guid key, int noOfPeriods, int accountYearId, int accountId, int actorCompanyId, System.DateTime today, bool getPrevious, System.Collections.Generic.List<int> dims, AsyncCallback callback, object asyncState);

        void EndGetBalanceChangesPerPeriod(IAsyncResult result);


        [OperationContract(
            IsOneWay = true,
            AsyncPattern = true)]
        IAsyncResult BeginGetBalanceChangesPerPeriodForAccounts(System.Guid key, System.Collections.Generic.Dictionary<int, bool> accounts, System.Collections.Generic.List<int> internalAccounts, int noOfPeriods, int accountYearId, int actorCompanyId, System.DateTime today, System.Collections.Generic.List<int> dims, AsyncCallback callback, object asyncState);

        void EndGetBalanceChangesPerPeriodForAccounts(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveDistributionCode(int actorCompanyId, SoftOne.Soe.Common.DTO.DistributionCodeHeadDTO dto, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveDistributionCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteDistributionCode(int actorCompanyId, int distributionCodeHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteDistributionCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetDistributionCode(int actorCompanyId, int distributionCodeHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.DistributionCodeHeadDTO EndGetDistributionCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetDistributionCodes(int actorCompanyId, bool includePeriods, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.DistributionCodeHeadDTO> EndGetDistributionCodes(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveGrossProfitCode(int actorCompanyId, SoftOne.Soe.Common.DTO.GrossProfitCodeDTO dto, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveGrossProfitCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteGrossProfitCode(int actorCompanyId, int grossProfitCodeId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteGrossProfitCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetGrossProfitCode(int actorCompanyId, int grossProfitCodeId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.GrossProfitCodeDTO EndGetGrossProfitCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetGrossProfitCodes(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.GrossProfitCodeDTO> EndGetGrossProfitCodes(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInventory(int inventoryId, bool loadAccountSettings, bool loadLogs, bool loadSupplierInvoiceInfo, bool loadCustomerInvoiceInfo, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.InventoryDTO EndGetInventory(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInventories(int actorCompanyId, bool loadInventoryAccount, int userId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.InventoryGridDTO> EndGetInventories(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSearchInventories(int actorCompanyId, string number, string name, string description, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.InventorySearchResultDTO> EndSearchInventories(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetNextInventoryNr(int actorCompanyId, AsyncCallback callback, object asyncState);

        string EndGetNextInventoryNr(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveInventory(SoftOne.Soe.Common.DTO.InventoryDTO inventoryInput, System.Collections.Generic.List<SoftOne.Soe.Common.DTO.CompanyCategoryRecordDTO> categoryRecords, System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountingSettingDTO> accountSettings, int debtAccountId, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveInventory(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteInventory(int inventoryId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteInventory(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveAdjustment(int inventoryId, SoftOne.Soe.Common.Util.TermGroup_InventoryLogType type, decimal amount, System.DateTime date, System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountingRowDTO> accountingRowDTOsInput, int? voucherHeadId, int? invoiceId, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveAdjustment(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInventoryWriteOffMethods(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.InventoryWriteOffMethodDTO> EndGetInventoryWriteOffMethods(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInventoryWriteOffMethod(int methodId, bool loadInventories, bool loadTemplates, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.InventoryWriteOffMethodDTO EndGetInventoryWriteOffMethod(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveInventoryWriteOffMethod(SoftOne.Soe.Common.DTO.InventoryWriteOffMethodDTO methodInput, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveInventoryWriteOffMethod(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteInventoryWriteOffMethod(int methodId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteInventoryWriteOffMethod(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInventoryWriteOffTemplates(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.InventoryWriteOffTemplateGridDTO> EndGetInventoryWriteOffTemplates(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetInventoryWriteOffTemplate(int templateId, bool loadAccountSettings, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.InventoryWriteOffTemplateDTO EndGetInventoryWriteOffTemplate(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveInventoryWriteOffTemplate(SoftOne.Soe.Common.DTO.InventoryWriteOffTemplateDTO templateInput, System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountingSettingDTO> accountSettings, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveInventoryWriteOffTemplate(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteInventoryWriteOffTemplate(int templateId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteInventoryWriteOffTemplate(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetReconciliationPerAccount(int actorCompanyId, int accountId, System.DateTime? fromDate, System.DateTime? toDate, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.ReconciliationRowDTO> EndGetReconciliationPerAccount(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetReconciliationRows(int actorCompanyId, int dim1Id, string fromDim1, string toDim1, System.DateTime? fromDate, System.DateTime? toDate, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.ReconciliationRowDTO> EndGetReconciliationRows(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVatCodes(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VatCodeDTO> EndGetVatCodes(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVatCodesDict(int actorCompanyId, bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetVatCodesDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVatCode(int vatCodeId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.VatCodeDTO EndGetVatCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetSysVatAccountCodesDictByCountry(bool addEmptyRow, int countryId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetSysVatAccountCodesDictByCountry(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetSysVatAccountCodesDict(bool addEmptyRow, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetSysVatAccountCodesDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveVatCode(SoftOne.Soe.Common.DTO.VatCodeDTO vatCode, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveVatCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteVatCode(int vatCodeId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteVatCode(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherHeads(int actorCompanyId, int accountYearId, bool includeTemplate, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherGridDTO> EndGetVoucherHeads(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherHeadsTemplate(int actorCompanyId, int accountYearId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherGridDTO> EndGetVoucherHeadsTemplate(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherHeadSeqNbrs(int accountYearId, int voucherSeriesId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<int> EndGetVoucherHeadSeqNbrs(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherTemplates(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetVoucherTemplates(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherHead(int voucherHeadId, bool includeRows, bool includeRowAccounts, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.VoucherHeadDTO EndGetVoucherHead(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherHeadBySeqNbr(int actorCompanyId, int voucherSeriesId, int seqNbr, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.VoucherHeadDTO EndGetVoucherHeadBySeqNbr(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherHeadCompanyGroups(int accountPeriod, int accountYearId, int voucherSeriesId, int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherGridDTO> EndGetVoucherHeadCompanyGroups(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSaveVoucher(SoftOne.Soe.Common.DTO.VoucherHeadDTO voucher, System.Collections.Generic.List<SoftOne.Soe.Common.DTO.AccountingRowDTO> accountingRowDTOs, System.Collections.Generic.List<int> householdRowIds, int? revertVatVoucherId, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndSaveVoucher(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteVoucher(int voucherHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteVoucher(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginDeleteVoucherOnlySuperSupport(int voucherHeadId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndDeleteVoucherOnlySuperSupport(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginUpdateVoucherNumberOnlySuperSupport(int voucherHeadId, int newVoucherNr, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.Util.ActionResult EndUpdateVoucherNumberOnlySuperSupport(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherRows(int voucherHeadId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherRowDTO> EndGetVoucherRows(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherRowsFromSelection(SoftOne.Soe.Common.DTO.EvaluatedSelection es, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherRowDTO> EndGetVoucherRowsFromSelection(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginSearchVoucherRows(int actorCompanyId, System.DateTime? from, System.DateTime? to, int sFrom, int sTo, decimal dFrom, decimal dTo, decimal kFrom, decimal kTo, decimal aFrom, decimal aTo, string text, System.DateTime? createdFrom, System.DateTime? createdTo, string createdBy, int d1Id, string d1From, string d1To, int d2Id, string d2From, string d2To, int d3Id, string d3From, string d3To, int d4Id, string d4From, string d4To, int d5Id, string d5From, string d5To, int d6Id, string d6From, string d6To, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.SearchVoucherRowDTO> EndSearchVoucherRows(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVatVerifyVoucherRows(int actorCompanyId, System.DateTime? from, System.DateTime? to, decimal excludeDiffAmountLimit, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VatVerificationVoucherRowDTO> EndGetVatVerifyVoucherRows(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherRowHistory(int actorCompanyId, int voucherHeadId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherRowHistoryDTO> EndGetVoucherRowHistory(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginAddConcernVoucherSeries(int accountYearId, int actorCompanyId, AsyncCallback callback, object asyncState);

        SoftOne.Soe.Common.DTO.VoucherSeriesDTO EndAddConcernVoucherSeries(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherSeries(int accountYearId, int actorCompanyId, bool includeTemplate, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherSeriesDTO> EndGetVoucherSeries(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherSeriesDict(int accountYearId, int actorCompanyId, bool addEmptyRow, bool includeTemplate, AsyncCallback callback, object asyncState);

        System.Collections.Generic.Dictionary<int, string> EndGetVoucherSeriesDict(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetVoucherSeriesTypes(int actorCompanyId, AsyncCallback callback, object asyncState);

        System.Collections.Generic.IEnumerable<SoftOne.Soe.Common.DTO.VoucherSeriesTypeDTO> EndGetVoucherSeriesTypes(IAsyncResult result);


        [OperationContract(
            AsyncPattern = true)]
        IAsyncResult BeginGetDefaultVoucherSeriesId(int accountYearId, SoftOne.Soe.Common.Util.CompanySettingType type, int actorCompanyId, AsyncCallback callback, object asyncState);

        int EndGetDefaultVoucherSeriesId(IAsyncResult result);
    }
}

