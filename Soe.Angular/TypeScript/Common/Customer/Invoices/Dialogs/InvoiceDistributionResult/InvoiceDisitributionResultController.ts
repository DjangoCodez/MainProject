import { IActionResult, ICustomerInvoiceDistributionResultDTO } from "../../../../../Scripts/TypeLite.Net4";

export class InvoiceDistributionResultController {

    //@ngInject
    constructor(private $uibModalInstance, private result: ICustomerInvoiceDistributionResultDTO) {
        this.setIcon(result.printResult);
        this.setIcon(result.eInvoiceResult);
        this.setIcon(result.emailResult);
    }

    setIcon(result: IActionResult) {
        result["icon"] = result.success ? 'fal fa-check' : 'fal fa-times';
    }

    buttonOkClick() {
        this.$uibModalInstance.close();
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}