export class OrderChecklistReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: OrderChecklistReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/OrderChecklistReport/OrderChecklistReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "orderChecklistReport";

    //@ngInject
    constructor() { }
}
