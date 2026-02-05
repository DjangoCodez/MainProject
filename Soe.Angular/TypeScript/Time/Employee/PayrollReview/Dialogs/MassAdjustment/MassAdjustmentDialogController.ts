export class MassAdjustmentDialogController {

    private value: number = 0;
    private adjustmentType: number = 1;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private nbrOfEmployeesSelected:number) {
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ value: this.value, adjustmentType: this.adjustmentType });
    }
}
