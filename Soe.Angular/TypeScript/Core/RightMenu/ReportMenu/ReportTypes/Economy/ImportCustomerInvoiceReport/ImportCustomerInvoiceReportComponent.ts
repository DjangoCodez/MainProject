export class ImportCustomerInvoiceReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ImportCustomerInvoiceReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/ImportCustomerInvoiceReport/ImportCustomerInvoiceReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "importCustomerInvoiceReport";

    //@ngInject
    constructor() { }
}
