import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";
import { MessageGroupDTO, MessageGroupMemberDTO } from "../../Common/Models/MessageDTOs";
import { IActionResult, ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { InformationDTO, InformationGridDTO } from "../../Common/Models/InformationDTOs";
import { OpeningHoursDTO } from "../../Common/Models/OpeningHoursDTO";
import { ScheduledJobHeadDTO, ScheduledJobHeadGridDTO, ScheduledJobLogDTO } from "../../Common/Models/ScheduledJobDTOs";
import { DateRangeDTO } from "../../Common/Models/DateRangeDTO";
import { CompanyExternalCodeDTO, CompanyExternalCodeGridDTO } from "../../Common/Models/CompanyExternalCodeDTOs";

export interface IRegistryService {

    // GET
    getCompanyExternalCodesGrid(): ng.IPromise<CompanyExternalCodeGridDTO[]>;
    getCompanyExternalCode(companyExternalCodeId: number): ng.IPromise<CompanyExternalCodeDTO>;
    getCompanyInformations(): ng.IPromise<InformationDTO[]>;
    getCompanyInformationGrids(): ng.IPromise<InformationGridDTO[]>;
    getOpeningHours(useCache: boolean): ng.IPromise<OpeningHoursDTO[]>;
    getOpeningHour(openingHourId: number): ng.IPromise<OpeningHoursDTO>;
    getMessageGroupsGrid(useCache: boolean): ng.IPromise<any>;
    getMessageGroup(messageGroupId: number): ng.IPromise<MessageGroupDTO>;
    getChecklistHeads(useCache: boolean): ng.IPromise<any>;
    getChecklistHead(checklistHeadId: number, loadRows: boolean): ng.IPromise<any>;
    getChecklistReports(sysReportTemplateTypeId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean): ng.IPromise<any>;
    getMultipleChoiceAnswerHeads(): ng.IPromise<any>;
    getMultipleChoiceAnswerRows(answerHeadId: number): ng.IPromise<any>;
    getScheduledJobHeads(useCache: boolean): ng.IPromise<ScheduledJobHeadDTO[]>;
    getScheduledJobHeadsForGrid(useCache: boolean): ng.IPromise<ScheduledJobHeadGridDTO[]>;
    getScheduledJobHeadsDict(addEmptyRow: boolean, includeSharedOnLicense: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>;
    getScheduledJobHead(scheduledJobHeadId: number, loadRows: boolean, loadLogs: boolean, loadSettings: boolean, loadSettingOptions: boolean, setRecurrenceIntervalText: boolean, setTimeIntervalText: boolean): ng.IPromise<ScheduledJobHeadDTO>;
    getScheduledJobLogs(scheduledJobHeadId: number, setLogLevelName: boolean, setStatusName: boolean): ng.IPromise<ScheduledJobLogDTO[]>;
    getScheduledJobSettingOptions(settingType: number): ng.IPromise<ISmallGenericType[]>;
    getSysTimeIntervals(): ng.IPromise<ISmallGenericType[]>;
    getSysTimeIntervalDateRange(sysTimeIntervalId: number): ng.IPromise<DateRangeDTO>;

    // POST
    saveCompanyExternalCode(companyExternalCode: any): ng.IPromise<any>;
    saveOpeningHours(openingHours: any): ng.IPromise<any>
    saveMessageGroup(messageGroup: any): ng.IPromise<any>
    saveChecklistHead(checkListHead: any): ng.IPromise<any>
    saveMultipleChoiceAnswerHead(answerHead: any, answerRows: any[]): ng.IPromise<any>;
    saveScheduledJobHead(head: ScheduledJobHeadDTO): ng.IPromise<IActionResult>

    // DELETE
    deleteCompanyInformations(informationIds: number[]): ng.IPromise<IActionResult>
    deleteCompanyExternalCode(companyExternalCodeId: number): ng.IPromise<any>
    deleteOpeningHours(openingHoursId: number): ng.IPromise<any>
    deleteMessageGroup(messageGroupId: number): ng.IPromise<any>
    deleteChecklistHead(checklistHeadId: number): ng.IPromise<any>
    deleteScheduledJobHead(scheduledJobHeadId: number): ng.IPromise<IActionResult>
}

export class RegistryService implements IRegistryService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getCompanyExternalCodesGrid(): ng.IPromise<CompanyExternalCodeGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_EXTERNAL_CODE, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj: CompanyExternalCodeGridDTO = new CompanyExternalCodeGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }
    getCompanyExternalCode(companyExternalCodeId: number): ng.IPromise<CompanyExternalCodeDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_EXTERNAL_CODE + companyExternalCodeId, false).then(x => {
            let obj = new CompanyExternalCodeDTO();
            angular.extend(obj, x);
            return obj;
        });
    }


    getCompanyInformations(): ng.IPromise<InformationDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj: InformationDTO = new InformationDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getCompanyInformationGrids(): ng.IPromise<InformationGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj: InformationGridDTO = new InformationGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getOpeningHours(useCache: boolean): ng.IPromise<OpeningHoursDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS, useCache).then(x => {
            return x.map(y => {
                let obj = new OpeningHoursDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getOpeningHour(openingHourId: number): ng.IPromise<OpeningHoursDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS + openingHourId, false).then(x => {
            let obj = new OpeningHoursDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getMessageGroupsGrid(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getMessageGroup(messageGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP + messageGroupId, false).then((x: MessageGroupDTO) => {
            if (x) {
                let group: MessageGroupDTO = new MessageGroupDTO();
                angular.extend(group, x);

                group.groupMembers = group.groupMembers.map(m => {
                    let member: MessageGroupMemberDTO = new MessageGroupMemberDTO();
                    angular.extend(member, m);
                    return member;
                });

                return group;
            } else {
                return x;
            }
        });
    }

    getChecklistHeads(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_HEADS, useCache);
    }

    getChecklistHead(checklistHeadId: number, loadRows: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_HEAD + checklistHeadId + "/" + loadRows, false);
    }

    getChecklistReports(sysReportTemplateTypeId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean) {
        return this.httpService.get(Constants.WEBAPI_REPORT_REPORTS + sysReportTemplateTypeId + "/" + onlyOriginal + "/" + onlyStandard + "/" + addEmptyRow + "/" + useRole, false);
    }

    getMultipleChoiceAnswerHeads() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_MULTIPLECHOICEANSWERHEADS, false);
    }

    getMultipleChoiceAnswerRows(answerHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_MULTIPLECHOICEANSWERROWS + answerHeadId, false);
    }

    getScheduledJobHeads(useCache: boolean): ng.IPromise<ScheduledJobHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD, useCache).then(x => {
            return x.map(y => {
                let obj = new ScheduledJobHeadDTO();
                angular.extend(obj, y);
                obj.setTypes();
                return obj;
            });
        });
    }

    getScheduledJobHeadsForGrid(useCache: boolean): ng.IPromise<ScheduledJobHeadGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new ScheduledJobHeadGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getScheduledJobHeadsDict(addEmptyRow: boolean, includeSharedOnLicense: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD + "?addEmptyRow=" + addEmptyRow + "&includeSharedOnLicense=" + includeSharedOnLicense, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getScheduledJobHead(scheduledJobHeadId: number, loadRows: boolean, loadLogs: boolean, loadSettings: boolean, loadSettingOptions: boolean, setRecurrenceIntervalText: boolean, setTimeIntervalText: boolean): ng.IPromise<ScheduledJobHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD + scheduledJobHeadId + "/" + loadRows + "/" + loadLogs + "/" + loadSettings + "/" + loadSettingOptions + "/" + setRecurrenceIntervalText + "/" + setTimeIntervalText, false).then(x => {
            let obj = new ScheduledJobHeadDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getScheduledJobLogs(scheduledJobHeadId: number, setLogLevelName: boolean, setStatusName: boolean): ng.IPromise<ScheduledJobLogDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_LOG + scheduledJobHeadId + "/" + setLogLevelName + "/" + setStatusName, false).then(x => {
            return x.map(y => {
                let obj = new ScheduledJobLogDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getScheduledJobSettingOptions(settingType: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_SETTING_OPTIONS + settingType, true);
    }

    public getSysTimeIntervals(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_SYS_TIME_INTERVALS, true);
    }

    public getSysTimeIntervalDateRange(sysTimeIntervalId: number): ng.IPromise<DateRangeDTO> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_SYS_TIME_INTERVAL_DATE_RANGE + "?sysTimeIntervalId=" + sysTimeIntervalId, false).then(x => {
            let obj = new DateRangeDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }


    // POST

    saveCompanyExternalCode(companyExternalCode: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_EXTERNAL_CODE, companyExternalCode);
    }

    saveOpeningHours(openingHours: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS, openingHours);
    }

    saveMessageGroup(messageGroup: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP, messageGroup);
    }

    saveChecklistHead(checklistHead: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_HEAD, checklistHead);
    }

    saveMultipleChoiceAnswerHead(answerHead: any, answerRows: any[]) {
        var model = {
            AnswerHead: answerHead,
            AnswerRows: answerRows
        };

        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_MULTIPLECHOICEANSWERHEADS, model);
    }

    saveScheduledJobHead(head: ScheduledJobHeadDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD, head);
    }

    // DELETE

    deleteCompanyInformations(informationIds: number[]): ng.IPromise<IActionResult> {
        var model = {
            numbers: informationIds
        }

        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION_DELETE_MULTIPLE, model);
    }

    deleteCompanyExternalCode(companyExternalCodeId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_EXTERNAL_CODE + companyExternalCodeId);
    }

    deleteOpeningHours(openingHoursId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_REGISTRY_OPENING_HOURS + openingHoursId);
    }

    deleteMessageGroup(messageGroupId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP + messageGroupId);
    }

    deleteChecklistHead(checklistHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_REGISTRY_CHECKLISTS_HEAD + checklistHeadId);
    }

    deleteScheduledJobHead(scheduledJobHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD + scheduledJobHeadId);
    }
}
