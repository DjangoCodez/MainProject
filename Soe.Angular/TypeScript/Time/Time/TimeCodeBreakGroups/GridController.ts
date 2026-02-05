import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ITimeService } from "../TimeService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag  implements ICompositionGridController {
    //@ngInject
    constructor(
        private timeService: ITimeService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,) {

        super(gridHandlerFactory, "Time.Time.TimeCodeBreakGroups", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeCodeBreakGroupId', 'name');
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private setUpGrid() {        
        var translationKeys: string[] = [
            "common.name",
            "common.description",
            "core.edit"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 100);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.time.timecodebreakgroups", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeCodeBreakGroups()
                .then(data => {
                    this.setData(data);
                });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
}
