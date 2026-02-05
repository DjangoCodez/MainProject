import { ActivateScheduleGridDTO } from "../../../../../Common/Models/EmployeeScheduleDTOs";
import { TimeScheduleTemplateHeadSmallDTO, TimeScheduleTemplatePeriodSmallDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandler } from "../../../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { CompanySettingType, SoeModule, TermGroup_AttestEntity, TermGroup_TemplateScheduleActivateFunctions } from "../../../../../Util/CommonEnumerations";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { RecalculateTimeStatusDialogController } from "../../../../Dialogs/RecalculateTimeStatus/RecalculateTimeStatusDialogController";
import { IScheduleService } from "../../../../Schedule/ScheduleService";
import { EditPlacementMode } from "./ScheduleDirective";

export class EditPlacementDialogController {

    // Terms
    private terms: { [index: string]: string; };

    private placement: ActivateScheduleGridDTO;
    private isNew: boolean;
    private startDate: Date;
    private stopDate: Date;
    private preliminary: boolean = false;

    // Company settings
    private showPreliminary: boolean
    private defaultPreliminary: boolean = false;
    private hasInitialAttestState: boolean = false;

    // Data
    private publicTemplateSchedules: TimeScheduleTemplateHeadSmallDTO[] = [];
    private templateSchedules: TimeScheduleTemplateHeadSmallDTO[] = [];
    private templatePeriods: TimeScheduleTemplatePeriodSmallDTO[] = [];
    private selectedPeriodId: number = 0;
    private templateType: number = 0;

    private stopDateOptions = {
        maxDate: CalendarUtility.getDateToday().addYears(2),
        customClass: this.getStopDateDayClass
    };

    // Flags
    private loading: boolean = false;
    private executing: boolean = false;

    private modalInstance: any;
    private progress: IProgressHandler;
    private validationHandler: IValidationSummaryHandler;
    private dialogform: ng.IFormController;

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private personalTemplateSchedules: TimeScheduleTemplateHeadSmallDTO[],
        private selectedTemplateHead: TimeScheduleTemplateHeadSmallDTO,
        employeeId: number,
        startDate: Date,
        placement: ActivateScheduleGridDTO,
        private mode: EditPlacementMode) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();
        this.validationHandler = validationSummaryHandlerFactory.create();

        this.modalInstance = $uibModal;

        this.isNew = !placement;

        this.placement = new ActivateScheduleGridDTO();
        angular.extend(this.placement, placement);
        this.startDate = this.placement.employeeScheduleStartDate;
        this.stopDate = this.placement.employeeScheduleStopDate;

        this.loading = true;

        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.getInitialAttestState()
        ]).then(() => {
            if (this.isNew) {
                this.placement.employeeId = employeeId;
                this.startDate = this.placement.employeeScheduleStartDate = startDate;

                this.$q.all([
                    this.loadTemplateHeads(),
                    this.loadTemplatePeriods()
                ]).then(() => {
                    this.setTemplates();
                    this.loading = false;
                });
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.weekshort",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimePlacementHidePreliminary);
        settingTypes.push(CompanySettingType.TimePlacementDefaultPreliminary);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.showPreliminary = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimePlacementHidePreliminary) && this.mode !== EditPlacementMode.Delete;
            this.defaultPreliminary = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimePlacementDefaultPreliminary);
            if (this.defaultPreliminary)
                this.preliminary = true;
        });
    }

    private getInitialAttestState(): ng.IPromise<any> {
        return this.coreService.hasInitialAttestState(TermGroup_AttestEntity.Unknown, SoeModule.Time).then(x => {
            this.hasInitialAttestState = x;
        });
    }

    private loadTemplateHeads(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTemplateHeadsForActivate().then(x => {
            this.publicTemplateSchedules = x;

            _.forEach(this.publicTemplateSchedules, t => {
                t['description'] = '';
                if (t.startDate)
                    t['description'] += "{0}, ".format(t.startDate.toFormattedDate());

                t['description'] += "{0} {1}, {2}".format((t.noOfDays / 7).toString(), this.terms["common.weekshort"], t.name);
                if (t.accountName)
                    t['description'] += " ({0})".format(t.accountName);
            });
        });
    }

    private loadTemplatePeriods(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (!this.selectedTemplateHead) {
            deferral.resolve();
            return;
        }

        this.scheduleService.getTimeScheduleTemplatePeriodsForActivate(this.selectedTemplateHead.timeScheduleTemplateHeadId).then(x => {
            this.templatePeriods = x;
            this.setPeriod();
            deferral.resolve();
        });

        return deferral.promise;
    }

    // EVENTS

    private setTemplates() {
        this.$timeout(() => {
            if (this.templateType == 0)
                this.selectedTemplateHead = null;
            else
                this.templateSchedules = this.templateType == 1 ? this.personalTemplateSchedules : this.publicTemplateSchedules;
        });
    }

    private selectedTemplateHeadChanged() {
        this.$timeout(() => {
            this.loadTemplatePeriods();
        });
    }

    private startDateChanged() {
        this.$timeout(() => {
            this.setPeriod();
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private initActivate() {
        this.executing = true;
        this.openRecalculateTimeStatusDialog(true);
    }

    private ok() {
        this.$uibModalInstance.close({ placement: this.placement });
    }

    // HELP-METHODS

    private get modeIsNew(): boolean {
        return this.mode === EditPlacementMode.New;
    }

    private get modeIsShorten(): boolean {
        return this.mode === EditPlacementMode.Shorten;
    }

    private get modeIsExtend(): boolean {
        return this.mode === EditPlacementMode.Extend;
    }

    private getStopDateDayClass(data) {
        return (data.mode === 'day' && CalendarUtility.convertToDate(data.date) > CalendarUtility.getDateToday().addYears(2)) ? 'disabledDate' : '';
    }

    private setPeriod() {
        this.selectedPeriodId = 0;

        if (!this.selectedTemplateHead || this.templatePeriods.length === 0 || !this.startDate)
            return;

        // Get closest first monday of cycle before start date
        let firstMonday: Date = this.selectedTemplateHead.firstMondayOfCycle;
        while (firstMonday.addDays(this.selectedTemplateHead.noOfDays).isSameOrBeforeOnDay(this.startDate)) {
            firstMonday = firstMonday.addDays(this.selectedTemplateHead.noOfDays);
        }

        // Calculate day number based on diff between first monday and selected start date
        let dayNumber = this.startDate.diffDays(firstMonday) + 1;

        if (dayNumber < 1) {
            this.translationService.translateMany([
                "time.schedule.activate.cantsetperiod",
                "time.schedule.activate.cantstartbeforetemplatestart"
            ]).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.activate.cantsetperiod"], terms["time.schedule.activate.cantstartbeforetemplatestart"], SOEMessageBoxImage.Error);
            });
        } else {
            // Get period with calculated day number
            let period = this.templatePeriods.find(p => p.dayNumber === dayNumber);
            if (period) {
                this.selectedPeriodId = period.timeScheduleTemplatePeriodId;
            } else {
                this.translationService.translateMany([
                    "time.schedule.activate.cantsetperiod",
                    "time.schedule.activate.cantsetperiod.toofewdays"
                ]).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.activate.cantsetperiod"], terms["time.schedule.activate.cantsetperiod.toofewdays"], SOEMessageBoxImage.Error);
                });
            }
        }
    }

    private openRecalculateTimeStatusDialog(activateMode: boolean) {
        let modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/RecalculateTimeStatus/Views/RecalculateTimeStatusDialog.html"),
            controller: RecalculateTimeStatusDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            keyboard: false,
            size: 'xl',
            scope: this.$scope,
            resolve: {
                activateMode: () => { return activateMode; },
                items: () => { return [this.placement]; },
                selectedFunction: () => { return this.modeIsNew ? TermGroup_TemplateScheduleActivateFunctions.NewPlacement : TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate; },
                selectedHeadId: () => { return this.selectedTemplateHead?.timeScheduleTemplateHeadId || 0; },
                selectedPeriodId: () => { return this.selectedPeriodId; },
                startDate: () => { return this.modeIsNew ? this.startDate : null; },
                stopDate: () => { return this.stopDate; },
                preliminary: () => { return this.preliminary; },
            }
        });

        modal.result.then(result => {
            this.executing = false;
            if (result.reload)
                this.ok();
        }, function () {
            // Cancelled
        });
    }

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['dialogform'].$error;

            if (errors['templateHead'])
                mandatoryFieldKeys.push("time.schedule.planning.templateschedule");

            if (errors['startDate'])
                mandatoryFieldKeys.push("common.startdate");

            if (errors['stopDate'])
                mandatoryFieldKeys.push("common.stopdate");

            if (errors['selectedPeriod'])
                validationErrorKeys.push("time.schedule.activate.cantstartbeforetemplatestart");

            if (errors['stopDateMaxTwoYears'])
                validationErrorKeys.push("time.schedule.activate.stopdatemaxtwoyears");

            if (errors['stopDateAfterStartDate'])
                validationErrorKeys.push("time.schedule.activate.stopdatebeforestartdate");

            if (errors['initialAttestState'])
                validationErrorKeys.push("time.schedule.activate.missinginitialatteststate");
        });
    }
}
