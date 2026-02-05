import { IHttpService } from "./HttpService";
import { IReportDTO, IReportSmallDTO, IActionResult, IReportItemDTO, ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { ProjectTransactionsReportDTO, ReportMenuDTO, ReportTemplateDTO, ReportViewDTO, ReportUserSelectionDTO, ReportViewGridDTO, SysReportTemplateViewGridDTO, ReportJobStatusDTO, ReportDTO } from "../../Common/Models/ReportDTOs";
import { SettingMainType, CompanySettingType, SoeReportTemplateType, OrderInvoiceRegistrationType, DatePeriodType, SoeReportType, ReportUserSelectionType, SoeModule } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { filter } from "lodash";

export interface IReportService {

    // GET

    getStandardReport(settingMainType: SettingMainType, settingType: number, reportTemplateType: number): ng.IPromise<any>;
    getStandardReportId(settingMainType: SettingMainType, settingType: number, reportTemplateType: number): ng.IPromise<any>;
    getSettingOrStandardReportId(settingMainType: SettingMainType, settingType: number, reportTemplateType: number, reportType: number): ng.IPromise<any>;
    getSettingReportCheckPermission(settingMainType: SettingMainType, settingType: CompanySettingType, reportTemplateType: SoeReportTemplateType): ng.IPromise<any>;
    getSettingReportHasPermission(settingMainType: SettingMainType, settingType: CompanySettingType, reportTemplateType: SoeReportTemplateType): ng.IPromise<boolean>;
    getReportGroupByModule(module: number): ng.IPromise<any>;
    getReportHeadersForModule(module: number, loadReportHeaderInterval: boolean): ng.IPromise<any>;
    getReportPackagesForModule(module: number, loadReport: boolean): ng.IPromise<any>;
    getReportPrintUrl(sysReportTemplateTypeId: number, id: number): ng.IPromise<any>;
    getReportViewsForModule(module: number, onlyOriginal: boolean, onlyStandard: boolean): ng.IPromise<ReportViewGridDTO[]>;
    getReportViewsInPackage(reportPackageId: number): ng.IPromise<any>;
    getReportsDict(sysReportTemplateTypeId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean): ng.IPromise<any>;
    getReport(reportId: number): ng.IPromise<IReportDTO>;
    getReportSmall(reportId: number): ng.IPromise<IReportSmallDTO>;
    getReportsForMenu(module: number, sysReportType: SoeReportType): ng.IPromise<ReportMenuDTO[]>;
    getReportItem(reportId: number, sysReportType: SoeReportType): ng.IPromise<IReportItemDTO>;
    getPrintedReportForMenu(reportPrintoutId: number): ng.IPromise<ReportMenuDTO>;
    getPrintedXmlForMenu(reportPrintoutId: number): ng.IPromise<string>;
    getReportSelectionFromReportPrintout(reportPrintoutId: number): ng.IPromise<ReportUserSelectionDTO>;
    getReportUserSelections(reportId: number, type: ReportUserSelectionType): ng.IPromise<ISmallGenericType[]>;
    getReportUserSelection(reportUserSelectionId: number): ng.IPromise<ReportUserSelectionDTO>;
    getReportExportTypes(sysReportTemplateId: number, userReportTemplateId: number, sysReportType: SoeReportType);
    getReportJobsStatus(reportPrintoutIds: number[], showDetails: boolean): ng.IPromise<ReportJobStatusDTO[]>;
    getExportTypes(): ng.IPromise<ISmallGenericType[]>;
    getSysReportTemplateTypesForModule(module: number): ng.IPromise<ISmallGenericType[]>;
    getSysReportTemplatesForModule(module: number, filterOnCountry: boolean): ng.IPromise<SysReportTemplateViewGridDTO[]>;
    getUserReportTemplatesForModule(module: number): ng.IPromise<any>;
    getReportTemplate(reportTemplateId: number, isSystem: boolean): ng.IPromise<any>;
    getDrilldownReportsDict(onlyOriginal: boolean, onlyStandard: boolean): ng.IPromise<any>;
    getDrilldownReport(reportId: number, accountPeriodIdFrom: number, accountPeriodIdTo: number, budgetHeadId: number): ng.IPromise<any>;
    getReportsForType(templateTypeIds: number[], onlyOriginal: boolean, onlyStandard: boolean, module?: SoeModule): ng.IPromise<ReportViewDTO[]>;
    getInvoiceReminderUrl(customerInvoiceIds: number[]);
    getInvoiceInterestUrl(customerInvoiceIds: number[]);
    //getOrderPrintUrl(invoiceId: number, emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any);
    getProjectTransactionsPrintUrl(reportItem: ProjectTransactionsReportDTO);
    getReportForPrint(reportId: number, loadReportSelection: boolean, loadSysReportTemplateType: boolean, loadReportRolePermission: boolean, loadSettings: boolean, loadSysReportTemplateSettings: boolean): ng.IPromise<any>;
    getProjectsBySearch(setStatusName: boolean, statusIds: number[], categoryIds: number[], stopDate: Date, withoutStopDate: boolean): ng.IPromise<any[]>;
    getChecklistReportURL(invoiceId: number, recordId: number, reportId: number): ng.IPromise<any>;
    getOrderPrintUrl(invoiceIds: number[], emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any, actorCustomerId: number, orderInvoiceRegistrationType: OrderInvoiceRegistrationType, invoiceCopy: boolean);
    getOrderPrintUrlSingle(invoiceId: number, emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any, actorCustomerId: number, printTimeReport: boolean, includeOnlyInvoiced: boolean, orderInvoiceRegistrationType: OrderInvoiceRegistrationType, invoiceCopy: boolean, emailTemplateId: number, asReminder: boolean);
    getPurchaseOrderPrintUrl(purchaseIds: number[], emailRecipients: number[], reportId: number, languageId: number);
    getReportsFromFile(dataStorageId: number);
    getHouseholdTaxDeductionPrintUrl(customerInvoiceRowIds: number[], reportId: number, templateType: number, sequenceNumber: number, useGreen: boolean);
    getEDistributionItems(originType: number, type: number, allItemsSelection: number): ng.IPromise<any>;
    getScheduledJobHeads(): ng.IPromise<ISmallGenericType[]>;
    getSalesEU(startDate: Date, stopDate: Date): ng.IPromise<any>;
    getSalesEUDetails(actorId: number, startDate: Date, stopDate: Date): ng.IPromise<any>;
    getSalesEUExportData(period: DatePeriodType, startDate: Date, stopDate: Date): ng.IPromise<any>;
    getProductListReportUrl(productIds: any[], reportId: number, sysReportTemplateTypeId: number): ng.IPromise<any>;
    getCompaniesByLicense(licenseId: string, onlyTemplates: boolean): ng.IPromise<any>;
    getGlobalTemplateCompanies(): ng.IPromise<any>;
    GetReportsByTemplateTypeDict(selectedImportCompanyId:number, sysReportTemplateId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean): ng.IPromise<any>;
    GetSysReportTemplateType(report: ReportDTO): ng.IPromise<any>;
    getInventoryCategories(): ng.IPromise<any>;

    // POST
    authAltInn(user: any): ng.IPromise<any>;
    getTimeEmployeeSchedulePrintUrl(employeeIds: number[], shiftTypeIds: number[], dateFrom: Date, dateTo: Date, reportId: number, reportTemplateType: SoeReportTemplateType);
    getTimeScheduleTasksAndDeliverysReportPrintUrl(timeScheduleTaskIds: number[], timeScheduleDeliveryHeadIds: number[], dateFrom: Date, dateTo: Date, isDayView: boolean);
    sendReport(invoiceId: number, emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any, actorCustomerId: number, printTimeReport: boolean, includeOnlyInvoiced: boolean, orderInvoiceRegistrationType: OrderInvoiceRegistrationType, invoiceCopy: boolean, emailTemplateId: number, addAttachmentsToEinvoice: boolean, attachmentIds: number[], checklistIds: number[], mergePdfs: boolean, singleRecipient: string): ng.IPromise<any>;
    saveReport(report: IReportDTO): ng.IPromise<IActionResult>;
    saveReportUrl(guid: string, url: string, reportId: number, sysReportTemplateTypeId: number): ng.IPromise<any>;
    saveReportTemplate(reportTemplate: ReportTemplateDTO, templateData: any, isSystem: boolean): ng.IPromise<any>;
    deleteReportTemplate(reportTemplate: ReportTemplateDTO): ng.IPromise<any>;
    deleteReportGroups(ids: number[]): ng.IPromise<any>;
    deleteReportHeaders(ids: number[]): ng.IPromise<any>;
    getDrilldownVoucherRows(dto: any): ng.IPromise<any>;
    saveReportUserSelection(reportUserSelection: ReportUserSelectionDTO): ng.IPromise<IActionResult>;

    // DELETE
    deleteReport(reportId: number): ng.IPromise<IActionResult>;
    deletePrintedReport(reportPrintoutId: number): ng.IPromise<IActionResult>;
    deleteReportUserSelection(reportUserSelectionId: number): ng.IPromise<IActionResult>;
}

export class ReportService implements IReportService {
    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getStandardReport(settingMainType: number, settingType: number, reportTemplateType: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_STANDARD_REPORT + settingMainType + "/" + settingType + "/" + reportTemplateType, true);
    }

    getStandardReportId(settingMainType: number, settingType: number, reportTemplateType: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_STANDARD_REPORT_ID + settingMainType + "/" + settingType + "/" + reportTemplateType, true);
    }

    getSettingOrStandardReportId(settingMainType: SettingMainType, settingType: number, reportTemplateType: number, reportType: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_SETTING_OR_STANDARD_REPORT_ID + settingMainType + "/" + settingType + "/" + reportTemplateType + "/" + reportType, true);
    }

    getSettingReportCheckPermission(settingMainType: number, settingType: number, reportTemplateType: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_SETTING_REPORT_CHECK_PERMISSION + settingMainType + "/" + settingType + "/" + reportTemplateType, true);
    }

    getSettingReportHasPermission(settingMainType: number, settingType: number, reportTemplateType: number): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_REPORT_SETTING_REPORT_HAS_PERMISSION + settingMainType + "/" + settingType + "/" + reportTemplateType, true);
    }

    getOrderPrintUrl(invoiceIds: number[], emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any, actorCustomerId: number, orderInvoiceRegistrationType: OrderInvoiceRegistrationType, invoiceCopy: boolean = false) {
        const model = { invoiceIds: invoiceIds, emailRecipients: emailRecipients, reportId: reportId, languageId: languageId, invoiceNr: invoiceNr, actorCustomerId: actorCustomerId, registrationType: orderInvoiceRegistrationType, invoiceCopy: invoiceCopy };
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_ORDER_URL, model);
    }

    getOrderPrintUrlSingle(invoiceId: number, emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any, actorCustomerId: number, printTimeReport: boolean, includeOnlyInvoiced: boolean, orderInvoiceRegistrationType: OrderInvoiceRegistrationType, invoiceCopy: boolean = false, emailTemplateId: number = 0, asReminder: boolean = false) {
        const model = { invoiceId: invoiceId, emailRecipients: emailRecipients, reportId: reportId, languageId: languageId, invoiceNr: invoiceNr, actorCustomerId: actorCustomerId, printTimeReport: printTimeReport, includeOnlyInvoicedTime: includeOnlyInvoiced, registrationType: orderInvoiceRegistrationType, invoiceCopy: invoiceCopy, emailTemplateId: emailTemplateId, asReminder: asReminder };
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_ORDER_URL_SINGLE, model);
    }

    getPurchaseOrderPrintUrl(purchaseIds: number[], emailRecipients: number[], reportId: number, languageId: number) {
        const model = { purchaseIds, emailRecipients, reportId, languageId }
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_PURCHASE_URL, model)
    }

    getProjectTransactionsPrintUrl(reportItem: ProjectTransactionsReportDTO) {

        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_PROJECTTRANSACTIONS_URL, reportItem);
    }

    getInvoiceReminderUrl(customerInvoiceIds: number[]) {
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_REMINDER_URL, customerInvoiceIds);
    }

    getInvoiceInterestUrl(customerInvoiceIds: number[]) {
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_INTEREST_URL, customerInvoiceIds);
    }

    getChecklistReportURL(invoiceId: number, recordId: number, reportId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_PRINT_CHECKLIST_URL + invoiceId + "/" + recordId + "/" + reportId, false);
    }

    getReportGroupByModule(module: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_GROUP_BY_MODULE + module, false);
    }

    getReportHeadersForModule(module: number, loadReportHeaderInterval: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_HEADER_BY_MODULE + module + "/" + loadReportHeaderInterval, false);
    }

    getReportPackagesForModule(module: number, loadReport: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_PACKAGE + module + "/" + loadReport, false);
    }

    getReportPrintUrl(sysReportTemplateTypeId: number, id: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_PRINT_URL + sysReportTemplateTypeId + "/" + id, false);
    }

    getReportViewsForModule(module: number, onlyOriginal: boolean, onlyStandard: boolean): ng.IPromise<ReportViewGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTS + module + "/" + onlyOriginal + "/" + onlyStandard, false).then(x => {
            return x.map(y => {
                let obj = new ReportViewGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getReportViewsInPackage(reportPackageId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTS_IN_PACKAGE + reportPackageId, false);
    }

    getReportsDict(sysReportTemplateTypeId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_REPORT_REPORTS + sysReportTemplateTypeId + "/" + onlyOriginal + "/" + onlyStandard + "/" + addEmptyRow + "/" + useRole, null, Constants.CACHE_EXPIRE_LONG);
    }

    getReport(reportId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT + reportId, true);
    }

    getReportSmall(reportId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT + reportId, true, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getReportsForMenu(module: number, sysReportType: SoeReportType): ng.IPromise<ReportMenuDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTS_FOR_MENU + module + "/" + sysReportType, false).then(x => {
            return x.map(y => {
                let obj = new ReportMenuDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getReportItem(reportId: number, sysReportType: SoeReportType): ng.IPromise<IReportItemDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM + reportId + "/" + sysReportType, false);
    }

    getPrintedReportForMenu(reportPrintoutId: number): ng.IPromise<ReportMenuDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_PRINTED_REPORT + reportPrintoutId, false).then(x => {
            let obj = new ReportMenuDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getPrintedXmlForMenu(reportPrintoutId: number): ng.IPromise<string> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_PRINTED_XML + reportPrintoutId, false);
    }

    getReportSelectionFromReportPrintout(reportPrintoutId: number): ng.IPromise<ReportUserSelectionDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_SELECTION_FROM_REPORT_PRINTOUT + reportPrintoutId, false).then(x => {
            let obj = new ReportUserSelectionDTO();
            angular.extend(obj, x);
            obj.setTypes();
            obj.setSelectionTypes();
            return obj;
        });
    }

    getReportUserSelections(reportId: number, type: ReportUserSelectionType): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_SELECTIONS + reportId + "/" + type, false);
    }

    getReportUserSelection(reportUserSelectionId: number): ng.IPromise<ReportUserSelectionDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_SELECTION + reportUserSelectionId, false).then(x => {
            let obj = new ReportUserSelectionDTO();
            angular.extend(obj, x);
            obj.setTypes();
            obj.setSelectionTypes();
            return obj;
        });
    }

    getReportExportTypes(sysReportTemplateId: number, userReportTemplateId: number, sysReportType: SoeReportType) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_EXPORTTYPES + sysReportTemplateId + "/" + userReportTemplateId + "/" + sysReportType, false);
    }

    getReportJobsStatus(reportPrintoutIds: number[], showDetails: boolean): ng.IPromise<ReportJobStatusDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU + (reportPrintoutIds && reportPrintoutIds.length > 0 ? reportPrintoutIds.join(',') : Constants.WEBAPI_STRING_EMPTY) + "/" + showDetails, false).then(x => {
            return x.map(y => {
                let obj = new ReportJobStatusDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getScheduledJobHeads(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_SCHEDULEDJOBHEADS, false);
    }

    getExportTypes() {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTTEMPLATE_EXPORTTYPES, false);
    }

    getSysReportTemplateTypesForModule(module: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_SYSREPORTTEMPLATETYPES + module, false);
    }

    getSysReportTemplatesForModule(module: number, filterOnCountry: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_SYSREPORTTEMPLATES + module + "/" + filterOnCountry, false).then(x => {
            return x.map(y => {
                let obj = new SysReportTemplateViewGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getUserReportTemplatesForModule(module: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_USERREPORTTEMPLATES + module, false);
    }

    getReportTemplate(reportTemplateId: number, isSystem: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTTEMPLATE + reportTemplateId + "/" + isSystem, false);
    }

    getDrilldownReportsDict(onlyOriginal: boolean, onlyStandard: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_DRILLDOWNREPORTS + onlyOriginal + "/" + onlyStandard, false);
    }

    getDrilldownReport(reportId: number, accountPeriodIdFrom: number, accountPeriodIdTo: number, budgetHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_DRILLDOWNREPORTS_REPORT + reportId + "/" + accountPeriodIdFrom + "/" + accountPeriodIdTo + "/" + budgetHeadId, false);
    }

    getDrilldownVoucherRows(dto: any) {
        return this.httpService.post(Constants.WEBAPI_REPORT_DRILLDOWNREPORT_VOUCHERROWS, dto);
    }

    getReportsForType(templateTypeIds: number[], onlyOriginal: boolean, onlyStandard: boolean, module: SoeModule = null) {
        const model = {
            reportTemplateTypeIds: templateTypeIds,
            onlyOriginal: onlyOriginal,
            onlyStandard: onlyStandard,
            module: module
        };

        return this.httpService.post(Constants.WEBAPI_REPORT_REPORTS_FOR_TYPES, model).then(x => {
            return x.map(y => {
                let obj = new ReportViewDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getReportForPrint(reportId: number, loadReportSelection: boolean, loadSysReportTemplateType: boolean, loadReportRolePermission: boolean, loadSettings: boolean, loadSysReportTemplateSettings: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORT + reportId + "/" + loadReportSelection + "/" + loadSysReportTemplateType + "/" + loadReportRolePermission + "/" + loadSettings + "/" + loadSysReportTemplateSettings, false);
    }

    getProjectsBySearch(setStatusName: boolean, statusIds: number[], categoryIds: number[], stopDate: Date, withoutStopDate: boolean) {
        const model = {
            statusIds: statusIds,
            categoryIds: categoryIds,
            setStatusName: setStatusName,
            stopDate: stopDate,
            withoutStopDate: withoutStopDate
        };
        return this.httpService.post(Constants.WEBAPI_REPORT_PROJECT_SEARCH, model);
    }

    getReportsFromFile(dataStorageId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_IMPORT_REPORTS + dataStorageId, false);
    }

    getHouseholdTaxDeductionPrintUrl(customerInvoiceRowIds: number[], reportId: number, templateType: number, sequenceNumber: number, useGreen: boolean) {
        const model = { customerInvoiceRowIds: customerInvoiceRowIds, reportId: reportId, sysReportTemplateTypeId: templateType, nextSequenceNumber: sequenceNumber, useGreen: useGreen };
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_HOUSEHOLDTAXDEDUCTION, model);
    }

    getEDistributionItems(originType: number, type: number, allItemsSelection: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_REPORT_EDISTRIBUTIONITEMS + originType + "/" + type + "/" + allItemsSelection, false);
    }

    getSalesEU(startDate: Date, stopDate: Date): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_REPORT_SALES_EU + startDate.toDateTimeString() + "/" + stopDate.toDateTimeString(), false);
    }
    getSalesEUDetails(actorId: number, startDate: Date, stopDate: Date): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_REPORT_SALES_EU_DETAILS + actorId + "/" + startDate.toDateTimeString() + "/" + stopDate.toDateTimeString(), false);
    }
    getSalesEUExportData(period: DatePeriodType, startDate: Date, stopDate: Date): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_REPORT_SALES_EU_EXPORT_FILE + period + "/" + startDate.toDateTimeString() + "/" + stopDate.toDateTimeString(), false);
    }

    getProductListReportUrl(productIds: number[], reportId: number, sysReportTemplateTypeId: number) {
        const model = { productIds: productIds, reportId: reportId, sysReportTemplateTypeId: sysReportTemplateTypeId }
        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_PRODUCTLIST_PRINT_URL, model);
    }

    getCompaniesByLicense(licenseId: string, onlyTemplates: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_COMPANIES_BY_LICENSE + licenseId + "/" + onlyTemplates, false);
    }

    getGlobalTemplateCompanies() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_TEMPLATE_GLOBAL_COMPANIES, false);
    }

    GetReportsByTemplateTypeDict(selectedImportCompanyId: number, sysReportTemplateId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTS + selectedImportCompanyId + "/"+ sysReportTemplateId + "/" + onlyOriginal + "/" + onlyStandard + "/" + addEmptyRow + "/" + useRole, false);
    }

    GetSysReportTemplateType(report: ReportDTO): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_REPORT_GET_SYS_REPORT_TEMPLATE_TYPE + report.reportTemplateId + "/" + report.standard, false);
    }

    getInventoryCategories(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_CATEGORIES, false);
    }

    // POST

    authAltInn(user: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_REPORT_ALTINN_LOGIN, user);
    }

    getTimeEmployeeSchedulePrintUrl(employeeIds: number[], shiftTypeIds: number[], dateFrom: Date, dateTo: Date, reportId: number, reportTemplateType: SoeReportTemplateType) {
        const model = {
            employeeIds: employeeIds,
            shiftTypeIds: shiftTypeIds,
            dateFrom: dateFrom,
            dateTo: dateTo,
            reportId: reportId,
            reportTemplateType: reportTemplateType
        };

        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_EMPLOYEE_SCHEDULE_URL, model);
    }

    getTimeScheduleTasksAndDeliverysReportPrintUrl(timeScheduleTaskIds: number[], timeScheduleDeliveryHeadIds: number[], dateFrom: Date, dateTo: Date, isDayView: boolean) {
        const model = {
            timeScheduleTaskIds: timeScheduleTaskIds,
            timeScheduleDeliveryHeadIds: timeScheduleDeliveryHeadIds,
            dateFrom: dateFrom,
            dateTo: dateTo,
            isDayView: isDayView,
        };

        return this.httpService.post(Constants.WEBAPI_REPORT_PRINT_TIME_SCHEDULE_TASKS_AND_DELIVERIES_REPORT_URL, model);
    }

    saveReportUrl(guid: string, url: string, reportId: number, sysReportTemplateTypeId: number) {
        const model = { guid: guid, url: url, reportId: reportId, sysReportTemplateTypeId: sysReportTemplateTypeId };
        return this.httpService.post(Constants.WEBAPI_REPORT_SAVE_REPORT_URL, model);
    }

    saveReport(report: IReportDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_REPORT_SAVE_REPORT, report);
    }

    saveReportTemplate(reportTemplate: ReportTemplateDTO, templateData: any, isSystem: boolean) {
        const model = { reportTemplate: reportTemplate, templateData: templateData, isSystem: isSystem };
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORTTEMPLATE_SAVE, model);
    }

    deleteReportTemplate(reportTemplate: ReportTemplateDTO) {
        const model = { reportTemplate: reportTemplate };
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORTTEMPLATE_DELETE, model);
    }

    sendReport(invoiceId: number, emailRecipients: number[], reportId: number, languageId: number, invoiceNr: any, actorCustomerId: number, printTimeReport: boolean, includeOnlyInvoiced: boolean, orderInvoiceRegistrationType: OrderInvoiceRegistrationType, invoiceCopy: boolean, emailTemplateId: number, addAttachmentsToEinvoice: boolean, attachmentIds: number[], checklistIds: number[], mergePdfs: boolean, singleRecipient: string): ng.IPromise<any> {
        const model = {
            invoiceId: invoiceId, emailRecipients: emailRecipients, reportId: reportId, languageId: languageId, invoiceNr: invoiceNr, actorCustomerId: actorCustomerId, printTimeReport: printTimeReport, includeOnlyInvoicedTime: includeOnlyInvoiced, registrationType: orderInvoiceRegistrationType,
            invoiceCopy: invoiceCopy, emailTemplateId: emailTemplateId, addAttachmentsToEinvoice: addAttachmentsToEinvoice, attachmentIds: attachmentIds, checklistIds: checklistIds, mergePdfs: mergePdfs, singleRecipient: singleRecipient
        };
        return this.httpService.post(Constants.WEBAPI_REPORT_SEND_REPORT_URL, model);
    }

    deleteReportGroups(ids: number[]) {
        var model = { reportGroupIds: ids }
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_GROUP_DELETE, model);
    }

    deleteReportHeaders(ids: number[]) {
        var model = { reportHeaderIds: ids }
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_HEADER_DELETE, model);
    }

    saveReportUserSelection(reportUserSelection: ReportUserSelectionDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_SELECTION, reportUserSelection);
    }

    // DELETE

    deleteReport(reportId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_REPORT_REPORT + reportId);
    }

    deletePrintedReport(reportPrintoutId: number) {
        return this.httpService.delete(Constants.WEBAPI_REPORT_REPORT_JOBS_FOR_MENU + reportPrintoutId);
    }

    deleteReportUserSelection(reportUserSelectionId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_REPORT_REPORT_MENU_ITEM_SELECTION + reportUserSelectionId);
    }
}
