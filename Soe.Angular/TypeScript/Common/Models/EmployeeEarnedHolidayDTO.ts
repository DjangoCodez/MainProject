import { IEmployeeEarnedHolidayDTO } from "../../Scripts/TypeLite.Net4";

export class EmployeeEarnedHolidayDTO implements IEmployeeEarnedHolidayDTO {
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeePercent: number;
    hasTransaction: boolean;
    hasTransactionString: string;
    suggestion: boolean;
    suggestionNote: string;
    suggestionString: string;
    work5DaysPerWeek: boolean;
    work5DaysPerWeekString: string;
}
