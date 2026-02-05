import { DistributionCodeHeadDTO, DistributionCodeGridDTO } from "../../../Common/Models/DistributionCodeHeadDTO";
import { IPaymentImportIODTO, IAccountDTO, IAccountDistributionEntryDTO, IAccountEditDTO, IVoucherTraceViewDTO, IPaymentInformationDTO, ISmallGenericType, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { AccountingRowDTO } from "../../../Common/Models/AccountingRowDTO";
import { AccountDistributionHeadDTO } from "../../../Common/Models/AccountDistributionHeadDTO";
import { AccountDistributionRowDTO } from "../../../Common/Models/AccountDistributionRowDTO";
import { VoucherHeadDTO } from "../../../Common/Models/VoucherHeadDTO";
import { GrossProfitCodeDTO } from "../../../Common/Models/GrossProfitCodeDTO";
import { BudgetHeadDTO, BudgetHeadSalesDTO, BudgetHeadFlattenedDTO } from "../../../Common/Models/BudgetDTOs";
import { PaymentImportDTO } from "../../../Common/Models/PaymentImportDTO";
import { PaymentImportIODTO } from "../../../Common/Models/PaymentImportIODTO";
import { IHttpService } from "../../../Core/Services/HttpService";
import { DistributionCodeBudgetType, CompanySettingType, ImportPaymentType, SoeInvoiceMatchingType, SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { FileUploadDTO } from "../../../Common/Models/FileUploadDTO";
import { AccountDimDTO, AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { AccountDTO, AccountSmallDTO, AccountEditDTO } from "../../../Common/Models/AccountDTO";
import { LiquidityPlanningDTO } from "../../../Common/Models/LiquidityPlanningDTO";
import { OpeningHoursDTO } from "../../../Common/Models/OpeningHoursDTO";
import { AccountYearBalanceFlatDTO } from "../../../Common/Models/AccountYearBalanceFlatDTO";
import { AccountYearDTO } from "../../../Common/Models/AccountYear";
import { VoucherSeriesDTO } from "../../../Common/Models/VoucherSeriesDTO";

export interface IAccountingService {

    // GET
    getAccounts(accountDimId: number, accountYearId: number, setLinkedToShiftType?: boolean, getCategories?: boolean, setParent?: boolean, useCache?: boolean): ng.IPromise<any>;
    getAccount(accountId: number): ng.IPromise<AccountEditDTO>;
    getAccountSmall(accountId: number): ng.IPromise<AccountSmallDTO>;
    getAccountName(accountId: number): ng.IPromise<string>;
    getAccountSysVatRate(accountId: number): ng.IPromise<any>;
    getAccountMappings(accountId: number);
    getAccountBalanceByAccount(accountId: number, loadYear: boolean): ng.IPromise<any>;
    getAccountBalances(accountYearId: number): ng.IPromise<any>;
    getAccountDims(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactiveDims: boolean, useCache?: boolean): ng.IPromise<AccountDimDTO[]>;
    getAccountDimsSmall(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, useCache?: boolean, loadInactives?: boolean): ng.IPromise<AccountDimSmallDTO[]>;
    getAccountDimsSmallMemoryCache(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactives?: boolean): ng.IPromise<AccountDimSmallDTO[]>;
    getAccountDim(accountDimId: number, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactiveDims: boolean, useCache?: boolean): ng.IPromise<AccountDimDTO>;
    getAccountDimSmall(accountDimId: number, loadAccounts: boolean, loadInternalAccounts: boolean, useCache?: boolean, ignoreHierarchyOnly?: boolean): ng.IPromise<AccountDimSmallDTO>;
    getAccountDimBySieNr(sieDimNr: number): ng.IPromise<any>;
    getAccountByAccountNr(accountNr: string, accountDimId: number, matchAll: boolean): ng.IPromise<any>;
    getAccountDimChars(): ng.IPromise<any>;
    getProjectAccountDim(): ng.IPromise<any>;
    getShiftTypeAccountDim(loadAccounts: boolean): ng.IPromise<any>;
    getAccountDistributionHeadsUsedIn(type: number, triggerType: number, date: Date, useInVoucher: boolean, useInSupplierInvoice: boolean, useInCustomerInvoice: boolean, useInImport: boolean, useInPayrollVoucher:boolean, useInPayollVacationVoucher:boolean, useCache?: boolean): ng.IPromise<any>;
    getAccountDistributionHeads(loadOpen: boolean, loadClosed: boolean, loadEntries: boolean): ng.IPromise<any>;
    getAccountDistributionHeadsAuto(useCache: boolean): ng.IPromise<any>;
    getAccountDistributionHead(accountDistributionHeadId: number): ng.IPromise<any>;
    getAccountDistributionTraceViews(accountDistributionHeadId: number): ng.IPromise<any>;
    getAccountDistributionEntries(date: Date, accontDistributionType: number, useCache: boolean, onlyActive: boolean): ng.IPromise<any>;
    getAccountDistributionEntriesForHead(accountDistributionHeadId: number): ng.IPromise<any>;
    getAccountDistributionEntriesForSource(accountDistributionHeadId: number, registrationType: number, sourceId: number): ng.IPromise<any>;
    getAccountStds(addEmptyRow: boolean): ng.IPromise<any>;
    getAccountStdsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getAccountStdsNumberName(addEmptyRow: boolean): ng.IPromise<any>;
    getAccountPeriod(accountYearId: number, date: Date, includeAccountYear: boolean, forceRefresh?: boolean): ng.IPromise<any>;
    getAccountPeriodId(accountYearId: number, date: Date): ng.IPromise<any>;
    getAccountPeriodDict(accountYearId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getAccountYearId(date: Date): ng.IPromise<any>;
    getAccountYearDict(addEmptyRow: boolean, excludeNew?: boolean): ng.IPromise<any>
    getAccountYear(accountYearId: number, getPeriods?: boolean, useCache?: boolean): ng.IPromise<any>;
    getAccountYearByDate(date: Date, useCache?: boolean): ng.IPromise<any>;
    getAccountYears(getPeriods?: boolean, excludeNew?: boolean, useCache?: boolean): ng.IPromise<any>;
    getAccountInternalsByDim(accountDimId: number): ng.IPromise<any>;
    getChildCompaniesDict(): ng.IPromise<any>;
    getCompanyGroupMappingsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getCompanyAdministrations(): ng.IPromise<any>;
    getCompanyAdministration(companyGroupAdminId: number): ng.IPromise<any>;
    getBudgetHeadsForGrid(budgetType: DistributionCodeBudgetType, actorCompanyId?: number): ng.IPromise<any>;
    getSalesBudgetHeadsForGrid(): ng.IPromise<any>;
    getBudget(budgetId: number, loadRows: boolean): ng.IPromise<any>;
    getSalesBudget(budgetId: number, loadRows: boolean, interval: number, dateFrom: Date): ng.IPromise<any>;
    getSalesBudgetV2(budgetId: number): ng.IPromise<BudgetHeadSalesDTO>;
    getDistributionCodesByType(distributionCodeType: DistributionCodeBudgetType, loadPeriods: boolean): ng.IPromise<DistributionCodeHeadDTO[]>;
    getDistributionCodes(includePeriods: boolean, useCache?: boolean, budgetType?: DistributionCodeBudgetType, fromDate?: Date, toDate?: Date): ng.IPromise<DistributionCodeHeadDTO[]>;
    getDistributionCodesForGrid(): ng.IPromise<DistributionCodeGridDTO[]>;
    getDistributionCode(distributionCodeHeadId: number): ng.IPromise<DistributionCodeHeadDTO>;
    getOpeningHours(useCache: boolean, fromDate?: Date, toDate?: Date): ng.IPromise<OpeningHoursDTO[]>;
    getOpeningHoursDict(useCache: boolean, addEmptyRow: boolean, includeDateInName: boolean): ng.IPromise<ISmallGenericType[]>;
    getOpeningHour(openingHourId: number): ng.IPromise<OpeningHoursDTO>;
    getSysAccountSruCodes(addEmptyRow: boolean): ng.IPromise<any>
    getSysAccountStd(sysAccountStdTypeId: number, accountNr: string): ng.IPromise<any>
    copySysAccountStd(sysAccountStdId: number): ng.IPromise<any>
    getSysAccountStdTypes(): ng.IPromise<any>
    getSysVatAccounts(sysCountryId: number, addEmptyRow: boolean): ng.IPromise<any>
    getSysVatRate(sysVatAccountId): ng.IPromise<any>
    getVatCodes(useCache: boolean): ng.IPromise<any>
    getVatCodesGrid(): ng.IPromise<any>
    getVatCodesDict(addEmptyRow: boolean): ng.IPromise<any>
    getVatCode(vatCodeId: number): ng.IPromise<any>
    getVouchersBySeries(accountYearId: number, voucherSeriesTypeId: number): ng.IPromise<any>
    getVoucherTemplates(accountYearId: number): ng.IPromise<any>
    getVoucherTemplatesDict(accountYearId: number): ng.IPromise<any>
    getVoucher(voucherHeadId: number, loadVoucherSeries: boolean, loadVoucherRows: boolean, loadVoucherRowAccounts: boolean, loadAccountBalance: boolean): ng.IPromise<any>
    getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean, useCache: boolean): ng.IPromise<any>
    getVoucherSeriesByYearDate(accountYear: Date, includeTemplate: boolean, useCache: boolean): ng.IPromise<any>
    getDefaultVoucherSeriesId(accountYearId: number, type: CompanySettingType): ng.IPromise<any>
    getVoucherSeriesTypes(useCache?: boolean): ng.IPromise<any>
    getVoucherSeriesType(voucherSeriesTypeId: number): ng.IPromise<any>
    getVatVerifyVoucherRows(fromDate: any, toDate: any, excludeDiffAmountLimit: number): ng.IPromise<any>
    getVoucherRowHistory(voucherHeadId: number): ng.IPromise<any>
    getVoucherRows(voucherHeadId: number): ng.IPromise<any>
    getGrossProfitCodes(refreshCache?: boolean): ng.IPromise<any>
    getGrossProfitCodesForAccountYear(accountYearId: number): ng.IPromise<any>
    getGrossProfitCode(grossProfitCodeId: number): ng.IPromise<any>
    getMatchCodesForGrid(forceRefresh?: boolean): ng.IPromise<any>
    getMatchCodes(soeInvoiceType: SoeInvoiceMatchingType, addEmptyRow: boolean): ng.IPromise<any>
    getMatchCodesDict(type: number): ng.IPromise<any>
    getMatchCode(matchCodeId: number): ng.IPromise<any>
    getReconciliationRows(dim1Id: number, fromDim1: any, toDim1: any, fromDate: Date, toDate: Date): ng.IPromise<any>
    getReconciliationPerAccount(accountId: number, fromDate: Date, toDate: Date): ng.IPromise<any>
    getAccountDimStd(): ng.IPromise<any>
    getUserNamesWithLogin(): ng.IPromise<any>
    getPaymentConditions(useCache?: boolean): ng.IPromise<any>
    getPaymentCondition(paymentConditionCodeId: number): ng.IPromise<any>
    getGetPaymentInformationFromActor(actorId: number, loadPaymentInformation: boolean, loadActor: boolean, includeForeginPayments: boolean): ng.IPromise<any>
    getPaymentInformationViews(actorId: number, useCache: boolean): ng.IPromise<any>
    getBalanceChangeResult(key: any): ng.IPromise<any>
    getAccountPeriods(accountYearId: number): ng.IPromise<any>
    getHouseholdProductAccountId(productId: number): ng.IPromise<any>
    getVoucherTransactions(accountId: number, accountYearId: number, accountPeriodIdFrom: number, accountPeriodIdTo: number): ng.IPromise<any>
    getCompanyGroupMappings(): ng.IPromise<any>
    getCompanyGroupMapping(companyGroupMappingHeadId: number): ng.IPromise<any>
    getCompanyGroupVoucherHistory(accountYearId: number, transferType: number): ng.IPromise<any>
    getInvoiceJournalReportId(reportType: SoeReportTemplateType): ng.IPromise<any>
    getLiquidityPlanning(from: Date, to: Date, exclusion: Date, balance: number, unpaid: boolean, unchecked: boolean, checked: boolean): ng.IPromise<any>
    getLiquidityPlanningNew(from: Date, to: Date, exclusion: Date, balance: number, unpaid: boolean, unchecked: boolean, checked: boolean): ng.IPromise<any>
    getAccountYearBalance(accountYearId: number): ng.IPromise<any>
    getAccountYearBalanceForPreviousYear(accountYearId: number): ng.IPromise<any>
    validateAccountNr(accountNr: string, accountId: number, accountDimId: number): ng.IPromise<any>
    validateAccountDimNr(accountDimNr: number, accountDimId: number): ng.IPromise<any>

    //Temporalily here
    getPaymentExports(exportType: number, itemsSelectionType: number): ng.IPromise<any>;
    getPaymentImports(importType: number, allItemsSelection: number): ng.IPromise<any>;
    getPaymentImport(importId: number): ng.IPromise<any>;
    getExportedIOInvoices(exportPaymentId: number): ng.IPromise<any>;
    getPaymentMethods(originTypeId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getPaymentMethodsForImport(originTypeId: number): ng.IPromise<any>;
    getSysPaymentTypeDict(): ng.IPromise<any>;
    getDataStorages(typeId: number): ng.IPromise<any>;
    getPaymentServiceRecords(): ng.IPromise<any>;
    cancelPaymentExport(paymentExportId: number): ng.IPromise<any>;
    getInvoicesForPaymentService(paymentService: number): ng.IPromise<any>;
    undoDataStorage(dataStorageId: number): ng.IPromise<any>;
    updatePaymentImportIODTOS(items: IPaymentImportIODTO[], bulkPayDate: Date, accountYearId: number): ng.IPromise<any>;
    updateCustomerPaymentImportIODTOS(items: IPaymentImportIODTO[], bulkPayDate: Date, accountYearId: number, paymentMethodId: number): ng.IPromise<any>;
    updatePaymentImportIODTOSStatus(items: IPaymentImportIODTO[]): ng.IPromise<any>;
    getAccountDict(accountDimId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getAccountChildren(parentAccountId: number): ng.IPromise<IAccountDTO[]>;
    saveCompanyGroupMapping(companyGroupMappingHead: any): ng.IPromise<any>;
    deleteCompanyGroupMapping(companyGroupMappingHeadId: number): ng.IPromise<any>;

    // POST
    editVoucherNrOnlySuperSupport(voucherHeadId: number, newVoucherNr: number): ng.IPromise<any>;
    saveAccountDistribution(accountDistributionHeadDto: AccountDistributionHeadDTO, accountDistributionRows: AccountDistributionRowDTO[]): ng.IPromise<any>;
    transferToAccountDistributionEntry(periodDate: Date, accontDistributionType: number): ng.IPromise<any>;
    transferAccountDistributionEntryToVoucher(accountDistributionEntryDTOs: IAccountDistributionEntryDTO[], periodDate: Date, accontDistributionType: number): ng.IPromise<any>;
    restoreAccountDistributionEntries(accountDistributionEntryDTO: IAccountDistributionEntryDTO, accontDistributionType: number): ng.IPromise<any>;
    deleteAccountDistributionEntries(accountDistributionEntryDTOs: IAccountDistributionEntryDTO[], accontDistributionType: number): ng.IPromise<any>;
    deleteAccountDistributionEntriesPermanently(accountDistributionEntryDTO: IAccountDistributionEntryDTO, accontDistributionType: number): ng.IPromise<any>;
    deleteAccountDistributionEntriesForSource(accountDistributionHeadId: number, registrationType: number, sourceRecordId: number): ng.IPromise<any>;
    saveAccount(account: any, translations: any, accountMappings: any, categoryAccounts: any, extraFields: any[]): ng.IPromise<any>;
    saveAccountSmall(account: IAccountEditDTO): ng.IPromise<any>;
    updateAccountsState(accounts: any): ng.IPromise<any>;
    calculateAccountBalanceForAccounts(accountYearId?: number): ng.IPromise<any>;
    calculateAccountBalanceForAccountsAllYears(): ng.IPromise<any>;
    calculateAccountBalanceForAccountsFromVoucher(accountYearId: number): ng.IPromise<any>;
    calculateAccountBalanceForAccountInAccountYears(accountId: number): ng.IPromise<any>;
    saveDistributionCode(distributionCodeHeadDto: DistributionCodeHeadDTO): ng.IPromise<any>;
    saveAccountDim(accountDim: any, reset: boolean): ng.IPromise<any>;
    saveVatCode(vatCode: any): ng.IPromise<any>;
    savePaymentCondition(paymentCondition: any): ng.IPromise<any>;
    saveVoucher(voucherHead: VoucherHeadDTO, accountingRows: AccountingRowDTO[], householdRowIds: number[], files: FileUploadDTO[], revertVatVoucherId?: number): ng.IPromise<any>;
    saveVoucherSeriesType(voucherSeriesType: any): ng.IPromise<any>;
    saveGrossProfitCode(grossProfitCode: GrossProfitCodeDTO): ng.IPromise<any>;
    saveMatchCode(matchCode: any): ng.IPromise<any>;
    getSearchedVoucherRows(dto: any): ng.IPromise<any>;
    getImportedIoInvoices(batchId, importPaymentType: ImportPaymentType): ng.IPromise<any>;
    saveBudget(budgetHead: BudgetHeadFlattenedDTO): ng.IPromise<any>;
    saveSalesBudget(budgetHead: BudgetHeadDTO): ng.IPromise<any>;
    saveSalesBudgetV2(budgetHead: BudgetHeadSalesDTO): ng.IPromise<any>;
    getBalanceChangePerPeriod(key: any, noOfPeriods: number, accountYearId: number, accountId: number, getPrevious: boolean, dims: any): ng.IPromise<any>;
    updateAccountPeriodStatus(accountPeriodId: number, status: number): ng.IPromise<any>;
    saveCompanyGroupAdministration(companyGroupAdministration: any): ng.IPromise<any>;
    saveCompanyGroupVoucherSeries(accountYearId: number): ng.IPromise<any>;
    transferCompanyGroup(transferType: number, accountYearId: number, voucherSeriesId: number, periodFrom: number, periodTo: number, includeIB: boolean, budgetCompanyFrom: number, budgetCompanyGroup: number, budgetChild: number): ng.IPromise<any>;
    saveLiquidityPlanningTransaction(liquidityPlanningTransaction: LiquidityPlanningDTO): ng.IPromise<any>;
    saveAccountYearBalances(accountYearId: number, rows: AccountYearBalanceFlatDTO[]): ng.IPromise<any>;
    saveAccountYear(accountYear: AccountYearDTO, voucherSeries: VoucherSeriesDTO[], keepNumbers: boolean): ng.IPromise<any>;
    copyVoucherTemplatesFromPreviousYear(accountYearId: number): ng.IPromise<any>;
    copyGrossProfitCodesFromPreviousYear(accountYearId: number): ng.IPromise<any>;
    savePaymentImportHeader(model: PaymentImportDTO);
    savePaymentImportRow(paymentImportRow): ng.IPromise<any>;
    updatePaymentImportIO(model: PaymentImportIODTO): ng.IPromise<any>;
    updatePaymentImportIOs(models: PaymentImportIODTO[]): ng.IPromise<IActionResult>;
    importSysAccountStdType(sysAccountStdTypeId: number): ng.IPromise<any>;

    // DELETE
    deleteAccountDistribution(accountDistributionHeadId: number): ng.IPromise<any>;
    deleteDistributionCode(distributionCodeHeadId: number): ng.IPromise<any>;
    deleteVatCode(vatCodeId: number): ng.IPromise<any>;
    deleteVoucher(voucherHeadId: number): ng.IPromise<any>;
    deleteVoucherOnlySuperSupport(voucherHeadId: number, checkTransfer?: boolean): ng.IPromise<any>;
    deleteVouchersOnlySuperSupport(voucherHeadIds: number[]): ng.IPromise<any>;
    deleteVoucherSeriesType(voucherSeriesTypeId: number): ng.IPromise<any>;
    getVoucherTraceViews(voucherHeadId: number): ng.IPromise<IVoucherTraceViewDTO>;
    deleteGrossProfitCode(vatCodeId: number): ng.IPromise<any>;
    deleteMatchCode(matchCodeId: number): ng.IPromise<any>;
    deleteBudget(budgetHeadId: number): ng.IPromise<any>;
            
    deleteAccount(accountId: number): ng.IPromise<any>;
    deleteAccountDims(accountDimIds: number[]): ng.IPromise<any>;
    deletePaymentCondition(paymentConditionId: number): ng.IPromise<any>
    deleteCompanyGroupAdministration(companyGroupAdministrationId: number): ng.IPromise<any>;
    deleteLiquidityPlanningTransaction(liquidityPlanningTransactionId: number): ng.IPromise<any>;
    deleteCompanyGroupTransfer(companyGroupTransferHeadId: number[]): ng.IPromise<any>;
    deleteAccountYear(accountYearId: number): ng.IPromise<any>;
    deletePaymentImportIOInvoices(batchId: number, paymentType: number): ng.IPromise<any>;
    deletePaymentImportIORow(paymentImportIOId: number): ng.IPromise<any>;

    saveCustomerInvoicePaymentService(paymentService: number, items: any): ng.IPromise<any>;
}

export class AccountingService implements IAccountingService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getAccounts(accountDimId: number, accountYearId: number, setLinkedToShiftType = false, getCategories = false, setParent?: boolean, useCache = true) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT + "?accountDimId=" + accountDimId + "&accountYearId=" + accountYearId + "&setLinkedToShiftType=" + setLinkedToShiftType + "&getCategories=" + getCategories + "&setParent=" + setParent, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_MEDIUM, !useCache);
    }

    getAccount(accountId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BY_ID + "?accountId=" + accountId, false).then(x => {
            let obj = new AccountEditDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getAccountSmall(accountId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BY_ID + "?accountId=" + accountId, false, Constants.WEBAPI_ACCEPT_SMALL_DTO).then(x => {
            let obj = new AccountSmallDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getAccountName(accountId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_NAME + accountId, false);
    }

    getAccountSysVatRate(accountId: number) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_SYS_VAT_RATE + accountId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountMappings(accountId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_MAPPING + accountId, false);
    }

    getAccountBalanceByAccount(accountId: number, loadYear: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_BY_ACCOUNT + accountId + "/" + loadYear, false);
    }

    getAccountDims(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactiveDims: boolean, useCache = true) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?onlyStandard=" + onlyStandard + "&onlyInternal=" + onlyInternal + "&loadAccounts=" + loadAccounts + "&loadInternalAccounts=" + loadInternalAccounts + "&loadInactiveDims=" + loadInactiveDims, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then((x: AccountDimDTO[]) => {
            return x.map(y => {
                let obj = new AccountDimDTO();
                angular.extend(obj, y);
                if (obj.accounts) {
                    obj.accounts = obj.accounts.map(a => {
                        let aObj = new AccountDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    });
                }
                return obj;
            })
        });
    }

    getAccountDimsSmallQueryString(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactives = false) {
        return Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?onlyStandard=" + onlyStandard + "&onlyInternal=" + onlyInternal + "&loadAccounts=" + loadAccounts + "&loadInternalAccounts=" + loadInternalAccounts + "&loadInactives=" + loadInactives;
    }

    extendAccountDimSmallDTO(dim: AccountDimSmallDTO) {
        let obj = new AccountDimSmallDTO();
        angular.extend(obj, dim);
        if (obj.accounts) {
            obj.accounts = obj.accounts.map(a => {
                let aObj = new AccountDTO();
                angular.extend(aObj, a);
                return aObj;
            });
        }
        return obj;
    }

    getAccountDimsSmallMemoryCache(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactives = false) {
        // Created this since the alternative is caching on disk, I just want a short lived in memory cache.
        // By passing true in the 2nd arg (useCache) Angular will memory cache it. So on new page load it will fetch from server.
        const qs = this.getAccountDimsSmallQueryString(onlyStandard, onlyInternal, loadAccounts, loadInternalAccounts, loadInactives);
        return this.httpService.get(qs, true, Constants.WEBAPI_ACCEPT_SMALL_DTO).then((x: AccountDimSmallDTO[]) => {
            return x.map(y => this.extendAccountDimSmallDTO(y));
        });
    }

    getAccountDimsSmall(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, useCache = true, loadInactives = false) {
        const qs = this.getAccountDimsSmallQueryString(onlyStandard, onlyInternal, loadAccounts, loadInternalAccounts, loadInactives);
        return this.httpService.getCache(qs, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then((x: AccountDimSmallDTO[]) => {
            return x.map(y => this.extendAccountDimSmallDTO(y));
        });
    }

    getAccountDim(accountDimId: number, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactiveDims: boolean, useCache = true) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?accountDimId=" + accountDimId + "&loadAccounts=" + loadAccounts + "&loadInternalAccounts=" + loadInternalAccounts + "&loadInactiveDims=" + loadInactiveDims, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            let obj = new AccountDimDTO();
            angular.extend(obj, x);
            if (obj.accounts) {
                obj.accounts = obj.accounts.map(a => {
                    let aObj = new AccountDTO();
                    angular.extend(aObj, a);
                    return aObj;
                });
            }
            return obj;
        });
    }

    getAccountDimSmall(accountDimId: number, loadAccounts: boolean, loadInternalAccounts: boolean, useCache = true, ignoreHierarchyOnly: boolean = false) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?accountDimId=" + accountDimId + "&loadAccounts=" + loadAccounts + "&loadInternalAccounts=" + loadInternalAccounts + "&ignoreHierarchyOnly=" + ignoreHierarchyOnly, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            return this.extendAccountDimSmallDTO(x);
        });
    }

    getAccountDimBySieNr(sieDimNr: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_BY_SIENR + sieDimNr, true);
    }

    getAccountByAccountNr(accountNr: string, accountDimId: number, matchAll: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BY_ACCOUNTNR + accountNr + "/" + accountDimId + "/" + matchAll, true);
    }

    getProjectAccountDim() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_PROJECT, false);
    }

    getShiftTypeAccountDim(loadAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_SHIFT_TYPE + loadAccounts, false);
    }

    getAccountDimChars() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_CHARS, false);
    }

    getAccountDistributionHeadsUsedIn(type: number, triggerType: number, date: Date, useInVoucher: boolean, useInSupplierInvoice: boolean, useInCustomerInvoice: boolean, useInImport: boolean, useInPayrollVoucher: boolean, useInPayollVacationVoucher: boolean, useCache: boolean = true) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_USED_IN + '?type=' + type + '&triggerType=' + triggerType + '&date=' + dateString + '&useInVoucher=' + useInVoucher + '&useInSupplierInvoice=' + useInSupplierInvoice + '&useInCustomerInvoice=' + useInCustomerInvoice + '&useInImport=' + useInImport + '&useInPayrollVoucher=' + useInPayrollVoucher + '&useInPayrollVacationVoucher=' + useInPayollVacationVoucher, useCache);
    }

    getAccountDistributionHeads(loadOpen: boolean, loadClosed: boolean, loadEntries: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION + loadOpen + '/' + loadClosed + '/' + loadEntries, false);
    }

    getAccountDistributionHeadsAuto(useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_AUTO, null, Constants.CACHE_EXPIRE_MEDIUM, !useCache);
    }

    getAccountDistributionHead(accountDistributionHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION + accountDistributionHeadId, false);
    }

    getAccountDistributionTraceViews(accountDistributionHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_TRACEVIEWS + accountDistributionHeadId, false);
    }

    getAccountDistributionEntries(date: Date, accountDistributionType: number, useCache: boolean, onlyActive: boolean) {
        const dateString = (date) ? date.toDateTimeString() : null;
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES + dateString + "/" + accountDistributionType + "/" + onlyActive, useCache);
    }

    getAccountDistributionEntriesForHead(accountDistributionHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_FOR_HEAD + accountDistributionHeadId, false);
    }

    getAccountDistributionEntriesForSource(accountDistributionHeadId: number, registrationType: number, sourceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_FOR_SOURCE + accountDistributionHeadId + "/" + registrationType + "/" + sourceId, false);
    }

    getAccountStdsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_STD + addEmptyRow, true);
    }

    getAccountStds(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_STDS + addEmptyRow, true);
    }

    getAccountStdsNumberName(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_STD_NUMBERNAME + addEmptyRow, true);
    }

    getAccountPeriod(accountYearId: number, date: Date, includeAccountYear: boolean, forceRefresh: boolean = false) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD + accountYearId + "/" + dateString + "/" + includeAccountYear, null, null, forceRefresh);
    }

    getAccountPeriodId(accountYearId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD_ID + accountYearId + "/" + dateString, true);
    }

    getAccountPeriodDict(accountYearId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD + accountYearId + "/" + addEmptyRow, true);
    }

    getAccountYearId(date: Date) {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_ID + dateString, true);
    }

    getAccountYearDict(addEmptyRow: boolean, excludeNew = false) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_DICT + addEmptyRow + "/" + excludeNew, true);
    }

    getAccountYear(accountYearId: number, getPeriods?: boolean, useCache = true): ng.IPromise<any> {

        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR + accountYearId + "/" + (getPeriods ? getPeriods : false), useCache);
    }

    getAccountYearByDate(date: Date, useCache?: boolean): ng.IPromise<any> {
        const dateString = date ? date.toDateTimeString() : null;
        
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR + dateString, useCache);
    }

    getAccountYears(getPeriods = false, excludeNew = false, useCache = true): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_ALL + "/" + getPeriods + "/" + excludeNew, useCache);
    }

    getAccountInternalsByDim(accountDimId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_INTERNAL_BYDIM + accountDimId, true);
    }

    getBudgetHeadsForGrid(budgetType: DistributionCodeBudgetType, actorCompanyId: number = 0) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_BUDGET + budgetType + "/" + actorCompanyId, false);
    }

    getSalesBudgetHeadsForGrid() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SALESBUDGET, false);
    }

    getBudget(budgetId: number, loadRows: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_BUDGETHEAD + budgetId + "/" + loadRows, false);
    }

    getSalesBudget(budgetId: number, loadRows: boolean, interval: number, dateFrom: Date) {
        var fromDateString: string = null;
        if (dateFrom)
            fromDateString = dateFrom.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SALESBUDGET_RESULT + budgetId + "/" + loadRows + "/" + interval + "/" + fromDateString, false);
    }

    getSalesBudgetV2(budgetId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SALESBUDGET_V2 + budgetId, false).then(x => {
            let obj = new BudgetHeadSalesDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getChildCompaniesDict() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_CHILDCOMPANIES_DICT, false);
    }

    getCompanyGroupMappingsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPMAPPINGS_DICT + "/" + addEmptyRow, false);
    }

    getCompanyAdministrations() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANY_GROUP_ADMINISTRATION, false);
    }

    getCompanyAdministration(companyGroupAdminId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANY_GROUP_ADMINISTRATION + companyGroupAdminId, false);
    }

    getDistributionCode(distributionCodeHeadId: number): ng.IPromise<DistributionCodeHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_DISTRIBUTION_CODE + distributionCodeHeadId, false).then(x => {
            let obj = new DistributionCodeHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getDistributionCodes(includePeriods: boolean, useCache: boolean = true, budgetType: DistributionCodeBudgetType = null, fromDate: Date = null, toDate: Date = null): ng.IPromise<DistributionCodeHeadDTO[]> {
        let url: string = Constants.WEBAPI_ECONOMY_ACCOUNTING_DISTRIBUTION_CODE + "?includePeriods=" + includePeriods;
        if (budgetType)
            url += '&budgetType=' + budgetType;
        if (fromDate)
            url += '&fromDate=' + fromDate.toDateTimeString();
        if (toDate)
            url += '&toDate=' + toDate.toDateTimeString();

        return this.httpService.getCache(url, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            return x.map(y => {
                let obj = new DistributionCodeHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getDistributionCodesForGrid(): ng.IPromise<DistributionCodeGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_DISTRIBUTION_CODE, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new DistributionCodeGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getDistributionCodesByType(distributionCodeType: DistributionCodeBudgetType, loadPeriods: boolean): ng.IPromise<DistributionCodeHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_DISTRIBUTIONCODES_BY_TYPE + distributionCodeType + "/" + loadPeriods, false).then(x => {
            return x.map(y => {
                let obj = new DistributionCodeHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getOpeningHours(useCache: boolean, fromDate: Date = null, toDate: Date = null): ng.IPromise<OpeningHoursDTO[]> {
        let qs: string = '';
        if (fromDate)
            qs += '?fromDate=' + fromDate.toDateTimeString();
        if (toDate)
            qs += (qs ? '&' : '?') + 'toDate=' + toDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS + qs, useCache).then(x => {
            return x.map(y => {
                let obj = new OpeningHoursDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getOpeningHoursDict(useCache: boolean, addEmptyRow: boolean, includeDateInName: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS + "?addEmptyRow=" + addEmptyRow + "&includeDateInName=" + includeDateInName, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getOpeningHour(openingHourId: number): ng.IPromise<OpeningHoursDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS + openingHourId, false).then(x => {
            let obj = new OpeningHoursDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getSysAccountSruCodes(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_ACCOUNT_SRU_CODE + addEmptyRow, true);
    }

    getSysAccountStd(sysAccountStdTypeId: number, accountNr: string) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_ACCOUNT_STD + sysAccountStdTypeId + "/" + accountNr, false);
    }

    copySysAccountStd(sysAccountStdId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_ACCOUNT_STD_COPY + sysAccountStdId, false);
    }

    getSysAccountStdTypes() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_ACCOUNT_STD_TYPE, false);
    }

    getSysVatAccounts(sysCountryId: number, addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_VAT_ACCOUNT + sysCountryId + "/" + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getSysVatRate(sysVatAccountId: number) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_VAT_RATE + sysVatAccountId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getVatCodes(useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getVatCodesGrid() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_VERY_SHORT);
    }

    getVatCodesDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getVatCode(vatCodeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE + vatCodeId, false);
    }

    getVouchersBySeries(accountYearId: number, voucherSeriesTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_BYSERIES + accountYearId + "/" + voucherSeriesTypeId, false);
    }

    getVoucherTemplates(accountYearId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_TEMPLATE + "?accountYearId=" + accountYearId, false);
    }

    getVoucherTemplatesDict(accountYearId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_TEMPLATE + "?accountYearId=" + accountYearId, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getVoucher(voucherHeadId: number, loadVoucherSeries: boolean, loadVoucherRows: boolean, loadVoucherRowAccounts: boolean, loadAccountBalance: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER + voucherHeadId + "/" + loadVoucherSeries + "/" + loadVoucherRows + "/" + loadVoucherRowAccounts + "/" + loadAccountBalance, false);
    }

    getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + accountYearId + "/" + includeTemplate, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getVoucherSeriesByYearDate(accountYear: Date, includeTemplate: boolean, useCache: boolean) {
        var dateString: string = null;
        if (accountYear)
            dateString = accountYear.toDateTimeString();
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + dateString + "/" + includeTemplate, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getDefaultVoucherSeriesId(accountYearId: number, type: CompanySettingType) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + accountYearId + "/" + type, true);
    }

    getVoucherTraceViews(voucherHeadId: number): ng.IPromise<IVoucherTraceViewDTO> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_VOUCHER_GETVOUCHERTRACEVIEWS + voucherHeadId, false);
    }

    getVoucherSeriesTypes(useCache = true) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE, useCache);
    }

    getVoucherSeriesType(voucherSeriesTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE + voucherSeriesTypeId, false);
    }

    getVatVerifyVoucherRows(fromDate: Date, toDate: Date, excludeDiffAmountLimit: number) {
        let toDateString: string = null;
        let fromDateString: string = null;
        if (toDate)
            toDateString = toDate.toDateTimeString();
        if (fromDate)
            fromDateString = fromDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VATVERIFICATION_VATVERIFYVOUCHERROWS + fromDateString + "/" + toDateString + "/" + excludeDiffAmountLimit, false);
    }

    getVoucherRowHistory(voucherHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_ROW_HISTORY + voucherHeadId, false);
    }

    getVoucherRows(voucherHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_ROW + voucherHeadId, false);
    }

    getGrossProfitCodes(refreshCache = false) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_GROSSPROFIT_CODE, null, Constants.CACHE_EXPIRE_LONG, refreshCache);
    }

    getGrossProfitCodesForAccountYear(accountYearId: number) {

        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_GROSSPROFIT_CODE_BYYEAR + accountYearId, false);
    }

    getGrossProfitCode(grossProfitCodeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_GROSSPROFIT_CODE + grossProfitCodeId, false);
    }

    getMatchCodes(soeInvoiceType: SoeInvoiceMatchingType, addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_MATCHCODES + soeInvoiceType + "/" + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getMatchCodesForGrid(forceRefresh: boolean = false) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_MATCHCODES_FOR_GRID, null, Constants.CACHE_EXPIRE_LONG, forceRefresh);
    }

    getMatchCodesDict(type: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_MATCHCODE_DICT + type, false);
    }

    getMatchCode(matchCodeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_MATCHCODE + matchCodeId, false);
    }

    getReconciliationRows(dim1Id: number, fromDim1: any, toDim1: any, fromDate: Date, toDate: Date) {
        var toDateString: string = null;
        var fromDateString: string = null;
        if (toDate)
            toDateString = toDate.toDateTimeString();
        var fomDateString: string = null;
        if (fromDate)
            fromDateString = fromDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_RECONCILIATION_ROWS + dim1Id + "/" + fromDim1 + "/" + toDim1 + "/" + fromDateString + "/" + toDateString, false);
    }

    getReconciliationPerAccount(accountId: number, fromDate: Date, toDate: Date) {
        var toDateString: string = null;
        var fromDateString: string = null;
        if (toDate)
            toDateString = toDate.toDateTimeString();
        var fomDateString: string = null;
        if (fromDate)
            fromDateString = fromDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_RECONCILIATION_PERACCOUNT + accountId + "/" + fromDateString + "/" + toDateString, false);
    }

    getAccountDimStd() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_STD, false);
    }

    getUserNamesWithLogin() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USERNAMES, false);
    }

    getPaymentConditions(useCache: boolean = true) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_PaymentConditions, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getPaymentCondition(paymentConditionId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTCONDITION + paymentConditionId, false);
    }

    getGetPaymentInformationFromActor(actorId: number, loadPaymentInformation: boolean, loadActor: boolean, includeForeginPayments: boolean): ng.IPromise<IPaymentInformationDTO> {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENT_INFORMATION + actorId + "/" + true + "/" + loadActor + "/" + includeForeginPayments, null, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getPaymentInformationViews(actorId: number, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENT_INFORMATION + actorId, null, Constants.CACHE_EXPIRE_MEDIUM, useCache);
    }

    getBalanceChangeResult(key: any) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_BUDGET_RESULT + key, false);
    }

    getAccountPeriods(accountYearId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIODS + accountYearId, true);
    }

    getHouseholdProductAccountId(productId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_HOUSEHOLDPRODUCTACCOUNT + productId, true);
    }

    //Temporary solution (before we get export/import manager to initiate)
    getPaymentExports(exportType: number, itemsSelectionType: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTEXPORTS + exportType + "/" + itemsSelectionType, false);
    }

    getPaymentImports(importType: number, allItemsSelection: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORTS + importType + "/" + allItemsSelection, false);
    }

    getExportedIOInvoices(exportPaymentId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTSERVICE_EXPORTEDIOINVOICES + exportPaymentId, false);

    }


    getImportedIoInvoices(batchId, importPaymentType: ImportPaymentType): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORTEDIOINVOICES + batchId + "/" + importPaymentType, false);
    }

    getPaymentImport(importId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORT + importId, false);
    }

    getPaymentMethods(originTypeId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTMETHODS + originTypeId + "/" + addEmptyRow, false);
    }

    getPaymentMethodsForImport(originTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTMETHODS_FORIMPORT + originTypeId, false);
    }

    getSysPaymentTypeDict() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYSPAYMENTTYPES, false);
    }

    getDataStorages(typeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_DATASTORAGES + typeId, false);
    }

    getPaymentServiceRecords() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_INVOICEEXPORTS, false);
    }

    getInvoicesForPaymentService(paymentService: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTSERVICE_INVOICE + paymentService, false);
    }

    cancelPaymentExport(paymentExportId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_CANCELEXPORT + paymentExportId, null);
    }

    undoDataStorage(dataStorageId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_UNDO_DATASTORAGE + dataStorageId, null);
    }

    getAccountDict(accountDimId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DICT + accountDimId + "/" + addEmptyRow, true);
    }

    getAccountChildren(parentAccountId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_CHILDREN + parentAccountId, true);
    }

    getVoucherTransactions(accountId: number, accountYearId: number, accountPeriodIdFrom: number, accountPeriodIdTo: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHERSEARCH_TRANSACTIONS + accountId + "/" + accountYearId + "/" + accountPeriodIdFrom + "/" + accountPeriodIdTo, true);
    }

    getCompanyGroupMappings() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPMAPPINGS, false);
    }

    getCompanyGroupMapping(companyGroupMappingHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPMAPPINGS + companyGroupMappingHeadId, false);
    }

    getCompanyGroupVoucherHistory(accountYearId: number, transferType: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPVOUCHERHISTORY + accountYearId + "/" + transferType, false);
    }

    getInvoiceJournalReportId(reportType: SoeReportTemplateType) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_INVOICEJOURNALREPORTID + reportType, false);
    }

    getAccountYearBalance(accountYearId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_BALANCE + accountYearId, false);
    }

    getAccountYearBalanceForPreviousYear(accountYearId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_BALANCE_TRANSFER + accountYearId, false);
    }
    
    validateAccountNr(accountNr: string, accountId: number, accountDimId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_VALIDATE + accountNr + "/" + accountId + "/" + accountDimId, false);
    }

    // POST
    getLiquidityPlanning(from: Date, to: Date, exclusion: Date, balance: number, unpaid: boolean, unchecked: boolean, checked: boolean) {
        const model = {
            from: from,
            to: to,
            exclusion: exclusion,
            balance: balance,
            unpaid: unpaid,
            paidUnchecked: unchecked,
            paidChecked: checked
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_LIQUIDITYPLANNING + "Get", model);
    }

    getLiquidityPlanningNew(from: Date, to: Date, exclusion: Date, balance: number, unpaid: boolean, unchecked: boolean, checked: boolean) {

        const model = {
            from: from,
            to: to,
            exclusion: exclusion,
            balance: balance,
            unpaid: unpaid,
            paidUnchecked: unchecked,
            paidChecked: checked
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_LIQUIDITYPLANNING + "Get/new", model);
    }

    editVoucherNrOnlySuperSupport(voucherHeadId: number, newVoucherNr: number) {
        const model = {
            voucherHeadId: voucherHeadId,
            newVoucherNr: newVoucherNr
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SUPER_SUPPORT_EDIT_VOUCHER_NR, model);
    }

    saveAccountDistribution(accountDistributionHead: AccountDistributionHeadDTO, accountDistributionRows: AccountDistributionRowDTO[]) {
        const model = {
            accountDistributionHead: accountDistributionHead,
            accountDistributionRows: accountDistributionRows
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION, model);
    }
    savePaymentImportHeader(model: PaymentImportDTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORTHEADER, model);
    }

    savePaymentImportRow(model: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORT, model);
    }

    updatePaymentImportIO(model: PaymentImportIODTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORTIO, model);
    }

    updatePaymentImportIOs(model: PaymentImportIODTO[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORTIOS, model);
    }

    transferToAccountDistributionEntry(periodDate: Date, accountDistributionType: number): ng.IPromise<any> {

        const model = {
            periodDate: periodDate,
            accountDistributionType: accountDistributionType
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_TRANSFERTOACCOUNT, model);
    }

    transferAccountDistributionEntryToVoucher(accountDistributionEntryDTOs: IAccountDistributionEntryDTO[], periodDate: Date, accountDistributionType: number): ng.IPromise<any> {

        const model = {
            accountDistributionEntryDTOs: accountDistributionEntryDTOs,
            periodDate: periodDate,
            accountDistributionType: accountDistributionType
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_TRANSFERTOVOUCHERS, model);
    }

    restoreAccountDistributionEntries(accountDistributionEntryDTO: IAccountDistributionEntryDTO, accountDistributionType: number): ng.IPromise<any> {

        const model = {
            accountDistributionEntryDTO: accountDistributionEntryDTO,
            accountDistributionType: accountDistributionType
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_RESTORE, model);
    }

    deleteAccountDistributionEntries(accountDistributionEntryDTOs: IAccountDistributionEntryDTO[], accountDistributionType: number) {
        const model = {
            accountDistributionEntryDTOs: accountDistributionEntryDTOs,
            accountDistributionType: accountDistributionType
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_DELETE, model);
    }

    deleteAccountDistributionEntriesPermanently(accountDistributionEntryDTO: IAccountDistributionEntryDTO, accountDistributionType: number) {
        const model = {
            accountDistributionEntryDTO: accountDistributionEntryDTO,
            accountDistributionType: accountDistributionType
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_DELETEPERMANENTLY, model);
    }

    deleteAccountDistributionEntriesForSource(accountDistributionHeadId: number, registrationType: number, sourceRecordId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION_ENTRIES_DELETE_FOR_SOURCE + accountDistributionHeadId + "/" + registrationType + "/" + sourceRecordId, null);
    }

    saveAccount(account: any, translations: any, accountMappings: any, categoryAccounts: any, extraFields: any[]) {
        const model = {
            account: account,
            translations: translations,
            accountMappings: accountMappings,
            categoryAccounts: categoryAccounts,
            extraFields: extraFields
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT, model);
    }

    saveAccountSmall(account: IAccountEditDTO) {
        const model = {
            accountNr: account.accountNr,
            name: account.name,
            accountTypeId: account.accountTypeSysTermId,
            vatAccountId: account.sysVatAccountId,
            sruCode1Id: account.sysAccountSruCode1Id
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_SMALL, model);
    }

    updateAccountsState(dict: any) {
        const model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_UPDATE_STATE, model);
    }

    calculateAccountBalanceForAccounts(accountYearId?: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_CALCULATE_FOR_ACCOUNTS + accountYearId, null);
    }

    calculateAccountBalanceForAccountsAllYears() {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_CALCULATE_FOR_ACCOUNTS_ALL_YEARS, null);
    }

    calculateAccountBalanceForAccountsFromVoucher(accountYearId: number) {
        const model = {
            AccountYearId: accountYearId
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_CALCULATE_FOR_ACCOUNTS_FROM_VOUCHER, model);
    }

    calculateAccountBalanceForAccountInAccountYears(accountId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_CALCULATE_FOR_ACCOUNT_IN_ACCOUNT_YEARS + accountId, null);
    }

    getAccountBalances(accountYearId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_GET_ACCOUNT_BALANCE + accountYearId, null);
    }

    validateAccountDimNr(accountDimNr: number, accountDimId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_VALIDATE + accountDimNr + "/" + accountDimId, false);
    }

    saveAccountDim(accountDim: any, reset: boolean): ng.IPromise<any> {
        const model = { AccountDim: accountDim, Reset: reset }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM, model);
    }


    saveDistributionCode(distributionCodeHeadDto: DistributionCodeHeadDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_DISTRIBUTION_CODE, distributionCodeHeadDto);
    }

    saveVatCode(vatCode: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE, vatCode);
    }

    savePaymentCondition(paymentCondition: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTCONDITION, paymentCondition);
    }

    saveVoucher(voucherHead: VoucherHeadDTO, accountingRows: AccountingRowDTO[], householdRowIds: number[], files: FileUploadDTO[], revertVatVoucherId?: number) {
        var model = {
            voucherHead: voucherHead,
            accountingRows: _.filter(accountingRows, r => !r.isDeleted && r.dim1Id),
            householdRowIds: householdRowIds,
            files: files,
            revertVatVoucherId: revertVatVoucherId
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER, model);
    }

    saveVoucherSeriesType(voucherSeriesType: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE, voucherSeriesType);
    }

    saveGrossProfitCode(grossProfitCode: GrossProfitCodeDTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_GROSSPROFIT_CODE, grossProfitCode);
    }

    saveMatchCode(matchCode: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_MATCHCODE, matchCode);
    }

    getSearchedVoucherRows(dto: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHERSEARCH, dto);
    }

    saveBudget(budgetHead: BudgetHeadFlattenedDTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_BUDGET, budgetHead);
    }
    saveCustomerInvoicePaymentService(paymentService: number, items: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTSERVICE_INVOICE + paymentService, items);
    }
    saveSalesBudget(budgetHead: BudgetHeadDTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_SALESBUDGET, budgetHead);
    }
    saveSalesBudgetV2(budgetHead: BudgetHeadSalesDTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_SALESBUDGET_V2, budgetHead);
    }

    getBalanceChangePerPeriod(key: any, noOfPeriods: number, accountYearId: number, accountId: number, getPrevious: boolean, dims: any) {

        const model = {
            key: key,
            noOfPeriods: noOfPeriods,
            accountYearId: accountYearId,
            accountId: accountId,
            getPrevious: getPrevious,
            dims: dims
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_BUDGET_RESULT, model);
    }

    updateAccountPeriodStatus(accountPeriodId: number, status: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD_UPDATESTATUS + accountPeriodId + "/" + status, null);
    }

    updatePaymentImportIODTOS(items: IPaymentImportIODTO[], bulkPayDate: Date, accountYearId: number) {
        const model = {
            items: items,
            bulkPayDate: bulkPayDate,
            accountYearId: accountYearId
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORT_UPDATEPAYMENTIMPORTIO, model);
    }

    updateCustomerPaymentImportIODTOS(items: IPaymentImportIODTO[], bulkPayDate: Date, accountYearId: number, paymentMethodId: number) {
        const model = {
            items: items,
            bulkPayDate: bulkPayDate,
            accountYearId: accountYearId,
            paymentMethodId: paymentMethodId,
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORT_UPDATECUSTOMERPAYMENTIMPORTIO, model);
    }

    updatePaymentImportIODTOSStatus(items: IPaymentImportIODTO[]) {
        const model = {
            items: items,
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORT_UPDATEPAYMENTIMPORTIOSTATUS, model);
    }

    saveCompanyGroupAdministration(companyGroupAdministration: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANY_GROUP_ADMINISTRATION, companyGroupAdministration);
    }

    saveCompanyGroupMapping(companyGroupMappingHead: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPMAPPINGS, companyGroupMappingHead);
    }

    saveCompanyGroupVoucherSeries(accountYearId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPVOUCHERSERIE + "/" + accountYearId, null);
    }

    transferCompanyGroup(transferType: number, accountYearId: number, voucherSeriesId: number, periodFrom: number, periodTo: number, includeIB: boolean, budgetCompanyFrom: number, budgetCompanyGroup: number, budgetChild: number) {
        const model = {
            transferType: transferType,
            accountYearId: accountYearId,
            voucherSeriesId: voucherSeriesId,
            periodFrom: periodFrom,
            periodTo: periodTo,
            includeIB: includeIB,
            budgetCompanyFrom: budgetCompanyFrom,
            budgetCompanyGroup: budgetCompanyGroup,
            budgetChild: budgetChild,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPTRANSFER, model);
    }

    saveLiquidityPlanningTransaction(liquidityPlanningTransaction: LiquidityPlanningDTO) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_LIQUIDITYPLANNING, liquidityPlanningTransaction);
    }

    saveAccountYearBalances(accountYearId: number, rows: AccountYearBalanceFlatDTO[]) {
        const model = {
            accountYearId: accountYearId,
            items: rows,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_BALANCE, model);
    }

    saveAccountYear(accountYear: AccountYearDTO, voucherSeries: VoucherSeriesDTO[], keepNumbers: boolean) {
        const model = {
            accountYear: accountYear,
            voucherSeries: voucherSeries,
            keepNumbers: keepNumbers,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR, model);
    }

    copyVoucherTemplatesFromPreviousYear(accountYearId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_COPYVT + accountYearId, null);
    }

    copyGrossProfitCodesFromPreviousYear(accountYearId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_COPYGPC + accountYearId, null);
    }

    importSysAccountStdType(sysAccountStdTypeId) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_SYS_ACCOUNT_STD_TYPE_IMPORT + sysAccountStdTypeId, null);
    }

    // DELETE
    deleteAccountDistribution(accountDistributionHeadId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DISTRIBUTION + accountDistributionHeadId);
    }

    deleteDistributionCode(distributionCodeHeadId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_DISTRIBUTION_CODE + distributionCodeHeadId);
    }

    deleteVatCode(vatCodeId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE + vatCodeId);
    }

    deletePaymentCondition(paymentConditionId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTCONDITION + paymentConditionId);
    }

    deleteVoucher(voucherHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER + voucherHeadId);
    }

    deleteVoucherOnlySuperSupport(voucherHeadId: number, checkTransfer: boolean = false) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SUPER_SUPPORT + voucherHeadId + "/" + checkTransfer);
    }

    deleteVouchersOnlySuperSupport(voucherHeadIds: number[]) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SUPER_SUPPORT_MULTIPLE + "?voucherHeadIds=" + voucherHeadIds.join(','));
    }

    deleteVoucherSeriesType(voucherSeriesTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE + voucherSeriesTypeId);
    }

    deleteGrossProfitCode(grossProfitCodeId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_GROSSPROFIT_CODE + grossProfitCodeId);
    }

    deleteMatchCode(matchCodeId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_MATCHCODE + matchCodeId);
    }

    deleteBudget(budgetHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_BUDGET + budgetHeadId);
    }
        
    deleteAccount(accountId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT + accountId);
    }

    deleteAccountDims(accountDimIds: number[]): ng.IPromise<any> {
        if (accountDimIds.length > 1)
            return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?accDimIds=" + accountDimIds.join(','));
        else if (accountDimIds.length === 1)
            return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?accDimIds=" + accountDimIds);
    }

    deleteCompanyGroupAdministration(companyGroupAdministrationId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANY_GROUP_ADMINISTRATION + "/" + companyGroupAdministrationId);
    }

    deleteLiquidityPlanningTransaction(liquidityPlanningTransactionId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_LIQUIDITYPLANNING + "/" + liquidityPlanningTransactionId);
    }

    deleteCompanyGroupMapping(companyGroupMappingHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPMAPPINGS + companyGroupMappingHeadId);
    }

    deleteCompanyGroupTransfer(companyGroupTransferHeadId: number[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_CONSOLIDATING_ACCOUNTING_COMPANYGROUPTRANSFER_DELETE + companyGroupTransferHeadId, null);
    } 

    deleteAccountYear(accountYearId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR + accountYearId);
    }

    deletePaymentImportIOInvoices(batchId: number, paymentType: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORTEDIOINVOICES + batchId + "/" + paymentType);
    }

    deletePaymentImportIORow(paymentImportIOId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTIMPORTIO + paymentImportIOId);
    }
}

