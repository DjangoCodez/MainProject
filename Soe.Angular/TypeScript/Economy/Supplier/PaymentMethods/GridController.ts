import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { Feature, SoeOriginType, TermGroup } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //@ngInject
    constructor(
        private $q: ng.IQService,
        private supplierService: ISupplierService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Supplier.PaymentMethods", progressHandlerFactory, messagingHandlerFactory);

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

        this.flowHandler.start({ feature: Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "economy.common.paymentmethods.exporttype",
            "economy.common.paymentmethods.paymentnr",
            "economy.common.paymentmethods.customernr",
            "economy.common.paymentmethods.accountnr",
            "economy.common.paymentmethods.payerbankid",
            "economy.supplier.invoice.currencycode",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null, true);
            this.gridAg.addColumnSelect("sysPaymentMethodName", terms["economy.common.paymentmethods.exporttype"], null, { displayField: "sysPaymentMethodName", selectOptions: null, populateFilterFromGrid: true, enableHiding: true});
            this.gridAg.addColumnText("paymentNr", terms["economy.common.paymentmethods.paymentnr"], null, true);
            this.gridAg.addColumnText("customerNr", terms["economy.common.paymentmethods.customernr"], null, true);
            this.gridAg.addColumnText("accountNr", terms["economy.common.paymentmethods.accountnr"], null, true);
            this.gridAg.addColumnText("payerBankId", terms["economy.common.paymentmethods.payerbankid"], null, true);
            this.gridAg.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.supplier.paymentmethod.paymentmethods", true);
        });
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getPaymentMethods(SoeOriginType.SupplierPayment, false, false, true, false).then((data) => {
                this.setData(data);
            });
        }]);
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }
}
