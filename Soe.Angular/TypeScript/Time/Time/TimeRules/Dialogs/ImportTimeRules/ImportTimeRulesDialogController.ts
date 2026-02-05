import { ModalUtility } from "../../../../../Util/ModalUtility";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IScheduleService } from "../../../../Schedule/ScheduleService";
import { ITimeService } from "../../../Timeservice";
import { TimeRuleEditDTO, TimeRuleExportImportUnmatchedDTO, TimeRuleExportImportDTO } from "../../../../../Common/Models/TimeRuleDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { TermGroup, SoeTimeCodeType, TimeRuleExportImportUnmatchedType } from "../../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButton, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { IValidationSummaryHandlerFactory } from "../../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IValidationSummaryHandler } from "../../../../../Core/Handlers/ValidationSummaryHandler";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";

export class ImportTimeRulesDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private types: ISmallGenericType[] = [];
    private directions: ISmallGenericType[] = [];
    private timeCodes: ISmallGenericType[] = [];
    private employeeGroups: ISmallGenericType[] = [];
    private scheduleTypes: ISmallGenericType[] = [];
    private timeDeviationCauses: ISmallGenericType[] = [];
    private dayTypes: ISmallGenericType[] = [];

    // Flags
    private showExpressions: boolean = false;
    private showExpressionsOnlyDiff: boolean = false;
    private hasUnmatchedExpressions: boolean = false;

    private validationHandler: IValidationSummaryHandler;
    private edit: ng.IFormController;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private timeService: ITimeService,
        private importResult: TimeRuleExportImportDTO) {

        if (validationSummaryHandlerFactory)
            this.validationHandler = validationSummaryHandlerFactory.create();

        this.hasUnmatchedExpressions = _.filter(this.importResult.timeRules, r => r.hasUnmatchedExpression).length > 0;
        this.showExpressions = this.hasUnmatchedExpressions;
        this.showExpressionsOnlyDiff = this.hasUnmatchedExpressions;

        this.loadTerms().then(() => {
            this.$q.all([
                this.loadTypes(),
                this.loadDirections(),
                this.loadTimeCodes(),
                this.loadEmployeeGroups(),
                this.loadScheduleTypes(),
                this.loadTimeDeviationCauses(),
                this.loadDayTypes()
            ]).then(() => {
                this.setSelectedOptions();
            });
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.continue",
            "core.warning",
            "common.all",
            "common.code",
            "common.employeegroups",
            "common.name",
            "common.timecode",
            "time.schedule.daytype.daytypes",
            "time.schedule.planning.deviationcauses",
            "time.schedule.scheduletype.scheduletypes",
            "time.time.timerule.scheduletype.all",
            "time.time.timerule.import.unmatched",
            "time.time.timerule.import.unmatched.daytypes",
            "time.time.timerule.import.unmatched.employeegroups",
            "time.time.timerule.import.unmatched.exportvalues",
            "time.time.timerule.import.unmatched.expressions",
            "time.time.timerule.import.unmatched.timedeviationcauses",
            "time.time.timerule.import.unmatched.timescheduletypes"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeRuleType, false, false).then(x => {
            this.types = x;
        });
    }

    private loadDirections(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeRuleDirection, false, false).then(x => {
            this.directions = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.WorkAndAbsenseAndAdditionDeduction, false, false).then(x => {
            this.timeCodes = x;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(false).then(x => {
            this.employeeGroups = x;
            this.employeeGroups.splice(0, 0, new SmallGenericType(0, this.terms["common.all"]));
        });
    }

    private loadScheduleTypes(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTypesDict(true, false).then(x => {
            this.scheduleTypes = x;
            this.scheduleTypes.splice(0, 0, new SmallGenericType(0, this.terms["time.time.timerule.scheduletype.all"]));
        });
    }

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(false, false).then(x => {
            this.timeDeviationCauses = x;
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        return this.scheduleService.getDayTypesDict(false).then(x => {
            this.dayTypes = x;
        });
    }

    // EVENTS

    private showUnmatchedTimeCode(rule: TimeRuleEditDTO) {
        let unmatched: TimeRuleExportImportUnmatchedDTO[] = rule.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeCode);
        if (unmatched) {
            let u = unmatched[0];   // Can only be one TimeCode
            let msg: string = this.terms["time.time.timerule.import.unmatched.exportvalues"] + '\n';
            msg += '\nId: {0}\n{1}: {2}\n{3}: {4}'.format(u.id.toString(), this.terms["common.code"], u.code, this.terms["common.name"], u.name);

            this.notificationService.showDialogEx(this.terms["time.time.timerule.import.unmatched"].format(this.terms["common.timecode"]), msg, SOEMessageBoxImage.Error);
        }
    }

    private showUnmatchedEmployeeGroup(rule: TimeRuleEditDTO) {
        let unmatched: TimeRuleExportImportUnmatchedDTO[] = rule.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.EmployeeGroup);
        if (unmatched) {
            let msg: string = this.terms["time.time.timerule.import.unmatched.exportvalues"] + '\n';
            _.forEach(unmatched, u => {
                msg += '\nId: {0}\n{1}: {2}\n'.format(u.id.toString(), this.terms["common.name"], u.name);
            });

            this.notificationService.showDialogEx(this.terms["time.time.timerule.import.unmatched"].format(this.terms["common.employeegroups"]), msg, SOEMessageBoxImage.Error);
        }
    }

    private showUnmatchedTimeScheduleType(rule: TimeRuleEditDTO) {
        let unmatched: TimeRuleExportImportUnmatchedDTO[] = rule.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeScheduleType);
        if (unmatched) {
            let msg: string = this.terms["time.time.timerule.import.unmatched.exportvalues"] + '\n';
            _.forEach(unmatched, u => {
                msg += '\nId: {0}\n{1}: {2}\n{3}: {4}\n'.format(u.id.toString(), this.terms["common.code"], u.code, this.terms["common.name"], u.name);
            });

            this.notificationService.showDialogEx(this.terms["time.time.timerule.import.unmatched"].format(this.terms["time.schedule.scheduletype.scheduletypes"]), msg, SOEMessageBoxImage.Error);
        }
    }

    private showUnmatchedTimeDeviationCause(rule: TimeRuleEditDTO) {
        let unmatched: TimeRuleExportImportUnmatchedDTO[] = rule.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeDeviationCause);
        if (unmatched) {
            let msg: string = this.terms["time.time.timerule.import.unmatched.exportvalues"] + '\n';
            _.forEach(unmatched, u => {
                msg += '\nId: {0}\n{1}: {2}\n'.format(u.id.toString(), this.terms["common.name"], u.name);
            });

            this.notificationService.showDialogEx(this.terms["time.time.timerule.import.unmatched"].format(this.terms["time.schedule.planning.deviationcauses"]), msg, SOEMessageBoxImage.Error);
        }
    }

    private showUnmatchedDayType(rule: TimeRuleEditDTO) {
        let unmatched: TimeRuleExportImportUnmatchedDTO[] = rule.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.DayType);
        if (unmatched) {
            let msg: string = this.terms["time.time.timerule.import.unmatched.exportvalues"] + '\n';

            _.forEach(unmatched, u => {
                msg += '\nId: {0}\n{1}: {2}\n'.format(u.id.toString(), this.terms["common.name"], u.name);
            });

            this.notificationService.showDialogEx(this.terms["time.time.timerule.import.unmatched"].format(this.terms["time.schedule.daytype.daytypes"]), msg, SOEMessageBoxImage.Error);
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss(ModalUtility.MODAL_CANCEL);
    }

    private ok() {
        this.validateSave().then(passed => {
            if (passed)
                this.$uibModalInstance.close({ result: this.importResult });
        });
    }

    // VALIDATION

    private validateSave(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let msg: string = '';

        _.forEach(this.importResult.timeRules, rule => {
            // Check that number of matched (selected) options is the same as exported number of options
            if (rule.selectedEmployeeGroups.length !== rule.nbrOfExportedEmployeeGroups)
                msg += this.terms["time.time.timerule.import.unmatched.employeegroups"].format(rule.name, rule.selectedEmployeeGroups.length.toString(), rule.nbrOfExportedEmployeeGroups.toString()) + '\n';

            if (rule.selectedScheduleTypes.length !== rule.nbrOfExportedTimeScheduleTypes)
                msg += this.terms["time.time.timerule.import.unmatched.timescheduletypes"].format(rule.name, rule.selectedScheduleTypes.length.toString(), rule.nbrOfExportedTimeScheduleTypes.toString()) + '\n';

            if (rule.selectedTimeDeviationCauses.length !== rule.nbrOfExportedTimeDeviationCauses)
                msg += this.terms["time.time.timerule.import.unmatched.timedeviationcauses"].format(rule.name, rule.selectedTimeDeviationCauses.length.toString(), rule.nbrOfExportedTimeDeviationCauses.toString()) + '\n';

            if (rule.selectedDayTypes.length !== rule.nbrOfExportedDayTypes)
                msg += this.terms["time.time.timerule.import.unmatched.daytypes"].format(rule.name, rule.selectedDayTypes.length.toString(), rule.nbrOfExportedDayTypes.toString()) + '\n';

            if (rule.hasUnmatchedExpression)
                msg += this.terms["time.time.timerule.import.unmatched.expressions"].format(rule.name) + '\n';

            if (msg)
                msg += '\n';
        });

        if (msg) {
            msg += this.terms["core.continue"];
            var modal = this.notificationService.showDialogEx(this.terms["core.warning"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                deferral.resolve(val);
            }, (reason) => {
                deferral.resolve(false);
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            _.forEach(this.importResult.timeRules, rule => {
                // Mandatory fields
                if (!rule.name && !_.includes(mandatoryFieldKeys, "common.name"))
                    mandatoryFieldKeys.push("common.name");
                if (!rule.type && !_.includes(mandatoryFieldKeys, "common.type"))
                    mandatoryFieldKeys.push("common.type");
                if (!rule.ruleStartDirection && !_.includes(mandatoryFieldKeys, "time.time.timerule.rulestartdirection"))
                    mandatoryFieldKeys.push("time.time.timerule.rulestartdirection");
                if (!rule.timeCodeId && !_.includes(mandatoryFieldKeys, "common.timecode"))
                    mandatoryFieldKeys.push("common.timecode");
            });
        });
    }

    // HELP-METHODS

    private setSelectedOptions() {
        _.forEach(this.importResult.timeRules, rule => {
            rule.nbrOfExportedEmployeeGroups = (rule.employeeGroupIds.length || 1) + rule.nbrOfUnmatchedEmployeeGroups;
            rule.nbrOfExportedTimeScheduleTypes = (rule.timeScheduleTypeIds.length || 1) + rule.nbrOfUnmatchedTimeScheduleTypes;
            rule.nbrOfExportedTimeDeviationCauses = rule.timeDeviationCauseIds.length + rule.nbrOfUnmatchedTimeDeviationCauses;
            rule.nbrOfExportedDayTypes = rule.dayTypeIds.length + rule.nbrOfUnmatchedDayTypes;

            rule.selectedEmployeeGroups = _.filter(this.employeeGroups, g => (rule.employeeGroupIds.length > 0 || rule.hasUnmatchedEmployeeGroup ? _.includes(rule.employeeGroupIds, g.id) : g.id === 0));
            rule.selectedScheduleTypes = _.filter(this.scheduleTypes, s => (rule.timeScheduleTypeIds.length > 0 || rule.hasUnmatchedTimeScheduleType ? _.includes(rule.timeScheduleTypeIds, s.id) : s.id === 0));
            rule.selectedTimeDeviationCauses = _.filter(this.timeDeviationCauses, t => _.includes(rule.timeDeviationCauseIds, t.id));
            rule.selectedDayTypes = _.filter(this.dayTypes, d => _.includes(rule.dayTypeIds, d.id));
        });
    }
}
