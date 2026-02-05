import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICommonCustomerService } from "../CommonCustomerService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { SmallGenericType } from "../../Models/smallgenerictype";
import { IVoucherSeriesDTO, IPaymentMethodDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CustomerInvoiceDTO, InvoiceDTO } from "../../Models/InvoiceDTO";
import { PaymentRowDTO, PaymentRowSaveDTO } from "../../Models/PaymentRowDTO";
import { CustomerDTO } from "../../Models/CustomerDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { CustomerPaymentEditSaveFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { AccountingRowDTO } from "../../Models/AccountingRowDTO";
import { SelectUnpaidInvoiceController } from "./Dialogs/SelectInvoice/SelectUnpaidInvoiceController";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { TermGroup_BillingType, Feature, CompanySettingType, TermGroup, SoeInvoiceMatchingType, SoeOriginType, SoeOriginStatus, TermGroup_Languages, CustomerAccountType, AccountingRowType, SoeEntityState, TermGroup_CurrencyType, SoeInvoiceType, TermGroup_InvoiceVatType, SoePaymentStatus } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { StringUtility } from "../../../Util/StringUtility";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { VatCodeDTO } from "../../Models/VatCodeDTO";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";


export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Config
    private accountYearId: number = 0;
    private accountYearIsOpen: boolean = false;

    private billingTypes: SmallGenericType[];
    private voucherSeries: IVoucherSeriesDTO[];
    private customers: any[] = [];
    private paymentMethods: IPaymentMethodDTO[];
    private paymentMethodsAccounts: any[] = [];
    private paymentInfos: any[];
    private unpaidInvoices: SmallGenericType[] = [];
    private currencies: any[];
    private matchCodes: any[];

    private selectedPaymentInfo: any;
    private selectedVoucherSeries: any;
    private accountPeriod: any;

    private invoice: CustomerInvoiceDTO;
    private payment: PaymentRowDTO;
    private manualCustomerPaymentTransferToVoucher: boolean = false;
    private paymentAskPrintVoucher: boolean = false;

    private seqNr: number;

    get isCredit(): boolean {
        return this.payment.billingType === TermGroup_BillingType.Credit;
    }

    private _selectedCustomer;
    get selectedCustomer() {
        return this._selectedCustomer;
    }
    set selectedCustomer(item: any) {
        this._selectedCustomer = item;
        this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null);
    }

    private _selectedInvoice;
    get selectedInvoice(): ISmallGenericType {
        return this._selectedInvoice;
    }
    set selectedInvoice(item: ISmallGenericType) {
        this._selectedInvoice  =  item;
        if (this._selectedInvoice) {
            const invoiceChanged = (!this.invoice || (this.invoice.invoiceId !== this._selectedInvoice.id) );
            this.invoiceId = this._selectedInvoice.id;
            if (this.payment)
                this.payment.invoiceId = this._selectedInvoice.id;
            if (this._selectedInvoice.id > 0 && invoiceChanged)
                this.loadInvoice(this._selectedInvoice.id);
            else if (invoiceChanged)
                this.invoice = new CustomerInvoiceDTO();
        }
        else {
            this.invoiceId = undefined;
        }
    }

    private _selectedPaymentMethod;
    get selectedPaymentMethod(): IPaymentMethodDTO {
        return this._selectedPaymentMethod;
    }
    set selectedPaymentMethod(item: IPaymentMethodDTO) {
        this._selectedPaymentMethod = item;
        if (this._selectedPaymentMethod && !this.loadingPayment) {
            this.loadPaymentMethod();
        }
    }

    private _selectedPayDate: Date;
    get selectedPayDate() {
        return this._selectedPayDate;
    }
    set selectedPayDate(date: Date) {
        this._selectedPayDate = CalendarUtility.convertToDate(date);
        if (this._selectedPayDate) {
            this.selectedVoucherDate = this._selectedPayDate;
            this.loadAccountPeriod();
        }

        if (this.payment)
            this.payment.payDate = this._selectedPayDate;

    }

    private _selectedVoucherDate: Date;
    get selectedVoucherDate() {
        return this._selectedVoucherDate;
    }
    set selectedVoucherDate(date: Date) {
        this._selectedVoucherDate = CalendarUtility.convertToDate(date);

        if (this.payment)
            this.payment.voucherDate = this._selectedVoucherDate;
    }

    private _selectedInvoiceDate: Date;
    get selectedInvoiceDate() {
        return this._selectedInvoiceDate;
    }
    set selectedInvoiceDate(date: Date) {
        this._selectedInvoiceDate = CalendarUtility.convertToDate(date);
    }

    private _selectedDueDate: Date;
    get selectedDueDate() {
        return this._selectedDueDate;
    }
    set selectedDueDate(date: Date) {
        this._selectedDueDate = CalendarUtility.convertToDate(date);
    }

    private _selectedMatchCode;
    get selectedMatchCode() {
        return this._selectedMatchCode;
    }
    set selectedMatchCode(item: any) {
        this._selectedMatchCode = item;
    }

    private _useVat: boolean;
    get useVat() {
        return this._useVat;
    }
    set useVat(item: any) {
        this._useVat = item;
        if (this.enableMatchCode && this.useVatVisibility) {
            this.$timeout(() => {
                this.generateAccountingRows();
            });
        }
    }

    get fullyPaidEnabled(): boolean {
        return !this.locked || this.isSupportSuperAdminUnlocked ? this.useMatching : false;
        /*if (this.negativeBalancePermission)
            return !this.locked || this.isSupportSuperAdminUnlocked ? this.payment.amount >= (this.invoice.totalAmount - this.invoice.paidAmount) : false;
        else 
            return !this.locked || this.isSupportSuperAdminUnlocked ? true : false;*/
    }

    private customer: CustomerDTO;

    private defaultVatRate: number = 0;
    private vatRate: number = Constants.DEFAULT_VAT_RATE;
    private vatCode: VatCodeDTO;
    private defaultVatCode: VatCodeDTO;

    private defaultPaymentMethodId: number;
    private defaultVatType: number;
    private defaultVoucherSeriesTypeId: number;
    private defaultVoucherSeriesId: number;
    private showTransactionCurrency: boolean = false;
    private showEnterpriseCurrency: boolean = false;
    private showLedgerCurrency: boolean = false;
    private voucherListReportId: number;
    private defaultVatCodeId: number;

    private locked: boolean = false;
    private isSupportUnlocked: boolean = false;
    private isSupportSuperAdminUnlocked: boolean = false;
    private isVoucherCreated: boolean = false;
    private isCancelled: boolean = false;
    private loadingPayment = false;
    private specificInvoiceOrPayment: boolean = false;

    //permissions
    private useMatching: boolean;
    private useCurrency: boolean;
    private extendedPaymentEditPermission: boolean = false;
    private negativeBalancePermission: boolean = false;

    private useVatVisibility: boolean;
    private enableMatchCode: boolean;
    // Company accounts
    private defaultCreditAccountId: number = 0;
    private defaultDebitAccountId: number = 0;
    private defaultVatAccountId: number = 0;
    //private defaultInterimAccountId: number = 0;

    private customerUnderpayAccountId: number = 0;
    private customerOverpayAccountId: number = 0;
    private currencyProfitAccountId: number = 0;
    private currencyLossAccountId: number = 0;
    private diffAccountId: number = 0;
    private bankFeeAccountId: number = 0;

    //Amounts
    preAmount: number;
    preAmountCurrency: number;

    // Flags
    private accRowsSetupDone = false;
    private delayAccRowsEvent = false;
    private currencyHasChanged = false;

    // Currency
    private currencyRate: number = 1;
    private currencyDate: Date;
    private currencyCode: string;
    private baseCurrencyCode: string;
    private ledgerCurrencyCode: string;
    private isLedgerCurrency: boolean;

    private _isBaseCurrency: boolean = true;
    get isBaseCurrency() {
        return this._isBaseCurrency;
    }
    set isBaseCurrency(item: any) {
        const changed = (item !== this._isBaseCurrency);
        this._isBaseCurrency = item;
        if (changed && (this.isNew || this.currencyHasChanged)) {
            this.generateAccountingRows();
        }
    }

    // Original amounts
    originalPaidAmount: number = 0;
    originaRemainingAmount: number = 0;

    // Functions
    saveFunctions: any = [];

    private edit: ng.IFormController;
    private modalInstance: any;
    
    public invoiceId: number;
    public paymentId: number;

    //@ngInject
    constructor(
        $uibModal,
        private $window,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private readonly requestReportService: IRequestReportService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
                .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
                .onLoadData(() => this.load()) 
                .onDoLookUp(() => this.doLookups()) 
                //.onSetUpGUI(() => this.setupGUI())
                .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        // Config parameters
        this.accountYearId = soeConfig.accountYearId;
        this.accountYearIsOpen = soeConfig.accountYearIsOpen;
        this.currencyDate = CalendarUtility.getDateToday();
    }

    public onInit(parameters: any) {
        this.paymentId = 0;
        this.invoiceId = 0;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        if (parameters.invoiceId != null) {
            this.invoiceId = parameters.invoiceId;
            this.specificInvoiceOrPayment = true;
        }

        if (parameters.paymentId != null) {
            this.paymentId = parameters.paymentId;
            this.specificInvoiceOrPayment = true;
        }

        this.flowHandler.start([
            { feature: Feature.Economy_Customer_Payment_Payments_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);

        this.messagingService.subscribe(Constants.EVENT_ACCOUNTING_ROWS_READY, (guid) => {
            if (this.guid === guid) {
                this.accRowsSetupDone = true;
                if (this.delayAccRowsEvent) {
                    this.delayAccRowsEvent = false;
                    this.refreshAccountRows();
                }
            }
        }, this.$scope);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Customer_Payment_Payments_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Customer_Payment_Payments_Edit].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadCurrencies(),
            this.loadCompanyAccounts(),
            this.loadCustomers(),
            this.loadBillingTypes(),
            this.loadMatchCodes(),
            this.loadUnpaidInvoices(0),
        ]).then(() => this.$q.all([
            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId),
            this.loadVoucherSeries(soeConfig.accountYearId),
            this.loadPaymentMethods()
        ]));
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        /*
        if (this.setupDefaultToolBar()) {
            this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
            // Unlock
            if (CoreUtility.isSupportAdmin) {
                this.toolbar.this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.payment.unlock", "economy.supplier.payment.unlock", IconLibrary.FontAwesome, "fa-unlock-alt", () => {
                    this.unlock();
                },
                    () => {
                        return !this.locked || this.isSupportUnlocked || this.isSupportSuperAdminUnlocked;
                    })));
            }
        }
        */
        // Functions
        const keys: string[] = [
            "core.save",
            "core.saveandclose"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions.push({ id: CustomerPaymentEditSaveFunctions.Save, name: terms["core.save"] });
            this.saveFunctions.push({ id: CustomerPaymentEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] });
        });
    }

    private setupWatchers() {
        // Convert currency amounts
        this.$scope.$watch(() => this.payment.amount, () => {
            this.convertAmount('amount', this.payment.amount);
        });
        this.$scope.$watch(() => this.payment.vatAmount, () => {
            this.convertAmount('vatAmount', this.payment.vatAmount);
        });
        this.$scope.$watch(() => this.payment.currencyId, () => {
            this.convertAmount('amount', this.payment.amount);
            this.convertAmount('vatAmount', this.payment.vatAmount);
            this.currencyHasChanged = true;
        });
        this.$scope.$watch(() => this.ledgerCurrencyCode, () => {
            this.convertAmount('amount', this.payment.amount);
            this.convertAmount('vatAmount', this.payment.vatAmount);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features = [
            Feature.Economy_Customer_Invoice_Matching,
            Feature.Economy_Preferences_Currency,
            Feature.Economy_Customer_Invoice_Payment_Extended_Rights,
            Feature.Economy_Customer_Payment_Payments_Edit_AllowNegativeBalance
        ];
        return this.coreService.hasModifyPermissions(features).then(x => {
            this.useMatching = x[Feature.Economy_Customer_Invoice_Matching];
            this.useCurrency = x[Feature.Economy_Preferences_Currency];
            this.extendedPaymentEditPermission = x[Feature.Economy_Customer_Invoice_Payment_Extended_Rights];
            this.negativeBalancePermission = x[Feature.Economy_Customer_Payment_Payments_Edit_AllowNegativeBalance];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentMethod);
        settingTypes.push(CompanySettingType.CustomerInvoiceDefaultVatType);
        settingTypes.push(CompanySettingType.CustomerPaymentVoucherSeriesType);
        settingTypes.push(CompanySettingType.CustomerShowTransactionCurrency);
        settingTypes.push(CompanySettingType.CustomerShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.CustomerShowLedgerCurrency);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.AccountingDefaultVatCode);

        return this.coreService.getCompanySettings(settingTypes).then(settings => {
            this.defaultPaymentMethodId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.CustomerPaymentDefaultPaymentMethod);
            this.defaultVatType = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.CustomerInvoiceDefaultVatType);
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.CustomerPaymentVoucherSeriesType);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.CustomerShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.CustomerShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.CustomerShowLedgerCurrency);
            this.voucherListReportId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.AccountingDefaultVoucherList);
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.AccountingDefaultVatCode);
        });
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountCustomerClaim);
        settingTypes.push(CompanySettingType.AccountCommonCheck);
        settingTypes.push(CompanySettingType.AccountCommonVatPayable1);
        settingTypes.push(CompanySettingType.AccountCustomerUnderpay);
        settingTypes.push(CompanySettingType.AccountCustomerOverpay);
        settingTypes.push(CompanySettingType.AccountCommonCurrencyProfit);
        settingTypes.push(CompanySettingType.AccountCommonCurrencyLoss);
        settingTypes.push(CompanySettingType.AccountCommonDiff);
        settingTypes.push(CompanySettingType.AccountCommonBankFee);
        settingTypes.push(CompanySettingType.CustomerPaymentManualTransferToVoucher); 
        settingTypes.push(CompanySettingType.CustomerPaymentAskPrintVoucherOnTransfer); 

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCheck);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1);
            this.customerUnderpayAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerUnderpay);
            this.customerOverpayAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerOverpay);
            this.currencyProfitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCurrencyProfit);
            this.currencyLossAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCurrencyLoss);
            this.diffAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonDiff);
            this.bankFeeAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonBankFee);
            this.manualCustomerPaymentTransferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentManualTransferToVoucher);
            this.paymentAskPrintVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentAskPrintVoucherOnTransfer);

            if (this.customerUnderpayAccountId === 0)
                this.customerUnderpayAccountId = this.diffAccountId;
            if (this.customerOverpayAccountId === 0)
                this.customerOverpayAccountId = this.diffAccountId;
        });
    }

    private loadCustomers(): ng.IPromise<any> {
        this.customers = [];

        if (this.specificInvoiceOrPayment) {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }
        else {
            return this.commonCustomerService.getCustomersSmall(true).then((x) => {
                this.customers.push({ id: 0, name: " " });
                _.forEach(x, (customer) => {
                    this.customers.push({ id: customer.actorCustomerId, name: customer.customerNr + " " + customer.customerName, number: customer.customerNr });
                });
            });
        }
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, true).then(x => {
            this.billingTypes = _.sortBy(x, 'id');
        });
    }

    private loadMatchCodes(): ng.IPromise<any> {
        return this.commonCustomerService.getMatchCodes(SoeInvoiceMatchingType.CustomerInvoiceMatching, true).then(x => {
            this.matchCodes = x;
        });
    }

    private loadDefaultVoucherSeriesId() {
        return this.accountingService.getDefaultVoucherSeriesId(this.accountYearId, CompanySettingType.CustomerPaymentVoucherSeriesType).then((x) => {
            this.defaultVoucherSeriesId = x;
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesByYear(accountYearId, false, true).then((x: IVoucherSeriesDTO[]) => {
            this.voucherSeries = x;
            this.selectedVoucherSeries = _.find(this.voucherSeries, { voucherSeriesId: this.defaultVoucherSeriesTypeId });
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentMethods(SoeOriginType.CustomerPayment, false, false, true, false).then((x: IPaymentMethodDTO[]) => {
            this.paymentMethods = x;
            // Set default
            this.selectedPaymentMethod = _.find(this.paymentMethods, { paymentMethodId: this.defaultPaymentMethodId });
        });
    }

    private loadPaymentMethod() {
        if (!this.paymentMethodsAccounts)
            this.paymentMethodsAccounts = [];

        if (!this.selectedPaymentMethod) {
            return;
        }

        if (this.paymentMethodsAccounts.length > 0) {
            if (_.find(this.paymentMethodsAccounts, { id: this.selectedPaymentMethod.paymentMethodId })) {
                if (!this.locked)
                    this.generateAccountingRows();
                else
                    this.updatePaymentMethodAccountingRow();

                return;
            }
        }

        this.commonCustomerService.getPaymentMethod(this.selectedPaymentMethod.paymentMethodId, true, false).then(method => {
            this.paymentMethodsAccounts.push({
                id: method.paymentMethodId,
                accountId: method.accountId
            });
            if (!this.locked)
                this.generateAccountingRows();
            else
                this.updatePaymentMethodAccountingRow();
        });
    }

    private load(): ng.IPromise<any> {
        if (this.invoiceId) {
            this.locked = true;
            return this.commonCustomerService.getInvoiceForPayment(this.invoiceId).then(invoice => {
                    this.createPayment(invoice);
                    return invoice;
                }).then(invoice => {
                    this.invoice = invoice;

                    if (this.unpaidInvoices.length === 0) {
                        this.unpaidInvoices.push({ id: this.invoice.invoiceId, name: this.invoice.invoiceNr });
                    }

                    return this.loadCustomer(invoice.actorId, invoice.invoiceId);
                }).then(() => {
                    this.selectedInvoice = _.find(this.unpaidInvoices, { id: this.invoiceId });
                    // Set customer
                    const customer = _.find(this.customers, c => c.id === this.invoice.actorId);
                    if (customer) {
                        this._selectedCustomer = undefined;
                        this.$timeout(() => {
                            if (!StringUtility.isEmpty(this.invoice.customerNameFromInvoice))
                                customer.name = customer.number + " " + this.invoice.customerNameFromInvoice;
                            this._selectedCustomer = customer;
                        });
                    }
                    this.generateAccountingRows();
                    this.setLocked();
                });
        }
        else if (this.paymentId !== 0) {
            this.locked = true;
            this.loadingPayment = true;
            return this.commonCustomerService.getPayment(this.paymentId, true, true, false).then(payment => {
                    this.isNew = false;
                    this.payment = payment;
                    this.payment.currencyDate = CalendarUtility.convertToDate(this.payment.currencyDate);

                    var obj = new PaymentRowDTO();
                    this.payment = angular.extend(obj, payment);

                    this.isVoucherCreated = this.payment.status === SoeOriginStatus.Payment;
                    this.isCancelled = this.payment.status === SoeOriginStatus.Cancel;
                    this.setLocked();

                    this.payment.paymentAccountRows = this.payment.paymentAccountRows.map(ca => {
                        var obj = new AccountingRowDTO();
                        angular.extend(obj, ca);
                        return obj;
                    });

                    this.invoiceId = payment.invoiceId;
                    
                    return this.commonCustomerService.getInvoiceForPayment(payment.invoiceId);
                }).then(invoice => {
                    this.invoice = invoice;

                    this.updatePayment(invoice);
                }).then(() => {
                    // Set customer

                    if (this.customers.length === 0) {
                        this.loadCustomer(this.invoice.actorId).then(x => {
                            this.setSelectedCustomer(this.invoice.actorId);
                        })
                    }
                    else {
                        this.setSelectedCustomer(this.invoice.actorId);
                    }

                    this.paymentInfos = [{ paymentNr: this.payment.paymentNr }];
                    this.selectedPaymentInfo = this.paymentInfos[0];
                    this.selectedPayDate = new Date(<any>this.payment.payDate);
                    this.selectedVoucherDate = new Date(<any>this.payment.voucherDate);
                    this.selectedVoucherSeries = _.find(this.voucherSeries, { voucherSeriesTypeId: this.payment.voucherSeriesTypeId });
                    this.selectedPaymentMethod = _.find(this.paymentMethods, { paymentMethodId: this.payment.paymentMethodId });
                    
                    this.loadingPayment = false;
                });
        } else {
            const deferral = this.$q.defer();
            this.new();
            deferral.resolve();
            return deferral.promise;
        }
    }

    private setSelectedCustomer(actorId: number) {
        const customer = _.find(this.customers, c => c.id === actorId);
        if (customer) {
            this._selectedCustomer = undefined;
            this.$timeout(() => {
                if (!StringUtility.isEmpty(this.invoice.customerNameFromInvoice))
                    customer.name = customer.number + " " + this.invoice.customerNameFromInvoice;
                this._selectedCustomer = customer;
            });
        }

        this.unpaidInvoices = [{ id: 0, name: this.invoice.invoiceNr + (customer ? customer.name : "") }];
        this._selectedInvoice = this.unpaidInvoices[0];
    }

    private loadVatRate(accountId: number) {
        if (accountId === 0) {
            this.setDefaultVatRate();
            return;
        }

        this.accountingService.getAccountSysVatRate(accountId).then(x => {
            this.defaultVatRate = x;
            this.setDefaultVatRate();
        });
    }

    private loadVatCode(): ng.IPromise<any> {
        this.vatCode = null;
        return this.accountingService.getVatCode(this.invoice && this.invoice.vatCodeId ? this.invoice.vatCodeId : 0).then(x => {
            this.vatCode = x;
        });
    }

    private setVatRate() {
        if (this.vatCode)
            this.vatRate = this.vatCode.percent;
        else
            this.setDefaultVatRate();
    }

    private setDefaultVatRate() {
        if (this.defaultVatRate === 0) {
            if (this.defaultVatCodeId) {
                this.accountingService.getVatCode(this.defaultVatCodeId).then(x => {
                    this.defaultVatCode = x;

                    if (this.defaultVatCode && this.defaultVatCode.percent)
                        this.defaultVatRate = this.defaultVatCode.percent;
                    else
                        this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;

                    this.vatRate = this.defaultVatRate;
                });
            }
            else {
                this.vatRate = this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;
            }
        }
        else {
            this.vatRate = this.defaultVatRate;
        }
    }

    public loadCustomer(customerId: number, invoiceId: number = null): ng.IPromise<any> {
        if (!customerId)
            return null;

        return this.commonCustomerService.getCustomer(customerId, true, true, true, true, true, true).then(x => {
            this.customer = x;
            const customerExist = _.find(this.customers, { id: this.customer.actorCustomerId });
            if (!customerExist)
            {
                this.customers.push({ id: this.customer.actorCustomerId, name: this.customer.name });
            }
            return this.$q.all([                
                this.commonCustomerService.getPaymentInformationViews(customerId).then(paymentInfos => {
                    this.paymentInfos = paymentInfos;
                    this.selectedPaymentInfo = _.find(this.paymentInfos, s => s.default);
                })
            ]);
        });
    }

    private loadUnpaidInvoices(customerId: number): ng.IPromise<any> {
        if (this.specificInvoiceOrPayment) {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }
        else {
            return this.commonCustomerService.getUnpaidInvoices(customerId).then(invoices => {
                this.unpaidInvoices = invoices;
            });
        }
    }

    private loadInvoice(id: number): ng.IPromise<any> {
        if (id) {
            return this.commonCustomerService.getInvoiceForPayment(id).then(invoice => {
                if (invoice) {
                    this.invoice = invoice;
                    const customer = _.find(this.customers, c => c.id === this.invoice.actorId);
                    if (customer) {
                        this.selectedCustomer = undefined;
                        this.$timeout(() => {
                            if (!StringUtility.isEmpty(this.invoice.customerNameFromInvoice))
                                customer.name = customer.number + " " + this.invoice.customerNameFromInvoice;
                            this.selectedCustomer = customer;
                        });
                    }

                    this.loadVatCode().then(() => {
                        this.createPayment(invoice);
                        this.setupWatchers();
                    });
                }
            });
        }
        this.invoice = null;
        this.vatCode = null;
        this.createPayment(null);
        return null;
    }

    private loadAccountPeriod() {
        if (!this.selectedPayDate || this.accountYearId === 0) {
            this.accountPeriod = null;
            return;
        }

        this.accountingService.getAccountPeriod(this.accountYearId, new Date(this.selectedPayDate.toString()), false).then((x) => {
            this.accountPeriod = x;
        });
    }

    private new() {
        this.isNew = true;
        this.createPayment(null);
        this.payment.paymentAccountRows = [];
        this.selectedCustomer = null;
        this.selectedInvoice = null;        
        this.selectedDueDate = null;
        this.selectedInvoiceDate = null;        
        this.invoice = new CustomerInvoiceDTO();
        this.invoice.paidAmount = 0;
        this.invoice.totalAmount = 0;
        this.invoice.vatAmount = 0;
        this.invoice.remainingAmount = 0;
        this.invoice.currencyId = this.currencies[0].currencyId;
        this.payment.originStatus = SoeOriginStatus.None;
    }

    private updatePayment(invoice: any, calculateRemaining: boolean = false) {
        this.payment.paidAmount = invoice.paidAmount || 0;
        this.payment.paidAmountCurrency = this.isBaseCurrency ? invoice.paidAmount || 0 : invoice.paidAmountCurrency || 0;
        this.payment.totalAmount = invoice.totalAmount || 0;
        this.payment.totalAmountCurrency = this.isBaseCurrency ? invoice.totalAmount || 0 : invoice.totalAmountCurrency || 0;
        this.payment.vatAmount = invoice.vatAmount || 0;
        this.payment.remainingAmount = (invoice.totalAmountCurrency - invoice.paidAmountCurrency) || 0;
        this.payment.currencyId = invoice.currencyId;
        this.seqNr = this.payment.seqNr;
        this.payment.fullyPaid = true;
        this.payment.billingType = this.invoice.billingType;
        this.selectedInvoiceDate = new Date(invoice.invoiceDate);
        this.selectedDueDate = new Date(invoice.dueDate);

        // Set original
        this.originalPaidAmount = this.payment.amount;
        this.originaRemainingAmount = this.payment.remainingAmount;

        if (this.invoice) {
            this.currencyRate = this.invoice.currencyRate;
            this.currencyDate = this.invoice.currencyDate;
        }
    }

    private createPayment(invoice: InvoiceDTO) {
        this.payment = new PaymentRowDTO();
        this.paymentId = 0;       
        this.seqNr = 0;
        this.payment.vatCodeId = 0;
        this.payment.voucherSeriesId = this.defaultVoucherSeriesTypeId;
        this.selectedVoucherSeries = _.find(this.voucherSeries, s => s.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId);
        this.payment.billingType = this.billingTypes[0].id;
        this.payment.paidAmount = 0;
        this.payment.totalAmount = 0;
        this.payment.vatAmount = 0;
        this.payment.vatAmountCurrency = 0;
        this.payment.remainingAmount = 0;
        this.payment.amount = 0;
        this.payment.amountCurrency = 0;
        this.payment.bankFeeCurrency = 0;
        this.payment.originStatus = SoeOriginStatus.None;

        this.selectedPayDate = CalendarUtility.getDateToday();
        this.selectedVoucherDate = CalendarUtility.getDateToday();

        this.preAmount = 0;
        this.preAmountCurrency = 0;

        if (invoice) {
            if (invoice.vatCodeId != null)
                this.payment.vatCodeId = invoice.vatCodeId;

            this.payment.actorId = invoice.actorId;
            this.payment.paidAmount = invoice.paidAmount || 0;
            this.payment.paidAmountCurrency = this.isBaseCurrency ? invoice.paidAmount || 0 : invoice.paidAmountCurrency || 0;
            this.payment.totalAmount = invoice.totalAmount || 0;
            this.payment.totalAmountCurrency = this.isBaseCurrency ? invoice.totalAmount || 0 : invoice.totalAmountCurrency || 0;
            this.payment.vatAmount = invoice.vatAmount || 0;
            this.payment.remainingAmount = (invoice.totalAmountCurrency - invoice.paidAmountCurrency) || 0;
            this.payment.currencyId = invoice.currencyId;
            this.selectedInvoiceDate = new Date(invoice.invoiceDate);
            this.selectedDueDate = new Date(invoice.dueDate);

            this.payment.amountCurrency = (invoice.totalAmountCurrency - invoice.paidAmountCurrency).round(2) ?? 0;
            this.payment.amount = (this.payment.amountCurrency * invoice.currencyRate).round(2) ?? 0;

            this.payment.fullyPaid = true;
            this.payment.vatType = invoice.vatType;
            this.payment.billingType = invoice.billingType;

            this.currencyRate = this.payment.currencyRate = invoice.currencyRate;
            this.currencyDate = this.payment.currencyDate = new Date(invoice.voucherDate);

            this.preAmount = this.payment.amount;
            this.preAmountCurrency = this.payment.amountCurrency;
        }
        else {
            this.payment.currencyId = this.currencies[0].currencyId;
            this.currencyRate = this.currencyRate;
            this.currencyDate = this.payment.currencyDate = CalendarUtility.getDateToday();
        }

        this.totalAmountChanged();
        this.loadPaymentMethod();
        
        this.dirtyHandler.setDirty();
    }

    private amountChanged(id: string) {
        this.$timeout(() => {
            var setFullyPaid: boolean = false;
            if (id === 'total') {
                this.invoice.totalAmount = this.invoice.totalAmountCurrency;
                this.updatePayment(this.invoice);
                this.calculateVatAmount();
            }
            if (id === 'amount') {
                if (this.invoice.totalAmount === 0) {
                    this.invoice.totalAmount = this.invoice.totalAmountCurrency = this.payment.amount;
                    this.calculateVatAmount();
                }

                setFullyPaid = true;
                if (this.isBaseCurrency && this.payment.amount !== this.preAmount) {
                    this.payment.amountCurrency = this.payment.amount;
                    this.preAmountCurrency = this.payment.amountCurrency;
                }
            }
            if (id === 'amountCurrency') {
                if (this.payment.amountCurrency !== this.preAmountCurrency) {
                    setFullyPaid = true;
                    this.payment.amount = this.payment.amountCurrency * this.currencyRate;
                    this.preAmount = this.payment.amount;
                    this.preAmountCurrency = this.payment.amountCurrency;
                }
            }

            this.generateAccountingRows(setFullyPaid);
        });
    }

    private generateAccountingRows(setFullyPaid: boolean = false) {
        if ((!this.payment))
            return;

        // Clear rows
        this.payment.paymentAccountRows = [];

        /*const bankFeeAmount: number = this.payment.bankFeeCurrency;

        var remainingAmount: number = this.payment.remainingAmount;
        var paidAmount: number = this.isBaseCurrency ? this.payment.amount : this.payment.amountCurrency;
        var fullyPaid: boolean = this.isCredit ? paidAmount <= remainingAmount : paidAmount >= remainingAmount;

        var amountDiff: number = this.isCredit ? remainingAmount - paidAmount : paidAmount - remainingAmount;
        var creditAmount: number = fullyPaid && !this.isCredit ? remainingAmount : paidAmount;
        var debitAmount: number = fullyPaid && this.isCredit ? remainingAmount : paidAmount;

        if ((fullyPaid) && (remainingAmount === 0)) {
            debitAmount = paidAmount;
        }*/

        var remainingAmount: number = this.payment.remainingAmount;
        var bankFeeAmount: number = this.payment.bankFeeCurrency / this.currencyRate;
        bankFeeAmount = bankFeeAmount.round(8);

        var paidAmount: number = this.isBaseCurrency ? this.payment.amount : this.payment.amountCurrency;
        var paidAmountFromBaseCurrency: number = this.payment.amount / this.currencyRate;
        paidAmountFromBaseCurrency = paidAmountFromBaseCurrency.round(8);

        var amountDiff: number = this.isCredit ? remainingAmount - paidAmount : paidAmount - remainingAmount;
        
        var fullyPaid = this.payment.fullyPaid || (this.isCredit ? paidAmount <= remainingAmount : paidAmount >= remainingAmount);
        var creditAmount: number = fullyPaid && remainingAmount;
        if ((fullyPaid) && (remainingAmount === 0)) {
            creditAmount = paidAmount;
        }

        // Fully Paid?
        if (this.payment && setFullyPaid) {
            if (amountDiff < 0) {
                this.payment.fullyPaid = fullyPaid = false;
                creditAmount = paidAmount;
            }
            else if (!this.payment.fullyPaid && amountDiff >= 0) {
                this.payment.fullyPaid = fullyPaid = true;
            }
            
            if (this.payment.fullyPaid && amountDiff == 0) { this.selectedMatchCode = null }
        }

        // Accounts receivables row
        const claimAccountId = this.invoice != null && this.invoice.claimAccountId != null && this.invoice.claimAccountId > 0 ? this.invoice.claimAccountId : this.getCustomerAccountId(CustomerAccountType.Debit, 1);
        this.createAccountingRow(CustomerAccountType.Debit, claimAccountId != null && claimAccountId != 0 ? claimAccountId : this.defaultCreditAccountId, creditAmount, false, false);

        // BankFee (if specified)
        if (bankFeeAmount > 0)
            this.createAccountingRow(CustomerAccountType.Unknown, this.bankFeeAccountId, bankFeeAmount, true, false);

        // VAT row
        if (vatAmount && vatAmount != 0)
            this.createAccountingRow(CustomerAccountType.VAT, 0, vatAmount, vatAmount > 0, true);

        //handling currency diff?
        let currencyDiff = paidAmountFromBaseCurrency - paidAmount;
        currencyDiff = currencyDiff.round(8);
        if ((!this.isBaseCurrency) && (currencyDiff != 0)) {
            const currencydiffAccountId = currencyDiff < 0 ? this.currencyLossAccountId : this.currencyProfitAccountId;
            const isDebet = this.isCredit ? currencyDiff > 0 : currencyDiff < 0;
            this.createAccountingRow(CustomerAccountType.Unknown, currencydiffAccountId, Math.abs(currencyDiff), isDebet, false);
            paidAmount = paidAmountFromBaseCurrency;
        }

        //use rest codes?
        if (this.useMatching) {
            if ((this.selectedMatchCode) && (amountDiff != 0)) {
                var matchAccountId = ((this.selectedMatchCode) && (this.selectedMatchCode.matchCodeId != 0)) ? this.selectedMatchCode.accountId : 0;
                if (matchAccountId != 0) {
                    if (this.useVat) {
                        const vatAmount = (amountDiff * this.vatRate / (100 + this.vatRate)).round(2);
                        amountDiff -= vatAmount;

                        this.createAccountingRow(CustomerAccountType.Unknown, matchAccountId, Math.abs(amountDiff), amountDiff < 0, false);

                        const matchVatAccountId = ((this.selectedMatchCode) && (this.selectedMatchCode.matchCodeId != 0)) ? this.selectedMatchCode.vatAccountId : 0;
                        if (matchVatAccountId > 0 && vatAmount !== 0)
                            this.createAccountingRow(CustomerAccountType.Unknown, matchVatAccountId, Math.abs(vatAmount), vatAmount < 0, false);

                    }
                    else {
                        this.createAccountingRow(CustomerAccountType.Unknown, matchAccountId, Math.abs(amountDiff), amountDiff < 0, false);
                    }
                }
            }

            this.enableMatchCode = (amountDiff != 0) && (fullyPaid); //&& (!this.isAfterPayment) ? true : false;
        }
        else if ((fullyPaid) && (amountDiff != 0)) {
            var vatAmount: number = 0;
            // over/under payment, if marked as fully paid, add diff row, otherwise this is just a partly payment
            if (this.useVat) {
                if (this.invoice.vatType === TermGroup_InvoiceVatType.Merchandise) {
                    vatAmount = (amountDiff * this.vatRate / (100 + this.vatRate)).round(2);
                    amountDiff -= vatAmount;
                }
            }

            var diffAccountId = amountDiff > 0 ? this.customerOverpayAccountId : this.customerUnderpayAccountId;
            this.createAccountingRow(CustomerAccountType.Unknown, diffAccountId, Math.abs(amountDiff), amountDiff > 0, false);
            // VAT row
            if (vatAmount != 0)
                this.createAccountingRow(CustomerAccountType.VAT, 0, vatAmount, vatAmount > 0, true);
        }

        var debitAmount: number = paidAmount;
        let accountId: number = 0;
        if (this.selectedPaymentMethod) {
            var acc = _.find(this.paymentMethodsAccounts, { id: this.selectedPaymentMethod.paymentMethodId });
            if (acc)
                accountId = acc.accountId;
        }
        this.createAccountingRow(CustomerAccountType.Credit, accountId, debitAmount, true, false);

        //Reset flags
        this.currencyHasChanged = false;

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnAllRows');
            this.$scope.$broadcast('rowsAdded');
        });
    }

    /*private generateAccountingRows() {
        if (!this.payment)
            return;

        if (this.selectedMatchCode && this.selectedMatchCode.matchCodeId > 0) {
            this.generateMatchingAccountingRows();
            return;
        }

        // Clear rows
        this.payment.paymentAccountRows = [];
        
        const bankFeeAmount: number = this.payment.bankFeeCurrency;

        var remainingAmount: number = this.payment.remainingAmount;
        var paidAmount: number = this.isBaseCurrency ? this.payment.amount : this.payment.amountCurrency;
        var fullyPaid: boolean = this.isCredit ? paidAmount <= remainingAmount : paidAmount >= remainingAmount;

        var amountDiff: number = this.isCredit ? remainingAmount - paidAmount : paidAmount - remainingAmount;
        var creditAmount: number = fullyPaid && !this.isCredit ? remainingAmount : paidAmount;
        var debitAmount: number = fullyPaid && this.isCredit ? remainingAmount : paidAmount;

        if ((fullyPaid) && (remainingAmount === 0)) {
            debitAmount = paidAmount;
        }
                
        var vatAmount: number = 0;

        var diffAccountId: number = 0;
        var setDiffRow: boolean = false;               

        //set amountDiff to 0 if prepayment
        if (!this.invoiceId)
            amountDiff = 0;

        if (this.useMatching) {
            if (amountDiff != 0) {
                this.enableMatchCode = true;

                // Fully Paid
                if (this.payment) {
                    if ((this.isCredit && amountDiff < 0) || (!this.isCredit && amountDiff < 0)) {
                        creditAmount = paidAmount;
                    }
                }

                if (amountDiff > 0 && !this.isCredit) {
                    // Overpaid
                    diffAccountId = 0;
                    setDiffRow = true;
                }
            } else {
                if (this.matchCodes.length > 0)
                    this.selectedMatchCode = this.matchCodes[0];

                this.enableMatchCode = false;
            }
        } else {
            if (amountDiff != 0) {
                // Fully Paid
                if (this.payment) {
                    if ((this.isCredit && amountDiff > 0) || (!this.isCredit && amountDiff < 0)) {
                        creditAmount = paidAmount;
                    }
                }

                // Overpaid
                if (amountDiff > 0 && !this.isCredit) {
                    diffAccountId = this.isBaseCurrency ? this.customerUnderpayAccountId : this.currencyProfitAccountId;
                    setDiffRow = true;
                } else {
                    // Underpaid
                    // If marked as fully paid, add diff row, otherwise this is just a partly payment
                    if (this.payment.fullyPaid) {
                        diffAccountId = this.isBaseCurrency ? this.customerOverpayAccountId : this.currencyLossAccountId;

                        setDiffRow = true;
                    }
                }
            }
        }

        // Accounts receivables row
        const claimAccountId = this.invoice != null && this.invoice.claimAccountId != null && this.invoice.claimAccountId > 0 ? this.invoice.claimAccountId : this.getCustomerAccountId(CustomerAccountType.Debit, 1);
        this.createAccountingRow(CustomerAccountType.Credit, claimAccountId != null && claimAccountId != 0 ? claimAccountId : this.defaultCreditAccountId, creditAmount, false, false);

        // BankFee (if specified)
        if (bankFeeAmount > 0)
            this.createAccountingRow(CustomerAccountType.Unknown, this.bankFeeAccountId, bankFeeAmount, true, false);

        // AmountDiff (if specified)
        if (setDiffRow) {
            var isDebitRow = true;
            if ((!this.isCredit && amountDiff > 0) || (this.isCredit && amountDiff > 0))
                isDebitRow = false;
            
            this.createAccountingRow(CustomerAccountType.Unknown, diffAccountId != 0 ? diffAccountId : this.diffAccountId, Math.abs(amountDiff), isDebitRow, false);
        }

        // VAT row
        if (vatAmount != 0)
            this.createAccountingRow(CustomerAccountType.VAT, 0, vatAmount, vatAmount > 0, true);

        // Debit row                        
        let accountId: number = 0;

        if (this.selectedPaymentMethod) {
            var acc = _.find(this.paymentMethodsAccounts, { id: this.selectedPaymentMethod.paymentMethodId });
            if (acc)
                accountId = acc.accountId;
        }
        this.createAccountingRow(CustomerAccountType.Debit, accountId, debitAmount, true, false);

        this.refreshAccountRows();
    }

    private generateMatchingAccountingRows() {
        if (!this.payment || !this.invoice)
            return;

        if (this.useMatching && this.selectedMatchCode && this.selectedMatchCode.matchCodeId > 0)
            this.payment.fullyPaid = true;
        else
            return;

        // Clear rows
        this.payment.paymentAccountRows = [];
        var remainingAmount: number = this.payment.remainingAmount;
        var bankFeeAmount: number = this.payment.bankFeeCurrency;
        var paidAmount: number = this.payment.amount - bankFeeAmount;
        var fullyPaid: boolean = this.payment.fullyPaid || (this.isCredit ? paidAmount >= remainingAmount : paidAmount <= remainingAmount);
        var creditAmount: number = fullyPaid ? remainingAmount : paidAmount;
        var debitAmount: number = paidAmount;
        var vatAmount: number = 0;
        var amountDiff: number = this.isCredit ? remainingAmount - paidAmount : paidAmount - remainingAmount;
        var matchCodeId: number = 0;

        // Accounts receivables row
        var claimAccountId = this.invoice != null && this.invoice.claimAccountId != null && this.invoice.claimAccountId > 0 ? this.invoice.claimAccountId : this.getCustomerAccountId(CustomerAccountType.Debit, 1);
        this.createAccountingRow(CustomerAccountType.Credit, claimAccountId != null && claimAccountId != 0 ? claimAccountId : this.defaultCreditAccountId, creditAmount, false, false);

        if (amountDiff != 0) {

            var isDebitRow = true;
            if ((!this.isCredit && amountDiff > 0) || (this.isCredit && amountDiff > 0))
                isDebitRow = false;
            
            this.payment.fullyPaid = fullyPaid = true;

            if (this.useVat) {
                var vatRateValue: number = this.vatRate / 100;
                creditAmount = amountDiff / (1 + vatRateValue);
                vatAmount = amountDiff - creditAmount;
                this.createAccountingRow(CustomerAccountType.Debit, this.selectedMatchCode.accountId, Math.abs(creditAmount), isDebitRow, false);
                this.createAccountingRow(CustomerAccountType.VAT, this.selectedMatchCode.vatAccountId, vatAmount, isDebitRow, true);
            } else {
                //Add match row                            
                this.createAccountingRow(CustomerAccountType.Debit, this.selectedMatchCode.accountId, Math.abs(amountDiff), isDebitRow, false);
            }

        }

        var accountId = 0;
        if (this.selectedPaymentMethod) {
            var acc = _.find(this.paymentMethodsAccounts, { id: this.selectedPaymentMethod.paymentMethodId });
            if (acc)
                accountId = acc.accountId;
        }
        this.createAccountingRow(CustomerAccountType.Debit, accountId, debitAmount, true, false);

        this.refreshAccountRows();
    }*/

    private updatePaymentMethodAccountingRow() {

        if (!this.payment?.paymentAccountRows)
            return;

        this.payment.paymentAccountRows.filter( row=> row.isDebitRow).forEach( row => {
            if (_.find(this.paymentMethods, { accountId: row.dim1Id })) {
                let index: number = this.payment.paymentAccountRows.indexOf(row);
                this.payment.paymentAccountRows[index].dim1Id = this.selectedPaymentMethod.accountId;
            }
        });

        this.refreshAccountRows()
    }

    private refreshAccountRows() {
        if (this.accRowsSetupDone) {
            this.$timeout(() => {
                this.$scope.$broadcast('rowsAdded', { setRowItemAccountsOnAllRows: true });
            });
        }
        else {
            this.delayAccRowsEvent = true;
        }
    }

    private createAccountingRow(type: CustomerAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean): AccountingRowDTO {
        // Credit invoice, negate isDebitRow
        if (this.isCredit)
            isDebitRow = !isDebitRow;

        amount = Math.abs(amount);

        const row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = 0;
        row.rowNr = AccountingRowDTO.getNextRowNr(this.payment.paymentAccountRows);
        row.debitAmountCurrency = isDebitRow ? amount : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : amount;
        
        row.quantity = null;
        row.date = new Date().date();
        row.isCreditRow = !isDebitRow;
        row.isDebitRow = isDebitRow;
        row.isVatRow = isVatRow;
        row.state = SoeEntityState.Active;
        row.invoiceId = this.invoice ? this.invoice.invoiceId : 0;
        row.isModified = false;

        // Set accounts
        if (type !== CustomerAccountType.Unknown) {
            row.dim1Id = this.getAccountId(type, 1);
            row.dim2Id = this.getAccountId(type, 2);
            row.dim3Id = this.getAccountId(type, 3);
            row.dim4Id = this.getAccountId(type, 4);
            row.dim5Id = this.getAccountId(type, 5);
            row.dim6Id = this.getAccountId(type, 6);
        }

        if (accountId !== 0)
            row.dim1Id = accountId;

        this.payment.paymentAccountRows.push(row);

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnRowIfMissing', row);
        });

        return row;
    }

    private getAccountId(type: CustomerAccountType, dimNr: number): number {
        // First try to get account from customer
        let accountId = this.getCustomerAccountId(type, dimNr);
        if (accountId === 0 && dimNr === 1) {
            // No account found on customer, use base account
            switch (type) {
                case CustomerAccountType.Credit:
                    accountId = this.defaultCreditAccountId;
                    break;
                case CustomerAccountType.Debit:
                    //if (this.invoice.interimInvoice) {
                    //    accountId = this.defaultInterimAccountId;
                    //    if (accountId === 0) {
                    //        accountId = this.defaultDebitAccountId;
                    //        var keys: string[] = [
                    //            "economy.supplier.invoice.interimaccountmissing.title",
                    //            "economy.supplier.invoice.interimaccountmissing.message"
                    //        ];
                    //        this.translationService.translateMany(keys).then((terms) => {
                    //            var modal = this.notificationService.showDialog(terms["economy.supplier.invoice.interimaccountmissing.title"], terms["economy.supplier.invoice.interimaccountmissing.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    //        });
                    //    }
                    //}
                    //else
                    accountId = this.defaultDebitAccountId;
                    break;
                case CustomerAccountType.VAT:
                    accountId = this.defaultVatAccountId;
                    break;
            }
        }

        return accountId;
    }

    private getCustomerAccountId(type: CustomerAccountType, dimNr: number): number {
        var accountId = 0;

        if (this.customer && this.customer.accountingSettings) {
            var setting = _.find(this.customer.accountingSettings, { type: type });
            if (setting) {
                switch (dimNr) {
                    case 1:
                        accountId = setting.account1Id ? setting.account1Id : 0;
                        break;
                    case 2:
                        accountId = setting.account2Id ? setting.account2Id : 0;
                        break;
                    case 3:
                        accountId = setting.account3Id ? setting.account3Id : 0;
                        break;
                    case 4:
                        accountId = setting.account4Id ? setting.account4Id : 0;
                        break;
                    case 5:
                        accountId = setting.account5Id ? setting.account5Id : 0;
                        break;
                    case 6:
                        accountId = setting.account6Id ? setting.account6Id : 0;
                        break;
                }
            }
        }

        return accountId;
    }

    private convertAmount(field: string, amount: number) {
        // Call amount currency converter in accounting rows directive
        const item = {
            field: field,
            amount: amount,
            sourceCurrencyType: TermGroup_CurrencyType.TransactionCurrency
        };
        this.$scope.$broadcast('amountChanged', item);
    }

    private amountConverted(item) {
        // Result from amount currency converter in accounting rows directive
        this.invoice[item.field] = item.baseCurrencyAmount;
        this.invoice[item.field + 'EnterpriceCurrency'] = item.enterpriseCurrencyAmount;
        this.invoice[item.field + 'LedgerCurrency'] = item.ledgerCurrencyAmount;
    }

    // EVENTS

    private billingTypeChanging() {
        this.$timeout(() => {
            this.generateAccountingRows();
        });
    }

    public fullyPaidChanged() {
        this.$timeout(() => {
            if (!this.payment.fullyPaid)
                this.selectedMatchCode = null;
            this.generateAccountingRows();
        });
    }

    public totalAmountChanged() {
        if (!this.invoiceId) {
            this.payment.remainingAmount = this.payment.amountCurrency = this.payment.totalAmountCurrency - this.payment.paidAmountCurrency;
            this.payment.amount = this.payment.totalAmount - this.payment.paidAmount;
            this.payment.fullyPaid = true;

            this.preAmount = this.payment.amount;
            this.preAmountCurrency = this.payment.amountCurrency;
        }
    }

    private matchCodeChanging() {
        this.$timeout(() => {
            if (this.selectedMatchCode === null || this.selectedMatchCode.matchCodeId === 0) {
                this.payment.fullyPaid = this.isCredit ? this.payment.remainingAmount >= 0 : this.payment.remainingAmount <= 0;
                this.useVatVisibility = false;
                this.useVat = false;
            }
            else {
                this.useVatVisibility = true;
                if (this.selectedMatchCode.vatAccountId && this.selectedMatchCode.vatAccountId != 0)
                    this.useVat = true;
                else
                    this.useVat = false;
            }

            this.generateAccountingRows();
        });
    }

    // ACTIONS

    private executeSaveFunction(option) {

        if (this.isVoucherCreated) {
            const keys: string[] = [
                "economy.supplier.payment.withoutinvoice",
                "economy.supplier.payment.successpreliminary",
                "economy.supplier.payment.successdefinitive",
                "economy.supplier.payment.successupdated",
                "economy.supplier.payment.askopenvoucher"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const message = terms["economy.supplier.payment.askopenvoucher"];
                const modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(() => {
                    switch (option.id) {
                        case CustomerPaymentEditSaveFunctions.Save:
                            this.initSave(false, true);
                            break;
                        case CustomerPaymentEditSaveFunctions.SaveAndClose:
                            this.initSave(true, true);
                            break;
                    }
                }, () => {
                    return;
                });
            });
        }
        else {
            switch (option.id) {
                case CustomerPaymentEditSaveFunctions.Save:
                    this.initSave(false, false);
                    break;
                case CustomerPaymentEditSaveFunctions.SaveAndClose:
                    this.initSave(true, false);
                    break;
            }
        }
    }

    private initSave(closeAfterSave: boolean, openVoucher: boolean) {
        if (this.invoice.totalAmountCurrency === this.payment.amount && !this.payment.fullyPaid) {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.payment.fullypaidwarning"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.payment.fullypaidwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.save(closeAfterSave, openVoucher);
                });
            });
        }
        else {
            this.save(closeAfterSave, openVoucher);
        }
    }

    private save(closeAfterSave: boolean, openVoucher: boolean) {

        this.progress.startSaveProgress((completion) => {

            const keys: string[] = [
                "economy.supplier.payment.withoutinvoice",
                "economy.supplier.payment.successpreliminary",
                "economy.supplier.payment.successdefinitive",
                "economy.supplier.payment.successupdated",
                "economy.supplier.payment.askopenvoucher",
                "economy.supplier.payment.openvouchermessage",
                "economy.accounting.voucher.voucher",
                "economy.supplier.payment.voucherscreated",
                "economy.supplier.payment.askPrintVoucher",
                "common.status"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var remainingAmount: number = this.payment.remainingAmount - (this.isBaseCurrency ? this.payment.amount : this.payment.amountCurrency);
                var paidAmount: number = this.isBaseCurrency ? this.payment.amount - this.payment.bankFeeCurrency : this.payment.amountCurrency - this.payment.bankFeeCurrency;
                var amountDiff: number = this.isCredit ? this.payment.remainingAmount - paidAmount : paidAmount - this.payment.remainingAmount;
                var ammountDiffCurrency: number = this.isBaseCurrency ? amountDiff : 0;
                amountDiff = amountDiff.round(2);

                if (this.invoiceId == 0)
                    amountDiff = 0;

                const saveDTO = new PaymentRowSaveDTO();
                // Origin
                saveDTO.originId = this.paymentId ? this.paymentId : 0;
                saveDTO.originType = SoeOriginType.CustomerPayment;
                saveDTO.originStatus = this.payment.originStatus && this.payment.originStatus !== SoeOriginStatus.None ? this.payment.originStatus : SoeOriginStatus.Origin;
                saveDTO.originDescription = this.payment.originDescription;
                saveDTO.voucherSeriesId = this.selectedVoucherSeries.voucherSeriesId;
                saveDTO.accountYearId = this.accountYearId;

                // Invoice
                saveDTO.invoiceId = this.invoiceId;
                saveDTO.invoiceType = SoeInvoiceType.CustomerInvoice;
                saveDTO.onlyPayment = true;
                saveDTO.billingType = this.payment.billingType;
                saveDTO.actorId = this.selectedCustomer.id;
                saveDTO.invoiceNr = terms["economy.supplier.payment.withoutinvoice"];
                saveDTO.invoiceDate = new Date();
                saveDTO.paymentDate = this.selectedPayDate;
                saveDTO.voucherDate = this.selectedVoucherDate ? this.selectedVoucherDate : (this.selectedPayDate ? this.selectedPayDate : null);
                saveDTO.totalAmount = this.payment.amount;
                saveDTO.totalAmountCurrency = this.payment.amountCurrency;
                saveDTO.vatAmount = this.payment.vatAmount;
                saveDTO.vatAmountCurrency = this.payment.vatAmountCurrency;

                saveDTO.amount = this.payment.amount;
                saveDTO.amountCurrency = this.payment.amountCurrency;
                saveDTO.amountDiff = amountDiff;
                saveDTO.amountDiffCurrency = ammountDiffCurrency;
                saveDTO.currencyId = this.payment.currencyId;
                saveDTO.currencyRate = this.currencyRate;
                saveDTO.currencyDate = this.invoiceId != 0 ? this.currencyDate : this.selectedPayDate;
                saveDTO.fullyPayed = (this.payment.fullyPaid || remainingAmount === 0);

                // PaymentRow
                saveDTO.paymentMethodId = this.selectedPaymentMethod.paymentMethodId;
                saveDTO.seqNr = this.seqNr ? this.seqNr : 0;
                saveDTO.paymentRowId = this.payment.paymentRowId;

                //TODO!!!
                ////console.log("this.selectedPaymentInfo: ", this.selectedPaymentInfo);
                //saveDTO.sysPaymentTypeId = this.selectedPaymentInfo.paymentInformationRowId;
                //saveDTO.paymentNr = this.selectedPaymentInfo.paymentNr;

                /*saveDTO.amount = this.payment.paidAmount;
                saveDTO.amountCurrency = this.payment.paidAmountCurrency;
                saveDTO.amountDiff = amountDiff;*/
                saveDTO.hasPendingAmountDiff = false;
                saveDTO.hasPendingBankFee = false;

                // Super Support
                saveDTO.isSuperSupportSave = this.isSupportSuperAdminUnlocked;

                let matchCodeId: number = 0;
                if (this.useMatching && this.selectedMatchCode)
                    matchCodeId = this.selectedMatchCode.matchCodeId;

                this.commonCustomerService.saveCustomerPayment(saveDTO, this.payment.paymentAccountRows, matchCodeId).then((result) => {
                    if (result.success) {
                        if (this.manualCustomerPaymentTransferToVoucher) {
                            this.commonCustomerService.CalculateAccountBalanceForAccountsFromVoucher(this.accountYearId).then((result) => {
                            });

                            if (this.paymentAskPrintVoucher && result.idDict) {
                                // Get keys
                                const voucherIds: number[] = []
                                _.forEach(Object.keys(result.idDict), (key) => {
                                    voucherIds.push(Number(key));
                                });

                                // Get values
                                var first: boolean = true;
                                var voucherNrs: string = "";
                                _.forEach(result.idDict, (pair) => {
                                    if (!first)
                                        voucherNrs = voucherNrs + ", ";
                                    else
                                        first = false;
                                    voucherNrs = voucherNrs + pair;
                                });

                                var mess = terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + terms["economy.supplier.payment.askPrintVoucher"];
                                var modal = this.notificationService.showDialog(terms["core.verifyquestion"], mess, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                                modal.result.then(val => {
                                    if (val != null && val === true) {
                                        this.printVouchers(voucherIds);
                                    };
                                });
                            }
                        }

                        if (this.isNew) {
                            // Set payment id to be able to reload it
                            if (result.integerValue && result.integerValue > 0) {
                                this.paymentId = result.integerValue;
                                this.payment.paymentId = result.integerValue;                                
                            }
                           
                            if (result.integerValue2 && result.integerValue2 > 0) {
                                if (this.isNew && this.specificInvoiceOrPayment) {
                                    this.payment.paymentRowId = result.integerValue2;
                                }
                            }
                            
                            // Set sequence number to update the tab header
                            if (result.value) {
                                this.seqNr = result.value;
                                this.payment.seqNr = this.seqNr;
                            }
                        }
                        this.payment.isSeqNrTabLabelDisplay = (this.isNew && !this.specificInvoiceOrPayment);
                        let message = '';
                        if (this.isNew) {
                            message = !this.seqNr ? terms["economy.supplier.payment.successpreliminary"] : terms["economy.supplier.payment.successdefinitive"].format(this.seqNr.toString());
                        }
                        else {
                            message = terms["economy.supplier.payment.successupdated"];
                        }

                        if (openVoucher)
                            message += terms["economy.supplier.payment.openvouchermessage"];
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.payment, true, message);

                        this.notificationService.showDialog(terms["common.status"], message, SOEMessageBoxImage.OK, SOEMessageBoxButtons.OK);
                  

                        if (openVoucher) {
                            this.accountingService.getVoucher(this.payment.voucherHeadId, false, false, false, false).then((voucher) => {
                                this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(terms["economy.accounting.voucher.voucher"] + " " + voucher.voucherNr, voucher.voucherHeadId, VouchersEditController, { id: voucher.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
                            });
                        }

                        if (closeAfterSave) {
                            this.dirtyHandler.clean();
                            this.closeMe(true);
                        }
                        else {
                           
                            if (this.isNew && !this.specificInvoiceOrPayment) {
                                this.new();
                                    this.translationService.translate("common.customer.payment.newpayment").then((term) => {
                                        this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                                            guid: this.guid,
                                            label: term,
                                            id: undefined,
                                        });
                                    });
                            }
                            if (this.isNew && this.specificInvoiceOrPayment) {
                                this.isNew = false;
                            }
                        }
                    } else {
                        completion.failed(result.errorMessage);
                    }
                });
            });

        }, this.guid).then(data => {

            this.dirtyHandler.clean();
        }, error => {

        });
    }

    protected initRevoke() {
        // Show verification dialog
        const keys: string[] = [
            "core.verifyquestion",
            "economy.supplier.payment.revokequestion",
            "economy.supplier.payment.revokeextendedquestion"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            let message = terms["economy.supplier.payment.revokequestion"];
            if (this.isVoucherCreated)
                message = terms["economy.supplier.payment.revokeextendedquestion"];

            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then((result) => {
                if (result) {
                    this.performDelete();
                }
                else
                    return;
            });
        });
    }

    private performDelete() {
        const keys: string[] = [
            "economy.accounting.voucher.voucher",
        ];

        this.progress.startDeleteProgress((completion) => {

            if (this.extendedPaymentEditPermission && this.isVoucherCreated) {
                this.commonCustomerService.cancelPaymentWithVoucher(this.payment.paymentRowId).then((result) => {
                    if (result.success) {
                        completion.completed(this.payment);

                        this.translationService.translateMany(keys).then((terms) => {
                            if (this.isVoucherCreated) {
                                this.accountingService.getVoucher(this.payment.voucherHeadId, false, false, false, false).then((voucher) => {
                                    this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(terms["economy.accounting.voucher.voucher"] + " " + voucher.voucherNr, voucher.voucherHeadId, VouchersEditController, { id: voucher.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
                                });
                            }
                        });

                        this.closeMe(true);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.errorMessage);
                });
            }
            else {
                this.commonCustomerService.cancelPayment(this.payment.paymentRowId).then((result) => {
                    if (result.success) {
                        completion.completed(this.payment);
                        this.closeMe(true);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.errorMessage);
                });
            }

        });
    }

    // HELP-METHODS

    private calculateVatAmount(forceContractor: boolean = false) {
        // Calculate VAT amount based on vat percent
        let vatAmount: number = 0;
        let vatRateValue: number = this.vatRate / 100;

        if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || forceContractor)
            vatAmount = this.invoice.totalAmountCurrency * vatRateValue;
        else
            vatAmount = this.invoice.totalAmountCurrency * (1 - (1 / (vatRateValue + 1)));

        this.invoice.vatAmountCurrency = vatAmount.round(2);
    }

    private hasModifiedRows(): boolean {
        let modified: boolean = false;
        if (this.payment) {
            _.forEach(this.payment.paymentAccountRows, (row) => {
                if (row.isModified || row.tempRowId != 0)
                    modified = true;
            });
        }
        return modified;
    }

    private setLocked() {
        const isDraft = this.payment.originStatus === SoeOriginStatus.Draft || this.payment.originStatus === SoeOriginStatus.None;
        const hasVoucher = this.payment.voucherHeadId !== null && this.payment.voucherHeadId > 0;
        this.locked = this.payment !== null && (!isDraft || hasVoucher || this.isCancelled);
    }

    private unlock() {

        if (CoreUtility.isSupportSuperAdmin) {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.invoice.supersupportunlockaccountingrows"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const message = terms["economy.supplier.invoice.supersupportunlockaccountingrows"];
                const modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.isSupportSuperAdminUnlocked = true;
                    this.extendedPaymentEditPermission = true;
                }, (reason) => {
                    this.isSupportSuperAdminUnlocked = false;
                });
            });
        }
        else {
            if (CoreUtility.isSupportAdmin) {
                this.isSupportUnlocked = true;
                this.extendedPaymentEditPermission = true;
            }
        }
    }

    private printVouchers(ids: number[]) {
        if (this.voucherListReportId) {

            this.requestReportService.printVoucherList(ids);
        }
        else {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.payment.defaultVoucherListMissing"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            });
        }
    }

    // VALIDATION

    public showValidationError() {

        const errors = this['edit'].$error;

        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {

            // Mandatory fields
            if (this.payment) {
                if (!this.selectedCustomer)
                    validationErrorKeys.push("common.customer.customer.customer");
                if (!this.selectedVoucherSeries)
                    validationErrorKeys.push("economy.supplier.invoice.voucherseries");
                if (!this.selectedPaymentMethod)
                    validationErrorKeys.push("economy.supplier.payment.paymentmethod");
                if (!this.payment.payDate)
                    validationErrorKeys.push("economy.supplier.payment.paymentdate");
                if (!this.payment.voucherDate)
                    validationErrorKeys.push("common.customer.payment.voucherdate");
                if (!this.payment.invoiceId)
                    validationErrorKeys.push("economy.supplier.invoice.invoicenr");
            }

            // Account year
            if (errors['accountYearStatus'])
                validationErrorKeys.push("economy.accounting.voucher.accountyearclosed");

            // Account period
            if (errors['accountPeriod'])
                validationErrorKeys.push("economy.accounting.voucher.accountperiodmissing");
            if (errors['accountPeriodStatus'])
                validationErrorKeys.push("economy.accounting.voucher.accountperiodclosed");

            // Voucher series
            if (errors['defaultVoucherSeries'])
                validationErrorKeys.push("economy.accounting.voucher.defaultvoucherseriesmissing");

            // Accounting row validation
            if (errors['accountStandard'])
                validationErrorKeys.push("economy.accounting.voucher.accountstandardmissing");
            if (errors['accountInternal'])
                validationErrorKeys.push("economy.accounting.voucher.accountinternalmissing");
            if (errors['rowAmount'])
                validationErrorKeys.push("economy.accounting.voucher.invalidrowamount");
            if (errors['amountDiff'])
                validationErrorKeys.push("economy.accounting.voucher.unbalancedrows");

            if (errors['rowAmounts'])
                validationErrorKeys.push("economy.supplier.payment.rowamountmismatch");
        });
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this.edit.$invalid;
    }

    // This should be done in a more logical and more userfriendly way, so dont implement now
    public selectInvoice() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Customer/Payments/Dialogs/SelectInvoice/SelectUnpaidInvoice.html"),
            controller: SelectUnpaidInvoiceController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                commonCustomerService: () => { return this.commonCustomerService },
                customerId: () => { return this.selectedCustomer ? this.selectedCustomer.id : 0 },
            }
        });
        modal.result.then(x => {
            if (x.rows && x.rows.length > 0) {
                this.invoiceId = x.rows[0].customerInvoiceId;
                this.load();                
            }
        });
    }
}