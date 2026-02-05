import { TimeScheduleScenarioHeadDTO, CreateTemplateFromScenarioDTO, CreateTemplateFromScenarioRowDTO, PreviewCreateTemplateFromScenarioDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IScheduleService } from "../../../ScheduleService";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CreateTemplateFromScenarioVerifyController } from "./CreateTemplateFromScenarioVerifyController";
import { SoeScheduleWorkRules } from "../../../../../Util/CommonEnumerations";

export class CreateTemplateFromScenarioController {

    // Terms
    private terms: { [index: string]: string; };

    private progress: IProgressHandler;

    // Flags
    private showOnlyInvalidWorkRules: boolean = true;
    private isDirty = true;
    private startDateNotMonday = false;

    // Properties
    private rows: PreviewCreateTemplateFromScenarioDTO[] = [];
    private dateFrom: Date;
    private dateTo?: Date;
    private nbrOfWeeks: number;
    private weekInCycle: number = 1;

    // Grid
    private gridHandler: EmbeddedGridController;

    //@ngInject
    constructor(private $uibModalInstance,
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private scheduleService: IScheduleService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private scenarioHead: TimeScheduleScenarioHeadDTO,
        private useStopDate: boolean,
        private templateScheduleEditHiddenPermission: boolean,
        private hiddenEmployeeId: number) {

        this.dateFrom = this.scenarioHead.dateFrom;
        if (this.useStopDate)
            this.dateTo = this.scenarioHead.dateTo;
        this.nbrOfWeeks = (this.scenarioHead.dateTo.diffDays(this.scenarioHead.dateFrom) + 1) / 7;

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "ActivateScenario");
    }

    $onInit() {
        this.progress.startLoadingProgress([() => {
            return this.loadLookups().then(() => {
                this.setupGrid();
            });
        }]);
    }

    private setupGrid() {
        let colDefEmployee = this.gridHandler.gridAg.addColumnText("name", this.terms["common.employee"], 150, false, { enableRowGrouping: true });
        this.gridHandler.gridAg.addColumnDate("date", this.terms["common.date"], 75, false, null, { enableRowGrouping: true });
        this.gridHandler.gridAg.addColumnDate("templateDateFrom", this.terms["time.schedule.planning.scenario.createtemplate.templatedatefrom"], 125, true, null, { enableRowGrouping: true });
        if (this.useStopDate)
            this.gridHandler.gridAg.addColumnDate("templateDateTo", this.terms["time.schedule.planning.scenario.createtemplate.templatedateto"], 125, true, null, { enableRowGrouping: true });
        this.gridHandler.gridAg.addColumnText("shiftTextScenario", this.terms["time.schedule.planning.scenario.activate.shifttextscenario"], null, false, { enableRowGrouping: false, suppressMovable: true, cellClassRules: { "warningColor": (gridRow: any) => gridRow.data && gridRow.data.hasScheduleDiff } });
        this.gridHandler.gridAg.addColumnText("workRuleName", this.terms["time.schedule.planning.scenario.activate.workrulename"], null, false, { enableRowGrouping: true, toolTipField: "workRuleText" });
        this.gridHandler.gridAg.addColumnIcon(null, null, null, { suppressFilter: true, icon: 'fal fa-user-clock warningColor', showIcon: (row: PreviewCreateTemplateFromScenarioDTO) => row && row.hasWorkRule, toolTip: this.terms["core.showinfo"], onClick: this.showWorkRuleText.bind(this) });
        this.gridHandler.gridAg.addColumnIcon(null, null, null, { suppressFilter: true, icon: 'fal fa-ban errorColor', showIcon: (row: PreviewCreateTemplateFromScenarioDTO) => row && row.hasInvalidWorkRule, toolTip: this.terms["time.schedule.planning.scenario.activate.invalidrow"] });

        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.setMinRowsToShow(15);
        this.gridHandler.gridAg.options.useGrouping(false, false);

        this.gridHandler.gridAg.options.groupRowsByColumn(colDefEmployee, true);

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
        const keys: string[] = [
            "core.showinfo",
            "common.employee",
            "common.date",
            "time.schedule.planning.scenario.activate.invalidrow",
            "time.schedule.planning.scenario.activate.shifttextscenario",
            "time.schedule.planning.scenario.activate.workrulename",
            "time.schedule.planning.scenario.createtemplate.templatedatefrom",
            "time.schedule.planning.scenario.createtemplate.templatedateto",
            "time.schedule.planning.scenario.createtemplate.nopermissionforhiddenemployee"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private evaluateWorkRules(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
            return this.scheduleService.previewCreateTemplateFromScenario(this.scenarioHead.timeScheduleScenarioHeadId, this.dateFrom, this.weekInCycle, this.dateTo).then(x => {
                this.rows = x;

                // If not permitted to modify template schedule on hidden employee,
                // check if hidden employee exists in schenario.
                // In that case, create a fake work rule error for it.
                if (!this.templateScheduleEditHiddenPermission) {
                    const hiddenEmployeeRows = this.rows.filter(r => r.employeeId === this.hiddenEmployeeId);
                    if (hiddenEmployeeRows.length) {
                        hiddenEmployeeRows.forEach(r => {
                            r.workRule = SoeScheduleWorkRules.AttestedDay;  // Need to set a rule for isValidToCreateTemplate() check below
                            r.workRuleCanOverride = false;
                            r.workRuleName = r.workRuleText = this.terms["time.schedule.planning.scenario.createtemplate.nopermissionforhiddenemployee"];
                        });
                    }
                } 

                this.filterRows();
                this.isDirty = false;
            });
        }]);
    }

    // HELP-METHODS

    private get isValidToCreateTemplate(): boolean {
        return !this.isDirty && this.rows.length > 0 && _.filter(this.rows, r => r.hasInvalidWorkRule).length === 0;
    }

    // EVENTS

    private dateFromChanged() {
        this.isDirty = true;
        this.$timeout(() => {
            if (this.dateTo)
                this.dateTo = this.dateFrom.addDays((this.nbrOfWeeks * 7) - 1);

            this.startDateNotMonday = (this.dateFrom.getDay() !== 1);
        });
    }

    private dateToChanged() {
        this.isDirty = true;
    }

    private weekInCycleChanged() {
        this.isDirty = true;
    }

    private filterRows() {
        this.$timeout(() => {
            this.gridHandler.gridAg.setData(this.showOnlyInvalidWorkRules ? _.filter(this.rows, r => r.hasWorkRule) : this.rows);
        });
    }

    private showWorkRuleText(row: PreviewCreateTemplateFromScenarioDTO) {
        this.notificationService.showDialogEx(row.workRuleName, row.workRuleText, row.workRuleCanOverride ? SOEMessageBoxImage.Warning : SOEMessageBoxImage.Forbidden);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private initOk() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateTemplateFromScenario/Views/createTemplateFromScenarioVerify.html"),
            controller: CreateTemplateFromScenarioVerifyController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                scenarioHead: () => { return this.scenarioHead },
                dateFrom: () => { return this.dateFrom },
                dateTo: () => { return this.dateTo }
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
            let model: CreateTemplateFromScenarioDTO = new CreateTemplateFromScenarioDTO();
            model.timeScheduleScenarioHeadId = this.scenarioHead.timeScheduleScenarioHeadId;
            model.dateFrom = this.dateFrom;
            model.weekInCycle = this.weekInCycle;
            model.dateTo = this.dateTo;
            model.rows = []
            _.forEach(this.rows, row => {
                model.rows.push(new CreateTemplateFromScenarioRowDTO(row.employeeId, row.date));
            });

            this.scheduleService.createTemplateFromScenario(model).then(result => {
                if (result.success) {
                    completion.completed();
                    this.$uibModalInstance.close({ success: true });
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null);
    }
}
