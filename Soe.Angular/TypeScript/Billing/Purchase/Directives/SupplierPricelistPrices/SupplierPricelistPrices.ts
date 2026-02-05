import { SupplierProductPriceComparisonDTO, SupplierProductPriceDTO } from "../../../../Common/Models/SupplierProductDTO";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISupplierProductGridDTO, ISupplierProductPriceComparisonDTO, ISupplierProductPriceSearchDTO } from "../../../../Scripts/TypeLite.Net4";
import { ISupplierProductService } from "../../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature, SoeEntityState } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { StandardRowFunctions, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { TypeAheadOptionsAg } from "../../../../Util/SoeGridOptionsAg";

interface PriceCompareRow {
    supplierProductId: number;
    comparePrice: number;
    compareQuantity: number;
    compareStartDate: Date;
    compareEndDate: Date;
}

class SupplierPricelistPricesController extends GridControllerBase2Ag implements ICompositionGridController {
    private supplierProductId: number;
    private parentGuid: any;
    private startDate: Date;
    private currencyId: number;
    private endDate: Date;
    private rows: SupplierProductPriceComparisonDTO[] = [];
    private comparisonPrices: { [productIdQuantity : string]: PriceCompareRow } = {};
    private supplierId: number;
    private products: ISupplierProductGridDTO[] = [];

    //gui
    private rowFunctions: any = [];

    //Terms
    terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private supplierProductService: ISupplierProductService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private messagingService: IMessagingService) {
        super(gridHandlerFactory, "billing.purchase.pricelist.prices", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.loadSupplierProducts())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.$scope.$on('refreshRows', (e, a) => {
            if (this.rows)
                this.gridAg.options.refreshRows();
        });

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.doubleClickToEdit = false;
        if (!this.products && this.supplierId) {
            this.loadSupplierProducts();
        }

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private loadSupplierProducts(): ng.IPromise<any> {
        this.products = [];
        const deferral = this.$q.defer();
        if (this.supplierId) {
            this.supplierProductService.getSupplierProducts({
                supplierIds: [this.supplierId],
                product: "",
                productName: "",
                supplierProduct: "",
                supplierProductName: "",
                invoiceProductId: 0,
            }).then(data => {
                deferral.resolve();
                this.products = data;
            });
        }
        else {
            deferral.resolve();
        }
        return deferral.promise;
    }

    private filterProducts(filter) {
        return this.products.filter(prod => {
            const name = prod.supplierProductName ? prod.supplierProductName.contains(filter) : false;
            const nr = prod.supplierProductNr ? prod.supplierProductNr.contains(filter) : false;
            return name || nr;
        });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Purchase_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
    }

    private setupGrid() {
        this.gridAg.options.setMinRowsToShow(15);

        const keys: string[] = [
            "billing.purchase.product.price",
            "billing.purchase.product.pricestartdate",
            "billing.purchase.product.priceqty",
            "billing.purchase.product.supplieritemno",
            "billing.purchase.product.supplieritemname",
            "billing.purchase.product.purchaseprice",
            "billing.product.number",
            "billing.purchaserows.productnr",
            "billing.purchase.product.actualpurchaseprices",
            "billing.purchase.product.newpurchaseprices",
            "billing.purchase.product.product",
            "billing.purchase.product.priceenddate",
            "common.newrow",
            "core.deleterow",
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.setupFunctions(terms);

            const headerColumnOptions = { enableHiding: true };
            const productHeader = this.gridAg.options.addColumnHeader("productGroup", terms["billing.purchase.product.product"], headerColumnOptions);
            const comparisonHeader = this.gridAg.options.addColumnHeader("comparisonGroup", terms["billing.purchase.product.actualpurchaseprices"], headerColumnOptions);
            const currentHeader = this.gridAg.options.addColumnHeader("currentGroup", terms["billing.purchase.product.newpurchaseprices"], headerColumnOptions);

            this.gridAg.addColumnIsModified("isModified");

            const options = {
                ...new TypeAheadOptionsAg(),
                source: (filter) => this.filterProducts(filter),
                minLength: 0,
                delay: 0,
                displayField: "supplierProductNr",
                dataField: "supplierProductNr",
                useScroll: true,
            };
            this.gridAg.addColumnTypeAhead("productNr", terms["billing.purchase.product.supplieritemno"], 100, { error: 'productError', typeAheadOptions: options, editable: true, suppressMovable: true }, null, productHeader);
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-pencil iconEdit", onClick: this.openPurchaseProduct.bind(this), showIcon: (row) => row.productNr, suppressMovable: true, enableHiding: false, suppressFilter: true }, productHeader)
            this.gridAg.addColumnText("productName", terms["billing.purchase.product.supplieritemname"], 60, true, { editable: false, suppressMovable: true }, productHeader);
            this.gridAg.addColumnText("ourProductName", terms["billing.product.number"], 60, true, { editable: false, suppressMovable: true }, productHeader);

            this.gridAg.addColumnNumber("compareQuantity", terms["billing.purchase.product.priceqty"], 60, { enableHiding: true, decimals: 2, editable: false, maxDecimals: 4, suppressMovable: true }, comparisonHeader);
            this.gridAg.addColumnNumber("comparePrice", terms["billing.purchase.product.purchaseprice"], 60, { enableHiding: true, decimals: 2, editable: false, maxDecimals: 4, suppressMovable: true }, comparisonHeader);
            this.gridAg.options.addColumnDate("compareStartDate", terms["billing.purchase.product.pricestartdate"], null, true, comparisonHeader, null, { enableHiding: true, editable: false, suppressMovable: true });
            this.gridAg.options.addColumnDate("compareEndDate", terms["billing.purchase.product.priceenddate"], null, true, comparisonHeader, null, { enableHiding: true, editable: false, suppressMovable: true });

            this.gridAg.addColumnNumber("quantity", terms["billing.purchase.product.priceqty"], 60, { enableHiding: true, decimals: 2, editable: true, maxDecimals: 4, suppressMovable: true }, currentHeader);
            this.gridAg.addColumnNumber("price", terms["billing.purchase.product.purchaseprice"], 60, { enableHiding: false, decimals: 2, editable: true, maxDecimals: 4, suppressMovable: true }, currentHeader);

            const gridEvents: GridEvent[] = [];
            gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            this.gridAg.options.subscribe(gridEvents);

            this.gridAg.finalizeInitGrid("billing.purchase.pricelist.prices", true);
        });
    }

    private setupFunctions(terms: { [index: string]: string }) {
        this.rowFunctions.push({ id: StandardRowFunctions.Add, name: terms["common.newrow"], icon: "fal fa-plus" });
        this.rowFunctions.push({ id: StandardRowFunctions.Delete, name: terms["core.deleterow"], icon: "fal fa-times iconDelete" });
    }

    private afterSetup() {
        if (!this.rows)
            this.rows = [];

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.rows, (newVal, oldVal) => {
            let updatePriceDict = false;
            if (newVal?.length > 0 && oldVal?.length == 0) {
                updatePriceDict = true;
            } else {
                newVal?.forEach(r => this.setComparisonValues(r));
            }
            this.rowsUpdated(updatePriceDict);
        });

        this.$scope.$watch(() => this.supplierId, (newVal, oldVal) => {
            if (newVal && newVal !== oldVal) {
                this.loadSupplierProducts();
            }
        });
    }

    private rowsUpdated(updatePriceDict = false, updateIsModified = false) {
        if (this.rows) {

            if (updateIsModified) {
                this.rows.forEach(r => r.isModified = true);
            }

            this.gridAg.setData(this.rows.filter(r => r.state === SoeEntityState.Active));

            if (updatePriceDict) this.createPriceComparisonDict(this.rows);
        }
    }

    private productChanged(row: SupplierProductPriceComparisonDTO) {
        const product = this.products.find(p => p.supplierProductNr === row.productNr);
        if (product) {
            row.supplierProductId = product.supplierProductId
            row.productNr = product.supplierProductNr
            row.productName = product.supplierProductName
            row.ourProductName = product.productName
        }
    }

    private afterCellEdit(row: SupplierProductPriceComparisonDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'productNr':
                this.productChanged(row);
                this.setComparisonValues(row);
                break;
            case 'quantity':
                row.quantity = NumberUtility.parseNumericDecimal(row.quantity);
                this.setComparisonValues(row);
                break;
            case 'price':
                row.price = NumberUtility.parseNumericDecimal(row.price);
                break;
        }

        this.setRowAsModified(row);
    }

    private executeRowFunction(option) {
        switch (option.id) {
            case StandardRowFunctions.Add:
                this.addRow();
                break;
            case StandardRowFunctions.Delete:
                this.deleteRows();
                break;
        }
    }

    private getComparisonValues() {
        const searchModel: ISupplierProductPriceSearchDTO = {
            compareDate: this.startDate,
            supplierId: this.supplierId,
            currencyId: this.currencyId,
            includePricelessProducts: false,
        }
        return this.supplierProductService.getSupplierPricelistCompare(searchModel).then(data => {
            this.createPriceComparisonDict(data);
            this.rows.forEach(r => this.setComparisonValues(r));
            this.rowsUpdated();
        })
    }

    private getAllProducts(): ng.IPromise<any> {
        const searchModel: ISupplierProductPriceSearchDTO = {
            compareDate: this.startDate,
            supplierId: this.supplierId,
            currencyId: this.currencyId,
            includePricelessProducts: true,
        }
        return this.progress.startLoadingProgress([() => {
            return this.supplierProductService.getSupplierPricelistCompare(searchModel).then((data: SupplierProductPriceComparisonDTO[]) => {
                this.rows = data.map(r => {
                    this.fixDates(r);
                    return r;
                });
                this.rowsUpdated(false, true);
                if (data.length > 0) {
                    this.gridAg.options.startEditingCell(this.rows[0], "price");
                    this.setParentAsModified();
                }
            });
        }]);
    }

    private createPriceComparisonDict(comparisonRows: ISupplierProductPriceComparisonDTO[]) {
        this.comparisonPrices = {};
        comparisonRows.forEach(r => {
            this.comparisonPrices[`${r.supplierProductId},${r.compareQuantity}`] = {
                supplierProductId: r.supplierProductPriceId,
                comparePrice: r.comparePrice,
                compareStartDate: r.compareStartDate,
                compareEndDate: r.compareEndDate,
                compareQuantity: r.compareQuantity,
            }
        })
    }

    private fixDates(row: SupplierProductPriceComparisonDTO): void {
        const startDate = new Date("1901-01-02");
        const stopDate = new Date("9998-12-31");
        const rowStart = CalendarUtility.convertToDate(row.startDate);
        const rowEnd = CalendarUtility.convertToDate(row.endDate);
        const rowStartCompare = CalendarUtility.convertToDate(row.compareStartDate);
        const rowEndCompare = CalendarUtility.convertToDate(row.compareEndDate);

        if (rowStart < startDate)
            row.startDate = null;
        if (rowEnd > stopDate || rowEnd < startDate)
            row.endDate = null;
        if (rowStartCompare <= startDate)
            row.compareStartDate = null;
        if (rowEndCompare >= stopDate)
            row.compareEndDate = null;
    }

    private setComparisonValues(row: SupplierProductPriceComparisonDTO) {
        const compVals = this.comparisonPrices[`${row.supplierProductId},${row.quantity}`];
        this.fixDates(row);
        if (compVals) {
            row.comparePrice = compVals.comparePrice;
            row.compareQuantity = compVals.compareQuantity;
            row.compareStartDate = compVals.compareStartDate;
            row.compareEndDate = compVals.compareEndDate;
        } else {
            row.comparePrice = 0;
            row.compareQuantity = 0;
            row.compareStartDate = null;
        }
    }

    private deleteRows() {
        const keys = ["core.warning", "core.deleterowwarning"];
        this.translationService.translateMany(keys).then(terms => {
            const modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deleterowwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    const selectedRows = this.gridAg.options.getSelectedRows();
                    if (selectedRows && selectedRows.length > 0) {
                        selectedRows.forEach((row: SupplierProductPriceDTO) => {
                            if (row.supplierProductPriceId) {
                                row.state = SoeEntityState.Deleted;
                                row.isModified = true;
                            }
                            else {
                                this.rows = this.rows.filter(r => r !== row);
                            }
                        });

                        this.rowsUpdated();
                        this.setParentAsModified();
                    }
                }
            });
        });
    }

    private addRow() {
        if (!this.rows) {
            this.rows = [];
        }

        let row = new SupplierProductPriceComparisonDTO();
        row.quantity = 0;
        row.price = 0;
        row.isModified = true;
        row.state = SoeEntityState.Active;

        this.rows.push(row);

        this.rowsUpdated();

        this.gridAg.options.startEditingCell(row, "productNr");

        this.setRowAsModified(row, true);
    }

    public setRowAsModified(row: any, notify = true) {
        if (row) {
            row.isModified = true;
            if (notify)
                this.setParentAsModified();
            this.gridAg.options.refreshRows(row);
        }
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingHandler.publishSetDirty( this.parentGuid ));
    }

    private openPurchaseProduct(row: SupplierProductPriceComparisonDTO) {
        this.messagingService.publish(Constants.EVENT_OPEN_PURCHASE_PRODUCT, {
            id: row.supplierProductId, name: this.terms["billing.purchase.product.product"] + " " + row.productNr
        });
    }
}

export class SupplierPicelistPricesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Billing/Purchase/Directives/SupplierPricelistPrices/SupplierPricelistPrices.html"),
            scope: {
                pricelistId: "=",
                parentGuid: "=",
                startDate: "=",
                currencyId: "=",
                endDate: "=",
                supplierId: "=",
                rows: "=",
            },
            restrict: 'E',
            replace: true,
            controller: SupplierPricelistPricesController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}