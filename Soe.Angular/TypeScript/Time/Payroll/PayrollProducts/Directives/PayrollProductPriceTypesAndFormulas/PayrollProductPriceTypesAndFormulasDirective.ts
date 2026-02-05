import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IPayrollPriceTypeAndFormulaDTO } from "../../../../../Scripts/TypeLite.Net4";
import { PayrollProductPriceTypeAndFormulaDTO, PayrollProductPriceFormulaDTO, PayrollProductPriceTypeDTO, PayrollProductPriceTypePeriodDTO } from "../../../../../Common/Models/ProductDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { PayrollProductPriceTypesAndFormulasDialogController } from "./PayrollProductPriceTypesAndFormulasDialogController";

export class PayrollProductPriceTypesAndFormulasDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollProducts/Directives/PayrollProductPriceTypesAndFormulas/Views/PayrollProductPriceTypesAndFormulas.html'),
            scope: {
                payrollPriceTypesAndFormulas: '=',
                productPriceTypes: '=',
                productFormulas: '=',
                readOnly: '='
            },
            restrict: 'E',
            replace: true,
            controller: PayrollProductPriceTypesAndFormulasController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollProductPriceTypesAndFormulasController {

    // Init parameters

    private productPriceTypes: PayrollProductPriceTypeDTO[];
    private productFormulas: PayrollProductPriceFormulaDTO[];
    private payrollPriceTypesAndFormulas: IPayrollPriceTypeAndFormulaDTO[];

    // Collections
    private productPriceTypesAndFormulas: PayrollProductPriceTypeAndFormulaDTO[] = [];

    // Flags
    private readOnly: boolean;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private payrollService: IPayrollService) {
    }

    $onInit() {
        this.buildProductPriceTypesAndFormulas();
        this.setupWatchers();
    }

    // SERVICE CALLS    

    // EVENTS

    private setupWatchers() {
        this.$scope.$watchCollection(() => this.productPriceTypesAndFormulas, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.updateProductCollections();
            }
        });
    }
    private updateProductCollections() {
        this.productPriceTypes = this.getProductPriceTypes();
        this.productFormulas = this.getProductFormulas();
    }

    private edit(productPriceTypeAndFormula: PayrollProductPriceTypeAndFormulaDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollProducts/Directives/PayrollProductPriceTypesAndFormulas/Views/PayrollProductPriceTypesAndFormulasDialog.html"),
            controller: PayrollProductPriceTypesAndFormulasDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                productPriceTypeAndFormula: () => { return productPriceTypeAndFormula },
                payrollPriceTypesAndFormulas: () => { return this.payrollPriceTypesAndFormulas },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.productPriceTypeAndFormula) {
                if (!productPriceTypeAndFormula) {
                    // Add new formula to the original collection
                    productPriceTypeAndFormula = new PayrollProductPriceTypeAndFormulaDTO();
                    this.productPriceTypesAndFormulas.push(productPriceTypeAndFormula);
                }

                productPriceTypeAndFormula.name = result.productPriceTypeAndFormula.name
                productPriceTypeAndFormula.payrollPriceFormulaId = result.productPriceTypeAndFormula.payrollPriceFormulaId;
                productPriceTypeAndFormula.payrollPriceTypeId = result.productPriceTypeAndFormula.payrollPriceTypeId
                productPriceTypeAndFormula.fromDate = result.productPriceTypeAndFormula.fromDate;
                productPriceTypeAndFormula.amount = result.productPriceTypeAndFormula.amount;

                this.updateProductCollections();
            }
        });
    }

    private delete(productPriceTypesAndFormula: PayrollProductPriceTypeAndFormulaDTO) {
        _.pull(this.productPriceTypesAndFormulas, productPriceTypesAndFormula);
    }

    // HELP-METHODS

    private buildProductPriceTypesAndFormulas() {
        this.productPriceTypesAndFormulas = [];
        _.forEach(this.productPriceTypes, productPriceType => {
            if (productPriceType.payrollPriceTypeId !== 0) {
                let priceTypeAndFormula = new PayrollProductPriceTypeAndFormulaDTO();
                priceTypeAndFormula.payrollProductPriceTypeId = productPriceType.payrollProductPriceTypeId;
                priceTypeAndFormula.payrollProductPriceTypePeriodId = productPriceType.periods[0].payrollProductPriceTypePeriodId;
                priceTypeAndFormula.payrollPriceTypeId = productPriceType.payrollPriceTypeId;
                priceTypeAndFormula.name = productPriceType.priceTypeName;
                priceTypeAndFormula.fromDate = productPriceType.periods[0].fromDate;
                priceTypeAndFormula.amount = productPriceType.periods[0].amount;

                this.productPriceTypesAndFormulas.push(priceTypeAndFormula)
            }

        });

        _.forEach(this.productFormulas, productFormula => {
            if (productFormula.payrollPriceFormulaId !== 0) {
                let priceTypeAndFormula = new PayrollProductPriceTypeAndFormulaDTO();
                priceTypeAndFormula.payrollProductPriceFormulaId = productFormula.payrollProductPriceFormulaId;
                priceTypeAndFormula.payrollPriceFormulaId = productFormula.payrollPriceFormulaId;
                priceTypeAndFormula.name = productFormula.formulaName;
                priceTypeAndFormula.fromDate = productFormula.fromDate;

                this.productPriceTypesAndFormulas.push(priceTypeAndFormula)
            }
        });
    }

    private getProductPriceTypes(): PayrollProductPriceTypeDTO[] {
        let priceTypes: PayrollProductPriceTypeDTO[] = [];
        _.forEach(_.filter(this.productPriceTypesAndFormulas, x => x.payrollPriceTypeId && x.payrollPriceTypeId !== 0), productPriceTypesAndFormula => {

            let productPriceType = new PayrollProductPriceTypeDTO();
            productPriceType.priceTypeName = productPriceTypesAndFormula.name;
            productPriceType.payrollProductPriceTypeId = productPriceTypesAndFormula.payrollProductPriceTypeId ? productPriceTypesAndFormula.payrollProductPriceTypeId : 0;
            productPriceType.payrollPriceTypeId = productPriceTypesAndFormula.payrollPriceTypeId ? productPriceTypesAndFormula.payrollPriceTypeId : 0
            productPriceType.periods = [];
            let period = new PayrollProductPriceTypePeriodDTO();
            period.payrollProductPriceTypePeriodId = productPriceTypesAndFormula.payrollProductPriceTypePeriodId ? productPriceTypesAndFormula.payrollProductPriceTypePeriodId : 0
            period.payrollProductPriceTypeId = productPriceTypesAndFormula.payrollProductPriceTypeId ? productPriceTypesAndFormula.payrollProductPriceTypeId : 0;
            period.fromDate = productPriceTypesAndFormula.fromDate;
            period.amount = productPriceTypesAndFormula.amount;

            productPriceType.periods.push(period);
            priceTypes.push(productPriceType);

        });

        return priceTypes;
    }

    private getProductFormulas(): PayrollProductPriceFormulaDTO[] {
        let priceFormulas: PayrollProductPriceFormulaDTO[] = [];
        _.forEach(_.filter(this.productPriceTypesAndFormulas, x => x.payrollPriceFormulaId && x.payrollPriceFormulaId !== 0), productPriceTypesAndFormula => {
            let productPriceFormula = new PayrollProductPriceFormulaDTO();
            productPriceFormula.formulaName = productPriceTypesAndFormula.name;
            productPriceFormula.payrollProductPriceFormulaId = productPriceTypesAndFormula.payrollProductPriceFormulaId ? productPriceTypesAndFormula.payrollProductPriceFormulaId : 0;
            productPriceFormula.payrollPriceFormulaId = productPriceTypesAndFormula.payrollPriceFormulaId ? productPriceTypesAndFormula.payrollPriceFormulaId : 0;
            productPriceFormula.fromDate = productPriceTypesAndFormula.fromDate;

            priceFormulas.push(productPriceFormula);
        });
        return priceFormulas
    }
}