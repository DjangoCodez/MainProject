export class ExpenseReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: ExpenseReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/ExpenseReport/ExpenseReportView.html",
            bindings: {
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "expenseReport";

    //@ngInject
    constructor() { }
}
