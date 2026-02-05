import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { ITimeService } from "../../../../Time/TimeService";
import {  TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import angular from "angular";
import { TimeWorkReductionEarningGroupRulesController } from "./TimeWorkReductionEarningGroupRulesController";

export class TimeWorkReductionEarningGroupRulesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeAccumulators/Directives/TimeWorkReductionEarningGroupRules/TimeWorkReductionEarningGroupRules.html'),
            scope: {
                rules: '=',
                readOnly: '=',
                onChange: '&',
            },
            restrict: 'E',
            replace: true, 
            controller: TimeWorkReductionEarningGroupRulesDialogController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class TimeWorkReductionEarningGroupRulesDialogController {

    // Init parameters
    private rules: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedRule: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO;
    private employeeGroups: ISmallGenericType[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private urlHelperService: IUrlHelperService) {

        this.$q.all([
            this.loadEmployeeGroups(),
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

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(true).then(x => {
            this.employeeGroups = x;
        });
    }

    // EVENTS

    private editRule(rule: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAccumulators/Directives/TimeWorkReductionEarningGroupRules/TimeWorkReductionEarningGroupRulesDialog.html"),
            controller: TimeWorkReductionEarningGroupRulesController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                employeeGroups: () => { return this.employeeGroups },
                rule: () => { return rule },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.rule) {
                if (!rule) {
                    // Add new
                    rule = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO();
                    if (!this.rules)
                        this.rules = [];
                    this.rules.push(rule);
                }

                rule.employeeGroupId = result.rule.employeeGroupId;
                rule.dateFrom = result.rule.dateFrom;
                rule.dateTo = result.rule.dateTo;

                this.selectedRule = rule;
                this.setRuleName(rule);
                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteRule(rule: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO) {
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

    private setRuleName(rule: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO) {
     
        let group = _.find(this.employeeGroups, e => e.id === rule.employeeGroupId);
        rule.employeeGroupName = group ? group.name : '';

    }
}