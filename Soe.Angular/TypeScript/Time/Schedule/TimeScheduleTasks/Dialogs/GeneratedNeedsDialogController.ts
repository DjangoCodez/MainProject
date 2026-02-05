import { IQService, ITimeoutService } from "angular";
import { TimeScheduleTaskGeneratedNeedDTO } from "../../../../Common/Models/StaffingNeedsDTOs";
import { EmbeddedGridController } from "../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { Feature } from "../../../../Util/CommonEnumerations";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IScheduleService } from "../../ScheduleService";

export class GeneratedNeedsDialogController {

    // Permissions
    private staffingNeedsModifyPermission: boolean = false;

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private tasks: TimeScheduleTaskGeneratedNeedDTO[];

    // Grid
    private gridHandler: EmbeddedGridController;
    private selectedRowIds: number[] = [];

    //@ngInject
    constructor(
        private $q: IQService,
        private $uibModalInstance,
        private $timeout: ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private scheduleService: IScheduleService,
        gridHandlerFactory: IGridHandlerFactory,
        private timeScheduleTaskId: number,
        private date: Date) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "time.schedule.timescheduletask.generatedneed");

        this.$q.all([
            this.loadModifyPermissions(),
            this.loadTerms()
        ]).then(() => {
            this.setupGrid();
            this.load();
        });
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.enableRowSelection = this.staffingNeedsModifyPermission;
        this.gridHandler.gridAg.options.setMinRowsToShow(10);

        this.gridHandler.gridAg.addColumnNumber("staffingNeedsRowId", this.terms["time.schedule.timescheduletask.generatedneed.rowid"], 100, { enableHiding: true });
        this.gridHandler.gridAg.addColumnText("type", this.terms["common.type"], null, true);
        this.gridHandler.gridAg.addColumnText("occurs", this.terms["time.schedule.timescheduletask.generatedneed.occurs"], null, true);
        this.gridHandler.gridAg.addColumnTime("startTime", this.terms["time.schedule.timescheduletask.starttime"], 100, { enableHiding: true });
        this.gridHandler.gridAg.addColumnTime("stopTime", this.terms["time.schedule.timescheduletask.stoptime"], 100, { enableHiding: true });

        let events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => { this.rowSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => { this.rowSelectionChanged(); }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.finalizeInitGrid("time.schedule.timescheduletask.generatedneed", false);
    }

    // SERVICE CALLS

    private loadModifyPermissions(): ng.IPromise<any> {
        let features: number[] = [];
        features.push(Feature.Time_Schedule_StaffingNeeds);

        return this.coreService.hasModifyPermissions(features).then(x => {
            this.staffingNeedsModifyPermission = x[Feature.Time_Schedule_StaffingNeeds];
        });
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.type",
            "time.schedule.timescheduletask.starttime",
            "time.schedule.timescheduletask.stoptime",
            "time.schedule.timescheduletask.generatedneed.rowid",
            "time.schedule.timescheduletask.generatedneed.occurs",
            "time.schedule.timescheduletask.generatedneed.deleterows",
            "time.schedule.timescheduletask.generatedneed.deleterows.warning"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private load(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTaskGeneratedNeeds(this.timeScheduleTaskId).then(x => {
            this.tasks = x;
            this.gridHandler.gridAg.setData(this.tasks);
        });
    }

    private delete() {
        this.scheduleService.deleteGeneratedNeeds(this.selectedRowIds).then(result => {
            if (result.success) {
                this.selectedRowIds = [];
                this.load();
            } else {
                this.notificationService.showErrorDialog(this.terms["time.schedule.timescheduletask.generatedneed.deleterows"], result.errorMessage, result.stackTrace);
            }
        });
    }

    // EVENTS

    private rowSelectionChanged() {
        this.$timeout(() => {
            this.selectedRowIds = this.gridHandler.gridAg.options.getSelectedIds("staffingNeedsRowPeriodId");
        });
    }

    private initDelete() {
        this.notificationService.showDialogEx(this.terms["time.schedule.timescheduletask.generatedneed.deleterows"], this.terms["time.schedule.timescheduletask.generatedneed.deleterows.warning"].format(this.selectedRowIds.length.toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
            if (val)
                this.delete();
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
