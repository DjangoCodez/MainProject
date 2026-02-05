import { StockTransactionDTO } from "../../../../Common/Models/StockTransactionDTO";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { StringUtility } from "../../../../Util/StringUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { TermGroup_StockTransactionType } from "../../../../Util/CommonEnumerations";
import { IStockService } from "../../../Billing/Stock/StockService";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { GridEvent } from "../../../../Util/SoeGridOptions";

export class MoveProductRowsToStockController {

    // Terms
    private terms: any;

    //Grid
    private soeGridOptions: ISoeGridOptionsAg;

    // Values
    private _selectedStock: any
    get selectedStock(): any {
        return this._selectedStock;
    }

    set selectedStock(value: any) {
        this._selectedStock = value;
        this.setStockValues();
    }

    private functionName: string;
    private functionNameDefinitive: string;
    private functionNameVerb: string;
    private moveSelection: string;
    private toolbarTitle: string;
    private selectLabel: string;
    private workingMessage: string;

    // Collections
    private stocksDict: SmallGenericType[] = [];
    private stockRows: StockTransactionDTO[] = [];
    private stockProducts: any[];

    // Flags
    private working: boolean = false;
    private showStatus: boolean = false;

    public rowSelected: boolean = false;

    //@ngInject
    constructor(
        $uibModal,
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        messagingService: IMessagingService,
        private stockService: IStockService,
        private notificationService: INotificationService,
        coreService: ICoreService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private productRows: ProductRowDTO[],
        private invoiceId: number
    ) {
        this.init();
    }

    private init() {
        this.convertProductRowsToStockRow();

        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.MoveProductRowsToStock.ProductRows", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.setMinRowsToShow(8);

        this.soeGridOptions.subscribe([
            new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
                this.$timeout(() => {
                    this.rowSelected = this.soeGridOptions.getSelectedRows().length > 0;
                });
            }),
        ]);

        this.$q.all([
            this.loadTerms(), this.loadStocks(), this.loadStockProducts()]).then(() => {
                this.setTerms();
                this.setupGridColumns();
            });
    }

    public loadStocks(): ng.IPromise<any> {
        return this.stockService.getStocks(false).then((x) => {
            this.stocksDict = x;
        });
    }

    public loadStockProducts(): ng.IPromise<any> {
        const ids = _.map(this.stockRows, 'productId');
        return this.stockService.getStockProductsByProducts(ids).then((x) => {
            this.stockProducts = x;
        });
    }

    private convertProductRowsToStockRow(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        _.forEach(this.productRows, (r) => {
            const stockTrans = new StockTransactionDTO();
            stockTrans.productId = r.productId;
            stockTrans.quantity = r.quantity;
            stockTrans.price = r.purchasePrice;
            stockTrans.productName = r.productName;
            stockTrans.productNumber = r.productNr;
            stockTrans.invoiceRowId = r.customerInvoiceRowId;
            this.stockRows.push(stockTrans);
        });

        deferral.resolve();
        return deferral.promise;
    }

    private setStockValues() {
        this.$timeout(() => {
            _.forEach(this.stockRows, (row) => {
                const stockProduct = _.find(this.stockProducts, (p) => p.invoiceProductId === row.productId && p.stockId === this._selectedStock.stockId);

                row.stockProductId = stockProduct?.stockProductId ?? undefined;
                row.stockShelfId = stockProduct?.stockShelfId ?? undefined;
                row.stockShelfName = stockProduct?.stockShelfName ?? "";

                if (row.stockShelfId)
                    this.soeGridOptions.selectRow(row, true);
            });

            this.soeGridOptions.setData(this.stockRows);
        }, null);
        this.soeGridOptions.clearSelectedRows();
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.rownr",
            "common.name",
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.productrows.quantity",
            "billing.productrows.purchaseprice",
            "core.copy",
            "core.thecopying",
            "billing.stock.stocks.stock",
            "core.copying",
            "core.move",
            "core.themove",
            "core.moving",
            "billing.order.productrows",
            "core.succeeded",
            "core.failed",
            "billing.dialogs.moverowstostock.stockshelfname",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnText("productNumber", this.terms["billing.productrows.productnr"], null)
        this.soeGridOptions.addColumnText("productName", this.terms["common.name"], null)
        this.soeGridOptions.addColumnNumber("price", this.terms["billing.productrows.purchaseprice"], null);
        this.soeGridOptions.addColumnNumber("quantity", this.terms["billing.productrows.quantity"], null, { editable: true }); //edit
        this.soeGridOptions.addColumnText("stockShelfName", this.terms["billing.dialogs.moverowstostock.stockshelfname"], null)

        this.soeGridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (row: any) => {
            return row && row.data && row.data.stockProductId;
        })]);

        this.soeGridOptions.getColumnDefs().forEach(col => {
            var cellcls: string = col.cellClass ? col.cellClass.toString() : "";
            col.cellClass = (grid: any) => {
                return cellcls + (!grid.data.stockProductId ? " closedRow" : "");
            };
        });

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });

        this.soeGridOptions.finalizeInitGrid();
        this.soeGridOptions.setData(this.stockRows);
    }

    private setTerms() {
        this.functionName = this.terms["core.move"];
        this.functionNameDefinitive = this.terms["core.themove"];
        this.functionNameVerb = this.terms["core.moving"];
        this.toolbarTitle = this.functionName + " " + this.terms["billing.order.productrows"].toLowerCase();
        this.selectLabel = this.terms["billing.stock.stocks.stock"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonEnabled(): boolean {
        return (this.selectedStock && this.rowSelected);
    }

    buttonOkClick() {
        const rowsToMove = this.soeGridOptions.getSelectedRows();
        if (rowsToMove.length === 0)
            return;

        _.forEach(rowsToMove, (r) => {
            r.actionType = TermGroup_StockTransactionType.Add;
            r.stockId = this.selectedStock.stockId;
        });

        this.stockService.saveStockTransactions(rowsToMove).then((result) => {
            if (result.success) {
                const changedProductRows: ProductRowDTO[] = [];
                _.forEach(rowsToMove, (r) => {
                    const productRow = _.find(this.productRows, { customerInvoiceRowId: r.invoiceRowId });
                    if (productRow) {
                        productRow.isStockRow = true;
                        productRow.quantity = productRow.quantity - r.quantity;
                        productRow.isModified = true;
                        changedProductRows.push(productRow);
                    }
                });
                this.close({ success: true, changedRows: changedProductRows });
            }
            else {
                this.notificationService.showDialog(this.terms["billing.stock.stocksaldo.importstockbalance"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            }
        });
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        } else {
            this.$uibModalInstance.close(result);
        }
    }
}
