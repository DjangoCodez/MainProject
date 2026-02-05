import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IProductService } from "../../../../Shared/Billing/Products/ProductService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { ISupplierProductPriceDTO } from "../../../../Scripts/TypeLite.Net4";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ISupplierService } from "../../../Economy/Supplier/SupplierService";
import { StockDTO } from "../../../../Common/Models/StockDTO";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { PurchaseRowDTO } from "../../../../Common/Models/PurchaseDTO";
import { ProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IPurchaseService } from "../../Purchase/Purchase/PurchaseService";
import { PurchaseRowType, SoeInvoiceRowType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { EditController as PurchaseEditController } from "../../../../Shared/Billing/Purchase/Purchase/EditController";
import { ISupplierProductService } from "../../Purchase/Purchase/SupplierProductService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";

export class CreatePurchaseController {
    private progressHandler: IProgressHandler;

    // Terms
    private terms: any;
    
    private success: boolean = false;
    private onGoingInvoiceNr: string
    private modelTitle: string = "";
    private isReadonly = false;

    // Search
    private existingPurchases: any[] = [];
    private existingFilteredPurchases: any[] = [];
    private suppliers: any[] = [];

    private copyProject = true;
    private copydeliveryaddress = false;

    private _selectedPurchase: any;
    get selectedPurchase() {
        return this._selectedPurchase;
    }
    set selectedPurchase(item: any) {
        if (!this.createNewPurchase && item) {
            this.selectedSupplier = this.suppliers.find(s => s.actorSupplierId === item.supplierId);
        }
        if (item) {
            this.setPurchaseOnRows(item);
        }
        this._selectedPurchase = item;
    }

    private _selectedSupplier: any;
    get selectedSupplier() {
        return this._selectedSupplier;
    }
    set selectedSupplier(item: any) {
        if (this._selectedSupplier !== item) {
            this._selectedSupplier = item;
            if (this.selectedPurchase) {
                this.selectedPurchase = undefined;
            }
            this.clearPurchaseNr();
            this.setPurchasePrices();
        }
    }

    private _createNewPurchase = false;
    get createNewPurchase() {
        return this._createNewPurchase;
    }
    set createNewPurchase(val: boolean) {
        this._createNewPurchase = val;
        if (this._createNewPurchase === true) {
            if (this.selectedPurchase) {
                this.clearPurchaseNr();
                this.selectedPurchase = null;
            }
        } else {
            this.selectedSupplier = null;
        }
    }

    private purchaseRows: PurchaseRowDTO[]
    private purchaseDate: Date;

    private soeGridOptions: ISoeGridOptionsAg;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private purchaseService: IPurchaseService,
        private supplierService: ISupplierService,
        private productService: IProductService,
        private supplierProductService: ISupplierProductService,
        private $q: ng.IQService,
        productRows: ProductRowDTO[],
        private $uibModal,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private invoiceId: number,
        private currencyId: number,
        onGoingInvoiceNr: string

    ) {
        this.purchaseDate = CalendarUtility.getDateToday();
        this.onGoingInvoiceNr = onGoingInvoiceNr;
        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.CreatePurchase.ProductRows", this.$timeout);
        this.soeGridOptions.translateText = (key, defaultValue) => {
            return this.translationService.translateInstant("core.aggrid." + key);
        }
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(8);

        this.purchaseRows = this.createPurchaseRows(productRows);
        this.setReadonly();
        this.progressHandler = new ProgressHandlerFactory(this.$uibModal, this.translationService, this.$q, this.messagingService, this.urlHelperService, null).create();
        this.startLoad();
    }

    private startLoad() {
        this.progressHandler.startLoadingProgress([() =>
            this.$q.all([
                this.getPurchases(),
                this.getSuppliers(),
                this.loadTerms()
            ]).then(() => {
                this.setupGrid();
                this.setModelTitle();
            })
        ])
    }

    private createPurchaseRows(rows: ProductRowDTO[]): PurchaseRowDTO[] {
        rows = rows.filter(r => r.customerInvoiceRowId && r.type === SoeInvoiceRowType.ProductRow);
        return rows.map(r => {
            return {
                ...new PurchaseRowDTO(),
                purchasePrice: 0,
                purchasePriceCurrency: r.purchasePriceCurrency,
                ["purchasePriceOrder"]: r.purchasePriceCurrency,
                sumAmount: r.purchasePrice * r.quantity,
                sumAmountCurrency: r.purchasePriceCurrency * r.quantity,
                productId: r.productId,
                productName: r.productName,
                purchaseUnitId: r.productUnitId,
                productNr: r.productNr,
                isTextRow: r.isTextRow,
                text: r.text,
                quantity: r.quantity,
                stockCode: r.stockCode,
                stockId: r.stockId,
                orderId: this.invoiceId,
                vatRate: r.vatRate,
                vatAmount: (r.vatRate * 0.01) * r.purchasePrice * r.quantity,
                vatAmountCurrency: (r.vatRate * 0.01) * r.purchasePriceCurrency * r.quantity,
                type: r.isTextRow ? PurchaseRowType.TextRow : PurchaseRowType.PurchaseRow,
                purchaseProductUnitCode: r.productUnitCode,
                stocksForProduct: [],
                customerInvoiceRowIds: [r.customerInvoiceRowId],
                purchaseNr: r.purchaseNr,
                purchaseId: r.purchaseId,
                ["orgPurchaseNr"]: r.purchaseNr,
            }
        })
    }

    private setReadonly() {
        this.isReadonly = true;
        this.purchaseRows.forEach( row => {
            if (!this.hasOrgPurchaseNr(row)) {
                this.isReadonly = false;
            }
        })
    }

    private getPurchases(): ng.IPromise<any> {
        return this.purchaseService.getPurchaseOrdersForSelect(false)
            .then(data => {                
                this.existingPurchases = data || [];
                this.existingFilteredPurchases = data || [];
            })
    }

    private getSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliers(true, true)
            .then(data => {
                this.suppliers = data;
                this.suppliers.forEach(r => {
                    r.displayName = `${r.supplierNr} ${r.name}`
                })
            })
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.name",
            "core.saving",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "billing.productrows.productnr",
            "billing.productrows.purchaseprice",
            "billing.products.taxdeductiontype",
            "billing.productrows.pricenotfoundforselectedwholeseller",
            "billing.productrows.quantity",
            "billing.productrows.stockcode",
            "billing.productrows.functions.purchase",
            "billing.purchaserows.purchaseprice.order",
            "billing.purchaserows.purchaseprice",
            "billing.productrows.functions.createpurchase",
            "billing.order.ordernr",
            "billing.purchase.list.purchase"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }
    private setModelTitle() {
        this.modelTitle = this.terms['billing.productrows.functions.createpurchase'] + ' - ' + this.terms['billing.order.ordernr'] + ' ' + this.onGoingInvoiceNr;
    }

    private setupGrid() {
        this.soeGridOptions.addColumnText("productNr", this.terms["billing.productrows.productnr"], null);
        this.soeGridOptions.addColumnText("productName", this.terms["common.name"], null);
        this.soeGridOptions.addColumnNumber("quantity", this.terms["billing.productrows.quantity"], null, { editable: (data) => this.isRowEditable(data) });
        this.soeGridOptions.addColumnNumber("purchasePriceOrder", this.terms["billing.purchaserows.purchaseprice.order"], null, { editable: false });
        this.soeGridOptions.addColumnNumber("purchasePriceCurrency", this.terms["billing.purchaserows.purchaseprice"], null, { editable: (data) => this.isRowEditable(data) });
        this.soeGridOptions.addColumnSelect("stockId", this.terms["billing.productrows.stockcode"], null, {
            selectOptions: [],
            displayField: "stockCode",
            enableHiding: true,
            dropdownIdLabel: "stockId",
            dropdownValueLabel: "name",
            dynamicSelectOptions: {
                idField: "id",
                displayField: "name",
                options: "stocksForProduct",
            },
            editable: (data) => this.isRowEditable(data)
        });

        this.soeGridOptions.addColumnText("purchaseNr", this.terms["billing.productrows.functions.purchase"], null, { buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.purchaseNr, callback: this.openPurchase.bind(this) } });
        //this.soeGridOptions.addColumnTypeAhead("purchaseName", this.terms["billing.productrows.functions.purchase"], 100, { typeAheadOptions, hide: this.createNewPurchase, editable: true })

        const gridEvents: GridEvent[] = [];
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.onCellEdit(entity, colDef) }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef) => { this.afterCellEdit(entity, colDef) }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (rowNode) => { return rowNode.data && !rowNode.data.orgPurchaseNr;}));
        this.soeGridOptions.subscribe(gridEvents);

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });

        this.soeGridOptions.finalizeInitGrid();

        this.soeGridOptions.setData(this.purchaseRows);
        this.$timeout(() => this.soeGridOptions.selectAllRows());
    }

    private onCellEdit(entity: PurchaseRowDTO, colDef) {
        switch (colDef.field) {
            case "stockId":
                this.setStocksForProduct(entity);
        }
    }

    private afterCellEdit(entity: PurchaseRowDTO, colDef) {
        if (this.success) return //prevent editing after success
        switch (colDef.field) {
            case "quantity":
                this.quantityChanged(entity);
                break;
            case "purchasePriceCurrency":
                this.setAmounts(entity);
                break;
        }
    }

    private hasOrgPurchaseNr(row: PurchaseRowDTO):boolean {
        return (row["orgPurchaseNr"]);
    }

    private isRowEditable(row: PurchaseRowDTO) {
        return !this.hasOrgPurchaseNr(row);
    }

    private quantityChanged(row: PurchaseRowDTO) {
        this.calculatePrice(row).then(() => {
            this.setAmounts(row);
        });
    }

    private setPurchaseOnRows(purchase) {
        if (!purchase) return

        this.purchaseRows.forEach(r => {
            if (!this.hasOrgPurchaseNr(r) ) {
                r.purchaseNr = purchase.purchaseNr;
                r.purchaseId = purchase.purchaseId;
            };
        });

        this.soeGridOptions.setData(this.purchaseRows);
        this.soeGridOptions.selectAllRows();
    }

    private clearPurchaseNr() {
        this.purchaseRows.forEach(r => {
            if (!this.hasOrgPurchaseNr(r) && r.purchaseNr) {
                r.purchaseNr = "";
            };
        });

        this.soeGridOptions.refreshRows();
    }

    private setAmounts(row: PurchaseRowDTO) {
        if (!row) return
        row.sumAmount = row.purchasePriceCurrency * row.quantity || 0;
        row.vatAmount = row.sumAmount * (row.vatRate * 0.01) || 0;
        this.soeGridOptions.refreshRows(row);
    }

    private setStocksForProduct(row: PurchaseRowDTO) {
        if (row.productId && row.stocksForProduct.length === 0) {

            row.stocksForProduct.push({ id: 0, name: "" });

            this.productService.getStocksByProduct(row.productId).then((stockList: StockDTO[]) => {

                stockList.forEach((stock) => {
                    row.stocksForProduct.push({ id: stock.stockId, name: stock.code + ' ' + stock.saldo });
                });

                this.soeGridOptions.refreshRows(row);
            });
        }
    }

    private setPurchasePrices() {
        if (this.selectedSupplier) {
            this.purchaseRows.forEach(r => {
                if (this.isRowEditable(r)) {
                    this.calculatePrice(r).then(() => {
                        this.setAmounts(r);
                    });
                }
            });
        }
    }

    private calculatePrice(row: PurchaseRowDTO): ng.IPromise<any> {
        return this.supplierProductService.getSupplierProductPriceByProduct(row.productId, this.selectedSupplier.actorSupplierId, this.purchaseDate, row.quantity, this.currencyId).then((priceDTO: ISupplierProductPriceDTO) => {
            if (priceDTO) {
                row.supplierProductId = priceDTO.supplierProductId;
                row.purchasePriceCurrency = priceDTO.price;
            }
            else {
                row.supplierProductId = undefined;
                row.purchasePriceCurrency = row["purchasePriceOrder"];
            }
        });
    }

    private isValid(): boolean {
        let valid = false;
        if (this.createNewPurchase) {
            valid = this.selectedSupplier && this.selectedSupplier.actorSupplierId > 0;
        } else {
            valid = this.selectedPurchase;
        }
        return valid && this.invoiceId > 0;
    }

    private selectSupplier(item) {
        this.existingFilteredPurchases = [];
        _.forEach(this.existingPurchases, (row) => {
            if (item) {
                if (item.actorSupplierId == row.supplierId) {
                    this.existingFilteredPurchases.push(row);
                }
            } else {
                this.existingFilteredPurchases.push(row);
            }
        });
    }

    private save(): ng.IPromise<any> {
        const selectedPurchaseRows: PurchaseRowDTO[] = this.soeGridOptions.getSelectedRows();
        return this.purchaseService.updatePurchaseFromOrder({
            createNewPurchase: this.createNewPurchase,
            purchaseId: this.selectedPurchase?.purchaseId || 0,
            supplierId: this.selectedSupplier?.actorSupplierId || 0,
            orderId: this.invoiceId,
            copyProject: this.copyProject,
            copyInternalAccounts: false,
            copyDeliveryAddress: this.copydeliveryaddress,
            purchaseRows: selectedPurchaseRows
        }).then( result => {
            if (result.success) {
                _.forEach(selectedPurchaseRows, r => {
                    r.purchaseId = result.integerValue;
                    r.purchaseNr = result.stringValue;
                    r["orgPurchaseNr"] = result.stringValue;
                });

                this.success = true;
                this.soeGridOptions.setData(this.purchaseRows);
                this.$scope.$applyAsync();
            }
        })
    }

    private getOrderRowsToUpdate() : any[] {
        let modifiedRows = [];
        this.purchaseRows.forEach((r) => {
            const rowId = r.customerInvoiceRowIds[0];
            if (rowId) {
                modifiedRows.push({
                    customerInvoiceRowId: rowId,
                    purchasePriceCurrency: r.purchasePriceCurrency,
                    stockId: r.stockId,
                    stockCode: r.stockCode,
                    purchaseId: r.purchaseId,
                    purchaseNr: r.purchaseNr,
                });
            }
        });

        return modifiedRows;
    }

    openPurchase(row: PurchaseRowDTO) {
        this.messagingService.publish(
            Constants.EVENT_OPEN_TAB,
            new TabMessage(`${this.terms["billing.purchase.list.purchase"]} ${row.purchaseNr}`,
                row.purchaseId, PurchaseEditController,
                { id: row.purchaseId },
                this.urlHelperService.getGlobalUrl('Billing/Purchase/Purchase/Views/edit.html')
            )
        );
        this.callClose();
    }

    callClose() {
        if (this.success) {
            this.close(this.getOrderRowsToUpdate());
        }
        else {
            this.close(null);
        }
    }

    buttonCancelClick() {
        this.callClose();
    }

    buttonCreateClick() {
        this.progressHandler.startLoadingProgress([
            () => this.save()
        ],
            this.terms["core.saving"]
        ).then(() => {

        });
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close(result);
        }
    }
}