import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //@ngInject
    constructor(
        private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Invoices.DeliveryType", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Billing_Preferences_DeliveryType, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.code",
            "common.name",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("code", terms["common.code"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            
            this.gridAg.finalizeInitGrid("billing.invoices.deliverytype.deliverytype", true);
        });
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getDeliveryTypes().then((x) => {
                this.setData(x);
            });
        }]);
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }
}
