import { ICoreService } from "../../../../../../Core/Services/CoreService";

export class CopyProductController {

    //@ngInject
    constructor(
        private $uibModalInstance,
        private coreService: ICoreService,
        private copyPrices: boolean,
        private copyAccounts: boolean,
        private copyStock: boolean) {
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ copyPrices: this.copyPrices, copyAccounts: this.copyAccounts, copyStock: this.copyStock });
    }
}