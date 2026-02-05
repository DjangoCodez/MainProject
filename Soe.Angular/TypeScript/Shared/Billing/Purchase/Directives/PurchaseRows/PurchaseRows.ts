import { ProductRowsProductDTO } from "../../../../../Common/Models/ProductDTOs";
import { PurchaseRowDTO } from "../../../../../Common/Models/PurchaseDTO";
import { StockDTO } from "../../../../../Common/Models/StockDTO";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IProductSmallDTO, IProductUnitDTO, ISupplierProductPriceDTO, ISupplierProductSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { SelectCustomerInvoiceHelper } from "../../../Helpers/SelectCustomerInvoiceHelper";
import { IProductService } from "../../../Products/ProductService";
import { CompanySettingType, Feature, PurchaseRowType, SimpleTextEditorDialogMode, SoeEntityState, SoeEntityType, SoeInvoiceRowDiscountType, SoeOriginStatus, SoeOriginType, TermGroup_CurrencyType, TermGroup_InvoiceProductVatType, TermGroup_InvoiceVatType, TermGroup_Languages, TextBlockType, UserSettingType } from "../../../../../Util/CommonEnumerations";
import { PurchaseRowsRowFunctions, PurchaseRowsRowFeatureFunctions, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage, ProductRowsContainers } from "../../../../../Util/Enumerations";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { IColumnAggregations, TypeAheadOptionsAg } from "../../../../../Util/SoeGridOptionsAg";
import { PurchaseAmountHelper } from "../../Helpers/PurchaseAmountHelper";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { ISupplierProductService } from "../../Purchase/SupplierProductService";
import { SupplierProductDTO } from "../../../../../Common/Models/SupplierProductDTO";
import { SelectDateController } from "../../../../../Common/Dialogs/SelectDate/SelectDateController";
import { ToolBarUtility } from "../../../../../Util/ToolBarUtility";
import { TextBlockDialogController } from "../../../../../Common/Dialogs/TextBlock/TextBlockDialogController";
import { StringUtility } from "../../../../../Util/StringUtility";
import { ChangeIntrastatCodeController } from "../../../Dialogs/ChangeIntrastatCode/ChangeIntrastatCodeController";
import { IntrastatTransactionDTO } from "../../../../../Common/Models/CommodityCodesDTO";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";

class PurchaseRowsController extends GridControllerBase2Ag implements ICompositionGridController {

    private purchaseRows: PurchaseRowDTO[] = [];
    private visibleRows: PurchaseRowDTO[] = [];
    private onAddRow: (PurchaseRowDTO) => PurchaseRowDTO;
    private getProject: () => { projectId: number, projectName: string }

    private products: IProductSmallDTO[] = [];
    private productList: ProductRowsProductDTO[] = [];
    private supplierProducts: ISupplierProductSmallDTO[] = [];
    private supplierProductList: SupplierProductDTO[] = [];

    private productUnits: any[];
    private discountTypes: any[];
    private supplierId = 0;

    private readOnly: boolean = false;
    private parentGuid: any;
    private purchaseId: number;

    // Company settings
    private defaultStockId = 0;
    private defaultProductUnitId = 0;
    private defaultVatCodeId = 0;
    private useCentRounding;
    private intrastatOriginType = 0;
    private vatType: TermGroup_InvoiceVatType;
    private defaultVatRate: number = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;
    private isEuBased = false;
    private intrastatCodeId?: number;
    private sysCountryId?: number;

    private wantedDeliveryDate: Date;
    private purchaseDate: Date;

    private purchaseStatus: SoeOriginStatus;

    //permissions
    private useStock = false;
    private showSalesPricePermission = false;
    private intrastatPermission = false;

    //helpers
    private orderSelectHelper: SelectCustomerInvoiceHelper;
    private amountHelper: PurchaseAmountHelper;

    //gui
    private rowFunctions: any = [];
    private rowFeatureFunctions: any = [];
    private totalAmountExVatCurrency = 0;
    private totalAmountExVat = 0;
    private centRounding = 0;
    private tempRowIdCounter = 0;
    private totalAmountText: string;
    private steppingRules: any;

    // Flags
    private hasSelectedRows = false;
    

    get baseCurrencyCode() {
        return this.amountHelper.baseCurrencyCode;
    }

    private _hideTransferred = false;
    get hideTransferred() {
        return this._hideTransferred;
    }
    set hideTransferred(val: boolean) {
        this._hideTransferred = val;
    }
    private _showAllRows = false;
    get showAllRows() {
        return this._showAllRows;
    }
    set showAllRows(val: boolean) {
        this._showAllRows = val;
    }

    private get isBaseCurrency(): boolean {
        return this.amountHelper ? this.amountHelper.isBaseCurrency : true;
    };

    // Terms
    terms: { [index: string]: string; };

    private modalInstance: any;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private productService: IProductService,
        private supplierProductService: ISupplierProductService,
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        protected messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "billing.purchase.rows", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.doubleClickToEdit = false;

        this.onInit({});

        this.productList = [];

        this.orderSelectHelper = new SelectCustomerInvoiceHelper(this, null, urlHelperService, translationService, notificationService, $q, $uibModal,
            (result, data) => {
                data.orderId = result.customerInvoiceId || 0;
                data.orderNr = result.number || "";
                data.isModified = true;
                this.purchaseRowsUpdated(false);
                this.setParentAsModified();
            },
            () => this.getProject()
        )

        this.$scope.$on('refreshRows', (e, a) => {
            if (this.purchaseRows)
                this.gridAg.options.refreshRows();
        });

        this.$scope.$on('recalculateRows', (e, a) => {
            if (this.purchaseRows)
                this.calculateRowSums(true);
        });

        this.$scope.$on('updateFromPurchase', (e, a) => {
            if (a) {
                const updateRows = this.supplierId && (this.supplierId !== a.supplierId);
                this.supplierId = a.supplierId;
                this.purchaseDate = a.purchaseDate;
                this.supplierChanged(updateRows);
            }
        });
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        this.setupSteppingRules();

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_ShowSalesPrice, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Intrastat, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Purchase_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        this.useStock = response[Feature.Billing_Stock].modifyPermission;
        this.showSalesPricePermission = response[Feature.Billing_Product_Products_ShowSalesPrice].modifyPermission;
        this.intrastatPermission = response[Feature.Economy_Intrastat].modifyPermission;
    }

    private setupFunctions() {
        this.rowFunctions.push({ id: PurchaseRowsRowFunctions.Add, name: this.terms["common.newrow"], icon: "fal fa-plus" });
        this.rowFunctions.push({ id: PurchaseRowsRowFunctions.AddText, name: this.terms["billing.productrows.addtextrow"], icon: "fal fa-text" });
        this.rowFunctions.push({ id: PurchaseRowsRowFunctions.Delete, name: this.terms["core.deleterow"], icon: "fal fa-times iconDelete", disabled: () => { return !this.hasSelectedRows } });
    }

    private setupFeatureFunctions() {
        this.rowFeatureFunctions.push({ id: PurchaseRowsRowFeatureFunctions.SetAcknowledgedDeliveryDate, name: this.terms["billing.purchaserows.acknowledgeDeliveryDate"], icon: "fal fa-plus", disabled: () => { return (!this.hasSelectedRows || this.purchaseStatus === SoeOriginStatus.Origin) } });
        if (this.intrastatPermission && this.intrastatOriginType === SoeOriginType.Purchase)
            this.rowFeatureFunctions.push({ id: PurchaseRowsRowFeatureFunctions.Intrastat, name: this.terms["billing.productrows.functions.changeintrastatcode"], icon: "fal fa-globe", disabled: () => { return !this.purchaseId || this.purchaseId === 0 || !this.hasSelectedRows || !this.isEuBased } });
    }

    protected setupSortGroup(sortProp: string = "sort", disabled = () => { }, hidden = () => { }) {
        const group = ToolBarUtility.createSortGroup(
            () => {
                this.sortFirst();
                this.setParentAsModified();
            },
            () => {
                this.sortUp();
                this.setParentAsModified();
            },
            () => {
                this.sortDown();
                this.setParentAsModified();
            },
            () => {
                this.sortLast();
                this.setParentAsModified();
            },
            disabled,
            hidden
        );
        this.sortMenuButtons = [];
        this.sortMenuButtons.push(group);
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.setupDiscountTypes(),
            this.loadAllProducts(),
            this.loadProductUnits(),
            this.loadCompanySettings(),
        ]).then(() => {
            this.setupFunctions();
            this.setupFeatureFunctions();
            this.setupSortGroup("rowNr");
            this.loadUserAndCompanySettings();
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.BillingDefaultInvoiceProductUnit,
            CompanySettingType.BillingDefaultStock,
            CompanySettingType.BillingDefaultVatCode,
            CompanySettingType.BillingUseCentRounding,
            CompanySettingType.IntrastatImportOriginType,
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultVatCode);
            this.defaultProductUnitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceProductUnit);
            this.defaultStockId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultStock);
            this.intrastatOriginType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.IntrastatImportOriginType);
        });
    }

    private loadUserAndCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingDefaultStockPlace
        ];

        return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
            const userDefaultStockId = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingDefaultStockPlace);
            if (userDefaultStockId) {
                this.defaultStockId = userDefaultStockId;
            }
        });
    }
    private afterSetup() {
        this.setupWatchers();
    }

    private setupWatchers() {
        if (!this.purchaseRows)
            this.purchaseRows = [];

        this.$scope.$watch(() => this.showAllRows, (newValue, oldValue) => {
            if (newValue != oldValue) {
                this.expandAllRows();
            }
        });

        this.$scope.$watch(() => this.purchaseRows, (newVal, oldVal) => {
            this.purchaseRowsUpdated(true);
        });
    }

    private expandAllRows() {
        this.gridAg.options.setAutoHeight(this.showAllRows);
        if (!this.showAllRows) {
            let rows = 0;
            if (this.visibleRows)
                rows = this.visibleRows.length + 1;
            else if (this.purchaseRows)
                rows = this.visibleRows.length + 1;

            this.gridAg.options.setMinRowsToShow(8); 
        }
    }

    private setupGrid() {

        this.gridAg.options.setMinRowsToShow(8);
        const defaultEditable = true;
        const editablePrices = true;

        const gridEvents: GridEvent[] = [];
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.beginCellEdit(entity, colDef); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
            this.$timeout(() => {
                this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
            });
        }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => {
            this.$timeout(() => {
                this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
            });
        }));
        this.gridAg.options.subscribe(gridEvents);

        const keys: string[] = [
            "core.deleterow",
            "common.newrow",
            "common.rownr",
            "common.status",
            "common.new",
            "billing.order.ordernr",
            "billing.productrows.stockcode",
            "billing.productrows.addtextrow",
            "billing.purchaserows.quantity",
            "billing.purchaserows.wanteddeliverydate",
            "billing.purchaserows.accdeliverydate",
            "billing.purchaserows.deliverydate",
            "billing.purchaserows.productnr",
            "billing.purchaserows.text",
            "billing.purchaserows.discount",
            "billing.purchaserows.discounttype",
            "billing.purchaserows.deliveredquantity",
            "billing.purchaserows.purchaseprice",
            "billing.purchaserows.vatrate",
            "billing.purchaserows.vatamount",
            "billing.purchaserows.sumamount",
            "billing.purchaserows.purchaseunit",
            "billing.purchaserows.supplieritemno",
            "billing.purchaserows.acknowledgeDeliveryDate",
            "billing.productrows.functions.changeintrastatcode",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnIsModified("isModified", "", null);
            this.gridAg.addColumnNumber("rowNr", terms["common.rownr"], 30, { minWidth: 50, maxWidth: 50, suppressMovable:true, enableHiding: false, pinned: "left", editable: false });

            const productNrOptions = new TypeAheadOptionsAg();
            productNrOptions.source = (filter) => this.filterProducts(filter);
            productNrOptions.minLength = 0; 
            productNrOptions.delay = 0; 
            productNrOptions.displayField = "numberName"
            productNrOptions.dataField = "number";
            productNrOptions.useScroll = true;
         
            const supplierProductNrOptions = new TypeAheadOptionsAg();
            supplierProductNrOptions.source = (filter) => this.filterSupplierProducts(filter);
            supplierProductNrOptions.minLength = 0;
            supplierProductNrOptions.delay = 0;
            supplierProductNrOptions.displayField = "numberName"
            supplierProductNrOptions.dataField = "number";
            supplierProductNrOptions.useScroll = true;

            this.gridAg.addColumnTypeAhead("supplierProductNr", terms["billing.purchaserows.supplieritemno"], 100, {
                cellClassRules: {
                    "errorRow": (gridRow: any) => !gridRow.data.supplierProductId && gridRow.data.productId,
                },
                error: 'productError', typeAheadOptions: supplierProductNrOptions, editable: (data) => this.isRowEditable(data)
            });

            this.gridAg.addColumnText("text", terms["billing.purchaserows.text"], 100, false, { enableHiding: false, editable: (data) => this.isRowEditable(data) });

            this.gridAg.addColumnTypeAhead("productNr", terms["billing.purchaserows.productnr"], 100, {
                error: 'productError',
                cellClassRules: {
                    "errorRow": (gridRow: any) => gridRow.data.supplierProductId && !gridRow.data.productId,
                },
                typeAheadOptions: productNrOptions, editable: (data) => this.isRowEditable(data)
            });

            /*
            this.gridAg.addColumnText("supplierProductNr", terms["billing.purchaserows.supplieritemno"], 100, false, {
                enableHiding: true,
                editable: false,
                cellClassRules: {
                    "errorRow": (gridRow: any) => !gridRow.data.supplierProductNr || StringUtility.isEmpty(gridRow.data.supplierProductNr),
                }
            });
            */

            this.gridAg.addColumnNumber("quantity", terms["billing.purchaserows.quantity"], 40, { enableHiding: false, editable: (data) => this.isRowEditable(data) });

            this.gridAg.addColumnSelect("PurchaseUnitId", terms["billing.purchaserows.purchaseunit"], 30, {
                selectOptions: this.productUnits,
                enableHiding: false,
                editable: (data) => this.isRowEditable(data),
                displayField: "purchaseProductUnitCode",
                dropdownIdLabel: "value",
                dropdownValueLabel: "label",
            });

            this.gridAg.addColumnNumber("purchasePriceCurrency", terms["billing.purchaserows.purchaseprice"], 60, {
                enableHiding: false,
                decimals: 2,
                editable: (data) => this.isRowEditable(data),
                maxDecimals: 4,
                cellClassRules: {
                    "errorRow": (gridRow: any) => (gridRow.data.supplierProductId || gridRow.data.productId) && (!gridRow.data.purchasePriceCurrency || gridRow.data.purchasePriceCurrency === 0),
                }
            });
            this.gridAg.addColumnDate("wantedDeliveryDate", terms["billing.purchaserows.wanteddeliverydate"], null, null, null, { enableHiding: false, editable: (data) => this.isRowEditable(data) });

            if (this.useStock) {
                this.gridAg.addColumnSelect("stockId", terms["billing.productrows.stockcode"], 50, {
                    selectOptions: [],
                    displayField: "stockCode",
                    enableHiding: true,
                    dropdownIdLabel: "stockId",
                    dropdownValueLabel: "name",
                    dynamicSelectOptions: {
                        idField: "id",
                        displayField: "name",
                        options: "stocksForProduct"
                    },
                    editable: (data) => this.isRowEditable(data)
                });
            }

            this.gridAg.addColumnText("orderNr", terms["billing.order.ordernr"], null, null, {
                buttonConfiguration:
                {
                    iconClass: "fal fa-search", callback: (params) => { this.orderSelectHelper.openOrderSearch(params) }, show: () => true
                }
            });

            this.gridAg.addColumnDate("accDeliveryDate", terms["billing.purchaserows.accdeliverydate"], null, null, null, { enableHiding: false, editable: (data) => this.isAccDateEditable(data) });
            this.gridAg.addColumnNumber("deliveredQuantity", terms["billing.purchaserows.deliveredquantity"], 50, null);
            this.gridAg.addColumnDate("deliveryDate", terms["billing.purchaserows.deliverydate"], null, null, null, { enableHiding: false, editable: false });
            this.gridAg.addColumnNumber("discountValue", terms["billing.purchaserows.discount"], 50, { enableHiding: true, decimals: 2, editable: (data) => this.isRowEditable(data) });
            this.gridAg.addColumnSelect("discountType", terms["billing.purchaserows.discounttype"], 30, {
                selectOptions: this.discountTypes,
                enableHiding: true,
                editable: defaultEditable,
                displayField: "discountTypeText",
                dropdownIdLabel: "value",
                dropdownValueLabel: "label",
            });

            this.gridAg.addColumnNumber("sumAmountCurrency", terms["billing.purchaserows.sumamount"], 50, {
                enableHiding: false,
                editable: false,
                decimals: 2,
                cellClassRules: {
                    "text-right": () => true,
                    "indiscreet": () => true
                }
            });

            this.gridAg.addColumnIcon("statusIcon", null, 30, { suppressSorting: false, enableHiding: true, toolTipField: "statusName", showTooltipFieldInFilter: true });

            this.gridAg.addColumnEdit("", (row: PurchaseRowDTO) => {
                switch (row.type) {
                    case PurchaseRowType.TextRow:
                        this.showEditTextRowDialog(row);
                        break;
                    default:
                        break;
                }
            }, false, (row: PurchaseRowDTO) => row.type === PurchaseRowType.TextRow && !this.readOnly)
          
            this.gridAg.options.setSingelValueConfiguration([
                {
                    field: "text",
                    predicate: (data: PurchaseRowDTO) => data.type === PurchaseRowType.TextRow && this.readOnly,
                    editable: false,
                },
                {
                    field: "text",
                    predicate: (data: PurchaseRowDTO) => data.type === PurchaseRowType.TextRow && !this.readOnly,
                    editable: true,
                },
            ]);

            this.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);

            this.gridAg.options.addFooterRow("#purchase-sum-footer-grid", {
                "quantity": "sum",                
                "deliveredQuantity": "sum",
                "sumAmountCurrency": "sum",
            } as IColumnAggregations);

            this.gridAg.options.addTotalRow("#purchase-totals-grid", {
                filtered: this.terms["core.aggrid.totals.filtered"],
                total: this.terms["core.aggrid.totals.total"],
                selected: this.terms["core.aggrid.totals.selected"]
            });
    
            this.gridAg.finalizeInitGrid("billing.purchase.rows", false);



        });
    }


    private isRowEditable(row: PurchaseRowDTO): boolean {
        return !row.isLocked;
    }

    private isAccDateEditable(row: PurchaseRowDTO): boolean {
        return !row.isLocked && this.purchaseStatus !== SoeOriginStatus.Origin;
    }

    private beginCellEdit(row: PurchaseRowDTO, colDef: uiGrid.IColumnDef) {
        switch (colDef.field) {
            case 'stockId':
                this.setStocksForProduct(row, false)
                break;
        }
    }

    private afterCellEdit(row: PurchaseRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue && colDef.field !== 'productNr')
            return;

        switch (colDef.field) {
            case 'productNr':
                this.productChanged(row);
                break;
            case 'quantity':
                row.quantity = NumberUtility.parseNumericDecimal(row.quantity);
                this.quantityChanged(row);
                break;
            case 'purchasePriceCurrency':
                row.purchasePriceCurrency = NumberUtility.parseNumericDecimal(row.purchasePriceCurrency);
                this.purchasePriceChanged(row);
                break;
            case 'discountValue':
            case 'discountType':
                this.discountChanged(row);
                break;
            case 'supplierProductNr':
                if (oldValue === undefined && newValue === "") {
                    return;
                }
                this.supplierProductChanged(row);
                break;
        }

        this.setRowAsModified(row);
    }

    public setRowAsModified(row: PurchaseRowDTO, notify = true) {
            if (row) {
                row.isModified = true;
                if (notify)
                    this.setParentAsModified();
                this.refreshRow(row);
            }
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingHandler.publishSetDirty(this.parentGuid));
    }

    private setupDiscountTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        const keys: string[] = [
            "billing.purchaserows.discounttype.percent",
            "billing.purchaserows.discounttype.amount"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.discountTypes = [];
            this.discountTypes.push({ value: SoeInvoiceRowDiscountType.Percent, label: terms["billing.purchaserows.discounttype.percent"] });
            this.discountTypes.push({ value: SoeInvoiceRowDiscountType.Amount, label: terms["billing.purchaserows.discounttype.amount"] });
            deferral.resolve();
        });

        return deferral.promise;
    }

    private setDiscountTypeTexts() {
        this.purchaseRows.forEach(row => row.discountTypeText = this.getDiscountTypeText(row.discountType));
    }

    private getDiscountTypeText(type: number): string {
        const dt = this.discountTypes.find(x => x.value === type);
        return dt ? dt.label : '';
    }

    private setProductValues(row: PurchaseRowDTO, product: ProductRowsProductDTO) {
        if (row.type === PurchaseRowType.TextRow) return;

        const prevProductId: number = row.productId ? row.productId : 0;
        // Set Product values
        row.productId = product ? product.productId : 0;
        row.productNr = product ? product.number : '';
        row.productName = product ? product.name : '';

        if (!row.purchaseUnitId) {
            row.purchaseUnitId = product && product.productUnitId ? product.productUnitId : this.defaultProductUnitId;
        }

        if (!row.sysCountryId)
            row.sysCountryId = product ? product.sysCountryId : undefined;

        if (!row.intrastatCodeId)
            row.intrastatCodeId = product ? product.intrastatCodeId : undefined;

        // Product has changed, set text from product
        if (prevProductId === 0 || (prevProductId !== row.productId)) {
            // Set ProductUnit values
            row.purchaseUnitId = product?.productUnitId ?? this.defaultProductUnitId;
            row.purchaseProductUnitCode = product && product.productUnitId ? product.productUnitCode : this.getProductUnitCode(this.defaultProductUnitId);
            row.sysCountryId = product ? product.sysCountryId : undefined;
            row.intrastatCodeId = product ? product.intrastatCodeId : undefined;
        }
        else if (!row.purchaseProductUnitCode && row.purchaseUnitId) {
            row.purchaseProductUnitCode = this.getProductUnitCode(row.purchaseUnitId);
        }

        if (!row.supplierProductId) {
            this.setPurchasePricefromProduct(row, product);
        }

        if (product?.showDescrAsTextRowOnPurchase && !row.purchaseRowId) {

            let textRow: PurchaseRowDTO = _.find(this.purchaseRows, r => r.parentRowId === row.tempRowId && r.type === PurchaseRowType.TextRow);
            if (!textRow) {
                // Add new TextRow
                textRow = this.addRow(PurchaseRowType.TextRow, false).row;

                this.multiplyRowNr();
                textRow.parentRowId = row.tempRowId;
                textRow.rowNr = row.rowNr + 1;
                textRow.text = product.description;
                this.reNumberRows();
                this.$timeout(() => {
                    this.gridAg.options.startEditingCell(row, "quantity");
                });
            }
        }
    }

    private setPurchasePricefromProduct(row: PurchaseRowDTO, product: ProductRowsProductDTO) {
        if (!product) return;
        if (product.purchasePrice && (row.purchasePrice !== product.purchasePrice)) {
            row.purchasePrice = product.purchasePrice;
            this.amountHelper.getCurrencyAmount(row.purchasePrice, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency).then(am => {
                row.purchasePriceCurrency = am;
                this.refreshRow(row);
            });
        }
    }

    private filterProducts(filter) {
        return this.products.filter(prod => {
            return prod.number.contains(filter) || prod.name.contains(filter);
        });
    }

    private filterSupplierProducts(filter) {
        return this.supplierProducts.filter(prod => {
            return prod.number.contains(filter) || prod.name.contains(filter);
        });
    }

    private getProductUnitCode(productUnitId: number): string {
        const unit = this.productUnits.find(x => x.value === productUnitId);
        return unit ? unit.label : "";
    }

    private findProduct(row: PurchaseRowDTO): IProductSmallDTO {
        return row.productNr ? _.find(this.products, p => p.number === row.productNr) : null;
    }

    private findFullProduct(productId: number): ng.IPromise<ProductRowsProductDTO> {
        const deferral = this.$q.defer<ProductRowsProductDTO>();

        const product = this.productList.find(p => p.productId === productId);
        if (product || !productId) {
            deferral.resolve(product);
        }
        else {
            this.productService.getProductForProductRows(productId).then(x => {
                this.productList.push(x);
                deferral.resolve(x);
            });
        }

        return deferral.promise;
    }

    private findSupplierProductSmall(row: PurchaseRowDTO): ISupplierProductSmallDTO {
        return row.supplierProductNr ? _.find(this.supplierProducts, p => p.number === row.supplierProductNr) : null;
    }

    private findSupplierProduct(productId: number, supplierId: number): ng.IPromise<SupplierProductDTO> {
        const deferral = this.$q.defer<SupplierProductDTO>();

        const product = this.supplierProductList.find(p => p.productId === productId && p.supplierId == supplierId);
        if (product || !productId) {
            deferral.resolve(product);
        }
        else {
            this.supplierProductService.getSupplierProductByInvoiceProduct(productId, supplierId).then((supplierProduct: SupplierProductDTO) => {
                if (supplierProduct) {
                    this.supplierProductList.push(supplierProduct);
                }
                deferral.resolve(supplierProduct);
            });
        }

        return deferral.promise;
    }

    private findSupplierProductById(supplierProductId: number): ng.IPromise<SupplierProductDTO> {
        const deferral = this.$q.defer<SupplierProductDTO>();

        const product = this.supplierProductList.find(p => p.supplierProductId === supplierProductId);
        if (product || !supplierProductId) {
            deferral.resolve(product);
        }
        else {
            this.supplierProductService.getSupplierProduct(supplierProductId).then((supplierProduct: SupplierProductDTO) => {
                if (supplierProduct) {
                    this.supplierProductList.push(supplierProduct);
                }
                deferral.resolve(supplierProduct);
            });
        }

        return deferral.promise;
    }

    private getSupplierPurchasePrice(row: PurchaseRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<any>();

        if (row.supplierProductId) {
            this.supplierProductService.getSupplierProductPrice(row.supplierProductId, this.purchaseDate, (row.quantity) ? row.quantity : 1, this.amountHelper.currencyId).then((priceDTO: ISupplierProductPriceDTO) => {
                row.purchasePriceCurrency = priceDTO ? priceDTO.price : 0;
                deferral.resolve(true);
            });
        }
        else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }


    private loadAllProducts(): ng.IPromise<any> {
        return this.productService.getInvoiceProductsSmall().then(x => {
            this.products = x;
        });
    }
    private loadAllSupplierProducts(): ng.IPromise<any> {
        this.supplierProducts = [];
        return this.supplierProductService.getSupplierProductsSmall(this.supplierId).then(x => {
            this.supplierProducts = x;
        });
    }
    
    private loadProducts(productIds: number[], setProductValuesAfterLoad: boolean): ng.IPromise<any> {

        const deferral = this.$q.defer<any>();

        if (productIds.length === 0) {
            deferral.resolve();
            return;
        }

        // Don't fetch if we already have the product in the list
        const loadedIds: number[] = _.map(this.productList, p => p.productId);
        _.pullAll(productIds, loadedIds);

        if (productIds.length > 0) {
            this.productService.getProductsForProductRows(productIds).then(x => {
                _.forEach(x, y => {
                    if (!_.includes(_.map(this.productList, p => p.productId), y.productId))
                        this.productList.push(y);
                });

                if (setProductValuesAfterLoad)
                    this.setProductRowExtentions();

                deferral.resolve();
            });
        }
        else {
            if (setProductValuesAfterLoad)
                this.setProductRowExtentions();

            deferral.resolve();
        }

        return deferral.promise;
    }

    private setProductRowExtentions() {
        _.forEach(this.purchaseRows, row => {
            this.findFullProduct(row.productId).then((product: ProductRowsProductDTO) => {
                this.setProductValues(row, product);
            });
        })
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then((x: IProductUnitDTO[]) => {
            this.productUnits = [];
            if (x) {

                x.forEach((unit) => {
                    this.productUnits.push({ value: unit.productUnitId, label: unit.code });
                });
            }
        });
    }

    private supplierChanged(updateRows: boolean) {
        if (updateRows) {
            _.forEach(this.purchaseRows, (r) => {
                this.productChanged(r);
            })
        }

        this.loadAllSupplierProducts().then(x => {
            
        });
    }

    private supplierProductChanged(row: PurchaseRowDTO) {
        if (StringUtility.isEmpty(row.supplierProductNr)) {
            this.clearProductInfo(row);
            return;
        }

        const supplierProductSmall = this.findSupplierProductSmall(row);
        this.findSupplierProductById(supplierProductSmall.supplierProductId).then((sp: SupplierProductDTO) => {
            this.findFullProduct(sp.productId).then((p: ProductRowsProductDTO) => {
                row.supplierProductId = sp.supplierProductId;
                if (p) {
                    row.productId = p.productId;
                    row.productNr = p.number;
                    row.productName = p.name;
                    this.productChanged(row);
                }
                else {
                    this.clearProductInfo(row);
                    this.getSupplierPurchasePrice(row).then(() => {
                        this.refreshRow(row);
                    })
                }
                row.text = sp.supplierProductName;
                this.refreshRow(row);
            });
        });
    }

    private productChanged(row: PurchaseRowDTO) {

        row.supplierProductNr = undefined;
        row.supplierProductId = 0;
        row.purchasePriceCurrency = 0;

        if (StringUtility.isEmpty(row.productNr)) {
            this.setProductValues(row, undefined);
            return;
        }

        const productSmall = this.findProduct(row);

        this.$q.all([
            this.findSupplierProduct(productSmall.productId, this.supplierId).then((supplierProduct: SupplierProductDTO) => {
                if (supplierProduct) {
                    row.supplierProductNr = supplierProduct.supplierProductNr;
                    row.supplierProductId = supplierProduct.supplierProductId;
                    row.text = supplierProduct.supplierProductName;
                    row.sysCountryId = supplierProduct.sysCountryId;

                    this.getSupplierPurchasePrice(row).then(() => {
                        this.refreshRow(row);
                    });
                }
                else {
                    row.text = productSmall.name;
                }
            }),
            this.findFullProduct(productSmall.productId).then((product: ProductRowsProductDTO) => {
                this.setProductValues(row, product);
                this.setStocksForProduct(row, true);
            }),
        ]);
    }

    private quantityChanged(row: PurchaseRowDTO) {
        this.getSupplierPurchasePrice(row).then(() => {
            this.calculateRowSum(row);
            this.refreshRow(row);
        });
    }

    private purchasePriceChanged(row: PurchaseRowDTO) {
        this.calculateRowSum(row);
    }

    private discountChanged(row: PurchaseRowDTO) {
        this.calculateRowSum(row);
    }

    public clearProductInfo(row: PurchaseRowDTO) {
        row.productNr = undefined;
        row.productId = 0;
        row.purchasePriceCurrency = 0;
        this.setProductValues(row, undefined);
    }

    public calculateRowSum(row: PurchaseRowDTO, calcTotals = true, ignoreSetModified = false): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.calculateDiscount(row);
        row.sumAmountCurrency = (row.quantity * row.purchasePriceCurrency) - row.discountAmountCurrency;
        this.amountHelper.getCurrencyAmount(row.sumAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => {
            row.sumAmount = am;

            if(!ignoreSetModified)
                row.isModified = true;

            if (calcTotals) {
                this.calculateTotals();
            }

            deferral.resolve();
        });

        return deferral.promise;
    }

    public calculateRowSums(refreshRows = false, ignoreSetModified = true) {
        const promiseList: ng.IPromise<any>[] = [];
        this.visibleRows.forEach(x => {
            promiseList.push(this.calculateRowSum(x, false, ignoreSetModified));
        });

        this.$q.all(promiseList).then(() => {
            this.calculateTotals();
            if (refreshRows)
                this.gridAg.options.refreshRows();
        });
    }

    private calculateTotals() {
        this.totalAmountExVatCurrency = 0;
        this.visibleRows.forEach(r => {
            this.totalAmountExVatCurrency += r.sumAmountCurrency;
        });

        let cent: number = 0;
        if (this.useCentRounding) {
            cent = Math.abs(this.totalAmountExVatCurrency) - Math.floor(Math.abs(this.totalAmountExVatCurrency))
            if (cent !== 0) {
                cent = this.totalAmountExVatCurrency.round(0) - this.totalAmountExVatCurrency;
                this.totalAmountExVatCurrency = this.totalAmountExVatCurrency.round(0);
                this.centRounding = cent;
            }
        }

        
        this.totalAmountExVat = (this.totalAmountExVatCurrency) * this.amountHelper.transactionCurrencyRate;
    }

    private calculateDiscount(row: PurchaseRowDTO) {
        row.discountAmountCurrency = 0;
        row.discountPercent = 0;

        if (row.purchasePriceCurrency !== 0) {
            const amountSum: number = row.purchasePriceCurrency * row.quantity;

            if (!row.discountValue)
                row.discountValue = 0;

            if (row.discountType === SoeInvoiceRowDiscountType.Amount) {
                row.discountAmountCurrency = NumberUtility.parseNumericDecimal(row.discountValue).round(2);
                row.discountPercent = amountSum !== 0 ? row.discountValue / amountSum * 100 : 0;
            } else if (row.discountType === SoeInvoiceRowDiscountType.Percent) {
                row.discountPercent = row.discountValue;
                row.discountAmountCurrency = (amountSum * row.discountPercent / 100).round(2);
            }
        }
    }

    private executeRowFunction(option) {
        switch (option.id) {
            case PurchaseRowsRowFunctions.Add:
                this.addRow(PurchaseRowType.PurchaseRow, true);
                break;
            case PurchaseRowsRowFunctions.Delete:
                this.deleteRows();
                break;
            case PurchaseRowsRowFunctions.AddText:
                this.addRow(PurchaseRowType.TextRow, true);
                break;
        }
    }

    private executeFeatureRowFunction(option) {
        switch (option.id) {
            case PurchaseRowsRowFeatureFunctions.SetAcknowledgedDeliveryDate:
                this.changeAcknowledgeDeliveryDate();
                break;
            case PurchaseRowsRowFeatureFunctions.Intrastat:
                this.changeIntrastat();
                break;
        }
    }

    private changeAcknowledgeDeliveryDate() {
        this.showSelectDateDialog(new Date()).then((selectedDate: Date) => {
            if (selectedDate) {

                const selectedRows = this.gridAg.options.getSelectedRows();
                if (selectedRows && selectedRows.length > 0) {
                    selectedRows.forEach((row: PurchaseRowDTO) => {
                        row.accDeliveryDate = selectedDate;
                        if (row.purchaseRowId) {
                            this.setRowAsModified(row);
                        }
                    });

                    this.purchaseRowsUpdated(false);
                }
            }
        });
    }

    private showSelectDateDialog(defaultDate: Date): ng.IPromise<Date> {
        const deferral = this.$q.defer<Date>();

        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectDate", "SelectDate.html"),
                controller: SelectDateController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    title: () => { return term },
                    defaultDate: () => { return defaultDate }
                }
            });

            modal.result.then(result => {
                if (result && result.selectedDate) {
                    deferral.resolve(result.selectedDate);
                }
                else {
                    deferral.resolve(undefined);
                }
            });
        });

        return deferral.promise;
    }

    private addRow(type: PurchaseRowType, setFocus: boolean): { rowIndex: number, row: PurchaseRowDTO, column: any } {
        if (!this.purchaseRows) {
            this.purchaseRows = [];
        }

        const selectedRow = this.gridAg.options.getCurrentRow();
        const rowNr = selectedRow && (type === PurchaseRowType.TextRow) ? selectedRow.rowNr : PurchaseRowDTO.getNextRowNr(this.visibleRows);

        let row = new PurchaseRowDTO();
        row.rowNr = rowNr;
        row.state = SoeEntityState.Active;
        row.quantity = 0;
        row.sumAmountCurrency = 0;
        row.purchasePriceCurrency = 0;
        row.vatAmountCurrency = 0;
        row.discountType = SoeInvoiceRowDiscountType.Percent;
        row.discountAmount = 0;
        row.type = type;
        row.status = 0;
        row.statusName = "";

        if (type === PurchaseRowType.PurchaseRow && this.wantedDeliveryDate)
            row.wantedDeliveryDate = this.wantedDeliveryDate;

        if (this.onAddRow)
            this.onAddRow(row);

        this.purchaseRows.push(row);

        const startColumnName = this.getStartColumnName();
        const focusColumn = this.gridAg.options.getColumnByField(startColumnName);

        this.purchaseRowsUpdated(false).then(x => {
            this.getStartColumn();
            this.setRowAsModified(row);
            if (setFocus) {
                this.gridAg.options.startEditingCell(row, startColumnName);
            }
        });
        
        return { rowIndex: this.visibleRows.length - 1, column: focusColumn, row };
    }

    private getStartColumnName(): string {
        let result = "productNr";
        const prodNr = this.gridAg.options.getColumnByField('productNr');
        const supplierProdNr = this.gridAg.options.getColumnByField('supplierProductNr');
        if (prodNr.left > supplierProdNr.left) {
            result = "supplierProductNr";
        }
        
        return result;
    }

    private getStartColumn() {
        return this.gridAg.options.getColumnByField(this.getStartColumnName());
    }

    private deleteRows() {
        const keys = ["core.warning", "core.deleterowwarning"];

        this.translationService.translateMany(keys).then(terms => {
            const modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deleterowwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    const selectedRows = this.gridAg.options.getSelectedRows();
                    if (selectedRows && selectedRows.length > 0) {
                        selectedRows.forEach((row: PurchaseRowDTO) => {
                            this.deleteRow(row);
                        });

                        this.purchaseRowsUpdated(false, true);
                    }
                }
            });
        });
    }

    private deleteRow(row: PurchaseRowDTO) {
        row.state = SoeEntityState.Deleted;
        if (row.purchaseRowId) {
            this.setRowAsModified(row);

            const childRows = this.purchaseRows.filter(x => x.parentRowId === row.purchaseRowId)
            for (const childRow of childRows) {
                this.deleteRow(childRow);
            }
        }
        else {
            const index: number = this.purchaseRows.indexOf(row);
            this.purchaseRows.splice(index, 1);
            const childRows = this.purchaseRows.filter(x => x.parentRowId === row.tempRowId)
            for (const childRow of childRows) {
                this.deleteRow(childRow);
            }
        }
    }

    private changeIntrastat() {
        const selectedRows = _.filter(this.gridAg.options.getSelectedRows(), r => r.type === PurchaseRowType.PurchaseRow && !r.isModified);
        this.productService.getProductsForProductRows(_.map(selectedRows, p => p.productId)).then(x => {
            const tempRows: IntrastatTransactionDTO[] = [];
            _.forEach(selectedRows, (r: PurchaseRowDTO) => {
                // Get product to check vattype
                let isService = false;
                let weight = 0;
                const product = x.find(p => p.productId === r.productId);
                if (product) {
                    if (product.vatType === TermGroup_InvoiceProductVatType.Service)
                        isService = true;
                    weight = product.weight;
                }

                if (!isService) {
                    const dto = new IntrastatTransactionDTO();
                    dto.rowNr = r.rowNr;
                    dto.customerInvoiceRowId = r.purchaseRowId;
                    dto.intrastatTransactionId = r.intrastatTransactionId;
                    dto.intrastatCodeId = r.intrastatCodeId;
                    dto.sysCountryId = r.sysCountryId;
                    dto.originId = this.purchaseId;
                    dto.productName = r.productName;
                    dto.productNr = r.productNr;
                    dto.productUnitId = r.purchaseUnitId;
                    dto.productUnitCode = r.purchaseProductUnitCode;
                    dto.quantity = r.quantity;
                    dto.state = SoeEntityState.Active;
                    dto.netWeight = weight;

                    if (!dto.intrastatCodeId && this.intrastatCodeId)
                        dto.intrastatCodeId = this.intrastatCodeId;

                    if (!dto.sysCountryId && this.sysCountryId)
                        dto.sysCountryId = this.sysCountryId;

                    tempRows.push(dto);
                }
            });
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ChangeIntrastatCode/ChangeIntrastatCode.html"),
                controller: ChangeIntrastatCodeController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => this.translationService,
                    coreService: () => this.coreService,
                    productService: () => this.productService,
                    transactions: () => tempRows,
                    originType: () => SoeOriginType.Purchase,
                    originId: () => this.purchaseId,
                    notificationService: () => this.notificationService,
                    urlHelperService: () => this.urlHelperService,
                    totalAmount: () => undefined,
                }
            });

            modal.result.then((result: any[]) => {
                if (result) {
                    this.messagingService.publish(Constants.EVENT_RELOAD_ROWS, this.parentGuid);
                }
            }, function () {
            });

            return modal;
        });
    }

    private showEditTextRowDialog(row: PurchaseRowDTO) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
            controller: TextBlockDialogController,
            controllerAs: "ctrl",
            backdrop: 'static',
            size: 'lg',
            resolve: {
                text: () => { return row.text },
                editPermission: () => { return this.readOnly === false },
                entity: () => { return SoeEntityType.CustomerInvoice },
                type: () => { return TextBlockType.TextBlockEntity },
                headline: () => { return "" },
                mode: () => { return SimpleTextEditorDialogMode.EditInvoiceRowText },
                container: () => { return ProductRowsContainers.Purchase },
                langId: () => { return TermGroup_Languages.Swedish },
                maxTextLength: () => { return null },
                textboxTitle: () => { return undefined },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                if (result.text !== row.text) {
                    this.setRowAsModified(row, true);
                }
                row.text = result.text;

            }
            this.gridAg.options.refreshRows();
        });
    }

    private purchaseRowsUpdated(updateTextProperties: boolean, renumber = false): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.purchaseRows) {

            const productsToLoad: number[] = [];

            this.purchaseRows.forEach(r => {

                if (r.productId && !_.includes(productsToLoad, r.productId))
                    productsToLoad.push(r.productId);

                if (r.purchaseRowId)
                    r.tempRowId = r.purchaseRowId;
                else if (!r.tempRowId) {
                    r.tempRowId = this.tempRowIdCounter;
                    this.tempRowIdCounter += 1;
                }

                if (!r.discountValue) {
                    r.discountValue = r.discountPercent ? r.discountPercent : r.discountAmountCurrency;
                }
                if (!r.stocksForProduct) {
                    r.stocksForProduct = [];
                }
            });

            this.visibleRows = _.orderBy(_.filter(this.purchaseRows, r => r.state === SoeEntityState.Active), 'rowNr');

            if (updateTextProperties) {
                this.setDiscountTypeTexts();
                this.calculateRowSums(false, true)
            }

            if(renumber)
                this.reNumberRows();

            this.$timeout(() => {
                if (updateTextProperties && productsToLoad.length > 0) {
                    this.loadProducts(productsToLoad, true).then(() => {
                        this.gridAg.setData(this.visibleRows);
                    });
                }
                else {
                    this.gridAg.setData(this.visibleRows);
                }

                this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;

                deferral.resolve();
            });
        }

        return deferral.promise;
    }


    private setStocksForProduct(row: PurchaseRowDTO, productChanged = false) {
        if (!this.useStock) {
            row.stocksForProduct = [];
            return;
        }

        if (!row.stocksForProduct || productChanged) {
            row.stocksForProduct = [];
        }

        // get stocks for product
        if (row.productId && (productChanged || row.stocksForProduct.length === 0)) {

            //add empty row

            row.stocksForProduct.push({ id: 0, name: "" });

            this.productService.getStocksByProduct(row.productId).then((stockList: StockDTO[]) => {

                stockList.forEach((stock) => {
                    row.stocksForProduct.push({ id: stock.stockId, name: stock.code + ' ' + stock.saldo });
                });

                //set default stock
                if (productChanged) {
                    const defaultStock = stockList.find(s => s.stockId === this.defaultStockId);
                    if (defaultStock) {
                        row.stockId = defaultStock.stockId;
                        row.stockCode = defaultStock.code;
                    }
                    else {
                        row.stockId = 0;
                        row.stockCode = "";
                    }
                }

                this.gridAg.options.refreshRows(row);
            });
        }
    }

    // Sorting
    private sortFirst() {
        // Get current row
        const handledRows: number[] = [];
        const rows: PurchaseRowDTO[] = this.gridAg.options.getSelectedRows().sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.gridAg.options.getCurrentRow());

        rows.forEach((row) => {
            if (!handledRows.find((id) => id === row.tempRowId)) {

                if (handledRows.length === 0) {
                    _.forEach(_.filter(this.visibleRows, r => r.rowNr > 0 && r.rowNr <= row.rowNr), (r) => {
                        this.setRowAsModified(r, false);
                    });
                }

                // Move row to the top
                row.rowNr = -(rows.length - handledRows.length);

                /*if (this.isParentRow(row)) {
                    // Current row is a parent row
                    // Move its child row(s) to be directly after
                    let rowNr = row.rowNr + 1;
                    this.getChildRows(row).forEach(child => {
                        child.rowNr = rowNr;
                        rowNr++;
                        this.setRowAsModified(child, false);
                        handledRows.push(child.tempRowId);
                    });
                } else if (this.isChildRow(row)) {
                    // Current row is a child row
                    // Move its parent row to be directly before
                    const parent = this.getParentRow(row);
                    if (parent) {
                        parent.rowNr = -100;
                        let rowNr = parent.rowNr + 1;
                        this.getChildRows(parent).forEach(child => {
                            child.rowNr = rowNr;
                            rowNr++;
                            this.setRowAsModified(child, false);
                            handledRows.push(child.tempRowId);
                        });
                        handledRows.push(parent.tempRowId);
                    }
                }*/
                handledRows.push(row.tempRowId);
            }
        });

        this.afterSortMultiple(rows);
    }

    private sortUp() {
        // Get current row
        const handledRows: number[] = [];
        const rows: PurchaseRowDTO[] = this.gridAg.options.getSelectedRows().sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.gridAg.options.getCurrentRow());

        if (rows.length > 0) {
            this.multiplyRowNr();

            // Move current row before previous row
            rows.forEach((row) => {
                const prevRow = _.last(_.sortBy(_.filter(this.visibleRows, (r) => r.rowNr < row.rowNr), r => r.rowNr));

                if (prevRow) {
                    row.rowNr = prevRow.rowNr - (rows.length - handledRows.length) - 10;
                    handledRows.push(row.tempRowId);
                    this.setRowAsModified(prevRow);
                }
            });

            this.afterSortMultiple(rows);
        }
    }

    private sortDown() {
        // Get current row
        const handledRows: number[] = [];
        const rows: PurchaseRowDTO[] = this.gridAg.options.getSelectedRows().filter(r => r.rowNr < this.visibleRows.length).sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.gridAg.options.getCurrentRow());

        if (rows.length > 0) {
            this.multiplyRowNr();
            rows.forEach((row) => {
                // Get next row
                let nextRow = _.head(_.sortBy(_.filter(this.visibleRows, r => r.rowNr > row.rowNr && !_.find(rows, (sr) => sr.tempRowId === r.tempRowId)), 'rowNr'));

                if (nextRow) {
                    if (!handledRows.find((id) => id === row.tempRowId)) {
                        // Move current row after next row                    
                        row.rowNr = nextRow.rowNr + (rows.length + handledRows.length) + 10;
                        handledRows.push(row.tempRowId);
                    }
                    this.setRowAsModified(nextRow);
                }
            });

            this.afterSortMultiple(rows);
        }
    }

    private sortLast() {
        const handledRows: number[] = [];
        const rows: PurchaseRowDTO[] = this.gridAg.options.getSelectedRows().filter(r => r.rowNr <= this.visibleRows.length).sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.gridAg.options.getCurrentRow());

        rows.forEach((row) => {
            if (!handledRows.find((id) => id === row.tempRowId)) {
                if (handledRows.length === 0) {
                    _.forEach(_.filter(this.visibleRows, r => r.rowNr >= row.rowNr), (r) => {
                        this.setRowAsModified(r, false);
                    });

                }

                // Move row to the bottom
                row.rowNr = NumberUtility.max(this.visibleRows, 'rowNr') + 2 + handledRows.length;

                /*if (this.isParentRow(row)) {
                    // Current row is a parent row
                    // Move its child row(s) to be directly after
                    let rowNr = row.rowNr + 1;
                    this.getChildRows(row).forEach(child => {
                        child.rowNr = rowNr;
                        rowNr++;
                        this.setRowAsModified(child, false);
                        handledRows.push(child.tempRowId);
                    });
                } else if (this.isChildRow(row)) {
                    // Current row is a child row
                    // Move its parent row to be directly before
                    const parent = this.getParentRow(row);
                    if (parent) {
                        parent.rowNr = NumberUtility.max(this.activeRows, 'rowNr') + 2;
                        let rowNr = parent.rowNr + 1;
                        this.getChildRows(parent).forEach(child => {
                            child.rowNr = rowNr;
                            rowNr++;
                            this.setRowAsModified(child, false);
                            handledRows.push(child.tempRowId);
                        });
                        handledRows.push(parent.tempRowId);
                    }
                }*/

                handledRows.push(row.tempRowId);
            }
        });

        this.afterSortMultiple(rows);
    }

    private afterSortMultiple(rows: PurchaseRowDTO[]) {
        rows.forEach((row) => {
            this.setRowAsModified(row, false);
        });

        this.reNumberRows();
        this.setParentAsModified();
    }

    private multiplyRowNr() {
        _.forEach(this.visibleRows, x => {
            x.rowNr *= 100;
        });
    }

    private reNumberRows() {

        let i = 1;
        _.forEach(_.orderBy(_.filter(this.visibleRows, r => r.type === PurchaseRowType.PurchaseRow || r.type === PurchaseRowType.TextRow), 'rowNr'), r => {
            const oldRowNr = r.rowNr;
            r.rowNr = i++;
            if (oldRowNr && oldRowNr !== r.rowNr) {
                r.isModified = true;
            }
        });

        this.resetRows();
    }

    private resetRows() {
        const selectedRowIds = this.gridAg.options.getSelectedIds("tempRowId");

        this.visibleRows = _.orderBy(this.visibleRows, 'rowNr');
        this.gridAg.setData(this.visibleRows);
        this.gridAg.options.refreshRows();

        if (selectedRowIds && selectedRowIds.length > 0) {
            const selectedRows = this.visibleRows.filter(p => selectedRowIds.some(s => s === p.tempRowId));
            this.gridAg.options.selectRows(selectedRows);
        }

        this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
    }
    private refreshRow(row: PurchaseRowDTO) {
        this.gridAg.options.refreshRows(row);
    }

    private setupSteppingRules() {
        const mappings =
        {
            productNr(row: PurchaseRowDTO) { return !row.productId && !row.supplierProductNr },
            supplerProductNr(row: PurchaseRowDTO) { return !row.productId && !row.supplierProductNr },
            quantity(row: PurchaseRowDTO) { return true },
            text(row: PurchaseRowDTO) { return false },
            purchasePriceCurrency(row: PurchaseRowDTO) { return true },
            wantedDeliveryDate(row: PurchaseRowDTO) { return !row.wantedDeliveryDate },
        };

        this.steppingRules = mappings;
    }

    protected handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;

        if (!nextCellPosition) {
            return null;
        }

        let { rowIndex, column } = nextCellPosition;
        const rowByIndex = this.gridAg.options.getVisibleRowByIndex(rowIndex);

        if (!rowByIndex)
        { return { rowIndex: rowIndex, column: column } }

        let row: PurchaseRowDTO = rowByIndex.data;

        const steppingResult = this.nextColumnSteppingRule(rowIndex, column, row, backwards);
        
        if (steppingResult) {
            return steppingResult;
        }

        //no new valid cell found to navigate to so return null so we will switch row...
        const nextRowResult = backwards ? this.findPreviousRow(row) : this.findNextRow(row);
        const newRowIndex = nextRowResult ? nextRowResult.rowIndex : this.addRow(PurchaseRowType.PurchaseRow, true).rowIndex;
        if (backwards) {
            const steppingResult2 = this.nextColumnSteppingRule(newRowIndex, this.gridAg.options.getLastEditableColumn(), nextRowResult.rowNode.data, backwards);
            if (steppingResult2) {
                return steppingResult2;
            }
        }

        return { rowIndex: newRowIndex, column: backwards ? this.gridAg.options.getLastEditableColumn() : this.getStartColumn() };
    }

    private nextColumnSteppingRule(rowIndex: number, column: any, row: PurchaseRowDTO, backwards: boolean): { rowIndex: number, column: any } {
        let nextColumnCaller: (column: any) => any = backwards ? this.gridAg.options.getPreviousVisibleColumn : this.gridAg.options.getNextVisibleColumn;
        while (!!column && !!this.steppingRules) {
            const { colDef } = column;
            if (this.gridAg.options.isCellEditable(row, colDef)) {
                const steppingRule = this.steppingRules[colDef.field];
                const stop = !!steppingRule ? steppingRule.call(this, row) : false;

                if (stop) {
                    return { rowIndex, column };
                }
            }

            column = nextColumnCaller(column);
        }

        return null;
    }
}

export class PurchaseRowsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Billing/Purchase/Directives/PurchaseRows/PurchaseRows.html"),
            scope: {
                parentGuid: "=",
                purchaseId: "=",
                purchaseRows: '=',
                onAddRow: "=",
                getProject: "=",
                amountHelper: "=",
                totalAmountExVatCurrency: '=',
                useCentRounding: '=',
                vatType: '=',
                wantedDeliveryDate: "=",
                purchaseStatus: "=",
                isEuBased: "=",
                intrastatCodeId: "=?",
                sysCountryId: "=?"
            },
            restrict: 'E',
            replace: true,
            controller: PurchaseRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}