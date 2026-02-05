import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IInvoiceProductSearchViewDTO, IPriceListTypeDTO } from "../../../../Scripts/TypeLite.Net4";
import { InvoiceProductPriceSearchViewDTO, ProductSearchResult } from "../../../../Common/Models/ProductDTOs";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IProductService } from "../../../../Shared/Billing/Products/ProductService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { Constants } from "../../../../Util/Constants";
import { CompanySettingType, TermGroup_InitProductSearch, UserSettingType } from "../../../../Util/CommonEnumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { IShortCutService } from "../../../../Core/Services/ShortCutService";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { ProductImageCellRenderer } from "./ProductImageAgCellRenderer";

export class SearchInvoiceProductController {

    // Company settings
    private useAutoSearch: boolean = false;

    // Terms
    private terms: any;
    private title: string;

    // Search
    private loadProductsTimeout: any;
    private searchingProducts: boolean = false;
    private searchingProductsMessage: string;
    private searchingProductsErrorMessage: string;
    private showPriceListSelect: boolean = false;
    private productSearchMinPrefixLength: number = 2;
    private productSearchMinPopulateDelay: number = 100;
    private numberOfSearches: number = 0;
    private useExtendSearchInfo = false;
    private searchText: string;
    private firstTierCategory: any;
    private secondTierCategory: any;

    private soeGridOptions: ISoeGridOptionsAg;
    private products: IInvoiceProductSearchViewDTO[];
    private selectedProduct: IInvoiceProductSearchViewDTO;
    private selectedPrice: InvoiceProductPriceSearchViewDTO;
    private pricelists: IPriceListTypeDTO[];
    private firstTierCategories: any[];
    private secondTierCategoriesAll: any[];
    private secondTierCategoriesFiltered: any[];

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private productService: IProductService,
        shortCutService: IShortCutService,
        private $q: ng.IQService,
        private hideProducts: boolean,
        private priceListTypeId: number,
        private customerId: number,
        private currencyId: number,
        private sysWholesellerId: number,
        private number: string,
        private name: string,
        private quantity: number,
        info: string) {

        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.SearchInvoiceProduct.Product", $timeout);

        // Events
        this.messagingService.subscribe(Constants.EVENT_PRODUCT_PRICE_SELECTED, (item) => {
            this.close(item ? item.price : this.selectedPrice);
        });

        this.searchText = this.number ? this.number : this.name;

        this.$q.all([
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadTerms(),
            this.loadPriceLists()]).then(() => {
                if (this.useExtendSearchInfo)
                    this.loadProductGroups();

                this.setupGrid();

                if (this.searchText && this.searchText.length >= this.productSearchMinPrefixLength && this.useAutoSearch)
                    this.buttonSearchClick();
            });

        shortCutService.bindShortCut($scope, "enter", () => this.buttonSearchClick());
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.BillingInitProductSearch);
        settingTypes.push(CompanySettingType.BillingShowExtendedInfoInExternalSearch);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            var searchType: TermGroup_InitProductSearch = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingInitProductSearch, TermGroup_InitProductSearch.WithEnter);
            this.useAutoSearch = (searchType === TermGroup_InitProductSearch.Automatic);
            this.useExtendSearchInfo = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingShowExtendedInfoInExternalSearch, false);//true; 
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(UserSettingType.BillingProductSearchMinPrefixLength);
        settingTypes.push(UserSettingType.BillingProductSearchMinPopulateDelay);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.productSearchMinPrefixLength = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchMinPrefixLength, this.productSearchMinPrefixLength);
            this.productSearchMinPopulateDelay = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchMinPopulateDelay, this.productSearchMinPopulateDelay);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.info",
            "common.searchinvoiceproduct.searchingproducts",
            "common.number",
            "common.name",
            "common.searchinvoiceproduct.limitreached",
            "common.searchinvoiceproduct.timeout",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.searchinvoiceproduct.showexternalproductinfo",
            "common.manufacturer",
            "common.all",
            "common.searchinvoiceproduct.imagemissing",
            "common.stopdate",
            "common.searchinvoiceproduct.discontinued",
        ];

        if (this.hideProducts)
            keys.push("common.searchinvoiceproduct.selectwholeseller");
        else
            keys.push("common.searchinvoiceproduct.searchproduct");

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.title = this.hideProducts ? this.terms["common.searchinvoiceproduct.selectwholeseller"] : this.terms["common.searchinvoiceproduct.searchproduct"];
            this.searchingProductsMessage = this.terms["common.searchinvoiceproduct.searchingproducts"];
        });
    }

    private setupGrid() {
        if (this.useExtendSearchInfo)
            this.soeGridOptions.setRowHeight(40);

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.ignoreResetFilterModel = true;
        this.soeGridOptions.enableSingleSelection();
        this.soeGridOptions.setMinRowsToShow(15);

        const imagecol = this.soeGridOptions.addColumnIcon("image", null, 45, {
            suppressFilter: true,
            hide: !this.useExtendSearchInfo,
        })
        imagecol.cellRenderer = ProductImageCellRenderer;

        this.soeGridOptions.addColumnText("number", this.terms["common.number"], 50, { suppressFilterUpdate: true });
        this.soeGridOptions.addColumnText("name", this.terms["common.name"], 150, { suppressFilterUpdate: true });
        this.soeGridOptions.addColumnText("extendedInfo", this.terms["core.info"], null, { suppressFilterUpdate: true, hide: !this.useExtendSearchInfo });
        this.soeGridOptions.addColumnText("manufacturer", this.terms["common.manufacturer"], 100, { suppressFilterUpdate: true, hide: !this.useExtendSearchInfo });
        this.soeGridOptions.addColumnIcon("endAtIcon", "...", null, { suppressFilter: true, toolTipField: "endAtTooltip", showIcon: (row) => row.endAt });

        const toolTip = this.terms["common.searchinvoiceproduct.showexternalproductinfo"];
        const weblinkcol = this.soeGridOptions.addColumnIcon("weblink", null, null, { suppressFilter: true, showIcon: (row) => row.type === 2, toolTip: toolTip })
        weblinkcol.cellRenderer = function (params) {
            if (params.data?.externalUrl) {
                return '<a target="_blank" title="' + toolTip + '" href="' + params.data.externalUrl + '"><span class="gridCellIcon fal fa-arrow-up-right-from-square"></span></a>'
            }
        }

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        this.soeGridOptions.finalizeInitGrid();

        // Grid events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
            this.$timeout(() => {
                this.selectedProduct = row;
            });
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: any[]) => {
            if (rows.length > 0) {
                this.$timeout(() => {
                    this.selectedProduct = rows[0];
                });
            }
        }));
        this.soeGridOptions.subscribe(events);
    }

    private filterNumberOnChanging(userinput) {
        this.number = userinput ? userinput : "";

        if (this.useAutoSearch)
            this.loadProducts();
    }

    private filterNameOnChanging(userinput) {
        this.name = userinput ? userinput : "";

        if (this.useAutoSearch)
            this.loadProducts();
    }

    private loadProducts() {
        this.searchingProducts = true;
        this.products = [];
        this.searchingProductsErrorMessage = '';
        this.selectedPrice = null;
        this.selectedProduct = null;

        this.productService.searchInvoiceProducts(this.number ? this.number : "null", this.name ? this.name : "null").then((x) => {
            this.numberOfSearches = this.numberOfSearches - 1;
            if (this.numberOfSearches > 0)
                return;

            this.products = x;
            this.soeGridOptions.setData(this.products);
            this.searchingProducts = false;

            // Select first row
            if (this.products.length > 0) {
                if (this.hideProducts)
                    this.selectedProduct = this.products[0];
                else {
                    this.$timeout(() => {
                        this.soeGridOptions.selectRowByVisibleIndex(0, true);
                    });
                }
            }

            if (this.products.length === 100)
                this.searchingProductsErrorMessage = this.terms["common.searchinvoiceproduct.limitreached"];
        }).catch((reason: any) => {
            this.searchingProductsErrorMessage = this.terms["common.searchinvoiceproduct.timeout"];
        });
    }

    private buttonSearchClick() {
        this.searchingProducts = true;
        this.products = [];
        this.searchingProductsErrorMessage = '';
        this.selectedPrice = null;
        this.selectedProduct = null;

        if (!this.useExtendSearchInfo) 
            this.name = this.searchText;

        let group = "null";
        if (this.secondTierCategory && this.secondTierCategory.sysProductGroupId > 0)
            group = this.secondTierCategory.identifier;
        else if (this.firstTierCategory && this.firstTierCategory.sysProductGroupId > 0)
            group = this.firstTierCategory.identifier;

        this.productService.searchInvoiceProductsExtended("null", "null", group, this.searchText ? this.searchText : "null").then((x) => {
            this.numberOfSearches = this.numberOfSearches - 1;
            if (this.numberOfSearches > 0)
                return;

            this.products = x;

            this.soeGridOptions.setData(this.products);
            this.searchingProducts = false;

            // Select first row
            if (this.products.length > 0) {
                if (this.hideProducts)
                    this.selectedProduct = this.products[0];
                else {
                    this.$timeout(() => {
                        this.soeGridOptions.selectRowByVisibleIndex(0, true);
                    });
                }
            }

            if (this.products.length === 100)
                this.searchingProductsErrorMessage = this.terms["common.searchinvoiceproduct.limitreached"];
        }).catch((reason: any) => {
            this.searchingProductsErrorMessage = this.terms["common.searchinvoiceproduct.timeout"];
        });
    }

    private loadPriceLists(): ng.IPromise<any> {
        return this.productService.getPriceLists().then(x => {
            this.pricelists = x;
            if (this.priceListTypeId == 0)
                this.showPriceListSelect = true;
        });
    }

    private loadProductGroups(): ng.IPromise<any> {
        return this.productService.getVVSProductGroupsForSearch().then(x => {
            this.firstTierCategories = _.filter(x, (y) => !y.parentSysProductGroupId);
            this.firstTierCategories.splice(0, 0, { sysProductGroupId: 0, parentSysProductGroupId: null, identifier: '', name: this.terms["common.all"], percent: 0 });
            this.firstTierCategory = this.firstTierCategories[0];

            this.secondTierCategoriesAll = _.filter(x, (y) => y.parentSysProductGroupId > 0);
            this.secondTierCategoriesFiltered = [];
            this.secondTierCategory = null;
        });
    }

    private firstTierCategoryChanging() {
        this.$timeout(() => {
            if (this.firstTierCategory.sysProductGroupId === 0) {
                this.secondTierCategoriesFiltered = [];
                this.secondTierCategory = null;
            }
            else {
                this.secondTierCategoriesFiltered = _.filter(this.secondTierCategoriesAll, (y) => y.parentSysProductGroupId === this.firstTierCategory.sysProductGroupId);
                this.secondTierCategoriesFiltered.splice(0, 0, { sysProductGroupId: 0, parentSysProductGroupId: null, identifier: '', name: this.terms["common.all"], percent: 0 });
                this.secondTierCategory = this.secondTierCategoriesFiltered[0];
            }
        });
    }

    onEnter() {
        this.buttonSearchClick();
    }

    protected edit(row) {
        // Do nothing when double clicking product
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        this.close(this.selectedPrice);
    }

    close(price: InvoiceProductPriceSearchViewDTO) {
        if (!price)
            this.$uibModalInstance.dismiss('cancel');
        else {
            const result = new ProductSearchResult();
            result.productId = price.productId;
            result.priceListTypeId = this.priceListTypeId;
            result.purchasePrice = price.nettoNettoPrice ? price.nettoNettoPrice : 0;
            result.salesPrice = price.customerPrice ? price.customerPrice : 0;
            result.productUnit = price.salesUnit ? price.salesUnit : price.purchaseUnit;
            result.sysPriceListHeadId = price.sysPriceListHeadId;
            result.sysWholesellerName = price.wholeseller;
            result.priceListOrigin = price.priceListOrigin;
            result.quantity = NumberUtility.parseNumericDecimal(this.quantity);
            this.$uibModalInstance.close(result);
        }
    }
}