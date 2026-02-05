import { ScheduledJobLogDTO } from "../../../../../Common/Models/ScheduledJobDTOs";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { IRegistryService } from "../../../RegistryService";

export class LogsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/Registry/ScheduledJobs/Directives/Logs/Logs.html'),
            scope: {
                scheduledJobHeadId: '=',
                scheduledJobRowId: '=',
                isVisible: '='
            },
            restrict: 'E',
            replace: true,
            controller: LogsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class LogsController {

    // Init parameters
    private scheduledJobHeadId: number;
    private scheduledJobRowId: number;
    private isVisible: boolean;

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private logLevels: ISmallGenericType[];
    private logStatuses: ISmallGenericType[];
    private logs: ScheduledJobLogDTO[] = [];

    // Grid
    private gridHandler: EmbeddedGridController;

    // Flags
    private filterOnRow: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private registryService: IRegistryService,
        gridHandlerFactory: IGridHandlerFactory) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "ScheduledJobLogs");
        this.gridHandler.gridAg.options.setMinRowsToShow(15);

        this.$q.all([
            this.loadLogLevels(),
            this.loadLogStatuses()
        ]).then(() => {
            this.setupGrid();
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.scheduledJobHeadId, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.loadLogs();
        });

        this.$scope.$watch(() => this.scheduledJobRowId, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.filterLogs();
        });

        this.$scope.$watch(() => this.isVisible, (newVal, oldVal) => {
            if (newVal !== oldVal && this.logs.length === 0)
                this.loadLogs();
        });
    }

    public setupGrid(): void {
        const keys: string[] = [
            "common.level",
            "common.message",
            "common.status",
            "common.time",
            "manage.registry.scheduledjobs.scheduledjoblog.batchnr"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.gridHandler.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false });
            this.gridHandler.gridAg.options.groupHideOpenParents = true;
            this.gridHandler.gridAg.options.addGroupTimeSpanSumAggFunction(true, true);

            this.gridHandler.gridAg.addColumnNumber("batchNr", this.terms["manage.registry.scheduledjobs.scheduledjoblog.batchnr"], 50, { enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnDateTime("time", this.terms["common.time"], 50, false, null, { enableRowGrouping: true });
            this.gridHandler.gridAg.addColumnSelect("statusName", this.terms["common.status"], 50, {
                displayField: "statusName",
                selectOptions: this.logStatuses,
                enableRowGrouping: true,
                cellStyle: (data: ScheduledJobLogDTO, field: string) => data ? data.getCellStyle(field) : undefined
            });

            this.gridHandler.gridAg.addColumnSelect("logLevelName", this.terms["common.level"], 50, {
                displayField: "logLevelName",
                selectOptions: this.logLevels,
                enableRowGrouping: true,
                cellStyle: (data: ScheduledJobLogDTO, field: string) => data ? data.getCellStyle(field) : undefined

            });
            this.gridHandler.gridAg.addColumnText("message", this.terms["common.message"], null, false, { enableRowGrouping: true });

            this.gridHandler.gridAg.finalizeInitGrid("manage.support.logs", true, "logs-totals-grid");
        });
    }

    // SERVICE CALLS

    private loadLogLevels(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobLogLevel, true, true, true).then(x => {
            this.logLevels = x;
        });
    }

    private loadLogStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobLogStatus, true, true, true).then(x => {
            this.logStatuses = x;
        });
    }

    private loadLogs() {
        if (!this.isVisible)
            return;

        this.registryService.getScheduledJobLogs(this.scheduledJobHeadId, true, true).then(x => {
            this.logs = x;
            this.filterLogs();
        })
    }

    // HELP-METHODS

    private filterLogsFromGui() {
        this.$timeout(() => {
            this.filterLogs();
        });
    }

    private filterLogs() {
        if (this.filterOnRow && this.scheduledJobRowId)
            this.gridHandler.gridAg.setData(_.filter(this.logs, l => l.scheduledJobRowId === this.scheduledJobRowId));
        else
            this.gridHandler.gridAg.setData(this.logs);
    }
}
