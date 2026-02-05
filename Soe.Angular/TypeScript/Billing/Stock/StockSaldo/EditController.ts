import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProductSmallDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { StockTransactionDTO } from "../../../Common/Models/StockTransactionDTO";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Feature, TermGroup, CompanySettingType, TermGroup_StockTransactionType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ProductUnitConvertDTO } from "../../../Common/Models/ProductUnitConvertDTO";
import { ProductService } from "../../../Shared/Billing/Products/ProductService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { StockProductDTO } from "../../../Common/Models/StockProductDTO";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { IFocusService } from "../../../Core/Services/focusservice";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    transactionsGrid: EmbeddedGridController;

    // Data
    actionTypes: ISmallGenericType[];
    stockProductId: number;
    isReadOnly: boolean;
    stockTransaction: StockTransactionDTO;
    hasPriceChangePermission = true;

    stockProduct: StockProductDTO;
    stockProducts: StockProductDTO[] = [];
    stockProductsForTypeahead: ISmallGenericType[] = [];
    targetStockProducts: ISmallGenericType[] = [];

    productUnitConverts: ProductUnitConvertDTO[] = [];
    useProductUnitConvert = false;
    showVourcherColumn = false;
    selectedProductUnitName: string;

    //new 
    products: ISmallGenericType[] = [];

    _selectedUnitConvert: ProductUnitConvertDTO;
    get selectedUnitConvert(): ProductUnitConvertDTO {
        return this._selectedUnitConvert;
    }
    set selectedUnitConvert(item: ProductUnitConvertDTO) {
        this._selectedUnitConvert = item;
        if (this._selectedUnitConvert) {
            this.selectedProductUnitName = this._selectedUnitConvert.productUnitName;
            this.stockTransaction.productUnitConvertId = this._selectedUnitConvert.productUnitConvertId;
        }
        else {
            this.selectedProductUnitName = "";
            this.stockTransaction.productUnitConvertId = undefined;
        }
    }

    private _selectedTargetStockProduct: ISmallGenericType;
    get selectedTargetStockProduct(): ISmallGenericType {
        return this._selectedTargetStockProduct;
    }
    set selectedTargetStockProduct(item: ISmallGenericType) {
        this._selectedTargetStockProduct = item;
        if (item?.id) {
            const targetStockProduct = this.stockProducts.find(x => x.stockProductId === item.id);
            this.stockTransaction.targetStockId = targetStockProduct.stockId;
            this.stockTransaction.productId = targetStockProduct.invoiceProductId;
            
            this.dirtyHandler.setDirty();
        }
        else {
            this.stockTransaction.targetStockId = undefined;
        }
    }

    private _selectedStockProduct: ISmallGenericType;
    get selectedStockProduct(): ISmallGenericType {
        return this._selectedStockProduct;
    }
    set selectedStockProduct(item: ISmallGenericType) {
        this._selectedStockProduct = item;
        if (item?.id) {
            this.stockProduct = this.stockProducts.find(x => x.stockProductId === item.id);
            this.stockTransaction.price = this.stockProduct.avgPrice;
        }

        this.filtertargetStockProducts();
    }

    private _selectedProduct: ISmallGenericType;
    get selectedProduct() {
        return this._selectedProduct;
    }
    set selectedProduct(item: ISmallGenericType) {
        this._selectedProduct = item;
        if (this._selectedProduct?.id) {
            this.stockTransaction.productId = item.id;
            this.loadProductStocks(item.id);
            this.selectedStockProduct = undefined;
        }
        else {
            this.stockTransaction.productId = undefined;
        }

        this.dirtyHandler.setDirty();
    }

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private stockService: IStockService,
        private productService: ProductService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private focusService: IFocusService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        $scope: ng.IScope,
        shortCutService: IShortCutService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private gridHandlerFactory: IGridHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookUp())
            .onAfterFirstLoad(() => this.setFirstFocusField())

        this.transactionsGrid = new EmbeddedGridController(this.gridHandlerFactory, "stockinventoryGrid");
        this.transactionsGrid.gridAg.options.setMinRowsToShow(8);

        shortCutService.bindSave($scope, () => { this.save(); });
    }
    
    public onInit(parameters: any) {
        this.stockProductId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([
            { feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Stock_Change_AvgPrice, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Stock].readPermission;
        this.modifyPermission = response[Feature.Billing_Stock].modifyPermission;
        this.hasPriceChangePermission = response[Feature.Billing_Stock_Change_AvgPrice].modifyPermission;
    }

    private setFirstFocusField() {
        if (this.isNew) {
            this.focusService.focusByName("ctrl_selectedProduct");
        }
        else {
            this.focusService.focusByName("ctrl_stockTransaction_actionType");
        }
    }

    private onDoLookUp() {
        return this.$q.all([
            this.loadActionTypes(),
            this.loadCompanySettings()
        ]).then(_ => {
            this.onSetupGui();
        });
    }

    private onSetupGui() {
        this.createTransactionsGrid();
    }

    private onLoadData() {
        if (this.stockProductId > 0) {
            return this.loadStockProduct().then(
                () => {
                    this.loadproductUnitConverts();
                    this.loadTransactions();
                }
            );
        }
        else {
            this.new();
        }
    }

    private actionTypeOnChanging(actionType: number) {
        if (actionType !== 2) {
            this.selectedUnitConvert = undefined;
        }

        if (actionType === TermGroup_StockTransactionType.StockTransfer && this.targetStockProducts.length === 0) {
            this.loadProductStocks(this.stockProduct.invoiceProductId);
        }
        else if (actionType !== TermGroup_StockTransactionType.StockTransfer) {
            this.selectedTargetStockProduct = undefined;
        }
    }

    public isQuantityRequired(): boolean {
        return (!this.stockTransaction || this.stockTransaction.actionType != TermGroup_StockTransactionType.AveragePriceChange);
    }

    // LOOKUPS
    private loadActionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.StockTransactionType, false, false).then((data: ISmallGenericType[]) => {
            this.actionTypes = data.filter(x => x.id < 3 || x.id > 4); 
            if (!this.hasPriceChangePermission) {
                this.actionTypes = this.actionTypes.filter(x => x.id != TermGroup_StockTransactionType.AveragePriceChange); 
            }
        });
    }

    private loadproductUnitConverts(): ng.IPromise<any> {
        return this.productService.getProductUnitConverts(this.stockProduct.invoiceProductId, true).then((x) => {
            this.productUnitConverts = x;
        });
    }

    private loadProductStocks(productId: number) {
        return this.stockService.getStockProductsByProduct(productId).then((data: StockProductDTO[]) => {
            this.stockProducts = data;
            this.setProductStockTypeahead();
            this.filtertargetStockProducts();
        });
    }

    private setProductStockTypeahead() {
        this.stockProductsForTypeahead = [];
        this.stockProducts.forEach(x => this.stockProductsForTypeahead.push({ id: x.stockProductId, name: x.stockName }));
        if (this.stockProductsForTypeahead.length == 1) {
            this.selectedStockProduct = this.stockProductsForTypeahead[0];
        }
    }

    private filtertargetStockProducts() {
        this.selectedTargetStockProduct = undefined;

        if (this.selectedStockProduct) {
            this.targetStockProducts = this.stockProductsForTypeahead.filter(x => x.id !== this.selectedStockProduct.id);
        }
    }

    private loadStockProduct(): ng.IPromise<any> {
        if (this.stockProductId > 0) {
            return this.stockService.getStockProduct(this.stockProductId).then((x:StockProductDTO) => {
                this.isNew = false;
                this.stockProduct = x;

                this.stockTransaction = this.createNewStockTransactionDTO();
                this.stockTransaction.price = this.stockProduct.avgPrice;

                this._selectedStockProduct = { id: this.stockProduct.stockProductId, name: this.stockProduct.stockName};

                this.stockProducts.push(this.stockProduct);
                this.setProductStockTypeahead();

                
                const product = { id: x.invoiceProductId, name: x.productNumber + " " + x.productName };
                this.products.push(product);
                this._selectedProduct = product;
            });
        }
    }

    private loadTransactions(): ng.IPromise<any> {
        return this.stockService.getStockProductTransactions(this.stockProductId).then((x:any[]) => {
            this.transactionsGrid.gridAg.setData(x);
            //To adjust for possible horizontal scrollbar
            this.transactionsGrid.gridAg.options.sizeColumnToFit();
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.BillingUseProductUnitConvert, CompanySettingType.AccountingCreateVouchersForStockTransactions];
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useProductUnitConvert = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseProductUnitConvert, this.useProductUnitConvert);
            this.showVourcherColumn = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingCreateVouchersForStockTransactions, this.showVourcherColumn);
        });
    }

    public save() {
        if (!this.stockTransaction.quantity && this.isQuantityRequired()) {
            this.translationService.translate("billing.stock.stocksaldo.actionquantity").then(term => {
                this.notificationService.showDialog("", term, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
            return;
        }

        this.stockTransaction.stockProductId = this.selectedStockProduct.id;
        this.stockTransaction.productName = this.selectedStockProduct.name;
        this.stockTransaction.invoiceRowId = null;

        this.progress.startSaveProgress((completion) => {
            const list: StockTransactionDTO[] = [this.stockTransaction];

            this.stockService.saveStockTransactions(list).then((result) => {
                if (result.success) {
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.stockTransaction);

                    this.stockTransaction = this.createNewStockTransactionDTO();

                    if (this.isNew) {
                        this.selectedProduct = undefined;
                        this.selectedStockProduct = undefined;

                        this.setFirstFocusField();
                    }

                    if (this.selectedUnitConvert) {
                        this.stockTransaction.productUnitConvertId = this.selectedUnitConvert.productUnitConvertId;
                    }

                    this.loadTransactions();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
            }, error => {

            });
    }

    private createTransactionsGrid() {
            const keys: string[] = [
                "billing.stock.stocksaldo.actiontype",
                "billing.stock.stocksaldo.actionquantity",
                "billing.stock.stocksaldo.actionprice",
                "billing.stock.stocksaldo.actionnote",
                "billing.stock.stocksaldo.actioncreated",
                "billing.stock.stocksaldo.actioncreatedby",
                "core.aggrid.totals.filtered",
                "core.aggrid.totals.total",
                "billing.stock.stocksaldo.stocktransactions",
                "common.created",
                "billing.stock.stocksaldo.vouchernr"
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                this.transactionsGrid.gridAg.addColumnSelect("actionTypeName", terms["billing.stock.stocksaldo.actiontype"], null, { displayField: "actionTypeName", selectOptions: null, populateFilterFromGrid: true });
                this.transactionsGrid.gridAg.addColumnNumber("quantity", terms['billing.stock.stocksaldo.actionquantity'], null, { decimals: 2 });
                this.transactionsGrid.gridAg.addColumnNumber("price", terms['billing.stock.stocksaldo.actionprice'], null, { decimals: 2 });
                this.transactionsGrid.gridAg.addColumnText("note", terms["billing.stock.stocksaldo.actionnote"], null);
                this.transactionsGrid.gridAg.addColumnDate("transactionDate", terms['billing.stock.stocksaldo.actioncreated'], null);
                this.transactionsGrid.gridAg.addColumnDate("created", terms['common.created'], null);
                this.transactionsGrid.gridAg.addColumnText("createdBy", terms['billing.stock.stocksaldo.actioncreatedby'], null);

                if (this.showVourcherColumn)
                    this.transactionsGrid.gridAg.addColumnText("voucherNr", terms['billing.stock.stocksaldo.vouchernr'], null);

                this.transactionsGrid.gridAg.finalizeInitGrid(terms['billing.stock.stocksaldo.stocktransactions'], true);
            });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.stockProductId = 0;
        this.stockTransaction = this.createNewStockTransactionDTO();
        
        this.loadStockProducts();
    }

    private createNewStockTransactionDTO(): StockTransactionDTO {
        const stockTransaction = new StockTransactionDTO();
        stockTransaction.transactionDate = CalendarUtility.getDateNow();
        stockTransaction.actionType = TermGroup_StockTransactionType.Add;

        if (this.stockProduct) {
            stockTransaction.price = this.stockProduct.avgPrice;
        }

        return stockTransaction;
    }

    private loadStockProducts(): ng.IPromise<any> {
        return this.stockService.getStockHandledProductsSmall().then((x: IProductSmallDTO[]) => {
            x.forEach(x => this.products.push({id: x.productId, name: x.numberName}));
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.stockTransaction) {
                if (!this.stockTransaction.actionType) {
                    mandatoryFieldKeys.push("billing.stock.stocksaldo.actiontype");
                }
                if (!this.stockTransaction.quantity && this.isQuantityRequired()) {
                    mandatoryFieldKeys.push("billing.stock.stocksaldo.actionquantity");
                }
                if (!this.stockTransaction.price) {
                    mandatoryFieldKeys.push("billing.stock.stocksaldo.actionprice");
                }
                if (this.isNew && !this.selectedStockProduct) {
                    mandatoryFieldKeys.push("billing.stock.stocks.stock");
                }
                if (this.isNew && !this.selectedProduct) {
                    mandatoryFieldKeys.push("billing.stock.stocksaldo.productnumber");
                }
            }
        });
    }
}