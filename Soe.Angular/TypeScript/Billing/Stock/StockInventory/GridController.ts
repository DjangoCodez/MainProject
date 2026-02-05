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
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { Feature } from "../../../Util/CommonEnumerations";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private stockService: IStockService,
        private translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Stock.StockInventory", progressHandlerFactory, messagingHandlerFactory);

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

        this.flowHandler.start({ feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.name",
            "billing.stock.stock",
            "billing.stock.stocks.stock",
            "billing.stock.stockinventory.inventorystart",
            "billing.stock.stockinventory.inventorystop",
            "common.createdby",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("headerText", terms["common.name"], null);
            this.gridAg.addColumnText("stockName", terms["billing.stock.stocks.stock"], null);
            this.gridAg.addColumnText("inventoryStart", terms["billing.stock.stockinventory.inventorystart"], null);
            this.gridAg.addColumnText("inventoryStop", terms["billing.stock.stockinventory.inventorystop"], null);
            this.gridAg.addColumnText("createdBy", terms["common.createdby"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            
            this.gridAg.finalizeInitGrid("economy.inventory.inventories.inventory", true);

        });
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.stockService.getStockInventories().then((x) => {
                _.forEach(x, (y) => {
                    y.inventoryStart = CalendarUtility.toFormattedDate(y.inventoryStart);
                    y.inventoryStop = CalendarUtility.toFormattedDate(y.inventoryStop);
                    y.name = y.headerText;
                });
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