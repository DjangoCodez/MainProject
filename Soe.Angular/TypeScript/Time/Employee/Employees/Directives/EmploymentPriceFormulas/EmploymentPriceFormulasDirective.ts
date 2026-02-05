import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { PayrollGroupPriceFormulaDTO, PayrollPriceFormulaResultDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class EmploymentPriceFormulasDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmploymentPriceFormulas/Views/EmploymentPriceFormulas.html'),
            scope: {
                employmentId: '=',
                payrollGroupId: '='
            },
            restrict: 'E',
            replace: true,
            controller: EmploymentPriceFormulasController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmploymentPriceFormulasController {

    // Init parameters
    private employmentId: number;
    private payrollGroupId: number;

    // Data
    private priceFormulas: PayrollGroupPriceFormulaDTO[] = [];

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private payrollService: IPayrollService) {

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employmentId, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.evaluateFormulas();
            }
        });
        this.$scope.$watch(() => this.payrollGroupId, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.loadPriceFormulas();
            }
        });
    }

    // SERVICE CALLS

    private loadPriceFormulas() {
        this.priceFormulas = [];

        if (this.payrollGroupId) {
            this.payrollService.getPayrollGroupPriceFormulas(this.payrollGroupId, true).then(x => {
                this.priceFormulas = x;
                this.evaluateFormulas();

                // TODO: Show result column only if any of the results are not zero
            });
        }
    }

    private evaluateFormulas() {
        _.forEach(this.priceFormulas, formula => {
            this.evaluateFormula(formula);
        });
    }

    private evaluateFormula(formula: PayrollGroupPriceFormulaDTO) {
        this.payrollService.evaluateFormulaGivenEmployment(CalendarUtility.getDateToday(), this.employmentId, 0, formula.payrollGroupPriceFormulaId).then((result: PayrollPriceFormulaResultDTO) => {
            if (result) {
                formula.formulaPlain = result.formulaPlain;
                formula.formulaNames = result.formulaNames;
                formula.formulaExtracted = result.formulaExtracted;
                formula.result = result.amount;
            }
        });
    }
}