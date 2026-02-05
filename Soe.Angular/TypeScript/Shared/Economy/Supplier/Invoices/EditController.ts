import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IScopeWatcherService } from "../../../../Core/Services/ScopeWatcherService";
import { SupplierInvoiceDTO, EdiEntryDTO, IInvoiceInterpretationDTO, InvoiceInterpretationDTO, InterpretationValueDTO } from "../../../../Common/Models/InvoiceDTO";
import { ISmallGenericType, IPaymentInformationViewDTO, IAccountInternalDTO, IAccountingRowDTO, IActionResult, ISupplierInvoiceProductRowDTO } from "../../../../Scripts/TypeLite.Net4";
import { SupplierDTO } from "../../../../Common/Models/SupplierDTO";
import { AccountingRowsContainers, IconLibrary, SupplierInvoiceEditSaveFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../Util/Enumerations";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IAccountingService } from "../../Accounting/AccountingService";
import { ISupplierService } from "../SupplierService";
import { IAddInvoiceToAttestFlowService } from "../../../../Common/Dialogs/addinvoicetoattestflow/addinvoicetoattestflowservice";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IFocusService } from "../../../../Core/Services/focusservice";
import { Guid } from "../../../../Util/StringUtility";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { SupplierInvoiceRowDTO } from "../../../../Common/Models/SupplierInvoiceRowDTO";
import { FlaggedEnum } from "../../../../Util/EnumerationsUtility";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { ChangeCompanyController } from "./Dialogs/ChangeCompany/ChangeCompanyController";
import { AccordionSettingsController } from "../../../../Common/Dialogs/AccordionSettings/AccordionSettingsController";
import { FileUploadDTO } from "../../../../Common/Models/FileUploadDTO";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { AccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { EditController as SupplierEditController } from "../../../../Shared/Economy/Supplier/Suppliers/EditController";
import { EditController as AccountDistributionEditController } from "../../../../Shared/Economy/Accounting/AccountDistribution/EditController";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { AddInvoiceToAttestFlowController } from "../../../../Common/Dialogs/AddInvoiceToAttestFlow/AddInvoiceToAttestFlowController";
import { SoeEntityType, SoeEntityImageType, SoeReportTemplateType, TermGroup_InvoiceVatType, SoeOriginStatus, TermGroup_BillingType, TermGroup_EDISourceType, Feature, AccountingRowType, SoeEntityState, SupplierInvoiceAccountRowAttestStatus, SoeStatusIcon, ScanningEntryRowType, TermGroup_ScanningInterpretation, CompanySettingType, UserSettingType, SettingMainType, TermGroup_ProjectType, SoeTimeCodeType, TermGroup, SoeOriginType, SoeOriginStatusClassification, OrderInvoiceRegistrationType, TermGroup_Languages, SupplierAccountType, TermGroup_CurrencyType, ActionResultSave, TermGroup_SieAccountDim, TermGroup_SupplierInvoiceType, ImageFormatType, TermGroup_ScanningReferenceTargetField, TermGroup_ScanningCodeTargetField, SoeAccountDistributionType, TermGroup_AccountDistributionRegistrationType, TextBlockType, SimpleTextEditorDialogMode, SupplierInvoiceRowType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IShortCutService } from "../../../../Core/Services/ShortCutService";
import { FilesHelper } from "../../../../Common/Files/FilesHelper";
import { SelectProjectController } from "../../../../Common/Dialogs/SelectProject/SelectProjectController";
import { SelectCustomerInvoiceController } from "../../../../Common/Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { TextBlockDialogController } from "../../../../Common/Dialogs/textblock/textblockdialogcontroller";
import { IProjectService } from "../../../Billing/Projects/ProjectService";
import { PurchaseDeliveryInvoiceDTO } from "../../../../Common/Models/PurchaseDeliveryDTO";
import { ChangeIntrastatCodeController } from "../../../Billing/Dialogs/ChangeIntrastatCode/ChangeIntrastatCodeController";
import { IProductService } from "../../../Billing/Products/ProductService";
import { IntrastatTransactionDTO } from "../../../../Common/Models/CommodityCodesDTO";
import { ScanningInformationController } from "./Dialogs/ScanningInformation/ScanningInformationController";
import { AccountDimSmallDTO } from "../../../../Common/Models/AccountDimDTO";
import { IRequestReportService } from "../../../Reports/RequestReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private modal;
    isModal = false;

    // File upload
    invoiceImageUrl: string = Constants.WEBAPI_CORE_FILES_UPLOAD_INVOICE + SoeEntityType.SupplierInvoice;
    invoiceInterpretation?: InvoiceInterpretationDTO;
    filesHelper: FilesHelper;

    // Config
    currentAccountYearId = 0;
    currentAccountYearIsOpen = false;

    // Permissions
    useCurrency = false;
    reportPermission = false;
    editSupplierPermission = false;
    unlockPermission = false;
    uploadImagePermission = false;
    changeCompanyPermission = false;
    attestFlowAdminPermission = false;
    attestFlowPermission = false;
    attestFlowAddPermission = false;
    attestFlowCancelPermission = false;
    attestFlowTransferRowsPermission = false;
    projectPermission = false;
    ordersPermission = false;
    unlockAccountingRowsPermission = false;
    purchasePermission = false;
    finvoicePermission = false;
    intrastatPermission = false;
    productRowsPermission = false;

    // Company settings
    supplierInvoiceProductRowsImport = false;
    supplierInvoiceTransferToVoucher = false;
    supplierInvoiceAskPrintVoucherOnTransfer = false;
    defaultPaymentConditionId = 0;
    defaultPaymentConditionDays = 0;
    paymentConditionDays = 0;
    discountDays: number = null;
    discountPercent = 0;
    defaultVoucherSeriesTypeId = 0;
    defaultVatType: TermGroup_InvoiceVatType = TermGroup_InvoiceVatType.Merchandise;
    defaultDraft = false;
    allowEditOrigin = false;
    defaultCopyInvoiceNr = false;
    copyInvoiceNr = false;
    allowInterim = false;
    showTransactionCurrency = false;
    showEnterpriseCurrency = false;
    showLedgerCurrency = false;
    voucherListReportId = 0;
    chargeCostsToProject = false;
    miscProduct = 0;
    checkFIOCR = false;
    defaultAttestType: SoeOriginStatus = SoeOriginStatus.Origin;
    defaultAttestGroup = 0;
    isAttest = false;
    useTimeDiscount = false;
    allowEditAccountingRows = false;
    defaultTimeCodeId = 0;
    keepSupplier = false;
    useInternalAccountWithBalanceSheetAccounts = false;
    roundVAT = false;
    getDefaultInternalAccountsFromOrder = false;
    scanningReferenceTargetField = 0;
    scanningCodeTargetField = 0;
    intrastatImportOriginType = 0;
    defaultVatCodeId = 0;

    // User settings
    public simplifiedRegistration = false;

    public attestUserExpanderOpen = false;
    public projectRowsExpanderOpen = false;
    public projectOrderExpanderOpen = false;
    public accountingRowsExpanderOpen = false;
    public purchaseExpanderOpen = false;
    public tracingExpanderOpen = false;
    public imageGalleryExpanderOpen = false;
    public filesExpanderOpen = false;
    public productRowsExpanderOpen = false;
    public costAllocationExpanderOpen = false;

    public purchaseRowsRendered = false;

    // ChangeCompany for supplierinvoice        
    supplierInvoiceChangeCompanyDTO: any;

    // Company accounts
    defaultCreditAccountId = 0;
    defaultDebitAccountId = 0;
    defaultVatAccountId = 0;
    defaultInterimAccountId = 0;
    defaultAttestRowDebitAccountId = 0;
    defaultAttestRowAmount = 0;

    reverseVatAccountPurchaseId = 0;
    contractorVatAccountDebitId = 0;
    contractorVatAccountCreditId = 0;

    euVatDebitAccountId = 0;
    euVatCredit1AccountId = 0;
    euVatCredit2AccountId = 0;
    euVatCredit3AccountId = 0;
    euVatPurchaseAccountId = 0;

    nonEuVatDebitAccountId = 0;
    nonEuVatCredit1AccountId = 0;
    nonEuVatCredit2AccountId = 0;
    nonEuVatCredit3AccountId = 0;
    nonEuVatPurchaseAccountId = 0;

    // Supplier accounts
    supplierVatAccountId = 0;

    defaultVatRate = 0;
    vatRate: number = Constants.DEFAULT_VAT_RATE;

    // Lookups 
    suppliers: ISmallGenericType[];
    billingTypes: any[];
    vatTypes: any[];
    vatCodes: any[];
    currencies: any[];
    paymentConditions: any[];
    voucherSeries: any[];
    paymentInfos: IPaymentInformationViewDTO[];
    attestComment: string;
    customerInvoices: any[] = [];
    customerInvoicesDict: any[] = [];
    projects: any[] = [];
    projectsDict: any[] = [];
    accountDims: AccountDimSmallDTO[];
    accountDim: any;
    accountInternal: IAccountInternalDTO[];
    vatDeductionDict: any[] = [];
    timecodes: any[] = [];

    // Data
    invoice: SupplierInvoiceDTO;
    supplier: SupplierDTO;
    container: AccountingRowsContainers;
    purchaseInvoiceRows: PurchaseDeliveryInvoiceDTO[] = [];

    private invoiceIds: any[];
    private previousOrderId: number = 0;
    private previousProjectId: number = 0;

    // Flags
    loadingInvoice = false;
    invoiceIsLoaded = false;
    settingInvoiceSupplier = false;
    ignoreAskUnbalanced = false;
    ignoreAskDuplicate = false;
    skipInvoiceNrCheck = false;
    recalculateVat = false;
    hasScanningEntryInvoice = false;
    keepOpen: boolean;
    supressNote: boolean;
    attestRowsTransferred = false;
    isLockedAccountingRows = false;
    isProjectCentral = false;
    useVatDeductionDim = false;
    useVatDeduction = false;
    traceRowsRendered = false;
    showBlockPaymentButton = true;
    showNavigationButtons = false;
    usesSecondaryDirtyEvent = false;
    pdfFullHeight = false;
    isSaving: boolean = false;
    orderRowsLoaded: boolean = false;
    projectRowsLoaded: boolean = false;
    costAllocationRowsLoaded: boolean = false;
    blockOrderLoad = false;
    blockProjectLoad = false;
    ignoreLoadCostAllocationRows = false;
    costAllocationSetupDone = false;
    addDefaultCostAllocationRow = false;

    // Attest
    saveAttest = false;
    attestWorkFlowHead: any;

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
    attestAdminInfo: string;
    existingAttestFlow = false;
    accountDimNrForCostplace: string;
    invoiceNr: string;
    accountDistributionHeadId = 0;
    accountDistributionName: string;

    // Scanning properties
    scanningIsCreditInvoieIcon: string;
    scanningIsCreditInvoiceTooltip: string;
    scanningInvoiceNrIcon: string;
    scanningInvoiceNrTooltip: string;
    scanningInvoiceDateIcon: string;
    scanningInvoiceDateTooltip: string;
    scanningDueDateIcon: string;
    scanningDueDateTooltip: string;
    scanningOrderNrIcon: string;
    scanningOrderNrTooltip: string;
    scanningReferenceYourIcon: string;
    scanningReferenceYourTooltip: string;
    scanningReferenceOurIcon: string;
    scanningReferenceOurTooltip: string;
    scanningTotalAmountExludeVatIcon: string;
    scanningTotalAmountExludeVatTooltip: string;
    scanningVatAmountIcon: string;
    scanningVatAmountTooltip: string;
    scanningTotalAmountIncludeVatIcon: string;
    scanningTotalAmountIncludeVatTooltip: string;
    scanningCurrencyCodeIcon: string;
    scanningCurrencyCodeTooltip: string;
    scanningOCRIcon: string;
    scanningOCRTooltip: string;
    scanningPlusgiroIcon: string;
    scanningPlusgiroTooltip: string;
    scanningBankgiroIcon: string;
    scanningBankgiroTooltip: string;
    scanningOrgNrIcon: string;
    scanningOrgNrTooltip: string;
    scanningIBANIcon: string;
    scanningIBANTooltip: string;
    scanningVatRateIcon: string;
    scanningVatRateTooltip: string;
    scanningVatNrIcon: string;
    scanningVatNrTooltip: string;
    scanningFreightAmountIcon: string;
    scanningFreightAmountTooltip: string;
    scanningCentRoundingIcon: string;
    scanningCentRoundingTooltip: string;
    scanningVatRegNumberFinIcon: string;
    scanningVatRegNumberFinTooltip: string;
    scanningSupplierBankCodeNumber1Icon: string;
    scanningSupplierBankCodeNumber1Tooltip: string;
    scanningBankNrIcon: string;
    scanningBankNrTooltip: string;
    scanningReferenceOurValue: string;
    scanningReferenceYourValue: string;
    scanningOrderNr: string;

    private tabIndexes;
    private widthRatio = 7;
    private invoiceScale;
    private invoiceWidthClass;
    private imageWidthClass;
    private imageAccordion;
    private expanderSetting;

    private showConfirmAccounting = false;
    private confirmAccounting = false;

    private linkToOrderOrderSet = true;
    private linkToProjectProjectSet = true;
    private linkToProjectTimeCodeSet = true;

    private showEditSeqNrButton = false;

    get isCredit(): boolean {
        return this.invoice.billingType === TermGroup_BillingType.Credit;
    }

    get vatTypeLocked(): boolean {
        return this.invoice && (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || this.invoice.vatType === TermGroup_InvoiceVatType.NoVat);
    }

    get ocrInvoiceNrLocked(): boolean {
        return !this.isNew && this.invoice && this.invoice.paidAmountCurrency && this.invoice.paidAmountCurrency !== 0 && (this.invoice.originStatus === SoeOriginStatus.Draft || this.invoice.originStatus === SoeOriginStatus.Origin || this.invoice.originStatus === SoeOriginStatus.Voucher);
    }

    private _selectedSupplier;
    get selectedSupplier(): ISmallGenericType {
        return this._selectedSupplier;
    }
    set selectedSupplier(item: ISmallGenericType) {
        this._selectedSupplier = item;
        if (this.selectedSupplier) {
            if (this.supplierId !== this.selectedSupplier.id) {
                this.supplierId = this.selectedSupplier.id;
                this.supressNote = false;
            }

            this.loadSupplier(this.selectedSupplier.id);
        }
    }

    private _selectedVoucherSeriesId;
    get selectedVoucherSeriesId(): number {
        return this._selectedVoucherSeriesId;
    }
    set selectedVoucherSeriesId(id: number) {
        this._selectedVoucherSeriesId = id;
        if (this.invoice) {
            this.invoice.voucherSeriesId = this._selectedVoucherSeriesId;
        }
    }

    private _selecedInvoiceDate: any;
    get selectedInvoiceDate() {
        return this._selecedInvoiceDate;
    }
    set selectedInvoiceDate(date: any) {
        this._selecedInvoiceDate = date;

        if (this.invoice) {
            this.invoice.invoiceDate = this.selectedInvoiceDate;
            if ((!this.loadingInvoice) && (this._selecedInvoiceDate))
                this.selectedVoucherDate = this.selectedInvoiceDate;
            this.setDueDate();
            this.setTimeDiscount();
        }
    }

    private _selectedVoucherDate: any;
    get selectedVoucherDate() {
        return this._selectedVoucherDate;
    }
    set selectedVoucherDate(date: any) {
        const oldDate = this._selectedVoucherDate;
        this._selectedVoucherDate = date;
        if (this.invoice) {
            this.invoice.voucherDate = this.selectedVoucherDate;
            this.invoice.currencyDate = this.currencyDate = this.selectedVoucherDate;
            if (!this.loadingInvoice && date) {
                _.forEach(this.invoice.accountingRows, (r) => {
                    r.date = date;
                });
            }
        }

        this.loadAccountYear(this.selectedVoucherDate);

        if (oldDate && oldDate !== this._selectedVoucherDate) {
            this.setVoucherDateOnAccountingRows();
        }
    }

    private _selectedPaymentInfo: IPaymentInformationViewDTO;
    get selectedPaymentInfo(): IPaymentInformationViewDTO {
        return this._selectedPaymentInfo;
    }
    set selectedPaymentInfo(item: IPaymentInformationViewDTO) {
        this._selectedPaymentInfo = item;

        if (this.invoice) {
            this.invoice.sysPaymentTypeId = item ? item.sysPaymentTypeId : null;
            this.invoice.paymentNr = item ? item.paymentNr : null;
        }
    }

    private _selectedCustomerInvoice: any;
    get selectedCustomerInvoice() {
        return this._selectedCustomerInvoice;
    }
    set selectedCustomerInvoice(item: any) {
        this._selectedCustomerInvoice = item;
        if (this.invoice && !this.loadingInvoice) {
            this.orderNrChanging(item ? item.number : 0);
        }
    }

    get selectedCustomerInvoiceName() {
        return this._selectedCustomerInvoice ? this._selectedCustomerInvoice.name : " ";
    }

    private _selectedProject: any;
    get selectedProject() {
        return this._selectedProject;
    }
    set selectedProject(item: any) {
        this._selectedProject = item;
        if (this.invoice && !this.loadingInvoice) {
            this.projectChanging(item ? item.id : 0);
        }
    }

    get selectedProjectName() {
        return this._selectedProject ? this._selectedProject.name : " ";
    }

    private loadingFiles = false;
    private invoiceImage: any;
    private invoiceHasImage = false;

    public draft = false;
    private _isDraft = false;
    get isDraft(): boolean {
        return this.isNew || this.invoice.originStatus === SoeOriginStatus.Draft;
    }
    private isLocked = false;
    private canEditLocked = false;

    // Functions
    saveFunctions: any = [];

    private edit: ng.IFormController;
    private modalInstance: any;
    public ediEntryId: number;
    private scanningEntryId: number;
    public supplierInvoiceId: number;
    public ediType: TermGroup_EDISourceType; //Used for viewmode
    public supplierId: number;

    // Default account dimensions
    private _defaultAccountDim2Id: number;
    get defaultAccountDim2Id(): number {
        return this._defaultAccountDim2Id;
    }
    set defaultAccountDim2Id(item: number) {
        var previousDimId = this._defaultAccountDim2Id ? this._defaultAccountDim2Id : 0;
        this._defaultAccountDim2Id = item;
        if (!this.loadingInvoice && this.invoice) {
            this.setVatDeduction(2, this._defaultAccountDim2Id);
            this.invoice.defaultDim2AccountId = item;
            if (!this.isLocked) {
                this.updateAccountRowDimAccounts(2, this.invoice.defaultDim2AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim3Id: number;
    get defaultAccountDim3Id(): number {
        return this._defaultAccountDim3Id;
    }
    set defaultAccountDim3Id(item: number) {
        var previousDimId = this._defaultAccountDim3Id ? this._defaultAccountDim3Id : 0;
        this._defaultAccountDim3Id = item;
        if (!this.loadingInvoice && this.invoice) {
            this.setVatDeduction(3, this._defaultAccountDim3Id);
            this.invoice.defaultDim3AccountId = item;
            if (!this.isLocked) {
                this.updateAccountRowDimAccounts(3, this.invoice.defaultDim3AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim4Id: number;
    get defaultAccountDim4Id(): number {
        return this._defaultAccountDim4Id;
    }
    set defaultAccountDim4Id(item: number) {
        var previousDimId = this._defaultAccountDim4Id ? this._defaultAccountDim4Id : 0;
        this._defaultAccountDim4Id = item;
        if (!this.loadingInvoice && this.invoice) {
            this.setVatDeduction(4, this._defaultAccountDim4Id);
            this.invoice.defaultDim4AccountId = item;
            if (!this.isLocked) {
                this.updateAccountRowDimAccounts(4, this.invoice.defaultDim4AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim5Id: number;
    get defaultAccountDim5Id(): number {
        return this._defaultAccountDim5Id;
    }
    set defaultAccountDim5Id(item: number) {
        var previousDimId = this._defaultAccountDim5Id ? this._defaultAccountDim5Id : 0;
        this._defaultAccountDim5Id = item;
        if (!this.loadingInvoice && this.invoice) {
            this.setVatDeduction(5, this._defaultAccountDim5Id);
            this.invoice.defaultDim5AccountId = item;
            if (!this.isLocked) {
                this.updateAccountRowDimAccounts(5, this.invoice.defaultDim5AccountId, previousDimId);
            }
        }
    }

    private _defaultAccountDim6Id: number;
    get defaultAccountDim6Id(): number {
        return this._defaultAccountDim6Id;
    }
    set defaultAccountDim6Id(item: number) {
        var previousDimId = this._defaultAccountDim6Id ? this._defaultAccountDim6Id : 0;
        this._defaultAccountDim6Id = item;
        if (!this.loadingInvoice && this.invoice) {
            this.setVatDeduction(6, this._defaultAccountDim6Id);
            this.invoice.defaultDim6AccountId = item;
            if (!this.isLocked) {
                this.updateAccountRowDimAccounts(6, this.invoice.defaultDim6AccountId, previousDimId);
            }
        }
    }

    //@ngInject
    constructor(
        private $uibModal,
        private $window,
        private $timeout: ng.ITimeoutService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private productService: IProductService,
        private accountingService: IAccountingService,
        private supplierService: ISupplierService,
        private projectService: IProjectService,
        private addInvoiceToAttestFlowService: IAddInvoiceToAttestFlowService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private shortCutService: IShortCutService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private scopeWatcherService: IScopeWatcherService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private readonly requestReportService: IRequestReportService
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        shortCutService.bindSave($scope, () => {
            if (this.modifyPermission)
                this.save(false);
        });
        shortCutService.bindSaveAndClose($scope, () => {
            if (this.modifyPermission)
                this.save(true);
        });
        shortCutService.bindNext(this.$scope, () => {
            if (this.showNavigationButtons && this.modifyPermission)
                this.save(false, true);
        });
        shortCutService.bindEnterAsTab($scope);

        // Events
        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            if (parameters && parameters.sourceGuid === this.guid) {
                return;
            }
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
        });

        this.modalInstance = $uibModal;

        this.guid = Guid.newGuid();

        // Config parameters
        this.currentAccountYearId = soeConfig.accountYearId;
        this.currentAccountYearIsOpen = soeConfig.accountYearIsOpen;
        this.showEditSeqNrButton = CoreUtility.isSupportSuperAdmin;
        this.container = AccountingRowsContainers.SupplierInvoice;

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookUp())
            .onLoadData(() => this.load())
            .onAfterFirstLoad(() => this.onAfterFirstLoad())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.setupSubscribtions($scope);
    }

    public onInit(parameters: any) {
        if (!parameters.noTabs) {
            this.setTabCallbacks(() => this.onTabActivated(), () => this.onTabDeActivated());
        }

        this.guid = parameters.guid;
        this.ediEntryId = parameters.ediEntryId || 0;
        this.supplierInvoiceId = parameters.id || 0;
        this.ediType = parameters.ediType || 0;
        this.supplierId = parameters.supplierId || 0;
        this.keepOpen = parameters.keepOpen || false;
        this.isProjectCentral = parameters.isProjectCentral || false;
        this.invoiceIds = [];
        if (parameters.invoiceIds && parameters.invoiceIds.length > 0) {
            this.showNavigationButtons = true;
            this.invoiceIds = parameters.invoiceIds;
        }

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        if (parameters.secondaryDirtyEvent) {
            this.dirtyHandler.setSecondaryEvent(Constants.EVENT_INVOICE_MODIFIED);
            this.usesSecondaryDirtyEvent = true;
        }

        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.SupplierInvoice, SoeEntityImageType.SupplierInvoice, () => this.supplierInvoiceId);
        this.setWidth();
        this.startFlow();
    }

    public setupSubscribtions($scope: ng.IScope) {
        this.messagingService.subscribe(Constants.EVENT_DELETE_SUPPLIER_INVOICE_IMAGE, (x) => {
            if (this.guid === x.guid) {
                this.invoiceImage.isDeleted = true;
                this.setInvoiceHasImage(false);
                this.dirtyHandler.isDirty = true;
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_ACCOUNTING_ROWS_READY, (x) => {
            if (x != this.guid) return;
            if (this.ediType === TermGroup_EDISourceType.Scanning && this.isNew) {
                this.generateAccountingRows(this.recalculateVat);
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, (x) => {
            if (x.detailedCodingRows && this.supplierInvoiceProductRowsImport) {
                this.progress.startLoadingProgress([() => {
                    return this.supplierService.getSupplierProductRows(this.supplierInvoiceId)
                        .then(productRows => {
                            this.generateAccountingRows(true, productRows, true);
                        })
                }])
            } else {
                this.generateAccountingRows(true);
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_ACCOUNTING_ROWS_MODIFIED, (parentGuid) => {
            if (parentGuid == this.guid) {
                this.updateAllAccountRowDimAccounts();
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_DIALOG, (parentGuid) => {
            if (parentGuid == this.guid) {
                if (this.invoice.seqNr)
                    this.$scope.$broadcast('accountDistributionName', this.invoice.seqNr + ", " + this.supplier.supplierNr + " " + this.supplier.name);
                else {
                    this.translationService.translate("economy.supplier.invoice.seqnr").then(label => {
                        this.$scope.$broadcast('accountDistributionName', "[" + label + "]" + ", " + this.supplier.supplierNr + " " + this.supplier.name);
                    });
                }

            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_RELOAD_INVOICE, (invoiceId) => {
            this.flowHandler.starting().finally(() => {
                this.reloadSupplierInvoice((invoiceId || invoiceId === -1) ? invoiceId : this.supplierInvoiceId)
            });
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_SET_INVOICE_IDS, (invoiceIds) => { this.invoiceIds = invoiceIds }, $scope);

        this.messagingService.subscribe(Constants.EVENT_TRANSFER_ATTEST_ROWS, (invoiceId) => {
            if (Number(invoiceId) === this.supplierInvoiceId)
                this.transferAdjustedAttestRows();
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_TOGGLE_INVOICE_EDIT_PARAMS, (params: any) => {
            this.showBlockPaymentButton = params.showBlockPaymentButton;
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_ADD_INVOICE_TO_ATTESTFLOW, (invoiceId) => {
            if (Number(invoiceId) === this.supplierInvoiceId)
                this.initShowAddInvoiceToAttestFlow();
        }, $scope);
        this.messagingService.subscribe(Constants.EVENT_COSTALLOCATIONDIRECTIVE_SETUP, (guid) => {
            if (this.guid === guid) {
                this.costAllocationSetupDone = true;
                if (this.addDefaultCostAllocationRow) {
                    this.$timeout(() => {
                        this.addProjectRow();
                    });
                }
            }
        }, this.$scope);
    }

    public invoiceImageUploaded(result) {
        if (result) {
            this.getImageByFileId(result.id);
            this.dirtyHandler.isDirty = true;
        }
    }

    private getImageByFileId(id: number) {
        if (id) {
            this.supplierService.getSupplierInvoiceImageByFileId(id).then(result => {
                this.invoiceImage = result;
                this.setInvoiceHasImage(true);
            });
        }
    }

    private getImageByInvoiceId(invoiceId: number) {
        if (invoiceId) {
            this.supplierService.getSupplierInvoiceImage(invoiceId).then(result => {
                this.invoiceImage = result;
                this.setInvoiceHasImage(true);
            });
        }
    }

    private getImageByEdiEntryId(ediEntryId: number) {
        if (ediEntryId) {
            this.supplierService.getSupplierInvoiceImageFromEdi(ediEntryId)
                .then(result => {
                    this.invoiceImage = result;
                    this.invoice.hasImage = true;
                });
        }
    }

    private createImageByEdiEntryId(ediEntryId: number): ng.IPromise<any> {
        if (ediEntryId) {
            var dict: any[] = [];
            dict.push(ediEntryId);
            return this.supplierService.generateReportForEdi(dict).then((result) => {
            });
        }
    }

    // SETUP

    private onDoLookUp(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadCompanyAccounts(),
            this.loadTimeCodes(),
            this.loadSuppliers(true)]).then(() => {
                return this.$q.all([
                    this.loadBillingTypes(),
                    this.loadVatTypes(),
                    this.loadCurrencies(),
                    this.loadPaymentConditions(),
                    this.loadAccountDims(),
                    this.loadVoucherSeries(this.currentAccountYearId)]);
            });
    }

    private onAfterFirstLoad() {
        this.setupWatchers();
        this.focusService.focusByName("ctrl_selectedSupplier");
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission;
        this.useCurrency = response[Feature.Economy_Preferences_Currency].modifyPermission;
        this.reportPermission = response[Feature.Economy_Distribution_Reports_Selection].modifyPermission && response[Feature.Economy_Distribution_Reports_Selection_Download].modifyPermission;
        this.editSupplierPermission = response[Feature.Economy_Supplier_Suppliers_Edit].modifyPermission;
        //this.unlockPermission = x[Feature.Economy_Supplier_Invoice_Unlock].modifyPermission;
        this.uploadImagePermission = response[Feature.Economy_Supplier_Invoice_AddImage].modifyPermission;
        this.changeCompanyPermission = response[Feature.Economy_Supplier_Invoice_ChangeCompany].modifyPermission;
        this.unlockAccountingRowsPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit_UnlockAccounting].modifyPermission;
        this.finvoicePermission = response[Feature.Economy_Supplier_Invoice_Finvoice].modifyPermission || response[Feature.Economy_Supplier_Invoice_Finvoice].readPermission;
        this.intrastatPermission = response[Feature.Economy_Intrastat].modifyPermission;

        // Attest flow
        this.attestFlowAdminPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Admin].modifyPermission;
        // If user has admin permission, set all the rest attest flow permissions also
        this.attestFlowPermission = this.attestFlowAdminPermission || response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;
        this.attestFlowAddPermission = this.attestFlowAdminPermission || response[Feature.Economy_Supplier_Invoice_AttestFlow_Add].modifyPermission;
        this.attestFlowCancelPermission = this.attestFlowAdminPermission || response[Feature.Economy_Supplier_Invoice_AttestFlow_Cancel].modifyPermission;
        this.attestFlowTransferRowsPermission = this.attestFlowAdminPermission || response[Feature.Economy_Supplier_Invoice_AttestFlow_TransferToLedger].modifyPermission;

        this.projectPermission = response[Feature.Economy_Supplier_Invoice_Project].modifyPermission;
        this.ordersPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.purchasePermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        this.productRowsPermission = response[Feature.Economy_Supplier_Invoice_ProductRows].modifyPermission;

        // VAT codes
        this.loadVatCodes();

    }

    private startFlow() {
        var features: Feature[] = [];
        features.push(Feature.Economy_Supplier_Invoice_Invoices_Edit);
        features.push(Feature.Economy_Preferences_Currency);                            // Use currency
        features.push(Feature.Economy_Distribution_Reports_Selection);                  // Invoice report
        features.push(Feature.Economy_Distribution_Reports_Selection_Download);         // Invoice report
        features.push(Feature.Economy_Supplier_Suppliers_Edit);                         // Edit supplier
        //features.push(Feature.Economy_Supplier_Invoice_Unlock);                       // Unlock invoice
        features.push(Feature.Economy_Supplier_Invoice_AddImage);                       // Add an invoice image
        features.push(Feature.Economy_Supplier_Invoice_ChangeCompany);                  // Change company
        features.push(Feature.Economy_Supplier_Invoice_AttestFlow);                     // AttestFlow
        features.push(Feature.Economy_Supplier_Invoice_AttestFlow_Admin);               // AttestFlow administrator
        features.push(Feature.Economy_Supplier_Invoice_AttestFlow_Add);                 // Add AttestFlow
        features.push(Feature.Economy_Supplier_Invoice_AttestFlow_Cancel);              // Cancel AttestFlow
        features.push(Feature.Economy_Supplier_Invoice_AttestFlow_TransferToLedger);    // Transfer attest rows to accounting rows
        features.push(Feature.Economy_Supplier_Invoice_Project);                        // Add project rows
        features.push(Feature.Billing_Order_Orders_Edit);                               // Edit orders
        features.push(Feature.Billing_Purchase_Purchase_Edit);                          // Edit Purchase
        features.push(Feature.Economy_Supplier_Invoice_Invoices_Edit_UnlockAccounting); // Unlock accountingrows
        features.push(Feature.Economy_Supplier_Invoice_Finvoice);                       // Finvoice permission
        features.push(Feature.Economy_Intrastat);                                       // Intrastat permission
        features.push(Feature.Economy_Supplier_Invoice_ProductRows);                    // Supplier invoice product rows permission

        var permissionRequests = _.map(features, (f) => { return { feature: f, loadModifyPermissions: true } });
        this.flowHandler.start(permissionRequests);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();


        if (this.changeCompanyPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.supplier.invoice.changecompany", IconLibrary.FontAwesome, "fa-building", () => {
                this.showChangeCompany();
            }, null, () => {
                return this.isNew || !this.invoice || this.isLocked;
            })));
        }

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.supplier.invoice.metadata", IconLibrary.FontAwesome, "fa-code", () => {
            this.showInterpretationStatusDialog();
        }, null, () => {
            return !this.invoiceInterpretation
        })))

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.supplier.invoice.blockforpayment", IconLibrary.FontAwesome, "fa-lock-alt okColor", () => {
            this.blockPayment();
        }, null, () => {
            return this.isNew || !this.invoice || this.invoice.blockPayment || !this.showBlockPaymentButton;
        })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.supplier.invoice.unblockforpayment", IconLibrary.FontAwesome, "fa-unlock-alt warningColor", () => {
            this.unblockPayment();
        }, null, () => {
            return this.isNew || !this.invoice || !this.invoice.blockPayment || !this.showBlockPaymentButton;
        })));

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.supplier.invoice.showaccountdistributionhead", IconLibrary.FontAwesome, "fa-calendar-alt", () => {
            this.showAccountDistributionHead();
        }, null, () => {
            return this.accountDistributionHeadId == 0;
        })));

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "common.settings", IconLibrary.FontAwesome, "fa-cog", () => {
            this.updateAccordionSettings();
        }, null, null
        )));

        if (this.intrastatPermission && this.intrastatImportOriginType === SoeOriginType.SupplierInvoice) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "common.commoditycodes.changeintrastatdata", IconLibrary.FontAwesome, "fa-globe", () => {
                this.changeIntrastat();
            }, null, () => {
                return (!this.supplier || !this.supplier.isEUCountryBased || this.isNew);
            }
            )));
        }

        // Functions
        const keys: string[] = [
            "core.save",
            "core.saveandclose",
            "core.saveandopennext"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions.push({ id: SupplierInvoiceEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)" });
            this.saveFunctions.push({ id: SupplierInvoiceEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] + " (Ctrl+Enter)" });
            if (this.showNavigationButtons) {
                this.saveFunctions.push({ id: SupplierInvoiceEditSaveFunctions.SaveAndOpenNext, name: terms["core.saveandopennext"] + " (Ctrl+H)" });
            }
        });

        var x = _.map(this.invoiceIds, 'id');

        this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (invoiceId) => {
            var items = this.invoiceIds.filter((x) => x.id === invoiceId);
            if (items && items.length > 0) {
                this.showInfoMessage = false;
                this.setIdAndTypeFromInvoiceIds(items[0]);
                this.load();
            }
        }, x, this.supplierInvoiceId);

        var sizeGroup = ToolBarUtility.createGroup();

        sizeGroup.buttons.push(new ToolBarButton("", "", IconLibrary.FontAwesome, "fa-arrow-to-left",
            () => { this.setWidth(0); },
            null,
            () => { return (!this.invoiceHasImage || this.imageAccordion) }
        ))
        sizeGroup.buttons.push(new ToolBarButton("", "", IconLibrary.FontAwesome, "fa-columns",
            () => { this.setWidth(); },
            null,
            () => { return (!this.invoiceHasImage || this.imageAccordion) }
        ))
        sizeGroup.buttons.push(new ToolBarButton("", "", IconLibrary.FontAwesome, "fa-arrow-to-right",
            () => { this.setWidth(12); },
            null,
            () => { return (!this.invoiceHasImage || this.imageAccordion) }
        ))
        this.toolbar.addButtonGroup(sizeGroup);


    }

    private setupWatchers() {
        this.watchUnRegisterCallbacks.push(
            // Convert currency amounts
            this.$scope.$watch(() => this.invoice.totalAmountCurrency, (newValue, oldValue) => {
                if (newValue != oldValue) {
                    this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
                }
            }),
            this.$scope.$watch(() => this.invoice.vatAmountCurrency, (newValue, oldValue) => {
                if (newValue != oldValue) {
                    this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
                }
            }),
            this.$scope.$watch(() => this.invoice.currencyId, (newValue, oldValue) => {
                if (newValue && oldValue && newValue != oldValue) {
                    this.currencyChanged();
                }
            }),
            this.$scope.$watch(() => this.ledgerCurrencyCode, (newValue, oldValue) => {
                if (newValue && oldValue && newValue != oldValue) {
                    this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
                    this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
                }
            }),
            this.$scope.$watch(() => this.currencyRate, (newValue, oldValue) => {
                if (newValue != oldValue) {
                    this.currencyChanged();
                }
            }),
            this.$scope.$watch(() => this.invoiceScale, (newValue, oldValue) => {
                if (newValue != oldValue && oldValue !== undefined) {
                    this.invoiceScaleChanged();
                }
            })
        );
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
            this.scopeWatcherService.suspendWatchers(this.$scope);
        });
    }

    private currencyChanged() {
        this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
        this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
    }

    private invoiceScaleChanged() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.BillingSupplierInvoiceScale, this.invoiceScale * 100);
    }

    private setupTabIndexes() {
        this.shortCutService.UseTabIndexWhenTabAsEnter = this.simplifiedRegistration;

        this.tabIndexes = new Array();
        if (this.simplifiedRegistration) {
            this.tabIndexes['supplier'] = 1;
            this.tabIndexes['invoicenr'] = 2;
            this.tabIndexes['ocr'] = 3;
            this.tabIndexes['invoicedate'] = 4;
            this.tabIndexes['voucherdate'] = 5;
            this.tabIndexes['duedate'] = 6;
            this.tabIndexes['total'] = 7;
            this.tabIndexes['vat'] = 8;
            this.tabIndexes['paymentInfo'] = 9;
            this.tabIndexes['accountingRows'] = 10;
            this.tabIndexes['save'] = 11;
            this.tabIndexes['error'] = 12;
        }
        // handle changes to tabIndexes also when using enter (setFocusByName below)
    }

    // LOOKUPS
    private clearDefaultDims() {
        if (this.invoice) {
            this.defaultAccountDim2Id = null;
            this.defaultAccountDim3Id = null;
            this.defaultAccountDim4Id = null;
            this.defaultAccountDim5Id = null;
            this.defaultAccountDim6Id = null;
        }
    }

    private setIdAndTypeFromInvoiceIds(invoiceIdsItem: any) {

        if (invoiceIdsItem.type === TermGroup_SupplierInvoiceType.Invoice ||
            invoiceIdsItem.type === TermGroup_SupplierInvoiceType.Uploaded) {
            this.supplierInvoiceId = invoiceIdsItem.id;
            this.ediEntryId = 0;
            this.ediType = TermGroup_EDISourceType.Unset;
        }
        else if (invoiceIdsItem.type === TermGroup_SupplierInvoiceType.Scanning) {
            this.ediEntryId = invoiceIdsItem.id;
            this.supplierInvoiceId = 0;
            this.ediType = TermGroup_EDISourceType.Scanning;
        }
    }

    private load(openNextInvoice: boolean = false, previousEdiEntryId: number = 0, previousSupplierInvoiceId: number = 0, ignoreAddCostAllocation = false): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.invoiceIsLoaded = false;

        if (openNextInvoice && this.invoiceIds.length > 1) {
            var i: number = 0;
            for (i; i < this.invoiceIds.length; i++) {
                let r = this.invoiceIds[i];
                if ((r.id === previousSupplierInvoiceId && (r.type === TermGroup_SupplierInvoiceType.Invoice || r.type === TermGroup_SupplierInvoiceType.Uploaded)) ||
                    r.id === previousEdiEntryId && r.type === TermGroup_SupplierInvoiceType.Scanning) {
                    break;
                }
            }

            if (i < this.invoiceIds.length - 1) {
                this.setIdAndTypeFromInvoiceIds(this.invoiceIds[i + 1]);
            }
            else if (i == this.invoiceIds.length - 1) {
                this.ediEntryId = 0;
                this.ediType = TermGroup_EDISourceType.Unset;

                const keys: string[] = [
                    "core.warning",
                    "economy.supplier.invoice.lastinvoiceinlistmessage"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    var modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.invoice.lastinvoiceinlistmessage"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                });
            }

            this.messagingService.publish(Constants.EVENT_INVOICE_CHANGED, this.supplierInvoiceId);
        }

        if (this.supplierInvoiceId > 0 && !this.ediType) {
            this.loadingInvoice = true;

            this.orderRowsLoaded = false;
            this.projectRowsLoaded = false;
            this.costAllocationRowsLoaded = false;

            this.clearDefaultDims();
            this.supplierService.getInvoice(this.supplierInvoiceId, false, false, true).then((x: SupplierInvoiceDTO) => {
                this.invoice = x;
                this.invoice.modified = this.invoice.modified ?? new Date();
                this.isNew = false;
                this.invoiceNr = this.invoice.invoiceNr;

                this.setLocked();

                //EdiEntryId is not passed, i.e. check if Invoice has EdiEntry
                if (this.ediEntryId == 0) {
                    this.LoadEdiEntryFromInvoice();
                }

                // Fix dates
                if (this.invoice.invoiceDate)
                    this.invoice.invoiceDate = new Date(<any>this.invoice.invoiceDate);
                if (this.invoice.dueDate)
                    this.invoice.dueDate = new Date(<any>this.invoice.dueDate);
                if (this.invoice.voucherDate)
                    this.invoice.voucherDate = new Date(<any>this.invoice.voucherDate);
                if (this.invoice.timeDiscountDate)
                    this.invoice.timeDiscountDate = new Date(<any>this.invoice.timeDiscountDate);

                //Default internal accounts
                if (this.invoice.defaultDim2AccountId && this.invoice.defaultDim2AccountId > 0)
                    this.defaultAccountDim2Id = this.invoice.defaultDim2AccountId;
                else
                    this.defaultAccountDim2Id = undefined;
                if (this.invoice.defaultDim3AccountId && this.invoice.defaultDim3AccountId > 0)
                    this.defaultAccountDim3Id = this.invoice.defaultDim3AccountId;
                else
                    this.defaultAccountDim3Id = undefined;
                if (this.invoice.defaultDim4AccountId && this.invoice.defaultDim4AccountId > 0)
                    this.defaultAccountDim4Id = this.invoice.defaultDim4AccountId;
                else
                    this.defaultAccountDim4Id = undefined;
                if (this.invoice.defaultDim5AccountId && this.invoice.defaultDim5AccountId > 0)
                    this.defaultAccountDim5Id = this.invoice.defaultDim5AccountId;
                else
                    this.defaultAccountDim5Id = undefined;
                if (this.invoice.defaultDim6AccountId && this.invoice.defaultDim6AccountId > 0)
                    this.defaultAccountDim6Id = this.invoice.defaultDim6AccountId;
                else
                    this.defaultAccountDim6Id = undefined;

                if (this.invoice.actorId) {
                    this.settingInvoiceSupplier = true;
                    this.selectedSupplier = _.find(this.suppliers, { id: this.invoice.actorId });
                    if (!this.selectedSupplier) {
                        //If supplier has been deleted or deactivated, it may not be in the supplier list.
                        this.selectedSupplier = {
                            name: "-",
                            id: this.invoice.actorId
                        }
                    }

                }
                else {
                    this.selectedSupplier = undefined;
                }
                this.selectedInvoiceDate = this.invoice.invoiceDate;
                this.selectedVoucherDate = this.invoice.voucherDate;
                this.currencyRate = this.invoice.currencyRate;

                if (this.invoice.currencyDate)
                    this.invoice.currencyDate = new Date(<any>this.invoice.currencyDate);

                this.currencyDate = this.invoice.currencyDate;

                if (this.invoice.orderNr)
                    this.selectedCustomerInvoice = { id: this.invoice.orderCustomerInvoiceId, name: this.invoice.orderNr + " " + this.invoice.orderCustomerName, number: this.invoice.orderNr, projectId: this.invoice.orderProjectId };
                else
                    this.selectedCustomerInvoice = null;
                if (this.invoice.projectId)
                    this.selectedProject = { id: this.invoice.projectId, name: this.invoice.projectNr + " " + this.invoice.projectName, number: this.invoice.projectNr };
                else
                    this.selectedProject = null;

                if (this.invoice.vatType.valueOf() === TermGroup_InvoiceVatType.Contractor ||
                    this.invoice.vatType.valueOf() === TermGroup_InvoiceVatType.NoVat) {
                    this.invoice.vatAmount = 0;
                    this.invoice.vatAmountCurrency = 0;
                }

                this.selectedVoucherSeriesId = this.invoice.voucherSeriesId;
                this.draft = (this.invoice.originStatus === SoeOriginStatus.Draft);
                if (x.orderNr)
                    this.invoice.orderNr = x.orderNr;
                var accountingRows = SupplierInvoiceRowDTO.toAccountingRowDTOs(this.invoice.supplierInvoiceRows);
                this.invoice.accountingRows = _.orderBy(accountingRows.filter(x => x.type === AccountingRowType.AccountingRow && x.state !== SoeEntityState.Deleted), 'rowNr');
                this.invoice.supplierInvoiceAttestRows = accountingRows.filter(x => x.type === AccountingRowType.SupplierInvoiceAttestRow);
                _.forEach(this.invoice.supplierInvoiceAttestRows, row => {
                    if (row.attestStatus == SupplierInvoiceAccountRowAttestStatus.Processed) {
                        row.isProcessed = true;
                        row['isReadOnly'] = true;
                        row['isAttestReadOnly'] = true;
                    } else if (row.attestStatus == SupplierInvoiceAccountRowAttestStatus.Deleted) {
                        row.isDeleted = true;
                        row['isReadOnly'] = true;
                        row['isAttestReadOnly'] = true;
                    }
                });

                if (this.supplierInvoiceId)
                    this.loadAttestWorkFlowHeadFromInvoiceId(this.supplierInvoiceId);

                // Mark image gallery with an asterix if any images or attachments are on the invoice
                var flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
                var statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(this.invoice.statusIcon);

                if (statusIcons.contains(SoeStatusIcon.Attachment) || statusIcons.contains(SoeStatusIcon.Image))
                    this.filesHelper.nbrOfFiles = '*';

                //Load files if expander is open
                this.filesHelper.reset();
                if (this.filesExpanderOpen) {
                    this.loadFiles();
                }

                this.setInvoiceHasImage(this.invoice.hasImage);
                this.invoiceImage = this.invoiceHasImage ? this.invoice.image : null;

                /*if (this.invoice.orderNr) {
                    var customerInvoice = _.find(this.customerInvoices, { orderNumber: this.invoice.orderNr });

                    if (customerInvoice)
                        this.selectedCustomerInvoice = customerInvoice.invoiceNr;
                }
                /*if (!this.invoice.supplierInvoiceOrderRows)
                    this.invoice.supplierInvoiceOrderRows = [];

                if (!this.invoice.supplierInvoiceProjectRows)
                    this.invoice.supplierInvoiceProjectRows = [];*/

                if (this.projectRowsExpanderOpen)
                    this.loadProjectRows(null);

                if (this.projectOrderExpanderOpen)
                    this.loadOrderRows(null, undefined);

                if (this.costAllocationExpanderOpen || (!ignoreAddCostAllocation && this.invoice.originStatus === SoeOriginStatus.Draft && this.selectedProject)) {
                    this.costAllocationRowsLoaded = false;
                    this.loadCostAllocationRows(null, null, ignoreAddCostAllocation);
                }


                this.updateTabCaption();

                //only image uploaded for this invoice
                if (this.invoice.type == TermGroup_SupplierInvoiceType.Uploaded.valueOf()) {
                    this.invoice.type = TermGroup_SupplierInvoiceType.Invoice.valueOf();
                    this.invoice.billingType = TermGroup_BillingType.Debit;
                    this.invoice.invoiceNr = null;
                    this.invoice.vatType = this.defaultVatType;
                    this.invoice.currencyId = this.currencies[0].currencyId;    // Base currency is first in collection
                    this.invoice.currencyDate = this.currencyDate = CalendarUtility.getDateToday();
                    this.selectedInvoiceDate = null;
                    this.selectedVoucherDate = this.currencyDate;
                    this.paymentConditionDays = this.defaultPaymentConditionDays;
                    this.invoice.interimInvoice = this.allowInterim;
                    this.draft = this.defaultDraft;
                    this.settingInvoiceSupplier = false;
                }

                //get data from accountdistribution
                var acRow = _.find(this.invoice.accountingRows, (row: AccountingRowDTO) => row.accountDistributionHeadId > 0);  //&& row.isAccrualAccount);
                if (acRow) {
                    this.accountDistributionHeadId = acRow.accountDistributionHeadId;
                    this.accountingService.getAccountDistributionHead(this.accountDistributionHeadId).then((head) => {
                        this.accountDistributionName = head.name;
                    });
                }

                this.dirtyHandler.isDirty = false;
                this.invoiceLoaded();

                deferral.resolve();
            });
        }
        else if (this.ediType === TermGroup_EDISourceType.EDI) {

        }
        else if (this.ediType === TermGroup_EDISourceType.Scanning) {
            this.loadingInvoice = true;
            var tmpEdiEntryId = this.ediEntryId; //don't loose ediEntryId when creating new


            this.supplierService.getInterpretedInvoice(this.ediEntryId).then((x: IInvoiceInterpretationDTO) => {
                this.new();
                this.attestWorkFlowHead = null;
                this.existingAttestFlow = false;
                this.setInvoiceHasImage(true);
                this.dirtyHandler.isDirty = true;
                this.ediEntryId = tmpEdiEntryId;
                this.convertInterpretationToSupplierInvoice(x);
                this.getImageByEdiEntryId(this.ediEntryId);

                this.updateTabCaption();

                this.invoiceLoaded();

                deferral.resolve();
            });
        }
        else if (this.ediType === TermGroup_EDISourceType.Finvoice) {
            this.loadingInvoice = true;

            if (this.supplierInvoiceId > 0) {
                this.supplierService.getEdiEntryFromInvoice(this.supplierInvoiceId).then((x: EdiEntryDTO) => {
                    this.ediEntryId = x.ediEntryId;
                    deferral.resolve();
                });
            }
            else {
                this.supplierService.getEdiEntry(this.ediEntryId, true).then((x: EdiEntryDTO) => {
                    this.setInvoiceHasImage(false);
                    this.isNew = true;
                    this.dirtyHandler.isDirty = true;
                    this.convertFinvoiceToSupplierInvoice(x);

                    this.$timeout(() => {
                        this.invoiceLoaded();
                    });
                    deferral.resolve();
                });
            }
        }
        else {
            this.new(openNextInvoice);
            this.invoiceLoaded();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadCostAllocationRows(selectedProject, selectedOrder, ignoreAddDefault = true) {
        if (this.ignoreLoadCostAllocationRows) {
            this.ignoreLoadCostAllocationRows = false;
            return;
        }

        if (!this.costAllocationRowsLoaded && this.invoice && (!this.blockProjectLoad || selectedProject)) {
            if (this.invoice.invoiceId) {
                this.progress.startLoadingProgress([() => {
                    this.invoice.supplierInvoiceCostAllocationRows = [];
                    return this.supplierService.getSupplierInvoiceOrderProjectRows(this.invoice.invoiceId).then((x: any[]) => {
                        this.invoice.supplierInvoiceCostAllocationRows = x;

                        if (!this.invoice.supplierInvoiceCostAllocationRows)
                            this.invoice.supplierInvoiceCostAllocationRows = [];

                        if (selectedOrder)
                            this.setOrderSelected(selectedOrder, selectedProject ? false : true);
                        else if (selectedProject)
                            this.setProjectSelected(selectedProject);

                        // Check if default row should be added
                        if (!ignoreAddDefault && this.invoice.supplierInvoiceCostAllocationRows.length === 0 && this.selectedProject && this.selectedProject.id > 0) {
                            if (!this.costAllocationSetupDone) {
                                this.addDefaultCostAllocationRow = true;

                                if (!this.costAllocationExpanderOpen)
                                    this.costAllocationExpanderOpen = true;
                            }
                            else {
                                this.$timeout(() => {
                                    this.addProjectRow();
                                });
                            }
                        }

                        this.costAllocationRowsLoaded = true;
                    });
                }]);
            }
            else if (this.isNew) {
                this.invoice.supplierInvoiceCostAllocationRows = [];

                if (selectedOrder)
                    this.setOrderSelected(selectedOrder, selectedProject ? false : true);
                else if (selectedProject)
                    this.setProjectSelected(selectedProject);

                if (!this.costAllocationSetupDone && selectedProject) {
                    this.addDefaultCostAllocationRow = true;

                    if (!this.costAllocationExpanderOpen)
                        this.costAllocationExpanderOpen = true;
                }

                this.costAllocationRowsLoaded = true;
            }
        }
    }

    private loadProjectRows(selectedProject) {
        if (!this.projectRowsLoaded && this.invoice && this.invoice.invoiceId && (!this.blockProjectLoad || selectedProject)) {
            this.progress.startLoadingProgress([() => {
                this.invoice.supplierInvoiceProjectRows = [];
                return this.supplierService.getSupplierInvoiceProjectRows(this.invoice.invoiceId).then((x: any[]) => {
                    this.invoice.supplierInvoiceProjectRows = x;

                    if (selectedProject)
                        this.setProjectSelected(selectedProject);

                    this.projectRowsLoaded = true;
                    this.blockProjectLoad = false;
                });
            }]);
        }
    }

    private loadOrderRows(selectedOrder, ignoreSetProject) {
        if (!this.orderRowsLoaded && this.invoice && this.invoice.invoiceId && (!this.blockOrderLoad || selectedOrder)) {
            this.progress.startLoadingProgress([() => {
                this.invoice.supplierInvoiceOrderRows = [];
                return this.supplierService.getSupplierInvoiceOrderRows(this.invoice.invoiceId).then((x: any[]) => {
                    this.invoice.supplierInvoiceOrderRows = x;

                    if (selectedOrder)
                        this.setOrderSelected(selectedOrder, ignoreSetProject);

                    this.orderRowsLoaded = true;
                    this.blockOrderLoad = false;
                });
            }]);
        }
    }

    private convertFinvoiceToSupplierInvoice(finvoiceEntry: EdiEntryDTO) {
        if (!this.invoice)
            this.invoice = new SupplierInvoiceDTO();

        this.draft = this.defaultDraft;

        this.invoice.ediEntryId = finvoiceEntry.ediEntryId;
        if (finvoiceEntry.actorSupplierId && finvoiceEntry.actorSupplierId > 0)
            this._selectedSupplier = _.find(this.suppliers, { id: finvoiceEntry.actorSupplierId });
        this.invoiceNr = this.invoice.invoiceNr = finvoiceEntry.invoiceNr;
        this.invoice.ocr = finvoiceEntry.ocr ? finvoiceEntry.ocr : "";
        if (finvoiceEntry.invoiceDate)
            this.selectedInvoiceDate = this.selectedVoucherDate = new Date(<any>finvoiceEntry.invoiceDate);
        if (finvoiceEntry.dueDate)
            this.invoice.dueDate = new Date(<any>finvoiceEntry.dueDate);
        if (finvoiceEntry.billingType)
            this.invoice.billingType = finvoiceEntry.billingType;

        //Sum
        this.invoice.totalAmount = finvoiceEntry.sum;
        this.invoice.totalAmountCurrency = finvoiceEntry.sumCurrency;

        //Vat
        this.invoice.vatAmount = finvoiceEntry.sumVat;
        this.invoice.vatAmountCurrency = finvoiceEntry.sumVatCurrency;

        if (finvoiceEntry.sumVat === 0)
            this.recalculateVat = true;

        if (finvoiceEntry.vatRate && finvoiceEntry.vatRate > 0)
            this.vatRate = finvoiceEntry.vatRate;

        this.invoice.currencyRate = finvoiceEntry.currencyRate;
        var setSupplierCurrency = true;

        if (finvoiceEntry.currencyId) {
            var currency = _.find(this.currencies, { currencyId: finvoiceEntry.currencyId });
            if (currency) {
                this.currencyCode = currency.code;
                this.currencyDate = this.invoice.currencyDate = new Date(<any>finvoiceEntry.currencyDate);
                this.currencyRate = this.invoice.currencyRate = finvoiceEntry.currencyRate;
                this.invoice.currencyId = finvoiceEntry.currencyId;

                setSupplierCurrency = false;
            }
        }

        // SupplierChanged will trigger the generation of accounting rows
        // therefore we load supplier after all amount fields has been set
        if (finvoiceEntry.actorSupplierId && finvoiceEntry.actorSupplierId > 0) {
            this.loadSupplier(finvoiceEntry.actorSupplierId, setSupplierCurrency, this.recalculateVat, true);
        }
        else
            this.generateAccountingRows(this.recalculateVat);

        this.invoice.paymentNr = finvoiceEntry.iban;

        this.invoice.voucherSeriesId = this.defaultVoucherSeriesTypeId;
        _.forEach(this.voucherSeries, (voucherSerie) => {
            if (voucherSerie.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId) {
                this.selectedVoucherSeriesId = voucherSerie.voucherSeriesId;
            }
        });

        this.invoice.supplierInvoiceProjectRows = [];
        this.invoice.supplierInvoiceOrderRows = [];
        this.invoice.supplierInvoiceCostAllocationRows = [];
    }

    private setInvoiceHasImage(hasImage = true) {
        this.invoiceHasImage = hasImage
        this.setWidth();
    }

    private setOrderDetailsFromOrderNr(orderNr: string) {
        if (!orderNr) return;
        this.supplierService.getOrderForSupplierByOrderNr(orderNr).then(order => {
            if (order) {
                const data = {
                    invoice: { ...order }
                }
                this.setOrderSelected(data, false)
            }
        })
    }

    private convertInterpretationToSupplierInvoice(invoiceInterpretationIn: IInvoiceInterpretationDTO) {
        if (!this.invoice)
            this.invoice = new SupplierInvoiceDTO();

        this.draft = this.defaultDraft;
        this.invoiceInterpretation = new InvoiceInterpretationDTO(invoiceInterpretationIn);
        this.invoice.ediEntryId = this.invoiceInterpretation.context.ediEntryId; 
        this.scanningEntryId = this.invoiceInterpretation.context.scanningEntryId;

        //Find supplier
        if (this.invoiceInterpretation.supplierId.hasValue)
            this._selectedSupplier = this.suppliers.find(s => s.id === this.invoiceInterpretation.supplierId.value);

        //References
        this.invoiceNr = this.invoice.invoiceNr = this.invoiceInterpretation.invoiceNumber.value; 
        this.invoice.ocr = this.invoiceInterpretation.paymentReferenceNumber.hasValue ?
            this.invoiceInterpretation.paymentReferenceNumber.value : "";
        this.invoice.referenceOur = this.invoiceInterpretation.buyerReference.value || "";
        this.invoice.referenceYour = this.invoiceInterpretation.sellerContactName.value || "";

        //Dates
        if (this.invoiceInterpretation.invoiceDate.hasValue)
            this.selectedInvoiceDate = this.selectedVoucherDate = this.invoiceInterpretation.invoiceDate.value;
        if (this.invoiceInterpretation.dueDate.hasValue)
            this.invoice.dueDate = this.invoiceInterpretation.dueDate.value;
        if (this.invoiceInterpretation.isCreditInvoice.hasValue)
            this.invoice.billingType = this.invoiceInterpretation.isCreditInvoice.value === true ? 
                TermGroup_BillingType.Credit : TermGroup_BillingType.Debit;

        //Sum
        this.invoice.totalAmount = this.invoiceInterpretation.amountIncVat.hasValue ?
            this.invoiceInterpretation.amountIncVat.value : 0;
        this.invoice.totalAmountCurrency = this.invoiceInterpretation.amountIncVatCurrency.hasValue ?
            this.invoiceInterpretation.amountIncVatCurrency.value : 0;

        //Vat
        this.invoice.vatAmount = this.invoiceInterpretation.vatAmount.hasValue ?
            this.invoiceInterpretation.vatAmount.value : 0;
        this.invoice.vatAmountCurrency = this.invoiceInterpretation.vatAmountCurrency.hasValue ?
            this.invoiceInterpretation.vatAmountCurrency.value : 0;

        if (this.invoice.vatAmount === 0)
            this.recalculateVat = true;

        if (this.invoiceInterpretation.context.scanningEntryId) {
            //If the interpreter is confident about vat rate, we take it from there
            this.hasScanningEntryInvoice = true;
            const vatRate = this.invoiceInterpretation.vatRatePercent;
            if (vatRate.hasValue && (vatRate.value > 0 || vatRate.confidenceLevel == TermGroup_ScanningInterpretation.ValueIsValid))
                this.vatRate = vatRate.value; 
        }

        //Currency
        let setSupplierCurrency = true;
        this.invoice.currencyRate = this.invoiceInterpretation.currencyRate.value;

        const currencyId = this.invoiceInterpretation.currencyId;
        if (currencyId.hasValue) {
            const currency = this.currencies.find(c => c.currencyId === currencyId.value);
            if (currency) {
                this.currencyCode = currency.code;
                this.currencyDate = this.invoice.currencyDate = new Date(this.invoiceInterpretation.currencyDate.value);
                this.currencyRate = this.invoice.currencyRate = this.invoiceInterpretation.currencyRate.value;
                this.invoice.currencyId = currency.currencyId;

                setSupplierCurrency = false;
            }
        }

        // SupplierChanged will trigger the generation of accounting rows
        // therefore we load supplier after all amount fields has been set
        const supplierId = this.invoiceInterpretation.supplierId.value;
        if (supplierId) {
            this.loadSupplier(supplierId, setSupplierCurrency, this.recalculateVat, true).then(() => {
                if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || this.invoice.vatType === TermGroup_InvoiceVatType.NoVat) {
                    this.generateAccountingRows(true);
                }
            })
        }
        else
            this.generateAccountingRows(this.recalculateVat);

        this.invoice.supplierInvoiceProjectRows = [];
        this.invoice.supplierInvoiceOrderRows = [];
        this.invoice.supplierInvoiceCostAllocationRows = [];

        let columnValueNumber = this.invoiceInterpretation.buyerOrderNumber.value ?? "";
        if (columnValueNumber) {
            this.coreService.getCustomerInvoicesBySearch(columnValueNumber, "", "", "", "", "", "", SoeOriginType.Order, false).then(invoices => {
                if (invoices[0]) this.setOrderSelected({ invoice: invoices[0], copy: false }, false);
            });
        }


        this.setScanningEntryInterpretations(this.invoiceInterpretation).then(() => {
            this.setOrderDetailsFromOrderNr(this.scanningOrderNr);
        })
    }
    private supplierPurchaseExpanderOpen() {
        if (!this.purchaseRowsRendered) {
            this.purchaseRowsRendered = true;
        }
    }

    private setScanningEntryInterpretations(interpretation: InvoiceInterpretationDTO) {
        const keys: string[] = [
            "economy.supplier.invoice.scanningvalues",
            "economy.supplier.invoice.scanningvaild",
            "economy.supplier.invoice.scanninginsecure",
            "economy.supplier.invoice.scanninginvaild"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.scanningIsCreditInvoieIcon = this.getInterpretationStateIcon(interpretation.isCreditInvoice);
            this.scanningIsCreditInvoiceTooltip = this.getInterpretationStateTooltip(interpretation.isCreditInvoice, terms);
            this.scanningInvoiceNrIcon = this.getInterpretationStateIcon(interpretation.invoiceNumber);
            this.scanningInvoiceNrTooltip = this.getInterpretationStateTooltip(interpretation.invoiceNumber, terms);
            this.scanningInvoiceDateIcon = this.getInterpretationStateIcon(interpretation.invoiceDate);
            this.scanningInvoiceDateTooltip = this.getInterpretationStateTooltip(interpretation.invoiceDate, terms);
            this.scanningDueDateIcon = this.getInterpretationStateIcon(interpretation.dueDate);
            this.scanningDueDateTooltip = this.getInterpretationStateTooltip(interpretation.dueDate, terms);
            this.scanningReferenceYourIcon = this.getInterpretationStateIcon(interpretation.sellerContactName);
            this.scanningReferenceYourTooltip = this.getInterpretationStateTooltip(interpretation.sellerContactName, terms);
            this.scanningReferenceOurIcon = this.getInterpretationStateIcon(interpretation.buyerContactName);
            this.scanningReferenceOurTooltip = this.getInterpretationStateTooltip(interpretation.buyerContactName, terms);
            this.scanningVatAmountIcon = this.getInterpretationStateIcon(interpretation.vatAmountCurrency);
            this.scanningVatAmountTooltip = this.getInterpretationStateTooltip(interpretation.vatAmountCurrency, terms);
            this.scanningTotalAmountIncludeVatIcon = this.getInterpretationStateIcon(interpretation.amountIncVatCurrency);
            this.scanningTotalAmountIncludeVatTooltip = this.getInterpretationStateTooltip(interpretation.amountIncVatCurrency, terms);
            this.scanningCurrencyCodeIcon = this.getInterpretationStateIcon(interpretation.currencyCode);
            this.scanningCurrencyCodeTooltip = this.getInterpretationStateTooltip(interpretation.currencyCode, terms);
            this.scanningOCRIcon = this.getInterpretationStateIcon(interpretation.paymentReferenceNumber);
            this.scanningOCRTooltip = this.getInterpretationStateTooltip(interpretation.paymentReferenceNumber, terms);
            this.scanningOrderNrIcon = this.getInterpretationStateIcon(interpretation.buyerOrderNumber);
            this.scanningOrderNrTooltip = this.getInterpretationStateTooltip(interpretation.buyerOrderNumber, terms);
        });
    }
    private getInterpretationStateIcon<T>(value: InterpretationValueDTO<T>) {
        switch (value.confidenceLevel) {
            case TermGroup_ScanningInterpretation.ValueIsValid:
                return "has-success scanning-success";
            case TermGroup_ScanningInterpretation.ValueIsUnsettled:
                return  "has-warning scanning-warning";
            case TermGroup_ScanningInterpretation.ValueNotFound:
                return "has-error scanning-error";
            default:
                return "";
        }
    }

    private getInterpretationStateTooltip<T>(value: InterpretationValueDTO<T>, terms: { [index: string]: string; }) {
        switch (value.confidenceLevel) {
            case TermGroup_ScanningInterpretation.ValueIsValid:
                return terms["economy.supplier.invoice.scanningvalues"] + " " + terms["economy.supplier.invoice.scanningvaild"];
            case TermGroup_ScanningInterpretation.ValueIsUnsettled:
                return terms["economy.supplier.invoice.scanningvalues"] + " " + terms["economy.supplier.invoice.scanninginsecure"];
            case TermGroup_ScanningInterpretation.ValueNotFound:
                return terms["economy.supplier.invoice.scanningvalues"] + " " + terms["economy.supplier.invoice.scanninginvaild"];
            default:
                return "";
        }
    }

    private updateTabCaption() {
        this.translationService.translateMany(["economy.supplier.invoice.new", "common.supplierinvoice"]).then((terms) => {
            var invoiceNr = this.invoice && this.invoice.invoiceNr ? this.invoice.invoiceNr : "";
            var label = this.isNew ? terms["economy.supplier.invoice.new"] : terms["common.supplierinvoice"] + " " + invoiceNr;
            this.messagingHandler.publishSetTabLabel(this.guid, label);
        });
    }

    private invoiceLoaded() {
        this.loadingInvoice = false;
        this.invoiceIsLoaded = true;

        if (this.expanderSetting) {
            const settings = this.expanderSetting.split(";");
            this.attestUserExpanderOpen = _.includes(settings, 'AttestUserExpander');
            this.projectRowsExpanderOpen = _.includes(settings, 'ProjectRowsExpander');
            this.projectOrderExpanderOpen = _.includes(settings, 'ProjectOrderExpander');
            this.accountingRowsExpanderOpen = _.includes(settings, 'AccountingRowsExpander');
            this.tracingExpanderOpen = _.includes(settings, 'TracingExpander');
            this.imageGalleryExpanderOpen = _.includes(settings, 'ImageGalleryExpander');
            this.filesExpanderOpen = _.includes(settings, 'FilesExpander');
            this.purchaseExpanderOpen = _.includes(settings, 'PurchaseExpander');
            this.productRowsExpanderOpen = _.includes(settings, 'ProductRowsExpander');
            this.costAllocationExpanderOpen = _.includes(settings, 'CostAllocationExpander');
        }

        this.updateHeight()
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.BillingCopyInvoiceNrToOcr);
        settingTypes.push(CompanySettingType.SupplierInvoiceTransferToVoucher);
        settingTypes.push(CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.SupplierInvoiceDefaultVatType);
        settingTypes.push(CompanySettingType.SupplierPaymentDefaultPaymentCondition);
        settingTypes.push(CompanySettingType.SupplierInvoiceVoucherSeriesType);
        settingTypes.push(CompanySettingType.SupplierInvoiceDefaultDraft);
        settingTypes.push(CompanySettingType.SupplierInvoiceAllowEditOrigin);
        settingTypes.push(CompanySettingType.SupplierInvoiceAllowInterim);
        settingTypes.push(CompanySettingType.SupplierShowTransactionCurrency);
        settingTypes.push(CompanySettingType.SupplierShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.SupplierShowLedgerCurrency);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.ProjectChargeCostsToProject);
        settingTypes.push(CompanySettingType.ProductMisc);
        settingTypes.push(CompanySettingType.FISupplierInvoiceOCRCheckReference);
        settingTypes.push(CompanySettingType.SaveSupplierInvoiceAttestType);
        settingTypes.push(CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup);
        settingTypes.push(CompanySettingType.SupplierUseTimeDiscount);
        settingTypes.push(CompanySettingType.SupplierInvoiceAllowEditAccountingRows);
        settingTypes.push(CompanySettingType.ProjectDefaultTimeCodeId);
        settingTypes.push(CompanySettingType.SupplierInvoiceKeepSupplier);
        settingTypes.push(CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts);
        settingTypes.push(CompanySettingType.SupplierInvoiceRoundVAT);
        settingTypes.push(CompanySettingType.SupplierInvoiceRoundVAT);
        settingTypes.push(CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder);
        settingTypes.push(CompanySettingType.ScanningReferenceTargetField);
        settingTypes.push(CompanySettingType.ScanningCodeTargetField);
        settingTypes.push(CompanySettingType.IntrastatImportOriginType);
        settingTypes.push(CompanySettingType.AccountingDefaultVatCode);
        settingTypes.push(CompanySettingType.SupplierInvoiceProductRowsImport);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCopyInvoiceNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCopyInvoiceNrToOcr);
            this.copyInvoiceNr = this.defaultCopyInvoiceNr;
            this.supplierInvoiceTransferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceTransferToVoucher);
            this.supplierInvoiceAskPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer);
            this.defaultVatType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceDefaultVatType, this.defaultVatType);
            this.defaultPaymentConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierPaymentDefaultPaymentCondition);
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceVoucherSeriesType);
            this.defaultDraft = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceDefaultDraft);
            this.allowEditOrigin = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAllowEditOrigin);
            this.allowInterim = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAllowInterim);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowLedgerCurrency);
            this.voucherListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVoucherList);
            this.chargeCostsToProject = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectChargeCostsToProject);
            this.miscProduct = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductMisc);
            this.checkFIOCR = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.FISupplierInvoiceOCRCheckReference);
            this.defaultAttestType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SaveSupplierInvoiceAttestType, this.defaultAttestType);
            this.defaultAttestGroup = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup, this.defaultAttestGroup);
            this.useTimeDiscount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierUseTimeDiscount);
            this.allowEditAccountingRows = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAllowEditAccountingRows);
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProjectDefaultTimeCodeId);
            this.keepSupplier = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceKeepSupplier);
            this.roundVAT = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceRoundVAT);
            this.useInternalAccountWithBalanceSheetAccounts = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts);
            this.getDefaultInternalAccountsFromOrder = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceGetInternalAccountsFromOrder);
            this.scanningReferenceTargetField = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ScanningReferenceTargetField);
            this.scanningCodeTargetField = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ScanningCodeTargetField);
            this.intrastatImportOriginType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.IntrastatImportOriginType);
            this.defaultVatCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVatCode);
            this.supplierInvoiceProductRowsImport = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceProductRowsImport);
        });
    }

    private loadUserSettings(handleExpanders = false): ng.IPromise<any> {
        var settingTypeIds: number[] = [];
        settingTypeIds.push(UserSettingType.SupplierInvoiceSimplifiedRegistration);
        settingTypeIds.push(UserSettingType.BillingSupplierInvoiceDefaultExpanders);
        settingTypeIds.push(UserSettingType.BillingSupplierInvoiceSlider);
        settingTypeIds.push(UserSettingType.BillingSupplierInvoiceScale);
        return this.coreService.getUserSettings(settingTypeIds, false).then(x => {
            this.simplifiedRegistration = x[UserSettingType.SupplierInvoiceSimplifiedRegistration];

            this.expanderSetting = x[UserSettingType.BillingSupplierInvoiceDefaultExpanders];

            if (x[UserSettingType.BillingSupplierInvoiceSlider])
                this.widthRatio = x[UserSettingType.BillingSupplierInvoiceSlider]

            if (x[UserSettingType.BillingSupplierInvoiceScale])
                this.invoiceScale = x[UserSettingType.BillingSupplierInvoiceScale] / 100;

            if (this.expanderSetting && handleExpanders) {
                const settings = this.expanderSetting.split(";");
                this.attestUserExpanderOpen = _.includes(settings, 'AttestUserExpander');
                this.projectRowsExpanderOpen = _.includes(settings, 'ProjectRowsExpander');
                this.projectOrderExpanderOpen = _.includes(settings, 'ProjectOrderExpander');
                this.accountingRowsExpanderOpen = _.includes(settings, 'AccountingRowsExpander');
                this.tracingExpanderOpen = _.includes(settings, 'TracingExpander');
                this.imageGalleryExpanderOpen = _.includes(settings, 'ImageGalleryExpander');
                this.filesExpanderOpen = _.includes(settings, 'FilesExpander');
                this.purchaseExpanderOpen = _.includes(settings, 'PurchaseExpander');
                this.productRowsExpanderOpen = _.includes(settings, 'ProductRowsExpander');
                this.costAllocationExpanderOpen = _.includes(settings, 'CostAllocationExpander');
            }
            this.setupTabIndexes();
        });
    }

    private saveSimplifiedRegistration() {
        this.$timeout(() => {
            this.setupTabIndexes();
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.SupplierInvoiceSimplifiedRegistration, this.simplifiedRegistration);
        })
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.AccountSupplierDebt,
            CompanySettingType.AccountSupplierPurchase,
            CompanySettingType.AccountCommonVatReceivable,
            CompanySettingType.AccountSupplierInterim,
            CompanySettingType.AccountCommonReverseVatPurchase,
            CompanySettingType.AccountCommonVatPayable1Reversed,
            CompanySettingType.AccountCommonVatReceivableReversed,

            CompanySettingType.AccountCommonVatPayable1EUImport,
            CompanySettingType.AccountCommonVatPayable2EUImport,
            CompanySettingType.AccountCommonVatPayable3EUImport,
            CompanySettingType.AccountCommonVatReceivableEUImport,
            CompanySettingType.AccountCommonVatPurchaseEUImport,

            CompanySettingType.AccountCommonVatPayable1NonEUImport,
            CompanySettingType.AccountCommonVatPayable2NonEUImport,
            CompanySettingType.AccountCommonVatPayable3NonEUImport,
            CompanySettingType.AccountCommonVatReceivableNonEUImport,
            CompanySettingType.AccountCommonVatPurchaseNonEUImport
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierDebt);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierPurchase);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivable);
            this.defaultInterimAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierInterim);
            this.reverseVatAccountPurchaseId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatPurchase);
            this.contractorVatAccountCreditId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1Reversed);
            this.contractorVatAccountDebitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivableReversed);

            this.euVatCredit1AccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1EUImport);
            this.euVatCredit2AccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable2EUImport);
            this.euVatCredit3AccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable3EUImport);
            this.euVatDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivableEUImport);
            this.euVatPurchaseAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPurchaseEUImport);

            this.nonEuVatCredit1AccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1NonEUImport);
            this.nonEuVatCredit2AccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable2NonEUImport);
            this.nonEuVatCredit3AccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable3NonEUImport);
            this.nonEuVatDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivableNonEUImport);
            this.nonEuVatPurchaseAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPurchaseNonEUImport);

            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId);
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.supplierService.getTimeCodes(SoeTimeCodeType.WorkAndMaterial, true, false).then((x) => {
            this.timecodes.push({ value: 0, label: " " });
            _.forEach(x, (timeCode: any) => {
                this.timecodes.push({ value: timeCode.timeCodeId, label: timeCode.name, timeCodeId: timeCode.timeCodeId });
            });
        });
    }

    private loadSuppliers(useCache: boolean): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, useCache).then((x: ISmallGenericType[]) => {
            this.suppliers = x;
        });
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then(x => {
            this.billingTypes = x;
        });
    }

    private loadVatTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, false, false).then(x => {
            this.vatTypes = _.filter(x, (y) => y.id < 7);
            if (this.defaultVatType === TermGroup_InvoiceVatType.None && this.vatTypes.length > 0) {
                if (_.includes(this.vatTypes, TermGroup_InvoiceVatType.Merchandise))
                    this.defaultVatType = TermGroup_InvoiceVatType.Merchandise;
                else
                    this.defaultVatType = this.vatTypes[0].id;
            }
        });
    }

    private loadAttestWorkFlowHeadFromInvoiceId(invoiceId: number): ng.IPromise<any> {
        if (invoiceId === 0)
            return;

        this.attestAdminInfo = undefined;
        this.existingAttestFlow = false;

        return this.supplierService.getAttestWorkFlowHeadFromInvoiceId(invoiceId, false, false, true, true).then(x => {
            this.attestWorkFlowHead = x;
            if (this.attestWorkFlowHead) {

                if (this.attestWorkFlowHead.adminInformation)
                    this.attestAdminInfo = this.attestWorkFlowHead.adminInformation;
                if (this.attestWorkFlowHead.attestWorkFlowHeadId)
                    this.existingAttestFlow = true;

                this.defaultAttestRowAmount = this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency;

                var isAttestRejected: boolean = false;
                if (this.attestWorkFlowHead.rows) {
                    isAttestRejected = this.attestWorkFlowHead.rows.filter(r => r.answer == false).length > 0;
                }
                if (isAttestRejected)
                    this.translationService.translate("economy.supplier.invoice.attestrejected").then(label => this.invoice.attestStateName = label);

            }
        });
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private loadPaymentConditions(): ng.IPromise<any> {
        return this.supplierService.getPaymentConditions().then(x => {
            this.paymentConditions = x;

            // Get default number of days (or use 30 if not specified)
            const def = _.find(this.paymentConditions, { paymentConditionId: this.defaultPaymentConditionId });
            this.defaultPaymentConditionDays = def ? def.days : 30;
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesByYear(accountYearId, false, true).then((x) => {
            this.voucherSeries = x;
            const voucherSeriesIds = _.map(this.voucherSeries, 'voucherSeriesId');
            _.includes(voucherSeriesIds, this.selectedVoucherSeriesId)
            if (this.defaultVoucherSeriesTypeId && (!this.selectedVoucherSeriesId || !_.includes(voucherSeriesIds, this.selectedVoucherSeriesId))) {
                _.forEach(this.voucherSeries, (voucherSerie) => {
                    this.defaultVoucherSeriesTypeId && !this.selectedVoucherSeriesId
                    if (voucherSerie.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId) {
                        this.selectedVoucherSeriesId = voucherSerie.voucherSeriesId;
                    }
                });
            }
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmallMemoryCache(false, true, false, false, false).then(x => {
            this.accountDims = x;
            // Remove this temporary. We have no customer using this functionality.
            // I leave it here, when we migrate the page, we should skip all vatDeduction functionality, to start with.
            this.useVatDeductionDim = false;
            this.accountDim = undefined;
        });
    }

    private loadVatDeductionDict(vatDeduction: number) {
        this.vatDeductionDict.push({ id: 100, name: "100,00" });
        this.vatDeductionDict.push({ id: 0, name: "0,00" });
        if (vatDeduction > 0) {
            this.vatDeductionDict.push({ id: vatDeduction, name: vatDeduction.toFixed(2).toString().replace(".", ",") });
        }
    }

    private loadAccountYear(date: Date) {
        if (!date)
            return;

        var prevAccountYearId = this.invoiceAccountYearId;

        this.accountingService.getAccountYearId(date).then((id: number) => {
            this.invoiceAccountYearId = id;
            if (this.invoiceAccountYearId !== this.currentAccountYearId || this.invoiceAccountYearId !== prevAccountYearId) {
                // If account year has changed, load voucher series for new year
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

        this.accountingService.getAccountPeriodId(accountYearId, this.invoice.voucherDate).then((id: number) => {
            this.accountPeriodId = id;
        });
    }

    private loadCustomerInvoices(): ng.IPromise<any> {
        //return this.supplierService.getCustomerInvoices(SoeOriginType.Order, SoeOriginStatusClassification.OrdersOpen, TermGroup.ProjectType, OrderInvoiceRegistrationType.Order, true, true, true).then(x => {
        return this.supplierService.getOrdersForSupplierInvoiceEdit(false).then(x => {
            this.customerInvoicesDict.push({ id: 0, name: '' });
            _.forEach(x, (item) => {
                item.orderNumber = Number(item.invoiceNr);
                this.customerInvoices.push(item);
                this.customerInvoicesDict.push({ id: item.invoiceId, name: item.customerInvoiceNumberName })
            });
        });
    }

    private loadProjects(useCache: boolean = true): ng.IPromise<any> {
        this.projectsDict = [];
        return this.supplierService.getProjectList(TermGroup_ProjectType.TimeProject, undefined, true, true, useCache).then(x => {
            this.projects = x;
            // Insert empty row
            this.projectsDict.splice(0, 0, { projectId: null, name: '', number: '' });
            //create special display property...
            for (var i = 0; i < this.projects.length; i++) {
                var project = this.projects[i];
                project.numberAndName = project.number + " " + project.name;
                this.projectsDict.push({ id: project.projectId, name: project.numberAndName })
            }
        });
    }

    private loadVatCodes(): ng.IPromise<any> {
        return this.accountingService.getVatCodes(true).then(x => {
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

        this.accountingService.getAccountSysVatRate(accountId).then(x => {
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

        if (vatCode)
            this.vatRate = vatCode.percent;
        else
            this.setDefaultVatRate();
    }

    private setDefaultVatRate() {
        if (this.defaultVatRate === 0)
            this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;

        this.vatRate = this.defaultVatRate;
    }

    private loadSupplier(supplierId: number, setSupplierCurrency: boolean = true, recalculateVat: boolean = true, ignoreSetDueDate: boolean = false): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();
        if (!supplierId) {
            
            this.supplier = null;
            this.supplierChanged(setSupplierCurrency, recalculateVat, ignoreSetDueDate);
            deferral.resolve();
        } else {
            this.supplierService.getSupplier(supplierId, false, true, false, false).then(x => {
                this.supplier = x;
                this.supplierChanged(setSupplierCurrency, recalculateVat, ignoreSetDueDate);
                this.loadingInvoice = false;
                deferral.resolve();
            });
        }
        return deferral.promise;
    }

    private loadPaymentInformation() {
        if (this.supplier && this.supplier.actorSupplierId) {
            this.accountingService.getPaymentInformationViews(this.supplier.actorSupplierId, true).then(x => {
                this.paymentInfos = x;

                let paymentInfo = null;
                if (this.invoice && this.invoice.sysPaymentTypeId && this.invoice.sysPaymentTypeId !== 0) {
                    paymentInfo = this.paymentInfos.find(x => x.sysPaymentTypeId === this.invoice.sysPaymentTypeId && x.paymentNr === this.invoice.paymentNr);
                }

                if (!paymentInfo) {
                    // Set suppliers default payment information row
                    if (this.paymentInfos && this.paymentInfos.length > 0) {
                        // First try to get default account within default payment type.
                        _.forEach(this.paymentInfos, (payInfo) => {
                            if (payInfo.sysPaymentTypeId === payInfo.defaultSysPaymentTypeId && payInfo.default === true)
                                paymentInfo = payInfo;
                        });

                        // If no account of default payment type found, get first default account, regardless of payment type
                        if (!paymentInfo)
                            paymentInfo = _.find(this.paymentInfos, { default: true });

                        // If no default account found, get first account of default payment type
                        if (!paymentInfo) {
                            _.forEach(this.paymentInfos, (payInfo) => {
                                if (!paymentInfo && payInfo.sysPaymentTypeId === payInfo.defaultSysPaymentTypeId)
                                    paymentInfo = payInfo;
                            });
                        }

                        // No default account found, get first account in list
                        if (!paymentInfo)
                            paymentInfo = this.paymentInfos[0];
                    }
                }

                this.selectedPaymentInfo = paymentInfo;
            });
        } else {
            this.paymentInfos = [];
            this.selectedPaymentInfo = null;
        }
    }

    private loadFiles() {
        if (!this.invoice) {
            return;
        }

        this.loadingFiles = true;
        this.filesHelper.loadFiles()
            .finally(() => this.loadingFiles = false);
    }

    // EVENTS
    private vatDeductionChanging(item) {
        if (item !== undefined && item !== null)
            this.calculateVatDeduction(item);
    }


    private billingTypeChanging(oldValue) {
        // Only show warning if amount is entered and user has manually modified any row
        if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows()) {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.invoice.billingtypechangewarning"
            ];

            // Check if connected inventories exists
            var key = this.getInventoryRowsExistWarningMessage();
            if (key)
                keys.push(key);

            this.translationService.translateMany(keys).then((terms) => {
                var message = terms["economy.supplier.invoice.billingtypechangewarning"];
                if (key)
                    message += "\n\n" + terms[key];

                const modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
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
                "economy.supplier.invoice.vattypechangewarning"
            ];

            // Check if connected inventories exists
            var key = this.getInventoryRowsExistWarningMessage();
            if (key)
                keys.push(key);

            this.translationService.translateMany(keys).then((terms) => {
                var message = terms["economy.supplier.invoice.vattypechangewarning"];
                if (key)
                    message += "\n\n" + terms[key];

                var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
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
            if (this.invoice.vatType === TermGroup_InvoiceVatType.NoVat)
                this.invoice.vatCodeId = null;

            this.generateAccountingRows(true);

            // Set default accountdim values to accounting rows
            this.updateAllAccountRowDimAccounts();
        });
    }

    private vatCodeChanging(oldValue) {
        // Only show warning if amount is entered and user has manually modified any row
        if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows()) {
            const keys: string[] = [
                "core.warning",
                "economy.supplier.invoice.vatcodechangewarning"
            ];

            // Check if connected inventories exists
            var key = this.getInventoryRowsExistWarningMessage();
            if (key)
                keys.push(key);

            this.translationService.translateMany(keys).then((terms) => {
                var message = terms["economy.supplier.invoice.vatcodechangewarning"];
                if (key)
                    message += "\n\n" + terms[key];

                var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.changeVatCode();
                }, (reason) => {
                    // User cancelled, revoke to previous vat code
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

    private supplierChanged(setSupplierCurrency = true, recalculateVat = true, ignoreSetDueDate = false) {
        // Set supplier dependant values
        if (!this.loadingInvoice && !this.settingInvoiceSupplier) {
            this.invoice.vatType = this.supplier && this.supplier.vatType !== TermGroup_InvoiceVatType.None ? this.supplier.vatType : this.defaultVatType;
            this.invoice.actorId = this.supplier ? this.supplier.actorSupplierId : 0;
        }

        // Vat code
        if (!this.loadingInvoice && !this.settingInvoiceSupplier)
            this.invoice.vatCodeId = null;
        this.supplierVatAccountId = 0;
        if (this.vatCodes) {
            let vatCode = undefined;
            if (this.supplier && this.supplier.vatCodeId)
                vatCode = _.find(this.vatCodes, { vatCodeId: this.supplier.vatCodeId });
            else if (this.defaultVatCodeId)
                vatCode = _.find(this.vatCodes, { vatCodeId: this.defaultVatCodeId });

            if (vatCode) {
                if (!this.loadingInvoice && !this.settingInvoiceSupplier)
                    this.invoice.vatCodeId = vatCode.vatCodeId;

                if (vatCode.accountId !== 0) {
                    this.supplierVatAccountId = vatCode.purchaseVATAccountId ? vatCode.purchaseVATAccountId : null;
                }
            }
        }

        // Payment
        this.setPaymentCondition(this.supplier && this.supplier.paymentConditionId ? this.supplier.paymentConditionId : 0);
        this.loadPaymentInformation();

        if (!this.loadingInvoice) {

            if (!this.settingInvoiceSupplier)
                this.setVatRate();

            if (!this.settingInvoiceSupplier) {
                //keep scanned duedate in first hand
                if (this.scanningEntryId > 0 && this.invoice.dueDate)
                    ignoreSetDueDate = true;

                if (!ignoreSetDueDate)
                    this.setDueDate();
                this.invoice.blockPayment = this.supplier && this.supplier.blockPayment;
            }

            if (!this.settingInvoiceSupplier)
                this.setTimeDiscount();

            // Copy invoice number to OCR supplier setting
            if (!this.settingInvoiceSupplier)
                this.copyInvoiceNr = this.supplier ? this.supplier.copyInvoiceNrToOcr : this.defaultCopyInvoiceNr;

            // Set currency from supplier
            if (setSupplierCurrency && !this.settingInvoiceSupplier && this.supplier && this.supplier.currencyId) {
                this.invoice.currencyId = this.supplier.currencyId;
            }

            // References (do not overwrite values from scanning)
            if (!this.invoiceInterpretation && !this.settingInvoiceSupplier) {
                this.invoice.referenceYour = this.supplier && this.supplier.invoiceReference ? this.supplier.invoiceReference : '';
                this.invoice.referenceOur = this.supplier && this.supplier.ourReference ? this.supplier.ourReference : '';
            }
        }

        // Note
        if (this.supplier && this.supplier.showNote && this.supplier.note) {
            if (!this.supressNote)
                this.showSupplierNote(this.supplier.note);
            else if (this.supplierId != this.supplier.actorSupplierId) {
                this.showSupplierNote(this.supplier.note);
                this.supressNote = false;
            }
        }

        this.defaultAttestRowDebitAccountId = this.defaultDebitAccountId;
        if (this.supplier && this.supplier.accountingSettings) {
            const setting = _.find(this.supplier.accountingSettings, { type: SupplierAccountType.Debit });
            if (setting && setting.account1Id)
                this.defaultAttestRowDebitAccountId = setting.account1Id;
        }

        // Confirm accounting
        this.showConfirmAccounting = this.supplier && this.supplier.manualAccounting;
        this.confirmAccounting = false;
        if (!this.loadingInvoice && !this.settingInvoiceSupplier)
            this.invoice.interimInvoice = this.allowInterim && this.supplier && this.supplier.interim;

        // Accounting rows
        if (!this.loadingInvoice && !this.settingInvoiceSupplier)
            this.generateAccountingRows(recalculateVat);

        // Set default accountdim values to accounting rows
        if (!this.loadingInvoice && !this.settingInvoiceSupplier)
            this.updateAllAccountRowDimAccounts();

        // Check if the invoice number already exist    
        if (!this.loadingInvoice && !this.settingInvoiceSupplier && this.invoice.invoiceNr != null) {
            this.checkIfSupplierInvoiceNumberExist(this.invoice.invoiceNr);
        }

        //Set supplierIdt
        if (!this.loadingInvoice && !this.settingInvoiceSupplier && this.supplier)
            this.supplierId = this.supplier ? this.supplier.actorSupplierId : 0;

        this.settingInvoiceSupplier = false;
        this.invoiceLoaded();

        // Reset info messages
        /*this.showInfoMessage = false;
        this.infoMessage = "";
        this.infoButtons = [];*/
    }

    private invoiceNrChanged() {
        this.$timeout(() => {
            this.checkIfSupplierInvoiceNumberExist(this.invoice.invoiceNr);
            
            // Copy invoice number to OCR
            if (this.invoice && this.copyInvoiceNr) {
                this.$timeout(() => {
                    this.invoice.ocr = this.invoice.invoiceNr;
                });
            }
        })
    }

    private amountChanged(id: string, generateAccountingRows: boolean = true) {
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
                this.invoice.totalAmount = this.isBaseCurrency ? this.invoice.totalAmountCurrency : this.invoice.totalAmountCurrency * this.currencyRate;

                this.calculateVatAmount();
            }
            if (id === 'vat') {
                if (!this.invoice.vatAmountCurrency) {
                    this.invoice.vatAmountCurrency = 0;
                }
            }

            if (generateAccountingRows)
                this.generateAccountingRows(id === 'total');

            if (this.edit["ctrl_invoice_vatAmountCurrency"])
                this.edit["ctrl_invoice_vatAmountCurrency"].$validate();           
        });
    }

    private vatAmountFieldFocus() {
        this.$timeout(() => {
            var elements = this.$window.document.getElementsByName('ctrl_invoice_vatAmountCurrency');
            if (elements && elements.length) {
                elements[elements.length - 1].select();
            }
        }, 100);
    }

    private convertAmount(field: string, amount: number) {
        if (this.loadingInvoice)
            return;

        // Call amount currency converter in accounting rows directive
        var item = {
            field: field,
            amount: amount,
            sourceCurrencyType: TermGroup_CurrencyType.TransactionCurrency
        };
        this.$scope.$broadcast('amountChanged', item);
    }

    private amountConverted(item) {
        // Result from amount currency converter in accounting rows directive
        if (item.parentRecordId === this.invoice.invoiceId) {
            this.invoice[item.field] = item.baseCurrencyAmount;
            this.invoice[item.field + 'EnterpriceCurrency'] = item.enterpriseCurrencyAmount;
            this.invoice[item.field + 'LedgerCurrency'] = item.ledgerCurrencyAmount;
        }
    }

    private interimChanged(item) {
        this.$timeout(() => {
            this.generateAccountingRows(true);
        });
    }

    private orderNrChanging(id: number) {
        if (this.selectedCustomerInvoice) {
            this.invoice.orderNr = id;
        }
        else {
            this.invoice.orderNr = null;
        }

        if (this.invoice.projectId && this.invoice.supplierInvoiceCostAllocationRows.length === 0 && this.costAllocationSetupDone) {
            this.addProjectRow();
        }

        if (this.getDefaultInternalAccountsFromOrder && this.selectedCustomerInvoice) {
            var order = this.supplierService.getOrder(this.selectedCustomerInvoice.id, false).then((x) => {
                this.defaultAccountDim2Id = x.defaultDim2AccountId ? x.defaultDim2AccountId : 0;
                this.defaultAccountDim3Id = x.defaultDim3AccountId ? x.defaultDim3AccountId : 0;
                this.defaultAccountDim4Id = x.defaultDim4AccountId ? x.defaultDim4AccountId : 0;
                this.defaultAccountDim5Id = x.defaultDim5AccountId ? x.defaultDim5AccountId : 0;
                this.defaultAccountDim6Id = x.defaultDim6AccountId ? x.defaultDim6AccountId : 0;
            });
        }
    }

    private projectChanging(id: number) {
        this.invoice.projectId = id ? id : undefined;

        if (this.invoice.projectId) {
            this.projectService.getProject(this.invoice.projectId).then((x) => {
                this.defaultAccountDim2Id = x.defaultDim2AccountId ? x.defaultDim2AccountId : 0;
                this.defaultAccountDim3Id = x.defaultDim3AccountId ? x.defaultDim3AccountId : 0;
                this.defaultAccountDim4Id = x.defaultDim4AccountId ? x.defaultDim4AccountId : 0;
                this.defaultAccountDim5Id = x.defaultDim5AccountId ? x.defaultDim5AccountId : 0;
                this.defaultAccountDim6Id = x.defaultDim6AccountId ? x.defaultDim6AccountId : 0;
            });

            if (this.invoice.supplierInvoiceCostAllocationRows && this.invoice.supplierInvoiceCostAllocationRows.length == 0 && this.costAllocationSetupDone) {
                this.$timeout(() => {
                    this.addProjectRow();
                });
            }
        }
    }

    private addProjectRow() {
        var timeCode = this.defaultTimeCodeId ? this.timecodes.find(x => x.value === this.defaultTimeCodeId) : undefined;
        this.$scope.$broadcast('addProjectRow', { guid: this.guid, project: this.selectedProject, timeCode: timeCode, amount: +(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceProjectRows, function (o) { return +o.amount; }), setFocus: false });

        /*var row: any = {};
        row.state = SoeEntityState.Active;
        //TimeCodeTransaction
        row.timeCodeTransactionId = 0;
        //Amount
        row.amount = +(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceProjectRows, function (o) { return +o.amount; });
        row.amountLedgerCurrency = 0;
        row.amountEntCurrency = 0;
        //TimeInvoiceTransaction
        row.timeInvoiceTransactionId = 0;
        //SupplierInvoice
        row.supplierInvoiceId = 0;
        //Project
        if (this.selectedProject && this.selectedProject.id && this.selectedProject.id > 0) {
            row.projectId = this.selectedProject.id;
            row.projectNr = this.selectedProject.number;
            row.projectName = this.selectedProject.name;
        } else {
            row.projectId = 0;
            row.projectName = " ";
        }

        //Customer Invoice      
        if (this.selectedCustomerInvoice && this.selectedCustomerInvoice.id && this.selectedCustomerInvoice.id > 0) {
            row.customerInvoiceId = this.selectedCustomerInvoice.id;
            row.customerInvoiceNr = this.selectedCustomerInvoice.number;
            row.customerInvoiceNumberName = this.selectedCustomerInvoice.name;
        } else {
            row.customerInvoiceId = 0;
            row.customerInvoiceNumberName = " ";
        }

        //TimeCode            
        var timecodes = this.defaultTimeCodeId ? this.timecodes.filter(x => x.value === this.defaultTimeCodeId) : [];

        if (timecodes.length > 0) {
            row.timeCodeId = timecodes[0].value;
            row.timeCodeName = timecodes[0].label;
        }
        else {
            row.timeCodeId = undefined;
            row.timeCodeName = "";
        }

        row.timeCodeCode = "";
        row.timeCodeDescription = "";

        //Employee
        row.employeeId = 0;
        row.employeeName = "";
        row.employeeNr = "";
        row.employeeDescription = "";

        //TimeBlockDate
        row.timeBlockDateId = null;
        row.date = new Date().toJSON().slice(0, 10);
        row.chargeCostToProject = this.chargeCostsToProject;
        this.invoice.supplierInvoiceProjectRows.push(row);
        this.$scope.$broadcast('costsToProjectChanged', null);*/
    }

    private chargeCostsToProjectChanged() {
        this.$timeout(() => {
            _.forEach(this.invoice.supplierInvoiceCostAllocationRows, (projectRow) => {
                projectRow.chargeCostToProject = this.chargeCostsToProject;
            })

            this.$scope.$broadcast('costsToProjectChanged', { guid: this.guid });
            /*_.forEach(this.invoice.supplierInvoiceProjectRows, (projectRow) => {
                projectRow.chargeCostToProject = this.chargeCostsToProject;
            })

            this.$scope.$broadcast('costsToProjectChanged', null);*/
        });
    }

    // TOOLBAR

    private blockPayment() {
        this.translationService.translateMany(["common.statereason", "economy.supplier.invoice.blockforpayment"]).then((terms) => {
            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
                controller: TextBlockDialogController,
                controllerAs: "ctrl",
                size: 'lg',
                resolve: {
                    text: () => { return "" },
                    editPermission: () => { return true },
                    entity: () => { return SoeEntityType.SupplierInvoice },
                    type: () => { return TextBlockType.TextBlockEntity },
                    headline: () => { return terms["economy.supplier.invoice.blockforpayment"] },
                    mode: () => { return SimpleTextEditorDialogMode.AddSupplierInvoiceBlockReason },
                    container: () => { return undefined },
                    langId: () => { return TermGroup_Languages.Swedish },
                    maxTextLength: () => { return 995 },
                    textboxTitle: () => { return terms["common.statereason"] },
                }
            }
            this.$uibModal.open(options).result.then((result: any) => {
                if (result) {
                    this.supplierService.blockSupplierInvoicePayment(this.supplierInvoiceId, true, result.text).then(x => {
                        if (x.success) {
                            this.invoice.blockPayment = !this.invoice.blockPayment;
                            this.invoice.blockReason = result.text;
                            this.messagingService.publish(this.evaluateInvoiceUpsertEventState(false), { guid: this.guid, data: this.invoice });
                        }
                    });
                }
            });
        });
    }

    private unblockPayment() {
        this.translationService.translateMany(["common.reason", "economy.supplier.invoice.unblockforpayment"]).then((terms) => {
            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
                controller: TextBlockDialogController,
                controllerAs: "ctrl",
                size: 'lg',
                resolve: {
                    text: () => { return !this.invoice.blockReason ? " " : this.invoice.blockReason },
                    editPermission: () => { return false },
                    entity: () => { return SoeEntityType.SupplierInvoice },
                    type: () => { return TextBlockType.TextBlockEntity },
                    headline: () => { return terms["economy.supplier.invoice.unblockforpayment"] },
                    mode: () => { return SimpleTextEditorDialogMode.AddSupplierInvoiceBlockReason },
                    container: () => { return undefined },
                    langId: () => { return TermGroup_Languages.Swedish },
                    maxTextLength: () => { return 995 },
                    textboxTitle: () => { return terms["common.reason"] },
                }
            }
            this.$uibModal.open(options).result.then((result: any) => {
                if (result) {
                    this.supplierService.blockSupplierInvoicePayment(this.supplierInvoiceId, false, "none").then(x => {
                        if (x.success) {
                            this.invoice.blockPayment = !this.invoice.blockPayment;
                            this.invoice.blockReason = "";
                            this.invoice.blockReasonTextId = undefined;
                            this.invoice.modified = x.modified;
                            this.messagingService.publish(this.evaluateInvoiceUpsertEventState(false), { guid: this.guid, data: this.invoice });
                        }
                    });
                }
            });
        });
    }

    private showAccountDistributionHead() {

        this.translationService.translate("economy.accounting.accountdistribution.accountdistribution").then(label => {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(label + " " + this.accountDistributionName, this.accountDistributionHeadId, AccountDistributionEditController, { id: this.accountDistributionHeadId, accountDistributionType: SoeAccountDistributionType.Period }, this.urlHelperService.getGlobalUrl('Shared/Economy/Accounting/AccountDistribution/Views/edit.html')));
        });

    }

    private showChangeCompany(): any {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Dialogs/ChangeCompany/Views/changecompany.html"),
            controller: ChangeCompanyController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                changeCompanyService: () => { return this.supplierService }
            }
        });

        modal.result.then(obj => {
            if (obj) {

                this.supplierInvoiceChangeCompanyDTO = {
                    InvoiceId: this.invoice.invoiceId,
                    ScanningEntryId: 0,
                    CompanyId: parseInt(obj.selectedCompanyId),
                    SupplierId: parseInt(obj.selectedSupplierId),
                    VoucherSeriesId: parseInt(obj.selectedVoucherSeriesId),
                };

                this.supplierService.saveSupplierInvoiceChangeCompany(this.supplierInvoiceChangeCompanyDTO).then((x) => {

                    var textMessage: string;
                    var image: SOEMessageBoxImage;
                    var title: string;
                    const keys: string[] = [
                        "core.warning",
                        "core.info",
                        "economy.supplier.invoice.transferredtocompany"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        if (x.success) {
                            textMessage = terms["economy.supplier.invoice.transferredtocompany"];
                            image = SOEMessageBoxImage.Warning;
                            title = terms["core.info"];
                        }
                        else {
                            textMessage = x.errorMessage;
                            image = SOEMessageBoxImage.Warning;
                            title = terms["core.warning"];
                        }

                        var modal = this.notificationService.showDialog(title, textMessage, image, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                        modal.result.then(val => {
                        });
                    });
                });
            }
        }, function () {
        });

        return modal;
    }

    private askEditSeqNr() {
        // Show edit invoicesequencenumber dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("editSeqNrDialog.html"),
            controller: EditInvoiceSeqNrDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                newSeqNr: () => { return this.invoice.seqNr },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.newSeqNr && Number(result.newSeqNr)) {
                this.editSeqNr(Number(result.newSeqNr));
            }
        });
    }

    private editSeqNr(newSeqNr: number) {
        this.supplierService.changeInvoiceSequenceNumberSuperAdmin(this.supplierInvoiceId, newSeqNr).then((result) => {
            if (result.success) {
                this.invoice.seqNr = newSeqNr;
                alert("completedSave? " + newSeqNr);
                //this.translationService.translate("economy.supplier.invoice.seqnredited").then((term) => { //TODO fix the term
                //    this.completedSave(this.invoice, false, term.format(newSeqNr.toString()));
                //});
            }
        });
    }

    private updateAccordionSettings() {

        const keys: string[] = [
            "economy.supplier.invoice.image",
            "economy.supplier.invoice.attest",
            "economy.supplier.invoice.linktoproject",
            "economy.supplier.invoice.orders",
            "economy.supplier.invoice.accountingrows",
            "common.tracing",
            "core.document",
            "economy.supplier.invoice.purchase",
            "economy.supplier.invoice.productrows",
            "economy.supplier.invoice.allocatecosts"
        ];
        var accordionList: any[] = [];

        this.translationService.translateMany(keys).then((terms) => {
            accordionList.push({ name: "ImageGalleryExpander", description: terms["economy.supplier.invoice.image"] });
            accordionList.push({ name: "AccountingRowsExpander", description: terms["economy.supplier.invoice.accountingrows"] });
            if (this.attestFlowAdminPermission || this.attestFlowPermission)
                accordionList.push({ name: "AttestUserExpander", description: terms["economy.supplier.invoice.attest"] });
            if (this.uploadImagePermission) {
                accordionList.push({ name: "FilesExpander", description: terms["core.document"] });
            }
            if (this.projectPermission || this.ordersPermission) {
                accordionList.push({ name: "CostAllocationExpander", description: terms["economy.supplier.invoice.allocatecosts"] })
            }
            accordionList.push({ name: "TracingExpander", description: terms["common.tracing"] });
            if (this.purchasePermission) {
                accordionList.push({ name: "PurchaseExpander", description: terms["economy.supplier.invoice.purchase"] })
            }
            if (this.productRowsPermission) {
                accordionList.push({ name: "ProductRowsExpander", description: terms["economy.supplier.invoice.productrows"] })
            }

        });

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AccordionSettings/Views/accordionsettings.html"),
            controller: AccordionSettingsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                coreService: () => { return this.coreService },
                userSettingType: () => { return UserSettingType.BillingSupplierInvoiceDefaultExpanders },
                accordionList: () => { return accordionList },
                userSliderSettingType: () => { return UserSettingType.BillingSupplierInvoiceSlider }
            }
        });

        modal.result.then(ids => {
            this.loadUserSettings(true).then(() => this.setWidth())
        }, function () {
            //Cancelled
        });
    }

    // ACTIONS

    private executeSaveFunction(option) {
        switch (option.id) {
            case SupplierInvoiceEditSaveFunctions.Save:
                this.save(false);
                break;
            case SupplierInvoiceEditSaveFunctions.SaveAndClose:
                this.save(true);
                break;
            case SupplierInvoiceEditSaveFunctions.SaveAndOpenNext:
                this.save(false, true);
                break;
        }
    }

    private save(closeAfterSave: boolean, openNextInvoice = false, attestStatusSetting: SoeOriginStatus = SoeOriginStatus.None, showAddInvoiceToAttestFlowDialog = false, callReload = false, reloadInvoiceId: number = null, reloadOpenExpander = false) {
        if (this.isSaving === true) {
            return
        }

        if (this['edit'].$invalid) {
            console.warn("Save called with invalid form");
            return;
        }

        this.$scope.$broadcast('stopEditing', {
            functionComplete: () => {}
        });

        this.$timeout(() => {
            this.isSaving = true;
            this.beforeSaveChecks().then((ok: boolean) => {
                if (ok) {
                    this.startSave(closeAfterSave, openNextInvoice, attestStatusSetting, showAddInvoiceToAttestFlowDialog, callReload, reloadInvoiceId, reloadOpenExpander);
                }
                else {
                    this.isSaving = false;
                }
            });
        }, 100); // 100 is used just because other places that this is depending on has 100 and we don't want to risk intermittent errors.
    }

    private startSave(closeAfterSave: boolean, openNextInvoice = false, attestStatusSetting: SoeOriginStatus = SoeOriginStatus.None, showAddInvoiceToAttestFlowDialog = false, callReload = false, reloadInvoiceId: number = null, reloadOpenExpander = false, disregardConcurrencyCheck = false) {
        // Empty info
        this.showInfoMessage = false;
        this.infoMessage = "";
        this.infoButtons = [];

        if (!this.dirtyHandler.isDirty) {
            if (closeAfterSave) {
                this.closeMe(true)
            }
            this.isSaving = false;
            return
        }

        const keys: string[] = [
            "core.verifyquestion",
            "economy.supplier.payment.askPrintVoucher",
            "economy.supplier.invoice.successpreliminary",
            "economy.supplier.invoice.successdefinitive",
            "economy.supplier.invoice.successupdated",
            "common.customer.invoices.asktransfervoucher",
            "economy.supplier.payment.voucherscreated",
            "core.errortryagain",
            "core.warning",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            //Currency
            this.invoice.currencyRate = this.currencyRate;

            //VatCode
            if (this.invoice.vatCodeId === 0)
                this.invoice.vatCodeId = null;

            //Remove spaces from OCR
            if (this.invoice.ocr)
                this.invoice.ocr = this.invoice.ocr.replace(/\s/g, "");

            this.invoice.supplierInvoiceFiles = this.filesHelper.getAsDTOs(undefined, undefined, true);

            if (this.hasScanningEntryInvoice === true && this.invoiceImage && (!this.invoice.invoiceId || this.invoice.invoiceId === 0)) {
                this.invoice.scanningImage = this.invoiceImage;
            }
            else {
                if (this.invoiceImage) {
                    if (!this.invoiceImage.id)
                        this.invoiceImage.id = this.invoiceImage.imageId;
                    const image = new FileUploadDTO();

                    if (this.invoiceImage.id)
                        image.id = this.invoiceImage.id;
                    else if (this.invoiceImage.formatType === ImageFormatType.NONE || this.invoiceImage.formatType === ImageFormatType.PDF)
                        image.id = this.invoiceImage.imageId;
                    else
                        image.imageId = this.invoiceImage.imageId;

                    image.isSupplierInvoice = true;
                    image.fileName = this.invoiceImage.fileName;
                    image.description = this.invoiceImage.description;
                    image.isDeleted = this.invoiceImage.isDeleted;
                    image.includeWhenDistributed = this.invoiceImage.includeWhenDistributed;
                    image.includeWhenTransfered = this.invoiceImage.includeWhenTransfered;
                    image.invoiceAttachmentId = this.invoiceImage.invoiceAttachmentId;
                    image.dataStorageRecordType = this.invoiceImage.dataStorageRecordType;
                    image.sourceType = this.invoiceImage.sourceType;

                    this.invoice.supplierInvoiceFiles.push(image);
                }
            }

            // Status icon
            var flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
            var statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(this.invoice.statusIcon);
            // Remove first in case of removed
            statusIcons = statusIcons.remove(SoeStatusIcon.Image);
            statusIcons = statusIcons.remove(SoeStatusIcon.Attachment);
            // Add existing
            /*_.forEach(statusIcons, (x) => {
                this.invoice.statusIcon |= x;
            });*/

            // Check attachements
            if (this.invoiceImage && !this.invoiceImage.isDeleted && !statusIcons.contains(SoeStatusIcon.Image))
                statusIcons.add(SoeStatusIcon.Image);
            if (this.invoice.supplierInvoiceFiles.length > 1 && !statusIcons.contains(SoeStatusIcon.Attachment))
                statusIcons.add(SoeStatusIcon.Attachment);
            this.invoice.statusIcon = statusIcons.toValue();

            let originStatus: SoeOriginStatus = attestStatusSetting;
            if (attestStatusSetting === SoeOriginStatus.None) {
                originStatus = this.draft ? SoeOriginStatus.Draft : SoeOriginStatus.Origin;
            }

            this.invoice.originStatus = originStatus;
            this.invoice.voucherSeriesTypeId = this.voucherSeries ? this.voucherSeries.find(vs => vs.voucherSeriesId == this.invoice.voucherSeriesId).voucherSeriesTypeId : this.defaultVoucherSeriesTypeId;

            //this.startSave();
            this.invoice.accountingRows = _.filter(this.invoice.accountingRows, r => r.dim1Id && r.type === AccountingRowType.AccountingRow);

            //Make sure no amounts are null - fails in serialization
            _.forEach(this.invoice.accountingRows, (row) => {
                if (!row.amount)
                    row.amount = 0;
                if (!row.amountCurrency)
                    row.amountCurrency = 0;
                if (!row.amountEntCurrency)
                    row.amountEntCurrency = 0;
                if (!row.amountLedgerCurrency)
                    row.amountLedgerCurrency = 0;

                if (!row.creditAmount)
                    row.creditAmount = 0;
                if (!row.creditAmountCurrency)
                    row.creditAmountCurrency = 0;
                if (!row.creditAmountEntCurrency)
                    row.creditAmountEntCurrency = 0;
                if (!row.creditAmountLedgerCurrency)
                    row.creditAmountLedgerCurrency = 0;

                if (!row.debitAmount)
                    row.debitAmount = 0;
                if (!row.debitAmountCurrency)
                    row.debitAmountCurrency = 0;
                if (!row.debitAmountEntCurrency)
                    row.debitAmountEntCurrency = 0;
                if (!row.debitAmountLedgerCurrency)
                    row.debitAmountLedgerCurrency = 0;

                // Fail safe
                if ((row.debitAmount != 0 && row.debitAmountCurrency != 0 && row.amount === 0 && row.amountCurrency === 0) || (row.creditAmount != 0 && row.creditAmountCurrency != 0 && row.amount === 0 && row.amountCurrency === 0))
                    row.updateAmount();
            });

            const modifiedPurchases = this.purchaseInvoiceRows.filter(r => r.isModified);
            this.progress.startSaveProgress((completion) => {
                this.supplierService.saveInvoice(this.invoice, modifiedPurchases, this.attestRowsTransferred, this.skipInvoiceNrCheck, disregardConcurrencyCheck).then((result) => {
                    if (result.success) {
                        if (modifiedPurchases && modifiedPurchases.length > 0) {
                            this.$scope.$broadcast('reloadPurchaseRows', {});
                        }
                        // Empty image
                        if (openNextInvoice)
                            this.invoiceImage = null;

                        // Set invoice id to be able to reload it
                        if (result.integerValue && result.integerValue > 0) {
                            this.supplierInvoiceId = result.integerValue;
                            if ((this.ediType == TermGroup_EDISourceType.Scanning || this.ediType == TermGroup_EDISourceType.Finvoice) && !openNextInvoice) {
                                this.ediType = TermGroup_EDISourceType.Unset;
                                this.ediEntryId = 0;
                            }
                        }

                        var showVoucherDialog = false;
                        var voucherNrs: string = "";
                        var voucherIds: number[] = [];
                        if (this.supplierInvoiceTransferToVoucher) {
                            this.accountingService.calculateAccountBalanceForAccountsFromVoucher(this.currentAccountYearId).then((balanceresult) => {
                                if (balanceresult.success) {
                                    //Do something?
                                }
                            });

                            if (this.supplierInvoiceAskPrintVoucherOnTransfer && result.idDict) {
                                showVoucherDialog = true;
                                // Get keys
                                _.forEach(Object.keys(result.idDict), (key) => {
                                    voucherIds.push(Number(key));
                                });

                                // Get values
                                var first: boolean = true;
                                _.forEach(result.idDict, (pair) => {
                                    if (!first)
                                        voucherNrs = voucherNrs + ", ";
                                    else
                                        first = false;
                                    voucherNrs = voucherNrs + pair;
                                });
                            }
                        }

                        // Set sequence number to update the tab header
                        if (result.value)
                            this.invoice.seqNr = result.value;

                        this.infoMessage = this.isNew ? (!this.invoice.seqNr ? terms["economy.supplier.invoice.successpreliminary"] : terms["economy.supplier.invoice.successdefinitive"].format(this.invoice.seqNr.toString())) + "." : terms["economy.supplier.invoice.successupdated"] + ".";
                        var callNewInvoice = (this.isNew && !showAddInvoiceToAttestFlowDialog);
                        var currentSupplierInvoiceId = this.supplierInvoiceId;
                        var currentEdiEntryId = this.ediEntryId;
                        this.showInfoMessage = true;

                        if (showVoucherDialog) {
                            //this.completedSave(this.invoice, true, "");
                            completion.completed(
                                this.evaluateInvoiceUpsertEventState(callNewInvoice),
                                this.invoice, true);

                            this.infoMessage += " " + terms["economy.supplier.payment.voucherscreated"] + ": " + voucherNrs + ".";
                            this.infoButtons.push(new ToolBarButton(null, "economy.supplier.payment.askPrintVoucher", IconLibrary.FontAwesome, "fa-print", () => {
                                this.printVouchers(voucherIds);
                            }, null, null
                            ));
                            /*var modal = this.notificationService.showDialog(terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OK);
                            modal.result.then(val => {
                                var voucherMessage = terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + terms["economy.supplier.payment.askPrintVoucher"];
                                var modal = this.notificationService.showDialog(terms["core.verifyquestion"], voucherMessage, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                                modal.result.then(val => {
                                    if (val != null && val === true) {
                                        this.printVouchers(voucherIds);
                                    };
                                });
                            });*/
                        }
                        else {
                            //this.completedSave(this.invoice, !this.isNew || this.saveAttest || closeAfterSave, message);
                            completion.completed(this.evaluateInvoiceUpsertEventState(callNewInvoice), this.invoice, true);
                            //!this.isNew || this.saveAttest || closeAfterSave, message);
                        }
                        this.dirtyHandler.clean();

                        if (callReload) {
                            this.reloadSupplierInvoice(reloadInvoiceId, reloadOpenExpander);
                        }
                        else {
                            if (this.isNew && this.saveAttest && this.attestWorkFlowHead !== null) {
                                this.attestWorkFlowHead.recordId = this.supplierInvoiceId;
                                return this.supplierService.saveAttestWorkFlow(this.attestWorkFlowHead).then(() => {
                                    this.load(null, 0, 0, true);
                                    this.saveAttest = false;
                                });
                            }
                            else {
                                if (closeAfterSave && !this.attestRowsTransferred && !this.keepOpen) {
                                    this.dirtyHandler.clean();
                                    this.closeMe(false); //Reload is taken care of in completedSave
                                } else {
                                    if (callNewInvoice) {
                                        this.supressNote = this.invoice.actorId === this.supplierId;

                                        this.new(openNextInvoice);
                                        if (this.ediType == TermGroup_EDISourceType.Scanning && openNextInvoice)
                                            this.load(openNextInvoice, currentEdiEntryId, currentSupplierInvoiceId, !openNextInvoice);

                                    } else {
                                        this.supressNote = true;

                                        if (showAddInvoiceToAttestFlowDialog)
                                            this.initShowAddInvoiceToAttestFlow(true);

                                        if (this.attestRowsTransferred) {
                                            this.supplierService.saveSupplierInvoiceAttestAccountingRows(this.supplierInvoiceId, this.invoice.supplierInvoiceAttestRows.filter(r => (r.isDeleted && r.isModified) || !r.isDeleted)).then(() => {
                                                this.invoice.accountingRows.filter(x => x.type === AccountingRowType.SupplierInvoiceAttestRow).forEach((r: IAccountingRowDTO) => r.isModified = false);
                                                this.load(openNextInvoice, currentEdiEntryId, currentSupplierInvoiceId, !openNextInvoice);
                                            });
                                        } else {
                                            if (openNextInvoice) {
                                                this.new(openNextInvoice); //to clear current info...
                                            }
                                            this.load(openNextInvoice, currentEdiEntryId, currentSupplierInvoiceId, !openNextInvoice);
                                        }
                                    }
                                }
                            }
                        }
                        if (this.isModal) {
                            this.closeModal(true, this.supplierInvoiceId);
                        }

                        this.skipInvoiceNrCheck = false;                            
                    } else {
                        if (result.errorNumber === ActionResultSave.EntityIsModifiedByOtherUser) {
                            this.$timeout(() => {
                                completion.completed();
                                const modal = this.notificationService.showDialog(terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                                modal.result.then(val => {
                                    if (val === true)
                                        this.startSave(closeAfterSave, openNextInvoice, attestStatusSetting, showAddInvoiceToAttestFlowDialog, callReload, reloadInvoiceId, reloadOpenExpander, true);
                                });
                            });
                        }
                        else {
                            completion.completed();
                            let message: string = result.errorMessage;
                            if (!message)
                                message = terms["core.errortryagain"];

                            if (result.errorNumber == ActionResultSave.Duplicate) {
                                if (this.ignoreAskDuplicate)
                                    return
                                this.ignoreAskDuplicate = true;
                                const modal = this.notificationService.showDialog(terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                                modal.result.then(val => {
                                    if (val != null && val === true) {
                                        this.skipInvoiceNrCheck = true;
                                        this.startSave(closeAfterSave, openNextInvoice, attestStatusSetting, showAddInvoiceToAttestFlowDialog);
                                    }
                                });
                                modal.result.finally(val => {
                                    this.ignoreAskDuplicate = false;
                                })
                            }
                            else {
                                completion.completed();

                                const modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                modal.result.then(val => {
                                    //
                                });
                                completion.failed(message);
                            }
                        }
                    }
                }, error => {
                    completion.failed(terms["core.errortryagain"]);
                });
            }, this.guid).then(() => {
                this.isSaving = false;
                this.updateHeight()
            })

            this.ignoreAskUnbalanced = false;
        });
    }

    private beforeSaveChecks(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        const keys = [
            "core.verifyquestion",
            "economy.supplier.invoice.zeroamountvarning",
            "common.customer.invoices.accountdimdiff",
            "common.customer.invoices.unbalancedamounts"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.checkInvoiceAmountZero(terms).then((okFromZeroAmount: boolean) => {
                if (okFromZeroAmount) {
                    this.askSaveUnbalanced(terms).then((okFromUnbalanced: boolean) => {
                        if (okFromUnbalanced) {
                            this.checkAccountingRowsAccountDims(terms).then((okFromAccountingRows: boolean) => {
                                deferral.resolve(okFromAccountingRows);
                            });
                        }
                        else {
                            deferral.resolve(false);
                        }
                    })
                }
                else {
                    deferral.resolve(false);
                }
            });
        });

        return deferral.promise;
    }

    private checkInvoiceAmountZero(terms: { [index: string]: string }): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (this.invoice.totalAmountCurrency === 0) {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.invoice.zeroamountvarning"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val && val === true) {
                    deferral.resolve(true);
                }
                else {
                    deferral.resolve(false);
                }
            });
        }
        else {
            deferral.resolve(true);
        }
        return deferral.promise;
    }

    private askSaveUnbalanced(terms: { [index: string]: string }): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        let accountingRowsTotalAmount = 0;
        const reversVAT = (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor) ||
            (this.invoice.vatType === TermGroup_InvoiceVatType.EU)

        if (this.invoice.billingType === TermGroup_BillingType.Credit) {
            accountingRowsTotalAmount = _.sumBy(_.filter(this.invoice.accountingRows, x => !x.isDeleted && x.isDebitRow === true && (reversVAT ? x.isContractorVatRow === false : true)), r => r.debitAmountCurrency);
            accountingRowsTotalAmount = (accountingRowsTotalAmount * -1);
        }
        else {
            accountingRowsTotalAmount = _.sumBy(_.filter(this.invoice.accountingRows, x => !x.isDeleted && x.isCreditRow === true && (reversVAT ? x.isContractorVatRow === false : true)), r => r.creditAmountCurrency);
        }

        if (accountingRowsTotalAmount !== this.invoice.totalAmountCurrency) {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.customer.invoices.unbalancedamounts"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val && val === true) {
                    this.ignoreAskUnbalanced = true;
                    deferral.resolve(true);
                }
                else {
                    deferral.resolve(false);
                }
            }, () => {
                deferral.resolve(false);

            });
        }
        else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private checkAccountingRowsAccountDims(terms: { [index: string]: string }): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        let hasAccountDimDiff = false;
        if (this.invoice.billingType === TermGroup_BillingType.Credit) {
            _.forEach(_.filter(this.invoice.accountingRows, { isCreditRow: true, isVatRow: false }), (row) => {
                if (row.dim2Id && this.invoice.defaultDim2AccountId && this.invoice.defaultDim2AccountId !== row.dim2Id)
                    hasAccountDimDiff = true;
                if (row.dim3Id && this.invoice.defaultDim3AccountId && this.invoice.defaultDim3AccountId !== row.dim3Id)
                    hasAccountDimDiff = true;
                if (row.dim4Id && this.invoice.defaultDim4AccountId && this.invoice.defaultDim4AccountId !== row.dim4Id)
                    hasAccountDimDiff = true;
                if (row.dim5Id && this.invoice.defaultDim5AccountId && this.invoice.defaultDim5AccountId !== row.dim5Id)
                    hasAccountDimDiff = true;
                if (row.dim6Id && this.invoice.defaultDim6AccountId && this.invoice.defaultDim6AccountId !== row.dim6Id)
                    hasAccountDimDiff = true;
            });
        }
        else {
            _.forEach(_.filter(this.invoice.accountingRows, { isDebitRow: true, isVatRow: false }), (row) => {
                if (row.dim2Id && this.invoice.defaultDim2AccountId && this.invoice.defaultDim2AccountId !== row.dim2Id)
                    hasAccountDimDiff = true;
                if (row.dim3Id && this.invoice.defaultDim3AccountId && this.invoice.defaultDim3AccountId !== row.dim3Id)
                    hasAccountDimDiff = true;
                if (row.dim4Id && this.invoice.defaultDim4AccountId && this.invoice.defaultDim4AccountId !== row.dim4Id)
                    hasAccountDimDiff = true;
                if (row.dim5Id && this.invoice.defaultDim5AccountId && this.invoice.defaultDim5AccountId !== row.dim5Id)
                    hasAccountDimDiff = true;
                if (row.dim6Id && this.invoice.defaultDim6AccountId && this.invoice.defaultDim6AccountId !== row.dim6Id)
                    hasAccountDimDiff = true;
            });
        }

        if (hasAccountDimDiff) {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["common.customer.invoices.accountdimdiff"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val && val === true) {
                    deferral.resolve(true);
                }
                else {
                    deferral.resolve(false);
                }
            }, () => {
                deferral.resolve(false);

            });
        }
        else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    protected delete() {
        this.performDelete(false);
    }

    private performDelete(copy: boolean) {
        this.progress.startDeleteProgress((completion) => {
            this.supplierService.deleteInvoice(this.invoice.invoiceId).then((result) => {
                if (result.success) {
                    //this.completedDelete(this.invoice);
                    completion.completed(this.invoice);
                    if (copy)
                        this.copy();
                    else
                        this.new();
                    this.translationService.translateMany(["economy.supplier.invoice.new"]).then((terms) => {
                        this.messagingHandler.publishSetTabLabel(this.guid, terms["economy.supplier.invoice.new"]);
                    });
                }
                else {
                    //this.failedDelete(result.errorMessage);
                    completion.failed(result.errorMessage);
                }
            }, error => {
                //this.failedDelete(error.message);
                completion.failed(error.message);
            });
        });
    }

    protected initCredit() {
        if (this.accountDistributionHeadId == 0) {
            this.copy(true);
            return;
        }

        this.accountingService.getAccountDistributionHead(this.accountDistributionHeadId).then((accountDistributionHead) => {
            if (accountDistributionHead.type != SoeAccountDistributionType.Period) {
                this.copy(true);
                return;
            }
            const keys: string[] = [
                "core.verifyquestion",
                "economy.accounting.accountdistribution.removeentriesmessage",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.accounting.accountdistribution.removeentriesmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.removeAccountDistribution();
                    this.copy(true);
                });
            });
        });
    }

    protected removeAccountDistribution() {
        this.accountingService.deleteAccountDistributionEntriesForSource(this.accountDistributionHeadId, TermGroup_AccountDistributionRegistrationType.SupplierInvoice, this.invoice.invoiceId).then(() => {

        });
    }

    protected initRevoke() {
        // Show verification dialog
        const keys: string[] = [
            "core.verifyquestion",
            "economy.supplier.invoice.revokequestion",
            "economy.accounting.accountdistribution.removeentriesmessage",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.invoice.revokequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel);
            modal.result.then(val => {

                if (this.accountDistributionHeadId > 0) {
                    const modal2 = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.accounting.accountdistribution.removeentriesmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                    modal2.result.then(val => {
                        this.removeAccountDistribution();
                        this.performDelete(val);
                    });
                }
                else
                    this.performDelete(val);
            });
        });
    }

    protected initShowFinvoice() {
        // Show finvoice picture
        if (this.ediEntryId > 0) {
            var uri = window.location.protocol + "//" + window.location.host;
            uri = uri + "/soe/common/xslt/" + "?templatetype=" + SoeReportTemplateType.FinvoiceEdiSupplierInvoice + "&id=" + this.ediEntryId + "&c=" + CoreUtility.actorCompanyId;
            window.open(uri, '_blank');
        }
    }

    protected convertFinvoiceImage() {
        this.progress.startWorkProgress((completion) => {
            this.supplierService.createFinvoiceImage(this.ediEntryId).then((result: IActionResult) => {
                if (result.success) {
                    completion.completed(result, false);
                    this.getImageByInvoiceId(this.supplierInvoiceId);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        });
    }

    protected LoadEdiEntryFromInvoice() {
        this.supplierService.getEdiEntryFromInvoice(this.supplierInvoiceId).then((x: EdiEntryDTO) => {
            //Null check                
            if (x != null) {
                this.ediEntryId = x.ediEntryId;
                this.scanningEntryId = x.scanningEntryInvoiceId;
                switch (x.type) {
                    case 1:
                        this.ediType = TermGroup_EDISourceType.EDI;
                        if (x.pdf == null)
                            this.createImageByEdiEntryId(this.ediEntryId).then(result => {
                                this.getImageByInvoiceId(this.supplierInvoiceId);
                            });
                        break;
                    case 2:
                        this.ediType = TermGroup_EDISourceType.Scanning;
                        break;
                    case 3:
                        this.ediType = TermGroup_EDISourceType.Finvoice;
                        break;
                }
            }
        });

    }

    protected initShowAddInvoiceToAttestFlow(calledFromSave = false) {
        // Show AddinvoiceToAttestFlow dialog
        if ((this.supplierInvoiceId == 0 || this.dirtyHandler.isDirty) && !calledFromSave) {
            this.save(false, false, SoeOriginStatus.None, true);
        }
        else {
            if (this.invoice.attestGroupId == null) {
                this.accountingService.getAccountDimBySieNr(TermGroup_SieAccountDim.CostCentre).then(x => {
                    var accountDim = x;
                    this.accountDimNrForCostplace = accountDim ? accountDim.accountDimNr : 0;
                });

                var projectId = this.invoice.projectId;
                if (projectId === undefined || projectId === null) projectId = 0;

                var costplaceAccountId = this.invoice['defaultDim' + this.accountDimNrForCostplace + 'AccountId'];
                if (costplaceAccountId === undefined || costplaceAccountId === null) costplaceAccountId = 0;

                var referenceOur = this.invoice.referenceOur;
                if (referenceOur === undefined || referenceOur === null || referenceOur === "") referenceOur = "null";

                this.supplierService.getAttestGroupSuggestion(this.selectedSupplier.id, projectId, costplaceAccountId, referenceOur).then(x => {
                    var attestWorkFlowGroup = x;
                    this.defaultAttestGroup = attestWorkFlowGroup ? attestWorkFlowGroup.attestWorkFlowHeadId : 0;
                    this.showAttestDialog();
                });
            }
            else {
                this.defaultAttestGroup = this.invoice.attestGroupId;
                this.showAttestDialog();
            }
        }
    }

    protected copy(creditInvoice = false) {
        this.isNew = true;
        if (creditInvoice) {
            this.invoice.prevInvoiceId = this.supplierInvoiceId;
            this.invoice.originStatus = SoeOriginStatus.Draft;
            this.invoice.originStatusName = null;

            this.invoice.totalAmountCurrency = (this.invoice.totalAmountCurrency * -1);
            this.invoice.vatAmountCurrency = (this.invoice.vatAmountCurrency * -1);

            this.invoice.totalAmount = (this.invoice.totalAmount * -1);
            this.invoice.vatAmount = (this.invoice.vatAmount * -1);

            this.invoice.invoiceNr = this.invoiceNr + "-1";
            this.invoice.billingType = TermGroup_BillingType.Credit;

            _.forEach(this.invoice.supplierInvoiceProjectRows, (row) => {
                row.supplierInvoiceId = 0;
                row.amount = (row.amount * -1);
                row.amountCurrency = (row.amountCurrency * -1);
                row.amountEntCurrency = (row.amountEntCurrency * -1);
                row.amountLedgerCurrency = (row.amountLedgerCurrency * -1);
            });

            this.invoice.supplierInvoiceOrderRows = [];
            this.invoice.supplierInvoiceAttestRows = [];
            this.invoice.voucheHeadId = 0;

            this.invoice.fullyPayed = false;
            this.invoice.paidAmount = 0;
            this.invoice.paidAmountCurrency = 0;
            this.invoice.paidAmountEntCurrency = 0;
            this.invoice.paidAmountLedgerCurrency = 0;

            this.filesHelper.reset();
            this.invoiceImage = null;
            this.invoice.supplierInvoiceFiles = null;
            this.filesHelper.nbrOfFiles = '';
            this.setInvoiceHasImage(false)

            this.setVatRate();

            //Accounting rows
            _.forEach(this.invoice.accountingRows, (dto: AccountingRowDTO) => {
                if (dto.isCreditRow) {
                    dto.isCreditRow = false;
                    dto.isDebitRow = true;

                    dto.amount = Math.abs(dto.amount);
                    dto.amountCurrency = Math.abs(dto.amountCurrency);
                    dto.amountEntCurrency = Math.abs(dto.amountEntCurrency);
                    dto.amountLedgerCurrency = Math.abs(dto.amountLedgerCurrency);

                    dto.debitAmount = dto.creditAmount;
                    dto.debitAmountCurrency = dto.creditAmountCurrency;
                    dto.debitAmountEntCurrency = dto.creditAmountEntCurrency;
                    dto.debitAmountLedgerCurrency = dto.creditAmountLedgerCurrency;
                    dto.creditAmount = 0;
                    dto.creditAmountCurrency = 0;
                    dto.creditAmountEntCurrency = 0;
                    dto.creditAmountLedgerCurrency = 0;
                }
                else {
                    dto.isCreditRow = true;
                    dto.isDebitRow = false;

                    dto.amount = dto.amount * -1;
                    dto.amountCurrency = dto.amountCurrency * -1;
                    dto.amountEntCurrency = dto.amountEntCurrency * -1;
                    dto.amountLedgerCurrency = dto.amountLedgerCurrency * -1;

                    dto.creditAmount = dto.debitAmount;
                    dto.creditAmountCurrency = dto.debitAmountCurrency;
                    dto.creditAmountEntCurrency = dto.debitAmountEntCurrency;
                    dto.creditAmountLedgerCurrency = dto.debitAmountLedgerCurrency;
                    dto.debitAmount = 0;
                    dto.debitAmountCurrency = 0;
                    dto.debitAmountEntCurrency = 0;
                    dto.debitAmountLedgerCurrency = 0;
                }

                dto.invoiceAccountRowId = undefined;
                dto.isModified = true;
                dto.text = "";
            });

            this.$scope.$broadcast('rowsAdded');

            this.draft = this.defaultDraft;

            this.setLocked();
        }
        else {
            this.draft = this.defaultDraft;

            // TODO: AccountingRowsDataGrid.ClearRowIds();
        }

        this.supplierInvoiceId = 0;
        this.accountDistributionHeadId = 0;
        this.invoice.invoiceId = 0;
        this.invoice.seqNr = null;

        this.dirtyHandler.isDirty = true;

        this.translationService.translate("economy.supplier.invoice.new").then((label) => {
            this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
                guid: this.guid,
                label: label,
            });
        });
    }

    public reloadSupplierInvoice(invoiceId: number, openAttestExpander = false) {
        if (this.dirtyHandler.isDirty && !this.usesSecondaryDirtyEvent) {
            this.notificationService.showConfirmOnExit().then((result) => {
                if (result) {
                    this.dirtyHandler.isDirty = false;
                    this.reloadSupplierInvoice(invoiceId, openAttestExpander);
                }
                else {
                    //notify back that we did not change invoice....
                    this.messagingService.publish(Constants.EVENT_INVOICE_CHANGED, this.supplierInvoiceId);
                }
            })
            return;
        }

        this.invoiceIsLoaded = false;
        this.supplierInvoiceId = invoiceId;
        this.ediEntryId = 0; //Clear, otherwise shows wrong picture
        this.ediType = TermGroup_EDISourceType.Unset; //Clear, otherwise attest overview don't work
        this.setInvoiceHasImage(true);

        if (openAttestExpander) {
            this.attestUserExpanderOpen = true;
        }

        if (invoiceId > 0)
            this.load();
        else if (invoiceId === -1) {
            this.new();
        }
    }

    // HELP-METHODS

    private evaluateDirtyness() {
        if (this.invoice && !this.dirtyHandler.isDirty)
            this.dirtyHandler.isDirty = true;
    }

    private new(openNextInvoice: boolean = false) {
        if (!this.currencyDate) {
            this.currencyDate = new Date(new Date().toDateString())
        }

        this.isNew = true;
        this.supplierInvoiceId = 0;
        this.ediEntryId = 0;

        this.invoice = new SupplierInvoiceDTO();
        this.invoice.originStatus = SoeOriginStatus.Draft;
        this.selectedSupplier = null;
        this.invoice.vatType = this.defaultVatType;
        this.invoice.vatCodeId = this.defaultVatCodeId;
        this.invoice.voucherSeriesId = this.defaultVoucherSeriesTypeId;
        this.invoice.voucherSeriesTypeId = this.defaultVoucherSeriesTypeId;
        this.invoice.currencyId = this.currencies[0].currencyId;    // Base currency is first in collection
        this.invoice.currencyDate = this.currencyDate = CalendarUtility.getDateToday();
        this.invoice.projectId = null;
        this.invoice.orderNr = null;
        this.selectedInvoiceDate = null;
        this.selectedVoucherDate = this.currencyDate;
        this.selectedPaymentInfo = null;
        this.selectedCustomerInvoice = null;
        this.selectedProject = null;
        this.paymentConditionDays = this.defaultPaymentConditionDays;
        this.invoice.interimInvoice = this.allowInterim;
        this.invoice.totalAmount = this.invoice.totalAmountCurrency = 0;

        //Dim accounts
        this.defaultAccountDim2Id = null;
        this.defaultAccountDim3Id = null;
        this.defaultAccountDim4Id = null;
        this.defaultAccountDim5Id = null;
        this.defaultAccountDim6Id = null;

        this.invoice.supplierInvoiceProjectRows = [];
        this.invoice.supplierInvoiceOrderRows = [];
        this.invoice.supplierInvoiceCostAllocationRows = [];

        _.forEach(this.voucherSeries, (voucherSerie) => {
            if (voucherSerie.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId) {
                this.selectedVoucherSeriesId = voucherSerie.voucherSeriesId;
            }
        });

        this.draft = this.defaultDraft;

        this.invoice.accountingRows = [];
        this.invoice.supplierInvoiceAttestRows = [];

        this.setInvoiceHasImage(false);
        this.invoiceImage = null;

        this.invoice.supplierInvoiceFiles = [];
        this.filesHelper.nbrOfFiles = '';
        this.filesHelper.reset();

        if (this.supplierId && this.supplierId > 0 && this.keepSupplier && !openNextInvoice)
            this.selectedSupplier = _.find(this.suppliers, { id: this.supplierId });

        // Scanning
        this.scanningEntryId = 0;
        this.invoiceInterpretation = null;

        if (this.hasScanningEntryInvoice) {
            this.hasScanningEntryInvoice = false;

            this.scanningIsCreditInvoieIcon = "";
            this.scanningIsCreditInvoiceTooltip = "";
            this.scanningInvoiceNrIcon = "";
            this.scanningInvoiceNrTooltip = "";
            this.scanningInvoiceDateIcon = "";
            this.scanningInvoiceDateTooltip = "";
            this.scanningDueDateIcon = "";
            this.scanningDueDateTooltip = "";
            this.scanningReferenceYourIcon = "";
            this.scanningReferenceYourTooltip = "";
            this.scanningVatAmountIcon = "";
            this.scanningVatAmountTooltip = "";
            this.scanningTotalAmountIncludeVatIcon = "";
            this.scanningTotalAmountIncludeVatTooltip = "";
            this.scanningCurrencyCodeIcon = "";
            this.scanningCurrencyCodeTooltip = "";
            this.scanningOCRIcon = "";
            this.scanningOCRTooltip = "";
        }

        this.updateTabCaption();

        this.isLocked = false;
        this.dirtyHandler.isDirty = !openNextInvoice;

        this.focusService.focusByName("ctrl_selectedSupplier");
    }

    private setPaymentCondition(paymentConditionId: number) {
        if (paymentConditionId === 0)
            paymentConditionId = this.defaultPaymentConditionId;

        // Get condition
        const condition = _.find(this.paymentConditions, { paymentConditionId: paymentConditionId });
        this.paymentConditionDays = condition ? condition.days : this.defaultPaymentConditionDays;
        this.discountDays = condition ? condition.discountDays : 0;
        this.discountPercent = condition && condition.discountPercent ? condition.discountPercent : 0;
    }

    private setTimeDiscount() {
        if (this.useTimeDiscount && !this.loadingInvoice) {
            if (this.selectedInvoiceDate && this.discountDays) {
                this.invoice.timeDiscountDate = this.invoice.invoiceDate.addDays(this.discountDays);
                this.invoice.timeDiscountPercent = this.discountPercent;
            }
            else {
                this.invoice.timeDiscountDate = null;
                this.invoice.timeDiscountPercent = null;
            }
        }
    }

    private setDueDate() {
        if (this.invoice && !this.loadingInvoice)
            this.invoice.dueDate = this.invoice.invoiceDate ? this.invoice.invoiceDate.addDays(this.paymentConditionDays) : null;
    }

    private showSupplierNote(message: string) {
        this.translationService.translate("common.note").then((title) => {
            this.notificationService.showDialog(title, message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }

    private GetVatCreditAccountId() {
        if (this.invoice.vatType === TermGroup_InvoiceVatType.EU) {
            switch (this.vatRate) {
                case 6:
                    return this.euVatCredit3AccountId;
                case 12:
                    return this.euVatCredit2AccountId;
                default:
                    return this.euVatCredit1AccountId;
            }
        }
        else if (this.invoice.vatType === TermGroup_InvoiceVatType.NonEU) {
            switch (this.vatRate) {
                case 6:
                    return this.nonEuVatCredit3AccountId;
                case 12:
                    return this.nonEuVatCredit2AccountId;
                default:
                    return this.nonEuVatCredit1AccountId;
            }
        }
    }

    private generateAccountingRows(calculateVat: boolean, productRows: ISupplierInvoiceProductRowDTO[] = null, supplierInvoiceProductRowsImport: boolean = false) {

        // Clear rows
        this.invoice.accountingRows = [];

        // Credit row
        this.createAccountingRow(SupplierAccountType.Credit, 0, this.invoice.totalAmountCurrency, false, false, false);

        // VAT row
        if (calculateVat)
            this.calculateVatAmount();

        let vatAmount = this.invoice.vatAmountCurrency;

        if (this.roundVAT) {
            vatAmount = vatAmount.round(0);
        }

        if (vatAmount && vatAmount != 0) {
            switch (this.invoice.vatType) {
                case (TermGroup_InvoiceVatType.Contractor):
                    // 'Contractor' invoices does not have any regular VAT
                    this.createAccountingRow(SupplierAccountType.Unknown, this.contractorVatAccountCreditId, vatAmount, false, false, true);
                    this.createAccountingRow(SupplierAccountType.Unknown, this.contractorVatAccountDebitId, vatAmount, true, false, true);
                    this.invoice.vatAmountCurrency = vatAmount = 0;
                    break;
                case (TermGroup_InvoiceVatType.EU):
                    this.createAccountingRow(SupplierAccountType.Unknown, this.GetVatCreditAccountId(), vatAmount, false, false, true);
                    this.createAccountingRow(SupplierAccountType.Unknown, this.euVatDebitAccountId, vatAmount, true, false, true);
                    this.invoice.vatAmountCurrency = vatAmount = 0;
                    break;
                case (TermGroup_InvoiceVatType.NonEU):
                    this.createAccountingRow(SupplierAccountType.Unknown, this.GetVatCreditAccountId(), vatAmount, false, false, true);
                    this.createAccountingRow(SupplierAccountType.Unknown, this.nonEuVatDebitAccountId, vatAmount, true, false, true);
                    this.invoice.vatAmountCurrency = vatAmount = 0;
                    break;
                case (TermGroup_InvoiceVatType.NoVat):
                    // 'No VAT' invoices does not have any VAT at all);
                    this.invoice.vatAmountCurrency = vatAmount = 0;
                    break;
                default:
                    if (this.vatRate > 0) {
                        this.createAccountingRow(SupplierAccountType.VAT, 0, vatAmount, (this.invoice.billingType === TermGroup_BillingType.Debit && vatAmount > 0) || (this.invoice.billingType === TermGroup_BillingType.Credit && vatAmount < 0), true, false);
                        break;
                    }
            }
        }

        if (supplierInvoiceProductRowsImport && productRows) {
            //CodingRows Based On ProductRows
            productRows.forEach(productRow => {
                if (productRow.rowType != SupplierInvoiceRowType.ProductRow) return;

                const text = productRow.sellerProductNumber + " " + productRow.text;
                let productRowAmount = productRow.amountCurrency;
                let isCreditRow = productRowAmount < 0;

                if (isCreditRow)
                    productRowAmount = -productRowAmount;

                this.createAccountingRow(SupplierAccountType.Debit, 0, productRowAmount, !isCreditRow, false, false, text);
            })
        } else {
            // Debit row
            this.createAccountingRow(SupplierAccountType.Debit, 0, this.invoice.totalAmountCurrency - vatAmount, true, false, false);
        }

        // Set all default internals
        this.updateAllAccountRowDimAccounts()

        this.$timeout(() => {
            //setInternalAccountFromAccount to set internal account properties and then call updateAllAccountRowDimAccounts via callback
            this.$scope.$broadcast('setRowItemAccountsOnAllRowsIfMissing');
            this.$scope.$broadcast('rowsAdded');
        });

    }

    private updateAllAccountRowDimAccounts() {
        if (this.defaultAccountDim2Id && this.defaultAccountDim2Id > 0)
            this.updateAccountRowDimAccounts(2, this.defaultAccountDim2Id, null);

        if (this.defaultAccountDim3Id && this.defaultAccountDim3Id > 0)
            this.updateAccountRowDimAccounts(3, this.defaultAccountDim3Id, null);

        if (this.defaultAccountDim4Id && this.defaultAccountDim4Id > 0)
            this.updateAccountRowDimAccounts(4, this.defaultAccountDim4Id, null);

        if (this.defaultAccountDim5Id && this.defaultAccountDim5Id > 0)
            this.updateAccountRowDimAccounts(5, this.defaultAccountDim5Id, null);

        if (this.defaultAccountDim6Id && this.defaultAccountDim6Id > 0)
            this.updateAccountRowDimAccounts(6, this.defaultAccountDim6Id, null);
    }

    private setVoucherDateOnAccountingRows() {
        if (!this.invoice.accountingRows?.length) return;
        this.invoice.accountingRows.forEach((row) => {
            row.date = this.selectedVoucherDate ? this.selectedVoucherDate.date() : new Date().date();
            if (!row.parentRowId)
                this.$scope.$broadcast('checkAccountDistribution', row, 2);
        });
    }

    private createAccountingRow(type: SupplierAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean, isContractorVatRow: boolean, text: string = ""): AccountingRowDTO {
        // Credit invoice, negate isDebitRow
        if (this.isCredit)
            isDebitRow = !isDebitRow;

        var row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = row.tempInvoiceRowId = this.invoice.accountingRows.length + 1;
        row.rowNr = AccountingRowDTO.getNextRowNr(this.invoice.accountingRows);

        row.amountCurrency = isDebitRow ? Math.abs(amount) : -Math.abs(amount);
        row.debitAmountCurrency = isDebitRow ? Math.abs(amount) : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : Math.abs(amount);
        row.quantity = null;
        row.date = this.selectedVoucherDate ? this.selectedVoucherDate.date() : new Date().date();
        row.isCreditRow = !isDebitRow;
        row.isDebitRow = isDebitRow;
        row.isVatRow = isVatRow;
        row.isContractorVatRow = isContractorVatRow;
        row.isInterimRow = type === SupplierAccountType.Debit && this.invoice.interimInvoice;
        row.state = SoeEntityState.Active;
        row.text = text;
        row.invoiceId = this.invoice.invoiceId;
        row.isModified = false;

        // Set accounts
        if (type !== SupplierAccountType.Unknown) {
            row.dim1Id = this.getAccountId(type, 1);
            let dimCounter = 2
            this.accountDims.forEach((dim) => {
                row["dim" + dimCounter + "Id"] = this.getAccountId(type, dim.accountDimNr);
                dimCounter++;
            });
        }
        else {
            row.dim1Id = 0;
            row.dim2Id = 0;
            row.dim3Id = 0;
            row.dim4Id = 0;
            row.dim5Id = 0;
            row.dim6Id = 0;
        }

        // Override default account
        if (accountId !== 0)
            row.dim1Id = accountId;

        this.invoice.accountingRows.push(row);

        this.$timeout(() => {
            this.$scope.$broadcast('checkAccountDistribution', row, 2);
            this.$scope.$broadcast('checkInventoryAccounts', row, 2);
            this.$scope.$broadcast('setRowItemAccountsOnRow', row);
            this.$scope.$broadcast('rowAdded', row, 2);
        });

        return row;
    }

    private getAccountId(type: SupplierAccountType, dimNr: number): number {
        // First try to get account from supplier
        var accountId = this.getSupplierAccountId(type, dimNr);

        if (accountId === 0 && dimNr === 1) {
            // No account found on supplier, use base account
            switch (type) {
                case SupplierAccountType.Credit:
                    accountId = this.defaultCreditAccountId;
                    break;
                case SupplierAccountType.Debit:
                    if (this.invoice.interimInvoice) {
                        accountId = this.defaultInterimAccountId;
                        if (accountId === 0) {
                            accountId = this.defaultDebitAccountId;
                            var keys: string[] = [
                                "economy.supplier.invoice.interimaccountmissing.title",
                                "economy.supplier.invoice.interimaccountmissing.message"
                            ];
                            this.translationService.translateMany(keys).then((terms) => {
                                var modal = this.notificationService.showDialog(terms["economy.supplier.invoice.interimaccountmissing.title"], terms["economy.supplier.invoice.interimaccountmissing.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                            });
                        }
                    }
                    else if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor)
                        accountId = this.reverseVatAccountPurchaseId;
                    else if (this.invoice.vatType === TermGroup_InvoiceVatType.EU)
                        accountId = this.euVatPurchaseAccountId;
                    else if (this.invoice.vatType === TermGroup_InvoiceVatType.NonEU)
                        accountId = this.nonEuVatPurchaseAccountId;
                    else
                        accountId = this.defaultDebitAccountId;
                    break;
                case SupplierAccountType.VAT:
                    accountId = this.defaultVatAccountId;
                    break;
                case SupplierAccountType.Interim:
                    accountId = this.defaultInterimAccountId;
                    break;
            }
        }

        return accountId;
    }

    private getSupplierAccountId(type: SupplierAccountType, dimNr: number): number {
        var accountId = 0;

        if (type === SupplierAccountType.VAT && dimNr === 1) {
            let vatCode = undefined;
            if (this.invoice.vatCodeId && this.invoice.vatCodeId != 0)
                vatCode = _.find(this.vatCodes, (v) => v.vatCodeId === this.invoice.vatCodeId);
            else if (this.defaultVatCodeId)
                vatCode = _.find(this.vatCodes, { vatCodeId: this.defaultVatCodeId });

            if (vatCode && vatCode.purchaseVATAccountId && vatCode.purchaseVATAccountId > 0)
                return vatCode.purchaseVATAccountId;
        }

        if (type === SupplierAccountType.VAT && dimNr === 1 && this.supplierVatAccountId)
            return this.supplierVatAccountId;

        if (type === SupplierAccountType.Debit && this.invoice.interimInvoice)
            type = SupplierAccountType.Interim;

        if (this.supplier && this.supplier.accountingSettings) {
            var setting = _.find(this.supplier.accountingSettings, { type: type });

            if (setting) {
                if (dimNr === setting.accountDim1Nr)
                    return setting.account1Id;
                else if (dimNr === setting.accountDim2Nr)
                    return setting.account2Id;
                else if (dimNr === setting.accountDim3Nr)
                    return setting.account3Id;
                else if (dimNr === setting.accountDim4Nr)
                    return setting.account4Id;
                else if (dimNr === setting.accountDim5Nr)
                    return setting.account5Id;
                else if (dimNr === setting.accountDim6Nr)
                    return setting.account6Id;
            }
            /*var setting = _.find(this.supplier.accountingSettings, { type: type });
            var shiftTypeAccountId = shiftType.accountingSettings.getAccountId(accountDim.accountDimNr);
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
            }*/
        }

        return accountId;
    }

    private updateAccountRowDimAccounts(dimNumber: number, accountId: number, previousDimNr) {
        if (!this.loadingInvoice) {
            _.forEach(this.invoice.accountingRows, (accRow) => {
                if ((this.useInternalAccountWithBalanceSheetAccounts || (!accRow.isVatRow && !accRow.isContractorVatRow && !accRow.isHouseholdRow && !accRow.isCentRoundingRow)) && !accRow.isDeleted) {
                    if (this.useInternalAccountWithBalanceSheetAccounts ||
                        ((this.invoice.billingType === TermGroup_BillingType.Credit && accRow.isCreditRow === true) || (this.invoice.billingType === TermGroup_BillingType.Debit && accRow.isDebitRow))
                    ) {
                        switch (dimNumber) {
                            case 2:
                                if (!accRow.dim2Disabled && ((!accRow.dim2Id) || (accRow.dim2Id == previousDimNr))) {
                                    accRow.dim2Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 3:
                                if (!accRow.dim3Disabled && ((!accRow.dim3Id) || (accRow.dim3Id == previousDimNr))) {
                                    accRow.dim3Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 4:
                                if (!accRow.dim4Disabled && ((!accRow.dim4Id) || (accRow.dim4Id == previousDimNr))) {
                                    accRow.dim4Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 5:
                                if (!accRow.dim5Disabled && ((!accRow.dim5Id) || (accRow.dim5Id == previousDimNr))) {
                                    accRow.dim5Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                            case 6:
                                if (!accRow.dim6Disabled && ((!accRow.dim6Id) || (accRow.dim6Id == previousDimNr))) {
                                    accRow.dim6Id = accountId ? accountId : 0;
                                    this.$scope.$broadcast('dimChanged', [accRow.rowNr, dimNumber]);
                                }
                                break;
                        }
                    }
                }
            });
        }
    }

    private calculateVatAmount() {
        if (this.loadingInvoice) {
            return;
        }

        // Calculate VAT amount based on vat percent
        var vatAmount: number = 0;
        var vatRateValue: number = this.vatRate / 100;

        if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor ||
            this.invoice.vatType === TermGroup_InvoiceVatType.EU)
            vatAmount = this.invoice.totalAmountCurrency * vatRateValue;
        else
            vatAmount = this.invoice.totalAmountCurrency * (1 - (1 / (vatRateValue + 1)));

        this.invoice.vatAmountCurrency = vatAmount.round(2);
    }

    private calculateVatDeduction(percent: number) {
        this.calculateVatAmount();
        this.invoice.vatAmountCurrency = this.invoice.vatAmountCurrency * percent * 0.01;
        this.generateAccountingRows(false);
    }

    private setVatDeduction(dim: number, accountId: number) {

        if ((!this.accountDim) || (!this.invoice)) {
            return;
        }

        if (!accountId) {
            if (dim === this.accountDim.accountDimNr) {
                this.useVatDeduction = false;
                this.invoice.vatDeductionAccountId = undefined;
                this.invoice.vatDeductionPercent = 100;
                this.vatDeductionDict = [];
                this.generateAccountingRows(true);
                return;
            }
            else {
                return;
            }
        }

        var account = _.find(this.accountInternal, i => i.accountId == accountId);

        if (account && account.accountDimId === this.accountDim.accountDimId) {
            if (account.useVatDeduction) {
                this.invoice.vatDeductionAccountId = account.accountId;
                this.useVatDeduction = true;
                this.loadVatDeductionDict(account.vatDeduction);
            }
            else {
                this.useVatDeduction = false;
                this.invoice.vatDeductionAccountId = undefined;
                this.invoice.vatDeductionPercent = 100;
                this.vatDeductionDict = [];
            }

            if (this.isNew && account.vatDeduction > 0) {
                this.invoice.vatDeductionPercent = account.vatDeduction;
                this.calculateVatDeduction(this.invoice.vatDeductionPercent);
            }
            else {
                this.calculateVatDeduction(this.invoice.vatDeductionPercent);
            }
        }
    }

    private hasModifiedRows(): boolean {
        return _.filter(this.invoice.accountingRows, r => r.isModified).length > 0;
    }

    private hasTransferredRows(): boolean {
        return _.filter(this.invoice.accountingRows, r => r.attestStatus == SupplierInvoiceAccountRowAttestStatus.Processed).length > 0;
    }

    private getInventoryRowsExistWarningMessage(): string {
        var counter: number = 0;
        _.forEach(this.invoice.accountingRows, (row) => {
            if (row.state === SoeEntityState.Active && row.inventoryId && row.inventoryId !== 0)
                counter++;
        });

        if (counter === 1)
            return "economy.supplier.invoice.inventoryexistswarning";
        else if (counter > 1)
            return "economy.supplier.invoice.inventoriesexistswarning";

        return null;
    }

    public openSupplier() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html"),
            controller: SupplierEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope

        });
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, id: this.selectedSupplier ? this.selectedSupplier.id : 0 });
        });

        modal.result.then(result => {
            if (this.invoice.originStatus === SoeOriginStatus.Draft || (this.invoice.originStatus === SoeOriginStatus.Origin && (!this.invoice.paidAmountCurrency || this.invoice.paidAmountCurrency === 0))) {
                this.loadSuppliers(false).then(() => {
                    if ((!this.supplier) || (this.supplier.actorSupplierId !== result.id)) {
                        this.supplierService.getSupplier(result.id, false, true, false, false).then(x => {
                            this.supplier = x;
                            this.supplierChanged();
                            this.loadingInvoice = false;
                        }).then(() => {
                            this._selectedSupplier = _.find(this.suppliers, { id: result.id })
                        });
                    }
                    else if (this.supplier && this.supplier.actorSupplierId === result.id && result.isModified) {
                        // Update the invoice if the supplier is edited.
                        this.loadPaymentInformation();
                        this.dirtyHandler.setDirty();

                        this._selectedSupplier = _.find(this.suppliers, { id: result.id });
                    }
                    else if (this.supplier && this.supplier.actorSupplierId === result.id && result.isRemoved) {

                        this.supplier = null;
                        this._selectedSupplier = undefined;
                        this.supplierChanged();
                    }
                });
            }
        });
    }

    private printVouchers(ids: number[]) {
        if (this.voucherListReportId) {

            this.requestReportService.printVoucherList(ids);
        }
        else {
            var keys: string[] = [
                "core.warning",
                "economy.supplier.payment.defaultVoucherListMissing"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            });
        }
    }

    private changeIntrastat() {
        var tempRows: IntrastatTransactionDTO[] = [];
        const dto = new IntrastatTransactionDTO();
        dto.intrastatCodeId = this.supplier.intrastatCodeId;
        dto.sysCountryId = this.supplier.sysCountryId;
        dto.originId = this.supplierInvoiceId;
        dto.amount = this.invoice.totalAmount;
        dto.quantity = 1;
        dto.state = SoeEntityState.Active;
        dto.isModified = true;
        tempRows.push(dto);
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
                originType: () => SoeOriginType.SupplierInvoice,
                originId: () => this.supplierInvoiceId,
                notificationService: () => this.notificationService,
                urlHelperService: () => this.urlHelperService,
                totalAmount: () => this.invoice.totalAmount,
            }
        });

        modal.result.then((result: any[]) => {
            if (result) {
            }
        }, function () {
        });

        return modal;
    }

    // VALIDATION

    private setLocked() {
        var locked: boolean = true;
        var editLocked: boolean = false;

        if (this.modifyPermission) {
            if (this.invoice.originStatus === SoeOriginStatus.Draft) {
                // An invoice in status Draft can always be edited
                locked = false;
            } else if (this.invoice.originStatus === SoeOriginStatus.Origin) {
                // An invoice in status Origin can sometimes be edited. It depends on the company setting SupplierInvoiceAllowEditOrigin.
                // If it's set, the invoice can be edited in status Origin if no PaymentRow has been created.
                // If it's not set, the invoice cannot be edited in status Origin.
                locked = (this.allowEditOrigin ? (this.invoice.paidAmount !== 0 || this.invoice.fullyPayed) : true);

                if (this.allowEditOrigin) {
                    if (this.invoice.paidAmount !== 0 || this.invoice.fullyPayed)
                        editLocked = true;
                    else
                        locked = false;
                }
                else {
                    editLocked = true;
                }
            } else if (this.invoice.originStatus === SoeOriginStatus.Voucher) {
                locked = true;
                editLocked = true;
            } else {
                // An invoice in another status can never be edited
                locked = true;
                editLocked = false;
            }
        }

        this.isLocked = locked;
        this.canEditLocked = editLocked;
        this.isLockedAccountingRows = this.isLocked;
        if ((this.invoice.originStatus === SoeOriginStatus.Origin && this.allowEditOrigin) ||
            (this.unlockAccountingRowsPermission && this.allowEditAccountingRows))
            this.isLockedAccountingRows = false;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;

            // Mandatory fields
            if (this.invoice) {
                if (!this.selectedSupplier)
                    mandatoryFieldKeys.push("economy.supplier.supplier.supplier");
                if (!this.invoice.invoiceNr)
                    mandatoryFieldKeys.push("economy.supplier.invoice.invoicenr");
                if (!this.invoice.vatType)
                    mandatoryFieldKeys.push("economy.supplier.invoice.vattype");
                if (!this.invoice.invoiceDate)
                    mandatoryFieldKeys.push("economy.supplier.invoice.invoicedate");
                if (!this.invoice.dueDate)
                    mandatoryFieldKeys.push("economy.supplier.invoice.duedate");
                if (!this.invoice.voucherDate)
                    mandatoryFieldKeys.push("economy.supplier.invoice.voucherdate");
            }

            if (errors['totalAmount'])
                mandatoryFieldKeys.push("economy.supplier.invoice.total");
            if (errors['vatAmount'])
                mandatoryFieldKeys.push("economy.supplier.invoice.vat");
            if (errors['voucherSeries'])
                mandatoryFieldKeys.push("economy.supplier.invoice.voucherseries");

            // Dates
            if (errors['invoiceDate'])
                validationErrorKeys.push("economy.supplier.invoice.errorinvoicedate");
            if (errors['dueDate'])
                validationErrorKeys.push("economy.supplier.invoice.errorduedate");
            if (errors['voucherDate'])
                validationErrorKeys.push("economy.supplier.invoice.errorvoucherdate");

            // billing type not valid in relation to invoice total amount
            if (errors['billingType'])
                validationErrorKeys.push("economy.supplier.invoice.falsebillingtypemessage");

            // Account period missing
            if (errors['accountPeriod'])
                validationErrorKeys.push("economy.supplier.invoice.missingaccountperiod");

            // Confirm accounting
            if (errors['confirmAccounting'])
                validationErrorKeys.push("economy.supplier.invoice.mustconfirmaccounting");

            // Accounting row validation
            if (errors['accountStandard'])
                validationErrorKeys.push("economy.accounting.voucher.accountstandardmissing");
            if (errors['accountInternal'])
                validationErrorKeys.push("economy.accounting.voucher.accountinternalmissing");
            if (errors['rowAmount'])
                validationErrorKeys.push("economy.accounting.voucher.invalidrowamount");
            if (errors['amountDiff'])
                validationErrorKeys.push("economy.accounting.voucher.unbalancedrows");

            if (errors['linkToOrderOrderSet'])
                validationErrorKeys.push("economy.supplier.invoice.missingorder");
            if (errors['linkToProjectProjectSet'])
                validationErrorKeys.push("economy.supplier.invoice.missingproject");
            if (errors['linkToProjectTimeCodeSet'])
                validationErrorKeys.push("economy.supplier.invoice.missingtimecode");
            if (errors['validFiOcr'])
                validationErrorKeys.push("economy.supplier.invoice.invalidocr");
        });
    }

    private skipVatNotZeroValidation(): boolean {
        return true;
        //return this.invoice.vatType !== TermGroup_InvoiceVatType.Merchandise;
    }

    public isDisabled() {
        if (this.isLocked && !this.canEditLocked && this.isLockedAccountingRows)
            return true;
        else
            return !this.dirtyHandler.isDirty || this.edit.$invalid;
    }

    private showDeleteButton(): boolean {
        return this.modifyPermission && !this.isNew && this.invoice && this.invoice.originStatus === SoeOriginStatus.Draft;
    }

    private showCreditButton(): boolean {
        return this.modifyPermission && !this.isNew && this.invoice.billingType !== TermGroup_BillingType.Credit && (this.invoice.originStatus === SoeOriginStatus.Origin || this.invoice.originStatus === SoeOriginStatus.Voucher);
    }

    private showRevokeButton(): boolean {
        return this.modifyPermission && !this.isNew && this.invoice && this.invoice.originStatus !== SoeOriginStatus.Draft && this.invoice.originStatus !== SoeOriginStatus.Voucher && this.invoice.originStatus !== SoeOriginStatus.Payment;
    }

    private showFinvoiceButton(): boolean {
        return (this.modifyPermission && !this.isNew && this.invoice && this.ediEntryId > 0 && this.ediType == TermGroup_EDISourceType.Finvoice);
    }

    private evaluateInvoiceUpsertEventState(callNewInvoice: boolean) {
        return (this.isNew && !callNewInvoice) ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED;
    }

    private showAttestDialog() {
        const keys: string[] = [
            "economy.supplier.invoice.senttoattest",
        ];

        const dict: any = [];
        if (this.supplierInvoiceId !== null && this.supplierInvoiceId !== 0)
            dict.push(this.supplierInvoiceId);

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/AddInvoiceToAttestFlow", "addinvoicetoattestflow.html"),
                controller: AddInvoiceToAttestFlowController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    selectedSupplierInvoiceIds: () => { return dict },
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    addInvoiceToAttestFlowService: () => { return this.addInvoiceToAttestFlowService },
                    defaultAttestGroupId: () => { return this.defaultAttestGroup },
                    highestAmount: () => { return this.invoice.totalAmount }
                }
            });

            if (dict.length === 0) {
                modal.result.then(attestWorkflowHead => {
                    if (attestWorkflowHead) {
                        this.saveAttest = true;
                        this.attestWorkFlowHead = attestWorkflowHead;
                        this.save(false, false, this.defaultAttestType);
                    }
                }, function () {

                });
            }
            else {
                modal.result.then(ids => {
                    if (ids) {
                        this.notificationService.showDialog("", terms["economy.supplier.invoice.senttoattest"].format(ids.length), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                        this.loadAttestWorkFlowHeadFromInvoiceId(this.supplierInvoiceId);
                    }
                }, function () {

                });
            }
        });
    }

    private transferAdjustedAttestRows() {
        // Check if BillingType is Debit or Credit
        const isCredit: boolean = (this.invoice.billingType === TermGroup_BillingType.Credit);
        var isInterim: boolean = this.invoice.interimInvoice;

        var nbrOfCreditRows: number = _.filter(this.invoice.accountingRows, r => r.state === SoeEntityState.Active && !r.isVatRow && !r.isCreditRow).length;
        var nbrOfDebitRows: number = _.filter(this.invoice.accountingRows, r => r.state === SoeEntityState.Active && !r.isVatRow && r.isCreditRow).length;

        var totalAdjustedAmountCurrency: number = _.sumBy(_.filter(this.invoice.supplierInvoiceAttestRows, r => r.state === SoeEntityState.Active && r.attestStatus == SupplierInvoiceAccountRowAttestStatus.New), r => r.amountCurrency);
        var adjustAmount: number = 0;

        // Create row that balance the adjusted rows
        if ((nbrOfCreditRows > 1 && !isCredit) || (nbrOfDebitRows > 1 && isCredit) || isInterim) {
            var interimAccountId: number = this.getAccountId(SupplierAccountType.Interim, 1);

            _.forEach(_.filter(this.invoice.accountingRows, r => r.state === SoeEntityState.Active && !r.isVatRow), row => {
                if (row.dim1Id === interimAccountId) {
                    var rowAmountCurrency: number = row.amountCurrency;
                    if (isCredit) {
                        // Make sure its negative
                        rowAmountCurrency = -(Math.abs(rowAmountCurrency));
                    }

                    this.createAccountingRow(SupplierAccountType.Credit, interimAccountId, rowAmountCurrency, false, false, false);

                    if (isCredit)
                        adjustAmount = totalAdjustedAmountCurrency - rowAmountCurrency;
                    else
                        adjustAmount = rowAmountCurrency - totalAdjustedAmountCurrency;

                    if (adjustAmount !== 0)
                        this.createAccountingRow(SupplierAccountType.Debit, this.defaultAttestRowDebitAccountId, adjustAmount, adjustAmount > 0, false, false);
                }
            });
        } else {
            var row = _.find(this.invoice.accountingRows, r => r.state === SoeEntityState.Active && !r.isVatRow && !r.isCreditRow);
            if (row)
                this.createAccountingRow(SupplierAccountType.Credit, row.dim1Id, totalAdjustedAmountCurrency, false, false, false);
        }

        // Copy attest rows to accounting rows
        _.forEach(_.filter(this.invoice.supplierInvoiceAttestRows, r => r.state === SoeEntityState.Active && r.attestStatus == SupplierInvoiceAccountRowAttestStatus.New), attestRow => {
            // Clone row and change type to accounting row
            var clonedRow = _.cloneDeep(attestRow);
            clonedRow.type = AccountingRowType.AccountingRow;
            clonedRow.rowNr = AccountingRowDTO.getNextRowNr(this.invoice.accountingRows);
            // Clear ID's to add rows on save
            clonedRow.invoiceRowId = 0;
            clonedRow.invoiceAccountRowId = 0;
            this.$timeout(() => {
                this.$scope.$broadcast('setRowItemAccountsOnRow', clonedRow);
                this.$scope.$broadcast('rowAdded', clonedRow, 2);
            });
            this.invoice.accountingRows.push(clonedRow);

            // Mark attest row as processed
            attestRow.attestStatus = SupplierInvoiceAccountRowAttestStatus.Processed;
            attestRow.isProcessed = true;
            attestRow.isModified = true;
        });

        this.attestRowsTransferred = true;
        this.isLocked = false;
        this.dirtyHandler.isDirty = true;

        const keys: string[] = [
            "economy.supplier.invoice.attestaccountingrowstransferred.title",
            "economy.supplier.invoice.attestaccountingrowstransferred.row1",
            "economy.supplier.invoice.attestaccountingrowstransferred.row2"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["economy.supplier.invoice.attestaccountingrowstransferred.title"], terms["economy.supplier.invoice.attestaccountingrowstransferred.row1"] + "\n\n" + terms["economy.supplier.invoice.attestaccountingrowstransferred.row2"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });

        this.ignoreAskUnbalanced = true;
    }

    // Check if specified invoice number already exists on selected actor (supplier/customer)
    private checkIfSupplierInvoiceNumberExist(invoiceNr: string) {

        if (!this.supplier || !this.supplier.actorSupplierId || !invoiceNr)
            return;

        this.supplierService.checkIfInvoiceNumberAlreadyExist(this.supplier.actorSupplierId, this.supplierInvoiceId, invoiceNr).then((result) => {
            if (result.success) {
                const keys: string[] = [
                    "core.warning",
                    "economy.supplier.invoice.invoicenumberalreadyexist"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    var message = terms["economy.supplier.invoice.invoicenumberalreadyexist"].format(result.integerValue);
                    var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
                });
            }
        });
    }

    private showOrderDialog() {
        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            var invoice = this.invoice.orderNr ? _.find(this.customerInvoices, { 'invoiceNr': this.invoice.orderNr }) : undefined;
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomerInvoice", "selectcustomerinvoice.html"),
                controller: SelectCustomerInvoiceController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => { return term },
                    isNew: () => { return this.isNew },
                    ignoreChildren: () => { return false },
                    originType: () => { return SoeOriginType.Order },
                    customerId: () => { return null },
                    projectId: () => { return this.selectedProject ? this.selectedProject.id : undefined },
                    invoiceId: () => { return null },
                    selectedProjectName: () => { return this.selectedProjectName.trim() ? this.selectedProjectName : undefined },
                    currentMainInvoiceId: () => { return this.selectedCustomerInvoice ? this.selectedCustomerInvoice.id : null },
                    userId: () => { return null },
                    includePreliminary: () => { return null },
                    includeVoucher: () => { return null },
                    fullyPaid: () => { return null },
                    useExternalInvoiceNr: () => { return null },
                    importRow: () => { return null },
                }
            });

            modal.result.then(result => {
                if (result) {
                    if (result.remove) {
                        this.selectedCustomerInvoice = undefined;
                    }
                    else if (result.invoice) {
                        if (!this.costAllocationRowsLoaded) {
                            this.costAllocationExpanderOpen = (result.invoice.projectId && result.invoice.projectId > 0);
                            this.loadCostAllocationRows(result.invoice.projectId && result.invoice.projectId > 0 ? { projectId: result.invoice.projectId, name: result.invoice.projectName, number: result.invoice.projectNr } : undefined, result);
                        }
                        else {
                            this.setOrderSelected(result, false);
                        }
                        /*if (!this.orderRowsLoaded && !this.isNew) {
                            if (!this.projectRowsLoaded) {
                                if (result.invoice.projectId && result.invoice.projectId > 0) {
                                    this.blockProjectLoad = true;
                                    this.projectRowsExpanderOpen = true;
                                    this.loadProjectRows({ projectId: result.invoice.projectId, name: result.invoice.projectName, number: result.invoice.projectNr });
                                }

                                this.blockOrderLoad = true;
                                this.loadOrderRows(result, true);
                            }
                            else {
                                this.invoice.supplierInvoiceOrderRows = [];
                                this.setOrderSelected(result, false);
                            }
                        }
                        else {
                            if (!this.projectRowsLoaded && result.invoice.projectId && result.invoice.projectId > 0) {
                                this.blockProjectLoad = true;
                                this.projectRowsExpanderOpen = true;
                                this.loadProjectRows({ projectId: result.invoice.projectId, name: result.invoice.projectName, number: result.invoice.projectNr });
                            }

                            this.setOrderSelected(result, (!this.projectRowsLoaded && !this.isNew));
                        }*/
                    }

                    this.dirtyHandler.setDirty();
                }
            });
        });
    }

    private setOrderSelected(result, ignoreSetProject) {
        if (this.selectedCustomerInvoice && this.selectedCustomerInvoice.id != result.invoice.customerInvoiceId)
            this.previousOrderId = this.selectedCustomerInvoice.id;

        this.selectedCustomerInvoice = { id: result.invoice.customerInvoiceId, name: result.invoice.number + " " + result.invoice.customerName, number: result.invoice.number, projectId: result.invoice.projectId };
        this.changeCostAllocationRows(this.invoice.supplierInvoiceCostAllocationRows);
        if (!ignoreSetProject && result.invoice.projectId && result.invoice.projectId > 0) {

            if (this.selectedProject && this.selectedProject.id != result.invoice.projectId)
                this.previousProjectId = this.selectedProject.id;

            this.selectedProject = { id: result.invoice.projectId, name: result.invoice.projectNr + " " + result.invoice.projectName, number: result.invoice.projectNr };
            this.changeCostAllocationRows(this.invoice.supplierInvoiceCostAllocationRows);
        }
    }

    private showProjectDialog() {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectProject/Views/selectproject.html"),
            controller: SelectProjectController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                projects: () => { return null },
                customerId: () => { return null },
                projectsWithoutCustomer: () => { return null },
                showAllProjects: () => { return false},
                showFindHidden: () => { return true },
                loadHidden: () => { return this.selectedProject ? true : null },
                useDelete: () => { return true },
                currentProjectNr: () => { return this.selectedProject ? this.selectedProject.number : null },
                currentProjectId: () => { return this.selectedProject ? this.selectedProject.id : null },
                excludedProjectId: () => { return null },
            }
        });

        modal.result.then((result) => {
            if (result) {
                if (result.remove) {
                    this.selectedProject = undefined;
                }
                else {
                    if (!this.costAllocationRowsLoaded/*this.projectRowsLoaded*/) {
                        if (this.invoice && this.invoice.invoiceId) {
                            this.ignoreLoadCostAllocationRows = true;
                            this.costAllocationExpanderOpen = true;
                            this.setProjectSelected(result);
                            this.loadCostAllocationRows(result, undefined);
                        }
                        else {
                            this.costAllocationExpanderOpen = true;
                            this.invoice.supplierInvoiceCostAllocationRows = [];
                            this.setProjectSelected(result);
                        }
                    }
                    else
                        this.setProjectSelected(result);
                }

                this.dirtyHandler.setDirty();
            }
        });
    }

    private showInterpretationStatusDialog() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Dialogs/ScanningInformation/Views/ScanningINformation.html"),
            controller: ScanningInformationController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                interpretation: () => { return this.invoiceInterpretation }
            }
        });

    }

    private setProjectSelected(result) {
        if (this.selectedProject && this.selectedProject.id != result.projectid)
            this.previousProjectId = this.selectedProject.id;
            
        this.selectedProject = { id: result.projectId, name: result.number + " " + result.name, number: result.number };
        this.changeCostAllocationRows(this.invoice.supplierInvoiceCostAllocationRows);
        if (this.selectedCustomerInvoice && this.selectedCustomerInvoice.projectId !== result.projectId) {
            this.previousOrderId = this.selectedCustomerInvoice.id;
            this._selectedCustomerInvoice = undefined;
        }
    }

    private changeCostAllocationRows(costAllocationRows: any[]) {
        costAllocationRows.forEach(row => {
            if (this.previousOrderId == row.orderId) {
                row.orderId = this.selectedCustomerInvoice?.id ?? row.orderId;
                row.orderNr = this.selectedCustomerInvoice?.number ?? row.orderNr;
                row.customerInvoiceNumberName = this.selectedCustomerInvoice?.name ?? row.customerInvoiceNumberName;
                row.isModified = true;
            }

            if (this.previousProjectId == row.projectId) {
                row.projectId = this.selectedProject?.id ?? row.projectId;
                row.projectNr = this.selectedProject.number ?? row.ProjectNr;
                row.projectName = this.selectedProject.name ?? row.ProjectName;
                row.isModified = true;
            }
        })

        this.invoice.supplierInvoiceCostAllocationRows = [...costAllocationRows];
    }

    private closeModal(success: boolean, id: number) {
        if (success) {
            this.modal.close(id);
        }
        else {
            this.modal.dismiss();
        }
    }

    private checkWidth() {
        var width = $("#supplier_invoice_edit").not(":hidden").width()
        if (width < 960) {
            this.imageAccordion = true;
        }
        else {
            this.imageAccordion = false;
        }
    }

    private setWidth(width = this.widthRatio) {
        this.checkWidth()
        if (!this.invoiceHasImage || this.imageAccordion) {
            width = 12;
        }

        if (width > 12) {
            width = 12;
        }
        else if (width < 0) {
            width = 0;
        }

        if (width == 0)
            this.invoiceWidthClass = "hide"
        else
            this.invoiceWidthClass = "col-sm-" + width

        if (width == 12)
            this.imageWidthClass = "hide"
        else
            this.imageWidthClass = "col-sm-" + (12 - width)
    }

    private updateHeight(timeout = 0) {
        this.$timeout(() => {
            this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW,
                { id: 'supplier_invoice_edit' });
        }, timeout)

    }
}

export class EditInvoiceSeqNrDialogController {

    public result: any = {};
    private newSeqNr: number;

    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, newSeqNr: number) {
        this.newSeqNr = newSeqNr;
    }

    public cancel() {
        this.result.newSeqNr = null;
        this.$uibModalInstance.close();
    }

    public ok() {
        this.result.newSeqNr = this.newSeqNr;
        this.$uibModalInstance.close(this.result);
    }
}