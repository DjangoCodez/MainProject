import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { ITimeService } from "../../../../Time/TimeService";
import { TimeAccumulatorEmployeeGroupRuleDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";
import { SoeTimeCodeType, TermGroup } from "../../../../../Util/CommonEnumerations";
import { EmployeeGroupRuleDialogController } from "./EmployeeGroupRuleController";
import { ICoreService } from "../../../../../Core/Services/CoreService";

export class EmployeeGroupRulesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeAccumulators/Directives/EmployeeGroupRules/EmployeeGroupRules.html'),
            scope: {
                rules: '=',
                useTimeWorkReductionWithdrawal: '=',
                readOnly: '=',
                onChange: '&',
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeGroupRulesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeGroupRulesController {

    // Init parameters
    private rules: TimeAccumulatorEmployeeGroupRuleDTO[];
    private useTimeWorkReductionWithdrawal: boolean;
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedRule: TimeAccumulatorEmployeeGroupRuleDTO;
    private types: ISmallGenericType[];
    private employeeGroups: ISmallGenericType[];
    private timeCodes: ISmallGenericType[];
    private scheduledJobHeads: ISmallGenericType[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private urlHelperService: IUrlHelperService) {

        this.$q.all([
            this.loadTypes(),
            this.loadEmployeeGroups(),
            this.loadTimeCodes(),
            this.loadScheduledJobs()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.rules, (newVal, oldVal) => {
            this.selectedRule = this.rules && this.rules.length > 0 ? this.rules[0] : null;
            this.setRuleNames();
        });
    }

    // SERVICE CALLS

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccumulatorTimePeriodType, true, true).then(x => {
            this.types = x;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(true).then(x => {
            this.employeeGroups = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.None, true, false).then(x => {
            this.timeCodes = x;
        });
    }

    private loadScheduledJobs(): ng.IPromise<any> {
        this.scheduledJobHeads = [];
        return this.coreService.getScheduledJobHeadsDict(true, false).then(x => {
            this.scheduledJobHeads = x;
        });
    }

    // EVENTS

    private editRule(rule: TimeAccumulatorEmployeeGroupRuleDTO) {
        if (rule && rule.thresholdMinutes === undefined) 
            rule.thresholdMinutes = 0;

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAccumulators/Directives/EmployeeGroupRules/EmployeeGroupRuleDialog.html"),
            controller: EmployeeGroupRuleDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                types: () => { return this.types },
                employeeGroups: () => { return this.employeeGroups },
                timeCodes: () => { return this.timeCodes },
                scheduledJobHeads: () => { return this.scheduledJobHeads },
                rule: () => { return rule },
                useTimeWorkReductionWithdrawal: () => { return this.useTimeWorkReductionWithdrawal }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.rule) {
                if (!rule) {
                    // Add new
                    rule = new TimeAccumulatorEmployeeGroupRuleDTO();
                    if (!this.rules)
                        this.rules = [];
                    this.rules.push(rule);
                }
                if (result.rule && result.rule.thresholdMinutes === undefined || result.rule.thresholdMinutes === null)
                    result.rule.thresholdMinutes = 0;

                // Update fields
                rule.type = result.rule.type;
                rule.employeeGroupId = result.rule.employeeGroupId;
                rule.minMinutes = result.rule.minMinutes;
                rule.minTimeCodeId = result.rule.minTimeCodeId;
                rule.maxMinutes = result.rule.maxMinutes;
                rule.maxTimeCodeId = result.rule.maxTimeCodeId;
                rule.showOnPayrollSlip = result.rule.showOnPayrollSlip;
                rule.minMinutesWarning = result.rule.minMinutesWarning;
                rule.maxMinutesWarning = result.rule.maxMinutesWarning;
                rule.scheduledJobHeadId = result.rule.scheduledJobHeadId;
                rule.thresholdMinutes = result.rule.thresholdMinutes;
                this.setRuleName(rule);
                this.selectedRule = rule;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteRule(rule: TimeAccumulatorEmployeeGroupRuleDTO) {
        _.pull(this.rules, rule);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setRuleNames() {
        _.forEach(this.rules, rule => {
            this.setRuleName(rule);
        });
    }

    private setRuleName(rule: TimeAccumulatorEmployeeGroupRuleDTO) {
        let type = _.find(this.types, t => t.id === rule.type);
        rule.typeName = type ? type.name : '';

        let group = _.find(this.employeeGroups, e => e.id === rule.employeeGroupId);
        rule.employeeGroupName = group ? group.name : '';

        let minTC = _.find(this.timeCodes, min => min.id === rule.minTimeCodeId);
        rule.minTimeCodeName = minTC ? minTC.name : '';

        let maxTC = _.find(this.timeCodes, max => max.id === rule.maxTimeCodeId);
        rule.maxTimeCodeName = maxTC ? maxTC.name : '';
    }
}