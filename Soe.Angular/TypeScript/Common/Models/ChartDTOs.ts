import { IDateChart, IDateChartData, IDateChartValue } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
 
export class DateChart implements IDateChart {
    data: DateChartData[];

    public setTypes() {
        this.data = this.data.map(x => {
            let obj = new DateChartData();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }
}

export class DateChartData implements IDateChartData {
    values: DateChartValue[];
    date: Date;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }

    public setTypes() {
        this.values = this.values.map(x => {
            let obj = new DateChartValue();
            angular.extend(obj, x);
            return obj;
        });
    }
}

export class DateChartValue implements IDateChartValue {
    type: number;
    value: number;
}
