export class SalesBudgetPreviousResultController {
    public result: any;
    public getResult: string;
    public getResultInfo: string;
    public includeCountDim: string;
    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, result: any, terms, public dim2Name: string, public dim3Name: string) {
        this.result = result;
        this.getResult = terms["economy.accounting.budget.getresult"];
        this.getResultInfo = terms["economy.accounting.budget.getresultinfotext"];
        this.includeCountDim = terms["economy.accounting.budget.includecountdim"];
    }

    public no() {
        this.result.getPreviousResult = false;
        this.$uibModalInstance.close(this.result);
    }

    public yes() {
        this.result.getPreviousResult = true;
        this.$uibModalInstance.close(this.result);
    }
}
