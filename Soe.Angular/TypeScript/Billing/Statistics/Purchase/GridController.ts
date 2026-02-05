import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
//import { IPurchaseStatisticsDTO } from "../../../Scripts/TypeLite.Net4";
import { IPurchaseService } from "../../../Shared/Billing/Purchase/Purchase/PurchaseService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    public searchModel: any = {};

    // Terms
    terms: { [index: string]: string; };

    // Selection
    private selectedDateFrom: Date;
    private selectedDateTo: Date;

    //@ngInject
    constructor(
        private purchaseService: IPurchaseService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Soe.Billing.Statistics.Purchase", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupStatisticsGrid())
            .onLoadGridData(() => this.setData(""));

        this.doubleClickToEdit = false;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadPurchaseStatistics(); });
        }

        const today: Date = CalendarUtility.getDateToday();
        this.selectedDateFrom = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        this.selectedDateTo = today;

        this.flowHandler.start({ feature: Feature.Billing_Statistics_Purchase, loadReadPermissions: true, loadModifyPermissions: false });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadPurchaseStatistics());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("searchHeader.html"));
    }
    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Statistics_Purchase].readPermission;

        if (this.readPermission) {
            this.messagingHandler.publishActivateAddTab();
        }
    }

    public setupStatisticsGrid() {

        const keys: string[] = [
            "billing.purchase.suppliername",
            "billing.purchase.supplierno",
            "billing.purchase.supplier",
            "common.customer.invoices.articlename",
            "billing.purchaserows.productnr",
            "billing.purchaserows.quantity",
            "billing.purchaserows.purchaseunit",
            "billing.purchase.purchasenr",
            "common.purchaseprice",
            "billing.purchase.purchasedate",
            "billing.purchaserows.wanteddeliverydate",
            "billing.purchaserows.accdeliverydate",
            "billing.purchaserows.deliveredquantity",
            "billing.purchaserows.deliverydate",
            "billing.purchaserows.sumamount",
            "billing.purchaserows.discount",
            "billing.purchaserow.totalexvat",
            "billing.purchase.foreignamount",
            "common.order",
            "common.status",
            "common.customer.invoices.currencycode",
            "common.report.selection.projectnr",
            "common.report.selection.stockplace",
            "common.customer.invoices.rowstatus",
            "common.code",
            "common.customer.invoices.foreignamount",
            "billing.purchase.product.supplieritemno",
            "billing.purchase.product.supplieritemname",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            // Set name
            this.gridAg.options.setName("statisticsGrid");

            // Add aggregation
            this.gridAg.options.addGroupAverageAggFunction();

            const numberOptions = { decimals: 2, enableHiding: true, enableRowGrouping: true, aggFuncOnGrouping: 'sum' };
            const columnOptions = { enableRowGrouping: true, enableHiding: true };

            this.gridAg.addColumnText("supplierNumberName", terms["billing.purchase.supplier"], null, true, columnOptions);
            this.gridAg.addColumnText("supplierItemNumber", terms["billing.purchase.product.supplieritemno"], null, true, columnOptions);
            this.gridAg.addColumnText("supplierItemName", terms["billing.purchase.product.supplieritemname"], null, true, columnOptions);
            this.gridAg.addColumnText("productNumber", terms["billing.purchaserows.productnr"], null, true, columnOptions);
            this.gridAg.addColumnText("productName", terms["common.customer.invoices.articlename"], null, true, columnOptions);
            this.gridAg.addColumnNumber("quantity", terms["billing.purchaserows.quantity"], null, { enableRowGrouping: true, aggFuncOnGrouping: 'sum', enableHiding: true });
            this.gridAg.addColumnText("unit", terms["billing.purchaserows.purchaseunit"], null, true, columnOptions);
            this.gridAg.addColumnNumber("purchasePrice", terms["common.purchaseprice"], null, { decimals: 2, enableHiding: true, enableRowGrouping: true });
            this.gridAg.addColumnText("purchaseNr", terms["billing.purchase.purchasenr"], null, true, columnOptions);
            this.gridAg.addColumnDate("purchaseDate", terms["billing.purchase.purchasedate"], null, true, null, columnOptions);
            this.gridAg.addColumnDate("wantedDeliveryDate", terms["billing.purchaserows.wanteddeliverydate"], null, true, null, columnOptions);
            this.gridAg.addColumnDate("acknowledgeDeliveryDate", terms["billing.purchaserows.accdeliverydate"], null, true, null, columnOptions);
            this.gridAg.addColumnText("customerOrderNumber", terms["common.order"], null, true, columnOptions);
            this.gridAg.addColumnNumber("deliveredQuantity", terms["billing.purchaserows.deliveredquantity"], null, { enableRowGrouping: true, aggFuncOnGrouping: 'sum', enableHiding: true });
            this.gridAg.addColumnDate("deliveryDate", terms["billing.purchaserows.deliverydate"], null, true, null, columnOptions);
            this.gridAg.addColumnNumber("sumAmount", terms["billing.purchaserow.totalexvat"], null, numberOptions);
            this.gridAg.addColumnNumber("sumAmountCurrency", terms["billing.purchase.foreignamount"], null, numberOptions);
            this.gridAg.addColumnText("currencyCode", terms["common.customer.invoices.currencycode"], null, true, columnOptions);
            this.gridAg.addColumnText("projectNumber", terms["common.report.selection.projectnr"], null, true, columnOptions);
            this.gridAg.addColumnText("statusName", terms["common.status"], null, true, columnOptions);
            this.gridAg.addColumnText("rowStatusName", terms["common.customer.invoices.rowstatus"], null, true, columnOptions);
            this.gridAg.addColumnText("stockPlace", terms["common.report.selection.stockplace"], null, true, columnOptions);
            this.gridAg.addColumnText("code", terms["common.code"], null, true, columnOptions);

            this.gridAg.options.useGrouping();
            this.gridAg.finalizeInitGrid("billing.purchase.statistics", true);
        });
    }

    private loadPurchaseStatistics() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.purchaseService.getPurchaseStatistics(this.selectedDateFrom, this.selectedDateTo).then((x: any[]) => {
                this.setData(x);
            });
        }]);
    }
}
