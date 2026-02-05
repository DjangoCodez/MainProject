import { IHttpService } from "../../Core/Services/HttpService";
import { SoeLogType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";

export interface ISupportService {

    // GET
    getSysLogs(logType: SoeLogType, showUnique: boolean): ng.IPromise<any>
    getSysLog(sysLogId: number): ng.IPromise<any>

    // POST
    searchSysLogs(dto: any): ng.IPromise<any>
}

export class SupportService implements ISupportService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getSysLogs(logType: SoeLogType, showUnique: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SUPPORT_SYS_LOG_LOG_TYPE + logType + "/" + showUnique, false);
    }

    getSysLog(sysLogId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SUPPORT_SYS_LOG + sysLogId, false);
    }

    // POST

    searchSysLogs(dto: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_SUPPORT_SYS_LOG_SEARCH, dto);
    }
}
