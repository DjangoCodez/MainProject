import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";
import { ApiMessageGridDTO } from "../Models/ApiMessageDTO";
import { TermGroup_ApiMessageSourceType, TermGroup_ApiMessageType } from "../../Util/CommonEnumerations";
import { ApiSettingDTO } from "../Models/ApiSettingDTO";
import { Guid } from "../../Util/StringUtility";
import { EmployeeDeviationAfterEmploymentDTO } from "../Models/EmployeeDeviationAfterEmploymentDTO";

export interface IApiService {

    // GET
    getApiMessages(type: TermGroup_ApiMessageType, source: TermGroup_ApiMessageSourceType, filterFromDate: Date, filterToDate: Date, filterShowVerified: boolean, filterShowOnlyErrors: boolean): ng.IPromise<ApiMessageGridDTO[]>;
    getApiSettings(): ng.IPromise<ApiSettingDTO[]>;
    getEmployeeDeviationsAfterEmployment(): ng.IPromise<EmployeeDeviationAfterEmploymentDTO[]>;

    // POST
    setApiMessagesAsVerified(timeAccumulatorIds: number[]): ng.IPromise<any>
    importApiMessageFile(type: TermGroup_ApiMessageType, file: any): ng.IPromise<any>;
    saveApiSettings(settings: ApiSettingDTO[]): ng.IPromise<any>
    deleteEmployeeDeviationsAfterEmployment(deviations: EmployeeDeviationAfterEmploymentDTO): ng.IPromise<any>
}

export class ApiService implements IApiService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getApiMessages(type: TermGroup_ApiMessageType, source: TermGroup_ApiMessageSourceType, filterFromDate: Date, filterToDate: Date, filterShowVerified: boolean, filterShowOnlyErrors: boolean): ng.IPromise<ApiMessageGridDTO[]> {
        var fromDate: string = null;
        if (filterFromDate)
            fromDate = filterFromDate.toDateTimeString();
        var toDate: string = null;
        if (filterToDate)
            toDate = filterToDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_CORE_API_MESSAGES + type + "/" + source + "/" + fromDate + "/" + toDate + "/" + filterShowVerified + "/" +  filterShowOnlyErrors, false).then(x => {
            return x.map(y => {
                let obj = new ApiMessageGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }
    getApiSettings(): ng.IPromise<ApiSettingDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_API_SETTINGS, false).then(x => {
            return x.map(y => {
                let obj = new ApiSettingDTO();
                obj.guid = Guid.newGuid();
                angular.extend(obj, y);
                return obj;
            });
        });
    }
    getEmployeeDeviationsAfterEmployment(): ng.IPromise<EmployeeDeviationAfterEmploymentDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATIONSAFTEREMPLOYMENT, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeDeviationAfterEmploymentDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    //POST

    setApiMessagesAsVerified(apiMessageIds: number[]) {
        var model = {
            numbers: apiMessageIds
        };

        return this.httpService.post(Constants.WEBAPI_CORE_API_MESSAGES_SETASVERIFIED, model);
    }
    importApiMessageFile(type: TermGroup_ApiMessageType, file: any) {
        var model = { type: type, file: file };
        return this.httpService.post(Constants.WEBAPI_CORE_API_MESSAGES_IMPORT, model);
    }
    saveApiSettings(settings: ApiSettingDTO[]) {
        var model = {
            settings: settings
        };
        return this.httpService.post(Constants.WEBAPI_CORE_API_SETTINGS, model);
    }
    deleteEmployeeDeviationsAfterEmployment(deviations: EmployeeDeviationAfterEmploymentDTO) {
        var model = {
            deviations: deviations
        };
        model.deviations.employeeDates = undefined; //WebApi-call fail when EmployeeDates is passed
        console.log('deleteEmployeeDeviationsAfterEmployment',model);
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_DEVIATIONSAFTEREMPLOYMENT_DELETE, model);
    }
}
