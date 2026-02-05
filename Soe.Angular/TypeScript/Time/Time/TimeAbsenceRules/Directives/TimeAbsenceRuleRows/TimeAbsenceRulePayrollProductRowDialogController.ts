import { TimeAbsenceRuleRowPayrollProductsDTO } from "../../../../../Common/Models/TimeAbsenceRuleHeadDTO";
import { PayrollProductDTO } from "../../../../../Common/Models/ProductDTOs";

export class TimeAbsenceRulePayrollProductRowDialogController {

    private payrollProductRow: TimeAbsenceRuleRowPayrollProductsDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        payrollProductRow: TimeAbsenceRuleRowPayrollProductsDTO,
        private payrollProducts: PayrollProductDTO[]) {

        this.isNew = !payrollProductRow;

        this.payrollProductRow = new TimeAbsenceRuleRowPayrollProductsDTO();
        angular.extend(this.payrollProductRow, payrollProductRow);
    }

    private isValid(): boolean {
        if (!this.payrollProductRow)
            return false;
        if (!this.payrollProductRow.sourcePayrollProductId)
            return false;
        if (!this.payrollProductRow.targetPayrollProductId)
            return false;
        return true;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ payrollProductRow: this.payrollProductRow });
    }
}
