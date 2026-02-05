export class SupplierInvoiceEdiReport {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: SupplierInvoiceEdiReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/SupplierInvoiceEdiReport/SupplierInvoiceEdiReportView.html",
            bindings: {
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "supplierInvoiceEdiReport";

    //@ngInject
    constructor() { }
}
