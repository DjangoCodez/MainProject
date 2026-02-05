import { IEmployeeCalculatedCostDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EmployeeCalculatedCostDTO implements IEmployeeCalculatedCostDTO {
    calculatedCostPerHour: number;
    employeeCalculatedCostId: number;
    employeeId: number;
    fromDate: Date;
    isDeleted: boolean;
    isModified: boolean;
    projectId: number;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}