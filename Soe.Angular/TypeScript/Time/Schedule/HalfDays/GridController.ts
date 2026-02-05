import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IScheduleService } from "../ScheduleService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    dayTypesDict: any[];
    halfDayTypesDict: any[];

    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {

        super(gridHandlerFactory, "Time.Schedule.HalfDays", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_ScheduleSettings_Halfdays, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_ScheduleSettings_Halfdays].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_Halfdays].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "time.schedule.daytypes.description",
            "time.schedule.daytype.halfdaytype",
            "time.schedule.daytype.value",
            "time.schedule.daytype.daytype",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 200);
            this.gridAg.addColumnText("description", terms["time.schedule.daytypes.description"], null);
            this.gridAg.addColumnSelect("typeName", terms["time.schedule.daytype.halfdaytype"], null, { displayField: "typeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnText("value", terms["time.schedule.daytype.value"], null);
            this.gridAg.addColumnSelect("dayTypeName", terms["time.schedule.daytype.daytype"], null, { displayField: "dayTypeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.schedule.daytype.halfdays", true);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getHalfDays()
                .then(data => {
                    this.setData(data);
                });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
}
