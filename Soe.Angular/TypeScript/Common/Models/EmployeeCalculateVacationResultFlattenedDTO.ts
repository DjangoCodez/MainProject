import { IEmployeeCalculateVacationResultFlattenedDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EmployeeCalculateVacationResultFlattenedDTO implements IEmployeeCalculateVacationResultFlattenedDTO {
    totValue: number;
    vBDValue: number;
    vFDValue: number;
    vIDValue: number;
    vSDValue: number;
    vBSTRValue: number;
    bSTRAValue: number;
    vISTRValue: number;
    vSSTRValue: number;
    sSTRAValue: number;
    totVSTRValue: number;
    actorCompanyId: number;
    created: Date;
    date: Date;
    dateStr: string;
    employeeCalculateVacationResultHeadId: number;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeeNrAndName: string;


    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.date = CalendarUtility.convertToDate(this.date);
    }
}