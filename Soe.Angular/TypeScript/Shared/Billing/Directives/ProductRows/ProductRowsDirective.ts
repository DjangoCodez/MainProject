import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBaseAg } from "../../../../Core/Controllers/GridControllerBaseAg";
import { IContextMenuHandler } from "../../../../Core/Handlers/ContextMenuHandler";
import { ProductRowsContainers, ProductRowsAddRowFunctions, ProductRowsAmountField, AmountEvent, SoeGridOptionsEvent, ProductRowsRowFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize, SOEMessageBoxButton, EditProductRowModes } from "../../../../Util/Enumerations";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { CustomerDTO } from "../../../../Common/Models/CustomerDTO";
import { ISmallGenericType, IAttestTransitionDTO, IProductSmallDTO, IInvoiceProductPriceResult, IInvoiceProductCopyResult, ICustomerProductPriceSmallDTO, IActionResult, IProductUnitSmallDTO } from "../../../../Scripts/TypeLite.Net4";
import { AttestStateDTO } from "../../../../Common/Models/AttestStateDTO";
import { ProductRowsProductDTO, ProductSearchResult, InvoiceProductDTO, ProductSmallDTO, ProductAccountsDTO } from "../../../../Common/Models/ProductDTOs";
import { VatCodeDTO } from "../../../../Common/Models/VatCodeDTO";
import { AccountVatRateViewSmallDTO } from "../../../../Common/Models/AccountDTO";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IProductService } from "../../../../Shared/Billing/Products/ProductService";
import { IStockService } from "../../../Billing/Stock/StockService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IContextMenuHandlerFactory } from "../../../../Core/Handlers/ContextMenuHandlerFactory";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { TypeAheadOptionsAg, IColumnAggregations } from "../../../../Util/SoeGridOptionsAg";
import { ToolBarUtility } from "../../../../Util/ToolBarUtility";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { EditProductRowDialogController } from "./EditProductRowDialogController";
import { TextBlockDialogController } from "../../../../Common/Dialogs/TextBlock/TextBlockDialogController";
import { AmountHelper, AmountHelperEvent } from "./Helpers/AmountHelper";
import { ChangeWholesellerController } from "../../Dialogs/ChangeWholeseller/ChangewholesellerController";
import { CreatePurchaseController } from "../../Dialogs/CreatePurchase/CreatePurchaseController";
import { ChangeDiscountController } from "../../Dialogs/ChangeDiscount/ChangeDiscountController";
import { MoveProductRowsToStockController } from "../../Dialogs/MoveProductRowsToStock/MoveProductRowsToStock";
import { CopyProductRowsController } from "../../Dialogs/CopyProductRows/CopyProductRowsController";
import { SearchInvoiceProductController } from "../../Dialogs/SearchInvoiceProduct/SearchInvoiceProductController";
import { Constants } from "../../../../Util/Constants";
import { TermGroup_Languages, TermGroup_MergeInvoiceProductRows, SoeOriginType, TermGroup_InvoiceVatType, TermGroup_BillingType, TermGroup_EDIPriceSettingRule, TermGroup_ProductSearchFilterMode, SoeEntityState, SoeInvoiceRowType, Feature, TermGroup_CurrencyType, CompanySettingType, UserSettingType, SettingMainType, TermGroup_AttestEntity, SoeInvoiceRowDiscountType, ActionResultSelect, TermGroup_InvoiceProductVatType, SoeEntityType, TextBlockType, SimpleTextEditorDialogMode, TermGroup_InvoiceProductCalculationType, TermGroup_HouseHoldTaxDeductionType, TermGroup, SoeOriginStatus, TermGroup_GrossMarginCalculationType, TermGroup_OrderType, OrderContractType } from "../../../../Util/CommonEnumerations";
import { EditController as ProductsEditController } from "../../Products/Products/EditController";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Guid, StringUtility } from "../../../../Util/StringUtility";
import { TimeRowsHelper } from "../../Helpers/TimeRowsHelper";
import { IOrderService } from "../../Orders/OrderService";
import { SplitAccountingDialogController } from "./SplitAccountingDialogController";
import { SplitAccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { StockDTO } from "../../../../Common/Models/StockDTO";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { ChangeIntrastatCodeController } from "../../Dialogs/ChangeIntrastatCode/ChangeIntrastatCodeController";
import { IntrastatTransactionDTO } from "../../../../Common/Models/CommodityCodesDTO";
import { ProductPricesRequestDTO, ProductPricesRowRequestDTO } from "../../../../Common/Models/pricelistdto";
import { ImportProductRowController } from "./ImportProductRowDialog";
import { ShowCustomerInvoiceInfoController } from "../../Dialogs/ShowCustomerInvoiceInfo/ShowCustomerInvoiceInfoController";

export class ProductRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Directives/ProductRows/Views/ProductRows.html'),
            scope: {
                container: '@',
                readOnly: '=?',
                invoiceId: '=',
                invoiceNr: '=',
                productRows: '=',
                nbrOfVisibleRows: '=',
                nbrOfActiveRows: '=',
                billingType: '=?',
                wholesellerId: '=',
                priceListTypeId: '=',
                priceListTypeInclusiveVat: '=?',
                priceListTypeIsProject: "=?",
                //customer: '=?',
                isCredit: '=?',
                vatType: '=',
                fixedPrice: '=?',
                fixedPriceKeepPrices: '=?',
                freightAmount: '=',
                freightAmountCurrency: '=',
                invoiceFee: '=',
                invoiceFeeCurrency: '=',
                disableInvoiceFee: '=',
                sumAmount: '=',
                sumAmountCurrency: '=',
                vatAmount: '=',
                vatAmountCurrency: '=',
                totalAmount: '=',
                totalAmountCurrency: '=',
                centRounding: '=',
                marginalIncomeCurrency: '=',
                marginalIncomeRatio: '=',
                remainingAmount: '=',
                remainingAmountExVat: '=',
                showRemainingAmountExVat: '=',
                //wholesellers: '=',
                hasHouseholdTaxDeduction: '=',
                isValidToChangeAttestState: '&',
                changeAttestStates: '&',
                loading: "=",
                isCashSale: '=?',
                crediting: '=?',
                copying: '=?',
                parentGuid: '=?',
                hideChangeAttestState: '=',
                tripartiteTrade: '=',
                originStatus: '=?',
                amountHelper: '=?',
                orderInfo: '=?'
            },
            restrict: 'E',
            replace: true,
            controller: ProductRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class ProductRowsController extends GridControllerBaseAg {

    // Helpers
    private amountHelper: AmountHelper;
    private delaySetCurrency: boolean;
    private delayedCurrencyId: number;
    private delayedCurrencyDate: Date;
    private contextMenuHandler: IContextMenuHandler;

    // Init parameters
    private container: ProductRowsContainers;
    private readOnly: boolean;
    private productsLoaded = false;
    private invoiceId: number;
    private invoiceNr: string;
    private productRows: ProductRowDTO[];
    private orderInfo: any;

    //these are set by productRowsChanged function....
    private nbrOfActiveRows = 0;
    private hasHouseholdTaxDeduction = false;
    private nbrOfTransferredRows = 0;
    private nbrOfVisibleRows = 0;

    private billingType: TermGroup_BillingType;
    private wholesellerId: number;
    private priceListTypeId: number;
    private priceListTypeIsProject: boolean;
    private _priceListTypeInclusiveVat = false;

    public get priceListTypeInclusiveVat(): boolean {
        return this._priceListTypeInclusiveVat;
    }
    public set priceListTypeInclusiveVat(value: boolean) {
        if (this._priceListTypeInclusiveVat !== value) {
            this._priceListTypeInclusiveVat = value;
            if (this.amountHelper) {
                this.amountHelper.priceListTypeInclusiveVat = this._priceListTypeInclusiveVat;
                this.reCalculateRowSums();
            }
        }
    }
    private customer: CustomerDTO;
    private customerPriceSet = false;
    public selectedAttestState: any;
    public selectedAttestStateValid = false;

    private _isCredit: boolean;
    public get isCredit(): boolean {
        return this._isCredit;
    }
    public set isCredit(value: boolean) {
        this._isCredit = value;
        if (this.amountHelper)
            this.amountHelper.isCredit = this._isCredit;
    }

    public get currencyId(): number {
        return this.amountHelper ? this.amountHelper.currencyId : 0;
    }

    public get currencyRateDate(): Date {
        return this.amountHelper ? this.amountHelper.currencyRateDate : null;
    }

    public get currencyDate(): Date {
        return this.amountHelper ? this.amountHelper.currencyDate : null;
    }

    private get transactionCurrencyCode(): string {
        return this.amountHelper ? this.amountHelper.transactionCurrencyCode : '';
    };

    private get isBaseCurrency(): boolean {
        return this.amountHelper?.isBaseCurrency ?? true;
    };

    private get baseCurrencyCode(): string {
        return this.amountHelper?.baseCurrencyCode ?? '';
    };

    private get isLedgerCurrency(): boolean {
        return this.amountHelper ? this.amountHelper.isLedgerCurrency : true;
    };

    private get isOrder(): boolean {
        return this.container == ProductRowsContainers.Order;
    }

    private isCashSale: boolean;
    private originType: SoeOriginType;
    private vatType: TermGroup_InvoiceVatType;
    private fixedPrice: boolean;
    private fixedPriceKeepPrices: boolean;
    private freightAmount: number;
    private freightAmountCurrency: number;
    private invoiceFee: number;
    private invoiceFeeCurrency: number;
    private disableInvoiceFee: boolean;
    private sumAmount: number;
    private sumAmountCurrency: number;
    private vatAmount: number;
    private vatAmountCurrency: number;
    private totalAmount: number;
    private totalAmountCurrency: number;
    private centRounding: number;
    private marginalIncomeCurrency: number;
    private marginalIncomeCurrencyToolTip: string;
    private marginalIncomeRatio: number;
    private marginalIncomeRatioToolTip: string;
    private remainingAmount: number;
    private remainingAmountExVat: number;
    private showRemainingAmountExVat: boolean;
    private wholesellers: ISmallGenericType[];
    private tripartiteTrade: boolean;
    private originStatus: SoeOriginStatus;

    private isValidToChangeAttestState: () => boolean;
    private changeAttestStates: (canCreateInvoice: any) => void;

    // Permissions
    private useStock = false;
    private stockMenu = false;
    private warnOnReducedQuantity = false;
    private showPurchasePricePermission = false;
    private showSalesPricePermission = false;
    private contractEditPermission = false;
    private copyProductRowsPermission = false;
    private changePurchasePricePermission = false;
    private mergeProductRowsPermission = false;
    private notAllowedToRemoveRows = false;
    private timeProjectPermission = false;
    private createPurchasePermission = false;
    private purchasePermission = false;
    private intrastatPermission = false;

    // Company settings
    private defaultProductUnitId = 0;
    private defaultProductUnitCode: string;
    private defaultVatCodeId = 0;
    private useFreightAmount = false;
    private freightAmountProductId = 0;
    private useInvoiceFee = false;
    private invoiceFeeProductId = 0
    private invoiceFeeManuallyChanged = false;
    private useInvoiceFeeLimit = false;
    private useInvoiceFeeLimitAmount = 0;
    private useCentRounding = false;
    private centRoundingProductId = 0;
    private miscProductId = 0;
    private fixedPriceProductId = 0;
    private fixedPriceKeepPricesProductId = 0;
    private productGuaranteeId = 0;
    private marginalIncomeLimit = 0;
    private mergeProductRowsMerchandise: TermGroup_MergeInvoiceProductRows = TermGroup_MergeInvoiceProductRows.Never;
    private mergeProductRowsService: TermGroup_MergeInvoiceProductRows = TermGroup_MergeInvoiceProductRows.Never;
    private defaultHouseholdDeductionType = 0;
    private householdTaxDeductionProductId = 0;
    private householdTaxDeductionDeniedProductId = 0;
    private household50TaxDeductionProductId = 0;
    private household50TaxDeductionDeniedProductId = 0;
    private rutTaxDeductionProductId = 0;
    private rutTaxDeductionDeniedProductId = 0;
    private green15TaxDeductionProductId = 0;
    private green15TaxDeductionDeniedProductId = 0;
    private green20TaxDeductionProductId = 0;
    private green20TaxDeductionDeniedProductId = 0;
    private green50TaxDeductionProductId = 0;
    private green50TaxDeductionDeniedProductId = 0;
    private defaultStockId = 0;
    private useProductGroupCustomerCategoryDiscount = false;
    private useQuantityPrices = false;
    private usePartialInvoicingOnOrderRow = false;
    private hideVatRate = false;
    private hideVatWarnings = false;
    private showExternalProductinfoLink = false;
    private showWholeseller = false;
    private askForWholeseller = false;
    private defaultWholesellerId = 0;
    private useEdi = false;
    private ediPriceRule: TermGroup_EDIPriceSettingRule;
    private attestTransitions: IAttestTransitionDTO[] = [];
    private attestStates: AttestStateDTO[] = [];
    private availableAttestStates: AttestStateDTO[] = [];
    private availableAttestStateOptions: any[] = [];
    private initialAttestState: AttestStateDTO;
    private excludedAttestStates: number[] = [];
    private attestStateReadyId = 0;
    private attestStateTransferredOfferToOrderId = 0;
    private attestStateTransferredOfferToInvoiceId = 0;
    private attestStateTransferredOrderToInvoiceId = 0;
    private attestStateOrderDeliverFromStockId = 0;
    private attestStateTransferredOrderToContractId = 0;
    private autoSetDateOnOrderRows = false;
    private discountAccountId = 0;
    private discountOffsetAccountId = 0;
    private calculateMarginalIncomeOnZeroPurchase = true;
    private useExtendSearchInfo = false;
    private grossMarginCalculationType: TermGroup_GrossMarginCalculationType;
    private hideTransferred = false;
    private showAdditionalDiscount = false;
    private showImportProductRows = false;
    private showAllRows = false;
    private useEDIPriceForRecalculation = false;

    // User settings
    private productSearchMinPrefixLength = 0;
    private productSearchMinPopulateDelay = 0;
    private productSearchFilterMode: TermGroup_ProductSearchFilterMode = TermGroup_ProductSearchFilterMode.StartsWidth;
    private disableWarningPopups = false;
    private useCashRounding = false;
    private showWarningBeforeRowDelete = false;
    private useRemainingAmountExVat = false;
    private hideIncomeRatioAndPercentage = false;
    private billingOfferLatestAttestStateTo = 0;

    // Lookups
    private terms: any;
    private products: IProductSmallDTO[];
    private liftProducts: IProductSmallDTO[];
    private productList: ProductRowsProductDTO[] = [];
    private productUnits: any[];
    private discountTypes: any[];
    private vatCodes: VatCodeDTO[];
    private vatAccounts: AccountVatRateViewSmallDTO[];
    private householdDeductionTypes: any[];
    //private stocks: any[];

    // GUI
    private gridHeightStyle: any;
    private useAttestState = false;
    private gridHasSelectedRows = false;
    private gridHasSelectedValidRows = false;
    private sumAmountCurrencyLabel: string;
    private feeCurrencyLabel: string;

    private defaultVatRate: number = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;
    private vatPercent = 0;
    private feeAmountCurrency = 0;

    private steppingRules: any;

    private modalInstance: any;
    private searchProductDialogIsOpen = false;

    // Flags
    private loading: boolean;
    private crediting: boolean;
    private copying: boolean;
    private executing = false;
    private warnNoCustomer = true;
    private warnDifferentCurrency = true;
    private delayUpdateVatType = false;
    private invoiceFeeLoaded = false;
    private freightAmountLoaded = false;
    private performDirectInvoicing = false;

    private visibleRows: ProductRowDTO[] = [];
    /*
    get visibleRows(): ProductRowDTO[] {
        return this._visibleRows;
    }
    */

    //private activeRows: ProductRowDTO[];        
    get activeRows(): ProductRowDTO[] {
        return _.filter(this.productRows, r => r.state === SoeEntityState.Active);
    }

    get activeProductRows(): ProductRowDTO[] {
        return _.filter(this.activeRows, r => r.type === SoeInvoiceRowType.ProductRow);
    }

    get unattestedRows(): ProductRowDTO[] {
        return _.filter(this.activeRows, r => (r.attestStateId == null || (this.initialAttestState != null && r.attestStateId === this.initialAttestState.attestStateId)));
    }

    get selectedUnattestedRows(): ProductRowDTO[] {
        return _.filter(this.soeGridOptions.getSelectedRows(), r => (r.attestStateId == null || (this.initialAttestState != null && r.attestStateId === this.initialAttestState.attestStateId)));
    }

    private internalIdCounter = 0;
    private prevProductId = 0;
    private prevQuantity = 0;
    private prevAmount = 0;
    private prevDiscountValue = 0;
    private prevDiscount2Value = 0;
    private rowsToDelete: number[] = [];
    private pendingProductId = 0;
    private vatChangedCounter = 0;

    // Household tax deduction properties
    private householdProperty: string;
    private householdApartmentNbr: string;
    private householdCooperativeOrgNbr: string;

    // Filters
    private amountFilter: any;

    // States
    private reverseSorting = false;
    private sortedNeedsSave = false;

    // Functions
    private rowFunctions: any = [];
    private addRowFunctions: any = [];

    private parentGuid: Guid;
    private hideChangeAttestState: boolean;
    private isEuBased: boolean;

    private debugMode = false;

    public progress: IProgressHandler;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        protected coreService: ICoreService,
        private productService: IProductService,
        private stockService: IStockService,
        private orderService: IOrderService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        progressHandlerFactory?: IProgressHandlerFactory) {
        super("Common.Directives.ProductRows", "billing.order.productrows", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants, null, null, "directiveCtrl");

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.$scope.$on('addNewRow', (e, a) => {
            if (a.fixedPrice) {
                const fixedPriceItems = this.activeProductRows.filter(x => x.isFixedPriceProduct);
                if (fixedPriceItems.length === 0) {
                    this.$scope.$applyAsync(() => {
                        this.executeAddRowFunction({ id: ProductRowsAddRowFunctions.Product, fixedPrice: a.fixedPrice });
                    });
                }
            }
            else if (a.household) {
                const productRow = this.addRow(SoeInvoiceRowType.ProductRow, false).row;
                if (productRow) {
                    var productId = this.rutTaxDeductionDeniedProductId;
                    if (a.taxDeductionType === TermGroup_HouseHoldTaxDeductionType.ROT) {
                        if (a.percent === '50%')
                            productId = this.household50TaxDeductionDeniedProductId;
                        else
                            productId = this.householdTaxDeductionDeniedProductId;
                    }
                    else if (a.taxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN) {
                        if (a.percent === '15%')
                            productId = this.green15TaxDeductionDeniedProductId;
                        else if (a.percent === '20%')
                            productId = this.green20TaxDeductionDeniedProductId;
                        else
                            productId = this.green50TaxDeductionDeniedProductId;
                    }
                    this.setProductValuesFromId(productRow, productId);
                    productRow.quantity = a.quantity;
                    productRow.amountCurrency = a.amount * -1;
                    this.amountHelper.calculateRowCurrencyAmount(productRow, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                    this.amountHelper.calculateRowSum(productRow);
                }
            }
            else {
                /*
                let onlyInitial = true;
                _.forEach(this.productRows, r => {
                    if (!this.initialAttestState || (r.attestStateId != undefined && r.attestStateId !== null && r.attestStateId !== this.initialAttestState.attestStateId) || r.productId === this.fixedPriceProductId || r.productId === this.fixedPriceKeepPricesProductId)
                        onlyInitial = false;

                });
                
                if (onlyInitial)
                    this.executeAddRowFunction({ id: ProductRowsAddRowFunctions.Product });
                */
            }
        });

        this.$scope.$on('addReminderRows', (e, a) => {
            if (a && a.reminders) {
                _.forEach(a.reminders, (reminder) => {
                    const productRow = this.addRow(SoeInvoiceRowType.ProductRow, false).row;
                    if (productRow) {
                        productRow.isReminderRow = true;
                        productRow.customerInvoiceReminderId = reminder.customerInvoiceReminderId;
                        productRow.amount = reminder.amount;
                        productRow.amountCurrency = reminder.amountCurrency;
                        productRow.quantity = 1;
                        if (reminder.invoiceProductId && reminder.invoiceProductId > 0)
                            this.setProductValuesFromId(productRow, reminder.invoiceProductId);

                        this.amountHelper.calculateRowCurrencyAmount(productRow, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                        this.amountHelper.calculateRowSum(productRow);

                        const textRowItem = this.addRow(SoeInvoiceRowType.TextRow, false).row;
                        textRowItem.text = this.terms["common.customer.invoices.remindertextrow"].format(reminder.invoiceNr, CalendarUtility.toFormattedDate(reminder.dueDate));
                        textRowItem.parentRowId = productRow.tempRowId;
                    }
                });

                this.soeGridOptions.refreshRows();
            }
        });

        this.$scope.$on('addInterestRows', (e, a) => {
            if (a && a.interests) {
                _.forEach(a.interests, (interest) => {
                    const productRow = this.addRow(SoeInvoiceRowType.ProductRow, false).row;
                    if (productRow) {
                        productRow.isInterestRow = true;
                        productRow.customerInvoiceInterestId = interest.customerInvoiceInterestId;
                        productRow.amount = interest.amount;
                        productRow.amountCurrency = interest.amountCurrency;
                        productRow.quantity = 1;
                        if (interest.invoiceProductId && interest.invoiceProductId > 0)
                            this.setProductValuesFromId(productRow, interest.invoiceProductId);

                        this.amountHelper.calculateRowCurrencyAmount(productRow, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                        this.amountHelper.calculateRowSum(productRow);

                        var textRowItem = this.addRow(SoeInvoiceRowType.TextRow, false).row;
                        textRowItem.text = this.terms["common.customer.invoices.interesttextrow"].format(interest.invoiceNr, CalendarUtility.toFormattedDate(interest.payDate), interest.payDate.diffDays(interest.dueDate), CalendarUtility.toFormattedDate(interest.dueDate));
                        textRowItem.parentRowId = productRow.tempRowId
                    }
                });

                this.soeGridOptions.refreshRows();
            }
        });

        this.$scope.$on('removeInterestReminderRows', (e, a) => {
            _.forEach(_.filter(this.activeProductRows, (r) => r.isReminderRow || r.isInterestRow), (row) => {
                this.deleteRow(row);
            });
        });

        this.$scope.$on('copyRows', (e, a) => {
            if (a.guid === this.parentGuid) {
                if (a.checkRecalculate)
                    this.initResetRowsForCopying(a && a.isCredit ? a.isCredit : false);
                else
                    this.resetRowsForCopying(a && a.isCredit ? a.isCredit : false);
            }
        });

        this.$scope.$on('reverseRowAmounts', (e, a) => {
            this.reverseRowAmounts(false);
        });

        this.$scope.$on('vatTypeChanged', (e, a) => {
            this.$timeout(() => {
                this.updateVatType();
            }, 0)
        });

        this.$scope.$on('updateCustomer', (e, a) => {
            if (a.customer) {
                this.customer = a.customer;
                this.customerChanged(a.getFreight, a.getInvoiceFee);
            }
        });

        this.$scope.$on('updateWholesellers', (e, a) => {
            this.wholesellers = a.wholesellers;
        });

        this.$scope.$on('updateInvoiceFee', (e, a) => {
            this.invoiceFeeCurrency = NumberUtility.parseNumericDecimal(a.invoiceFeeCurrency);
            if (!this.invoiceFeeCurrency)
                this.invoiceFeeCurrency = 0;
            this.amountHelper.getCurrencyAmount(this.invoiceFeeCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.invoiceFee = am });
            this.updateInvoiceFee();
        });

        this.$scope.$on('updateFreighAmount', (e, a) => {
            this.freightAmountCurrency = NumberUtility.parseNumericDecimal(a.freightAmountCurrency);
            if (!this.freightAmountCurrency)
                this.freightAmountCurrency = 0;
            this.amountHelper.getCurrencyAmount(this.freightAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.freightAmount = am });

            this.updateFreightAmount();
        });

        this.$scope.$on('validateTransferToInvoice', (e, a) => {
            if (a.directInvoicing) {
                this.performDirectInvoicing = true;
                this.selectedAttestState = _.find(this.attestStates, (a) => a.attestStateId === this.attestStateReadyId).attestStateId;
                if (this.soeGridOptions.getSelectedCount() === 0) {
                    _.forEach(_.filter(this.soeGridOptions.getFilteredRows(), r => r.type !== SoeInvoiceRowType.AccountingRow), row => {
                        if (_.filter(this.attestTransitions, (a) => (a.attestStateFromId === row.attestStateId && this.selectedAttestState === a.attestStateToId && row.attestStateId != this.attestStateTransferredOrderToInvoiceId) || row.attestStateId === this.selectedAttestState).length > 0)
                            this.soeGridOptions.selectRow(row);
                    });
                    this.soeGridOptions.refreshRows();
                }

                this.saveAttestState(a.guid);
            }
            else {
                this.messagingService.publish(Constants.EVENT_VALIDATE_TRANSFER_TO_INVOICE_RESULT, {
                    invoiceId: this.invoiceId,
                    notTransferable: this.hasRowsNotTransferableToInvoice(),
                    fixedPriceLeavingOthers: this.transferFixedPriceToInvoiceLeavingOthers(),
                    transferringContractProducts: this.numberOfContractRowsTransferrableToInvoice() > 0,
                    hasDeductionAmountMismatch: this.hasDeductionAmountMismatch(),
                    guid: a.guid
                });
            }
        });

        this.$scope.$on('updateInternalIdCounter', (e, a) => {
            this.internalIdCounter = a.numberOfRows;
        });

        this.$scope.$on('stopEditing', (e, a) => {
            this.soeGridOptions.stopEditing(false);
            this.$timeout(() => {
                a.functionComplete("productrows");
            }, 0)
        });

        this.$scope.$on('refreshRows', (e, a) => {
            if (this.visibleRows) {
                this.soeGridOptions.refreshRows.apply(this.soeGridOptions, this.visibleRows.filter(x => x.purchaseStatus));
            }
        });

        this.$scope.$on('recalculateTotals', (e, a) => {
            if (a.guid && a.guid === this.parentGuid) {
                this.reCalculateRowSums(true);
                this.calculateAmounts();
            }
        });
    }

    // SETUP
    public $onInit() {
        this.modalInstance = this.$uibModal;
        this.amountFilter = this.$filter("amount");
        if (this.amountHelper) {
            this.amountHelper.priceListTypeInclusiveVat = this.priceListTypeInclusiveVat;
            this.amountHelper.isCredit = this.isCredit;
        }

        this.useAttestState = (this.container == ProductRowsContainers.Offer || this.container == ProductRowsContainers.Order);

        this.setupSteppingRules();

        if (this.container == ProductRowsContainers.Contract)
            this.originType = SoeOriginType.Contract;
        else if (this.container == ProductRowsContainers.Invoice)
            this.originType = SoeOriginType.CustomerInvoice;
        else if (this.container == ProductRowsContainers.Offer)
            this.originType = SoeOriginType.Offer;
        else if (this.container == ProductRowsContainers.Order)
            this.originType = SoeOriginType.Order;
    }

    public productsStartupLoad() {
        this.$q.all([
            this.loadAllProducts(false),
            this.loadLiftProducts(),
        ]).then(x => {
            this.productsLoaded = true;
        });
    }

    public setupGrid() {
        const gridEvents: GridEvent[] = [];
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.beginCellEdit(entity, colDef); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.soeGridOptions.subscribe(gridEvents);

        this.productsStartupLoad();

        this.startLoad();
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadReadOnlyPermissions(),
            this.loadCompanySettings(),
            this.getUseEdi(),
            this.loadUserAttestTransitions()]).then(() => {
                // Amount helper
                const amountEvents: AmountHelperEvent[] = [];
                amountEvents.push(new AmountHelperEvent(AmountEvent.SetFixedPrice, (row: ProductRowDTO) => { this.setFixedPrice(row); }));
                amountEvents.push(new AmountHelperEvent(AmountEvent.CalculateAmounts, () => { this.calculateAmounts(); }));
                this.amountHelper.subscribe(amountEvents);
                this.amountHelper.init();

                // Handle visible
                this.visibleRows = _.orderBy(_.filter(this.productRows, r => r.state === SoeEntityState.Active &&
                    (!this.hideTransferred || !this.isRowTransferred(r)) &&
                    (r.type === SoeInvoiceRowType.ProductRow ||
                        r.type === SoeInvoiceRowType.TextRow ||
                        r.type === SoeInvoiceRowType.PageBreakRow ||
                        r.type === SoeInvoiceRowType.SubTotalRow)), 'rowNr');

                const nbrOfRows = this.visibleRows ? this.visibleRows.length + 1 : 0;
                if (nbrOfRows < 8)
                    this.soeGridOptions.setMinRowsToShow(8);
                else if (nbrOfRows > 30)
                    this.soeGridOptions.setMinRowsToShow(30);
                else
                    this.soeGridOptions.setMinRowsToShow(nbrOfRows);
                this.$q.all([
                    this.loadUserSettings(),
                    this.loadUserCompanySettings(),
                    //this.loadAllProducts(false),
                    //this.loadLiftProducts(),
                    this.loadProductUnits(),
                    this.setupDiscountTypes(),
                    this.loadVatCodes(),
                    this.loadVatAccounts(),
                    this.loadHouseholdDeductionTypes()]).then(() => {
                        this.setupToolBar();
                        this.gridAndDataIsReady();
                    });
            });
    }

    private gridAndDataIsReady() {
        this.setupGridColumns();
        this.soeGridOptions.setSingelValueConfiguration([
            {
                field: "text",
                predicate: (data: ProductRowDTO) => data.isTextRow && (this.readOnly || data.isReadOnly),
                editable: false,
            },
            {
                field: "text",
                predicate: (data: ProductRowDTO) => data.isTextRow && !this.readOnly && !data.isReadOnly,
                editable: true,
            },
            { field: "text", predicate: (data: ProductRowDTO) => data.isPageBreakRow, editable: false, cellClass: "bold" },
            {
                field: "text",
                predicate: (data: ProductRowDTO) => data.isSubTotalRow,
                editable: true,
                cellClass: "bold",
                cellRenderer: (data, value) => {
                    const sum = data["sumAmountCurrency"] || "";
                    return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                },
                spanTo: "sumAmountCurrency"
            },
        ]);

        this.soeGridOptions.addFooterRow("#article-sum-footer-grid", {
            "quantity": "sum",
            "sumAmountCurrency": "sum",
            "vatAmountCurrency": "sum",
            "purchasePriceSum": "sum"
        } as IColumnAggregations, (data: ProductRowDTO) => data.isProductRow);

        this.soeGridOptions.addTotalRow("#article-totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });

        this.setupWatchers();
        //this.soeGridOptions.useGrouping();
        this.soeGridOptions.finalizeInitGrid();
        this.messagingService.publish(Constants.EVENT_PRODUCTROW_GRID_READY, this.parentGuid);
    }


    private setupGridColumns() {
        const defaultEditable = (data: ProductRowDTO) => !this.readOnly && !data.isReadOnly;
        const editablePrices = (data: ProductRowDTO) => !this.readOnly && !data.isReadOnly && !data.isExpenseRow;
        const defaultEditableTimeCheck = (data: ProductRowDTO) => !this.readOnly && this.productsLoaded && !data.isReadOnly && !data.isTimeProjectRow && !data.isExpenseRow;
        super.addColumnIsModified("isModified", "", null, (params) => this.edit(params.data));
        super.addColumnIcon("rowTypeIcon", null, null, { pinned: "left", editable: false });
        super.addColumnNumber("rowNr", this.terms["billing.productrows.rownr"], 20, { enableHiding: false, pinned: "left", editable: false });

        if (this.useEdi && this.container !== ProductRowsContainers.Offer && this.container != ProductRowsContainers.Contract)
            super.addColumnText("ediTextValue", this.terms["billing.productrows.edi"], 20, { enableHiding: true, editable: false });

        const options = new TypeAheadOptionsAg();
        options.source = (filter) => this.filterProducts(filter);
        options.minLength = this.productSearchMinPrefixLength;
        options.delay = this.productSearchMinPopulateDelay;
        //options.updater = this.productSelectionCallback.bind(this);
        options.displayField = "numberName"
        options.dataField = "number";
        options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);
        options.useScroll = true;

        if (!this.readOnly) {
            options.buttonConfig = {
                icon: "fal fa-search",
                tooltipKey: "billing.productrows.searchproduct",
                click: this.searchProductFromRow.bind(this)
            };
        }
        super.addColumnTypeAhead("productNr", this.terms["billing.productrows.productnr"], 100, { error: 'productError', typeAheadOptions: options, editable: defaultEditableTimeCheck });
        super.addColumnText("text", this.terms["billing.productrows.text"], 100, { enableHiding: false, editable: defaultEditable });
        super.addColumnNumber("quantity", this.terms["billing.productrows.quantity"], 30, {
            editable: defaultEditableTimeCheck,
            cellClassRules: {
                "warningRow": (gridRow: any) => gridRow.data.timeManuallyChanged,
                "deleted": (gridRow: any) => gridRow.data.state == SoeEntityState.Deleted.valueOf()
            }
        });

        if (this.container == ProductRowsContainers.Order && this.usePartialInvoicingOnOrderRow) {
            super.addColumnNumber("invoiceQuantity", this.terms["billing.productrows.invoicequantity"], 50, { enableHiding: true, editable: defaultEditable });
        }

        super.addColumnSelect("productUnitId", this.terms["billing.productrows.productunit"], 30, {
            selectOptions: this.productUnits,
            enableHiding: false,
            editable: defaultEditableTimeCheck,
            displayField: "productUnitCode",
            dropdownIdLabel: "value",
            dropdownValueLabel: "label",
            //onChanged: this.productUnitChanged.bind(this)
        });

        if (this.useStock && this.container !== ProductRowsContainers.Contract) {
            super.addColumnBool("isStockRow", this.terms["billing.productrows.isstockrow"], 30, true);
            super.addColumnSelect("stockId", this.terms["billing.productrows.stockcode"], 50, {
                selectOptions: [],
                displayField: "stockCode",
                enableHiding: true,
                dropdownIdLabel: "stockId",
                dropdownValueLabel: "name",
                onChanged: ({ data }) => this.stockChanged(data),
                dynamicSelectOptions: {
                    idField: "id",
                    displayField: "name",
                    options: "stocksForProduct"
                },
                hide: this.container == ProductRowsContainers.Offer,
                editable: defaultEditable
            });
        }

        if (this.showSalesPricePermission) {
            super.addColumnNumber("amountCurrency", this.terms["billing.productrows.amount"], 50, { enableHiding: false, decimals: 2, editable: editablePrices, maxDecimals: 4 });
            super.addColumnNumber("discountValue", this.terms["billing.productrows.discount"], 50, { enableHiding: true, decimals: 2, editable: defaultEditable });
            super.addColumnSelect("discountType", this.terms["billing.order.discounttype"], 30, {
                selectOptions: this.discountTypes,
                enableHiding: true,
                editable: defaultEditable,
                displayField: "discountTypeText",
                dropdownIdLabel: "value",
                dropdownValueLabel: "label",
                onChanged: this.discountTypeChanged.bind(this)
            });

            if (this.showAdditionalDiscount) {
                super.addColumnNumber("discount2Value", this.terms["billing.productrows.discount2"], 50, { enableHiding: true, decimals: 2, editable: defaultEditable, hide: true });
                super.addColumnSelect("discount2Type", this.terms["billing.productrows.discount2type"], 30, {
                    selectOptions: this.discountTypes,
                    enableHiding: true,
                    editable: defaultEditable,
                    displayField: "discount2TypeText",
                    dropdownIdLabel: "value",
                    hide: true,
                    dropdownValueLabel: "label",
                    onChanged: this.discountTypeChanged.bind(this)
                });
            }

            super.addColumnNumber("supplementCharge", this.terms["billing.productrows.supplementcharge"], 50, {
                enableHiding: true,
                decimals: 2,
                editable: (data) => !this.isRowTransferred(data) && defaultEditable(data),
                cellClassRules: {
                    "text-right": () => true,
                    "errorRow": (gridRow: any) => gridRow.data.supplementCharge < 0 || gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                    "deleted": (gridRow: any) => gridRow.data.state === SoeEntityState.Deleted.valueOf()
                }
            });

            super.addColumnNumber("sumAmountCurrency", this.terms["billing.productrows.sumamount"], 50, {
                enableHiding: false,
                editable: false,
                decimals: 2,
                cellClassRules: {
                    "text-right": () => true,
                    "indiscreet": () => true,
                    "errorRow": (gridRow: any) => gridRow.data.supplementCharge < 0 || gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                    "deleted": (gridRow: any) => gridRow.data.state === SoeEntityState.Deleted.valueOf()
                }
            });

            super.addColumnNumber("sumTotalAmountCurrency", this.terms["common.total"], 50, {
                enableHiding: true,
                hide: true,
                editable: false,
                decimals: 2,
                cellClassRules: {
                    "text-right": () => true,
                    "indiscreet": () => true,
                    "errorRow": (gridRow: any) => gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                    "deleted": (gridRow: any) => gridRow.data.state === SoeEntityState.Deleted.valueOf()
                }
            });
        }

        //var vatCodeColDef = super.addColumnSelect("vatCodeId", this.terms["billing.productrows.vatcode"], "5%", this.vatCodes, true, true, "vatCodeCode", "value", "label", "vatCodeChanged", "directiveCtrl");
        //vatCodeColDef.cellEditableCondition = scope => { return !this.isRowTransferred(scope.row.entity); };
        //var vatAccountColDef = super.addColumnSelect("vatAccountId", this.terms["billing.productrows.vataccount"], "5%", this.vatAccounts, true, true, "vatAccountNr", "value", "label", "vatAccountChanged", "directiveCtrl");
        //vatAccountColDef.cellEditableCondition = scope => { return !this.isRowTransferred(scope.row.entity); };

        super.addColumnNumber("vatRate", this.terms["billing.productrows.vatrate"], 50, { enableHiding: true, decimals: 2, editable: false });
        if (this.showSalesPricePermission) {
            super.addColumnNumber("vatAmountCurrency", this.terms["billing.productrows.vatamount"], 50, {
                enableHiding: true, decimals: 2, editable: false, cellClassRules: {
                    "errorRow": (gridRow: any) => gridRow.data.amountCurrency === 0 && gridRow.data.vatAmountCurrency !== 0
                }
            });
        }

        if (this.showPurchasePricePermission) {
            let purchColDef = super.addColumnNumber("purchasePriceCurrency", this.terms["billing.productrows.purchaseprice"], 50, {
                enableHiding: true,
                decimals: 2,
                editable: (data) => !this.isRowTransferred(data) && defaultEditable(data)
            });

            super.addColumnNumber("purchasePriceSum", this.terms["billing.productrows.purchasepricesum"], 50, {
                enableHiding: true,
                decimals: 2,
                editable: false,
                hide: true,
            });

            super.addColumnText("sysWholesellerName", this.terms["billing.order.syswholeseller"], 50, { enableHiding: true, editable: false });
            if (!this.hideIncomeRatioAndPercentage && this.showSalesPricePermission) {
                super.addColumnNumber("marginalIncomeCurrency", this.terms["billing.productrows.marginalincome.short"], 50, {
                    enableHiding: true,
                    decimals: 2,
                    editable: (data) => !this.isRowTransferred(data) && defaultEditable(data),
                    cellClassRules: {
                        "text-right": () => true,
                        "errorRow": (gridRow: any) => gridRow.data.supplementCharge < 0 || gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                        "deleted": (gridRow: any) => gridRow.data.state === SoeEntityState.Deleted.valueOf()
                    }
                });

                super.addColumnNumber("marginalIncomeRatio", this.terms["billing.productrows.marginalincomeratio.short"], 50, {
                    enableHiding: true,
                    decimals: 2,
                    editable: (data) => !this.isRowTransferred(data) && defaultEditable(data),
                    cellClassRules: {
                        "text-right": () => true,
                        "errorRow": (gridRow: any) => gridRow.data.supplementCharge < 0 || gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                        "deleted": (gridRow: any) => gridRow.data.state === SoeEntityState.Deleted.valueOf()
                    }
                });
            }
        }

        super.addColumnSelect("householdTaxDeductionType", this.terms["billing.products.taxdeductiontype"], 30, {
            selectOptions: this.householdDeductionTypes,
            enableHiding: true,
            editable: false,
            displayField: "householdDeductionTypeText",
            dropdownIdLabel: "value",
            dropdownValueLabel: "label",
            hide: true,
        });

        if (this.container == ProductRowsContainers.Contract) {
            const fromColumn = super.addColumnDate("date", this.terms["common.from"], 50, true, null, null, { disabled: () => { return !defaultEditable } });
            fromColumn.enableCellEdit = true;
            fromColumn.cellEditableCondition = (row) => { return this.isRowEditable(row) };

            const toColumn = super.addColumnDate("dateTo", this.terms["common.to"], 50, true, null, null, { disabled: () => { return !defaultEditable } });
            toColumn.enableCellEdit = true;
            //toColumn.cellEditableCondition = (row) => { return !this.readOnly && !row.isReadOnly; };
        }
        else if (this.container == ProductRowsContainers.Order) {
            super.addColumnDate("date", this.terms["common.date"], 50, true, null, null, { disabled: () => { return !defaultEditable }, hide: true, editable: (data) => !this.isRowTransferred(data) && defaultEditable(data) && (data.type === SoeInvoiceRowType.ProductRow || data.type === SoeInvoiceRowType.TextRow) });
            if (this.purchasePermission) {
                super.addColumnText("purchaseStatus", this.terms["billing.productrows.purchasestatus"], 40, { enableHiding: true, hide: true, editable: false });
            }
        }

        if (this.useAttestState)
            super.addColumnShape("attestStateName", null, 40, { maxWidth: 40, shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", colorField: "attestStateColor", showIconField: "attestStateColor" }).pinned = "right";

        const defs = this.soeGridOptions.getColumnDefs();
        _.forEach(defs, (colDef: uiGrid.IColumnDef) => {
            if (colDef.field !== "isModified" &&
                colDef.field !== "rowNr" &&
                colDef.field !== "text" &&
                this.getSoeType(colDef) !== Constants.GRID_COLUMN_TYPE_ICON &&
                this.getSoeType(colDef) !== Constants.GRID_COLUMN_TYPE_SHAPE) {
                colDef['collapseOnTextRow'] = true;
                colDef['collapseOnPageBreakRow'] = true;
                if (colDef.field !== "sumAmountCurrency")
                    colDef['collapseOnSubTotalRow'] = true;
            }
        });

        //if (this.readOnly) {
        //super.addColumnIcon("edit", null, null, { icon: "fal fa-search", toolTip: this.terms["core.show"], onClick: this.edit.bind(this), pinned: "right", showIcon: this.isRowEditable.bind(this), enableHiding: false, enableResizing: false });
        //} else {

        super.addColumnIcon("edit", null, null, { icon: "fal fa-pencil iconEdit", toolTip: this.terms["core.edit"], onClick: this.edit.bind(this), pinned: "right", showIcon: this.isRowEditable.bind(this), editable: (row: ProductRowDTO) => { return !row.isReadOnly }, enableHiding: false, enableResizing: false, suppressFilter: true });
        super.addColumnDelete(this.terms["core.deleterow"], (data) => this.initDeleteRow(data), null, (data) => data && this.allowDeleteRow(data));

        _.forEach(defs, (colDef: any) => {
            // Add strike through on deleted or processed rows
            // If a cell class function is alredy added, we can't add the class since it will break the added function,
            // therefore in all places above where functions are added, the strike through class must be added there also.
            if (!colDef.cellClass || !angular.isFunction(colDef.cellClass)) {
                var cellClass: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                colDef.cellClass = (params) => {
                    const { data } = params;
                    return cellClass + (data.state == SoeEntityState.Deleted ? " deleted" : "");
                };
            }
        });
        //}

        // TODO: Seems to be called too early in the base class
        this.restoreState();
    }

    protected setupToolBar() {
        //if (!this.readOnly) {
        // Context menu
        this.contextMenuHandler = this.contextMenuHandlerFactory.create();

        // Functions
        const keys: string[] = [
            "billing.productrows.functions.addproduct",
            "billing.productrows.functions.refreshproducts",
            "billing.productrows.functions.changewholeseller",
            "billing.productrows.functions.changediscount",
            "billing.productrows.functions.recalculatepricesonselectedrows",
            "billing.productrows.functions.setstock",
            "billing.productrows.functions.sortrowsbynumber",
            "billing.productrows.functions.copyrows",
            "billing.productrows.functions.copyrowstocontract",
            "billing.productrows.functions.mergerows",
            "billing.productrows.functions.moverows",
            "billing.productrows.functions.moverowswithinorder",
            "billing.productrows.functions.deleterows",
            "billing.productrows.functions.showallsums",
            "billing.productrows.functions.showdeletedrows",
            "billing.productrows.functions.unlockrows",
            "billing.productrows.functions.renumberrows",
            "billing.productrows.addproductrow",
            "billing.productrows.importproductrow",
            "billing.productrows.addtextrow",
            "billing.productrows.addpagebreak",
            "billing.productrows.addsubtotal",
            "billing.productrows.functions.moverowstostock",
            "billing.productrows.functions.movetootherorder",
            "billing.productrows.functions.movetootherinvoice",
            "billing.offer.moverowswithinorder",
            "billing.offer.movetootheroffer",
            "billing.productrows.functions.moverowswithincontract",
            "billing.productrows.functions.moverowstoothercontract",
            "billing.productrows.functions.showconnectedtimerows",
            "billing.productrows.splitaccounting",
            "billing.productrows.functions.recalculatetimerows",
            "billing.productrows.functions.changedeductiontype",
            "billing.productrows.functions.createpurchase",
            "billing.productrows.functions.changeintrastatcode",
            "common.searchinvoiceproduct.showexternalproductinfo",
            "billing.productrows.functions.uppercaserows"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            // Context menu
            this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.addproductrow"], 'fa-box-alt', ($itemScope, $event, modelValue) => { this.executeAddRowFunction({ id: ProductRowsAddRowFunctions.Product }); }, () => { return !this.readOnly });
            this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.addtextrow"], 'fa-text', ($itemScope, $event, modelValue) => { this.executeAddRowFunction({ id: ProductRowsAddRowFunctions.Text }); }, () => { return !this.readOnly });
            this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.addpagebreak"], 'fa-cut', ($itemScope, $event, modelValue) => { this.executeAddRowFunction({ id: ProductRowsAddRowFunctions.PageBreak }); }, () => { return !this.readOnly });
            this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.addsubtotal"], 'fa-calculator-alt', ($itemScope, $event, modelValue) => { this.executeAddRowFunction({ id: ProductRowsAddRowFunctions.SubTotal }); }, () => { return !this.readOnly });
            this.contextMenuHandler.addContextMenuSeparator();

            if (this.showExternalProductinfoLink) {
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ShowExternalProductinfoLink, name: terms["common.searchinvoiceproduct.showexternalproductinfo"], icon: "fal fa-fw fa-arrow-up-right-from-square", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["common.searchinvoiceproduct.showexternalproductinfo"], 'fa-arrow-up-right-from-square', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ShowExternalProductinfoLink }); }, () => { return this.gridHasSelectedRows; });
            }

            this.rowFunctions.push({ id: ProductRowsRowFunctions.AddProduct, name: terms["billing.productrows.functions.addproduct"], icon: "fal fa-fw fa-plus" });
            this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.addproduct"], 'fa-plus', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.AddProduct }); }, () => { return !this.readOnly });

            if (this.showImportProductRows) {
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ImportRow, name: terms["billing.productrows.importproductrow"], icon: "fal fa-fw fa-file-import", disabled: () => { return this.invoiceId == 0 } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.importproductrow"], 'fal fa-fw fa-file-import', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ImportRow }); }, () => { return !this.readOnly }); 
            }
            this.rowFunctions.push({ id: ProductRowsRowFunctions.RefreshProducts, name: terms["billing.productrows.functions.refreshproducts"], icon: "fal fa-fw fa-sync" });
            this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.refreshproducts"], 'fa-sync', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.RefreshProducts }); }, () => { return !this.readOnly });
            if (!this.hasFixedPriceProducts()) {
                this.rowFunctions.push({ id: ProductRowsRowFunctions.RecalculatePrices, name: terms["billing.productrows.functions.recalculatepricesonselectedrows"], icon: "fal fa-fw fa-calculator-alt", disabled: () => { return !this.gridHasSelectedValidRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.recalculatepricesonselectedrows"], 'fa-calculator-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.RecalculatePrices }); }, () => { return this.gridHasSelectedValidRows && !this.readOnly; });
            }
            if (this.timeProjectPermission && this.container == ProductRowsContainers.Order) {
                this.rowFunctions.push({ id: ProductRowsRowFunctions.RecalculateTimeRow, name: terms["billing.productrows.functions.recalculatetimerows"], icon: "fal fa-fw fa-history" });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.recalculatetimerows"], 'fa-history', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.RecalculateTimeRow }); }, () => { return this.allowTimeRowFunc(); });
            }
            this.contextMenuHandler.addContextMenuSeparator();

            if (this.container == ProductRowsContainers.Offer) {
                if (this.copyProductRowsPermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.CopyRows, name: terms["billing.productrows.functions.copyrows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows || !this.copyProductRowsPermission } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.copyrows"], 'fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.CopyRows }); }, () => { return this.gridHasSelectedRows; });
                }

                //uppercase
                this.rowFunctions.push({ id: ProductRowsRowFunctions.UppercaseRows, name: terms["billing.productrows.functions.uppercaserows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.uppercaserows"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.UppercaseRows }); }, () => { return this.gridHasSelectedRows; });

                if (this.copyProductRowsPermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRowsWithinOrder, name: terms["billing.offer.moverowswithinorder"], icon: "fal fa-fw fa-chevron-down", disabled: () => { return !this.gridHasSelectedRows } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.offer.moverowswithinorder"], 'fa-chevron-down', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRowsWithinOrder }); }, () => { return this.gridHasSelectedRows; });

                    this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRows, name: terms["billing.offer.movetootheroffer"], icon: "fal fa-fw fa-chevron-double-right", disabled: () => { return !this.gridHasSelectedRows } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.offer.movetootheroffer"], 'fa-chevron-double-right', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRows }); }, () => { return this.gridHasSelectedRows; });
                }

                this.rowFunctions.push({ id: ProductRowsRowFunctions.MergeRows, name: terms["billing.productrows.functions.mergerows"], icon: "fal fa-fw fa-compress-alt", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.mergerows"], 'fa-compress-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MergeRows }); }, () => { return this.gridHasSelectedRows; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.ShowAllSums, name: terms["billing.productrows.functions.showallsums"], icon: "fal fa-fw fa-list" });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.SplitAccounting, name: terms["billing.productrows.splitaccounting"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedValidRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.splitaccounting"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.SplitAccounting }); }, () => { return this.gridHasSelectedValidRows; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.DeleteRows, name: terms["billing.productrows.functions.deleterows"], icon: "fal fa-fw fa-times iconDelete", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.deleterows"], 'fa-times iconDelete', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.DeleteRows }); }, () => { return this.gridHasSelectedRows; });

                this.contextMenuHandler.addContextMenuSeparator();

                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeWholeseller, name: terms["billing.productrows.functions.changewholeseller"], icon: "fal fa-fw fa-truck", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changewholeseller"], 'fa-truck', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeWholeseller }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDeductionType, name: terms["billing.productrows.functions.changedeductiontype"], icon: "fal fa-fw fa-money-bill-alt", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changedeductiontype"], 'fa-money-bill-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDeductionType }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDiscount, name: terms["billing.productrows.functions.changediscount"], icon: "fal fa-fw fa-percent", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changediscount"], 'fa-percent', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDiscount }); }, () => { return this.gridHasSelectedRows; });
            } else if (this.container == ProductRowsContainers.Order) {
                //purchase
                if (this.createPurchasePermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.CreatePurchase, name: terms["billing.productrows.functions.createpurchase"], icon: "fal fa-fw fa-shopping-cart", disabled: () => { return !this.gridHasSelectedRows || !this.invoiceId } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.createpurchase"], 'fa-shopping-cart', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.CreatePurchase }); }, () => this.gridHasSelectedRows && this.invoiceId > 0);
                }

                //COPY ROWS - Måste ändra utifrån status
                if (this.copyProductRowsPermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.CopyRows, name: terms["billing.productrows.functions.copyrows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows || !this.copyProductRowsPermission } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.copyrows"], 'fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.CopyRows }); }, () => { return this.gridHasSelectedRows; });

                    if (this.contractEditPermission) {
                        this.rowFunctions.push({ id: ProductRowsRowFunctions.CopyRowsToContract, name: terms["billing.productrows.functions.copyrowstocontract"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows || !this.copyProductRowsPermission } });
                        this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.copyrowstocontract"], 'fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.CopyRowsToContract }); }, () => { return this.gridHasSelectedRows; });
                    }
                }

                //uppercase
                this.rowFunctions.push({ id: ProductRowsRowFunctions.UppercaseRows, name: terms["billing.productrows.functions.uppercaserows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.uppercaserows"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.UppercaseRows }); }, () => { return this.gridHasSelectedRows; });

                //COPY ROWS - Måste ändra utifrån status
                if (this.copyProductRowsPermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRowsWithinOrder, name: terms["billing.productrows.functions.moverowswithinorder"], icon: "fal fa-fw fa-chevron-down", disabled: () => { return !this.gridHasSelectedRows } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.moverowswithinorder"], 'fa-chevron-down', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRowsWithinOrder }); }, () => { return this.gridHasSelectedRows; });

                    this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRows, name: terms["billing.productrows.functions.movetootherorder"], icon: "fal fa-fw fa-chevron-double-right", disabled: () => { return !this.gridHasSelectedRows || !this.copyProductRowsPermission } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.movetootherorder"], 'fa-chevron-double-right', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRows }); }, () => { return this.gridHasSelectedRows; });
                }

                this.rowFunctions.push({ id: ProductRowsRowFunctions.MergeRows, name: terms["billing.productrows.functions.mergerows"], icon: "fal fa-fw fa-compress-alt", disabled: () => { return !this.gridHasSelectedRows || !this.mergeProductRowsPermission } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.mergerows"], 'fa-compress-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MergeRows }); }, () => { return this.gridHasSelectedRows && this.mergeProductRowsPermission; });
                if (this.stockMenu) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.SetStock, name: terms["billing.productrows.functions.setstock"], icon: "fal fa-fw fa-inventory", disabled: () => { return !this.gridHasSelectedRows } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.setstock"], 'fa-inventory', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.SetStock }); }, () => { return this.gridHasSelectedRows; });

                    this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRowsToStock, name: terms["billing.productrows.functions.moverowstostock"], icon: "fal fa-fw fa-chevron-double-right", disabled: () => { return !this.gridHasSelectedRows || !this.copyProductRowsPermission } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.moverowstostock"], 'fa-chevron-double-right', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRowsToStock }); }, () => { return this.gridHasSelectedRows; });
                }
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ShowAllSums, name: terms["billing.productrows.functions.showallsums"], icon: "fal fa-fw fa-list" });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.SplitAccounting, name: terms["billing.productrows.splitaccounting"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedValidRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.splitaccounting"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.SplitAccounting }); }, () => { return this.gridHasSelectedValidRows; });

                if (!this.notAllowedToRemoveRows) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.DeleteRows, name: terms["billing.productrows.functions.deleterows"], icon: "fal fa-fw fa-times iconDelete", disabled: () => { return !this.gridHasSelectedRows } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.deleterows"], 'fa-times iconDelete', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.DeleteRows }); }, () => { return this.gridHasSelectedRows; });
                }

                //this.rowFunctions.push({ id: ProductRowsRowFunctions.ShowTimeRows, name: terms["billing.productrows.functions.showconnectedtimerows"], icon: "fal fa-clock", disabled: () => { return !this.allowShowTimeRows() } });
                if (this.timeProjectPermission) {
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.showconnectedtimerows"], 'fal fa-clock', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ShowTimeRows }); }, () => { return this.allowTimeRowFunc(); });
                }

                this.contextMenuHandler.addContextMenuSeparator();
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeWholeseller, name: terms["billing.productrows.functions.changewholeseller"], icon: "fal fa-fw fa-truck", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changewholeseller"], 'fa-truck', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeWholeseller }); }, () => { return this.gridHasSelectedRows; });

                if (this.intrastatPermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeIntrastatCode, name: terms["billing.productrows.functions.changeintrastatcode"], icon: "fal fa-fw fa-globe", disabled: () => { return !this.gridHasSelectedRows || !this.customer || !this.customer.isEUCountryBased } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changeintrastatcode"], 'fa-globe', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeIntrastatCode }); }, () => { return (this.gridHasSelectedRows && this.customer && this.customer.isEUCountryBased); });
                }

                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDeductionType, name: terms["billing.productrows.functions.changedeductiontype"], icon: "fal fa-fw fa-money-bill-alt", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changedeductiontype"], 'fa-money-bill-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDeductionType }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDiscount, name: terms["billing.productrows.functions.changediscount"], icon: "fal fa-fw fa-percent", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changediscount"], 'fa-percent', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDiscount }); }, () => { return this.gridHasSelectedRows; });

            } else if (this.container == ProductRowsContainers.Contract) {
                //COPY ROWS - Måste ändra utifrån status
                this.rowFunctions.push({ id: ProductRowsRowFunctions.CopyRows, name: terms["billing.productrows.functions.copyrows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.copyrows"], 'fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.CopyRows }); }, () => { return this.gridHasSelectedRows; });

                //uppercase
                this.rowFunctions.push({ id: ProductRowsRowFunctions.UppercaseRows, name: terms["billing.productrows.functions.uppercaserows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.uppercaserows"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.UppercaseRows }); }, () => { return this.gridHasSelectedRows; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRowsWithinOrder, name: terms["billing.productrows.functions.moverowswithincontract"], icon: "fal fa-fw fa-chevron-down", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.moverowswithincontract"], 'fa-chevron-down', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRowsWithinOrder }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRows, name: terms["billing.productrows.functions.moverowstoothercontract"], icon: "fal fa-fw fa-chevron-double-right", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.moverowstoothercontract"], 'fa-chevron-double-right', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRows }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.MergeRows, name: terms["billing.productrows.functions.mergerows"], icon: "fal fa-fw fa-compress-alt", disabled: () => { return !this.gridHasSelectedRows || !this.mergeProductRowsPermission } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.mergerows"], 'fa-compress-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MergeRows }); }, () => { return this.gridHasSelectedRows && this.mergeProductRowsPermission; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ShowAllSums, name: terms["billing.productrows.functions.showallsums"], icon: "fal fa-fw fa-list" });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.SplitAccounting, name: terms["billing.productrows.splitaccounting"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedValidRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.splitaccounting"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.SplitAccounting }); }, () => { return this.gridHasSelectedValidRows; });

                //this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.showallsums"], 'fa-list', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ShowAllSums }); }, () => { return true; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.DeleteRows, name: terms["billing.productrows.functions.deleterows"], icon: "fal fa-fw fa-times iconDelete", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.deleterows"], 'fa-times', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.DeleteRows }); }, () => { return this.gridHasSelectedRows; });
                this.contextMenuHandler.addContextMenuSeparator();
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeWholeseller, name: terms["billing.productrows.functions.changewholeseller"], icon: "fal fa-fw fa-truck", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changewholeseller"], 'fa-truck', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeWholeseller }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDeductionType, name: terms["billing.productrows.functions.changedeductiontype"], icon: "fal fa-fw fa-money-bill-alt", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changedeductiontype"], 'fa-money-bill-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDeductionType }); }, () => { return this.gridHasSelectedRows; });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDiscount, name: terms["billing.productrows.functions.changediscount"], icon: "fal fa-fw fa-percent", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changediscount"], 'fa-percent', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDiscount }); }, () => { return this.gridHasSelectedRows; });
            } else if (this.container == ProductRowsContainers.Invoice) {
                //COPY ROWS - Måste ändra utifrån status
                this.rowFunctions.push({ id: ProductRowsRowFunctions.CopyRows, name: terms["billing.productrows.functions.copyrows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.copyrows"], 'fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.CopyRows }); }, () => { return this.gridHasSelectedRows; });

                //uppercase
                this.rowFunctions.push({ id: ProductRowsRowFunctions.UppercaseRows, name: terms["billing.productrows.functions.uppercaserows"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.uppercaserows"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.UppercaseRows }); }, () => { return this.gridHasSelectedRows; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.MoveRows, name: terms["billing.productrows.functions.movetootherinvoice"], icon: "fal fa-fw fa-chevron-double-right", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.movetootherinvoice"], 'fa-chevron-double-right', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.MoveRows }); }, () => { return this.gridHasSelectedRows && !this.readOnly; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.SplitAccounting, name: terms["billing.productrows.splitaccounting"], icon: "fal fa-fw fa-clone", disabled: () => { return !this.gridHasSelectedValidRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.splitaccounting"], 'fal fa-fw fa-clone', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.SplitAccounting }); }, () => { return this.gridHasSelectedValidRows && !this.readOnly; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeDeductionType, name: terms["billing.productrows.functions.changedeductiontype"], icon: "fal fa-fw fa-money-bill-alt", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changedeductiontype"], 'fa-money-bill-alt', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeDeductionType }); }, () => { return this.gridHasSelectedRows; });

                this.rowFunctions.push({ id: ProductRowsRowFunctions.DeleteRows, name: terms["billing.productrows.functions.deleterows"], icon: "fal fa-fw fa-times iconDelete", disabled: () => { return !this.gridHasSelectedRows } });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.deleterows"], 'fa-times iconDelete', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.DeleteRows }); }, () => { return this.gridHasSelectedRows && !this.readOnly; });

                if (this.intrastatPermission) {
                    this.rowFunctions.push({ id: ProductRowsRowFunctions.ChangeIntrastatCode, name: terms["billing.productrows.functions.changeintrastatcode"], icon: "fal fa-fw fa-globe", disabled: () => { return (!this.invoiceId || this.invoiceId === 0 || !this.gridHasSelectedRows || !this.customer || !this.customer.isEUCountryBased) } });
                    this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.changeintrastatcode"], 'fa-globe', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.ChangeIntrastatCode }); }, () => {
                        return (this.invoiceId > 0 && this.gridHasSelectedRows && this.customer && this.customer.isEUCountryBased);
                    });
                }
            }
            this.contextMenuHandler.addContextMenuSeparator();

            if (!this.hasNonSortableRows()) {
                this.rowFunctions.push({ id: ProductRowsRowFunctions.SortRowsByProductNr, name: terms["billing.productrows.functions.sortrowsbynumber"], icon: "fal fa-fw fa-sort-numeric-down" });
                this.contextMenuHandler.addContextMenuItem(terms["billing.productrows.functions.sortrowsbynumber"], 'fa-sort-numeric-down', ($itemScope, $event, modelValue) => { this.executeRowFunction({ id: ProductRowsRowFunctions.SortRowsByProductNr }); }, () => { return !this.readOnly });
            }

            //SUPERADMIN NOT IMPLEMENTED
            /*if (this.isSuperAdmin) {
                this.rowFunctions.push({ id: ProductRowsRowFunctions.ShowDeletedRows, name: terms["billing.productrows.functions.showdeletedrows"], icon: "fal fa-fw fa-trash-alt" });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.UnlockRows, name: terms["billing.productrows.functions.unlockrows"], icon: "fal fa-fw fa-unlock-alt" });
                this.rowFunctions.push({ id: ProductRowsRowFunctions.RenumberRows, name: terms["billing.productrows.functions.renumberrows"], icon: "fal fa-fw fa-sort-numeric-down" });
            }*/

            // Add row functions
            this.addRowFunctions.push({ id: ProductRowsAddRowFunctions.Product, name: terms["billing.productrows.addproductrow"], icon: "fal fa-fw fa-box-alt" });
            this.addRowFunctions.push({ id: ProductRowsAddRowFunctions.Text, name: terms["billing.productrows.addtextrow"], icon: "fal fa-fw fa-text" });
            this.addRowFunctions.push({ id: ProductRowsAddRowFunctions.PageBreak, name: terms["billing.productrows.addpagebreak"], icon: "fal fa-fw fa-cut" });
            this.addRowFunctions.push({ id: ProductRowsAddRowFunctions.SubTotal, name: terms["billing.productrows.addsubtotal"], icon: "fal fa-fw fa-calculator-alt" });
        });

        this.setupSortGroup("rowNr");
        /*}
        else {
            if (this.addRowFunctions)
                this.addRowFunctions = [];
            if (this.rowFunctions)
                this.rowFunctions = [];
            if (this.contextMenuHandler)
                this.contextMenuHandler.clearContextMenuItems();
        }*/
    }

    private getContextMenuOptions() {
        return this.contextMenuHandler.getContextMenuOptions();
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

    private parentIsOrder() {
        return this.container == ProductRowsContainers.Order;
    }

    private allowDeleteRow(row: ProductRowDTO): boolean {
        if (row.isReadOnly || this.notAllowedToRemoveRows)
            return false;

        if ((row.isTimeBillingRow || row.isTimeProjectRow || row.isExpenseRow) && this.parentIsOrder() && row.quantity != 0)
            return false;

        return true;
    }

    private setupSteppingRules() {
        const mappings =
        {
            productNr(row: ProductRowDTO) { return row.isProductRow },
            text(row: ProductRowDTO) { return ((this.pendingProductId) && ((this.pendingProductId === this.miscProductId) || this.isLiftProduct(this.pendingProductId))) },
            quantity(row: ProductRowDTO) { return row.isProductRow },
            invoiceQuantity(row: ProductRowDTO) { return (row.isProductRow && this.container == ProductRowsContainers.Order && this.usePartialInvoicingOnOrderRow) },
            amountCurrency(row: ProductRowDTO) { return row.isProductRow },
        };

        this.steppingRules = mappings;
    }
    private productRowsUpdated() {
        if (this.productRows) {
            var productsToLoad: number[] = [];
            _.forEach(this.productRows, r => {

                    if (r.productId && !_.includes(productsToLoad, r.productId))
                    productsToLoad.push(r.productId);

                if (r.customerInvoiceRowId && r.customerInvoiceRowId > 0)
                    r.tempRowId = r.customerInvoiceRowId;

                this.setRowTypeIcon(r);

                if (r.type === SoeInvoiceRowType.ProductRow)
                    r.ediTextValue = r.ediEntryId ? this.terms["core.yes"] : this.terms["core.no"];

                this.amountHelper.calculateSupplementCharge(r);
                this.amountHelper.calculateMarginalIncomeLimit(r);

                if (this.useAttestState)
                    this.setAttestStateValues(r);

                if (r.date)
                    r.date = CalendarUtility.convertToDate(r.date);
                if (r.dateTo)
                    r.dateTo = CalendarUtility.convertToDate(r.dateTo);

                r.purchasePriceSum = r._quantity && r._purchasePriceCurrency ? r._quantity * r._purchasePriceCurrency : 0;

                r.sumTotalAmountCurrency = this.priceListTypeInclusiveVat ? r.sumAmountCurrency : r.sumAmountCurrency + r.vatAmountCurrency;

                r.isReadOnly = this.isRowTransferred(r, true);

            });
            this.loadProducts(productsToLoad, true);
            this.setDiscountTypeText();
            this.setHouseholdDeductionTypeText();
        }

        this.resetRows();
        this.attestStateChanged(true);
    }

    private setupWatchers() {
        if (!this.productRows)
            this.productRows = [];

        this.$scope.$watch(() => this.productRows, (newVal, oldVal) => {
            this.productRowsUpdated();
        });

        /*this.$scope.$watch(() => this.readOnly, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                //this.setupToolBar();
                //this.gridAndDataIsReady();
            }
        });*/
        /*
        this.$scope.$watch(() => this.productRows.length, (newValue, oldValue) => {
            // Set grid height based on number of rows
            // Limit rows between 8 and 30
            
            
            var rows = this.productRows ? this.productRows.length : 0;
            if (rows < 8)
                rows = 8;
            if (rows > 30)
                rows = 30;

            var height: number = (rows * 22) + 136;

            this.gridHeightStyle = { height: height + "px" };
        });
        */
        /*
        this.$scope.$watch(() => this.customer, () => {
            this.customerChanged();
        });
        */
        this.$scope.$watch(() => this.priceListTypeId, (newValue, oldValue) => {
            if (this.crediting)
                return;

            if (!this.loading && newValue !== oldValue) {
                this.recalculatePricesDialog();
                this.getFreightAmount();
                this.getInvoiceFee();
            }
        });
        this.$scope.$watch(() => this.billingType, (newValue, oldValue) => {
            if (!this.loading && newValue != oldValue) {
                if (!this.crediting || this.copying) {
                    this.getFreightAmount();
                    this.getInvoiceFee();
                }
                else {
                    this.crediting = false;
                    this.copying = false;
                }
            }
        });
        this.$scope.$watch(() => this.hideTransferred, (newValue, oldValue) => {
            if (newValue != oldValue) {
                this.resetRows();
            }
        });
        this.$scope.$watch(() => this.showAllRows, (newValue, oldValue) => {
            if (newValue != oldValue) {
                this.expandAllRows();
            }
        });

        this.$scope.$watch(() => this.freightAmountCurrency, (newValue, oldValue) => {
            if (!this.loading && newValue != oldValue) {
                this.freightAmountCurrency = NumberUtility.parseNumericDecimal(newValue);
                if (!this.freightAmountCurrency)
                    this.freightAmountCurrency = 0;
                this.amountHelper.getCurrencyAmount(this.freightAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.freightAmount = am });

                this.updateFreightAmount();
            }
        });
        this.$scope.$watch(() => this.invoiceFeeCurrency, (newValue, oldValue) => {
            if (!this.loading && newValue != oldValue) {
                this.invoiceFeeCurrency = NumberUtility.parseNumericDecimal(newValue);
                if (!this.invoiceFeeCurrency)
                    this.invoiceFeeCurrency = 0;
                this.amountHelper.getCurrencyAmount(this.invoiceFeeCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.invoiceFee = am });

                this.updateInvoiceFee();
            }
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        // Columns
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.edit",
            "core.deleterow",
            "core.deleterowwarning",
            "core.info",
            "core.show",
            "core.warning",
            "core.donotshowagain",
            "common.date",
            "billing.order.syswholeseller",
            "billing.productrows.addpagebreak",
            "billing.productrows.addsubtotal",
            "billing.productrows.rownr",
            "billing.productrows.edi",
            "billing.productrows.productnr",
            "billing.productrows.searchproduct",
            "billing.productrows.text",
            "billing.productrows.quantity",
            "billing.productrows.invoicequantity",
            "billing.productrows.isstockrow",
            "billing.productrows.stockcode",
            "billing.productrows.productunit",
            "billing.productrows.amount",
            "billing.productrows.discount",
            "billing.order.discounttype",
            "billing.productrows.discounttype.percent",
            "billing.productrows.discounttype.amount",
            "billing.productrows.supplementcharge",
            "billing.productrows.sumamount",
            "billing.productrows.sumamount.exclvat",
            "billing.productrows.sumamount.inclvat",
            "billing.productrows.vatcode",
            "billing.productrows.vataccount",
            "billing.productrows.vatrate",
            "billing.productrows.vatamount",
            "billing.productrows.purchaseprice",
            "billing.productrows.purchasepricesum",
            "billing.productrows.marginalincome.short",
            "billing.productrows.marginalincomeratio.short",
            "billing.productrows.marginalincome.tooltip",
            "billing.productrows.marginalincome.ratiotooltip",
            "billing.productrows.customerprice",
            "billing.productrows.editproductrow",
            "billing.productrows.edittextrow",
            "billing.productrows.householddeductiontype",
            "billing.productrows.registerhousehold.rot",
            "billing.productrows.registerhousehold.rut",
            "billing.productrows.registerhousehold.green",
            "billing.productrows.textrow.rut",
            "billing.productrows.textrow.rot",
            "billing.productrows.textrow.green",
            "billing.productrows.textrow.cooperative",
            "billing.productrows.changeatteststate",
            "billing.productrows.changeatteststate.errortitle",
            "billing.productrows.changeatteststate.errorlift",
            "billing.productrows.changeatteststate.errorstock",
            "billing.invoices.householddeduction.householddeduction",
            "billing.invoices.householddeduction.greendeduction",
            "billing.invoices.householddeduction.rutdeduction",
            "billing.productrows.nocustomerusedefaultpricelist",
            "billing.productrows.pricelistcurrencydifferfrominvoice",
            "billing.productrows.pricelistcurrencydifferfromoffer",
            "billing.productrows.pricelistcurrencydifferfromorder",
            "billing.productrows.pricelistcurrencydifferfromcontract",
            "billing.productrows.pricenotfoundforselectedwholeseller",
            "billing.productrows.defaultwholesellermissing",
            "billing.productrows.errorgettingprice",
            "billing.productrows.changepricelistrecalculate",
            "billing.productrows.calculatebuyingprice",
            "billing.productrows.purchasestatus",
            "common.customer.invoices.timerowsnotcopied",
            "common.customer.invoices.row",
            "common.customer.invoices.wrongstatetotransfer",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "common.from",
            "common.to",
            "common.customer.invoices.remindertextrow",
            "common.customer.invoices.interesttextrow",
            "common.total",
            "billing.products.taxdeductiontype",
            "billing.productrows.dialogs.openliftrowserror",
            "billing.productrows.dialogs.deductiontypewarning",
            "billing.productrows.dialogs.deductiontypewarninginfo",
            "billing.productrows.discount2",
            "billing.productrows.discount2type",
            "billing.customer.payment.fee",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];

        // Common
        // Container specific
        if (this.container == ProductRowsContainers.Order) {
            featureIds.push(Feature.Billing_Stock);
            featureIds.push(Feature.Billing_Order_Orders_Edit_ProductRows_QuantityWarning);
            featureIds.push(Feature.Billing_Contract_Contracts_Edit);
            featureIds.push(Feature.Billing_Order_Orders_Edit_ProductRows_Copy);
            featureIds.push(Feature.Billing_Order_Orders_Edit_ProductRows_Merge);
            featureIds.push(Feature.Billing_Order_Orders_Edit_ProductRows_NoDeletion);
            featureIds.push(Feature.Economy_Intrastat);
            featureIds.push(Feature.Billing_Order_Orders_Edit_ProductRows_AllowUpdatePurchasePrice);
        }
        else if (this.container == ProductRowsContainers.Invoice) {
            featureIds.push(Feature.Billing_Stock);
            featureIds.push(Feature.Billing_Invoice_Invoices_Edit_ProductRows_Copy);
            featureIds.push(Feature.Billing_Invoice_Invoices_Edit_ProductRows_AllowUpdatePurchasePrice);
            featureIds.push(Feature.Economy_Intrastat);
        }
        else if (this.container == ProductRowsContainers.Contract) {
            featureIds.push(Feature.Billing_Contract_Contracts_Edit_ProductRows_Copy);
            featureIds.push(Feature.Billing_Contract_Contracts_Edit_ProductRows_Merge);
        }
        else if (this.container == ProductRowsContainers.Offer) {
            featureIds.push(Feature.Billing_Stock);
            featureIds.push(Feature.Billing_Offer_Offers_Edit_ProductRows_Copy);
        }

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            // Common
            // Container specific
            if (this.container == ProductRowsContainers.Order) {
                this.useStock = this.stockMenu = x[Feature.Billing_Stock];
                this.warnOnReducedQuantity = x[Feature.Billing_Order_Orders_Edit_ProductRows_QuantityWarning];
                this.contractEditPermission = x[Feature.Billing_Contract_Contracts_Edit];
                this.copyProductRowsPermission = x[Feature.Billing_Order_Orders_Edit_ProductRows_Copy];
                this.mergeProductRowsPermission = x[Feature.Billing_Order_Orders_Edit_ProductRows_Merge];
                this.notAllowedToRemoveRows = x[Feature.Billing_Order_Orders_Edit_ProductRows_NoDeletion];
                this.intrastatPermission = x[Feature.Economy_Intrastat];
                this.changePurchasePricePermission = x[Feature.Billing_Order_Orders_Edit_ProductRows_AllowUpdatePurchasePrice];
            }
            else if (this.container == ProductRowsContainers.Invoice) {
                this.useStock = this.stockMenu = x[Feature.Billing_Stock];
                this.copyProductRowsPermission = x[Feature.Billing_Invoice_Invoices_Edit_ProductRows_Copy];
                this.changePurchasePricePermission = x[Feature.Billing_Invoice_Invoices_Edit_ProductRows_AllowUpdatePurchasePrice];
                this.intrastatPermission = x[Feature.Economy_Intrastat];
            }
            else if (this.container == ProductRowsContainers.Contract) {
                this.copyProductRowsPermission = x[Feature.Billing_Contract_Contracts_Edit_ProductRows_Copy];
                this.mergeProductRowsPermission = x[Feature.Billing_Contract_Contracts_Edit_ProductRows_Merge];
            }
            else if (this.container == ProductRowsContainers.Offer) {
                this.useStock = x[Feature.Billing_Stock];
                this.copyProductRowsPermission = x[Feature.Billing_Offer_Offers_Edit_ProductRows_Copy];
            }
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Product_Products_ShowPurchasePrice,
            Feature.Billing_Product_Products_ShowSalesPrice,
            Feature.Time_Project_Invoice_Edit,
            Feature.Billing_Project_TimeSheetUser_OtherEmployees,
            Feature.Billing_Purchase_Purchase_Edit,
            Feature.Billing_Purchase];

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.showPurchasePricePermission = x[Feature.Billing_Product_Products_ShowPurchasePrice];
            this.showSalesPricePermission = x[Feature.Billing_Product_Products_ShowSalesPrice];
            this.timeProjectPermission = x[Feature.Time_Project_Invoice_Edit] && x[Feature.Billing_Project_TimeSheetUser_OtherEmployees];
            this.createPurchasePermission = x[Feature.Billing_Purchase_Purchase_Edit];
            this.purchasePermission = x[Feature.Billing_Purchase];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        // Common
        settingTypes.push(CompanySettingType.BillingDefaultInvoiceProductUnit);
        settingTypes.push(CompanySettingType.BillingDefaultVatCode);
        settingTypes.push(CompanySettingType.BillingDefaultWholeseller);
        settingTypes.push(CompanySettingType.BillingEDIPriceSettingRule);

        settingTypes.push(CompanySettingType.BillingUseFreightAmount);
        settingTypes.push(CompanySettingType.ProductFreight);
        settingTypes.push(CompanySettingType.BillingUseInvoiceFee);
        settingTypes.push(CompanySettingType.ProductInvoiceFee);
        settingTypes.push(CompanySettingType.BillingUseInvoiceFeeLimit);
        settingTypes.push(CompanySettingType.BillingUseInvoiceFeeLimitAmount);
        settingTypes.push(CompanySettingType.BillingUseCentRounding);
        settingTypes.push(CompanySettingType.ProductCentRounding);
        settingTypes.push(CompanySettingType.ProductMisc);
        settingTypes.push(CompanySettingType.ProductFlatPrice);
        settingTypes.push(CompanySettingType.ProductFlatPriceKeepPrices);
        settingTypes.push(CompanySettingType.ProductGuarantee);

        settingTypes.push(CompanySettingType.BillingProductRowMarginalLimit);

        settingTypes.push(CompanySettingType.BillingMergeInvoiceProductRowsMerchandise);
        settingTypes.push(CompanySettingType.BillingMergeInvoiceProductRowsService);

        settingTypes.push(CompanySettingType.BillingDefaultHouseholdDeductionType);
        settingTypes.push(CompanySettingType.ProductHouseholdTaxDeduction);
        settingTypes.push(CompanySettingType.ProductHouseholdTaxDeductionDenied);
        settingTypes.push(CompanySettingType.ProductHousehold50TaxDeduction);
        settingTypes.push(CompanySettingType.ProductHousehold50TaxDeductionDenied);
        settingTypes.push(CompanySettingType.ProductRUTTaxDeduction);
        settingTypes.push(CompanySettingType.ProductRUTTaxDeductionDenied);
        settingTypes.push(CompanySettingType.ProductGreen15TaxDeduction);
        settingTypes.push(CompanySettingType.ProductGreen15TaxDeductionDenied);
        settingTypes.push(CompanySettingType.ProductGreen20TaxDeduction);
        settingTypes.push(CompanySettingType.ProductGreen20TaxDeductionDenied);
        settingTypes.push(CompanySettingType.ProductGreen50TaxDeduction);
        settingTypes.push(CompanySettingType.ProductGreen50TaxDeductionDenied);

        settingTypes.push(CompanySettingType.BillingDefaultStock);

        settingTypes.push(CompanySettingType.BillingUseProductGroupCustomerCategoryDiscount);

        settingTypes.push(CompanySettingType.BillingUsePartialInvoicingOnOrderRow);

        settingTypes.push(CompanySettingType.BillingHideVatRate);
        settingTypes.push(CompanySettingType.BillingHideVatWarnings);

        settingTypes.push(CompanySettingType.BillingUseExternalProductInfoLink);

        settingTypes.push(CompanySettingType.AccountCustomerDiscount);
        settingTypes.push(CompanySettingType.AccountCustomerDiscountOffset);
        settingTypes.push(CompanySettingType.BillingCalculateMarginalIncomeForRowsWithZeroPurchasePrice);
        settingTypes.push(CompanySettingType.BillingUseQuantityPrices);
        settingTypes.push(CompanySettingType.BillingShowExtendedInfoInExternalSearch);
        settingTypes.push(CompanySettingType.BillingDefaultGrossMarginCalculationType);
        settingTypes.push(CompanySettingType.BillingUseAdditionalDiscount);
        settingTypes.push(CompanySettingType.BillingShowImportProductRows);

        // Container specific
        if (this.container == ProductRowsContainers.Offer) {
            settingTypes.push(CompanySettingType.BillingStatusTransferredOfferToOrder);
            settingTypes.push(CompanySettingType.BillingStatusTransferredOfferToInvoice);
            settingTypes.push(CompanySettingType.BillingHideRowsTransferredToOrderInvoiceFromOffer);
            settingTypes.push(CompanySettingType.BillingHideWholesaleSettings);
        } else if (this.container == ProductRowsContainers.Order) {
            // TODO: Is this correct way of getting attest state 'Klar' or do we need another setting for this?
            settingTypes.push(CompanySettingType.BillingStatusOrderReadyMobile);
            settingTypes.push(CompanySettingType.BillingStatusTransferredOrderToInvoice);
            settingTypes.push(CompanySettingType.BillingHideRowsTransferredToInvoiceFromOrder);
            settingTypes.push(CompanySettingType.BillingStatusTransferredOrderToContract);
            settingTypes.push(CompanySettingType.BillingHideWholesaleSettings);
            settingTypes.push(CompanySettingType.BillingOrderAskForWholeseller);
            settingTypes.push(CompanySettingType.BillingStatusOrderDeliverFromStock);
            settingTypes.push(CompanySettingType.BillingAutoCreateDateOnProductRows);
            settingTypes.push(CompanySettingType.BillingUseEDIPriceForSalesPriceRecalculation);
        } else if (this.container == ProductRowsContainers.Invoice) {
            settingTypes.push(CompanySettingType.BillingHideWholesaleSettings);
            settingTypes.push(CompanySettingType.BillingInvoiceAskForWholeseller);
        }

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            // Common
            this.defaultProductUnitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceProductUnit);
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultVatCode);
            this.defaultWholesellerId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultWholeseller);
            this.ediPriceRule = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingEDIPriceSettingRule);

            this.useFreightAmount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseFreightAmount);
            this.freightAmountProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFreight);
            this.invoiceFeeProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductInvoiceFee);
            this.useInvoiceFee = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceFee);
            this.useInvoiceFeeLimit = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceFeeLimit);
            this.useInvoiceFeeLimitAmount = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingUseInvoiceFeeLimitAmount);
            this.useCentRounding = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseCentRounding);
            this.centRoundingProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductCentRounding);
            this.miscProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductMisc);
            this.fixedPriceProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFlatPrice);
            this.fixedPriceKeepPricesProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFlatPriceKeepPrices);
            this.loadProducts([this.freightAmountProductId, this.invoiceFeeProductId, this.centRoundingProductId, this.miscProductId], false);

            this.calculateMarginalIncomeOnZeroPurchase = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCalculateMarginalIncomeForRowsWithZeroPurchasePrice, true);
            this.amountHelper.calculateMarginalIncomeOnZeroPurchase = this.calculateMarginalIncomeOnZeroPurchase;

            this.productGuaranteeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGuarantee);
            this.amountHelper.productGuaranteeId = this.productGuaranteeId;
            this.marginalIncomeLimit = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingProductRowMarginalLimit);
            this.amountHelper.marginalIncomeLimit = this.marginalIncomeLimit;

            this.mergeProductRowsMerchandise = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingMergeInvoiceProductRowsMerchandise, this.mergeProductRowsMerchandise);
            this.mergeProductRowsService = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingMergeInvoiceProductRowsService, this.mergeProductRowsService);

            this.defaultHouseholdDeductionType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultHouseholdDeductionType);
            this.householdTaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductHouseholdTaxDeduction);
            this.householdTaxDeductionDeniedProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductHouseholdTaxDeductionDenied);
            this.household50TaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductHousehold50TaxDeduction);
            this.household50TaxDeductionDeniedProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductHousehold50TaxDeductionDenied);
            this.rutTaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductRUTTaxDeduction);
            this.rutTaxDeductionDeniedProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductRUTTaxDeductionDenied);
            this.green15TaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGreen15TaxDeduction);
            this.green15TaxDeductionDeniedProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGreen15TaxDeductionDenied);
            this.green20TaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGreen20TaxDeduction);
            this.green20TaxDeductionDeniedProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGreen20TaxDeductionDenied);
            this.green50TaxDeductionProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGreen50TaxDeduction);
            this.green50TaxDeductionDeniedProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGreen50TaxDeductionDenied);

            this.defaultStockId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultStock);

            this.useProductGroupCustomerCategoryDiscount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseProductGroupCustomerCategoryDiscount);
            this.useQuantityPrices = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseQuantityPrices);
            this.usePartialInvoicingOnOrderRow = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUsePartialInvoicingOnOrderRow);

            this.hideVatRate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideVatRate);
            this.hideVatWarnings = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideVatWarnings);

            this.showExternalProductinfoLink = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseExternalProductInfoLink);

            this.discountAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscount, 0);
            this.discountOffsetAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscountOffset, 0);
            this.useExtendSearchInfo = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingShowExtendedInfoInExternalSearch, false);//true; 
            this.grossMarginCalculationType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultGrossMarginCalculationType, 0);
            this.showAdditionalDiscount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseAdditionalDiscount, false);
            this.showImportProductRows = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingShowImportProductRows, false);
            
            // Container specific
            this.excludedAttestStates = [];
            if (this.container == ProductRowsContainers.Offer) {
                this.showWholeseller = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideWholesaleSettings);

                this.attestStateTransferredOfferToOrderId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOfferToOrder);
                if (this.attestStateTransferredOfferToOrderId !== 0 && !_.includes(this.excludedAttestStates, this.attestStateTransferredOfferToOrderId))
                    this.excludedAttestStates.push(this.attestStateTransferredOfferToOrderId);
                this.attestStateTransferredOfferToInvoiceId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOfferToInvoice);
                if (this.attestStateTransferredOfferToInvoiceId !== 0 && !_.includes(this.excludedAttestStates, this.attestStateTransferredOfferToInvoiceId))
                    this.excludedAttestStates.push(this.attestStateTransferredOfferToInvoiceId);

                this.hideTransferred = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideRowsTransferredToOrderInvoiceFromOffer);
            } else if (this.container == ProductRowsContainers.Order) {
                this.showWholeseller = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideWholesaleSettings);
                this.askForWholeseller = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingOrderAskForWholeseller);

                this.attestStateReadyId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusOrderReadyMobile);
                this.attestStateOrderDeliverFromStockId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusOrderDeliverFromStock);
                this.autoSetDateOnOrderRows = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingAutoCreateDateOnProductRows);
                this.useEDIPriceForRecalculation = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseEDIPriceForSalesPriceRecalculation);

                this.attestStateTransferredOrderToInvoiceId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToInvoice);
                if (this.attestStateTransferredOrderToInvoiceId !== 0 && !_.includes(this.excludedAttestStates, this.attestStateTransferredOrderToInvoiceId))
                    this.excludedAttestStates.push(this.attestStateTransferredOrderToInvoiceId);
                this.attestStateTransferredOrderToContractId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToContract);
                if (this.attestStateTransferredOrderToContractId !== 0 && !_.includes(this.excludedAttestStates, this.attestStateTransferredOrderToContractId))
                    this.excludedAttestStates.push(this.attestStateTransferredOrderToContractId);

                this.hideTransferred = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideRowsTransferredToInvoiceFromOrder);
            } else if (this.container == ProductRowsContainers.Invoice) {
                this.showWholeseller = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideWholesaleSettings);
                this.askForWholeseller = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingInvoiceAskForWholeseller);
            }
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingProductSearchMinPrefixLength,
            UserSettingType.BillingProductSearchMinPopulateDelay,
            UserSettingType.BillingProductSearchFilterMode,
            UserSettingType.BillingDisableWarningPopupWindows,
            UserSettingType.BillingShowWarningBeforeDeletingRow,
            UserSettingType.BillingOrderIncomeRatioVisibility,
            UserSettingType.BillingOfferLatestAttestStateTo,
        ];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.productSearchMinPrefixLength = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchMinPrefixLength, this.productSearchMinPrefixLength);
            this.productSearchMinPopulateDelay = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchMinPopulateDelay, this.productSearchMinPopulateDelay);
            this.productSearchFilterMode = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchFilterMode, this.productSearchFilterMode);
            this.disableWarningPopups = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingDisableWarningPopupWindows);
            this.showWarningBeforeRowDelete = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingShowWarningBeforeDeletingRow);
            this.hideIncomeRatioAndPercentage = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingOrderIncomeRatioVisibility);
            this.billingOfferLatestAttestStateTo = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingOfferLatestAttestStateTo);
        });
    }

    private loadUserCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingDefaultStockPlace
        ];

        return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
            const userDefaultStockId = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingDefaultStockPlace);
            if (userDefaultStockId)
                this.defaultStockId = userDefaultStockId;
        });
    }

    private saveDisableWarningPopups() {
        this.disableWarningPopups = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.BillingDisableWarningPopupWindows, this.disableWarningPopups);
    }

    private getUseEdi(): ng.IPromise<any> {
        return this.productService.useEdi().then(x => {
            this.useEdi = x;
        });
    }

    private loadUserAttestTransitions(startDate?: Date, stopDate?: Date): ng.IPromise<any> {
        if (!this.useAttestState) {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }

        let entity: TermGroup_AttestEntity = TermGroup_AttestEntity.Unknown;
        if (this.container == ProductRowsContainers.Offer)
            entity = TermGroup_AttestEntity.Offer;
        else if (this.container == ProductRowsContainers.Order)
            entity = TermGroup_AttestEntity.Order;

        return this.coreService.getUserAttestTransitions(entity, startDate, stopDate).then(x => {
            this.attestTransitions = x;
            // Add states from returned transitions
            _.forEach(this.attestTransitions, (trans) => {
                if (_.filter(this.attestStates, a => a.attestStateId === trans.attestStateToId).length === 0)
                    this.attestStates.push(trans.attestStateTo);
            });

            // Sort states
            this.attestStates = _.orderBy(this.attestStates, 'sort');

            // Get initial state
            this.initialAttestState = _.find(this.attestStates, a => a.initial === true);
            if (!this.initialAttestState) {
                this.loadInitialAttestState();
            }

            // Setup available states (exclude finished states)
            this.availableAttestStates = [];
            _.forEach(this.attestStates, (attestState) => {
                if (!_.includes(this.excludedAttestStates, attestState.attestStateId)) {
                    // Map to correct type
                    var obj = new AttestStateDTO();
                    angular.extend(obj, attestState);
                    this.availableAttestStates.push(obj);
                }
            });

            // Setup available states for selector
            this.availableAttestStateOptions = [];
            this.availableAttestStateOptions.push({ id: 0, name: this.terms["billing.productrows.changeatteststate"] });
            _.forEach(this.availableAttestStates, (a: AttestStateDTO) => {
                this.availableAttestStateOptions.push({ id: a.attestStateId, name: a.name });
            });
            this.selectedAttestState = 0;
        });
    }

    private loadInitialAttestState() {
        let entity: TermGroup_AttestEntity = TermGroup_AttestEntity.Unknown;
        if (this.container == ProductRowsContainers.Offer)
            entity = TermGroup_AttestEntity.Offer;
        else if (this.container == ProductRowsContainers.Order)
            entity = TermGroup_AttestEntity.Order;

        this.coreService.getAttestStateInitial(entity).then(x => {
            this.initialAttestState = x;

            if (!this.initialAttestState) {
                const keys: string[] = [
                    "billing.productrows.initialstatemissing.title",
                    "billing.productrows.initialstatemissing.message"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["billing.productrows.initialstatemissing.title"], terms["billing.productrows.initialstatemissing.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                });
            } else {
                this.attestStates.push(this.initialAttestState);
                // Sort states
                this.attestStates = _.orderBy(this.attestStates, 'sort');
            }
        });
    }

    private loadAllProducts(setLoading: boolean): ng.IPromise<any> {
        if (setLoading)
            this.startLoad();

        return this.productService.getInvoiceProductsSmall().then(x => {
            this.products = x;

            if (setLoading)
                this.stopProgress();
        });
    }

    private loadLiftProducts(): ng.IPromise<any> {
        return this.productService.getLiftProductsSmall().then(x => {
            this.liftProducts = x;
        });
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then(x => {
            this.productUnits = [];
            _.forEach(x, (unit) => {
                this.productUnits.push({ value: unit.productUnitId, label: unit.code });
            });

            if (this.defaultProductUnitId) {
                const unit = _.find(this.productUnits, { value: this.defaultProductUnitId });
                if (unit)
                    this.defaultProductUnitCode = unit.label;
            }
        });
    }

    private setupDiscountTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.discountTypes = [];
        this.discountTypes.push({ value: SoeInvoiceRowDiscountType.Percent, label: this.terms["billing.productrows.discounttype.percent"] });
        this.discountTypes.push({ value: SoeInvoiceRowDiscountType.Amount, label: this.terms["billing.productrows.discounttype.amount"] });
        
        this.setDiscountTypeText();
        this.setInitDiscount2()

        deferral.resolve();
        return deferral.promise;
    }

    private setDiscountTypeText() {
        _.forEach(this.productRows, row => { row.discountTypeText = this.getDiscountTypeText(row.discountType); row.discount2TypeText = this.getDiscountTypeText(row.discount2Type) });
    }

    private setInitDiscount2() {
        _.forEach(this.productRows, row => { row.discount2Type == 0 ? row.discount2Type = SoeInvoiceRowDiscountType.Percent : row.discount2Type; });
    
    }

    private getDiscountTypeText(type: number): string {
        var dt = _.find(this.discountTypes, { value: type });
        return dt ? dt.label : '';
    }

    private loadVatCodes(): ng.IPromise<any> {
        return this.productService.getVatCodes().then(x => {
            this.vatCodes = x;
            // Add empty
            var vatCode = new VatCodeDTO();
            vatCode.vatCodeId = 0;
            vatCode.accountId = 0;
            vatCode.percent = 0;
            vatCode.code = '';
            vatCode.accountNr = '';
            this.vatCodes.splice(0, 0, vatCode);

            // Assign to value/label to make select column work
            _.forEach(this.vatCodes, vatCode => {
                (<any>vatCode).value = vatCode.vatCodeId;
                (<any>vatCode).label = vatCode.code;
            });

            this.defaultVatRate = this.getDefaultVatPercent(this.defaultVatCodeId);
        });
    }

    /*
    private loadStocks(): ng.IPromise<any> {
        return this.stockService.getStocks(false).then(x => {
            this.stocks = x;
        });
    }
    */

    private loadVatAccounts(): ng.IPromise<any> {
        return this.productService.getAccountVatRates(true).then(x => {
            this.vatAccounts = x;

            // Assign to value/label to make select column work
            _.forEach(this.vatAccounts, vatAccount => {
                (<any>vatAccount).value = vatAccount.accountId;
                (<any>vatAccount).label = vatAccount.accountNr + ' ' + vatAccount.name;
            });
        });
    }

    private loadHouseholdDeductionTypes(): ng.IPromise<any> {
        return this.productService.getHouseholdDeductionTypes(true).then(x => {
            this.householdDeductionTypes = [];
            _.forEach(_.sortBy(x, 'name'), (type) => {
                this.householdDeductionTypes.push({ value: type.id, label: type.name });
            });

        });
    }

    private setHouseholdDeductionTypeText() {
        _.forEach(this.productRows, row => row.householdDeductionTypeText = this.getHouseholdDeductionTypeText(row.householdDeductionType));
    }

    private getHouseholdDeductionTypeText(type: number): string {
        const dt = _.find(this.householdDeductionTypes, { value: type });
        return dt ? dt.label : '';
    }

    private setStocksForProduct(row: ProductRowDTO, productChanged = false) {

        if (!this.useStock) {
            row.stocksForProduct = [];
            return;
        }

        if (!row.stocksForProduct) {
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
                        this.stockChanged(row);
                    }
                    else {
                        row.stockId = 0;
                        row.stockCode = "";
                    }
                }

                this.soeGridOptions.refreshRows(row);
            });
        }
    }

    private loadProduct(productId: number, row: ProductRowDTO, skipWholesellerDialog: boolean = false): ng.IPromise<any> {
        const existingProduct = _.filter(this.productList, p => p.productId === productId);
        // Don't fetch if we already have the product in the list
        if (productId === 0 || existingProduct.length > 0) {
            if (existingProduct.length == 1 && row) {
                this.productChanged(row, existingProduct[0], false, skipWholesellerDialog);
            }
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }

        return this.productService.getProductForProductRows(productId).then(x => {
            this.productList.push(x);
            if (row)
                this.productChanged(row, x, false, skipWholesellerDialog);
        });
    }

    private loadProducts(productIds: number[], setProductValuesAfterLoad: boolean) {
        if (productIds.length === 0)
            return;

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
            });
        }
        else {
            if (setProductValuesAfterLoad)
                this.setProductRowExtentions();
        }
    }

    private setProductRowExtentions() {
        _.forEach(this.productRows, row => {
            // Set values from product
            if (row.productId) {
                const product = _.find(this.productList, p => p.productId === row.productId);
                if (product) {
                    row.productNr = product.number;
                    row.productName = product.name;
                    row.setCalculationTypeFlag(product.calculationType);
                }
            }

            // Set values from product unit
            if (row.productUnitId) {
                const unit = _.find(this.productUnits, u => u.value === row.productUnitId);
                if (unit)
                    row.productUnitCode = unit.label;
            }

            // Set values from VAT account
            if (row.vatAccountId) {
                const vatAccount = _.find(this.vatAccounts, a => a.accountId === row.vatAccountId);
                if (vatAccount) {
                    row.vatAccountNr = vatAccount.accountNr;
                    row.vatAccountName = vatAccount.name;
                }
            }

            // Set values from VAT code
            if (row.vatCodeId) {
                const vatCode = _.find(this.vatCodes, v => v.vatCodeId === row.vatCodeId);
                if (vatCode)
                    row.vatCodeCode = vatCode.code;
            }

            // Set values from deduction type
            if (row.householdDeductionType) {
                const householdDeductionType = _.find(this.householdDeductionTypes, d => d.value === row.householdDeductionType);
                if (householdDeductionType)
                    row.householdDeductionTypeText = householdDeductionType.label;
            }

            /*
            // Set values from stock
            if (row.stockId) {
                var stock = _.find(this.stocks, s => s.stockId === row.stockId);
                if (stock)
                    row.stockCode = stock.code;
            }
            */
            // get stocks for product           
            row.stocksForProduct = [];
        });

        this.calculateAmounts();

        this.soeGridOptions.refreshRows();
    }

    private loadProductVatAccount(row: ProductRowDTO, isTimeProjectRow = false) {
        this.productService.getProductAccounts(row.tempRowId, row.productId, 0, this.customer ? this.customer.actorCustomerId : 0, 0, this.vatType, false, false, true, false, isTimeProjectRow, this.tripartiteTrade === true).then(x => {
            if (x && x.vatAccountDim1Id !== 0)
                this.setProductVatAccount(row, x);
            else if (this.defaultVatCodeId !== 0)
                this.setProductVatAccountFromVatCode(row, this.defaultVatCodeId);
            else
                this.amountHelper.calculateRowSum(row, false);
        });
    }

    private getProductPrice(row: ProductRowDTO, productId: number, timeRowIsLoadingProductPrice: boolean, updatePurchasePrice = false) {
        // Products without prices
        if (productId === 0 || productId === this.fixedPriceProductId || productId === this.fixedPriceKeepPricesProductId || productId === this.miscProductId)
            return;

        // Get price for selected product based on specified pricelist
        this.productService.getProductPrice(this.priceListTypeId, productId, this.customer ? this.customer.actorCustomerId : 0, this.currencyId, this.wholesellerId ? this.wholesellerId : 0, row.quantity, false, true).then((result: IInvoiceProductPriceResult) => {
            this.productPriceFetched(row, result, timeRowIsLoadingProductPrice, updatePurchasePrice);
        });
    }

    private getProductPrices(rows: ProductRowDTO[], updatePurchasePrice = false, useCurrentWholeseller = true) {
        const products: ProductPricesRowRequestDTO[] = [];
        const failureMessages: string[] = []
        const customerProducts = this.customer ? this.customer.customerProducts : null;

        let customerProductRows = [];
        if (customerProducts && customerProducts.length > 0) {
            customerProductRows = rows.filter(x => _.find(customerProducts, { productId: x.productId }));
        }

        let rowsToRecalculate = [];
        if (this.priceListTypeIsProject || !customerProducts) {
            rowsToRecalculate = rows;
        } else {
            _.forEach(rows, (r) => {
                if (!_.find(customerProducts, p => p.productId === r.productId))
                    rowsToRecalculate.push(r);
            });
        }

        rowsToRecalculate.forEach((row: ProductRowDTO) => {
            const purchasePrice = this.useEDIPriceForRecalculation && row.ediEntryId ? row.purchasePrice : null;
            const priceRow = new ProductPricesRowRequestDTO(row.tempRowId, row.productId, row.quantity, row.sysWholesellerName, purchasePrice);
            products.push(priceRow);
        });

        //Check customer prices after server fetch if project pricelist.
        if (!this.priceListTypeIsProject) {
            this.setCustomerProductPrices(customerProductRows, customerProducts);
        }

        const wholesellerId = (useCurrentWholeseller && this.wholesellerId) ? this.wholesellerId : 0;

        this.fetchProductPrices(products, wholesellerId, updatePurchasePrice, failureMessages);
    }

    private setCustomerProductPrice(row: ProductRowDTO, customerProduct: ICustomerProductPriceSmallDTO) {
        row.amountCurrency = customerProduct.price;
        this.initChangeAmount(row);
    }

    private setCustomerProductPrices(rows: ProductRowDTO[], customerProducts: ICustomerProductPriceSmallDTO[]) {
        rows.forEach((row: ProductRowDTO) => {
            const customerProduct = customerProducts.find(r => r.productId === row.productId);
            if (customerProduct) {
                this.setCustomerProductPrice(row, customerProduct);
            }
        });
    }

    private fetchProductPrices(products: ProductPricesRowRequestDTO[], wholesellerId: number, updatePurchasePrice: boolean, failureMessages: string[]) {

        const getPricesDTO = new ProductPricesRequestDTO();
        getPricesDTO.products = products;
        getPricesDTO.priceListTypeId = this.priceListTypeId;
        getPricesDTO.customerId = this.customer ? this.customer.actorCustomerId : 0;
        getPricesDTO.currencyId = this.currencyId;
        getPricesDTO.wholesellerId = wholesellerId;
        getPricesDTO.copySysProduct = true;
        getPricesDTO.includeCustomerPrices = true;

        this.progress.startWorkProgress((completion) => {
            this.productService.getProductPrices(getPricesDTO).then((result: any) => {
                if (result.length > 0) {
                    let notify = false;
                    result.forEach(item => {
                        if (!notify && item.success)
                            notify = true;

                        const row = this.productRows.find(r => r.tempRowId === item.rowId);
                        if (row) {
                            const messages = this.productPriceFetched(row, item, false, updatePurchasePrice, false, false);
                            messages.forEach(m => {
                                failureMessages.push(m);
                            });
                        }
                    });
                    this.resetRows();

                    const keys: string[] = [
                        "core.info",
                        "common.wasupdated",
                        "billing.productrows.article",
                        "billing.productrows.articles",
                        "billing.productrows.dialogs.wholesellercouldntupdate"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        let first = true;
                        let message = "";
                        failureMessages.forEach(m => {
                            if (first) {
                                message = m;
                                first = false;
                            }
                            else {
                                message += "<br/>" + "----------------------------------------" + "<br/>" + m;
                            }
                        });

                        completion.completed(null, false, message);

                        if (notify)
                            this.setParentAsModified();
                    });
                } else {
                    completion.completed(null, true);
                }
            });
        });
    }

    private productPriceFetched(row: ProductRowDTO, result: IInvoiceProductPriceResult, timeRowIsLoadingProductPrice: boolean, updatePurchasePrice = false, showDialogs = true, refreshRows = true): string[] {
        const warningMsg: string[] = [];
        if (row)
            row.isTimeProjectRow = row.isTimeProjectRow ? true : timeRowIsLoadingProductPrice;

        if (row && this.ediPriceRule === TermGroup_EDIPriceSettingRule.UsePriceRulesKeepEDIPurchasePrice && row.ediEntryId)
            updatePurchasePrice = false;

        if (row && row.grossMarginCalculationType === TermGroup_GrossMarginCalculationType.StockAveragePrice)
            updatePurchasePrice = false;

        if (!this.customer || this.customer.actorCustomerId === 0 && this.warnNoCustomer) {
            warningMsg.push(this.terms["billing.productrows.nocustomerusedefaultpricelist"]);
            if (showDialogs) {
                this.notificationService.showDialog(this.terms["core.warning"], this.terms["billing.productrows.nocustomerusedefaultpricelist"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK).result.then(() => { this.soeGridOptions.refocusCell(); });
            }

            // Only show this warning once (per session)
            this.warnNoCustomer = false;
        }

        if (result.success) {
            if (result.warning && !this.disableWarningPopups) {
                warningMsg.push(result.errorMessage);
                if (showDialogs) {
                    this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium, true).result.then(() => { this.soeGridOptions.refocusCell(); });
                }
            }

            if (result.currencyDiffer && this.warnDifferentCurrency) {
                //Set key depending on origin type
                let key: string = "";
                switch (this.originType) {
                    case SoeOriginType.Offer:
                        key = "billing.productrows.pricelistcurrencydifferfromoffer";
                        break;
                    case SoeOriginType.Order:
                        key = "billing.productrows.pricelistcurrencydifferfromorder";
                        break;
                    case SoeOriginType.CustomerInvoice:
                        key = "billing.productrows.pricelistcurrencydifferfrominvoice";
                        break;
                    case SoeOriginType.Contract:
                        key = "billing.productrows.pricelistcurrencydifferfromcontract";
                        break;
                }

                warningMsg.push(this.terms[key]);
                if (showDialogs)
                    this.notificationService.showDialog(this.terms["core.warning"], this.terms[key], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK).result.then(result => { this.soeGridOptions.refocusCell(); });

                // Only show this warning once (per session)
                this.warnDifferentCurrency = false;
            }

            if (row) {
                if (result.productIsSupplementCharge) {
                    row.supplementChargePercent = result.salesPrice.round(2);
                    row.isSupplementChargeProduct = true;
                } else {
                    // Do not update price when product is internal or salesprice is zero
                    const isExternal = !!row.sysWholesellerName;
                    if (isExternal || result.salesPrice > 0 || (result.salesPrice === 0 && !result.warning)) {
                        row.amount = result.salesPrice.round(4); //.round(2);
                    }

                    // Set product unit
                    if (result.productUnit) {
                        const productUnit = _.find(this.productUnits, p => p.label.toLowerCase() == result.productUnit.toLowerCase());
                        if (productUnit) {
                            row.productUnitCode = productUnit.label;
                            row.productUnitId = productUnit.value;
                        }
                    }

                    // If the currencies differ, interpret it as if we got the
                    // base currency amount and calculate transaction currency.
                    //
                    // However, this leads to problems if the price is in i.e.USD and
                    // the transaction currency is EUR while the base currency is SEK.
                    //  -> We will interpret it as if the received price is in SEK.
                    // This should be fixed in the future.
                    //
                    // What we don't want is recalculating if the price is in the correct
                    // currency.

                    if (result.currencyDiffer) {
                        this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
                    } else {
                        row.amountCurrency = row.amount;
                        this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                    }
                }

                if (updatePurchasePrice && !row.ediEntryId) {
                    //purchaseprice is always base currency...
                    row.purchasePrice = result.purchasePrice.round(5); //.round(2);
                    this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.PurchasePrice, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
                }

                row.amountFormula = result.priceFormula;
                row.sysWholesellerName = result.sysWholesellerName;

                if (result.discountPercent) {
                    row.discountPercent = row.discountValue = result.discountPercent;
                    row.discountType = SoeInvoiceRowDiscountType.Percent;
                }

                this.amountHelper.calculateRowSum(row);
            }
        } else {
            if (result.errorNumber == ActionResultSelect.PriceNotFound) {

                if (row && this.useProductGroupCustomerCategoryDiscount && result.discountPercent !== 0) {
                    row.discountPercent = result.discountPercent ? result.discountPercent : 0;
                    row.discountValue = result.discountPercent ? result.discountPercent : 0;
                }

                let msg: string = "{0}: {1} - {2}".format(this.terms["billing.productrows.rownr"], row ? row.rowNr.toString() : "?", this.terms["billing.productrows.pricenotfoundforselectedwholeseller"].format(result.sysWholesellerName ? result.sysWholesellerName : "?"));
                if (this.defaultWholesellerId === 0)
                    msg += "\n" + this.terms["billing.productrows.defaultwholesellermissing"];

                warningMsg.push(msg);

                if (showDialogs) {
                    if (row)
                        this.searchProduct(row, null, msg);
                    else if (!this.disableWarningPopups) {
                        const modal = this.notificationService.showDialog(this.terms["core.warning"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium, false, true, this.terms["core.donotshowagain"]);
                        modal.result.then(result => {
                            if (result.isChecked)
                                this.saveDisableWarningPopups();

                            this.soeGridOptions.refocusCell();
                        });
                    }
                }
            } else {
                if (!this.disableWarningPopups) {
                    let msg: string = this.terms["billing.productrows.errorgettingprice"] + ". \n\n" + result.errorMessage;
                    warningMsg.push(msg);
                    if (showDialogs) {
                        const modal = this.notificationService.showDialog(this.terms["core.warning"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium, false, true, this.terms["core.donotshowagain"]);
                        modal.result.then(result => {
                            if (result.isChecked)
                                this.saveDisableWarningPopups();

                            this.soeGridOptions.refocusCell();
                        });
                    }
                }
            }

            if (row) {
                row.amount = 0;
                row.amountCurrency = 0;
                row.sumAmount = 0;
                row.sumAmountCurrency = 0;
                // Keep purchase price from product
                /*row.purchasePrice = 0;
                row.purchasePriceCurrency = 0;*/
                row.amountFormula = '';
                row.sysWholesellerName = '';
            }
        }

        if (row && refreshRows)
            this.soeGridOptions.refreshRows(row);

        return warningMsg;
    }

    private getFreightAmount() {
        if (this.crediting || this.copying) {
            if (this.invoiceFeeLoaded) {
                this.invoiceFeeLoaded = false;
                this.crediting = false;
                this.copying = false;
                return;
            }
            else {
                this.freightAmountLoaded = true;
                return;
            }
        }

        if (this.loading || !this.useFreightAmount)
            return;

        this.productService.getProductPriceDecimal(this.priceListTypeId, this.freightAmountProductId).then(x => {
            if (this.billingType == TermGroup_BillingType.Credit)
                x = 0;

            if (this.freightAmount !== x) {
                this.freightAmount = x;
                this.amountHelper.getCurrencyAmount(this.freightAmount, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency).then(am => { this.freightAmountCurrency = am });
            }
        });
    }

    private getInvoiceFee() {
        if (this.crediting || this.copying) {
            if (this.invoiceFeeLoaded) {
                this.invoiceFeeLoaded = false;
                this.crediting = false;
                this.copying = false;
                return;
            }
            else {
                this.freightAmountLoaded = true;
                return;
            }
        }

        if (!this.useInvoiceFee || this.disableInvoiceFee || this.invoiceFeeManuallyChanged)
            return;

        this.productService.getProductPriceDecimal(this.priceListTypeId, this.invoiceFeeProductId).then(x => {
            if (this.billingType === TermGroup_BillingType.Credit)
                x = 0;

            if (this.invoiceFee !== x) {
                this.invoiceFee = x;
                this.amountHelper.getCurrencyAmount(this.invoiceFee, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency).then(am => { this.invoiceFeeCurrency = am });
            }
        });
    }

    // FUNCTIONS

    private executeRowFunction(option) {
        switch (option.id) {
            case ProductRowsRowFunctions.AddProduct:
                this.addProduct();
                break;
            case ProductRowsRowFunctions.ImportRow:
                this.importProductRow();
                break;
            case ProductRowsRowFunctions.RefreshProducts:
                this.loadAllProducts(true);
                break;
            case ProductRowsRowFunctions.ChangeWholeseller:
                this.changeWholeSeller();
                break;
            case ProductRowsRowFunctions.ChangeDeductionType:
                this.changeDeductionType();
                break;
            case ProductRowsRowFunctions.ChangeDiscount:
                this.changeProductDiscount();
                break;
            case ProductRowsRowFunctions.RecalculatePrices:
                this.initRecalculatePrices();
                break;
            case ProductRowsRowFunctions.SetStock:
                this.setRowStockStatus();
                break;
            case ProductRowsRowFunctions.SortRowsByProductNr:
                this.sortRowsByProductNumber();
                break;
            case ProductRowsRowFunctions.CopyRows:
                this.copyRows(option.id);
                break;
            case ProductRowsRowFunctions.CopyRowsToContract:
                this.copyRows(option.id, false, false, true);
                break;
            case ProductRowsRowFunctions.MergeRows:
                this.tryMergeProductRows();
                break;
            case ProductRowsRowFunctions.MoveRows:
                this.moveRowsToOther(null, null);
                break;
            case ProductRowsRowFunctions.MoveRowsWithinOrder:
                this.copyRows(option.id, true, true);
                break;
            case ProductRowsRowFunctions.DeleteRows:
                this.initDeleteRows();
                break;
            case ProductRowsRowFunctions.UppercaseRows:
                this.uppercaseRows();
                break;
            case ProductRowsRowFunctions.ShowAllSums:
                this.calculateProductRowSums();
                break;
            case ProductRowsRowFunctions.SplitAccounting:
                this.initSplitAccountingRows();
                break;
            case ProductRowsRowFunctions.ShowExternalProductinfoLink:
                this.ShowExternalProductInfo();
                break;
            case ProductRowsRowFunctions.ShowDeletedRows:
                break;
            case ProductRowsRowFunctions.UnlockRows:
                break;
            case ProductRowsRowFunctions.RenumberRows:
                this.reNumberRows();
                break;
            case ProductRowsRowFunctions.MoveRowsToStock:
                this.moveRowsToStock();
                break;
            case ProductRowsRowFunctions.ShowTimeRows:
                this.showTimeRows();
                break;
            case ProductRowsRowFunctions.RecalculateTimeRow:
                this.recalculateTimeRows();
                break;
            case ProductRowsRowFunctions.CreatePurchase:
                this.createPurchase();
                break;
            case ProductRowsRowFunctions.ChangeIntrastatCode:
                this.changeIntrastat();
                break;
        }
    }

    private importProductRow() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Directives/ProductRows/Views/ImportProductRowDialog.html"),
            controller: ImportProductRowController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {}
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.progress.startWorkProgress((completion) => {
                    return this.coreService.importProductRows(this.wholesellerId, this.invoiceId, this.customer.actorCustomerId, result.typeId, result.bytes).then(res => {
                        if (res.success && res.value.$values) {
                            res.value.$values.forEach(r => {
                                this.addRowImport(SoeInvoiceRowType.ProductRow, r);
                            })
                            this.updateVatType();
                            completion.completed(null, true);
                        } else {
                            completion.failed(res.errorMessage, false);
                        }
                    })
                });
            }
        }, () => {
        });        
    }

    private addRowImport(type: SoeInvoiceRowType, productRow: ProductRowDTO) {
            productRow.productUnitCode = _.find(this.productUnits, u => u.value === productRow.productUnitId).label;
            productRow.quantity = productRow._quantity || 1;
            productRow.purchasePriceCurrency = productRow._purchasePriceCurrency,
            productRow.rowNr = ProductRowDTO.getNextRowNr(_.filter(this.activeRows, (r) => !(r.type === SoeInvoiceRowType.AccountingRow)));
            productRow.purchasePriceSum = productRow._quantity && productRow._purchasePriceCurrency ? productRow._quantity * productRow._purchasePriceCurrency : 0;
            productRow.productNr = this.getSmallProduct(productRow.productId).number;
            productRow.discountTypeText = this.getDiscountTypeText(productRow.discountType);
            productRow.discount2TypeText = this.getDiscountTypeText(productRow.discount2Type);
            productRow.householdDeductionTypeText = this.getHouseholdDeductionTypeText(productRow.householdDeductionType);

            this.setPurchasePrice(productRow, productRow.purchasePrice, productRow.sysWholesellerName);
            this.setAttestStateValues(productRow);

            this.productRows.push(productRow);
            this.setRowAsModified(productRow, true);
            this.resetRows();
    }

    private addProduct(productNumber: string = null, row: ProductRowDTO = null) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Products/Products/Views/edit.html"),
            controller: ProductsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {}

        });
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                presetProductNumber: productNumber
            });
        });

        modal.result.then(product => {
            if (product) {
                this.loadProduct(product.productId, row);
                this.products.push({ productId: product.productId, number: product.number, name: product.name, numberName: product.number + " " + product.name });
            }
        });
    }

    private setPurchasePrice(row: any, purchasePrice: number, sysWholesellerName: string = undefined, priceFormula: string = undefined): ng.IPromise<any> {
        row.purchasePrice = purchasePrice.round(5); //.round(2);

        if (priceFormula) {
            row.amountFormula = priceFormula;
        }

        if (sysWholesellerName) {
            row.sysWholesellerName = sysWholesellerName;
        }

        return this.amountHelper.getCurrencyAmount(row.purchasePrice, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency).then(am => {
            row.purchasePriceCurrency = am;
        })
    }

    private changeWholeSeller() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ChangeWholeseller/ChangeWholeseller.html"),
            controller: ChangeWholesellerController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                productService: () => { return this.productService },
                productRows: () => { return this.soeGridOptions.getSelectedRows() },
                values: () => { return this.wholesellers },
                priceListTypeId: () => { return this.priceListTypeId },
                customerId: () => { return this.customer.actorCustomerId },
                currencyId: () => { return this.currencyId },
                functionType: () => { return ProductRowsRowFunctions.ChangeWholeseller }
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                let countFailures: number = 0;
                _.forEach(result, (item: any) => {
                    if (!item.success) {
                        if (item.errorNumber === ActionResultSelect.PriceNotFound)
                            countFailures++;
                    }
                    else {
                        var row = _.find(this.productRows, { tempRowId: item.rowId });
                        if (row) {
                            if (item.productIsSupplementCharge) {
                                row.supplementChargePercent = item.salesPrice.round(2);
                                row.isSupplementChargeProduct = true;
                            }
                            else {
                                row.amount = item.salesPrice.round(4); //round(2);
                                this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
                            }

                            this.setPurchasePrice(row, item.purchasePrice, item.sysWholesellerName, item.priceFormula).then(() => {
                                this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.PurchasePrice, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
                                this.setRowAsModified(row, false);
                                this.amountHelper.calculateRowSum(row);
                                this.soeGridOptions.refreshRows(row);
                            })
                        }
                    }
                });

                const keys: string[] = [
                    "core.info",
                    "common.wasupdated",
                    "billing.productrows.article",
                    "billing.productrows.articles",
                    "billing.productrows.dialogs.wholesellercouldntupdate"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    let message: string = "";
                    const countSuccess: number = result.length - countFailures;
                    if (countSuccess > 0) {
                        message += countSuccess + " " + (countSuccess > 1 ? terms["billing.productrows.articles"] : terms["billing.productrows.article"]) + " " + terms["common.wasupdated"] + "<br/>";
                        this.setParentAsModified();
                    }
                    if (countFailures > 0)
                        message += countFailures + " " + (countFailures > 1 ? terms["billing.productrows.articles"] : terms["billing.productrows.article"]) + " " + terms["billing.productrows.dialogs.wholesellercouldntupdate"] + "<br/>";
                    this.notificationService.showDialog(terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                });

            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    private createPurchase() {
        const selectedRows: ProductRowDTO[] = this.soeGridOptions.getSelectedRows();
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/CreatePurchase/CreatePurchase.html"),
            controller: CreatePurchaseController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => this.translationService,
                productService: () => this.productService,
                productRows: () => selectedRows,
                invoiceId: () => this.invoiceId,
                onGoingInvoiceNr: () => this.invoiceNr,
                notificationService: () => this.notificationService,
                urlHelperService: () => this.urlHelperService,
                currencyId: () => this.currencyId,
            }
        });

        modal.result.then((result: any[]) => {
            if (result) {
                result.forEach(r => {
                    const row = selectedRows.find(x => x.customerInvoiceRowId === r.customerInvoiceRowId);
                    this.setGrossMarginCalculationType(row);
                    let rowChange = false;
                    if (r.purchasePriceCurrency) {
                        if (row && (row.purchasePriceCurrency !== r.purchasePriceCurrency && row.grossMarginCalculationType != TermGroup_GrossMarginCalculationType.StockAveragePrice)) {
                            row.purchasePriceCurrency = r.purchasePriceCurrency;
                            rowChange = true
                        }
                        if (row && (row.stockId !== r.stockId)) {
                            row.stockId = r.stockId;
                            row.stockCode = r.stockCode;
                            rowChange = true
                        }
                        if (rowChange) {
                            this.setRowAsModified(row, true);
                            this.soeGridOptions.refreshRows(row);
                        }
                    }
                    if (!row.purchaseId) {
                        row.purchaseId = r.purchaseId;
                        row.purchaseNr = r.purchaseNr;
                    }
                })
            }
        }, function () {
            console.log("cancel");
        });

        return modal;
    }

    private changeIntrastat() {
        var tempRows: IntrastatTransactionDTO[] = [];
        _.forEach(_.filter(this.soeGridOptions.getSelectedRows(), r => r.type === SoeInvoiceRowType.ProductRow && !r.isExpenseRow && !r.isTimeProjectRow && !r.isHouseholdRow && !r.isFreightAmountRow && !r.isInvoiceFeeRow && !r.isCentRoundingRow && !r.isModified), (r: ProductRowDTO) => {
            // Get product to check vattype
            let isService = false;
            let weight = 0;
            var product = this.getFullProduct(r.productId);

            if (product) {
                if (product.vatType === TermGroup_InvoiceProductVatType.Service)
                    isService = true;
                weight = product.weight;
            }

            if (!isService) {
                const dto = new IntrastatTransactionDTO();
                dto.rowNr = r.rowNr;
                dto.customerInvoiceRowId = r.customerInvoiceRowId;
                dto.intrastatTransactionId = r.intrastatTransactionId;
                dto.intrastatCodeId = r.intrastatCodeId;
                dto.sysCountryId = r.sysCountryId;
                dto.originId = this.invoiceId;
                dto.productName = r.productName;
                dto.productNr = r.productNr;
                dto.productUnitId = r.productUnitId;
                dto.productUnitCode = r.productUnitCode;
                dto.quantity = r.quantity;
                dto.state = SoeEntityState.Active;
                dto.netWeight= weight;
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
                originType: () => this.originType,
                originId: () => this.invoiceId,
                notificationService: () => this.notificationService,
                urlHelperService: () => this.urlHelperService,
                totalAmount: () => undefined,
            }
        });

        modal.result.then((result: any[]) => {
            if (result) {
                this.reloadParent();
            }
        }, function () {
            console.log("cancel");
        });

        return modal;
    }

    private changeDeductionType() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ChangeWholeseller/ChangeWholeseller.html"),
            controller: ChangeWholesellerController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                productService: () => { return this.productService },
                productRows: () => { return this.soeGridOptions.getSelectedRows() },
                values: () => { return this.householdDeductionTypes },
                priceListTypeId: () => { return this.priceListTypeId },
                customerId: () => { return this.customer.actorCustomerId },
                currencyId: () => { return this.currencyId },
                functionType: () => { return ProductRowsRowFunctions.ChangeDeductionType }
            }
        });

        modal.result.then((rows: any) => {
            if (rows) {
                _.forEach(rows, (item: any) => {
                    const row = _.find(this.productRows, { tempRowId: item.rowId });
                    if (row) {
                        row.householdDeductionType = item.deductionId;
                        row.householdDeductionTypeText = item.deductionName
                        this.setRowAsModified(row, true);
                        this.soeGridOptions.refreshRows(row);
                    }
                });
            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    private changeProductDiscount() {

        if (this.hasFixedPriceProducts()) {
            const keys: string[] = [
                "billing.productrows.dialogs.fixedpricefound",
                "billing.productrows.dialogs.fixedpricefoundremove",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["billing.productrows.dialogs.fixedpricefound"], terms["billing.productrows.dialogs.fixedpricefoundremove"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
            });
        }
        else {
            const rows = [];
            this.soeGridOptions.getSelectedRows().forEach((row) => {
                const obj = new ProductRowDTO();
                angular.extend(obj, row);
                rows.push(obj);
            });

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ChangeDiscount/ChangeDiscount.html"),
                controller: ChangeDiscountController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    productService: () => { return this.productService },
                    productRows: () => { return rows },
                    useAdditionalDiscount: () => { return this.showAdditionalDiscount },
                }
            });

            modal.result.then((result: any) => {
                if (result) {
                    let countFailures = 0;
                    _.forEach(result.rows, (item: any) => {
                        const row = this.productRows.find(r => r.tempRowId === item.tempRowId);
                        if (row) {
                            const updateSupplement = ((result.supplement != null) && result.supplement != row.supplementCharge);
                            if ((result.discount != null) && (result.discount !== row.discountPercent)) {
                                row.discountPercent = result.discount;
                                row.discountType = SoeInvoiceRowDiscountType.Percent;
                                row.discountValue = result.discount;
                                this.amountHelper.calculateRowSum(row);
                                this.setRowAsModified(row, false);
                                this.soeGridOptions.refreshRows(row);
                            }
                            if ((result.discount2 != null) && (result.discount2 !== row.discount2Percent)) {
                                row.discount2Percent = result.discount2;
                                row.discount2Type = SoeInvoiceRowDiscountType.Percent;
                                row.discount2Value = result.discount2;
                                this.amountHelper.calculateRowSum(row);
                                this.setRowAsModified(row, false);
                                this.soeGridOptions.refreshRows(row);
                            }
                            if (updateSupplement) {
                                row.supplementCharge = result.supplement;
                                if (this.supplementChargeChanged(row))
                                    this.setRowAsModified(row, false);
                                else
                                    countFailures++;
                            }
                            if (result.ratio != null && result.ratio != row.marginalIncomeRatio && row.purchasePrice > 0) {
                                row.marginalIncomeRatio = NumberUtility.parseNumericDecimal(result.ratio);
                                this.marginalIncomeRatioChanged(row);
                                this.setRowAsModified(row, false);
                            }
                        }
                    });

                    this.setParentAsModified();

                    const keys: string[] = [
                        "core.info",
                        "common.wasupdated",
                        "common.wasntupdated",
                        "billing.productrows.article",
                        "billing.productrows.articles"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        let message: string = "";
                        const countSuccess: number = result.rows.length - countFailures;
                        if (countSuccess > 0)
                            message += countSuccess + " " + (countSuccess > 1 ? terms["billing.productrows.articles"] : terms["billing.productrows.article"]) + " " + terms["common.wasupdated"] + "<br/>";
                        if (countFailures > 0)
                            message += countFailures + " " + (countFailures > 1 ? terms["billing.productrows.articles"] : terms["billing.productrows.article"]) + " " + terms["common.wasntupdated"] + "<br/>";
                        this.notificationService.showDialog(terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                    });
                }
            }, function () {
                // Cancelled
            });

            return modal;
        }
    }

    private recalculatePricesDialog() {
        if (this.activeProductRows.length > 0) {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["billing.productrows.changepricelistrecalculate"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.terms["billing.productrows.calculatebuyingprice"]);
            modal.result.then(result => {
                this.recalculatePrices(result.isChecked, true);
                this.getFreightAmount();
                this.getInvoiceFee();
            });
        }
    }

    private initRecalculatePrices() {
        const keys: string[] = [
            "core.question",
            "billing.productrows.function.purchasepricequestion",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.question"], terms["billing.productrows.function.purchasepricequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel);
            modal.result.then(val => {
                if(val === true || val === false)
                    this.recalculatePrices(val);
            });
        });
    }

    private recalculatePrices(updatePurchasePrice = false, priceListChanged = false) {
        let productRows: ProductRowDTO[] = priceListChanged ? this.getOnlyProductRows() : this.soeGridOptions.getSelectedRows();

        if (this.container == ProductRowsContainers.Offer || this.container == ProductRowsContainers.Order)
            productRows = productRows.filter(p => p.attestStateId === this.initialAttestState.attestStateId);

        const rowsToCollect = [];
        productRows.forEach(r => {

            if (!((this.miscProductId !== 0 && r.productId === this.miscProductId) ||
                (this.fixedPriceProductId !== 0 && r.productId === this.fixedPriceProductId) ||
                (this.isTaxDeductionRow(r)) ||
                (r.isInvoiceFeeRow) ||
                (r.supplierInvoiceId) ||
                (r.isFreightAmountRow) ||
                (r.isExpenseRow)
                //(r.ediEntryId !== null && this.ediPriceRule === TermGroup_EDIPriceSettingRule.UseEDIPurchasePriceWithSupplementCharge)
            )) {
                rowsToCollect.push(r);
            }
        });

        if (rowsToCollect.length > 0) {
            this.getProductPrices(rowsToCollect, updatePurchasePrice, false);
        }
    }

    private setRowStockStatus() {
        const rows = _.filter(this.selectedUnattestedRows, r => r.type === SoeInvoiceRowType.ProductRow && r.householdAmountCurrency === 0);
        if (rows.length > 0) {
            const keys: string[] = [
                "billing.productrows.dialogs.setstock",
                "billing.productrows.productrow",
                "billing.productrows.productrows",
                "billing.productrows.dialogs.setstockquestion"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var message: string = "";
                if (rows.length > 1)
                    message += rows.length + " " + terms["billing.productrows.productrows"] + " " + terms["billing.productrows.dialogs.setstockquestion"];
                else
                    message += rows.length + " " + terms["billing.productrows.productrow"] + " " + terms["billing.productrows.dialogs.setstockquestion"];

                const modal = this.notificationService.showDialog(terms["billing.productrows.dialogs.setstock"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                modal.result.then(val => {
                    _.forEach(rows, (row: ProductRowDTO) => {
                        row.isStockRow = true;
                        this.setRowAsModified(row, false);
                    });
                    this.setParentAsModified();
                });
            });
        }
    }

    private sortRowsByProductNumber() {
        const keys: string[] = [
            "core.warning",
            "billing.productrows.dialogs.numbersortingwarning",
            "billing.productrows.functions.sortseparately"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.warning"], terms["billing.productrows.dialogs.numbersortingwarning"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, terms["billing.productrows.functions.sortseparately"], false);
            modal.result.then(val => {
                this.customSorting("productnumber", true, val.isChecked);
            });
        });
    }

    private allowTimeRowFunc(): boolean {
        const result = this.gridHasSelectedRows;
        if (!result)
            return false;
        const selectedRows: ProductRowDTO[] = this.soeGridOptions.getSelectedRows();
        if (selectedRows.length !== 1) {
            return false;
        }

        return selectedRows[0].isTimeBillingRow || selectedRows[0].isTimeProjectRow;
    }

    private recalculateTimeRows() {
        const rowsToShow: ProductRowDTO[] = this.soeGridOptions.getSelectedRows();
        const row = rowsToShow[0];
        if (row && row.isTimeProjectRow) {
            if (row.attestStateId !== this.initialAttestState.attestStateId) {
                return;
            }

            this.startWorkModal(null);

            this.orderService.recalculateTimeRow(row.customerInvoiceRowId).then((result: IActionResult) => {
                this.stopProgress();
                if (result.success) {
                    this.reloadParent();
                }
                else {
                    this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                }
            });
        }
    }

    private showTimeRows() {

        const rowsToShow: ProductRowDTO[] = this.soeGridOptions.getSelectedRows();
        const row = rowsToShow[0];

        if (row) {
            const isReadonly = (row.attestStateId !== this.initialAttestState.attestStateId);
            const timeRowsHelper = new TimeRowsHelper(this.guid, this.$q, this.$uibModal, this.$scope, this.messagingService, this.urlHelperService, this.translationService, this.orderService, this.coreService, this.invoiceId, row.customerInvoiceRowId);
            timeRowsHelper.showTimeRows(row, this.activeProductRows, isReadonly).then(() => {
                if (timeRowsHelper.reloadInvoiceAfterClose) {
                    this.reloadParent();
                }
            })
        }
    }

    private moveRowsToStock() {
        const rowsToMove: ProductRowDTO[] = this.soeGridOptions.getSelectedRows();

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/MoveProductRowsToStock/MoveProductRowsToStock.html"),
            controller: MoveProductRowsToStockController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                notificationService: () => { return this.notificationService },
                stockService: () => { return this.stockService },
                coreService: () => { return this.coreService },
                productRows: () => { return rowsToMove },
                invoiceId: () => { return this.invoiceId },
            }
        });

        modal.result.then((result: any) => {
            if (result.success) {
                let rowsToDelete: ProductRowDTO[] = [];
                _.forEach(result.changedRows, (r: ProductRowDTO) => {
                    if (r.quantity === 0) {
                        rowsToDelete.push(r);
                    }
                    else {
                        this.soeGridOptions.refreshRows(r);
                    }
                });
                if (rowsToDelete.length > 0) {
                    this.deleteRows(rowsToDelete, true);
                }
                this.setParentAsModified();
            }
        }, function () {
            // Cancelled
        });
    }

    private moveRowsToOther(rows: ProductRowDTO[], quantities: number[]): ng.IPromise<boolean> {

        const deferral = this.$q.defer<boolean>();

        var selectedIds: any[] = [];
        var rowsToMove: ProductRowDTO[] = [];

        if (rows) {
            selectedIds = _.map(rows, 'tempRowId');
        }
        else {
            selectedIds = _.map(this.soeGridOptions.getSelectedRows(), 'tempRowId');
            rows = _.filter(this.activeRows, r => !r.isTimeProjectRow && r.type != SoeInvoiceRowType.AccountingRow && r.type != SoeInvoiceRowType.BaseProductRow && r.type != SoeInvoiceRowType.SubTotalRow && !r.isHouseholdRow);
        }

        for (var i = 0; i < rows.length; i++) {
            var clonedRow = _.cloneDeep(rows[i]);

            if (clonedRow.type == SoeInvoiceRowType.TextRow) {
                clonedRow.productId = 0;
                clonedRow.productName = "";
                clonedRow.productNr = "";
            }
            if (quantities) {
                clonedRow.quantity = quantities[i];
            }

            rowsToMove.push(clonedRow);
        }

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/CopyProductRows/CopyProductRows.html"),
            controller: CopyProductRowsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                productRows: () => { return rowsToMove },
                selectedProductRows: () => { return selectedIds },
                originType: () => { return this.originType },
                buttonFunction: () => { return ProductRowsRowFunctions.MoveRows },
                move: () => { return true },
                within: () => { return false },
                toContract: () => { return false },
                invoiceId: () => { return this.invoiceId }
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                //Copy or transfer to other order
                _.forEach(result.rows, (r) => {
                    var row = _.find(this.activeRows, { tempRowId: r.id });
                    if (row) {
                        if (!row.quantity || row.quantity === r.quantity) {
                            this.deleteRow(row, true);
                        }
                        else {
                            row.quantity = row.quantity - r.quantity;
                            this.amountHelper.calculateRowSum(row);
                            this.soeGridOptions.refreshRows(row);
                            this.setParentAsModified();
                        }
                    }
                });
            }
            deferral.resolve(true);
        }, (x) => {
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    private copyRows(buttonFunction: ProductRowsRowFunctions, move: boolean = false, within: boolean = false, toContract: boolean = false) {
        var selectedIds: any[] = [];
        var rowsToCopy: ProductRowDTO[] = [];
        var rowsToMove: ProductRowDTO[] = [];

        if (within) {
            if (this.hasNonSortableRows())
                return;

            rowsToMove = this.soeGridOptions.getSelectedRows();
            _.forEach(_.filter(this.activeRows, r => r.type != SoeInvoiceRowType.AccountingRow && !_.includes(rowsToMove, r)), (row: ProductRowDTO) => {
                rowsToCopy.push(row);
            });
            rowsToCopy = _.orderBy(rowsToCopy, 'rowNr');
        } else {
            selectedIds = _.map(this.soeGridOptions.getSelectedRows(), 'tempRowId');
            _.forEach(_.filter(this.activeRows, r => !r.isTimeProjectRow && r.type != SoeInvoiceRowType.AccountingRow && r.type != SoeInvoiceRowType.BaseProductRow && r.type != SoeInvoiceRowType.SubTotalRow && !r.isHouseholdRow), (row: ProductRowDTO) => {
                var clonedRow = _.cloneDeep(row);

                if (clonedRow.type == SoeInvoiceRowType.TextRow) {
                    clonedRow.productId = 0;
                    clonedRow.productName = "";
                    clonedRow.productNr = "";
                }

                rowsToCopy.push(clonedRow);
            });
        }

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/CopyProductRows/CopyProductRows.html"),
            controller: CopyProductRowsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                productRows: () => { return rowsToCopy },
                selectedProductRows: () => { return selectedIds },
                originType: () => { return this.originType },
                buttonFunction: () => { return buttonFunction },
                move: () => { return move },
                within: () => { return within },
                toContract: () => { return toContract },
                invoiceId: () => { return this.invoiceId }
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                if (within) {
                    // Move within order
                    if (result.moveTo) {
                        this.moveRows(rowsToMove, result.position === 1, result.moveTo);
                    }
                }
                else if (move) {
                    //Copy or transfer to other order
                    _.forEach(result.rows, (r) => {
                        var row = _.find(this.activeRows, { tempRowId: r.id });
                        if (row) {
                            if (!row.quantity || row.quantity === r.quantity) {
                                this.deleteRow(row, true);
                            }
                            else {
                                row.quantity = row.quantity - r.quantity;
                                this.amountHelper.calculateRowSum(row);
                                this.soeGridOptions.refreshRows(row);
                                this.setParentAsModified();
                            }
                        }
                    });
                } else if (buttonFunction === ProductRowsRowFunctions.CopyRows && result.targetInvoiceId === this.invoiceId) {
                    //Copying row(s) to the same order
                    this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_ORDER_PRODUCT_ROWS_COPIED, { guid: this.parentGuid, invoiceId: this.invoiceId }));
                }
            }
        }, function () {
            // Cancelled
        });
    }

    private tryMergeProductRows() {
        const deletedRows: ProductRowDTO[] = [];
        const changedRows: ProductRowDTO[] = [];
        let selectedValidRows: ProductRowDTO[] = _.filter(this.soeGridOptions.getSelectedRows(), r => (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow) && (this.container == ProductRowsContainers.Contract || r.attestStateId === this.initialAttestState.attestStateId));

        if (selectedValidRows.length > 1) {
            _.forEach(selectedValidRows, (row: ProductRowDTO) => {
                let deletedRow = _.find(deletedRows, function (x) { return x.rowNr == row.rowNr });
                if (!deletedRow) {
                    _.forEach(selectedValidRows, (rowItem: ProductRowDTO) => {
                        if (rowItem.state === SoeEntityState.Active &&
                            rowItem.productId === row.productId &&
                            rowItem.tempRowId != row.tempRowId &&
                            rowItem.productId != this.miscProductId &&
                            rowItem.amountCurrency === row.amountCurrency &&
                            rowItem.discountType === row.discountType &&
                            rowItem.discountValue === row.discountValue &&
                            rowItem.sysWholesellerName === row.sysWholesellerName &&
                            rowItem.purchasePrice === row.purchasePrice) {
                            //Add quantity to target
                            row.quantity += rowItem.quantity;
                            //Set reference on deleted row
                            rowItem.mergeToId = row.customerInvoiceRowId;
                            deletedRows.push(rowItem);
                            if (changedRows.indexOf(row) < 1) {
                                changedRows.push(row);
                            }
                        }
                    });
                }
            });

            _.forEach(deletedRows, (rowItem: ProductRowDTO) => {
                this.setRowAsModified(rowItem, false);
                this.deleteRow(rowItem);
            });

            _.forEach(changedRows, (rowItem: ProductRowDTO) => {
                this.setRowAsModified(rowItem, false);
                this.amountHelper.calculateRowSum(rowItem);
            });

            const keys: string[] = [
                "core.info",
                "billing.productrows.dialogs.merged",
                "billing.productrows.dialogs.notmerged",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const nbrOfSelectedRows = selectedValidRows.length;
                const nbrOfMergedRows = deletedRows.length + changedRows.length;

                var msg: string = "";
                if (nbrOfMergedRows === 0) {
                    msg += "{0} {1}".format((nbrOfSelectedRows - nbrOfMergedRows).toString(), terms["billing.productrows.dialogs.notmerged"]);
                }
                else {
                    msg += "{0} {1}".format(nbrOfMergedRows.toString(), terms["billing.productrows.dialogs.merged"]);
                    if (nbrOfSelectedRows > nbrOfMergedRows)
                        msg += "\n" + "{0} {1}".format((nbrOfSelectedRows - nbrOfMergedRows).toString(), terms["billing.productrows.dialogs.notmerged"]);
                    this.reNumberRows();
                    this.resetRows();
                    this.calculateAmounts();
                }
                this.notificationService.showDialog(terms["core.info"], msg, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
            });
        }
    }

    private initSplitAccountingRows() {
        const validRows: ProductRowDTO[] = [];
        _.forEach(this.soeGridOptions.getSelectedRows(), (r) => {
            if (this.originType === SoeOriginType.CustomerInvoice) {
                if (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow)
                    validRows.push(r);
            }
            else if (this.originType === SoeOriginType.Contract) {
                if (!r.isLiftProduct && (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow))
                    validRows.push(r);
            }
            else {
                if (r.attestStateId === this.initialAttestState.attestStateId && !r.isLiftProduct && (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow))
                    validRows.push(r);
            }
        });

        if (validRows.length === 1) {
            if (validRows[0].customerInvoiceRowId && validRows[0].customerInvoiceRowId > 0) {
                return this.orderService.getSplitAccountingRows(validRows[0].customerInvoiceRowId, true).then(x => {
                    validRows[0].splitAccountingRows = x;
                    _.forEach(validRows[0].splitAccountingRows, (r) => {
                        r.excludeFromSplit = (r.dim1Id === this.discountAccountId || r.dim1Id === this.discountOffsetAccountId);
                    });
                    this.splitAccountingRows(validRows, validRows[0].splitAccountingRows);
                });
            }
            else {
                const splitRow = new SplitAccountingRowDTO();
                splitRow.splitValue = 100;
                splitRow.splitType = SoeInvoiceRowDiscountType.Percent;
                splitRow.amountCurrency = validRows[0].sumAmountCurrency;

                this.splitAccountingRows(validRows, [splitRow]);
            }
        }
        else if (validRows.length > 1) {
            const mergedSplitRow = new SplitAccountingRowDTO();
            mergedSplitRow.splitValue = 100;
            mergedSplitRow.splitType = SoeInvoiceRowDiscountType.Percent;
            mergedSplitRow.amountCurrency = _.sum(_.map(validRows, r => this.priceListTypeInclusiveVat ? (r.sumAmountCurrency - r.vatAmountCurrency).round(2) : r.sumAmountCurrency));

            this.splitAccountingRows(validRows, [mergedSplitRow]);
        }
    }

    private splitAccountingRows(validRows: ProductRowDTO[], splitRows: SplitAccountingRowDTO[]) {

        let amount = 0;
        if (validRows.length === 1) {
            const row = validRows[0];
            amount = this.priceListTypeInclusiveVat ? (row.sumAmountCurrency - row.vatAmountCurrency).round(2) : row.sumAmountCurrency;
        }
        else {
            amount = splitRows[0].amountCurrency;
        }

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Directives/ProductRows/Views/SplitAccountingDialog.html"),
            controller: SplitAccountingDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                isReadonly: () => { return this.readOnly },
                accountingRows: () => { return splitRows },
                productRowAmount: () => { return amount },
                isCredit: () => { return this.isCredit },
                multipleRowSplit: () => { return validRows.length > 1 },
                negativeRow: () => { return validRows.length === 1 ? amount < 0 : false; }
            }
        }
        const modal = this.$uibModal.open(options);
        modal.result.then((result: any) => {
            if (result) {
                if (validRows.length > 1) {
                    _.forEach(validRows, (row) => {
                        row.splitAccountingRows = [];

                        //this.calculateRowSum(row, changeTo, handleCount === (this.accountingRows.length), total);
                        var handleCount = 0;
                        var total = 0;
                        _.forEach(result, (accRow) => {
                            var clonedRow = _.cloneDeep(accRow);
                            clonedRow.amountCurrency = handleCount === result.length ? (row.sumAmountCurrency - total).round(2) : (row.sumAmountCurrency * clonedRow.splitValue / 100).round(2);

                            const isNegative = clonedRow.amountCurrency < 0;

                            clonedRow.isCreditRow = !isNegative;
                            clonedRow.isDebitRow = isNegative;

                            clonedRow.creditAmountCurrency = isNegative ? clonedRow.amountCurrency : 0;
                            clonedRow.debitAmountCurrency = isNegative ? 0 : clonedRow.amountCurrency;

                            row.splitAccountingRows.push(clonedRow);

                            total += row.amountCurrency;
                            handleCount++;
                        });

                        row.isModified = true;
                    })
                }
                else {
                    validRows[0].splitAccountingRows = result;
                    validRows[0].isModified = true;
                }

                this.soeGridOptions.refreshRows();
                this.setParentAsModified();
            }

        }, (result: any) => {
            if (validRows.length === 1)
                validRows[0].splitAccountingRows = [];
        });
    }

    private calculateProductRowSums() {
        var fixedPriceRows: number = 0;
        var liftRows: number = 0;
        var productRows: number = 0;
        var serviceRows: number = 0;
        var totalRows: number = 0;

        const keys: string[] = [
            "billing.productrows.dialogs.productrowssummary",
            "billing.productrows.dialogs.sumfixed",
            "billing.productrows.dialogs.sumlift",
            "billing.productrows.dialogs.sumservice",
            "billing.productrows.dialogs.summerchandise",
            "billing.productrows.dialogs.sumtotal"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            _.forEach(this.activeRows, (row: ProductRowDTO) => {
                if ((row.type === SoeInvoiceRowType.ProductRow || row.type === SoeInvoiceRowType.BaseProductRow) && row.state != SoeEntityState.Deleted && row.productId != null) {
                    if (row.productId === this.fixedPriceKeepPricesProductId || row.productId == this.fixedPriceProductId) {
                        fixedPriceRows += row.sumAmountCurrency;

                        if (row.productId == this.fixedPriceKeepPricesProductId)
                            totalRows += row.sumAmountCurrency;
                    }
                    else if (row.isLiftProduct) {
                        liftRows += row.sumAmountCurrency;
                    }
                    else {
                        var product: ProductRowsProductDTO = _.find(this.productList, p => p.productId == row.productId);

                        if (product != null) {
                            if (product.vatType == TermGroup_InvoiceProductVatType.Service) {
                                serviceRows += row.sumAmountCurrency;
                                totalRows += row.sumAmountCurrency;
                            }
                            else {
                                productRows += row.sumAmountCurrency;
                                totalRows += row.sumAmountCurrency;
                            }
                        }
                    }
                }
            });

            const message = terms["billing.productrows.dialogs.sumfixed"] + " " + fixedPriceRows + "<br/>" +
                terms["billing.productrows.dialogs.sumlift"] + " " + liftRows + "<br/>" +
                terms["billing.productrows.dialogs.sumservice"] + " " + serviceRows + "<br/>" +
                terms["billing.productrows.dialogs.summerchandise"] + " " + productRows + "<br/>" +
                terms["billing.productrows.dialogs.sumtotal"] + " " + totalRows + "<br/>"
            this.notificationService.showDialog(terms["billing.productrows.dialogs.productrowssummary"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
        });
    }

    private ShowExternalProductInfo() {
        const selectedRows = this.soeGridOptions.getSelectedRows();
        const selectedIds: number[] = _.map(selectedRows, p => p.productId);

        this.productService.getProductExternalUrl(selectedIds).then(urls => {
            if (urls.length == 0) {
                this.translationService.translate("common.searchinvoiceproduct.noexternalproductinfo").then(term => {
                    this.notificationService.showDialog(this.terms["core.info"], term, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                })
            }
            else {
                urls.forEach(url => {
                    window.open(url, '_blank');
                })
            }
        });
    }

    private reNumberRows(ignoreOrderByRowNr = false, forceReset = true) {
        let i = 1;

        if (ignoreOrderByRowNr) {
            _.forEach(_.filter(this.activeRows, r => r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow || r.type === SoeInvoiceRowType.BaseProductRow), r => {
                if (r.type === SoeInvoiceRowType.BaseProductRow)
                    r.rowNr = 0;
                else
                    r.rowNr = i++;
            });
        } else {
            _.forEach(_.orderBy(_.filter(this.activeRows, r => r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow || r.type === SoeInvoiceRowType.BaseProductRow), 'rowNr'), r => {
                const oldRowNr = r.rowNr;

                if (r.type === SoeInvoiceRowType.BaseProductRow)
                    r.rowNr = 0;
                else
                    r.rowNr = i++;

                if (oldRowNr && oldRowNr !== r.rowNr) {
                    r.isModified = true;
                }
            });
        }

        this.amountHelper.calculateSubTotals(this.activeRows);
        if (forceReset) {
            this.resetRows(true, true);
        }
    }

    private multiplyRowNr() {
        _.forEach(this.activeRows, x => {
            x.rowNr *= 100;
        });
    }

    private sortFirst() {
        // Get current row
        const handledRows: number[] = [];
        const rows: ProductRowDTO[] = this.soeGridOptions.getSelectedRows().sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.soeGridOptions.getCurrentRow());

        rows.forEach((row) => {
            if (!handledRows.find((id) => id === row.tempRowId)) {

                if (handledRows.length === 0) {
                    _.forEach(_.filter(this.activeRows, r => r.rowNr > 0 && r.rowNr <= row.rowNr), (r) => {
                        this.setRowAsModified(r, false);
                    });
                }

                // Move row to the top
                row.rowNr = -(rows.length - handledRows.length);

                if (this.isParentRow(row)) {
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
                }
                handledRows.push(row.tempRowId);
            }
        });

        this.afterSortMultiple(rows, null);
    }

    private sortUp() {
        // Get current row
        const handledRows: number[] = [];
        const rows: ProductRowDTO[] = this.soeGridOptions.getSelectedRows().sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.soeGridOptions.getCurrentRow());

        if (rows.length > 0) {
            this.multiplyRowNr();

            // Move current row before previous row
            rows.forEach((row) => {
                const prevRow = row.parentRowId && row.parentRowId > 0 ? _.last(_.sortBy(_.filter(this.activeRows, (r) => r.rowNr < row.rowNr && r.customerInvoiceRowId !== row.parentRowId), r => r.rowNr)) : _.last(_.sortBy(_.filter(this.activeRows, (r) => r.rowNr < row.rowNr), r => r.rowNr));

                if (prevRow) {
                    row.rowNr = prevRow.rowNr - (rows.length - handledRows.length) - 10;

                    if (this.isParentRow(row)) {
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
                        // Move it some more to get before the row above its parent row
                        const parent = this.getParentRow(row);
                        if (parent) {
                            parent.rowNr = prevRow.rowNr - (rows.length - handledRows.length) - 10;
                            this.setRowAsModified(parent, false);
                            // Move its parent row to be directly before
                            let rowNr = parent.rowNr + 1;
                            this.getChildRows(parent).forEach(child => {
                                child.rowNr = rowNr;
                                rowNr++;
                                this.setRowAsModified(child, false);
                                handledRows.push(child.tempRowId);
                            });
                            handledRows.push(parent.tempRowId);
                        }
                    } else if (this.isChildRow(prevRow)) {
                        // Current row is not a parent nor a child row
                        // But the previous row is a child row
                        // Move current row some more to get before its parent row
                        row.rowNr -= 2;
                    }

                    handledRows.push(row.tempRowId);

                    if (this.isParentRow(prevRow)) {
                        let rowNr = prevRow.rowNr + 1;
                        this.getChildRows(prevRow).forEach(child => {
                            child.rowNr = rowNr;
                            this.setRowAsModified(child, false);
                            rowNr++;
                        });
                    }
                    this.setRowAsModified(prevRow);
                }
            });

            this.afterSortMultiple(rows, null);
        }
    }

    private sortDown() {
        // Get current row
        const handledRows: number[] = [];
        const rows: ProductRowDTO[] = this.soeGridOptions.getSelectedRows().filter(r => r.rowNr < this.activeRows.length).sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.soeGridOptions.getCurrentRow());

        if (rows.length > 0) {
            this.multiplyRowNr();
            rows.forEach((row) => {
                // Get next row
                let nextRow = _.head(_.sortBy(_.filter(this.activeRows, r => r.rowNr > row.rowNr && !_.find(rows, (sr) => sr.tempRowId === r.tempRowId) &&
                    (!r.parentRowId || (row.customerInvoiceRowId && r.parentRowId !== row.customerInvoiceRowId) || (!row.customerInvoiceRowId && r.parentRowId !== row.tempRowId))), 'rowNr'));

                if (this.isParentRow(nextRow))
                    nextRow = _.last(_.sortBy(this.getChildRows(nextRow), (r) => r.rowNr));

                if (nextRow) {
                    if (!handledRows.find((id) => id === row.tempRowId)) {
                        // Move current row after next row                    
                        row.rowNr = nextRow.rowNr + (rows.length + handledRows.length) + 10;

                        if (this.isParentRow(row)) {
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
                                parent.rowNr = nextRow.rowNr + (rows.length + handledRows.length) + 10;
                                this.setRowAsModified(parent, false);
                                // Move its parent row to be directly before
                                let rowNr = parent.rowNr + 1;
                                this.getChildRows(parent).forEach(child => {
                                    child.rowNr = rowNr;
                                    rowNr++;
                                    this.setRowAsModified(child, false);
                                    handledRows.push(child.tempRowId);
                                });
                                handledRows.push(parent.tempRowId);
                            }
                        } else if (this.isParentRow(nextRow)) {
                            // Current row is not a parent nor a child row
                            // But the next row is a parent row
                            // Move current row some more to get after its child row
                            row.rowNr += 100;
                        }
                        handledRows.push(row.tempRowId);
                    }

                    if (this.isParentRow(nextRow)) {
                        let rowNr = nextRow.rowNr + 1;
                        this.getChildRows(nextRow).forEach(child => {
                            child.rowNr = rowNr;
                            this.setRowAsModified(child, false);
                            rowNr++;
                        });
                    }
                    else if (this.isChildRow(nextRow)) {
                        // Current row is a child row
                        // Move its parent row to be directly before
                        const parent = this.getParentRow(nextRow);
                        if (parent) {
                            this.setRowAsModified(parent, false);
                            // Move its parent row to be directly before
                            let rowNr = parent.rowNr + 1;
                            this.getChildRows(parent).forEach(child => {
                                child.rowNr = rowNr;
                                rowNr++;
                                this.setRowAsModified(child, false);
                            });
                        }
                    }
                    this.setRowAsModified(nextRow);
                }
            });

            this.afterSortMultiple(rows, null);
        }
    }

    private sortLast() {
        const handledRows: number[] = [];
        const rows: ProductRowDTO[] = this.soeGridOptions.getSelectedRows().filter(r => r.rowNr <= this.activeRows.length).sort(r => r.rowNr);
        if (rows.length === 0)
            rows.push(this.soeGridOptions.getCurrentRow());

        rows.forEach((row) => {
            if (!handledRows.find((id) => id === row.tempRowId)) {
                if (handledRows.length === 0) {
                    _.forEach(_.filter(this.activeRows, r => r.rowNr >= row.rowNr), (r) => {
                        this.setRowAsModified(r, false);
                    });

                }

                // Move row to the bottom
                row.rowNr = NumberUtility.max(this.activeRows, 'rowNr') + 2 + handledRows.length;

                if (this.isParentRow(row)) {
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
                }

                handledRows.push(row.tempRowId);
            }
        });

        this.afterSortMultiple(rows, null);
    }

    private afterSortMultiple(rows: ProductRowDTO[], nextPrevRow: ProductRowDTO) {
        rows.forEach((row) => {
            this.setRowAsModified(row, false);
        });

        if (nextPrevRow) {
            this.setRowAsModified(nextPrevRow, false);
        }

        this.reNumberRows();
        //const currentPos = this.visibleRows.indexOf(row);
        //this.soeGridOptions.setFocusedCell(currentPos, this.getProductNrColumn());
    }

    private afterSort(row: ProductRowDTO, nextPrevRow: ProductRowDTO) {
        this.setRowAsModified(row, false);
        if (nextPrevRow) {
            this.setRowAsModified(nextPrevRow, false);
        }

        this.reNumberRows();
        const currentPos = this.visibleRows.indexOf(row);
        this.soeGridOptions.setFocusedCell(currentPos, this.getProductNrColumn());
    }

    private customSorting(columnName: string, renumber: boolean, sortRowTypeSeparately = false) {
        switch (columnName.toLowerCase()) {
            case "productnumber":
                if (!this.hasNonSortableRows()) {
                    this.sortedNeedsSave = true;
                    this.sortByProductNumber(sortRowTypeSeparately);
                }
                else
                    return;
                break;
        }

        if (renumber) {
            this.reNumberRows(true);
        }
        this.resetRows();
    }

    private sortByProductNumber(sortRowTypeSeparately = false) {
        let rows: ProductRowDTO[] = [];

        if (this.reverseSorting) {
            rows = _.filter(this.productRows, r => (r.type == SoeInvoiceRowType.ProductRow || r.type == SoeInvoiceRowType.BaseProductRow) && (!r.parentRowId || r.parentRowId === 0)).reverse();
            this.reverseSorting = false;
        }
        else {
            rows = _.filter(this.productRows, r => (r.type == SoeInvoiceRowType.ProductRow || r.type == SoeInvoiceRowType.BaseProductRow) && (!r.parentRowId || r.parentRowId === 0)).sort((a, b) => (a.productNr > b.productNr) ? 1 : (b.productNr > a.productNr) ? -1 : 0);

            if (sortRowTypeSeparately) {
                const rowsWithTimeRow = rows.filter(r => r.isTimeProjectRow === true);
                rows = rowsWithTimeRow.concat(rows.filter(r => r.isTimeProjectRow === false));
            }

            this.reverseSorting = true;
        }

        const rowsWithParents = this.productRows.filter(r => r.parentRowId != null && r.parentRowId !== 0);
        const rowsWithoutParents = this.productRows.filter(r => !_.find(rows, rp => rp.tempRowId === r.tempRowId) && !_.find(rowsWithParents, rp => rp.tempRowId === r.tempRowId));
        rowsWithParents.forEach((row) => {
            const parentRow = _.find(rows, r => r.tempRowId === row.parentRowId);
            if (parentRow) {
                const index = rows.indexOf(parentRow);
                const parentRowTemp = rows.slice(0, index + 1);
                parentRowTemp.push(row);
                rows = parentRowTemp.concat(rows.slice(index + 1));
            }
        });

        let accountRowTypes = rowsWithoutParents.filter(r => r.type === SoeInvoiceRowType.AccountingRow);
        accountRowTypes = accountRowTypes.concat(rowsWithoutParents.filter(r => r.type !== SoeInvoiceRowType.AccountingRow));
        rows = rows.concat(accountRowTypes);

        //Set all modified
        rows.forEach((row) => {
            if (row.type !== SoeInvoiceRowType.AccountingRow)
                row.isModified = true;
        });

        this.productRows = rows;
        this.setParentAsModified();
    }

    private executeAddRowFunction(option) {
        let type: SoeInvoiceRowType;
        switch (option.id) {
            case ProductRowsAddRowFunctions.Product:
                type = SoeInvoiceRowType.ProductRow;
                break;
            case ProductRowsAddRowFunctions.Text:
                type = SoeInvoiceRowType.TextRow;
                break;
            case ProductRowsAddRowFunctions.PageBreak:
                type = SoeInvoiceRowType.PageBreakRow;
                break;
            case ProductRowsAddRowFunctions.SubTotal:
                type = SoeInvoiceRowType.SubTotalRow;
                break;
        }

        this.soeGridOptions.stopEditing(false);
        this.addRow(type, true, true, false, option.fixedPrice ? option.fixedPrice : false);
    }

    private addRow(type: SoeInvoiceRowType, setFocus: boolean, insertAtCurrentRow = false, timeProjectRow = false, fixedPriceRow = false): { rowIndex: number, row: ProductRowDTO, column: any } {
        // Create a new row and set some properties

        const row: ProductRowDTO = new ProductRowDTO();
        let reNumberRows = false;
        row.type = type;
        row.tempRowId = ++this.internalIdCounter;
        row.state = SoeEntityState.Active;
        let selectedRow = undefined;

        const breakRow = _.find(this.activeRows, r => r.type === 5);

        if (breakRow)
            insertAtCurrentRow = false;

        switch (type) {
            case SoeInvoiceRowType.ProductRow:
            case SoeInvoiceRowType.TextRow:
            case SoeInvoiceRowType.PageBreakRow:
            case SoeInvoiceRowType.SubTotalRow:
                if (insertAtCurrentRow) {
                    selectedRow = this.soeGridOptions.getCurrentRow();

                    if (!selectedRow) {
                        var selectRows = this.soeGridOptions.getSelectedRows();
                        if (selectRows.length > 0) {
                            selectedRow = selectRows[0];
                        }
                    }

                    if (selectedRow) {
                        reNumberRows = true;
                        var childRows = this.activeRows.filter((r) => r.parentRowId === selectedRow.tempRowId);
                        if (childRows && childRows.length > 0) {
                            var childRow = _.last(_.orderBy(childRows, 'rowNr'));
                            if (childRow)
                                row.rowNr = childRow.rowNr;
                            else
                                row.rowNr = selectedRow.rowNr;
                        }
                        else {
                            row.rowNr = selectedRow.rowNr;
                        }
                    }
                    else {
                        row.rowNr = ProductRowDTO.getNextRowNr(_.filter(this.activeRows, (r) => !(r.type === SoeInvoiceRowType.AccountingRow)));
                    }
                } else {
                    row.rowNr = ProductRowDTO.getNextRowNr(_.filter(this.activeRows, (r) => !(r.type === SoeInvoiceRowType.AccountingRow)));
                }

                row.attestStateId = this.initialAttestState ? this.initialAttestState.attestStateId : null;
                row.attestStateName = this.initialAttestState ? this.initialAttestState.name : '';
                row.attestStateColor = this.initialAttestState ? this.initialAttestState.color : "#FFFFFF"; // White
                break;
            default:
                row.rowNr = 0;
        }

        // Set type specific properties
        let focusColumn: number = this.getRowNrColumn();
        switch (type) {
            case SoeInvoiceRowType.ProductRow:
                row.quantity = 1;
                row.productUnitId = this.defaultProductUnitId;
                row.productUnitCode = this.defaultProductUnitCode;
                row.amountCurrency = 0;
                row.discountValue = 0;
                row.discount2Value = 0;
                row.discountType = row.discount2Type = SoeInvoiceRowDiscountType.Percent;
                row.discountTypeText = this.getDiscountTypeText(row.discountType);
                row.discount2TypeText = this.getDiscountTypeText(row.discount2Type);
                row.currencyCode = this.transactionCurrencyCode;
                row.ediTextValue = this.terms["core.no"];
                row.isTimeProjectRow = timeProjectRow;
                row.vatAccountEnabled = (this.vatType !== TermGroup_InvoiceVatType.Contractor && this.vatType !== TermGroup_InvoiceVatType.NoVat);
                if (this.defaultHouseholdDeductionType !== 0)
                    row.householdDeductionType = this.defaultHouseholdDeductionType;

                focusColumn = this.getProductNrColumn();

                this.amountHelper.calculateRowSum(row);
                if (fixedPriceRow) {
                    this.productChangedFromId(row, this.fixedPriceProductId);
                }

                if (this.originType == SoeOriginType.Order && this.autoSetDateOnOrderRows)
                    row.date = CalendarUtility.getDateToday();
                break;
            case SoeInvoiceRowType.TextRow:
                //    this.deleteRowData(row, "type", "text", "tempRowId", "state", "rowNr", "attestStateId", "attestStateName", "attestStateColor");
                focusColumn = this.getSingelValueColumn();

                if (this.originType == SoeOriginType.Order && this.autoSetDateOnOrderRows)
                    row.date = CalendarUtility.getDateToday();
                break;
            case SoeInvoiceRowType.PageBreakRow:
                row.text = this.terms['billing.productrows.addpagebreak'];
                focusColumn = 0;
                //this.deleteRowData(row, "type", "text", "tempRowId", "state", "rowNr", "attestStateId", "attestStateName", "attestStateColor", "isReadOnly");
                break;
            case SoeInvoiceRowType.SubTotalRow:
                row.text = this.terms['billing.productrows.addsubtotal'];
                focusColumn = 0;
                //this.deleteRowData(row, "type", "text", "tempRowId", "state", "rowNr", "attestStateId", "attestStateName", "attestStateColor", "isReadOnly");
                break;
        }

        this.setRowTypeIcon(row);
        this.setRowAsModified(row, (type == SoeInvoiceRowType.SubTotalRow || type == SoeInvoiceRowType.PageBreakRow));

        // Add the row to the collection
        if (!this.productRows)
            this.productRows = [];

        this.productRows.push(row);
        this.productRowsChanged();

        // If sub total row was added, calculate sub totals
        if (type === SoeInvoiceRowType.SubTotalRow) {
            this.amountHelper.calculateSubTotals(this.activeRows);
        }

        if (reNumberRows) {
            this.reNumberRows(false, false);
            _.forEach(_.filter(this.productRows, r => r.rowNr > row.rowNr), productRow => {
                this.setRowAsModified(productRow, false);
            });
        }

        this.resetRows(false, true);

        if (setFocus && focusColumn) {
            this.soeGridOptions.startEditingCell(row, focusColumn);
        }

        if (this.showAllRows)
            this.expandAllRows();


        return { rowIndex: this.soeGridOptions.getRowIndexFor(row), column: focusColumn, row };
    }

    private initResetRowsForCopying(isCredit = false) {
        const keys: string[] = [
            "billing.invoices.copy.recalculatepriceslabel",
            "billing.invoices.copy.recalculatepricesmessage"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialogDefButton(terms["billing.invoices.copy.recalculatepriceslabel"], terms["billing.invoices.copy.recalculatepricesmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo, SOEMessageBoxButton.Yes);
            modal.result.then(recalculate => {
                this.resetRowsForCopying(isCredit, recalculate);
            });
        });
    }

    private resetRowsForCopying(isCredit = false, recalculate = false) {
        let counter = 0;
        this.productRows.forEach((r) => {
            //if (this.crediting)
            r.prevCustomerInvoiceRowId = r.customerInvoiceRowId;

            r.customerInvoiceRowId = undefined;
            r.tempRowId = counter;
            r.isReadOnly = (r.type === SoeInvoiceRowType.PageBreakRow);

            // Purchase
            r.purchaseId = undefined;
            r.purchaseNr = undefined;

            if (this.initialAttestState) {
                r.attestStateId = this.initialAttestState.attestStateId;
                r.attestStateName = this.initialAttestState.name;
                r.attestStateColor = this.initialAttestState.color;
            }

            if (this.originType === SoeOriginType.CustomerInvoice)
                r.isTimeProjectRow = false;

            r.isModified = true;
            counter = counter + 1;
        });

        let hasTimeProjectRows = false;
        if (!isCredit && this.originType === SoeOriginType.Order) {

            _.forEach(_.filter(this.productRows, (r) => (r.type === SoeInvoiceRowType.ProductRow && r.isTimeProjectRow || r.type === SoeInvoiceRowType.AccountingRow)), (r) => {
                if (r.isTimeProjectRow)
                    hasTimeProjectRows = true;

                this.deleteRow(r);
            });
        }
        else {
            _.forEach(_.filter(this.productRows, (r) => (r.type === SoeInvoiceRowType.AccountingRow)), (r) => {
                this.deleteRow(r);
            });
        }

        this.soeGridOptions.refreshRows();

        if (hasTimeProjectRows)
            this.notificationService.showDialog(this.terms["core.info"], this.terms["common.customer.invoices.timerowsnotcopied"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);

        if (isCredit) {
            this.isCredit = this.isCredit ? false : true; //Must be before revers so later calculateAmounts now what its a credit or not
            this.reverseRowAmounts(true);
        }

        if (this.invoiceFeeCurrency) {
            //add back invoice fee row if nessessary since it can have been removed earlier in function
            this.updateInvoiceFee();
        }
        
        if (recalculate) {
            this.recalculatePrices(true, true);
        }
    }

    private reverseRowAmounts(clearRowId: boolean, creditCheck = false) {

        this.productRows.forEach((r) => {
            if (clearRowId) {
                r.customerInvoiceRowId = undefined;
                r.isTimeProjectRow = false;
            }

            if (creditCheck) {
                if (r.type !== SoeInvoiceRowType.TextRow) {
                    r.vatAmount = r.vatAmount * -1;
                    r.vatAmountCurrency = r.vatAmountCurrency * -1;
                    r.sumAmount = r.sumAmount * -1;
                    r.sumAmountCurrency = r.sumAmountCurrency * -1;
                    r.marginalIncome = r.marginalIncome * -1;
                    r.marginalIncomeCurrency = r.marginalIncomeCurrency * -1;
                    r.marginalIncomeLimit = r.marginalIncomeLimit * -1;
                    r.marginalIncomeRatio = r.marginalIncomeRatio * -1;
                    r.supplementCharge = r.supplementCharge * -1;

                    r.householdAmount = r.householdAmount * -1;
                    r.householdAmountCurrency = r.householdAmountCurrency * -1;
                }
            }
            else {
                if (r.type !== SoeInvoiceRowType.TextRow) {
                    r.vatAmount = r.vatAmount * -1;
                    r.vatAmountCurrency = r.vatAmountCurrency * -1;
                    r.sumAmount = r.sumAmount * -1;
                    r.sumAmountCurrency = r.sumAmountCurrency * -1;
                    r.marginalIncome = r.marginalIncome * -1;
                    r.marginalIncomeCurrency = r.marginalIncomeCurrency * -1;
                    r.marginalIncomeLimit = r.marginalIncomeLimit * -1;
                    r.marginalIncomeRatio = r.marginalIncomeRatio * -1;
                    r.supplementCharge = r.supplementCharge * -1;

                    r.householdAmount = r.householdAmount * -1;
                    r.householdAmountCurrency = r.householdAmountCurrency * -1;
                }
            }

            r.previouslyInvoicedQuantity = 0;

            r.isModified = true;
        });

        this.calculateAmounts();

        this.soeGridOptions.refreshRows();
    }

    private attestStateChanged(ignoreDeselect = false) {

        this.$timeout(() => {

            if (this.selectedAttestState === undefined || this.selectedAttestState === null)
                return;

            let attestState: AttestStateDTO = _.find(this.attestStates, a => a.attestStateId == this.selectedAttestState);

            if (!attestState) {
                if (!ignoreDeselect) {
                    this.selectedAttestStateValid = false;
                    _.forEach(_.filter(this.activeRows, r => r.type === SoeInvoiceRowType.BaseProductRow || r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.TextRow), row => {
                        this.soeGridOptions.unSelectRow(row);
                    });
                }
                this.soeGridOptions.refreshRows();
            }
            else {
                this.selectedAttestStateValid = true;

                if (this.soeGridOptions.getSelectedCount() > 0) {
                    _.forEach(_.filter(this.soeGridOptions.getSelectedRows(), r => r.type !== SoeInvoiceRowType.AccountingRow), row => {
                        if (attestState.attestStateId == this.attestStateReadyId && row.isStockRow && row.invoiceQuantity == 0 && this.usePartialInvoicingOnOrderRow)
                            return;

                        if (_.filter(this.attestTransitions, (a) => a.attestStateFromId === row.attestStateId && attestState.attestStateId === a.attestStateToId).length === 0)
                            this.soeGridOptions.unSelectRow(row);
                    });

                    if (this.soeGridOptions.getSelectedCount() === 0 && !ignoreDeselect)
                        this.selectAllValidRows(attestState);

                    this.soeGridOptions.refreshRows();
                }
                else if (!ignoreDeselect) {
                    this.selectAllValidRows(attestState);
                }
            }
        });
    }

    private selectAllValidRows(attestState: AttestStateDTO) {
        _.forEach(_.filter(this.soeGridOptions.getFilteredRows(), r => r.type !== SoeInvoiceRowType.AccountingRow), row => {
            if (attestState.attestStateId == this.attestStateReadyId && row.isStockRow && row.invoiceQuantity == 0 && this.usePartialInvoicingOnOrderRow)
                return;

            if (_.filter(this.attestTransitions, (a) => a.attestStateFromId === row.attestStateId && attestState.attestStateId === a.attestStateToId).length > 0)
                this.soeGridOptions.selectRow(row);
            else
                this.soeGridOptions.unSelectRow(row);
        });
    }

    public saveAttestState = _.debounce((guid?: Guid) => {

        if (!this.selectedAttestState)
            return;

        this.executing = true;

        // Get selected attest state
        const attestState: AttestStateDTO = _.find(this.attestStates, a => a.attestStateId === this.selectedAttestState);

        // Validate product rows
        if (!this.validateChangeAttestState(attestState)) {
            this.executing = false;
            return;
        }

        // Validate parent control
        if (this.isValidToChangeAttestState && !this.isValidToChangeAttestState()) {
            this.executing = false;
            return;
        }

        this.soeGridOptions.stopEditing(false);

        // Change attest state on selected rows
        let nbrOfChangedRows: number = 0;
        let currentHidden: boolean = false;

        const selectedRows = this.soeGridOptions.getSelectedRows();
        _.forEach(selectedRows, (row: ProductRowDTO) => {
            if (row.type === SoeInvoiceRowType.ProductRow && !row.productId)
                return; // Next row

            let currentAttestState: AttestStateDTO = row.attestStateId ? _.find(this.availableAttestStates, a => a.attestStateId === row.attestStateId) : null;
            if (currentAttestState && (currentAttestState.hidden || currentAttestState.closed))
                currentHidden = true;

            row.attestStateId = attestState.attestStateId;
            row.attestStateName = attestState.name;
            row.attestStateColor = attestState.color;
            row.isReadOnly = attestState.locked;
            //row.isSelectDisabled = true;
            this.soeGridOptions.unSelectRow(row);
            this.setRowAsModified(row, false);
            nbrOfChangedRows++;
        });

        if (nbrOfChangedRows > 0 && (attestState.hidden || attestState.closed || currentHidden)) {
            this.calculateAmounts();
        }

        this.setParentAsModified();

        // If changing status to order ready, send true in the changeAttestStates function.
        // This will trigger a dialog asking to create an invoice when saving the order.
        this.coreService.canUserCreateInvoice(attestState.attestStateId).then(canCreateInvoice => {
            if (this.performDirectInvoicing) {
                this.messagingService.publish(Constants.EVENT_VALIDATE_TRANSFER_TO_INVOICE_RESULT, {
                    invoiceId: this.invoiceId,
                    notTransferable: this.hasRowsNotTransferableToInvoice(),
                    fixedPriceLeavingOthers: this.transferFixedPriceToInvoiceLeavingOthers(),
                    transferringContractProducts: this.numberOfContractRowsTransferrableToInvoice() > 0,
                    guid: guid,
                    performDirectInvoicing: true,
                    hasLiftRowsNotTransferable: this.hasLiftRowsNotTransferableToInvoice(),
                    canCreateInvoice: canCreateInvoice,
                });
            }
            else if (this.changeAttestStates) {
                this.changeAttestStates({ canCreateInvoice: canCreateInvoice });
            }

            this.executing = false;
        })

        this.soeGridOptions.refreshRows.apply(this.soeGridOptions, selectedRows);

    }, 500, { leading: false, trailing: true });


    private copyInvoiceProduct(row: ProductRowDTO, searchResult: ProductSearchResult, askMerge: boolean = false) {
        this.productService.copyInvoiceProduct(searchResult.productId, searchResult.purchasePrice, searchResult.salesPrice, searchResult.productUnit, searchResult.priceListTypeId, searchResult.sysPriceListHeadId, searchResult.sysWholesellerName, this.customer ? this.customer.actorCustomerId : 0, searchResult.priceListOrigin).then((result: IInvoiceProductCopyResult) => {
            const product: InvoiceProductDTO = result.product;
            if (product) {
                if (this.isNewProduct(product.productId)) {
                    this.products.push({ productId: product.productId, number: product.number, name: product.name, numberName: product.number + " " + product.name });
                }
                this.loadProduct(product.productId, row, true).then(x => {
                    // Set product on current row 
                    if (row) {
                        //this.setProductValues(row, product);
                        row.productId = product.productId;
                        row.quantity = searchResult.quantity;
                        row.amountCurrency = product.salesPrice;

                        this.setPurchasePrice(row, product.purchasePrice, result.sysWholesellerName, result.priceFormula).then(am => {
                            if (askMerge && row.quantity && row.quantity !== 0) {
                                if (!this.askMergeProductRow(row, false))
                                    this.amountHelper.calculateRowSum(row);
                            }
                            else {
                                // this.setCustomerDiscount(row, product);
                                this.amountHelper.calculateRowSum(row);
                            }

                            this.soeGridOptions.refreshRows(row);
                        });
                    }
                });

                this.soeGridOptions.selectRow(row);
                this.soeGridOptions.startEditingColumn(this.getQuantityColumn());
            }
        });
    }

    private askMergeProductRow(row: ProductRowDTO, addNewRowIfNoMerge: boolean): boolean {
        // Can only merge offer- and orderrows in initial state
        if ((this.container == ProductRowsContainers.Offer || this.container == ProductRowsContainers.Order) &&
            (!this.initialAttestState || row.attestStateId !== this.initialAttestState.attestStateId))
            return false;

        if (row["noMerge"] && row["noMerge"] === true)
            return;

        // Get selected product
        var product = this.getFullProduct(row.productId);
        if (!product || product.vatType == TermGroup_InvoiceProductVatType.None)
            return false;

        // Check if selected product exists on another row
        // Only rows with same amount, discount, wholeseller and attest state can be merged
        // Get last row, if more than one
        const existingRow = _.findLast(_.orderBy(this.activeRows, 'rowNr'),
            r => r.productId === product.productId &&
                r.tempRowId !== row.tempRowId &&
                r.amountCurrency === row.amountCurrency &&
                r.discountType === row.discountType &&
                r.discount2Type === row.discount2Type &&
                r.discountValue === row.discountValue &&
                r.discount2Value === row.discount2Value &&
                r.sysWholesellerName === row.sysWholesellerName &&
                r.attestStateId === row.attestStateId);

        if (!existingRow)
            return false;

        // Selected product do exist on another row
        // Check company setting, if it should be merged
        var merge: boolean = false;
        var ask: boolean = false;
        if (product.vatType === TermGroup_InvoiceProductVatType.Merchandise) {
            merge = this.mergeProductRowsMerchandise === TermGroup_MergeInvoiceProductRows.Always;
            ask = this.mergeProductRowsMerchandise === TermGroup_MergeInvoiceProductRows.Ask;
        } else if (product.vatType === TermGroup_InvoiceProductVatType.Service) {
            merge = this.mergeProductRowsService === TermGroup_MergeInvoiceProductRows.Always;
            ask = this.mergeProductRowsService === TermGroup_MergeInvoiceProductRows.Ask;
        }

        if (merge) {
            if (addNewRowIfNoMerge)
                ask = true;

            this.mergeProductRow(row, existingRow, !ask);
        }
        else if (ask) {
            const keys: string[] = [
                "billing.productrows.askmerge.title",
                "billing.productrows.askmerge.message"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialogDefButton(terms["billing.productrows.askmerge.title"], terms["billing.productrows.askmerge.message"].format(existingRow.rowNr.toString()), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo, SOEMessageBoxButton.Yes);
                modal.result.then(val => {
                    if (val === true)
                        this.mergeProductRow(row, existingRow);
                    else if (addNewRowIfNoMerge)
                        this.addRow(SoeInvoiceRowType.ProductRow, true);
                    else {
                        this.amountHelper.calculateRowSum(row);
                        this.soeGridOptions.refreshRows(row);
                        row["noMerge"] = true;
                    }
                });
            });
        }

        return ask;
    }

    private mergeProductRow(newRow: ProductRowDTO, existingRow: ProductRowDTO, startEdit = true) {

        if (newRow.quantity)
            existingRow.quantity = NumberUtility.parseNumericDecimal(existingRow.quantity) + NumberUtility.parseNumericDecimal(newRow.quantity);

        this.deleteRow(newRow);

        this.amountHelper.calculateRowSum(existingRow);
        this.soeGridOptions.refreshRows(existingRow);
        if(startEdit)
            this.soeGridOptions.startEditingCell(existingRow, this.getQuantityColumn());

    }

    // ACTIONS
    private resetRows(triggerRowsChanged = true, keepSelectedRows = false) {
        const selectedRowIds = keepSelectedRows ? this.soeGridOptions.getSelectedIds("tempRowId") : [];
        if (triggerRowsChanged) {
            this.productRowsChanged();
        }

        super.gridDataLoaded(this.visibleRows);
        this.soeGridOptions.refreshRows();

        //add back any previous selected rows since methods called by reset clears it
        if (keepSelectedRows && selectedRowIds && selectedRowIds.length > 0) {
            const selectedRows = this.visibleRows.filter(p => selectedRowIds.some(s => s === p.tempRowId));
            this.soeGridOptions.selectRows(selectedRows);
        }
        else {
            this.gridSelectionChanged();
        }
    }

    protected edit(row: ProductRowDTO) {
        if (row.type === SoeInvoiceRowType.ProductRow)
            this.showEditProductRowDialog(row, EditProductRowModes.EditProductRow);
        else if (row.type === SoeInvoiceRowType.TextRow)
            this.showEditTextRowDialog(row);
    }

    private showEditProductRowDialog(row: ProductRowDTO, mode: EditProductRowModes, deleteOnCancel = false) {
        let title: string;
        if (mode === EditProductRowModes.EditProductRow)
            title = this.terms["billing.productrows.editproductrow"];
        else {
            switch (row.householdTaxDeductionType) {
                case TermGroup_HouseHoldTaxDeductionType.ROT:
                    title = this.terms["billing.productrows.registerhousehold.rot"];
                    break;
                case TermGroup_HouseHoldTaxDeductionType.RUT:
                    title = this.terms["billing.productrows.registerhousehold.rut"];
                    break;
                case TermGroup_HouseHoldTaxDeductionType.GREEN:
                    title = this.terms["billing.productrows.registerhousehold.green"];
                    break;
            }
        }

        const isHousehold = row.isHouseholdRow;
        const prevHouseholdAmount: number = row.householdAmountCurrency;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Directives/ProductRows/Views/EditProductRowDialog.html"),
            controller: EditProductRowDialogController,
            controllerAs: "ctrl",
            size: 'xl',
            backdrop: 'static',
            windowClass: 'fullsize-modal',
            resolve: {
                mode: () => { return mode },
                titleTerm: () => { return title },
                householdTitleTerm: () => { return row.householdTaxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN ? this.terms["billing.invoices.householddeduction.greendeduction"] : this.terms["billing.invoices.householddeduction.householddeduction"] },
                rutTitleTerm: () => { return this.terms["billing.invoices.householddeduction.rutdeduction"] },
                isCredit: () => { return (this.billingType == TermGroup_BillingType.Credit); },
                row: () => { return row },
                rowsCtrl: () => { return this },
                priceListTypeInclusiveVat: () => { return this.priceListTypeInclusiveVat },
                invoiceId: () => { return this.invoiceId },
                purchasePriceEnabled: () => { return (this.originType === SoeOriginType.CustomerInvoice || this.originType === SoeOriginType.Order) && this.changePurchasePricePermission },
                calculateMarginalIncomeOnZeroPurchase: () => { return this.calculateMarginalIncomeOnZeroPurchase },
                usePartialInvoicingOnOrderRow: () => { return this.usePartialInvoicingOnOrderRow },
                isTaxDeductionRow: () => {
                    return (this.householdTaxDeductionProductId && row.productId === this.householdTaxDeductionProductId) ||
                        (this.household50TaxDeductionProductId && row.productId === this.household50TaxDeductionProductId) ||
                        (this.rutTaxDeductionProductId && row.productId === this.rutTaxDeductionProductId) ||
                        (this.green15TaxDeductionProductId && row.productId === this.green15TaxDeductionProductId) ||
                        (this.green20TaxDeductionProductId && row.productId === this.green20TaxDeductionProductId) ||
                        (this.green50TaxDeductionProductId && row.productId === this.green50TaxDeductionProductId);
                }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {

            if (row.isModified) {
                this.setParentAsModified();
            }
            else if (result && result.reloadInvoiceAfterClose) {
                this.reloadParent();
            }

            if (row.isHouseholdRow) {
                row.vatCodeId = null;
                row.vatCodeCode = '';
                row.vatAmount = 0;
                row.vatAmountCurrency = 0;
                row.vatAccountId = null;
                row.vatAccountNr = '';
                row.vatAccountName = '';

                // Create text row below product row and set social security number and name in the text field
                if (!row.customerInvoiceRowId || row.customerInvoiceRowId === 0) {
                    var textRow: ProductRowDTO = _.find(this.activeRows, r => r.parentRowId === row.tempRowId && r.type === SoeInvoiceRowType.TextRow && r.isHouseholdTextRow);
                    if (!textRow) {
                        // Add new TextRow
                        textRow = this.addRow(SoeInvoiceRowType.TextRow, false).row;

                        this.multiplyRowNr();
                        textRow.parentRowId = row.tempRowId;
                        textRow.rowNr = row.rowNr + 1;
                        textRow.isHouseholdTextRow = true;
                        this.reNumberRows();
                    }
                }
                else {
                    var textRow: ProductRowDTO = _.find(this.activeRows, r => r.parentRowId === row.tempRowId && r.type === SoeInvoiceRowType.TextRow);
                    if (!textRow) {
                        // Add new TextRow
                        textRow = this.addRow(SoeInvoiceRowType.TextRow, false).row;

                        this.multiplyRowNr();
                        textRow.parentRowId = row.tempRowId;
                        textRow.rowNr = row.rowNr + 1;
                        textRow.isHouseholdTextRow = true;
                        this.reNumberRows();
                    }
                }

                const brf: string = row.householdApartmentNbr || row.householdCooperativeOrgNbr ? ' ' + this.terms["billing.productrows.textrow.cooperative"].format(row.householdCooperativeOrgNbr, row.householdApartmentNbr) : '';
                switch (row.householdTaxDeductionType) {
                    case TermGroup_HouseHoldTaxDeductionType.ROT:
                        textRow.text = this.terms["billing.productrows.textrow.rot"].format(row.householdProperty, brf, row.householdSocialSecNbr, row.householdName);
                        break;
                    case TermGroup_HouseHoldTaxDeductionType.RUT:
                        textRow.text = this.terms["billing.productrows.textrow.rut"].format(row.householdSocialSecNbr, row.householdName);
                        break;
                    case TermGroup_HouseHoldTaxDeductionType.GREEN:
                        textRow.text = this.terms["billing.productrows.textrow.green"].format(row.householdProperty, brf, row.householdSocialSecNbr, row.householdName);
                        break;
                }

                // Remember latest used properties
                this.householdProperty = row.householdProperty;
                this.householdApartmentNbr = row.householdApartmentNbr;
                this.householdCooperativeOrgNbr = row.householdCooperativeOrgNbr;

                // If household tax deduction was added, raise event to change payment condition on invoice
                if (isHousehold)
                    this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_HOUSEHOLD_TAX_DEDUCTION_ADDED, this.invoiceId));

                this.calculateRemainingAmount();

                this.messagingService.publish(Constants.EVENT_PAUSE_AUTOSAVE, { guid: this.parentGuid });
            }
            if (prevHouseholdAmount != row.householdAmountCurrency) {
                row.amountCurrency = -row.householdAmountCurrency;
                this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.HouseholdAmount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                this.amountHelper.calculateRowSum(row);
            }

            this.soeGridOptions.refreshRows();
        }, () => {
            if (deleteOnCancel)
                this.deleteRow(row);
        });
    }

    private showEditTextRowDialog(row: ProductRowDTO) {
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
                container: () => { return this.container },
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
            this.soeGridOptions.refreshRows();
        });
    }

    private reloadParent() {
        this.messagingService.publish(Constants.EVENT_RELOAD_INVOICE, { guid: this.parentGuid });
    }

    private validateTaxDeductionType(productId: number, deductionType: number) {
        if (productId === this.householdTaxDeductionProductId || productId === this.household50TaxDeductionProductId) {
            return [1, 2, 3, 4, 5, 6, 7, 16].includes(deductionType);
        }
        else if (productId === this.rutTaxDeductionProductId) {
            return [8, 9, 11, 12, 13, 14, 17, 18, 19, 23, 24, 25, 26, 16].includes(deductionType);
        }
        else if (productId === this.green15TaxDeductionProductId || productId === this.green20TaxDeductionProductId) {
            return [20, 16].includes(deductionType);
        }
        else {
            return [21, 22, 16].includes(deductionType);
        }
    }

    private validateTaxDeductionTypes(row: ProductRowDTO) {
        let types;
        let typeTerms;
        if (row.productId === this.householdTaxDeductionProductId || row.productId === this.household50TaxDeductionProductId) {
            types = [1, 2, 3, 4, 5, 6, 7, 16];
            typeTerms = [1, 2, 3, 4, 5, 6, 7, 16];
        }
        else if (row.productId === this.rutTaxDeductionProductId) {
            types = [8, 9, 11, 12, 13, 14, 17, 18, 19, 23, 24, 25, 26, 16];
            typeTerms = [8, 11, 12, 13, 14, 17, 18, 19, 20, 24, 25, 26, 27, 16];
        }
        else if (row.productId === this.green15TaxDeductionProductId) {
            types = [20, 16];
            typeTerms = [21, 16];
        }
        else if (row.productId === this.green20TaxDeductionProductId) {
            types = [20, 16];
            typeTerms = [21, 16];
        }
        else {
            types = [21, 22, 16];
            typeTerms = [22, 23, 16];
        }

        if (_.some(this.activeProductRows, (r) => !this.isTaxDeductionRow(r) && !_.includes(types, r.householdDeductionType))) {
            this.coreService.getTermGroupContent(TermGroup.SysHouseholdType, false, true).then(terms => {
                let infoText = this.terms["billing.productrows.dialogs.deductiontypewarninginfo"].format(_.map(_.filter(terms, a => _.includes(typeTerms, a.id) && a.id != 16), t => t.name).join(", "));
                const modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["billing.productrows.dialogs.deductiontypewarning"] + "\n\n" + infoText, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val)
                        this.showEditProductRowDialog(row, EditProductRowModes.EditHousehold, true);
                }, () => {
                    this.deleteRow(row);
                });
            });
        }
        else {
            this.showEditProductRowDialog(row, EditProductRowModes.EditHousehold, true);
        }
    }

    private showHouseholdTaxDeduction(row: ProductRowDTO) {
        if (this.vatType === TermGroup_InvoiceVatType.Merchandise) {

            // Pause auto save
            this.messagingService.publish(Constants.EVENT_PAUSE_AUTOSAVE, { guid: this.parentGuid });

            // Set latest property used (if any previous registered)
            if (!this.householdProperty) {
                // Get properties from a previous row
                var taxRow = _.find(this.activeRows, r => r.isHouseholdRow);
                if (taxRow) {
                    this.householdProperty = taxRow.householdProperty;
                    this.householdApartmentNbr = taxRow.householdApartmentNbr;
                    this.householdCooperativeOrgNbr = taxRow.householdCooperativeOrgNbr;
                }
            }

            row.householdProperty = this.householdProperty;
            row.householdApartmentNbr = this.householdApartmentNbr;
            row.householdCooperativeOrgNbr = this.householdCooperativeOrgNbr;

            const householdAmountCurrency = this.calculateMaxDeductableAmount(row);

            row.quantity = 1;
            row.amount = row.amountCurrency = -householdAmountCurrency;
            row.householdAmountCurrency = householdAmountCurrency;

            this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);

            // Show modal window
            this.validateTaxDeductionTypes(row);
        } else {
            _.forEach(this.rowsToDelete, (rowId) => {
                var row = _.find(this.productRows, (r) => r.tempRowId === rowId);
                if (row) {
                    var childRows = _.filter(this.productRows, r => r.parentRowId === row.tempRowId);
                    if (childRows && childRows.length > 0) {
                        _.forEach(childRows, (childRow) => {
                            var childIndex = this.productRows.indexOf(childRow);
                            this.productRows.splice(childIndex, 1);
                        });
                    }
                    var index = this.productRows.indexOf(row);
                    this.productRows.splice(index, 1);
                }
            });
            this.rowsToDelete = [];

            this.setProductValues(row, null);

            let termKey = '';
            switch (row.householdTaxDeductionType) {
                case TermGroup_HouseHoldTaxDeductionType.ROT:
                    termKey = "billing.productrows.cannotuserotonvattype";
                    break;
                case TermGroup_HouseHoldTaxDeductionType.RUT:
                    termKey = "billing.productrows.cannotuserutonvattype";
                    break;
                case TermGroup_HouseHoldTaxDeductionType.GREEN:
                    termKey = "billing.productrows.cannotusegreenonvattype";
                    break;
            }

            this.translationService.translate(termKey).then(term => {
                const modal = this.notificationService.showDialogEx(this.terms["core.warning"], term, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                modal.result.then(val => {
                    this.resetRows(false);
                });
            });
        }
    }

    private calculateMaxDeductableAmount(row: ProductRowDTO, ignoreUsed = false): number {
        var amountCurrency: number = 0;
        var usedAmountCurrency: number = 0;
        var hiddenAttestStates: number[] = _.map(_.filter(this.availableAttestStates, a => a.hidden), a => a.attestStateId);
        _.forEach(_.filter(this.activeRows, r => r.type === SoeInvoiceRowType.ProductRow && !this.isRowTransferred(r) && !(r.attestStateId && _.includes(hiddenAttestStates, r.attestStateId))), r => {
            var prod = _.find(this.productList, p => p.productId === r.productId);
            if (prod) {
                if (!this.isLiftProduct(prod.productId)) { 
                    //if (prod.productId === this.householdTaxDeductionProductId || prod.productId === this.household50TaxDeductionProductId || prod.productId === this.rutTaxDeductionProductId || r.productId === this.green15TaxDeductionProductId || r.productId === this.green20TaxDeductionProductId || r.productId === this.green50TaxDeductionProductId) {
                    if (this.isTaxDeductionRow(r)) {
                        // Sum amount of any previously added household tax deduction rows
                        if (row.productId === r.productId)
                        usedAmountCurrency += r.sumAmountCurrency;
                    }
                    else if (row.productId === this.green15TaxDeductionProductId || row.productId === this.green20TaxDeductionProductId || row.productId === this.green50TaxDeductionProductId || prod.vatType === TermGroup_InvoiceProductVatType.Service) {
                        if (this.validateTaxDeductionType(row.productId, r.householdDeductionType)) {
                            // Calculate amount based on percent
                            if (prod.householdDeductionPercentage && prod.householdDeductionPercentage > 0) {
                                var percentage = prod.householdDeductionPercentage / 100;
                                var amountToCalculate = (this.priceListTypeInclusiveVat ? r.sumAmountCurrency : (r.sumAmountCurrency + r.vatAmountCurrency).round(2));
                                amountCurrency += (amountToCalculate * percentage).round(2);
                            } else if (r.householdDeductionType > 0 && r.householdDeductionType != 16) {
                                // Sum amount of product rows where the products VAT type is Service
                                amountCurrency += r.sumAmountCurrency;
                                // Add VAT amount
                                if (!this.priceListTypeInclusiveVat)
                                    amountCurrency += r.vatAmountCurrency;
                            }
                        }
                    }
                }
            }
        });

        let householdAmountCurrency = 0; // usedAmount is negative, therefore we add it here instead of subtracting

        switch (row.householdTaxDeductionType) {
            case TermGroup_HouseHoldTaxDeductionType.ROT:
                if (row.productId === this.household50TaxDeductionProductId)
                    householdAmountCurrency = ((amountCurrency * 0.5) + (ignoreUsed ? 0 : usedAmountCurrency)).round(2);
                else
                    householdAmountCurrency = ((amountCurrency * 0.3) + (ignoreUsed ? 0 : usedAmountCurrency)).round(2);
                break;
            case TermGroup_HouseHoldTaxDeductionType.RUT:
                householdAmountCurrency = ((amountCurrency * 0.5) + (ignoreUsed ? 0 : usedAmountCurrency)).round(2);
                break;
            case TermGroup_HouseHoldTaxDeductionType.GREEN:
                if (row.productId === this.green15TaxDeductionProductId)
                    householdAmountCurrency = ((amountCurrency * 0.15) + (ignoreUsed ? 0 : usedAmountCurrency)).round(2);
                else if (row.productId === this.green20TaxDeductionProductId)
                    householdAmountCurrency = ((amountCurrency * 0.20) + (ignoreUsed ? 0 : usedAmountCurrency)).round(2);
                else
                    householdAmountCurrency = ((amountCurrency * 0.50) + (ignoreUsed ? 0 : usedAmountCurrency)).round(2);
                break;
        }

        // If credit invoice, the amount will be calculated as negative, but should always be entered as positive.
        // Only the row sum will be changed, not the price.
        if (this.isCredit && householdAmountCurrency < 0)
            householdAmountCurrency = -householdAmountCurrency;

        return Math.floor(householdAmountCurrency);
    }

    protected initDeleteRow(row: ProductRowDTO) {
        if (!row)
            return;

        this.validateDeleteRow(row).then(result => {
            if (result)
                this.deleteRow(row);
        });
    }

    private deleteRow(row: ProductRowDTO, silent = false, recalculateOnDelete = true, fromStockMove = false) {
        if (!row)
            return;

        // Publish event to container
        if (row.isTimeProjectRow)
            this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_MANUALLY_DELETED_TIME_PROJECT_ROW, this.invoiceId));

        // Restore all products prices if all flat price product rows are deleted
        const recalculatePrices: boolean = this.isFixedPriceRow(row);

        if (row.customerInvoiceRowId) {
            row.state = SoeEntityState.Deleted;
            row.isModified = true;

            let childRows = this.activeRows.filter(r => r.parentRowId === row.customerInvoiceRowId);
            if (fromStockMove) {
                childRows = childRows.filter(r => r.type !== SoeInvoiceRowType.ProductRow);
            }
            for (const child of childRows) {
                child.state = SoeEntityState.Deleted;
                child.isModified = true;
            };
        }
        else {
            const index: number = this.productRows.indexOf(row);
            this.productRows.splice(index, 1);

            let childRows1 = this.activeRows.filter(r => r.parentRowId === row.tempRowId);
            if (fromStockMove) {
                childRows1 = childRows1.filter(r => r.type !== SoeInvoiceRowType.ProductRow);
            }
            for (const child1 of childRows1) {
                const index: number = this.productRows.indexOf(child1);
                this.productRows.splice(index, 1);
            };
        }

        if (!silent)
            this.setParentAsModified();

        this.productRowsChanged();

        if (this.isFixedPriceRow(row) && this.fixedPrice && !this.hasFixedPriceProducts())
            this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_FIXED_PRICE_ADDED, { guid: this.parentGuid, orderType: OrderContractType.Continuous }));

        if (recalculateOnDelete) {
            this.reNumberRows();
            this.calculateAmounts();

            if (recalculatePrices && !this.hasFixedPriceProducts())
                this.recalculatePrices();
        }
    }

    private uppercaseRows() {
        const selectedRows = this.soeGridOptions.getSelectedRows();
        if (!selectedRows || selectedRows.length === 0)
            return;

        else {
            selectedRows.forEach(row => {
                row.text = row.text?.toUpperCase();
                this.setRowAsModified(row);
                this.setParentAsModified();
                this.soeGridOptions.refreshRows();
            });
        }
    }

    private initDeleteRows() {
        const selectedRows = this.soeGridOptions.getSelectedRows();
        if (!selectedRows || selectedRows.length === 0)
            return;

        if (this.showWarningBeforeRowDelete) {
            this.translationService.translate("billing.productrows.dialogs.verifydeleterows").then((term) => {
                const modal = this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, false, null, false, null, null, null, null, SOEMessageBoxButton.OK);
                modal.result.then(val => {
                    this.validateDeleteRows(selectedRows).then(result => {
                        if (result)
                            this.deleteRows(selectedRows);
                    });
                });
            });
        }
        else {
            this.validateDeleteRows(selectedRows).then(result => {
                if (result)
                    this.deleteRows(selectedRows);
            });
        }
    }

    private deleteRows(rows: ProductRowDTO[], fromStockMove: boolean = false) {
        let recalculatePrices: boolean = false;

        const rowsToDelete = rows.filter(r => !r.isReadOnly);
        for (const row of rowsToDelete) {
            if (this.isFixedPriceRow(row))
                recalculatePrices = true;

            this.setRowAsModified(row);
            this.deleteRow(row, false, false, fromStockMove);
        }

        this.setParentAsModified();
        this.reNumberRows();
        this.calculateAmounts();

        if (recalculatePrices && !this.hasFixedPriceProducts())
            this.recalculatePrices();
    }

    protected allowNavigationFromTypeAhead(value, entity, colDef) {
        if (!value)  // If no value, allow it.
            return true;

        var matched = _.some(this.products, (p) => p.number === value);
        if (matched) {
            return true;
        }
        else {
            this.productNotFound(value);
            return false;
        }
    }

    protected handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {

        const { nextCellPosition, previousCellPosition, backwards } = params;
        let { rowIndex, column } = nextCellPosition;
        let row: ProductRowDTO = this.soeGridOptions.getVisibleRowByIndex(rowIndex).data;

        const steppingResult = this.nextColumnSteppingRule(rowIndex, column, row, backwards);
        if (steppingResult) {
            return steppingResult;
        }

        // Ask merge
        const prevRow = this.findPreviousRow(row);
        if (this.askMergeProductRow(row, true))
            row = prevRow && prevRow.rowNode ? prevRow.rowNode.data : undefined;

        //no new valid cell found to navigate to so return null so we will switch row...
        const nextRowResult = row ? (backwards ? this.findPreviousRow(row) : this.findNextRow(row)) : undefined;
        const newRowIndex = nextRowResult ? nextRowResult.rowIndex : this.addRow(SoeInvoiceRowType.ProductRow, false).rowIndex;
        if (backwards) {
            const steppingResult2 = this.nextColumnSteppingRule(newRowIndex, this.soeGridOptions.getLastEditableColumn(), nextRowResult.rowNode.data, backwards);
            if (steppingResult2) {
                return steppingResult2;
            }
        }

        return { rowIndex: newRowIndex, column: backwards ? this.soeGridOptions.getLastEditableColumn() : this.getProductNrColumn() };
    }

    private nextColumnSteppingRule(rowIndex: number, column: any, row: ProductRowDTO, backwards: boolean): { rowIndex: number, column: any } {
        let nextColumnCaller: (column: any) => any = backwards ? this.soeGridOptions.getPreviousVisibleColumn : this.soeGridOptions.getNextVisibleColumn;
        while (!!column && !!this.steppingRules) {
            const { colDef } = column;
            if (this.soeGridOptions.isCellEditable(row, colDef)) {
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

    protected findProduct(row: ProductRowDTO): ProductSmallDTO {
        return row.productNr ? _.find(this.products, p => p.number === row.productNr) : null;
    }

    private getSmallProduct(productId: number): ProductSmallDTO {
        return _.find(this.products, p => p.productId === productId);
    }

    private getFullProduct(productId: number): ProductRowsProductDTO {
        return _.find(this.productList, p => p.productId === productId);
    }

    private isLiftProduct(productId: number): boolean {
        var lift = _.find(this.liftProducts, p => p.productId === productId);
        return (lift !== null && lift !== undefined);
    }

    public filterProducts(filter) {
        return this.products.filter(prod => {
            if (this.productSearchFilterMode === TermGroup_ProductSearchFilterMode.Equals)
                return prod.number == filter;
            else if (this.productSearchFilterMode === TermGroup_ProductSearchFilterMode.StartsWidth)
                return prod.number.startsWithCaseInsensitive(filter) || prod.name.startsWithCaseInsensitive(filter);
            else if (this.productSearchFilterMode === TermGroup_ProductSearchFilterMode.Contains)
                return prod.number.contains(filter) || prod.name.contains(filter);
        });
    }

    private searchProductFromRow(e) {

        // Refocus to propagate data to the row
        //this.soeGridOptions.refocusCell();
        this.soeGridOptions.stopEditing(false);
        var row: ProductRowDTO = this.soeGridOptions.getCurrentRow();
        var product: ProductRowsProductDTO;

        if (row && row.productId)
            product = _.find(this.productList, p => p.productId === row.productId);

        this.searchProduct(row, product);
    }

    protected searchProduct(row: ProductRowDTO, product: ProductRowsProductDTO, info: string = '') {
        if (this.searchProductDialogIsOpen) {
            return;
        }

        this.searchProductDialogIsOpen = true;

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/SearchInvoiceProduct/SearchInvoiceProduct.html"),
            controller: SearchInvoiceProductController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: (this.useExtendSearchInfo ? 'fullsize-modal-top' : ""),
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                productService: () => { return this.productService },
                hideProducts: () => { return null },
                priceListTypeId: () => { return this.priceListTypeId },
                customerId: () => { return this.customer ? this.customer.actorCustomerId : 0 },
                currencyId: () => { return this.currencyId },
                number: () => { return row.productNr },
                name: () => { return null },
                quantity: () => { return row.quantity },
                sysWholesellerId: () => { return this.wholesellerId },
                info: () => { return info }
            }
        });

        modal.result.then((result: ProductSearchResult) => {
            if (result) {
                this.searchProductDialogIsOpen = false;

                row.productId = result.productId;
                row.purchasePriceCurrency = result.purchasePrice;
                row.quantity = result.quantity;
                // Set default unit if not specified
                if (!result.productUnit)
                    result.productUnit = this.defaultProductUnitCode;

                this.copyInvoiceProduct(row, result, true);

                this.setParentAsModified();
            }
        },
            (result) => {
                // User cancelled, 
                this.searchProductDialogIsOpen = false;
            });

        return modal;
    }

    private setAttestStateValues(row: ProductRowDTO) {
        const attestState: AttestStateDTO = this.attestStates && row.attestStateId ? _.find(this.attestStates, a => a.attestStateId === row.attestStateId) : null;
        row.attestStateName = attestState ? attestState.name : '';
        row.attestStateColor = attestState ? attestState.color : "FFFFFF";  // White
    }

    public moveRows(rows: ProductRowDTO[], before: boolean, position: number) {
        const ids = _.map(rows, 'tempRowId');

        let minRowNr: number = _.min(_.map(rows, r => r.rowNr));
        if (position < minRowNr)
            minRowNr = position;
        let maxRowNr: number = _.max(_.map(rows, r => r.rowNr));
        if (position > maxRowNr)
            maxRowNr = position;

        let counter = minRowNr;

        if (before) {
            // Rows to move
            _.forEach(_.orderBy(rows, 'rowNr'), (row) => {
                row.rowNr = counter++;
                this.soeGridOptions.unSelectRow(row);
                this.setRowAsModified(row, false);
            });

            // Other affecred rows
            _.forEach(_.orderBy(_.filter(this.activeRows, r => r.type !== SoeInvoiceRowType.AccountingRow && r.rowNr >= minRowNr && r.rowNr <= maxRowNr && !_.includes(ids, r.tempRowId)), 'rowNr'), (row) => {
                row.rowNr = counter++;
                this.setRowAsModified(row, false);
            });
        } else {
            // Other affecred rows
            _.forEach(_.orderBy(_.filter(this.activeRows, r => r.type !== SoeInvoiceRowType.AccountingRow && r.rowNr >= minRowNr && r.rowNr <= maxRowNr && !_.includes(ids, r.tempRowId)), 'rowNr'), (row) => {
                row.rowNr = counter++;
                this.setRowAsModified(row, false);
            });

            // Rows to move
            _.forEach(_.orderBy(rows, 'rowNr'), (row) => {
                row.rowNr = counter++;
                this.soeGridOptions.unSelectRow(row);
                this.setRowAsModified(row, false);
            });
        }

        this.setParentAsModified();
        this.amountHelper.calculateSubTotals(this.activeRows);
        this.resetRows();
    }

    // EVENTS

    private customerChanged(getFreight = true, getInvoiceFee = true) {
        // Invoice fee
        if (this.customer && this.customer.disableInvoiceFee) {
            this.updateInvoiceFee();
        } else if (!this.invoiceId) {
            if (getInvoiceFee)
                this.getInvoiceFee();
            if (getFreight)
                this.getFreightAmount();
        }
    }

    private currencyChanged() {
        this.calculateAmounts();
        this.soeGridOptions.refreshRows();
    }

    private currencyIdChanged() {
        let rows = this.activeRows;
        for (var i = 0; i < rows.length; i++) {
            let item = rows[i];
            for (let enumItem in ProductRowsAmountField) {
                if (isNaN(Number(item))) {
                    this.amountHelper.calculateAllRowsCurrencyAmounts(item, Number(enumItem), TermGroup_CurrencyType.TransactionCurrency);
                    item.isModified = true;
                }
            }
        }
        this.currencyChanged();
    }

    private beginCellEdit(row: ProductRowDTO, colDef: uiGrid.IColumnDef) {
        switch (colDef.field) {
            case 'quantity':
                this.prevQuantity = row.quantity;
                break;
            case 'amountCurrency':
                this.prevAmount = row.amountCurrency;
                break;
            case 'discountValue':
                this.prevDiscountValue = row.discountValue;
                break;
            case 'discount2Value':
                this.prevDiscount2Value = row.discount2Value;
                break;
            case 'stockId':
                this.setStocksForProduct(row, false)
                break;
            /*case 'text':
                if (row.type === SoeInvoiceRowType.TextRow)
                    this.showEditTextRowDialog(row);
                break;*/
        }
    }

    private afterCellEdit(row: ProductRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue && colDef.field !== 'productNr')
            return;

        switch (colDef.field) {
            case 'productNr':
                this.initChangeProduct(row);
                break;
            case 'quantity':
                row.quantity = NumberUtility.parseNumericDecimal(row.quantity);
                this.initChangeQuantity(row);
                break;
            case 'invoiceQuantity':
                row.invoiceQuantity = NumberUtility.parseNumericDecimal(row.invoiceQuantity);
                this.initChangeInvoiceQuantity(row);
                break;
            case 'amountCurrency':
                row.amountCurrency = NumberUtility.parseNumericDecimal(row.amountCurrency);
                this.initChangeAmount(row);
                break;
            case 'discountValue':
                row.discountValue = NumberUtility.parseNumericDecimal(row.discountValue);
                this.initChangeDiscount(row);
                break;
            case 'discount2Value':
                row.discount2Value = row.discount2Value > 0 ? NumberUtility.parseNumericDecimal(row.discount2Value) : 0;
                this.initChangeDiscount2(row);
                break;
            case 'supplementCharge':
                row.supplementCharge = NumberUtility.parseNumericDecimal(row.supplementCharge);
                this.supplementChargeChanged(row);
                break;
            case 'purchasePriceCurrency':
                row.purchasePriceCurrency = NumberUtility.parseNumericDecimal(row.purchasePriceCurrency);
                this.purchasePriceChanged(row);
                break;
            case 'marginalIncomeCurrency':
                row.marginalIncomeCurrency = NumberUtility.parseNumericDecimal(row.marginalIncomeCurrency);
                this.marginalIncomeChanged(row);
                break;
            case 'marginalIncomeRatio':
                const newRatio = NumberUtility.parseNumericDecimal(row.marginalIncomeRatio);
                if (newRatio >= 100)
                    row.marginalIncomeRatio = NumberUtility.parseNumericDecimal(oldValue);
                else
                    row.marginalIncomeRatio = NumberUtility.parseNumericDecimal(newValue);
                this.marginalIncomeRatioChanged(row);
                break;
            case 'soe-ag-single-value-column':
                this.soeGridOptions.refreshRows(row);
                break;
        }

        // If any of these columns are modified, post event to parent so accounting rows will be generated
        switch (colDef.field) {
            case 'quantity':
            case 'amountCurrency':
            case 'discountValue':
            case 'discount2Value':
            case 'discountType':
            case 'discount2Type':
            case 'vatCodeId':
            case 'vatAccountId':
                this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, this.invoiceId));
                break;
        }

        this.setParentAsModified();
    }

    public beginCellEditInTypeahead(entity, colDef) {
        this.prevProductId = entity && entity['productId'] ? entity['productId'] : 0;
    }

    public initChangeProduct(row: ProductRowDTO) {
        var product = this.findProduct(row);
        var selectedProductId: number = (product ? product.productId : 0);
        if (selectedProductId !== this.prevProductId) {

            this.pendingProductId = selectedProductId;
            this.validateChangeProduct(row).then((result) => {
                if (result) {
                    // All validations are OK, update the row
                    this.productChangedFromId(row, selectedProductId);
                } else {
                    this.pendingProductId = this.prevProductId;
                    this.revertProduct(row);
                }
            });
        }
    }

    private productChangedFromId(row: ProductRowDTO, productId: number) {
        // Check if product exists in the product list
        const product = this.getFullProduct(productId);
        if (product)
            this.productChanged(row, product);
        else {
   this.setProductValuesFromId(row, productId);
        }
    }

    private productChanged(row: ProductRowDTO, product: ProductRowsProductDTO, ignorePurchasePrice: boolean = false, skipWholesellerDialog: boolean = false) {
        this.setProductValues(row, product, ignorePurchasePrice);
        const calculationType: TermGroup_InvoiceProductCalculationType = product ? product.calculationType : TermGroup_InvoiceProductCalculationType.Regular;

        this.setStocksForProduct(row, true);

        this.setCustomerDiscount(row, product);

        if (row.productId === this.householdTaxDeductionProductId || row.productId === this.household50TaxDeductionProductId || row.productId === this.rutTaxDeductionProductId || row.productId === this.green15TaxDeductionProductId || row.productId === this.green20TaxDeductionProductId || row.productId === this.green50TaxDeductionProductId) {
            // Household tax deduction product
            this.rowsToDelete.push(row.tempRowId);
            this.showHouseholdTaxDeduction(row);
        }
        else if (product && calculationType === TermGroup_InvoiceProductCalculationType.FixedPrice) {
            this.setIsFixedPrice(row);
        }
        else if (product && calculationType === TermGroup_InvoiceProductCalculationType.Lift && product.guaranteePercentage) {
            // Lift product
            if (this.productGuaranteeId === 0) {
                const keys: string[] = [
                    "billing.productrows.baseproductmissing",
                    "billing.productrows.baseproductmissing.guarantee"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["billing.productrows.baseproductmissing"], terms["billing.productrows.baseproductmissing.guarantee"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK).result.then(result => { this.soeGridOptions.refocusCell(); });
                });
            } else if (!this.hasFixedPriceProducts() && !this.hasFixedPriceKeepPricesProducts()) {
                const keys: string[] = [
                    "billing.productrows.fixedproductmissing",
                    "billing.productrows.fixedproductmissing.guarantee"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["billing.productrows.baseproductmissing"], terms["billing.productrows.fixedproductmissing.guarantee"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK).result.then(result => { this.soeGridOptions.refocusCell(); });
                });
            } else {
                // Try to find base product guaranteepercentage
                let guaranteeRow = null;
                if (this.initialAttestState)
                    guaranteeRow = _.find(this.activeRows, r => r.productId === this.productGuaranteeId && r.attestStateId === this.initialAttestState.attestStateId);
                if (!guaranteeRow) {
                    this.$timeout(() => {
                        const gridRow = this.addRow(SoeInvoiceRowType.ProductRow, false);
                        if (gridRow && gridRow.row) {
                            guaranteeRow = gridRow.row;
                            this.productChangedFromId(guaranteeRow, this.productGuaranteeId);
                            this.soeGridOptions.startEditingCell(row, this.getProductNrColumn());
                        }
                    }, 10);
                }
            }
        } else if (product && product.productId === this.productGuaranteeId) {
            // Find all lift products with guarantee percentage and add percentage to the guaranteerow
            var liftRows = this.activeRows.filter(r => r.isLiftProduct);
            var percentages: number[] = [];
            liftRows.forEach((liftRow) => {
                var prod = this.getFullProduct(liftRow.productId);
                if (prod && prod.guaranteePercentage) {
                    if (!_.includes(percentages, prod.guaranteePercentage))
                        percentages.push(prod.guaranteePercentage);
                }
            });
            if (percentages.length > 0)
                row.text = "{0} ({1} %)".format(product.name, percentages.join(', '));
        } else {
            if (row.isHouseholdRow) {
                // User changed from a household tax deduction row, reset data
                row.householdProperty = '';
                row.householdSocialSecNbr = '';
                row.householdName = '';
                row.householdAmount = 0;
                row.householdAmountCurrency = 0;
                row.householdApartmentNbr = '';
                row.householdCooperativeOrgNbr = '';
            }

            if (!skipWholesellerDialog &&
                !this.isTaxDeductionRow(row) &&
                row.productId !== this.fixedPriceProductId &&
                calculationType !== TermGroup_InvoiceProductCalculationType.Lift &&
                calculationType !== TermGroup_InvoiceProductCalculationType.Clearing &&
                !row.isInterestRow) {
                if (product) {
                    if ((this.askForWholeseller || !this.wholesellerId) && product.sysProductId) {
                        if (!this.customerPriceSet) {
                            // If ask for wholeseller setting and external product, always show product search dialog (but with products hidden)
                            row.productNr = product.number;

                            if (!this.askForWholeseller) {
                                const keys: string[] = [
                                    "billing.productrows.wholesellermissing",
                                    "billing.productrows.defaultwholesellermissing"
                                ];

                                this.translationService.translateMany(keys).then((terms) => {
                                    let info: string = terms["billing.productrows.wholesellermissing"];
                                    if (this.defaultWholesellerId === 0)
                                        info += "\n" + terms["billing.productrows.defaultwholesellermissing"];
                                    this.searchProduct(row, product, info);
                                });
                            } else {
                                this.searchProduct(row, product);
                            }
                        }
                    } else {
                        if (this.customerPriceSet) {
                            this.customerPriceSet = false;
                            if (this.setFixedPrice(row)) {
                                row.sysWholesellerName = '';
                            }
                        } else {
                            // If selected product is an external product, get amount from external price list,
                            // otherwise get the price from the internal price list.
                            this.getProductPrice(row, product.productId, row.isTimeProjectRow, true);                            
                        }
                    }
                } else {
                    row.amount = 0;
                    row.amountCurrency = 0;
                }
            }
        }

        this.productRowsChanged();
        //if (row.productId !== this.fixedPriceProductId)
        this.soeGridOptions.refreshRows(row);
    }

    private productNotFound(value: string) {
        const row: ProductRowDTO = this.soeGridOptions.getCurrentRow();
        if (row) {
            row.productNr = value;
            const keys: string[] = [
                "core.cancel",
                "billing.products.newproduct",
                "billing.productrows.nointernalproductfound",
                "billing.productrows.nointernalproductfoundsearch",
                "billing.productrows.externalsearch",
                "billing.productrows.selectedwholeseller"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["billing.productrows.nointernalproductfound"], terms["billing.productrows.nointernalproductfoundsearch"].format(row.productNr, terms["billing.productrows.selectedwholeseller"], terms["billing.productrows.externalsearch"]), SOEMessageBoxImage.Error, SOEMessageBoxButtons.YesNoCancel, SOEMessageBoxSize.Medium, false, false, null, false, "", "billing.productrows.externalsearch", "billing.products.newproduct", "core.cancel", SOEMessageBoxButton.Yes);
                modal.result.then(val => {
                    if (val === true) {
                        this.searchProduct(row, null);
                    }
                    else if (val === false) {
                        this.addProduct(row.productNr, row);
                    }
                });
            });
        }
    }

    private initChangeQuantity(row: ProductRowDTO) {
        this.validateChangeQuantity(row).then((result) => {
            if (result) {
                if (row.isTimeProjectRow)
                    row.timeManuallyChanged = true;

                // All validations are OK, calculate row sum
                this.amountHelper.calculateRowSum(row);
                if (this.useQuantityPrices && !row.supplierInvoiceId) {
                    this.getProductPrice(row, row.productId, row.isTimeProjectRow, false);
                }
            } else {
                this.revertQuantity(row);
            }
            this.soeGridOptions.refreshRows(row);
        });
    }

    private initChangeInvoiceQuantity(row: ProductRowDTO) {
        this.setRowAsModified(row, false);
        if (row.invoiceQuantity > row.quantity) {
            row.invoiceQuantity = row.quantity;
        }
        this.soeGridOptions.refreshRows(row);
    }

    /*
    private productUnitChanged(row: any) {
        var productUnit = _.find(this.productUnits, p => p.value === row.productUnitId);
        row.productUnitCode = productUnit ? productUnit.label : '';
    }
    */

    private setGrossMarginCalculationType(row: ProductRowDTO) {
        if (row.stockId) {
            const product = this.productList.find(x => x.productId === row.productId);
            if (product.grossMarginCalculationType === TermGroup_GrossMarginCalculationType.Unknown)
                {row.grossMarginCalculationType = this.grossMarginCalculationType;}
            else
                {row.grossMarginCalculationType = product.grossMarginCalculationType;}
        }
    }

    private stockChanged(row: ProductRowDTO) {
        row.isModified = true;
        if (row.stockId) {
            this.setGrossMarginCalculationType(row);
            if (row.grossMarginCalculationType == TermGroup_GrossMarginCalculationType.StockAveragePrice) {
                this.stockService.getStockProductAvgPrice(row.stockId, row.productId).then(x => {
                    if (x?.avgPrice) {
                        row.grossMarginCalculationType = TermGroup_GrossMarginCalculationType.StockAveragePrice;
                        this.setPurchasePrice(row, x.avgPrice).then(() => {
                            this.soeGridOptions.refreshRows(row);
                        });
                    }
                });
            }
        }
    }

    private initChangeAmount(row: ProductRowDTO) {
        this.validateChangeAmount(row).then((result) => {
            if (result) {
                // Can't have discount if no amount
                if (row.amountCurrency == 0 && row.discountValue)
                    row.discountValue = 0;

                if (row.amountCurrency == 0 && row.discount2Value) {
                    row.discount2Value = 0;
                }

                // All validations are OK, calculate row sum and supplement charge
                this.amountHelper.calculateRowSum(row, false, null, null, null, this.isTaxDeductionRow(row));
                this.amountHelper.calculateSupplementCharge(row);

            } else {
                this.revertAmount(row);
            }

            this.soeGridOptions.refreshRows(row);
        });
    }

    private initChangeDiscount(row: ProductRowDTO) {
        this.validateChangeDiscount(row).then((result) => {
            if (result) {
                // All validations are OK, calculate row sum
                this.amountHelper.calculateRowSum(row);
            } else {
                this.revertDiscount(row);
            }

            this.soeGridOptions.refreshRows(row);
        });
    }

    private initChangeDiscount2(row: ProductRowDTO) {
        this.initChangeDiscount(row);
        if (row.discount2Type === SoeInvoiceRowDiscountType.Percent) 
            row.discount2Amount = 0;
        if (row.discount2Type === SoeInvoiceRowDiscountType.Amount)
            row.discount2Percent = 0;
    }
    
    private discountTypeChanged(row: ProductRowDTO) {
        this.validateChangeDiscount(row).then((result) => {
            if (result) {
                // All validations are OK, calculate row sum
                row.discountTypeText = this.getDiscountTypeText(row.discountType);
                this.amountHelper.calculateRowSum(row);
            } else {
                this.revertDiscount(row);
            }
        });
    }

    private supplementChargeChanged(row: ProductRowDTO): boolean {
        try {
            return this.amountHelper.supplementChargeChanged(row);
        }
        finally {
            this.soeGridOptions.refreshRows(row);
        }
    }

    private vatCodeChanged(row: ProductRowDTO) {
        // Get selected code
        var vatCode: VatCodeDTO = _.find(this.vatCodes, v => v.vatCodeId === row.vatCodeId);
        row.vatCodeId = vatCode ? vatCode.vatCodeId : 0;
        row.vatCodeCode = vatCode ? vatCode.code : '';
        row.vatRate = vatCode ? vatCode.percent : 0;
        row.vatAccountId = vatCode ? vatCode.accountId : 0;
        row.vatAccountNr = vatCode ? vatCode.accountNr : '';

        this.amountHelper.calculateRowSum(row);
    }

    private vatAccountChanged(row: ProductRowDTO) {
        // Get selected account
        var vatAccount: AccountVatRateViewSmallDTO = _.find(this.vatAccounts, v => v.accountId === row.vatAccountId);

        row.vatAccountId = vatAccount ? vatAccount.accountId : 0;
        row.vatAccountNr = vatAccount ? vatAccount.accountNr : '';
        row.vatRate = vatAccount ? (vatAccount.vatRate ? vatAccount.vatRate : 0) : 0;

        this.amountHelper.calculateRowSum(row);
    }

    private purchasePriceChanged(row: ProductRowDTO) {
        try {
            this.amountHelper.purchasePriceChanged(row);

            // Calculate marginal income
            this.calculateMarginalIncome();
        } finally {
            this.soeGridOptions.refreshRows(row);
        }
    }

    private marginalIncomeChanged(row: ProductRowDTO) {
        try {
            this.amountHelper.marginalIncomeChanged(row);
        } finally {
            this.soeGridOptions.refreshRows(row);
        }
    }

    private marginalIncomeRatioChanged(row: ProductRowDTO) {
        try {
            this.amountHelper.marginalIncomeRatioChanged(row);
        } finally {
            this.soeGridOptions.refreshRows(row);
        }
    }

    private householdDeductionTypeChanged(row: ProductRowDTO) {
        var type = _.find(this.householdDeductionTypes, h => h.value === row.householdDeductionType);
        row.householdDeductionTypeText = this.getHouseholdDeductionTypeText(type.value);
    }

    // VALIDATION

    private validateChangeProduct(row: ProductRowDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.validateMultipleSalesRowsOnChangedRow(row).then(val => {
            deferral.resolve(val);
        });

        return deferral.promise;
    }

    private validateChangeQuantityDialogWasShown: boolean = false;
    private validateChangeQuantity(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        this.validateChangeQuantityDialogWasShown = false;
        this.validateLiftOnChangedRow(row).then(val => {
            if (val === false)
                deferral.resolve(false);

            this.validateTimeProjectOnChangedRow(row).then(val => {
                if (val === false)
                    deferral.resolve(false);

                this.validateMultipleSalesRowsOnChangedRow(row).then(val => {
                    if (val === false)
                        deferral.resolve(false);

                    /*this.validateStockOnChangedRow(row).then(val => {
                        if (val === false)
                            deferral.resolve(false);*/

                    this.validateChangeQuantityEdi(row).then(val => {
                        if (val === false)
                            deferral.resolve(false);
                        else
                            deferral.resolve(true);
                    });
                    //});
                });
            });
        });

        return deferral.promise;
    }

    private validateChangeAmount(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        if (this.fixedPrice) {
            var product = this.getFullProduct(row.productId);
            if (product) {
                if (!(this.isFixedPriceRow(row) || row.isLiftProduct || this.isGuaranteeRow(row) || product.calculationType === TermGroup_InvoiceProductCalculationType.FixedPrice)) {
                    deferral.resolve(false);
                    return deferral.promise;
                }
            }
            else {
                if (!(this.isFixedPriceRow(row) || row.isLiftProduct || this.isGuaranteeRow(row))) {
                    deferral.resolve(false);
                    return deferral.promise;
                }
            }
        }

        this.validateMultipleSalesRowsOnChangedRow(row).then(val => {
            if (val === false)
                deferral.resolve(false);
            else
                deferral.resolve(true);
        });

        return deferral.promise;
    }

    private validateChangeDiscount(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        this.validateDiscountOnChangedRow(row).then(val => {
            if (val === false)
                deferral.resolve(false);

            this.validateMultipleSalesRowsOnChangedRow(row).then(val => {
                if (val === false)
                    deferral.resolve(false);

                this.validateTimeProjectOnChangedRow(row).then(val => {
                    if (val === false)
                        deferral.resolve(false);
                    else
                        deferral.resolve(true);
                });
            });
        });

        return deferral.promise;
    }

    private validateLiftOnChangedRow(row: ProductRowDTO): ng.IPromise<boolean> {
        // Quantity can not be negative on lift product rows
        var deferral = this.$q.defer<boolean>();

        if (row.isLiftProduct && row.quantity < 0) {
            this.translationService.translate("billing.productrows.quantitychanged.liftproduct.message").then((term) => {
                var modal = this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                modal.result.then(val => {
                    deferral.resolve(false);
                });
                this.validateChangeQuantityDialogWasShown = true;
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateTimeProjectOnChangedRow(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Row is connected to a time project row, show question
        if (row.isTimeProjectRow) {
            this.translationService.translate("billing.productrows.quantitychanged.timeproject.message").then((term) => {
                const modal = this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    row.timeManuallyChanged = true;
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
                this.validateChangeQuantityDialogWasShown = true;
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateMultipleSalesRowsOnChangedRow(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Row has split accounting, show question
        if (row.hasMultipleSalesRows) {
            this.translationService.translate("billing.productrows.quantitychanged.multiplesalesrows.message").then((term) => {
                var modal = this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
                this.validateChangeQuantityDialogWasShown = true;
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateDiscountOnChangedRow(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (row.amountCurrency == 0 && row.discountValue) {
            this.translationService.translate("billing.productrows.cannotsetdiscountonzeroamount").then((term) => {
                this.notificationService.showDialog(this.terms["core.info"], term, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                deferral.resolve(false);
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateStockOnChangedRow(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Row is a stock row, show question
        if (this.useStock && row.stockId) {
            this.translationService.translate("billing.productrows.quantitychanged.stock.message").then((term) => {
                var modal = this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Large);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
                this.validateChangeQuantityDialogWasShown = true;
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateChangeQuantityEdi(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Show question about moving EDI rows to another order if quantity is reduced
        if (this.warnOnReducedQuantity && this.prevQuantity > row.quantity && row.ediEntryId) {
            const keys: string[] = [
                "billing.productrows.quantitychanged.moverows.title",
                "billing.productrows.quantitychanged.moverows.message"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["billing.productrows.quantitychanged.moverows.title"], terms["billing.productrows.quantitychanged.moverows.message"].format(this.prevQuantity.toString(), row.quantity.toString(), (this.prevQuantity - row.quantity).toString()), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, SOEMessageBoxSize.Large);
                modal.result.then(val => {
                    if (val === true) {
                        const qtyToMove = [this.prevQuantity - row.quantity];
                        row.quantity = this.prevQuantity;
                        this.moveRowsToOther([row], qtyToMove).then((result) => {
                            this.soeGridOptions.refreshRows(row);
                            deferral.resolve(true);
                        });
                    }
                    else {
                        deferral.resolve(true);
                    }
                }, () => { deferral.resolve(false) });
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private revertProduct(row: ProductRowDTO) {
        // Revert to previous product
        var product = this.getSmallProduct(this.prevProductId);
        row.productId = product ? product.productId : 0;
        row.productNr = product ? product.number : '';
        row.productName = product ? product.name : '';
    }

    private revertQuantity(row: ProductRowDTO) {
        // Revert to previous quantity
        row.quantity = this.prevQuantity;
    }

    private revertAmount(row: ProductRowDTO) {
        // Revert to previous amount
        row.amountCurrency = this.prevAmount;
    }

    private revertDiscount(row: ProductRowDTO) {
        // Revert to previous discount
        row.discountValue = this.prevDiscountValue;
        row.discount2Value = this.prevDiscount2Value;
    }

    private validateChangeAttestState(attestState: AttestStateDTO): boolean {
        if (!attestState)
            return false;

        var errorMessage: string = '';

        var selectedRows = this.soeGridOptions.getSelectedRows();
        var selectedIds: number[] = this.soeGridOptions.getSelectedIds('customerInvoiceRowId');

        if (this.performDirectInvoicing && attestState.attestStateId !== this.initialAttestState.attestStateId) {
            var openLiftRows = _.filter(this.getLiftProductRows(), r => r.attestStateId === this.initialAttestState.attestStateId);
            // If guarantee product, all lift products must have been transfered first (or being transferered now)
            if (openLiftRows.length > 0) {
                // If not all of the open lift rows are selected then show error
                var openLiftRowIds: number[] = _.map(openLiftRows, r => r.customerInvoiceRowId);
                // Remove selected ids from list of all open ids 
                _.pullAll(openLiftRowIds, selectedIds);
                // If any open ids remains, it means that not all open rows are selected
                if (openLiftRowIds.length > 0)
                    errorMessage += this.terms["billing.productrows.dialogs.openliftrowserror"] + '\n';
            }
        }

        _.forEach(selectedRows, row => {
            if (row.productId === this.productGuaranteeId && attestState.attestStateId !== this.initialAttestState.attestStateId) {
                var openLiftRows = _.filter(this.getLiftProductRows(), r => r.attestStateId === this.initialAttestState.attestStateId);
                // If guarantee product, all lift products must have been transfered first (or being transferered now)
                if (openLiftRows.length > 0) {
                    // If not all of the open lift rows are selected then show error
                    var openLiftRowIds: number[] = _.map(openLiftRows, r => r.customerInvoiceRowId);
                    // Remove selected ids from list of all open ids 
                    _.pullAll(openLiftRowIds, selectedIds);
                    // If any open ids remains, it means that not all open rows are selected
                    if (openLiftRowIds.length > 0)
                        errorMessage += this.terms["billing.productrows.changeatteststate.errorlift"] + '\n';
                }
            }

            if (attestState.attestStateId == this.attestStateReadyId && row.isStockRow && row.invoiceQuantity == 0 && this.usePartialInvoicingOnOrderRow) {
                errorMessage += this.terms["billing.productrows.changeatteststate.errorstock"] + '\n';
            }

            if (!this.performDirectInvoicing && _.filter(this.attestTransitions, (a) => a.attestStateFromId === row.attestStateId && attestState.attestStateId === a.attestStateToId).length === 0)
                errorMessage += this.terms["common.customer.invoices.row"] + " " + row.rowNr + " " + this.terms["common.customer.invoices.wrongstatetotransfer"] + " " + attestState.name + '\n';
        });

        if (errorMessage.length > 0) {
            this.notificationService.showDialogEx(this.terms["billing.productrows.changeatteststate.errortitle"], errorMessage, SOEMessageBoxImage.Error);
            return false;
        }

        return true;
    }

    private validateDeleteRow(row: ProductRowDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        this.validateTimeProjectOnChangedRow(row).then(val => {
            if (val === false)
                deferral.resolve(false);
            else {
                if (!row.productNr)
                    deferral.resolve(true);
                else {
                    if (this.showWarningBeforeRowDelete) {
                        this.translationService.translate("billing.productrows.dialogs.verifydeleterow").then((term) => {
                            const modal = this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, false, null, false, null, null, null, null, SOEMessageBoxButton.OK);
                            modal.result.then(val => {
                                deferral.resolve(!!val);
                            });
                        });
                    }
                    else {
                        deferral.resolve(true);
                    }
                }
            }
        });

        return deferral.promise;
    }

    private validateDeleteRows(rows: ProductRowDTO[]): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        const timeRows = this.parentIsOrder() ? rows.filter(r => r.isTimeBillingRow || r.isTimeProjectRow) : [];
        const expenseRows = this.parentIsOrder() ? rows.filter(r => r.isExpenseRow) : [];

        if (timeRows.length > 0) {
            this.translationService.translate("billing.productrows.deleterows.timeproject.message").then((term) => {
                this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                deferral.resolve(false);
            })
        }
        else if (expenseRows.length > 0) {
            this.translationService.translate("billing.productrows.deleterows.expense.message").then((term) => {
                this.notificationService.showDialog(this.terms["core.warning"], term, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                deferral.resolve(false);
            })
        }
        else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private isRowEditable(row: ProductRowDTO): boolean {
        return (row.type === SoeInvoiceRowType.ProductRow || row.type === SoeInvoiceRowType.TextRow || row.type === SoeInvoiceRowType.SubTotalRow);
    }

    private isRowHidden(row: ProductRowDTO): boolean {
        const hiddenAttestStates = _.map(_.filter(this.availableAttestStates, a => a.hidden === true), a => a.attestStateId);
        return (row.attestStateId && _.includes(hiddenAttestStates, row.attestStateId))
    }

    private isRowClosed(row: ProductRowDTO, skipStockStatus: boolean = false): boolean {
        let closedAttestStates = _.map(_.filter(this.availableAttestStates, a => a.closed === true), a => a.attestStateId);
        if (skipStockStatus) {
            closedAttestStates = closedAttestStates.filter(x => x != this.attestStateOrderDeliverFromStockId);
        }
        return (row.attestStateId && _.includes(closedAttestStates, row.attestStateId))
    }

    private isRowLocked(row: ProductRowDTO): boolean {
        const lockedAttestStates = _.map(_.filter(this.availableAttestStates, a => a.locked === true), a => a.attestStateId);
        return (row.attestStateId && _.includes(lockedAttestStates, row.attestStateId))
    }

    private isRowTransferred(row: ProductRowDTO, checkLocked = false, checkPurchasePriceSetting = false): boolean {
        let isTransferred = false;

        if (this.container == ProductRowsContainers.Invoice)
            if (checkPurchasePriceSetting)
                return this.changePurchasePricePermission ? row.isReadOnly && this.originStatus > SoeOriginStatus.Origin : row.isReadOnly;
            else
                return row.isReadOnly;
        else if (this.container == ProductRowsContainers.Offer)
            isTransferred = (this.attestStateTransferredOfferToOrderId === row.attestStateId) || (this.attestStateTransferredOfferToInvoiceId === row.attestStateId);
        else if (this.container == ProductRowsContainers.Order)
            isTransferred = (this.attestStateTransferredOrderToInvoiceId === row.attestStateId || this.attestStateOrderDeliverFromStockId === row.attestStateId);
        else if (this.container == ProductRowsContainers.Contract) {
            const today = CalendarUtility.getDateToday();
            isTransferred = (row.date && row.date > today) || (row.dateTo && row.dateTo < today);
        }

        if (checkLocked && !isTransferred && row.attestStateId && this.attestStates.length > 0) {
            const attestState: AttestStateDTO = _.find(this.attestStates, a => a.attestStateId === row.attestStateId);
            if (attestState && attestState.locked)
                isTransferred = true;
        }

        return isTransferred;
    }

    private isParentRow(row: ProductRowDTO): boolean {
        return _.filter(this.activeRows, r => r.parentRowId === row.tempRowId).length > 0;
    }

    private isChildRow(row: ProductRowDTO): boolean {
        return !!row.parentRowId;
    }

    private isFixedPriceRow(row: ProductRowDTO): boolean {
        let product = row && row.productId && row.productId > 0 ? this.getFullProduct(row.productId) : undefined;
        return (row && (this.fixedPriceProductId !== 0 && row.productId === this.fixedPriceProductId) || (row.productId && product && product.calculationType === TermGroup_InvoiceProductCalculationType.FixedPrice)) || this.isFixedPriceKeepPricesRow(row);
    }

    private isGuaranteeRow(row: ProductRowDTO): boolean {
        return (row && this.productGuaranteeId !== 0 && row.productId === this.productGuaranteeId);
    }

    private isFixedPriceKeepPricesRow(row: ProductRowDTO): boolean {
        return row && this.fixedPriceKeepPricesProductId !== 0 && row.productId === this.fixedPriceKeepPricesProductId;
    }

    private hasFixedPriceProducts(): boolean {
        return this.getFixedPriceProductRows().length > 0;
    }

    private hasFixedPriceKeepPricesProducts(): boolean {
        return this.getFixedPriceKeepPricesProductRows().length > 0;
    }

    private hasLiftProducts(): boolean {
        return this.getLiftProductRows().length > 0;
    }

    private hasNonSortableRows() {
        return _.filter(this.activeRows, r => r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow).length > 0;
    }

    private isNewProduct(productId: number): boolean {
        return _.find(this.products, p => p.productId === productId) === undefined;
    }

    private isTaxDeductionRow(row: ProductRowDTO): boolean {
        return (this.householdTaxDeductionProductId && row.productId === this.householdTaxDeductionProductId) ||
            (this.householdTaxDeductionDeniedProductId && row.productId === this.householdTaxDeductionDeniedProductId) ||
            (this.household50TaxDeductionProductId && row.productId === this.household50TaxDeductionProductId) ||
            (this.household50TaxDeductionDeniedProductId && row.productId === this.household50TaxDeductionDeniedProductId) ||
            (this.rutTaxDeductionProductId && row.productId === this.rutTaxDeductionProductId) ||
            (this.rutTaxDeductionDeniedProductId && row.productId === this.rutTaxDeductionDeniedProductId) ||
            (this.green15TaxDeductionProductId && row.productId === this.green15TaxDeductionProductId) ||
            (this.green15TaxDeductionDeniedProductId && row.productId === this.green15TaxDeductionDeniedProductId) ||
            (this.green20TaxDeductionProductId && row.productId === this.green20TaxDeductionProductId) ||
            (this.green20TaxDeductionDeniedProductId && row.productId === this.green20TaxDeductionDeniedProductId) ||
            (this.green50TaxDeductionProductId && row.productId === this.green50TaxDeductionProductId) ||
            (this.green50TaxDeductionDeniedProductId && row.productId === this.green50TaxDeductionDeniedProductId);
    }

    private hasLiftRowsNotTransferableToInvoice(): boolean {
        let notReadyForInvoice = false;

        if (this.hasFixedPriceProducts() && this.hasLiftProducts && this.useAttestState) {
            _.forEach(_.filter(this.activeProductRows, r => r.isLiftProduct && r.attestStateId !== this.attestStateTransferredOrderToInvoiceId), row => {
                if (row.attestStateId === this.initialAttestState.attestStateId) {
                    notReadyForInvoice = true;
                    return;
                }

                var attestTransition = _.find(this.attestTransitions, a => a.attestStateFrom && a.attestStateFrom.attestStateId === row.attestStateId && a.attestStateTo && a.attestStateTo.attestStateId === this.attestStateTransferredOrderToInvoiceId);
                if (!attestTransition)
                    notReadyForInvoice = true;
            });
        }

        return notReadyForInvoice;
    }

    private hasRowsNotTransferableToInvoice(): boolean {
        let notReadyForInvoice = false;

        if (this.hasFixedPriceProducts() && this.hasLiftProducts && this.useAttestState) {
            _.forEach(_.filter(this.activeProductRows, r => (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow) && r.attestStateId !== this.attestStateTransferredOrderToInvoiceId), row => {
                if (row.attestStateId === this.initialAttestState.attestStateId) {
                    notReadyForInvoice = true;
                    return;
                }

                var attestTransition = _.find(this.attestTransitions, a => a.attestStateFrom && a.attestStateFrom.attestStateId === row.attestStateId && a.attestStateTo && a.attestStateTo.attestStateId === this.attestStateTransferredOrderToInvoiceId);
                if (!attestTransition)
                    notReadyForInvoice = true;
            });
        }

        return notReadyForInvoice;
    }

    private numberOfContractRowsTransferrableToInvoice(): number {
        if (this.container === ProductRowsContainers.Order)
            return this.numberOfTransferrableContractRows(this.attestStateTransferredOrderToInvoiceId);

        return 0;
    }

    private numberOfTransferrableContractRows(targetAttestStateId: number): number {
        var noOfRows: number = 0;

        if (this.attestTransitions) {
            _.forEach(_.filter(this.activeRows, r => !this.isRowTransferred(r) && r.isContractProduct && (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow || r.type === SoeInvoiceRowType.TextRow)), row => {
                // Check if any row has an attest state which has a transition to the target attest state
                var attestTransition = _.find(this.attestTransitions, a => a.attestStateFrom.attestStateId === row.attestStateId && a.attestStateTo.attestStateId === targetAttestStateId);
                if (attestTransition)
                    noOfRows++;
            });
        }

        return noOfRows;
    }

    private transferFixedPriceToInvoiceLeavingOthers(): boolean {
        if (this.container == ProductRowsContainers.Offer)
            return this.transferFixedPriceLeavingOthers(this.attestStateTransferredOfferToInvoiceId);
        else if (this.container == ProductRowsContainers.Order)
            return this.transferFixedPriceLeavingOthers(this.attestStateTransferredOrderToInvoiceId);

        return false;
    }

    private transferFixedPriceLeavingOthers(targetAttestStateId: number): boolean {
        var transferFixedPrice: boolean = false;
        var rowsWithInitialAttestState: boolean = false;

        if (this.attestTransitions) {
            _.forEach(_.filter(this.activeRows, r => !this.isRowTransferred(r) && (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow || r.type === SoeInvoiceRowType.TextRow)), row => {
                // Check if any row has an attest state which has a transition to the target attest state
                var attestTransition = _.find(this.attestTransitions, a => a.attestStateFrom.attestStateId === row.attestStateId && a.attestStateTo.attestStateId === targetAttestStateId);
                if (attestTransition) {
                    if (this.isFixedPriceRow(row))
                        transferFixedPrice = true;
                } else {
                    rowsWithInitialAttestState = true;
                }
            });
        }

        return transferFixedPrice && rowsWithInitialAttestState;
    }

    private hasDeductionAmountMismatch(): any {
        const existingHouseholdAmount = _.sumBy(_.filter(this.activeProductRows, (x) => x.isHouseholdRow), (y) => y.amountCurrency);
        const row = _.find(this.activeProductRows, (x) => x.isHouseholdRow);
        const totalHouseholdAmount = row ? this.calculateMaxDeductableAmount(row, true) * -1 : 0;
        return { value: totalHouseholdAmount !== existingHouseholdAmount, existingAmount: existingHouseholdAmount, calculatedAmount: totalHouseholdAmount };
    }

    private getRowNrColumn() {
        return this.soeGridOptions.getColumnByField('rowNr');
    }

    private getProductNrColumn() {
        return this.soeGridOptions.getColumnByField('productNr');
    }

    private getSingelValueColumn() {
        return this.soeGridOptions.getSingelValueColumn();
    }

    private getQuantityColumn() {
        return this.soeGridOptions.getColumnByField('quantity');
    }

    private getAmountColumn() {
        return this.soeGridOptions.getColumnByField('amountCurrency');
    }

    private getDefaultVatPercent(vatCodeId: number): number {
        var value: number = this.defaultVatRate;

        // Get specified Vat code
        var vatCode: VatCodeDTO = _.find(this.vatCodes, v => v.vatCodeId === vatCodeId);
        if (vatCode && vatCode.percent > 0)
            value = vatCode.percent;

        return value;
    }

    private getChildRows(row: ProductRowDTO): ProductRowDTO[] {
        return _.sortBy(_.filter(this.activeRows, r => r.parentRowId === row.tempRowId), 'rowNr');
    }

    private getParentRows(row: ProductRowDTO): ProductRowDTO[] {
        return _.sortBy(_.filter(this.activeRows, r => r.tempRowId === row.parentRowId), 'rowNr');
    }

    private getParentRow(row: ProductRowDTO): ProductRowDTO {
        return _.find(this.activeRows, r => r.tempRowId === row.parentRowId);
    }

    private getOnlyProductRows(ignoreHidden: boolean = false): ProductRowDTO[] {
        const rows = _.filter(this.activeRows, r => r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow || r.isFreightAmountRow || r.isInvoiceFeeRow);
        if (ignoreHidden && this.availableAttestStates != null) {
            var hiddenAttestState = _.filter(this.availableAttestStates, a => a.hidden === true);
            return _.filter(rows, r => r.attestStateId != null && _.find(hiddenAttestState, a => a.attestStateId === r.attestStateId) != null);
        }
        else {
            return rows;
        }
    }

    private getFixedPriceProductRows(): ProductRowDTO[] {
        return this.activeRows.filter(r => r.isFixedPriceProduct);
    }

    private getFixedPriceKeepPricesProductRows(): ProductRowDTO[] {
        return _.filter(this.activeRows, r => r.productId === this.fixedPriceKeepPricesProductId && this.fixedPriceKeepPricesProductId !== 0);
    }

    private getLiftProductRows(): ProductRowDTO[] {
        return _.filter(this.activeRows, r => r.isLiftProduct);
    }


    private setRowTypeIcon(row: ProductRowDTO) {
        if (row.isTimeBillingRow) {
            row.rowTypeIcon = 'fal fa-file-invoice-dollar';
        }
        else if (row.isExpenseRow) {
            row.rowTypeIcon = 'fal fa-wallet';
        }
        else if (row.isTimeProjectRow) {
            row.rowTypeIcon = 'fal fa-clock';
        }
        else if (row.supplierInvoiceId) {
            row.rowTypeIcon = 'fal fa-file-import';
        }
        else {
            switch (row.type) {
                case SoeInvoiceRowType.ProductRow:
                    row.rowTypeIcon = 'fal fa-box-alt';
                    break;
                case SoeInvoiceRowType.TextRow:
                    row.rowTypeIcon = 'fal fa-text';
                    break;
                case SoeInvoiceRowType.PageBreakRow:
                    row.rowTypeIcon = 'fal fa-cut';
                    break;
                case SoeInvoiceRowType.SubTotalRow:
                    row.rowTypeIcon = 'fal fa-calculator-alt';
                    break;
            }
        }
    }

    public setAllRowAsModified(notify: boolean) {
        _.forEach(this.activeProductRows, r => {
            this.setRowAsModified(r, false);
        });
        if (notify)
            this.setParentAsModified();
    }

    public setRowAsModified(row: ProductRowDTO, notify: boolean = true) {
        if (row) {
            row.isModified = true;
            if (notify)
                this.setParentAsModified();
        }
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid }));
    }

    private setProductValuesFromId(row: ProductRowDTO, productId: number, ignorePurchasePrice = false) {
        const product = this.getFullProduct(productId);
        if (product)
            this.setProductValues(row, product, ignorePurchasePrice);
        else
            this.loadProduct(productId, row);
    }

    private setProductValues(row: ProductRowDTO, product: ProductRowsProductDTO, ignorePurchasePrice = false) {
        let prevProductId: number = row.productId ? row.productId : 0;
        let prevAmount: number = row.amount;

        // Set Product values
        row.productId = product ? product.productId : 0;
        row.productNr = product ? product.number : '';
        row.productName = product ? product.name : '';
        row.isStockRow = product ? product.isStockProduct : false;

        // Product has changed, set text from product
        if (prevProductId === 0 || (prevProductId !== row.productId))
            row.text = product ? product.name : '';

        //Intrastat
        row.sysCountryId = product && product.sysCountryId ? product.sysCountryId : undefined;
        row.intrastatCodeId = product && product.intrastatCodeId ? product.intrastatCodeId : undefined;

        // Set household type
        if (product) {
            if (product.productId === this.householdTaxDeductionProductId || product.productId === this.household50TaxDeductionProductId)
                row.householdTaxDeductionType = TermGroup_HouseHoldTaxDeductionType.ROT;
            else if (product.productId === this.rutTaxDeductionProductId)
                row.householdTaxDeductionType = TermGroup_HouseHoldTaxDeductionType.RUT;
            else if (product.productId === this.green15TaxDeductionProductId || product.productId === this.green20TaxDeductionProductId || product.productId === this.green50TaxDeductionProductId)
                row.householdTaxDeductionType = TermGroup_HouseHoldTaxDeductionType.GREEN;
        }

        if (product && product.householdDeductionType && product.householdDeductionType !== 0) {
            this.setHouseholdDeductionType(row, product.householdDeductionType);
        }
        else {
            this.setHouseholdDeductionType(row, this.defaultHouseholdDeductionType);
        }

        // Get customer specific product prices
        let customerProduct: ICustomerProductPriceSmallDTO = null;
        if (product && this.customer && !this.priceListTypeIsProject && !product.isExternal) //If project pricelist -> Go to server.
            customerProduct = _.find(this.customer.customerProducts, { productId: product.productId });

        this.customerPriceSet = false;

        if ((!product || (!this.isTaxDeductionRow(row))) && !row.isInterestRow) {
            if (customerProduct) {
                row.amount = customerProduct.price;
                row.sysWholesellerName = this.terms["billing.productrows.customerprice"];
                this.customerPriceSet = true;
            } else {
                row.amount = product ? product.salesPrice : 0;
            }

            // Only calculate currencies if amount has changed
            if (prevAmount !== row.amount)
                this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
        }

        // Discount was missing when adding timebook rows
        if (product)
            this.setCustomerDiscount(row, product);

        if (ignorePurchasePrice) {
            row.purchasePrice = 0;
            row.purchasePriceCurrency = 0;
        }
        else {
            row.purchasePrice = product ? product.purchasePrice : 0;
            this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.PurchasePrice, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
        }

        if (!this.customerPriceSet)
            row.sysWholesellerName = product ? product.sysWholesellerName : '';

        // Set ProductUnit values
        row.productUnitId = product && product.productUnitId ? product.productUnitId : this.defaultProductUnitId;
        row.productUnitCode = product && product.productUnitId ? product.productUnitCode : this.defaultProductUnitCode;

        // Lift 
        row.isLiftProduct = product ? product.isLiftProduct : false;

        // Contract
        row.isContractProduct = product ? product.calculationType === TermGroup_InvoiceProductCalculationType.Contract : false;

        // Clearing
        row.isClearingProduct = product ? product.calculationType === TermGroup_InvoiceProductCalculationType.Clearing : false;

        // Fixed price
        row.isFixedPriceProduct = product ? product.calculationType === TermGroup_InvoiceProductCalculationType.FixedPrice : false;

        // Set VAT account values
        this.resetVatAccount(row);

        if (product && product.showDescriptionAsTextRow) {
            let textRow: ProductRowDTO = _.find(this.activeRows, r => r.parentRowId === row.tempRowId && r.type === SoeInvoiceRowType.TextRow && !r.isHouseholdTextRow);
            if (!textRow) {
                // Add new TextRow
                textRow = this.addRow(SoeInvoiceRowType.TextRow, false).row;

                this.multiplyRowNr();
                textRow.parentRowId = row.tempRowId;
                textRow.rowNr = row.rowNr + 1;
                textRow.text = product.description;
                this.reNumberRows();
            }
        }
    }

    private setHouseholdDeductionType(row: ProductRowDTO, householdDeductionType: number) {
        row.householdDeductionType = householdDeductionType;
        const type = this.householdDeductionTypes.find(t => t.value === householdDeductionType);
        row.householdDeductionTypeText = type?.label ?? "";
    }

    private setCustomerDiscount(row: ProductRowDTO, product: ProductRowsProductDTO) {
        row.discountType = row.discount2Type = SoeInvoiceRowDiscountType.Percent;
        if (product) {
            switch (product.vatType) {
                case TermGroup_InvoiceProductVatType.None:
                    row.discountValue = row.discount2Value = 0;
                    break;
                case TermGroup_InvoiceProductVatType.Merchandise:
                    if (product.dontUseDiscountPercent === false) { 
                        row.discountValue = row.discountValue === 0 && this.customer ? this.customer.discountMerchandise : row.discountValue;
                        row.discount2Value = row.discount2Value === 0 && this.customer ? this.customer.discount2Merchandise : row.discount2Value;
                    }
                    break;
                case TermGroup_InvoiceProductVatType.Service:
                    if (product.dontUseDiscountPercent === false) { 
                        row.discountValue = row.discountValue === 0 && this.customer ? this.customer.discountService : row.discountValue;
                        row.discount2Value = row.discount2Value === 0 && this.customer ? this.customer.discount2Service : row.discount2Value;
                    }
                    break;
            }
        }
    }

    private resetVatAccount(row: ProductRowDTO) {
        if (((this.vatType === TermGroup_InvoiceVatType.ExportWithinEU && this.customer && StringUtility.isEmpty(this.customer.vatNr)) || (this.vatType !== TermGroup_InvoiceVatType.ExportWithinEU && this.vatType !== TermGroup_InvoiceVatType.ExportOutsideEU && this.vatType !== TermGroup_InvoiceVatType.Contractor && this.vatType !== TermGroup_InvoiceVatType.NoVat)) &&
            row.productId && row.productId !== this.householdTaxDeductionDeniedProductId && row.productId !== this.household50TaxDeductionDeniedProductId && row.productId !== this.rutTaxDeductionDeniedProductId &&
            row.productId !== this.green15TaxDeductionDeniedProductId && row.productId !== this.green20TaxDeductionDeniedProductId && row.productId !== this.green50TaxDeductionDeniedProductId) {
            // Priority order for setting VAT account
            // 1. Product has a VAT code, use account on code
            // 2. Product has a VAT account, user it
            // 3. Company setting for default VAT code

            var product = this.getFullProduct(row.productId);
            if (product && product.vatCodeId)
                this.setProductVatAccountFromVatCode(row, product.vatCodeId);
            else
                this.loadProductVatAccount(row);
        } else {
            this.$timeout(() => {
                this.setProductVatAccount(row, null);
            }, 100);
        }
    }

    private setProductVatAccount(row: ProductRowDTO, accountsItem: ProductAccountsDTO) {
        row.vatAccountId = accountsItem ? accountsItem.vatAccountDim1Id : undefined;
        row.vatAccountNr = accountsItem ? accountsItem.vatAccountDim1Nr : '';
        row.vatAccountName = accountsItem ? accountsItem.vatAccountDim1Name : '';
        row.vatRate = accountsItem ? accountsItem.vatRate : 0;

        this.amountHelper.calculateRowSum(row, false);

        if (this.vatChangedCounter > 0) {
            this.vatChangedCounter = this.vatChangedCounter - 1;
            if (this.vatChangedCounter === 0)
                this.resetRows();
        }
    }

    private setProductVatAccountFromVatCode(row: ProductRowDTO, vatCodeId: number) {
        // Get specified Vat code
        const vatCode: VatCodeDTO = _.find(this.vatCodes, v => v.vatCodeId === vatCodeId);
        if (vatCode) {
            // Set Vat code on row
            row.vatCodeId = vatCode.vatCodeId;
            row.vatCodeCode = vatCode.code;

            // Set Vat account and rate from Vat code
            const productAccounts: ProductAccountsDTO = new ProductAccountsDTO();
            productAccounts.vatAccountDim1Id = vatCode.accountId;
            productAccounts.vatAccountDim1Nr = vatCode.accountNr;
            productAccounts.vatRate = vatCode.percent;
            this.setProductVatAccount(row, productAccounts);
        }
    }

    private setFixedPriceDialog(productNr: string): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        const keys: string[] = [
            "billing.productrows.dialogs.fixedpricefound",
            "billing.productrows.fixpriceitemwarning",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["billing.productrows.dialogs.fixedpricefound"], terms["billing.productrows.fixpriceitemwarning"].format(productNr), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Medium, false, false, undefined, false, undefined, undefined, undefined, undefined, SOEMessageBoxButton.No);
            modal.result.then(result => {
                if (result)
                    deferral.resolve(true);
                else
                    deferral.resolve(false);
            });
        });

        return deferral.promise;
    }

    private setFixedPrice(row: ProductRowDTO): boolean {
        if (!this.isFixedPriceRow(row) && this.hasFixedPriceProducts()) {
            row.amount = 0;
            row.amountCurrency = 0;
            this.amountHelper.calculateRowSum(row, false);
            return true;
        }
        else {
            return false;
        }
    }

    private setIsFixedPrice(row: ProductRowDTO) {
        const hasExistingFixedPriceRow = this.activeRows.find(r => r.isFixedPriceProduct && r.tempRowId !== row.tempRowId);
        const setFixed = () => {

            this.fixedPrice = true;
            this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_FIXED_PRICE_ADDED, { guid: this.parentGuid, orderType: OrderContractType.Fixed }));

            //row.quantity = 1;
            if (!this.fixedPriceKeepPrices) {
                // Clear amount on all "non fixed price product" rows (if not transferred or household or guaranteeproduct)
                this.activeRows.filter(r => r.type === SoeInvoiceRowType.ProductRow && !r.isHouseholdRow && !r.isLiftProduct && !r.isClearingProduct && r.productId !== this.productGuaranteeId && r.amountCurrency !== 0).forEach((loopRow: ProductRowDTO) => {
                    const product = this.getFullProduct(loopRow.productId);
                    if (!this.isFixedPriceRow(loopRow) && !this.isRowTransferred(loopRow) && (!product || product.calculationType !== TermGroup_InvoiceProductCalculationType.FixedPrice)) {
                        loopRow.amount = 0;
                        loopRow.amountCurrency = 0;
                        this.amountHelper.calculateRowSum(loopRow, false);
                        this.soeGridOptions.refreshRows(loopRow);
                    }
                });
            }
        }

        if (!hasExistingFixedPriceRow) {
            if (this.activeProductRows.length > 1) {
                this.setFixedPriceDialog(row.productNr).then((answer: boolean) => {
                    if (answer) {
                        setFixed();
                    }
                    else {
                        row.productNr = undefined;
                        row.productName = undefined;
                        row.text = undefined;
                        this.soeGridOptions.refreshRows(row);
                        this.soeGridOptions.startEditingCell(row, this.getProductNrColumn());
                    }
                });
            }
            else {
                setFixed();
            }
        }
    }

    private setSumAmountLabel() {
        if (!this.terms)
            return;

        // Set sum label without VAT if rows contains only a household tax deduction denied product
        if (_.filter(this.activeProductRows, r => r.productId === this.householdTaxDeductionDeniedProductId || r.productId === this.household50TaxDeductionDeniedProductId || r.productId === this.rutTaxDeductionDeniedProductId).length === 0 ||
            _.filter(this.activeProductRows, r => r.productId !== this.householdTaxDeductionDeniedProductId && r.productId !== this.household50TaxDeductionDeniedProductId && r.productId !== this.rutTaxDeductionDeniedProductId).length > 0)
            this.sumAmountCurrencyLabel = this.terms["billing.productrows.sumamount.exclvat"];
        else
            this.sumAmountCurrencyLabel = this.terms["billing.productrows.sumamount.inclvat"];

    }

    private setFeeAmountLabel() {
        if (!this.terms)
            return;

        this.feeCurrencyLabel = this.terms["billing.customer.payment.fee"];
    }

    private reCalculateRowSums(forceUpdate = false) {
        this.activeProductRows.forEach((r: ProductRowDTO) => {
            this.amountHelper.calculateRowSum(r, true, false, null, true);
        });
    }

    private calculateAmounts() {
        // Check VAT type
        const noVAT: boolean = (this.vatType === TermGroup_InvoiceVatType.Contractor || this.vatType === TermGroup_InvoiceVatType.ExportOutsideEU || this.vatType === TermGroup_InvoiceVatType.NoVat || (this.vatType === TermGroup_InvoiceVatType.ExportWithinEU && this.customer && !StringUtility.isEmpty(this.customer.vatNr)));

        // Sum the gross amount and VAT amount of all product rows
        let sumCurrency: number = 0;
        this.vatAmountCurrency = 0;
        let sumHHTaxCurrency: number = 0;
        if (this.container == ProductRowsContainers.Order || this.container == ProductRowsContainers.Offer) {
            _.forEach(_.filter(this.activeProductRows, r => !r.isLiftProduct && !r.isClearingProduct), (row) => {
                if (((this.fixedPriceKeepPrices && row.productId === this.fixedPriceKeepPricesProductId) || row.productId !== this.productGuaranteeId) && (this.isFixedPriceKeepPricesRow(row) || this.isFixedPriceRow(row) ? true : !this.isRowClosed(row))) {
                    if (!row.householdAmountCurrency) {
                        sumCurrency += row.sumAmountCurrency;
                        this.vatAmountCurrency += row.vatAmountCurrency;
                    } else {
                        sumHHTaxCurrency += row.sumAmountCurrency;
                    }
                }
            });
        } else {
            _.forEach(this.activeProductRows, (row) => {
                if (((this.fixedPriceKeepPrices && row.productId === this.fixedPriceKeepPricesProductId) || !this.fixedPriceKeepPrices) && !this.isRowClosed(row)) {
                    if (!row.householdAmountCurrency) {
                        sumCurrency += row.sumAmountCurrency;
                        this.vatAmountCurrency += row.vatAmountCurrency;
                    } else {
                        sumHHTaxCurrency += row.householdAmountCurrency;
                    }
                }
            });
        }
        sumCurrency = sumCurrency.round(2);
        sumHHTaxCurrency = sumHHTaxCurrency.round(2);

        // Add VAT amount for freight
        if (!noVAT) {
            var freightRow = _.find(this.activeRows, r => r.isFreightAmountRow);
            var freightAmountVatCurrency: number = 0;
            if (freightRow)
                freightAmountVatCurrency = freightRow.vatAmountCurrency;
            else if (this.freightAmountCurrency) {
                if (this.priceListTypeInclusiveVat)
                    freightAmountVatCurrency = (this.freightAmountCurrency - (this.freightAmountCurrency / (1 + this.defaultVatRate / 100)));
                else
                    freightAmountVatCurrency = (this.freightAmountCurrency * this.defaultVatRate / 100);
            }
            this.vatAmountCurrency += freightAmountVatCurrency;
        }
        this.amountHelper.getCurrencyAmount(this.freightAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.freightAmount = am.round(2) });

        // Check for invoice fee limit
        if (this.container != ProductRowsContainers.Offer && this.useInvoiceFee && this.useInvoiceFeeLimit && !this.disableInvoiceFee) {
            if ((sumCurrency + sumHHTaxCurrency + this.freightAmountCurrency) > this.useInvoiceFeeLimitAmount) {
                // Limit reached remove invoice fee
                this.invoiceFee = 0;
                this.invoiceFeeCurrency = 0;

                const invoiceFeeRowForDelete = _.find(this.activeRows, r => r.isInvoiceFeeRow);
                if (invoiceFeeRowForDelete)
                    this.deleteRow(invoiceFeeRowForDelete);
            } else if (!this.invoiceFee && !this.invoiceFeeCurrency) {
                // Set default invoice fee
                this.getInvoiceFee();
            }
        }

        // Add VAT amount for invoice fee
        if (!noVAT) {
            const invoiceFeeRow = _.find(this.activeRows, r => r.isInvoiceFeeRow);
            let invoiceFeeVatAmountCurrency = 0;
            if (invoiceFeeRow)
                invoiceFeeVatAmountCurrency = invoiceFeeRow.vatAmountCurrency;
            else {
                if (this.priceListTypeInclusiveVat)
                    invoiceFeeVatAmountCurrency = (this.invoiceFeeCurrency - (this.invoiceFeeCurrency / (1 + this.defaultVatRate / 100)));
                else
                    invoiceFeeVatAmountCurrency = (this.invoiceFeeCurrency * this.defaultVatRate / 100);
            }

            this.vatAmountCurrency += invoiceFeeVatAmountCurrency;
        }

        this.vatAmountCurrency = noVAT ? 0 : this.vatAmountCurrency.roundToNearest(2);

        // Sum exclusive VAT
        this.sumAmountCurrency = this.priceListTypeInclusiveVat ? (sumCurrency + this.freightAmountCurrency + this.invoiceFeeCurrency - this.vatAmountCurrency) : sumCurrency;
        this.amountHelper.getCurrencyAmount(this.sumAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.sumAmount = am.round(2) });
        this.amountHelper.getCurrencyAmount(this.invoiceFeeCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.invoiceFee = am.round(2) });

        // VAT
        this.amountHelper.getCurrencyAmount(this.vatAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => {
            this.vatAmount = am.roundToNearest(2);
        });

        // Sum the total amount
        let totalAmountCurrency = sumCurrency + this.freightAmountCurrency + this.invoiceFeeCurrency;
        if (this.container == ProductRowsContainers.Invoice) {
            totalAmountCurrency = totalAmountCurrency - sumHHTaxCurrency;
        }
        else {
            totalAmountCurrency = totalAmountCurrency + sumHHTaxCurrency;
        }

        if (!this.priceListTypeInclusiveVat)
            totalAmountCurrency += this.vatAmountCurrency;

        var cent: number = 0;
        if (this.useCentRounding) {
            cent = Math.abs(totalAmountCurrency) - Math.floor(Math.abs(totalAmountCurrency))
            if (cent !== 0) {
                cent = totalAmountCurrency.round(0) - totalAmountCurrency;
                if (this.isCredit) {
                    totalAmountCurrency = Math.abs(totalAmountCurrency).round(0);
                    totalAmountCurrency = -totalAmountCurrency;
                }
                else
                    totalAmountCurrency = totalAmountCurrency.round(0);
            }
        }
        else if (this.isCashSale && this.centRounding && this.centRounding !== 0) {
            totalAmountCurrency = totalAmountCurrency + this.centRounding;
        }

        // Cash payment (in Finland round to the nearest 5 cents) - IS THIS STILL NEEDED?      
        /*if (this.isCashSale && CoreUtility.sysCountryId == TermGroup_Languages.Finnish) {            
            cent = Math.abs(totalAmountCurrency) - Math.floor(Math.abs(totalAmountCurrency))        
            if (cent) {
                cent = ((totalAmountCurrency / 0.05).round(0) * 0.05) - totalAmountCurrency;                
                totalAmountCurrency = (totalAmountCurrency / 0.05).round(0) * 0.05;                
            }
        }*/
        if (!(this.isCashSale && !this.useCentRounding && this.centRounding && this.centRounding !== 0))
            this.centRounding = +cent.toFixed(2);

        // TODO: This causes this error:
        // angular.js ? v = 60.4 : 13424 RangeError: Maximum call stack size exceeded
        // at ProductRowsController.get[as activeRows] (http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/ProductRowsDirective.js?v=60.4:296:35)
        // at ProductRowsController.get[as activeProductRows] (http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/ProductRowsDirective.js?v=60.4:304:45)
        // at ProductRowsController.calculateAmounts(http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/ProductRowsDirective.js?v=60.4:2148:48)
        // at http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/ProductRowsDirective.js?v=60.4:412:35
        // at http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/Helpers/AmountHelper.js?v=60.4:49:65
        // at Array.forEach(native)
        // at AmountHelper.calculateAmounts(http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/Helpers/AmountHelper.js?v=60.4:49:35)
        // at AmountHelper.calculateRowSum(http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/Helpers/AmountHelper.js?v=60.4:102:38)
        // at ProductRowsController.updateCentRounding(http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/ProductRowsDirective.js?v=60.4:2368:43)
        // at ProductRowsController.calculateAmounts(http://main.softone.se/angular/TypeScript/Billing/Directives/ProductRows/ProductRowsDirective.js?v=60.4:2252:26)
        //this.updateCentRounding(cent);
        this.totalAmountCurrency = totalAmountCurrency;

        this.amountHelper.getCurrencyAmount(totalAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { this.totalAmount = am.round(2) });

        this.feeAmountCurrency = 0;
        if (this.freightAmountCurrency) {
            this.feeAmountCurrency += this.freightAmountCurrency;
        }
        if (this.useInvoiceFee && this.invoiceFeeCurrency) {
            this.feeAmountCurrency += this.invoiceFeeCurrency;
        }

        this.vatPercent = 0;
        if (this.priceListTypeInclusiveVat) {
            if ((this.totalAmountCurrency - this.vatAmountCurrency - this.centRounding) !== 0) {
                this.vatPercent = ((this.vatAmountCurrency / this.sumAmountCurrency) * 100);
            }
        } else {
            if ((this.sumAmountCurrency) !== 0) {
                this.vatPercent = (this.vatAmountCurrency / (this.sumAmountCurrency + this.freightAmountCurrency + this.invoiceFeeCurrency) * 100);
            }
        }
        if (!this.vatPercent)
            this.vatPercent = this.getDefaultVatPercent(0);
        this.vatPercent = parseFloat(this.vatPercent.toFixed(1));

        // Calculate marginal income
        this.calculateMarginalIncome();

        this.setSumAmountLabel();
        this.setFeeAmountLabel();

        if (this.container == ProductRowsContainers.Order)
            this.calculateRemainingAmount();

        if (this.amountHelper.calculateSubTotals(this.activeRows))
            this.soeGridOptions.refreshRows();
    }

    private calculateMarginalIncome() {
        let purchaseAmountCurrency: number = 0;
        let purchaseAmountTimeCurrency: number = 0;
        let purchaseAmountProductCurrency: number = 0;
        let salesAmountCurrency: number = 0;
        let salesAmountTimeCurrency: number = 0;
        let salesAmountProductCurrency: number = 0;
        let fixedPricePurchaseAmount: number = 0;

        let rows = _.filter(this.activeProductRows, r => r.quantity && (this.isFixedPriceKeepPricesRow(r) || this.isFixedPriceRow(r) ? true : !this.isRowClosed(r, true)) && (r.purchasePriceCurrency || r.purchasePriceCurrency === 0 || this.isFixedPriceRow(r) || r.isLiftProduct) && !r.isHouseholdRow);
        let hasFixedPriceRows = _.some(rows, (r) => r.productId === this.fixedPriceProductId || r.productId === this.fixedPriceKeepPricesProductId);

        if (!this.calculateMarginalIncomeOnZeroPurchase)
            rows = _.filter(rows, r => r.purchasePriceCurrency !== 0);

        _.forEach(rows, (row) => {
            const product = _.find(this.productList, p => p.productId === row.productId);

            // Count time and product income amounts and ratios separately
            if (product) {
                if (product.vatType === TermGroup_InvoiceProductVatType.Service) {
                    if (this.isFixedPriceRow(row) && row.purchasePriceCurrency > 0) {
                        purchaseAmountTimeCurrency = (row.purchasePriceCurrency * row.quantity).round(2);
                        salesAmountTimeCurrency = this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                    } else {
                        purchaseAmountTimeCurrency += (row.purchasePriceCurrency * row.quantity).round(2);
                        salesAmountTimeCurrency += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                    }
                } else {
                    if (this.isFixedPriceRow(row) && row.purchasePriceCurrency > 0) {
                        purchaseAmountProductCurrency = (row.purchasePriceCurrency * row.quantity).round(2);
                        salesAmountProductCurrency = this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                    } else {
                        purchaseAmountProductCurrency += (row.purchasePriceCurrency * row.quantity).round(2);
                        salesAmountProductCurrency += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                    }
                }
            } else {
                if (this.isFixedPriceRow(row) && row.purchasePriceCurrency > 0) {
                    purchaseAmountProductCurrency = (row.purchasePriceCurrency * row.quantity).round(2);
                    salesAmountProductCurrency = this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                } else {
                    purchaseAmountProductCurrency += (row.purchasePriceCurrency * row.quantity).round(2);
                    salesAmountProductCurrency += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                }
            }

            if (this.isFixedPriceRow(row)) {
                salesAmountCurrency += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                fixedPricePurchaseAmount += (row.purchasePriceCurrency * row.quantity).round(2);
            } else {
                if (!(this.container == ProductRowsContainers.Order && (row.isLiftProduct || row.isClearingProduct))) {
                    purchaseAmountCurrency += (row.purchasePriceCurrency * row.quantity).round(2);
                    salesAmountCurrency += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                }
            }
        });

        if (fixedPricePurchaseAmount != 0)
            purchaseAmountCurrency = fixedPricePurchaseAmount;

        this.marginalIncomeCurrency = this.isCredit ? salesAmountCurrency + purchaseAmountCurrency : salesAmountCurrency - purchaseAmountCurrency;
        this.marginalIncomeRatio = (salesAmountCurrency !== 0 ? (this.marginalIncomeCurrency / salesAmountCurrency).round(4) : 1) * 100;
        var marginalIncomeCurrencyTime: number = this.isCredit ? salesAmountTimeCurrency + purchaseAmountTimeCurrency : salesAmountTimeCurrency - purchaseAmountTimeCurrency;
        var marginalIncomeRatioTime: number = (salesAmountTimeCurrency !== 0 ? (marginalIncomeCurrencyTime / salesAmountTimeCurrency).round(4) : 1) * 100;
        var marginalIncomeCurrencyProduct: number = this.isCredit ? salesAmountProductCurrency + purchaseAmountProductCurrency : salesAmountProductCurrency - purchaseAmountProductCurrency;
        var marginalIncomeRatioProduct: number = (salesAmountProductCurrency !== 0 ? (marginalIncomeCurrencyProduct / salesAmountProductCurrency).round(4) : 1) * 100;

        if (this.marginalIncomeCurrency < 0 && this.marginalIncomeRatio > 0)
            this.marginalIncomeRatio *= -1;

        // Tooltips for material and work
        this.marginalIncomeCurrencyToolTip = this.terms ? this.terms["billing.productrows.marginalincome.tooltip"].format(this.amountFilter(marginalIncomeCurrencyProduct), this.amountFilter(marginalIncomeCurrencyTime)) : "";
        this.marginalIncomeRatioToolTip = this.terms ? this.terms["billing.productrows.marginalincome.ratiotooltip"].format(this.amountFilter(marginalIncomeRatioProduct, 1), this.amountFilter(marginalIncomeRatioTime, 1)) : "";
    }

    private calculateRemainingAmount() {
        var ra: number = 0;
        var raExVat: number = 0;

        var hasTransferedRows: boolean = false;
        var cents: number = 0;
        var centsExVat: number = 0;

        var excludedAttestStates: number[] = _.map(_.filter(this.availableAttestStates, a => a.closed), a => a.attestStateId);
        var hasProductRowsNotTransferred = _.filter(this.activeRows, (r) => (r.type == SoeInvoiceRowType.ProductRow || r.type == SoeInvoiceRowType.BaseProductRow) && !_.includes(excludedAttestStates, r.attestStateId) && r.attestStateId !== this.attestStateTransferredOrderToInvoiceId).length > 0;

        _.forEach(this.activeRows, row => {
            if (row.attestStateId && _.includes(excludedAttestStates, row.attestStateId) && (!row.productId || (row.productId !== this.fixedPriceProductId && row.productId !== this.fixedPriceKeepPricesProductId)))
                return; // Next row

            if (((row.type == SoeInvoiceRowType.ProductRow || (row.type == SoeInvoiceRowType.BaseProductRow && !row.isCentRoundingRow)) ||
                (row.isFreightAmountRow || row.isInterestRow || row.isInvoiceFeeRow || row.isReminderRow)) &&
                row.attestStateId !== this.attestStateTransferredOrderToInvoiceId &&
                row.state !== SoeEntityState.Deleted &&
                !row.isLiftProduct &&
                !row.isClearingProduct) {
                ra += row.sumAmountCurrency;
                if (this.vatType === TermGroup_InvoiceVatType.Merchandise && !this.priceListTypeInclusiveVat)
                    ra += row.vatAmountCurrency;

                raExVat += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;

            } else if (row.isCentRoundingRow) {
                cents = row.sumAmountCurrency;
                if (this.vatType === TermGroup_InvoiceVatType.Merchandise && !this.priceListTypeInclusiveVat)
                    cents += row.vatAmountCurrency;
                centsExVat = row.sumAmountCurrency;
            } else if ((row.type === SoeInvoiceRowType.ProductRow || row.type === SoeInvoiceRowType.BaseProductRow) &&
                ((row.isLiftProduct && hasProductRowsNotTransferred) || row.isClearingProduct) &&
                row.attestStateId === this.attestStateTransferredOrderToInvoiceId) {
                ra += row.sumAmountCurrency;
                if (this.vatType === TermGroup_InvoiceVatType.Merchandise && !this.priceListTypeInclusiveVat)
                    ra += row.vatAmountCurrency;
                raExVat += this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency;
                hasTransferedRows = true;
            } else if (row.attestStateId === this.attestStateTransferredOrderToInvoiceId) {
                hasTransferedRows = true;
            }
        });

        if (hasTransferedRows) {
            if ((ra - 0.5) < Math.floor(ra)) {
                ra = Math.floor(ra);
            } else {
                ra = Math.ceil(ra);
            }
        }

        this.remainingAmount = ra;
        this.remainingAmountExVat = raExVat;
    }

    private updateFreightAmount() {
        if (this.readOnly)
            return;

        if (!this.useFreightAmount || this.container == ProductRowsContainers.Offer) {
            this.freightAmount = 0;
            this.freightAmountCurrency = 0;
        }

        this.calculateAmounts();

        // Add/Update product row
        var row = _.find(this.activeRows, r => r.isFreightAmountRow);
        if (this.freightAmountCurrency === 0) {
            if (row)
                this.deleteRow(row);
            return;
        }

        var rowType: SoeInvoiceRowType = this.freightAmountCurrency !== 0 ? SoeInvoiceRowType.BaseProductRow : SoeInvoiceRowType.AccountingRow;

        if (!row) {
            // Add row
            row = this.addRow(rowType, false).row;
            row.quantity = 1;
            row.isFreightAmountRow = true;
            this.setProductValuesFromId(row, this.freightAmountProductId, true);
        } else if (row.type !== rowType) {
            // Change row type
            row.type = rowType;
            this.reNumberRows();
        }

        if (row.amountCurrency !== this.freightAmountCurrency) {
            row.amountCurrency = this.freightAmountCurrency;
            this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
            this.amountHelper.calculateRowSum(row);
        }

        //this.soeGridOptions.refreshRows(row);
    }

    private updateInvoiceFee() {
        if (this.readOnly)
            return;

        if (!this.useInvoiceFee || this.disableInvoiceFee || this.container == ProductRowsContainers.Offer) {
            this.invoiceFee = 0;
            this.invoiceFeeCurrency = 0;
        }

        this.calculateAmounts();

        // Add/Update product row
        var row = _.find(this.activeRows, r => r.isInvoiceFeeRow);

        if (this.invoiceFeeCurrency === 0) {
            if (row)
                this.deleteRow(row);
            return;
        }

        var rowType: SoeInvoiceRowType = this.invoiceFeeCurrency !== 0 ? SoeInvoiceRowType.BaseProductRow : SoeInvoiceRowType.AccountingRow;

        if (!row) {
            // Add row
            row = this.addRow(rowType, false).row;
            row.quantity = 1;
            row.isInvoiceFeeRow = true;
            this.setProductValuesFromId(row, this.invoiceFeeProductId, true);
        } else if (row.type !== rowType) {
            // Change row type
            row.type = rowType;
            this.reNumberRows();
        }

        if (row.amountCurrency !== this.invoiceFeeCurrency) {
            row.amountCurrency = this.invoiceFeeCurrency;
            this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
            this.amountHelper.calculateRowSum(row);
        }
    }

    private updateCentRounding(cent: number) {
        // Add/Update product row
        var row = _.find(this.activeRows, r => r.isCentRoundingRow);
        if (cent === 0) {
            if (row)
                this.deleteRow(row);
            return;
        }

        var rowType: SoeInvoiceRowType = cent !== 0 ? SoeInvoiceRowType.BaseProductRow : SoeInvoiceRowType.AccountingRow;

        if (!row) {
            // Add row
            row = this.addRow(rowType, false).row;
            row.quantity = 1;
            row.isCentRoundingRow = true;
            this.setProductValuesFromId(row, this.centRoundingProductId, true);
        } else if (row.type !== rowType) {
            // Change row type
            row.type = rowType;
            this.reNumberRows();
        }

        if (row.amountCurrency !== cent) {
            row.amountCurrency = cent;
            this.amountHelper.calculateRowCurrencyAmount(row, ProductRowsAmountField.Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
            this.amountHelper.calculateRowSum(row);
        }
    }

    private updateVatType() {
        this.vatChangedCounter = this.getOnlyProductRows().length;
        _.forEach(this.getOnlyProductRows(), (r) => {
            this.resetVatAccount(r);
        });
        this.calculateAmounts();
    }

    private productRowsChanged() {
        _.forEach(this.productRows, this.deleteUnsusedPropertiesByType.bind(this));

        if (this.debugMode)
            this.visibleRows = _.orderBy(this.productRows, 'rowNr');
        else
            this.visibleRows = _.orderBy(_.filter(this.productRows, r => r.state === SoeEntityState.Active &&
                (!this.hideTransferred || !this.isRowTransferred(r)) &&
                (r.type === SoeInvoiceRowType.ProductRow ||
                    r.type === SoeInvoiceRowType.TextRow ||
                    r.type === SoeInvoiceRowType.PageBreakRow ||
                    r.type === SoeInvoiceRowType.SubTotalRow)), 'rowNr');

        this.nbrOfVisibleRows = this.visibleRows.length;

        var activeProdRows = this.activeProductRows;

        this.nbrOfTransferredRows = _.filter(activeProdRows, r => this.isRowTransferred(r)).length;
        this.hasHouseholdTaxDeduction = _.filter(activeProdRows, r => r.isHouseholdRow).length > 0;

        this.nbrOfActiveRows = this.activeRows ? _.filter(this.activeRows, r => r.type === SoeInvoiceRowType.ProductRow ||
            r.type === SoeInvoiceRowType.TextRow ||
            r.type === SoeInvoiceRowType.PageBreakRow ||
            r.type === SoeInvoiceRowType.SubTotalRow).length : 0;
    }

    private deleteUnsusedPropertiesByType(row: ProductRowDTO) {
        const { type } = row;
        switch (type) {
            case SoeInvoiceRowType.TextRow:
                this.deleteRowData(row, "created", "createdBy", "modified", "modifiedBy", "customerInvoiceRowId", "type", "rowTypeIcon", "text", "tempRowId", "state", "rowNr", "attestStateId", "attestStateName", "attestStateColor", "parentRowId", "isHouseholdTextRow", "isModified", "date");
                break;
            case SoeInvoiceRowType.PageBreakRow:
                this.deleteRowData(row, "created", "createdBy", "modified", "modifiedBy", "customerInvoiceRowId", "type", "rowTypeIcon", "text", "tempRowId", "state", "rowNr", "attestStateId", "attestStateName", "attestStateColor", "isReadOnly", "isModified");
                break;
            case SoeInvoiceRowType.SubTotalRow:
                this.deleteRowData(row, "created", "createdBy", "modified", "modifiedBy", "customerInvoiceRowId", "type", "rowTypeIcon", "text", "tempRowId", "state", "rowNr", "attestStateId", "attestStateName", "attestStateColor", "isReadOnly", "sumAmount", "sumAmountCurrency", "supplementCharge", "isModified");
                break;
        }
    }

    private deleteRowData(row: ProductRowDTO, ...exceptProps: string[]) {
        for (const p in row) {
            if (row.hasOwnProperty(p) && typeof (row[p]) !== "function" && exceptProps.indexOf(p) < 0) {
                delete row[p];
            }
        }
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.gridHasSelectedRows = (this.soeGridOptions.getSelectedCount() > 0);

            if (this.originType === SoeOriginType.CustomerInvoice) {
                this.gridHasSelectedValidRows = _.filter(this.soeGridOptions.getSelectedRows(), (row) => (row.type === SoeInvoiceRowType.ProductRow || row.type === SoeInvoiceRowType.BaseProductRow)).length > 0;
            }
            else if (this.originType === SoeOriginType.Contract) {
                this.gridHasSelectedValidRows = _.filter(this.soeGridOptions.getSelectedRows(), (row) => (row.type === SoeInvoiceRowType.ProductRow || row.type === SoeInvoiceRowType.BaseProductRow) && !this.isLiftProduct(row.productId)).length > 0;
            }
            else {
                this.gridHasSelectedValidRows = _.filter(this.soeGridOptions.getSelectedRows(), (row) => (row.attestStateId === this.initialAttestState.attestStateId) && (row.type === SoeInvoiceRowType.ProductRow || row.type === SoeInvoiceRowType.BaseProductRow) && !this.isLiftProduct(row.productId)).length > 0;
            }
        });
    }

    private expandAllRows() {
        if (this.showAllRows) {
            this.soeGridOptions.setAutoHeight(true);
        }
        else {
            this.soeGridOptions.setAutoHeight(false);

            let rows = 0;
            if (this.visibleRows)
                rows = this.visibleRows.length + 1;
            else if (this.productRows)
                rows = this.visibleRows.length + 1;

            if (rows < 8)
                this.soeGridOptions.setMinRowsToShow(8);
            else if (rows > 30)
                this.soeGridOptions.setMinRowsToShow(30);
            else
                this.soeGridOptions.setMinRowsToShow(rows);

        }
    }

    public viewOrderInformation() {
        this.translationService.translate("billing.order.ordersummary").then((term) => {
            this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ShowCustomerInvoiceInfo/ShowCustomerInvoiceInfo.html"),
                controller: ShowCustomerInvoiceInfoController,
                controllerAs: "ctrl",
                size: 'lg',
                backdrop: 'static',
                resolve: {
                    customerInvoiceId: () => { return this.orderInfo.invoiceId },
                    projectId: () => { return this.orderInfo.projectId ? this.orderInfo.projectId : 0 },
                    title: () => { return term + " " + this.orderInfo.orderNr }
                }
            });
        });
    }

    private debug() {
        this.debugMode = true;
        this.gridDataLoaded(this.productRows);
        console.log(this.productRows);
    }
}
