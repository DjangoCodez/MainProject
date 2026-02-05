export class DemandLetterReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: DemandLetterReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/DemandLetterReport/DemandLetterReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "demandLetterReport";

    //@ngInject
    constructor() { }
}
