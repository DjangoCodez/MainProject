import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { InvoiceCurrencyHelper } from "../Helpers/InvoiceCurrencyHelper";
import { ISmallGenericType, IPriceListTypeDTO, IShiftTypeGridDTO, IOrderShiftDTO, ICustomerProductPriceSmallDTO, IActionResult, ICustomerInvoicePrintDTO } from "../../../Scripts/TypeLite.Net4";
import { CustomerInvoiceAccountRowDTO, ProductRowDTO, BillingInvoiceDTO } from "../../../Common/Models/InvoiceDTO";
import { CustomerDTO } from "../../../Common/Models/CustomerDTO";
import { ProjectDTO, ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";
import { TimeProjectContainer, ProductRowsContainers, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, OrderEditProjectFunctions, OrderEditSaveFunctions, OrderInvoiceEditPrintFunctions, SOEMessageBoxButton, SOEMessageBoxSize } from "../../../Util/Enumerations";
import { ShiftTypeGridDTO } from "../../../Common/Models/ShiftTypeDTO";
import { InvoiceEditHandler } from "../Helpers/InvoiceEditHandler";
import { IReportService } from "../../../Core/Services/ReportService";
import { IProductService } from "../Products/ProductService";
import { IInvoiceService } from "./InvoiceService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { EditController as BillingProjectsEditController } from "../../../Shared/Billing/Projects/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as CustomerEditController } from "../../../Common/Customer/Customers/EditController";
import { SelectProjectController } from "../../../Common/Dialogs/SelectProject/SelectProjectController";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SelectEmailController } from "../../../Common/Dialogs/SelectEmail/SelectEmailController";
import { SelectReportAndAttachmentsController } from "../../../Common/Dialogs/SelectReportAndAttachments/SelectReportAndAttachmentsController";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { FileUploadDTO } from "../../../Common/Models/fileuploaddto";
import { TextBlockDialogController } from "../../../Common/Dialogs/TextBlock/TextBlockDialogController";
import { SelectCustomerController } from "../../../Common/Dialogs/SelectCustomer/SelectCustomerController";
import { AccordionSettingsController } from "../../../Common/Dialogs/AccordionSettings/AccordionSettingsController";
import { IOrderService } from "../Orders/OrderService";
import { SoeEntityType, SoeEntityImageType, Feature, SoeProjectRecordType, TermGroup_ProjectType, TermGroup_BillingType, OrderContractType, SoeOriginStatus, CompanySettingType, UserSettingType, TermGroup, TermGroup_Languages, TermGroup_CurrencyType, TermGroup_InvoiceVatType, OrderInvoiceRegistrationType, TermGroup_OrderType, TermGroup_EInvoiceFormat, SoeStatusIcon, SoeReportTemplateType, SoeOriginStatusChange, SoeInvoiceRowType, TextBlockType, SimpleTextEditorDialogMode, ActionResultSave, CustomerAccountType, SoeOriginType, EmailTemplateType, SoeEntityState, SoeInvoiceDeliveryProvider, SoeModule, SoeInvoiceDeliveryType, TermGroup_Country } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { EditDeliveryAddressController } from "../../../Shared/Billing/Dialogs/EditDeliveryAddress/EditDeliveryAddressController";
import { FilesHelper } from "../../../Common/Files/FilesHelper";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IScopeWatcherService } from "../../../Core/Services/ScopeWatcherService";
import { OneTimeCustomerController } from "../Dialogs/OneTimeCustomer/OneTimeCustomerController";
import { StringUtility } from "../../../Util/StringUtility";
import { CashSalesController } from "./Dialogs/CashSales/CashSalesController";
import { CashSalesDefinitiveController } from "./Dialogs/CashSalesDefinitive/CashSalesDefinitiveController";
import { OriginUserHelper } from "../Helpers/OriginUserHelper";
import { IRequestReportService } from "../../Reports/RequestReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Helpers
    private currencyHelper: InvoiceCurrencyHelper;
    private invoiceEditHandler: InvoiceEditHandler;
    private invoiceFilesHelper: FilesHelper;
    private originUserHelper: OriginUserHelper;

    // Config
    private currentAccountYearId = 0;
    private currentAccountYearIsOpen = false;
    private isTemplateRegistration = false;

    // Permissions
    private unlockPermission = false;
    private productRowsPermission = false;
    private timeProjectPermission = false;
    private filesPermission = false;
    private accountingRowsPermission = false;
    private tracingPermission = false;

    private templatesPermission = false;
    private showSalesPricePermission = true;
    private editCustomerPermission = false;
    private useCurrency = false;
    private reportPermission = false;
    private showProjectsWithoutCustomer = false;
    private useDiffWarning = false;

    private draftToOriginPermission = false;
    private createEInvoicePermission = false;
    private downloadEInvoicePermission = false;

    // Company settings
    private showPayingCustomer = false;
    private showTransactionCurrency = false;
    private showEnterpriseCurrency = false;
    private showLedgerCurrency = false;

    private defaultInvoiceText: string;
    private defaultOurReference: string;
    private defaultWholesellerId = 0;
    private defaultPriceListTypeId = 0;
    private includeWorkDescriptionOnInvoice = false;
    private defaultDeliveryTypeId = 0;
    private defaultDeliveryConditionId = 0;
    private useDeliveryAddressAsInvoiceAddress = false;
    private defaultPaymentConditionHouseholdDeductionId = 0;

    private discountDays: number = null;
    private discountPercent = 0;
    private paymentConditionDays = 0;
    private paymentConditionStartOfNextMonth = false;
    private paymentServiceOnlyToContract = false;

    private fixedPriceProductId = 0;
    private freightAmountProductId = 0;
    private invoiceFeeProductId = 0;
    private hideVatRate = false;
    private useFreightAmount = false;
    private useInvoiceFee = false;
    private disableInvoiceFee = false;

    private autoGenerateProject = false;
    private suggestOrderNrAsProjectNr = false;
    private useCustomerNameAsProjectName = false;
    private projectIncludeTimeProjectReport = false;
    private projectIncludeOnlyInvoicedTimeInTimeProjectReport = false;
    private triangulationSales = false;
    private defaultOneTimeCustomerId = 0;
    private allowChangingOfInternalAccounts = false;
    private useExternalInvoiceNr = false;

    private emailTemplateId = 0;
    private eInvoiceFormat = 0;
    private reminderReportId = 0;
    private interestReportId = 0;
    private voucherListReportId = 0;
    private timeProjectReportId = 0;
    private emailTemplateIdCashSales = 0;
    private reportIdCashSales = 0;
    private defaultBillingInvoiceReportId = 0;

    private transferToVoucher = false;
    private askPrintVoucherOnTransfer = false;
    private usePartialInvoicingOnOrderRow = false;
    private askCreateInvoiceWhenOrderReady = false;
    private createInvoiceWhenOrderReady = false;

    private showZeroRowWarning = false;

    private useCashSales = false;
    private useInvoiceDeliveryProvider = false;

    // User settings
    private defaultOurReferenceUserId = 0;
    private checkConflictsOnSave = false;
    private useOneTimeCustomer = false;
    private expanderSettings: any;

    // Company accounts
    private defaultCreditAccountId = 0;
    private defaultDebitAccountId = 0;
    private defaultCashAccountId = 0;
    private defaultVatAccountId = 0;
    private reverseVatPurchaseId = 0;
    private reverseVatSalesId = 0;
    private contractorVatAccountDebitId = 0;
    private contractorVatAccountCreditId = 0;
    private defaultCustomerDiscountAccount = 0;
    private defaultCustomerDiscountOffsetAccount = 0;
    private defaultVatRate = 0;

    // Customer accounts
    private customerVatAccountId = 0;

    private vatRate: number = Constants.DEFAULT_VAT_RATE;

    // Lookups
    private terms: any;
    private customers: any[];
    private billingTypes: ISmallGenericType[];
    private invoiceTemplates: ISmallGenericType[];
    private fixedPriceOrderTypes: ISmallGenericType[];
    private ourReferences: ISmallGenericType[];

    private invoiceDeliveryTypes: ISmallGenericType[];
    private invoiceDeliveryProviders: ISmallGenericType[];
    private priceListTypes: IPriceListTypeDTO[];

    private invoicePaymentServices: ISmallGenericType[];
    private voucherSeries: any[];
    private shiftTypes: IShiftTypeGridDTO[];

    // Data
    private invoice: BillingInvoiceDTO;
    private originalInvoice: BillingInvoiceDTO;
    private accountRows: CustomerInvoiceAccountRowDTO[];
    private customer: CustomerDTO; //Also used for paying customer
    private deliveryCustomer: CustomerDTO;
    private customerReferences: ISmallGenericType[];
    private customerEmails: ISmallGenericType[];
    private pendingInvoiceReminders: any[];
    private pendingInvoiceInterests: any[];
    private currentBalance: number;
    private checklists: any[] = [];

    private plannedShifts: IOrderShiftDTO[];
    private projectTimeBlockRows: ProjectTimeBlockDTO[] = [];
    private invoiceIds: number[];

    //Household parameters
    private createDeniedHouseholdInvoice = false;
    private originalCustomerInvoiceRowId: number;
    private householdAmount: number;
    private householdQuantity: number;
    private taxDeductionType: string;
    private taxDeductionPercent: string;
    private partialHouseholdAmount: number;

    // Flags
    private loadingInvoice = false;
    private invoiceIsLoaded = false;
    private hasModifiedProductRows = false;
    private hasHouseholdTaxDeduction = false;
    private isStopped = false;
    private isLocked = false;
    private loadingTimeProjectRows = false;
    private createCopy = false;
    private showNavigationButtons = true;
    private copyChecklists = false;
    private loadChecklistsRecords = false;
    private paymentServiceReadOnly = false;
    private allowModifyDefinitive = false;
    private showRevokeButton = false;
    private showDeleteButton = false;
    private showCreditButton = false;
    private showPrintInvoiceAsCopy = false;
    private showPrintInvoiceAsReminder = false;
    private printInvoiceAsReminderEnabled = false;
    private invoiceFeeUpdated = false;
    private freightAmountUpdated = false;
    private loadingCustomer = false;
    private loadingDeliveryCustomer = false;
    private executing = false;
    private showUnlockButton = false;
    private creditProductRows = false;
    private originIsCredit = false;
    private creditingInvoice = false;
    private copyingInvoice = false;
    private ignoreReloadInvoiceFee = false;
    private ignoreReloadFreightAmount = false;
    private vatTypeUpdated = false;
    private tryCopyAccountingRows = false;
    private resetDocumentsGridData = false;
    private doNotLoadCustomer = false;
    private sentAsEInvoice = false;

    // Properties
    public invoiceId: number;
    private customerId: number;
    private feature: Feature;
    private invoiceAccountYearId = 0;
    private accountPeriodId = 0;

    private fixedPrice = false;
    private fixedPriceKeepPrices = false;
    private showRemainingAmountExVat = false;
    private recordType = SoeProjectRecordType.Order;
    private projectType = TermGroup_ProjectType.TimeProject;
    private projectContainer = TimeProjectContainer.Order;
    private employeeId = 0;
    private timeProjectFrom: Date;
    private timeProjectTo: Date;
    private sendXEMail = false;
    private isProjectCentral = false;
    private printInvoiceAsCopy = false;
    private printInvoiceAsReminder = false;
    private isDisableProjectCreateMerge = false;
    private openedFromOrder = false;
    private module;
    private createEInvoicePermissionFromEDI = false;

    private saveAsTemplate = false;

    get isNewOrDraft(): boolean {
        return this.invoice == null || this.invoice.originStatus == SoeOriginStatus.Draft;
    }

    get isCancelled(): boolean {
        return this.invoice != null && this.invoice.originStatus == SoeOriginStatus.Cancel;
    }

    get isInsecureDebt(): boolean {
        return this.invoice != null && this.invoice.insecureDebt;
    }

    get isCredit(): boolean {
        return this.invoice && this.invoice.billingType === TermGroup_BillingType.Credit;
    }

    set isCredit(value: boolean) { /* Not actually a setter, just to make binding work */ }

    get isReminder(): boolean {
        return this.invoice != null && this.invoice.billingType == TermGroup_BillingType.Reminder;
    }

    get isPrinted(): boolean {
        return this.invoice != null && this.invoice.billingInvoicePrinted;
    }

    get disableProject(): boolean {
        let disable = false;
        if (!this.invoice) return disable;

        if (!this.invoice.actorId)
            disable = true;
        else if (this.invoice.originStatus == SoeOriginStatus.Origin || this.invoice.originStatus == SoeOriginStatus.Voucher)
            disable = true;

        return disable;
    }

    private resetReference = false;
    private _selectedCustomer;
    get selectedCustomer(): ISmallGenericType {
        return this._selectedCustomer;
    }
    set selectedCustomer(item: ISmallGenericType) {
        if (this.doNotLoadCustomer) {
            this._selectedCustomer = item;
        }
        else {
            if (item && this.invoice.actorId !== item.id) {
                this.invoice.customerName = undefined;
                this.invoice.customerEmail = undefined;
                this.invoice.customerPhoneNr = undefined;
            }

            if (item && item.id > 0) {
                this.resetReference = (this.invoice && this.invoice.actorId !== item.id);
                this._selectedCustomer = item;
                if (this.invoice)
                    this.invoice.actorId = this._selectedCustomer.id;
                this.setAsDirty();
                this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null);
            }
            else {
                this._selectedCustomer = undefined;
                if (item === null) {
                    if (this.invoice)
                        this.invoice.actorId = undefined;
                    if (this.customer)
                        this.customer = undefined;
                }
            }
        }
    }

    private _selectedDeliveryCustomer;
    get selectedDeliveryCustomer(): ISmallGenericType {
        return this._selectedDeliveryCustomer;
    }
    set selectedDeliveryCustomer(item: ISmallGenericType) {
        if (this.doNotLoadCustomer) {
            this._selectedDeliveryCustomer = item;
        }
        else {
            if (item && item.id > 0) {
                this._selectedDeliveryCustomer = item;
                if (this._selectedDeliveryCustomer && this._selectedDeliveryCustomer.id) {
                    if (this.invoice)
                        this.invoice.deliveryCustomerId = this._selectedDeliveryCustomer.id;
                    this.loadDeliveryCustomer();
                }
            }
            else {
                this._selectedDeliveryCustomer = undefined;
                if (this.invoice)
                    this.invoice.deliveryCustomerId = undefined;
                if (this.deliveryCustomer)
                    this.deliveryCustomer = undefined;
            }
        }
    }

    get customerProducts(): ICustomerProductPriceSmallDTO[] {
        return this.customer ? this.customer.customerProducts : [];
    }

    private _selectedFixedPriceOrder: ISmallGenericType;
    get selectedFixedPriceOrder(): ISmallGenericType {
        return this._selectedFixedPriceOrder;
    }
    set selectedFixedPriceOrder(item: ISmallGenericType) {
        this._selectedFixedPriceOrder = item;
        this.invoice.fixedPriceOrder = item && item.id === OrderContractType.Fixed;
    }

    private selectedInvoiceTemplate;

    private _selecedInvoiceDate: Date;
    get selectedInvoiceDate() {
        return this._selecedInvoiceDate;
    }
    set selectedInvoiceDate(date: Date) {
        this._selecedInvoiceDate = date ? new Date(<any>date.toString()) : null;

        if (this.invoice) {
            this.invoice.invoiceDate = this.selectedInvoiceDate;
            this.selectedVoucherDate = this.selectedInvoiceDate;
            this.currencyHelper.currencyDate = this.selectedInvoiceDate;
            this.setDueDate();
        }
    }

    private _selectedVoucherDate: Date;
    get selectedVoucherDate() {
        return this._selectedVoucherDate;
    }
    set selectedVoucherDate(date: Date) {
        this._selectedVoucherDate = date ? new Date(<any>date.toString()) : null;
        if (this.invoice) {
            this.invoice.voucherDate = this.selectedVoucherDate;
            this.currencyHelper.currencyDate = this.selectedVoucherDate;
        }
    }

    private _selectedPriceListType: IPriceListTypeDTO;
    get selectedPriceListType() {
        return this._selectedPriceListType;
    }
    set selectedPriceListType(type: IPriceListTypeDTO) {
        this.setPriceListType(type ? type.priceListTypeId : 0);
    }

    private _selectedShiftType;
    get selectedShiftType(): ShiftTypeGridDTO {
        return this._selectedShiftType;
    }
    set selectedShiftType(item: ShiftTypeGridDTO) {
        this._selectedShiftType = item;
        if (this.invoice) {
            this.invoice.shiftTypeId = item ? item.shiftTypeId : undefined;
            if (!this.loadingInvoice) {
                this.setValuesFromShiftType(item);
                this.setAsDirty();
            }
        }
    }

    set definitive(value: boolean) {
        if (this.invoice.originStatus !== SoeOriginStatus.Voucher) {
            if (value) {
                this.invoice.originStatus = SoeOriginStatus.Origin;
                if (!this.invoice.invoiceDate)
                    this.selectedInvoiceDate = CalendarUtility.getDateToday();
                else if (!this.invoice.dueDate)
                    this.setDueDate();
            }
            else {
                this.invoice.originStatus = SoeOriginStatus.Draft;
            }
        }
    }
    get definitive(): boolean {
        //return this.isNew || this.invoice.originStatus === SoeOriginStatus.Draft;
        if (this.invoice) {
            return (this.invoice.originStatus === SoeOriginStatus.Origin) ||
                (this.invoice.originStatus === SoeOriginStatus.Export) ||
                (this.invoice.originStatus === SoeOriginStatus.Voucher);
        }
        else { return false; }
    }

    get enableInternalAccounts(): boolean {
        return !this.isLocked || (this.allowChangingOfInternalAccounts && this.invoice.originStatus === SoeOriginStatus.Origin && this.invoice.paidAmount != 0);
    }

    private productRowsExpanderIsOpen = false
    private accountingRowsExpanderIsOpen = false;
    private timeProjectRowsExpanderIsOpen = false;
    private traceRowsExpanderIsOpen = false;
    private documentExpanderIsOpen = false;
    private invoiceConditionExpanderIsOpen = false;
    private invoiceExpanderIsOpen = false;
    private invoiceInvoiceExpanderIsOpen = false;

    private productRowsRendered = false;
    private productRowsRenderFinalized = false;
    private traceRowsRendered = false;

    get orderExpanderLabel(): string {
        if (!this.terms || !this.invoice)
            return '';

        const type = _.find(this.billingTypes, { id: this.invoice.billingType });
        const typeName: string = type ? type.name : this.terms["billing.order.notspecified"];
        const invoiceNr: string = this.invoice.invoiceNr ? this.invoice.invoiceNr : '';
        const customerName: string = this.customer ? this.customer.name : this.terms["billing.order.notspecified"];
        const statusName: string = this.invoice.originStatusName ? this.invoice.originStatusName : '';
        const projectNr: string = this.invoice.projectNr ? this.invoice.projectNr : this.terms["billing.order.noproject"];

        const label: string = "{0} {1} | {2}: {3} | {4}: {5} | {6}: {7}".format(
            typeName,
            invoiceNr,
            this.terms["common.customer.customer.customer"],
            customerName.toEllipsisString(50),
            this.terms["billing.order.status"],
            statusName,
            this.terms["billing.project.projectnr"],
            projectNr
        );

        //if (this.isOrderTypeUnspecified || this.isOrderTypeProject)
        //    label += " | {0}: {1} ".format(this.terms["billing.project.projectnr"], projectNr);

        return label;
    }

    get productRowsExpanderLabel(): string {
        if (!this.terms || !this.invoice)
            return '';

        let label: string = '';

        // If no rows are hidden (transferred), just display one number
        // Otherwise show both active and visible
        if (this.nbrOfVisibleRows === this.nbrOfActiveRows)
            label = "({0})".format(this.nbrOfActiveRows.toString());
        else
            label = "({0}/{1})".format(this.nbrOfVisibleRows.toString(), this.nbrOfActiveRows.toString());

        label += " {0}: {1} | {2}: {3} | {4}: {5}".format(
            this.terms["billing.productrows.sumamount"],
            this.amountFilter(this.invoice.sumAmountCurrency),
            this.terms["billing.productrows.vatamount"],
            this.amountFilter(this.invoice.vatAmountCurrency),
            this.terms["billing.productrows.totalamount"],
            this.amountFilter(this.invoice.totalAmountCurrency));

        return label;
    }

    get showNote(): boolean {
        return this.invoice ? this.invoice.note && this.invoice.note.length > 0 : false;
    }

    private nbrOfVisibleRows: number = 0;
    private nbrOfActiveRows: number = 0;

    // Filters
    private amountFilter: any;

    // Functions
    private projectFunctions: any = [];
    private saveFunctions: any = [];
    private printFunctions: any = [];

    private edit: ng.IFormController;
    private modalInstance: any;

    private buttonStatusGroup: any = ToolBarUtility.createGroup();

    //@ngInject
    constructor(
        private $uibModal,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private commonCustomerService: ICommonCustomerService,
        private orderService: IOrderService,
        private invoiceService: IInvoiceService,
        private productService: IProductService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private scopeWatcherService: IScopeWatcherService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $window: ng.IWindowService,
        shortCutService: IShortCutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private readonly requestReportService: IRequestReportService
    ) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.load())
            .onAfterFirstLoad(() => this.onAfterFirstLoad())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        shortCutService.bindSave($scope, () => { this.save(false); });
        shortCutService.bindSaveAndClose($scope, () => { this.save(true); });
        shortCutService.bindEnterAsTab($scope);

        //Set currency
        this.currencyHelper = new InvoiceCurrencyHelper(ProductRowsContainers.Invoice, coreService, $q, $timeout, () => this.currencyChanged(), () => this.currencyIdChanged());
        this.originUserHelper = new OriginUserHelper(this, coreService, urlHelperService, translationService, $q, $uibModal);

        this.feature = Feature.Billing_Invoice_Invoices_Edit;
        this.modalInstance = $uibModal;
        this.amountFilter = $filter("amount");

        // Config parameters
        this.currentAccountYearId = soeConfig.accountYearId;
        this.currentAccountYearIsOpen = soeConfig.accountYearIsOpen;
        this.isTemplateRegistration = soeConfig.isTemplateRegistration;

        // Events - Quarantine
        /*this.messagingService.subscribe(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, (invoiceId) => {
            // Make sure event does not come from any other orders product rows
            if (invoiceId === this.invoiceId)
                this.generateAccountingRows(true);
        });*/

        this.messagingService.subscribe(Constants.EVENT_HOUSEHOLD_TAX_DEDUCTION_ADDED, (invoiceId) => {
            // Make sure event does not come from any other orders product rows
            if (invoiceId === this.invoiceId) {
                // Set payment condition from company setting if household tax deduction row is added
                if (this.defaultPaymentConditionHouseholdDeductionId) {
                    this.setPaymentCondition(this.defaultPaymentConditionHouseholdDeductionId);
                }
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_MANUALLY_DELETED_TIME_PROJECT_ROW, (invoiceId) => {
            // Make sure event does not come from any other orders product rows
            if (invoiceId === this.invoiceId)
                this.invoice.hasManuallyDeletedTimeProjectRows = true;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SEARCH_TIME_PROJECT_ROWS, (x) => {
            // Make sure event does not come from any other orders product rows
            if (x.guid === this.guid)
                this.loadTimeProjectRows();
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_RELOAD_INVOICE, (x) => {
            // Make sure event does not come from any other orders time project rows
            if (x.guid === this.guid) {
                this.load();
                this.loadTimeProjectRows();
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_PRODUCTROW_GRID_READY, (parentGuid) => {
            if (parentGuid === this.guid) {
                if (!this.creditProductRows)
                    this.$scope.$broadcast('updateCustomer', { customer: this.customer, getFreight: false, getInvoiceFee: !this.invoiceFeeUpdated });
                this.$scope.$broadcast('updateWholesellers', { wholesellers: this.invoiceEditHandler.wholesellers });
                if (this.invoice && this.invoice.customerInvoiceRows.length === 0) {
                    this.$scope.$broadcast('addNewRow', { fixedPrice: false });
                }
                if (this.createDeniedHouseholdInvoice && this.householdAmount && this.householdQuantity) {
                    this.$scope.$broadcast('addNewRow', { household: true, amount: this.householdAmount, quantity: this.householdQuantity, taxDeductionType: this.taxDeductionType, percent: this.taxDeductionPercent });
                    this.accountRows = [];
                    this.hasModifiedProductRows = true;
                    this.createDeniedHouseholdInvoice = false;
                }
                if (this.invoiceFeeUpdated) {
                    this.$scope.$broadcast('updateInvoiceFee', { invoiceFeeCurrency: this.invoice.invoiceFeeCurrency });
                    this.invoiceFeeUpdated = false;
                }
                if (this.freightAmountUpdated) {
                    this.$scope.$broadcast('updateFreighAmount', { freightAmountCurrency: this.invoice.freightAmountCurrency });
                    this.freightAmountUpdated = false;
                }
                if (this.vatTypeUpdated) {
                    this.$scope.$broadcast('vatTypeChanged');
                    this.vatTypeUpdated = false;
                }
                if (this.creditProductRows) {
                    //Product rows
                    this.$scope.$broadcast('copyRows', { guid: this.guid, isCredit: this.originIsCredit, checkRecalculate: !this.originIsCredit });

                    this.setLocked();
                    this.setAsDirty(true);
                    this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
                        guid: this.guid, label: this.terms["common.customer.invoices.newcustomerinvoice"]
                    });
                    this.creditProductRows = false;
                    this.originIsCredit = false;
                }
                if (this.pendingInvoiceInterests) {
                    this.$scope.$broadcast('addInterestRows', { interests: this.pendingInvoiceInterests });
                    this.pendingInvoiceInterests = undefined;
                }
                if (this.pendingInvoiceReminders) {
                    this.$scope.$broadcast('addReminderRows', { reminders: this.pendingInvoiceReminders });
                    this.pendingInvoiceReminders = undefined;
                }
                this.productRowsRenderFinalized = true;
            }
        }, this.$scope);
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

        this.setTabCallbacks(() => this.onTabActivated(), () => this.onTabDeActivated())
    }

    private onTabActivated() {
        if (this.invoice && this.watchUnRegisterCallbacks.length == 0) {
            this.setupWatchers()
        }

        this.scopeWatcherService.resumeWatchers(this.$scope);
    }

    private onTabDeActivated() {
        this.flowHandler.starting().finally(() => {
            if (this.isTabActivated) {
                return;
            }

            this.traceRowsRendered = false;
            this.traceRowsExpanderIsOpen = false;

            this.scopeWatcherService.suspendWatchers(this.$scope);
        });
    }

    // SETUP
    public onInit(parameters: any) {

        this.module = soeConfig.module;

        this.guid = parameters.guid;

        this.invoiceId = parameters.id || 0;
        this.customerId = parameters.customerId ?? Number(soeConfig.customerId ?? 0);
        this.isProjectCentral = parameters.isProjectCentral || false;
        this.openedFromOrder = parameters.fromOrder || false;
        this.createEInvoicePermissionFromEDI = parameters.createEInvoicePermissionFromEDI || false;

        if (parameters.createHousehold) {
            this.createDeniedHouseholdInvoice = true;
            this.invoiceId = parameters.id;
            this.originalCustomerInvoiceRowId = parameters.rowId;
            this.taxDeductionType = parameters.taxDeductionType;
            this.taxDeductionPercent = parameters.percent;
            this.partialHouseholdAmount = parameters.amount;
        }
        else if (parameters.ids && parameters.ids.length > 0) {
            this.invoiceIds = parameters.ids;
        }
        else
            this.showNavigationButtons = false;

        this.createCopyParam = (parameters.hasOwnProperty("createCopy") && parameters.createCopy);
        this.keepOrderNrParam = (parameters.hasOwnProperty("keepOrderNr") && parameters.keepOrderNr);

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.translationService.translate("common.manuallyadded").then(term => {
            this.invoiceFilesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.CustomerInvoice, SoeEntityImageType.OrderInvoice, () => this.invoiceId, term);
        });

        this.invoiceEditHandler = new InvoiceEditHandler(this, this.coreService, this.commonCustomerService, this.urlHelperService, this.notificationService, this.translationService, this.reportService, this.$uibModal, this.progress, this.messagingService, this.guid);

        this.startFlow();
    }
    private createCopyParam: boolean;
    private keepOrderNrParam: boolean;

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadCompanyAccounts(),
            this.loadCurrentAccountYear(),
            this.invoiceEditHandler.loadCurrencies(),
            this.loadCustomers()]).then(() => {
                return this.$q.all([
                    this.loadBillingTypes(),
                    this.loadTemplates(this.isTemplateRegistration),
                    this.loadFixedPriceOrderTypes(),
                    this.invoiceEditHandler.loadVatTypes(),
                    this.loadOurReferences(),
                    this.loadInvoiceDeliveryTypes(),
                    this.loadInvoiceDeliveryProviders(),
                    this.invoiceEditHandler.loadWholesellers(),
                    this.loadPriceListTypes(),
                    this.invoiceEditHandler.loadDeliveryTypes(),
                    this.invoiceEditHandler.loadDeliveryConditions(),
                    this.invoiceEditHandler.loadPaymentConditions(),
                    this.loadInvoicePaymentServices(),
                    this.invoiceEditHandler.loadDefaultVoucherSeriesId(this.currentAccountYearId),
                    this.loadVoucherSeries(this.currentAccountYearId)])
            });
    }

    private onAfterFirstLoad() {
        this.invoiceEditHandler.loadEmployeeId((employeeId) => {
            this.employeeId = employeeId;
            this.loadTimeProjectLastDate();
        });

        this.setupWatchers();
        this.handleExpanderSettings();

        if (this.createCopyParam) {
            this.copyingInvoice = true;
            this.$timeout(() => {
                this.copy(!this.keepOrderNrParam, false, false, this.keepOrderNrParam, false, true);
            }, 400);
        }
    }

    private startFlow() {
        this.flowHandler.start([
            { feature: Feature.Billing_Invoice_Invoices_Edit, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_ProductRows, loadModifyPermissions: true },
            { feature: Feature.Time_Project_Invoice_Edit, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_Images, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_AccountingRows, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_Tracing, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_Unlock, loadModifyPermissions: true },
            { feature: Feature.Billing_Preferences_InvoiceSettings_Templates, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_ShowSalesPrice, loadModifyPermissions: true },
            { feature: Feature.Billing_Customer_Customers_Edit, loadModifyPermissions: true },
            { feature: Feature.Economy_Preferences_Currency, loadModifyPermissions: true },
            { feature: Feature.Billing_Distribution_Reports_Selection, loadModifyPermissions: true },
            { feature: Feature.Billing_Distribution_Reports_Selection_Download, loadModifyPermissions: true },
            { feature: Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_UseDiffWarning, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateIntruminvoice, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice, loadModifyPermissions: true },

            { feature: Feature.Manage_System, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Status_DraftToOrigin, loadModifyPermissions: true },
        ])
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        // Expanders
        this.modifyPermission = response[Feature.Billing_Invoice_Invoices_Edit].modifyPermission;
        this.productRowsPermission = response[Feature.Billing_Invoice_Invoices_Edit_ProductRows].modifyPermission;
        this.timeProjectPermission = response[Feature.Time_Project_Invoice_Edit].modifyPermission;
        this.filesPermission = response[Feature.Billing_Invoice_Invoices_Edit_Images].modifyPermission;
        this.accountingRowsPermission = response[Feature.Billing_Invoice_Invoices_Edit_AccountingRows].modifyPermission;
        this.tracingPermission = response[Feature.Billing_Invoice_Invoices_Edit_Tracing].modifyPermission;
        this.unlockPermission = response[Feature.Billing_Invoice_Invoices_Edit_Unlock].modifyPermission;

        this.templatesPermission = response[Feature.Billing_Preferences_InvoiceSettings_Templates].modifyPermission;
        this.showSalesPricePermission = response[Feature.Billing_Product_Products_ShowSalesPrice].modifyPermission;
        this.editCustomerPermission = response[Feature.Billing_Customer_Customers_Edit].modifyPermission;
        this.useCurrency = response[Feature.Economy_Preferences_Currency].modifyPermission;
        this.reportPermission = response[Feature.Billing_Distribution_Reports_Selection].modifyPermission && response[Feature.Billing_Distribution_Reports_Selection_Download].modifyPermission;
        this.showProjectsWithoutCustomer = response[Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer].modifyPermission;
        this.useDiffWarning = response[Feature.Billing_Order_UseDiffWarning].modifyPermission;

        // Tools
        this.draftToOriginPermission = response[Feature.Billing_Invoice_Status_DraftToOrigin].modifyPermission;

        this.createEInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit_EInvoice].modifyPermission && (
            response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura].modifyPermission ||
            response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateIntruminvoice].modifyPermission ||
            response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice].modifyPermission);

        this.downloadEInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice].modifyPermission
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy());

        const statusGroup = this.buttonStatusGroup;

        statusGroup.buttons.push(new ToolBarButton("", "billing.order.hashousehold", IconLibrary.FontAwesome, "fa-home textColor", () => {
            this.notificationService.showDialogEx(this.terms["core.info"], this.terms["billing.order.hashousehold"], SOEMessageBoxImage.Information);
        }, null, () => {
            return !this.hasHouseholdTaxDeduction;
        }));
        statusGroup.buttons.push(new ToolBarButton("", "billing.order.isfixedprice", IconLibrary.FontAwesome, "fa-money-bill-alt textColor", () => {
            this.notificationService.showDialogEx(this.terms["core.info"], this.terms["billing.order.isfixedprice"], SOEMessageBoxImage.Information);
        }, null, () => {
            return !this.invoice.fixedPriceOrder;
        }));

        statusGroup.buttons.push(new ToolBarButton("", "common.accordionsettings", IconLibrary.FontAwesome, "fa-cog", () => {
            this.updateAccordionSettings();
        }, null, () => {
            return false;
        }));

        statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.editordertext", IconLibrary.FontAwesome, "fa-file-alt textColor", () => {
            this.editOrderText();
        }, () => {
            return this.isLocked;
        }, () => {
            return !this.invoice;
        }));
        statusGroup.buttons.push(new ToolBarButton("", "core.unlock", IconLibrary.FontAwesome, "fa-unlock-alt textColor", () => {
            if (this.invoice.billingInvoicePrinted) {
                var modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["billing.invoices.unlockquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.unlockInvoice();
                });
            }
            else {
                this.unlockInvoice();
            }
        }, null, () => {
            return !this.showUnlockButton;
        }));
        statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.reloadinvoice", IconLibrary.FontAwesome, "fa-sync", () => {
            this.load();
        }, null, () => {
            return false;
        }));

        this.toolbar.addButtonGroup(statusGroup);

        //Navigation
        this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (newInvoiceId) => {
            this.invoiceId = newInvoiceId;
            this.load(true);
        }, this.invoiceIds, this.invoiceId);

        this.setupFunctions();
    }

    private setupFunctions() {
        // Functions
        const keys: string[] = [
            "billing.order.project.create",
            "billing.order.project.link",
            "core.save",
            "core.saveandclose",
            "common.report.report.print",
            "common.customer.invoice.sendemail",
            "common.report.report.reports",
            "common.report.report.efaktura",
            "common.customer.invoice.sendeinvoice",
            "common.customer.invoice.downloadeinvoice",
            "common.report.report.printwithattachments"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.projectFunctions.push({ id: OrderEditProjectFunctions.Create, name: terms["billing.order.project.create"], icon: 'fal fa-fw fa-plus' });
            this.projectFunctions.push({ id: OrderEditProjectFunctions.Link, name: terms["billing.order.project.link"], icon: 'fal fa-fw fa-link' });

            this.saveFunctions.push({ id: OrderEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)", icon: 'fal fa-fw fa-save' });
            this.saveFunctions.push({ id: OrderEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] + " (Ctrl+Enter)", icon: 'fal fa-fw fa-save' });

            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.Print, name: terms["common.report.report.print"], icon: 'fal fa-fw fa-print' });
            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.PrintWithAttachments, name: terms["common.report.report.printwithattachments"], icon: 'fal fa-fw fa-print' });
            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.ReportDialog, name: terms["common.report.report.reports"], icon: 'fal fa-fw fa-print' });
            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.eMail, name: terms["common.customer.invoice.sendemail"], icon: 'fal fa-fw fa-envelope' });
            if (this.createEInvoicePermissionFromEDI || (this.createEInvoicePermission && this.module === SoeModule.Billing)) {
                this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.EInvoice, name: terms["common.customer.invoice.sendeinvoice"], icon: 'fal fa-fw fa-paper-plane' });
            }
            if (this.downloadEInvoicePermission) {
                this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.EInvoiceDownload, name: terms["common.customer.invoice.downloadeinvoice"], icon: 'fal fa-fw fa-download' });
            }
        });
    }
    private setupWatchers() {
        // Convert currency amounts
        this.watchUnRegisterCallbacks.push(
            this.$scope.$watch(() => this.invoice.totalAmountCurrency, () => {
                this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
            })
            ,
            this.$scope.$watch(() => this.invoice.vatAmountCurrency, () => {
                this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
            })
            ,
            this.$scope.$watch(() => this.invoice.invoiceFeeCurrency, (newValue, oldValue) => {
                if (!this.productRowsRendered && newValue !== oldValue) {
                    this.invoiceFeeUpdated = true;
                    this.productRowsRendered = true;
                }
            }),
            this.$scope.$watch(() => this.invoice.addAttachementsToEInvoice, () => {
                this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice;
            }),
            this.$scope.$watch(() => this.invoice.freightAmountCurrency, (newValue, oldValue) => {
                if (!this.productRowsRendered && newValue !== oldValue) {
                    this.freightAmountUpdated = true;
                    this.productRowsRendered = true;
                }
            })
        )
    }

    private handleExpanderSettings() {
        if (this.expanderSettings) {
            const settings = this.expanderSettings.split(";");

            if (_.includes(settings, 'ProductRowsExpander'))
                this.openProductRowExpander();

            this.invoiceExpanderIsOpen = this.isNew || _.includes(settings, 'InvoiceExpander');
            this.invoiceInvoiceExpanderIsOpen = _.includes(settings, 'InvoiceInvoiceExpander');
            this.invoiceConditionExpanderIsOpen = _.includes(settings, 'InvoiceConditionExpander');
            this.documentExpanderIsOpen = _.includes(settings, 'DocumentExpander');

            this.timeProjectRowsExpanderIsOpen = _.includes(settings, 'TimeRowExpander');
            this.accountingRowsExpanderIsOpen = _.includes(settings, 'AccountingRowExpander');
            this.traceRowsExpanderIsOpen = _.includes(settings, 'TracingExpander');
        }
    }

    private load(updateTab = false, loadingTemplate = false, loadRows = true): ng.IPromise<any> {

        const deferral = this.$q.defer();
        this.invoiceIsLoaded = false;
        if (this.invoiceId > 0) {
            this.loadingInvoice = true;
            let currentInvoiceRows: any;
            if (!loadRows && this.invoice) {
                currentInvoiceRows = this.invoice.customerInvoiceRows;
            }

            this.invoiceService.getInvoice(this.invoiceId, false, loadRows).then((x) => {
                // Keep old invoicelabel
                const tempInvoiceLabel = this.invoice ? this.invoice.invoiceLabel : null;
                this.invoice = new BillingInvoiceDTO();
                angular.extend(this.invoice, x);

                //Fix invoiceLabel
                if (loadingTemplate) {
                    this.invoice.invoiceLabel = this.invoice.invoiceLabel ? this.invoice.invoiceLabel : tempInvoiceLabel;
                }

                //Fix vat type
                if (this.invoice.vatType === TermGroup_InvoiceVatType.EU || this.invoice.vatType === TermGroup_InvoiceVatType.NonEU)
                    this.invoiceEditHandler.addMissingVatType(this.invoice.vatType);

                this.isNew = loadingTemplate;

                this.originUserHelper.setOriginUsers(this.invoice.originUsers);

                // Fix dates
                if (this.invoice.orderDate)
                    this.invoice.orderDate = CalendarUtility.convertToDate(this.invoice.orderDate);
                if (this.invoice.deliveryDate)
                    this.invoice.deliveryDate = CalendarUtility.convertToDate(this.invoice.deliveryDate);
                if (this.invoice.invoiceDate)
                    this.invoice.invoiceDate = CalendarUtility.convertToDate(this.invoice.invoiceDate);
                if (this.invoice.dueDate)
                    this.invoice.dueDate = CalendarUtility.convertToDate(this.invoice.dueDate);
                if (this.invoice.voucherDate)
                    this.invoice.voucherDate = CalendarUtility.convertToDate(this.invoice.voucherDate);
                if (this.invoice.plannedStartDate)
                    this.invoice.plannedStartDate = CalendarUtility.convertToDate(this.invoice.plannedStartDate);
                if (this.invoice.plannedStopDate)
                    this.invoice.plannedStopDate = CalendarUtility.convertToDate(this.invoice.plannedStopDate);
                if (this.invoice.currencyDate)
                    this.invoice.currencyDate = CalendarUtility.convertToDate(this.invoice.currencyDate);

                if (this.createDeniedHouseholdInvoice) {
                    // Fix rows
                    this.invoice.customerInvoiceRows = _.sortBy(this.invoice.customerInvoiceRows, 'rowNr').map(r => {
                        var obj = new ProductRowDTO();
                        angular.extend(obj, r);
                        obj.isReadOnly = this.definitive || (this.customer && this.customer.blockInvoice);
                        return obj;
                    });

                    this.createNewInvoiceFromHousehold();
                    deferral.resolve();
                    this.invoiceLoaded();
                    return;
                }

                if (loadingTemplate) {
                    this.invoice.isTemplate = false;
                    this.invoice.originDescription = "";
                    this.selectedInvoiceTemplate = null;
                    this.invoice.invoiceId = 0;
                    this.invoiceId = 0;
                    if (this.customer) {
                        this.invoice.actorId = this.customer.actorCustomerId;
                    }

                    var tempRowId = 1;
                    _.forEach(this.invoice.customerInvoiceRows, (o) => {
                        o.customerInvoiceRowId = 0;
                        o.tempRowId = tempRowId;
                        tempRowId = tempRowId + 1;
                    });
                }

                // Change customer name
                if (!StringUtility.isEmpty(this.invoice.customerName) && this.invoice.actorId && this.invoice.actorId > 0) {
                    const customer = _.find(this.customers, c => c.id === this.invoice.actorId);
                    if (customer)
                        customer.name = customer.number + " " + this.invoice.customerName;
                }

                this.selectedCustomer = _.find(this.customers, c => c.id === this.invoice.actorId);

                //Customer might be inactive, trigger customer load anyways.
                if (!this.selectedCustomer) {
                    this.selectedCustomer = {
                        id: this.invoice.actorId,
                        name: "",
                    }
                }
                this.loadingCustomer = true;
                if (this.invoice.deliveryCustomerId) {
                    this.loadingDeliveryCustomer = true;

                    //prevent set method kicking off
                    let selectedDeliveryCustomer = _.find(this.customers, c => c.id === this.invoice.deliveryCustomerId);
                    if (selectedDeliveryCustomer) this.selectedDeliveryCustomer = selectedDeliveryCustomer;

                    //Customer might be inactive, trigger customer load anyways.
                    if (!this.selectedDeliveryCustomer) {
                        this.selectedDeliveryCustomer = {
                            id: this.invoice.deliveryCustomerId,
                            name: "",
                        }
                    }
                }

                this.selectedFixedPriceOrder = _.find(this.fixedPriceOrderTypes, f => f.id === (this.invoice.fixedPriceOrder ? OrderContractType.Fixed : OrderContractType.Continuous));
                if (this.invoice.shiftTypeId)
                    this.selectedShiftType = _.find(this.shiftTypes, s => s.shiftTypeId === this.invoice.shiftTypeId);

                this.setPriceListType(this.invoice.priceListTypeId, true);

                this._selecedInvoiceDate = this.invoice.invoiceDate;
                this._selectedVoucherDate = this.invoice.voucherDate;
                this.currencyHelper.currencyDate = this.invoice.currencyDate;

                this.currencyHelper.fromInvoice(this.invoice);

                this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice;

                if (loadRows) {
                    this.invoice.customerInvoiceRows = _.sortBy(this.invoice.customerInvoiceRows, 'rowNr').map(r => {
                        var obj = new ProductRowDTO();
                        angular.extend(obj, r);
                        obj.isReadOnly = this.definitive || (this.customer && this.customer.blockInvoice);
                        return obj;
                    });

                    this.hasHouseholdTaxDeduction = _.some(this.invoice.customerInvoiceRows, r => r.isHouseholdRow);
                } else {
                    _.forEach(currentInvoiceRows, (r) => {
                        r.isReadOnly = this.definitive || (this.customer && this.customer.blockInvoice);
                    });
                    this.invoice.customerInvoiceRows = currentInvoiceRows;
                }

                // Set temp row ids
                this.resetTempRowIds(this.invoice.customerInvoiceRows);

                // Save original to be able to compare when saving
                this.originalInvoice = new BillingInvoiceDTO();
                angular.extend(this.originalInvoice, CoreUtility.cloneDTO(this.invoice));

                if (this.invoice.paymentConditionId)
                    this.setPaymentCondition(this.invoice.paymentConditionId);

                this.invoiceFilesHelper.reset();
                if (this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon))
                    this.invoiceFilesHelper.nbrOfFiles = '*';

                this.hasModifiedProductRows = false;
                if (!loadingTemplate)
                    this.setAsDirty(false);

                this.setLocked();

                this.allowModifyDefinitive = (this.draftToOriginPermission && (this.isNew || (this.invoice.originStatus === SoeOriginStatus.Draft)));
                this.showRevokeButton = this.modifyPermission && !this.isNew && this.invoice.originStatus === SoeOriginStatus.Origin && this.invoice.paidAmount === 0;
                this.showDeleteButton = this.modifyPermission && !this.isNew && this.invoice.originStatus === SoeOriginStatus.Draft;
                this.showCreditButton = this.modifyPermission && this.definitive;
                this.showPrintInvoiceAsCopy = (this.invoice.originStatus === SoeOriginStatus.Draft || this.invoice.originStatus === SoeOriginStatus.Cancel) ? false : true;
                this.printInvoiceAsCopy = this.invoice.billingInvoicePrinted;

                this.showPrintInvoiceAsReminder = (this.invoice.originStatus === SoeOriginStatus.Draft || this.invoice.originStatus === SoeOriginStatus.Cancel) ? false : true;

                if (this.isNewOrDraft || this.isCancelled || this.isInsecureDebt || this.isCredit || this.isReminder) {
                    this.showPrintInvoiceAsReminder = false;
                    this.printInvoiceAsReminderEnabled = false;
                    this.printInvoiceAsReminder = false;
                }
                else if (this.isPrinted || this.sentAsEInvoice) {
                    this.showPrintInvoiceAsReminder = true;
                    this.printInvoiceAsReminderEnabled = this.reminderReportId > 0;
                    this.printInvoiceAsReminder = false;
                }

                if (updateTab) {
                    this.updateTabCaption();
                }

                if (x.customerBlockNote) {
                    this.invoiceEditHandler.showBlockNote(x.customerBlockNote);
                }

                if (this.isProjectCentral)
                    this.updateTabCaption();

                this.GetEInvoiceEntry();

                // If user has opened the accounting rows expander, reload them
                this.accountRows = null;

                if (this.accountingRowsExpanderIsOpen) {
                    this.loadAccountRows();
                }

                // If user has opened the time project rows expander, reload them
                if (this.timeProjectRowsExpanderIsOpen)
                    this.loadTimeProjectRows();

                if (this.documentExpanderIsOpen) {
                    this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0);
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

    private updateTabCaption(setNew: boolean = false) {
        if (setNew) {
            this.messagingHandler.publishSetTabLabelNew(this.guid, this.terms["common.customer.invoices.newcustomerinvoice"]);
        } else {
            var invoiceNr = this.invoice.invoiceNr ? this.invoice.invoiceNr : this.terms["economy.supplier.invoice.preliminary"];
            var label = this.invoice && this.invoice.isTemplate ? this.invoice.originDescription : (this.terms["common.customerinvoice"] + " " + invoiceNr);
            this.messagingHandler.publishSetTabLabel(this.guid, label, this.invoice.invoiceId);
        }
    }

    private invoiceLoaded() {
        this.loadingInvoice = false;
        this.invoiceIsLoaded = true;
    }

    private loadAccountRows() {
        if (!this.accountRows || this.accountRows.length === 0) {
            this.orderService.getAccountRows(this.invoiceId).then(rows => {
                this.accountRows = _.sortBy(rows, 'rowNr').map(dto => {
                    var obj = new CustomerInvoiceAccountRowDTO();
                    angular.extend(obj, dto);
                    return obj;
                });
            });
        }
    }

    private loadTimeProjectLastDate() {
        if (this.invoice.projectId && this.invoice.invoiceId) {
            this.orderService.getProjectTimeBlocksLastDate(this.invoice.projectId, this.invoice.invoiceId, this.recordType, this.employeeId, false).then(date => {
                date = CalendarUtility.convertToDate(date);
                this.timeProjectFrom = date.beginningOfWeek();
                this.timeProjectTo = date.endOfWeek();
            });
        }
    }

    private loadTimeProjectRowsTimeout: any;
    private loadTimeProjectRows() {
        if (!this.invoice.projectId || !this.invoice.invoiceId || !this.timeProjectFrom || !this.timeProjectTo)
            return;

        if (this.loadTimeProjectRowsTimeout)
            this.$timeout.cancel(this.loadTimeProjectRowsTimeout);

        this.loadTimeProjectRowsTimeout = this.$timeout(() => {
            this.loadingTimeProjectRows = true;
            this.orderService.getProjectTimeBlocks(this.invoice.projectId, this.invoice.invoiceId, this.recordType, this.employeeId, false, this.invoice.vatType, this.timeProjectFrom, this.timeProjectTo).then(rows => {
                this.projectTimeBlockRows = rows.map(dto => {
                    var obj = new ProjectTimeBlockDTO();
                    angular.extend(obj, dto);
                    obj.isEditable = true;
                    obj.date = CalendarUtility.convertToDate(obj.date);
                    return obj;
                });
                this.loadingTimeProjectRows = false;
            });
        }, 500);
    }

    private loadTerms(): ng.IPromise<any> {

        const keys: string[] = [
            "core.info",
            "core.warning",
            "core.verifyquestion",
            "common.customer.customer.customer",
            "billing.order.status",
            "billing.project.projectnr",
            "billing.order.notspecified",
            "billing.order.noproject",
            "billing.order.hashousehold",
            "billing.order.isfixedprice",
            "billing.productrows.sumamount",
            "billing.productrows.vatamount",
            "billing.productrows.totalamount",
            "billing.invoices.checklist.hasmandatorysingle",
            "billing.invoices.checklists.hasmandatorymany",
            "common.customer.invoices.closeorderwarning",
            "common.customer.invoices.orderunlockstatusfailed",
            "common.customer.invoices.orderunlockfailed",
            "common.customer.invoices.orderclosefailed",
            "common.customer.invoices.editordertext",
            "common.customer.invoices.yourreference",
            "common.customerinvoice",
            "economy.supplier.invoice.preliminary",
            "common.report.report.efakturaissent",
            "common.waitingforanswer",
            "common.sent",
            "billing.invoices.einvoiceasdefinitive",
            "billing.invoices.hasbeensentelectronically",
            "billing.invoices.finvoiceasdefinitive",
            "common.customer.invoices.autotovoucher",
            "economy.supplier.payment.voucherscreated",
            "economy.supplier.payment.askPrintVoucher",
            "economy.supplier.payment.defaultVoucherListMissing",
            "billing.invoices.unlockquestion",
            "common.customer.invoices.newcustomerinvoice",
            "common.customer.invoices.copyattachmentsheader",
            "common.customer.invoices.copyattachmentstext",
            "billing.invoices.cashsales.newsale",
            "billing.invoices.cashsales.startnewsale",
            "billing.invoices.delete",
            "core.missingmandatoryfield",
            "billing.order.creditlimit.message1",
            "billing.order.creditlimit.message2",
            "billing.order.creditlimit",
            "common.customer.invoice.einvoicingoperatorvalidation",
            "common.customer.invoices.einvoice.redownloadedit",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        // Order fields
        settingTypes.push(CompanySettingType.CustomerInvoiceUseDeliveryCustomer);
        settingTypes.push(CompanySettingType.CustomerShowTransactionCurrency);
        settingTypes.push(CompanySettingType.CustomerShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.CustomerShowLedgerCurrency);
        settingTypes.push(CompanySettingType.CustomerInvoiceDefaultVatType);
        settingTypes.push(CompanySettingType.BillingInvoiceText);
        settingTypes.push(CompanySettingType.CustomerInvoiceOurReference);
        settingTypes.push(CompanySettingType.BillingDefaultWholeseller);
        settingTypes.push(CompanySettingType.BillingDefaultPriceListType);
        settingTypes.push(CompanySettingType.BillingIncludeWorkDescriptionOnInvoice);
        settingTypes.push(CompanySettingType.BillingDefaultDeliveryType);
        settingTypes.push(CompanySettingType.BillingDefaultDeliveryCondition);
        settingTypes.push(CompanySettingType.UseDeliveryAddressAsInvoiceAddress);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentCondition);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction);
        settingTypes.push(CompanySettingType.CustomerPaymentServiceOnlyToContract);
        settingTypes.push(CompanySettingType.BillingUseFreightAmount);
        settingTypes.push(CompanySettingType.BillingUseInvoiceFee);
        settingTypes.push(CompanySettingType.ProductFreight);
        settingTypes.push(CompanySettingType.ProductInvoiceFee);
        settingTypes.push(CompanySettingType.CustomerInvoiceTriangulationSales);
        settingTypes.push(CompanySettingType.BillingDefaultOneTimeCustomer);
        settingTypes.push(CompanySettingType.AllowChangesToInternalAccountsOnPaidCustomerInvoice);
        settingTypes.push(CompanySettingType.BillingUseExternalInvoiceNr);
        settingTypes.push(CompanySettingType.BillingUseInvoiceDeliveryProvider);

        // Product rows
        settingTypes.push(CompanySettingType.ProductFlatPrice);
        settingTypes.push(CompanySettingType.BillingHideVatRate);

        // Project
        settingTypes.push(CompanySettingType.ProjectAutoGenerateOnNewInvoice);
        settingTypes.push(CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber);
        settingTypes.push(CompanySettingType.ProjectUseCustomerNameAsProjectName);
        settingTypes.push(CompanySettingType.ProjectIncludeTimeProjectReport);
        settingTypes.push(CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport);


        // Printing
        settingTypes.push(CompanySettingType.BillingDefaultInvoiceTemplate);
        settingTypes.push(CompanySettingType.BillingDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingEInvoiceFormat);
        settingTypes.push(CompanySettingType.CustomerDefaultReminderTemplate);
        settingTypes.push(CompanySettingType.CustomerDefaultInterestTemplate);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.BillingDefaultTimeProjectReportTemplate);
        settingTypes.push(CompanySettingType.BillingDefaultInvoiceTemplateCashSales);
        settingTypes.push(CompanySettingType.BillingDefaultEmailTemplateCashSales);

        // Transfer
        settingTypes.push(CompanySettingType.CustomerInvoiceTransferToVoucher);
        settingTypes.push(CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.BillingUsePartialInvoicingOnOrderRow);
        settingTypes.push(CompanySettingType.BillingAskCreateInvoiceWhenOrderReady);

        // Validation
        settingTypes.push(CompanySettingType.BillingShowZeroRowWarning);

        //Cash sales
        settingTypes.push(CompanySettingType.BillingUseCashSales);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            // Order fields
            this.showPayingCustomer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceUseDeliveryCustomer);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowLedgerCurrency);
            this.invoiceEditHandler.defaultVatType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceDefaultVatType, this.invoiceEditHandler.defaultVatType);
            this.defaultInvoiceText = SettingsUtility.getStringCompanySetting(x, CompanySettingType.BillingInvoiceText);
            this.defaultOurReference = SettingsUtility.getStringCompanySetting(x, CompanySettingType.CustomerInvoiceOurReference);
            this.defaultWholesellerId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultWholeseller);
            this.defaultPriceListTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultPriceListType);
            if (this.defaultPriceListTypeId === 0)
                this.showMissingDefaultPriceListTypeWarning();
            this.includeWorkDescriptionOnInvoice = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingIncludeWorkDescriptionOnInvoice);
            this.defaultDeliveryTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultDeliveryType);
            this.defaultDeliveryConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultDeliveryCondition);
            this.useDeliveryAddressAsInvoiceAddress = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseDeliveryAddressAsInvoiceAddress);
            this.invoiceEditHandler.defaultPaymentConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentCondition);
            this.defaultPaymentConditionHouseholdDeductionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction);
            this.paymentServiceOnlyToContract = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentServiceOnlyToContract);

            this.useFreightAmount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseFreightAmount);
            this.useInvoiceFee = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceFee);
            this.freightAmountProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFreight);
            this.invoiceFeeProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductInvoiceFee);
            this.triangulationSales = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceTriangulationSales, true);
            this.defaultOneTimeCustomerId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultOneTimeCustomer);
            this.allowChangingOfInternalAccounts = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AllowChangesToInternalAccountsOnPaidCustomerInvoice);
            this.useExternalInvoiceNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseExternalInvoiceNr);

            // Product rows
            this.fixedPriceProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFlatPrice);
            this.hideVatRate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideVatRate);

            // Project
            this.autoGenerateProject = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectAutoGenerateOnNewInvoice);
            this.suggestOrderNrAsProjectNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber);
            this.useCustomerNameAsProjectName = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectUseCustomerNameAsProjectName, true);
            this.projectIncludeTimeProjectReport = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectIncludeTimeProjectReport);
            this.projectIncludeOnlyInvoicedTimeInTimeProjectReport = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport, true);

            // Printing
            this.emailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultEmailTemplate);
            this.eInvoiceFormat = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingEInvoiceFormat);
            this.reminderReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultReminderTemplate);
            this.interestReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultInterestTemplate);
            this.voucherListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVoucherList);
            this.timeProjectReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultTimeProjectReportTemplate);
            this.emailTemplateIdCashSales = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultEmailTemplateCashSales);
            this.reportIdCashSales = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceTemplateCashSales);
            this.defaultBillingInvoiceReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceTemplate);

            // Transfer
            this.transferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceTransferToVoucher);
            this.askPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
            this.usePartialInvoicingOnOrderRow = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUsePartialInvoicingOnOrderRow);
            this.askCreateInvoiceWhenOrderReady = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingAskCreateInvoiceWhenOrderReady);

            // Validation
            this.showZeroRowWarning = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingShowZeroRowWarning);

            //Cash sales
            this.useCashSales = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseCashSales);
            this.useInvoiceDeliveryProvider = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceDeliveryProvider);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingInvoiceOurReference,
            UserSettingType.BillingCheckConflictsOnSave,
            UserSettingType.BillingInvoiceDefaultExpanders,
            UserSettingType.BillingUseOneTimeCustomerAsDefault
        ];

        return this.coreService.getUserSettings(settingTypes).then(x => {

            this.defaultOurReferenceUserId = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingInvoiceOurReference);
            this.checkConflictsOnSave = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingCheckConflictsOnSave);
            this.useOneTimeCustomer = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingUseOneTimeCustomerAsDefault);
            this.expanderSettings = x[UserSettingType.BillingInvoiceDefaultExpanders];
        });
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.AccountCustomerSalesVat,
            CompanySettingType.AccountCustomerClaim,
            CompanySettingType.AccountCommonCheck,
            CompanySettingType.AccountCommonVatPayable1,
            CompanySettingType.AccountCommonReverseVatPurchase,
            CompanySettingType.AccountCommonReverseVatSales,
            CompanySettingType.AccountCommonVatPayable1Reversed,
            CompanySettingType.AccountCommonVatReceivableReversed
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesVat);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim);
            this.defaultCashAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCheck);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1);
            this.reverseVatPurchaseId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatPurchase);
            this.reverseVatSalesId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatSales);
            this.contractorVatAccountCreditId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1Reversed);
            this.contractorVatAccountDebitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivableReversed);
            this.defaultCustomerDiscountAccount = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscount);
            this.defaultCustomerDiscountOffsetAccount = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscountOffset);

            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId);
        });
    }

    private loadCustomers(): ng.IPromise<any> {
        this.customers = [];

        return this.commonCustomerService.getCustomersSmall(true).then((data: any[]) => {
            this.customers.push({ id: 0, name: " " });
            data.forEach(customer => {
                this.customers.push({ id: customer.actorCustomerId, name: customer.customerNr + " " + customer.customerName, number: customer.customerNr });
            });
        });
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then(x => {
            this.billingTypes = x;
        });
    }

    private loadTemplates(useCache: boolean = true): ng.IPromise<any> {
        if (this.templatesPermission) {
            return this.orderService.getTemplates(SoeOriginType.CustomerInvoice, useCache).then(x => {
                this.invoiceTemplates = x;
            });
        }
    }

    private loadFixedPriceOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderContractType, false, false).then(x => {
            this.fixedPriceOrderTypes = x;
        });
    }

    private loadInvoiceDeliveryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryType, true, false).then(x => {
            this.invoiceDeliveryTypes = x;
            if (this.eInvoiceFormat !== TermGroup_EInvoiceFormat.Intrum) {
                this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(x => x.id !== SoeInvoiceDeliveryType.EDI);
            }
        });
    }

    private loadInvoiceDeliveryProviders(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryProvider, true, false).then(x => {
            if (this.useInvoiceDeliveryProvider) {
                this.invoiceDeliveryProviders = x;
            }
        });
    }

    private loadOurReferences(): ng.IPromise<any> {
        return this.coreService.getUsersDict(true, false, true, false).then(x => {
            this.ourReferences = x;
        });
    }

    private loadPriceListTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getPriceLists().then(x => {
            this.translationService.translate("common.customer.invoices.projectpricelist").then(term => {
                this.priceListTypes = x;
                this.priceListTypes.forEach(r => {
                    if (r.isProjectPriceList) {
                        r.name = `${r.name} (${term})`;
                    }
                })
                if (this.isNew)
                    this.selectedPriceListType = _.find(this.priceListTypes, { priceListTypeId: 0 });
            })
        });
    }

    private loadInvoicePaymentServices(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoicePaymentService, true, false).then(x => {
            this.invoicePaymentServices = x;
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.commonCustomerService.getVoucherSeriesByYear(accountYearId, false).then((x) => {
            this.voucherSeries = x;
        });
    }

    private loadCurrentAccountYear(): ng.IPromise<any> {
        return this.coreService.getCurrentAccountYear().then((x) => {
            //this.currentAccountYearId =  x.accountYearId;
            this.currentAccountYearId = x != null ? x.accountYearId : 0;
        });
    }


    private loadAccountYear(date: Date) {
        var prevAccountYearId = this.invoiceAccountYearId;

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
        this.setDefaultVatRate();
    }

    private setDefaultVatRate() {
        if (this.defaultVatRate === 0)
            this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;

        this.vatRate = this.defaultVatRate;
    }

    private loadCustomer(customerId: number, customerEdit = false) {
        if (customerId) {
            this.commonCustomerService.getCustomer(customerId, false, true, false, false, false, false).then(x => {
                this.customer = x;

                if (!this.selectedCustomer.name) {
                    this.selectedCustomer.name = `${this.customer.customerNr} ${this.customer.name}`
                    if (!this.customers.find(c => c.id === this.selectedCustomer.id)) {
                        this.customers.push(this.selectedCustomer);
                    }
                    this._selectedCustomer = { ...this.selectedCustomer }
                }

                this.loadingCustomer = false;
                this.customerChanged(customerEdit);
            });
        } else {
            this.customer = null;
            this.customerChanged(customerEdit);
        }
    }

    private loadDeliveryCustomer() {
        if (this.selectedDeliveryCustomer && this.selectedDeliveryCustomer.id !== 0) {
            this.commonCustomerService.getCustomer(this.selectedDeliveryCustomer.id, true, true, true, false, false, false).then(x => {
                this.deliveryCustomer = x;
                this.loadingDeliveryCustomer = false;
                if (this.deliveryCustomer) {

                    //handle inactive customer
                    if (!this.selectedDeliveryCustomer.name) {
                        this.selectedDeliveryCustomer.name = `${this.deliveryCustomer.customerNr} ${this.deliveryCustomer.name}`
                        if (!this.customers.find(c => c.id === this.selectedDeliveryCustomer.id)) {
                            this.customers.push(this.selectedDeliveryCustomer);
                        }
                        this._selectedDeliveryCustomer = { ...this.selectedDeliveryCustomer }
                    }

                    if (!this.invoice.invoiceId || this.invoice.invoiceId === 0) {
                        if (this.deliveryCustomer.payingCustomerId && this.deliveryCustomer.payingCustomerId > 0) {
                            if (!this.selectedCustomer)
                                this.selectedCustomer = _.find(this.customers, c => c.id === this.deliveryCustomer.payingCustomerId);
                            else
                                this.customerChanged();
                        }
                        else if (!this.selectedCustomer) {
                            this.selectedCustomer = _.find(this.customers, c => c.id === this.deliveryCustomer.actorCustomerId);
                            this.customer = this.deliveryCustomer;
                            this.customerChanged();
                        }
                    }
                    else {
                        this.customerChanged();
                    }
                }
            });
        }
        else {
            this.selectedDeliveryCustomer = null;
            this.customerChanged();
        }
    }

    private loadCustomerReferences(customerId: number) {
        this.commonCustomerService.getCustomerReferences(customerId, true).then(x => {
            this.customerReferences = x;

            // Add customer invoice reference to list
            if (this.customer.invoiceReference) {
                this.customerReferences.splice(1, 0, {
                    id: 1, name: this.customer.invoiceReference
                });
            }

            if (!_.find(this.customerReferences, { 'name': this.invoice.referenceYour }) && this.resetReference)
                this.invoice.referenceYour = '';

            if ((this.isNew || (StringUtility.isEmpty(this.invoice.referenceYour) && this.resetReference)) && this.customerReferences.length > 1 && this.customer.invoiceReference)
                this.invoice.referenceYour = this.customerReferences[1].name;

            this.resetReference = false;
        });
    }

    private loadCustomerEmails(customerId: number): ng.IPromise<any> {
        return this.commonCustomerService.getCustomerEmails(customerId, true, true).then(x => {
            this.customerEmails = x;
            if (!this.isNew && this.invoice.customerEmail) {
                this.customerEmails[0].name = this.invoice.customerEmail;
                this.invoice.contactEComId = 0;
            }
            else if (this.isNew) {
                if (this.customer.contactEComId) {
                    this.invoice.contactEComId = this.customer.contactEComId;
                }
                else if (this.customerEmails.length > 1) {
                    this.invoice.contactEComId = this.customerEmails[1].id;
                }
            }
        });
    }

    private getFreightAmount() {
        if (this.loadingInvoice || !this.invoice || this.productRowsRendered || !this.useFreightAmount)
            return;

        if (this.ignoreReloadFreightAmount) {
            this.ignoreReloadFreightAmount = false;
            return;
        }

        this.productService.getProductPriceDecimal(this.invoice.priceListTypeId, this.freightAmountProductId).then(x => {
            if (this.invoice.billingType === TermGroup_BillingType.Credit)
                x = 0;

            if (x && this.invoice.freightAmount !== x) {
                this.invoice.freightAmount = x;
                this.currencyHelper.amountOrCurrencyChanged(this.invoice);

                if (!this.productRowsRenderFinalized)
                    this.freightAmountUpdated = true;
            }
        });
    }

    private getInvoiceFee() {
        if (this.loadingInvoice || !this.invoice || this.productRowsRendered || !this.useInvoiceFee || this.disableInvoiceFee)
            return;

        if (this.ignoreReloadInvoiceFee) {
            this.ignoreReloadInvoiceFee = false;
            return;
        }

        this.productService.getProductPriceDecimal(this.invoice.priceListTypeId, this.invoiceFeeProductId).then(x => {
            if (this.invoice.billingType === TermGroup_BillingType.Credit)
                x = 0;

            if (x && this.invoice.invoiceFee !== x) {
                this.invoice.invoiceFee = x;
                this.currencyHelper.amountOrCurrencyChanged(this.invoice);

                if (!this.productRowsRenderFinalized)
                    this.invoiceFeeUpdated = true;
            }
        });
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
                    this.changeBillingType((this.invoice.billingType !== TermGroup_BillingType.Credit && oldValue === TermGroup_BillingType.Credit) || (this.invoice.billingType === TermGroup_BillingType.Credit && oldValue !== TermGroup_BillingType.Credit));
                }, (reason) => {
                    // User cancelled, revoke to previous billing type
                    this.invoice.billingType = oldValue;
                });
            });
        } else {
            this.$timeout(() => {
                this.changeBillingType((this.invoice.billingType !== TermGroup_BillingType.Credit && oldValue === TermGroup_BillingType.Credit) || (this.invoice.billingType === TermGroup_BillingType.Credit && oldValue !== TermGroup_BillingType.Credit));
            });
        }
    }

    private changeBillingType(reverse = false) {
        // Switch sign on total amount
        this.$timeout(() => {
            //if ((this.isCredit && this.invoice.totalAmountCurrency > 0) || (!this.isCredit && this.invoice.totalAmountCurrency < 0))
            //    this.invoice.totalAmountCurrency = -this.invoice.totalAmountCurrency;

            this.currencyHelper.isCreditChanged(this.isCredit);
            if (reverse)
                this.$scope.$broadcast('reverseRowAmounts', {});

            this.getFreightAmount();
            this.getInvoiceFee();
        });
    }

    private vatTypeChanging(oldValue) {
        this.$timeout(() => {
            if (this.hasHouseholdTaxDeduction && (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || this.invoice.vatType === TermGroup_InvoiceVatType.NoVat)) {
                const keys: string[] = [
                    "core.warning",
                    "common.customer.invoices.householdvattypewarning"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.householdvattypewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    this.invoice.vatType = oldValue;
                });
                return;
            }
            // Only show warning if amount is entered and user has manually modified any row
            else if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows() && !this.isNew) {
                const keys: string[] = [
                    "core.warning",
                    "common.customer.invoices.vattypechangewarning"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    var modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.vattypechangewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (this.productRowsRendered)
                            this.$scope.$broadcast('vatTypeChanged');
                        else
                            this.vatTypeUpdated = true;
                    }, (reason) => {
                        // User cancelled, revoke to previous vat type
                        this.invoice.vatType = oldValue;
                    });
                });
            } else {
                if (this.productRowsRendered)
                    this.$scope.$broadcast('vatTypeChanged');
                else
                    this.vatTypeUpdated = true;
            }
        });
    }

    private changeVatType() {
        this.$timeout(() => {
            //Quarantine
            //this.generateAccountingRows(true);
        });
    }

    private isBaseCurrencyAndSameCurrency():boolean {
        return (this.currencyHelper.isBaseCurrency && this.currencyHelper.currencyId === this.invoice.currencyId);
    }

    private currencyChanged() {
        if (this.invoice && !this.loadingInvoice && !this.creditProductRows && !this.isBaseCurrencyAndSameCurrency() ) {
            this.currencyHelper.toInvoice(this.invoice);

            this.currencyHelper.amountOrCurrencyChanged(this.invoice);
            this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
            this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);

            this.$scope.$broadcast('recalculateTotals', { guid: this.guid });

            this.hasModifiedProductRows = true;
        }
    }

    private currencyIdChanged() {
        if (this.invoice && !this.loadingInvoice && !this.creditProductRows) {
            if (this.triangulationSales) {
                this.$timeout(() => {
                    const settingCustomer = this.deliveryCustomer && this.showPayingCustomer ? this.deliveryCustomer : this.customer;
                    this.invoice.triangulationSales = !this.currencyHelper.isBaseCurrency && settingCustomer ? settingCustomer.triangulationSales : false;
                });
            }
        }
    }

    private customerChanged(customerEdit: boolean = false) {
        if (this.loadingCustomer || this.loadingDeliveryCustomer)
            return;

        // Set customer dependant values
        let settingCustomer = this.customer;
        let deliveryCustomer = this.deliveryCustomer && this.showPayingCustomer ? this.deliveryCustomer : this.customer;

        this.$scope.$broadcast('updateCustomer', { customer: this.customer, getFreight: true });

        //Disable invoice fee
        this.disableInvoiceFee = this.customer && this.customer.disableInvoiceFee;
        const oldVatType = this.invoice.vatType;
        if (!this.loadingInvoice) {
            if (settingCustomer && settingCustomer.currencyId)
                this.invoice.currencyId = settingCustomer.currencyId;

            this.currencyHelper.currencyId = this.invoice.currencyId;

            // Wholeseller
            this.invoice.sysWholeSellerId = settingCustomer && settingCustomer.sysWholeSellerId ? settingCustomer.sysWholeSellerId : this.defaultWholesellerId;

            // Price list
            this.setPriceListType(settingCustomer && settingCustomer.priceListTypeId ? settingCustomer.priceListTypeId : this.defaultPriceListTypeId);

            // VAT
            this.invoice.vatType = settingCustomer && settingCustomer.vatType !== TermGroup_InvoiceVatType.None ? settingCustomer.vatType : this.invoiceEditHandler.defaultVatType;
            this.setVatRate();

            // Delivery type
            this.invoice.deliveryTypeId = deliveryCustomer && deliveryCustomer.deliveryTypeId ? deliveryCustomer.deliveryTypeId : this.defaultDeliveryTypeId;

            // Delivery condition
            this.invoice.deliveryConditionId = deliveryCustomer && deliveryCustomer.deliveryConditionId ? deliveryCustomer.deliveryConditionId : this.defaultDeliveryConditionId;

            this.invoice.invoiceDeliveryType = settingCustomer && settingCustomer.invoiceDeliveryType ? settingCustomer.invoiceDeliveryType : 0;
            this.invoice.invoiceDeliveryProvider = settingCustomer && settingCustomer.invoiceDeliveryProvider ? settingCustomer.invoiceDeliveryProvider : this.getDefaultInvoiceProvider();

            // Attachments
            this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice = this.customer && this.customer.addAttachementsToEInvoice ? this.customer.addAttachementsToEInvoice : false;
            this.invoiceFilesHelper.addSupplierInvoicesToEInvoice = this.invoice.addSupplierInvoicesToEInvoice = this.customer && this.customer.addSupplierInvoicesToEInvoice ? this.customer.addSupplierInvoicesToEInvoice : false;

            // GLN number
            this.invoice.contactGLNId = settingCustomer.contactGLNId;

            // Contract number
            this.invoice.contractNr = settingCustomer.contractNr;

            if (!customerEdit || !this.invoice.invoiceLabel) {
                this.invoice.invoiceLabel = settingCustomer.invoiceLabel;
            }

            // Payment
            this.invoice.paymentConditionId = this.customer && this.customer.paymentConditionId ? this.customer.paymentConditionId : 0;
            this.paymentConditionChanged(this.invoice.paymentConditionId);

            // Payment service
            this.invoice.invoicePaymentService = this.customer && this.customer.invoicePaymentService ? this.customer.invoicePaymentService : 0;
            this.paymentServiceReadOnly = this.customer && (this.customer.invoicePaymentService && this.customer.invoicePaymentService > 0);

            // Invoice fee
            if (this.disableInvoiceFee)
                this.invoice.invoiceFee = this.invoice.invoiceFeeCurrency = 0;

            // Note
            if (settingCustomer && settingCustomer.showNote && settingCustomer.note) {
                this.invoiceEditHandler.showCustomerNote(settingCustomer);
            }

            if (this.customer) {
                this.invoiceEditHandler.showCustomerBlockNote(this.customer, OrderInvoiceRegistrationType.Invoice);
            }

            //Cash sales
            this.invoice.cashSale = this.useCashSales && settingCustomer.isCashCustomer;
            if (this.invoice.cashSale)
                this.definitive = false;

            this.getFreightAmount();
            this.getInvoiceFee();
        }

        if (settingCustomer) {
            this.loadCustomerReferences(settingCustomer.actorCustomerId);
            this.loadCustomerEmails(this.customer.actorCustomerId);
            this.invoiceEditHandler.loadCustomerGLNs(settingCustomer);
            this.loadAddresses(settingCustomer, deliveryCustomer, !this.loadingInvoice || this.isNew);

            if (this.isNew) {
                if (this.productRowsRendered)
                    this.$scope.$broadcast('removeInterestReminderRows', {});
                this.loadCustomerInvoiceReminders(settingCustomer.actorCustomerId);
                this.loadCustomerInvoiceInterests(settingCustomer.actorCustomerId);
            }

            if (settingCustomer.creditLimit)
                this.checkCustomerCreditLimit(settingCustomer.actorCustomerId, settingCustomer.creditLimit);
        }
        else {
            this.customerReferences = [];
            this.customerEmails = [];
            this.invoiceEditHandler.deliveryAddresses = [];
            this.invoiceEditHandler.invoiceAddresses = [];
        }

        if (this.invoice.vatType !== oldVatType && !this.loadingInvoice)
            this.vatTypeChanging(oldVatType);
        this.setLocked();
        this.invoiceLoaded();
    }

    private loadAddresses(settingCustomer: CustomerDTO, deliveryCustomer: CustomerDTO, setFirstAsDefault: boolean) {
        this.$q.all(
            [this.invoiceEditHandler.loadDeliveryAddresses(deliveryCustomer.actorCustomerId),
            this.invoiceEditHandler.loadInvoiceAddresses(settingCustomer.actorCustomerId)]
        ).then(() => {

            //delivery
            if (setFirstAsDefault && this.invoiceEditHandler.deliveryAddresses.length > 1)
                this.invoice.deliveryAddressId = this.invoiceEditHandler.deliveryAddresses[1].contactAddressId;
            else if (this.invoice.invoiceHeadText != null && this.invoice.invoiceHeadText != "")
                this.invoiceEditHandler.deliveryAddresses[0].address = this.invoice.invoiceHeadText;

            //invoice
            if (setFirstAsDefault && this.invoiceEditHandler.invoiceAddresses.length > 1)
                this.invoice.billingAddressId = this.invoiceEditHandler.invoiceAddresses[1].contactAddressId;
            else if (this.isNew && this.useDeliveryAddressAsInvoiceAddress && this.invoice.deliveryAddressId && !this.invoice.billingAddressId) {
                this.invoice.billingAdressText = this.invoiceEditHandler.formatDeliveryAddress(_.filter(this.invoiceEditHandler.deliveryAddresses, i => i.contactAddressId == this.invoice.deliveryAddressId)[0].contactAddressRows, settingCustomer.isFinvoiceCustomer);
                this.invoiceEditHandler.invoiceAddresses[0].address = this.invoice.billingAdressText;
                this.invoice.billingAddressId = 0;
            }
            else if (this.invoice.billingAdressText != null && this.invoice.billingAdressText != "")
                this.invoiceEditHandler.invoiceAddresses[0].address = this.invoice.billingAdressText;
        });
    }

    private loadCustomerInvoiceReminders(customerId: number) {
        return this.commonCustomerService.getCustomerInvoiceReminders(customerId, true, true).then(x => {
            if (!x || x.length === 0)
                return;

            // Fix dates
            _.forEach(x, (y) => {
                if (y.dueDate)
                    y.dueDate = CalendarUtility.convertToDate(y.dueDate);
            });

            const keys: string[] = [
                "core.delete",
                "core.fileupload.removeall",
                "common.customer.invoices.remindersheader",
                "common.customer.invoices.reminderstext"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["common.customer.invoices.remindersheader"], terms["common.customer.invoices.reminderstext"].format(x[0].customerName, x.length), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, { buttonCancelLabelKey: x.length === 1 ? terms["core.delete"] : terms["core.fileupload.removeall"] });
                modal.result.then(val => {
                    var save: boolean = true;
                    if (val === true) {
                        if (!this.productRowsRendered) {
                            this.pendingInvoiceReminders = x;
                            this.openProductRowExpander();
                        }
                        else {
                            this.$scope.$broadcast('addReminderRows', { reminders: x });
                        }
                    }
                }, (reason) => {
                    this.deleteCustomerInvoiceReminders(customerId);
                });
            });
        });
    }

    private deleteCustomerInvoiceReminders(customerId: number) {
        this.progress.startDeleteProgress((completion) => {
            return this.commonCustomerService.deleteCustomerInvoiceReminders(customerId).then(x => {
                completion.completed(null, true);
            });
        });
    }

    private loadCustomerInvoiceInterests(customerId: number) {
        return this.commonCustomerService.getCustomerInvoiceInterests(customerId, true, true).then(x => {
            if (!x || x.length === 0)
                return;
            // Fix dates
            _.forEach(x, (y) => {
                if (y.dueDate)
                    y.dueDate = CalendarUtility.convertToDate(y.dueDate);
                if (y.payDate)
                    y.payDate = CalendarUtility.convertToDate(y.payDate);
            });


            var keys: string[] = [
                "core.delete",
                "core.fileupload.removeall",
                "common.customer.invoices.interestsheader",
                "common.customer.invoices.intereststext"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["common.customer.invoices.interestsheader"], terms["common.customer.invoices.intereststext"].format(x[0].customerName, x.length), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, { buttonCancelLabelKey: x.length === 1 ? terms["core.delete"] : terms["core.fileupload.removeall"] });
                modal.result.then(val => {
                    var save: boolean = true;
                    if (val === true) {
                        if (!this.productRowsRendered) {
                            this.pendingInvoiceInterests = x;
                            this.openProductRowExpander();
                        }
                        else {
                            this.$scope.$broadcast('addInterestRows', { interests: x });
                        }
                    }
                }, (reason) => {
                    this.deleteCustomerInvoiceInterests(customerId);
                });
            });
        });
    }

    private deleteCustomerInvoiceInterests(customerId: number) {
        this.progress.startDeleteProgress((completion) => {
            return this.commonCustomerService.deleteCustomerInvoiceInterests(customerId).then(x => {
                completion.completed(null, true);
            });
        });
    }

    private checkCustomerCreditLimit(customerId: number, creditLimit: number) {
        if (!creditLimit || creditLimit === 0)
            return;

        this.commonCustomerService.checkCustomerCreditLimit(customerId, creditLimit).then(limit => {
            if (limit) {
                if (this.customer.creditLimit && this.customer.creditLimit < limit) {
                    var message;

                    if (this.customer != null && this.customer.creditLimit) {
                        this.currentBalance = this.customer.creditLimit - limit;
                        var filter: Function = this.$filter("amount");
                        message = this.terms["billing.order.creditlimit.message1"].format(filter(this.customer.creditLimit), filter(limit - this.customer.creditLimit));
                    }
                    else {
                        var filter: Function = this.$filter("amount");
                        message = this.terms["billing.order.creditlimit.message2"] + " " + filter(limit.toString());
                    }

                    var modal = this.notificationService.showDialog(this.terms["billing.order.creditlimit"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    modal.result.then(val => {
                        this.showValidationError();
                    });
                }
                else {
                    this.currentBalance = this.customer.creditLimit - limit;
                }
            }
        });
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
        });
    }

    private convertAmount(field: string, amount: number) {
        if (this.loadingInvoice)
            return;

        // Call amount currency converter in accounting rows directive
        var item = {
            field: field,
            amount: amount ? amount : 0,
            sourceCurrencyType: TermGroup_CurrencyType.TransactionCurrency
        };
        this.$scope.$broadcast('amountChanged', item);
    }

    private amountConverted(item) {

        if (item.parentRecordId === this.invoice.invoiceId) {
            // Result from amount currency converter in accounting rows directive
            this.invoice[item.field] = item.baseCurrencyAmount;
            this.invoice[item.field + 'Currency'] = item.transactionCurrencyAmount;
            this.invoice[item.field + 'EnterpriceCurrency'] = item.enterpriseCurrencyAmount;
            this.invoice[item.field + 'LedgerCurrency'] = item.ledgerCurrencyAmount;
        }
    }

    private setValuesFromShiftType(shiftType: ShiftTypeGridDTO) {
        // Set default length from shift type if not already specified
        if (shiftType && shiftType.defaultLength > 0 && this.invoice.estimatedTime === 0) {
            this.invoice.estimatedTime = shiftType.defaultLength;
            this.invoice.remainingTime = shiftType.defaultLength;
        }
    }

    private getDefaultInvoiceProvider():number {
        if (this.eInvoiceFormat == TermGroup_EInvoiceFormat.Intrum) {
            return SoeInvoiceDeliveryProvider.Intrum;
        }
        else {
            return SoeInvoiceDeliveryProvider.Unknown;
        }
    }

    // ACTIONS

    private new(prevInvoiceId = null, keepInternalAccounts = true) {
        this.isNew = true;
        this.allowModifyDefinitive = this.draftToOriginPermission;
        this.invoiceExpanderIsOpen = true;
        this.showDeleteButton = false;
        this.showRevokeButton = false;
        this.showCreditButton = false;

        var dim2Id;
        var dim3Id;
        var dim4Id;
        var dim5Id;
        var dim6Id;

        if (this.invoice && keepInternalAccounts) {
            dim2Id = this.invoice.defaultDim2AccountId;
            dim3Id = this.invoice.defaultDim3AccountId;
            dim4Id = this.invoice.defaultDim4AccountId;
            dim5Id = this.invoice.defaultDim5AccountId;
            dim6Id = this.invoice.defaultDim6AccountId;
        }

        this.invoiceId = 0;
        this.invoice = new BillingInvoiceDTO();

        if (prevInvoiceId)
            this.invoice.prevInvoiceId = prevInvoiceId;

        this.invoice.originStatus = SoeOriginStatus.Draft;
        this.invoice.orderType = TermGroup_OrderType.Project;
        if (this.fixedPriceOrderTypes.length > 0)
            this.selectedFixedPriceOrder = _.find(this.fixedPriceOrderTypes, { id: OrderContractType.Continuous });
        this.invoice.invoiceText = this.defaultInvoiceText;
        this.invoice.vatType = this.invoiceEditHandler.defaultVatType;
        this.invoice.sysWholeSellerId = this.defaultWholesellerId;
        this.invoice.includeOnInvoice = this.includeWorkDescriptionOnInvoice;
        this.invoice.deliveryTypeId = this.defaultDeliveryTypeId;
        this.invoice.deliveryConditionId = this.defaultDeliveryConditionId;
        this.invoice.paymentConditionId = this.invoiceEditHandler.defaultPaymentConditionId;
        this.invoice.voucherSeriesId = this.invoiceEditHandler.defaultVoucherSeriesId;
        this.invoice.voucherSeriesTypeId = this.voucherSeries.find(vs => vs.voucherSeriesId == this.invoiceEditHandler.defaultVoucherSeriesId).voucherSeriesTypeId;
        this.invoice.currencyId = this.invoiceEditHandler.currencies[0].currencyId;    // Base currency is first in collection
        this.invoice.currencyDate = this.currencyHelper.currencyDate = CalendarUtility.getDateToday();
        this.invoice.currencyRate = this.currencyHelper.currencyRate;
        this.invoice.freightAmount = this.invoice.freightAmountCurrency = 0;
        this.invoice.invoiceFee = this.invoice.invoiceFeeCurrency = 0;
        this.invoice.billingType = TermGroup_BillingType.Debit;
        this.invoice.customerInvoiceRows = [];
        this.invoice.invoiceDeliveryProvider = this.getDefaultInvoiceProvider();
        
        if (this.isTemplateRegistration)
            this.invoice.isTemplate = true;

        // Get origin status text for new
        this.translationService.translate("core.new").then(term => {
            this.invoice.originStatusName = term;
        });

        this.setOurReference();
        this.setPriceListType(0);   // Will set default
        this.setLocked();

        if (this.customerId)
            this.selectedCustomer = _.find(this.customers, c => c.id === this.customerId);
        else
            this.selectedCustomer = null;

        if (keepInternalAccounts) {
            this.invoice.defaultDim2AccountId = dim2Id;
            this.invoice.defaultDim3AccountId = dim3Id;
            this.invoice.defaultDim4AccountId = dim4Id;
            this.invoice.defaultDim5AccountId = dim5Id;
            this.invoice.defaultDim6AccountId = dim6Id;
        }

        this.invoiceFilesHelper.reset();
        this.originUserHelper.clear();
        this.originUserHelper.setDefaultUser();
        if (this.useOneTimeCustomer && this.defaultOneTimeCustomerId > 0)
            this.selectedCustomer = _.find(this.customers, c => c.id === this.defaultOneTimeCustomerId);
    }

    private createNewInvoiceFromHousehold() {
        var originCustomerId = this.invoice.actorId;
        var originHouseholdRow = _.find(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId === this.originalCustomerInvoiceRowId);
        if (originCustomerId && originHouseholdRow) {
            //Create new
            this.new(this.invoice.invoiceId, true);

            //Set properties from previous invoice
            this.selectedCustomer = _.find(this.customers, c => c.id === originCustomerId);

            //Set row properties
            this.householdAmount = this.partialHouseholdAmount && this.partialHouseholdAmount < 0 ? this.partialHouseholdAmount : originHouseholdRow.amount;
            this.householdQuantity = originHouseholdRow.quantity;
            this.openProductRowExpander();
        }
    }

    private openProductRowExpander() {
        if (!this.productRowsRendered) {
            this.productRowsRendered = true;
            this.productRowsExpanderIsOpen = true;
        }
    }

    private executeProjectFunction(option) {
        switch (option.id) {
            case OrderEditProjectFunctions.Create:
                this.openProject(true);
                break;
            case OrderEditProjectFunctions.Link:
                this.openSelectProject();
                break;
            case OrderEditProjectFunctions.Change:
                this.openSelectProject();
                break;
            case OrderEditProjectFunctions.Remove:
                this.removeProject();
                break;
            case OrderEditProjectFunctions.Open:
                this.openProject(false);
                break;
        }
    }

    private paymentConditionChanged(paymentConditionId: number) {
        if (this.invoice) {
            this.setPaymentCondition(paymentConditionId);
        }
    }

    private openProject(newProject: boolean) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Projects/Views/edit.html"),
            controller: BillingProjectsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, id: this.invoice.projectId ? this.invoice.projectId : 0 });
        });

        modal.result.then(result => {
            if (newProject) {
                this.setProjectValues(result.id, result.number);
                this.$scope.$broadcast(Constants.EVENT_RELOAD_ACCOUNT_DIMS, {});
            }
        });
    }

    private openSelectProject() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectProject", "selectproject.html"),
            controller: SelectProjectController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                projects: () => { return null },
                customerId: () => { return this.invoice.actorId },
                projectsWithoutCustomer: () => { return this.showProjectsWithoutCustomer },
                showFindHidden: () => { return null },
                loadHidden: () => { return true },
                useDelete: () => { return false },
                currentProjectNr: () => { return null },
                currentProjectId: () => { return null },
                excludedProjectId: () => { return null },
                showAllProjects: () => { return false },
            }
        });

        modal.result.then(project => {
            const projectId: number = (project ? project.projectId : 0);

            if (this.invoice.projectId)
                this.changeProject(projectId);

            this.setProjectValues(projectId, project ? project.number : '');
        });
    }

    private changeProject(projectId: number) {
        this.progress.startSaveProgress((completion) => {
            this.orderService.changeProjectOnInvoice(projectId, this.invoice.invoiceId, SoeProjectRecordType.Invoice, false).then(result => {
                if (result.success) {
                    this.load();
                    completion.completed("", this.invoice);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            })
        }, this.guid).then(data => {


        }, error => {

        });
    }

    private removeProject() {
        if (!this.invoice.projectId)
            return;

        if (this.isDisabled()) {
            var keys: string[] = [
                "core.verifyquestion",
                "billing.order.project.removeproject.question"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["billing.order.project.removeproject.question"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.invoice.projectId = null;
                    this.setAsDirty();
                    this.save(false);
                });
            });
        }
    }

    private setProjectValues(projectId: number, projectNr: string) {
        this.orderService.getProject(projectId).then((project: ProjectDTO) => {
            this.invoice.projectId = projectId;
            this.invoice.projectNr = projectNr;
            this.invoice.printTimeReport = this.projectIncludeTimeProjectReport;
            this.invoice.includeOnlyInvoicedTime = this.projectIncludeOnlyInvoicedTimeInTimeProjectReport;

            if (project.priceListTypeId) {
                this.setPriceListType(project.priceListTypeId);
                this.setAsDirty();
            }

            var date = CalendarUtility.getDateToday();
            this.timeProjectFrom = date.beginningOfWeek();
            this.timeProjectTo = date.endOfWeek();
            this.loadTimeProjectRows();
        });
    }

    private executeSaveFunction = _.debounce((option) => {
        this.executing = true;
        switch (option.id) {
            case OrderEditSaveFunctions.Save:
                this.save(false);
                break;
            case OrderEditSaveFunctions.SaveAndClose:
                this.save(true);
                break;
        }
    }, 500, { leading: false, trailing: true });

    private executePrintFunction(option) {

        if (this.invoice) {
            switch (option.id) {
                case OrderInvoiceEditPrintFunctions.Print:
                case OrderInvoiceEditPrintFunctions.ReportDialog:
                    case OrderInvoiceEditPrintFunctions.PrintWithAttachments:
                    this.initPrint(option);
                    break;
                case OrderInvoiceEditPrintFunctions.eMail:
                    this.showEmailDialog();
                    break;
                case OrderInvoiceEditPrintFunctions.EInvoice:
                    this.tryCreateEInvoice(false);
                    break;
                case OrderInvoiceEditPrintFunctions.EInvoiceDownload:
                    this.tryCreateEInvoice(true);
                    break;
            }
        }
    }

    private setCashSalesDefinitive() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Invoices/Dialogs/CashSalesDefinitive/CashSalesDefinitive.html"),
            controller: CashSalesDefinitiveController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                commonCustomerService: () => { return this.commonCustomerService },
                invoiceId: () => { return this.invoice.invoiceId },
                emails: () => { return this.customerEmails },
                contactEComId: () => { return this.invoice.contactEComId }
            }
        });

        modal.result.then((result) => {
            if (result) {
                this.definitive = true;
                this.save(false, null, null, null, result.sendEmail, result.email, !result.sendEmail);
            }
        }, (reason) => {
        });
    }

    private tryCreateEInvoice(download: boolean) {
        //check if invoice is preliminary and save it as definitive
        if (this.invoice && this.invoice.originStatus.valueOf() === SoeOriginStatus.Draft) {
            const text: string = download ? this.terms["billing.invoices.finvoiceasdefinitive"] : this.terms["billing.invoices.einvoiceasdefinitive"];

            const modal = this.notificationService.showDialog(this.terms["core.info"], text, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            modal.result.then((val) => {
                this.definitive = true;
                this.setAsDirty();
                this.save(false, false, true, download);
            });
        }
        else if (download && this.invoice.statusIcon && this.invoice.statusIcon == SoeStatusIcon.DownloadEinvoice) {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.einvoice.redownloadedit"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val)
                    this.createEInvoice(download);
            })
        }
        else {
            const alreadySent: boolean = ((this.invoice.statusIcon & Number(SoeStatusIcon.ElectronicallyDistributed)) == Number(SoeStatusIcon.ElectronicallyDistributed));

            if (alreadySent) {

                const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["billing.invoices.hasbeensentelectronically"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.createEInvoice(download);
                })
            }
            else { this.createEInvoice(download); }
        }
    }

    private createEInvoice(download: boolean, overrideWarnings = false) {
        if (this.invoiceId) {
            this.progress.startSaveProgress((completion) => {
                this.invoiceService.createEInvoice(this.invoiceId, download, overrideWarnings).then((result: IActionResult) => {
                    if (result.success) {
                        if (result.integerValue2 && (this.eInvoiceFormat === TermGroup_EInvoiceFormat.Finvoice || this.eInvoiceFormat === TermGroup_EInvoiceFormat.Finvoice2 || this.eInvoiceFormat === TermGroup_EInvoiceFormat.Finvoice3)) {

                            this.doDownload(result.stringValue, result.integerValue2);

                            if (result.keys && result.keys.length > 0 && result.strings.length > 0) {
                                this.doDownload(result.strings[0], result.keys[0]);
                            }
                        }
                        else {
                            this.translationService.translate("common.report.report.efakturaissent").then((text) => {
                                this.notificationService.showDialog(this.terms["core.info"], text, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                            });
                        }

                        // Activate reminder
                        this.$timeout(() => {
                            this.sentAsEInvoice = true;

                            this.showPrintInvoiceAsReminder = true;
                            this.printInvoiceAsReminderEnabled = this.reminderReportId > 0;
                            this.printInvoiceAsReminder = false;
                        });
                    }
                    else if (result.errorMessage && result.canUserOverride) {
                        var title: string = this.terms["core.warning"];
                        var text = result.errorMessage + "\n\n" + this.terms["common.customer.invoice.einvoicingoperatorvalidation"];
                        var modal = this.notificationService.showDialog(title, text, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);

                        modal.result.then(val => {
                            if (result.canUserOverride) {
                                this.createEInvoice(download, result.canUserOverride);
                            } else {
                                this.notificationService.showDialog(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                completion.failed(result.errorMessage);
                            }
                        }).catch(err => {
                            completion.failed(result.errorMessage);
                        });
                    }
                    else {
                        this.notificationService.showDialog(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                        completion.failed(result.errorMessage);
                    }
                }).finally(() => {
                    completion.completed(null, null, true);
                })
            }, error => {
                this.notificationService.showDialog(this.terms["core.error"], error.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }
    }

    private GetOrderChecklists(): ng.IPromise<any> {
        if(!this.invoice || !this.invoice.hasOrder) {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }

        return this.invoiceService.getInvoiceFromOrderCheckLists(this.invoice.invoiceId).then((x) => {
            this.checklists = x;
            return this.checklists;
        });
    }

    private initPrint(option, bypassValidation = false) {
        if (!bypassValidation && this.invoice && this.invoice.originStatus.valueOf() === SoeOriginStatus.Draft) {
            const keys: string[] = [
                "core.warning",
                "common.customer.invoices.preliminaryprint",
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.preliminaryprint"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then((val) => {
                    this.initPrint(option, true);
                });
            });
        }
        else {
            switch (option.id) {
                case OrderInvoiceEditPrintFunctions.Print:
                    this.executePrintRequest(this.customer && this.customer.billingTemplate ? this.customer.billingTemplate : 0, 0, this.printInvoiceAsCopy);
                    break;
                case OrderInvoiceEditPrintFunctions.ReportDialog:
                    this.printFromDialog();
                    break;
                case OrderInvoiceEditPrintFunctions.PrintWithAttachments:
                    this.executePrintWithAttachmentsRequest(this.customer && this.customer.billingTemplate ? this.customer.billingTemplate : 0, 0, this.printInvoiceAsCopy);
                    break;
            }
        }
    }

    private executePrintRequest(reportId: number, languageId: number, copy: boolean) {
        this.progress.startWorkProgress((completion) => {
            let model: ICustomerInvoicePrintDTO = {
                reportId: this.printInvoiceAsReminder ? this.reminderReportId : reportId,
                ids: [this.invoice.invoiceId],
                queue: false,
                sysReportTemplateTypeId: 0,
                attachmentIds: [],
                checklistIds: [],
                includeOnlyInvoiced: this.invoice.includeOnlyInvoicedTime,
                orderInvoiceRegistrationType: OrderInvoiceRegistrationType.Invoice,
                printTimeReport: this.invoice.printTimeReport,
                invoiceCopy: copy,
                asReminder: this.printInvoiceAsReminder,
                mergePdfs: false,
                reportLanguageId: languageId,
            };
            return this.requestReportService.printCustomerInvoice(model).then(() => {
                completion.completed(null, true);
            });
        });
    }

    private executePrintWithAttachmentsRequest(reportId: number, languageId: number, copy: boolean) {
        this.progress.startWorkProgress((comp) => {
            this.$q.all([this.GetOrderChecklists(), this.invoiceFilesHelper.loadFiles()]).then(() => {
                if ((this.checklists && this.checklists.length > 0) || (this.invoiceFilesHelper.filesLoaded ? this.invoiceFilesHelper.nbrOfFiles > 0 : this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon))) {
                    const modal = this.modalInstance.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReportAndAttachments/SelectReportAndAttachments.html"),
                        controller: SelectReportAndAttachmentsController,
                        controllerAs: 'ctrl',
                        backdrop: 'static',
                        size: 'lg',
                        resolve: {
                            reportTypes: () => { return [] },
                            showCopy: () => { return this.invoice.billingInvoicePrinted },
                            copyValue: () => { return this.printInvoiceAsCopy },
                            showEmail: () => { return false },
                            langId: () => { return this.customer && this.customer.sysLanguageId ? this.customer.sysLanguageId : null },
                            showLangSelection: () => { return false },
                            attachments: () => { return _.filter(this.invoiceFilesHelper.files, (file) => { return !file['isDeleted'] }) },
                            checklists: () => { return this.checklists },
                            attachmentsSelected: () => { return this.invoice.addAttachementsToEInvoice },
                            showReportSelection: () => { return false }
                        }
                    });

                    modal.result.then((result: any) => {
                        this.progress.startWorkProgress((completion) => {
                            let model: ICustomerInvoicePrintDTO = {
                                reportId: this.printInvoiceAsReminder ? this.reminderReportId : reportId,
                                ids: [this.invoice.invoiceId],
                                queue: false,
                                sysReportTemplateTypeId: 0,
                                attachmentIds: result.attachmentIds,
                                checklistIds: result.checklistIds,
                                includeOnlyInvoiced: this.invoice.includeOnlyInvoicedTime,
                                orderInvoiceRegistrationType: OrderInvoiceRegistrationType.Invoice,
                                printTimeReport: this.invoice.printTimeReport,
                                invoiceCopy: result.createCopy,
                                asReminder: this.printInvoiceAsReminder,
                                mergePdfs: result.mergePdfs,
                                reportLanguageId: languageId,
                            };
                            return this.requestReportService.printCustomerInvoice(model).then(() => {
                                completion.completed(null, true);
                            });
                        });
                    });
                }
                else {
                    this.executePrintRequest(reportId, languageId, copy);
                }

                comp.completed(null, true);
            });
        });
    }

    private printOrder(reportId: number, languageId: number, copy: boolean, recipients: any[] = null, emailTemplate = 0) {
    this.invoice.billingInvoicePrinted = this.definitive;
    if (this.printInvoiceAsReminder)
        reportId = this.reminderReportId;

    this.reportService.getOrderPrintUrlSingle(this.invoice.invoiceId, recipients, reportId, languageId, this.invoice.invoiceNr, this.customer.actorCustomerId, this.invoice.printTimeReport, this.invoice.includeOnlyInvoicedTime, OrderInvoiceRegistrationType.Invoice, this.printInvoiceAsCopy, emailTemplate, this.printInvoiceAsReminder)
        .then((url) => {
            if (!this.printInvoiceAsCopy)
                this.printInvoiceAsCopy = true;

            HtmlUtility.openInSameTab(this.$window, url);
        });
}

    private showEmailDialog(reportId = 0) {
    if (this.invoice && this.invoice.originStatus.valueOf() === SoeOriginStatus.Draft) {
        const keys: string[] = [
            "core.warning",
            "common.customer.invoices.preliminaryemail",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.preliminaryemail"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            return;
        });
    }

    const keys: string[] = [
        "billing.invoices.invoice",
        "common.customer.invoices.reminder",
    ];

    return this.translationService.translateMany(keys).then((types) => {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectEmail/SelectEmail.html"),
            controller: SelectEmailController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                defaultEmail: () => { return this.invoice.contactEComId },
                defaultEmailTemplateId: () => { return this.emailTemplateId },
                recipients: () => { return this.customerEmails },
                attachments: () => { return this.invoiceFilesHelper.loadFiles().then(() => { return _.filter(this.invoiceFilesHelper.files, (file) => { return !file['isDeleted'] }) }) },
                attachmentsSelected: () => { return this.invoice.addAttachementsToEInvoice },
                checklists: () => { return this.GetOrderChecklists().then((x) => { return x }) },
                types: () => { return types },
                grid: () => { return false },
                type: () => { return this.isReminder ? EmailTemplateType.Reminder : EmailTemplateType.Invoice },
                showReportSelection: () => { return false },
                reports: () => { return [] },
                defaultReportTemplateId: () => { return null },
                langId: () => { return this.customer && this.customer.sysLanguageId ? this.customer.sysLanguageId : null }
            }
        });

        modal.result.then((result: any) => {
            let singleRecipient = "";
            const recs: number[] = [];
            const attachmentIds: number[] = [];
            const checklistIds: number[] = [];

            _.forEach(result.recipients, rec => {
                if (rec.id > 0)
                    recs.push(rec.id);
                else
                    singleRecipient = rec.name;
            });

            _.forEach(result.attachments, att => {
                attachmentIds.push(att.imageId ? att.imageId : att.id);
            });

            _.forEach(result.checklists, chk => {
                checklistIds.push(chk.checklistHeadRecordId);
            });

            const params = {
                invoiceId: this.invoice.invoiceId, invoiceNr: this.invoice.invoiceNr,
                actorCustomerId: this.customer.actorCustomerId,
                printTimeReport: this.invoice.printTimeReport,
                includeOnlyInvoicedTime: this.invoice.includeOnlyInvoicedTime,
                addAttachmentsToEinvoice: this.invoice.addAttachementsToEInvoice,
                attachmentIds: attachmentIds,
                checklistIds: checklistIds,
                singleRecipient: singleRecipient,
            }

            this.invoiceEditHandler.sendReport(params, OrderInvoiceRegistrationType.Invoice, true, reportId, 0, false, recs, result.emailTemplateId, result.mergePdfs);
        });
    });
}

    private printFromDialog() {
        const reportTypes: number[] = [];
        reportTypes.push(SoeReportTemplateType.BillingInvoice);
        if (this.invoice.billingInvoicePrinted) {
            reportTypes.push(SoeReportTemplateType.BillingInvoiceReminder);
        }
        if ((this.definitive) && (!this.isCredit)) {
            reportTypes.push(SoeReportTemplateType.BillingInvoiceInterest);
        }

        this.progress.startWorkProgress((comp) => {
            this.$q.all([this.GetOrderChecklists(), this.invoiceFilesHelper.loadFiles()]).then(() => {
                const modal = this.modalInstance.open({
                    templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReportAndAttachments/SelectReportAndAttachments.html"),
                    controller: SelectReportAndAttachmentsController,
                    controllerAs: 'ctrl',
                    backdrop: 'static',
                    size: 'lg',
                    resolve: {
                        reportTypes: () => { return reportTypes },
                        showCopy: () => { return this.invoice.billingInvoicePrinted },
                        showEmail: () => { return true },
                        copyValue: () => { return this.printInvoiceAsCopy },
                        langId: () => { return this.customer?.sysLanguageId ?? null },
                        showLangSelection: () => { return true },
                        attachments: () => { return _.filter(this.invoiceFilesHelper.files, (file) => { return !file['isDeleted'] }) },
                        checklists: () => { return this.checklists },
                        attachmentsSelected: () => { return this.invoice.addAttachementsToEInvoice },
                        showReportSelection: () => { return true }
                    }
                });

                modal.result.then((result: any) => {
                    if (result?.reportId) {
                        if (result.email)
                            this.showEmailDialog(result.reportId);
                        else {
                            this.progress.startWorkProgress((completion) => {
                                let model: ICustomerInvoicePrintDTO = {
                                    reportId: result.reportId,
                                    ids: [this.invoice.invoiceId],
                                    queue: false,
                                    sysReportTemplateTypeId: 0,
                                    attachmentIds: result.attachmentIds,
                                    checklistIds: result.checklistIds,
                                    includeOnlyInvoiced: this.invoice.includeOnlyInvoicedTime,
                                    orderInvoiceRegistrationType: OrderInvoiceRegistrationType.Invoice,
                                    printTimeReport: this.invoice.printTimeReport,
                                    invoiceCopy: result.createCopy,
                                    asReminder: this.printInvoiceAsReminder,
                                    mergePdfs: result.mergePdfs,
                                    reportLanguageId: result.languageId,
                                };
                                this.requestReportService.printCustomerInvoice(model).then(() => {
                                    completion.completed(null, true);
                                });
                            });
                        }
                    }
                });

                comp.completed(null, true);
            });
        });
    }

    private doDownload(exportDataFileName: string, exportId: number) {
    //guid = guid.substring(guid.lastIndexOf("_") + 1);

    let uri = window.location.protocol + "//" + window.location.host;
    uri = uri + "/soe/billing/invoice/status/default.aspx" + "?classificationgroup=" + SoeOriginStatusChange.CustomerInvoice_EInvoice_Create + "&custInvExportBatchId=" + exportId + "&fileName=" + exportDataFileName;
    window.open(uri, '_blank');
}

    private selectUsers() {
    this.originUserHelper.selectUsersDialog(true, true, true).then((result) => {
        if (result) {
            this.sendXEMail = result.sendMessage;
            this.setAsDirty(true);
        }
    })
}

    private changeInvoiceTemplate() {
    this.$timeout(() => {
        const keys: string[] = [
            "core.warning",
            "billing.invoices.templates.load",
            "billing.invoices.templates.clear"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.warning"], this.selectedInvoiceTemplate ? terms["billing.invoices.templates.load"] : terms["billing.invoices.templates.clear"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                //Load template
                if (this.selectedInvoiceTemplate) {
                    this.loadInvoiceTemplate(this.selectedInvoiceTemplate);
                }
                else {
                    //Clear project?
                    this.new();
                }
            }, (reason) => {
                // User cancelled
                this.selectedInvoiceTemplate = 0;
            });
        });
    });
}

    private loadInvoiceTemplate(orderTemplateId: number): ng.IPromise < any > {
    this.selectedInvoiceTemplate = orderTemplateId;
    this.invoiceId = orderTemplateId;
    this.createCopy = true;
    return this.load(false, true);
}

    private showYourReferenceInfo() {
    var reference = this.customerReferences.filter((x) => x.name == this.invoice.referenceYour);
    if (reference && reference.length > 0) {
        this.invoiceEditHandler.showContactInfo(reference[0].id);
    }
}

    private emailChanging() {
    this.$timeout(() => {
        if (this.invoice.contactEComId && this.invoice.contactEComId > 0)
            this.invoice.customerEmail = undefined;
        else {
            const email = _.find(this.customerEmails, e => e.id === this.invoice.contactEComId);
            if (email && !StringUtility.isEmpty(email.name))
                this.invoice.customerEmail = email.name;
        }
    });
}

    private editEmailAddress() {
    const modal = this.modalInstance.open({
        templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/OneTimeCustomer/OneTimeCustomer.html"),
        controller: OneTimeCustomerController,
        controllerAs: 'ctrl',
        backdrop: 'static',
        size: 'sm',
        resolve: {
            translationService: () => { return this.translationService },
            coreService: () => { return this.coreService },
            name: () => { return "" },
            deliveryAddress: () => { return "" },
            phone: () => { return "" },
            email: () => { return this.invoice.customerEmail },
            isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
            isLocked: () => { return false },
            isEmailMode: () => { return true }
        }
    });

    modal.result.then((result: any) => {
        if (result) {
            this.invoice.customerEmail = result.email;
            this.customerEmails[0].name = result.email;
            this.invoice.contactEComId = 0;
            this.setAsDirty();
        }
    });
}

    private editDeliveryAddress() {

    var tmpInvoiceHeadText: string = this.invoice.invoiceHeadText;

    if (this.invoice.deliveryAddressId && this.invoice.deliveryAddressId != 0) {
        tmpInvoiceHeadText = this.invoiceEditHandler.formatDeliveryAddress(_.filter(this.invoiceEditHandler.deliveryAddresses, i => i.contactAddressId == this.invoice.deliveryAddressId)[0].contactAddressRows, this.customer.isFinvoiceCustomer);
    }

    const modal = this.modalInstance.open({
        templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/EditDeliveryAddress/EditDeliveryAddress.html"),
        controller: EditDeliveryAddressController,
        controllerAs: 'ctrl',
        backdrop: 'static',
        size: 'sm',
        resolve: {
            translationService: () => { return this.translationService },
            coreService: () => { return this.coreService },
            deliveryAddress: () => { return tmpInvoiceHeadText },
            isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
            isLocked: () => { return this.isLocked }
        }
    });

    modal.result.then((result: any) => {
        if ((result) && (result.deliveryAddress != null)) {
            this.invoice.invoiceHeadText = result.deliveryAddress;
            this.invoice.deliveryAddressId = 0;
            if (!this.invoice.billingAddressId && this.useDeliveryAddressAsInvoiceAddress) {
                this.invoiceEditHandler.invoiceAddresses[0].address = result.deliveryAddress;
                this.invoice.billingAdressText = result.deliveryAddress;
                this.invoice.billingAddressId = 0;
            }
            this.setAsDirty();
        }
        this.invoiceEditHandler.deliveryAddresses[0].address = this.invoice.invoiceHeadText;
    });
}

    // Called from product rows when changing attest states
    private changeAttestStates(canCreateInvoice: boolean) {
    this.createInvoiceWhenOrderReady = (canCreateInvoice && this.askCreateInvoiceWhenOrderReady);
    this.save(false);
}

    private save(closeAfterSave: boolean, discardConcurrencyCheck = false, createEInvoice = false, downloadEInvoice = false, emailForCashSales = false, emailAddressForCashSales = "", printForCashSales = false) {
    if (this.productRowsRendered) {
        this.$scope.$broadcast('stopEditing', {
            functionComplete: (source: string) => {
                if (source === "productrows") {
                    this.savePhase2(closeAfterSave, discardConcurrencyCheck, createEInvoice, downloadEInvoice, emailForCashSales, emailAddressForCashSales, printForCashSales)
                }
            }
        });
    }
    else {
        this.savePhase2(closeAfterSave, discardConcurrencyCheck, createEInvoice, downloadEInvoice, emailForCashSales, emailAddressForCashSales, printForCashSales);
    }

}

    private savePhase2(closeAfterSave: boolean, discardConcurrencyCheck: boolean = false, createEInvoice: boolean = false, downloadEInvoice = false, emailForCashSales = false, emailAddressForCashSales = "", printForCashSales = false) {
    if (this['edit'].$invalid) {
        console.warn("Save called with invalid form");
        this.executing = false;
        return;
    }

    if (!this.dirtyHandler.isDirty && !emailForCashSales && !printForCashSales) {
        if (closeAfterSave) {
            this.closeMe(true)
        }
        this.executing = false;
        return;
    }

    //Validate template name
    if (this.invoice.isTemplate) {
        var existing = _.find(this.invoiceTemplates, (t) => t.name === this.invoice.originDescription);
        if (existing) {
            //common.templateexists
            this.translationService.translate("common.templateexists").then((text) => {
                this.notificationService.showDialogEx(this.terms["core.warning"], text, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            });
            this.executing = false;
            return;
        }
    }

    // Contact ecom double check
    if (this.invoice.contactEComId && !_.find(this.customerEmails, { 'id': this.invoice.contactEComId }))
        this.invoice.contactEComId = undefined;

    let reloadProductRows = false;

    // Fix for automatic origin when cash sale customer
    if (this.useCashSales) {
        if (this.invoice.seqNr > 0 && this.invoice.originStatus === SoeOriginStatus.Draft)
            this.invoice.originStatus = SoeOriginStatus.Origin;
    }

    this.progress.startSaveProgress((completion) => {
        this.accountingRowsExpanderIsOpen = false;
        this.timeProjectRowsExpanderIsOpen = false;

        if (this.invoice.deliveryAddressId > 0) {
            this.invoice.invoiceHeadText = null;
            this.invoiceEditHandler.deliveryAddresses[0].address = "";
        }

        if (this.invoice.contactEComId > 0) {
            this.invoice.customerEmail = null;
            this.customerEmails[0].name = "";
        }

        var modifiedFields = null;
        if (this.isNew) {
            modifiedFields = CoreUtility.toDTO(this.invoice, BillingInvoiceDTO.getPropertiesToSkipOnSave(), true)
        }
        else {
            modifiedFields = CoreUtility.diffDTO(this.originalInvoice, this.invoice, BillingInvoiceDTO.getPropertiesToSkipOnSave(), true);
            modifiedFields['id'] = this.invoice.invoiceId ? this.invoice.invoiceId : 0;
        }

        //Make sure originstatus is present
        if (!modifiedFields['originstatus'])
            modifiedFields['originstatus'] = this.invoice.originStatus;

        if (this.originalInvoice)
            modifiedFields['modified'] = this.originalInvoice.modified; // this.originalInvoice.modified ? CalendarUtility.convertToDate(this.originalInvoice.modified).toFormattedDateTime() : null;

        //modifiedFields['istemplate'] = this.saveAsTemplate;
        modifiedFields['checkconflictsonsave'] = this.checkConflictsOnSave;

        // Check if renumbering needs to be done
        let firstRowToRemove = undefined;
        let lastRow = undefined;
        _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active), 'rowNr'), (r) => {
            if (!firstRowToRemove && r.type === SoeInvoiceRowType.ProductRow && (!r.productId || r.productId === 0))
                firstRowToRemove = r.rowNr;
            lastRow = r.rowNr;
        });

        if (firstRowToRemove && firstRowToRemove !== lastRow) {
            let i: number = 1;
            _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active && (r.type === SoeInvoiceRowType.ProductRow && r.productId && r.productId > 0) || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow), 'rowNr'), r => {
                r.rowNr = i++;
                r.isModified = true;
            });
        }

        // New product rows
        let newRows = _.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId && (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0)));

        // Modified product rows (only modified fields)
        var modifiedRows: any[] = [];
        _.forEach(_.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.isModified), row => {
            var origRow = _.find(this.originalInvoice.customerInvoiceRows, r => r.customerInvoiceRowId == row.customerInvoiceRowId);
            if (origRow) {
                var modFields: any[] = [];
                var rowDiffs = CoreUtility.diffDTO(origRow, row, ProductRowDTO.getPropertiesToSkipOnSave(), true);
                if (row.quantity !== origRow.quantity)
                    rowDiffs["quantity"] = row.quantity;
                if (row.purchasePriceCurrency !== origRow.purchasePriceCurrency)
                    rowDiffs["purchasepricecurrency"] = row.purchasePriceCurrency;
                rowDiffs["customerinvoicerowid"] = origRow.customerInvoiceRowId;
                rowDiffs["type"] = origRow.type;
                rowDiffs["state"] = row.state;
                modifiedRows.push(rowDiffs);
                /*var modFields: StringKeyValueList = new StringKeyValueList(row.customerInvoiceRowId);
                var rowDiffs = Util.CoreUtility.diffDTO(origRow, row, ProductRowDTO.getPropertiesToSkipOnSave());
                _.forEach(Object.keys(rowDiffs), key => {
                    modFields.values.push(new StringKeyValue(key, rowDiffs[key]));
                });
                modifiedRows.push(modFields);*/
            } else {
                newRows.push(row);
            }
        });

        // Extra seat belt, suspenders and parachute
        newRows = this.resetTempRowIds(newRows);

        reloadProductRows = (newRows.length > 0 || modifiedRows.length > 0);

        const filesDto: FileUploadDTO[] = this.invoiceFilesHelper.getAsDTOs(true);

        //OriginUser
        const users = this.originUserHelper.getOriginUserDTOs();
        this.invoiceService.saveInvoice(modifiedFields, newRows, modifiedRows, null, null, users, filesDto, discardConcurrencyCheck, this.tryCopyAccountingRows).then(result => {
            if (result.success) {
                if (this.transferToVoucher && this.askPrintVoucherOnTransfer) {
                    if (result.idDict) {

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

                        const message = this.terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + this.terms["economy.supplier.payment.askPrintVoucher"];
                        const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val != null && val === true) {
                                this.printVouchers(voucherIds);
                            };
                        });
                    }
                }

                const seqNr = result.value ? result.value : 0;
                if (this.invoice && seqNr > 0) {
                    this.invoice.seqNr = seqNr;
                    this.invoice.invoiceNr = seqNr;
                }

                if (!this.invoiceId || this.invoiceId === 0)
                    this.invoiceId = result.integerValue;

                if (createEInvoice)
                    this.createEInvoice(downloadEInvoice);

                if (emailForCashSales) {
                    const params = {
                        invoiceId: this.invoice.invoiceId,
                        invoiceNr: this.invoice.invoiceNr,
                        actorCustomerId: this.customer.actorCustomerId,
                        printTimeReport: true,
                        includeOnlyInvoicedTime: this.invoice.includeOnlyInvoicedTime,
                        addAttachmentsToEinvoice: this.invoice.addAttachementsToEInvoice,
                        attachmentIds: [],
                        checklistIds: [],
                        singleRecipient: emailAddressForCashSales,
                    }
                    this.invoiceEditHandler.sendReport(params, OrderInvoiceRegistrationType.Invoice, true, this.defaultBillingInvoiceReportId, 0, false, [], this.emailTemplateId, true);
                }

                completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.invoice);

                this.creditingInvoice = false;
                this.tryCopyAccountingRows = false;
            }
            else {
                completion.failed(result.errorMessage);
            }
        });

        this.executing = false;
    }, this.guid).then(data => {
        this.dirtyHandler.clean();

        if (closeAfterSave) {
            this.closeMe(true)
        }
        else {
            this.progress.startLoadingProgress([() => {
                return this.load(true, false, reloadProductRows).then(() => {
                    if (printForCashSales) {
                        this.$timeout(() => {
                            this.executePrintRequest(this.defaultBillingInvoiceReportId, this.customer?.sysLanguageId ?? CoreUtility.sysCountryId, false);
                        }, 500);
                    }
                });
            }]);
        }

    }, error => {

    });
}

    private resetTempRowIds(rows: ProductRowDTO[]): ProductRowDTO[] {
    var i: number = 1;
    _.forEach(rows, (r) => {
        var childRows = _.filter(rows, (cr) => cr.parentRowId === r.tempRowId);
        if (childRows && childRows.length > 0) {
            _.forEach(childRows, (c) => { c.parentRowId = i });
        }
        r.tempRowId = i;
        i = i + 1;
    });
    return rows;
}

    private printVouchers(ids: number[]) {
    if (this.voucherListReportId) {

        this.requestReportService.printVoucherList(ids);
    }
    else {
        this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
    }
}

    public editOrderText() {
    const options: angular.ui.bootstrap.IModalSettings = {
        templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
        controller: TextBlockDialogController,
        controllerAs: "ctrl",
        size: 'lg',
        resolve: {
            text: () => { return this.invoice.invoiceText },
            editPermission: () => { return this.isLocked === false },
            entity: () => { return SoeEntityType.Order },
            type: () => { return TextBlockType.TextBlockEntity },
            headline: () => { return this.terms["common.customer.invoices.editordertext"] },
            mode: () => { return SimpleTextEditorDialogMode.EditInvoiceDescription },
            container: () => { return ProductRowsContainers.Order },
            langId: () => { return TermGroup_Languages.Swedish },
            maxTextLength: () => { return 995 },
            textboxTitle: () => { return undefined },
        }
    }
    this.$uibModal.open(options).result.then((result: any) => {
        if (result) {
            this.invoice.invoiceText = result.text;
        }
    });
}

    private editWorkingDescription() {
    const options: angular.ui.bootstrap.IModalSettings = {
        templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
        controller: TextBlockDialogController,
        controllerAs: "ctrl",
        size: 'lg',
        resolve: {
            text: () => { return this.invoice.workingDescription },
            editPermission: () => { return this.isLocked === false },
            entity: () => { return SoeEntityType.CustomerInvoice },
            type: () => { return TextBlockType.WorkingDescription },
            headline: () => { return this.terms["billing.order.workingdescription"] },
            mode: () => { return SimpleTextEditorDialogMode.EditWorkingDescription },
            container: () => { return ProductRowsContainers.Order },
            langId: () => { return TermGroup_Languages.Swedish },
            maxTextLength: () => { return null },
            textboxTitle: () => { return undefined },
        }
    }
    this.$uibModal.open(options).result.then((result: any) => {
        if (result) {
            this.invoice.workingDescription = result.text;
        }
    });
}

    public unlockInvoice() {
    this.progress.startLoadingProgress([() => {
        return this.orderService.unlockOrder(this.invoice.invoiceId).then(result => {

            if (result.success) {
                this.load();
            }
            else {
                var message = result.errorNumber === ActionResultSave.InvalidStateTransition ? this.terms["common.customer.invoices.orderunlockstatusfailed"] : this.terms["common.customer.invoices.orderunlockfailed"];
            }
        });
    }]);
}

    protected copy(removeProject: boolean = true, ignoreEnable: boolean = false, isCredit: boolean = false, keepOrderNr: boolean = false, openNewCopy: boolean = false, setPrevInvoice: boolean = false) {
    if (this.invoiceEditHandler && this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon)) {
        const dialog = this.notificationService.showDialog(this.terms["common.customer.invoices.copyattachmentsheader"], this.terms["common.customer.invoices.copyattachmentstext"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
        dialog.result.then(val => {
            if (val) {
                if (!this.documentExpanderIsOpen) {
                    var filesWatch = this.$scope.$watch(() => this.invoiceFilesHelper.filesLoaded, (newValue, oldValue) => {
                        if (newValue) {
                            this.performCopy(removeProject, ignoreEnable, isCredit, keepOrderNr, openNewCopy, setPrevInvoice);
                            filesWatch();
                        }
                    });
                    this.documentExpanderIsOpen = true;
                }
                else {
                    this.performCopy(removeProject, ignoreEnable, isCredit, keepOrderNr, openNewCopy, setPrevInvoice);
                }
            }
            else {
                this.invoiceFilesHelper.files = [];
                this.performCopy(removeProject, ignoreEnable, isCredit, keepOrderNr, openNewCopy, setPrevInvoice);
            }
        });
    }
    else {
        this.performCopy(removeProject, ignoreEnable, isCredit, keepOrderNr, openNewCopy, setPrevInvoice);
    }
}

    protected performCopy(removeProject: boolean = true, ignoreEnable: boolean = false, isCredit: boolean = false, keepOrderNr: boolean = false, openNewCopy: boolean = false, setPrevInvoice: boolean = false) {
    this.allowModifyDefinitive = this.draftToOriginPermission;
    this.showDeleteButton = false;
    this.showRevokeButton = false;
    this.showCreditButton = false;
    this.ignoreReloadInvoiceFee = true;
    this.ignoreReloadFreightAmount = true;

    this.isNew = true;
    this.printInvoiceAsCopy = this.invoice.billingInvoicePrinted = false;

    if (isCredit) {
        this.invoice.prevInvoiceId = this.invoice.invoiceId;
        if (this.invoice.invoiceFeeCurrency && this.invoice.invoiceFeeCurrency > 0) {
            this.invoice.invoiceFee = -this.invoice.invoiceFee;
            this.invoice.invoiceFeeCurrency = -this.invoice.invoiceFeeCurrency;
        }
        if (this.invoice.freightAmountCurrency && this.invoice.freightAmountCurrency > 0) {
            this.invoice.invoiceFee = -this.invoice.invoiceFee;
            this.invoice.freightAmountCurrency = -this.invoice.freightAmountCurrency;
        }
    }
    else if (setPrevInvoice) {
        this.invoice.prevInvoiceId = this.invoice.invoiceId;
    }

    // Used for copy
    var currentInvoiceId: number = this.invoiceId;

    //Always clear invoice and due date
    this.selectedInvoiceDate = undefined;
    this.invoice.dueDate = undefined;
    this.selectedVoucherDate = undefined;

    this.invoiceId = 0;
    this.invoice.invoiceId = 0;
    this.invoice.invoiceNr = undefined;
    this.invoice.originStatus = SoeOriginStatus.Draft;

    this.invoice.seqNr = undefined;
    this.invoice.shiftTypeId = undefined;
    this.invoice.invoiceDate = null;
    this.invoice.currencyDate = CalendarUtility.getDateToday();
    this.invoice.modified = undefined;
    this.invoice.modifiedBy = '';
    if (!keepOrderNr)
        this.invoice.orderNumbers = '';

    // Paid amounts
    this.invoice.paidAmount = 0;
    this.invoice.paidAmountCurrency = 0;

    // Status icons
    this.invoice.statusIcon = undefined;
    this.invoice.billingInvoicePrinted = false;

    // Get origin status text for new
    this.translationService.translate("core.new").then(term => {
        this.invoice.originStatusName = term;
    });

    if (removeProject) {
        this.invoice.projectId = undefined;
        this.invoice.projectNr = undefined;
    }

    this.setLocked();

    //Product rows..timeout to let productrow expander open up if closed
    if (!this.productRowsRendered) {
        this.creditProductRows = true;
        this.originIsCredit = isCredit;
        this.openProductRowExpander();
    }
    else {
        //Product rows
        this.$scope.$broadcast('copyRows', { guid: this.guid, isCredit: isCredit, checkRecalculate: !isCredit });

        this.setAsDirty(true);
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
            guid: this.guid, label: this.terms["common.customer.invoices.newcustomerinvoice"]
        });
    }

    _.forEach(this.invoiceFilesHelper.files, (f) => {
        f.isModified = true;
    });

    this.accountRows = [];

    this.setAsDirty(true);
    this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
        guid: this.guid, label: this.terms["common.customer.invoices.newcustomerinvoice"]
    });


    if (openNewCopy) {
        this.messagingHandler.publishOpenTab(new TabMessage(this.terms["common.customer.invoices.newcustomerinvoice"], currentInvoiceId, BillingInvoicesEditController, { id: currentInvoiceId, forceNewTab: true, createCopy: true, keepOrderNr: keepOrderNr }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')));
    }

    this.updateTabCaption(true);
}

    public reloadCustomerInvoice(invoiceId: any) {
    this.invoiceId = invoiceId;
}

    // HELP-METHODS

    private hasModifiedRows(): boolean {
    return _.filter(this.invoice.customerInvoiceRows, r => r.isModified).length > 0;
}


    private getAccountId(type: CustomerAccountType, dimNr: number): number {
    // First try to get account from customer
    var accountId = this.getCustomerAccountId(type, dimNr);
    if (accountId === 0 && dimNr === 1) {
        // No account found on customer, use base account
        switch (type) {
            case CustomerAccountType.Credit:
                if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor)
                    accountId = this.reverseVatSalesId;
                else
                    accountId = this.defaultCreditAccountId;
                break;
            case CustomerAccountType.Debit:
                if (this.invoice.cashSale)
                    accountId = this.defaultCashAccountId;
                else
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
    let accountId = 0;

    if (type === CustomerAccountType.VAT && dimNr === 1 && this.customerVatAccountId !== 0)
        return this.customerVatAccountId;

    if (this.customer && this.customer.accountingSettings) {
        const setting = _.find(this.customer.accountingSettings, { type: type });
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


    private setAsDirty(dirty = true) {
    this.dirtyHandler.isDirty = dirty;
}

    private copyChanging() {
    console.log("copy changing");
}

    private setOurReference() {
    this.invoice.referenceOur = null;

    // User setting
    if (this.defaultOurReferenceUserId !== 0) {
        var ref = _.find(this.ourReferences, { id: this.defaultOurReferenceUserId });
        if (ref)
            this.invoice.referenceOur = ref.name;

    }

    // Company setting
    if (!this.invoice.referenceOur && this.defaultOurReference) {
        var ref = _.find(this.ourReferences, { name: this.defaultOurReference });
        if (!ref) {
            ref = {
                id: -1,
                name: this.defaultOurReference
            };
            this.ourReferences.push(ref);
        }
        this.invoice.referenceOur = ref.name;
    }

    // Current user
    if (!this.invoice.referenceOur) {
        var ref = _.find(this.ourReferences, { id: CoreUtility.userId });
        if (ref)
            this.invoice.referenceOur = ref.name;
    }
}

    private setPriceListType(typeId: number, fromLoad = false) {
        if (this.selectedPriceListType && this.selectedPriceListType.priceListTypeId === typeId)
            return;

        if (typeId === 0)
            typeId = this.defaultPriceListTypeId;

        const priceListType: IPriceListTypeDTO = _.find(this.priceListTypes, { priceListTypeId: typeId });

        if (priceListType) {
            this._selectedPriceListType = priceListType;
            if (this.invoice)
                this.invoice.priceListTypeId = priceListType.priceListTypeId;
            this.getFreightAmount();
            this.getInvoiceFee();
            this.currencyHelper.priceListTypeInclusiveVatChanged(priceListType.inclusiveVat);
        }
        else if (fromLoad && typeId !== 0) {
            this._selectedPriceListType = undefined;

        } else if (this.defaultPriceListTypeId !== 0) {
            this.setPriceListType(this.defaultPriceListTypeId);
        }
    }

    private setPaymentCondition(paymentConditionId: number) {
    if (!paymentConditionId || paymentConditionId === 0 && !this.loadingInvoice)
        paymentConditionId = this.invoiceEditHandler.defaultPaymentConditionId;

    if (this.invoice)
        this.invoice.paymentConditionId = paymentConditionId;

    // Get condition
    const condition = this.invoiceEditHandler.paymentConditions.find(x => x.paymentConditionId === paymentConditionId);
    this.paymentConditionDays = condition ? condition.days : this.invoiceEditHandler.defaultPaymentConditionDays;
    this.paymentConditionStartOfNextMonth = condition ? condition.startOfNextMonth : this.invoiceEditHandler.defaultPaymentConditionStartOfNextMonth;
    this.discountDays = condition ? condition.discountDays : 0;
    this.discountPercent = condition && condition.discountPercent ? condition.discountPercent : 0;

    this.setDueDate();
}

    private setDueDate() {
    if (this.invoice && this.invoice.invoiceDate && !this.loadingInvoice) {
        const startDate = this.paymentConditionStartOfNextMonth ? this.invoice.invoiceDate.endOfMonth().addDays(1) : this.invoice.invoiceDate;
        this.invoice.dueDate = startDate.addDays(this.paymentConditionDays);
    }
}

    /*
    private createAccountingRow(type: CustomerAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean, isContractorVatRow: boolean): AccountingRowDTO {
        // Credit invoice, negate isDebitRow
        if (this.isCredit)
            isDebitRow = !isDebitRow;

        amount = Math.abs(amount);

        var row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = 0;
        //row.rowNr = AccountingRowDTO.getNextRowNr(this.invoice.accountingRows);
        row.debitAmountCurrency = isDebitRow ? amount : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : amount;
        row.quantity = null;
        row.date = new Date().date();
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

        //this.invoice.accountingRows.push(row);

        return row;
    }
    */
    private calculateVatAmount(forceContractor = false) {
    // Calculate VAT amount based on vat percent
    var vatAmount: number = 0;
    var vatRateValue: number = this.vatRate / 100;

    if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || forceContractor)
        vatAmount = this.invoice.totalAmountCurrency * vatRateValue;
    else
        vatAmount = this.invoice.totalAmountCurrency * (1 - (1 / (vatRateValue + 1)));

    this.invoice.vatAmountCurrency = vatAmount.round(2);
}


    public openCustomer(openDeliveryCustomer: boolean) {
    if (this.customer && this.customer.isOneTimeCustomer) {
        let tmpInvoiceHeadText = this.invoice.invoiceHeadText;

        if (this.invoice.deliveryAddressId && this.invoice.deliveryAddressId !== 0) {
            tmpInvoiceHeadText = this.invoiceEditHandler.formatDeliveryAddress(_.filter(this.invoiceEditHandler.deliveryAddresses, i => i.contactAddressId === this.invoice.deliveryAddressId)[0].contactAddressRows, this.customer.isFinvoiceCustomer);
        }

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/OneTimeCustomer/OneTimeCustomer.html"),
            controller: OneTimeCustomerController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                name: () => { return this.invoice.customerName },
                deliveryAddress: () => { return tmpInvoiceHeadText ? tmpInvoiceHeadText : "" },
                phone: () => { return this.invoice.customerPhoneNr },
                email: () => { return this.invoice.customerEmail },
                isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
                isLocked: () => { return this.isLocked },
                isEmailMode: () => { return false }
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                this.invoice.customerName = result.name;
                this.invoice.customerPhoneNr = result.phone;

                if (!StringUtility.isEmpty(result.email)) {
                    this.invoice.customerEmail = result.email;
                    this.customerEmails[0].name = result.email;
                    this.invoice.contactEComId = 0;
                }

                if (result.address !== tmpInvoiceHeadText) {
                    this.invoice.invoiceHeadText = result.address;
                    this.invoiceEditHandler.deliveryAddresses[0].address = result.address;
                    this.invoice.deliveryAddressId = 0;
                }

                // Change customer name
                if (this.invoice.actorId && this.invoice.actorId > 0) {
                    const customer = _.find(this.customers, c => c.id === this.invoice.actorId);
                    if (customer) {
                        this.doNotLoadCustomer = true;
                        this.selectedCustomer = undefined;
                        if (this.showPayingCustomer)
                            this.selectedDeliveryCustomer = undefined;
                        this.$timeout(() => {
                            customer.name = customer.number + " " + this.invoice.customerName;
                            this.selectedCustomer = customer;

                            if (this.showPayingCustomer)
                                this.selectedDeliveryCustomer = customer;

                            this.doNotLoadCustomer = false;
                        });
                    }
                }

                this.setAsDirty();
            }
        });
    }
    else {
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
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, id: openDeliveryCustomer ? (this.deliveryCustomer ? this.deliveryCustomer.actorCustomerId : 0) : (this.customer ? this.customer.actorCustomerId : 0) });
        });


        modal.result.then(result => {
            if (openDeliveryCustomer) {
                const customer = this.selectedDeliveryCustomer = _.find(this.customers, c => c.id === result.customerId);
                if (!customer) {
                    const x = { id: result.customerId, name: result.customerName };
                    this.customers.push(x);
                    this.selectedDeliveryCustomer = x;
                }

                if (result.saved && this.invoice.originStatus === SoeOriginStatus.Draft)
                    this.loadDeliveryCustomer();
            }
            else {
                const newCustomer = _.find(this.customers, c => c.id === result.customerId);

                if (!this.selectedCustomer || newCustomer.id !== this.selectedCustomer.id) {
                    this.selectedCustomer = newCustomer;
                    if (!newCustomer) {
                        const x = { id: result.customerId, name: result.customerName };
                        this.customers.push(x);
                        this.selectedCustomer = x;
                    }
                }

                if (result.saved && this.invoice.originStatus === SoeOriginStatus.Draft) {
                    this.loadCustomer(result.customerId, true);
                }
            }
        });
    }
}

    public searchCustomer(isDeliveryCustomer: boolean) {
    var modal = this.modalInstance.open({
        templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomer", "selectcustomer.html"),
        controller: SelectCustomerController,
        controllerAs: 'ctrl',
        backdrop: 'static',
        size: 'lg',
        resolve: {
            translationService: () => { return this.translationService },
            coreService: () => { return this.coreService },
            commonCustomerService: () => { return this.commonCustomerService }
        }
    });

    modal.result.then(item => {
        if (item) {
            if (isDeliveryCustomer) {
                this.selectedDeliveryCustomer = _.find(this.customers, c => c.id === item.actorCustomerId);
            }
            else {
                this.selectedCustomer = _.find(this.customers, c => c.id === item.actorCustomerId);
            }
        }
    }, function () {
    });

    return modal;
}

    private showMissingDefaultPriceListTypeWarning() {
    var keys: string[] = [
        "billing.order.missingdefaultpricelisttype.title",
        "billing.order.missingdefaultpricelisttype.message"
    ];

    this.translationService.translateMany(keys).then((terms) => {
        this.notificationService.showDialog(terms["billing.order.missingdefaultpricelisttype.title"], terms["billing.order.missingdefaultpricelisttype.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    });
}

    // VALIDATION

    public showValidationError() {
    // Mandatory fields
    if (this.invoice) {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['edit'].$error;
            if (!this.invoice.vatType)
                mandatoryFieldKeys.push("common.customer.invoices.vattype");

            var mandatoryDims = _.filter(Object.keys(errors), (e) => e.startsWith("DIM_"));
            if (mandatoryDims.length > 0) {
                _.forEach(mandatoryDims, (dim) => {
                    var strings = dim.split('_');
                    validationErrorStrings.push(this.terms["core.missingmandatoryfield"] + " " + strings[1]);
                });
            }

            if (errors['locked'])
                validationErrorKeys.push("common.customer.invoice.orderlocked");

            if (errors['templateDescription'])
                validationErrorKeys.push("common.customer.invoices.validationdesc");

            if (errors['customer'])
                validationErrorKeys.push("common.customer.invoices.validationcustomer");

            if (errors['priceList'])
                validationErrorKeys.push("common.customer.invoices.validationpricelist");

            if (errors['standardVoucherSeriesId'])
                validationErrorKeys.push("common.customer.invoices.validationstandvoucherseries");

            if (errors['defaultVoucherSeries'])
                validationErrorKeys.push("common.customer.invoices.validationvoucherserie");

            if (errors['customerBlocked'])
                validationErrorKeys.push("common.customer.invoices.validationcustomerblocked");

            if (errors['freightBase'])
                validationErrorKeys.push("common.customer.invoices.validationfreightsetting");

            if (errors['invoiceFeeBase'])
                validationErrorKeys.push("common.customer.invoices.validationfeesetting");

            if (errors['centRoundingBase'])
                validationErrorKeys.push("common.customer.invoices.validationcentsetting");

            if (errors['missingProduct']) {
                _.forEach(_.filter(this.invoice.customerInvoiceRows, (r) => (!r.productId || r.productId === 0) && (r.sumAmountCurrency && r.sumAmountCurrency != 0)), (r) => {
                    validationErrorKeys.push("common.customer.invoices.validationprodmissing");
                });
            }

            if (errors['credit'])
                validationErrorKeys.push("common.customer.invoices.validationcredit");

            if (errors['nonCredit'])
                validationErrorKeys.push("common.customer.invoices.validationdebetnew");
        });
    }
}

    public isDisabled() {
    return !this.dirtyHandler.isDirty || this.edit.$invalid || this.executing;
}

    public enableCashPayment() {
    return this.invoice && this.invoice.paidAmountCurrency > 0 && (this.invoice.originStatus === SoeOriginStatus.Draft || this.invoice.originStatus === SoeOriginStatus.Origin);
}

    public isValidToChangeAttestState(): boolean {
    return true;
}

    public setLocked() {
    this.isLocked = !this.isNew && (this.definitive || (this.customer && this.customer.blockInvoice));
    this.showUnlockButton = this.invoice.originStatus === SoeOriginStatus.Origin &&
        !this.invoice.manuallyAdjustedAccounting &&
        this.invoice.paidAmount === 0 &&
        this.modifyPermission &&
        (this.unlockPermission || CoreUtility.isSupportAdmin);
}

    public doCashSale() {
    const modal = this.modalInstance.open({
        templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Invoices/Dialogs/CashSales/CashSales.html"),
        controller: CashSalesController,
        controllerAs: 'ctrl',
        backdrop: 'static',
        size: 'md',
        resolve: {
            commonCustomerService: () => { return this.commonCustomerService },
            invoiceId: () => { return this.invoice.invoiceId },
            billingType: () => { return this.invoice.billingType },
            totalAmount: () => { return this.invoice.totalAmountCurrency.round(2) },
            emails: () => { return this.customerEmails },
            contactEComId: () => { return this.invoice.contactEComId }
        }
    });

    modal.result.then((cashPayment) => {
        if (cashPayment) {
            this.progress.startSaveProgress((completion) => {
                this.invoiceService.createCashPaymentsFromInvoice(cashPayment).then(result => {
                    if (result.success) {
                        const reportId = this.reportIdCashSales ? this.reportIdCashSales : this.defaultBillingInvoiceReportId;
                        if (cashPayment.sendEmail) {
                            const params = {
                                invoiceId: this.invoice.invoiceId, invoiceNr: this.invoice.invoiceNr,
                                actorCustomerId: this.customer.actorCustomerId,
                                printTimeReport: true,
                                includeOnlyInvoicedTime: this.invoice.includeOnlyInvoicedTime,
                                addAttachmentsToEinvoice: this.invoice.addAttachementsToEInvoice,
                                attachmentIds: [],
                                checklistIds: [],
                                singleRecipient: cashPayment.email,
                            }
                            this.invoiceEditHandler.sendReport(params, OrderInvoiceRegistrationType.Invoice, true, reportId, 0, false, [], this.emailTemplateIdCashSales, true);
                        }
                        else {
                            this.executePrintRequest(reportId, this.customer && this.customer.sysLanguageId ? this.customer.sysLanguageId : CoreUtility.sysCountryId, false);
                        }
                        completion.completed(Constants.EVENT_EDIT_SAVED, this.invoice);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                })
            }, this.guid).then(data => {
                if (this.openedFromOrder) {
                    this.load(true);
                }
                else {
                    const resultModal = this.notificationService.showDialog(this.terms["billing.invoices.cashsales.newsale"], this.terms["billing.invoices.cashsales.startnewsale"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                    resultModal.result.then(val => {
                        if (val)
                            this.new();
                        else
                            this.load(true);

                    });
                }
            }, error => {

            });
        }
    }, function () {
    });
}

    public delete (copy = false, message: string = null) {
        let restoreRowStatus = true;
        let deleteMessageKey = "";
        if (this.invoice.transferedFromOriginType === SoeOriginType.Order) {
            deleteMessageKey = "common.customer.invoice.restorerowstatusquestion";
        }
        else if (this.invoice.transferedFromOriginType === SoeOriginType.Offer) {
            deleteMessageKey = "common.customer.invoice.restorerowstatusquestionoffer";
        }
        else if (this.invoice.transferedFromOriginType === SoeOriginType.Contract) {
            deleteMessageKey = "common.customer.invoices.restoreagreementstatus";
        }
        else {
            restoreRowStatus = false;
        }

        const keys: string[] = [
            "core.verifyquestion",
            deleteMessageKey
        ];

        this.progress.startDeleteProgress((completion) => {
            if (restoreRowStatus) {
                this.translationService.translateMany(keys).then((terms) => {
                    const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms[deleteMessageKey], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);

                    modal.result.then(val => {
                        if (!val) 
                            restoreRowStatus = false;

                        this.invoiceService.deleteInvoice(this.invoice.invoiceId, false, restoreRowStatus, this.invoice.transferedFromOriginType == SoeOriginType.Contract).then((result) => {
                            if (result.success) {
                                completion.completed(this.invoice, false, message);

                                if (copy)
                                    this.copy();
                                else
                                    this.new();

                                this.updateTabCaption();
                            }
                            else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });

                    });
                });
            }
            else {
                this.invoiceService.deleteInvoice(this.invoice.invoiceId, false, restoreRowStatus, false).then((result) => {
                    if (result.success) {
                        completion.completed(this.invoice, false, message);

                        if (copy)
                            this.copy();
                        else
                            this.new();

                        this.updateTabCaption();
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }
        }, null, this.terms["billing.invoices.delete"]);
    }

    public revokeInvoice() {
    const keys: string[] = [
        "core.verifyquestion",
        "common.customer.invoices.revokequestion",
        "common.customer.invoices.revokecompleted"
    ];
    this.translationService.translateMany(keys).then((terms) => {
        const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.customer.invoices.revokequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel);
        modal.result.then(val => {
            const message = terms["common.customer.invoices.revokecompleted"].format(this.invoice.invoiceNr);
            this.delete(val, message);
            if (!val) {
                this.closeMe(false);
            }
        });
    });
}

    public creditInvoice() {
    const keys: string[] = [
        "core.verifyquestion",
        "common.customer.invoices.creditquestion",
        "common.customer.invoices.newcustomerinvoice",
        "common.customer.invoices.keepordernrandproject"
    ];
    this.translationService.translateMany(keys).then((terms) => {
        const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.customer.invoices.creditquestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, SOEMessageBoxSize.Medium, false, true, terms["common.customer.invoices.keepordernrandproject"], true);
        modal.result.then(val => {
            this.creditingInvoice = true;
            this.tryCopyAccountingRows = true;
            this.copy(false, false, true, val.isChecked, val.result);
            this.invoice.billingType = (this.invoice.billingType === TermGroup_BillingType.Credit) ? TermGroup_BillingType.Debit : TermGroup_BillingType.Credit;
        });
    });
}

    private GetEInvoiceEntry() {
    if (this.invoiceId && this.createEInvoicePermission) {
        this.invoiceService.getEInvoiceEntry(this.invoiceId).then(entry => {
            const statusGroup = this.buttonStatusGroup;
            statusGroup.deleteButton("fa-paper-plane");
            if (entry) {
                var cssClass = "warningColor";
                var text = "common.waitingforanswer";
                var captionText = "core.info";
                var message = entry.message;
                switch (entry.invoiceState) {
                    case 0:
                        message = this.terms["common.waitingforanswer"];
                        break;
                    case 1:
                        cssClass = "successColor";
                        text = "common.sent";
                        message = this.terms["common.sent"];
                        break;
                    case 2:
                        text = "core.error";
                        cssClass = "errorColor";
                        break;
                    case 3:
                        text = "common.stoped";
                        cssClass = "errorColor";
                        break;
                };

                const button = new ToolBarButton("", text, IconLibrary.FontAwesome, "fa-paper-plane " + cssClass, () => {
                    this.notificationService.showDialogEx(this.terms[captionText], message, SOEMessageBoxImage.Information);
                }, null, () => {
                    return false;
                });
                button.idString = "fa-paper-plane";
                statusGroup.buttons.push(button);

                // Flag
                this.sentAsEInvoice = true;
            }
        });
    }
}

    private updateAccordionSettings() {

    const keys: string[] = [
        "billing.order.productrows",
        "core.document",
        "common.checklists",
        "common.customer.invoices.accountingrows",
        "common.tracing",
        "billing.invoices.invoice",
        "billing.order.conditions",
        "billing.invoices.invoicedetail"
    ];
    var accordionList: any[] = [];

    this.translationService.translateMany(keys).then((terms) => {
        let orderText = terms["billing.invoices.invoice"];
        accordionList.push({ name: "InvoiceExpander", description: orderText });
        accordionList.push({ name: "InvoiceInvoiceExpander", description: orderText + " >> " + terms["billing.invoices.invoicedetail"] });
        accordionList.push({ name: "InvoiceConditionExpander", description: orderText + " >> " + terms["billing.order.conditions"] });

        if (this.productRowsPermission)
            accordionList.push({ name: "ProductRowsExpander", description: terms["billing.order.productrows"] });
        if (this.filesPermission)
            accordionList.push({ name: "DocumentExpander", description: terms["core.document"] });
        if (this.accountingRowsPermission)
            accordionList.push({ name: "AccountingRowExpander", description: terms["common.customer.invoices.accountingrows"] });
        if (this.tracingPermission)
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
}