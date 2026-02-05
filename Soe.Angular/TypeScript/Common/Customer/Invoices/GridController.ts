import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ICommonCustomerService } from "../CommonCustomerService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { InvoiceUtility } from "../../../Util/InvoiceUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IActionResult, ICustomerInvoicePrintDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CustomerInvoiceGridDTO } from "../../Models/InvoiceDTO";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, CustomerInvoiceGridButtonFunctions, SOEMessageBoxButton } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../Models/smallgenerictype";
import { SelectReportController } from "../../Dialogs/SelectReport/SelectReportController";
import { SelectEmailController } from "../../Dialogs/SelectEmail/SelectEmailController";
import { EditController as CustomerPaymentsEditController } from "../Payments/EditController";
import { SoeOriginType, SoeOriginStatusChange, SoeOriginStatusClassification, SoeModule, Feature, TermGroup, TermGroup_BillingType, InvoiceRowInfoFlag, SoeInvoiceRowType, SoeInvoiceRowDiscountType, SoeStatusIcon, SettingMainType, UserSettingType, CompanySettingType, SoeReportTemplateType, SoeOriginStatus, SoeInvoiceExportStatusType, TermGroup_AttestEntity, OrderInvoiceRegistrationType, SoePaymentStatus, SoeInvoiceReminderHandlingType, TermGroup_InvoiceClaimLevel, SoeInvoiceDeliveryType, TermGroup_EInvoiceFormat, SoeInvoiceInterestHandlingType, SoeEntityState, TermGroup_EDIStatus, TermGroup_OrderType, SoeOriginInvoiceMappingType, TermGroup_TimeScheduleTemplateBlockType, EmailTemplateType, TermGroup_HouseHoldTaxDeductionType, SoeInvoiceDeliveryProvider, TermGroup_EDIOrderStatus, TermGroup_EDistributionStatusType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { GridController as EdiGridController } from "../../../Shared/Billing/Import/Edi/GridController";
import { ImportService } from "../../../Shared/Billing/Import/ImportService";
import { NumberUtility } from "../../../Util/NumberUtility";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SaveAsDefinitiveController } from "./Dialogs/SaveAsDefinitive/SaveAsDefinitiveController";
import { UpdateContractPricesController } from "./Dialogs/UpdateContractPrices/UpdateContractPricesController";
import { GridController as ContactPersonsGridController } from "../../../Common/Directives/ContactPersons/ContactPersonsExtended"
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { InvoiceDistributionResultController } from "./Dialogs/InvoiceDistributionResult/InvoiceDisitributionResultController";
import { EditController as OrderEditController } from "../../../Shared/Billing/Orders/EditController";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups 
    allItemsSelectionDict: any[];
    invoiceTypes: any[];
    invoiceBillingTypes: any[];
    orderTypes: any[];
    orderTypesDict: any[];
    invoiceDeliveryTypes: any[];
    originStatus: any[];
    exportStatus: any[];
    paymentStatus: any[];
    paymentMethods: any[];
    customers: ISmallGenericType[];
    attestStatesFull = [];
    attestStates = [];
    accountDims: any[];
    items: CustomerInvoiceGridDTO[];
    shiftTypes: any[];
    isCashSalesStates: any[];

    //Terms
    terms: { [index: string]: string; };
    loadOpenTermKey: string;

    // Config
    currentAccountYearId = 0;
    currentAccountYearFromDate: Date;
    currentAccountYearToDate: Date;

    //Transfer
    buttonOption: any;
    ignoreDateValidation = false;
    ignoreEinvoiceRedownloadValidation = false;

    // Variables
    private module;
    private feature;
    private classification;
    originType: SoeOriginType;
    lookups: number;
    showOnlyMine: boolean;
    showActive: boolean;
    hideOpen = false;
    hideClosed = false;
    hideAllItemsSelection = false;
    openIsLoaded: boolean;
    closedIsLoaded: boolean;
    hasCurrencyPermission: boolean;
    hasPaymentEditPermission: boolean;
    hasDraftToOriginPermission: boolean;
    hasOriginToVoucherPermission: boolean;
    hasOriginToPaymentPermission: boolean;
    hasPaymentToVoucherPermission: boolean;
    hasReportPermission: boolean;
    hasExportPermission: boolean;
    hasSOPExportPermission: boolean;
    hasFortnoxExportPermission = false;
    hasZetesExportPermission = false;
    hasVismaEAccountingPermission = false;
    hasDIExportPermission: boolean;
    hasUniExportPermission: boolean;
    hasDnBExportPermission: boolean;
    private hasSendEInvoicePermission = false;
    private hasDownloadEInvoicePermission = false;
    hasDeletePermission: boolean;
    hasProductSalesPricePermission: boolean;
    hasPlanningPermission: boolean;
    hasEdiPermission: boolean;
    hasEditPermission: boolean;
    hasOrderEditPermission: boolean;

    protected setupComplete: boolean;
    protected hasOpenPermission: boolean;
    protected hasClosedPermission: boolean;
    protected hasIntrestReminderPermission: boolean;
    protected hasTransferToPreliminaryPermission: boolean;
    protected hasTransferToInvoiceAndMergePermission: boolean;
    protected hasTransferToInvoiceAndPrintPermission: boolean;
    hasOfferToOrderPermission: boolean;
    hasOfferToInvoicePermission: boolean;
    hasContractToOrderPermission: boolean;
    hasContractToInvoicePermission: boolean;
    selectedInvoiceDate: Date = null;
    selectedDueDate: Date = null;
    selectedPayDate: Date = null;
    selectedVoucherDate: Date = null;
    filteredTotal = 0;
    selectedTotal = 0;
    filteredTotalExVat = 0;
    selectedTotalIncVat = 0;
    filteredTotalIncVat = 0;
    selectedTotalExVat = 0;
    filteredPaid = 0;
    selectedPaid = 0;
    filteredToPay = 0;
    selectedToPay = 0;
    filteredToBeInvoicedTotal = 0;
    selectedToBeInvoicedTotal = 0;
    filteredToBeInvoicedTotalExVat = 0;
    selectedToBeInvoicedTotalIncVat = 0;
    filteredToBeInvoicedTotalIncVat = 0;
    selectedToBeInvoicedTotalExVat = 0;
    selectedYearly = 0;
    filteredYearly = 0;
    selectedYearlyIncVat = 0;
    filteredYearlyIncVat = 0;
    selectedYearlyExVat = 0;
    filteredYearlyExVat = 0;
    showVatFree = false;
    includeInternalOrders = false;
    isInvoice = false;
    isOrder = false;
    isOffer = false;
    isContract = false;
    showUpdatePrices = false;
    hideFunctionsButton = false;

    //GUI flags
    showSplitButton = false;
    showPaymentMethod = false;
    showPayDate = false;
    showPaymentInformation = false;
    hideAutogiroVisibility = false;
    showToPayTotals = false;
    showPaidTotals = false;
    showTotals = false;
    showYearlyTotals = false;
    isProjectCentral = false;
    isHandleBilling = false;
    showUnpaid = false;
    onlyMineLocked = false;

    //Compsetting
    userIdNeededWithTotalAmount = 0;
    totalAmountWhenUserReguired = 0;
    transferAndPrint = false;
    defaultBillingInvoiceReportId = 0;
    reminderNoOfClaimLevels = 0;
    customerDefaultPaymentMethod = 0;
    allItemsSelectionSettingType = 0;
    onlyMineSelectionSettingType = 0;
    autoTransferPaymentToVoucher: boolean;
    autoTransferInvoiceToVoucher: boolean;
    usePartialInvoicingOnOrderRows: boolean;
    defaultVoucherListReportId: number;
    defaultTimeProjectReportId: number;
    defaultBalanceListReportId: number;
    defaultInvoiceJournalReportId: number;
    defaultInterestRateCalculationReportId: number;
    defaultReminderReportId: number;
    defaultInterestReportId: number;
    eInvoiceFormat: number;
    emailTemplateId: number;
    offerEmailTemplateId: number;
    orderEmailTemplateId: number;
    contractEmailTemplateId: number;
    customerPaymentAskPrintVoucherOnTransfer: boolean;
    customerInvoiceAskPrintVoucherOnTransfer: boolean;
    coreBaseCurrency: number = 0;
    addInterestToNextInvoice: boolean;
    addReminderToNextInvoice: boolean;
    useExternalInvoiceNr: boolean;
    inexchangeSendActivated: boolean;
    showVatFreeSettingType = 0;
    useInvoiceDeliveryProvider = false;
    includeOnlyInvoicedTime = false;

    // Export
    exportDataFileName: string;
    exportDataDataStorageId: number;
    exportXMLFileName: string;
    exportXMLDataStorageId: number;
    defaultHandlingType: number;

    //Project central
    projectId: number;
    customerId: number;
    includeChildProjects: boolean;
    orders: number[];
    invoices: number[];
    fromDate: Date;
    toDate: Date;

    //StatusChange
    originStatusChange: SoeOriginStatusChange;

    //Split button label
    splitButtonSelectedOption: {};

    //Edi label
    openEdiPosts: number;
    openEdiPostsLabel: string;

    // Propertiesprivate 
    _loadActive = false;
    get loadActive() {
        return this._loadActive;
    }
    set loadActive(item: boolean) {
        this._loadActive = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _loadOpen = false;
    get loadOpen() {
        return this._loadOpen;
    }
    set loadOpen(item: boolean) {
        this._loadOpen = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _loadClosed = false;
    get loadClosed() {
        return this._loadClosed;
    }
    set loadClosed(item: boolean) {
        this._loadClosed = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _loadMine = false;
    get loadMine() {
        return this._loadMine;
    }
    set loadMine(item: boolean) {
        this._loadMine = item;
        if (this.setupComplete)
            this.updateOnlyMineSelection();
    }

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete)
            this.updateItemsSelection();
    }

    get showSearchButton() {
        return this.allItemsSelection === 999 && !this.hideAllItemsSelection;
    }

    private _selectedItems: any[];
    get selectedItems() {
        return this._selectedItems;
    }
    set selectedItems(item: any[]) {
        this._selectedItems = item;
        if (this.setupComplete)
            this.updateSelectedItems();
    }

    private _selectedPaymentMethod: any;
    get selectedPaymentMethod() {
        return this._selectedPaymentMethod;
    }
    set selectedPaymentMethod(item: any) {
        this._selectedPaymentMethod = item;
    }

    get showInvoiceDate() {
        return (this.classification === SoeOriginStatusClassification.CustomerInvoicesAll || this.classification === SoeOriginStatusClassification.ContractsRunning);
    }

    get showDueDate() {
        return this.classification === SoeOriginStatusClassification.CustomerInvoicesAll;
    }

    get showVoucherDate() {
        return this.classification === SoeOriginStatusClassification.CustomerInvoicesAll;
    }

    // Functions
    buttonFunctions: any = [];

    // Grid header and footer
    toolbarInclude: any;
    gridFooterComponentUrl: any;

    //modal
    private modalInstance: any;

    private activated = false;
    private doReload = false;

    private isCustomerBalanceListPrinting = false;
    private isCustomerInvoiceJournalPrinting = false;

    //@ngInject
    constructor(
        private $window,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private accountingService: IAccountingService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private importService: ImportService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private readonly requestReportService: IRequestReportService,) {

        super(gridHandlerFactory, "Common.Customer.Invoices" + "_" + soeConfig.feature, progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadSettings(() => this.doLoadSettings())
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.modalInstance = $uibModal;

        this.onTabActivetedAndModified(() => this.loadGridData());
        this.onTabActivated(() => this.localOnTabActivated());
        this.setupComplete = false;
        this.showOnlyMine = false;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;
        this.classification = this.parameters.classification || 0;
        this.idProperty = 'invoiceId';
        this.init();

        //if (this.isHomeTab) {
        //    this.messagingHandler.onGridDataReloadRequired(x => {
        //        if (this.setupComplete)
        //            this.loadGridData();
        //    });
        //}
    }

    private localOnTabActivated() {
        if (!this.activated) {
            this.flowHandler.start(this.getPermissions());
            this.activated = true;
        }
    }

    private getPermissions(): any[] {
        const features: any[] = [];

        features.push({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });

        switch (this.originType) {
            case SoeOriginType.Offer:
                features.push({ feature: Feature.Billing_Offer_OffersAll, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Offer_OffersUser, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Offer_Offers, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Offer_Status_OfferToOrder, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Offer_Status_OfferToInvoice, loadReadPermissions: true, loadModifyPermissions: false });
                break;
            case SoeOriginType.Order:
                features.push({ feature: Feature.Billing_Order_OrdersAll, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Order_OrdersUser, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Order_Orders, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Order_Status_OrderToInvoice, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Order_Planning, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
            case SoeOriginType.CustomerInvoice:
                if (this.module === SoeModule.Economy) {
                    features.push({ feature: Feature.Economy_Customer_Invoice_Invoices_All, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Economy_Customer_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: false });
                }
                else {
                    features.push({ feature: Feature.Billing_Invoice_InvoicesAll, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Billing_Invoice_InvoicesUser, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Billing_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Billing_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                    features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_ExportZetes, loadReadPermissions: true, loadModifyPermissions: true });
                }
                break;
            case SoeOriginType.CustomerPayment:
                if (this.module === SoeModule.Economy) {
                    features.push({ feature: Feature.Economy_Customer_Payment_Payments, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest, loadReadPermissions: true, loadModifyPermissions: false });
                }
                else {
                    features.push({ feature: Feature.Billing_Invoice_InvoicesAll, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Billing_Invoice_InvoicesUser, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Billing_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest, loadReadPermissions: true, loadModifyPermissions: false });
                    features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_ExportZetes, loadReadPermissions: true, loadModifyPermissions: true });
                }
                break;
            case SoeOriginType.Contract:
                features.push({ feature: Feature.Billing_Contract_ContractsUser, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Contract_Contracts, loadReadPermissions: true, loadModifyPermissions: false });
                features.push({ feature: Feature.Billing_Contract_Status_ContractToOrder, loadReadPermissions: true, loadModifyPermissions: true });
                features.push({ feature: Feature.Billing_Contract_Status_ContractToInvoice, loadReadPermissions: true, loadModifyPermissions: true });
                features.push({ feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true });
                break;
        }

        features.push({ feature: Feature.Economy_Customer_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Invoices_All, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Status_OriginToPayment, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Status_OriginToPayment, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Status_OriginToVoucher, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Status_PayedToVoucher, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Status_Foreign, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_ExportSOP, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_ExportFortnox, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_ExportVismaEAccounting, loadReadPermissions: true, loadModifyPermissions: true });

        features.push({ feature: Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_InvoicesAll, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice, loadReadPermissions: true, loadModifyPermissions: true });

        features.push({ feature: Feature.Economy_Customer_Payment_Payments_Edit, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Invoices_Edit_Delete, loadReadPermissions: true, loadModifyPermissions: true });
        //Reports 
        features.push({ feature: Feature.Economy_Distribution_Reports_Selection, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Distribution_Reports_Selection_Download, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Distribution_Reports_Selection, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Distribution_Reports_Selection_Download, loadReadPermissions: true, loadModifyPermissions: true });
        //Offer
        features.push({ feature: Feature.Billing_Offer_Status_OfferToOrder, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Offer_Status_OfferToInvoice, loadReadPermissions: true, loadModifyPermissions: true });
        //Order
        features.push({ feature: Feature.Billing_Order_Status_OrderToInvoice, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Order_ShowOnMap, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Product_Products_ShowSalesPrice, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Order_Orders_Edit_Delete, loadReadPermissions: true, loadModifyPermissions: true });
        //Edi
        features.push({ feature: Feature.Billing_Import_EDI, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Import_EDI_All, loadReadPermissions: true, loadModifyPermissions: true });

        return features;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[soeConfig.feature].readPermission;
        this.modifyPermission = response[soeConfig.feature].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();

        //Read only permissions
        switch (this.originType) {
            case SoeOriginType.Offer:
                if (response[Feature.Billing_Offer_OffersAll] || response[Feature.Billing_Offer_Offers]/* || response[Feature.Billing_Offer_Status_OfferToOrder] || response[Feature.Billing_Offer_Status_OfferToInvoice]*/) {
                    this.loadOpen = true;
                    this.hasOpenPermission = true;
                    this.hasClosedPermission = true;
                    this.showOnlyMine = true;
                }
                else if (response[Feature.Billing_Offer_OffersUser]) {
                    this.loadOpen = true;
                    this.showOnlyMine = true;
                    this.loadMine = true;
                    this.onlyMineLocked = true;
                }

                this.loadMine = this.onlyMineLocked = response[Feature.Billing_Offer_OffersUser].readPermission;
                break;
            case SoeOriginType.Order:
                if (response[Feature.Billing_Order_OrdersAll] || response[Feature.Billing_Order_Orders]/* || response[Feature.Billing_Order_Status_OrderToInvoice]*/) {
                    this.loadOpen = true;
                    this.hasOpenPermission = true;
                    this.hasClosedPermission = true;
                    this.showOnlyMine = true;
                }
                else if (response[Feature.Billing_Order_OrdersUser]) {
                    this.loadOpen = true;
                    this.hasOpenPermission = true;
                    this.hasClosedPermission = true;
                    this.showOnlyMine = true;
                }
                this.loadMine = this.onlyMineLocked = response[Feature.Billing_Order_OrdersUser].readPermission;

                this.hasPlanningPermission = response[Feature.Billing_Order_Planning].readPermission;
                this.hasDeletePermission = response[Feature.Billing_Order_Orders_Edit_Delete].modifyPermission;
                break;
            case SoeOriginType.CustomerInvoice:
                if (this.module === SoeModule.Economy) {
                    if (response[Feature.Economy_Customer_Invoice_Invoices_All] || response[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest] || response[Feature.Economy_Customer_Invoice_Invoices]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        this.showOnlyMine = true;
                    }
                }
                else {
                    if (response[Feature.Billing_Invoice_InvoicesAll] || response[Feature.Billing_Invoice_Invoices] || response[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        this.showOnlyMine = true;
                    }
                    this.loadMine = this.onlyMineLocked = response[Feature.Billing_Invoice_InvoicesUser].readPermission;
                }

                this.hasDeletePermission = response[Feature.Billing_Invoice_Invoices_Edit_Delete].modifyPermission;
                break;
            case SoeOriginType.CustomerPayment:
                if (this.module === SoeModule.Economy) {
                    if (response[Feature.Economy_Customer_Invoice_Invoices_All] || response[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest] || response[Feature.Economy_Customer_Invoice_Invoices]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                    }
                }
                else {
                    if (response[Feature.Billing_Invoice_InvoicesAll] || response[Feature.Billing_Invoice_Invoices]/* || response[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest]*/) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        if (response[Feature.Billing_Invoice_InvoicesUser]) {
                            this.showOnlyMine = true;
                        }
                    }
                    else if (response[Feature.Billing_Invoice_InvoicesUser]) {
                        this.loadOpen = true;
                        this.showOnlyMine = true;
                        this.loadMine = true;
                        this.onlyMineLocked = true;
                    }
                }
                break;
            case SoeOriginType.Contract:
                if (response[Feature.Billing_Contract_Contracts]) {
                    if (this.classification === SoeOriginStatusClassification.ContractsRunning) {
                        this.loadActive = true;
                        this.loadOpen = false;
                        this.hideOpen = true;
                        this.hideClosed = true;
                    }
                    else {
                        this.loadActive = false;
                        this.loadOpen = true;
                    }
                    this.hasOpenPermission = true;
                    this.hasClosedPermission = true;
                    if (response[Feature.Billing_Contract_ContractsUser]) {
                        this.showOnlyMine = true;
                    }
                }
                else if (response[Feature.Billing_Contract_ContractsUser]) {
                    if (this.classification === SoeOriginStatusClassification.ContractsRunning) {
                        this.loadActive = true;
                        this.loadOpen = false;
                        this.hideOpen = true;
                        this.hideClosed = true;
                        this.showOnlyMine = true;
                        this.loadMine = true;
                        this.onlyMineLocked = true;
                    }
                    else {
                        this.loadActive = false;
                        this.loadOpen = true;
                        this.showOnlyMine = true;
                        this.loadMine = true;
                        this.onlyMineLocked = true;
                    }
                }
                break;

        }

        //Modify permissions
        if (response[Feature.Billing_Order_Orders_Edit]) {
            if (this.originType === SoeOriginType.Order)
                this.hasEditPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
            else
                this.hasOrderEditPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        }

        if (response[Feature.Billing_Invoice_Invoices_Edit])
            this.hasEditPermission = response[Feature.Billing_Invoice_Invoices_Edit].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Status_Foreign])
            this.hasCurrencyPermission = response[Feature.Economy_Customer_Invoice_Status_Foreign].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Status_OriginToPayment] && this.module === SoeModule.Economy)
            this.hasOriginToPaymentPermission = response[Feature.Economy_Customer_Invoice_Status_OriginToPayment].modifyPermission;

        if (response[Feature.Billing_Invoice_Status_OriginToPayment] && this.module === SoeModule.Billing)
            this.hasOriginToPaymentPermission = response[Feature.Billing_Invoice_Status_OriginToPayment].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Status_DraftToOrigin] && this.module === SoeModule.Economy)
            this.hasDraftToOriginPermission = response[Feature.Economy_Customer_Invoice_Status_DraftToOrigin].modifyPermission;
        else if (response[Feature.Billing_Invoice_Status_DraftToOrigin] && this.module === SoeModule.Billing)
            this.hasDraftToOriginPermission = response[Feature.Billing_Invoice_Status_DraftToOrigin].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Status_OriginToVoucher])
            this.hasOriginToVoucherPermission = response[Feature.Economy_Customer_Invoice_Status_OriginToVoucher].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Status_PayedToVoucher] && this.module === SoeModule.Economy)
            this.hasPaymentToVoucherPermission = response[Feature.Economy_Customer_Invoice_Status_PayedToVoucher].modifyPermission;

        if (response[Feature.Economy_Customer_Payment_Payments_Edit])
            this.hasPaymentEditPermission = response[Feature.Economy_Customer_Payment_Payments_Edit].modifyPermission;

        if (this.module === SoeModule.Economy && response[Feature.Economy_Distribution_Reports_Selection].modifyPermission === true && response[Feature.Economy_Distribution_Reports_Selection_Download].modifyPermission === true)
            this.hasReportPermission = true;
        else if (this.module === SoeModule.Billing && response[Feature.Billing_Distribution_Reports_Selection].modifyPermission === true && response[Feature.Billing_Distribution_Reports_Selection_Download].modifyPermission === true)
            this.hasReportPermission = true;

        if (response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP].modifyPermission === true || response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro].modifyPermission === true ||
            response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap].modifyPermission === true || response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor].modifyPermission === true)
            this.hasExportPermission = true;

        if (response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP] && this.module === SoeModule.Economy)
            this.hasSOPExportPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP].modifyPermission;
        else if (response[Feature.Billing_Invoice_Invoices_Edit_ExportSOP] && this.module === SoeModule.Billing)
            this.hasSOPExportPermission = response[Feature.Billing_Invoice_Invoices_Edit_ExportSOP].modifyPermission;

        if (response[Feature.Billing_Invoice_Invoices_Edit_ExportFortnox])
            this.hasFortnoxExportPermission = response[Feature.Billing_Invoice_Invoices_Edit_ExportFortnox].modifyPermission;

        if (response[Feature.Billing_Invoice_Invoices_Edit_ExportZetes])
            this.hasZetesExportPermission = response[Feature.Billing_Invoice_Invoices_Edit_ExportZetes].modifyPermission;

        if (response[Feature.Billing_Invoice_Invoices_Edit_ExportVismaEAccounting])
            this.hasVismaEAccountingPermission = response[Feature.Billing_Invoice_Invoices_Edit_ExportVismaEAccounting].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro])
            this.hasUniExportPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap])
            this.hasDIExportPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap].modifyPermission;

        if (response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor])
            this.hasDnBExportPermission = response[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor].modifyPermission;

        if (response[Feature.Billing_Invoice_Invoices_Edit_EInvoice].modifyPermission === true && (response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura].modifyPermission === true ||
            response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice].modifyPermission === true))
            this.hasSendEInvoicePermission = true;

        if (response[Feature.Billing_Invoice_Invoices_Edit_EInvoice].modifyPermission === true && response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice].modifyPermission === true)
            this.hasDownloadEInvoicePermission = true;

        if (response[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest].modifyPermission === true)
            this.hasIntrestReminderPermission = true;
        if (response[Feature.Billing_Offer_Status_OfferToOrder].modifyPermission === true)
            this.hasOfferToOrderPermission = true;

        if (response[Feature.Billing_Offer_Status_OfferToInvoice].modifyPermission === true)
            this.hasOfferToInvoicePermission = true;

        if (response[Feature.Billing_Order_Status_OrderToInvoice].modifyPermission === true) {
            this.hasTransferToPreliminaryPermission = true;
            this.hasTransferToInvoiceAndMergePermission = true;
        }

        if (response[Feature.Billing_Order_Status_OrderToInvoice].modifyPermission === true && response[Feature.Billing_Invoice_Status_DraftToOrigin].modifyPermission === true) {
            this.hasTransferToInvoiceAndPrintPermission = true;
        }

        if (response[Feature.Billing_Product_Products_ShowSalesPrice].modifyPermission === true) {
            this.hasProductSalesPricePermission = true;
        }

        if (response[Feature.Billing_Import_EDI].modifyPermission === true || response[Feature.Billing_Import_EDI_All].modifyPermission === true)
            this.hasEdiPermission = true;

        if (response[Feature.Billing_Contract_Status_ContractToOrder] && response[Feature.Billing_Contract_Status_ContractToOrder].modifyPermission === true)
            this.hasContractToOrderPermission = true;

        if (response[Feature.Billing_Contract_Status_ContractToInvoice] && response[Feature.Billing_Contract_Status_ContractToInvoice].modifyPermission === true)
            this.hasContractToInvoicePermission = true;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData(true));
        this.toolbar.addInclude(this.toolbarInclude);

        this.setupToolbar();
    }

    private setupToolbar() {
        //Setup toolbar
        if (this.toolbar) {
            if (this.originType === SoeOriginType.CustomerInvoice || this.classification === SoeOriginStatusClassification.CustomerPaymentsPayed || this.classification === SoeOriginStatusClassification.CustomerPaymentsVoucher) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.printbalance", "economy.supplier.invoice.printbalance", IconLibrary.FontAwesome, "fa-print", () => {
                    this.printSelectedInvoicesBalanceList();
                }, () => {
                    return this.gridAg.options.getSelectedCount() === 0
                        || this.isCustomerBalanceListPrinting;
                }, () => { return !(this.defaultBalanceListReportId && this.defaultBalanceListReportId > 0) })));
            }
            if (this.originType === SoeOriginType.CustomerInvoice) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.invoicejournal", "economy.supplier.invoice.invoicejournal", IconLibrary.FontAwesome, "fa-print", () => {
                    this.printSelectedInvoicesInvoiceJournal();
                }, () => {
                    return this.gridAg.options.getSelectedCount() === 0
                        || this.isCustomerInvoiceJournalPrinting;
                }, () => { return !(this.defaultBalanceListReportId && this.defaultBalanceListReportId > 0) })));
            }
            if (this.parameters.isProjectCentral === true && this.originType === SoeOriginType.Order) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.customer.invoices.neworder", "common.customer.invoices.neworder", IconLibrary.FontAwesome, "fa-plus", () => {
                    this.messagingService.publish(Constants.EVENT_NEW_ORDER, { projectId: this.projectId, customerId: this.customerId });
                }, () => {
                })));
            }

            return this.translationService.translate("common.customer.invoices.openediposts").then((term) => {
                this.openEdiPostsLabel = term;
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton(null, "", IconLibrary.FontAwesome, "fa-download", () => {
                    this.openEdiGrid();
                }, () => {

                }, () => {
                    return !(this.hasEdiPermission && this.originType === SoeOriginType.Order && this.parameters.isProjectCentral === !!false);
                }, { labelValue: this.openEdiPostsLabel })));
            });

        }
    }

    public doLoadSettings(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadTerms()
        ]);
    }

    public doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadFunctions(),
            this.loadSelectionTypes(),
            this.loadDeliveryTypes(),
            this.loadInvoiceBillingTypes(),
            this.loadOrderTypes(),
            this.loadOriginStatus(),
            this.loadPaymentStatus(),
            this.loadCurrentAccountYear(),
            this.loadPaymentMethods(),
            this.loadAttestStates(),
            this.loadShiftTypes(),
            this.loadAccountDims(),
            this.loadInvoiceJournalReportId(),
        ]).then(() => {
            this.loadNumberOfOpenEDIPosts();
        });
    }

    public onControllActivated(tabGuid: any) {
        if (tabGuid !== this.guid)
            return;

        if (!this.activated) {
            this.flowHandler.start(this.getPermissions());
            this.activated = true;
        }
        else if (this.doReload) {
            this.loadGridData();
            this.doReload = false;
        }
    }

    public init() {
        if (this.parameters.isProjectCentral && this.parameters.isProjectCentral === true) {
            this.isProjectCentral = true;
            if (this.parameters.classification === SoeOriginStatusClassification.OrdersAll) {
                this.module = SoeModule.Billing;
                this.feature = Feature.Billing_Order_Status;
                this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/gridFooterOrder.html");

                this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
                    this.projectId = x.projectId;
                    this.customerId = x.customerId;
                    this.includeChildProjects = x.includeChildProjects;
                    this.orders = x.orders;
                    this.fromDate = x.fromDate;
                    this.toDate = x.toDate;

                    if (this.activated)
                        this.doReload = true;
                });
            }
            else if (this.parameters.classification === SoeOriginStatusClassification.CustomerInvoicesAll) {
                this.module = SoeModule.Billing;
                this.feature = Feature.Billing_Invoice_Status;
                this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/gridFooter.html");

                this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
                    this.projectId = x.projectId;
                    this.customerId = x.customerId;
                    this.includeChildProjects = x.includeChildProjects;
                    this.invoices = x.customerInvoices;
                    this.fromDate = x.fromDate;
                    this.toDate = x.toDate;

                    if (this.activated)
                        this.doReload = true;
                });
            }

            this.setupTypesAndClassication();
        }
        else if (this.parameters.handleBilling) {
            this.isHandleBilling = true;
            this.module = SoeModule.Billing;
            this.feature = Feature.Billing_Invoice_Status;
            this.toolbarInclude = this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/gridHeader.html");
            this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/gridFooter.html");
            this.setupTypesAndClassication();
        }
        else {
            this.module = soeConfig.module;
            if (this.module === SoeModule.Economy)
                this.toolbarInclude = this.urlHelperService.getViewUrl("gridHeader.html");
            else
                this.toolbarInclude = this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/gridHeader.html");

            this.feature = soeConfig.feature;
            if (this.feature === Feature.Billing_Order_Status)
                this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooterOrder.html");
            else {
                this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
            }

            this.setupTypesAndClassication();
        }

        this.messagingService.subscribe(Constants.EVENT_TAB_ACTIVATED, (x) => {
            this.onControllActivated(x);
        });

        this.$scope.$on('onTabActivated', (e, a) => {
            this.onControllActivated(a);
        });
    }

    private lookupCustomerInvoice() {

        //common
        this.commonCustomerService.getPaymentConditions();
        this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, false, false);

        if (this.module === SoeModule.Economy) {
            this.coreService.getCompCurrenciesSmall();
            this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false);
        }
        else {
            switch (soeConfig.feature) {
                case Feature.Billing_Offer_Status:
                case Feature.Billing_Order_Status:
                case Feature.Billing_Invoice_Status:
                case Feature.Billing_Contract_ContractsAll:
                    this.coreService.getTermGroupContent(TermGroup.OrderType, false, false);
                    this.coreService.getTermGroupContent(TermGroup.OrderContractType, false, false);
                    this.commonCustomerService.getDeliveryTypesDict(true);
                    this.commonCustomerService.getDeliveryConditionsDict(true);
                    this.coreService.getTermGroupContent(TermGroup.InvoicePaymentService, false, false);
                    this.commonCustomerService.getOrderTemplates();
                    this.coreService.getUsersDict(true, false, true, false);
                    this.commonCustomerService.getSysWholesellersDict(true);
                    this.commonCustomerService.getPriceLists();
            }
        }
    }

    public setupGrid() {
        this.gridAg.options.setName("Common.Customer.Invoices" + "_" + soeConfig.feature + "_" + this.classification);

        //Export dropdown
        this.exportStatus = [];
        this.exportStatus.push({ value: 0, label: " " });
        this.exportStatus.push({ value: 1, label: this.terms["common.customer.invoices.export"] });

        //Cash sales dropdown
        this.isCashSalesStates = [];
        this.isCashSalesStates.push({ value: 0, label: this.terms["core.no"] });
        this.isCashSalesStates.push({ value: 1, label: this.terms["core.yes"] });

        var exportFileNameKey: string = "";

        switch (this.classification) {
            case SoeOriginStatusClassification.CustomerPaymentsUnpayed:
                exportFileNameKey = this.setupCustomerPaymentsUnpaidColumns();
                break;
            case SoeOriginStatusClassification.CustomerInvoicesReminder:
                exportFileNameKey = this.setupCustomerInvoicesReminderColumns();
                break;
            case SoeOriginStatusClassification.CustomerInvoicesInterest:
                exportFileNameKey = this.setupCustomerInvoicesInterestColumns();
                break;
            case SoeOriginStatusClassification.CustomerPaymentsPayed:
                exportFileNameKey = this.setupCustomerPaymentsPayedColumns();
                break;
            case SoeOriginStatusClassification.CustomerPaymentsVoucher:
                exportFileNameKey = this.setupCustomerPaymentsVoucherColumns();
                break;
            case SoeOriginStatusClassification.OffersAll:
                exportFileNameKey = this.setupOffersAllColumns();
                break;
            case SoeOriginStatusClassification.OrdersAll:
                exportFileNameKey = this.setupOrdersAllColumns();
                break;
            case SoeOriginStatusClassification.CustomerInvoicesAll:
                exportFileNameKey = this.setupCustomerInvoicesAllColumns();
                break;
            case SoeOriginStatusClassification.ContractsAll:
            case SoeOriginStatusClassification.ContractsRunning:
                exportFileNameKey = this.setupContractsAllColumns();
                break;
            default:
                break;
        }

        if (this.gridAg.detailOptions) {
            this.gridAg.detailOptions.setSingelValueConfiguration([
                { field: "text", predicate: (data) => { return data.type === SoeInvoiceRowType.TextRow; }, editable: false },
                { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.PageBreakRow, editable: false, cellClass: "bold" },
                {
                    field: "text",
                    predicate: (data) => data.type === SoeInvoiceRowType.SubTotalRow,
                    editable: false,
                    cellClass: "bold",
                    cellRenderer: (data, value) => {
                        const sum = data["sumAmountCurrency"] || "";
                        return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                    },
                    spanTo: "sumAmountCurrency"
                },
            ]);
        }

        this.gridAg.options.getColumnDefs()
            .forEach(f => {
                // Append closedRow to cellClass
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any) => {
                    //return cellcls + (row.entity.useClosedStyle ? " closedRow" : "");
                    // Append closedRow to cellClass
                    var newCls: string = cellCls + (grid.data.useClosedStyle ? " closedRow" : "");
                    // Append modifiedCell to cellClass on editable columns
                    if (f.field === 'payAmount') {
                        newCls += (grid.data.payAmountModified ? " modifiedCell" : "");
                    } else if (f.field === 'payAmountCurrency') {
                        newCls += (grid.data.paidAmountModified ? " modifiedCell" : "");
                    } else if (f.field === 'paidAmount') {
                        newCls += (grid.data.paidAmountModified ? " modifiedCell" : "");
                    } else if (f.field === 'payDate') {
                        newCls += (grid.data.payDateModified ? " modifiedCell" : "");
                    } else if (f.field === 'paymentInformationRowId') {
                        newCls += (grid.data.paymentInformationRowIdModified ? " modifiedCell" : "");
                    } else if (f.field === "dueDate") {
                        if (grid.data['isOverdued'] && !grid.data['fullyPaid'])
                            newCls += " errorRow";
                    }

                    return newCls;
                };

                if (f.field === "payDate") {
                    f.editable = (node, column, colDef, context, api, columnApi) => {
                        return (!node.data.useClosedStyle && this.classification !== SoeOriginStatusClassification.CustomerPaymentsVoucher);
                    }
                }

                if (f.field === "payAmount") {
                    f.editable = (node, column, colDef, context, api, columnApi) => {
                        return !node.data.useClosedStyle;
                    }
                }

                if (f.field === "payAmountCurrency") {
                    f.editable = (node, column, colDef, context, api, columnApi) => {
                        return !node.data.useClosedStyle;
                    }
                }
            });

        // Events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
            this.afterCellEdit(entity, colDef, newValue, oldValue);
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: CustomerInvoiceGridDTO) => {
            this.summarizeSelected();
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: CustomerInvoiceGridDTO) => {
            this.summarizeSelected();
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: CustomerInvoiceGridDTO[]) => {
            var items = null;
            if (this.includeInternalOrders || this.originType !== SoeOriginType.Order)
                items = rows;
            else
                items = _.filter(rows, i => i.orderType !== TermGroup_OrderType.Internal);

            this.summarizeFiltered(items);
        }));
        this.gridAg.options.subscribe(events);


        this.gridAg.finalizeInitGrid(exportFileNameKey, true);
    }

    //CustomerPaymentsUnpayed, CustomerInvoicesReminder, CustomerInvoicesInterest, CustomerPaymentsPayed, CustomerPaymentsVoucher, OffersAll, OrdersAll, CustomerInvoicesAll
    private setupCustomerPaymentsUnpaidColumns(): string {
        this.gridAg.addColumnText("seqNr", this.terms["common.customer.invoices.invoiceseqnr"], null, true, { enableHiding: true });
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null, true);
        this.gridAg.addColumnText("ocr", this.terms["common.customer.invoices.ocr"], null, true);
        this.gridAg.addColumnSelect("billingTypeName", this.terms["common.customer.invoices.type"], null, { displayField: "billingTypeName", selectOptions: this.invoiceBillingTypes });
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        if (this.hasExportPermission)
            this.gridAg.addColumnText("exportStatusName", this.terms["common.customer.invoices.export"], null, true);
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true);
        if (this.useExternalInvoiceNr) {
            this.gridAg.addColumnText("externalInvoiceNr", this.terms["billing.invoices.externalinvoicenr"], null, true, { enableHiding: true, hide: false });
        }
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        if (this.hasCurrencyPermission) {
            this.gridAg.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, true);
        }
        var customerInvoicePayAmount = this.gridAg.addColumnNumber("payAmount", this.terms["economy.import.payment.invoicetotalamount"], null, { enableHiding: true, decimals: 2 });
        customerInvoicePayAmount.enableCellEdit = true;
        customerInvoicePayAmount.cellEditableCondition = scope => { return !scope.row.useClosedStyle; };
        if (this.hasCurrencyPermission) {
            var customerInvoicePayAmountCurrency = this.gridAg.addColumnNumber("payAmountCurrency", this.terms["common.customer.invoices.payamountcurrency"], null, { enableHiding: true, decimals: 2 });
            customerInvoicePayAmountCurrency.enableCellEdit = true;
            customerInvoicePayAmountCurrency.cellEditableCondition = scope => { return !scope.row.useClosedStyle; };
        }
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["common.customer.invoices.duedate"], null, true);
        const customerInvoicePayDate = this.gridAg.addColumnDate("payDate", this.terms["economy.supplier.payment.changepaydate"], null, true, null);
        customerInvoicePayDate.enableCellEdit = true;
        customerInvoicePayDate.cellEditableCondition = scope => { return !scope.row.useClosedStyle; };
        if (this.module === SoeModule.Billing)
            this.gridAg.addColumnIcon(null, this.terms["economy.supplier.payment.registerpayment"], null, { icon: "fal fa-plus iconEdit", onClick: this.createPayment.bind(this), showIcon: this.showCreatePayment.bind(this) });
        else
            this.gridAg.addColumnIcon(null, this.terms["economy.supplier.payment.registerpayment"], null, { icon: "fal fa-plus iconEdit", onClick: this.createPayment.bind(this) });
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openInvoice.bind(this) });
        this.gridAg.addColumnIcon("statusIconValue", null, 40, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });
        return "common.customer.invoices.unpaid";
    }

    private setupCustomerInvoicesReminderColumns(): string {
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null);
        this.gridAg.addColumnText("orderNumbers", this.terms["common.customer.invoices.ordernr"], null, true);
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("reminderContactEComText", this.terms["common.emailaddress"], null, true);
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true);
        if (this.useExternalInvoiceNr) {
            this.gridAg.addColumnText("externalInvoiceNr", this.terms["billing.invoices.externalinvoicenr"], null, true, { enableHiding: true, hide: false });
        }
        this.gridAg.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, true);
        this.gridAg.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("paidAmount", this.terms["common.customer.invoices.paidamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnText("noOfRemindersText", this.terms["common.customer.invoices.reminderlevel"], null);
        this.gridAg.addColumnDate("lastCreatedReminder", this.terms["common.customer.invoices.lastcreated"], null, true);
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["common.customer.invoices.duedate"], null, true);
        //Printed icon
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openInvoice.bind(this) });
        this.gridAg.addColumnIcon("statusIconValue", null, 40, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false });
        return "common.customer.invoices.reminder";
    }

    private setupCustomerInvoicesInterestColumns(): string {
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null);
        this.gridAg.addColumnText("orderNumbers", this.terms["common.customer.invoices.ordernr"], null, true);
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true);
        if (this.useExternalInvoiceNr) {
            this.gridAg.addColumnText("externalInvoiceNr", this.terms["billing.invoices.externalinvoicenr"], null, true, { enableHiding: true, hide: false });
        }
        this.gridAg.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, true);
        this.gridAg.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("paidAmount", this.terms["common.customer.invoices.paidamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnText("noOfRemindersText", this.terms["common.customer.invoices.reminderlevel"], null);
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["common.customer.invoices.duedate"], null, true);
        this.gridAg.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null, true);
        //Printed icon
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openInvoice.bind(this) });
        this.gridAg.addColumnIcon("statusIconValue", null, 40, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false });
        return "common.customer.invoices.intrest";
    }

    private setupCustomerPaymentsPayedColumns(): string {
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null, true);
        this.gridAg.addColumnText("ocr", this.terms["common.customer.invoices.ocr"], null, true);
        this.gridAg.addColumnText("seqNr", this.terms["common.customer.invoices.invoiceseqnr"], null, true, { enableHiding: true });
        this.gridAg.addColumnText("paymentSeqNr", this.terms["common.customer.payment.paymentseqnr"], null, true, { enableHiding: true });
        this.gridAg.addColumnText("paymentNrString", this.terms["economy.supplier.payment.paymentnr"], null);
        this.gridAg.addColumnSelect("billingTypeName", this.terms["common.customer.invoices.type"], null, { displayField: "billingTypeName", selectOptions: this.invoiceBillingTypes });
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true);
        if (this.useExternalInvoiceNr) {
            this.gridAg.addColumnText("externalInvoiceNr", this.terms["billing.invoices.externalinvoicenr"], null, true, { enableHiding: true, hide: false });
        }
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        if (this.hasExportPermission)
            this.gridAg.addColumnText("exportStatusName", this.terms["common.customer.invoices.export"], null, true);
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnNumber("paymentAmount", this.terms["common.customer.invoices.paidamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null);
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openPayment.bind(this) });
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-file-search iconEdit", onClick: this.openInvoice.bind(this) });
        return "common.customer.payment.paid";
    }

    private setupCustomerPaymentsVoucherColumns(): string {
        this.gridAg.addColumnText("seqNr", this.terms["common.customer.invoices.invoiceseqnr"], null, true, { enableHiding: true });
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null, true);
        this.gridAg.addColumnText("ocr", this.terms["common.customer.invoices.ocr"], null, true);
        this.gridAg.addColumnText("paymentSeqNr", this.terms["common.customer.payment.paymentseqnr"], null, true, { enableHiding: true });
        this.gridAg.addColumnText("paymentNrString", this.terms["economy.supplier.payment.paymentnr"], null);
        this.gridAg.addColumnSelect("billingTypeName", this.terms["common.customer.invoices.type"], null, { displayField: "billingTypeName", selectOptions: this.invoiceBillingTypes, });
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true);
        if (this.useExternalInvoiceNr) {
            this.gridAg.addColumnText("externalInvoiceNr", this.terms["billing.invoices.externalinvoicenr"], null, true, { enableHiding: true, hide: false });
        }
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        if (this.hasExportPermission)
            this.gridAg.addColumnText("exportStatusName", this.terms["common.customer.invoices.export"], null, true);
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnNumber("paymentAmount", this.terms["common.customer.invoices.paidamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null);
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openPayment.bind(this) });
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-file-search iconEdit", onClick: this.openInvoice.bind(this) });
        return "common.customer.payment.paidvoucher";
    }

    private setupOffersAllColumns(): string {
        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadCustomerInvoiceRows(params);
        });

        this.gridAg.detailOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 50, { enableHiding: true, pinned: "left" });
        this.gridAg.detailOptions.addColumnIcon("rowTypeIcon", null, null, { pinned: "left", editable: false });
        this.gridAg.detailOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.hasProductSalesPricePermission) {
            this.gridAg.detailOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2, maxDecimals: 4 });
            this.gridAg.detailOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.detailOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.gridAg.detailOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }
        this.gridAg.detailOptions.addColumnShape("attestStateColor", null, null, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" });
        this.gridAg.detailOptions.finalizeInitGrid();

        // Master
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.offernr"], null);
        this.gridAg.addColumnSelect("attestStateNames", this.terms["common.customer.invoices.rowstatus"], null, {
            populateFilterFromGrid: true,
            toolTipField: "attestStateNames", displayField: "attestStateNames", selectOptions: this.attestStates, shape: Constants.SHAPE_CIRCLE, shapeValueField: "attestStateColor", useGradient: true, gradientField: "useGradient", colorField: "attestStateColor", enableHiding: true
        });
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("customerCategories", this.terms["common.customer.invoices.customercategories"], null, true);
        this.gridAg.addColumnText("users", this.terms["common.customer.invoices.participant"], null, true, { toolTipField: "users", enableHiding: true });
        this.gridAg.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, true, { toolTipField: "mainUserName", enableHiding: true });
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true, { toolTipField: "internalText" });
        this.gridAg.addColumnText("invoiceLabel", this.terms["common.customer.customer.invoicelabel"], null, true, { toolTipField: "invoiceLabel", enableHiding: true });
        this.gridAg.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, true);
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        if (this.hasCurrencyPermission) {
            this.gridAg.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, true);
        }
        this.gridAg.addColumnText("priceListName", this.terms["common.customer.invoices.pricelisttype"], null, true, { toolTipField: "priceListName", enableHiding: true, hide: true });
        if (this.hasProductSalesPricePermission)
            this.gridAg.addColumnNumber("remainingAmount", this.terms["common.customer.invoices.remainingamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.offerdate"], null);
        this.gridAg.addColumnIcon("billingIconValue", null, null, { showIcon: this.showBillingIcon.bind(this), toolTipField: "billingIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });
        this.gridAg.addColumnIcon("statusIconValue", this.terms["economy.supplier.payment.registerpayment"], 40, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });
        this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.offeredit"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openOffer.bind(this) });

        return "common.customer.invoices.offers";
    }

    private setupOrdersAllColumns(): string {
        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadCustomerInvoiceRows(params);
        });

        this.gridAg.detailOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 50, { enableHiding: true, pinned: "left" });
        this.gridAg.detailOptions.addColumnIcon("rowTypeIcon", null, null, { pinned: "left", editable: false });
        this.gridAg.detailOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.hasProductSalesPricePermission) {
            this.gridAg.detailOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2, maxDecimals: 4 });
            this.gridAg.detailOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.detailOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.gridAg.detailOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }
        this.gridAg.detailOptions.addColumnShape("attestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" });
        this.gridAg.detailOptions.finalizeInitGrid();

        // Master
        this.gridAg.addColumnNumber("invoiceNr", this.terms["common.customer.invoices.ordernr"], null, { alignLeft: true, formatAsText: true });
        this.gridAg.addColumnText("projectNr", this.terms["common.customer.invoices.projectnr"], null, true);
        this.gridAg.addColumnText("projectName", this.terms["common.report.selection.projectname"], null, true);
        this.gridAg.addColumnSelect("orderTypeName", this.terms["common.customer.invoices.ordertype"], null, { displayField: "orderTypeName", selectOptions: this.orderTypesDict, enableHiding: true });
        this.gridAg.addColumnSelect("attestStateNames", this.terms["common.customer.invoices.rowstatus"], null, {
            populateFilterFromGrid: true,
            toolTipField: "attestStateNames", displayField: "attestStateNames", selectOptions: this.attestStates, shape: Constants.SHAPE_CIRCLE, shapeValueField: "attestStateColor", useGradient: true, gradientField: "useGradient", colorField: "attestStateColor", enableHiding: true
        });
        /*(<any>colAttestStates).filters = [
                    {
                        condition: (term, value, row, column) => {
                            if (term === this.terms["common.customer.invoices.multiplestatuses"]) {
                                return (<string>value).indexOf(',') >= 0 ? true : false;
                            }
                            else {
                                return (<string>value).indexOf(term) >= 0 ? true : false;
                            }
                        }
                    }
                ];*/

        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("customerCategories", this.terms["common.customer.invoices.customercategories"], null, true);
        this.gridAg.addColumnText("categories", this.terms["billing.order.ordercategories"], null, true, { hide: true });
        this.gridAg.addColumnText("users", this.terms["common.customer.invoices.participant"], null, true, { toolTipField: "users", enableHiding: true });
        this.gridAg.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, true, { toolTipField: "mainUserName", enableHiding: true });
        this.gridAg.addColumnText("referenceOur", this.terms["billing.order.ourreference"], null, true, { toolTipField: "referenceOur", enableHiding: true, hide: true });
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true, { toolTipField: "internalText" });
        this.gridAg.addColumnText("invoiceLabel", this.terms["common.customer.customer.invoicelabel"], null, true, { toolTipField: "invoiceLabel", enableHiding: true });
        this.gridAg.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, true);
        this.gridAg.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, true, { enableHiding: true, toolTipField: "deliveryAddress" });
        this.gridAg.addColumnText("billingAddress", this.terms["common.customer.invoices.billingaddress"], null, true, { enableHiding: true, toolTipField: "billingAddress", hide: true });
        if (this.hasProductSalesPricePermission) {
            this.gridAg.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
            if (this.hasCurrencyPermission) {
                this.gridAg.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
                this.gridAg.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, true);
            }
            this.gridAg.addColumnNumber("remainingAmount", this.terms["common.customer.invoices.remainingamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("remainingAmountExVat", this.terms["common.customer.invoices.remainingamountexvat"], null, { enableHiding: true, decimals: 2 });
        }
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.orderdate"], null, true);
        this.gridAg.addColumnText("fixedPriceOrderName", this.terms["common.customer.invoices.fixedpriceordertype"], null, true);
        //this.gridAg.addColumnText("defaultDimAccountNames", this.terms["common.internalaccounts"], null, true, { hide: true, enableHiding: true });

        // Internal accounts
        var dimCounter = 2;
        _.forEach(this.accountDims, (a) => {
            this.gridAg.addColumnSelect("defaultDim" + dimCounter + "AccountName", a.name, null, { populateFilterFromGrid: true, displayField: "defaultDim" + dimCounter + "AccountName", selectOptions: a.accounts, enableHiding: true, hide: true });
            dimCounter++;
        });

        this.gridAg.addColumnDate("deliveryDate", this.terms["common.customer.invoices.deliverydate"], null, true);

        this.gridAg.addColumnText("priceListName", this.terms["common.customer.invoices.pricelisttype"], null, true, { toolTipField: "priceListName", enableHiding: true, hide: true });
        this.gridAg.addColumnIcon("myReadyStateIcon", this.terms["common.customer.invoices.myreadystatus"], null, { suppressSorting: false, enableHiding: true, toolTipField: "myReadyStateIconText", showTooltipFieldInFilter: true });
        this.gridAg.addColumnIcon("orderReadyStateIcon", this.terms["common.customer.invoices.orderreadystatus"], null, { enableHiding: true, toolTipField: "orderReadyStateText" });
        this.gridAg.addColumnIcon("billingIconValue", null, null, { showIcon: this.showBillingIcon.bind(this), toolTipField: "billingIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });
        this.gridAg.addColumnIcon("statusIconValue", this.terms["economy.supplier.payment.registerpayment"], 40, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });

        if (this.hasPlanningPermission) {
            //this.gridAg.addColumnShape("shiftTypeColor", null, 50, "", Constants.SHAPE_SQUARE, "shiftTypeName", "", "shiftTypeColor"); 
            this.gridAg.addColumnSelect("shiftTypeName", this.terms["common.ordershifttype"], 50, {
                populateFilterFromGrid: false, toolTipField: "shiftTypeName", displayField: "shiftTypeName", selectOptions: this.shiftTypes, shape: Constants.SHAPE_SQUARE, shapeValueField: "shiftTypeColor", colorField: "shiftTypeColor", enableHiding: true, ignoreTextInFilter: true,
            });
        }
        this.gridAg.addColumnNumber("seqNr", this.terms["common.customer.invoices.seqnr"], null, { alignLeft: true, enableHiding: true, hide: true });
        this.gridAg.addColumnNumber("mappedContractNr", this.terms["common.customer.contracts.contractnr"], null, { alignLeft: true, formatAsText: true, enableHiding: true, hide: true });
        this.gridAg.addColumnText("workDescription", this.terms["billing.order.workingdescription"], 100, true, { toolTipField: "workDescription", enableHiding: true, hide: true});
        if (this.hasEditPermission) {
            this.gridAg.addColumnEdit(this.terms["common.customer.invoices.orderedit"], this.openOrder.bind(this));
        }

        return "common.customer.invoices.order";
    }

    private setupCustomerInvoicesAllColumns(): string {
        // Details first!
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadCustomerInvoiceRows(params);
        });

        this.gridAg.detailOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 50, { enableHiding: true, pinned: "left" });
        this.gridAg.detailOptions.addColumnIcon("rowTypeIcon", null, null, { pinned: "left", editable: false });
        this.gridAg.detailOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.gridAg.detailOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.hasProductSalesPricePermission) {
            this.gridAg.detailOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2, maxDecimals: 4 });
            this.gridAg.detailOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.detailOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.gridAg.detailOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }

        // Master
        this.gridAg.addColumnNumber("seqNr", this.terms["common.customer.invoices.seqnr"], null, { alignLeft: true, enableHiding: true, formatAsText: true });
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null);
        this.gridAg.addColumnText("externalInvoiceNr", this.terms["billing.invoices.externalinvoicenr"], null, true, { enableHiding: true, hide: true });
        this.gridAg.addColumnSelect("deliveryTypeName", this.terms["common.customer.customer.invoicedeliverytypeshort"], null, { displayField: "deliveryTypeName", selectOptions: this.invoiceDeliveryTypes });
        this.gridAg.addColumnText("orderNumbers", this.terms["common.customer.invoices.ordernr"], null, true);
        this.gridAg.addColumnSelect("billingTypeName", this.terms["common.customer.invoices.type"], null, { displayField: "billingTypeName", selectOptions: this.invoiceBillingTypes });
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        if (this.hasExportPermission)
            this.gridAg.addColumnText("exportStatusName", this.terms["common.customer.invoices.export"], null, true);
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("customerCategories", this.terms["common.customer.invoices.customercategories"], null, true);
        this.gridAg.addColumnText("users", this.terms["common.customer.invoices.participant"], null, true, { toolTipField: "users", enableHiding: true });
        this.gridAg.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, true, { toolTipField: "mainUserName", enableHiding: true });
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true, { toolTipField: "internalText", enableHiding: true });
        this.gridAg.addColumnText("invoiceLabel", this.terms["common.customer.customer.invoicelabel"], null, true, { toolTipField: "invoiceLabel", enableHiding: true });
        this.gridAg.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, true, { toolTipField: "deliveryAddress", enableHiding: true });
        this.gridAg.addColumnText("billingAddress", this.terms["common.customer.invoices.billingaddress"], null, true, { enableHiding: true, toolTipField: "billingAddress", hide: true });
        this.gridAg.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, true);
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        if (this.hasCurrencyPermission) {
            this.gridAg.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, true);
        }
        this.gridAg.addColumnText("projectNr", this.terms["common.customer.invoices.projectnr"], null, true);
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["common.customer.invoices.duedate"], null, true);
        this.gridAg.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null, true);
        this.gridAg.addColumnText("priceListName", this.terms["common.customer.invoices.pricelisttype"], null, true, { toolTipField: "priceListName", enableHiding: true, hide: true });
        this.gridAg.addColumnText("referenceYour", this.terms["common.customer.invoices.yourreference"], null, true, { toolTipField: "referenceYour", enableHiding: true, hide: true });
        this.gridAg.addColumnSelect("isCashSalesText", this.terms["common.customer.invoices.iscashsales"], null, { displayField: "isCashSalesText", selectOptions: this.isCashSalesStates, enableHiding: true, hide: true });

        if (this.useInvoiceDeliveryProvider) {
            this.gridAg.addColumnText("invoiceDeliveryProviderName", this.terms["common.customer.invoices.invoicedeliveryprovider"], null, true, { hide: true, enableHiding: true });
        }

        // Internal accounts
        var dimCounter = 2;
        _.forEach(this.accountDims, (a) => {
            this.gridAg.addColumnSelect("defaultDim" + dimCounter + "AccountName", a.name, null, { displayField: "defaultDim" + dimCounter + "AccountName", selectOptions: a.accounts, populateFilterFromGrid: true, enableHiding: true, hide: true });
            dimCounter++;
        });

        this.gridAg.addColumnIcon("billingIconValue", null, null, { showIcon: this.showBillingIcon.bind(this), toolTipField: "billingIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });
        if (this.isProjectCentral && this.hasEditPermission)
            this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openInvoiceProjectCentral.bind(this) });
        else if (!this.isProjectCentral)
            this.gridAg.addColumnIcon(null, this.terms["common.customer.invoices.editinvoice"], null, { icon: "fal fa-pencil iconEdit", onClick: this.openInvoice.bind(this) });
        this.gridAg.addColumnIcon("statusIconValue", null, null, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });

        this.gridAg.detailOptions.finalizeInitGrid();

        return "common.customer.invoices.customerinvoice"
    }

    private setupContractsAllColumns(): string {
        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadServiceOrdersForAgreement(params);
        });

        //Detail columns
        this.gridAg.detailOptions.addColumnNumber("invoiceNr", this.terms["common.customer.invoices.ordernr"], null, { alignLeft: true, formatAsText: true });
        this.gridAg.detailOptions.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: false });
        this.gridAg.detailOptions.addColumnDate("invoiceDate", this.terms["common.customer.invoices.orderdate"], null, false);
        this.gridAg.detailOptions.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, { toolTipField: "internalText", enableHiding: false });
        if (this.hasProductSalesPricePermission) {
            this.gridAg.detailOptions.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        }

        if (this.hasOrderEditPermission)
            this.gridAg.detailOptions.addColumnEdit(this.terms["common.customer.invoices.orderedit"], this.openOrderFromAgreements.bind(this));

        this.gridAg.detailOptions.finalizeInitGrid();

        //Columns
        this.gridAg.addColumnNumber("invoiceNr", this.terms["common.customer.contracts.contractnr"], null, { alignLeft: true, enableHiding: true });
        this.gridAg.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { displayField: "statusName", selectOptions: this.originStatus, enableHiding: true });
        this.gridAg.addColumnText("actorCustomerNr", this.terms["common.report.selection.customernr"], null, true);
        this.gridAg.addColumnText("actorCustomerName", this.terms["common.customer.customer.customername"], null);
        this.gridAg.addColumnText("customerCategories", this.terms["common.customer.invoices.customercategories"], null, true);
        this.gridAg.addColumnText("users", this.terms["common.customer.invoices.participant"], null, true, { toolTipField: "users", enableHiding: true });
        this.gridAg.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, true, { toolTipField: "mainUserName", enableHiding: true });
        this.gridAg.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, true, { toolTipField: "internalText" });
        this.gridAg.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, true);
        this.gridAg.addColumnText("categories", this.terms["common.customer.invoices.contractcategories"], null, true);
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        if (this.hasCurrencyPermission) {
            this.gridAg.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, true);
        }
        this.gridAg.addColumnNumber("contractYearlyValueExVat", this.terms["common.customer.contracts.yearlyvalueexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("contractYearlyValue", this.terms["common.customer.contracts.yearlyvalue"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.startdate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["common.stopdate"], null, true);
        this.gridAg.addColumnText("nextContractPeriod", this.terms["common.customer.contracts.nextperiod"], null, true);
        this.gridAg.addColumnDate("nextInvoiceDate", this.terms["billing.contract.nextinvoicedate"], null, true);
        this.gridAg.addColumnText("contractGroupName", this.terms["common.customer.contracts.contractgroup"], null, true);
        this.gridAg.addColumnText("priceListName", this.terms["common.customer.invoices.pricelisttype"], null, true, { toolTipField: "priceListName", enableHiding: true, hide: true });
        this.gridAg.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, true, { toolTipField: "deliveryAddress", enableHiding: true, hide: true });
        this.gridAg.addColumnText("deliveryPostalCode", this.terms["common.customer.invoices.deliverypostalcode"], null, true, { enableHiding: true, hide: true });
        this.gridAg.addColumnText("deliveryCity", this.terms["common.customer.invoices.deliverycity"], null, true, { enableHiding: true, hide: true });
        this.gridAg.addColumnIcon("statusIconValue", null, 30, { onClick: this.showInformationMessage.bind(this), showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });

        this.gridAg.addColumnEdit(this.terms["common.customer.contract.editcontract"], this.openContract.bind(this));

        return "common.customer.invoices.contract";
    }

    private afterCellEdit(row: CustomerInvoiceGridDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'payAmount':
            case 'paidAmount':
                if (colDef.field === 'payAmount' && !row['payAmountOriginal'])
                    row['payAmountOriginal'] = oldValue;
                else if (colDef.field === 'paidAmount' && !row['paidAmountOriginal'])
                    row['paidAmountOriginal'] = oldValue;

                var num: number = NumberUtility.parseDecimal(newValue);
                if ((row.billingTypeId === TermGroup_BillingType.Debit ? num > (row.totalAmount - row.paidAmount) : num < (row.totalAmount + row.paidAmount)) || num === 0) {
                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.payamountinvalidmessage"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    row.payAmount = oldValue;
                    this.gridAg.options.refreshRows(row);
                } else if (!num) {
                    row.payAmount = oldValue;
                    this.gridAg.options.refreshRows(row);
                } else {
                    //Set amounts
                    if (colDef.field === 'payAmount')
                        row.payAmount = num;
                    else if (colDef.field === 'paidAmount')
                        row.paidAmount = num;

                    //Set currency amount
                    row.payAmountCurrency = (row.currencyRate && row.currencyRate > 0) ? (row.payAmount / row.currencyRate).round(2) : row.payAmount;
                    row.paidAmountCurrency = (row.currencyRate && row.currencyRate > 0) ? (row.paidAmount / row.currencyRate).round(2) : row.paidAmount;
                    if (colDef.field === 'payAmount')
                        row['payAmountModified'] = row['payAmountOriginal'] && row['payAmountOriginal'] == newValue ? false : true;
                    else if (colDef.field === 'paidAmount')
                        row['paidAmountModified'] = row['paidAmountOriginal'] && row['paidAmountOriginal'] == newValue ? false : true;

                    this.gridAg.options.refreshRows(row);
                }
                this.summarizeSelected();
                break;
            case 'payAmountCurrency':
            case 'paidAmountCurrency':
                if (colDef.field === 'payAmountCurrency' && !row['payAmountCurrencyOriginal'])
                    row['payAmountCurrencyOriginal'] = oldValue;
                else if (colDef.field === 'paidAmountCurrency' && !row['paidAmountCurrencyOriginal'])
                    row['paidAmountCurrencyOriginal'] = oldValue;

                var num: number = NumberUtility.parseDecimal(newValue);
                if ((row.billingTypeId === TermGroup_BillingType.Debit ? num > (row.totalAmountCurrency - row.paidAmountCurrency) : num < (row.totalAmountCurrency + row.paidAmountCurrency)) || num === 0) {
                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.payamountinvalidmessage"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    row.payAmountCurrency = oldValue;
                    this.gridAg.options.refreshRows(row);
                } else if (!num) {
                    row.payAmountCurrency = oldValue;
                    this.gridAg.options.refreshRows(row);
                } else {
                    //Set amounts
                    if (colDef.field === 'payAmountCurrency')
                        row.payAmountCurrency = num;
                    else if (colDef.field === 'paidAmountCurrency')
                        row.paidAmountCurrency = num;

                    //Set base currency amount
                    row.payAmount = (row.payAmountCurrency * row.currencyRate).round(2);
                    row.paidAmount = (row.paidAmountCurrency * row.currencyRate).round(2);
                    if (colDef.field === 'payAmountCurrency')
                        row['payAmountCurrencyModified'] = row['payAmountCurrencyOriginal'] && row['payAmountCurrencyOriginal'] == newValue ? false : true;
                    else if (colDef.field === 'paidAmountCurrency')
                        row['paidAmountCurrencyModified'] = row['paidAmountCurrencyOriginal'] && row['paidAmountCurrencyOriginal'] == newValue ? false : true;

                    this.gridAg.options.refreshRows(row);
                }
                this.summarizeSelected();
                break;
            case 'payDate':
                if (!row['payDateOriginal'])
                    row['payDateOriginal'] = oldValue;
                row['payDateModified'] = row['payDateOriginal'] && CalendarUtility.convertToDate(row['payDateOriginal']).isSameDayAs(CalendarUtility.convertToDate(newValue)) ? false : true;

                this.gridAg.options.refreshRows(row);
                break;
        }
    }

    public showCreatePayment(row: any): boolean {
        return row.showCreatePayment;
    }

    public showStatusIcon(row: any): boolean {
        return row.statusIconValue;
    }

    public showBillingIcon(row: any): boolean {
        return row.billingIconValue;
    }

    public createPayment(row: any) {
        var message = new TabMessage(this.terms["common.customer.payment.payment"], "pay_" + row.customerInvoiceId, CustomerPaymentsEditController, { invoiceId: row.customerInvoiceId }, this.urlHelperService.getGlobalUrl("Common/Customer/Payments/Views/edit.html"));
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }

    public openPayment(row) {
        var message = new TabMessage(this.terms["common.customer.payment.payment"] + " " + row.paymentSeqNr.toString(), "pay_" + row.customerPaymentRowId, CustomerPaymentsEditController, { paymentId: row.customerPaymentRowId }, this.urlHelperService.getGlobalUrl("Common/Customer/Payments/Views/edit.html"));
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }

    public edit(row) {
        if (this.classification === SoeOriginStatusClassification.CustomerPaymentsUnpayed) {
            this.openInvoice(row);
        }
        else if (this.classification === SoeOriginStatusClassification.CustomerPaymentsPayed || this.classification === SoeOriginStatusClassification.CustomerPaymentsVoucher) {
            this.openPayment(row);
        }
        else {
            // Send message to TabsController, handles both invoice and order
            if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission || this.isProjectCentral || this.isHandleBilling)) {
                if (this.isProjectCentral && this.classification == SoeOriginStatusClassification.CustomerInvoicesAll)
                    this.openInvoiceProjectCentral(row);
                else if (this.isHandleBilling)
                    this.openInvoice(row);
                else
                    this.openOrder(row);
            }
        }
    }

    public openContract(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_CONTRACT, row);
    }

    public openOffer(row) {
        //var message = new Soe.Core.TabMessage(this.terms["common.customer.invoices.order"] + " " + row.seqNr, row.customerInvoiceId, EditController, { id: row.customerInvoiceId, ids: _.map(this.soeGridOptions.getFilteredRows(), 'entity.customerInvoiceId') }, this.urlHelperService.getGlobalUrl("Billing/Orders/Views/edit.html"));
        //console.log(this.soeGridOptions.getFilteredRows());
        this.messagingService.publish(Constants.EVENT_OPEN_OFFER, row);
    }

    public openOrder(row) {
        //var message = new Soe.Core.TabMessage(this.terms["common.customer.invoices.order"] + " " + row.seqNr, row.customerInvoiceId, EditController, { id: row.customerInvoiceId, ids: _.map(this.soeGridOptions.getFilteredRows(), 'entity.customerInvoiceId') }, this.urlHelperService.getGlobalUrl("Billing/Orders/Views/edit.html"));
        //console.log(this.soeGridOptions.getFilteredRows());
        this.messagingService.publish(Constants.EVENT_OPEN_ORDER, { row: row, ids: _.map(this.gridAg.options.getFilteredRows(), 'customerInvoiceId') });
    }

    public openOrderFromAgreements(row) {
        this.translationService.translate("common.order").then((term) => {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(term + " " + row.invoiceNr, row.customerInvoiceId, OrderEditController, { id: row.customerInvoiceId, ids: _.map(this.gridAg.options.getFilteredRows()), updateTab: true, feature: Feature.Billing_Order_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
        });
    }

    public openInvoice(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_INVOICE, { row: row, ids: _.map(this.gridAg.options.getFilteredRows(), 'customerInvoiceId') });
    }

    openInvoiceProjectCentral(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_INVOICE, row);
    }

    public showInformationMessage(row: CustomerInvoiceGridDTO) {
        var message: string = "";


        if (row.noOfPrintedReminders > 0 && (this.classification == SoeOriginStatusClassification.CustomerInvoicesReminder ||
            this.classification == SoeOriginStatusClassification.CustomerInvoicesReminderForeign)) {
            this.showPrintedRemindersInformation(row.customerInvoiceId);
            return;
        }

        if (!row.fullyPaid) {
            var isTotalAmountPaid: boolean = false;
            var partlyPaid: boolean = false;
            var partlyPaidForeign: boolean = false;
            if (row.paidAmount >= 0) {
                isTotalAmountPaid = row.paidAmount != 0 && row.paidAmount >= row.totalAmount;
                partlyPaid = row.paidAmount != 0 && row.paidAmount < row.totalAmount;
                partlyPaidForeign = row.paidAmountCurrency != 0 && row.paidAmountCurrency < row.totalAmountCurrency;
            }
            else {
                isTotalAmountPaid = row.paidAmount != 0 && row.paidAmount <= row.totalAmount;
                partlyPaid = row.paidAmount != 0 && row.paidAmount > row.totalAmount;
                partlyPaidForeign = row.paidAmountCurrency != 0 && row.paidAmountCurrency > row.totalAmountCurrency;
            }

            if (isTotalAmountPaid) {
                message = message + this.terms["economy.supplier.invoice.paidlate"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
            }
            else if (partlyPaid || partlyPaidForeign) {
                message = message + this.terms["economy.supplier.invoice.partlypaid"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
            }
        }

        if (row.multipleAssetRows) {
            if (message != "")
                message = message + "---<br/>";

            message = message + this.terms["common.customer.invoice.multipleassetrows"] + "<br/>";
            message = message + this.terms["economy.supplier.invoice.manualadjustmentneeded"] + "<br/>";
        }

        if (row.insecureDebt) {
            if (message != "")
                message = message + "---<br/>";

            message += this.terms["common.customer.invoice.stopped"] + "<br/>";
        }

        if (((row.infoIcon & Number(InvoiceRowInfoFlag.Error)) == Number(InvoiceRowInfoFlag.Error)) && row.vatRate > 0) {

            message = message + this.terms["common.customer.invoices.checkvatamount"].format(row.vatRate.toString()) + "<br/>";
        }

        if (message !== "")
            this.notificationService.showDialog(this.terms["core.information"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    public showPrintedRemindersInformation(customerInvoiceId: number) {
        this.commonCustomerService.getPrintedReminderInformation(customerInvoiceId).then((x) => {
            if (x !== "")
                this.notificationService.showDialog(this.terms["core.information"], x, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        });
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });


    public loadGridData(ignoreModified = false) {
        if (this.allItemsSelection && !this.isProjectCentral) {
            if (this.allItemsSelection === 999) {
                this.setupComplete = true;
                return;
            }
            else {
                this.loadCustomerInvoices(ignoreModified);
            }
        }
        else if (this.isProjectCentral) {
            this.loadGridDataForProjectCentral(this.projectId, this.includeChildProjects);
        }
    }

    public loadGridDataForProjectCentral(projectId: number, includeChildProjects: boolean) {
        if (this.isProjectCentral) {
            // Load data
            this.progress.startLoadingProgress([() => {
                return this.commonCustomerService.getCustomerInvoicesForProjectCentral(this.classification, this.originType, projectId, includeChildProjects, this.fromDate, this.toDate, this.parameters.classification === SoeOriginStatusClassification.OrdersAll ? this.orders : this.invoices).then((x) => {
                    this.items = x;
                    this.SetInvoiceData(this.items);

                    this.summarize(this.items);
                    this.setupComplete = true;
                });
            }]).then(() => {
                this.setData(this.items);
            });
        }
    }

    private loadCustomerInvoices(ignoreModified = false) {
        if (ignoreModified)
            this.modifiedData = [];

        // Load data
        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getCustomerInvoices(this.classification, this.originType, this.loadOpen, this.loadClosed, this.showOnlyMine ? this.loadMine : true, this.showActive ? this.loadActive : true, this.allItemsSelection, this.module === SoeModule.Billing, this.setupComplete ? this.modifiedData : null).then((itemsFromServer) => {
                //Set data
                this.SetInvoiceData(itemsFromServer);

                // Check if partial load from edit
                if (this.modifiedData.length > 0 && this.setupComplete) {
                    if ((!itemsFromServer) || (itemsFromServer.length === 0)) {
                        _.forEach(this.modifiedData, (modifiedId: number) => {
                            const index = _.findIndex(this.items, { 'customerInvoiceId': modifiedId });
                            if (index > -1) {
                                this.items.splice(index, 1);
                            }
                        });
                    }
                    else {
                        // Update items returned from server
                        _.forEach(itemsFromServer, (item) => {
                            const index = _.findIndex(this.items, { 'customerInvoiceId': item.customerInvoiceId });
                            if (index > -1)
                                this.items.splice(index, 1, item);
                            else //New
                                this.items.splice(0, 0, item);
                            _.pull(this.modifiedData, item.customerInvoiceId);
                        });

                        // Remove modified not returned from server
                        _.forEach(this.modifiedData, (modifiedId: number) => {
                            const index = _.findIndex(this.items, { 'customerInvoiceId': modifiedId });
                            if (index > -1) {
                                this.items.splice(index, 1);
                            }
                        });
                    }
                }
                else {
                    this.items = itemsFromServer;
                }

                //Set up summarizing
                /*this.gridAg.options.addFooterRow("#sum-footer-grid", {
                    "totalAmount": "sum",
                    "totalAmountExVat": "sum",
                    "totalAmountCurrency": "sum",
                    "remainingAmount": "sum",
                    "remainingAmountExVat": "sum",
                } as IColumnAggregations);*/

                this.summarize(this.items);

                this.setupComplete = true;
                //console.log("invoices", this.items);
            });
        }]).then(() => {
            this.$scope.$applyAsync(() => {
                if (this.isHandleBilling)
                    this.items = _.orderBy(this.items, s => s.created, 'desc');

                _.forEach(this.accountDims, (a) => {
                    const name = "defaultDim" + a.accountDimNr + "AccountName";
                    this.items.forEach((item) => {
                        if (item[name] === "" || item[name] === null || item[name] === undefined) {
                            item[name] = " "
                        }
                    });
                })
                this.setData(this.items)
            });
        });
    }

    public search() {
        const filterModels = this.gridAg.options.getFilterModels();
        if (filterModels)
            this.loadFilteredGridData(filterModels);
    }

    public loadFilteredGridData(filterModels: any) {
        // Basic values
        filterModels["origintype"] = this.originType;
        filterModels["classification"] = this.classification;
        filterModels["billing"] = this.module === SoeModule.Billing;

        // Selection values
        filterModels["loadopen"] = this.loadOpen;
        filterModels["loadclosed"] = this.loadClosed;
        filterModels["loadmine"] = this.loadMine;

        if (filterModels["billingTypeName"]) {
            var filteredBillingTypes = [];
            _.forEach(filterModels["billingTypeName"].values, (value) => {
                var billingType = _.find(this.invoiceBillingTypes, { value: value.toString() });
                if (billingType)
                    filteredBillingTypes.push(billingType.id);

            });
            filterModels["billingTypeName"] = filteredBillingTypes;
        }

        if (filterModels["statusName"]) {
            var filteredStatusNames = [];
            _.forEach(filterModels["statusName"].values, (value) => {
                var statusName = _.find(this.originStatus, { value: value.toString() });
                if (statusName)
                    filteredStatusNames.push(statusName.id);

            });
            filterModels["statusName"] = filteredStatusNames;
        }

        if (filterModels["attestStateNames"]) {
            var filteredAttestStateNames = [];
            _.forEach(filterModels["attestStateNames"], (value) => {
                var attestStateNames = _.find(this.attestStates, { value: value.toString() });
                if (attestStateNames)
                    filteredAttestStateNames.push(attestStateNames.value);

            });
            filterModels["attestStateNames"] = filteredAttestStateNames;
        }

        if (filterModels["orderTypeName"]) {
            var filteredOrderTypeNames = [];
            _.forEach(filterModels["orderTypeName"].values, (value) => {
                var orderTypeName = _.find(this.orderTypesDict, { value: value.toString() });
                if (orderTypeName)
                    filteredOrderTypeNames.push(orderTypeName.id);

            });
            filterModels["orderTypeName"] = filteredOrderTypeNames;
        }

        if (filterModels["shiftTypeName"]) {
            var filteredShiftTypeNames = [];
            _.forEach(filterModels["shiftTypeName"].values, (value) => {
                var shiftTypeName = _.find(this.shiftTypes, { value: value.toString() });
                if (shiftTypeName)
                    filteredShiftTypeNames.push(shiftTypeName.value);

            });
            filterModels["shiftTypeName"] = filteredShiftTypeNames;
        }

        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getFilteredCustomerInvoices(filterModels).then((x) => {
                this.items = x;

                this.SetInvoiceData(this.items);

                this.summarize(this.items);
            });
        }]).then(() => {
            this.setData(this.items);
        });
    }

    private SetInvoiceData(items: CustomerInvoiceGridDTO[]) {
        const today = CalendarUtility.getDateToday();
        for (let i = 0; i < items.length; i++) {
            let invoice = items[i];
            invoice['expander'] = "";
            if (invoice.invoiceDate)
                invoice.invoiceDate = new Date(<any>invoice.invoiceDate).date();
            if (invoice.deliveryDate)
                invoice.deliveryDate = new Date(<any>invoice.deliveryDate).date();
            if (invoice.payDate)
                invoice.payDate = new Date(<any>invoice.payDate).date();
            if (invoice.dueDate)
                invoice.dueDate = new Date(<any>invoice.dueDate).date();
            if (invoice.nextInvoiceDate)
                invoice.nextInvoiceDate = new Date(<any>invoice.nextInvoiceDate).date();

            invoice.expandableDataIsLoaded = false;

            if (invoice.paidAmountCurrency === 0 && invoice.paidAmount === 0)
                invoice.payDate = null;
            if (invoice.deliveryType) {
                var delType = _.find(this.invoiceDeliveryTypes, t => t.id === invoice.deliveryType);
                if (delType)
                    invoice.deliveryTypeName = delType.value;
            }
            else
                invoice.deliveryTypeName = " ";

            if (invoice.exportStatus) {
                invoice.exportStatus = 1;
                invoice.exportStatusName = this.terms["common.customer.invoices.export"];
            }

            if (invoice.orderType) {
                var orderType = _.find(this.orderTypes, o => o.id === invoice.orderType);
                if (orderType)
                    invoice.orderTypeName = orderType.name;
            }
            else {
                invoice.orderTypeName = this.terms["common.customer.invoices.notspecified"];
            }

            if (!invoice.shiftTypeName)
                invoice.shiftTypeName = this.terms["common.customer.invoices.noshifttype"];

            if (!invoice.attestStates || invoice.attestStates.length === 0) {
                invoice.useGradient = false;
                invoice.attestStateNames = this.terms["common.customer.invoice.norows"];
            }
            else if (invoice.attestStates.length === 1) {
                invoice.useGradient = false;
                invoice.attestStateColor = invoice.attestStates[0].color;
            }
            else {
                invoice.useGradient = true;
                invoice.attestStateColor = undefined;
            }

            this.setInformationIconAndTooltip(invoice);

            if (this.classification == SoeOriginStatusClassification.OrdersAll) {
                this.setReadyStateIcon(invoice);
            }

            if (invoice.useClosedStyle) {
                invoice.showCreatePayment = !invoice.useClosedStyle;
                invoice.payAmount = invoice.payAmountCurrency = 0;
            }
            else {
                invoice.showCreatePayment = true;
            }

            if (this.classification == SoeOriginStatusClassification.CustomerInvoicesReminder || this.classification == SoeOriginStatusClassification.CustomerInvoicesInterest) {
                invoice["noOfRemindersText"] = invoice.noOfReminders == 5 ? "IN" : invoice.noOfReminders.toString();
            }

            if (this.classification == SoeOriginStatusClassification.CustomerPaymentsVoucher ||
                this.classification == SoeOriginStatusClassification.CustomerPaymentsPayed ||
                this.classification == SoeOriginStatusClassification.CustomerPaymentsUnpayed ||
                this.classification == SoeOriginStatusClassification.CustomerInvoicesAll) {

                if (invoice.seqNr == 0)
                    invoice.seqNr = undefined;

                if (invoice.dueDate < today)
                    invoice.isOverdued = true;
            }
            else if (this.classification == SoeOriginStatusClassification.OrdersAll) {
                if (invoice.seqNr == 0)
                    invoice.seqNr = undefined;
            }
        }
    }

    private loadCustomerInvoiceRows(params: any) {
        if (!params.data['rowsLoaded']) {
            this.progress.startLoadingProgress([() => {
                return this.coreService.getCustomerInvoiceRowsSmall(params.data.customerInvoiceId).then((x) => {
                    const rows = [];
                    _.forEach(_.filter(x, r => (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow)), (y) => {
                        y.ediTextValue = y.ediEntryId ? this.terms["core.yes"] : this.terms["core.no"];
                        y.ediTextValue = y.ediEntryId ? this.terms["core.yes"] : this.terms["core.no"];
                        y['discountTypeText'] = y.discountType === SoeInvoiceRowDiscountType.Percent ? "%" : params.data.currencyCode;
                        if (y.attestStateId != null) {
                            const attestState = _.find(this.attestStatesFull, { attestStateId: y.attestStateId });
                            if (attestState) {
                                y.attestStateName = attestState.name;
                                y.attestStateColor = attestState.color;
                            }
                        }

                        if (y.isTimeBillingRow) {
                            y['rowTypeIcon'] = 'fal fa-file-invoice-dollar';
                        }
                        else if (y.isTimeProjectRow) {
                            y['rowTypeIcon'] = 'fal fa-clock';
                        }
                        else if (y.isExpenseRow) {
                            y['rowTypeIcon'] = 'fal fa-wallet';
                        }
                        else {
                            switch (y.type) {
                                case SoeInvoiceRowType.ProductRow:
                                    y['rowTypeIcon'] = 'fal fa-box-alt';
                                    break;
                                case SoeInvoiceRowType.TextRow:
                                    y['rowTypeIcon'] = 'fal fa-text';
                                    break;
                                case SoeInvoiceRowType.PageBreakRow:
                                    y['rowTypeIcon'] = 'fal fa-cut';
                                    break;
                                case SoeInvoiceRowType.SubTotalRow:
                                    y['rowTypeIcon'] = 'fal fa-calculator-alt';
                                    break;
                            }
                        }

                        rows.push(y);
                    });

                    params.data['rows'] = rows;
                    params.data['rowsLoaded'] = true;
                });
            }]).then(() => {
                params.successCallback(params.data['rows']);
            });

        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private loadServiceOrdersForAgreement(params: any) {
        if (!params.data['rowsLoaded']) {
            this.progress.startLoadingProgress([() => {
                return this.coreService.getServiceOrdersForAgreementDetails(params.data.customerInvoiceId).then((x) => {
                    const rows = x;

                    params.data['rows'] = rows;
                    params.data['rowsLoaded'] = true;
                });
            }]).then(() => {
                params.successCallback(params.data['rows']);
            });

        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private setReadyStateIcon(item: CustomerInvoiceGridDTO) {

        switch (<number>item.myReadyState) {
            case 1:
                item.myReadyStateIcon = "fal fa-thumbs-down warningColor";
                item.myReadyStateIconText = this.terms["common.customer.invoices.notready"];
                break;
            case 2:
                item.myReadyStateIcon = "fal fa-thumbs-up okColor";
                item.myReadyStateIconText = this.terms["common.customer.invoices.ready"];
                break;
            default:
                item.myReadyStateIcon = "";
                item.myReadyStateIconText = " ";
                break;
        }

        if (item.orderReadyStatePercent >= 100) {
            item.orderReadyStateIcon = "fal fa-users okColor";
        }
        else if (item.orderReadyStatePercent <= 0) {
            item.orderReadyStateIcon = "fal fa-users errorColor";
        }
        else {
            item.orderReadyStateIcon = "fal fa-users warningColor";
        }
    }

    public setInformationIconAndTooltip(item: CustomerInvoiceGridDTO) {

        let hasInfo: boolean = ((item.infoIcon & Number(InvoiceRowInfoFlag.Info)) == Number(InvoiceRowInfoFlag.Info));
        let hasError: boolean = ((item.infoIcon & Number(InvoiceRowInfoFlag.Error)) == Number(InvoiceRowInfoFlag.Error));
        let hasHousehold: boolean = ((item.infoIcon & Number(InvoiceRowInfoFlag.HouseHold)) == Number(InvoiceRowInfoFlag.HouseHold));

        // Get status icons
        let flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.DownloadEinvoice + 1);
        let statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(item.statusIcon);
        let statusIconArray = statusIcons.toNumbersArray();

        if (this.originType === SoeOriginType.CustomerInvoice) {
            if (this.classification === SoeOriginStatusClassification.CustomerInvoicesReminder ||
                this.classification === SoeOriginStatusClassification.CustomerInvoicesReminderForeign) {
                if (item.noOfPrintedReminders > 0) {
                    item.statusIconValue = "fal fa-info-circle infoColor";
                    item.statusIconMessage = this.terms["core.showinfo"];
                }
            }
        }

        //Printing - distribution
        if (item.billingInvoicePrinted) {
            item.billingIconValue = "fal fa-print";
            item.billingIconMessage = this.terms["common.customer.invoices.printed"];
        }

        if (_.includes(statusIconArray, SoeStatusIcon.ElectronicallyDistributed)) {
            switch (item.einvoiceDistStatus) {
                case TermGroup_EDistributionStatusType.PendingInPlatform: {
                    item.billingIconValue = "fal fa-paper-plane warningColor";
                    item.billingIconMessage = this.terms["common.customer.invoices.einvoicedpending"];
                    break;
                }
                case TermGroup_EDistributionStatusType.Sent: {
                    item.billingIconValue = "fal fa-paper-plane successColor";
                    item.billingIconMessage = this.terms["common.customer.invoices.einvoicedsuccess"];
                    break;
                }
                case TermGroup_EDistributionStatusType.Error:
                case TermGroup_EDistributionStatusType.Stopped: {
                    item.billingIconValue = "fal fa-paper-plane errorColor";
                    item.billingIconMessage = this.terms["common.customer.invoices.einvoicederror"];
                    break;
                }
                default: {
                    item.billingIconValue = "fal fa-paper-plane";
                    item.billingIconMessage = this.terms["common.customer.invoices.einvoiced"];
                    break;
                }
            }
        }
        else if (_.includes(statusIconArray, SoeStatusIcon.Email)) {
            item.billingIconValue = "fal fa-envelope";
            item.billingIconMessage = this.terms["common.customer.invoices.emailsent"];
        }
        else if (_.includes(statusIconArray, SoeStatusIcon.EmailError)) {
            item.billingIconValue = "fal fa-envelope errorColor";
            item.billingIconMessage = this.terms["common.customer.invoices.sendemailfailed"];
        }
        else if (_.includes(statusIconArray, SoeStatusIcon.DownloadEinvoice)) {
            item.billingIconValue = "fal fa-download";
            item.billingIconMessage = this.terms["common.customer.invoices.einvoicedownloaded"];
        }

        if (!item.billingIconValue) {
            item.billingIconValue = "";
            item.billingIconMessage = " ";
        }

        if (this.classification != SoeOriginStatusClassification.CustomerInvoicesReminder &&
            this.classification != SoeOriginStatusClassification.CustomerInvoicesReminderForeign) {

            if (hasError || hasInfo || hasHousehold || (item.statusIcon != SoeStatusIcon.None)) {
                if (_.includes(statusIconArray, SoeStatusIcon.Imported)) {
                    item.statusIconValue = "fal fa-download";
                } else if (hasError) {
                    item.statusIconValue = "fal fa-exclamation-triangle errorColor";
                    item.statusIconMessage = this.terms["core.showinfo"];
                } else if (hasHousehold) {
                    switch (item.householdTaxDeductionType) {
                        case TermGroup_HouseHoldTaxDeductionType.GREEN:
                            item.statusIconValue = "fal fa-leaf okColor";
                            item.statusIconMessage = (hasInfo ? this.terms["core.showinfo"] + " - " : "") + this.terms["common.customer.invoices.hasgreenreduction"];
                            break;
                        case TermGroup_HouseHoldTaxDeductionType.ROT:
                            item.statusIconValue = "fal fa-home";
                            item.statusIconMessage = (hasInfo ? this.terms["core.showinfo"] + " - " : "") + this.terms["common.customer.invoices.hasrotreduction"];
                            break;
                        case TermGroup_HouseHoldTaxDeductionType.RUT:
                            item.statusIconValue = "fal fa-hand-sparkles";
                            item.statusIconMessage = (hasInfo ? this.terms["core.showinfo"] + " - " : "") + this.terms["common.customer.invoices.hasrutreduction"];
                            break;
                        default:
                            item.statusIconValue = "fal fa-home";
                            item.statusIconMessage = (hasInfo ? this.terms["core.showinfo"] + " - " : "") + this.terms["common.customer.invoices.hashousededuction"];
                            break;
                    }
                } else if (hasInfo && !hasHousehold) {
                    item.statusIconValue = "fal fa-info-circle infoColor";
                    item.statusIconMessage = this.terms["core.showinfo"];
                }
                else if (item.statusIcon != SoeStatusIcon.None) {
                    // Maybe think about stacking icons?
                    if (_.includes(statusIconArray, SoeStatusIcon.Imported)) {
                        item.statusIconValue = "fal fa-paperclip";
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? item.statusIconMessage + ", " + this.terms["common.imported"] : this.terms["common.imported"];
                    }
                    if (_.includes(statusIconArray, SoeStatusIcon.Attachment)) {
                        item.statusIconValue = "fal fa-paperclip";
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? item.statusIconMessage + ", " + this.terms["common.hasaattachedfiles"] : this.terms["common.hasaattachedfiles"];
                    }
                    if (_.includes(statusIconArray, SoeStatusIcon.Image)) {
                        item.statusIconValue = "fal fa-paperclip";
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? item.statusIconMessage + ", " + this.terms["common.hasattachedimages"] : this.terms["common.hasattachedimages"];
                    }
                    if (_.includes(statusIconArray, SoeStatusIcon.Checklist)) {
                        item.statusIconValue = "fal fa-paperclip";
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? item.statusIconMessage + ", " + this.terms["common.customer.invoices.haschecklists"] : this.terms["common.customer.invoices.haschecklists"];
                    }
                }
            }
        }
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, this.allItemsSelectionSettingType, this.allItemsSelection)
        this.loadGridData();
    }

    public updateOnlyMineSelection() {
        this.coreService.saveBoolSetting(SettingMainType.User, this.onlyMineSelectionSettingType, this.loadMine).then((x) => {
            this.reloadGridFromFilter();
        });
    }

    public updateSelectedItems() {
        this.selectedTotalIncVat = 0;
        this.selectedTotalExVat = 0;
        _.forEach(this.selectedItems, (y) => {
            this.selectedTotalIncVat += y.totalAmount;
            this.selectedTotalExVat += y.totalAmountExVat;
        });
        if (this.showVatFree)
            this.selectedTotal = Number(this.selectedTotalIncVat);
        else
            this.selectedTotal = Number(this.selectedTotalExVat);
    }

    protected showPdfIcon(row) {
        if (row.hasPDF === true)
            return true;
        else
            return false;
    }

    // Lookups
    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        //USED TO SETUP WHAT SHOULD BE LOADED
        var featureIds: number[] = [];
        switch (this.originType) {
            case SoeOriginType.Offer:
                featureIds.push(Feature.Billing_Offer_OffersAll);
                featureIds.push(Feature.Billing_Offer_OffersUser);
                featureIds.push(Feature.Billing_Offer_Offers);
                featureIds.push(Feature.Billing_Offer_Status_OfferToOrder);
                featureIds.push(Feature.Billing_Offer_Status_OfferToInvoice);
                this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                    if (x[Feature.Billing_Offer_OffersAll] || x[Feature.Billing_Offer_Offers] || x[Feature.Billing_Offer_Status_OfferToOrder] || x[Feature.Billing_Offer_Status_OfferToInvoice]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        if (x[Feature.Billing_Offer_OffersUser]) {
                            this.showOnlyMine = true;
                        }
                    }
                    else if (x[Feature.Billing_Offer_OffersUser]) {
                        this.loadOpen = true;
                        this.showOnlyMine = false;
                        this.loadMine = true;
                    }
                    deferral.resolve();
                });
                break;
            case SoeOriginType.Order:
                featureIds.push(Feature.Billing_Order_OrdersAll);
                featureIds.push(Feature.Billing_Order_OrdersUser);
                featureIds.push(Feature.Billing_Order_Orders);
                featureIds.push(Feature.Billing_Order_Status_OrderToInvoice);
                featureIds.push(Feature.Billing_Order_Planning);
                this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                    if (x[Feature.Billing_Order_OrdersAll] || x[Feature.Billing_Order_Orders] || x[Feature.Billing_Order_Status_OrderToInvoice]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        this.showOnlyMine = true;
                    }
                    else if (x[Feature.Billing_Order_OrdersUser]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        this.showOnlyMine = false;
                        this.loadMine = true;
                    }

                    this.hasPlanningPermission = x[Feature.Billing_Order_Planning];
                    deferral.resolve();
                });
                break;
            case SoeOriginType.CustomerInvoice:
                if (this.module === SoeModule.Economy) {
                    featureIds.push(Feature.Economy_Customer_Invoice_Invoices_All);
                    featureIds.push(Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest);
                    featureIds.push(Feature.Economy_Customer_Invoice_Invoices);
                    this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                        if (x[Feature.Economy_Customer_Invoice_Invoices_All] || x[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest] || x[Feature.Economy_Customer_Invoice_Invoices]) {
                            this.loadOpen = true;
                            this.hasOpenPermission = true;
                            this.hasClosedPermission = true;
                            this.showOnlyMine = true;
                        }
                        deferral.resolve();
                    });
                }
                else {
                    featureIds.push(Feature.Billing_Invoice_InvoicesAll);
                    featureIds.push(Feature.Billing_Invoice_InvoicesUser);
                    featureIds.push(Feature.Billing_Invoice_Invoices);
                    featureIds.push(Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest);
                    this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                        if (x[Feature.Billing_Invoice_InvoicesAll] || x[Feature.Billing_Invoice_Invoices] || x[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest]) {
                            this.loadOpen = true;
                            this.hasOpenPermission = true;
                            this.hasClosedPermission = true;
                            this.showOnlyMine = true;
                        }
                        deferral.resolve();
                    });
                }
                break;
            case SoeOriginType.CustomerPayment:
                if (this.module === SoeModule.Economy) {
                    featureIds.push(Feature.Economy_Customer_Payment_Payments);
                    featureIds.push(Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest);
                    this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                        if (x[Feature.Economy_Customer_Invoice_Invoices_All] || x[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest] || x[Feature.Economy_Customer_Invoice_Invoices]) {
                            this.loadOpen = true;
                            this.hasOpenPermission = true;
                            this.hasClosedPermission = true;
                            this.loadMine = false;
                        } else if (x[Feature.Billing_Invoice_InvoicesUser]) {
                            this.loadOpen = true;
                            this.hasOpenPermission = true;
                            this.hasClosedPermission = true;
                            this.showOnlyMine = false;
                            this.loadMine = true;
                        }
                        deferral.resolve();
                    });
                }
                else {
                    featureIds.push(Feature.Billing_Invoice_InvoicesAll);
                    featureIds.push(Feature.Billing_Invoice_InvoicesUser);
                    featureIds.push(Feature.Billing_Invoice_Invoices);
                    featureIds.push(Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest);
                    this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                        if (x[Feature.Billing_Invoice_InvoicesAll] || x[Feature.Billing_Invoice_Invoices] || x[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest]) {
                            this.loadOpen = true;
                            this.hasOpenPermission = true;
                            this.hasClosedPermission = true;
                            if (x[Feature.Billing_Invoice_InvoicesUser]) {
                                this.showOnlyMine = true;
                            }
                        }
                        else if (x[Feature.Billing_Invoice_InvoicesUser]) {
                            this.loadOpen = true;
                            this.showOnlyMine = false;
                            this.loadMine = true;
                        }
                        deferral.resolve();
                    });
                }
                break;
            case SoeOriginType.Contract:
                featureIds.push(Feature.Billing_Contract_ContractsUser);
                featureIds.push(Feature.Billing_Contract_Contracts);
                featureIds.push(Feature.Billing_Contract_Status_ContractToOrder);
                featureIds.push(Feature.Billing_Contract_Status_ContractToInvoice);
                this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
                    if (x[Feature.Billing_Contract_Contracts]) {
                        this.loadOpen = true;
                        this.hasOpenPermission = true;
                        this.hasClosedPermission = true;
                        if (x[Feature.Billing_Contract_ContractsUser]) {
                            this.showOnlyMine = true;
                        }
                    }
                    else if (x[Feature.Billing_Contract_ContractsUser]) {
                        this.loadOpen = true;
                        this.showOnlyMine = false;
                        this.loadMine = true;
                    }
                    deferral.resolve();
                });
                break;
        }
        return deferral.promise;
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        if (this.allItemsSelectionSettingType != null)
            settingTypes.push(this.allItemsSelectionSettingType);
        if (this.onlyMineSelectionSettingType != null)
            settingTypes.push(this.onlyMineSelectionSettingType);
        if (this.showVatFreeSettingType != null)
            settingTypes.push(this.showVatFreeSettingType);
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, this.allItemsSelectionSettingType, 1, false);
            this.showVatFree = SettingsUtility.getBoolUserSetting(x, this.showVatFreeSettingType, false);
            if (!this.onlyMineLocked)
                this.loadMine = this.classification !== SoeOriginStatusClassification.CustomerPaymentsUnpayed ? SettingsUtility.getBoolUserSetting(x, this.onlyMineSelectionSettingType, false) : this.loadMine;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.controlquestion",
            "core.continue",
            "core.warning",
            "core.showinfo",
            "core.yes",
            "core.no",
            "common.customer.invoices.invoicenr",
            "common.customer.invoices.invoiceseqnr",
            "common.customer.customer.invoicedeliverytypeshort",
            "common.customer.invoices.ordernr",
            "common.customer.invoices.type",
            "common.customer.invoices.status",
            "common.customer.invoices.customer",
            "common.customer.invoices.owner",
            "common.customer.invoices.internaltext",
            "common.customer.customer.invoicelabel",
            "common.customer.invoices.payservice",
            "common.customer.invoices.amount",
            "common.customer.invoices.amountexvat",
            "common.customer.invoices.invoicedate",
            "common.customer.invoices.duedate",
            "common.customer.invoices.paydate",
            "common.customer.invoices.editinvoice",
            "common.customer.invoices.showinfo",
            "common.customer.invoices.haschecklists",
            "common.customer.invoices.hashousededuction",
            "common.customer.invoices.export",
            "common.customer.invoices.foreignamount",
            "common.customer.invoices.currencycode",
            "common.customer.invoices.projectnr",
            "common.customer.invoices.orderdate",
            "common.customer.invoices.deliveryaddress",
            "common.customer.invoices.customerinvoice",
            "common.customer.payment.paymentseqnr",
            "common.customer.invoices.reminder",
            "common.customer.invoices.payableamount",
            "common.customer.invoices.paidamount",
            "common.customer.payment.payment",
            "common.customer.invoices.offernr",
            "common.customer.invoices.offerdate",
            "common.customer.invoices.rowstatus",
            "economy.supplier.payment.validinvoice",
            "economy.supplier.payment.validinvoices",
            "common.customer.invoices.invoicedatesnotentered",
            "common.customer.invoices.invoicedatesnotwithincurrentaccountyear",
            "common.customer.invoices.duedatesnotentered",
            "common.customer.invoices.duedatesnotwithincurrentaccountyear",
            "common.customer.invoices.autotovoucher",
            "common.customer.invoices.einvoicealreadysent",
            "common.customer.invoices.einvoice",
            "common.customer.invoices.einvoices",
            "common.customer.invoices.paymentautotransfertovoucher",
            "common.customer.invoices.insecureinvoiceerror",
            "common.customer.invoices.paydatemustbeset",
            "common.customer.payment.payment",
            "common.customer.payment.payments",
            "common.customer.invoices.customerinvoice",
            "common.customer.invoices.customerinvoices",
            "common.customer.invoices.createpaymentinvalid",
            "economy.supplier.payment.changepaydate",
            "common.imported",
            "common.hasaattachedfiles",
            "common.hasattachedimages",
            "economy.supplier.payment.voucherscreated",
            "economy.supplier.payment.defaultVoucherListMissing",
            "common.invoicesdrafttooriginvalid",
            "common.invoicedrafttooriginvalid",
            "common.invoicesdrafttoorigininvalid",
            "common.invoicedrafttoorigininvalid",
            "common.invoicessavedasorigin",
            "common.invoicesavedasorigin",
            "common.invoicessavedasoriginfailed",
            "common.invoicesavedasoriginfailed",
            "common.invoicesorigintovouchervalid",
            "common.invoiceorigintovouchervalid",
            "common.invoicesorigintovoucherinvalid",
            "common.invoiceorigintovoucherinvalid",
            "economy.supplier.invoice.voucherscreated",
            "common.invoicestransfertovoucherfailed",
            "common.invoicetransfertovoucherfailed",
            "common.customer.invoices.invoiceexportvalid",
            "common.customer.invoices.invoicesexportvalid",
            "common.customer.invoices.invoiceexported",
            "common.customer.invoices.invoicesexported",
            "common.customer.invoices.invoiceexportfailed",
            "common.customer.invoices.invoicesexportfailed",
            "common.customer.invoices.invoiceexportinvalid",
            "common.customer.invoices.invoicesexportinvalid",
            "common.customer.invoices.invoicesmatchvalid",
            "common.customer.invoices.invoicesmatchinvalid",
            "common.customer.invoices.invoicesmatched",
            "common.customer.invoices.invoicesmatchfailed",
            "economy.supplier.payment.validcreatepayment",
            "economy.supplier.payment.invoicescreatepaymentinvalid",
            "economy.supplier.payment.invoicecreatepaymentinvalid",
            "economy.supplier.payment.paymentcreated",
            "economy.supplier.payment.paymentcreatedfailed",
            "common.paymentsorigintovouchervalid",
            "common.paymentorigintovouchervalid",
            "common.paymentsorigintovoucherinvalid",
            "common.paymentorigintovoucherinvalid",
            "common.paymentstransferedtovoucher",
            "common.paymenttransferedtovoucher",
            "common.paymentstransferedtovoucherfailed",
            "common.paymenttransferedtovoucherfailed",
            "common.customer.invoices.reminderprintvalid",
            "common.customer.invoices.remindersprintvalid",
            "common.customer.invoices.invoiceprintinvalid",
            "common.customer.invoices.invoicesprintinvalid",
            "common.customer.invoices.reminderprinted",
            "common.customer.invoices.remindersprinted",
            "common.customer.invoices.reminderprintfailed",
            "common.customer.invoices.remindersprintfailed",
            "common.customer.invoice.printreminderlettervalid",
            "common.customer.invoices.createremindervalid",
            "common.customer.invoices.createremindersvalid",
            "common.customer.invoices.createreminderlettervalid",
            "common.customer.invoices.createreminderinvalid",
            "common.customer.invoices.createremindersinvalid",
            "common.customer.invoices.remindercreated",
            "common.customer.invoices.reminderscreated",
            "common.customer.invoices.reminderlettercreated",
            "common.customer.invoices.createreminderfailed",
            "common.customer.invoices.createremindersfailed",
            "common.customer.invoices.createreminderletterfailed",
            "common.customer.invoices.createreminderlettermergevalid",
            "common.customer.invoices.changereminderlevelvalid",
            "common.customer.invoices.changereminderslevelvalid",
            "common.customer.invoices.changereminderletterlevelvalid",
            "common.customer.invoices.changereminderlevelinvalid",
            "common.customer.invoices.changeremindersleveninvalid",
            "common.customer.invoices.changereminderletterlevelinvalid",
            "common.customer.invoices.reminderlevelchanged",
            "common.customer.invoices.reminderslevelchanged",
            "common.customer.invoices.reminderletterlevelchanged",
            "common.customer.invoices.changereminderlevelfailed",
            "common.customer.invoices.changereminderslevelfailed",
            "common.customer.invoices.changereminderletterlevelfailed",
            "common.customer.invoice.createinterestvalid",
            "common.customer.invoice.createinterestsvalid",
            "common.customer.invoice.createinterestinvoicevalid",
            "common.customer.invoice.createinterestinvoicesvalid",
            "common.customer.invoice.createinterestinvalid",
            "common.customer.invoice.createinterestsinvalid",
            "common.customer.invoice.createinterestinvoiceinvalid",
            "common.customer.invoice.createinterestinvoicesinvalid",
            "common.customer.invoice.interestcreated",
            "common.customer.invoice.interestscreated",
            "common.customer.invoice.interestinvoicecreated",
            "common.customer.invoice.interestinvoicescreated",
            "common.customer.invoice.createinterestfailed",
            "common.customer.invoice.createinterestsfailed",
            "common.customer.invoice.createinterestinvoicefailed",
            "common.customer.invoice.createinterestinvoicesfailed",
            "common.customer.invoice.invoicesmerge",
            "common.customer.invoice.invoiceclosevalid",
            "common.customer.invoice.invoicesclosevalid",
            "common.customer.invoice.invoicecloseinvalid",
            "common.customer.invoice.invoicescloseinvalid",
            "common.customer.invoice.invoiceclosed",
            "common.customer.invoice.invoicesclosed",
            "common.customer.invoice.invoiceclosefailed",
            "common.customer.invoice.invoicesclosefailed",
            "common.customer.invoice.printinvoicevalid",
            "common.customer.invoice.printinvoicesvalid",
            "common.customer.invoice.printinvoiceinvalid",
            "common.customer.invoice.printinvoicesinvalid",
            "common.customer.invoice.printinvoicefailed",
            "common.customer.invoice.printinvoicesfailed",
            "common.customer.invoice.createeinvoicevalid",
            "common.customer.invoice.createeinvoicesvalid",
            "common.customer.invoice.createeinvoiceinvalid",
            "common.customer.invoice.createeinvoicesinvalid",
            "common.customer.invoice.einvoicecreated",
            "common.customer.invoice.einvoicescreated",
            "common.customer.invoice.createeinvoicefailed",
            "common.customer.invoice.createeinvoicesfailed",
            "common.customer.invoices.showinvoice",
            "economy.supplier.invoice.paidlate",
            "economy.supplier.invoice.matches.totalamount",
            "economy.supplier.payment.paymentamount",
            "economy.supplier.invoice.amounttopay",
            "common.customer.invoice.multipleassetrows",
            "economy.supplier.invoice.manualadjustmentneeded",
            "economy.supplier.invoice.partlypaid",
            "economy.import.payment.invoicetotalamount",
            "economy.supplier.payment.payamountinvalidmessage",
            "common.customer.invoice.norows",
            "common.customer.invoices.remainingamountexvat",
            "common.customer.invoices.remainingamount",
            "common.customer.invoices.fixedpriceordertype",
            "common.customer.invoices.deliverydate",
            "common.customer.invoices.row",
            "common.customer.invoices.edi",
            "common.customer.invoices.productnr",
            "common.customer.invoices.productname",
            "common.customer.invoices.quantity",
            "common.customer.invoices.unit",
            "common.customer.invoices.price",
            "common.customer.invoices.discount",
            "common.customer.invoices.type",
            "common.customer.invoices.sum",
            "common.customer.invoices.multiplestatuses",
            "common.customer.invoices.interestinvoice",
            "common.customer.invoices.prelinterestcreated",
            "common.customer.invoices.prelinterestscreated",
            "common.customer.invoices.printquestion",
            "common.customer.invoices.defaultinterestreportmissing",
            "economy.supplier.payment.paymentnr",
            "common.customer.invoices.checkvatamount",
            "common.customer.invoices.einvoicedpending",
            "common.customer.invoices.einvoicedsuccess",
            "common.customer.invoices.einvoicederror",
            "common.customer.invoices.einvoiced",
            "common.customer.invoices.emailsent",
            "common.customer.invoices.seqnr",
            "common.customer.invoices.ordertype",
            "common.customer.invoices.notspecified",
            "common.customer.invoices.orderedit",
            "common.customer.invoices.printed",
            "economy.supplier.payment.askPrintVoucher",
            "common.customer.invoices.sendemailvalid",
            "common.customer.invoices.sendemailfailed",
            "common.customer.invoices.sendinvalid",
            "common.customer.invoice.willtransferorder2invoice",
            "common.customer.invoices.notsamecustomer",
            "economy.supplier.payment.notmatchedamount",
            "common.internalaccounts",
            "common.customer.invoices.openediposts",
            "common.ordershifttype",
            "common.customer.invoices.noshifttype",
            "common.customer.invoices.payamountcurrency",
            "common.customer.invoices.offeredit",
            "billing.offer.transfertoinvoiceandmerge",
            "billing.offer.transfertoinvoicesingle",
            "billing.offer.transfertoinvoicemultiple",
            "billing.offer.transfertoinvoiceinvalidsingle",
            "billing.offer.transfertoinvoiceinvalidmultiple",
            "billing.offer.transfertoordersingle",
            "billing.offer.transfertoordermultiple",
            "billing.offer.transfertoorderinvalidsingle",
            "billing.offer.transfertoorderinvalidmultiple",
            "common.customer.invoices.transferdefinitiveinfo",
            "common.customer.contracts.contractnr",
            "common.categories",
            "common.customer.contracts.nextperiod",
            "common.customer.contracts.contractgroup",
            "billing.contract.nextinvoicedate",
            "common.startdate",
            "common.stopdate",
            "common.customer.contract.editcontract",
            "common.customer.contracts.yearlyvalue",
            "common.customer.contracts.yearlyvalueexvat",
            "common.customer.contract.transfertoinvoice",
            "common.customer.contract.transfertoinvoiceandmerge",
            "common.customer.contract.transfertoorder",
            "common.customer.contract.transfertoinvoiceinvalid",
            "common.customer.contract.transfertoorderinvalid",
            "common.customer.invoices.reminderssendvalid",
            "common.customer.invoices.remindersendvalid",
            "common.customer.invoices.sendreminderlettervalid",
            "common.customer.invoices.reminderssent",
            "common.customer.invoices.remindersent",
            "common.from",
            "common.to",
            "common.custom",
            "common.customer.contract.transfertofinishedvalid",
            "common.customer.contract.transfertofinishedinvalid",
            "common.customer.contract.transfertofinishedsuccess",
            "common.customer.contract.transfertofinishedfailed",
            "common.customer.invoices.reminderlevel",
            "common.emailaddress",
            "common.customer.invoices.billingaddress",
            "common.customer.invoices.pricelisttype",
            "common.customer.invoices.ready",
            "common.customer.invoices.notready",
            "common.report.selection.projectname",
            "common.customer.invoices.contractcategories",
            "common.customer.invoices.customercategories",
            "common.reportsettingmissing",
            "common.customer.invoices.myreadystatus",
            "common.customer.invoices.orderreadystatus",
            "common.deleteordervalid",
            "common.deleteordersvalid",
            "common.deleteorderinvalid",
            "common.deleteordersinvalid",
            "common.deleteordersuccess",
            "common.deleteorderssuccess",
            "common.deleteorderfailed",
            "common.deleteordersfailed",
            "common.deleteinvoicevalid",
            "common.deleteinvoicesvalid",
            "common.deleteinvoiceinvalid",
            "common.deleteinvoicesinvalid",
            "common.deleteinvoicesuccess",
            "common.deleteinvoicessuccess",
            "common.deleteinvoicefailed",
            "common.deleteinvoicesfailed",
            "common.customer.invoices.responsible",
            "common.customer.invoices.participant",
            "common.customer.invoices.lastcreated",
            "common.customer.customer.customername",
            "common.report.selection.customernr",
            "billing.order.ourreference",
            "common.customer.invoices.hasrotreduction",
            "common.customer.invoices.hasrutreduction",
            "common.customer.invoices.hasgreenreduction",
            "common.customer.invoices.invoicedeliveryprovider",
            "core.aggrid.blanks",
            "billing.invoices.externalinvoicenr",
            "common.customer.invoices.yourreference",
            "common.customer.invoices.iscashsales",
            "common.customer.invoices.inexchangevalidation",
            "common.customer.invoice.einvoicingoperatorvalidation",
            "billing.order.ordercategories",
            "common.customer.invoices.ocr",
            "common.customer.invoices.createserviceordervalidsingle",
            "common.customer.invoices.createserviceordervalidmulti",
            "common.customer.invoices.createserviceorderinvalidsingle",
            "common.customer.invoices.createserviceorderinvalidmulti",
            "common.customer.invoices.createserviceordersuccesssingle",
            "common.customer.invoices.createserviceordersuccessmulti",
            "common.customer.invoices.createserviceorderserrorsingle",
            "common.customer.invoices.createserviceorderserrormulti",
            "common.customer.invoices.einvoicedownloaded",
            "common.customer.invoices.einvoice.redownloadgrid",
            "common.customer.invoices.deliverycity",
            "common.customer.invoices.deliverypostalcode",
            "billing.order.workingdescription"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCurrentAccountYear(): ng.IPromise<any> {
        const accountYearId = soeConfig.accountYearId;
        if (accountYearId) {
            return this.coreService.getAccountYearById(accountYearId).then((x) => {
                if (x) {
                    this.currentAccountYearFromDate = CalendarUtility.convertToDate(x.from);
                    this.currentAccountYearToDate = CalendarUtility.convertToDate(x.to);
                    this.currentAccountYearId = x.accountYearId;
                }
            });
        }
        else {
            return this.coreService.getCurrentAccountYear().then((x) => {
                if (x) {
                    this.currentAccountYearFromDate = CalendarUtility.convertToDate(x.from);
                    this.currentAccountYearToDate = CalendarUtility.convertToDate(x.to);
                    this.currentAccountYearId = x.accountYearId;
                }
            });
        }
    }

    private loadCompanySettings() {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.BillingStatusTransferredOrderToInvoiceAndPrint);
        settingTypes.push(CompanySettingType.CustomerReminderNrOfClaimLevels);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentMethod);
        settingTypes.push(CompanySettingType.BillingDefaultInvoiceTemplate);
        settingTypes.push(CompanySettingType.CustomerPaymentManualTransferToVoucher);
        settingTypes.push(CompanySettingType.CustomerInvoiceTransferToVoucher);
        settingTypes.push(CompanySettingType.BillingUsePartialInvoicingOnOrderRow);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.BillingDefaultTimeProjectReportTemplate);
        settingTypes.push(CompanySettingType.CustomerDefaultBalanceList);
        settingTypes.push(CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.CustomerPaymentAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.CoreBaseCurrency);
        settingTypes.push(CompanySettingType.CustomerDefaultReminderTemplate);
        settingTypes.push(CompanySettingType.CustomerDefaultInterestTemplate);
        settingTypes.push(CompanySettingType.BillingEInvoiceFormat);
        settingTypes.push(CompanySettingType.BillingDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingOfferDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingOrderDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingContractDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.CustomerDefaultInterestRateCalculationTemplate);
        settingTypes.push(CompanySettingType.CustomerInterestHandlingType);
        settingTypes.push(CompanySettingType.CustomerReminderHandlingType);
        settingTypes.push(CompanySettingType.BillingUseExternalInvoiceNr);
        settingTypes.push(CompanySettingType.InExchangeAPISendRegistered);
        settingTypes.push(CompanySettingType.BillingUseInvoiceDeliveryProvider); 
        settingTypes.push(CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.transferAndPrint = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToInvoiceAndPrint, false);
            this.reminderNoOfClaimLevels = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerReminderNrOfClaimLevels, 0);
            this.customerDefaultPaymentMethod = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentMethod, 0);
            this.defaultBillingInvoiceReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceTemplate);
            this.autoTransferPaymentToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentManualTransferToVoucher, false);
            this.autoTransferInvoiceToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceTransferToVoucher, false);
            this.usePartialInvoicingOnOrderRows = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUsePartialInvoicingOnOrderRow, false);
            this.defaultVoucherListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVoucherList, 0);
            this.defaultTimeProjectReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultTimeProjectReportTemplate, 0);
            this.defaultBalanceListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultBalanceList, 0);
            this.defaultInterestRateCalculationReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultInterestRateCalculationTemplate, 0);
            this.customerInvoiceAskPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer, false);
            this.customerPaymentAskPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentAskPrintVoucherOnTransfer, false);
            this.coreBaseCurrency = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CoreBaseCurrency, 0);
            this.defaultReminderReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultReminderTemplate, 0);
            this.defaultInterestReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultInterestTemplate, 0);
            this.eInvoiceFormat = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingEInvoiceFormat, 0);
            this.emailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultEmailTemplate);
            this.offerEmailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOfferDefaultEmailTemplate);
            this.orderEmailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOrderDefaultEmailTemplate);
            this.contractEmailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingContractDefaultEmailTemplate);
            this.addInterestToNextInvoice = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInterestHandlingType, 1) === 2;
            this.addReminderToNextInvoice = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerReminderHandlingType, 1) === 2;
            this.useExternalInvoiceNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseExternalInvoiceNr);
            this.inexchangeSendActivated = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.InExchangeAPISendRegistered);
            this.useInvoiceDeliveryProvider = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceDeliveryProvider);
            this.includeOnlyInvoicedTime = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport);
        });

    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
            //Add custom
            this.allItemsSelectionDict.push({ id: 999, name: this.terms["common.custom"] });
        });
    }

    private loadDeliveryTypes(): ng.IPromise<any> {
        this.invoiceDeliveryTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryType, false, false).then((x) => {
            this.invoiceDeliveryTypes.push({ id: 0, value: ' ' });
            _.forEach(x, (row) => {
                this.invoiceDeliveryTypes.push({ id: row.id, value: row.name });
            });
        });
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then((x) => {
            this.invoiceBillingTypes = [];
            _.forEach(x, (row) => {
                this.invoiceBillingTypes.push({ id: row.id, value: row.name });
            });
        });
    }

    private loadOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderType, false, false).then((x) => {
            this.orderTypes = x;
            this.orderTypesDict = [];
            _.forEach(x, (row) => {
                this.orderTypesDict.push({ id: row.id, value: row.name });
            });
        });
    }

    private loadOriginStatus(): ng.IPromise<any> {
        return this.commonCustomerService.getInvoiceAndPaymentStatus(this.originType, false).then((x) => {
            this.originStatus = [];
            _.forEach(x, (row) => {
                this.originStatus.push({ id: row.id, value: row.name });
            });
        });
    }

    private loadPaymentStatus(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PaymentStatus, false, false).then((x) => {
            this.paymentStatus = [];
            _.forEach(x, (row) => {
                this.paymentStatus.push({ id: row.name, value: row.name });
            });
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentMethods(SoeOriginType.CustomerPayment, true, false, false, false).then((x) => {
            this.paymentMethods = x;
            if (this.customerDefaultPaymentMethod != null) {
                const defaultPaymentMethod = _.find(this.paymentMethods, { paymentMethodId: this.customerDefaultPaymentMethod });
                if (defaultPaymentMethod)
                    this.selectedPaymentMethod = defaultPaymentMethod;
                else if (this.paymentMethods.length > 0)
                    this.selectedPaymentMethod = _.first(this.paymentMethods);
            }
            else {
                if (this.paymentMethods.length > 0)
                    this.selectedPaymentMethod = _.first(this.paymentMethods);
            }
        });
    }

    public loadAttestStates(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.originType === SoeOriginType.Offer || this.originType === SoeOriginType.Order) {
            return this.coreService.getAttestStates(this.originType === SoeOriginType.Offer ? TermGroup_AttestEntity.Offer : TermGroup_AttestEntity.Order, SoeModule.Billing, false).then((x) => {
                this.attestStatesFull = x;
                this.attestStates.push({ id: this.terms["common.customer.invoice.norows"], value: this.terms["common.customer.invoice.norows"] });
                _.forEach(x, (y: any) => {
                    this.attestStates.push({ id: y.name, value: y.name })
                });
                this.attestStates.push({ id: this.terms["common.customer.invoices.multiplestatuses"], value: this.terms["common.customer.invoices.multiplestatuses"] });
            });
        }
        deferral.resolve();
        return deferral.promise;
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, true, false, true).then((dims) => {
            this.accountDims = dims;
        });
    }

    private loadNumberOfOpenEDIPosts() {
        if (this.originType === SoeOriginType.Order) {
            this.commonCustomerService.getEdiEntryViewsCount(SoeEntityState.Active, SoeOriginType.Order).then((x) => {
                this.openEdiPosts = x;
                this.$timeout(() => {
                    this.openEdiPostsLabel = " (" + this.openEdiPosts.toString() + ")";
                });
            });
        }
        else {
            this.openEdiPosts = 0;
        }
    }

    private loadShiftTypes(): ng.IPromise<any> {
        this.shiftTypes = [];
        return this.coreService.getShiftTypesForUsersCategories(0, true, [TermGroup_TimeScheduleTemplateBlockType.Order]).then(x => {
            this.shiftTypes.push({ id: 0, value: this.terms["common.customer.invoices.noshifttype"] })
            _.forEach(x, (row) => {
                this.shiftTypes.push({ id: row.shiftTypeId, value: row.name });
            });
        });
    }

    private loadInvoiceJournalReportId(): ng.IPromise<any> {
        return this.accountingService.getInvoiceJournalReportId(SoeReportTemplateType.CustomerInvoiceJournal).then(x => {
            this.defaultInvoiceJournalReportId = x;
        });
    }

    private executeButtonFunction(option) {
        const noOfItems: number = this.gridAg.options.getSelectedRows().length;
        if (noOfItems === 0)
            return;

        if (option.id === CustomerInvoiceGridButtonFunctions.SaveAsDefinitiv) {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Dialogs/SaveAsDefinitive/Views/saveasdefinitive.html"),
                controller: SaveAsDefinitiveController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'md',
                resolve: {
                    infoText: () => { return (noOfItems.toString() + " " + this.terms["common.customer.invoices.transferdefinitiveinfo"]) },
                    hasSendEInvoicePermission: () => { return this.hasSendEInvoicePermission },
                    hasDownloadEInvoicePermission: () => { return this.hasDownloadEInvoicePermission },
                    hasReportPermission: () => { return this.hasReportPermission }
                }
            });

            modal.result.then(result => {
                if (result) {
                    this.initTransfer({ id: result.option }, result.print);
                }
            }, function () {
                //Cancelled
            });
        }
        else {
            this.initTransfer(option);
        }
    }

    private getReminderValidationText(validCount: number, textMany: string, textOne: string, textNotNextInvoice: string): string {

        if (this.addReminderToNextInvoice) {
            return validCount > 1 ? this.terms[textMany] : this.terms[textOne];
        }
        else {
            return this.terms[textNotNextInvoice];
        }
    }

    private initTransfer(option, printAfterTransfer = false) {
        this.buttonOption = option;
        let merge = false;
        let checkPartialInvoicing = false;
        let claimLevel: number = option.level != null ? option.level : 0;
        let validatedItems: any = [];
        let validMessage: string = "";
        let invalidMessage: string = "";
        let successMessage: string = "";
        let errorMessage: string = "";

        let groupedValid;
        let groupSelected;

        const selectedItems = this.gridAg.options.getSelectedRows();

        switch (option.id) {
            case CustomerInvoiceGridButtonFunctions.SaveAsDefinitiv:
                if (this.originType === SoeOriginType.CustomerInvoice) {
                    if (this.ignoreDateValidation || this.validateInvoiceDates(true, false, printAfterTransfer)) {
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Draft) {
                                validatedItems.push(row);
                            }
                        });

                        validMessage += validatedItems.length > 1 ? this.terms["common.invoicesdrafttooriginvalid"] : this.terms["common.invoicedrafttooriginvalid"];
                        invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.invoicesdrafttoorigininvalid"] : this.terms["common.invoicedrafttoorigininvalid"];
                        successMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasorigin"] : this.terms["common.invoicesavedasorigin"];
                        errorMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasoriginfailed"] : this.terms["common.invoicesavedasoriginfailed"];

                        this.originStatusChange = SoeOriginStatusChange.DraftToOrigin
                    }
                    else {
                        this.selectedItems = [];
                        return;
                    }
                }
                else {
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Draft) {
                            validatedItems.push(row);
                        }
                    });

                    //MESSAGES

                    this.originStatusChange = this.originType === SoeOriginType.Offer ? SoeOriginStatusChange.Billing_DraftToOrder : SoeOriginStatusChange.Billing_DraftToOffer;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.TransferToVoucher:
                _.forEach(selectedItems, (row) => {
                    if ((row.status === SoeOriginStatus.Origin || row.exportStatus != SoeInvoiceExportStatusType.ExportedAndClosed) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.invoicesorigintovouchervalid"] : this.terms["common.invoiceorigintovouchervalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.invoicesorigintovoucherinvalid"] : this.terms["common.invoiceorigintovoucherinvalid"];
                successMessage = this.terms["economy.supplier.invoice.voucherscreated"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.invoicestransfertovoucherfailed"] : this.terms["common.invoicetransfertovoucherfailed"];

                this.originStatusChange = SoeOriginStatusChange.OriginToVoucher;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportSOP:
                //this.exportSOP();

                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToExportSOP;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportUniMicro:
                //this.exportUniMicro();

                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToExportUniMicro;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportDI:
                //this.exportDIRegnskap();

                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToExportDIRegnskap;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportFortnox:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToExportFortnox;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportVismaEAccounting:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToExportVismaEAccounting;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportZetes:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = (this.classification === SoeOriginStatusClassification.CustomerPaymentsUnpayed) ? SoeOriginStatusChange.CustomerPayment_PayedToZetes : SoeOriginStatusChange.CustomerInvoice_OriginToExportZetes;
                break;
            case CustomerInvoiceGridButtonFunctions.ExportDnB:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportvalid"] : this.terms["common.customer.invoices.invoiceexportvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesexportinvalid"] : this.terms["common.customer.invoices.invoiceexportinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexported"] : this.terms["common.customer.invoices.invoiceexported"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.invoicesexportfailed"] : this.terms["common.customer.invoices.invoiceexportfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToExportDnBNor;
                break;
            case CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice:
                switch (this.originType) {
                    case SoeOriginType.Offer:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.OfferPartlyInvoice) {
                                validatedItems.push(row);
                            }
                        });

                        //MESSAGES
                        validMessage += validatedItems.length > 1 ? this.terms["billing.offer.transfertoinvoicemultiple"] : this.terms["billing.offer.transfertoinvoicesingle"];
                        invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["billing.offer.transfertoinvoiceinvalidmultiple"] : this.terms["billing.offer.transfertoinvoiceinvalidsingle"];
                        errorMessage = "";

                        this.originStatusChange = SoeOriginStatusChange.Billing_OfferToInvoice;
                        break;
                    case SoeOriginType.Order:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Draft || row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.OrderPartlyInvoice) {
                                validatedItems.push(row);
                            }
                        });

                        //MESSAGES
                        if (this.usePartialInvoicingOnOrderRows)
                            checkPartialInvoicing = true;

                        //MESSAGES MISSING
                        validMessage += this.terms["common.customer.invoice.willtransferorder2invoice"];
                        errorMessage = "";
                        this.originStatusChange = SoeOriginStatusChange.Billing_OrderToInvoice;
                        break;
                    case SoeOriginType.Contract:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Origin && (!row.nextContractPeriodDate || (row.nextContractPeriodDate && row.nextContractPeriodDate <= CalendarUtility.getDateToday())) && (!row.dueDate || (row.dueDate && row.dueDate.date() >= CalendarUtility.getDateToday()))) {
                                row.invoiceDate = this.selectedInvoiceDate ? this.selectedInvoiceDate : row.nextInvoiceDate;
                                validatedItems.push(row);
                            }
                        });

                        //MESSAGES
                        validMessage += this.terms["common.customer.contract.transfertoinvoice"];
                        invalidMessage += this.terms["common.customer.contract.transfertoinvoiceinvalid"];
                        errorMessage = "";

                        this.originStatusChange = SoeOriginStatusChange.Billing_ContractToInvoice;
                        break;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders:
                switch (this.originType) {
                    case SoeOriginType.Offer:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.OfferPartlyInvoice) {
                                validatedItems.push(row);
                            }
                        });

                        // Sort
                        validatedItems = _.sortBy(validatedItems, 'invoiceNr');

                        //MESSAGES
                        validMessage += this.terms["billing.offer.transfertoinvoiceandmerge"];
                        invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["billing.offer.transfertoinvoiceinvalidmultiple"] : this.terms["billing.offer.transfertoinvoiceinvalidsingle"];
                        errorMessage = "";
                        this.originStatusChange = SoeOriginStatusChange.Billing_OfferToInvoice;
                        merge = true;
                        break;
                    case SoeOriginType.Order:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Draft || row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.OrderPartlyInvoice) {
                                validatedItems.push(row);
                            }
                        });

                        //MESSAGES

                        //MESSAGES MISSING
                        validMessage += this.terms["common.customer.invoice.willtransferorder2invoice"];
                        errorMessage = "";
                        this.originStatusChange = SoeOriginStatusChange.Billing_OrderToInvoice;
                        merge = true;
                        break;
                    case SoeOriginType.Contract:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Origin && (!row.nextContractPeriodDate || (row.nextContractPeriodDate && row.nextContractPeriodDate <= CalendarUtility.getDateToday())) && (!row.dueDate || (row.dueDate && row.dueDate.date() >= CalendarUtility.getDateToday()))) {
                                row.invoiceDate = this.selectedInvoiceDate ? this.selectedInvoiceDate : row.nextInvoiceDate;
                                validatedItems.push(row);
                            }
                        });

                        //MESSAGES
                        validMessage += this.terms["common.customer.contract.transfertoinvoice"];
                        invalidMessage += this.terms["common.customer.contract.transfertoinvoiceinvalid"];
                        errorMessage = "";

                        this.originStatusChange = SoeOriginStatusChange.Billing_ContractToInvoice;
                        merge = true;
                        break;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndPrint:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Draft || row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.OrderPartlyInvoice) {
                        validatedItems.push(row);
                    }
                });

                //MESSAGES

                //MESSAGES MISSING
                validMessage += this.terms["common.customer.invoice.willtransferorder2invoice"];
                errorMessage = "";
                this.originStatusChange = SoeOriginStatusChange.Billing_OrderToInvoiceAndPrint;
                break;
            case CustomerInvoiceGridButtonFunctions.Match:
                if (selectedItems.length > 1) {
                    var sameActor = true;
                    var amountMatched = true;

                    // Check actor 
                    var invalidActors = false;
                    var firstActorId: number = null;
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                            if (firstActorId === null) {
                                firstActorId = row.actorCustomerId;
                            }
                            else {
                                if (firstActorId != row.actorCustomerId)
                                    invalidActors = true;
                            }

                            validatedItems.push(row);
                        }
                    });

                    if (invalidActors) {
                        validatedItems = []
                        this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.notsamecustomer"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        return;
                    }

                    // Check amounts
                    if (validatedItems.length > 2) {
                        var totalAmount: number = 0;
                        var paidAmount: number = 0;
                        var payAmount: number = 0;
                        _.forEach(selectedItems, (row) => {
                            totalAmount += row.totalAmountCurrency;
                            paidAmount += row.paidAmount;
                            payAmount += row.payAmount;
                        });

                        if (totalAmount != 0 || paidAmount != 0 || payAmount != 0) {
                            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.notmatchedamount"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                            return;
                        }
                    }
                    else {
                        if ((selectedItems.filter(i => i.billingTypeId === TermGroup_BillingType.Credit).length == 0 &&
                            selectedItems.filter(i => i.billingTypeId === TermGroup_BillingType.Debit && i.totalAmount < 0).length == 0) ||
                            selectedItems.filter(i => i.billingTypeId === TermGroup_BillingType.Debit && i.totalAmount > 0).length == 0) {
                            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.notmatchedamount"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                            return;
                        }
                    }

                    validMessage += this.terms["common.customer.invoices.invoicesmatchvalid"];
                    invalidMessage += this.terms["common.customer.invoices.invoicesmatchinvalid"];
                    successMessage = this.terms["common.customer.invoices.invoicesmatched"];
                    errorMessage = this.terms["common.customer.invoices.invoicesmatchfailed"];

                    this.originStatusChange = SoeOriginStatusChange.OriginToMatched;
                }
                else {
                    this.selectedItems = [];
                    return;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.CreatePaymentFile:
                if (!this.ignoreDateValidation) {
                    var message: string = "";
                    let showMessageBox = false;

                    if (this.selectedPayDate != null || InvoiceUtility.IsPayDatesEntered(selectedItems)) {
                        if (this.selectedPayDate != null) {
                            if (!InvoiceUtility.IsDateWithinCurrentAccountYear(this.selectedPayDate, this.currentAccountYearFromDate, this.currentAccountYearToDate)) {
                                message += this.terms["common.customer.invoices.invoicedatesnotwithincurrentaccountyear"]; //En eller flera betalningar har ett betaldatum i felaktigt år.";
                                message += "<br/>";
                                showMessageBox = true;
                            }
                        }
                        else if (!InvoiceUtility.IsPayDatesWithinCurrentAccountYear(selectedItems, this.currentAccountYearFromDate, this.currentAccountYearToDate)) {
                            message += this.terms["common.customer.invoices.invoicedatesnotwithincurrentaccountyear"]; //"En eller flera betalningar har ett betaldatum i felaktigt år.";
                            message += "<br/>";
                            showMessageBox = true;
                        }

                        if (this.autoTransferPaymentToVoucher) {
                            message += this.terms["common.customer.invoices.paymentautotransfertovoucher"]; //"Manuell betalning kommer föras över till verifikat direkt.";
                            message += "<br/>";
                            showMessageBox = true;
                        }

                        if (_.filter(selectedItems, { isInsecureDebt: true }).length > 0) {
                            message += this.terms["common.customer.invoices.insecureinvoiceerror"]; //"En eller flera fakturor är markerade som osäkra, dessa kommer inte att betalas.";
                            message += "<br/>";
                            showMessageBox = true;
                        }
                    }
                    else {
                        this.notificationService.showDialog("varning", this.terms["common.customer.invoices.paydatemustbeset"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        return;
                    }

                    if (showMessageBox) {
                        var modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            this.ignoreDateValidation = val;
                            this.initTransfer(this.buttonOption);
                        });
                        return;
                    }
                }

                _.forEach(selectedItems, (row) => {
                    if ((row.registrationType === OrderInvoiceRegistrationType.Ledger || row.registrationType === OrderInvoiceRegistrationType.Invoice) &&
                        (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher || row.exportStatus === SoeInvoiceExportStatusType.NotExported || row.exportStatus === SoeInvoiceExportStatusType.ExportedAndOpen) &&
                        row.fullyPaid === false && row.insecureDebt === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += this.terms["economy.supplier.payment.validcreatepayment"];

                if (this.selectedPayDate != null) {
                    validMessage += "<br\>(" + this.terms["common.customer.invoices.paydate"] + " " + CalendarUtility.toFormattedDate(this.selectedPayDate) + ")";
                }

                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.payment.invoicescreatepaymentinvalid"] : this.terms["economy.supplier.payment.invoicecreatepaymentinvalid"];
                successMessage += this.terms["economy.supplier.payment.paymentcreated"];
                errorMessage += this.terms["economy.supplier.payment.paymentcreatedfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_OriginToPayment;
                break;
            case CustomerInvoiceGridButtonFunctions.TransferPaymentToVoucher:
                _.forEach(selectedItems, (row) => {
                    if ((row.status === SoePaymentStatus.ManualPayment || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.paymentsorigintovouchervalid"] : this.terms["common.paymentorigintovouchervalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.paymentsorigintovoucherinvalid"] : this.terms["common.paymentorigintovoucherinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.paymentstransferedtovoucher"] : this.terms["common.paymenttransferedtovoucher"];
                errorMessage += validatedItems.length > 1 ? this.terms["common.paymentstransferedtovoucherfailed"] : this.terms["common.paymenttransferedtovoucherfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerPayment_PayedToVoucher;
                break;
            case CustomerInvoiceGridButtonFunctions.PrintReminder:
                for (const element of selectedItems) {
                    const prRow: CustomerInvoiceGridDTO = element;

                    if (prRow.status === SoeOriginStatus.Origin || prRow.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(prRow);
                    }
                }

                validMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.remindersprintvalid", "common.customer.invoices.reminderprintvalid", "common.customer.invoice.printreminderlettervalid");

                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesprintinvalid"] : this.terms["common.customer.invoices.invoiceprintinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.remindersprinted"] : this.terms["common.customer.invoices.reminderprinted"];
                errorMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.remindersprintfailed"] : this.terms["common.customer.invoices.reminderprintfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_PrintReminder;
                break;
            case CustomerInvoiceGridButtonFunctions.SendReminderAsEmail:
                for (const element of selectedItems) {
                    const preRow: CustomerInvoiceGridDTO = element;

                    if (option.id == CustomerInvoiceGridButtonFunctions.SendReminderAsEmail && !preRow.reminderContactEComId) {
                        this.translationService.translate("common.customer.invoices.missingemail").then((text) => {
                            this.notificationService.showErrorDialog(this.terms["common.customer.payment.emailreminder"], text.format(preRow.invoiceNr), "");
                        });
                        return;
                    }

                    if (preRow.status === SoeOriginStatus.Origin || preRow.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(preRow);
                    }
                }
                validMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.reminderssendvalid", "common.customer.invoices.remindersendvalid", "common.customer.invoices.sendreminderlettervalid");

                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.invoicesprintinvalid"] : this.terms["common.customer.invoices.invoiceprintinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.reminderssent"] : this.terms["common.customer.invoices.remindersent"];
                errorMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.remindersprintfailed"] : this.terms["common.customer.invoices.reminderprintfailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_SendReminderAsEmail;
                break;
            case CustomerInvoiceGridButtonFunctions.CreateReminder:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.createremindersvalid", "common.customer.invoices.createremindervalid", "common.customer.invoices.createreminderlettervalid");
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.createremindersinvalid"] : this.terms["common.customer.invoices.createreminderinvalid"];
                successMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.reminderscreated", "common.customer.invoices.remindercreated", "common.customer.invoices.reminderlettercreated");
                errorMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.createremindersfailed", "common.customer.invoices.createreminderfailed", "common.customer.invoices.createreminderletterfailed");

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_InvoiceToReminder;

                break;
            case CustomerInvoiceGridButtonFunctions.CreateReminderAndMerge:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.createremindersvalid", "common.customer.invoices.createremindervalid", "common.customer.invoices.createreminderlettermergevalid");
                successMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.reminderscreated", "common.customer.invoices.remindercreated", "common.customer.invoices.reminderlettercreated");
                errorMessage += this.getReminderValidationText(validatedItems.length, "common.customer.invoices.createremindersfailed", "common.customer.invoices.createreminderfailed", "common.customer.invoices.createreminderletterfailed");

                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.createremindersinvalid"] : this.terms["common.customer.invoices.createreminderinvalid"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_InvoiceToReminder;
                merge = true;

                break;
            case CustomerInvoiceGridButtonFunctions.ChangeReminderLevel:
                _.forEach(selectedItems, (row) => {
                    if ((row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) && row.noOfReminders != claimLevel && row.noOfReminders != TermGroup_InvoiceClaimLevel.None) {
                        validatedItems.push(row);
                    }
                });

                if (this.addReminderToNextInvoice) {
                    validMessage += (validatedItems.length > 1 ? this.terms["common.customer.invoices.changereminderslevelvalid"] : this.terms["common.customer.invoices.changereminderlevelvalid"]);
                    invalidMessage += (selectedItems.length - validatedItems.length > 1 ? this.terms["common.customer.invoices.changereminderslevelinvalid"] : this.terms["common.customer.invoices.changereminderlevelinvalid"]);
                    successMessage += (validatedItems.length > 1 ? this.terms["common.customer.invoices.reminderslevelchanged"] : this.terms["common.customer.invoices.reminderlevelchanged"]);
                    errorMessage += (validatedItems.length > 1 ? this.terms["common.customer.invoices.changereminderslevelfailed"] : this.terms["common.customer.invoices.changereminderlevelfailed"]);
                }
                else {
                    validMessage += this.terms["common.customer.invoices.changereminderletterlevelvalid"];
                    invalidMessage += this.terms["common.customer.invoices.changereminderletterlevelinvalid"];
                    successMessage += this.terms["common.customer.invoices.reminderletterlevelchanged"];
                    errorMessage += this.terms["common.customer.invoices.changereminderletterlevelfailed"];
                }

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_UpdateReminderLevel;
                break;
            case CustomerInvoiceGridButtonFunctions.CreateInterestInvoice:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                if (this.addInterestToNextInvoice) {
                    validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.createinterestsvalid"] : this.terms["common.customer.invoice.createinterestvalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoice.createinterestsinvalid"] : this.terms["common.customer.invoice.createinterestinvalid"];
                    successMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.interestscreated"] : this.terms["common.customer.invoice.interestcreated"];
                    errorMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.createinterestsfailed"] : this.terms["common.customer.invoice.createinterestfailed"];
                }
                else {
                    validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.createinterestinvoicesvalid"] : this.terms["common.customer.invoice.createinterestinvoicevalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoice.createinterestinvoicesinvalid"] : this.terms["common.customer.invoice.createinterestinvoiceinvalid"];
                    successMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.interestinvoicescreated"] : this.terms["common.customer.invoice.interestinvoicecreated"];
                    errorMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.createinterestinvoicesfailed"] : this.terms["common.customer.invoice.createinterestinvoicefailed"];

                }

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_InvoiceToInterest;

                break;
            case CustomerInvoiceGridButtonFunctions.CreateInterestInvoiceMerge:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                groupedValid = _.groupBy(validatedItems, 'actorCustomerId');
                groupSelected = _.groupBy(selectedItems, 'actorCustomerId');
                if (this.addInterestToNextInvoice) {
                    validMessage += Object.keys(groupedValid).length > 1 ? this.terms["common.customer.invoice.createinterestsvalid"] : this.terms["common.customer.invoice.createinterestvalid"];
                    invalidMessage += (Object.keys(groupSelected).length - Object.keys(groupedValid).length) > 1 ? this.terms["common.customer.invoice.createinterestsinvalid"] : this.terms["common.customer.invoice.createinterestinvalid"];
                    successMessage += Object.keys(groupedValid).length > 1 ? this.terms["common.customer.invoice.interestscreated"] : this.terms["common.customer.invoice.interestcreated"];
                    errorMessage += Object.keys(groupedValid).length > 1 ? this.terms["common.customer.invoice.createinterestsfailed"] : this.terms["common.customer.invoice.createinterestfailed"];
                }
                else {
                    validMessage += (Object.keys(groupedValid).length > 1 ? this.terms["common.customer.invoice.createinterestinvoicesvalid"] : this.terms["common.customer.invoice.createinterestinvoicevalid"]) + this.terms["common.customer.invoice.invoicesmerge"];
                    invalidMessage += (Object.keys(groupSelected).length - Object.keys(groupedValid).length) > 1 ? this.terms["common.customer.invoice.createinterestinvoicesinvalid"] : this.terms["common.customer.invoice.createinterestinvoiceinvalid"];
                    successMessage += Object.keys(groupedValid).length > 1 ? this.terms["common.customer.invoice.interestinvoicescreated"] : this.terms["common.customer.invoice.interestinvoicecreated"];
                    errorMessage += Object.keys(groupedValid).length > 1 ? this.terms["common.customer.invoice.createinterestinvoicesfailed"] : this.terms["common.customer.invoice.createinterestinvoicefailed"];

                }

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_InvoiceToInterest;
                merge = true;
                break;
            case CustomerInvoiceGridButtonFunctions.PrintInterestRateCalculation:
                _.forEach(selectedItems, (row) => {
                    validatedItems.push(row);
                });

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_PrintInterestRateCalculation;
                break;
            case CustomerInvoiceGridButtonFunctions.CloseInvoice:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.invoicesclosevalid"] : this.terms["common.customer.invoice.invoiceclosevalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoice.invoicescloseinvalid"] : this.terms["common.customer.invoice.invoicecloseinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.invoicesclosed"] : this.terms["common.customer.invoice.invoiceclosed"];
                errorMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.invoicesclosefailed"] : this.terms["common.customer.invoice.invoiceclosefailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_InvoiceToClosed;
                break;
            case CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndPrint:
                if (this.ignoreDateValidation || this.validateInvoiceDates(true, false)) {
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Draft) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["common.invoicesdrafttooriginvalid"] : this.terms["common.invoicedrafttooriginvalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.invoicesdrafttoorigininvalid"] : this.terms["common.invoicedrafttoorigininvalid"];
                    successMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasorigin"] : this.terms["common.invoicesavedasorigin"];
                    errorMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasoriginfailed"] : this.terms["common.invoicesavedasoriginfailed"];

                    this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_DraftToInvoiceAndPrintInvoice;
                }
                else {
                    this.selectedItems = [];
                    return;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.PrintInvoices:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Draft || row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher || row.status === SoeOriginStatus.Export) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.printinvoicesvalid"] : this.terms["common.customer.invoice.printinvoicevalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.printinvoicesinvalid"] : this.terms["common.printinvoiceinvalid"];
                successMessage = "";
                errorMessage = validatedItems.length > 1 ? this.terms["common.printinvoicesfailed"] : this.terms["common.printinvoicefailed"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_PrintInvoice;
                break;
            case CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndCreateEInvoice:
            case CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndSendEInvoice:
                if (this.ignoreDateValidation || this.validateInvoiceDates(true, true, printAfterTransfer)) {
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Draft && (row.deliveryType === SoeInvoiceDeliveryType.Electronic || row.invoiceDeliveryProvider == SoeInvoiceDeliveryProvider.Intrum)) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.createeinvoicesvalid"] : this.terms["common.customer.invoice.createeinvoicevalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoice.createeinvoicesinvalid"] : this.terms["common.customer.invoice.createeinvoiceinvalid"];
                    successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoice.einvoicescreated"] : this.terms["common.customer.invoice.einvoicecreated"];
                    errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoice.createeinvoicesfailed"] : this.terms["common.customer.invoice.createeinvoicefailed"];

                    this.originStatusChange = option.id === CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndSendEInvoice ? SoeOriginStatusChange.CustomerInvoice_DraftToInvoice_And_SendEInvoice : SoeOriginStatusChange.CustomerInvoice_DraftToInvoice_And_CreateEInvoice;
                }
                else {
                    this.selectedItems = [];
                    return;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.SendasEInvoice:
                let hasInexchangeItems = _.some(selectedItems, (row) => (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) && (row.invoiceDeliveryProvider == SoeInvoiceDeliveryProvider.Inexchange));
                if (!this.inexchangeSendActivated && hasInexchangeItems) {
                    this.notificationService.showDialog(this.terms["core.error"], this.terms["common.customer.invoices.inexchangevalidation"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    return;
                }
            case CustomerInvoiceGridButtonFunctions.DownloadEInvoice:

                if ((this.ignoreEinvoiceRedownloadValidation || this.validateReDownloadEInvoice()) && (this.ignoreDateValidation || this.validateInvoiceDates(false, true))) {
                    _.forEach(selectedItems, (row) => {
                        if (
                            (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) &&
                            (row.deliveryType === SoeInvoiceDeliveryType.Electronic || row.invoiceDeliveryProvider == SoeInvoiceDeliveryProvider.Intrum)) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoice.createeinvoicesvalid"] : this.terms["common.customer.invoice.createeinvoicevalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoice.createeinvoicesinvalid"] : this.terms["common.customer.invoice.createeinvoiceinvalid"];
                    successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoice.einvoicescreated"] : this.terms["common.customer.invoice.einvoicecreated"];
                    errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoice.createeinvoicesfailed"] : this.terms["common.customer.invoice.createeinvoicefailed"];

                    this.originStatusChange = option.id == CustomerInvoiceGridButtonFunctions.DownloadEInvoice ? SoeOriginStatusChange.CustomerInvoice_EInvoice_Create : SoeOriginStatusChange.CustomerInvoice_EInvoice_Send;
                }
                else {
                    this.selectedItems = [];
                    return;
                }
                break;

            case CustomerInvoiceGridButtonFunctions.TransferToOrder:
                switch (this.originType) {
                    case SoeOriginType.Offer:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.OfferPartlyOrder) {
                                validatedItems.push(row);
                            }
                        });

                        //MESSAGES
                        validMessage += validatedItems.length > 1 ? this.terms["billing.offer.transfertoordermultiple"] : this.terms["billing.offer.transfertoordersingle"];
                        invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["billing.offer.transfertoorderinvalidmultiple"] : this.terms["billing.offer.transfertoorderinvalidsingle"];
                        this.originStatusChange = SoeOriginStatusChange.Billing_OfferToOrder;
                        break;
                    case SoeOriginType.Contract:
                        _.forEach(selectedItems, (row) => {
                            if (row.status === SoeOriginStatus.Origin && (!row.nextContractPeriodDate || (row.nextContractPeriodDate && row.nextContractPeriodDate <= CalendarUtility.getDateToday())) && (!row.dueDate || (row.dueDate && row.dueDate.date() >= CalendarUtility.getDateToday()))) {
                                row.invoiceDate = this.selectedInvoiceDate ? this.selectedInvoiceDate : row.nextInvoiceDate;
                                validatedItems.push(row);
                            }
                        });

                        //"common.customer.contract.transfertoinvoice",
                        //"common.customer.contract.transfertoinvoiceandmerge",
                        //"common.customer.contract.transfertoorder",
                        //"common.customer.contract.transfertoinvoiceinvalid",
                        //"common.customer.contract.transfertoorderinvalid",
                        //MESSAGES
                        validMessage += this.terms["common.customer.contract.transfertoorder"];
                        invalidMessage += this.terms["common.customer.contract.transfertoorderinvalid"];
                        errorMessage = "";

                        this.originStatusChange = SoeOriginStatusChange.Billing_ContractToOrder;
                        break;
                }

                break;
            case CustomerInvoiceGridButtonFunctions.SendAsEmail:
                _.forEach(selectedItems, (row) => {
                    if (
                        (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher)) {
                        validatedItems.push(row);
                    }
                });
                validMessage += this.terms["common.customer.invoices.sendemailvalid"];
                invalidMessage += this.terms["common.customer.invoices.sendinvalid"];

                this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_SendAsEmail;
                break;
            case CustomerInvoiceGridButtonFunctions.PrintOrder:
                _.forEach(selectedItems, (row) => {
                    validatedItems.push(row);
                });

                this.originStatusChange = SoeOriginStatusChange.Billing_PrintOrder;
                break;
            case CustomerInvoiceGridButtonFunctions.SaveAsDefinitiveAndSendAsEmail:
                if (this.ignoreDateValidation || this.validateInvoiceDates(true, false, printAfterTransfer)) {
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Draft) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["common.invoicesdrafttooriginvalid"] : this.terms["common.invoicedrafttooriginvalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.invoicesdrafttoorigininvalid"] : this.terms["common.invoicedrafttoorigininvalid"];
                    successMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasorigin"] : this.terms["common.invoicesavedasorigin"];
                    errorMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasoriginfailed"] : this.terms["common.invoicesavedasoriginfailed"];

                    this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_DraftToInvoiceAndSendAsEmail;
                }
                else {
                    this.selectedItems = [];
                    return;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.CloseContracts:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin) {
                        validatedItems.push(row);
                    }
                });

                validMessage += this.terms["common.customer.contract.transfertofinishedvalid"];
                invalidMessage += this.terms["common.customer.contract.transfertofinishedinvalid"];
                successMessage = this.terms["common.customer.contract.transfertofinishedsuccess"];
                errorMessage = this.terms["common.customer.contract.transfertofinishedfailed"];

                this.originStatusChange = SoeOriginStatusChange.Billing_ContractToClosed;
                break;
            case CustomerInvoiceGridButtonFunctions.CreateServiceOrderFromAgreement:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.customer.invoices.createserviceordervalidmulti"] : this.terms["common.customer.invoices.createserviceordervalidsingle"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.customer.invoices.createserviceorderinvalidmulti"] : this.terms["common.customer.invoices.createserviceorderinvalidsingle"];
                successMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.createserviceordersuccessmulti"] : this.terms["common.customer.invoices.createserviceordersuccesssingle"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.customer.invoices.createserviceorderserrormulti"] : this.terms["common.customer.invoices.createserviceorderserroreingle"];

                this.originStatusChange = SoeOriginStatusChange.Billing_ContractToServiceOrder;
                break;
            case CustomerInvoiceGridButtonFunctions.UpdatePrices:
                this.updateContractPrices();
                return;
            case CustomerInvoiceGridButtonFunctions.Delete:
                if (this.originType === SoeOriginType.Order) {
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Draft) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["common.deleteordersvalid"] : this.terms["common.deleteordervalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.deleteordersinvalid"] : this.terms["common.deleteorderinvalid"];
                    successMessage = validatedItems.length > 1 ? this.terms["common.deleteorderssuccess"] : this.terms["common.deleteordersuccess"];
                    errorMessage = validatedItems.length > 1 ? this.terms["common.deleteordersfailed"] : this.terms["common.deleteorderfailed"];

                    this.originStatusChange = SoeOriginStatusChange.Billing_OrderToDeleted;
                }
                else if (this.originType === SoeOriginType.CustomerInvoice) {
                    _.forEach(selectedItems, (row) => {
                        if (row.status === SoeOriginStatus.Draft) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["common.deleteinvoicesvalid"] : this.terms["common.deleteinvoicevalid"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.deleteinvoicesinvalid"] : this.terms["common.deleteinvoiceinvalid"];
                    successMessage = validatedItems.length > 1 ? this.terms["common.deleteinvoicessuccess"] : this.terms["common.deleteinvoicesuccess"];
                    errorMessage = validatedItems.length > 1 ? this.terms["common.deleteinvoicesfailed"] : this.terms["common.deleteinvoicefailed"];

                    this.originStatusChange = SoeOriginStatusChange.CustomerInvoice_Delete;
                }
                break;
            case CustomerInvoiceGridButtonFunctions.AutomaticDistribution:
                if (this.originType === SoeOriginType.CustomerInvoice) {
                    this.invoiceDistribution(selectedItems);
                }
                return;
        }

        var title: string = "";
        var text: string = "";
        var yesButtonText: string = "";
        var noButtonText: string = "";
        var cancelButtonText: string = "";
        var image: SOEMessageBoxImage = SOEMessageBoxImage.None;
        var buttons: SOEMessageBoxButtons = SOEMessageBoxButtons.None;

        var noOfValid: number = validatedItems.length;
        var noOfInvalid = selectedItems.length - validatedItems.length;

        if (this.originStatusChange == SoeOriginStatusChange.CustomerInvoice_InvoiceToInterest) {
            groupedValid = _.groupBy(validatedItems, 'actorCustomerId');
            groupSelected = _.groupBy(selectedItems, 'actorCustomerId');
            noOfValid = Object.keys(groupedValid).length;
            noOfInvalid = (Object.keys(groupSelected).length - noOfValid);
        }

        if (selectedItems.length === validatedItems.length) {

            title = this.terms["core.verifyquestion"];

            text += noOfValid.toString() + " " + validMessage + "<br\>";
            text += this.terms["core.continue"];

            image = SOEMessageBoxImage.Question;
            buttons = SOEMessageBoxButtons.OKCancel;
        }
        else if (selectedItems.length > validatedItems.length) {
            if (noOfValid === 0) {
                title = this.terms["core.warning"];

                text += noOfInvalid.toString() + " " + invalidMessage + "<br\>";

                image = SOEMessageBoxImage.Warning;
                buttons = SOEMessageBoxButtons.OK;
            }
            else {
                title = this.terms["core.verifyquestion"];

                text += noOfInvalid.toString() + " " + invalidMessage + "<br\>";
                text += noOfValid.toString() + " " + validMessage + "<br\>";
                text += this.terms["core.continue"];

                image = SOEMessageBoxImage.Question;
                buttons = SOEMessageBoxButtons.OKCancel;
            }
        }

        if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_PrintInvoice || this.originStatusChange === SoeOriginStatusChange.Billing_PrintOrder) {
            this.showInvoicesReport(validatedItems);
        }
        else if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_SendAsEmail || this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_SendReminderAsEmail) {
            this.showSendEmailDialog(validatedItems, errorMessage, title, text, image, buttons, this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_SendReminderAsEmail);
        }
        else if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_PrintReminder) {
            const dict: SmallGenericType[] = [];
            _.forEach(validatedItems, item => {
                dict.push(new SmallGenericType(item.invoiceId, item.invoiceNr));
            });
            this.showReminderInvoicesReport(dict); //NOT IMPLEMENTED YET!
        }
        else if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_PrintInterestRateCalculation) {
            this.printInterestRateCalculation();
        }
        else {
            const modalDialog = this.notificationService.showDialog(title, text, image, buttons);
            modalDialog.result.then(val => {
                if (val != null && val === true) {
                    if (errorMessage && errorMessage != "")
                        errorMessage = validatedItems.length.toString() + " " + errorMessage;
                    this.transfer(validatedItems, merge, claimLevel, checkPartialInvoicing, errorMessage, successMessage, null, printAfterTransfer);
                };
            });
        }

        this.ignoreDateValidation = false;
        this.ignoreEinvoiceRedownloadValidation = false;
    }

    private transfer(validatedItems: any[], merge: boolean, claimLevel: number, checkPartialInvoicing: boolean, errorMessage: string, succesMessage: string, emailTemplateId?: number, printAfterTransfer = false, reportId = 0, languageId = 0, mergePdfs = false, overrideWarnings = false) {
        if ((!validatedItems) || (validatedItems.length < 1)) { return; }

        const setStatusToOrigin = (this.originStatusChange === SoeOriginStatusChange.Billing_ContractToOrder || this.originStatusChange === SoeOriginStatusChange.Billing_OfferToOrder || this.originStatusChange === SoeOriginStatusChange.Billing_ContractToServiceOrder) ? true : false;

        this.progress.startSaveProgress((completion) => {
            this.commonCustomerService.transferCustomerInvoices(validatedItems, this.originStatusChange, this.currentAccountYearId, this.selectedPaymentMethod.paymentMethodId, merge, claimLevel, checkPartialInvoicing, setStatusToOrigin, this.selectedPayDate, this.selectedInvoiceDate, this.selectedDueDate, this.selectedVoucherDate, emailTemplateId, reportId, languageId, mergePdfs, false, overrideWarnings).then((result) => {
                if (result.success) {
                    this.selectedDueDate = null;
                    this.selectedInvoiceDate = null;
                    this.selectedPayDate = null;
                    this.selectedVoucherDate = null;
                    let showVoucherDialog = false;
                    if (this.originType === SoeOriginType.CustomerInvoice) {
                        if (((this.originStatusChange === SoeOriginStatusChange.DraftToOrigin && this.autoTransferInvoiceToVoucher) || this.originStatusChange === SoeOriginStatusChange.OriginToVoucher) && this.customerInvoiceAskPrintVoucherOnTransfer)
                            showVoucherDialog = true;
                        if ((this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_OriginToPayment && this.autoTransferPaymentToVoucher) && this.customerPaymentAskPrintVoucherOnTransfer)
                            showVoucherDialog = true;
                    }
                    else if (this.originType === SoeOriginType.CustomerPayment) {
                        if (((this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_OriginToPayment && this.autoTransferPaymentToVoucher) || this.originStatusChange === SoeOriginStatusChange.CustomerPayment_PayedToVoucher) && this.customerPaymentAskPrintVoucherOnTransfer)
                            showVoucherDialog = true;
                    }
                    if (showVoucherDialog) {
                        this.commonCustomerService.CalculateAccountBalanceForAccountsFromVoucher(this.currentAccountYearId).then((result) => {
                            if (result.success) {
                                //Do something?
                            }
                        });
                        if (result.integerValue && result.infoMessage) {
                            const modal = this.notificationService.showDialog(this.terms["core.warning"], result.infoMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                            modal.result.then(val => {
                                if (result.idDict) {
                                    // Get keys
                                    this.printVouchersInit(result);
                                }
                            });
                        }
                        else {
                            if (result.idDict) {
                                // Get keys
                                this.printVouchersInit(result);
                            }
                        }
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_OriginToExportSOP || this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_OriginToExportUniMicro) {

                        this.exportDataFileName = result.stringValue;
                        this.exportDataDataStorageId = result.integerValue2;

                        if (result.errorMessage && result.errorMessage != "") {
                            this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        }

                        if (this.exportDataFileName && this.exportDataFileName != "" && this.exportDataDataStorageId > 0) {
                            /*LinkLoadFile.Visibility = Visibility.Visible;
                            LinkLoadFile.IsEnabled = true;*/
                        }
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_OriginToExportDIRegnskap || this.originStatusChange === SoeOriginStatusChange.CustomerPayment_PayedToExportDIRegnskap || this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_OriginToExportDnBNor) {

                        this.exportXMLFileName = result.stringValue;
                        this.exportXMLDataStorageId = result.integerValue2;

                        if (result.errorMessage && result.errorMessage != "") {
                            this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        }
                        else if (this.exportXMLFileName && this.exportXMLFileName !== "" && this.exportXMLDataStorageId > 0) {
                            /*LinkLoadFile.Visibility = Visibility.Visible;
                            LinkLoadFile.IsEnabled = true;*/
                        }
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_DraftToInvoice_And_CreateEInvoice || this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_EInvoice_Create) {

                        this.exportDataFileName = result.stringValue;
                        this.exportDataDataStorageId = result.integerValue2;

                        if (result.errorMessage && result.errorMessage !== "") {
                            this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        }
                        else if (result.integerValue2 && (this.eInvoiceFormat === TermGroup_EInvoiceFormat.Finvoice || this.eInvoiceFormat === TermGroup_EInvoiceFormat.Finvoice2 || this.eInvoiceFormat === TermGroup_EInvoiceFormat.Finvoice3)) {
                            this.triggerEInvoiceDownload(result);
                        }

                        if (this.exportXMLFileName && this.exportXMLFileName !== "" && this.exportXMLDataStorageId > 0) {
                            if (result.Keys != null && result.Keys.Count > 0) {

                                if (this.defaultTimeProjectReportId === 0) {
                                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.permissionorreportmissing"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                                    return;
                                }
                            }
                        }
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_DraftToInvoiceAndPrintInvoice || printAfterTransfer) {
                        if (validatedItems && validatedItems.length > 0) {
                            this.showInvoicesReport(validatedItems, true);
                        }
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_InvoiceToReminder) {
                        this.defaultHandlingType = result.integerValue2;
                        if (result.strDict && result.strDict.length > 0)
                            this.showReminderInvoicesReport(result.strDict);
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_InvoiceToInterest) {
                        this.defaultHandlingType = result.integerValue2;
                        if (result.strDict)
                            this.showInterestsReport(result.strDict);
                    }



                    if (result.infoMessage && result.infoMessage != "" && !result.integerValue) {
                        //SOEMessageBox dialog = new SOEMessageBox(termUtil.GetTerm(739, "Kontering"), termUtil.GetTerm(740, "Följande fakturor saknar eller har felaktig kontering:") + "\r\n" + result.InfoMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.errorinaccounting"] + "<br/>" + result.infoMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    }

                    if (this.originStatusChange === SoeOriginStatusChange.Billing_OrderToDeleted || this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_Delete) {
                        let message = "";
                        if (result.integerValue && result.integerValue > 0)
                            message += result.integerValue + " " + succesMessage;
                        if (result.integerValue2 && result.integerValue2 > 0)
                            message += result.integerValue2 + " " + errorMessage;

                        if (message.length > 0)
                            completion.completed(null, null, false, message);
                        else
                            completion.completed(null, null, true);
                    }
                    else if (this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_DraftToInvoiceAndSendAsEmail || this.originStatusChange === SoeOriginStatusChange.CustomerInvoice_SendAsEmail) {
                        if (result.errorMessage.length > 0)
                            completion.completed(null, null, false, result.errorMessage);
                        else
                            completion.completed(null, null, true);
                    }
                    else if (this.originStatusChange === SoeOriginStatusChange.Billing_ContractToServiceOrder) {
                        let message = succesMessage;
                        if (result.integerValue && result.integerValue > 0)
                            message = result.integerValue + " " + message;
                        completion.completed(null, null, false, message);
                    }
                    else {
                        completion.completed(null, null, false, result.infoMessage);
                    }
                }
                else {
                    if (result.errorMessage && result.canUserOverride) {
                        var title: string = this.terms["core.warning"];
                        var text = result.errorMessage + "\n\n" + this.terms["common.customer.invoice.einvoicingoperatorvalidation"];
                        var modal = this.notificationService.showDialog(title, text, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);

                        modal.result.then(() => {
                            if (result.canUserOverride) {
                                this.transfer(validatedItems, merge, claimLevel, checkPartialInvoicing, errorMessage, succesMessage, null, printAfterTransfer, 0, 0, false, result.canUserOverride);
                                completion.completed(null, null, true);
                            } else {
                                completion.failed(errorMessage + "\n" + result.errorMessage);
                            }
                        }).catch(err => {
                            completion.failed(errorMessage);
                        });
                    }
                    else if (result.booleanValue2) {
                        completion.failed(result.errorMessage);
                    }
                    else if (result.errorMessage) {
                        completion.failed(errorMessage + "\n" + result.errorMessage);
                    }
                    else {
                        completion.failed(errorMessage);
                    }
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });
    }

    private printVouchersInit(result: any) {
        const voucherIds: number[] = []
        _.forEach(Object.keys(result.idDict), (key) => {
            voucherIds.push(Number(key));
        });

        // Get values
        let first = true;
        let voucherNrs: string = "";
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

    private invoiceDistribution(invoices: CustomerInvoiceGridDTO[]) {
        if (invoices.some(i => i.status === SoeOriginStatus.Draft)) {
            return this.notificationService.showDialog(
                this.translationService.translateInstant("core.error"),
                this.translationService.translateInstant("common.customer.invoices.preliminaryfordistribution"),
                SOEMessageBoxImage.Error,
                SOEMessageBoxButtons.OK);
        }

        if (invoices.some(i => !i.deliveryType)) {
            const seqNrs = invoices.filter(i => !i.deliveryType).map(i => i.seqNr).join(", ");
            return this.notificationService.showDialog(
                this.translationService.translateInstant("core.error"),
                this.translationService
                    .translateInstant("common.customer.invoices.missinginvoicemethod")
                    .replace("{0}", seqNrs),
                SOEMessageBoxImage.Error,
                SOEMessageBoxButtons.OK);
        }

        const modal = this.notificationService.showDialog(
            this.translationService.translateInstant("core.verifyquestion"),
            this.translationService.translateInstant("common.customer.invoices.automaticdistributionverify"),
            SOEMessageBoxImage.Information,
            SOEMessageBoxButtons.YesNo);

        modal.result.then((val) => {
            if (val === true) {
                this.performAutomaticInvoiceDistibution(invoices);
            }
        });
    }

    private performAutomaticInvoiceDistibution(invoices: CustomerInvoiceGridDTO[]) {
        this.progress.startWorkProgress((completion) => {
            this.commonCustomerService.automaticallyDistribute(invoices)
                .then((result) => {
                    const permissionCheck = result.permissionCheck;
                    if (!permissionCheck.success) {
                        completion.failed(permissionCheck.errorMessage);
                        return;
                    }
                    completion.completed(null, true, null);

                    const modal = this.modalInstance.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Dialogs/InvoiceDistributionResult/Views/InvoiceDistributionResult.html"),
                        controller: InvoiceDistributionResultController,
                        controllerAs: 'ctrl',
                        backdrop: 'static',
                        size: 'md',
                        resolve: {
                            result: result
                        }
                    });

                    //Trigger EInvoice downloads
                    const eInvoiceResult = result.eInvoiceResult;
                    this.triggerEInvoiceDownload(eInvoiceResult);

                    //Open right menu
                    if (result.printedCount > 0) {
                        this.messagingService.publish(Constants.EVENT_SHOW_REPORT_MENU, {});
                    }
                })
        });
    }

    private updateContractPrices() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Dialogs/UpdateContractPrices/Views/updatecontractprices.html"),
            controller: UpdateContractPricesController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
            }
        });

        modal.result.then(result => {
            let dict: number[] = [];
            const rows = this.gridAg.options.getSelectedRows();
            _.forEach(rows, (y: any) => {
                if (y.customerInvoiceId > 0)
                    dict.push(y.customerInvoiceId);
            });

            this.progress.startWorkProgress((completion) => {
                return this.commonCustomerService.UpdateContractPrices(dict, result.rounding, result.percent, result.amount).then((result) => {
                    if (result.success) {
                        completion.completed(null, true, null);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, null)
                .then(data => {
                    this.loadGridData();
                }, error => {
                });
        }, function () {
            //Cancelled
        });
    }

    private getValidReports(): ng.IPromise<any[]> {

        const deferral = this.$q.defer<any[]>();

        const reportTypes: number[] = [];

        if (this.originType === SoeOriginType.Contract) {
            reportTypes.push(SoeReportTemplateType.BillingContract);
        }
        else if (this.originType === SoeOriginType.Offer) {
            reportTypes.push(SoeReportTemplateType.BillingOffer);
        }
        else if (this.originType === SoeOriginType.Order) {
            reportTypes.push(SoeReportTemplateType.BillingOrder);
        }
        else if (this.originType === SoeOriginType.CustomerInvoice) {
            reportTypes.push(SoeReportTemplateType.BillingInvoice);
            reportTypes.push(SoeReportTemplateType.BillingInvoiceReminder);
            reportTypes.push(SoeReportTemplateType.BillingInvoiceInterest);
        }

        this.reportService.getReportsForType(reportTypes, true, false).then((reportsFromServer: any[]) => {
            deferral.resolve(reportsFromServer);
        });

        return deferral.promise;
    }

    private getValidReportsSmall(): ng.IPromise<SmallGenericType[]> {

        const deferral = this.$q.defer<SmallGenericType[]>();

        const reportTypes: number[] = [];
        const filteredReports: SmallGenericType[] = [];

        if (this.originType === SoeOriginType.Contract) {
            reportTypes.push(SoeReportTemplateType.BillingContract);
        }
        else if (this.originType === SoeOriginType.Offer) {
            reportTypes.push(SoeReportTemplateType.BillingOffer);
        }
        else if (this.originType === SoeOriginType.Order) {
            reportTypes.push(SoeReportTemplateType.BillingOrder);
        }
        else if (this.originType === SoeOriginType.CustomerInvoice) {
            reportTypes.push(SoeReportTemplateType.BillingInvoice);
            reportTypes.push(SoeReportTemplateType.BillingInvoiceReminder);
            reportTypes.push(SoeReportTemplateType.BillingInvoiceInterest);
        }

        this.reportService.getReportsForType(reportTypes, true, false).then((reportsFromServer: any[]) => {
            reportsFromServer.forEach(report => {
                filteredReports.push({ id: report.reportId, name: report.reportNr + " " + report.reportName });
            });

            deferral.resolve(filteredReports);
        });

        return deferral.promise;
    }

    private showInvoicesReport(validatedItems: any[], bypassValidation = false) {

        if (validatedItems.length === 0)
            return;

        if (!bypassValidation && validatedItems.filter(x => x.status === SoeOriginStatus.Draft).length > 0) {
            const keys: string[] = [
                "core.warning",
                "common.customer.invoices.preliminaryprintgrid",
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.preliminaryprintgrid"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then((val) => {
                    this.showInvoicesReport(validatedItems, true);
                });
            });
        }

        let registrationType = null;
        if (this.originType === SoeOriginType.Contract) {
            registrationType = OrderInvoiceRegistrationType.Contract;
        }
        else if (this.originType === SoeOriginType.Offer) {
            registrationType = OrderInvoiceRegistrationType.Offer;
        }
        else if (this.originType === SoeOriginType.Order) {
            registrationType = OrderInvoiceRegistrationType.Order;
        }
        else if (this.originType === SoeOriginType.CustomerInvoice) {
            registrationType = OrderInvoiceRegistrationType.Invoice;
        }

        this.getValidReports().then((reports) => {

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
                controller: SelectReportController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    module: () => { return null },
                    reportTypes: () => { return null },
                    showCopy: () => { return this.originType === SoeOriginType.CustomerInvoice },
                    showEmail: () => { return false },
                    copyValue: () => { return false },
                    reports: () => { return reports },
                    defaultReportId: () => { return null },
                    langId: () => { return null },
                    showReminder: () => { return this.originType === SoeOriginType.CustomerInvoice },
                    showLangSelection: () => { return true },
                    showSavePrintout: () => { return false },
                    savePrintout: () => { return false }
                }
            });

            modal.result.then((result: any) => {
                if ((result) && (result.reportId)) {
                    this.progress.startWorkProgress((completion) => {
                        let invoiceIds: number[] = [];
                        _.forEach(validatedItems, (v) => {
                            invoiceIds.push(v.customerInvoiceId);
                        });

                    let model: ICustomerInvoicePrintDTO = {
                        reportId: result.reminder ? this.defaultReminderReportId : result.reportId,
                        ids: invoiceIds,
                        queue: false,
                        sysReportTemplateTypeId: 0,
                        attachmentIds: [],
                        checklistIds: [],
                        includeOnlyInvoiced: (this.originType === SoeOriginType.Order || this.originType === SoeOriginType.CustomerInvoice) ? this.includeOnlyInvoicedTime : false,
                        orderInvoiceRegistrationType: registrationType,
                        printTimeReport: (this.originType === SoeOriginType.Order || this.originType === SoeOriginType.CustomerInvoice),
                        invoiceCopy: result.createCopy,
                        asReminder: result.reminder,
                        reportLanguageId: result.languageId,
                        mergePdfs: false,
                    };

                    return this.requestReportService.printCustomerInvoice(model).then(() => {
                            completion.completed(null, true);
                        });
                    });
                }
            });
        });
    }

    private showSendEmailDialog(validatedItems: any[], errorMessage: string, title: string, text: string, image: any, buttons: any, sendReminder: boolean) {

        this.getValidReportsSmall().then(reports => {

            this.coreService.getEmailTemplates().then((x) => {
                if (!x || x.length === 0) {
                    return;
                }
                else if (x.length === 1 && reports.length < 2) {
                    const modalMessage = this.notificationService.showDialog(title, text, image, buttons);
                    modalMessage.result.then(val => {
                        if (val != null && val === true) {
                            this.transfer(validatedItems, false, 0, false, errorMessage, x[0].emailTemplateId);
                        };
                    });
                }
                else {
                    const keys: string[] = [
                        "billing.invoices.invoice",
                        "common.customer.invoices.reminder",
                    ];

                    return this.translationService.translateMany(keys).then((types) => {
                        let reportTypes: number[] = [];
                        reportTypes.push(SoeReportTemplateType.BillingOrder);

                        let templateType: any = null;
                        if (sendReminder) {
                            templateType = EmailTemplateType.Reminder;
                        }
                        else {
                            var reminderCount: number = 0;
                            var invoiceCount: number = 0;
                            _.forEach(validatedItems, (item) => {
                                if (item.billingTypeId === TermGroup_BillingType.Reminder)
                                    reminderCount += 1;
                                else
                                    invoiceCount += 1;
                            });

                            if (reminderCount > 0 && invoiceCount === 0 || sendReminder)
                                templateType = EmailTemplateType.Reminder;
                            else if (reminderCount === 0 && invoiceCount > 0)
                                templateType = EmailTemplateType.Invoice;
                        }

                        let emailTemplateId = this.emailTemplateId;
                        if (this.originType === SoeOriginType.Offer && this.offerEmailTemplateId)
                            emailTemplateId = this.offerEmailTemplateId;
                        else if (this.originType === SoeOriginType.Order && this.orderEmailTemplateId)
                            emailTemplateId = this.orderEmailTemplateId;
                        else if (this.originType === SoeOriginType.Contract && this.contractEmailTemplateId)
                            emailTemplateId = this.contractEmailTemplateId;

                        const modal = this.modalInstance.open({
                            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectEmail/SelectEmail.html"),
                            controller: SelectEmailController,
                            controllerAs: 'ctrl',
                            backdrop: 'static',
                            size: 'lg',
                            resolve: {
                                translationService: () => { return this.translationService },
                                coreService: () => { return this.coreService },
                                defaultEmail: () => { return null },
                                defaultEmailTemplateId: () => { return emailTemplateId },
                                recipients: () => { return null },
                                attachments: () => { return null },
                                attachmentsSelected: () => { return null },
                                checklists: () => { return null },
                                types: () => { return types },
                                grid: () => { return true },
                                type: () => { return templateType },
                                showReportSelection: () => { return true },
                                reports: () => { return reports },
                                defaultReportTemplateId: () => { return this.defaultBillingInvoiceReportId },
                                langId: () => { return null }
                            }
                        });

                        modal.result.then((result: any) => {
                            const modalMessage2 = this.notificationService.showDialog(title, text, image, buttons);
                            modalMessage2.result.then(val => {
                                if (val != null && val === true) {
                                    this.transfer(validatedItems, false, 0, false, errorMessage, "", result.emailTemplateId, false, result.reportId, result.languageId, result.mergePdfs);
                                };
                            });
                        });
                    });
                }
            });
        });
    }

    private printReminder(ids: number[]) {
        if (this.defaultReminderReportId) {
            this.progress.startWorkProgress((completion) => {
                let model: ICustomerInvoicePrintDTO = {
                    reportId: this.defaultReminderReportId,
                    ids: ids,
                    queue: false,
                    sysReportTemplateTypeId: 0,
                    attachmentIds: [],
                    checklistIds: [],
                    printTimeReport: false,
                    orderInvoiceRegistrationType: OrderInvoiceRegistrationType.Invoice,
                    invoiceCopy: false,
                    includeOnlyInvoiced: false,
                    asReminder: true,
                    mergePdfs: false,
                };

                return this.requestReportService.printCustomerInvoice(model).then(() => {
                    completion.completed(null, true);
                });
            });
        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private showReminderInvoicesReport(items: any[]) {
        var invoiceNbrs: string = "";
        var ids: number[] = [];

        _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
            ids.push(row.customerInvoiceId);
        });

        _.forEach(items, (key) => {
            invoiceNbrs += key.name + ",";
        });

        invoiceNbrs = invoiceNbrs.slice(0, -1);
        const message = ids.length + " " + this.terms["common.customer.invoice.printreminderlettervalid"] + "<br/>" + invoiceNbrs + "<br/>" + this.terms["core.continue"];
        const modal = this.notificationService.showDialogDefButton(this.terms["common.customer.invoices.reminder"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel, SOEMessageBoxButton.OK);
        modal.result.then(val => {
            if (val != null && val === true) {
                this.printReminder(ids);
            };
        });
    }

    private printInterest(ids: number[]) {
        if (this.defaultInterestReportId) {
            this.reportService.getInvoiceInterestUrl(ids).then((x) => {
                var url = x;
                HtmlUtility.openInSameTab(this.$window, url);
            });
        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.defaultinterestreportmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private showInterestsReport(items: any[]) {
        if (this.addInterestToNextInvoice) {
            const ids: number[] = [];

            _.forEach(Object.keys(items), (key: number) => {
                ids.push(key);
            });

            if (ids.length > 0) {
                const message = (ids.length > 1 ? ids.length + " " + this.terms["common.customer.invoices.prelinterestscreated"] : this.terms["common.customer.invoices.prelinterestcreated"]) + "<br/>" + this.terms["common.customer.invoices.printquestion"];
                const modal = this.notificationService.showDialogDefButton(this.terms["common.customer.invoices.interestinvoice"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel, SOEMessageBoxButton.OK);
                modal.result.then(val => {
                    if (val != null && val === true) {
                        this.printInterest(ids);
                    };
                });
            }
        }
    }

    private validateInvoiceDates(validateDates: boolean, validateEInvoice: boolean, printAfterTransfer = false) {
        let message: string = "";
        let showMessageBox = false;
        const rows = this.gridAg.options.getSelectedRows();

        if (validateDates) {
            //Check if invoicedate is entered
            if (!this.selectedInvoiceDate && !InvoiceUtility.IsInvoiceDatesEntered(rows)) {
                message += this.terms["common.customer.invoices.invoicedatesnotentered"] + "<br><br>";
                showMessageBox = true;
            }
            //Check if invoicedate is within current accountyear
            else if (this.selectedInvoiceDate ? !InvoiceUtility.IsDateWithinCurrentAccountYear(this.selectedInvoiceDate, this.currentAccountYearFromDate, this.currentAccountYearToDate) : !InvoiceUtility.IsInvoiceDatesWithinCurrentAccountYear(rows, this.currentAccountYearFromDate, this.currentAccountYearToDate)) {
                message += this.terms["common.customer.invoices.invoicedatesnotwithincurrentaccountyear"] + "<br><br>";
                showMessageBox = true;
            }
            //Check if duedate is entered
            if (!this.selectedDueDate && !InvoiceUtility.IsDueDatesEntered(rows)) {
                message += this.terms["common.customer.invoices.duedatesnotentered"] + "<br><br>";
                showMessageBox = true;
            }
            //Check if duedate is within current accountyear
            else if (this.selectedDueDate ? !InvoiceUtility.IsDateWithinCurrentAccountYear(this.selectedDueDate, this.currentAccountYearFromDate, this.currentAccountYearToDate) : !InvoiceUtility.IsDueDatesWithinCurrentAccountYear(rows, this.currentAccountYearFromDate, this.currentAccountYearToDate)) {
                message += this.terms["common.customer.invoices.duedatesnotwithincurrentaccountyear"] + "<br><br>";
                showMessageBox = true;
            }

            //Check if voucherdate is within current accountyear
            if (this.selectedVoucherDate && !InvoiceUtility.IsDateWithinCurrentAccountYear(this.selectedVoucherDate, this.currentAccountYearFromDate, this.currentAccountYearToDate)) {
                message += this.terms["common.customer.invoices.voucherdatenotwithincurrentaccountyear"] + "<br><br>";
                showMessageBox = true;
            }

            if (this.autoTransferInvoiceToVoucher) {
                message += this.terms["common.customer.invoices.autotovoucher"] + "<br><br>";
                showMessageBox = true;
            }
        }

        if (validateEInvoice) {
            let alreadySent: boolean = false;
            _.forEach(rows, (row) => {
                if (!alreadySent) {
                    alreadySent = ((row.statusIcon & Number(SoeStatusIcon.ElectronicallyDistributed)) == Number(SoeStatusIcon.ElectronicallyDistributed))
                    if (alreadySent) {
                        message += this.terms["common.customer.invoices.einvoicealreadysent"] + "<br><br>";
                        showMessageBox = true;
                    }
                }
            })
        }

        if (showMessageBox) {
            const modal = this.notificationService.showDialog(this.terms["core.controlquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.ignoreDateValidation = val;
                this.initTransfer(this.buttonOption, printAfterTransfer);
            });
        }

        return !showMessageBox;
    }

    private validateReDownloadEInvoice() {
        let showMessageBox = false;
        let message: string = "";
        const rows = this.gridAg.options.getSelectedRows();
        let alreadySent: boolean = false;
        _.forEach(rows, (row) => {
            if (!alreadySent) {
                alreadySent = (row.statusIcon && row.statusIcon === SoeStatusIcon.DownloadEinvoice)
                if (alreadySent) {
                    message += this.terms["common.customer.invoices.einvoice.redownloadgrid"];
                    showMessageBox = true;
                }
            }
        })

        if (showMessageBox) {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.ignoreEinvoiceRedownloadValidation = val;
                    this.initTransfer(this.buttonOption, false);
                }
            });
        }

        return !showMessageBox;
    }

    private triggerEInvoiceDownload(result: IActionResult) {
        if (!result || !result.stringValue || !result.integerValue2) return;
        this.doDownloadInvoiceFile(result.stringValue, result.integerValue2);

        if (!result.keys || !result.keys.length) return;
        if (result.keys && result.keys.length > 0 && result.strings.length > 0) {
            for (let i = 0; i < result.keys.length; i++) {
                this.doDownloadInvoiceFile(result.strings[i], result.keys[i]);
            }
        }
    }

    private exportSOP() {
        var dict: any = [];
        //Create a collection of entries to moveall to invoices
        var rows = this.gridAg.options.getSelectedRows();
        _.forEach(rows, (y: any) => {
            if (y.customerInvoiceId > 0)
                dict.push(y.customerInvoiceId);
        });

        //SoeOriginStatusChange.CustomerInvoice_OriginToExportSOP
        this.progress.startSaveProgress((completion) => {
            return this.commonCustomerService.ExportCustomerInvoicesToSOP(dict).then((result) => {
                if (result.success) {
                    if (result.stringValue)
                        completion.completed(null, null, false, result.stringValue);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });

    }

    private exportUniMicro() {
        var dict: any = [];
        //Create a collection of entries to move to invoices
        var rows = this.gridAg.options.getSelectedRows();
        _.forEach(rows, (y: any) => {
            if (y.customerInvoiceId > 0)
                dict.push(y.customerInvoiceId);
        });

        //SoeOriginStatusChange.CustomerInvoice_OriginToExportUniMicro
        this.progress.startSaveProgress((completion) => {
            return this.commonCustomerService.ExportCustomerInvoicesToUniMicro(dict).then((result) => {
                if (result.success) {
                    if (result.stringValue)
                        completion.completed(null, null, false, result.stringValue);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });

    }

    private exportDIRegnskap() {
        var dict: any = [];
        //Create a collection of entries to move to invoices
        var rows = this.gridAg.options.getSelectedRows();
        _.forEach(rows, (y: any) => {
            if (y.customerInvoiceId > 0)
                dict.push(y.customerInvoiceId);
        });

        //SoeOriginStatusChange.CustomerInvoice_OriginToExportDIRegnskap
        this.progress.startSaveProgress((completion) => {
            return this.commonCustomerService.ExportCustomerInvoicesToDIRegnskap(dict).then((result) => {
                if (result.success) {
                    if (result.stringValue)
                        completion.completed(null, null, false, result.stringValue);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });

        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });
    }

    private summarize(invoices: CustomerInvoiceGridDTO[]) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        this.filteredPaid = 0;
        this.filteredToPay = 0;
        this.filteredToBeInvoicedTotal = 0;
        this.filteredToBeInvoicedTotalIncVat = 0;
        this.filteredToBeInvoicedTotalExVat = 0;
        this.filteredYearly = 0;
        this.filteredYearlyExVat = 0;
        this.filteredYearlyIncVat = 0;
        this.filteredYearlyExVat = 0;

        this.filteredTotalIncVat = _.sum(_.map(invoices, i => i.totalAmount));
        this.filteredTotalExVat = _.sum(_.map(invoices, i => i.totalAmountExVat));
        this.filteredToBeInvoicedTotalIncVat = _.sum(_.map(invoices, i => i.remainingAmount));
        this.filteredToBeInvoicedTotalExVat = _.sum(_.map(invoices, i => i.remainingAmountExVat));
        this.filteredToPay = _.sum(_.map(invoices, i => i.payAmount ? i.payAmount : 0));
        this.filteredPaid = _.sum(_.map(invoices, i => i.paymentAmount ? i.paymentAmount : 0));
        this.filteredYearlyIncVat = _.sum(_.map(invoices, i => i.contractYearlyValue));
        this.filteredYearlyExVat = _.sum(_.map(invoices, i => i.contractYearlyValueExVat));

        /*for (var i = 0; i < invoices.length; i++) {
            let invoice = invoices[i];
            this.filteredTotalIncVat += invoice.totalAmount;
            this.filteredTotalExVat += invoice.totalAmountExVat;
            this.filteredToBeInvoicedTotalIncVat += invoice.remainingAmount;
            this.filteredToBeInvoicedTotalExVat += invoice.remainingAmountExVat;
            this.filteredToPay += invoice.payAmount ? invoice.payAmount : 0;
            this.filteredPaid += invoice.paymentAmount ? invoice.paymentAmount : 0;
            this.filteredYearlyIncVat += invoice.contractYearlyValue;
            this.filteredYearlyExVat += invoice.contractYearlyValueExVat;
        }*/

        if (this.showVatFree) {
            this.filteredTotal = this.filteredTotalIncVat;
            this.filteredToBeInvoicedTotal = this.filteredToBeInvoicedTotalIncVat;
            this.filteredYearly = this.filteredYearlyIncVat;
        }
        else {
            this.filteredTotal = this.filteredTotalExVat;
            this.filteredToBeInvoicedTotal = this.filteredToBeInvoicedTotalExVat;
            this.filteredYearly = this.filteredYearlyExVat;
        }
    }

    private summarizeFiltered(invoices: CustomerInvoiceGridDTO[]) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        this.filteredPaid = 0;
        this.filteredToPay = 0;
        this.filteredToBeInvoicedTotal = 0;
        this.filteredToBeInvoicedTotalIncVat = 0;
        this.filteredToBeInvoicedTotalExVat = 0;
        this.filteredYearly = 0;
        this.filteredYearlyExVat = 0;
        this.filteredYearlyIncVat = 0;
        this.filteredYearlyExVat = 0;

        this.filteredTotalIncVat = _.sum(_.map(invoices, i => i.totalAmount));
        this.filteredTotalExVat = _.sum(_.map(invoices, i => i.totalAmountExVat));
        this.filteredToBeInvoicedTotalIncVat = _.sum(_.map(invoices, i => i.remainingAmount));
        this.filteredToBeInvoicedTotalExVat = _.sum(_.map(invoices, i => i.remainingAmountExVat));
        this.filteredToPay = _.sum(_.map(invoices, i => i.payAmount ? i.payAmount : 0));
        this.filteredPaid = _.sum(_.map(invoices, i => i.paymentAmount ? i.paymentAmount : 0));
        this.filteredYearlyIncVat = _.sum(_.map(invoices, i => i.contractYearlyValue));
        this.filteredYearlyExVat = _.sum(_.map(invoices, i => i.contractYearlyValueExVat));

        if (this.showVatFree) {
            this.filteredTotal = this.filteredTotalIncVat;
            this.filteredToBeInvoicedTotal = this.filteredToBeInvoicedTotalIncVat;
            this.filteredYearly = this.filteredYearlyIncVat;
        }
        else {
            this.filteredTotal = this.filteredTotalExVat;
            this.filteredToBeInvoicedTotal = this.filteredToBeInvoicedTotalExVat;
            this.filteredYearly = this.filteredYearlyExVat;
        }
    }

    private summarizeSelected() {
        this.selectedTotal = 0;
        this.selectedTotalIncVat = 0;
        this.selectedTotalExVat = 0;
        this.selectedPaid = 0;
        this.selectedToPay = 0;
        this.selectedToBeInvoicedTotal = 0;
        this.selectedToBeInvoicedTotalIncVat = 0;
        this.selectedToBeInvoicedTotalExVat = 0;
        this.selectedYearly = 0;
        this.selectedYearlyExVat = 0;
        this.selectedYearlyIncVat = 0;
        this.selectedYearlyExVat = 0;

        var rows: CustomerInvoiceGridDTO[] = null;
        if (this.includeInternalOrders || this.originType !== SoeOriginType.Order)
            rows = this.gridAg.options.getSelectedRows();
        else
            rows = _.filter(this.gridAg.options.getSelectedRows(), i => i.orderType !== TermGroup_OrderType.Internal);

        this.selectedTotalIncVat = _.sum(_.map(rows, i => i.totalAmount));
        this.selectedTotalExVat = _.sum(_.map(rows, i => i.totalAmountExVat));
        this.selectedToBeInvoicedTotalIncVat = _.sum(_.map(rows, i => i.remainingAmount));
        this.selectedToBeInvoicedTotalExVat = _.sum(_.map(rows, i => i.remainingAmountExVat));
        this.selectedToPay = _.sum(_.map(rows, i => i.payAmount ? i.payAmount : 0));
        this.selectedPaid = _.sum(_.map(rows, i => i.paymentAmount ? i.paymentAmount : 0));
        this.selectedYearlyIncVat = _.sum(_.map(rows, i => i.contractYearlyValue));
        this.selectedYearlyExVat = _.sum(_.map(rows, i => i.contractYearlyValueExVat));

        /*_.forEach(rows, (y: any) => {
            this.selectedTotalIncVat += y.totalAmount;
            this.selectedTotalExVat += y.totalAmountExVat;
            this.selectedToBeInvoicedTotalIncVat += y.remainingAmount;
            this.selectedToBeInvoicedTotalExVat += y.remainingAmountExVat;
            this.selectedToPay += y.payAmount ? y.payAmount : 0;
            this.selectedPaid += y.paymentAmount ? y.paymentAmount : 0;
            this.selectedYearlyIncVat += y.contractYearlyValue;
            this.selectedYearlyExVat += y.contractYearlyValueExVat;
        });*/

        this.$timeout(() => {
            if (this.showVatFree) {
                this.selectedTotal = Number(this.selectedTotalIncVat);
                this.selectedToBeInvoicedTotal = this.selectedToBeInvoicedTotalIncVat;
                this.selectedYearly = this.selectedYearlyIncVat;
            }
            else {
                this.selectedTotal = Number(this.selectedTotalExVat);
                this.selectedToBeInvoicedTotal = this.selectedToBeInvoicedTotalExVat;
                this.selectedYearly = this.selectedYearlyExVat;
            }
        });
    }

    private showVatFreeChanged() {
        this.$timeout(() => {
            if (this.showVatFree) {
                this.filteredTotal = this.filteredTotalIncVat;
                this.selectedTotal = Number(this.selectedTotalIncVat);
                this.filteredToBeInvoicedTotal = this.filteredToBeInvoicedTotalIncVat;
                this.selectedToBeInvoicedTotal = this.selectedToBeInvoicedTotalIncVat;
                this.filteredYearly = this.filteredYearlyIncVat;
                this.selectedYearly = this.selectedYearlyIncVat;
            } else {
                this.filteredTotal = this.filteredTotalExVat;
                this.selectedTotal = Number(this.selectedTotalExVat);
                this.filteredToBeInvoicedTotal = this.filteredToBeInvoicedTotalExVat;
                this.selectedToBeInvoicedTotal = this.selectedToBeInvoicedTotalExVat;
                this.filteredYearly = this.filteredYearlyExVat;
                this.selectedYearly = this.selectedYearlyExVat;
            }
            this.coreService.saveBoolSetting(SettingMainType.User, this.showVatFreeSettingType, this.showVatFree)
        });
    }

    private includeInternalOrdersChanged() {
        this.$timeout(() => {
            const rows = (this.includeInternalOrders) ? this.gridAg.options.getFilteredRows() : this.gridAg.options.getFilteredRows().filter(i => i.orderType !== TermGroup_OrderType.Internal);

            this.summarizeFiltered(rows);
            this.summarizeSelected();
        });
    }

    private printSelectedInvoicesBalanceList() {
        if (this.defaultBalanceListReportId) {
            const ids = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                ids.push(row.customerInvoiceId);
            });

            this.isCustomerBalanceListPrinting = true;
            this.requestReportService.printCustomerBalanceList(ids)
            .then(() => {
                this.isCustomerBalanceListPrinting = false;
            });

        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private printSelectedInvoicesInvoiceJournal() {
        if (this.defaultInvoiceJournalReportId) {
            const ids = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                ids.push(row.customerInvoiceId);
            });

            this.isCustomerInvoiceJournalPrinting = true;
            this.requestReportService.printInvoicesJournal(this.defaultInvoiceJournalReportId, ids)
                .then(() => {
                    this.isCustomerInvoiceJournalPrinting = false;
                });

        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    showContactPersonsForSelectedInvoices() {
        const ids = [];
        _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
            ids.push(row.actorCustomerId || row.actorId);
        });
        this.openContactPersons(ids)
    }

    public openContactPersons(actorIds: number[]) {
        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"),
            controller: ContactPersonsGridController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, actorIds });
        });

        modal.result.then(result => null);
    }

    private printInterestRateCalculation() {
        if (this.defaultInterestRateCalculationReportId) {
            var ids = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                ids.push(row.customerInvoiceId);
            });

            this.commonCustomerService.getInterestRateCalculationReportPrintUrl(ids, this.defaultInterestRateCalculationReportId, SoeReportTemplateType.InterestRateCalculation).then((x) => {
                var url = x;
                HtmlUtility.openInSameTab(this.$window, url);
            });
        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private printVouchers(ids: number[]) {
        if (this.defaultVoucherListReportId) {

            this.requestReportService.printVoucherList(ids);

        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private openEdiGrid() {

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"),
            controller: EdiGridController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                paramaters: () => { return { originType: SoeOriginType.Order, status: TermGroup_EDIStatus.Unprocessed } },
                coreService: () => { return this.coreService },
                reportService: () => { return this.reportService },
                importService: () => { return this.importService },
                translationService: () => { return this.translationService },
                urlHelperService: () => { return this.urlHelperService },
                messagingService: () => { return this.messagingService },
                notificationService: () => { return this.notificationService },
                reportTypes: () => { return null },
                showCopy: () => { return false },
                showEmail: () => { return false },
                copyValue: () => { return false },
                reports: () => { return [] },
                langId: () => { return null },
            }
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, originType: SoeOriginType.Order, status: TermGroup_EDIStatus.Unprocessed });
        });

        modal.result.then((result: any) => {
            if (result && result.reload)
                this.loadGridData();
        });
    }

    private doDownloadInvoiceFile(fileName: string, exportId: number) {
        let uri = window.location.protocol + "//" + window.location.host;
        uri = uri + `/soe/billing/invoice/status/default.aspx?custInvExportBatchId=${exportId}&fileName=${fileName}`;
        window.open(uri, '_blank');
    }

    private setupTypesAndClassication() {
        if (this.module === SoeModule.Billing) {
            switch (this.feature) {
                case Feature.Billing_Offer_Status:
                    this.originType = SoeOriginType.Offer;
                    this.allItemsSelectionSettingType = UserSettingType.BillingOfferAllItemsSelection;
                    this.onlyMineSelectionSettingType = UserSettingType.BillingOfferShowOnlyMineSelection;
                    this.showVatFreeSettingType = UserSettingType.BillingOfferShowVatFree;
                    this.isOffer = true;
                    this.showTotals = true;
                    break;
                case Feature.Billing_Order_Status:
                    this.originType = SoeOriginType.Order;
                    this.allItemsSelectionSettingType = UserSettingType.BillingOrderAllItemsSelection;
                    this.onlyMineSelectionSettingType = UserSettingType.BillingOrderShowOnlyMineSelection;
                    this.showVatFreeSettingType = UserSettingType.BillingOrderShowVatFree;
                    this.isOrder = true;
                    break;
                case Feature.Billing_Invoice_Status:
                case Feature.Economy_Customer_Invoice_Status:
                    this.originType = SoeOriginType.CustomerInvoice;
                    this.allItemsSelectionSettingType = UserSettingType.CustomerInvoiceAllItemsSelection;
                    this.onlyMineSelectionSettingType = UserSettingType.BillingInvoiceShowOnlyMineSelection;
                    this.showVatFreeSettingType = UserSettingType.BillingInvoiceShowVatFree;
                    this.isInvoice = true;
                    this.showTotals = true;
                    break;
                case Feature.Billing_Contract_Status:
                    this.originType = SoeOriginType.Contract;
                    this.allItemsSelectionSettingType = UserSettingType.BillingContractAllItemsSelection;
                    this.onlyMineSelectionSettingType = UserSettingType.BillingContractShowOnlyMineSelection;
                    this.showVatFreeSettingType = UserSettingType.BillingContractShowVatFree;
                    this.isContract = true;
                    this.showTotals = true;
                    this.showYearlyTotals = true;
                    this.hideAllItemsSelection = this.classification === SoeOriginStatusClassification.ContractsRunning;
                    break;
                case Feature.Economy_Customer_Payment:
                    //In billing handling payments
                    this.allItemsSelectionSettingType = UserSettingType.CustomerInvoiceAllItemsSelection;
                    switch (this.classification) {
                        case SoeOriginStatusClassification.CustomerPaymentsUnpayed:
                            this.originType = SoeOriginType.CustomerInvoice;
                            this.showPaymentMethod = true;
                            this.showPayDate = true;
                            this.showPaymentInformation = true;
                            this.hideAutogiroVisibility = true;
                            this.showSplitButton = true;
                            this.showTotals = true;
                            this.showToPayTotals = true;
                            this.showUnpaid = true;
                            this.onlyMineSelectionSettingType = UserSettingType.BillingInvoiceShowOnlyMineSelection;
                            this.showVatFreeSettingType = UserSettingType.BillingInvoiceShowVatFree;
                            this.hideAllItemsSelection = true;
                            break;
                        case SoeOriginStatusClassification.CustomerPaymentsPayed:
                            this.originType = SoeOriginType.CustomerPayment;
                            this.showSplitButton = true;
                            this.showPaidTotals = true;
                            this.hideAllItemsSelection = true;
                            break;
                        case SoeOriginStatusClassification.CustomerPaymentsVoucher:
                            this.originType = SoeOriginType.CustomerPayment;
                            this.showPaidTotals = true;
                            this.hideAllItemsSelection = true;
                            break;
                    }
                    break;
            }
        }
        else if (this.module === SoeModule.Economy) {
            this.originType = SoeOriginType.CustomerInvoice;
            this.allItemsSelectionSettingType = UserSettingType.CustomerInvoiceAllItemsSelection;
            this.onlyMineSelectionSettingType = UserSettingType.BillingInvoiceShowOnlyMineSelection;
            switch (this.classification) {
                case SoeOriginStatusClassification.CustomerInvoicesAll:
                    this.originType = SoeOriginType.CustomerInvoice;
                    this.allItemsSelectionSettingType = UserSettingType.CustomerInvoiceAllItemsSelection;
                    this.showVatFreeSettingType = UserSettingType.BillingInvoiceShowVatFree;
                    this.isInvoice = true;
                    this.showTotals = true;
                    break;
                case SoeOriginStatusClassification.CustomerPaymentsUnpayed:
                    this.showPaymentMethod = true;
                    this.showPayDate = true;
                    this.showPaymentInformation = true;
                    this.hideAutogiroVisibility = true;
                    this.showSplitButton = true;
                    this.showTotals = true;
                    this.showToPayTotals = true;
                    this.hideAllItemsSelection = true;
                    break;
                case SoeOriginStatusClassification.CustomerInvoicesReminder:
                    this.showSplitButton = true;
                    this.showTotals = true;
                    this.showToPayTotals = true;
                    this.showPaidTotals = true;
                    this.hideAllItemsSelection = true;
                    break;
                case SoeOriginStatusClassification.CustomerInvoicesInterest:
                    this.showSplitButton = true;
                    this.showTotals = true;
                    this.showToPayTotals = true;
                    this.showPaidTotals = true;
                    this.hideAllItemsSelection = true;
                    break;
                case SoeOriginStatusClassification.CustomerPaymentsPayed:
                    this.originType = SoeOriginType.CustomerPayment;
                    this.showSplitButton = true;
                    this.showPaidTotals = true;
                    this.hideAllItemsSelection = true;
                    break;
                case SoeOriginStatusClassification.CustomerPaymentsVoucher:
                    this.originType = SoeOriginType.CustomerPayment;
                    this.showPaidTotals = true;
                    this.hideAllItemsSelection = true;
                    break;
            }
        }
    }

    loadFunctions(): ng.IPromise<any> {
        // Functions
        const keys: string[] = [
            "core.saveasdefinitive",
            "core.transfertovoucher",
            "core.exportsop",
            "core.exportdiregnskap",
            "core.exportunimicro",
            "core.transfertopreliminaryinvoice",
            "core.transfertoinvoiceandprint",
            "core.createpayment",
            "core.match",
            "core.exportfortnox",
            "core.exportvismaeaccounting",
            "common.customer.payment.printreminder",
            "common.customer.payment.createreminder",
            "common.customer.invocies.creatererminderandmerge",
            "common.customer.payment.changereminderlevel",
            "common.customer.payment.topreminderlevel",
            "common.customer.payment.createinterestinvoice",
            "common.customer.payment.createinterestinvoicemerge",
            "common.customer.payment.closeinvoice",
            "common.customer.invoices.saveasdefinitiveandprint",
            "common.customer.invoices.printinvoices",
            "common.customer.invoices.saveasdefinitiveandcreateeinvoice",
            "common.customer.invoices.customerinvoice",
            "core.transfertoorder",
            "common.customer.invoice.sendeinvoice",
            "common.customer.invoice.sendemail",
            "common.customer.invoice.downloadeinvoice",
            "common.customer.invoices.printorders",
            "common.customer.payment.emailreminder",
            "core.transfertofinished",
            "common.customer.contracts.updateprices",
            "billing.contract.transfertopreliminaryandmerge",
            "common.customer.payment.printinterestratecalculation",
            "billing.order.deleteorder",
            "billing.invoices.deleteinvoice",
            "common.customer.payment.addremindertonext",
            "common.customer.payment.addinteresttonext",
            "common.customer.invoices.exporttozetes",
            "common.customer.invoices.automaticdistribution",
            "billing.contract.createserviceorder",
            "common.customer.invoices.einvoicehasalreadydownloaded",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            switch (this.classification) {
                case SoeOriginStatusClassification.ContractsRunning:
                    if (this.hasContractToOrderPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToOrder, name: terms["core.transfertoorder"] });
                    if (this.hasContractToInvoicePermission) {
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice, name: terms["core.transfertopreliminaryinvoice"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders, name: terms["billing.contract.transfertopreliminaryandmerge"] });
                    }
                    if (this.modifyPermission) {
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CreateServiceOrderFromAgreement, name: terms["billing.contract.createserviceorder"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.UpdatePrices, name: terms["common.customer.contracts.updateprices"] });
                    }
                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.ContractsAll:
                    if (this.modifyPermission) {
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CreateServiceOrderFromAgreement, name: terms["billing.contract.createserviceorder"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.UpdatePrices, name: terms["common.customer.contracts.updateprices"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CloseContracts, name: terms["core.transfertofinished"] });
                    }
                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.OffersAll:
                    if (this.hasOfferToOrderPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToOrder, name: terms["core.transfertoorder"] });
                    if (this.hasOfferToInvoicePermission) {
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice, name: terms["core.transfertopreliminaryinvoice"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders, name: terms["billing.contract.transfertopreliminaryandmerge"] });
                    }
                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.OrdersAll:
                    if (this.hasTransferToPreliminaryPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice, name: terms["core.transfertopreliminaryinvoice"] });
                    if (this.hasTransferToInvoiceAndMergePermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders, name: terms["billing.contract.transfertopreliminaryandmerge"] });
                    if (this.hasTransferToInvoiceAndPrintPermission && this.transferAndPrint)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndPrint, name: terms["core.transfertoinvoiceandprint"] });
                    if (this.hasDeletePermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.Delete, name: terms["billing.order.deleteorder"] });
                    if (this.hasReportPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.PrintOrder, name: terms["common.customer.invoices.printorders"] });
                    break;
                case SoeOriginStatusClassification.CustomerInvoicesAll:
                    if (this.module === SoeModule.Economy) {
                        if (this.hasDraftToOriginPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.SaveAsDefinitiv, name: terms["core.saveasdefinitive"] });
                        if (this.hasOriginToVoucherPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToVoucher, name: terms["core.transfertovoucher"] });
                        if (this.hasReportPermission && this.defaultBillingInvoiceReportId != 0)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.PrintInvoices, name: terms["common.customer.invoices.printinvoices"] });
                        if (this.hasSOPExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportSOP, name: terms["core.exportsop"] });
                        if (this.hasDIExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportDI, name: terms["core.exportdiregnskap"] });
                        if (this.hasUniExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportUniMicro, name: terms["core.exportunimicro"] });
                        if (this.hasDnBExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportDnB, name: terms["core.exportdnb"] });
                        if (this.hasFortnoxExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportFortnox, name: terms["core.exportfortnox"] });
                        if (this.hasVismaEAccountingPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportVismaEAccounting, name: terms["core.exportvismaeaccounting"] });
                    }
                    else if (this.module === SoeModule.Billing) {
                        if (this.hasDraftToOriginPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.SaveAsDefinitiv, name: terms["core.saveasdefinitive"] });
                        if (this.hasOriginToVoucherPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToVoucher, name: terms["core.transfertovoucher"] });
                        if (this.hasFortnoxExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportFortnox, name: terms["core.exportfortnox"] });
                        if (this.hasVismaEAccountingPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportVismaEAccounting, name: terms["core.exportvismaeaccounting"] });
                        if (this.hasSOPExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportSOP, name: terms["core.exportsop"] });
                        if (this.hasZetesExportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportZetes, name: terms["common.customer.invoices.exporttozetes"] });
                        if (this.hasReportPermission && this.defaultBillingInvoiceReportId != 0) {
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.PrintInvoices, name: terms["common.customer.invoices.printinvoices"] });
                        }

                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.SendAsEmail, name: terms["common.customer.invoice.sendemail"] });

                        if (this.hasSendEInvoicePermission === true) {
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.SendasEInvoice, name: terms["common.customer.invoice.sendeinvoice"] });
                        }
                        if (this.hasDownloadEInvoicePermission === true) {
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.DownloadEInvoice, name: terms["common.customer.invoice.downloadeinvoice"] });
                        }

                        if (this.hasDeletePermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.Delete, name: terms["billing.invoices.deleteinvoice"] });

                        if ((this.hasSendEInvoicePermission || this.hasDownloadEInvoicePermission) && this.hasReportPermission)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.AutomaticDistribution, name: terms["common.customer.invoices.automaticdistribution"] });
                    }

                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.CustomerPaymentsUnpayed:

                    //Make sure button is visible
                    this.showSplitButton = true;

                    if (this.hasOriginToPaymentPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CreatePaymentFile, name: terms["core.createpayment"] });

                    if (this.hasZetesExportPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ExportZetes, name: terms["common.customer.invoices.exporttozetes"], disabled: () => { return !this.loadClosed } });

                    this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.Match, name: terms["core.match"] });

                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.CustomerInvoicesReminder:
                    //Make sure button is visible
                    this.showSplitButton = true;

                    this.hasOpenPermission = false;
                    this.hasClosedPermission = false;
                    if (this.hasIntrestReminderPermission) {
                        this.buttonFunctions.push({
                            id: CustomerInvoiceGridButtonFunctions.CreateReminder, name: this.addReminderToNextInvoice ? terms["common.customer.payment.addremindertonext"] : terms["common.customer.payment.createreminder"]
                        });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.PrintReminder, name: terms["common.customer.payment.printreminder"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.SendReminderAsEmail, name: terms["common.customer.payment.emailreminder"] });

                        if (this.hasReportPermission && !this.addReminderToNextInvoice)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CreateReminderAndMerge, name: terms["common.customer.invocies.creatererminderandmerge"] });

                        if (this.reminderNoOfClaimLevels > 0) {
                            var noOfClaimLevels = this.reminderNoOfClaimLevels > TermGroup_InvoiceClaimLevel.Collection ? TermGroup_InvoiceClaimLevel.Collection : this.reminderNoOfClaimLevels;

                            var i: number = 0;
                            while (i <= noOfClaimLevels) {
                                this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ChangeReminderLevel, name: terms["common.customer.payment.changereminderlevel"] + " " + i.toString(), level: i });
                                i++;
                            }

                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.ChangeReminderLevel, name: terms["common.customer.payment.changereminderlevel"] + " " + terms["common.customer.payment.topreminderlevel"], level: TermGroup_InvoiceClaimLevel.Collection });
                        }
                    }

                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.CustomerInvoicesInterest:
                    //Make sure button is visible
                    this.showSplitButton = true;
                    if (this.hasIntrestReminderPermission) {
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CreateInterestInvoice, name: this.addInterestToNextInvoice ? terms["common.customer.payment.addinteresttonext"] : terms["common.customer.payment.createinterestinvoice"] });
                        if (!this.addInterestToNextInvoice)
                            this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CreateInterestInvoiceMerge, name: terms["common.customer.payment.createinterestinvoicemerge"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.PrintInterestRateCalculation, name: terms["common.customer.payment.printinterestratecalculation"] });
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.CloseInvoice, name: terms["common.customer.payment.closeinvoice"] });
                    }

                    break;
                case SoeOriginStatusClassification.CustomerPaymentsPayed:
                    //Make sure button is visible
                    this.showSplitButton = true;

                    if (this.hasPaymentToVoucherPermission)
                        this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferPaymentToVoucher, name: terms["core.transfertovoucher"] });

                    if (this.buttonFunctions.length > 0)
                        this.splitButtonSelectedOption = this.buttonFunctions[0];
                    break;
                case SoeOriginStatusClassification.CustomerPaymentsVoucher:
                    this.showSplitButton = false;

                    break;
            }
        });
    }
}