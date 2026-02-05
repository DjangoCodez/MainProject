import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { InvoiceProductDTO } from "../../../../Common/Models/ProductDTOs";
import { IPriceListDTO, ISmallGenericType, IProductUnitSmallDTO, IActionResult } from "../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../Common/Models/smallgenerictype";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IProductService } from "../ProductService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Guid } from "../../../../Util/StringUtility";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { TermGroup_InvoiceProductVatType, Feature, CompanySettingType, ProductAccountType, SoeEntityState, CompTermsRecordType, TermGroup, SoeTimeCodeType, TermGroup_InvoiceProductCalculationType, ActionResultSave, ActionResultDelete, SoeOriginType, SoeEntityType, SettingMainType, ExternalProductType, TermGroup_Country } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ProductStockHelper } from "./Helpers/ProductStockHelper";
import { ProductUnitConvertDTO } from "../../../../Common/Models/ProductUnitConvertDTO";
import { IStockService } from "../../Stock/StockService";
import { ExtraFieldGridDTO, ExtraFieldRecordDTO } from "../../../../Common/Models/ExtraFieldDTO";
import { EmbeddedGridController } from "../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { AccountDimSmallDTO } from "../../../../Common/Models/AccountDimDTO";
import { CopyProductController } from "./Dialogs/CopyProduct/CopyProductController";
import { CoreUtility } from "../../../../Util/CoreUtility";
export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //helpers
    private productStockHelper: ProductStockHelper;

    // Modal        
    private modal;
    isModal = false;

    // Data
    private product: InvoiceProductDTO;

    // Lookups
    private categoryRecords: any = [];
    private compTermRows: any[];
    productAccountPriorityRows: any[];
    productAccountingSettings: any[] = [];
    stockAccountingSettings: any[] = [];
    private extraFieldRecords: ExtraFieldRecordDTO[];
    
    priceListRows: IPriceListDTO[] = [];

    private terms: any;

    // Properties
    productId: number;
    productIds: number[];
    private presetProductNumber: string;
    private active = true;
    private showCalculatedCost = false;
    private showGuaranteePercentage = false;
    private isExternal = false;
    private compTermRecordType: number;
    private fromStock: number;
    private toStock: number;
    private amountToMove: number;
    private eanNaN = false;

    productAccountSettingTypes: SmallGenericType[];
    stockAccountSettingTypes: SmallGenericType[];
    productBaseAccounts: SmallGenericType[];
    stockBaseAccounts: SmallGenericType[];

    // company settings
    private defaultVatType: TermGroup_InvoiceProductVatType;
    private defaultProductUnitId: number;
    private defaultTimeCodeId: number;
    private defaultVatCodeId: number;
    private defaultHouseholdDeductionType: number;
    defaultStockId: number;
    private useProductUnitConvert = false;
    private copyProductPrices = false;
    private copyProductAccounts = false;
    private copyProductStock = false;

    // Permissions
    modifyCategoryPermission = false;
    modifyPermission = false;
    showPurchasePrice = false;
    stockHandling = false;
    useStockPermission = false;
    hasExtraFieldPermission = false;
    hasCommodityCodesPermission = false;
    hasPurchaseProductsPermission = false;

    // Lookups
    private vatTypes: ISmallGenericType[];
    private productUnits: IProductUnitSmallDTO[];
    private productGroups: any[] = [];
    private calculationTypes: ISmallGenericType[];
    private materialCodes: ISmallGenericType[];
    private vatCodes: any[];
    private householdDeductionTypes: any[];
    private accountingPrios: any[];
    private stocks: ISmallGenericType[] = [];
    private commodityCodes: any[];
    private countries: ISmallGenericType[] = [];
    private grossMarginCalculationTypes: ISmallGenericType[];

    //StockTransactions
    private productUnitConverts: ProductUnitConvertDTO[] = [];

    // Extra fields
    private extraFields: ExtraFieldGridDTO[] = [];
    get showExtraFieldsExpander() {
        return this.hasExtraFieldPermission && this.extraFields.length > 0;
    }

    //Statistics expander
    private gridHandler: EmbeddedGridController;
    private statisticsGridButtonGroups = new Array<ToolBarButtonGroup>();
    productStatisticsGridFooterComponentUrl: any;
    filteredTotal = 0;
    selectedTotal = 0;
    originTypes: any[];
    selectedOriginType: number;
    allItemsSelectionDict: any[];

    //Flags
    priceExpanderRendered = false;
    accountSettingsExpanderRendered = false;
    stockExpanderRendered = false;
    supplierProductsExpanderRendered = false;
    statisticsExpanderRendered= false;
    translationsExpanderRendered= false;
    extraFieldsExpanderRendered = false;

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
    }

    private _selectedProductGroup;
    get selectedProductGroup() {
        return this._selectedProductGroup;
    }
    set selectedProductGroup(item: any) {
        this._selectedProductGroup = item;
        if (this._selectedProductGroup?.productGroupId > 0)
            this.product.productGroupId = item.productGroupId;
        else
            this.product.productGroupId = undefined;

        this.dirtyHandler.setDirty();
    }

    private _selectedCommodityCode;
    get selectedCommodityCode() {
        return this._selectedCommodityCode;
    }
    set selectedCommodityCode(item: any) {
        this._selectedCommodityCode = item;
        if (this._selectedCommodityCode && this._selectedCommodityCode.id > 0) {
            this.product.intrastatCodeId = item.id;
        }
        else {
            this.product.intrastatCodeId = undefined;
        }

        this.setCommodityCodeTooltip();
        this.dirtyHandler.setDirty();
    }

    private commodityCodeTooltip: string;

    private modalInstance: any;

    //@ngInject
    constructor(
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private productService: IProductService,
        stockService: IStockService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private toolbarFactory: IToolbarFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.presetProductNumber = parameters.presetProductNumber || undefined;
            this.onInit(parameters);
        });

        this.flowHandler = this.controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.productStockHelper = new ProductStockHelper(productService);

        //statistics
        this.productStatisticsGridFooterComponentUrl = urlHelperService.getGlobalUrl("Billing/Products/Products/Views/productStatisticsGridFooter.html");
        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "common.statistics");

        this.allItemsSelection = 1; //default   
    }

    // SETUP

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.productId = parameters.id || 0;
        this.isExternal = parameters.isExternal;
        this.productIds = parameters.ids;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Billing_Product_Products_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Product_Products_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Product_Products_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy());
        const buttonGroup = ToolBarUtility.createGroup();
        this.toolbar.addButtonGroup(buttonGroup);

        if (this.productId > 0) {
            this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (newproductId) => {
                this.productId = newproductId;
                this.onLoadData();
            }, this.productIds, this.productId);

            if (CoreUtility.sysCountryId == TermGroup_Country.FI || this.product.sysProductType == ExternalProductType.Plumbing) {
                buttonGroup.buttons.push(new ToolBarButton("common.searchinvoiceproduct.showexternalproductinfo", "common.searchinvoiceproduct.showexternalproductinfo", IconLibrary.FontAwesome, "fa-arrow-up-right-from-square",
                    () => { this.openProductInfo(); },
                    null,
                    () => { return !this.isExternal; }));
            }
        }

        buttonGroup.buttons.push( new ToolBarButton("billing.products.products.converttouserproduct", "billing.products.products.converttouserproductbuttontooltip", IconLibrary.FontAwesome, "fa-sync",
            () => { this.convertToUserProduct(); },
            null,
            () => { return !this.isExternal; }));

        this.statisticsGridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.search", "core.search", IconLibrary.FontAwesome, "fa-search", () => {
            this.loadProductStatistics();
        }, null, () => { return this.isNew; })));
    }

    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.$q.all([
                this.loadModifyPermissions(),
                this.loadCompanySettings(),
                this.loadTerms(),
                this.loadVatTypes(),
                this.loadProductGroups(),
                this.loadCalculationTypes(),
                this.loadHouseholdDeductionTypes(),
                this.loadSelectionTypes()]).then(() => {
                    this.$q.all([
                        this.loadOriginTypes(),
                        this.loadVatCodes(),
                        this.loadProductUnits(),
                        this.loadMaterialCodes(),
                        this.loadproductUnitConverts(),
                        this.loadAccountingPriority(),
                        this.loadSettingTypes(),
                        this.loadExtraFields(),
                        this.loadCommodityCodes(),
                        this.loadCountries(),
                        this.loadGrossMarginCalculationTypes()])
                        .then(() => {
                            this.setupStatisticsSubGrid();
                        })
                })
        ]);
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Product_Products_Edit,
            Feature.Common_Categories_Product_Edit,
            Feature.Billing_Product_Products_ShowPurchasePrice,
            Feature.Billing_Stock,
            Feature.Billing_Order_Orders_Edit_ProductRows_Stock,
            Feature.Billing_Product_Products_ExtraFields,
            Feature.Economy_Intrastat,
            Feature.Billing_Purchase_Products,
        ];

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.modifyPermission = x[Feature.Billing_Product_Products_Edit];
            this.modifyCategoryPermission = x[Feature.Common_Categories_Product_Edit];
            this.showPurchasePrice = x[Feature.Billing_Product_Products_ShowPurchasePrice];
            this.stockHandling = x[Feature.Billing_Stock];
            this.useStockPermission = x[Feature.Billing_Order_Orders_Edit_ProductRows_Stock];
            this.hasExtraFieldPermission = x[Feature.Billing_Product_Products_ExtraFields];
            this.hasCommodityCodesPermission = x[Feature.Economy_Intrastat];
            this.hasPurchaseProductsPermission = x[Feature.Billing_Purchase_Products]
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.BillingDefaultInvoiceProductVatType);
        settingTypes.push(CompanySettingType.BillingDefaultInvoiceProductUnit);
        settingTypes.push(CompanySettingType.BillingStandardMaterialCode);
        settingTypes.push(CompanySettingType.BillingDefaultVatCode);
        settingTypes.push(CompanySettingType.BillingDefaultHouseholdDeductionType);
        settingTypes.push(CompanySettingType.BillingDefaultStock);

        settingTypes.push(CompanySettingType.AccountCustomerClaim);
        settingTypes.push(CompanySettingType.AccountCustomerSalesVat);
        settingTypes.push(CompanySettingType.AccountCommonVatPayable1);
        settingTypes.push(CompanySettingType.AccountCustomerSalesNoVat);
        settingTypes.push(CompanySettingType.AccountCommonReverseVatSales);

        settingTypes.push(CompanySettingType.AccountStockIn);
        settingTypes.push(CompanySettingType.AccountStockInChange);
        settingTypes.push(CompanySettingType.AccountStockOut);
        settingTypes.push(CompanySettingType.AccountStockOutChange);
        settingTypes.push(CompanySettingType.AccountStockInventory);
        settingTypes.push(CompanySettingType.AccountStockInventoryChange);
        settingTypes.push(CompanySettingType.AccountStockLoss);
        settingTypes.push(CompanySettingType.AccountStockLossChange);
        settingTypes.push(CompanySettingType.AccountStockTransferChange);
        
        settingTypes.push(CompanySettingType.BillingUseProductUnitConvert);

        settingTypes.push(CompanySettingType.BillingCopyProductPrices);
        settingTypes.push(CompanySettingType.BillingCopyProductAccounts);
        settingTypes.push(CompanySettingType.BillingCopyProductStock);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultVatType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceProductVatType, this.defaultVatType);
            this.defaultProductUnitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceProductUnit, this.defaultProductUnitId);
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStandardMaterialCode, this.defaultTimeCodeId);
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultVatCode, this.defaultVatCodeId);
            this.defaultHouseholdDeductionType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultHouseholdDeductionType, this.defaultHouseholdDeductionType);
            this.defaultStockId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultStock, this.defaultStockId);
            this.useProductUnitConvert = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseProductUnitConvert, this.useProductUnitConvert);

            // Base accounts for product
            this.productBaseAccounts = [];
            this.productBaseAccounts.push(new SmallGenericType(ProductAccountType.Purchase, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim).toString()));
            this.productBaseAccounts.push(new SmallGenericType(ProductAccountType.Sales, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesVat).toString()));
            this.productBaseAccounts.push(new SmallGenericType(ProductAccountType.VAT, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1).toString()));
            this.productBaseAccounts.push(new SmallGenericType(ProductAccountType.SalesNoVat, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesNoVat).toString()));
            this.productBaseAccounts.push(new SmallGenericType(ProductAccountType.SalesContractor, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatSales).toString()));

            // Base accounts for stock
            this.stockBaseAccounts = [];
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockIn, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockIn).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockInChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockInChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockOut, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockOut).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockOutChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockOutChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockInv, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockInventory).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockInvChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockInventoryChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockLoss, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockLoss).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockLossChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockLossChange).toString()));
            this.stockBaseAccounts.push(new SmallGenericType(ProductAccountType.StockTransferChange, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountStockTransferChange).toString()));

            // Settings for copying
            this.copyProductPrices = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCopyProductPrices, this.copyProductPrices);
            this.copyProductAccounts = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCopyProductAccounts, this.copyProductAccounts);
            this.copyProductStock = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCopyProductStock, this.copyProductStock);
        });
    }

    private loadSettingTypes() {        
        this.productAccountSettingTypes = [];
        this.productAccountSettingTypes.push(new SmallGenericType(ProductAccountType.Purchase, this.terms["billing.products.products.accountingsettingtype.receivables"]));
        this.productAccountSettingTypes.push(new SmallGenericType(ProductAccountType.Sales, this.terms["billing.products.products.accountingsettingtype.sales"]));
        this.productAccountSettingTypes.push(new SmallGenericType(ProductAccountType.VAT, this.terms["billing.products.products.accountingsettingtype.vat"]));
        this.productAccountSettingTypes.push(new SmallGenericType(ProductAccountType.SalesNoVat, this.terms["billing.products.products.accountingsettingtype.salesnovat"]));
        this.productAccountSettingTypes.push(new SmallGenericType(ProductAccountType.SalesContractor, this.terms["billing.products.products.accountingsettingtype.salescontractor"]));

        this.stockAccountSettingTypes = [];
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockIn, this.terms["billing.products.products.stockaccountsettingtype.stockin"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockInChange, this.terms["billing.products.products.stockaccountsettingtype.stockinchange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockOut, this.terms["billing.products.products.stockaccountsettingtype.stockout"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockOutChange, this.terms["billing.products.products.stockaccountsettingtype.stockoutchange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockInv, this.terms["billing.products.products.stockaccountsettingtype.stockinv"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockInvChange, this.terms["billing.products.products.stockaccountsettingtype.stockinvchange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockLoss, this.terms["billing.products.products.stockaccountsettingtype.stockloss"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockLossChange, this.terms["billing.products.products.stockaccountsettingtype.stocklosschange"]));
        this.stockAccountSettingTypes.push(new SmallGenericType(ProductAccountType.StockTransferChange, this.terms["billing.products.products.stockaccountsettingtype.stocktransferchange"]));
    }

    private loadExtraFields(): ng.IPromise<any> {
        return this.coreService.getExtraFields(SoeEntityType.InvoiceProduct).then(x => {
            this.extraFields = x;
        });
    }

    private loadCommodityCodes(): ng.IPromise<any> {
        return this.productService.getCustomerCommodityCodesDict(true).then(x => {
            this.commodityCodes = x;
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(true, false).then(x => {
            this.countries = x;
        });
    }

    // LOOKUPS
    private onLoadData() {
        return this.progress.startLoadingProgress([
            () => this.load()
        ]);
    }

    private load(): ng.IPromise<any> {

        const deferral = this.$q.defer();

        if (this.productId > 0) {

            this.productService.getInvoiceProduct(this.productId).then((x) => {
                this.product = x;
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["billing.products.product"] + ' ' + this.product.name);
                
                if (this.isExternal != this.product.isExternal) 
                    this.isExternal = this.product.isExternal;

                this.active = (this.product.state == SoeEntityState.Active);
                this.showCalculatedCost = (this.product.vatType == TermGroup_InvoiceProductVatType.Service.valueOf());
                this.showGuaranteePercentage = (this.product.calculationType == TermGroup_InvoiceProductCalculationType.Lift);

                this.isExternal = this.product.isExternal;
                const startDate = new Date("1901-01-02");
                const stopDate = new Date("9998-12-31");
                _.forEach(this.product.priceLists, (row: any) => {
                    const rowStart = CalendarUtility.convertToDate(row.startDate);
                    const rowStop = CalendarUtility.convertToDate(row.stopDate);
                    if (rowStart < startDate)
                        row.startDate = null;
                    if (rowStop > stopDate)
                        row.stopDate = null;
                });
                this.priceListRows = this.product.priceLists;
                this.compTermRecordType = CompTermsRecordType.ProductName;
                this.isNew = false;
                this.productAccountingSettings = [];
                this.stockAccountingSettings = [];

                this._selectedProductGroup = this.product.productGroupId ? this.productGroups.find(g => g.productGroupId === this.product.productGroupId) : this.productGroups[0];
                this._selectedCommodityCode = this.product.intrastatCodeId ? _.find(this.commodityCodes, g => g.id === this.product.intrastatCodeId) : undefined;

                _.forEach(this.product.accountingSettings, (row) => {
                    if (row.type == ProductAccountType.Purchase || row.type == ProductAccountType.Sales || row.type == ProductAccountType.VAT ||
                        row.type == ProductAccountType.SalesNoVat || row.type == ProductAccountType.SalesContractor)
                        this.productAccountingSettings.push(row)
                    else
                        this.stockAccountingSettings.push(row);
                });

                this.setCommodityCodeTooltip();

                deferral.resolve();
            });
        }
        else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadAccountPriorityRows() {

        const productAccountingPriorities: any[] = [];

        const array = this.product.accountingPrio.split(',');
        _.forEach(array, (row) => {
            productAccountingPriorities.push({ dimNr: parseInt(row.split('=')[0]), prioNr: row.split('=')[1] });
        });

        this.productAccountPriorityRows = [];

        return this.coreService.getAccountDimsSmall(false, false, false, false, false, false).then((dims: AccountDimSmallDTO[]) => {
            dims.forEach(dim => {
                const productPrio = productAccountingPriorities.find(z => z.dimNr === dim.accountDimNr);
                if (productPrio) {
                    const prio = (_.filter(this.accountingPrios, a => a.id == productPrio.prioNr))[0];
                    this.productAccountPriorityRows.push( { dimNr: dim.accountDimNr, dimName: dim.name, prioNr: productPrio.prioNr, prioName: prio ? prio.name : '' } );
                }
            });
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.products.products.productnotdeletedmessage",
            "billing.products.products.producthasinvoicerowsmessage",
            "billing.products.products.producthasinterestrowsmessage",
            "billing.products.products.producthasreminderrowsmessage",
            "billing.products.products.producthastimecodemessage",
            "billing.products.products.failedsavemessage",
            "billing.products.products.productexistsmessage",
            "billing.products.products.productnotsavedmessage",
            "billing.products.products.productinusemessage",
            "billing.products.products.productpricelistnotsavedmessage",
            "billing.products.products.productcategoriesnotsavedmessage",
            "billing.products.products.productaccountsnotsavedmessage",
            "billing.products.products.translationsnotsavedmessage",
            "billing.products.product.fromstock",
            "billing.products.product.tostock",
            "billing.products.product.amounttomove",
            "billing.products.product.convertedtouserproduct",
            "billing.products.products.productweightinvalidmessage",
            "core.error",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "billing.products.product",
            "billing.products.products.accountingsettingtype.receivables",
            "billing.products.products.accountingsettingtype.sales",
            "billing.products.products.accountingsettingtype.vat",
            "billing.products.products.accountingsettingtype.salesnovat",
            "billing.products.products.accountingsettingtype.salescontractor",
            "billing.products.products.stockaccountsettingtype.stockin",
            "billing.products.products.stockaccountsettingtype.stockinchange",
            "billing.products.products.stockaccountsettingtype.stockout",
            "billing.products.products.stockaccountsettingtype.stockoutchange",
            "billing.products.products.stockaccountsettingtype.stockinv",
            "billing.products.products.stockaccountsettingtype.stockinvchange",
            "billing.products.products.stockaccountsettingtype.stockloss",
            "billing.products.products.stockaccountsettingtype.stocklosschange",
            "billing.products.products.stockaccountsettingtype.stocktransferchange",
            "billing.products.product.isstockproduct",
            "billing.products.product.stocks",
            "common.customer.customer.rot.socialsecnr",
            "common.customer.customer.rot.name",
            "common.customer.customer.rot.property",
            "common.customer.customer.rot.apartmentnr",
            "common.customer.customer.rot.cooperativeorgnr",
            "core.edit",
            "core.delete",
            "common.type",
            "common.invoicenr",
            "common.productnr",
            "common.name",
            "common.quantity",
            "common.amount",
            "common.purchaseprice",
            "common.price",
            "common.date",
            "common.customer.customer.marginalincome",
            "common.customer.customer.marginalincomeratio",
            "billing.productrows.purchasepricesum",
            "common.customerinvoice",
            "common.order",
            "common.offer",
            "common.contract",
        ];
        return this.translationService.translateMany(keys)
            .then(terms => this.terms = terms);
    }

    private loadVatTypes() {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceProductVatType, true, false).then(x => {
            this.vatTypes = x;
        });
    }

    private loadVatCodes() {
        return this.productService.getVatCodes().then(x => {
            this.vatCodes = x;
            // Insert empty row
            this.vatCodes.splice(0, 0, { vatCodeId: 0, name: '', percent: 0 });
        });
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then((x) => {
            this.productUnits = x;
        });
    }

    private loadMaterialCodes() {
        return this.productService.getTimeCodes(SoeTimeCodeType.Material, true, false).then(x => {
            this.materialCodes = x;
            this.materialCodes.splice(0, 0, { id: 0, name: '' });
        });
    }

    private loadProductGroups() {
        return this.productService.getProductGroups(true).then((x:any[]) => {
            if (x) {
                this.productGroups = x;
                this.productGroups.forEach((y) => {
                    if (y.productGroupId) {
                        y.name = y.code + ' - ' + y.name;
                    }
                });
            }
        });
    }

    private loadproductUnitConverts(): ng.IPromise<any> {
        return this.productService.getProductUnitConverts(this.productId, false).then((x) => {
            this.productUnitConverts = x;
        });
    }

    private loadCalculationTypes() {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceProductCalculationType, true, false).then(x => {
            this.calculationTypes = x;
        });
    }

    private loadHouseholdDeductionTypes() {
        return this.productService.getHouseholdDeductionTypes(true).then(x => {
            this.householdDeductionTypes = _.orderBy(x, 'name');
        });
    }

    private loadAccountingPriority() {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceProductAccountingPrio, true, false).then(x => {
            this.accountingPrios = x;
        });
    }

    private loadGrossMarginCalculationTypes() {
        return this.coreService.getTermGroupContent(TermGroup.GrossMarginCalculationType, true, false).then(x => {
            this.grossMarginCalculationTypes = x;
        });
    }

    // EVENTS        
    private vatTypeChanged(item) {
        if (item == TermGroup_InvoiceProductVatType.Service)
            this.showCalculatedCost = true
        else
            this.showCalculatedCost = false;
    }

    private calculationTypeChanged(item) {
        if (item == TermGroup_InvoiceProductCalculationType.Lift)
            this.showGuaranteePercentage = true
        else
            this.showGuaranteePercentage = false;
    }

    private weightBlur(item) {
        if (this.product.weight != null && this.product.weight < 0) {
            this.notificationService.showDialog(this.terms["core.ok"], this.terms["billing.products.products.productweightinvalidmessage"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        }
    }

    // ACTIONS
    private convertToUserProduct() {
        if (this.productId > 0 && this.product.isExternal) {
            this.product.isExternal = this.isExternal = false;
            this.dirtyHandler.setDirty();
            this.notificationService.showDialog(this.terms["core.ok"], this.terms["billing.products.product.convertedtouserproduct"], SOEMessageBoxImage.OK, SOEMessageBoxButtons.OK);
        }
    }

    private openProductInfo() {
        this.productService.getProductExternalUrl([this.productId]).then(urls => {
            urls.forEach(url => {
                window.open(url, '_blank');
            })
        });
    }

    private save() {

        this.progress.startSaveProgress((completion) => {
            //state
            this.product.state = (this.active) ? SoeEntityState.Active : SoeEntityState.Inactive;

            //categories         
            this.categoryRecords = [];
            _.forEach(this.product.categoryIds, (id) => {
                this.categoryRecords.push({
                    categoryId: id,
                    default: false,
                });
            });

            //accounting priorities
            let prioString: any = "";
            _.forEach(this.productAccountPriorityRows, (row) => {
                prioString += row.dimNr + "=" + row.prioNr + ",";
            });
            this.product.accountingPrio = prioString.substring(0, prioString.length - 1);

            //accounting settings                        
            const mergedAccountingSettings = this.productAccountingSettings.concat(this.stockAccountingSettings);
            this.product.accountingSettings = mergedAccountingSettings;

            // Check vat code
            if (this.product.vatCodeId === 0)
                this.product.vatCodeId = null;

            this.productService.saveInvoiceProduct(this.product, this.priceListRows.filter(x=>x["isModified"]), this.categoryRecords, this.productStockHelper.stocksForProduct, this.compTermRows, _.filter(this.extraFieldRecords, (r) => r.isModified === true)).then((result) => {
                if (result.success) {
                    //save if productUnitConvert have changed...
                    const modifiedUnitConverts = this.productUnitConverts.filter(x => x.isModified);
                    if (modifiedUnitConverts && modifiedUnitConverts.length > 0) {
                        this.productService.saveProductUnitConvert(this.productUnitConverts).then((result: IActionResult) => {
                            if (result.success) {
                                this.loadproductUnitConverts()
                            }
                            else {
                                this.notificationService.showDialogEx(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                this.dirtyHandler.setDirty();
                            }

                        });
                    }

                    if (result.integerValue && result.integerValue > 0) {
                        this.isNew = false;
                        if (this.productId == 0) {
                            if (this.productIds) {
                                this.productIds.push(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.productId = result.integerValue;
                        this.product.productId = result.integerValue;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.product);
                    }
                       

                    if (this.extraFieldsExpanderRendered)
                        this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.productId });

                    if (this.stockExpanderRendered) {
                        //reload to get fresh data...
                        this.productStockHelper.loadStocksForProduct(this.productId);
                    }
                } else {
                    let message = "";
                    switch (result.errorNumber) {
                        case ActionResultSave.ProductExists:
                            message = this.terms["billing.products.products.productexistsmessage"];
                            break;
                        case ActionResultSave.ProductNotSaved:
                            message = this.terms["billing.products.products.productnotsavedmessage"];
                            break;
                        case ActionResultSave.ProductInUse:
                            message = this.terms["billing.products.products.productinusemessage"];
                            break;
                        case ActionResultSave.ProductPriceListNotSaved:
                            message = this.terms["billing.products.products.productpricelistnotsavedmessage"];
                            break;
                        case ActionResultSave.ProductCategoriesNotSaved:
                            message = this.terms["billing.products.products.productcategoriesnotsavedmessage"];
                            break;
                        case ActionResultSave.ProductAccountsNotSaved:
                            message = this.terms["billing.products.products.productaccountsnotsavedmessage"];
                            break;
                        case ActionResultSave.TranslationsSaveFailed:
                            message = this.terms["billing.products.products.translationsnotsavedmessage"];
                            break;
                        case ActionResultSave.ProductWeightInvalid:
                            message = this.terms["billing.products.products.productweightinvalidmessage"];
                            break;
                        default:
                            message = this.terms["billing.products.products.failedsavemessage"];
                            break;
                    }

                    if (result.errorMessage) {
                        message = message + '\n' + result.errorMessage;
                    }

                    completion.failed(message);
                }
            })
        }, this.guid).then(data => {
            this.dirtyHandler.clean();

            if (this.isModal)
                this.modal.close({ productId: this.productId, number: this.product.number, name: this.product.name, productUnitId: this.product.productUnitId });
            else
                this.onLoadData();
        }, error => {

        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.productIds = [];
        this.productId = selectedRecord;
        this.product.productId = selectedRecord;
        this.productService.getInvoiceProductsForGrid(false, true, false, true, true).then(data => {
            _.forEach(data, (row) => {
                this.productIds.push(row.productId);
            });
        });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.productService.deleteProduct(this.productId).then((result) => {
                if (result.success) {
                    completion.completed(this.product);
                    super.closeMe(true);
                } else {
                    var message: string = result.errorMessage;
                    if (result.errorNumber == ActionResultDelete.ProductNotDeleted)
                        message = this.terms["billing.products.products.productnotdeletedmessage"];
                    if (result.errorNumber == ActionResultDelete.ProductHasInvoiceRows)
                        message = this.terms["billing.products.products.producthasinvoicerowsmessage"];
                    if (result.errorNumber == ActionResultDelete.ProductHasInterestRows)
                        message = this.terms["billing.products.products.producthasinterestrowsmessage"];
                    if (result.errorNumber == ActionResultDelete.ProductHasReminderRows)
                        message = this.terms["billing.products.products.producthasreminderrowsmessage"];
                    if (result.errorNumber == ActionResultDelete.ProductHasTimeCodes)
                        message = this.terms["billing.products.products.producthastimecodemessage"]; 
                    completion.failed(message);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    public showValidationError() {
        if (this.product) {
            this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
                if (!this.product.number)
                    mandatoryFieldKeys.push("billing.product.number");
                if (!this.product.name)
                    mandatoryFieldKeys.push("billing.product.name");
            });
        }
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.product = new InvoiceProductDTO;
        this.product.vatType = this.defaultVatType;
        this.product.timeCodeId = this.defaultTimeCodeId;
        this.product.productUnitId = this.defaultProductUnitId;
        this.product.vatCodeId = this.defaultVatCodeId;
        this.product.householdDeductionType = this.defaultHouseholdDeductionType;
        this.product.accountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0";
        this.categoryRecords = [];
        this.compTermRows = [];
        this.loadAccountPriorityRows();
        this.isExternal = false;
        this.compTermRecordType = CompTermsRecordType.ProductName;

        if (this.presetProductNumber)
            this.product.number = this.presetProductNumber;
    }
    /*public onAccountSettingsExpanderOpen() {
        if (!this.accountSettingsExpanderRendered) {
            this.accountSettingsExpanderRendered = true;
            this.$timeout(() => {
                this.loadAccountPriorityRows();
            }, 50);
        }
    }

    public onStockExpanderOpen() {
        if (!this.stockExpanderRendered) {
            this.stockExpanderRendered = true;
            this.$timeout(() => {
                this.productStockHelper.onRenderStockExpander(this.productId);
            },50);
        }
    } */
    protected copy() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Products/Products/Dialogs/CopyProduct/CopyProduct.html"),
            controller: CopyProductController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                coreService: () => { return this.coreService },
                copyPrices: () => { return this.copyProductPrices },
                copyAccounts: () => { return this.copyProductAccounts; },
                copyStock: () => { return this.copyProductStock }
            }
        });

        modal.result.then(result => {
            if (result) {
                if (result.copyPrices) {
                    _.forEach(_.filter(this.product.priceLists, (x) => x.stopDate && x.stopDate < CalendarUtility.getDateToday()), (p) => {
                        p.productId = undefined;
                    });
                }
                else {
                    this.product.purchasePrice = 0;
                    this.product.priceLists = [];
                    this.priceListRows = [];
                }

                if (!result.copyAccounts)
                    this.productAccountingSettings = [];

                if (result.copyStock) {
                    if (!this.stockExpanderRendered) {
                        this.productStockHelper.loadStocksForProduct(this.productId).then(() => {
                            this.stockExpanderRendered = true;
                            var temp = [];
                            _.forEach(this.productStockHelper.stocksForProduct, (s) => {
                                s.stockProductId = undefined;
                                s.saldo = 0;
                                s.avgPrice = 0;
                                s.stockShelfId = 0;
                                s.stockShelfName = undefined;
                                temp.push(s);
                            });
                            this.productStockHelper.stocksForProduct = temp;
                        });
                    }
                    else {
                        var temp = [];
                        _.forEach(this.productStockHelper.stocksForProduct, (s) => {
                            s.stockProductId = undefined;
                            s.saldo = 0;
                            s.avgPrice = 0;
                            s.stockShelfId = 0;
                            s.stockShelfName = undefined;
                            temp.push(s);
                        });
                        this.productStockHelper.stocksForProduct = temp;
                    }
                }
                else {
                    this.product.isStockProduct = false;
                    this.productStockHelper.stocksForProduct = [];
                    this.stockAccountingSettings = [];
                }

                this.isNew = true;
                this.productId = 0;
                this.product.productId = 0;
                this.product.number = "";
                this.product.ean = "";
                this.product.isExternal = false;

                this.extraFieldRecords = [];
                this.compTermRows = [];

                // Update settings
                this.coreService.saveBoolSetting(SettingMainType.Company, CompanySettingType.BillingCopyProductPrices, result.copyPrices);
                this.coreService.saveBoolSetting(SettingMainType.Company, CompanySettingType.BillingCopyProductAccounts, result.copyAccounts);
                this.coreService.saveBoolSetting(SettingMainType.Company, CompanySettingType.BillingCopyProductStock, result.copyStock);
            }
        });

    }

    // VALIDATION

    // ACTIONS      

    //Start Statisticsgrid
    private setupStatisticsSubGrid() { 
        this.gridHandler.gridAg.addColumnDate("date", this.terms["common.date"], null, null, null, { enableRowGrouping: true });
        this.gridHandler.gridAg.addColumnText("invoiceNr", this.terms["common.invoicenr"], null, null, { enableRowGrouping: true });
        this.gridHandler.gridAg.addColumnNumber("productQuantity", this.terms["common.quantity"], null, { enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
        this.gridHandler.gridAg.addColumnNumber("productPrice", this.terms["common.price"], null, { enableRowGrouping: true })
        this.gridHandler.gridAg.addColumnNumber("productSumAmount", this.terms["common.amount"], null, { enableRowGrouping: true, aggFuncOnGrouping: 'sum' })
        this.gridHandler.gridAg.addColumnNumber("productPurchasePrice", this.terms["common.purchaseprice"], null, { enableRowGrouping: true });
        this.gridHandler.gridAg.addColumnNumber("productPurchaseAmount", this.terms["billing.productrows.purchasepricesum"], null, { enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
        this.gridHandler.gridAg.options.useGrouping(true, true, { keepGroupState: true });

        this.gridHandler.gridAg.options.enableGridMenu = true;
        this.gridHandler.gridAg.options.setMinRowsToShow(20);

        this.gridHandler.gridAg.addStandardMenuItems();
        this.gridHandler.gridAg.finalizeInitGrid("common.statistics", true);
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadOriginTypes(): ng.IPromise<any> {

        return this.coreService.getTermGroupContent(TermGroup.OriginType, true, false).then((x) => {
            this.originTypes = [];
            _.forEach(x, (row) => {
                if (row.id === SoeOriginType.CustomerInvoice ||
                    row.id === SoeOriginType.Order ||
                    row.id === SoeOriginType.Offer ||
                    row.id === SoeOriginType.Contract)
                    this.originTypes.push({ value: row.id, label: row.name });
                if (row.id === SoeOriginType.CustomerInvoice)
                    this.selectedOriginType = row.id;
            });
        });
    }

    private loadProductStatistics() {
        this.progress.startLoadingProgress([
            () => this.productService.getProductStatistics(this.productId, this.selectedOriginType, this.allItemsSelection).then((x) => {
                _.forEach(x, (y) => {
                    switch (y.originType) {
                        case SoeOriginType.CustomerInvoice:
                            y.originType = this.terms["common.customerinvoice"];
                            break;
                        case SoeOriginType.Order:
                            y.originType = this.terms["common.order"];
                            break;
                        case SoeOriginType.Offer:
                            y.originType = this.terms["common.offer"];
                            break;
                        case SoeOriginType.Contract:
                            y.originType = this.terms["common.contract"];
                            break;
                    }
                });

                this.gridHandler.gridAg.setData(x);
            })
        ]);
    }

    protected addSumAggregationFooterToColumns(numberWithoutDecimals: boolean, ...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            if (numberWithoutDecimals)
                col.footerCellFilter = 'number:0';
            else
                col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }

    public onPriceExpanderOpen() {
        this.priceExpanderRendered = true;
    }

    public onAccountSettingsExpanderOpen() {
        if (!this.accountSettingsExpanderRendered) {
            this.accountSettingsExpanderRendered = true;
            this.$timeout(() => {
                this.loadAccountPriorityRows();
            }, 50);
        }
    }

    public onStockExpanderOpen() {
        if (!this.stockExpanderRendered) {
            this.stockExpanderRendered = true;
            this.$timeout(() => {
                this.productStockHelper.onRenderStockExpander(this.productId);
            },50);
        }
    }

    public onSupplierProductsExpanderOpen() {
        if (!this.supplierProductsExpanderRendered) {
            this.supplierProductsExpanderRendered = true;
        }
    }

    public onStatisticsExpanderOpen() {
        this.statisticsExpanderRendered = true;
    }

    public onExtraFieldsExpanderOpen() {
        if (!this.extraFieldsExpanderRendered) {
            this.extraFieldRecords = [];
            this.extraFieldsExpanderRendered = true;
        }
    }

    public onTranslationsExpanderOpen() {
        this.translationsExpanderRendered = true;
    }

    public setCommodityCodeTooltip() {
        this.$timeout(() => {
            this.commodityCodeTooltip = this._selectedCommodityCode ? this._selectedCommodityCode.text as string : "TEXT";;
        });
    }
}