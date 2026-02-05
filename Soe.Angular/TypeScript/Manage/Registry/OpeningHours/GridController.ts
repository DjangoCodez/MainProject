import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IRegistryService } from "../RegistryService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { CompanySettingType, Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: { [index: string]: string; };

    // Data
    private days: SmallGenericType[] = [];
    private useAccountsHierarchy: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private registryService: IRegistryService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Manage.Registry.OpeningHours", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => {
                this.reloadData();
            });
        }

        this.flowHandler.start([
            { feature: Feature.Manage_Preferences_Registry_OpeningHours, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadDays(),
            this.loadCompanySettings()]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Preferences_Registry_OpeningHours].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_OpeningHours].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadDays(): ng.IPromise<any> {
        this.days = [];
        return this.coreService.getTermGroupContent(TermGroup.StandardDayOfWeek, false, true).then((x) => {
            this.days = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "core.edit",
            "common.name",
            "common.validfrom",
            "manage.registry.openinghours.closingtime",
            "manage.registry.openinghours.description",
            "manage.registry.openinghours.openingtime",
            "manage.registry.openinghours.specificdate",
            "manage.registry.openinghours.standardweekday",
            "common.user.attestrole.accounthierarchy"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["manage.registry.openinghours.description"], null);
            this.gridAg.addColumnText("weekdayName", terms["manage.registry.openinghours.standardweekday"], 200);
            this.gridAg.addColumnDate("specificDate", terms["manage.registry.openinghours.specificdate"], 120);
            this.gridAg.addColumnTime("openingTime", terms["manage.registry.openinghours.openingtime"], 100);
            this.gridAg.addColumnTime("closingTime", terms["manage.registry.openinghours.closingtime"], 100);
            this.gridAg.addColumnDate("fromDate", terms["common.validfrom"], 120);
            if (this.useAccountsHierarchy)
                this.gridAg.addColumnText("accountName", this.terms["common.user.attestrole.accounthierarchy"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("manage.registry.openinghours.openinghours", false);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.registryService.getOpeningHours(useCache).then(data => {
                _.forEach(data, row => {
                    if (row.standardWeekDay === 0) {
                        row.weekdayName = "";
                    } else {
                        let weekday = _.find(this.days, s => s.id === row.standardWeekDay);
                        row.weekdayName = weekday.name;
                    }
                });

                this.setData(data);
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