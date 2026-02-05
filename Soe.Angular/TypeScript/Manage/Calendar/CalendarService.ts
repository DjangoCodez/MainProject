import { ICoreService } from "../../Core/Services/CoreService";
import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";

export interface ICalendarService {

    // GET
    getSchoolHolidays(useCache: boolean): ng.IPromise<any>
    getSchoolHoliday(schoolHolidayId: number): ng.IPromise<any>

    // POST
    saveSchoolHoliday(schoolHoliday: any): ng.IPromise<any>

    // DELETE
    deleteSchoolHoliday(schoolHolidayId: number): ng.IPromise<any>
}

export class CalendarService implements ICalendarService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getSchoolHolidays(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_CALENDAR_SCHOOLHOLIDAY, useCache);
    }

    getSchoolHoliday(schoolHolidayId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_CALENDAR_SCHOOLHOLIDAY + schoolHolidayId, false);
    }

    // POST

    saveSchoolHoliday(schoolHoliday: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_CALENDAR_SCHOOLHOLIDAY, schoolHoliday);
    }

    // DELETE

    deleteSchoolHoliday(schoolHolidayId: number) {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_CALENDAR_SCHOOLHOLIDAY + schoolHolidayId);
    }

}
