export class GenericReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: GenericReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Generic/GenericReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "genericReport";

    //@ngInject
    constructor() { }
}
