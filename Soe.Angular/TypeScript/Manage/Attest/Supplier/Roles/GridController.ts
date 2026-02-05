import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IAttestService } from "../../AttestService";
import { Feature, SoeModule } from "../../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Terms:
    private terms: any;

    //@ngInject
    constructor(
        private attestService: IAttestService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Manage.Attest.Supplier.Roles", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
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
        this.flowHandler.start([{ feature: Feature.Manage_Attest_Supplier_AttestRoles, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Attest_Supplier_AttestRoles].readPermission;
        this.modifyPermission = response[Feature.Manage_Attest_Supplier_AttestRoles].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public setupGrid() {
        // Columns
        this.gridAg.options.enableRowSelection = false;

        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("description", this.terms["common.description"], 400);
        this.gridAg.addColumnNumber("defaultMaxAmount", this.terms["manage.attest.role.defaultmaxamount"], null, { decimals: 2, enableHiding: true });
        this.gridAg.addColumnText("showAllCategoriesText", this.terms["manage.attest.role.showallcategories"], null);
        this.gridAg.addColumnText("showUncategorizedText", this.terms["manage.attest.role.showuncategorized"], null);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.finalizeInitGrid("manage.attest.role.roles", true);
    }

    // SERVICE CALLS
    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.name",
            "common.description",
            "manage.attest.role.defaultmaxamount",
            "manage.attest.role.showallcategories",
            "manage.attest.role.showuncategorized",
            "core.edit"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.attestService.getAttestRoles(SoeModule.Economy, false).then(x => {
                this.setData(x);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
}