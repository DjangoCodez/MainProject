import { ModalUtility } from "../../../../../Util/ModalUtility";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IScheduleService } from "../../../../Schedule/ScheduleService";
import { ITimeService } from "../../../Timeservice";
import { TimeRuleExportImportDTO } from "../../../../../Common/Models/TimeRuleDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { TermGroup, SoeTimeCodeType, TimeRuleExportImportUnmatchedType, SoeTimeRuleOperatorType } from "../../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButton, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { IValidationSummaryHandlerFactory } from "../../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IValidationSummaryHandler } from "../../../../../Core/Handlers/ValidationSummaryHandler";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";

export class ImportTimeRulesMatchingDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private allTimeCodes: ISmallGenericType[] = [];
    private allEmployeeGroups: ISmallGenericType[] = [];
    private allScheduleTypes: ISmallGenericType[] = [];
    private allTimeDeviationCauses: ISmallGenericType[] = [];
    private allDayTypes: ISmallGenericType[] = [];

    private timeCodes: ISmallGenericType[] = [];
    private employeeGroups: ISmallGenericType[] = [];
    private scheduleTypes: ISmallGenericType[] = [];
    private timeDeviationCauses: ISmallGenericType[] = [];
    private dayTypes: ISmallGenericType[] = [];

    // Flags
    private showInfo: boolean = true;

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

        this.loadTerms().then(() => {
            this.$q.all([
                this.loadTimeCodes(),
                this.loadEmployeeGroups(),
                this.loadScheduleTypes(),
                this.loadTimeDeviationCauses(),
                this.loadDayTypes()
            ]).then(() => {
                this.setupMatchingData();
            });
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.all",
            "time.time.timerule.scheduletype.all"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.WorkAndAbsenseAndAdditionDeduction, false, false).then(x => {
            this.allTimeCodes = x;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(false).then(x => {
            this.allEmployeeGroups = x;
            this.allEmployeeGroups.splice(0, 0, new SmallGenericType(0, this.terms["common.all"]));
            this.allEmployeeGroups.splice(0, 0, new SmallGenericType(-1, ''));
        });
    }

    private loadScheduleTypes(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTypesDict(true, false).then(x => {
            this.allScheduleTypes = x;
            this.allScheduleTypes.splice(0, 0, new SmallGenericType(0, this.terms["time.time.timerule.scheduletype.all"]));
            this.allScheduleTypes.splice(0, 0, new SmallGenericType(-1, ''));
        });
    }

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(false, false).then(x => {
            this.allTimeDeviationCauses = x;
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        return this.scheduleService.getDayTypesDict(false).then(x => {
            this.allDayTypes = x;
        });
    }

    // SETUP

    private setupMatchingData() {
        this.importResult.timeCodes.filter(t => !t.matchedTimeCodeId).forEach(timeCode => timeCode.originallyUnmatched = true);
        this.importResult.employeeGroups.filter(t => t.employeeGroupId && !t.matchedEmployeeGroupId).forEach(employeeGroup => { employeeGroup.originallyUnmatched = true; employeeGroup.matchedEmployeeGroupId = -1; });
        this.importResult.timeScheduleTypes.filter(t => t.timeScheduleTypeId && !t.matchedTimeScheduleTypeId).forEach(timeScheduleType => { timeScheduleType.originallyUnmatched = true; timeScheduleType.matchedTimeScheduleTypeId = -1; });
        this.importResult.timeDeviationCauses.filter(t => !t.matchedTimeDeviationCauseId).forEach(timeDeviationCause => timeDeviationCause.originallyUnmatched = true);
        this.importResult.dayTypes.filter(t => !t.matchedDayTypeId).forEach(dayType => dayType.originallyUnmatched = true);
    }

    // EVENTS    

    private cancel() {
        this.$uibModalInstance.dismiss(ModalUtility.MODAL_CANCEL);
    }

    private ok() {
        this.$uibModalInstance.close({ result: this.importResult });
    }

    // VALIDATION

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['edit'].$error;

            if (errors['timeCodesMatch'])
                validationErrorKeys.push("time.time.timerule.import.matching.unmatchedtimecodes");

            if (errors['employeeGroupsMatch'])
                validationErrorKeys.push("time.time.timerule.import.matching.unmatchedemployeegroups");

            if (errors['timeScheduleTypesMatch'])
                validationErrorKeys.push("time.time.timerule.import.matching.unmatchedtimescheduletypes");

            if (errors['timeDeviationCausesMatch'])
                validationErrorKeys.push("time.time.timerule.import.matching.unmatchedtimedeviationcauses");

            if (errors['dayTypesMatch'])
                validationErrorKeys.push("time.time.timerule.import.matching.unmatcheddaytypes");
        });
    }

    // HELP-METHODS

    private addTimeCode(timeCodeId: number) {
        if (timeCodeId && !_.includes(this.timeCodes.map(t => t.id), timeCodeId)) {
            let timeCode = this.allTimeCodes.find(t => t.id === timeCodeId);
            if (timeCode)
                this.timeCodes.push(new SmallGenericType(timeCode.id, timeCode.name));
        }
    }

    private addEmployeeGroup(employeeGroupId: number) {
        if (employeeGroupId && !_.includes(this.employeeGroups.map(t => t.id), employeeGroupId)) {
            let employeeGroup = this.allEmployeeGroups.find(t => t.id === employeeGroupId);
            if (employeeGroup)
                this.employeeGroups.push(new SmallGenericType(employeeGroup.id, employeeGroup.name));
        }
    }

    private addScheduleTypes(timeScheduleTypeId: number) {
        if (timeScheduleTypeId && !_.includes(this.scheduleTypes.map(t => t.id), timeScheduleTypeId)) {
            let scheduleType = this.allScheduleTypes.find(t => t.id === timeScheduleTypeId);
            if (scheduleType)
                this.scheduleTypes.push(new SmallGenericType(scheduleType.id, scheduleType.name));
        }
    }

    private addTimeDeviationCauses(timeDeviationCauseId: number) {
        if (timeDeviationCauseId && !_.includes(this.timeDeviationCauses.map(t => t.id), timeDeviationCauseId)) {
            let timeDeviationCause = this.allTimeDeviationCauses.find(t => t.id === timeDeviationCauseId);
            if (timeDeviationCause)
                this.timeDeviationCauses.push(new SmallGenericType(timeDeviationCause.id, timeDeviationCause.name));
        }
    }

    private addDayTypes(dayTypeId: number) {
        if (dayTypeId && !_.includes(this.dayTypes.map(t => t.id), dayTypeId)) {
            let dayType = this.allDayTypes.find(t => t.id === dayTypeId);
            if (dayType)
                this.dayTypes.push(new SmallGenericType(dayType.id, dayType.name));
        }
    }
}
