export class TimeReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: TimeReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/TimeReport/TimeReportView.html",
            bindings: {
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "timeReport";

    //@ngInject
    constructor() { }
}