export class InterestEstimateReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: InterestEstimateReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/InterestEstimateReport/InterestEstimateReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "interestEstimateReport";

    //@ngInject
    constructor() { }
}
