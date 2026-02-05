import { CalendarUtility } from "../../Util/CalendarUtility";

export class IsSameMonthFilter {

    // Returns all items where date field specified in 'fieldName' has the same month as specified 'date' parameter.

    private static filter(items: any[], fieldName: string, date: Date) {
            var filteredItems: any[] = [];

            _.forEach(items, item => {
                let itemDate: Date = CalendarUtility.convertToDate(item[fieldName]);
                if (itemDate && itemDate.isSameMonthAs(date))
                    filteredItems.push(item);
            });

            return filteredItems;
        }

    public static create() {
        return IsSameMonthFilter.filter;
    }
}
