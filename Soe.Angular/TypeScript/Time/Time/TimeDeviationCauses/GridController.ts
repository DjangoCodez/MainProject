import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { ITimeService } from "../timeservice";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Filters
    private types:ISmallGenericType[] = [];

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Time.TimeDeviationCauses", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeDeviationCauseId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTypes())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {
                this.reloadData();
            });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeDeviationCause, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "common.type",
            "time.time.timedeviationcause.timecode",
            "time.time.timedeviationcause.validforstandby",
            "time.time.timedeviationcause.validforhibernating",
            "time.time.timedeviationcause.candidateforovertime",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 100);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnIcon("icon", null, null);
            this.gridAg.addColumnSelect("typeName", terms["common.type"], null, { displayField: "typeName", selectOptions: this.types, dropdownValueLabel: "name" });
            this.gridAg.addColumnText("timeCodeName", terms["time.time.timedeviationcause.timecode"], null, true);
            this.gridAg.addColumnBool("validForStandby", terms["time.time.timedeviationcause.validforstandby"], null);
            this.gridAg.addColumnBool("validForHibernating", terms["time.time.timedeviationcause.validforhibernating"], null);
            this.gridAg.addColumnBool("candidateForOvertime", terms["time.time.timedeviationcause.candidateforovertime"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.time.timedeviationcause.timedeviationcauses", true);
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeDeviationCauseType, false, true).then(x => {
            this.types = x;
        });
    }

    public loadGridData() {
        // Load data
        this.timeService.getTimeDeviationCausesGrid().then(x => {
            this.setData(x);
        });
    }

    private reloadData() {
        this.loadGridData();
    }
}
