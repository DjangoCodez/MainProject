export class TimeSalarySpecificationReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeSalarySpecificationReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeSalarySpecificationReport/TimeSalarySpecificationReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeSalarySpecificationReport";

    //@ngInject
    constructor() { }
}
