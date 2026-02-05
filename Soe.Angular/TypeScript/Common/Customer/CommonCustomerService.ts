import { IHttpService } from "../../Core/Services/HttpService";
import { PaymentRowSaveDTO } from "../Models/PaymentRowDTO";
import { AccountingRowDTO } from "../Models/AccountingRowDTO";
import { CustomerInvoiceGridDTO } from "../Models/InvoiceDTO";
import { CustomerLedgerSaveDTO } from "../Models/CustomerLedgerSaveDTO";
import { TermGroup_SysContactAddressType, SoeInvoiceMatchingType, SoeInvoiceType, CompanySettingType, SoeOriginType, SoeModule, TermGroup_AttestEntity, OrderInvoiceRegistrationType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { ICustomerInvoiceDistributionResultDTO, IHouseholdTaxDeductionApplicantDTO } from "../../Scripts/TypeLite.Net4";
import { FileUploadDTO } from "../Models/FileUploadDTO";

export interface ICommonCustomerService {

    // GET

    getAccountYearId(date: Date): ng.IPromise<any>;
    getAccountPeriodId(accountYearId: number, date: Date): ng.IPromise<any>;
    getAccountSysVatRate(accountId: number): ng.IPromise<any>;
    getContactAddresses(actorId: number, type: TermGroup_SysContactAddressType, addEmptyRow: boolean, includeRows: boolean, includeCareOf: boolean): ng.IPromise<any>;
    getCurrentAccountYear(): ng.IPromise<any>;
    getCustomers(onlyActive: boolean): ng.IPromise<any>;
    getCustomersSmall(onlyActive: boolean): ng.IPromise<any>;
    getCustomersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean): ng.IPromise<any>;
    getCustomersBySearch(customerNr: string, name: string, billingAddress: string, deliveryAddress: string, note: string, actorCustomerId: number): ng.IPromise<any>;
    getCustomer(customerId: number, loadActor: boolean, loadAccount: boolean, loadNote: boolean, loadCustomerUser: boolean, loadContactAddresses: boolean, loadCategories: boolean): ng.IPromise<any>;
    getCustomerForExport(customerId: number): ng.IPromise<any>;
    getCustomerStatistics(customerId: number, allItemSelection: number): ng.IPromise<any>;
    getHouseHoldTaxApplicants(customerId: number): ng.IPromise<any>;
    getCustomerStatisticsAllCustomers(originType: number, fromDate: Date, toDate: Date): ng.IPromise<any>;
    getCustomerEmails(customerId: number, loadContactPersonsEmails: boolean, addEmptyRow: boolean): ng.IPromise<any>;
    getCustomerGLNs(customerId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getCustomerReferences(customerId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getCustomerContactAddressDict(contactPersonId): ng.IPromise<any>;
    getDeliveryConditionsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getDeliveryTypesDict(addEmptyRow: boolean): ng.IPromise<any>;
    getInvoiceProductsSmall(): ng.IPromise<any>;
    getNextCustomerNr(): ng.IPromise<any>;
    getPaymentConditions(useCache?: boolean): ng.IPromise<any>;
    getPaymentConditionsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getCustomerReportTemplateId(customerId: number, registrationType: OrderInvoiceRegistrationType): ng.IPromise<any>;

    getMatchCodes(soeInvoiceType: SoeInvoiceMatchingType, addEmptyRow: boolean): ng.IPromise<any>;
    getInvoicePaymentsMatches(supplierId: number, soeInvoiceType: SoeInvoiceType): ng.IPromise<any>;
    getMatchingCustomerSupplier(soeInvoiceType: number): ng.IPromise<any>;
    getUnpaidInvoices(customerId: number): ng.IPromise<any>;
    getUnpaidCustomerInvoicesForDialog(customerId: number): ng.IPromise<any>;
    getInvoiceForPayment(invoiceId: number): ng.IPromise<any>;
    getPayment(paymentRowId: number, loadInvoiceAndOrigin: boolean, loadAccountRows: boolean, loadAccounts: boolean): ng.IPromise<any>;
    getPaymentInformationViews(supplierId: number): ng.IPromise<any>;
    getPaymentMethod(paymentMethodId: number, loadAccount: boolean, loadPaymentInformationRow: boolean): ng.IPromise<any>;
    saveCustomerPayment(paymentRowSaveDTO: PaymentRowSaveDTO, accountingRows: AccountingRowDTO[], matchCodeId?: number): ng.IPromise<any>;

    getPriceLists(useCache? : boolean): ng.IPromise<any>;
    getPriceListsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<any>;
    getSysWholesellersDict(addEmptyRow: boolean): ng.IPromise<any>;
    getCustomerInvoices(classification: number, originType: number, loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, loadActive: boolean, allItemsSelection: number, billing: boolean, modifiedIds?: number[]): ng.IPromise<CustomerInvoiceGridDTO[]>;
    getInvoice(invoiceId: number): ng.IPromise<any>;
    getCustomerLedger(invoiceId: number): ng.IPromise<any>;
    getPrintedReminderInformation(invoiceId: number): ng.IPromise<any>;
    getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean): ng.IPromise<any>;
    getDefaultVoucherSeriesId(accountYearId: number, accountingVoucherSeriesTypeManual: CompanySettingType);
    getVatCodes(): ng.IPromise<any>;
    getPaymentMethods(paymentType: SoeOriginType, addEmptyRow: boolean, includePaymentInformationRows: boolean, includeAccount: boolean, onlyCashSales: boolean, forceRefresh?: boolean): ng.IPromise<any>;
    getAttestStatesGenericList(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean, addMultipleRow: boolean): ng.IPromise<any>;
    getInvoiceAndPaymentStatus(soeOriginType: SoeOriginType, addEmptyRow: boolean);
    getInterestRateCalculationReportPrintUrl(invoiceIds: any[], reportId: number, sysReportTemplateId: number): ng.IPromise<any>;
    getOrderTemplates(): ng.IPromise<any>;
    getEdiEntryViewsCount(classification: number, originType: number): ng.IPromise<any>;
    checkCustomerCreditLimit(customerId: number, creditLimit: number): ng.IPromise<any>;
    getFilteredCustomerInvoices(filterModels: any): ng.IPromise<any>;
    getCustomerInvoiceInterests(customerId: number, loadCustomer: boolean, loadProduct: boolean): ng.IPromise<any>;
    getCustomerInvoiceReminders(customerId: number, loadCustomer: boolean, loadProduct: boolean): ng.IPromise<any>;

    // GET - FROM CUSTOMERSERVICE
    getSysPaymentMethodsDict(paymentType: SoeOriginType, addEmptyRow: boolean): ng.IPromise<any>;
    getPaymentInformationViewsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getAccountStdsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getInvoicesPaymentsAndMatches(parameters: any): ng.IPromise<any>;
    getInvoicesMatches(recordId: number, actorId: number, type: any): ng.IPromise<any>;
    
    // POST                
    //saveCustomerInvoice(invoice: Soe.Common.Models.CustomerInvoiceDTO): ng.IPromise<any>;        
    saveCustomerLedger(invoice: CustomerLedgerSaveDTO, filesDto: FileUploadDTO[], accountingRows: AccountingRowDTO[]): ng.IPromise<any>;
    saveCustomer(customer: any, householdTaxApplicants: IHouseholdTaxDeductionApplicantDTO[], extraFields: any[]): ng.IPromise<any>;
    updateCustomersState(dict: any): ng.IPromise<any>;
    updateCustomersIsPrivatePerson(dict: any): ng.IPromise<any>;
    transferCustomerInvoices(items: any[], statusChange: number, accountYearId: number, paymentMethodId: number, mergeInvoices: boolean, claimLevel: number, checkPartialInvoicing: boolean, setStatusToOrigin?: boolean, bulkPayDate?: Date, bulkInvoiceDate?: Date, bulkDueDate?: Date, bulkVoucherDate?: Date, emailTemplateId?: number, reportId?: number, languageId?: number, mergePdfs?: boolean, keepFixedPriceOrderOpen?: boolean, isOverrideFinvoiceOperatorWarning?: boolean): ng.IPromise<any>;
    TransferCustomerInvoicesToVouchers(IdsToTransfer: any, accYear: number): ng.IPromise<any>;
    automaticallyDistribute(items: CustomerInvoiceGridDTO[]): ng.IPromise<ICustomerInvoiceDistributionResultDTO>;
    ExportCustomerInvoicesToSOP(IdsToTransfer: any): ng.IPromise<any>;
    ExportCustomerInvoicesToUniMicro(IdsToTransfer: any): ng.IPromise<any>;
    ExportCustomerInvoicesToDIRegnskap(IdsToTransfer: any): ng.IPromise<any>;
    getCustomerInvoicesForProjectCentral(classification: number, originType: number, projectId: number, loadChildProjects: boolean, fromDate?: Date, toDate?: Date, invoiceIds?: number[]): ng.IPromise<any>;
    getCustomerInvoicesForCustomerCentral(classification: number, originType: number, actorCustomerId: number, onlyMine: boolean): ng.IPromise<any>;

    CalculateAccountBalanceForAccountsFromVoucher(accountYearId: number): ng.IPromise<any>;
    InvoicePaymentMatchAndVoucher(invoicePaymentMatchAndVoucher: any): ng.IPromise<any>;

    UpdateContractPrices(invoiceIds: number[], rounding: number, percent: number, amount: number): ng.IPromise<any>;

    // POST - FROM CUSTOMERSERVICE
    savePaymentMethod(paymentMethod: any): ng.IPromise<any>;
    saveInsecureDebts(selectedIds: number[]): ng.IPromise<any>;
    saveNotInsecureDebts(selectedIds: number[]): ng.IPromise<any>;
    getCustomerCentralCountersAndBalance(counterTypes: any, customerId: number, accountYearId: number, baseSysCurrencyId: number): ng.IPromise<any>;
    getAgeDistribution(searchModel);

    // DELETE        
    deleteCustomer(customerId: number): ng.IPromise<any>;
    deleteCustomerInvoice(customerInvoiceId: number): ng.IPromise<any>;
    cancelPayment(paymentRowId: number): ng.IPromise<any>;
    cancelPaymentWithVoucher(paymentRowId: number): ng.IPromise<any>;
    deleteCustomerInvoiceInterests(customerId: number): ng.IPromise<any>;
    deleteCustomerInvoiceReminders(customerId: number): ng.IPromise<any>;

    // DELETE - CUSTOMERSERVICE
    deletePaymentMethod(paymentMethod: any): ng.IPromise<any>
}

export class CommonCustomerService implements ICommonCustomerService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getAccountYearId(date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_ID + dateString, true);
    }

    getAccountPeriodId(accountYearId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD_ID + accountYearId + "/" + dateString, true);
    }

    getAccountSysVatRate(accountId: number) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_SYS_VAT_RATE + accountId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getContactAddresses(actorId: number, type: TermGroup_SysContactAddressType, addEmptyRow: boolean, includeRows: boolean, includeCareOf: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_ADDRESS + actorId + "/" + type + "/" + addEmptyRow + "/" + includeRows + "/" + includeCareOf, false);
    }

    getCurrentAccountYear() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_CURRENT_ACCOUNT_YEAR, null, Constants.CACHE_EXPIRE_LONG);
    }

    getCustomers(onlyActive: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + "?onlyActive=" + onlyActive, false);
    }

    getCustomersSmall(onlyActive: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + "?onlyActive=" + onlyActive, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getCustomersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + "?onlyActive=" + onlyActive + "&addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_VERY_SHORT, !useCache);
    }

    getCustomersBySearch(customerNr: string, name: string, billingAddress: string, deliveryAddress: string, note: string, actorCustomerId: number) {
        const model = { customerNr: customerNr, name: name, billingAddress: billingAddress, deliveryAddress: deliveryAddress, note: note, actorCustomerId: actorCustomerId };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_SEARCH, model);
    }

    getCustomer(customerId: number, loadActor: boolean, loadAccount: boolean, loadNote: boolean, loadCustomerUser: boolean, loadContactAddresses: boolean, loadCategories: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + customerId + "/" + loadActor + "/" + loadAccount + "/" + loadNote + "/" + loadCustomerUser + "/" + loadContactAddresses + "/" + loadCategories, false);
    }

    getCustomerForExport(customerId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_EXPORT + customerId, false);
    }

    getCustomerEmails(customerId: number, loadContactPersonsEmails: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_EMAIL + customerId + "/" + loadContactPersonsEmails + "/" + addEmptyRow, false);
    }

    getCustomerGLNs(customerId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_GLN + customerId + "/" + addEmptyRow, false);
    }

    getCustomerReferences(customerId: number, addEmptyRow: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_REFERENCE + customerId + "/" + addEmptyRow, false);
    }

    getCustomerContactAddressDict(contactPersonId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_ADDRESSDICT + contactPersonId, false);
    }

    getCustomerStatistics(customerId: number, allItemSelection: number) {
        const model = {
            CustomerId: customerId,
            AllItemSelection: allItemSelection
        };

        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_STATISTICS, model);
    }

    getCustomerStatisticsAllCustomers(originType: number, fromDate: Date, toDate: Date) {
        const model = {
            OriginType: originType,
            FromDate: fromDate,
            ToDate: toDate,
        };

        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_STATISTICS_ALL_CUSTOMERS, model);
    }

    getHouseHoldTaxApplicants(customerId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_CUSTOMER_HOUSEHOLD_TAXAPPLICANTS + customerId, false);
    }

    getDeliveryConditionsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_CONDITION + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getDeliveryTypesDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_TYPE + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getInvoiceProductsSmall() {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + true + "/" + false + "/" + false + "/" + false + "/" + false, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
        //            return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getNextCustomerNr() {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_NEXT_CUSTOMER_NR, false);
    }

    getPaymentConditions(useCache: boolean = true) {
        if (useCache)
            return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG);
        else
            return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getPaymentConditionsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getCustomerReportTemplateId(customerId: number, registrationType: OrderInvoiceRegistrationType) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_BILLINGREPORT + customerId + "/" + registrationType, false);
    }

    getMatchCodes(soeInvoiceMatchingType: SoeInvoiceMatchingType, addEmptyRow: boolean = false): ng.IPromise<any> {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_MATCHCODES + soeInvoiceMatchingType + "/" + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }
    getMatchingCustomerSupplier(soeInvoiceType: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_MATCHINGCUSTOMERSUPPLIER + soeInvoiceType, false);
    }
    getInvoicePaymentsMatches(supplierId: number, soeInvoiceType: SoeInvoiceType): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_INVOICEPAYMENTSMATCHES + supplierId + "/" + soeInvoiceType, false);
    }
    getUnpaidInvoices(customerId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_UNPAID + customerId, false);
    }
    getUnpaidCustomerInvoicesForDialog(customerId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_UNPAID_DIALOG + customerId, false);
    }

    getPaymentMethod(paymentMethodId: number, loadAccount: boolean, loadPaymentInformationRow: boolean = false) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD + paymentMethodId + "/" + loadAccount + "/" + loadPaymentInformationRow, false);
    }
    getInvoiceForPayment(invoiceId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_PAYMENT + invoiceId, false);
    }
    getPaymentInformationViews(customerId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_INFORMATION + customerId, false);
    }
    getPayment(paymentRowId: number, loadInvoiceAndOrigin: boolean, loadAccountRows: boolean, loadAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENTROW + paymentRowId + "/" + loadInvoiceAndOrigin + "/" + loadAccountRows + "/" + loadAccounts, false);
    }

    saveCustomerPayment(paymentRowSaveDTO: PaymentRowSaveDTO, accountingRows: AccountingRowDTO[], matchCodeId: number = null) {
        const model = {
            payment: paymentRowSaveDTO,
            accountingRows: _.filter(accountingRows, r => !r.isDeleted && r.dim1Id),
            matchCodeId: matchCodeId
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENTROW, model);
    }

    getPriceLists(useCache: boolean = true) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, null, Constants.CACHE_EXPIRE_LONG,!useCache);
    }
    
    getPriceListsDict(addEmptyRow: boolean, useCache: boolean = true) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getSysWholesellersDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_SYS_WHOLESELLERS + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getCustomerInvoices(classification: number, originType: number, loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, loadActive: boolean, allItemsSelection: number, billing: boolean, modifiedIds?: number[]): ng.IPromise<CustomerInvoiceGridDTO[]> {
        const model = {
            classification: classification,
            originType: originType,
            loadOpen: loadOpen,
            loadClosed: loadClosed,
            onlyMine: onlyMine,
            loadActive: loadActive,
            allItemsSelection: allItemsSelection,
            billing: billing,
            modifiedIds: modifiedIds,  
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES, model);
    }

    getInvoice(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE + invoiceId, false);
    }

    getCustomerLedger(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_CUSTOMERLEDGER + invoiceId, false);
    }

    getPrintedReminderInformation(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_REMINDERINFORMATION + invoiceId, false);
    }

    getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + accountYearId + "/" + includeTemplate, null, Constants.CACHE_EXPIRE_LONG);
    }

    getDefaultVoucherSeriesId(accountYearId: number, type: CompanySettingType) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + accountYearId + "/" + type, false);
    }

    getVatCodes() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE, null, Constants.CACHE_EXPIRE_LONG);
    }

    getPaymentMethods(paymentType: SoeOriginType, addEmptyRow: boolean, includePaymentInformationRows: boolean, includeAccount: boolean, onlyCashSales: boolean, forceRefresh?: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD + paymentType + "/" + addEmptyRow + "/" + includePaymentInformationRows + "/" + includeAccount + "/" + onlyCashSales, null, Constants.CACHE_EXPIRE_LONG, forceRefresh);
    }

    getAttestStatesGenericList(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean, addMultipleRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_GL + entity + "/" + module + "/" + addEmptyRow + "/" + addMultipleRow, false);
    }

    getInvoiceAndPaymentStatus(soeOriginType: SoeOriginType, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_CUSTOMER_INVOICE_INVOICEANDPAYMENTSTATUS + soeOriginType + "/" + addEmptyRow, true);
    }

    getInterestRateCalculationReportPrintUrl(invoiceIds: any[], reportId: number, sysReportTemplateId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_PRINT_INTERESTRATECALCULATION_PRINTURL + reportId + "/" + sysReportTemplateId + "/?invoiceIds=" + invoiceIds.join(','), false);
    }

    getOrderTemplates(): ng.IPromise<any> {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_ORDER_TEMPLATE, null, Constants.CACHE_EXPIRE_LONG);
    }

    getEdiEntryViewsCount(classification: number, originType: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_EDI_EDIENTRYVIEWS_COUNT + classification + "/" + originType, false);
    }

    checkCustomerCreditLimit(customerId: number, creditLimit: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_CREDITLIMIT + customerId + "/" + creditLimit, false);
    }

    getFilteredCustomerInvoices(filterModels: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_FILTERED, filterModels);
    }
    
    getCustomerInvoiceInterests(customerId: number, loadCustomer: boolean, loadProduct: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_INTERESTS + customerId + "/" + loadCustomer + "/" + loadProduct, false);
    }
    getCustomerInvoiceReminders(customerId: number, loadCustomer: boolean, loadProduct: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_REMINDERS + customerId + "/" + loadCustomer + "/" + loadProduct, false);
    }

    // GET - FROM CUSTOMERSERVICE

    getSysPaymentMethodsDict(paymentType: SoeOriginType, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_SYS_PAYMENT_METHOD + paymentType + "/" + addEmptyRow, false);
    }

    getPaymentInformationViewsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_INFORMATION + addEmptyRow, false);
    }

    getAccountStdsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_STD + addEmptyRow, true);
    }
    
    getInvoicesPaymentsAndMatches(parameters: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_CUSTOMER_INVOICE_MATCHES_PAYMENTS, parameters);
    }

    getInvoicesMatches(recordId: number, actorId: number, type: any): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_CUSTOMER_INVOICE_MATCHES + `?recordId=${recordId}&actorId=${actorId}&type=${type}`, false);
    }

    // POST
    saveCustomer(customer: any, householdTaxApplicants: IHouseholdTaxDeductionApplicantDTO[], extraFields: any[]) {
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER, { customer: customer, houseHoldTaxApplicants: householdTaxApplicants, extraFields: extraFields });
    }
   
    saveCustomerLedger(customerLedger: CustomerLedgerSaveDTO, filesDto: FileUploadDTO[], accountingRows: AccountingRowDTO[]) {
        const model = {
            invoice: customerLedger,
            accountingRows: _.filter(accountingRows, r => !r.isDeleted && r.dim1Id),
            files: filesDto
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_CUSTOMERLEDGER, model);
    }

    updateCustomersState(dict: any) {
        const model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_UPDATE_STATE, model);
    }

    updateCustomersIsPrivatePerson(list: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_UPDATE_IS_PRIVATE_PERSON, list);
    }

    transferCustomerInvoices(items: any[], originStatusChange: number, accountYearId: number, paymentMethodId: number, mergeInvoices: boolean, claimLevel: number, checkPartialInvoicing: boolean, setStatusToOrigin?: boolean, bulkPayDate?: Date, bulkInvoiceDate?: Date, bulkDueDate?: Date, bulkVoucherDate?: Date, emailTemplateId?: number, reportId?: number, languageId?: number, mergePdfs?: boolean, keepFixedPriceOrderOpen = false, isOverrideFinvoiceOperatorWarning = false) {
        const model = {
            items: items,
            accountYearId: accountYearId,
            originStatusChange: originStatusChange,
            paymentMethodId: paymentMethodId,
            claimLevel: claimLevel,
            mergeInvoices: mergeInvoices,
            bulkPayDate: bulkPayDate,
            bulkInvoiceDate: bulkInvoiceDate,
            bulkDueDate: bulkDueDate,
            bulkVoucherDate: bulkVoucherDate,
            checkPartialInvoicing: checkPartialInvoicing,
            setStatusToOrigin: setStatusToOrigin,
            emailTemplateId: emailTemplateId,
            reportId: reportId, 
            languageId: languageId,
            mergePdfs: mergePdfs,
            keepFixedPriceOrderOpen: keepFixedPriceOrderOpen,
            overrideFinvoiceOperatorWarning: isOverrideFinvoiceOperatorWarning
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_TRANSFER, model);
    }

    TransferCustomerInvoicesToVouchers(idsToTransfer: any, accYear: number) {
        const model = {
            IdsToTransfer: idsToTransfer,
            AccountYearId: accYear
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_TRANSFER_TO_VOUCHER, model);
    }

    automaticallyDistribute(items: CustomerInvoiceGridDTO[]) {
        const model = {
            items
        }
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_AUTOMATICALLYDISTRIBUTE, model);
    }

    ExportCustomerInvoicesToSOP(idsToTransfer: any) {
        const model = {
            IdsToTransfer: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_TRANSFER_TO_SOP, model);
    }

    ExportCustomerInvoicesToUniMicro(idsToTransfer: any) {
        const model = {
            IdsToTransfer: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_TRANSFER_TO_UNIMICRO, model);
    }

    ExportCustomerInvoicesToDIRegnskap(idsToTransfer: any) {
        const model = {
            IdsToTransfer: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_INVOICE_TRANSFER_TO_DIREGNSKAP, model);
    }

    CalculateAccountBalanceForAccountsFromVoucher(accountYearId: number) {
        const model = {
            AccountYearId: accountYearId
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_BALANCE_CALCULATE_FOR_ACCOUNTS_FROM_VOUCHER, model);
    }

    InvoicePaymentMatchAndVoucher(invoicePaymentMatchAndVoucher): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_INVOICEPAYMENTSMATCHANDVOUCHER, invoicePaymentMatchAndVoucher);
    }

    UpdateContractPrices(invoiceIds: number[], rounding: number, percent: number, amount: number) {
        const model = {
            InvoiceIds: invoiceIds,
            Rounding: rounding, 
            Percent: percent,
            Amount: amount,
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_CONTRACT_UPDATEPRICES, model);
    }

    getCustomerInvoicesForProjectCentral(classification: number, originType: number, projectId: number, loadChildProjects: boolean, fromDate: Date = undefined, toDate: Date = undefined, invoiceIds: number[] = undefined) {
        const model = {
            classification: classification,
            originType: originType,
            projectId: projectId,
            loadChildProjects: loadChildProjects,
            invoiceIds: invoiceIds,
            fromDate: fromDate, 
            toDate: toDate,
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_PROJECTCENTRAL, model);
    }

    getCustomerInvoicesForCustomerCentral(classification: number, originType: number, actorCustomerId: number, onlyMine: boolean) {
        const model = {
            classification: classification,
            originType: originType,
            actorCustomerId: actorCustomerId,
            onlyMine: onlyMine
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_CUSTOMERCENTRAL, model);
    }

    // POST - FROM CUSTOMERSERVICE
    savePaymentMethod(paymentMethod: any) {
        var path = "Economy/Common/PaymentMethods/SavePaymentMethod";
        //var model = { paymentMethod: paymentMethod, accountnr: accountnr, paymentType: paymentType };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD, paymentMethod);
    }

    saveInsecureDebts(selectedIds: number[]): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_CUSTOMER_INVOICE_INSECUREDEBTS, selectedIds);
    }

    saveNotInsecureDebts(selectedIds: number[]): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_CUSTOMER_INVOICE_NOTINSECUREDEBTS, selectedIds);
    }

    getCustomerCentralCountersAndBalance(counterTypes: Array<number>, customerId: number, accountYearId: number, baseSysCurrencyId: number) {

        const model = {
            CounterTypes: counterTypes,
            CustomerId: customerId,
            AccountYearId: accountYearId,
            BaseSysCurrencyId: baseSysCurrencyId,
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_CUSTOMERCENTRALCOUNTERSANDBALANCE, model);
    }

    getAgeDistribution(parameters: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_AGEDISTRIBUTION, parameters);
    }

    // DELETE

    deleteCustomer(customerId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + customerId);
    }

    deleteCustomerInvoice(customerInvoiceId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_COMMON_CUSTOMERLEDGER + customerInvoiceId);
    }

    cancelPayment(paymentRowId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENT_CANCEL + paymentRowId + "/" + false);
    }

    cancelPaymentWithVoucher(paymentRowId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENT_CANCEL_EXTENDED + paymentRowId);
    }

    deleteCustomerInvoiceInterests(customerId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_CUSTOMERINVOICES_INTERESTS + customerId);
    }

    deleteCustomerInvoiceReminders(customerId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_CUSTOMERINVOICES_REMINDERS + customerId);
    }

    // DELETE -FROM CUSTOMERSERVICE

    deletePaymentMethod(paymentMethodId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD + paymentMethodId);
    }
}