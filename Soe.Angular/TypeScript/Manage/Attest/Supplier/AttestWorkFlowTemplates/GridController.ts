import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandler } from "../../../../Core/Handlers/MessagingHandler";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IToolbar } from "../../../../Core/Handlers/Toolbar";
import { Feature, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IAttestService } from "../../AttestService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $filter: ng.IFilterService,
        private attestService: IAttestService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $uibModal) {

        super(gridHandlerFactory, "Manage.Attest.Supplier.AttestWorkFlowTemplates", progressHandlerFactory, messagingHandlerFactory);  

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })            
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Attest_Supplier_WorkFlowTemplate, loadReadPermissions: true, loadModifyPermissions: true });
    }    

    public edit(row: any) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    };

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this))

            this.gridAg.finalizeInitGrid("manage.attest.supplier.attestworkflowtemplate.attestworkflowtemplates", true);
        });
    }

    public loadGridData() {
        // Load data
        this.attestService.getAttestWorkFlowTemplates(TermGroup_AttestEntity.SupplierInvoice).then((x) => {
            this.gridAg.setData(x);
        });
    }

    private reloadData() {
        this.loadGridData();
    }
}
