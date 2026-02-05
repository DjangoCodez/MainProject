export class PreviousPeriodResultController {
    public result: any;
    public getPeriodResult: string;
    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, result: any, terms) {
        this.result = result;
        this.getPeriodResult = terms["economy.accounting.budget.getresultperiod"];
    }

    public no() {
        this.result.getPreviousPeriodResult = false;
        this.$uibModalInstance.close(this.result);
    }

    public yes() {
        this.result.getPreviousPeriodResult = true;
        this.$uibModalInstance.close(this.result);
    }
}