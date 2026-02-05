import { PayrollProductPriceTypeAndFormulaDTO} from "../../../../../Common/Models/ProductDTOs";
import { IPayrollPriceTypeAndFormulaDTO } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollProductPriceTypesAndFormulasDialogController {

    private productPriceTypeAndFormula: PayrollProductPriceTypeAndFormulaDTO;
    private isNew: boolean;

    private _selectedProductPriceTypesAndFormula: IPayrollPriceTypeAndFormulaDTO;
    private get selectedProductPriceTypesAndFormula(): IPayrollPriceTypeAndFormulaDTO {
        return this._selectedProductPriceTypesAndFormula;
    }
    private set selectedProductPriceTypesAndFormula(item: IPayrollPriceTypeAndFormulaDTO) {
        this._selectedProductPriceTypesAndFormula = item;

        this.productPriceTypeAndFormula.name = item ? item.name : "";
        this.productPriceTypeAndFormula.payrollPriceFormulaId = item ? item.payrollPriceFormulaId : 0;
        this.productPriceTypeAndFormula.payrollPriceTypeId = item ? item.payrollPriceTypeId : 0;
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        productPriceTypeAndFormula: PayrollProductPriceTypeAndFormulaDTO,
        private payrollPriceTypesAndFormulas: IPayrollPriceTypeAndFormulaDTO[]) {
     
        this.isNew = !productPriceTypeAndFormula;

        this.productPriceTypeAndFormula = new PayrollProductPriceTypeAndFormulaDTO();
        angular.extend(this.productPriceTypeAndFormula, productPriceTypeAndFormula);
       
    }

    $onInit() {
        if (this.productPriceTypeAndFormula.payrollPriceTypeId && this.productPriceTypeAndFormula.payrollPriceTypeId !== 0)
            this.selectedProductPriceTypesAndFormula = _.find(this.payrollPriceTypesAndFormulas, t => t.payrollPriceTypeId === this.productPriceTypeAndFormula.payrollPriceTypeId);
        else if (this.productPriceTypeAndFormula.payrollPriceFormulaId && this.productPriceTypeAndFormula.payrollPriceFormulaId !== 0)
            this.selectedProductPriceTypesAndFormula = _.find(this.payrollPriceTypesAndFormulas, t => t.payrollPriceFormulaId === this.productPriceTypeAndFormula.payrollPriceFormulaId);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ productPriceTypeAndFormula: this.productPriceTypeAndFormula });
    }
}
