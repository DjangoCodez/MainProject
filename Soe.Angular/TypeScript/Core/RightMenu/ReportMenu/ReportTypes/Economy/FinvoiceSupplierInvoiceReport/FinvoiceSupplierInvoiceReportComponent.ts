export class FinvoiceSupplierInvoiceReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: FinvoiceSupplierInvoiceReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/FinvoiceSupplierInvoiceReport/FinvoiceSupplierInvoiceReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "finvoiceSupplierInvoiceReport";

    //@ngInject
    constructor() { }
}
