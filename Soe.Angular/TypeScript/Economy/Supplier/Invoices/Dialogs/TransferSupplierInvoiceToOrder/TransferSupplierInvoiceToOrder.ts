export class TransferSupplierInvoiceToOrderController {

    //@ngInject
    constructor(private $uibModalInstance, private transferSupplierInvoiceRows: boolean, private useMiscProduct: boolean, private infoText: string) {
    }
    buttonOkClick() {
        this.$uibModalInstance.close({ transferSupplierInvoiceRows: this.transferSupplierInvoiceRows, useMiscProduct: this.useMiscProduct });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}