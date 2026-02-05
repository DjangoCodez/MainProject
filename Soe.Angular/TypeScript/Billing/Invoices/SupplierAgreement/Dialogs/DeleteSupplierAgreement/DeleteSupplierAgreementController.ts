export class DeleteSupplierAgreementController {
    private wholesellerId: number;
    private pricelistTypeId: number;

    get enableSave(): boolean {
        return this.wholesellerId && this.wholesellerId > 0;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private wholesellers: any[],
        private pricelistTypes: any[]) {
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ wholesellerId: this.wholesellerId, pricelistId: this.pricelistTypeId });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}