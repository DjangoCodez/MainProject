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
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { ISigningService } from "../../SigningService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Company settings
    private useAccountHierarchy: boolean = false;

    public selectedAttestRoleId: number = 30;
    gridFooterComponentUrl: any;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private signingService: ISigningService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Manage.Signing.Document.Roles", progressHandlerFactory, messagingHandlerFactory);

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

        this.flowHandler.start([{ feature: Feature.Manage_Signing_Document_Roles, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Signing_Document_Roles].readPermission;
        this.modifyPermission = response[Feature.Manage_Signing_Document_Roles].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setupGrid() {
        this.gridAg.options.enableRowSelection = true;
        this.gridAg.options.enableSingleSelection();

        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
        //this.gridAg.addColumnText("showAllCategoriesText", this.useAccountHierarchy ? this.terms["manage.attest.role.showallaccounts"] : this.terms["manage.attest.role.showallcategories"], 150);
        //this.gridAg.addColumnText("showUncategorizedText", this.useAccountHierarchy ? this.terms["manage.attest.role.showwithoutaccounts"] : this.terms["manage.attest.role.showuncategorized"], 150);
        this.gridAg.addColumnNumber("defaultMaxAmount", this.terms["manage.attest.role.defaultmaxamount"], 150, { decimals: 2, enableHiding: true });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.finalizeInitGrid("manage.signing.role.roles", true);

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
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.edit",
            "common.description",
            "common.name",
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
            return this.signingService.getAttestRoles(SoeModule.Manage).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    // EVENTS

    public edit(row: any) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}