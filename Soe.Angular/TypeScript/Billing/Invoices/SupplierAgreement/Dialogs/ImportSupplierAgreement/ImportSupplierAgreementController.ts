export class ImportSupplierAgreementController {
    private bytes: any;
    private filename: string;
    private generalDiscount: number;
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

    fileUploaded(result: any) {
        if (result) {
            this.bytes = result.array;
            this.filename = result.fileName;
        }
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ bytes: this.bytes, wholesellerId: this.wholesellerId, pricelistId: this.pricelistTypeId, generalDiscount: this.generalDiscount ? this.generalDiscount : 0 });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}