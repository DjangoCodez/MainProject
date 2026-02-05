export class ImportVoucherReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ImportVoucherReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/ImportVoucherReport/ImportVoucherReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "importVoucherReport";

    //@ngInject
    constructor() { }
}
