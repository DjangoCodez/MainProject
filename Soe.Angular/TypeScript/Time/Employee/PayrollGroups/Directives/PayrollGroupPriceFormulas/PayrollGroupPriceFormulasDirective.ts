import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { PayrollGroupPriceFormulaDTO, PayrollPriceFormulaResultDTO, PayrollPriceFormulaDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { PayrollGroupPriceFormulaDialogController } from "./PayrollGroupPriceFormulaDialogController";

export class PayrollGroupPriceFormulasDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/PayrollGroups/Directives/PayrollGroupPriceFormulas/Views/PayrollGroupPriceFormulas.html'),
            scope: {
                payrollGroupId: '=',
                priceFormulas: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollGroupPriceFormulasController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollGroupPriceFormulasController {

    // Init parameters
    private payrollGroupId: number;
    private priceFormulas: PayrollGroupPriceFormulaDTO[] = [];

    // Data
    private payrollPriceFormulas: PayrollPriceFormulaDTO[] = [];

    // Flags
    private readOnly: boolean;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private payrollService: IPayrollService) {

        this.$q.all([
            this.loadPayrollPriceFormulas()
        ]).then(() => {
        });
    }

    // SERVICE CALLS

    private loadPayrollPriceFormulas(): ng.IPromise<any> {
        this.payrollPriceFormulas = [];

        return this.payrollService.getPayrollPriceFormulas().then(x => {
            this.payrollPriceFormulas = x;
        });
    }

    // EVENTS

    private editFormula(formula: PayrollGroupPriceFormulaDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollGroups/Directives/PayrollGroupPriceFormulas/Views/PayrollGroupPriceFormulaDialog.html"),
            controller: PayrollGroupPriceFormulaDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                formula: () => { return formula },
                payrollPriceFormulas: () => { return this.getAvailablePayrollPriceFormulas(formula) },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.formula) {
                if (!formula) {
                    // Add new formula to the original collection
                    formula = new PayrollGroupPriceFormulaDTO();
                    this.priceFormulas.push(formula);
                }

                formula.payrollPriceFormulaId = result.formula.payrollPriceFormulaId;
                formula.showOnEmployee = result.formula.showOnEmployee;
                formula.fromDate = result.formula.fromDate;
                formula.toDate = result.formula.toDate;

                // Set name
                let payrollPriceFormula = _.find(this.payrollPriceFormulas, p => p.payrollPriceFormulaId === formula.payrollPriceFormulaId);
                if (payrollPriceFormula) {
                    formula.formulaName = payrollPriceFormula.name;
                    formula.formulaPlain = payrollPriceFormula.formulaPlain;
                }

                this.setAsDirty();
            }
        });
    }

    private deleteFormula(formula: PayrollGroupPriceFormulaDTO) {
        _.pull(this.priceFormulas, formula);

        this.setAsDirty();
    }

    // HELP-METHODS

    private getAvailablePayrollPriceFormulas(formula: PayrollGroupPriceFormulaDTO): PayrollPriceFormulaDTO[] {
        let formulas = _.filter(this.payrollPriceFormulas, p => !_.includes(_.map(this.priceFormulas, pf => pf.payrollPriceFormulaId), p.payrollPriceFormulaId));
        if (formula) {
            let payrollPriceFormula = _.find(this.payrollPriceFormulas, p => p.payrollPriceFormulaId === formula.payrollPriceFormulaId);
            if (payrollPriceFormula)
                formulas.push(payrollPriceFormula);
        }
        return formulas;
    }

    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}