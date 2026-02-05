import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ICommonCustomerService } from "../CommonCustomerService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CustomerInvoiceDTO } from "../../Models/InvoiceDTO";
import { CustomerDTO } from "../../Models/CustomerDTO";
import { CustomerInvoiceEditSaveFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../Util/Enumerations";
import { CustomerInvoiceRowDTO } from "../../Models/CustomerInvoiceRowDTO";
import { CustomerLedgerSaveDTO } from "../../Models/CustomerLedgerSaveDTO";
import { AccountingRowDTO } from "../../Models/AccountingRowDTO";
import { TermGroup_InvoiceVatType, TermGroup_BillingType, SoeOriginStatus, Feature, AccountingRowType, CompanySettingType, TermGroup, TermGroup_Languages, TermGroup_CurrencyType, CustomerAccountType, SoeEntityState, SoeEntityType, SoeEntityImageType, UserSettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditController as CustomerEditController } from "../../../Common/Customer/Customers/EditController";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { FilesHelper } from "../../Files/FilesHelper";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { InvoiceEditHandler } from "../../../Shared/Billing/Helpers/InvoiceEditHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { FileUploadDTO } from "../../Models/FileUploadDTO";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { VatCodeDTO } from "../../Models/VatCodeDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { AccordionSettingsController } from "../../Dialogs/AccordionSettings/AccordionSettingsController";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    // Helpers
    private invoiceFilesHelper: FilesHelper;
    private invoiceEditHandler: InvoiceEditHandler;

    public dirtyHandler: IDirtyHandler;

    public customerInvoiceId: number;
    // Config
    currentAccountYearId = 0;

    // Permissions        
    useCurrency = false;
    reportPermission = false;
    editCustomerPermission = false;
    editCustomerInvoicePermission = false;
    tracingTabPermission = false;
    draftToOriginPermission = false;
    unlockAccountingRowsPermission = false;
    extendedInvoiceEditPermission = false;
    importFromAutomasterPermission = false;
    private filesPermission = false;

    // Company settings
    copyInvoiceNr = false;
    customerInvoiceTransferToVoucher = false;
    customerInvoiceAskPrintVoucherOnTransfer = false;
    defaultVatType: TermGroup_InvoiceVatType = TermGroup_InvoiceVatType.Merchandise;
    defaultPaymentConditionId = 0;
    defaultPaymentConditionDays = 0;
    defaultPaymentConditionStartOfNextMonth = false;
    private defaultVatCodeId: number;

    discountDays: number = null;
    discountPercent = 0;
    paymentConditionDays = 0;
    paymentConditionStartOfNextMonth = false;

    defaultVoucherSeriesTypeId = 0;
    defaultDraft = false;
    allowEditOrigin = false;
    showTransactionCurrency = false;
    showEnterpriseCurrency = false;
    showLedgerCurrency = false;
    voucherListReportId = 0;
    allowEditAccountingRows = false;
    useExternalInvoiceNr = false;

    // User settings
    private expanderSettings: any;

    // Company accounts
    defaultCreditAccountId = 0;
    defaultDebitAccountId = 0;
    defaultVatAccountId = 0;
    defaultOutsideEUAccountId = 0;
    defaultWithinEUAccountId = 0;
    reverseVatPurchaseId = 0;
    reverseVatSalesId = 0;
    contractorVatAccountDebitId = 0;
    contractorVatAccountCreditId = 0;
    defaultVatRate = 0;

    // Customer accounts
    customerVatAccountId = 0;

    documentExpanderIsOpen = false;
    invoiceExpanderIsOpen = false;
    accountingRowsExpanderIsOpen = false;
    traceRowsExpanderIsOpen = false;

    vatRate: number = Constants.DEFAULT_VAT_RATE;

    // Lookups 
    customers: ISmallGenericType[];
    billingTypes: any[];
    vatTypes: any[];
    filteredVatTypes: any[]; 
    currencies: any[];
    paymentConditions: any[];
    voucherSeries: any[];
    vatCodes: any[];

    // Data
    invoice: CustomerInvoiceDTO;
    customer: CustomerDTO;
    defaultVatCode: VatCodeDTO;

    private manualCustomerPaymentTransferToVoucher = false;

    // Flags
    loadingInvoice = false;
    invoiceIsLoaded = false;
    private isLocked = false;
    private isLockedVoucherSeries = false;
    isLockedAccountingRows = false;
    ignoreAskZero = false;
    ignoreAskVoucher = false;
    ignoreAskUnbalanced = false;
    private resetDocumentsGridData = false;
    private readOnlyFileUpload = false;

    // Properties
    invoiceAccountYearId = 0;
    accountPeriodId = 0;
    currencyRate = 1;
    currencyDate: Date;
    currencyCode: string;
    baseCurrencyCode: string;
    ledgerCurrencyCode: string;
    isBaseCurrency: boolean;
    isLedgerCurrency: boolean;

    get isCredit(): boolean {
        return this.invoice.billingType === TermGroup_BillingType.Credit;
    }

    private _selectedCustomer;
    get selectedCustomer(): ISmallGenericType {
        return this._selectedCustomer;
    }
    set selectedCustomer(item: ISmallGenericType) {
        this._selectedCustomer = item;
        this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null);
    }

    private _selectedVoucherSeriesId;
    get selectedVoucherSeriesId(): number {
        return this._selectedVoucherSeriesId;
    }
    set selectedVoucherSeriesId(id: number) {
        this._selectedVoucherSeriesId = id;
        this.selectedVoucherSeriesTypeId = this.voucherSeries.find(x => x.voucherSeriesId === id)?.voucherSeriesTypeId;
        if (this.invoice) {
            this.invoice.voucherSeriesId = this._selectedVoucherSeriesId;
        }
    }

    private _selectedVoucherSeriesTypeId;
    get selectedVoucherSeriesTypeId(): number {
        return this._selectedVoucherSeriesTypeId;
    }
    set selectedVoucherSeriesTypeId(id: number) {
        this._selectedVoucherSeriesTypeId = id;
        if (this.invoice) {
            this.invoice.voucherSeriesTypeId = this._selectedVoucherSeriesTypeId;
        }
    }

    private _selecedInvoiceDate: Date;
    get selectedInvoiceDate() {
        return this._selecedInvoiceDate;
    }
    set selectedInvoiceDate(date: Date) {
        this._selecedInvoiceDate = date ? new Date(<any>date.toString()) : null;

        if (this.invoice) {
            this.invoice.invoiceDate = this.selectedInvoiceDate;
            this.selectedVoucherDate = this.selectedInvoiceDate;
            this.setDueDate();
        }
    }

    private _selectedVoucherDate: Date;
    get selectedVoucherDate() {
        return this._selectedVoucherDate;
    }
    set selectedVoucherDate(date: Date) {
        const oldDate = this._selectedVoucherDate;
        this._selectedVoucherDate = date ? new Date(<any>date.toString()) : null;

        if (this.invoice) {
            this.invoice.voucherDate = this.selectedVoucherDate;
            this.invoice.currencyDate = this.currencyDate = this.selectedVoucherDate;
        }

        if (oldDate && oldDate !== this._selectedVoucherDate) {
            this.setVoucherDateOnAccountingRows();
        }
    }

    public draft = false;
    private _isDraft = false;
    get isDraft(): boolean {
        return this.isNew || this.invoice.originStatus === SoeOriginStatus.Draft;
    }

    // Functions
    saveFunctions: any = [];

    private edit: ng.IFormController;
    private modalInstance: any;

    //@ngInject
    constructor(
        private $uibModal,
        private $window,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private accountingService: IAccountingService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        shortCutService: IShortCutService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private readonly requestReportService: IRequestReportService,
        progressHandlerFactory?: IProgressHandlerFactory,
    ) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.load())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        // Config parameters
        this.currentAccountYearId = soeConfig.accountYearId;

        shortCutService.bindSave($scope, () => { this.save(false); });
        shortCutService.bindSaveAndClose($scope, () => { this.save(true); });

        // Events
        this.messagingService.subscribe(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, (x) => {
            this.generateAccountingRows(true);
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_DIALOG, (parentGuid) => {
            if (parentGuid == this.guid) {
                this.$scope.$broadcast('accountDistributionName', this.invoice.invoiceNr + ", " + this.customer.customerNr + " " + this.customer.name);
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_RELOAD_ORDER_IMAGES, (x) => {
            if (x.guid === this.guid && this.invoiceFilesHelper.filesRendered) {
                this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0);
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_UPDATE_DISTRIBUTE_ALL, (x) => {
            if (x.guid === this.guid && this.invoiceFilesHelper.filesRendered) {
                this.invoiceFilesHelper.changeDistributeBatch(x.value);
                this.resetDocumentsGridData = true;
            }
        }, this.$scope);
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.customerInvoiceId = parameters.id || 0;

        this.translationService.translate("common.manuallyadded").then(term => {
            this.invoiceFilesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.CustomerInvoice, SoeEntityImageType.OrderInvoice, () => this.customerInvoiceId, term);
        });

        this.invoiceEditHandler = new InvoiceEditHandler(this, this.coreService, this.commonCustomerService, this.urlHelperService, this.notificationService, this.translationService, this.reportService, this.$uibModal, this.progress, this.messagingService, this.guid);
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([
            { feature: Feature.Economy_Customer_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Preferences_Currency, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Distribution_Reports_Selection, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Distribution_Reports_Selection_Download, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Customer_Customers_Edit, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Billing_Invoice_Invoices_Edit_Tracing, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Customer_Invoice_Invoices_Edit_UnlockAccounting, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Customer_Invoice_Payment_Extended_Rights, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Economy_Import_Invoices_Automaster, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Billing_Invoice_Invoices_Edit_Images, loadReadPermissions: true, loadModifyPermissions: false }
        ]);

    }

    private onDoLookups(): ng.IPromise<any> {

        return this.$q.all([
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadCompanyAccounts(),
            this.loadCurrentAccountYear(),
            this.loadCustomers(true),

            this.loadBillingTypes(),
            this.loadVatTypes(),
            this.loadCurrencies(),
            this.loadPaymentConditions(),
            this.loadVatCodes(),
        ]).then(() => this.$q.all([
            this.loadVoucherSeries(soeConfig.accountYearId)
        ]));

    }


    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit].modifyPermission;

        this.useCurrency = response[Feature.Economy_Preferences_Currency].readPermission;
        this.reportPermission = response[Feature.Economy_Distribution_Reports_Selection].readPermission && response[Feature.Economy_Distribution_Reports_Selection_Download].readPermission;
        this.editCustomerPermission = response[Feature.Economy_Customer_Customers_Edit].readPermission;
        this.tracingTabPermission = response[Feature.Billing_Invoice_Invoices_Edit_Tracing].readPermission;
        this.editCustomerInvoicePermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit].readPermission;
        this.draftToOriginPermission = response[Feature.Economy_Customer_Invoice_Status_DraftToOrigin].readPermission;
        this.unlockAccountingRowsPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit_UnlockAccounting].readPermission;
        this.extendedInvoiceEditPermission = response[Feature.Economy_Customer_Invoice_Payment_Extended_Rights].readPermission;
        this.importFromAutomasterPermission = response[Feature.Economy_Import_Invoices_Automaster].readPermission;
        this.filesPermission = response[Feature.Billing_Invoice_Invoices_Edit_Images].readPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "common.accordionsettings", IconLibrary.FontAwesome, "fa-cog",
            () => { this.updateAccordionSettings(); },
            null,
            null)));

        const keys: string[] = [
            "core.save",
            "core.saveandclose"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions.push({ id: CustomerInvoiceEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)" });
            this.saveFunctions.push({ id: CustomerInvoiceEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] + " (Ctrl+Enter)" });
        });

    }

    private updateAccordionSettings() {

        const keys: string[] = [
            "core.document",
            "common.customer.invoices.accountingrows",
            "common.tracing",
            "billing.invoices.invoice",
            
        ];
        var accordionList: any[] = [];

        this.translationService.translateMany(keys).then((terms) => {
            accordionList.push({ name: "InvoiceExpander", description: terms["billing.invoices.invoice"] });
            accordionList.push({ name: "DocumentExpander", description: terms["core.document"] });
            accordionList.push({ name: "AccountingRowExpander", description: terms["common.customer.invoices.accountingrows"] });
            accordionList.push({ name: "TracingExpander", description: terms["common.tracing"] });
        });

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AccordionSettings/Views/accordionsettings.html"),
            controller: AccordionSettingsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                coreService: () => { return this.coreService },
                userSettingType: () => { return UserSettingType.BillingInvoiceDefaultExpanders },
                accordionList: () => { return accordionList },
                userSliderSettingType: () => { return null }
            }
        });

        modal.result.then(ids => {
            this.loadUserSettings();
        }, function () {
            //Cancelled
        });
    }

    private setupWatchers() {
        // Convert currency amounts
        this.$scope.$watch(() => this.invoice.totalAmountCurrency, () => {
            this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
        });
        this.$scope.$watch(() => this.invoice.vatAmountCurrency, () => {
            this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
        });
        this.$scope.$watch(() => this.invoice.currencyId, () => {
            this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
            this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
        });
        this.$scope.$watch(() => this.ledgerCurrencyCode, () => {
            this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
            this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
        });
        this.$scope.$watch(() => this.invoice.addAttachementsToEInvoice, () => {
            this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice;
        });
    }

    // LOOKUPS
    private load(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.invoiceIsLoaded = false;
        this.readOnlyFileUpload = false;
        if (this.customerInvoiceId > 0) {
            this.loadingInvoice = true;
            return this.commonCustomerService.getCustomerLedger(this.customerInvoiceId).then((x) => {
                this.invoice = x;
                //check status
                if (this.invoice.originStatus == 4) {
                    this.readOnlyFileUpload = true;
                }

                this.isNew = false;

                //Fix vat type
                if (this.invoice.vatType === TermGroup_InvoiceVatType.EU || this.invoice.vatType === TermGroup_InvoiceVatType.NonEU)
                    this.addMissingVatType(this.invoice.vatType);

                // Fix dates
                this.invoice.invoiceDate = CalendarUtility.convertToDate(this.invoice.invoiceDate);
                this.invoice.dueDate = CalendarUtility.convertToDate(this.invoice.dueDate);
                this.invoice.voucherDate = CalendarUtility.convertToDate(this.invoice.voucherDate);
                this.invoice.currencyDate = CalendarUtility.convertToDate(this.invoice.currencyDate);

                this.selectedCustomer = _.find(this.customers, { id: this.invoice.actorId });
                this._selecedInvoiceDate = this.invoice.invoiceDate;
                this._selectedVoucherDate = this.invoice.voucherDate;

                this.selectedVoucherSeriesId = this.invoice.voucherSeriesId;
                this.selectedVoucherSeriesTypeId = this.invoice.voucherSeriesTypeId;
                this.currencyRate = this.invoice.currencyRate;
                this.currencyDate = this.invoice.currencyDate;
                this.draft = (this.invoice.originStatus === SoeOriginStatus.Draft);

                if (this.invoice.vatType.valueOf() === TermGroup_InvoiceVatType.Contractor ||
                    this.invoice.vatType.valueOf() === TermGroup_InvoiceVatType.NoVat ||
                    this.invoice.vatType.valueOf() === TermGroup_InvoiceVatType.ExportOutsideEU) {
                    this.invoice.vatAmount = 0;
                    this.invoice.vatAmountCurrency = 0;
                }
                this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice;

                const accountingRows = CustomerInvoiceRowDTO.toAccountingRowDTOs(this.invoice.customerInvoiceRows);
                this.invoice.accountingRows = _.orderBy(accountingRows.filter(x => x.type === AccountingRowType.AccountingRow), 'rowNr');

                this.invoiceFilesHelper.reset();
                if (this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon))
                    this.invoiceFilesHelper.nbrOfFiles = '*';

                this.dirtyHandler.isDirty = false;
                this.dirtyHandler.clean();
                this.setLocked();

                if (this.documentExpanderIsOpen) { 
                    this.invoiceFilesHelper.loadFiles(true, this.invoice?.projectId ? this.invoice.projectId : 0);
                    this.resetDocumentsGridData = true;
                }

                deferral.resolve();
            });
        }
        else {
            this.new();
            deferral.resolve();
            this.invoiceLoaded();
        }

        return deferral.promise;
    }

    private invoiceLoaded() {
        this.loadingInvoice = false;
        this.invoiceIsLoaded = true;
        this.setupWatchers();
    }



    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.BillingCopyInvoiceNrToOcr);
        settingTypes.push(CompanySettingType.CustomerInvoiceTransferToVoucher);
        settingTypes.push(CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.CustomerInvoiceDefaultVatType);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentCondition);
        settingTypes.push(CompanySettingType.CustomerInvoiceVoucherSeriesType);
        settingTypes.push(CompanySettingType.CustomerInvoiceDefaultDraft);
        settingTypes.push(CompanySettingType.CustomerInvoiceAllowEditOrigin);
        settingTypes.push(CompanySettingType.CustomerShowTransactionCurrency);
        settingTypes.push(CompanySettingType.CustomerShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.CustomerShowLedgerCurrency);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.CustomerPaymentManualTransferToVoucher);
        settingTypes.push(CompanySettingType.CustomerInvoiceAllowEditAccountingRows);
        settingTypes.push(CompanySettingType.BillingUseExternalInvoiceNr);
        settingTypes.push(CompanySettingType.AccountingDefaultVatCode);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.copyInvoiceNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCopyInvoiceNrToOcr);
            this.customerInvoiceTransferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceTransferToVoucher);
            this.customerInvoiceAskPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
            this.defaultVatType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceDefaultVatType, this.defaultVatType);
            this.defaultPaymentConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentCondition);
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceVoucherSeriesType);
            this.defaultDraft = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceDefaultDraft);
            this.allowEditOrigin = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAllowEditOrigin);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowLedgerCurrency);
            this.voucherListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVoucherList);
            this.manualCustomerPaymentTransferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentManualTransferToVoucher);
            this.allowEditAccountingRows = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAllowEditAccountingRows);
            this.useExternalInvoiceNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseExternalInvoiceNr);
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVatCode);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingInvoiceDefaultExpanders,
        ];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.expanderSettings = x[UserSettingType.BillingInvoiceDefaultExpanders];
            this.handleExpanderSettings();
        });
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.AccountCustomerSalesVat,
            CompanySettingType.AccountCustomerClaim,
            CompanySettingType.AccountCommonVatPayable1,
            CompanySettingType.AccountCommonReverseVatPurchase,
            CompanySettingType.AccountCommonReverseVatSales,
            CompanySettingType.AccountCommonVatPayable1Reversed,
            CompanySettingType.AccountCommonVatReceivableReversed,
            CompanySettingType.AccountCustomerSalesOutsideEU,
            CompanySettingType.AccountCustomerSalesWithinEU,
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesVat);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1);
            this.reverseVatPurchaseId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatPurchase);
            this.reverseVatSalesId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatSales);
            this.contractorVatAccountCreditId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1Reversed);
            this.contractorVatAccountDebitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivableReversed);
            this.defaultWithinEUAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesWithinEU);
            this.defaultOutsideEUAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesOutsideEU);

            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId);
        });
    }

    private loadCustomers(useCache: boolean): ng.IPromise<any> {
        return this.commonCustomerService.getCustomersDict(true, true, useCache).then((x: ISmallGenericType[]) => {
            this.customers = x;
        });
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then(x => {
            this.billingTypes = x;
        });
    }

    private loadVatTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, false, false).then(x => {
            this.vatTypes = x;
            this.filteredVatTypes = _.filter(x, (y) => y.id !== 5 && y.id != 6);
            if (this.defaultVatType === TermGroup_InvoiceVatType.None && this.vatTypes.length > 0) {
                if (_.includes(this.vatTypes, TermGroup_InvoiceVatType.Merchandise))
                    this.defaultVatType = TermGroup_InvoiceVatType.Merchandise;
                else
                    this.defaultVatType = this.vatTypes[0].id;
            }
        });
    }

    public addMissingVatType(vatType: TermGroup_InvoiceVatType) {
        this.filteredVatTypes.push(_.find(this.vatTypes, { 'id': vatType }));
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private loadPaymentConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentConditions(false).then(x => {
            this.paymentConditions = x;

            //Get default number of days (or use 30 if not specified)
            var def = _.find(this.paymentConditions, { paymentConditionId: this.defaultPaymentConditionId });
            this.defaultPaymentConditionDays = def ? def.days : 30;
            this.defaultPaymentConditionStartOfNextMonth = def ? def.startOfNextMonth : 30;
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.commonCustomerService.getVoucherSeriesByYear(accountYearId, false).then((x) => {
            this.voucherSeries = x;
        });
    }

    private loadCurrentAccountYear(): ng.IPromise<any> {
        return this.coreService.getCurrentAccountYear().then((x) => {
            if (x) {
                this.currentAccountYearId = x.accountYearId;
            }
        });
    }

    private loadAccountYear(date: Date) {
        let prevAccountYearId = this.invoiceAccountYearId;

        this.commonCustomerService.getAccountYearId(date).then((id: number) => {
            this.invoiceAccountYearId = id;
            if (this.invoiceAccountYearId !== this.currentAccountYearId || this.invoiceAccountYearId !== prevAccountYearId) {
                //If account year has changed, load voucher series for new year
                this.loadVoucherSeries(this.invoiceAccountYearId);
                this.loadAccountPeriod(this.invoiceAccountYearId);
            } else {
                this.loadAccountPeriod(this.currentAccountYearId);
            }
        });
    }

    private loadAccountPeriod(accountYearId: number) {
        if (!this.invoice || !this.invoice.voucherDate)
            return;

        this.commonCustomerService.getAccountPeriodId(accountYearId, this.invoice.voucherDate).then((id: number) => {
            this.accountPeriodId = id;
        });
    }

    private loadVatCodes(): ng.IPromise<any> {
        return this.commonCustomerService.getVatCodes().then(x => {
            this.vatCodes = x;
            // Insert empty row
            this.vatCodes.splice(0, 0, { vatCodeId: 0, name: '', percent: 0 });
        });
    }

    private loadVatRate(accountId: number) {
        if (accountId === 0) {
            this.setDefaultVatRate();
            return;
        }

        this.commonCustomerService.getAccountSysVatRate(accountId).then(x => {
            this.defaultVatRate = x;
            this.setDefaultVatRate();
        });
    }

    private setVatRate() {
        let vatCode;
        if (this.invoice.vatCodeId && this.invoice.vatCodeId !== 0)
            vatCode = _.find(this.vatCodes, { vatCodeId: this.invoice.vatCodeId });
        else if (this.defaultVatCodeId)
            vatCode = _.find(this.vatCodes, { vatCodeId: this.defaultVatCodeId });

        if (vatCode) {
            this.vatRate = vatCode.percent;
            if (this.invoice.vatCodeId !== vatCode.vatCodeId && this.invoice.vatType === TermGroup_InvoiceVatType.Merchandise)
                this.invoice.vatCodeId = vatCode.vatCodeId;
        }
        else
            this.setDefaultVatRate();
    }

    private setDefaultVatRate() {
        if (this.defaultVatRate === 0)
            this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;

        this.vatRate = this.defaultVatRate;
    }

    private loadCustomer(customerId: number) {
        if (!customerId) {
            this.customer = null;
            this.customerChanged();
        } else {
            this.commonCustomerService.getCustomer(customerId, false, true, false, false, false, false).then(x => {
                this.customer = x;
                this.customerChanged();
                this.loadingInvoice = false;
            });
        }
    }

    // EVENTS
    private billingTypeChanging(oldValue) {
        // Only show warning if amount is entered and user has manually modified any row
        if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows()) {
            const keys: string[] = [
                "core.warning",
                "common.customer.invoices.billingtypechangewarning"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.billingtypechangewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.changeBillingType();
                }, (reason) => {
                    // User cancelled, revoke to previous billing type
                    this.invoice.billingType = oldValue;
                });
            });
        } else {
            this.changeBillingType();
        }
    }

    private changeBillingType() {
        // Switch sign on total amount
        this.$timeout(() => {
            if ((this.isCredit && this.invoice.totalAmountCurrency > 0) || (!this.isCredit && this.invoice.totalAmountCurrency < 0))
                this.invoice.totalAmountCurrency = -this.invoice.totalAmountCurrency;

            this.generateAccountingRows(true);
        });
    }

    private vatTypeChanging(oldValue) {
        // Only show warning if amount is entered and user has manually modified any row
        if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows()) {
            const keys: string[] = [
                "core.warning",
                "common.customer.invoices.vattypechangewarning"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.vattypechangewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.changeVatType();
                }, (reason) => {
                    // User cancelled, revoke to previous vat type
                    this.invoice.vatType = oldValue;
                });
            });
        } else {
            this.changeVatType();
        }
    }

    private changeVatType() {
        this.$timeout(() => {
            if (this.invoice.vatType === TermGroup_InvoiceVatType.NoVat || this.invoice.vatType === TermGroup_InvoiceVatType.ExportOutsideEU || this.invoice.vatType === TermGroup_InvoiceVatType.ExportWithinEU)
                this.invoice.vatCodeId = null;

            this.setVatRate();
            this.generateAccountingRows(true);
        });
    }

    private vatCodeChanging(oldValue) {
        // Only show warning if amount is entered and user has manually modified any row
        if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows()) {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.invoice.vatcodechangewarning"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.invoice.vatcodechangewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.changeVatCode();
                }, (reason) => {
                    // User cancelled, revoke to previous vat type
                    this.invoice.vatCodeId = oldValue;
                });
            });
        } else {
            this.changeVatCode();
        }
    }

    private changeVatCode() {
        this.$timeout(() => {
            this.setVatRate();
            this.generateAccountingRows(true);
        });
    }

    private customerChanged() {
        // Set customer dependant values
        if (!this.loadingInvoice) {
            this.invoice.actorId = this.customer ? this.customer.actorCustomerId : 0;

            // VAT
            this.invoice.vatType = this.customer && this.customer.vatType !== TermGroup_InvoiceVatType.None ? this.customer.vatType : this.defaultVatType;
            this.setVatRate();

            this.setPaymentCondition(this.customer && this.customer.paymentConditionId ? this.customer.paymentConditionId : 0);
            this.setDueDate();

            // Reference
            this.invoice.referenceYour = this.customer && this.customer.invoiceReference ? this.customer.invoiceReference : '';

            // Note
            if (this.customer && this.customer.showNote && this.customer.note) {
                this.showCustomerNote(this.customer.note);
            }

            // Attachments
            this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice = this.customer && this.customer.addAttachementsToEInvoice ? this.customer.addAttachementsToEInvoice : false;
            this.invoiceFilesHelper.addSupplierInvoicesToEInvoice = this.invoice.addSupplierInvoicesToEInvoice = this.customer && this.customer.addSupplierInvoicesToEInvoice ? this.customer.addSupplierInvoicesToEInvoice : false;


            // Accounting rows
            this.generateAccountingRows(true);
        }

        this.invoiceLoaded();
    }

    private amountChanged(id: string) {
        this.$timeout(() => {
            if (id === 'total') {
                var totalAmount = this.invoice.totalAmountCurrency;
                if (totalAmount < 0) {
                    // If a negative total amount is entered, change billing type to credit.
                    if (!this.isCredit)
                        this.invoice.billingType = TermGroup_BillingType.Credit;
                }
                else if (totalAmount > 0 && this.isCredit) {
                    // If a positive total amount is entered for a credit invoice, negate the amount
                    totalAmount = -totalAmount;
                    this.invoice.totalAmountCurrency = totalAmount;
                }

                this.calculateVatAmount();
            }

            this.generateAccountingRows(id === 'total');
        });
    }

    private convertAmount(field: string, amount: number) {
        if (this.loadingInvoice)
            return;

        // Call amount currency converter in accounting rows directive
        const item = {
            field: field,
            amount: amount,
            sourceCurrencyType: TermGroup_CurrencyType.TransactionCurrency
        };
        this.$scope.$broadcast('amountChanged', item);
    }

    private amountConverted(item) {
        if (item.parentRecordId === this.invoice.invoiceId) {
            // Result from amount currency converter in accounting rows directive
            this.invoice[item.field] = item.baseCurrencyAmount;
            this.invoice[item.field + 'EnterpriceCurrency'] = item.enterpriseCurrencyAmount;
            this.invoice[item.field + 'LedgerCurrency'] = item.ledgerCurrencyAmount;
        }
    }

    // ACTIONS

    private executeSaveFunction(option) {
        switch (option.id) {
            case CustomerInvoiceEditSaveFunctions.Save:
                this.initSave(false);
                break;
            case CustomerInvoiceEditSaveFunctions.SaveAndClose:
                this.initSave(true);
                break;
        }
    }

    private initSave(closeAfterSave: boolean) {
        this.$scope.$broadcast('stopEditing', {
            functionComplete: () => {
                this.save(closeAfterSave)
            }
        });
    }
    private save(closeAfterSave: boolean) {

        const keys: string[] = [
            "core.verifyquestion",
            "economy.supplier.payment.askPrintVoucher",
            "economy.supplier.invoice.successpreliminary",
            "economy.supplier.invoice.successdefinitive",
            "economy.supplier.invoice.successupdated",
            "common.customer.invoices.unbalancedamounts",
            "common.customer.invoices.asktransfervoucher",
            "economy.supplier.payment.voucherscreated",
            "economy.supplier.invoice.zeroamountvarning"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            if (!this.ignoreAskZero) {
                if (this.askSaveZeroAmount(terms, closeAfterSave))
                    return;
            }

            if (!this.ignoreAskVoucher) {
                if (this.askTransferToVoucher(terms, closeAfterSave))
                    return;
            }

            if (!this.ignoreAskUnbalanced) {
                if (this.askSaveUnbalanced(terms, closeAfterSave))
                    return;
            }

            this.invoice.currencyRate = this.currencyRate;

            const customerLedgerSaveDTO = new CustomerLedgerSaveDTO();

            if (!this.invoice.originStatus || this.invoice.originStatus == SoeOriginStatus.Draft || this.invoice.originStatus == SoeOriginStatus.Origin)
                this.invoice.originStatus = this.draft ? SoeOriginStatus.Draft : SoeOriginStatus.Origin;

            customerLedgerSaveDTO.originStatus = this.invoice.originStatus;
            customerLedgerSaveDTO.voucherSeriesId = this.selectedVoucherSeriesId;
            customerLedgerSaveDTO.voucherSeriesTypeId = this.selectedVoucherSeriesTypeId;
            customerLedgerSaveDTO.originDescription = this.invoice.originDescription;
            customerLedgerSaveDTO.invoiceId = this.customerInvoiceId;
            customerLedgerSaveDTO.billingType = this.invoice.billingType;
            customerLedgerSaveDTO.vatType = this.invoice.vatType;
            customerLedgerSaveDTO.vatCodeId = this.invoice.vatCodeId;
            customerLedgerSaveDTO.actorId = this.invoice.actorId;
            customerLedgerSaveDTO.seqNr = this.invoice.seqNr;
            customerLedgerSaveDTO.invoiceNr = this.invoice.invoiceNr;
            customerLedgerSaveDTO.ocr = this.invoice.ocr;
            customerLedgerSaveDTO.invoiceDate = this.invoice.invoiceDate;
            customerLedgerSaveDTO.dueDate = this.invoice.dueDate;
            customerLedgerSaveDTO.voucherDate = this.invoice.voucherDate;
            customerLedgerSaveDTO.currencyId = this.invoice.currencyId;
            customerLedgerSaveDTO.currencyDate = this.invoice.currencyDate;
            customerLedgerSaveDTO.currencyRate = this.invoice.currencyRate;
            customerLedgerSaveDTO.referenceYour = this.invoice.referenceYour;
            customerLedgerSaveDTO.totalAmount = this.invoice.totalAmount;
            customerLedgerSaveDTO.totalAmountCurrency = this.invoice.totalAmountCurrency;
            customerLedgerSaveDTO.vatAmount = this.invoice.vatAmount;
            customerLedgerSaveDTO.vatAmountCurrency = this.invoice.vatAmountCurrency;
            customerLedgerSaveDTO.paidAmount = this.invoice.paidAmount;
            customerLedgerSaveDTO.paidAmountCurrency = this.invoice.paidAmountCurrency;
            customerLedgerSaveDTO.remainingAmount = this.invoice.remainingAmount ?? 0;
            customerLedgerSaveDTO.fullyPayed = this.invoice.fullyPayed;
            customerLedgerSaveDTO.sysPaymentTypeId = 0;
            customerLedgerSaveDTO.paymentNr = "";
            customerLedgerSaveDTO.sumAmount = this.invoice.sumAmount;
            customerLedgerSaveDTO.sumAmountCurrency = this.invoice.sumAmountCurrency;
            customerLedgerSaveDTO.invoiceText = this.invoice.invoiceText;
            customerLedgerSaveDTO.paymentConditionId = this.invoice.paymentConditionId;
            customerLedgerSaveDTO.originDescription = this.invoice.originDescription;

            const filesDto: FileUploadDTO[] = this.invoiceFilesHelper.getAsDTOs(true);

            this.progress.startSaveProgress((completion) => {
                this.commonCustomerService.saveCustomerLedger(customerLedgerSaveDTO, filesDto, this.invoice.accountingRows).then((result) => {
                    if (result.success) {
                        if (this.manualCustomerPaymentTransferToVoucher) {
                            this.commonCustomerService.CalculateAccountBalanceForAccountsFromVoucher(this.currentAccountYearId).then((result) => {
                                if (result.success) {
                                }
                            });

                            if (this.customerInvoiceAskPrintVoucherOnTransfer && result.idDict) {
                                // Get values
                                let first: boolean = true;
                                let voucherNrs: string = "";
                                _.forEach(result.idDict, (pair) => {
                                    if (!first)
                                        voucherNrs = voucherNrs + ", ";
                                    else
                                        first = false;
                                    voucherNrs = voucherNrs + pair;
                                });

                                const messageVoucherCreated = terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + terms["economy.supplier.payment.askPrintVoucher"];
                                const modal = this.notificationService.showDialog(terms["core.verifyquestion"], messageVoucherCreated, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                                modal.result.then(val => {
                                    if (val != null && val === true) {
                                        this.printVouchers(result.idDict);
                                    };
                                });
                            }
                        }

                        if (this.isNew && !closeAfterSave) {
                            // Set invoice id to be able to reload it
                            if (result.integerValue && result.integerValue > 0)
                                this.customerInvoiceId = result.integerValue;
                            // Set sequence number to update the tab header
                            if (result.value)
                                this.invoice.seqNr = result.value;
                        }

                        const message: string = this.isNew ? (!this.invoice.seqNr ? terms["economy.supplier.invoice.successpreliminary"] : terms["economy.supplier.invoice.successdefinitive"].format(this.invoice.seqNr.toString())) : terms["economy.invoice.payment.successupdated"];

                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.invoice, false, message);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, this.guid).then(data => {

                if (closeAfterSave) {
                    this.dirtyHandler.clean();
                    this.closeMe(true);
                }
                else
                    this.load();
            }, error => {

            });
            this.ignoreAskZero = false;
            this.ignoreAskUnbalanced = false;
            this.ignoreAskVoucher = false;
        });
    }

    private askSaveZeroAmount(terms: { [index: string]: string }, closeAfterSave: boolean): boolean {

        if (this.invoice.totalAmountCurrency === 0) {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.invoice.zeroamountvarning"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val != null && val === true) {
                    this.ignoreAskZero = true;
                    this.save(closeAfterSave);
                }
            });
            return true;
        }
        else {
            return false;
        }
    }

    private askTransferToVoucher(terms: { [index: string]: string; }, closeAfterSave: boolean): boolean {
        if (this.draft === false && this.customerInvoiceTransferToVoucher) {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.customer.invoices.asktransfervoucher"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val != null && val === true) {
                    this.ignoreAskVoucher = true;
                    this.save(closeAfterSave);
                };
            });
            return true;
        }
        else {
            return false;
        }
    }

    private askSaveUnbalanced(terms: { [index: string]: string; }, closeAfterSave: boolean): boolean {
        var accountingRowsTotalAmount: number = 0;
        if (this.invoice.billingType === TermGroup_BillingType.Credit) {
            accountingRowsTotalAmount = _.sumBy(_.filter(this.invoice.accountingRows, x => x.isDebitRow === true), r => r.debitAmountCurrency);
            accountingRowsTotalAmount = (accountingRowsTotalAmount * -1);
        }
        else {
            accountingRowsTotalAmount = _.sumBy(_.filter(this.invoice.accountingRows, x => x.isCreditRow === true), r => r.creditAmountCurrency);
        }
        if (accountingRowsTotalAmount != this.invoice.totalAmountCurrency) {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.customer.invoices.unbalancedamounts"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val != null && val === true) {
                    this.ignoreAskUnbalanced = true;
                    this.save(closeAfterSave);
                };
            });
            return true;
        }
        else {
            return false;
        }
    }

    protected initDelete() {
        this.performDelete(false);
    }

    private performDelete(copy: boolean) {

        this.progress.startDeleteProgress((completion) => {

            this.commonCustomerService.deleteCustomerInvoice(this.invoice.invoiceId).then((result) => {
                if (result.success) {
                    completion.completed(this.invoice);

                    if (copy)
                        this.copy();
                    else
                        this.closeMe(true);

                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private handleExpanderSettings() {
        if (this.expanderSettings) {
            const settings = this.expanderSettings.split(";");
            this.invoiceExpanderIsOpen = this.isNew || _.includes(settings, 'InvoiceExpander');
            this.documentExpanderIsOpen = _.includes(settings, 'DocumentExpander');
            this.accountingRowsExpanderIsOpen = _.includes(settings, 'AccountingRowExpander');
            this.traceRowsExpanderIsOpen = _.includes(settings, 'TracingExpander');
        }
    }


    private handleError(error) {
        this.notificationService.showDialog(error.errorMessage, error.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
    }

    protected copy() {
        this.isNew = true;
        this.customerInvoiceId = 0;
        this.invoice.invoiceId = 0;
        this.invoice.seqNr = null;
        this.draft = this.defaultDraft;

        if (this.invoiceEditHandler && this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon)) {
            const keys: string[] = [
                "common.customer.invoices.copyattachmentsheader",
                "common.customer.invoices.copyattachmentstext"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const dialog = this.notificationService.showDialog(terms["common.customer.invoices.copyattachmentsheader"], terms["common.customer.invoices.copyattachmentstext"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                dialog.result.then(val => {
                    if (val) {
                        if (!this.documentExpanderIsOpen) {
                            var filesWatch = this.$scope.$watch(() => this.invoiceFilesHelper.filesLoaded, (newValue, oldValue) => {
                                if (newValue) {
                                    filesWatch();
                                }
                            });
                            this.documentExpanderIsOpen = true;
                        }
                    }
                    else {
                        this.invoiceFilesHelper.files = [];
                    }
                    _.forEach(this.invoiceFilesHelper.files, (f) => {
                        f.isModified = true;
                    });
                });
            });
        }

        // TODO: AccountingRowsDataGrid.ClearRowIds();

        this.messagingService.publish(Constants.EVENT_EDIT_NEW, this.guid);
    }

    public reloadCustomerInvoice(invoiceId: any) {
        this.customerInvoiceId = invoiceId;
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.customerInvoiceId = 0;
        this.customer = null;
        this.selectedInvoiceDate = null;
        this.invoice = new CustomerInvoiceDTO();
        this.invoice.vatType = this.defaultVatType;
        this.invoice.vatCodeId = this.defaultVatCodeId;
        this.invoice.currencyId = this.currencies[0].currencyId;    // Base currency is first in collection
        this.invoice.currencyDate = this.currencyDate = CalendarUtility.getDateToday();
        this.invoice.totalAmount = null;
        this.selectedCustomer = null;

        _.forEach(this.voucherSeries, (voucherSerie) => {
            if (voucherSerie.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId) {
                this._selectedVoucherSeriesId = voucherSerie.voucherSeriesId;
                this._selectedVoucherSeriesTypeId = voucherSerie.voucherSeriesTypeId;
            }
        });

        this.draft = this.defaultDraft;
        this.invoiceFilesHelper.reset();
        this.invoice.accountingRows = [];
    }

    private setPaymentCondition(paymentConditionId: number) {
        if (paymentConditionId === 0)
            paymentConditionId = this.defaultPaymentConditionId;

        // Get condition
        const condition = _.find(this.paymentConditions, { paymentConditionId: paymentConditionId });
        this.invoice.paymentConditionId = condition ? condition.paymentConditionId : 0;
        this.paymentConditionDays = condition ? condition.days : this.defaultPaymentConditionDays;
        this.paymentConditionStartOfNextMonth = condition ? condition.startOfNextMonth : this.defaultPaymentConditionStartOfNextMonth;
        this.discountDays = condition ? condition.discountDays : 0;
        this.discountPercent = condition && condition.discountPercent ? condition.discountPercent : 0;
    }

    private setDueDate() {
        if (this.invoice && this.invoice.invoiceDate && !this.loadingInvoice) {
            const startDate = this.paymentConditionStartOfNextMonth ? this.invoice.invoiceDate.endOfMonth().addDays(1) : this.invoice.invoiceDate;
            this.invoice.dueDate = startDate.addDays(this.paymentConditionDays);
        }
    }

    private showCustomerNote(message: string) {
        this.translationService.translate("common.customer.customer.customernote").then((title) => {
            this.notificationService.showDialog(title, message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }

    private setVoucherDateOnAccountingRows() {
        if (!this.invoice.accountingRows?.length) return;
        this.invoice.accountingRows.forEach((row) => {
            row.date = this.selectedVoucherDate ?? new Date().date();
            if (!row.parentRowId)
                this.$scope.$broadcast('checkAccountDistribution', row, 3);
        });
    }

    private generateAccountingRows(calculateVat: boolean) {
        // Clear rows
        this.invoice.accountingRows = [];

        // Debit row
        this.createAccountingRow(CustomerAccountType.Debit, 0, this.invoice.totalAmountCurrency, true, false, false);

        // VAT row
        if (calculateVat)
            this.calculateVatAmount();

        // Remember VAT amount (to be used on contractor VAT rows)
        const vatAmount = this.invoice.vatAmountCurrency;

        switch (this.invoice.vatType) {
            case (TermGroup_InvoiceVatType.Contractor):
            case (TermGroup_InvoiceVatType.NoVat):
            case (TermGroup_InvoiceVatType.ExportOutsideEU):
                // 'Contractor' invoices does not have any regular VAT
                // 'No VAT' invoices does not have any VAT at all
                this.invoice.vatAmountCurrency = 0;
                break;
            default:
                if (this.vatRate > 0) {
                    this.createAccountingRow(CustomerAccountType.VAT, 0, this.invoice.vatAmountCurrency, false, true, false);
                    break;
                }
        }

        // Credit row
        const row = this.createAccountingRow(CustomerAccountType.Credit, 0, (this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency).round(2), false, false, false);

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnAllRows');
            this.$scope.$broadcast('rowsAdded');
            this.$scope.$broadcast('checkAccountDistribution', row, 3);
        });
    }

    private createAccountingRow(type: CustomerAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean, isContractorVatRow: boolean): AccountingRowDTO {
        // Credit invoice, negate isDebitRow
        if (this.isCredit)
            isDebitRow = !isDebitRow;

        amount = Math.abs(amount);

        const row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = row.tempInvoiceRowId = this.invoice.accountingRows.length + 1;
        row.rowNr = AccountingRowDTO.getNextRowNr(this.invoice.accountingRows);
        row.debitAmountCurrency = isDebitRow ? amount : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : amount;
        row.quantity = null;
        row.date = this.selectedVoucherDate ?? new Date().date();
        row.isCreditRow = !isDebitRow;
        row.isDebitRow = isDebitRow;
        row.isVatRow = isVatRow;
        row.isContractorVatRow = isContractorVatRow;
        row.isInterimRow = false;
        row.state = SoeEntityState.Active;
        row.invoiceId = this.invoice.invoiceId;
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

        this.invoice.accountingRows.push(row);
        return row;
    }

    private getAccountId(type: CustomerAccountType, dimNr: number): number {
        // First try to get account from customer
        var accountId = this.getCustomerAccountId(type, dimNr);
        if (accountId === 0 && dimNr === 1) {
            // No account found on customer, use base account
            switch (type) {
                case CustomerAccountType.Credit:
                    switch (this.invoice.vatType) {
                        case TermGroup_InvoiceVatType.ExportOutsideEU:
                            accountId = this.defaultOutsideEUAccountId;
                            break;
                        case TermGroup_InvoiceVatType.ExportWithinEU:
                            accountId = this.defaultWithinEUAccountId;
                            break;
                        case TermGroup_InvoiceVatType.Contractor:
                            accountId = this.reverseVatSalesId;
                            break;
                        default:
                            accountId = this.defaultCreditAccountId;
                            break;
                    }
                    break;
                case CustomerAccountType.Debit:
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

        if (type === CustomerAccountType.VAT && dimNr === 1 && this.customerVatAccountId !== 0)
            return this.customerVatAccountId;

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
        _.forEach(this.invoice.accountingRows, (row) => {
            if (row.isModified)
                modified = true;
        });

        return modified;
    }

    public openCustomer() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Customer/Customers/Views/edit.html"),
            controller: CustomerEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, id: this.selectedCustomer ? this.selectedCustomer.id : 0 });
        });

        modal.result.then(result => {
            this.loadCustomers(false).then(() => {
                this.commonCustomerService.getCustomer(result.customerId, false, true, false, false, false, false).then(x => {
                    this.customer = x;
                    this.customerChanged();
                    this.loadingInvoice = false;
                }).then(() => {
                    this._selectedCustomer = _.find(this.customers, { id: result.customerId });
                });
            });
        });
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
            if (this.invoice) {
                if (!this.selectedCustomer)
                    mandatoryFieldKeys.push("common.customer.customer.customer");
                if (!this.invoice.invoiceNr)
                    mandatoryFieldKeys.push("common.customer.invoices.invoicenr");
                if (!this.invoice.vatType)
                    mandatoryFieldKeys.push("common.customer.invoices.vattype");
                if (!this.invoice.invoiceDate)
                    mandatoryFieldKeys.push("common.customer.invoices.invoicedate");
                if (!this.invoice.dueDate)
                    mandatoryFieldKeys.push("common.customer.invoices.duedate");
                if (!this.invoice.voucherDate)
                    mandatoryFieldKeys.push("common.customer.invoices.voucherdate");
            }

            if (errors['vatAmount'])
                mandatoryFieldKeys.push("common.customer.invoices.vat");
            if (errors['voucherSeriesId'])
                mandatoryFieldKeys.push("common.customer.invoices.voucherseries");

            // Accounting row validation
            if (errors['accountStandard'])
                validationErrorKeys.push("economy.accounting.voucher.accountstandardmissing");
            if (errors['accountInternal'])
                validationErrorKeys.push("economy.accounting.voucher.accountinternalmissing");
            if (errors['rowAmount'])
                validationErrorKeys.push("economy.accounting.voucher.invalidrowamount");
            if (errors['amountDiff'])
                validationErrorKeys.push("economy.accounting.voucher.unbalancedrows");
        });
    }
    private skipVatNotZeroValidation(): boolean {
        return typeof (this.invoice) != "undefined" ? (this.invoice.vatType !== TermGroup_InvoiceVatType.Merchandise) : false;
    }
    private setLocked() {
        var locked = true;

        if (this.invoice.originStatus === SoeOriginStatus.Draft) {
            // An invoice in status Draft can always be edited
            locked = false;
            this.isLockedVoucherSeries = false;
        }
        else if (this.invoice.originStatus === SoeOriginStatus.Origin) {
            // An invoice (ledger) in status Origin can sometimes be edited. It depends on the company setting CustomerInvoiceAllowEditOrigin.
            // If it's set, the invoice can be edited in status Origin if no PaymentRow has been created.
            // If it's not set, the invoice cannot be edited in status Origin.
            locked = (this.allowEditOrigin ? this.invoice.paidAmount != 0 : true);
            this.isLockedVoucherSeries = (this.allowEditOrigin ? false : true);
        }
        else {
            // An invoice in another status can never be edited
            this.isLockedVoucherSeries = true;
        }

        this.isLocked = locked;

        this.isLockedAccountingRows = this.isLocked;
        if (this.unlockAccountingRowsPermission && this.allowEditAccountingRows)
            this.isLockedAccountingRows = false;
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this.edit.$invalid;
    }

    private showDeleteButton(): boolean {
        return this.modifyPermission && !this.isNew && this.invoice && (this.invoice.originStatus === SoeOriginStatus.Draft || (
            this.invoice.originStatus === SoeOriginStatus.Origin && !this.isLocked));
    }
}