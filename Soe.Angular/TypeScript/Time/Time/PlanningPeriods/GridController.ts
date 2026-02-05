import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ITimeService } from "../TimeService";
import { Feature, TermGroup_TimePeriodType, TermGroup, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Company settings
    private useAccountHierarchy = false;
    private defaultEmployeeAccountDimId = 0;
    private useAveragingPeriod = false;
    private accountDim: AccountDimSmallDTO;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private sharedAccountingService: IAccountingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {
        super(gridHandlerFactory, "Time.Time.PlanningPeriod", progressHandlerFactory, messagingHandlerFactory);
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
            .onLoadSettings(()=>this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadAccountDim())
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_PlanningPeriod, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        let keys: string[] = [
            "common.name",
            "common.description",
            "core.edit",
            "time.time.planningperiod.child"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (this.useAccountHierarchy)
                this.gridAg.addColumnText("accountName", this.accountDim ? this.accountDim.name : '', 100);
            this.gridAg.addColumnText("name", terms["common.name"], 100);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            if (this.useAveragingPeriod)
                this.gridAg.addColumnText("childName", terms["time.time.planningperiod.child"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.time.planningperiod.planningperiods", true);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.useAveragingPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);
        });
    }

    private loadAccountDim(): ng.IPromise<any> {
        return this.sharedAccountingService.getAccountDimSmall(this.defaultEmployeeAccountDimId, true, false).then(x => {
            this.accountDim = x;
        });
    }

    public loadGridData() {
        // Load data
        this.timeService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.RuleWorkTime, false, true, this.useAveragingPeriod).then(heads => {
            this.setData(heads);
        });
    }

    private reloadData() {
        this.loadGridData();
    }
}
