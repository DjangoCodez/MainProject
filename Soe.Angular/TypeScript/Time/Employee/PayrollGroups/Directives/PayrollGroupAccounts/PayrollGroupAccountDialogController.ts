import { PayrollGroupAccountsDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { AccountSmallDTO } from "../../../../../Common/Models/AccountDTO";

export class PayrollGroupAccountDialogController {

    private accountRow: PayrollGroupAccountsDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        accountRow: PayrollGroupAccountsDTO,
        private accountStds: AccountSmallDTO[],
        private usePayrollTax: boolean) {

        this.isNew = !accountRow;

        this.accountRow = new PayrollGroupAccountsDTO();
        angular.extend(this.accountRow, accountRow);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ accountRow: this.accountRow });
    }
}
