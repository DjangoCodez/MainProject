import { PayrollGroupPriceFormulaDTO, PayrollPriceFormulaDTO } from "../../../../../Common/Models/PayrollGroupDTOs";

export class PayrollGroupPriceFormulaDialogController {

    private formula: PayrollGroupPriceFormulaDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        formula: PayrollGroupPriceFormulaDTO,
        private payrollPriceFormulas: PayrollPriceFormulaDTO[]) {

        this.isNew = !formula;

        this.formula = new PayrollGroupPriceFormulaDTO();
        angular.extend(this.formula, formula);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ formula: this.formula });
    }
}
