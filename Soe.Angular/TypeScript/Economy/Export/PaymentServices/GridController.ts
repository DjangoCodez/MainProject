import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    public exportType: number;
    // Collections
    termsArray: any;
    // Subgrid rows
    subGridRows: any;
    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,) {
        super(gridHandlerFactory, "Economy.Export.PaymentServices", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Export_Invoices_PaymentService, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Export_Invoices_PaymentService].readPermission;
        this.modifyPermission = response[Feature.Economy_Export_Invoices_PaymentService].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
    }

    public setupGrid() {

        // Columns
        var keys: string[] = [
            "economy.export.paymentservice.batchcid",
            "economy.export.paymentservice.exportdate",
            "economy.export.paymentservice.paymentservice",
            "economy.export.paymentservice.totalamount",
            "economy.export.paymentservice.numberofinvoices",
            "core.createdby",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;
            this.gridAg.addColumnText("batchId", terms["economy.export.paymentservice.batchcid"], 125);
            this.gridAg.addColumnDate("exportDate", terms["economy.export.paymentservice.exportdate"], null);
            this.gridAg.addColumnText("sysPaymentServiceId", terms["economy.export.paymentservice.paymentservice"], null);
            this.gridAg.addColumnText("totalAmount", terms["economy.export.paymentservice.totalamount"], null);
            this.gridAg.addColumnText("numberOfInvoices", terms["economy.export.paymentservice.numberofinvoices"], null);
            this.gridAg.addColumnText("createdBy", terms["core.createdby"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("economy.export.paymentServices.paymentServices", true);
        });
    }

    public loadGridData() {
        // Load data
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getPaymentServiceRecords().then(data => {
                this.setData(data);
            });
        }]);
    }

    protected showDownloadIcon(row) {
        return true;
    }

    private showDownload(row) {
        //Show downloadlink            
    }
}