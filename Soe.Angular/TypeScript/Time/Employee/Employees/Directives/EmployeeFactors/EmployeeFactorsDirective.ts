import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmployeeFactorDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { EmployeeFactorDialogController } from "./EmployeeFactorDialogController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { TermGroup_EmployeeFactorType, TermGroup, TermGroup_VacationGroupVacationHandleRule, TermGroup_VacationGroupVacationDaysHandleRule } from "../../../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { VacationGroupDTO } from "../../../../../Common/Models/VacationGroupDTO";
import { EmployeeService } from "../../../EmployeeService";

export class EmployeeFactorsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeFactors/Views/EmployeeFactors.html'),
            scope: {
                factors: '=',
                vacationGroupId: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeFactorsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeFactorsController {
    // Data
    private types: SmallGenericType[] = [];
    private factors: EmployeeFactorDTO[];
    private filteredFactors: EmployeeFactorDTO[];
    private selectedFactor: EmployeeFactorDTO;
    private vacationGroupId: number;
    private vacationGroup: VacationGroupDTO;

    // Flags
    private readOnly: boolean;
    private showAllGenerations: boolean = false;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private employeeService: EmployeeService) {

        this.$q.all([
            this.loadFactorTypes()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.factors, (newVal, oldVal) => {
            this.setFilteredFactors();
            this.selectedFactor = this.factors && this.factors.length > 0 ? _.orderBy(this.factors, ['sortableDate'], ['desc'])[0] : null;
        });
        this.$scope.$watch(() => this.vacationGroupId, (newVal, oldVal) => {
            // Timing issue!
            // Sometimes vacationGroupId is set after factors and oldVal === newVal, then vacationGroup will not be loaded
            if (newVal !== oldVal || (newVal && !this.vacationGroup)) {
                this.loadVacationGroup().then(() => {
                    this.setFilteredFactors();
                });
            }
        });
    }

    // SERVICE CALLS

    private loadFactorTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeFactorType, false, true).then(x => {
            this.types = x;
        });
    }

    private loadVacationGroup(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (!this.vacationGroupId) {
            this.vacationGroup = null;
            deferral.resolve();
        } else {
            return this.employeeService.getVacationGroup(this.vacationGroupId).then(x => {
                this.vacationGroup = x;
            });
        }

        return deferral.promise;
    }

    // EVENTS

    private showAllGenerationsChanged() {
        this.$timeout(() => {
            this.setFilteredFactors();
        });
    }

    private editFactor(factor: EmployeeFactorDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmployeeFactors/Views/EmployeeFactorDialog.html"),
            controller: EmployeeFactorDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                types: () => { return this.getValidTypesSelections() },
                factor: () => { return factor },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.factor) {
                if (!factor) {
                    // Add new factor to the original collection
                    factor = new EmployeeFactorDTO();
                    this.updateFactor(factor, result.factor);
                    this.factors.push(factor);
                } else {
                    // Update original factor
                    var originalFactor = _.find(this.factors, v => v.employeeFactorId === factor.employeeFactorId);
                    if (originalFactor)
                        this.updateFactor(originalFactor, result.factor);
                }

                this.setFilteredFactors();

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private updateFactor(factor: EmployeeFactorDTO, input: EmployeeFactorDTO) {
        factor.type = input.type;
        factor.fromDate = input.fromDate;
        factor.factor = input.factor;
        factor.vacationGroupId = this.isTypeBoundToVacationGroup(factor.type) && this.vacationGroup && this.vacationGroup.vacationGroupId ? this.vacationGroup.vacationGroupId : null;

        let type = this.getType(factor.type);
        factor.typeName = type ? type.name : '';
    }

    private deleteFactor(factor: EmployeeFactorDTO) {
        _.pull(this.factors, factor);
        this.setFilteredFactors();

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setFilteredFactors() {
        var vacationGroupId = this.vacationGroup ? this.vacationGroup.vacationGroupId : 0;

        // Reset current
        _.forEach(this.factors, factor => {
            factor.isCurrent = false;
        });

        // Set current
        var tmpFactors = _.filter(this.factors, f => (f.vacationGroupId === vacationGroupId || !f.vacationGroupId) && (!f.fromDate || f.fromDate.isSameOrBeforeOnDay(CalendarUtility.getDateToday())));
        var types: TermGroup_EmployeeFactorType[] = _.uniq(_.map(tmpFactors, f => f.type));
        _.forEach(types, type => {
            let factorsOfType = _.filter(tmpFactors, f => f.type == type);
            if (factorsOfType.length > 0)
                _.orderBy(factorsOfType, 'sortableDate', 'desc')[0].isCurrent = true;
        });

        var validTypes: TermGroup_EmployeeFactorType[] = this.getValidTypes();
        this.filteredFactors = _.orderBy(_.filter(this.factors, f => (f.vacationGroupId === vacationGroupId || !f.vacationGroupId) && _.includes(validTypes, f.type) && (this.showAllGenerations || f.isCurrent)), ['type', 'isCurrent', 'sortableDate'], ['asc', 'desc', 'desc']);
    }

    private getValidTypesSelections() {
        var validTypesSelection: SmallGenericType[] = [];
        var validTypes = this.getValidTypes();
        _.forEach(this.types, t => {
            if (_.includes(validTypes, t.id))
                validTypesSelection.push(t);
        });
        return validTypesSelection;
    }

    private getValidTypes(): TermGroup_EmployeeFactorType[] {
        // Get settings from vacation group
        var vacationHandleRule: TermGroup_VacationGroupVacationHandleRule = TermGroup_VacationGroupVacationHandleRule.Unknown;
        var vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule = TermGroup_VacationGroupVacationDaysHandleRule.Unknown;
        if (this.vacationGroup && this.vacationGroup.vacationGroupSE) {
            vacationHandleRule = this.vacationGroup.vacationGroupSE.vacationHandleRule;
            vacationDaysHandleRule = this.vacationGroup.vacationGroupSE.vacationDaysHandleRule;
        }

        var list: number[] = [];

        //switch (DisplayMode) {
        //    case EmployeeFactorDisplayMode.Absence:
        this.setValidType(list, TermGroup_EmployeeFactorType.CalendarDayFactor);
        this.setValidType(list, TermGroup_EmployeeFactorType.BalanceLasDays);
        this.setValidType(list, TermGroup_EmployeeFactorType.CurrentLasDays);
        this.setValidType(list, TermGroup_EmployeeFactorType.BalanceLasDaysAva);  
        this.setValidType(list, TermGroup_EmployeeFactorType.BalanceLasDaysSva);
        this.setValidType(list, TermGroup_EmployeeFactorType.BalanceLasDaysVik);
        this.setValidType(list, TermGroup_EmployeeFactorType.TimeWorkAccountPaidLeave);

        //    break;
        //case EmployeeFactorDisplayMode.Vacation:
        if (vacationDaysHandleRule == TermGroup_VacationGroupVacationDaysHandleRule.VacationFactor || vacationDaysHandleRule == TermGroup_VacationGroupVacationDaysHandleRule.VacationCoefficient)
            this.setValidType(list, TermGroup_EmployeeFactorType.VacationCoefficient);
        if (vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Hours)
            this.setValidType(list, TermGroup_EmployeeFactorType.AverageWorkTimeWeek);
        if (vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Shifts)
            this.setValidType(list, TermGroup_EmployeeFactorType.AverageWorkTimeShift);
        this.setValidType(list, TermGroup_EmployeeFactorType.Net);
        if (this.vacationGroup) {
            this.setValidType(list, TermGroup_EmployeeFactorType.VacationDaysPaidByLaw);
            if (vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Days || vacationHandleRule == TermGroup_VacationGroupVacationHandleRule.Unknown) {
                this.setValidType(list, TermGroup_EmployeeFactorType.VacationDayPercent);
                this.setValidType(list, TermGroup_EmployeeFactorType.VacationDayPercentFinalSalary);
            }
            else {
                this.setValidType(list, TermGroup_EmployeeFactorType.VacationHoursPaid);
                this.setValidType(list, TermGroup_EmployeeFactorType.VacationHourPercent);
            }
            this.setValidType(list, TermGroup_EmployeeFactorType.VacationVariablePercent);
            this.setValidType(list, TermGroup_EmployeeFactorType.VacationVariableAmountPerDay);
            this.setValidType(list, TermGroup_EmployeeFactorType.GuaranteeAmount);
        }
        //        break;
        //}

        return list;
    }

    private setValidType(list: number[], type: TermGroup_EmployeeFactorType) {
        if (this.getType(type))
            list.push(type);
    }

    private getType(type: TermGroup_EmployeeFactorType) {
        return _.find(this.types, t => t.id == type);
    }

    private isTypeBoundToVacationGroup(type: TermGroup_EmployeeFactorType): boolean {
        switch (type) {
            case TermGroup_EmployeeFactorType.VacationDaysPaidByLaw:
            case TermGroup_EmployeeFactorType.VacationDayPercent:
            case TermGroup_EmployeeFactorType.VacationHourPercent:
            case TermGroup_EmployeeFactorType.VacationVariablePercent:
            case TermGroup_EmployeeFactorType.GuaranteeAmount:
            case TermGroup_EmployeeFactorType.VacationVariableAmountPerDay:
                return true;
            default:
                return false;
        }
    }
}