import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGenerateStockPurchaseSuggestionDTO, IPurchaseRowFromStockDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IProductService } from "../../../Shared/Billing/Products/ProductService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { TypeAheadOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ISupplierProductService } from "../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { EditDeliveryAddressController } from "../../../Shared/Billing/Dialogs/EditDeliveryAddress/EditDeliveryAddressController";
import { IPurchaseService } from "../../../Shared/Billing/Purchase/Purchase/PurchaseService";
import { INotificationService } from "../../../Core/Services/NotificationService";


class GeneratePurchaseModelDTO implements IGenerateStockPurchaseSuggestionDTO {
    purchaseGenerationType = 0;
    productNrFrom = "";
    productNrTo = "";
    triggerQuantityPercentage = 0;
    excludeMissingTriggerQuantity = true;
    excludeMissingPurchaseQuantity = true;
    excludePurchaseQuantityZero = false;
    stockPlaceIds = [];
    purchaser = "";
    defaultDeliveryAddress = "";
    triggerQuantityPercent = 0;
}

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private selectionIsOpen: boolean = true;
    //Filter
    private generationModel = new GeneratePurchaseModelDTO();
    private rows: IPurchaseRowFromStockDTO[] = []

    private stockPlaces: any[] = [];
    private selectedStockPlaces: any[] = [];
    private currencies: { currencyId: number, code: string, name: string }[] = [];

    private productUnits: any[];
    private generationTypes: ISmallGenericType[] = [];

    private supplierAlternatives: { [invoiceProductId: number]: ISmallGenericType[] } = {}

    get performButtonEnabled() {
        return this.generationModel.purchaseGenerationType > 0;
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private stockService: IStockService,
        private productService: IProductService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private supplierProductService: ISupplierProductService,
        private purchaseService: IPurchaseService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Soe.Billing.Stock.Purchase", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Billing_Stock_Purchase, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }


    //GRID

    private beginCellEdit(entity: IPurchaseRowFromStockDTO, colDef, newValue, oldValue) {
        switch (colDef.field) {
            case "supplierName":
                this.loadSupplierAlternatives(entity);
                break;
        }
    }

    private afterCellEdit(entity: IPurchaseRowFromStockDTO, colDef, newValue, oldValue) {
        switch (colDef.field) {
            case "quantity":
                this.onGetNewPrice(entity);
                break;
            case "supplierName":
                this.onSupplierChanged(entity);
                break;
            case "deliveryLeadTimeDays":
                this.onLeadTimeDaysChanged(entity, newValue);
                break;
            case "discountPercentage":
                this.onRecalculateSum(entity, true);
                break;
            case "price":
                this.onRecalculateSum(entity, true);
                break;
        }
    }

    private refocus(row, cell) {
        this.$timeout(() => {
            const data = this.gridAg.options.getData();
            if (data) this.gridAg.options.clearFocusedCell();
            this.gridAg.options.startEditingCell(row, cell);
        }, 0);
    }

    private filterSuppliers(filter: string) {
        const currentRow = this.gridAg.options.getCurrentRow() as IPurchaseRowFromStockDTO;
        if (!this.supplierAlternatives[currentRow.productId]) return [];
        return this.supplierAlternatives[currentRow.productId].filter(s => s.name.contains(filter));
    }

    private loadSupplierAlternatives(entity: IPurchaseRowFromStockDTO) {
        if (this.supplierAlternatives[entity.productId]) return;

        this.progress.startLoadingProgress([
            () => this.supplierProductService.getSuppliersByInvoiceProduct(entity.productId).then(data => {
                this.supplierAlternatives[entity.productId] = data;
                this.refocus(entity, "supplierName");
            })
        ])
    }

    private onSupplierChanged(entity: IPurchaseRowFromStockDTO) {
        const supplier = this.supplierAlternatives[entity.productId]?.find(s => s.name === entity.supplierName);
        if (!supplier) {
            entity.supplierId = 0;
            return;
        }

        entity.supplierId = supplier.id;

        this.supplierProductService.getSupplierProductByInvoiceProduct(entity.productId, entity.supplierId).then(data => {
            entity.supplierProductId = data.supplierProductId;
            entity.packSize = data.packSize;
            entity.supplierUnitId = data.supplierProductUnitId;

            if (data.deliveryLeadTimeDays) {
                entity.deliveryLeadTimeDays = data.deliveryLeadTimeDays;
                this.onLeadTimeDaysChanged(entity, entity.deliveryLeadTimeDays, false)
            }

            this.onGetNewPrice(entity);
            this.refreshRows(entity);
        })
    }

    private onGetNewPrice(entity: IPurchaseRowFromStockDTO) {
        this.supplierProductService.getSupplierProductPrice(entity.supplierProductId, new Date(), entity.quantity, 0).then(price => {
            entity.price = price.price;
            entity.currencyId = price.currencyId;

            this.onRecalculateSum(entity, false);
            this.refreshRows(entity);
        })
    }

    private onRecalculateSum(entity: IPurchaseRowFromStockDTO, refresh = true) {
        const discount = ((100 - entity.discountPercentage) / 100);
        entity.sum = entity.quantity * entity.price * discount;
        if (refresh) this.refreshRows(entity);
    }

    private onLeadTimeDaysChanged(entity: IPurchaseRowFromStockDTO, newValue: number, refresh = true) {
        if (!newValue) newValue = 0;
        entity.requestedDeliveryDate = new Date().addDays(newValue);
        if (refresh) this.refreshRows(entity);
    }

    private refreshRows(...rows: IPurchaseRowFromStockDTO[]) {
        this.gridAg.options.refreshRows(...rows);
    }

    public setupGrid() {
        const gridEvents: GridEvent[] = [];
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef, newValue, oldValue) => { this.beginCellEdit(entity, colDef, newValue, oldValue); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (val) => null));
        this.gridAg.options.subscribe(gridEvents);


        const keys: string[] = [
            "common.code",
            "common.name",
            "common.quantity",
            "common.price",
            "common.sum",
            "common.currency",
            "common.report.selection.purchasenr",
            "common.customer.invoices.articlename",
            "core.edit",
            "common.productnr",
            "billing.stock.stocksaldo.productnumber",
            "billing.stock.stocksaldo.purchasetriggerquantity",
            "billing.stock.stocksaldo.purchasequantity",
            "billing.stock.stocksaldo.purchasedquantity",
            "billing.stock.stocksaldo.saldo",
            "billing.stock.stocksaldo.ordered",
            "billing.stock.stocksaldo.reserved",
            "billing.stock.stocks.stock",
            "billing.stock.purchase.availablequantity",
            "billing.stock.purchase.purchasequantity",
            "billing.stock.purchase.unitsupplier",
            "billing.purchase.supplier",
            "billing.purchase.deliveryaddress",
            "billing.purchaserows.purchaseunit",
            "billing.purchaserows.wanteddeliverydate",
            "billing.stock.stocksaldo.leadtime",
            "common.customer.invoices.articlename",
            "billing.stock.purchase.separatepurchase",
            "billing.productrows.dialogs.discountpercent",
            "billing.purchase.list.purchase",
            "billing.purchase.supplier",
            "billing.product.stock",
            "billing.products.product",
        ];


        this.translationService.translateMany(keys).then((terms) => {
            const headerColumnOptions = { enableHiding: true };


            const productHeader = this.gridAg.options.addColumnHeader("productGroup", terms["billing.products.product"], headerColumnOptions);
            this.gridAg.addColumnText("productNr", terms["billing.stock.stocksaldo.productnumber"], null, null, null, productHeader);
            this.gridAg.addColumnText("productName", terms["common.customer.invoices.articlename"], null, null, null, productHeader);
            this.gridAg.addColumnSelect("unitId", terms["billing.purchaserows.purchaseunit"], null, {
                selectOptions: this.productUnits,
                enableHiding: true,
                editable: false,
                displayField: "unitCode",
                dropdownIdLabel: "value",
                dropdownValueLabel: "label",
            }, productHeader);

            const stockHeader = this.gridAg.options.addColumnHeader("stockGroup", terms["billing.product.stock"], headerColumnOptions);
            this.gridAg.addColumnText("stockName", terms["billing.stock.stocks.stock"], null, null, null, stockHeader);
            this.gridAg.addColumnNumber("stockPurchaseTriggerQuantity", terms["billing.stock.stocksaldo.purchasetriggerquantity"], null, null, stockHeader);
            this.gridAg.addColumnNumber("stockPurchaseQuantity", terms["billing.stock.stocksaldo.purchasequantity"], null, null, stockHeader);
            this.gridAg.addColumnNumber("totalStockQuantity", terms["billing.stock.stocksaldo.saldo"], null, null, stockHeader);
            this.gridAg.addColumnNumber("availableStockQuantity", terms["billing.stock.purchase.availablequantity"], null, null, stockHeader);
            this.gridAg.addColumnNumber("purchasedQuantity", terms["billing.stock.stocksaldo.purchasedquantity"], null, null, stockHeader);

            const supplierHeader = this.gridAg.options.addColumnHeader("supplierGroup", terms["billing.purchase.supplier"], headerColumnOptions);

            const supplierCellClassRules = {
                "infoRow": ({ data }) => data.supplierId && data.multipleSupplierMatches,
                "errorRow": ({ data }) => !data.supplierId
            }

            const supplierOptions = {
                ...new TypeAheadOptionsAg(),
                source: (filter) => this.filterSuppliers(filter),
                minLength: 0,
                delay: 0,
                displayField: "name",
                dataField: "name",
                useScroll: true,
            }

            this.gridAg.addColumnTypeAhead("supplierName", terms["billing.purchase.supplier"], null,
                {
                    typeAheadOptions: supplierOptions,
                    editable: (data) => data.multipleSupplierMatches,
                    cellClassRules: supplierCellClassRules
                },
                null,
                supplierHeader
            )

            this.gridAg.addColumnSelect("supplierUnitId", terms["billing.stock.purchase.unitsupplier"], null, {
                selectOptions: this.productUnits,
                enableHiding: true,
                hide: true,
                editable: false,
                displayField: "supplierUnitCode",
                dropdownIdLabel: "value",
                dropdownValueLabel: "label",
            }, supplierHeader);

            const purchaseHeader = this.gridAg.options.addColumnHeader("purchaseGroup", terms["billing.purchase.list.purchase"], headerColumnOptions);

            this.gridAg.addColumnNumber("deliveryLeadTimeDays", terms["billing.stock.stocksaldo.leadtime"], null, {
                editable: true,
                decimals: 0,
                maxDecimals: 0
            }, purchaseHeader);

            this.gridAg.addColumnNumber("quantity", terms["billing.stock.purchase.purchasequantity"], null, {
                editable: true
            }, purchaseHeader);
            this.gridAg.addColumnNumber("price", terms["common.price"], null, {
                editable: true
            }, purchaseHeader);

            this.gridAg.addColumnNumber("discountPercentage", terms["billing.productrows.dialogs.discountpercent"], null, {
                editable: true,
                maxDecimals: 2,
                clearZero: true
            }, purchaseHeader);

            this.gridAg.addColumnNumber("sum", terms["common.sum"], null, {
                decimals: 2
            }, purchaseHeader);

            this.gridAg.addColumnSelect("currencyId", terms["common.currency"], null, {
                selectOptions: this.currencies,
                enableHiding: true,
                editable: false,
                displayField: "currencyCode",
                dropdownIdLabel: "currencyId",
                dropdownValueLabel: "code",
            }, purchaseHeader);

            this.gridAg.addColumnDate("requestedDeliveryDate", terms["billing.purchaserows.wanteddeliverydate"], null, null, null, {
                editable: true
            }, purchaseHeader);

            this.gridAg.addColumnText("deliveryAddress", terms["billing.purchase.deliveryaddress"], null, null, {
                buttonConfiguration:
                {
                    iconClass: "fal fa-pen", callback: (entity: IPurchaseRowFromStockDTO) => { this.editDeliveryAddresForRow(entity) }, show: () => true
                }
            }, purchaseHeader);

            this.gridAg.addColumnBoolEx("exclusivePurchase", terms["billing.stock.purchase.separatepurchase"], null, { enableEdit: true }, purchaseHeader);

            this.gridAg.addColumnText("purchaseNr", terms["common.report.selection.purchasenr"], null, null, null, purchaseHeader);

            this.gridAg.finalizeInitGrid("billing.stock.stocks.stocks", true);

            this.messagingHandler.publishResizeWindow();
        });
    }


    //LOOKUPS

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadStockPlaces(),
            this.loadProductUnits(),
            this.loadCalculationTypes(),
            this.loadCurrencies(),
        ]);
    }

    private loadStockPlaces(): ng.IPromise<any> {
        return this.stockService.getStocks(false).then(data => {
            this.stockPlaces = data;
        })
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then(data => {
            this.productUnits = [];
            data?.forEach((unit) => {
                this.productUnits.push({ value: unit.productUnitId, label: unit.code });
            });
        });
    }

    private loadCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.StockPurchaseGenerationOptions, false, true, true).then(data => {
            this.generationTypes = data;
        })
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(data => {
            this.currencies = data;
        })
    }

    //ACTIONS
    private editDefaultDeliveryAddress() {
        const onEdit = (newAddress: string) => {
            this.generationModel.defaultDeliveryAddress = newAddress
        }

        this.editDeliveryAddress(this.generationModel.defaultDeliveryAddress, onEdit);
    }

    private editDeliveryAddresForRow(entity: IPurchaseRowFromStockDTO) {
        const onEdit = (newAddress: string) => {
            entity.deliveryAddress = newAddress;
            this.gridAg.options.refreshRows(entity);
        }

        this.editDeliveryAddress(entity.deliveryAddress, onEdit);
    }

    private editDeliveryAddress(current: string, setValue: (newAddress: string) => void) {
        var temp: string = current;

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/EditDeliveryAddress/EditDeliveryAddress.html"),
            controller: EditDeliveryAddressController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                translationService: () => this.translationService,
                coreService: () => this.coreService,
                deliveryAddress: () => temp,
                isFinvoiceCustomer: () => false,
                isLocked: () => false,
            }
        });

        modal.result.then((result: any) => {
            if (result && result.deliveryAddress != null && result.deliveryAddress !== temp) {
                setValue(result.deliveryAddress)
            }
        });
    }

    private purchaseGeneration() {
        const rows = this.gridAg.options.getSelectedRows() as IPurchaseRowFromStockDTO[];

        if (rows.length === 0) {
            this.translationService.translateMany(["billing.stock.purchase.noselectedrows", "core.warning"]).then(terms => {
                this.notificationService.showDialog(
                    terms["core.warning"],
                    terms["billing.stock.purchase.noselectedrows"],
                    SOEMessageBoxImage.Error,
                    SOEMessageBoxButtons.OK);
            })
            return;
        }
        else if (rows.some(r => !r.supplierId)) {
            this.translationService.translateMany(["billing.stock.purchase.missingsupplier", "core.warning"]).then(terms => {
                this.notificationService.showDialog(
                    terms["core.warning"],
                    terms["billing.stock.purchase.missingsupplier"],
                    SOEMessageBoxImage.Error,
                    SOEMessageBoxButtons.OK);
            })
           return;
        }
        else {
            this.translationService.translateMany(["billing.stock.purchase.selectedrows", "core.info"]).then(terms => {
                const modalDialog = this.notificationService.showDialog(
                    terms["core.info"],
                    terms["billing.stock.purchase.selectedrows"].replace("{0}", rows.length.toString()),
                    SOEMessageBoxImage.Information,
                    SOEMessageBoxButtons.YesNo);
                modalDialog.result.then(val => {
                    if (val != null && val === true) {
                        this.performPurchaseGeneration(rows);
                    };
                });
            })
        }
    }

    private performPurchaseGeneration(rows: IPurchaseRowFromStockDTO[]) {
        if (this.generationModel.purchaser) {
            rows.forEach(r => {
                r.referenceOur = this.generationModel.purchaser;
            })
        }
        this.progress.startLoadingProgress([
            () => this.purchaseService.createPurchaseFromStockSuggestion(rows).then(result => {
                const purchaseIds = new Set<number>();
                rows.forEach(r => {
                    if (result[r.tempId]) {
                        r.purchaseId = result[r.tempId].id;
                        r.purchaseNr = result[r.tempId].name;
                        purchaseIds.add(result[r.tempId].id);
                    }
                })

                this.refreshRows(...rows);
                this.translationService.translateMany(["billing.stock.purchase.createdpurchases", "core.info"]).then(terms => {
                    this.notificationService.showDialog(
                        terms["core.info"],
                        terms["billing.stock.purchase.createdpurchases"].replace("{0}", purchaseIds.size.toString()),
                        SOEMessageBoxImage.OK,
                        SOEMessageBoxButtons.OK);
                })
            })
        ])

    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            this.generationModel.stockPlaceIds = this.selectedStockPlaces.map(s => s.stockId);
            this.generationModel.triggerQuantityPercent = Number(this.generationModel.triggerQuantityPercent)
            return this.stockService.generatePurchaseSuggestion(this.generationModel).then((x) => {
                x.forEach(r => this.onRecalculateSum(r, false));
                this.rows = x;
                this.setGridData();

                this.$timeout(() => {
                    this.messagingHandler.publishResizeWindow();
                }, 0)
            });
        }]);
    }

    private setGridData(val?: boolean) {
        this.$timeout(() => {
            let rows = this.rows;
            if (this.generationModel.excludePurchaseQuantityZero) {
                rows = rows.filter(r => r.quantity != 0);
            }

            this.setData(rows);
        }, 0)
    }
}