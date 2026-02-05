import { IHttpService } from "../../../Core/Services/httpservice";
import { Constants } from "../../../Util/Constants";
import { EmployeeListDTO, EmployeeListEmploymentDTO } from "../../../Common/Models/EmployeeListDTO";
import { IActionResult, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TimeDeviationCauseDTO } from "../../../Common/Models/TimeDeviationCauseDTOs";

export interface ITimeService {

    // GET
    getAbsenceTimeDeviationCauses(): ng.IPromise<any>
    getAbsenceTimeDeviationCausesFromEmployeeId(employeeId: number, date: Date, onlyUseInTimeTerminal: boolean): ng.IPromise<any>
    getAbsenceTimeDeviationCausesAbsenceAnnouncement(employeeGroupId: number, addEmptyRow: boolean): ng.IPromise<any>
    getTimeDeviationCause(timeDeviationCauseId: number): ng.IPromise<TimeDeviationCauseDTO>
    getEmployee(employeeId: number, dateFrom: Date, dateTo: Date, includeEmployments: boolean, includeEmployeeGroup: boolean, includePayrollGroup: boolean, includeVacationGroup: boolean): ng.IPromise<any>
    getEmployeeGroupId(employeeId: number, date: Date): ng.IPromise<number>
    getEmployeeChildsDict(employeeId: number, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getEmployeesForAbsencePlanning(dateFrom: Date, dateTo: Date, mandatoryEmployeeId: number, excludeCurrentUserEmployee: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<EmployeeListDTO[]>
    getTimeDeviationCauseRequests(employeeId: number, employeeGroupId: number): ng.IPromise<any>
    getTimeRuleTimeCodesLeft(): ng.IPromise<ISmallGenericType[]>
    getTimeRuleTimeCodesRight(): ng.IPromise<ISmallGenericType[]>

    // POST
    sendAttestReminder(employeeIds: number[], dateFrom: Date, dateTo: Date, sendAttestreminderToEmployee: boolean, sendAttestreminderToExecutive: boolean): ng.IPromise<IActionResult>

    // DELETE
}

export class TimeService implements ITimeService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getAbsenceTimeDeviationCauses() {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?onlyAbsence=true", true, Constants.WEBAPI_ACCEPT_DTO);
    }

    getAbsenceTimeDeviationCausesFromEmployeeId(employeeId: number, date: Date, onlyUseInTimeTerminal: boolean) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_EMPLOYEE_ABSENCE + employeeId + "/" + dateString + "/" + onlyUseInTimeTerminal , true);
    }

    getAbsenceTimeDeviationCausesAbsenceAnnouncement(employeeGroupId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_ABSENCE_ANNOUNCEMENTS + employeeGroupId + "/" + addEmptyRow, true);
    }

    getTimeDeviationCause(timeDeviationCauseId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + timeDeviationCauseId, false).then(x => {
            let obj = new TimeDeviationCauseDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getEmployee(employeeId: number, dateFrom: Date, dateTo: Date, includeEmployments: boolean, includeEmployeeGroup: boolean, includePayrollGroup: boolean, includeVacationGroup: boolean) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE + employeeId + "/" + dateFromString + "/" + dateToString + "/" + includeEmployments + "/" + includeEmployeeGroup + "/" + includePayrollGroup + "/" + includeVacationGroup + "/" + false + "/" + 0, false);
    }

    getEmployeeChildsDict(employeeId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_CHILD + "?employeeId=" + employeeId + "&addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getEmployeeGroupId(employeeId: number, date: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP + employeeId + "/" + dateString, false);
    }

    getEmployeesForAbsencePlanning(dateFrom: Date, dateTo: Date, mandatoryEmployeeId: number, excludeCurrentUserEmployee: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<EmployeeListDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_ABSENCE_PLANNING + "/" + dateFromString + "/" + dateToString + "/" + mandatoryEmployeeId + "/" + excludeCurrentUserEmployee + "/" + timeScheduleScenarioHeadId, false).then((x: EmployeeListDTO[]) => {
            return x.map(y => {
                var obj = new EmployeeListDTO();
                angular.extend(obj, y);

                if (obj.employments && obj.employments.length > 0) {
                    obj.employments = obj.employments.map(e => {
                        let eObj = new EmployeeListEmploymentDTO();
                        angular.extend(eObj, e);
                        eObj.fixDates();
                        return eObj;
                    });
                }

                return obj;
            });
        })
    }

    getTimeDeviationCauseRequests(employeeId: number, employeeGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_REQUESTS + "?employeeGroupId=" + employeeGroupId + "&employeeId=" + employeeId, true);
    }     

    getTimeRuleTimeCodesLeft(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_RULE_TIME_CODE_LEFT, true);
    }

    getTimeRuleTimeCodesRight(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_RULE_TIME_CODE_RIGHT, true);
    }


    // POST
    sendAttestReminder(employeeIds: number[], dateFrom: Date, dateTo: Date, sendAttestreminderToEmployee: boolean, sendAttestreminderToExecutive: boolean): ng.IPromise<IActionResult> {
        
        var model = {
            employeeIds: employeeIds,
            startDate: dateFrom,
            stopDate: dateTo,
            doSendToEmployee: sendAttestreminderToEmployee,
            doSendToExecutive: sendAttestreminderToExecutive
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_SENDREMINDER, model);
    }

    // DELETE

}