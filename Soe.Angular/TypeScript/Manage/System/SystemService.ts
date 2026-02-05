import { IHttpService } from "../../Core/Services/HttpService";
import { ISysCompanyDTO, ISysCompDBDTO, ISysCompServerDTO, ISysWholesellerDTO, ISysEdiMessageRawDTO, ISysEdiMessageHeadDTO, IActionResult, ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { SysEdiMessageHeadStatus } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";
import { SysTermDTO } from "../../Common/Models/SysTermDTO";
import { SysScheduledJobDTO, SysJobDTO, SysScheduledJobLogDTO } from "../../Common/Models/SysJobDTO";
import { TermGroup_SysPayrollPrice } from "../../Util/CommonEnumerations";
import { SysPayrollPriceDTO, SysPayrollPriceIntervalDTO } from "../../Common/Models/SysPayrollPriceDTO";
import { SysImportDefinitionDTO } from "../../Common/Models/SysImportDTO";
import { InformationDTO, InformationGridDTO, InformationRecipientDTO, SysInformationSysCompDbDTO } from "../../Common/Models/InformationDTOs";
import { Guid } from "../../Util/StringUtility";
import { CommodityCodeDTO } from "../../Common/Models/CommodityCodesDTO";

export interface ISystemService {

    // GET

    getSysCompany(companyApiKey: string): ng.IPromise<any>
    getSysCompanyWithId(sysCompanyId: number): ng.IPromise<any>
    getSysCompanies(): ng.IPromise<any>
    getSysCompDB(sysCompDBId: number): ng.IPromise<any>
    getSysCompDBs(): ng.IPromise<any>
    getSysCompServer(sysCompanyServerId: number): ng.IPromise<any>
    getSysCompServers(): ng.IPromise<any>
    getSysInformations(): ng.IPromise<InformationDTO[]>;
    getSysInformationGrids(): ng.IPromise<InformationGridDTO[]>;
    getSysInformationForEdit(sysInformationId: number): ng.IPromise<InformationDTO>;
    getSysInformationFolders(): ng.IPromise<string[]>;
    getSysInformationSysCompDbs(): ng.IPromise<ISmallGenericType[]>;
    getSysInformationSysFeatures(): ng.IPromise<ISmallGenericType[]>;
    getSysInformationHasConfirmations(sysInformationId: number): ng.IPromise<boolean>;
    getSysInformationRecipientInfo(sysInformationId: number): ng.IPromise<InformationRecipientDTO[]>;
    getSysWholeseller(sysWholesellerId: number): ng.IPromise<any>
    getSysWholesellers(): ng.IPromise<any>
    getSysEdiMessageRaw(sysCompDBId: number): ng.IPromise<any>
    getSysEdiMessageRaws(): ng.IPromise<any>
    getSysEdiMessageHead(sysCompDBId: number): ng.IPromise<any>
    getSysEdiMessageHeads(): ng.IPromise<any>
    getSysEdiMessageGridHeads(sysEdiMessageHeadStatus: SysEdiMessageHeadStatus, take: number, missingSysCompanyId: boolean): ng.IPromise<any>
    getSysEdiMessageHeadMessage(sysEdiMessageHeadId: number): ng.IPromise<any>
    getSoftOneServerUtilityPageStatuses(): ng.IPromise<any>
    getSysPayrollPrices(sysCountryId: number, sysPayrollPrices: TermGroup_SysPayrollPrice[], setName: boolean, setTypeName: boolean, setAmountTypeName: boolean, includeIntervals: boolean, onlyLatest: boolean, date: Date): ng.IPromise<SysPayrollPriceDTO[]>
    getSysPayrollPrice(sysPayrollPriceId: number, includeIntervals: boolean, setName: boolean, setTypeName: boolean, setAmountTypeName: boolean): ng.IPromise<SysPayrollPriceDTO>
    validatePasswordForSysPayrollPrice(password: string): ng.IPromise<boolean>
    getSysVehicleTypes(): ng.IPromise<any>
    getSysVehicleType(sysVehicleTypeId: number): ng.IPromise<any>
    getSysTermGroups(): ng.IPromise<any>
    getSysTerms(sysTermGroupId: number, langId: number, date?: Date): ng.IPromise<any>
    getSysTermSuggestion(text: string, primaryLangId: number, secondaryLangId: number): ng.IPromise<any>
    getScheduledJob(sysScheduledJobId: number, loadSettings: boolean, loadJob: boolean): ng.IPromise<SysScheduledJobDTO>
    getScheduledJobs(): ng.IPromise<SysScheduledJobDTO[]>
    getScheduledJobLog(sysScheduledJobId: number): ng.IPromise<SysScheduledJobLogDTO[]>
    getNoOfActiveScheduledJobs(): ng.IPromise<number>
    getRegisteredJobs(useCache: boolean): ng.IPromise<SysJobDTO[]>
    getEdiEntries(dateFrom: Date, dateTo: Date): ng.IPromise<any>
    getStandardDefinitions(): ng.IPromise<any>
    getSysImportHeadsDict(): ng.IPromise<any>
    getSysImportDefinition(sysImportDefinitionId: number): ng.IPromise<any>
    getSysImportHead(sysImportHeadId: number): ng.IPromise<any>
    getTestCases(): ng.IPromise<any>
    getTestCase(testCaseId: number): ng.IPromise<any>
    getTestCaseGroups(): ng.IPromise<any>
    getTestCaseGroup(testCaseGroupId: number): ng.IPromise<any>
    getTestTracking(trackingGuid: Guid): ng.IPromise<any>
    getTestCaseResultsByTestCaseId(testCaseId: number): ng.IPromise<any>
    getTestCaseResultsByTestCaseGroupId(testCaseGroupId: number): ng.IPromise<any>
    getTestCaseGroupResults(testCaseGroupId: number): ng.IPromise<any>
    getTestCaseGroupOverview(): ng.IPromise<any>
    getTestCaseGroupOverviewByGroup(testCaseGroupId: number): ng.IPromise<any>
    getTestCaseSettings(): ng.IPromise<any>;
    getCommodityCodes(langId:number): ng.IPromise<CommodityCodeDTO[]>


    // POST
    saveSysCompany(sysCompanyDTO: ISysCompanyDTO): ng.IPromise<any>
    saveSysCompDB(sysCompanyDTO: ISysCompDBDTO): ng.IPromise<any>
    saveSysCompServer(sysCompanyDTO: ISysCompServerDTO): ng.IPromise<any>
    saveSysInformation(information: InformationDTO): ng.IPromise<IActionResult>
    saveSysWholeseller(sysCompanyDTO: ISysWholesellerDTO): ng.IPromise<any>
    saveSysEdiMessageRaw(sysEdiMessageRaw: ISysEdiMessageRawDTO): ng.IPromise<any>
    saveSysEdiMessageHead(sysEdiMessageHead: ISysEdiMessageHeadDTO): ng.IPromise<any>
    saveSysPayrollPrice(sysPayrollPrice: SysPayrollPriceDTO): ng.IPromise<IActionResult>
    saveSysVehicleType(file: any): ng.IPromise<any>
    getSysTermSuggestion(text: string, primaryLangId: number, secondaryLangId: number): ng.IPromise<any>
    saveSysTerms(terms: SysTermDTO[]): ng.IPromise<any>
    runScheduledJob(sysScheduledJobId: number): ng.IPromise<any>
    runScheduledJobByService(sysScheduledJobId: number): ng.IPromise<any>
    saveScheduledJob(job: SysScheduledJobDTO): ng.IPromise<IActionResult>
    saveJob(job: SysJobDTO): ng.IPromise<IActionResult>
    updateAttestRuleState(attestRuleHeads: any[]): ng.IPromise<any>;
    saveSysImportDefinition(sysImportDefinition: SysImportDefinitionDTO): ng.IPromise<any>;
    saveTestCase(testCase: any): ng.IPromise<any>
    saveTestCaseGroup(testCaseGroup: any): ng.IPromise<any>
    runTestCaseGroup(testCaseGroupId: number): ng.IPromise<Guid>
    scheduleTestCaseGroupsNow(testCaseGroupIds: number[]): ng.IPromise<any>

    // DELETE
    deleteSysCompany(sysCompanyId: number): ng.IPromise<IActionResult>
    deleteSysInformation(sysInformationId: number): ng.IPromise<IActionResult>
    deleteSysInformations(sysInformationIds: number[]): ng.IPromise<IActionResult>
    deleteSysInformationNotificationSent(sysInformationId: number, sysCompDbId: number): ng.IPromise<IActionResult>
    deleteSysVehicleType(sysVehicleTypeId: number): ng.IPromise<IActionResult>
    deleteSchedueldJob(sysScheduledJobId: number): ng.IPromise<IActionResult>
    deleteJob(sysJobId: number): ng.IPromise<IActionResult>
    deleteSysImportDefinition(sysImportDefinitionId: number): ng.IPromise<IActionResult>;
}

export class SystemService implements ISystemService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getSysCompany(companyApiKey: string) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPANY_COMPANYAPIKEY + companyApiKey, false);
    }

    getSysCompanyWithId(sysCompanyId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPANY + sysCompanyId, false);
    }

    getSysCompanies() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPANY, false);
    }

    getSysCompServer(sysCompanyServerId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPSERVER + sysCompanyServerId, false);
    }

    getSysCompServers() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPSERVER, false);
    }

    getSysCompDB(sysCompDBId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPDB + sysCompDBId, false);
    }

    getSysCompDBs() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPDB, false);
    }

    getSysInformations(): ng.IPromise<InformationDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj: InformationDTO = new InformationDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();

                if (obj.sysInformationSysCompDbs) {
                    obj.sysInformationSysCompDbs = obj.sysInformationSysCompDbs.map(c => {
                        let cObj: SysInformationSysCompDbDTO = new SysInformationSysCompDbDTO();
                        angular.extend(cObj, c);
                        cObj.fixDates();
                        return cObj;
                    });
                } else {
                    obj.sysInformationSysCompDbs = [];
                }

                return obj;
            });
        });
    }

    getSysInformationGrids(): ng.IPromise<InformationGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj: InformationGridDTO = new InformationGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getSysInformationForEdit(sysInformationId: number): ng.IPromise<InformationDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION + sysInformationId, false).then(x => {
            let obj: InformationDTO = new InformationDTO();
            angular.extend(obj, x);
            obj.fixDates();

            if (obj.sysInformationSysCompDbs) {
                obj.sysInformationSysCompDbs = obj.sysInformationSysCompDbs.map(c => {
                    let cObj: SysInformationSysCompDbDTO = new SysInformationSysCompDbDTO();
                    angular.extend(cObj, c);
                    cObj.fixDates();
                    return cObj;
                });
            } else {
                obj.sysInformationSysCompDbs = [];
            }

            return obj;
        });
    }

    getCommodityCodes(langId: number): ng.IPromise<CommodityCodeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_COMMODITY_CODES + langId, false);
    }

    getSysInformationFolders(): ng.IPromise<string[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_FOLDERS, false);
    }

    getSysInformationSysCompDbs(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_SYS_COMPDB, false);
    }

    getSysInformationSysFeatures(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_SYS_FEATURE, false);
    }

    getSysInformationHasConfirmations(sysInformationId: number): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_HAS_CONFIRMATIONS + sysInformationId, false);
    }

    getSysInformationRecipientInfo(sysInformationId: number): ng.IPromise<InformationRecipientDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_RECIPIENT_INFO + sysInformationId, false).then(x => {
            return x.map(y => {
                let obj: InformationRecipientDTO = new InformationRecipientDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getSysWholesellers() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSWHOLESELLER_SYSWHOLESELLER, false);
    }

    getSysWholeseller(sysWholesellerId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSWHOLESELLER_SYSWHOLESELLER + sysWholesellerId, false);
    }

    getSysEdiMessageRaw(sysEdiMessageRawId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGERAW + sysEdiMessageRawId, false);
    }

    getSysEdiMessageRaws() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGERAW, false);
    }

    getSysEdiMessageHead(sysEdiMessageHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGEHEAD + sysEdiMessageHeadId, false);
    }

    getSysEdiMessageHeads() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGEHEAD, false);
    }

    getSysEdiMessageGridHeads(sysEdiMessageHeadStatus: SysEdiMessageHeadStatus, take: number, missingSysCompanyId: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGEGRIDHEAD + sysEdiMessageHeadStatus + "/" + take + "/" + missingSysCompanyId, false);
    }

    getSysEdiMessageHeadMessage(sysEdiMessageHeadId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGEHEAD_MSG + sysEdiMessageHeadId, false);
    }

    getSoftOneServerUtilityPageStatuses() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SERVER_UTILITY_PAGE_STATUS, false);
    }

    getSysPayrollPrices(sysCountryId: number, sysPayrollPrices: TermGroup_SysPayrollPrice[], setName: boolean, setTypeName: boolean, setAmountTypeName: boolean, includeIntervals: boolean, onlyLatest: boolean, date: Date): ng.IPromise<SysPayrollPriceDTO[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        var sysPayrollPricesString: string = null;
        if (sysPayrollPrices)
            sysPayrollPricesString = sysPayrollPrices.join(',');

        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_PAYROLL_PRICE + sysCountryId + "/" + sysPayrollPricesString + "/" + setName + "/" + setTypeName + "/" + setAmountTypeName + "/" + includeIntervals + "/" + onlyLatest + "/" + dateString, false).then(x => {
            return x.map(y => {
                let obj = new SysPayrollPriceDTO();
                angular.extend(obj, y);
                obj.fixDates();

                if (y.intervals) {
                    obj.intervals = y.intervals.map(i => {
                        let iObj = new SysPayrollPriceIntervalDTO();
                        angular.extend(iObj, i);
                        return iObj;
                    });
                } else {
                    obj.intervals = [];
                }
                return obj;
            });
        });
    }

    getSysPayrollPrice(sysPayrollPriceId: number, includeIntervals: boolean, setName: boolean, setTypeName: boolean, setAmountTypeName: boolean): ng.IPromise<SysPayrollPriceDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_PAYROLL_PRICE + sysPayrollPriceId + "/" + includeIntervals + "/" + setName + "/" + setTypeName + "/" + setAmountTypeName, false).then(x => {
            let obj = new SysPayrollPriceDTO();
            angular.extend(obj, x);
            obj.fixDates();

            if (x.intervals) {
                obj.intervals = x.intervals.map(i => {
                    let iObj = new SysPayrollPriceIntervalDTO();
                    angular.extend(iObj, i);
                    return iObj;
                });
            } else {
                obj.intervals = [];
            }
            return obj;
        });
    }

    validatePasswordForSysPayrollPrice(password: string): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_PAYROLL_PRICE_VALIDATE_PASSWORD + password, false);
    }

    getSysVehicleTypes() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE, false);
    }

    getSysVehicleType(sysVehicleTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE + sysVehicleTypeId, false);
    }

    getSysTermGroups() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_TERM_GROUPS, false);
    }

    getSysTerms(sysTermGroupId: number, langId: number, date?: Date) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_TERMS + sysTermGroupId + "/" + langId + "/" + (date ? date.toDateTimeString() : ""), false);
    }

    getEdiEntries(dateFrom: Date, dateTo: Date): ng.IPromise<any> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_EDI_ENTRIES + dateFromString + "/" + dateToString, false);
    }

    getStandardDefinitions(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_IMPORT_STANDARDDEFS, false);
    }

    getSysImportHeadsDict(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_IMPORT_SYSIMPORTHEADSDICT, false);
    }

    getSysImportDefinition(sysImportDefinitionId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_IMPORT_STANDARDDEF + sysImportDefinitionId, false);
    }

    getSysImportHead(sysImportHeadId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_IMPORT_SYSIMPORTHEAD + sysImportHeadId, false);
    }

    getTestCases(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASES + "?testcasetype=0", false);
    }

    getTestCase(testCaseId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASE + testCaseId, false);
    }

    getTestCaseGroups(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUPS, false);
    }

    getTestCaseGroup(testCaseGroupId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUP + testCaseGroupId, false);
    }

    getTestTracking(trackingGuid: Guid): ng.IPromise<Guid> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TRACKING + trackingGuid, false);
    }

    getTestCaseResultsByTestCaseId(testCaseId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASERESULTS_BY_TESTCASEID + testCaseId, false);
    }

    getTestCaseResultsByTestCaseGroupId(testCaseGroupId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASERESULTS_BY_TESTCASEGROUPID + testCaseGroupId, false);
    }

    getTestCaseGroupResults(testCaseGroupId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUPRESULTS + testCaseGroupId, false);
    }

    getTestCaseGroupOverview(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUPOVERVIEW, false);
    }

    getTestCaseGroupOverviewByGroup(testCaseGroupId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUPOVERVIEWBYGROUP + testCaseGroupId, false);
    }

    getTestCaseSettings(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_TEST_TESTCASESETTINGS, false);
    }

    // POST
    saveSysCompany(sysCompanyDTO: ISysCompanyDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPANY, sysCompanyDTO);
    }

    saveSysCompDB(sysCompanyDTO: ISysCompDBDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPDB, sysCompanyDTO);
    }

    saveSysCompServer(sysCompanyDTO: ISysCompServerDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPSERVER, sysCompanyDTO);
    }

    saveSysInformation(information: InformationDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION, information);
    }

    saveSysWholeseller(sysCompanyDTO: ISysWholesellerDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSWHOLESELLER_SYSWHOLESELLER, sysCompanyDTO);
    }

    saveSysEdiMessageRaw(sysEdiMessageRaw: ISysEdiMessageRawDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGERAW, sysEdiMessageRaw);
    }

    saveSysEdiMessageHead(sysEdiMessageHead: ISysEdiMessageHeadDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_EDI_SYSEDIMESSAGEHEAD, sysEdiMessageHead);
    }

    saveSysPayrollPrice(sysPayrollPrice: SysPayrollPriceDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_PAYROLL_PRICE, sysPayrollPrice);

    }

    saveSysVehicleType(file: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE, file);
    }

    getSysTermSuggestion(text: string, primaryLangId: number, secondaryLangId: number) {
        var model = {
            text: text,
            primaryLanguageId: primaryLangId,
            secondaryLanguageId: secondaryLangId
        };
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_TERMS_SUGGESTION, model);
    }

    saveSysTerms(terms: SysTermDTO[]) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_TERMS, terms);
    }

    getScheduledJob(sysScheduledJobId: number, loadSettings: boolean, loadJob: boolean): ng.IPromise<SysScheduledJobDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB + sysScheduledJobId + "/" + loadSettings + "/" + loadJob, false).then(x => {
            let obj = new SysScheduledJobDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getScheduledJobs(): ng.IPromise<SysScheduledJobDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB, false).then(x => {
            return x.map(y => {
                let obj = new SysScheduledJobDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getScheduledJobLog(sysScheduledJobId: number): ng.IPromise<SysScheduledJobLogDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB_GET_LOG + sysScheduledJobId, false).then(x => {
            return x.map(y => {
                let obj = new SysScheduledJobLogDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getNoOfActiveScheduledJobs(): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB_NO_OF_ACTIVE, false);
    }

    getRegisteredJobs(useCache: boolean): ng.IPromise<SysJobDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_REGISTERED_JOB, useCache).then(x => {
            return x.map(y => {
                let obj = new SysJobDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    runScheduledJob(sysScheduledJobId: number) {
        var model = {
            sysScheduledJobId: sysScheduledJobId,
        };
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB_RUN, model);
    }

    runScheduledJobByService(sysScheduledJobId: number) {
        var model = {
            sysScheduledJobId: sysScheduledJobId,
        };
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB_RUN_BY_SERVICE, model);
    }

    saveScheduledJob(job: SysScheduledJobDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB, job);
    }

    saveJob(job: SysJobDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_REGISTERED_JOB, job);
    }

    updateAttestRuleState(items: any[]) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_REGISTERED_JOB_UPDATE_STATE, items);
    }

    saveSysImportDefinition(sysImportDefinition: SysImportDefinitionDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_IMPORT_STANDARDDEF, sysImportDefinition);
    }

    saveTestCase(testCase: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_TEST_TESTCASE, testCase)
    }

    saveTestCaseGroup(testCaseGroup: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUP, testCaseGroup)
    }

    runTestCaseGroup(testCaseGroupId: number) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUP_RUN + testCaseGroupId, testCaseGroupId)
    }

    scheduleTestCaseGroupsNow(testCaseGroupIds: number[]) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_TEST_TESTCASEGROUP_SCHEDULENOW, testCaseGroupIds)
    }

    // DELETE

    deleteSysCompany(sysCompanyId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SYSCOMPANY_SYSCOMPANY + sysCompanyId);
    }

    deleteSysInformation(sysInformationId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION + sysInformationId);
    }

    deleteSysInformations(sysInformationIds: number[]): ng.IPromise<IActionResult> {
        var model = {
            numbers: sysInformationIds
        }

        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_DELETE_MULTIPLE, model);
    }

    deleteSysInformationNotificationSent(sysInformationId: number, sysCompDbId: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SYSTEM_SYS_INFORMATION_DELETE_NOTIFICATION_SENT + sysInformationId + "/" + (sysCompDbId || 0), null);
    }

    deleteSysVehicleType(sysVehicleTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE + sysVehicleTypeId);
    }

    deleteSchedueldJob(sysScheduledJobId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_SCHEDULED_JOB + sysScheduledJobId);
    }

    deletePerformanceSetting(performanceSettingId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_SOFTONE_SERVER_UTILITY_PERFORMANCESETTING + performanceSettingId);

    }

    deleteJob(sysJobId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_SCHEDULER_REGISTERED_JOB + sysJobId);
    }

    deleteSysImportDefinition(sysImportDefinitionId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_SYSTEM_IMPORT_STANDARDDEF + sysImportDefinitionId);
    }
}
