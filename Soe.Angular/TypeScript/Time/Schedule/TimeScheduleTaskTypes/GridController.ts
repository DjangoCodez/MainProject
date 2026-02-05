import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Company settings
    private useAccountsHierarchy: boolean = false;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Schedule.TimeScheduleTaskTypes", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.beforeSetUpGrid())
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

        this.flowHandler.start({ feature: Feature.Time_Schedule_StaffingNeeds_TaskTypes, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_StaffingNeeds_TaskTypes].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_TaskTypes].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private beforeSetUpGrid(): ng.IPromise<any> {
        return this.loadCompanySettings();
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private setUpGrid() {
        var translationKeys: string[] = [
            "common.name",
            "common.description",
            "common.user.attestrole.accounthierarchy",
            "core.edit"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.gridAg.addColumnText("name", terms["common.name"], 100, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            if (this.useAccountsHierarchy)
                this.gridAg.addColumnText("accountName", terms["common.user.attestrole.accounthierarchy"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"],this.edit.bind(this));

            this.gridAg.finalizeInitGrid("time.schedule.timescheduletasktypes.types", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    private loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getTimeScheduleTaskTypesGrid(useCache).then(x => {
                this.setData(x);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    public edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}
