export class TimeSalaryControlInfoReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeSalaryControlInfoReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeSalaryControlInfoReport/TimeSalaryControlInfoReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeSalaryControlInfoReport";

    //@ngInject
    constructor() { }
}
