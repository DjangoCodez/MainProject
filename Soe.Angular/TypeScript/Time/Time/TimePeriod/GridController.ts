import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ITimeService } from "../TimeService";
import { Feature, TermGroup_TimePeriodType, TermGroup } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //Lookups
    types: any[];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {
        super(gridHandlerFactory, "Time.Time.TimePeriod", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timePeriodHeadId', 'name');
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
            .onSetUpGrid(() => this.setupGrid())
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimePeriodHead_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "time.time.timeperiod.type",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 150);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnSelect("timePeriodTypeName", terms["time.time.timeperiod.type"], null, { displayField: "timePeriodTypeName", dropdownValueLabel: "label", selectOptions: this.types, enableHiding: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.time.timeperiod.timeperiods", true);
        });
    }

    private onBeforeSetUpGrid() {
        return this.$q.all([
            this.loadPeriodTypes()
        ])
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        this.types = [];
        return this.coreService.getTermGroupContent(TermGroup.TimePeriodHeadType, false, false).then(x => {
            _.forEach(x, (row) => {
                if (row.id != TermGroup_TimePeriodType.RuleWorkTime)
                    this.types.push({ value: row.name, label: row.name });
            });
        });
    }

    public loadGridData() {
        // Load data
        this.timeService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.Unknown, true, false, false).then(heads => {
            this.setData(_.filter(heads, x => x.timePeriodType !== TermGroup_TimePeriodType.RuleWorkTime));
        });
    }

    private reloadData() {
        this.loadGridData();
    }
}
