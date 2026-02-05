import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IScheduleService } from "../ScheduleService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Schedule.TimeScheduleEvents", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeScheduleEventId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Schedule_SchedulePlanning_SalesCalender, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_SchedulePlanning_SalesCalender].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_SchedulePlanning_SalesCalender].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private setUpGrid() {

        var translationKeys: string[] = [
            "common.name",
            "common.description",
            "common.date",
            "core.edit"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.gridAg.addColumnText("name", terms["common.name"], 100, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnDate("date", terms["common.date"], 100, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.schedule.timescheduleevents.events", true);
        });

        this.gridAg.setExporterFilenamesAndHeader("time.schedule.timescheduleevents.events");
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private loadGridData(useCache: boolean = true) {

        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getTimeScheduleEvents(useCache).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
