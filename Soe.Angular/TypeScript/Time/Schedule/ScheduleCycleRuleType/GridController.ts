import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private terms: any;

    // Company settings
    private useAccountsHierarchy: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super(gridHandlerFactory, "Time.Schedule.ScheduleCycleRuleType", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.beforeSetUpGrid())
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

        this.flowHandler.start({ feature: Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.name",
            "time.schedule.schedulecycleruletype.weekday",
            "time.schedule.schedulecycleruletype.starttime",
            "time.schedule.schedulecycleruletype.stoptime",
            "common.user.attestrole.accounthierarchy",
            "core.edit"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private beforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms(), this.loadCompanySettings()])
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private setUpGrid() {
        this.gridAg.addColumnText("name", this.terms["common.name"], 125);
        if (this.useAccountsHierarchy)
            this.gridAg.addColumnText("accountName", this.terms["common.user.attestrole.accounthierarchy"], null, true);
        this.gridAg.addColumnText("dayOfWeeksGridString", this.terms["time.schedule.schedulecycleruletype.weekday"], null);
        this.gridAg.addColumnTime("startTime", this.terms["time.schedule.schedulecycleruletype.starttime"], 100);
        this.gridAg.addColumnTime("stopTime", this.terms["time.schedule.schedulecycleruletype.stoptime"], 100);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.finalizeInitGrid("time.schedule.schedulecycleruletype.schedulecycleruletype", true);

        this.gridAg.setExporterFilenamesAndHeader("time.schedule.schedulecycleruletype.schedulecycleruletype");
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private loadGridData(useCache: boolean = true) {

        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getScheduleCycleRuleTypes().then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
