import { IHttpService } from "../../../Core/Services/HttpService";
import { IProjectGridDTO, IVoucherTraceViewDTO, IActionResult, ISupplierAgreementDTO, IProjectTinyDTO, IPriceListTypeMarkupDTO } from "../../../Scripts/TypeLite.Net4";
import { ProductRowDTO, BillingInvoiceDTO, MarkupDTO, OriginUserDTO, PriceBasedMarkupDTO } from "../../../Common/Models/InvoiceDTO";
import { StringKeyValueList } from "../../../Common/Models/StringKeyValue";
import { ChecklistHeadRecordCompactDTO, ChecklistExtendedRowDTO } from "../../../Common/Models/ChecklistDTO";
import { FileUploadDTO } from "../../../Common/Models/FileUploadDTO";
import { Constants } from "../../../Util/Constants";
import { CompanyWholesellerPriceListViewDTO } from "../../../Common/Models/CompanyWholeSellerPriceListViewDTO"
import { CompanyWholesellerDTO } from "../../../Common/Models/CompanyWholesellerDTO"
import { PriceRuleDTO } from "../../../Common/Models/PriceRuleDTO"
import { FormulaWidget } from "../../../Common/Models/FormulaBuilderDTOs";
import { HouseholdTaxDeductionApplicantDTO } from "../../../Common/Models/HouseholdTaxDeductionApplicantDTO";
import { HouseholdTaxDeductionFileRowDTO } from "../../../Common/Models/HouseholdTaxDeductionFileRowDTOs";
import { TermGroup_HouseHoldTaxDeductionType } from "../../../Util/CommonEnumerations";

export interface IInvoiceService {

    // GET
    getDeliveryConditions(): ng.IPromise<any>;
    getDeliveryConditions(useCache: boolean): ng.IPromise<any>;
    getDeliveryConditionsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getDeliveryCondition(deliveryConditionId: number): ng.IPromise<any>;
    getDeliveryTypes(): ng.IPromise<any>;
    getDeliveryTypesDict(addEmptyRow: boolean): ng.IPromise<any>;
    getDeliveryType(deliveryTypeId: number): ng.IPromise<any>;
    getInvoice(invoiceId: number, includeCategories: boolean, includeRows: boolean): ng.IPromise<BillingInvoiceDTO>;
    getCashCustomerId(): ng.IPromise<any>;
    getPaymentConditions(): ng.IPromise<any>;
    getPaymentConditionsGrid(): ng.IPromise<any>;
    getPaymentConditionsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getPaymentCondition(paymentConditionId: number): ng.IPromise<any>;
    getPriceLists(): ng.IPromise<any>;
    getPriceListsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getPriceListsGrid(): ng.IPromise<any>;
    getPriceList(priceListId: number): ng.IPromise<any>;
    getCompanyPriceRules(): ng.IPromise<any>;
    GetPriceListTypeMarkups(): ng.IPromise<IPriceListTypeMarkupDTO[]>;
    getPriceRule(priceRuleId: number): ng.IPromise<any>;
    getProductGroups(addEmptyRow: boolean, refreshCache?: boolean): ng.IPromise<any>;
    getProductGroup(productGroupId: number): ng.IPromise<any>;
    getProjects(onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean, projectStatus: number): ng.IPromise<IProjectGridDTO[]>;
    getProjectsSmall(onlyActive: boolean, hidden: boolean, sortOnNumber: boolean): ng.IPromise<IProjectTinyDTO[]>;
    getProjectsForList(projectStatus: number, loadMine: boolean): ng.IPromise<IProjectGridDTO[]>;
    getProject(projectId: number): ng.IPromise<any>;
    getVoucherTraceViews(voucherHeadId: number): ng.IPromise<IVoucherTraceViewDTO>;
    getEInvoiceEntry(invoiceId: number): ng.IPromise<any>;
    getProjectGridDTO(projectId: number): ng.IPromise<any>;
    getInvoiceFromOrderCheckLists(invoiceId: number): ng.IPromise<any>;
    getSysWholesellersDict(addEmptyRow: boolean): ng.IPromise<any>;
    getSysWholesellersByCompanyDict(onlyNotUsed: boolean, addEmptyRow: boolean): ng.IPromise<any>;
    getSysWholeseller(sysWholesellerId: number, loadSysWholesellerEdi: boolean, loadSysEdiMsg: boolean, loadSysEdiType: boolean): ng.IPromise<any>;
    getSupplierAgreements(providerType: number): ng.IPromise<any>;
    getSupplierAgreementProviders(): ng.IPromise<any>;
    getCompanyWholesellerPriceLists(isUsed: boolean): ng.IPromise<any>;
    getCompanyWholeseller(companyWholesellerId: number): ng.IPromise<any>;
    getCompanyWholesellers(): ng.IPromise<any>;
    getSupplierBySysWholeseller(sysWholesellerId: number): ng.IPromise<any>;
    getPriceListsToUpdate(): ng.IPromise<any>;
    getSysPricelistCodeBySysWholesellerId(wholesellerIds: number[]): ng.IPromise<any>;
    getMarkup(isDiscount: boolean): ng.IPromise<any>;
    getDiscount(sysWholesellerId: number, code: string): ng.IPromise<any>;
    getInvoiceTemplates(): ng.IPromise<any>;
    getHouseHoldTaxRowInfo(invoiceId: number, customerInvoiceId: number): ng.IPromise<any>;
    getHouseholdTaxDeductionRows(classificationGroup: number, taxDeductionType: number): ng.IPromise<any>;
    getPriceBasedMarkup(): ng.IPromise<any>;
    getHouseHoldTaxRowForEdit(customerInvoiceRowId: number): ng.IPromise<any>;
    getHouseHoldTaxFileForEdit(ids: number[]): ng.IPromise<any>;

    // POST
    saveDeliveryCondition(deliveryCondition: any): ng.IPromise<any>;
    saveDeliveryType(deliveryType: any): ng.IPromise<any>;
    saveInvoice(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], checklistHeads: ChecklistHeadRecordCompactDTO[], checklistRows: ChecklistExtendedRowDTO[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean, crediting: boolean): ng.IPromise<any>;
    savePaymentCondition(paymentCondition: any): ng.IPromise<any>;
    savePriceList(priceList: any): ng.IPromise<any>;
    savePriceRule(priceRule: PriceRuleDTO): ng.IPromise<any>;
    saveProductGroup(productGroup: any): ng.IPromise<any>;
    updateProjectStatus(ids: number[], newStatus: number): ng.IPromise<any>;
    createEInvoice(invoiceId: number, download: boolean, overrideFinvoiceOperatorWarning: boolean): ng.IPromise<any>;
    saveSupplierAgreement(wholesellerId: number, priceListTypeId: number, generalDiscount: number, bytes: any[]): ng.IPromise<any>;
    saveCompanyWholesellerSetting(companyWholesellerDTO: CompanyWholesellerDTO, customerNbrs: string[], actorSupplierId: number): ng.IPromise<any>;
    saveSupplierAgreementDiscount(dto: ISupplierAgreementDTO): ng.IPromise<any>;
    saveCompanyWholesellerPriceLists(wholesellerPriceLists: CompanyWholesellerPriceListViewDTO[]): ng.IPromise<any>;
    upgradeCompanyWholesellerPriceLists(sysWholesellerIds: number[]): ng.IPromise<any>;
    saveMarkup(markupRows: MarkupDTO[]): ng.IPromise<any>;
    validatePriceRule(input): ng.IPromise<IActionResult>;
    saveHouseholdTaxReceived(rowIds: number[], receivedDate: Date): ng.IPromise<any>;
    saveHouseholdTaxApplied(rowIds: number[], appliedDate: Date): ng.IPromise<any>;
    saveHouseholdTaxDenied(invoiceId: number, rowId: number, deniedDate: Date): ng.IPromise<any>;
    saveHouseholdTaxWithdrawn(rowIds: number[]): ng.IPromise<any>;
    createCashPaymentsFromInvoice(cashPayment: any): ng.IPromise<any>;
    savePriceBasedMarkup(priceBasedMarkup: PriceBasedMarkupDTO): ng.IPromise<any>;
    saveHouseHoldTaxRowForEdit(item: HouseholdTaxDeductionApplicantDTO): ng.IPromise<any>;
    saveHouseholdTaxPartiallyApproved(id: number, amount: number, receivedDate: Date): ng.IPromise<any>;
    downloadHouseHoldTaxFile(items: HouseholdTaxDeductionFileRowDTO[], taxDeductionType: TermGroup_HouseHoldTaxDeductionType, sequenceNumber: number): ng.IPromise<any>;

    // DELETE
    deleteDeliveryCondition(deliveryConditionId: number): ng.IPromise<any>;
    deleteDeliveryType(deliveryTypeId: number): ng.IPromise<any>;
    deletePaymentCondition(paymentConditionId: number): ng.IPromise<any>;
    deletePriceList(priceListId: number): ng.IPromise<any>;
    deletePriceRule(priceRuleId: number): ng.IPromise<any>;
    deleteProductGroup(productGroupId: number): ng.IPromise<any>;
    deleteInvoice(invoiceId: number, deleteProject: boolean, restoreRowStatus: boolean, isContract: boolean): ng.IPromise<any>;
    deleteSupplierAgreement(wholesellerId: number, priceListTypeId: number): ng.IPromise<any>;
    deleteCompPriceList(priceListImportedHeadId: number): ng.IPromise<any>;
    deleteCompanyWholeseller(wholesellerId: number): ng.IPromise<any>;
    deleteHouseholdTaxDeductionRow(rowId: number): ng.IPromise<any>;
    deletePriceBasedMarkup(priceBasedMarkupId: number): ng.IPromise<any>;
    withdrawReceived(customerInvoiceRowId: number): ng.IPromise<any>;
}

export class InvoiceService implements IInvoiceService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getDeliveryConditions(useCache: boolean = true) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_CONDITION, null, Constants.CACHE_EXPIRE_LONG);
    }

    getDeliveryConditionsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_CONDITION + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getDeliveryCondition(deliveryConditionId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_CONDITION + deliveryConditionId, false);
    }

    getDeliveryTypes() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_TYPE, null);
    }

    getDeliveryTypesDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_TYPE + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getDeliveryType(deliveryTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_TYPE + deliveryTypeId, false);
    }

    getEInvoiceEntry(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_EINVOICEENTRY + invoiceId, false);
    }
    getInvoice(invoiceId: number, includeCategories: boolean, includeRows: boolean): ng.IPromise<BillingInvoiceDTO> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE + invoiceId + "/" + includeCategories + "/" + includeRows, false);
    }

    getCashCustomerId() {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_CASHCUSTOMER, false);
    }

    getVoucherTraceViews(voucherHeadId: number): ng.IPromise<IVoucherTraceViewDTO> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_VOUCHER_GETVOUCHERTRACEVIEWS + voucherHeadId, false);
    }

    getPaymentConditions() {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, null, Constants.CACHE_EXPIRE_LONG);
    }

    getPaymentConditionsGrid() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getPaymentConditionsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getPaymentCondition(paymentConditionId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION + paymentConditionId, false);
    }

    getPriceLists() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, false);
    }

    getPriceListsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getPriceListsGrid() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getPriceList(priceListId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST + priceListId, false);
    }

    getCompanyPriceRules() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_PRICE_RULE, false);
    }

    GetPriceListTypeMarkups() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST_TYPE_MARKUPS, false);
    }

    getPriceRule(priceRuleId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_RULE + priceRuleId, false);
    }

    getProductGroups(addEmptyRow: boolean, refreshCache = false) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PRODUCT_GROUP + addEmptyRow, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_LONG, refreshCache);
    }

    getProductGroup(productGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRODUCT_GROUP + productGroupId, false);
    }

    getProjects(onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean, projectStatus: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT + onlyActive + "/" + hidden + "/" + setStatusName + "/" + includeManagerName + "/" + loadOrders + "/" + projectStatus, false);
    }

    getProjectsSmall(onlyActive: boolean, hidden: boolean, sortOnNumber: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_SMALL + onlyActive + "/" + hidden + "/" + sortOnNumber, false);
    }

    getProjectsForList(projectStatus: number, loadMine: boolean): ng.IPromise<IProjectGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_LIST + projectStatus + "/" + loadMine, false);
    }

    getProject(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT + projectId, false);
    }

    getProjectGridDTO(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_GRIDDTO + projectId, false);
    }

    getInvoiceFromOrderCheckLists(invoiceId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_CHECKLISTS + invoiceId, false);
    }

    getSysWholesellersDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_SYS_WHOLESELLERS + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getSysWholesellersByCompanyDict(onlyNotUsed: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_SYS_WHOLESELLERS_BY_COMPANY + onlyNotUsed + "/" + addEmptyRow, false);
    }

    getSysWholeseller(sysWholesellerId: number, loadSysWholesellerEdi: boolean, loadSysEdiMsg: boolean, loadSysEdiType: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_SYS_WHOLESELLER + sysWholesellerId + "/" + loadSysWholesellerEdi + "/" + loadSysEdiMsg + "/" + loadSysEdiType, false);
    }

    getSupplierAgreements(providerType:number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_SUPPLIERAGREEMENTS + "/" + providerType, false);
    }
    getSupplierAgreementProviders() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_SUPPLIERAGREEMENTPROVIDERS, false);
    }

    getCompanyWholesellerPriceLists(isUsed: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER_PRICELISTS + "/" + isUsed, false);
    }

    getCompanyWholeseller(companyWholesellerId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER + companyWholesellerId, false);
    }

    getCompanyWholesellers() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLERS, false);
    }

    getSupplierBySysWholeseller(sysWholesellerId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_SUPPLIERBYSYSWHOLESELLER + sysWholesellerId, false);
    }

    getPriceListsToUpdate() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER_PRICELISTS_TOUPDATE, false);
    }

    getSysPricelistCodeBySysWholesellerId(wholesellerIds: number[]) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_COMPANY_SYSPRICELIST_CODES + "?swIds=" + wholesellerIds.join(','), false);
    }

    getMarkup(isDiscount: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_MARKUP + isDiscount, false);
    }

    getDiscount(sysWholesellerId: number, code: string) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_MARKUP_DISCOUNT + sysWholesellerId + "/" + (code.length === 0 ? "null" : code), false);
    }

    getInvoiceTemplates() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_TEMPLATES, false);
    }

    getHouseHoldTaxRowInfo(invoiceId: number, customerInvoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_ROWINFO + invoiceId + "/" + customerInvoiceId, false);
    }

    getHouseholdTaxDeductionRows(classificationGroup: number, taxDeductionType: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD + classificationGroup + "/" + taxDeductionType, false);
    }
    getPriceBasedMarkup() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_MARKUP_PRICEBASED, false);
    }
    getHouseHoldTaxRowForEdit(customerInvoiceRowId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_ROWEDIT + customerInvoiceRowId, false);
    }

    getHouseHoldTaxFileForEdit(ids: number[]) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_FILEEDIT, ids);
    }

    // POST
    saveDeliveryCondition(deliveryCondition: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_CONDITION, deliveryCondition);
    }

    saveDeliveryType(deliveryType: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_TYPE, deliveryType);
    }

    saveInvoice(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], checklistHeads: ChecklistHeadRecordCompactDTO[], checklistRows: ChecklistExtendedRowDTO[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean, crediting: boolean): ng.IPromise<any> {
        const model = {
            modifiedFields: modifiedFields,
            newRows: newRows,
            modifiedRows: modifiedRows,
            checklistHeads: checklistHeads,
            checklistRows: checklistRows,
            originUsers: originUsers,
            files: files,
            discardConcurrencyCheck: discardConcurrencyCheck,
            crediting: crediting,
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE, model);
    }

    savePaymentCondition(paymentCondition: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, paymentCondition);
    }

    savePriceList(priceList: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, priceList);
    }

    savePriceRule(priceRule: PriceRuleDTO) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PRICE_RULE, priceRule);
    }

    saveProductGroup(productGroup: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PRODUCT_GROUP, productGroup);
    }

    updateProjectStatus(ids: number[], newStatus: number) {
        const model = {
            Ids: ids,
            NewState: newStatus
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PROJECT, model);
    }

    createEInvoice(invoiceId: number, download: boolean, overrideFinvoiceOperatorWarning: boolean) {
        const model = { InvoiceId: invoiceId, download: download, OverrideFinvoiceOperatorWarning: overrideFinvoiceOperatorWarning };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_CREATEEINVOICE, model);
    }

    saveSupplierAgreement(wholesellerId: number, priceListTypeId: number, generalDiscount: number, bytes: any[]) {
        const model = {
            wholesellerId: wholesellerId,
            priceListTypeId: priceListTypeId,
            generalDiscount: generalDiscount,
            bytes: bytes,
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_SUPPLIERAGREEMENTS, model);
    }

    saveCompanyWholesellerSetting(companyWholesellerDTO: CompanyWholesellerDTO, customerNbrs: string[], actorSupplierId: number) {
        const model = {
            CompanyWholesellerDTO: companyWholesellerDTO,
            CustomerNbrs: customerNbrs,
            ActorSupplierId: actorSupplierId
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER, model);
    }

    saveSupplierAgreementDiscount(dto: ISupplierAgreementDTO) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_SUPPLIERAGREEMENT_DISCOUNT, dto);
    }

    saveCompanyWholesellerPriceLists(wholesellerPriceLists: CompanyWholesellerPriceListViewDTO[]) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER_PRICELISTS, wholesellerPriceLists);
    }

    upgradeCompanyWholesellerPriceLists(sysWholesellerIds: number[]) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER_UPGRADE_PRICELISTS, sysWholesellerIds);
    }

    saveMarkup(markupRows: MarkupDTO[]) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_MARKUP, markupRows);
    }

    validatePriceRule(widgets: FormulaWidget[]): ng.IPromise<IActionResult> {
        const input = {
            items: []
        };

        let sort: number = 1;
        _.forEach(widgets, widget => {
            input.items.push({
                sort: sort++,
                priceRuleType: widget.priceRuleType,
                data: widget.data
            });
        });

        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PRICE_RULE_VALIDATE, input);
    }

    saveHouseholdTaxReceived(rowIds: number[], receivedDate: Date) {
        const model = {
            idsToUpdate: rowIds,
            bulkDate: receivedDate
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_RECEIVED, model);
    }

    saveHouseholdTaxApplied(rowIds: number[], appliedDate: Date) {
        const model = {
            idsToUpdate: rowIds,
            bulkDate: appliedDate
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_APPLIED, model);
    }

    saveHouseholdTaxDenied(invoiceId: number, rowId: number, deniedDate: Date) {
        const model = {
            customerInvoiceId: invoiceId,
            customerInvoiceRowId: rowId,
            bulkDate: deniedDate
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_DENIED, model);
    }

    saveHouseholdTaxWithdrawn(rowIds: number[]) {
        const model = {
            idsToUpdate: rowIds,
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_WITHDRAWAPPLIED, model);
    }

    createCashPaymentsFromInvoice(cashPayment: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CASHPAYMENT, cashPayment);
    }

    savePriceBasedMarkup(priceBasedMarkup: PriceBasedMarkupDTO) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_MARKUP_PRICEBASED, priceBasedMarkup);
    }

    saveHouseHoldTaxRowForEdit(item: HouseholdTaxDeductionApplicantDTO) {
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_ROWEDIT, item);
    }

    saveHouseholdTaxPartiallyApproved(id: number, amount: number, receivedDate: Date) {
        const model = {
            idsToUpdate: [id],
            amount: amount, 
            bulkDate: receivedDate
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_RECEIVED_PARTIALLY, model);
    }

    downloadHouseHoldTaxFile(items: HouseholdTaxDeductionFileRowDTO[], taxDeductionType: TermGroup_HouseHoldTaxDeductionType, sequenceNumber: number) {
        const model = {
            applications: items,
            seqNr: sequenceNumber,
            type: taxDeductionType
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_FILEDOWNLOAD, model);
    }

    // DELETE
    deleteDeliveryCondition(deliveryConditionId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_CONDITION + deliveryConditionId);
    }

    deleteDeliveryType(deliveryTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_DELIVERY_TYPE + deliveryTypeId);
    }

    deletePaymentCondition(paymentConditionId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION + paymentConditionId);
    }

    deletePriceList(priceListTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST + priceListTypeId);
    }

    deletePriceRule(priceRuleId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_PRICE_RULE + priceRuleId);
    }

    deleteProductGroup(productGroupId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_PRODUCT_GROUP + productGroupId);
    }

    deleteInvoice(invoiceId: number, deleteProject: boolean, restoreRowStatus: boolean, isContract: boolean) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE + invoiceId + "/" + deleteProject + "/" + restoreRowStatus + "/" + isContract);
    }

    deleteSupplierAgreement(wholesellerId: number, priceListTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_SUPPLIERAGREEMENTS + wholesellerId + "/" + priceListTypeId);
    }

    deleteCompPriceList(priceListImportedHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER_PRICELIST_DELETE + priceListImportedHeadId);
    }

    deleteCompanyWholeseller(wholesellerId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_COMPANY_WHOLESELLER + wholesellerId);
    }

    deleteHouseholdTaxDeductionRow(rowId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_DELETE + rowId);
    }

    deletePriceBasedMarkup(priceBasedMarkupId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_MARKUP_PRICEBASED + priceBasedMarkupId);
    }

    withdrawReceived(customerInvoiceRowId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_WITHDRAWRECEIVED + customerInvoiceRowId);
    }
}
