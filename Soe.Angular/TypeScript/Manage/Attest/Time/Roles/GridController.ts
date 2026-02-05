import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { CompanySettingType, Feature, SoeModule } from "../../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { SOEMessageBoxButtons, SOEMessageBoxImage, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { IAttestService } from "../../AttestService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { ISelectedItemsService } from "../../../../Core/Services/SelectedItemsService";
import { IActionResult, IUpdateAttestRoleModel } from "../../../../Scripts/TypeLite.Net4";
import { INotificationService } from "../../../../Core/Services/NotificationService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Company settings
    private useAccountHierarchy: boolean = false;

    public selectedAttestRoleId: number = 30;
    gridFooterComponentUrl: any;
    gridHasSelectedRows: boolean;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private attestService: IAttestService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Manage.Attest.Time.Roles", progressHandlerFactory, messagingHandlerFactory);
        this.selectedItemsService.setup($scope, "attestRoleId", (items: number[]) => this.save(items));
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {
                this.reloadData();
            });
        }

        this.flowHandler.start([{ feature: Feature.Manage_Attest_Time_AttestRoles, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Attest_Time_AttestRoles].readPermission;
        this.modifyPermission = response[Feature.Manage_Attest_Time_AttestRoles].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setupGrid() {
        this.gridAg.options.enableRowSelection = false;
       
        this.gridAg.addColumnActive("isActive", this.terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
        this.gridAg.addColumnText("name", this.terms["common.name"], 100);
        this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
        this.gridAg.addColumnText("externalCodesString", this.terms["common.externalcode"], 50);
        this.gridAg.addColumnNumber("sort", this.terms["common.level"], 40);
        this.gridAg.addColumnText("showAllCategoriesText", this.useAccountHierarchy ? this.terms["manage.attest.role.showallaccounts"] : this.terms["manage.attest.role.showallcategories"], 150);
        this.gridAg.addColumnText("showUncategorizedText", this.useAccountHierarchy ? this.terms["manage.attest.role.showwithoutaccounts"] : this.terms["manage.attest.role.showuncategorized"], 150);
        this.gridAg.addColumnNumber("defaultMaxAmount", this.terms["manage.attest.role.defaultmaxamount"], 150, { decimals: 2, enableHiding: true });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.finalizeInitGrid("manage.attest.role.roles", true, undefined, true);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => {
            if (rowNode && rowNode.length > 0) {
                this.$timeout(() => {
                    this.selectedAttestRoleId = rowNode[0].attestRoleId;
                });
            }
        }));
        this.gridAg.options.subscribe(events);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.edit",
            "common.description",
            "common.name",
            "common.externalcode",
            "common.level",
            "common.active",
            "manage.attest.role.defaultmaxamount",
            "manage.attest.role.showallaccounts",
            "manage.attest.role.showallcategories",
            "manage.attest.role.showuncategorized",
            "manage.attest.role.showwithoutaccounts"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.attestService.getAttestRoles(SoeModule.Time, true).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private save(items: number[]) {
        let dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            const entity: any = this.gridAg.options.findInData((ent: any) => ent["attestRoleId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if (dict !== undefined) {
            const updateAttestRoleState: IUpdateAttestRoleModel = { dict: dict, module: SoeModule.Time };
            this.attestService.updateAttestRoleState(updateAttestRoleState).then((result: IActionResult) => {
                if (!result.success) {
                    this.notificationService.showDialog("", result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                }

                this.loadGridData();
            });
        }
    }

    // EVENTS

    public edit(row: any) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
    private saveButtonIsDisabled(): boolean {
        return !this.selectedItemsService.SelectedItemsExist();
    }

    
}