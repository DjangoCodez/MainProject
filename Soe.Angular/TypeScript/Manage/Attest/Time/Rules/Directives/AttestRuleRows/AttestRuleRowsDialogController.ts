import { AttestRuleRowDTO } from "../../../../../../Common/Models/AttestRuleHeadDTO";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";

export class AttestRuleRowsDialogController {

    private row: AttestRuleRowDTO;
    private isNew: boolean;
    private isNegative: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout: ng.ITimeoutService,
        row: AttestRuleRowDTO,
        private comparisonOperators: SmallGenericType[],
        private leftOperators: SmallGenericType[],
        private rightOperators: SmallGenericType[],
        private timeCodes: SmallGenericType[],
        private payrollProducts: SmallGenericType[],
        private invoiceProducts: SmallGenericType[]) {

        this.isNew = !row;

        this.row = new AttestRuleRowDTO();
        angular.extend(this.row, row);

        this.minutesChanged();
    }

    // EVENTS

    private leftOperatorChanged() {
        this.$timeout(() => {
            this.row.setLeftValueTypeName(this.leftOperators);
        });
    }

    private rightOperatorChanged() {
        this.$timeout(() => {
            this.row.setRightValueTypeName(this.rightOperators);
        });
    }

    private togglePlusMinus() {
        this.isNegative = !this.isNegative;
    }

    private minutesChanged() {
        this.$timeout(() => {
            if (!this.row.minutes)
                this.row.minutes = 0;

            if (this.row.minutes < 0) {
                this.isNegative = true;
                this.row.minutes = Math.abs(this.row.minutes);
            }
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        if (this.isNegative)
            this.row.minutes = -Math.abs(this.row.minutes);

        this.$uibModalInstance.close({ row: this.row });
    }
}
