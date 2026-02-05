import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { PayrollGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../PayrollService";
import { IEmployeeDTO, IEmployeeSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { TimePeriodHeadDTO } from "../../../../../Common/Models/TimePeriodHeadDTO";
import { TermGroup_TimePeriodType } from "../../../../../Util/CommonEnumerations";
import { TimePeriodDTO } from "../../../../../Common/Models/TimePeriodDTO";


export class PayrollEmployeesSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollEmployeesSelection,
            templateUrl: soeConfig.baseUrl + "Time/Payroll/Payment/Directives/PayrollEmployeesSelection/PayrollEmployeesSelectionView.html",
            bindings: {
                onSelected: "&",
                timePeriodId: "<"
            }
        };

        return options;
    }
    public static componentKey = "payrollEmployeesSelection";

    // Terms
    private terms: { [index: string]: string; };

    private onSelected: (_: { selections: IdListSelectionDTO }) => void = angular.noop;
    private timePeriodId: number;

    private timePeriodHeads: TimePeriodHeadDTO[];
    private selectedTimePeriod: TimePeriodDTO;
    private payrollGroups: PayrollGroupDTO[];
    private selectedPayrollGroupId: number;
    private employees: IEmployeeSmallDTO[];
    private selectedEmployees: any[] = [];

    private populating: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadTimePeriods()
        ]).then(() => {
            this.loadPayrollGroups();
        });
    }

    public $onChanges(objChanged) {
        // Reload employees if time period changes
        if (objChanged.hasOwnProperty('timePeriodId')) {
            this.setSelectedTimePeriod();
            if (this.timePeriodId) {
                this.loadEmployees();
            } else {
                this.employees = [];
            }
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.all"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTimePeriods(): ng.IPromise<any> {
        return this.payrollService.getTimePeriodHeads(TermGroup_TimePeriodType.Payroll, false, true).then(x => {
            this.timePeriodHeads = x;
        });
    }

    private loadPayrollGroups(): ng.IPromise<any> {
        return this.payrollService.getPayrollGroups().then(x => {
            this.payrollGroups = x;

            let group = new PayrollGroupDTO();
            group.payrollGroupId = 0;
            group.name = this.terms["common.all"];
            this.payrollGroups.splice(0, 0, group);
            this.selectedPayrollGroupId = 0;
        })
    }

    private loadEmployees(): ng.IPromise<any> {
        this.populating = true;

        return this.payrollService.getEmployeesWithPayrollExport(this.timePeriodId, this.selectedPayrollGroupId || 0).then(x => {
            this.employees = x.map(employee => ({
                ...employee,
                nrAndName: `${employee.employeeNr} ${employee.name}`
            }));
            this.selectedEmployees = [];
            this.populating = false;
        });
    }

    // EVENTS

    private onPayrollGroupSelected() {
        this.$timeout(() => {
            this.loadEmployees();
        })
    }

    private propagateEmployeeSelection() {
        const selections = new IdListSelectionDTO(this.selectedEmployees.map(e => e.employeeId));

        this.onSelected({ selections });
    }

    // HELP-METHODS

    private setSelectedTimePeriod() {
        this.selectedTimePeriod = null;

        if (this.timePeriodId) {
            _.forEach(this.timePeriodHeads, h => {
                let period = _.find(h.timePeriods, p => p.timePeriodId === this.timePeriodId);
                if (period) {
                    this.selectedTimePeriod = period;
                    return false;
                }
            });
        }
    }
}