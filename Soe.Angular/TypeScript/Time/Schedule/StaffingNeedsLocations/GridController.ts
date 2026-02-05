import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Company settings
    private useAccountsHierarchy: boolean = false;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $q: ng.IQService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Schedule.StaffingNeedsLocations", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('staffingNeedsLocationId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.doLookups())
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_NeedsSettings_Locations, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_NeedsSettings_Locations].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_NeedsSettings_Locations].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "common.group",
            "common.externalcode",
            "common.user.attestrole.accounthierarchy",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 100);
            this.gridAg.addColumnText("externalCode", terms["common.externalcode"], 100, true);
            this.gridAg.addColumnText("groupName", terms["common.group"], 100, true);
            if (this.useAccountsHierarchy)
                this.gridAg.addColumnText("groupAccountName", terms["common.user.attestrole.accounthierarchy"], null, true);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.schedule.staffingneedslocation.staffingneedslocations", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    protected doLookups() {
        return this.$q.all([this.loadCompanySettings()
        ]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private reloadData() {
        this.loadGridData();
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getStaffingNeedsLocations().then(data => {
                this.setData(data);
            });
        }]);
        // Load data
    }
}
