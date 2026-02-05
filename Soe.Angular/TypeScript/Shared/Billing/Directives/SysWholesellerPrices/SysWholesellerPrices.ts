import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { InvoiceProductPriceSearchViewDTO } from "../../../../Common/Models/ProductDTOs";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IProductService } from "../../Products/ProductService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { SoeSysPriceListProviderType, Feature, PriceListOrigin, TermGroup } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { GridControllerBaseAg } from "../../../../Core/Controllers/GridControllerBaseAg";


export class SysWholesellerPricesFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Directives/SysWholesellerPrices/SysWholesellerPrices.html'),
            scope: {
                priceListTypeId: '=',
                customerId: '=',
                currencyId: '=',
                sysWholesellerId: '=',
                number: '=',
                providerType: '=',
                selectedPrice: '='
            },
            restrict: 'E',
            replace: true,
            controller: SysWholesellerPricesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class SysWholesellerPricesController extends GridControllerBaseAg {

    // Setup
    private priceListTypeId: number;
    private customerId: number;
    private currencyId: number;
    private sysWholesellerId: number;
    private number: string;
    private providerType: SoeSysPriceListProviderType;
    private selectedPrice: InvoiceProductPriceSearchViewDTO;

    // Permissions
    private showPurchasePricePermission: boolean = true;
    private showSalesPricePermission: boolean = true;

    // Collections
    private terms: any;
    private prices: InvoiceProductPriceSearchViewDTO[];
    private syswholesellerTypes: any[] = [];

    private searching: boolean = false;
    private searchingMessage: string;
    private searchingErrorMessage: string;
    private compPriceExist = false;

    //@ngInject
    constructor($http,
        $templateCache,
        $uibModal,
        $timeout: ng.ITimeoutService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private productService: IProductService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super("Common.Directives.SysWholesellerPrices", "", Feature.Economy_Supplier_Invoice_Invoices_Edit, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.SearchInvoiceProduct.Prices", $timeout);

        this.init();
    }

    public init() {
        this.$q.all([this.loadTerms(),
            this.loadSysWholesellerTypes(),
            this.loadReadOnlyPermissions()]).then(() => this.setupGridColumns());
    }

    private loadSysWholesellerTypes(): ng.IPromise<any> {
        this.syswholesellerTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.SysWholesellerType, false, false).then(x => {
            this.syswholesellerTypes = x;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.syswholesellerprices.wholeseller",
            "common.syswholesellerprices.gnp",
            "common.syswholesellerprices.nettonetto",
            "common.syswholesellerprices.customerprice",
            "common.syswholesellerprices.marginalincome",
            "common.syswholesellerprices.marginalincomeratio",
            "common.syswholesellerprices.providertype",
            "common.syswholesellerprices.purchaseunit",
            "common.syswholesellerprices.searchingprices",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.searchingMessage = this.terms["common.syswholesellerprices.searchingprices"];
        });
    }

    public setupGridColumns() {
        this.soeGridOptions.autoHeight = true;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.ignoreResetFilterModel = true;
        this.soeGridOptions.enableSingleSelection();
        this.soeGridOptions.setMinRowsToShow(4);

        this.soeGridOptions.addColumnText("wholeseller", this.terms["common.syswholesellerprices.wholeseller"], null);
        this.soeGridOptions.addColumnNumber("gnp", this.terms["common.syswholesellerprices.gnp"], null, { enableHiding: false, decimals: 2});
        this.soeGridOptions.addColumnNumber("nettoNettoPrice", this.terms["common.syswholesellerprices.nettonetto"], null, { enableHiding: false, decimals: 2, toolTipField: "code" });
        this.soeGridOptions.addColumnNumber("customerPrice", this.terms["common.syswholesellerprices.customerprice"], null, { enableHiding: false, decimals: 2, toolTipField: "priceFormula" }); 
        this.soeGridOptions.addColumnNumber("marginalIncome", this.terms["common.syswholesellerprices.marginalincome"], null, { enableHiding: false, decimals: 2 });
        this.soeGridOptions.addColumnNumber("marginalIncomeRatio", this.terms["common.syswholesellerprices.marginalincomeratio"], null, { enableHiding: false, decimals: 2 });
        this.soeGridOptions.addColumnText("productProviderTypeText", this.terms["common.syswholesellerprices.providertype"], null);
        this.soeGridOptions.addColumnText("purchaseUnit", this.terms["common.syswholesellerprices.purchaseunit"], null);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
            this.selectedPrice = row.entity;
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: any[]) => {
            if (rows.length > 0) {
                this.$timeout(() => {
                    this.selectedPrice = rows[0];
                }, 10);
            }
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        this.soeGridOptions.finalizeInitGrid();

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.number, () => {
            this.loadPrices();
        });
        this.$scope.$watch(() => this.providerType, () => {
            if (!this.searching)
                this.loadPrices();
        });
        this.$scope.$watch(() => this.priceListTypeId, () => {
            if (!this.searching)
                this.loadPrices();
        });
    }

    // Lookups

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Product_Products_ShowPurchasePrice,
            Feature.Billing_Product_Products_ShowSalesPrice
        ];
        
        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.showPurchasePricePermission = x[Feature.Billing_Product_Products_ShowPurchasePrice];
            this.showSalesPricePermission = x[Feature.Billing_Product_Products_ShowSalesPrice];
        });
    }

    private loadPrices() {
        if (!this.number)
            return;

        this.searching = true;

        this.productService.searchInvoiceProductPrices(this.priceListTypeId, this.customerId, this.currencyId, this.number, this.providerType).then((data: any[]) => {
            
            this.prices = data.map(p => {
                var obj = new InvoiceProductPriceSearchViewDTO();
                angular.extend(obj, p);
                return obj;
            });

            if (this.sysWholesellerId) {
                const indexToFirst = this.prices.findIndex(r => r.sysWholesellerId === this.sysWholesellerId);
                if (indexToFirst >= 0) {
                    const objectToFirst = this.prices[indexToFirst];
                    this.prices.splice(indexToFirst, 1);
                    this.prices.unshift(objectToFirst);
                }
            }

            this.prices.forEach( (y) => {
                if (y.priceListOrigin == PriceListOrigin.CompDbPriceList) {
                    y.wholeseller = y.wholeseller + " (N*)";
                    this.compPriceExist = true;
                }

                if (y.productProviderType) {
                    const type = this.syswholesellerTypes.find(x => y.productProviderType === x.id);
                    if (type) {
                        y.productProviderTypeText = type.name;
                    }
                }
            });

            this.soeGridOptions.setData(this.prices);
            this.searching = false;
            if (this.prices.length > 0) {
                this.$timeout(() => {
                    this.soeGridOptions.selectRowByVisibleIndex(0, true);
                    this.soeGridOptions.setMinRowsToShow(this.prices.length > 3 ? this.prices.length + 1 : 4);
                });
            }
        });
    }

    protected edit(row) {
        // Double click price will select it and close the dialog
        this.messagingService.publish(Constants.EVENT_PRODUCT_PRICE_SELECTED, { price: row });
    }
}
