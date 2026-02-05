import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { Feature } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Filters
    typeFilterOptions = [];

    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Schedule.Skills", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('skillId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onDoLookUp(() => this.loadTypes())
            .onLoadGridData(() => this.loadGridData())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_ScheduleSettings_Skill, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_ScheduleSettings_Skill].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_Skill].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.type",
            "common.name",
            "common.description",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("skillTypeName", terms["common.type"], 125, { displayField: "skillTypeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnText("name", terms["common.name"], 125);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.schedule.skill.skills", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    // Load data
    public loadGridData(useCache: boolean = false) {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getSkills(useCache).then(data => {
                this.setData(data);
            });
        }]);
    }

    protected loadLookups() {
        this.loadTypes();
    }

    private loadTypes(): ng.IPromise<any> {
        return this.scheduleService.getSkillTypesDict(false).then((x) => {
            _.forEach(x, (y: any) => {
                this.typeFilterOptions.push({ value: y.name, label: y.name })
            });
        });
    }

    private reloadData() {
        this.loadGridData();
    }
}
