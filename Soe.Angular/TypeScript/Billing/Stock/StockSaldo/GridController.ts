import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { ImportStockSaldoDialogController } from "../../Dialogs/ImportStockSaldo/ImportStockSaldo";
import { Feature } from "../../../Util/CommonEnumerations";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    stockProducts: any[];
    private modalInstance: any;
    private purchasePermission: boolean;
    private avgPriceAndValuePermission: boolean;

    private _includeInactive = false;
    get includeInactive(): boolean{
        return this._includeInactive;
    }
    set includeInactive(value:boolean)
    {
        if (this._includeInactive !== value) {
            this._includeInactive = value;
            this.loadGridData();
        }
    }

    //@ngInject
    constructor(
        private stockService: IStockService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private commonCustomerService: ICommonCustomerService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        $uibModal) {
        super(gridHandlerFactory, "Billing.Stock.StockSaldo", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((features) => {
                this.readPermission = features[Feature.Billing_Stock].readPermission;
                this.modifyPermission = features[Feature.Billing_Stock].modifyPermission;
                this.purchasePermission = features[Feature.Billing_Purchase].readPermission;
                this.avgPriceAndValuePermission = features[Feature.Billing_Stock_ViewAvgPriceAndValue].readPermission;

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

        this.modalInstance = $uibModal;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start([
            { feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Purchase, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Stock_ViewAvgPriceAndValue, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.stock.stocksaldo.recalculatebalance", "billing.stock.stocksaldo.recalculatebalance", IconLibrary.FontAwesome, "fa-cog", () => { this.recalculateStockSaldo(); })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.stock.stocksaldo.importstockbalance", "billing.stock.stocksaldo.importstockbalance", IconLibrary.FontAwesome, "fa-upload", () => { this.importFileDialog(); })));
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "billing.stock.stocksaldo.productnumber",
            "common.name",
            "billing.stock.stocks.stock",
            "billing.stock.stockplaces.stockplace",
            "billing.stock.stocksaldo.saldo",
            "billing.stock.stocksaldo.ordered",
            "billing.stock.stocksaldo.reserved",
            "billing.stock.stocksaldo.avgprice",
            "billing.stock.stocksaldo.value",
            "billing.stock.stocksaldo.purchasetriggerquantity",
            "billing.stock.stocksaldo.purchasequantity",
            "billing.stock.stocksaldo.purchasedquantity",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("productNumber", terms["billing.stock.stocksaldo.productnumber"], null);
            this.gridAg.addColumnText("productName", terms["common.name"], null);
            this.gridAg.addColumnText("stockName", terms["billing.stock.stocks.stock"], null);
            this.gridAg.addColumnText("stockShelfName", terms["billing.stock.stockplaces.stockplace"], null);
            this.gridAg.addColumnNumber("quantity", terms["billing.stock.stocksaldo.saldo"], null, { decimals: 2, enableHiding:false });
            this.gridAg.addColumnNumber("orderedQuantity", terms["billing.stock.stocksaldo.ordered"], null, { decimals: 2, enableHiding: false });
            this.gridAg.addColumnNumber("reservedQuantity", terms["billing.stock.stocksaldo.reserved"], null, { decimals: 2, enableHiding: false });

            if (this.avgPriceAndValuePermission) {
                this.gridAg.addColumnNumber("avgPrice", terms["billing.stock.stocksaldo.avgprice"], null, { decimals: 2, enableHiding: false });
                this.gridAg.addColumnNumber("stockValue", terms["billing.stock.stocksaldo.value"], null, { decimals: 2, enableHiding: false });
            }

            if (this.purchasePermission) {
                this.gridAg.addColumnNumber("purchaseTriggerQuantity", terms["billing.stock.stocksaldo.purchasetriggerquantity"], null, { decimals: 2, enableHiding: false });
                this.gridAg.addColumnNumber("purchaseQuantity", terms["billing.stock.stocksaldo.purchasequantity"], null, { decimals: 2, enableHiding: false });
                this.gridAg.addColumnNumber("purchasedQuantity", terms["billing.stock.stocksaldo.purchasedquantity"], null, { decimals: 2, enableHiding: false });
            }

            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.addFooterRow("#sum-footer-grid", {
                "quantity": "sum",
                "orderedQuantity": "sum",
                "reservedQuantity": "sum",
                "stockValue": "sum",
                "purchasedQuantity": "sum"
            } as IColumnAggregations);

            this.gridAg.finalizeInitGrid("billing.stock.stocksaldo.stocksaldo", true);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.stockService.getStockProducts(this.includeInactive).then((x) => {
                _.forEach(x, (y) => {
                    y.name = y.productName;
                });
                this.setData(x);
            });
        }]);
    }

    public recalculateStockSaldo() {
        this.progress.startWorkProgress((completion) => {
            this.stockService.recalculateStockBalance(0).then((result) => {
                if (result.success) {
                    completion.completed(null, true);
                    this.loadGridData();
                }
                else { completion.failed(result.errorMessage); }
            })
        });
    }

    public importFileDialog() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Dialogs/ImportStockSaldo/ImportStockSaldo.html"),
            controller: ImportStockSaldoDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                stockService: () => { return this.stockService },
                commonCustomerService: () => { return this.commonCustomerService },
                notificationService: () => { return this.notificationService },
                stockInventoryHeadId: () => { return null }
            }
        });

        modal.result.then((result: any) => {
            if ((result) && (result.reportId)) {
                //this.printOrder(result.email, result.reportId, result.languageId);
            }
        });
    }
}
