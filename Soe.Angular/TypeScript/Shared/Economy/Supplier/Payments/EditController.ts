import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { SmallGenericType } from "../../../../Common/Models/smallgenerictype";
import { IVoucherSeriesDTO, IPaymentMethodDTO, ISmallGenericType,IAccountDTO } from "../../../../Scripts/TypeLite.Net4";
import { SupplierInvoiceDTO } from "../../../../Common/Models/InvoiceDTO";
import { PaymentRowDTO, PaymentRowSaveDTO } from "../../../../Common/Models/PaymentRowDTO";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { IAccountingService } from "../../Accounting/AccountingService";
import { ISupplierService } from "../SupplierService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SupplierPaymentEditSaveFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { AccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { TermGroup_BillingType, TermGroup_Languages, Feature, CompanySettingType, TermGroup, SoeInvoiceMatchingType, SoeOriginType, SupplierAccountType, TermGroup_InvoiceVatType, AccountingRowType, SoeEntityState, TermGroup_CurrencyType, SoePaymentStatus, SoeOriginStatus, SoeInvoiceType, TermGroup_SysPaymentType, TermGroup_PaymentTransferStatus, TermGroup_AccountType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { VatCodeDTO } from "../../../../Common/Models/VatCodeDTO";
import { CurrencyHelper } from "../../../../Common/Directives/Helpers/CurrencyHelper";
import { SupplierInvoiceRowDTO } from "../../../../Common/Models/SupplierInvoiceRowDTO";
import { IRequestReportService } from "../../../Reports/RequestReportService";

export class EditController extends EditControllerBase {

    // Config
    private accountYearId: number = 0;
    private accountYearIsOpen: boolean = false;

    private billingTypes: SmallGenericType[];
    private voucherSeries: IVoucherSeriesDTO[];
    private suppliers: SmallGenericType[];
    private paymentMethods: IPaymentMethodDTO[];
    private paymentMethodsAccounts: any[];
    private paymentInfos: any[];
    private unpaidInvoices: any[];
    private currencies: any[];
    private matchCodes: any[];
    private accountStds: IAccountDTO[];

    private selectedPaymentInfo: any;
    private selectedVoucherSeries: any;
    private accountPeriod: any;

    private invoice: SupplierInvoiceDTO;
    private payment: PaymentRowDTO;

    private seqNr: number;

    private currencyHelper: CurrencyHelper;

    get isCredit(): boolean {
        return this.payment.billingType === TermGroup_BillingType.Credit;
    }

    get isVoucher(): boolean {
        return this.payment && ((this.payment.voucherHeadId && this.payment.voucherHeadId !== 0) || this.payment.status == SoeOriginStatus.Cancel);
    }

    get isCancelled(): boolean {
        return this.payment && this.payment.status == SoeOriginStatus.Cancel;
    }

    get fullyPaidEnabled(): boolean {
        return this.locked ? (this.isSupportSuperAdminUnlocked ? Math.abs(this.payment.paidAmountCurrency + this.payment.amountCurrency) <= Math.abs(this.invoice.totalAmountCurrency) : true) : false;
    }

    private _selectedSupplier;
    get selectedSupplier() {
        return this._selectedSupplier;
    }
    set selectedSupplier(item: any) {
        this._selectedSupplier = item;

        if (this.payment)
            this.payment.actorId = this.selectedSupplier ? this.selectedSupplier.id : null;

        if (!this.supplier || !this.selectedSupplier || (this.supplier.actorSupplierId !== this.selectedSupplier.id)) {
            this.loadSupplier(this.selectedSupplier ? this.selectedSupplier.id : null);
        }
    }

    private _selectedInvoice;
    get selectedInvoice(): ISmallGenericType {
        return this._selectedInvoice;
    }
    set selectedInvoice(item: ISmallGenericType) {
        this._selectedInvoice = item;
        if (this.selectedInvoice) {
            if (this.payment)
                this.payment.invoiceId = this.selectedInvoice.id;
            if (this.selectedInvoice.id > 0) {
                this.loadInvoice(this.selectedInvoice ? this.selectedInvoice.id : null);
                this.invoiceId = this._selectedInvoice.id;
            }
            else {
                this.invoice = new SupplierInvoiceDTO();
            }
        }
    }

    private _selectedPaymentMethod;
    get selectedPaymentMethod(): IPaymentMethodDTO {
        return this._selectedPaymentMethod;
    }
    set selectedPaymentMethod(item: IPaymentMethodDTO) {
        this._selectedPaymentMethod = item;
        if (this.payment && this.selectedPaymentMethod && this.payment.paymentMethodId !== this.selectedPaymentMethod.paymentMethodId) {
            this.payment.paymentMethodId = this.selectedPaymentMethod.paymentMethodId
            this.loadPaymentMethod(this.payment.paymentMethodId);
        }
    }

    private _selectedPayDate: Date;
    get selectedPayDate() {
        return this._selectedPayDate;
    }
    set selectedPayDate(date: Date) {
        this._selectedPayDate = CalendarUtility.convertToDate(date);
        if (this._selectedPayDate) {
            this.loadAccountPeriod();
        }

        if (this.payment)
            this.payment.payDate = this._selectedPayDate;

        if (CoreUtility.sysCountryId == TermGroup_Languages.Finnish && this.isNew)
            this.showTimeDiscountDialog(); //Only for finnish syscountryid
    }

    private _selectedMatchCode;
    get selectedMatchCode() {
        return this._selectedMatchCode;
    }
    set selectedMatchCode(item: any) {
        this._selectedMatchCode = item;
    }

    private supplier: SupplierDTO;

    private defaultVatRate: number = 0;
    private vatRate: number = Constants.DEFAULT_VAT_RATE;
    private vatCode: VatCodeDTO;
    private defaultVatCode: VatCodeDTO;

    private autoTransferPaymentToVoucher: boolean;
    private askPrintVoucher: boolean;
    private supplierPaymentVoucherReportId: number;
    private defaultPaymentMethodId: number;
    private defaultPaymentConditionId: number;
    private defaultVoucherSeriesTypeId: number;
    private showTransactionCurrency = false;
    private showEnterpriseCurrency = false;
    private showLedgerCurrency = false;
    private useInternalAccountWithBalanceSheetAccounts = false;
    private defaultVatCodeId: number;

    private locked = false;
    private enableRevoke = false;
    private isSupportUnlocked = false;
    private isSupportSuperAdminUnlocked = false;
    private isAfterPayment = false;
    private isSpecificInvoice = false;

    private useMatching: boolean;
    private enableMatchCode: boolean;
    private useCurrency: boolean;
    private _useVat: boolean;
    get useVat() {
        return this._useVat;
    }
    set useVat(item: any) {
        this._useVat = item;
    }

    private useTimeDiscount: boolean;

    //Amounts
    preAmount: number;
    preAmountCurrency: number;

    // Company accounts
    private defaultCreditAccountId: number = 0;
    private defaultDebitAccountId: number = 0;
    private defaultVatAccountId: number = 0;
    private defaultInterimAccountId: number = 0;

    private supplierUnderpayAccountId: number = 0;
    private supplierOverpayAccountId: number = 0;
    private currencyProfitAccountId: number = 0;
    private currencyLossAccountId: number = 0;
    private diffAccountId: number = 0;
    private bankFeeAccountId: number = 0;

    // Currency
    private _currencyRate: number = 1;
    get currencyRate() {
        return this._currencyRate;
    }
    set currencyRate(item: any) {
        var changed = item && (item !== this._currencyRate);
        this._currencyRate = item;
        if (changed && (this.isNew || this.currencyHasChanged)) {
            this.generateAccountingRows();
        }
    }
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
        var changed = item && (item !== this._isBaseCurrency);
        this._isBaseCurrency = item;
        if (changed && (this.isNew || this.currencyHasChanged)) {
            this.generateAccountingRows();
        }
    }

    // Original amounts
    originalPaidAmount: number = 0;
    originaRemainingAmount: number = 0;

    //Flags 
    currencyHasChanged = false;

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
        coreService: ICoreService,
        private readonly requestReportService: IRequestReportService,
        private accountingService: IAccountingService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Economy.Supplier.InvoicesSupplierPayments.Edit", Feature.Economy_Supplier_Payment_Payments_Edit, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
        this.modalInstance = $uibModal;

        // Config parameters
        this.accountYearId = soeConfig.accountYearId;
        this.accountYearIsOpen = soeConfig.accountYearIsOpen;
    }

    protected init() {
        this.invoiceId = this.parameters.invoiceId;
        this.paymentId = this.parameters.paymentId;

        this.currencyHelper = new CurrencyHelper(this.coreService, this.$timeout, this.$q);
        this.currencyHelper.init();

        this.isSpecificInvoice = (this.invoiceId) ? true : false;
        this.isAfterPayment = (this.paymentId) ? true : false;

        this.$q.all([
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadCurrencies(),
            this.loadCompanyAccounts(),
            this.loadSuppliers(),
            this.loadBillingTypes(),
            this.loadMatchCodes(),
            this.loadAccountStds(),
        ]).then(() => this.$q.all([
            this.loadVoucherSeries(soeConfig.accountYearId),
            this.loadPaymentMethods()
        ])).then(() =>
            this.load())
            .then(() => {
                this.setupToolBar();
                this.stopProgress();
            });

        this.messagingService.subscribe(Constants.EVENT_ACCOUNTING_ROWS_READY, (guid) => {
            if (this.guid === guid && this.isNew) {
                this.generateAccountingRows();
            }
        }, this.$scope);
    }

    private setupToolBar() {

        if (this.payment) {
            const buttonGroup = ToolBarUtility.createGroup();
            this.buttonGroups.push(buttonGroup);

            let buttonIcon = "fal fa-info-circle infoColor";
            if (this.payment.status === SoePaymentStatus.Error) {
                buttonIcon = "fal fa-exclamation-triangle errorColor";
            }
            else if (this.payment.transferStatus === TermGroup_PaymentTransferStatus.PendingTransfer) {
                buttonIcon = "fal fa-exchange warningColor";
            }

            buttonGroup.buttons.push(new ToolBarButton("", this.getStatusMessage(), IconLibrary.FontAwesome, buttonIcon, () => {
                this.notificationService.showDialogEx("", this.getStatusMessage(), SOEMessageBoxImage.Information);
            }, null, null
            ));
        }

        // Functions
        const keys: string[] = [
            "core.save",
            "core.saveandclose"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions.push({ id: SupplierPaymentEditSaveFunctions.Save, name: terms["core.save"] });
            this.saveFunctions.push({ id: SupplierPaymentEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] });
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

    protected setupLookups() {
        super.setupLookups();
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features = [Feature.Economy_Supplier_Invoice_Matching, Feature.Economy_Preferences_Currency];
        return this.coreService.hasModifyPermissions(features).then(x => {
            this.useMatching = this.useVat = x[Feature.Economy_Supplier_Invoice_Matching];
            this.useCurrency = x[Feature.Economy_Preferences_Currency];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.SupplierPaymentManualTransferToVoucher);
        settingTypes.push(CompanySettingType.SupplierPaymentDefaultPaymentMethod);
        settingTypes.push(CompanySettingType.SupplierPaymentDefaultPaymentCondition);
        settingTypes.push(CompanySettingType.SupplierPaymentVoucherSeriesType);
        settingTypes.push(CompanySettingType.SupplierUseTimeDiscount);
        settingTypes.push(CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.SupplierShowTransactionCurrency);
        settingTypes.push(CompanySettingType.SupplierShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.SupplierShowLedgerCurrency);
        settingTypes.push(CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts);
        settingTypes.push(CompanySettingType.AccountingDefaultVatCode);

        return this.coreService.getCompanySettings(settingTypes).then(settings => {
            this.autoTransferPaymentToVoucher = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.SupplierPaymentManualTransferToVoucher);
            this.askPrintVoucher = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer);
            this.supplierPaymentVoucherReportId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.AccountingDefaultVoucherList);
            this.defaultPaymentMethodId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.SupplierPaymentDefaultPaymentMethod);
            if (this.defaultPaymentMethodId > 0)
                this.loadPaymentMethod(this.defaultPaymentMethodId);
            this.defaultPaymentConditionId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.SupplierPaymentDefaultPaymentMethod);
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.SupplierPaymentVoucherSeriesType);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.SupplierShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.SupplierShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.SupplierShowLedgerCurrency);
            this.useTimeDiscount = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.SupplierUseTimeDiscount);
            this.useInternalAccountWithBalanceSheetAccounts = SettingsUtility.getBoolCompanySetting(settings, CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts);
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(settings, CompanySettingType.AccountingDefaultVatCode);
        });
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        const settingTypes: number[] = [
                CompanySettingType.AccountCommonCheck,
                CompanySettingType.AccountSupplierDebt,
                CompanySettingType.AccountCommonVatReceivable,
                CompanySettingType.AccountSupplierInterim,
                CompanySettingType.AccountSupplierUnderpay,
                CompanySettingType.AccountSupplierOverpay,
                CompanySettingType.AccountCommonCurrencyProfit,
                CompanySettingType.AccountCommonCurrencyLoss,
                CompanySettingType.AccountCommonDiff,
                CompanySettingType.AccountCommonBankFee
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCheck);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierDebt);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivable);
            this.defaultInterimAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierInterim);
            this.supplierUnderpayAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierUnderpay);
            this.supplierOverpayAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierOverpay);
            this.currencyProfitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCurrencyProfit);
            this.currencyLossAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCurrencyLoss);
            this.diffAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonDiff);
            this.bankFeeAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonBankFee);

            if (this.supplierUnderpayAccountId === 0)
                this.supplierUnderpayAccountId = this.diffAccountId;
            if (this.supplierOverpayAccountId === 0)
                this.supplierOverpayAccountId = this.diffAccountId;

            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId);
        });
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, true).then((x: SmallGenericType[]) => {
            this.suppliers = x;
        });
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then(x => {
            this.billingTypes = _.sortBy(x, 'id');
        });
    }

    private loadMatchCodes(): ng.IPromise<any> {
        return this.supplierService.getMatchCodes(SoeInvoiceMatchingType.SupplierInvoiceMatching, true).then(x => {
            this.matchCodes = x;
        });
    }

    private loadAccountStds(): ng.IPromise<any> {
        return this.accountingService.getAccountStds(false).then(x => {
            this.accountStds = x;
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesByYear(accountYearId, false, true).then((x: IVoucherSeriesDTO[]) => {
            this.voucherSeries = x;
            if(!this.paymentId)
                this.selectedVoucherSeries = _.find(this.voucherSeries, s => s.voucherSeriesId === this.defaultVoucherSeriesTypeId);
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        return this.supplierService.getPaymentMethods(SoeOriginType.SupplierPayment, false, false, false, true).then((x: IPaymentMethodDTO[]) => {
            this.paymentMethods = x;
            this.selectedPaymentMethod = _.find(this.paymentMethods, s => s.paymentMethodId === this.defaultPaymentMethodId);
        });
    }

    private loadPaymentMethod(paymentMethodId: number) {
        if (!this.paymentMethodsAccounts)
            this.paymentMethodsAccounts = [];

        if (this.selectedPaymentMethod && this.paymentMethodsAccounts.length > 0) {
            if (_.includes(this.paymentMethodsAccounts, { id: paymentMethodId }))
                return;
        }

        this.supplierService.getPaymentMethod(paymentMethodId, true, false).then(method => {
            this.paymentMethodsAccounts.push({
                id: method.paymentMethodId,
                accountId: method.accountId
            });
            this.generateAccountingRows();
        });
    }

    private getStatusMessage(): string {
        if (this.payment && this.payment.statusMsg)
            return this.payment.statusMsg;
        else if (this.payment && this.payment.statusName)
            return this.payment.statusName;
        else
            return "";
    }

    private load(): ng.IPromise<any> {
        if (this.invoiceId) {
            return this.supplierService.getInvoiceForPayment(this.invoiceId)
                .then(invoice => {
                    this.createPayment(invoice);
                    return invoice;
                }).then(invoice => {
                    this.invoice = invoice;
                    var accountingRows = SupplierInvoiceRowDTO.toAccountingRowDTOs(this.invoice.supplierInvoiceRows);
                    this.invoice.accountingRows = _.orderBy(accountingRows.filter(x => x.type === AccountingRowType.AccountingRow && x.state !== SoeEntityState.Deleted), 'rowNr');
                    return this.loadSupplier(invoice.actorId, invoice.invoiceId);
                }).then(() => {
                    this.selectedSupplier = _.find(this.suppliers, s => s.id === this.invoice.actorId);
                    this.isDirty = true;
                    this.unpaidInvoices = [{ id: this.invoice.invoiceId, name: this.invoice.invoiceNr }];
                    this._selectedInvoice = this.unpaidInvoices[0];
                    this.generateAccountingRows();
                });
        }
        else if (this.paymentId) {
            return this.supplierService.getPayment(this.paymentId, true, true, false)
                .then(payment => {
                    this.isNew = false;
                    var obj = new PaymentRowDTO();
                    this.payment = angular.extend(obj, payment);
                    this.payment.paymentAccountRows = this.payment.paymentAccountRows.map(ca => {
                        var obj = new AccountingRowDTO();
                        angular.extend(obj, ca);
                        return obj;
                    });
                    this.invoiceId = payment.invoiceId;
                    this.setLocked();
                    return this.supplierService.getInvoiceForPayment(payment.invoiceId);
                }).then(invoice => {
                    this.invoice = invoice;
                    this.updatePayment(invoice);
                    return this.loadSupplier(invoice.actorId, invoice.invoiceId);
                }).then(() => {
                    this._selectedSupplier = _.find(this.suppliers, s => s.id === this.invoice.actorId);
                    this.selectedVoucherSeries = _.find(this.voucherSeries, s => s.voucherSeriesTypeId === this.payment.voucherSeriesTypeId);
                    this.unpaidInvoices = [{ id: this.invoice.invoiceId, name: this.invoice.invoiceNr }];
                    this._selectedInvoice = this.unpaidInvoices[0];
                    this.paymentInfos = [{ paymentNr: this.payment.paymentNr }];
                    this.selectedPaymentInfo = this.paymentInfos[0];
                    this.selectedPaymentMethod = _.find(this.paymentMethods, m => m.paymentMethodId == this.payment.paymentMethodId);
                    this.selectedPayDate = new Date(<any>this.payment.payDate);
                });
        }
        const deferral = this.$q.defer();
        this.new();

        deferral.resolve();
        return deferral.promise;
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

    private loadVatCode(vatCodeId: number): ng.IPromise<any> {
        this.vatCode = null;
        if (vatCodeId) {
            return this.accountingService.getVatCode(vatCodeId).then(x => {
                this.vatCode = x;
            });
        }
        else {
            this.vatCode = null;
            var deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }
    }

    private setVatRate(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        if (this.vatCode)
            this.vatRate = this.vatCode.percent;
        else
            this.setDefaultVatRate();
        deferral.resolve();
        return deferral.promise;
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

    private loadUnpaidInvoice(supplierId: number): ng.IPromise<any> {
        return this.supplierService.getUnpaidInvoices(supplierId, false).then(invoices => {
            this.unpaidInvoices = invoices;
            if (this.invoiceId && this.invoiceId > 0) {
                this.selectedInvoice = _.find(this.unpaidInvoices, s => s.id === this.invoiceId);
            }
        })
    }

    private loadPaymentInformation(supplierId: number): ng.IPromise<any> {
        return this.supplierService.getPaymentInformationViews(supplierId).then(paymentInfos => {
            this.paymentInfos = paymentInfos;
            this.selectedPaymentInfo = _.find(this.paymentInfos, s => s.default);
        })
    }
    public loadSupplier(supplierId: number, invoiceId: number = null): ng.IPromise<any> {
        if (!supplierId)
            return null;

        return this.supplierService.getSupplier(supplierId, false, true, false, false).then(x => {
            this.supplier = x;

            if (this.isSpecificInvoice) {
                return this.loadPaymentInformation(supplierId)
            }
            else {
                return this.$q.all([
                    this.loadUnpaidInvoice(supplierId),
                    this.loadPaymentInformation(supplierId)
                ]);
            }
        });
    }

    private loadInvoice(id: number): ng.IPromise<any> {
        if (id) {
            return this.supplierService.getInvoiceForPayment(id).then(invoice => {
                this.invoice = invoice;
                this.unpaidInvoices = [{ id: this.invoice.invoiceId, name: this.invoice.invoiceNr }];
                this._selectedInvoice = this.unpaidInvoices[0];

                this.loadVatCode(this.invoice.vatCodeId).then(() => {
                    this.setVatRate().then(() => {
                        if (!this.paymentId)
                            this.createPayment(invoice);
                        this.setupWatchers();
                    });
                });
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
    
    private new(emptyIds: boolean = false) {
        this.isNew = true;
        this.seqNr = undefined;
        this.selectedSupplier = null;
        this.invoice = new SupplierInvoiceDTO();
        this.invoice.paidAmount = 0;
        this.invoice.totalAmount = 0;
        this.invoice.vatAmount = 0;
        this.invoice.remainingAmount = 0;
        this.invoice.paidAmountCurrency = 0;
        this.invoice.fullyPayed = false;
        this.invoice.currencyId = this.currencies[0].currencyId;
        this.createPayment(null);
        this.payment.paymentAccountRows = [];

        if (emptyIds) {
            this.paymentId = undefined;
            this.invoiceId = undefined;
            this.isSpecificInvoice = false;
            this.isAfterPayment = false;
        }

        this.setLocked();
    }

    private updatePayment(invoice: any, calculateRemaining: boolean = false) {
        this.payment.paidAmount = invoice.paidAmount || 0;
        this.payment.totalAmount = invoice.totalAmount || 0;
        this.payment.vatAmount = invoice.vatAmount || 0;
        this.payment.remainingAmount = invoice.fullyPayed && !calculateRemaining ? 0 : (invoice.totalAmountCurrency - invoice.paidAmountCurrency) || 0;
        this.payment.currencyId = invoice.currencyId;
        this.seqNr = this.payment.seqNr;
        this.payment.fullyPaid = (this.payment.remainingAmount === 0);
        this.payment.billingType = this.invoice.billingType;

        // Set original
        this.originalPaidAmount = this.payment.amount;
        this.originaRemainingAmount = this.payment.remainingAmount;

        if (this.invoice) {
            this.currencyRate = this.invoice.currencyRate;
            this.currencyDate = this.invoice.currencyDate;
        }
    }

    private createPayment(invoice: any) {
        this.payment = new PaymentRowDTO();
        this.payment.vatCodeId = null;
        this.payment.voucherSeriesId = this.defaultVoucherSeriesTypeId;
        this.selectedVoucherSeries = _.find(this.voucherSeries, s => s.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId);
        this.payment.currencyId = this.currencies[0].currencyId;
        this.payment.billingType = this.billingTypes[0].id;
        this.payment.paidAmount = 0;
        this.payment.totalAmount = 0;
        this.payment.vatAmount = 0;
        this.payment.remainingAmount = 0;
        this.payment.amount = 0;
        this.payment.bankFeeCurrency = 0;
        this.currencyRate = this.currencyRate;
        this.currencyDate = this.payment.currencyDate = CalendarUtility.getDateToday();

        this.preAmount = 0;
        this.preAmountCurrency = 0;

        if (invoice) {
            //this.seqNr = invoice.seqNr;
            this.payment.invoiceId = invoice.invoiceId;
            this.payment.paidAmount = invoice.paidAmount || 0;
            this.payment.paidAmountCurrency = invoice.paidAmountCurrency || 0;
            this.payment.totalAmount = invoice.totalAmount || 0;
            this.payment.totalAmountCurrency = invoice.totalAmountCurrency || 0;
            this.payment.vatAmount = invoice.vatAmount || 0;
            this.payment.remainingAmount = (invoice.totalAmountCurrency - invoice.paidAmountCurrency) || 0;
            this.payment.currencyId = invoice.currencyId;
            this.selectedPayDate = new Date(invoice.dueDate);
            this.payment.amountCurrency = (invoice.totalAmountCurrency - invoice.paidAmountCurrency) || 0;
            this.payment.amount = (this.payment.amountCurrency * invoice.currencyRate) || 0;
            //this.payment.amount = (invoice.totalAmount - invoice.paidAmount) || 0;
            this.payment.fullyPaid = this.useMatching;
            this.payment.vatType = invoice.vatType;
            this.payment.vatCodeId = invoice.vatCodeId;
            this.payment.billingType = invoice.billingType;
            this.payment.originDescription = invoice.originDescription;
            this.payment.text = invoice.originDescription;

            this.currencyRate = invoice.currencyRate;
            this.currencyDate = invoice.currencyDate;

            this.preAmount = this.payment.amount;
            this.preAmountCurrency = this.payment.amountCurrency;

            this.loadVatCode(invoice.vatCodeId).then(x => { this.setVatRate() });
        }

        this.useVat = this.useMatching && invoice !== null;

        this.totalAmountChanged();
        this.generateAccountingRows();
    }

    private amountChanged(id: string) {
        this.$timeout(() => {
            var setFullyPaid: boolean = false;
            if (id === 'total') {
                this.invoice.totalAmount = this.invoice.totalAmountCurrency;
                //if (!this.invoice.invoiceId || this.invoice.invoiceId === 0)
                    this.updatePayment(this.invoice);
                this.calculateVatAmount();
            }
            if (id === 'amount') {
                //if (!this.invoice.invoiceId || this.invoice.invoiceId === 0) {
                    if (this.invoice.totalAmount === 0) {
                        this.invoice.totalAmount = this.invoice.totalAmountCurrency = this.payment.amount;
                        this.calculateVatAmount();
                    }
                //}
                //setFullyPaid = true;
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
                }
            }
            this.generateAccountingRows();
        });
    }


    private calculateRemainingAmountAfterPayment() {
        //Payment has been saved before.
        //This is now calculating as if the payment is being edited.
        const diff = this.payment.invoiceTotalAmount - this.payment.paidAmount;
        if (this.isCredit) {
            return diff - this.originalPaidAmount;

        } else {
            return diff + this.originalPaidAmount;
        }
    }

    private generateAccountingRows() {
        if ((!this.payment) || (!this.supplier))
            return;

        // Clear rows
        this.payment.paymentAccountRows = [];

        const fromBaseCurrency = (amount: number) => {
            return (amount / this.currencyRate).round(8);
        }
        const fullyPaid = this.payment.fullyPaid;

        //Prepare amounts.
        //We do the accounting in the Transaction Currency (TC).
        //Some fields such as the payment amount is always in the Base Currency (BC).
        const paymentAmountBC = this.payment.amount;
        const paymentAmountTC = fromBaseCurrency(paymentAmountBC);

        //This amount can be edited by the user, which corresponds to the invoice currency amount.
        const paymentAmountInitialTC = this.payment.amountCurrency;
        

        // Calculate the difference in the transaction currency.
        // Eg. invoice is 100 EUR (1100 SEK), at payment date is 100 EUR is 1050 SEK, creating a currency diff of 50 SEK.
        const currencyDiffTC = paymentAmountTC - paymentAmountInitialTC; 

        let remainingAmountTC = 0;
        if (this.isAfterPayment) {
            const remainingAmountBC = this.calculateRemainingAmountAfterPayment();
            remainingAmountTC = fromBaseCurrency(remainingAmountBC);
        } else {
            remainingAmountTC = this.payment.remainingAmount;
        }

        const supplierDebtAmountTC = fullyPaid ? remainingAmountTC : paymentAmountTC;

        const bankFeeAmountBC = this.payment.bankFeeCurrency; //This input is always in the base currency.
        const bankFeeAmountTC = fromBaseCurrency(bankFeeAmountBC);

        // Calculate the difference in the transaction currency.
        // We need to handle +/- due to how it should be accounted.
        const amountDiffTC = this.isCredit ?
            remainingAmountTC - paymentAmountTC :
            paymentAmountTC - remainingAmountTC;

        this.resetMatchCode(amountDiffTC);

        if (fullyPaid) {
            // We handle automatic rest accounting here.
            // Prio 1: Match code if one is selected, as that is up to the user.
            // Prio 2: Currency diff if it's a currency payment.
            // Prio 3: Over/Under payment diff if it's a normal payment.
            // We currently have no way of handling a combination of the latter two as we 
            // don't allow the user to input a payment amount in the transaction currency.
            if (this.useMatching && this.selectedMatchCode?.accountId && amountDiffTC !== 0) {
                this.generateAccountingRowsForMatchCode(amountDiffTC);
            }
            else if (!this.isBaseCurrency && currencyDiffTC !== 0) {
                this.generateAccountingRowsForCurrencyDiff(currencyDiffTC);
            }
            else if (amountDiffTC != 0) {
                this.generateAccountingRowsForUnderOverPaymentDiff(amountDiffTC);
            }
        }
        //else if (!this.isBaseCurrency && currencyDiffTC !== 0) {
        //    this.generateAccountingRowsForCurrencyDiff(currencyDiffTC);
        //}

        // Adjust supplier debt
        const debitAccountId = this.findDebtAccountId();
        this.createAccountingRow(SupplierAccountType.Debit, debitAccountId, supplierDebtAmountTC, true, false, this.useInternalAccountWithBalanceSheetAccounts);

        // Adjust bank balance
        const creditAccountId = this.paymentMethodsAccounts.find(s => s.id === this.selectedPaymentMethod.paymentMethodId).accountId || 0;
        this.createAccountingRow(SupplierAccountType.Credit, creditAccountId, paymentAmountTC, false, false, this.useInternalAccountWithBalanceSheetAccounts);

        // Handle Bank fee (if specified)
        if (bankFeeAmountTC > 0) {
            this.createAccountingRow(SupplierAccountType.Unknown, this.bankFeeAccountId, bankFeeAmountTC, true, false, this.useInternalAccountWithBalanceSheetAccounts);
            this.createAccountingRow(SupplierAccountType.Credit, creditAccountId, bankFeeAmountTC, false, false, this.useInternalAccountWithBalanceSheetAccounts);
        }
        
        //Reset flags
        this.currencyHasChanged = false;

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnAllRows');
            this.$scope.$broadcast('rowsAdded');
        });
    }

    private resetMatchCode(amountDiffTC: number) {
        if (this.payment.fullyPaid) {
            if (amountDiffTC === 0) {
                this.enableMatchCode = false;
                this.selectedMatchCode = null;
            } else {
                this.enableMatchCode = true;
            }
        }
    }

    private generateAccountingRowsForCurrencyDiff(currencyDiffTC: number) {
        const currencyDiffAccountId = currencyDiffTC > 0 ? this.currencyLossAccountId : this.currencyProfitAccountId;
        const isDebit = this.isCredit ? currencyDiffTC < 0 : currencyDiffTC > 0;
        this.createAccountingRow(SupplierAccountType.Unknown, currencyDiffAccountId, Math.abs(currencyDiffTC), isDebit, false, this.useInternalAccountWithBalanceSheetAccounts);
    }

    private generateAccountingRowsForMatchCode(amountDiffTC: number) {
        let adjustedAmountDiffTC = amountDiffTC;
        const matchAccountId = this.selectedMatchCode.accountId;
        if (matchAccountId) {
            if (this.useVat) {
                const vatAmountTC = (amountDiffTC * this.vatRate / (100 + this.vatRate)).round(2);
                adjustedAmountDiffTC -= vatAmountTC;

                this.createAccountingRow(SupplierAccountType.Unknown, matchAccountId, Math.abs(adjustedAmountDiffTC), adjustedAmountDiffTC > 0, false, this.useInternalAccountWithBalanceSheetAccounts);

                const matchVatAccountId = this.selectedMatchCode.vatAccountId;
                if (matchVatAccountId && vatAmountTC)
                    this.createAccountingRow(SupplierAccountType.Unknown, matchVatAccountId, Math.abs(vatAmountTC), vatAmountTC > 0, false, this.useInternalAccountWithBalanceSheetAccounts);

            }
            else {
                this.createAccountingRow(SupplierAccountType.Unknown, matchAccountId, Math.abs(amountDiffTC), amountDiffTC > 0, false, this.useInternalAccountWithBalanceSheetAccounts);
            }
        }

        this.enableMatchCode = amountDiffTC != 0 && this.payment.fullyPaid;
    }

    private generateAccountingRowsForUnderOverPaymentDiff(amountDiffTC: number) {
        // Handle over/under payments.

        let adjustedAmountDiffTC = amountDiffTC;
        let vatAmountTC = 0;

        if (this.useTimeDiscount && this.useVat) {
            if (this.invoice.vatType === TermGroup_InvoiceVatType.Merchandise) {
                vatAmountTC = (amountDiffTC * this.vatRate / (100 + this.vatRate)).round(2);
                adjustedAmountDiffTC -= vatAmountTC;
            }
        }

        var diffAccountId = Math.abs(adjustedAmountDiffTC) > 0 ? this.supplierOverpayAccountId : this.supplierUnderpayAccountId;
        this.createAccountingRow(SupplierAccountType.Unknown, diffAccountId, adjustedAmountDiffTC, adjustedAmountDiffTC > 0, false, this.useInternalAccountWithBalanceSheetAccounts);
        // VAT row
        if (vatAmountTC != 0)
            this.createAccountingRow(SupplierAccountType.VAT, 0, vatAmountTC, vatAmountTC > 0, true, this.useInternalAccountWithBalanceSheetAccounts);
    }

    private createAccountingRow(type: SupplierAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean, getInternalAccountFromInvoice: boolean = false): AccountingRowDTO {
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
        row.isInterimRow = type === SupplierAccountType.Debit && (this.invoice ? this.invoice.interimInvoice : false);
        row.state = SoeEntityState.Active;
        row.invoiceId = this.invoice ? this.invoice.invoiceId : 0;
        row.isModified = false;

        // Set accounts
        if (type !== SupplierAccountType.Unknown && !accountId) {
            row.dim1Id = this.getAccountId(type, 1, row);
        } else {
            row.dim1Id = accountId;
        }

        row.dim2Id = this.getAccountIdFromInvoiceRow(2);
        row.dim3Id = this.getAccountIdFromInvoiceRow(3);
        row.dim4Id = this.getAccountIdFromInvoiceRow(4);
        row.dim5Id = this.getAccountIdFromInvoiceRow(5);
        row.dim6Id = this.getAccountIdFromInvoiceRow(6);

        this.payment.paymentAccountRows.push(row);
        return row;
    }

    private getAccountIdFromInvoiceRow(dimNr: number) {
        const invoiceDebtAccountingRow = this.findDebtRow();
        if (!invoiceDebtAccountingRow) return 0;

        switch (dimNr) {
            case 2:
                return invoiceDebtAccountingRow.dim2Id;
            case 3:
                return invoiceDebtAccountingRow.dim3Id;
            case 4:
                return invoiceDebtAccountingRow.dim4Id;
            case 5:
                return invoiceDebtAccountingRow.dim5Id;
            case 6:
                return invoiceDebtAccountingRow.dim6Id;
        }
    }

    private findDebtRow() {
        return this.invoice.accountingRows.find(row => !row.isInterimRow &&
            !row.isVatRow &&
            !row.isContractorVatRow &&
            ((this.invoice.billingType === TermGroup_BillingType.Credit && row.amount >= 0) ||
                (this.invoice.billingType !== TermGroup_BillingType.Credit && row.amount < 0))
        )
    }

    private getAccountId(type: SupplierAccountType, dimNr: number, row: AccountingRowDTO): number {
        // First try to get account from invoice
        let accountId = this.getSupplierAccountId(type, dimNr);
        if (accountId === 0 && dimNr === 1) {
            // Then try to get account from supplier
            if (accountId === 0 && dimNr === 1) {
                // No account found on supplier, use base account
                switch (type) {
                    case SupplierAccountType.Credit:
                        accountId = this.defaultCreditAccountId;
                        break;
                    case SupplierAccountType.Debit:
                        accountId = this.defaultDebitAccountId;
                        break;
                    case SupplierAccountType.VAT:
                        accountId = this.defaultVatAccountId;
                        break;
                }
            }
        }
        return accountId;
    }

    private isDebtAccount(accountId: number) {
        if (_.find(this.accountStds, (row) => row.accountId == accountId && row.accountTypeSysTermId == TermGroup_AccountType.Debt))
            return true;
        return false;
    }

    private findDebtAccountId() {
        const interimAccountId = this.getInterimAccountId();
        if (interimAccountId) return interimAccountId;

        const invoiceDebtAccountingRow = this.findDebtRow();

        if (invoiceDebtAccountingRow && this.isDebtAccount(invoiceDebtAccountingRow.dim1Id)) {
            return invoiceDebtAccountingRow.dim1Id;
        }
        return this.getDefaultDebtAccountId();
    }

    private getDefaultDebtAccountId() {
        const supplierDebtAccountId = this.getSupplierAccountId(SupplierAccountType.Debit, 1);
        if (this.isDebtAccount(supplierDebtAccountId)) {
            return supplierDebtAccountId;
        }
        return this.defaultDebitAccountId;
    }

    private getInterimAccountId() {
        //First check if the invoice is interim.
        if (this.invoice && this.invoice.interimInvoice) {
            const supplierInterimAccountId = this.getSupplierAccountId(SupplierAccountType.Interim, 1);
            if (supplierInterimAccountId) return supplierInterimAccountId;

            const defaultInterimAccountId = this.defaultInterimAccountId;
            if (defaultInterimAccountId) return defaultInterimAccountId;

            const keys: string[] = [
                "economy.supplier.invoice.interimaccountmissing.title",
                "economy.supplier.invoice.interimaccountmissing.message"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["economy.supplier.invoice.interimaccountmissing.title"], terms["economy.supplier.invoice.interimaccountmissing.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }

        return null;
    }

    private getSupplierAccountId(type: SupplierAccountType, dimNr: number): number {
        if (this.supplier && this.supplier.accountingSettings) {
            var setting = _.find(this.supplier.accountingSettings, { type: type });
            if (setting) {
                switch (dimNr) {
                    case 1:
                        return setting.account1Id ? setting.account1Id : 0;
                    case 2:
                        return setting.account2Id ? setting.account2Id : 0;
                    case 3:
                        return setting.account3Id ? setting.account3Id : 0;
                    case 4:
                        return setting.account4Id ? setting.account4Id : 0;
                    case 5:
                        return setting.account5Id ? setting.account5Id : 0;
                    case 6:
                        return setting.account6Id ? setting.account6Id : 0;
                    default:
                        return 0;
                }
            }
        }
    }

    private convertAmount(field: string, amount: number) {
        //if (this.loadingInvoice)
        //    return;

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
        this.invoice[item.field] = item.baseCurrencyAmount ? item.baseCurrencyAmount.round(2) : 0;
        this.invoice[item.field + 'EnterpriceCurrency'] = item.enterpriseCurrencyAmount ? item.enterpriseCurrencyAmount.round(2) : 0;
        this.invoice[item.field + 'LedgerCurrency'] = item.ledgerCurrencyAmount ? item.ledgerCurrencyAmount.round(2) : 0;
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
            this.generateAccountingRows();
        });
    }

    private useVatChanging() {
        this.$timeout(() => {
            this.generateAccountingRows();
        });
    }

    // ACTIONS

    private executeSaveFunction(option) {
        switch (option.id) {
            case SupplierPaymentEditSaveFunctions.Save:
                this.trySave(false);
                break;
            case SupplierPaymentEditSaveFunctions.SaveAndClose:
                this.trySave(true);
                break;
        }
    }

    public trySave(closeAfterSave: boolean, ignoreVoucherCheck = false, ignoreAmountCheck = false) {
        if (this.autoTransferPaymentToVoucher && !ignoreVoucherCheck) {
            const keys: string[] = [
                "core.verifyquestion",
                "economy.supplier.payment.autotransfertovoucher"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.payment.autotransfertovoucher"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.trySave(closeAfterSave, true);
                });
            });

            return;
        }

        if (this.payment.fullyPaid && !ignoreAmountCheck) {
            var noOfAccRows: number = 0;
            var compareAmount: number = 0;
            var paidAmount = this.invoice.paidAmount || 0;
            var invoiceAmount = this.invoice.totalAmount || 0;
            var payAmount = this.payment.amount || 0;
            var isPendingPayment: boolean = (this.payment.status === SoePaymentStatus.Pending);
            var hasVoucher: boolean = (this.payment.voucherHeadId && this.payment.voucherHeadId !== 0);

            if (invoiceAmount < 0) {
                compareAmount = _.reduce(this.payment.paymentAccountRows, (x, y) => { return x + y.creditAmount; }, 0) * -1;
                noOfAccRows = _.filter(this.payment.paymentAccountRows, { isCreditRow: true }).length;
            }
            else {
                compareAmount = _.reduce(this.payment.paymentAccountRows, (x, y) => { return x + y.debitAmount; }, 0);
                noOfAccRows = _.filter(this.payment.paymentAccountRows, { isDebitRow: true }).length;
            }

            if (!this.isSupportSuperAdminUnlocked && (invoiceAmount - paidAmount).round(2) != compareAmount.round(2) && !(isPendingPayment && !hasVoucher)) {
                const keys2: string[] = [
                    "core.verifyquestion",
                    "economy.supplier.payment.amountsnotmatching"
                ];

                this.translationService.translateMany(keys2).then((terms) => {
                    const modal2 = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.payment.amountsnotmatching"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal2.result.then(val => {
                        this.trySave(closeAfterSave, ignoreVoucherCheck, true);
                    });
                });
                return;
            }
        }

        this.save(closeAfterSave);
    }

    private save(closeAfterSave: boolean) {
        this.startSave();

        const keys: string[] = [
            "economy.supplier.payment.withoutinvoice",
            "economy.supplier.payment.successpreliminary",
            "economy.supplier.payment.successpreliminary",
            "economy.supplier.payment.successdefinitive",
            "economy.supplier.payment.successupdated",
            "economy.supplier.payment.voucherscreated",
            "economy.supplier.payment.askPrintVoucher",
            "core.verifyquestion"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var remainingAmount: number = this.payment.remainingAmount - (this.isBaseCurrency ? this.payment.amount : this.payment.amountCurrency);
            var paidAmount: number = this.isBaseCurrency ? this.payment.amount - this.payment.bankFeeCurrency : this.payment.amountCurrency - this.payment.bankFeeCurrency;
            var amountDiff: number = this.isCredit ? this.payment.remainingAmount - paidAmount : paidAmount - this.payment.remainingAmount;
            var ammountDiffCurrency: number = this.isBaseCurrency ? amountDiff : 0;

            const saveDTO = new PaymentRowSaveDTO();
            // Origin
            saveDTO.originId = this.paymentId ? this.paymentId : 0;
            saveDTO.originType = SoeOriginType.SupplierPayment;
            saveDTO.originStatus = this.payment.originStatus && this.payment.originStatus !== SoeOriginStatus.None ? this.payment.originStatus : SoeOriginStatus.Origin;
            saveDTO.originDescription = this.payment.originDescription;
            saveDTO.voucherSeriesId = this.selectedVoucherSeries.voucherSeriesId;
            saveDTO.accountYearId = this.accountYearId;

            // Invoice
            saveDTO.invoiceId = this.invoiceId;
            saveDTO.invoiceType = SoeInvoiceType.SupplierInvoice;
            saveDTO.onlyPayment = true;
            saveDTO.billingType = this.invoice.billingType;
            saveDTO.actorId = this.selectedSupplier.id;
            saveDTO.invoiceNr = terms["economy.supplier.payment.withoutinvoice"];
            saveDTO.invoiceDate = new Date();
            saveDTO.paymentDate = this.selectedPayDate;
            saveDTO.voucherDate = this.selectedPayDate;
            saveDTO.totalAmount = this.payment.amount;
            saveDTO.totalAmountCurrency = this.payment.amountCurrency;
            saveDTO.vatAmount = this.payment.vatAmount;
            saveDTO.vatAmountCurrency = this.payment.vatAmountCurrency;
            saveDTO.currencyId = this.payment.currencyId;
            saveDTO.currencyRate = this.currencyRate;
            saveDTO.currencyDate = this.currencyDate;
            saveDTO.fullyPayed = this.isSupportSuperAdminUnlocked ? this.payment.fullyPaid : (this.payment.fullyPaid || remainingAmount === 0);

            // PaymentRow
            saveDTO.paymentRowId = this.payment.paymentRowId;
            saveDTO.paymentMethodId = this.selectedPaymentMethod.paymentMethodId;
            saveDTO.seqNr = this.seqNr ? this.seqNr : 0;
            saveDTO.sysPaymentTypeId = this.selectedPaymentInfo.sysPaymentTypeId;
            saveDTO.paymentNr = this.selectedPaymentInfo.paymentNr;
            saveDTO.amount = this.payment.amount;
            saveDTO.amountCurrency = this.payment.amountCurrency;
            saveDTO.amountDiff = amountDiff;
            saveDTO.amountDiffCurrency = ammountDiffCurrency;
            saveDTO.hasPendingAmountDiff = false;
            saveDTO.hasPendingBankFee = false;
            saveDTO.text = this.payment.text;


            // Super Support
            saveDTO.isSuperSupportSave = this.isSupportSuperAdminUnlocked;

            var matchCodeId: number = 0;
            if (this.useMatching && this.selectedMatchCode)
                matchCodeId = this.selectedMatchCode.matchCodeId;

            // round rows
            _.forEach(this.payment.paymentAccountRows, (r) => {
                r.amount = r.amount.round(2);
                r.amountCurrency = r.amountCurrency.round(2);
                r.amountEntCurrency = r.amountEntCurrency.round(2);
                r.amountLedgerCurrency = r.amountLedgerCurrency.round(2);
                r.creditAmount = r.creditAmount.round(2);
                r.creditAmountCurrency = r.creditAmountCurrency.round(2);
                r.creditAmountEntCurrency = r.creditAmountEntCurrency.round(2);
                r.creditAmountLedgerCurrency = r.creditAmountLedgerCurrency.round(2);
                r.debitAmount = r.debitAmount.round(2);
                r.debitAmountCurrency = r.debitAmountCurrency.round(2);
                r.debitAmountEntCurrency = r.debitAmountEntCurrency.round(2);
                r.debitAmountLedgerCurrency = r.debitAmountLedgerCurrency.round(2);
            });

            this.supplierService.saveSupplierPayment(saveDTO, this.payment.paymentAccountRows, matchCodeId).then((result) => {
                if (result.success) {
                    if (this.autoTransferPaymentToVoucher) {
                        this.accountingService.calculateAccountBalanceForAccountsFromVoucher(this.accountYearId).then((result) => {
                        });
                    }

                    if (this.isNew) {
                        // Set payment id to be able to reload it
                        if (result.integerValue && result.integerValue > 0)
                            this.paymentId = result.integerValue;
                        // Set sequence number to update the tab header
                        if (result.value)
                            this.seqNr = result.value;
                    }

                    var message = this.isNew ? (!this.seqNr ? terms["economy.supplier.payment.successpreliminary"] : terms["economy.supplier.payment.successdefinitive"].format(this.seqNr.toString())) : terms["economy.supplier.payment.successupdated"];
                    this.completedSave(this.payment, false, message);

                    if (this.askPrintVoucher && (result.idDict && result.idDict.length > 0)) {
                        // Get keys
                        var voucherIds: number[] = []
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

                        var message: string = terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + terms["economy.supplier.payment.askPrintVoucher"];
                        var modal = this.notificationService.showDialog(terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val != null && val === true) {
                                this.printVouchers(voucherIds);
                            };
                        });
                    }

                    if (closeAfterSave) {
                        this.closeMe(true);
                    }
                    else {
                        if (!this.isNew) {
                            this.invoiceId = undefined;
                            this.load();
                        }
                        else
                            this.new();
                    }
                } else {
                    this.failedSave(result.errorMessage);
                }
            }, error => {
                this.failedSave(error.message);
            });
        });
    }

    private showTimeDiscountDialog() {

        //Only for finnish syscountryid
        if (CoreUtility.sysCountryId == TermGroup_Languages.Finnish) {
            if (this.invoice === undefined) return; //if called before the invoice is loaded
            if (this.invoice != null && this.useTimeDiscount && this.invoice.timeDiscountDate != null && this.invoice.timeDiscountPercent != null && this.invoice.timeDiscountPercent != 0) {
                if (CalendarUtility.convertToDate(this.payment.payDate) <= CalendarUtility.convertToDate(this.invoice.timeDiscountDate)) {
                    const keys: string[] = [
                        "core.controlquestion",
                        "economy.supplier.invoice.useitmediscountquestion"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        var message = terms["economy.supplier.invoice.useitmediscountquestion"];
                        var modal = this.notificationService.showDialog(terms["core.controlquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                        modal.result.then(val => {
                            this.payment.fullyPaid = true;
                            this.useMatching = false;

                            var paidAmount: any = 0;
                            var remainingAmount: any = 0;

                            paidAmount = this.payment.remainingAmount;
                            remainingAmount = this.payment.remainingAmount;

                            this.payment.amount = this.payment.amountCurrency = paidAmount - (paidAmount * this.invoice.timeDiscountPercent / 100);
                            this.generateAccountingRows();

                            this.preAmount = this.payment.amount;
                            this.preAmountCurrency = this.payment.amountCurrency

                        }, (reason) => {
                            this.generateAccountingRows();

                        });
                    });
                }
                else {
                    this.generateAccountingRows();
                }
            }
        }
    }

    protected initRevoke() {
        // Show verification dialog
        const keys: string[] = [
            "core.verifyquestion",
            "economy.supplier.payment.revokequestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.payment.revokequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {
                    this.startDelete();
                    this.performDelete();
                }
            });
        });
    }

    private performDelete() {
        this.supplierService.cancelPayment(this.payment.paymentRowId, this.payment.voucherHeadId > 0).then((result) => {
            if (result.success) {
                this.completedDelete(this.payment,true);
                this.closeMe(true);
            }
            else {
                this.failedDelete(result.errorMessage);
            }
        }, error => {
            this.failedDelete(error.message);
        });
    }

    // HELP-METHODS

    private calculateVatAmount(forceContractor: boolean = false) {
        // Calculate VAT amount based on vat percent
        var vatAmount: number = 0;
        var vatRateValue: number = this.vatRate / 100;

        if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || forceContractor)
            vatAmount = this.invoice.totalAmountCurrency * vatRateValue;
        else
            vatAmount = this.invoice.totalAmountCurrency * (1 - (1 / (vatRateValue + 1)));

        this.invoice.vatAmountCurrency = vatAmount.round(2);
    }

    private hasModifiedRows(): boolean {
        var modified: boolean = false;
        _.forEach(this.payment.paymentAccountRows, (row) => {
            if (row.isModified)
                modified = true;
        });

        return modified;
    }

    private setLocked() {
        const isPayed = this.payment.status === SoePaymentStatus.Checked;
        const isDraft = this.payment.status === SoeOriginStatus.Draft;
        const isPendingPayment = this.payment.status === SoePaymentStatus.Pending;
        const hasVoucher = this.payment.voucherHeadId != null && this.payment.voucherHeadId > 0;
        const isCancelled = this.payment.status == SoePaymentStatus.Cancel;

        this.locked = (this.payment.paymentRowId > 0 && (isPayed || hasVoucher || this.payment.isSuggestion || isCancelled));
        this.enableRevoke = (this.payment.paymentRowId > 0 && !this.payment.isSuggestion && !isCancelled && this.payment.sysPaymentTypeId != TermGroup_SysPaymentType.Autogiro);

        //Super support override
        this.locked = this.isSupportSuperAdminUnlocked ? false : this.locked;
    }

    private unlock() {

        if (CoreUtility.isSupportSuperAdmin) {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.invoice.supersupportunlockaccountingrows"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var message = terms["economy.supplier.invoice.supersupportunlockaccountingrows"];
                var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.isSupportSuperAdminUnlocked = true;
                }, (reason) => {
                    this.isSupportSuperAdminUnlocked = false;
                });
            });
        }
        else {
            if (CoreUtility.isSupportAdmin)
                this.isSupportUnlocked = true;
        }
    }

    private printVouchers(ids: number[]) {
        const keys: string[] = [
            "core.warning",
            "economy.supplier.payment.defaultVoucherListMissing"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (this.supplierPaymentVoucherReportId) {

                this.requestReportService.printVoucherList(ids);
            }
            else {
                //Show messagebox
                this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            }
        });
    }

    private showRevokeDisabledMessage() {
        const keys: string[] = [
            "core.warning",
            "economy.supplier.payment.revokedisabled"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.payment.revokedisabled"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        });
    }

    // VALIDATION

    protected validate() {
        const errors = this['edit'].$error;
        console.log("errors", errors)
        // Mandatory fields
        if (this.payment) {
            if (!this.selectedSupplier)
                this.mandatoryFieldKeys.push("economy.supplier.supplier.supplier");
            if (!this.selectedVoucherSeries)
                this.mandatoryFieldKeys.push("economy.supplier.invoice.voucherseries");
            if (!this.selectedPaymentMethod)
                this.mandatoryFieldKeys.push("economy.supplier.payment.paymentmethod");
            if (!this.selectedPaymentInfo)
                this.mandatoryFieldKeys.push("economy.supplier.invoice.paytoaccount");
            if (!this.payment.payDate)
                this.mandatoryFieldKeys.push("economy.supplier.payment.paymentdate"); 
            if (!this.payment.invoiceId)
                this.mandatoryFieldKeys.push("economy.supplier.invoice.ocr");
        }

        // Account year
        if (errors['accountYearStatus'])
            this.validationErrorKeys.push("economy.accounting.voucher.accountyearclosed");

        // Account period
        if (errors['accountPeriod'])
            this.validationErrorKeys.push("economy.accounting.voucher.accountperiodmissing");
        if (errors['accountPeriodStatus'])
            this.validationErrorKeys.push("economy.accounting.voucher.accountperiodclosed");

        // Voucher series
        if (errors['defaultVoucherSeries'])
            this.validationErrorKeys.push("economy.accounting.voucher.defaultvoucherseriesmissing");

        // Accounting row validation
        if (errors['accountStandard'])
            this.validationErrorKeys.push("economy.accounting.voucher.accountstandardmissing");
        if (errors['accountInternal'])
            this.validationErrorKeys.push("economy.accounting.voucher.accountinternalmissing");
        if (errors['rowAmount'])
            this.validationErrorKeys.push("economy.accounting.voucher.invalidrowamount");
        if (errors['amountDiff'])
            this.validationErrorKeys.push("economy.accounting.voucher.unbalancedrows");

        if (errors['rowAmounts'])
            this.validationErrorKeys.push("economy.supplier.payment.rowamountmismatch");
    }

    public isDisabled() {
        return !this.isDirty || this.edit.$invalid;
    }
}