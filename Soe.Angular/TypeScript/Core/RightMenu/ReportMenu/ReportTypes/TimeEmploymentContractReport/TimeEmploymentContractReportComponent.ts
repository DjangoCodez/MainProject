export class TimeEmploymentContractReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeEmploymentContractReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TimeEmploymentContractReport/TimeEmploymentContractReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "timeEmploymentContractReport";

    //@ngInject
    constructor() { }
}
