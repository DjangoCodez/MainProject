import { IHttpService } from "../../../Core/Services/HttpService";
import { IEmployeeVacationSEDTO } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { EmployeeGridDTO } from "../../../Common/Models/EmployeeUserDTO";
import { EmployeeSmallDTO } from "../../../Common/Models/EmployeeListDTO";
import { EmployeeVacationPrelUsedDaysDTO } from "../../../Common/Models/EmployeeVacationPrelUsedDaysDTO";


export interface IEmployeeService {

    //GET
    getEmployeeVacation(employeeId: number): ng.IPromise<IEmployeeVacationSEDTO>
    getEmployeesForGrid(loadPayrollGroups: boolean, showInactive: boolean, showEnded: boolean, showNotStarted: boolean, setAge: boolean, date?: Date, employeeFilter?: number[], loadAnnualLeaveGroups?: boolean): ng.IPromise<EmployeeGridDTO[]>
    getEmployeesForGridSmall(onlyActive: boolean): ng.IPromise<EmployeeSmallDTO[]>
    getPrelUsedVacationDays(employeeId: number, date: Date): ng.IPromise<EmployeeVacationPrelUsedDaysDTO>
    getVacationGroupForEmployee(employeeId: number, date: Date): ng.IPromise<any>  
}

export class EmployeeService implements IEmployeeService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) {}

    // GET
    getEmployeeVacation(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACATION + employeeId, false);
    }

    getEmployeesForGrid(loadPayrollGroups: boolean, showInactive: boolean, showEnded: boolean, showNotStarted: boolean, setAge: boolean, date?: Date, employeeFilter?: number[], loadAnnualLeaveGroups = false): ng.IPromise<EmployeeGridDTO[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GRID + "?date=" + dateString + "&employeeFilter=" + employeeFilter + "&loadPayrollGroups=" + loadPayrollGroups + "&showInactive=" + showInactive + "&showEnded=" + showEnded + "&showNotStarted=" + showNotStarted + "&setAge=" + setAge + "&loadAnnualLeaveGroups=" + loadAnnualLeaveGroups, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEmployeesForGridSmall(onlyActive: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GRID + "?showInactive=" + onlyActive, false, Constants.WEBAPI_ACCEPT_SMALL_DTO).then(x => {
            return x.map(y => {
                let obj = new EmployeeSmallDTO();
                angular.extend(obj, y);
                return obj;
            })
        });
    }

    getVacationGroupForEmployee(employeeId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACATION_GROUP_FOR_EMPLOYEE + employeeId + "/" + dateString, false);
    }

    getPrelUsedVacationDays(employeeId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACATION_GET_PREL_USED_VACATION_DAYS + employeeId + "/" + dateString, false);
    }}

