import { TimeScheduleScenarioHeadDTO, PreviewActivateScenarioDTO, ActivateScenarioDTO, ActivateScenarioRowDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IScheduleService } from "../../../ScheduleService";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ActivateScenarioVerifyController } from "./ActivateScenarioVerifyController";

export class ActivateScenarioController {

    // Terms
    private terms: { [index: string]: string; };

    private progress: IProgressHandler;

    // Flags
    private hideUnchanged: boolean = true;
    private sendMessage: boolean = false;
    private evaluating: boolean = false;

    // Properties
    private rows: PreviewActivateScenarioDTO[] = [];
    private preliminaryDateFrom: Date = null;

    private preliminaryDateOptions = {
        dateDisabled: this.disabledPreliminaryDates,
        customClass: this.getPreliminaryDateDayClass,
        controller: this,
    };

    // Polling
    private pollStatusInterval;
    private pollTimeout = 30000;

    // Grid
    private gridHandler: EmbeddedGridController;

    //@ngInject
    constructor(private $uibModalInstance,
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $interval: ng.IIntervalService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private scheduleService: IScheduleService,
        private progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private scenarioHead: TimeScheduleScenarioHeadDTO) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "ActivateScenario");
    }

    $onInit() {
        this.progress.startLoadingProgress([() => {
            return this.loadLookups().then(() => {
                this.setupGrid();
                this.initEvaluateWorkRules(true);
            });
        }]);
    }

    private setupGrid() {
        let colDefEmployee = this.gridHandler.gridAg.addColumnText("name", this.terms["common.employee"], 150, false, { enableRowGrouping: true, showRowGroup: "name" });
        let colDefStatus = this.gridHandler.gridAg.addColumnText("statusText", this.terms["common.status"], null, false, { enableRowGrouping: true, showRowGroup: "statusText", toolTipField: "statusMessage" });

        this.gridHandler.gridAg.addColumnDate("date", this.terms["common.date"], 75, false, null, { enableRowGrouping: false });
        this.gridHandler.gridAg.addColumnText("shiftTextSchedule", this.terms["time.schedule.planning.scenario.activate.shifttextschedule"], null, false, { enableRowGrouping: false, suppressMovable: true });
        this.gridHandler.gridAg.addColumnText("shiftTextScenario", this.terms["time.schedule.planning.scenario.activate.shifttextscenario"], null, false, { enableRowGrouping: false, suppressMovable: true, cellClassRules: { "warningColor": (gridRow: any) => gridRow.data && gridRow.data.hasScheduleDiff } });
        this.gridHandler.gridAg.addColumnText("workRuleName", this.terms["time.schedule.planning.scenario.activate.workrulename"], null, false, { enableRowGrouping: false, toolTipField: "workRuleText" });
        this.gridHandler.gridAg.addColumnIcon(null, null, null, { suppressFilter: true, icon: 'fal fa-user-clock warningColor', showIcon: (row: PreviewActivateScenarioDTO) => row && row.hasWorkRule, toolTip: this.terms["core.showinfo"], onClick: this.showWorkRuleText.bind(this) });
        this.gridHandler.gridAg.addColumnIcon(null, null, null, { suppressFilter: true, icon: 'fal fa-ban errorColor', showIcon: (row: PreviewActivateScenarioDTO) => row && row.hasInvalidWorkRule, toolTip: this.terms["time.schedule.planning.scenario.activate.invalidrow"] });

        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.setMinRowsToShow(15);
        this.gridHandler.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false, hideGroupPanel: true, suppressCount: true });

        this.gridHandler.gridAg.options.groupRowsByColumn(colDefEmployee, colDefEmployee.field, 1);
        this.gridHandler.gridAg.options.groupRowsByColumn(colDefStatus, colDefStatus.field, 1);
        this.gridHandler.gridAg.options.groupHideOpenParents = true;

        this.gridHandler.gridAg.finalizeInitGrid("time.schedule.planning.scenario.activate", true);
    }

    // SERVICE CALLS

    private loadLookups(): ng.IPromise<any> {
        let deferral = this.$q.defer<any>();

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.message",
            "core.showinfo",
            "common.date",
            "common.employee",
            "common.status",
            "time.schedule.planning.scenario.activate.invalidrow",
            "time.schedule.planning.scenario.activate.shifttextschedule",
            "time.schedule.planning.scenario.activate.shifttextscenario",
            "time.schedule.planning.scenario.activate.workrulename"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private initEvaluateWorkRules(showProgress: boolean): ng.IPromise<any> {
        this.evaluating = true;
        if (showProgress) {
            return this.progress.startLoadingProgress([() => {
                return this.evaluateWorkRules();
            }]);
        } else {
            return this.evaluateWorkRules();
        }
    }

    private evaluateWorkRules(): ng.IPromise<any> {
        return this.scheduleService.previewActivateScenario(this.scenarioHead.timeScheduleScenarioHeadId, this.preliminaryDateFrom).then(x => {
            this.rows = x;
            this.filterRows();
            this.evaluating = false;
        });
    }

    private loadStatus(): ng.IPromise<any> {
        return this.scheduleService.getActivateScenarioEmployeeStatus(this.scenarioHead.timeScheduleScenarioHeadId).then(x => {
            if (x.length > 0) {
                for (let status of x) {
                    for (let row of this.rows.filter(r => r.employeeId == status.employeeId)) {
                        row.statusName = status.statusName;
                        row.statusMessage = status.statusMessage;
                    }
                }
                this.filterRows();
            }
        });
    }

    // HELP-METHODS

    private disabledPreliminaryDates(data) {
        let self: ActivateScenarioController = this['datepickerOptions']['controller'];
        if (self && self.scenarioHead) {
            return data.mode === 'day' && ((<Date>data.date).isBeforeOnDay(self.scenarioHead.dateFrom) || (<Date>data.date).isAfterOnDay(self.scenarioHead.dateTo));
        }

        return false;
    }

    private getPreliminaryDateDayClass(data) {
        let self: ActivateScenarioController = this['datepickerOptions']['controller'];
        if (self && self.scenarioHead) {
            return data.mode === 'day' && ((<Date>data.date).isBeforeOnDay(self.scenarioHead.dateFrom) || (<Date>data.date).isAfterOnDay(self.scenarioHead.dateTo)) ? 'disabledDate' : '';
        }

        return '';
    }

    private get isValidToActivate(): boolean {
        return !this.evaluating && this.rows.length > 0 && _.filter(this.rows, r => r.hasInvalidWorkRule).length === 0;
    }

    private pollStatus() {
        // Cancel any active polling
        this.cancelPollStatus();

        this.pollStatusInterval = this.$interval(() => {
            this.loadStatus();
        }, this.pollTimeout);
    }

    private cancelPollStatus() {
        if (this.pollStatusInterval)
            this.$interval.cancel(this.pollStatusInterval);
    }

    // EVENTS

    private preliminaryDateFromChanged() {
        this.$timeout(() => {
            this.initEvaluateWorkRules(true);
        });
    }

    private filterRows() {
        this.$timeout(() => {
            this.gridHandler.gridAg.setData(this.hideUnchanged ? _.filter(this.rows, r => r.hasScheduleDiff) : this.rows);
        });
    }

    private showWorkRuleText(row: PreviewActivateScenarioDTO) {
        this.notificationService.showDialogEx(row.workRuleName, row.workRuleText, row.workRuleCanOverride ? SOEMessageBoxImage.Warning : SOEMessageBoxImage.Forbidden);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private initOk() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/ActivateScenario/Views/activateScenarioVerify.html"),
            controller: ActivateScenarioVerifyController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                scenarioHead: () => { return this.scenarioHead },
            }
        }

        this.$uibModal.open(options).result.then(result => {
            if (result && result.success) {
                this.ok();
            }
        }, (reason) => {
            // User cancelled dialog
        });
    }

    private ok() {
        this.progress.startSaveProgress((completion) => {
            let model: ActivateScenarioDTO = new ActivateScenarioDTO();
            model.timeScheduleScenarioHeadId = this.scenarioHead.timeScheduleScenarioHeadId;
            model.preliminaryDateFrom = this.preliminaryDateFrom;
            model.sendMessage = this.sendMessage;
            model.rows = [];
            _.forEach(_.filter(this.rows, r => r.hasScheduleDiff), row => {
                model.rows.push(new ActivateScenarioRowDTO(row.employeeId, row.date));
            });

            this.pollStatus();
            this.scheduleService.activateScenario(model).then(result => {
                this.cancelPollStatus();

                if (result.success) {
                    completion.completed();
                    this.$uibModalInstance.close({ success: true });
                } else {
                    completion.failed(result.errorMessage);
                    this.initEvaluateWorkRules(false);
                }
            }, error => {
                this.cancelPollStatus();
                completion.failed(error.message);
                this.initEvaluateWorkRules(false);
            });
        }, null);
    }
}
