import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { StockDTO } from "../../../../../Common/Models/StockDTO";
import { ToolBarUtility, ToolBarButton } from "../../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { IStockService } from "../../../Stock/StockService";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/MessagingHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IToolbarFactory } from "../../../../../Core/Handlers/ToolbarFactory";
import { SelectColumnOptions } from "../../../../../Util/SoeGridOptionsAg";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";

export class StocksDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Products/Products/Directives/Views/Stocks.html'),
            scope: {
                productId: '=',
                stocksForProduct: '=',
                defaultStockId: '=',
                readOnly: '=?',
                parentGuid: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: StocksDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class StocksDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    // Setup
    private stocksForProduct: StockDTO[];
    private defaultStockId: number;
    private readOnly: boolean;
    private purchasePermission: boolean;
    private parentGuid;

    // Collections
    stocks: any[] = [];
    stocksForCompany: any[] = [];
    stockShelves: any[] = [];

    //@ngInject
    constructor(
        private stockService: IStockService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super(gridHandlerFactory, "Billing.Products.Products.Views.Stocks", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
        this.onInit({});
    }

    onInit(parameters: any) {
        this.setupWatchers();
        this.flowHandler.start([
            { feature: Feature.Billing_Stock_Place, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Purchase, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.purchasePermission = response[Feature.Billing_Purchase].modifyPermission;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.stocksForProduct, () => {
            this.setData(this.stocksForProduct || []);
        });
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadStocksForCompany(),
            this.loadShelfsForCompany(),
        ])
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            () => { return !this.stocksForProduct },
            () => { return this.readOnly; }
        )));
    }

    public setupGrid() {
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(8);

        const events: GridEvent[] = [
            new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }),
        ];
        this.gridAg.options.subscribe(events);

        this.setupGridColumns();
    }

    private setupGridColumns() {
        const keys: string[] = [
            "billing.product.stocks.stock.name",
            "billing.product.stocks.stock.stockshelfname",
            "common.quantity",
            "billing.product.stocks.stock.avgprice",
            "billing.stock.stocksaldo.leadtime",
            "billing.stock.stocksaldo.purchasequantity",
            "billing.stock.stocksaldo.purchasetriggerquantity"
        ];

        this.translationService.translateMany(keys).then(terms => {
            const stockSelectOptions : SelectColumnOptions = {
                selectOptions: this.stocksForCompany,
                displayField: "name",
                dropdownIdLabel: "stockId",
                dropdownValueLabel: "name",
                editable: (data) => {
                    return !data.stockProductId
                }
            }

            const editableFunc = (data: StockDTO) => !data.stockProductId;

            this.gridAg.options.addColumnSelect("stockId", terms["billing.product.stocks.stock.name"], null, stockSelectOptions);

            const shelfSelectOptions: SelectColumnOptions = {
                selectOptions: [],
                dynamicSelectOptions: {
                    options: (row) => {
                        return this.stockShelves.filter(r => r.stockId == row.stockId || r.stockId === 0);
                    },
                },
                displayField: "stockShelfName",
                dropdownIdLabel: "stockShelfId",
                dropdownValueLabel: "shelfName",
                editable: () => {
                    return true;
                }
            }
            this.gridAg.options.addColumnSelect("stockShelfId", terms["billing.product.stocks.stock.stockshelfname"], null, shelfSelectOptions);

            this.gridAg.options.addColumnNumber("saldo", terms["common.quantity"], null, { editable: false, decimals: 2, clearZero: false })
            this.gridAg.options.addColumnNumber("avgPrice", terms["billing.product.stocks.stock.avgprice"], null, { editable: editableFunc, decimals: 2, clearZero: false })

            if (this.purchasePermission) {
                this.gridAg.options.addColumnNumber("purchaseQuantity", terms["billing.stock.stocksaldo.purchasequantity"], null, { editable: true , decimals: 2, clearZero: false })
                this.gridAg.options.addColumnNumber("purchaseTriggerQuantity", terms["billing.stock.stocksaldo.purchasetriggerquantity"], null, { editable: true, decimals: 2, clearZero: false })
                this.gridAg.options.addColumnNumber("deliveryLeadTimeDays", terms["billing.stock.stocksaldo.leadtime"], null, { editable: true, decimals: 0, clearZero: false,  })
            }

            this.gridAg.addColumnDelete(terms["core.delete"], this.deleteRow.bind(this), false, editableFunc );

            this.gridAg.finalizeInitGrid("billing.product.stocks.stock", false)
        });
    }

    private afterCellEdit(row, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;
        this.setParentAsModified();
    }

    private loadStocksForCompany(): ng.IPromise<any> {
        return this.stockService.getStocks(true).then(x => {
            this.stocksForCompany = x;
        });
    }

    private loadShelfsForCompany(): ng.IPromise<any> {
        return this.stockService.getStockPlaces(true, 0).then(x => {
            this.stockShelves = x;
            this.stockShelves.forEach(r => r.shelfName = r.name); //two selects cannot use same attribute name
        });
    }


    // Actions
    private addRow() {
        let stockplace = this.stocksForCompany.find(s => s.stockId == this.defaultStockId);
        let row: StockDTO = { ...new StockDTO(), saldo: 0, avgPrice: 0, stockId: this.defaultStockId, name: stockplace.name || "", code: stockplace.code || "" };
        this.stocksForProduct.push(row);
        this.setData(this.stocksForProduct);
    }

    private deleteRow(row: StockDTO) {
        if (!row.stockProductId) {
            this.stocksForProduct.splice(this.stocksForProduct.indexOf(row), 1);
            this.setData(this.stocksForProduct);
        }
    }

    private setParentAsModified() {
        this.messagingHandler.publishSetDirty(this.parentGuid);
    }
}