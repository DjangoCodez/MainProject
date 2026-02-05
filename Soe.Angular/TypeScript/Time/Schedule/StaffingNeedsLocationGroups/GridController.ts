import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    timeScheduleTasks: any = [];

    // Company settings
    private useAccountsHierarchy: boolean = false;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Schedule.StaffingNeedsLocationGroups", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('staffingNeedsLocationGroupId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_NeedsSettings_LocationGroups, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_NeedsSettings_LocationGroups].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_NeedsSettings_LocationGroups].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "core.edit",
            "time.schedule.staffingneedslocationgroup.timescheduletask",
            "common.user.attestrole.accounthierarchy"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 125);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            if (this.useAccountsHierarchy)
                this.gridAg.addColumnText("accountName", terms["common.user.attestrole.accounthierarchy"], null, true);
            this.gridAg.addColumnText("timeScheduleTaskName", terms["time.schedule.staffingneedslocationgroup.timescheduletask"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.schedule.staffingneedslocationgroup.staffingneedslocationgroups", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    protected doLookups() {
        return this.$q.all([this.loadCompanySettings(),this.loadTimeScheduleTasks()
        ]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadTimeScheduleTasks(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTasksDict().then((x) => {
            this.timeScheduleTasks = [];
            this.timeScheduleTasks = x;
        });
    }

    private reloadData() {
        this.loadGridData();
    }

    // Load data
    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getStaffingNeedsLocationGroups().then(data => {
                _.forEach(data, (row: any) => {
                    row.timeScheduleTaskName = "";
                    if (row.timeScheduleTaskId > 0){
                        _.forEach(this.timeScheduleTasks, (task: any) => {
                            if (task.id == row.timeScheduleTaskId)
                                row.timeScheduleTaskName = task.name;
                        });
                    }

                });
                this.setData(data);
            });
        }]);
    }
}
