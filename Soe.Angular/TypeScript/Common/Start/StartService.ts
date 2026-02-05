import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";

export interface IStartService {

    // GET
    getEmployeesCount(): ng.IPromise<any>
}

export class StartService implements IStartService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) {
    }

    // GET

    getEmployeesCount() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_COUNT, false);
    }
}