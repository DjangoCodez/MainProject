import { IEmployeeStatisticsChartData } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EmployeeStatisticsChartData implements IEmployeeStatisticsChartData {
    color: string;
    date: Date;
    toolTip: string;
    value: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}