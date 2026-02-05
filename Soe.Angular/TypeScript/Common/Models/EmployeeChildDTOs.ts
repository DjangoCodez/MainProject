import { IEmployeeChildDTO, IEmployeeChildCareDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EmployeeChildDTO implements IEmployeeChildDTO {
    birthDate: Date;
    employeeChildId: number;
    employeeId: number;
    firstName: string;
    lastName: string;
    name: string;
    openingBalanceUsedDays: number;
    singleCustody: boolean;
    state: SoeEntityState;
    usedDays: number;
    usedDaysText: string;

    // Extensions
    public get nbrOfDays(): number {
        return this.singleCustody ? 180 : 120;
    }
    public set nbrOfDays(days: number) { }

    public get daysLeft(): number {
        var count = (this.nbrOfDays || 0) - (this.openingBalanceUsedDays || 0) - (this.usedDays || 0);
        return count < 0 ? 0 : count;
    }
    public set daysLeft(days: number) { }

    public fixDates() {
        this.birthDate = CalendarUtility.convertToDate(this.birthDate);
    }
}

export class EmployeeChildCareDTO implements IEmployeeChildCareDTO {
    dateFrom: Date;
    dateTo: Date;
    daysLeft: number;
    name: string;    
    nbrOfDays: number;
    openingBalanceUsedDays: number;
    usedDays: number;
    usedDaysText: string;
    year: number;
}