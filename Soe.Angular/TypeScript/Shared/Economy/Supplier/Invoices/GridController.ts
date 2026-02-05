import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { SupplierInvoiceGridDTO, UpdateEdiEntryDTO, SupplierInvoiceDTO } from "../../../../Common/Models/InvoiceDTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { IAddInvoiceToAttestFlowService } from "../../../../Common/Dialogs/addinvoicetoattestflow/addinvoicetoattestflowservice";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SupplierGridButtonFunctions, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { FlaggedEnum } from "../../../../Util/EnumerationsUtility";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { ISmallGenericType, IActionResult } from "../../../../Scripts/TypeLite.Net4";
import { InvoiceUtility } from "../../../../Util/InvoiceUtility";
import { AddInvoiceToAttestFlowController } from "../../../../Common/Dialogs/addinvoicetoattestflow/addinvoicetoattestflowcontroller";
import { SoeOriginStatusChange, Feature, SoeEntityType, TermGroup_EDISourceType, TermGroup_ScanningInterpretation, TermGroup_EDIStatus, TermGroup_EDIInvoiceStatus, SoeStatusIcon, ActionResultSave, SettingMainType, UserSettingType, SoeReportTemplateType, TermGroup_ProjectType, CompanySettingType, TermGroup, TermGroup_AttestEntity, SoeModule, TermGroup_ChangeStatusGridAllItemsSelection, SoeOriginType, SoeOriginStatus, TermGroup_SupplierInvoiceType, TermGroup_BillingType, SoePaymentStatus, AzoraOneStatus } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { EditController as SupplierInvoicesEditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { AccountDimSmallDTO } from "../../../../Common/Models/AccountDimDTO";
import { ShowAttestCommentsAndAnswersDialogController } from "../../../../Common/Dialogs/ShowAttestCommentsAndAnswersDialog/ShowAttestCommentsAndAnswersDialogController";
import { Guid } from "../../../../Util/StringUtility";
import { IRequestReportService } from "../../../Reports/RequestReportService";
import { BalanceListPrintDTO } from "../../../../Common/Models/RequestReports/BalanceListPrintDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups
    items: SupplierInvoiceGridDTO[];
    allItems: SupplierInvoiceGridDTO[];
    allItemsSelectionDict: any[];
    invoiceTypes: any[];
    invoiceBillingTypes: any[];
    originStatus: any[];
    attestStates: any[];
    hiddenAttestStates: any[];
    hasHiddenAttestState = false;
    attestGroups: any[];
    suppliers: any[];
    scanningUnprocessedCount = 0;
    currencyCodes: any[];
    accountDims: AccountDimSmallDTO[];

    // Permissions
    hasCurrencyPermission = false;
    hasOpenPermission = false;
    hasClosedPermission = false;
    hasDraftToOriginPermission = false;
    hasOriginToVoucherPermission = false;
    hasScanningPermission = false;
    hasAttestAdminPermission = false;
    hasAttestAddPermission = false;
    hasSendAttestRemindersPermission = false;
    hasEDIPermission = false;
    hasFinvoicePermission = false;
    uploadImagePermission = false;
    hasAttestFlowPermission = false;

    // Variables
    lookups: number;
    activated: boolean;
    showOnlyMine: boolean;
    openIsLoaded: boolean;
    closedIsLoaded: boolean;
    setupComplete: boolean;
    selectedInvoiceDate: Date = null;
    selectedDueDate: Date = null;
    filteredTotal = 0;
    selectedTotal = 0;
    filteredTotalExVat = 0;
    selectedTotalIncVat = 0;
    filteredTotalIncVat = 0;
    selectedTotalExVat = 0;
    showVatFree = true;
    attestWorkFlowHead: any;
    isProjectCentral = false;
    isImagesUploaded = false;
    doReload = false;
    invoiceJournalReportId = 0;

    //Terms
    terms: { [index: string]: string; };
    noAttestStateTerm: string;
    attestRejectedTerm: string;

    //Accountyear
    currentAccountYearId = 0;
    currentAccountYearFromDate: Date;
    currentAccountYearToDate: Date;

    //Compsetting
    supplierInvoiceTransferToVoucher = false;
    supplierInvoiceAskPrintVoucherOnTransfer = false;
    supplierCloseInvoicesWhenTransferredToVoucher = false;
    supplierBalanceListReportId = 0;
    userIdNeededWithTotalAmount = 0;
    totalAmountWhenUserReguired = 0;
    defaultAttestGroupId = 0;
    supplierInvoiceVoucherReportId = 0;
    supplierUseTimeDiscount = false;
    coreBaseCurrency = 0;
    showTransactionCurrency = false;
    showEnterpriseCurrency = false;
    showLedgerCurrency = false;
    showGetEInvoices = false;
    usesAzoraOneScanning: AzoraOneStatus = AzoraOneStatus.Deactivated;

    //Project central
    projectId: number;
    includeChildProjects: boolean;
    invoices: number[];
    fromDate: Date;
    toDate: Date;

    invoice: SupplierInvoiceDTO[];

    // Properties
    private _loadOpen: any;
    get loadOpen() {
        return this._loadOpen;
    }
    set loadOpen(item: any) {
        this._loadOpen = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _loadClosed: any;
    get loadClosed() {
        return this._loadClosed;
    }
    set loadClosed(item: any) {
        this._loadClosed = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete === true)
            this.updateItemsSelection();
    }

    private _selectedItems: any[];
    get selectedItems() {
        return this._selectedItems;
    }
    set selectedItems(item: any[]) {
        this._selectedItems = item;
        if (this.setupComplete === true)
            this.updateSelectedItems();
    }

    get showSearchButton() {
        return this.allItemsSelection === 999;
    }

    // Flags - special one for 
    hasEditReadPermission: boolean;

    // Functions
    buttonFunctions: any = [];

    //Transfer
    buttonOption: any;
    ignoreDateValidation: boolean = false;

    //StatusChange
    originStatusChange: SoeOriginStatusChange;

    // Grid header and footer
    toolbarInclude: any;
    gridFooterComponentUrl: any;

    // Polling
    timerToken: any;
    currentGuid: Guid;

    //modal
    private modalInstance: any;

    private isSupplierBalanceListPrinting = false;
    private isSupplierInvoiceJournalPrinting = false;

    //@ngInject
    constructor(
        private $scope,
        private $window,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private supplierService: ISupplierService,
        private addInvoiceToAttestFlowService: IAddInvoiceToAttestFlowService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private $q: ng.IQService,
        private readonly requestReportService: IRequestReportService) {

        super(gridHandlerFactory, "Economy.Supplier.Invoices" + "_" + Feature.Economy_Supplier_Invoice_Invoices_Edit, progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadSettings(() => this.doLoadSettings())
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.initSetupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.onTabActivetedAndModified(() => this.loadGridData());

        //this.soeGridOptions.enableFullRowSelection = false;
        this.setupComplete = false;
        this.showOnlyMine = false;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;

        this.init();

        if (this.parameters.setup) {
            this.flowHandler.start(this.getPermissions());
            this.activated = true;
        }

        /*
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {
                if (this.setupComplete)
                    this.loadGridData();
            });
        }
        */
    }

    private getPermissions(): any[] {
        var features: any[] = [];

        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices_All, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_Foreign, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_OriginToVoucher, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Scanning, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_Admin, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_Add, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Import_EDI, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AddImage, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Finvoice, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_Edit, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow, loadReadPermissions: true, loadModifyPermissions: true });

        return features;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Supplier_Invoice_Invoices].readPermission || response[Feature.Economy_Supplier_Invoice_Invoices].modifyPermission;
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission || response[Feature.Economy_Supplier_Invoice_Invoices_Edit].readPermission;

        if (response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission)
            this.messagingHandler.publishActivateAddTab();

        this.hasCurrencyPermission = response[Feature.Economy_Supplier_Invoice_Status_Foreign].modifyPermission;
        this.hasDraftToOriginPermission = response[Feature.Economy_Supplier_Invoice_Status_DraftToOrigin].modifyPermission;
        this.hasOriginToVoucherPermission = response[Feature.Economy_Supplier_Invoice_Status_OriginToVoucher].modifyPermission;
        this.hasScanningPermission = response[Feature.Economy_Supplier_Invoice_Scanning].modifyPermission;
        this.hasAttestAdminPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Admin].modifyPermission;
        this.hasAttestAddPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Add].modifyPermission;
        this.hasSendAttestRemindersPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Edit].modifyPermission;
        this.hasEDIPermission = response[Feature.Billing_Import_EDI].modifyPermission;
        this.hasFinvoicePermission = response[Feature.Economy_Supplier_Invoice_Finvoice].modifyPermission;
        this.uploadImagePermission = response[Feature.Economy_Supplier_Invoice_AddImage].modifyPermission;
        this.hasAttestFlowPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow].readPermission || response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;


        if (response[Feature.Economy_Supplier_Invoice_Invoices_All].modifyPermission) {
            this.loadOpen = true;
            this.loadClosed = false;
            this.hasOpenPermission = true;
            this.hasClosedPermission = true;
        }
        else {
            if (response[Feature.Economy_Supplier_Invoice_Invoices].modifyPermission || this.hasDraftToOriginPermission || this.hasOriginToVoucherPermission) {
                this.loadOpen = true;
                this.hasOpenPermission = true;
            }
            if (response[Feature.Economy_Supplier_Invoice_Invoices].modifyPermission || this.hasOriginToVoucherPermission) {
                this.loadClosed = false;
                this.hasClosedPermission = true;
            }
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
            this.loadCurrentAccountYear(),
            this.loadSelectionTypes(),
            this.loadInvoiceTypes(),
            this.loadInvoiceBillingTypes(),
            this.loadOriginStatus(),
            this.loadSuppliers(),
            this.loadAttestStates(),
            this.getHasHiddenAttestState(),
            this.loadAttestGroups(),
            this.loadScanningUnprocessedCount(),
            this.loadEdiStatuses(),
            this.loadScanningStatuses(),
            this.loadCurrencies(),
            this.loadAccounts(true),
            this.loadInvoiceJournalReportId(),
        ]).then(() => {
            this.setupComplete = true;
            this.lookupSupplierInvoice();
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.toolbarInclude);
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

    protected init() {
        this.currentAccountYearId = soeConfig.accountYearId;
        this.isProjectCentral = this.parameters.isProjectCentral;

        if (this.isProjectCentral) {
            this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("shared/economy/supplier/invoices/views/gridFooter.html");

            this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
                this.projectId = x.projectId;
                this.includeChildProjects = x.includeChildProjects;
                this.invoices = x.supplierInvoices;
                this.fromDate = x.fromDate;
                this.toDate = x.toDate;

                if (this.activated)
                    this.doReload = true;
            });
        }
        else {
            this.toolbarInclude = this.urlHelperService.getGlobalUrl("shared/economy/supplier/invoices/views/gridHeader.html");
            this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("shared/economy/supplier/invoices/views/gridFooter.html"); this.messagingService.publish(Constants.EVENT_RELOAD_ATTEST_FLOW_OVERVIEW, {});

            // Using this event to reload after changes to attest
            this.messagingService.subscribe(Constants.EVENT_RELOAD_ATTEST_FLOW_OVERVIEW, (x) => {
                this.reloadGridFromFilter();
            });
        }

        this.messagingService.subscribe(Constants.EVENT_TAB_ACTIVATED, (x) => {
            this.onControllActivated(x);
        });

        this.$scope.$on('onTabActivated', (e, a) => {
            this.onControllActivated(a);
        });
    }

    protected initSetupGrid() {
        this.setupToolbar();
        this.setupInvoiceGrid();
    }

    protected setupToolbar() {
        if (this.toolbar) {
            // Print
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.printbalance", "economy.supplier.invoice.printbalance", IconLibrary.FontAwesome, "fa-print", () => {
                this.printSelectedInvoices(SoeReportTemplateType.SupplierBalanceList);
            }, () => {
                return this.gridAg.options.getSelectedCount() === 0
                    || this.isSupplierBalanceListPrinting;
            }, () => {
                    return (!this.invoiceJournalReportId || this.invoiceJournalReportId === 0);
                })));
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.invoicejournal", "economy.supplier.invoice.invoicejournal", IconLibrary.FontAwesome, "fa-print", () => {
                this.printSelectedInvoices(SoeReportTemplateType.SupplierInvoiceJournal);
            }, () => {
                return this.gridAg.options.getSelectedCount() === 0
                    || this.isSupplierInvoiceJournalPrinting;
            }, () => {
                return this.gridAg.options.getSelectedCount() === 0;
            })));
                       

            //upload images
            if (this.uploadImagePermission) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.uploadimages", "economy.supplier.invoice.uploadimages", IconLibrary.FontAwesome, "fa-upload", () => {
                    this.uploadImages();
                }, null, () => {
                    return this.isProjectCentral;
                })));
            }

            if (this.showGetEInvoices) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.getEinvoices", "economy.supplier.invoice.getEinvoices", IconLibrary.FontAwesome, "fa-download", () => {
                    this.getEInvoices(TermGroup_EDISourceType.InExchange);
                }, null, () => {
                    return this.isProjectCentral;
                })));
            }
        }

        // TODO: Change translation key from core to economy.supplier.invoice

        // Functions
        const keys: string[] = [
            "core.save",
            "core.saveasdefinitive",
            "core.transfertovoucher",
            "core.createsupplierInvoicefromedi",
            "core.transfertoclosed",
            "core.transfertodeleted",
            "core.addtoattestflow",
            "core.startattestflow",
            "core.sendattestreminder",
            "economy.supplier.invoice.hideunhandled",
            "core.createpdffromedi",
            "economy.supplier.invoice.downloadinvoiceimages"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (!this.isProjectCentral) {
                if (this.hasDraftToOriginPermission)
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.SaveAsDefinitiv, name: terms["core.saveasdefinitive"] });
                if (this.hasOriginToVoucherPermission)
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.TransferToVoucher, name: terms["core.transfertovoucher"] });
                if (this.hasEDIPermission || this.hasFinvoicePermission)
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.CreateInvoiceOutOfEdi, name: terms["core.createsupplierInvoicefromedi"] });
                if (this.hasEDIPermission)
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.CreatePDF, name: terms["core.createpdffromedi"] });
                this.buttonFunctions.push({ id: SupplierGridButtonFunctions.Save, name: terms["core.save"] });
                this.buttonFunctions.push({ id: SupplierGridButtonFunctions.CloseEdi, name: terms["core.transfertoclosed"] });
                this.buttonFunctions.push({ id: SupplierGridButtonFunctions.RemoveDraftOrEdi, name: terms["core.transfertodeleted"] });
                if (this.hasAttestAdminPermission || this.hasAttestAddPermission) {
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.AddToAttestFlow, name: terms["core.addtoattestflow"] });
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.StartAttestFlow, name: terms["core.startattestflow"] });
                }
                if (this.hasAttestAdminPermission || this.hasSendAttestRemindersPermission)
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.SendAttestReminder, name: terms["core.sendattestreminder"] });
                if (this.hasHiddenAttestState)
                    this.buttonFunctions.push({ id: SupplierGridButtonFunctions.HideUnhandled, name: terms["economy.supplier.invoice.hideunhandled"] });
            }
            this.buttonFunctions.push({ id: SupplierGridButtonFunctions.PrintInvoiceImages, name: terms["economy.supplier.invoice.downloadinvoiceimages"] });
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.edit",
            "core.showinfo",
            "core.continue",
            "common.type",
            "common.imported",
            "core.save",
            "core.error",
            "common.hasaattachedfiles",
            "common.hasattachedimages",
            "economy.supplier.invoice.seqnr",
            "economy.supplier.invoice.invoicenr",
            "economy.supplier.invoice.invoicetype",
            "common.tracerows.status",
            "economy.supplier.supplier.suppliernr.grid",
            "economy.supplier.supplier.suppliername.grid",
            "economy.supplier.invoice.amountexvat",
            "economy.supplier.invoice.amountincvat",
            "economy.supplier.invoice.remainingamount",
            "economy.supplier.invoice.foreignamount",
            "economy.supplier.invoice.foreignremainingamount",
            "economy.supplier.invoice.currencycode",
            "economy.supplier.invoice.invoicedate",
            "economy.supplier.invoice.duedate",
            "common.customer.invoices.paydate",
            "economy.supplier.invoice.voucherdate",
            "economy.supplier.invoice.attest",
            "economy.supplier.invoice.attestname",
            "economy.supplier.invoice.attestgroup",
            "economy.supplier.invoice.openpdf",
            "economy.supplier.invoice.showscannedstate",
            "common.customer.invoices.invoicedatesnotentered",
            "common.customer.invoices.invoicedatesnotwithincurrentaccountyear",
            "common.customer.invoices.duedatesnotentered",
            "common.customer.invoices.duedatesnotwithincurrentaccountyear",
            "common.customer.invoices.autotovoucher",
            "economy.supplier.payment.validinvoices",
            "economy.supplier.payment.validinvoice",
            "common.customer.invoices.drafttooriginvalid",
            "common.customer.invoices.drafttoorigininvalid",
            "common.customer.invoices.origintovouchervalid",
            "common.customer.invoices.origintovoucherinvalid",
            "economy.supplier.invoice.messagefromoperator",
            "economy.supplier.invoice.invoicestatus",
            "economy.supplier.invoice.suppliermissing",
            "economy.supplier.invoice.invoicenrmissing",
            "economy.supplier.invoice.retrievedate",
            "economy.supplier.invoice.messagetype",
            "economy.accounting.currency.source",
            "economy.supplier.invoice.currency",
            "common.errormessage",
            "common.reportsettingmissing",
            "economy.supplier.invoice.nonewscanninginvoices",
            "economy.supplier.invoice.scanningvalue",
            "economy.supplier.invoice.scanningvalues",
            "economy.supplier.invoice.scanningvaild",
            "economy.supplier.invoice.scanninginsecure",
            "economy.supplier.invoice.scanninginvalid",
            "core.warning",
            "economy.supplier.payment.defaultVoucherListMissing",
            "economy.supplier.payment.voucherscreated",
            "economy.supplier.payment.askPrintVoucher",
            "economy.supplier.invoice.savedasoriginsingle",
            "economy.supplier.invoice.savedasoriginmultiple",
            "economy.supplier.invoice.failedsaveasorigin",
            "economy.supplier.invoice.voucherscreated",
            "economy.supplier.invoice.createvouchersfailed",
            "economy.supplier.invoice.createinvoice",
            "common.customer.invoices.showinvoice",
            "economy.supplier.invoice.invoice",
            "economy.supplier.invoice.invoices",
            "economy.supplier.invoice.partlypaid",
            "economy.supploer.invoice.invoiceimported",
            "economy.supplier.invoice.partlypaid",
            "economy.supplier.invoice.paidlate",
            "economy.supplier.invoice.matches.totalamount",
            "economy.supplier.payment.paymentamount",
            "economy.supplier.invoice.hastimediscount",
            "economy.supplier.invoice.amounttopay",
            "economy.supplier.invoice.timediscount",
            "economy.supplier.invoice.missinginterpretation",
            "common.invoicedrafttooriginvalid",
            "common.invoicesdrafttooriginvalid",
            "common.invoicedrafttoorigininvalid",
            "common.invoicesdrafttoorigininvalid",
            "common.invoicesavedasorigin",
            "common.invoicessavedasorigin",
            "common.invoicesavedasoriginfailed",
            "common.invoicessavedasoriginfailed",
            "common.invoiceorigintovouchervalid",
            "common.invoicesorigintovouchervalid",
            "common.invoiceorigintovoucherinvalid",
            "common.invoicesorigintovoucherinvalid",
            "common.invoicetransfertovoucherfailed",
            "common.invoicestransfertovoucherfailed",
            "common.editransfertoinvoicevalid",
            "common.edistransfertoinvoicevalid",
            "common.editransfertoinvoiceinvalid",
            "common.edistransfertoinvoiceinvalid",
            "common.invoicewascreated",
            "common.invoiceswascreated",
            "common.editransfertoinvoicefailed",
            "common.edistransfertoinvoicefailed",
            "common.ediclosevalid",
            "common.edisclosevalid",
            "common.edicloseinvalid",
            "common.ediscloseinvalid",
            "common.ediclosed",
            "common.edisclosed",
            "common.ediclosefailed",
            "common.edisclosefailed",
            "common.edideletevalid",
            "common.edisdeletevalid",
            "common.edideleteinvalid",
            "common.edisdeleteinvalid",
            "common.edideleted",
            "common.edisdeleted",
            "common.edideletefailed",
            "common.edisdeletefailed",
            "economy.supplier.invoice.saveedichangesvalid",
            "economy.supplier.invoice.saveedischangesvalid",
            "economy.supplier.invoice.saveedichangesinvalid",
            "economy.supplier.invoice.saveedischangesinvalid",
            "economy.supplier.invoice.edichangessaved",
            "economy.supplier.invoice.edischangessaved",
            "economy.supplier.invoice.edichangessavedfailed",
            "economy.supplier.invoice.edischangessavedfailed",
            "economy.supplier.invoice.noatteststate",
            "economy.supplier.invoice.attestrejected",
            "common.invoicesavedasoriginseqnr",
            "common.invoicessavedasoriginseqnr",
            "economy.supplier.invoice.hideunhandled.askinvoice",
            "economy.supplier.invoice.hideunhandled.askinvoices",
            "economy.supplier.invoice.hideunhandled.invalidinvoice",
            "economy.supplier.invoice.hideunhandled.invalidinvoices",
            "common.createpdfvalid",
            "common.createpdfsvalid",
            "common.postinvalid",
            "common.postsinvalid",
            "common.pdfcreated",
            "common.pdfscreated",
            "common.pdferror",
            "common.pdfserror",
            "economy.supplier.invoice.description",
            "economy.supplier.invoice.custom",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "common.text",
            "common.quantity",
            "common.unit",
            "common.accountingrows.rownr",
            "common.debit",
            "common.credit",
            "common.debitcurrency",
            "common.creditcurrency",
            "common.debitentcurrency",
            "common.creditentcurrency",
            "common.debitledgercurrency",
            "common.creditledgercurrency",
            "economy.supplier.invoice.falsebillingtypemessage",
            "economy.supplier.invoice.ocr",
            "economy.supplier.invoice.importedscanningitem",
            "economy.supplier.invoice.importedscanningitems",
            "economy.supplier.invoice.importedscanningitemfailed",
            "economy.supplier.invoice.importedscanningitemsfailed",
            "core.attestflowregistered",
            "economy.supplier.invoice.liquidityplanning.sequencenr",
            "common.noone",
            "economy.supplier.invoice.paymentstatuses",
            "common.customer.invoices.invoicepaid",
            "economy.supplier.attestflowoverview.invoiceunpaid",
            "common.customer.invoices.invoicepartlypaid",
            "economy.supplier.invoice.invoicepaidbutnotchecked",
            "economy.supplier.payment.totalamount",
            "economy.supplier.payment.paymentamount",
            "economy.supplier.invoice.paid2",
            "common.amount",
            "common.created",
            "economy.supplier.invoice.paidshort",
            "economy.supplier.invoice.paidbutnotcheckedshort",
            "economy.supplier.invoice.partlypaidshort",
            "economy.supplier.invoice.unpaidshort",
            "common.reason",
            "economy.supplier.invoice.failedtransfertovoucher1",
            "economy.supplier.invoice.failedtransfertovoucher2",
            "common.dailyrecurrencepattern.startdate",
            "economy.supplier.invoice.sumlinkedtoproject",
            "common.customer.invoices.inexchangevalidation",
            "economy.supplier.invoice.sumlinkedtoorder",
            "economy.supplier.invoice.documentprovider",
            "economy.supplier.invoice.usingnewscanningflowconfirmation"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.noAttestStateTerm = this.terms["economy.supplier.invoice.noatteststate"];
            this.attestRejectedTerm = this.terms["economy.supplier.invoice.attestrejected"];

        });
    }

    public setupInvoiceGrid() {

        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadSupplierInvoiceAccountingRows(params);
        });

        this.gridAg.detailOptions.addColumnNumber("rowNr", this.terms["common.accountingrows.rownr"], 10);
        this.accountDims.forEach((ad, i) => {
            let index = i + 1;
            this.gridAg.detailOptions.addColumnText("dim" + index + "Name", ad.name, null);
        });
        this.gridAg.detailOptions.addColumnText("text", this.terms["common.text"], null);
        this.gridAg.detailOptions.addColumnNumber("debitAmount", this.terms["common.debit"], null, { enableHiding: false, decimals: 2 });
        this.gridAg.detailOptions.addColumnNumber("creditAmount", this.terms["common.credit"], null, { enableHiding: false, decimals: 2 });
        if (this.showTransactionCurrency) {
            this.gridAg.detailOptions.addColumnNumber("debitAmountCurrency", this.terms["common.debitcurrency"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("creditAmountCurrency", this.terms["common.creditcurrency"], null, { enableHiding: false, decimals: 2 });
        }
        if (this.showLedgerCurrency) {
            this.gridAg.detailOptions.addColumnNumber("debitAmountEntCurrency", this.terms["common.debitentcurrency"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("creditAmountEntCurrency", this.terms["common.creditentcurrency"], null, { enableHiding: false, decimals: 2 });
        }
        if (this.showEnterpriseCurrency) {
            this.gridAg.detailOptions.addColumnNumber("debitAmountLedgerCurrency", this.terms["common.debitledgercurrency"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("creditAmountLedgerCurrency", this.terms["common.creditledgercurrency"], null, { enableHiding: false, decimals: 2 });
        }

        this.gridAg.detailOptions.finalizeInitGrid();

        //this.setupTypeAhead(); ??
        this.gridAg.addColumnSelect("typeName", this.terms["common.type"], null, {
            enableHiding: true,
            displayField: "typeName",
            selectOptions: this.invoiceTypes,
            cellClassRules:
            {
                "scanning-cell-azoraone": (gridRow: any) => gridRow.data.ediType === TermGroup_EDISourceType.Scanning && gridRow.data.ediMessageProviderName === "AzoraOne",
                "scanning-cell-success": (gridRow: any) => gridRow.data.ediType ===  TermGroup_EDISourceType.Scanning && gridRow.data.roundedInterpretation == TermGroup_ScanningInterpretation.ValueIsValid,
                "scanning-cell-warning": (gridRow: any) => gridRow.data.ediType === TermGroup_EDISourceType.Scanning && gridRow.data.roundedInterpretation == TermGroup_ScanningInterpretation.ValueIsUnsettled,
                "scanning-cell-error": (gridRow: any) => gridRow.data.ediType === TermGroup_EDISourceType.Scanning && gridRow.data.roundedInterpretation == TermGroup_ScanningInterpretation.ValueNotFound
            }
        });

        this.gridAg.addColumnNumber("seqNr", this.terms["economy.supplier.invoice.liquidityplanning.sequencenr"], null, {alignLeft: true, formatAsText: true} );
        this.gridAg.addColumnText("invoiceNr", this.terms["economy.supplier.invoice.invoicenr"], null);
        this.gridAg.addColumnSelect("billingTypeName", this.terms["economy.supplier.invoice.invoicetype"], null, { enableHiding: true, displayField: "billingTypeName", selectOptions: this.invoiceBillingTypes });
        this.gridAg.addColumnSelect("statusName", this.terms["common.tracerows.status"], null, { enableHiding: true, displayField: "statusName", selectOptions: this.originStatus });
        this.gridAg.addColumnText("supplierNr", this.terms["economy.supplier.supplier.suppliernr.grid"], null, true);
        this.gridAg.addColumnText("supplierName", this.terms["economy.supplier.supplier.suppliername.grid"], null, true);
        this.gridAg.addColumnText("internalText", this.terms["economy.supplier.invoice.description"], null, true);
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["economy.supplier.invoice.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("totalAmount", this.terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("payAmount", this.terms["economy.supplier.invoice.remainingamount"], null, { enableHiding: true, decimals: 2 });
        if (this.hasCurrencyPermission) {
            this.gridAg.addColumnNumber("totalAmountCurrency", this.terms["economy.supplier.invoice.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("payAmountCurrency", this.terms["economy.supplier.invoice.foreignremainingamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnSelect("currencyCode", this.terms["economy.supplier.invoice.currencycode"], null, { enableHiding: true, displayField: "currencyCode", selectOptions: this.currencyCodes });
        }
        this.gridAg.addColumnDate("invoiceDate", this.terms["economy.supplier.invoice.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["economy.supplier.invoice.duedate"], null, true);
        this.gridAg.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null, true);
        this.gridAg.addColumnDate("voucherDate", this.terms["economy.supplier.invoice.voucherdate"], null, true);
        this.gridAg.addColumnText("ocr", this.terms["economy.supplier.invoice.ocr"], null, true);
        if (this.hasAttestFlowPermission) {
            this.gridAg.addColumnSelect("attestStateName", this.terms["economy.supplier.invoice.attest"], null, { enableHiding: true, displayField: "attestStateName", selectOptions: this.attestStates });
            this.gridAg.addColumnText("currentAttestUserName", this.terms["economy.supplier.invoice.attestname"], null, true);
            this.gridAg.addColumnSelect("attestGroupName", this.terms["economy.supplier.invoice.attestgroup"], null, { enableHiding: true, editable: true, displayField: "attestGroupName", selectOptions: this.attestGroups, onChanged: this.attestGroupChanged.bind(this) });
        }

        this.gridAg.addColumnIcon("paymentStatusIcon", null, null, { suppressSorting: false, enableHiding: true, toolTipField: "paymentStatusTooltip", hide: true, showTooltipFieldInFilter: true, onClick: this.showPayments.bind(this) });
        this.gridAg.addColumnIcon("pdfIcon", null, null, { icon: "fal fa-file", onClick: this.showPdf.bind(this), showIcon: this.showPdfIcon.bind(this), toolTip: this.terms["economy.supplier.invoice.openpdf"] });
        if (this.modifyPermission) {
            this.gridAg.addColumnIcon("showCreateInvoiceIcon", null, null, { onClick: this.showInvoice.bind(this), toolTipField: "showCreateInvoiceTooltip" });
        }
        if (this.isProjectCentral) {
            this.gridAg.addColumnNumber("projectInvoicedAmount", this.terms["economy.supplier.invoice.sumlinkedtoorder"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("projectAmount", this.terms["economy.supplier.invoice.sumlinkedtoproject"], null, {enableHiding: true, decimals: 2});
        }
        this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-comment-dots", showIcon: this.showAttestCommentIcon.bind(this), onClick: this.showAttestCommentDialog.bind(this) });
        this.gridAg.addColumnIcon("infoIconValue", null, null, { onClick: this.showInformationMessage.bind(this), toolTipField: "infoIconTooltip" });
        this.gridAg.addColumnIcon("blockIcon", null, null, { onClick: this.showBlockReason.bind(this), toolTipField: "blockReason" });

        this.gridAg.options.getColumnDefs().forEach(f => {
            // Append closedRow to cellClass
            var cellcls: string = f.cellClass ? f.cellClass.toString() : "";
            if (f.field === "dueDate") {
                f.cellClass = (grid: any) => {
                    if (grid.data.useClosedStyle)
                        return cellcls + " closedRow";
                    else if (grid.data.isOverdue)
                        return cellcls + " errorRow";
                    else if (grid.data.blockPayment)
                        return cellcls + " warningRow";
                    else
                        return cellcls;
                };
            }
            else {
                f.cellClass = (grid: any) => {
                    if (grid.data.useClosedStyle)
                        return cellcls + " closedRow";
                    else if (grid.data.blockPayment)
                        return cellcls + " warningRow";
                    else
                        return cellcls;
                };
            }

            if (f.field === "supplierId")
                f.cellEditableCondition = (node) => { return !node.data.isReadOnly && node.data.ediType === TermGroup_EDISourceType.Scanning; }

            if (f.field === "attestGroupId")
                f.cellEditableCondition = (node) => { return !node.data.isReadOnly && node.data.ediType === TermGroup_EDISourceType.Scanning; }
        });

        // Subscribe to grid events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => { this.summarizeFiltered(rows); }));
        this.gridAg.options.subscribe(events);

        /**/

        this.gridAg.finalizeInitGrid("economy.supplier.invoice.invoice",true);
    }

    public rowDataTest(rowData: any) {
        console.log(rowData);
        return true
    }

    public filtersuppliers(filter) {
        return this.suppliers.filter(acc => {
            return acc.name.contains(filter);
        });
    }
    public loadGridData() {
        if (!this.setupComplete)
            return;

        if (this.isProjectCentral) {
            if (this.projectId && this.projectId > 0)
                this.loadGridDataForProjectCentral(this.projectId, this.includeChildProjects)
            /*else
                super.gridDataLoaded(null);*/
        }
        else {
            this.loadInvoices();
        }
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public loadInvoices() {
        if (this.allItemsSelection === 999)
            return;

        // Load data
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getInvoicesForGrid(this.allItemsSelection, this.loadOpen, this.loadClosed).then((x) => {
                this.allItems = x;
                const date = new Date();
                const dateToday: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0);

                //_.forEach(this.allItems, (y) => {
                for (const y of this.allItems) {
                    if (y.type === TermGroup_SupplierInvoiceType.Invoice)
                        y['expander'] = "";

                    y.payDate = y.payDate ? new Date(<any>y.payDate).date() : null;
                    y.invoiceDate = y.invoiceDate ? new Date(<any>y.invoiceDate).date() : null;
                    y.dueDate = y.dueDate ? new Date(<any>y.dueDate).date() : null;
                    y.voucherDate = y.voucherDate ? new Date(<any>y.voucherDate).date() : null;
                    y.timeDiscountDate = y.timeDiscountDate ? new Date(<any>y.timeDiscountDate).date() : null;

                    if (y.status === SoeOriginStatus.Draft || y.ediType === TermGroup_EDISourceType.Scanning || y.type === TermGroup_SupplierInvoiceType.Uploaded)
                        y.seqNr = undefined;

                    if (y.fullyPaid == false && y.timeDiscountPercent != null && y.timeDiscountPercent != 0 && y.timeDiscountDate != null && dateToday.isSameOrBeforeOnDay(y.timeDiscountDate)) {
                        y.payAmount = ((100 - y.timeDiscountPercent) / 100 * y.payAmount);
                        y.payAmountCurrency = ((100 - y.timeDiscountPercent) / 100 * y.payAmountCurrency);
                    }

                    if (!y.attestStateName)
                        y.attestStateName = this.noAttestStateTerm;

                    if (y.isAttestRejected)
                        y.attestStateName = this.attestRejectedTerm;

                    y.expandableDataIsLoaded = false;

                    if (y.ediType && y.ediType > 0) {
                        if (y.ediType === TermGroup_EDISourceType.Scanning) {
                            y.showCreateInvoiceIcon = "fal fa-plus iconEdit";
                            y.showCreateInvoiceTooltip = this.terms["economy.supplier.invoice.createinvoice"];
                        }
                        else if (y.ediType == TermGroup_EDISourceType.Finvoice) {
                            y['interpretationStateName'] = this.terms["common.noone"];
                            y.showCreateInvoiceIcon = "fal fa-plus iconEdit";
                            y.showCreateInvoiceTooltip = this.terms["economy.supplier.invoice.createinvoice"];
                        }
                    }
                    else {
                        y['interpretationStateName'] = this.terms["common.noone"];
                        y.showCreateInvoiceIcon = "fal fa-pencil iconEdit";
                        y.showCreateInvoiceTooltip = this.terms["common.customer.invoices.showinvoice"];
                    }

                    this.setInformationIconAndTooltip(y);
                    this.setPaymentStatusIcon(y);

                }

                if (this.isImagesUploaded) {
                    this.items = this.allItems.filter(x => x.type === TermGroup_SupplierInvoiceType.Uploaded || x.type === TermGroup_SupplierInvoiceType.Scanning);
                    this.isImagesUploaded = false;
                }
                else {
                    this.items = this.allItems;
                }
                this.summarize(this.items);
            });
        }]).then(() => {
            this.setData(this.items);
        });
    }

    public loadGridDataForProjectCentral(projectId?: number, includeChildProjects?: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getSupplierInvoicesForProjectCentral(projectId, includeChildProjects, this.fromDate, this.toDate, this.invoices).then((x) => {
                this.allItems = x;

                var date = new Date();
                var dateToday: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0);

                for (const y of this.allItems) {
                    y.payDate = y.payDate ? new Date(<any>y.payDate).date() : null;
                    y.invoiceDate = y.invoiceDate ? new Date(<any>y.invoiceDate).date() : null;
                    y.dueDate = y.dueDate ? new Date(<any>y.dueDate).date() : null;
                    y.voucherDate = y.voucherDate ? new Date(<any>y.voucherDate).date() : null;

                    if (!y.attestStateName)
                        y.attestStateName = this.noAttestStateTerm;

                    if (y.isAttestRejected)
                        y.attestStateName = this.attestRejectedTerm;

                    y.expandableDataIsLoaded = false;

                    if (y.ediType && y.ediType > 0) {
                        if (y.ediType === TermGroup_EDISourceType.Scanning) {
                            y.showCreateInvoiceIcon = "fal fa-plus iconEdit";
                            y.showCreateInvoiceTooltip = this.terms["economy.supplier.invoice.createinvoice"];
                        }
                        else if (y.ediType == TermGroup_EDISourceType.Finvoice) {
                            y['interpretationStateName'] = this.terms["common.noone"];
                            y.showCreateInvoiceIcon = "fal fa-plus iconEdit";
                            y.showCreateInvoiceTooltip = this.terms["economy.supplier.invoice.createinvoice"];
                        }
                    }
                    else {
                        y['interpretationStateName'] = this.terms["common.noone"];
                        y.showCreateInvoiceIcon = "fal fa-pencil iconEdit";
                        y.showCreateInvoiceTooltip = this.terms["common.customer.invoices.showinvoice"];
                    }

                    this.setInformationIconAndTooltip(y);
                }

                this.projectId = 0;
                this.includeChildProjects = false;

                this.items = this.allItems;
                this.summarize(this.items);
            });
        }]).then(() => {
            this.setData(this.items);
        });
    }

    private setPaymentStatusIcon(invoice: SupplierInvoiceGridDTO) {
        if (invoice.fullyPaid) {
            if (invoice.noOfPaymentRows == invoice.noOfCheckedPaymentRows) {
                invoice["paymentStatusIcon"] = "fas fa-circle okColor";
                invoice["paymentStatusTooltip"] = this.terms["economy.supplier.invoice.paidshort"];
            }
            else {
                invoice["paymentStatusIcon"] = "fas fa-circle warningColor";
                invoice["paymentStatusTooltip"] = this.terms["economy.supplier.invoice.paidbutnotcheckedshort"];
            }
        }
        else if (invoice.paidAmount !== 0 && !invoice.fullyPaid) {
            invoice["paymentStatusIcon"] = "fas fa-circle yellowColor";
            invoice["paymentStatusTooltip"] = this.terms["economy.supplier.invoice.partlypaidshort"];
        }
        else {
            invoice["paymentStatusIcon"] = "fas fa-circle errorColor";
            invoice["paymentStatusTooltip"] = this.terms["economy.supplier.invoice.unpaidshort"];
        }

        if (invoice.blockPayment)
            invoice.blockIcon =  "fal fa-lock-alt errorColor";

        if (invoice.hasPDF)
            invoice.pdfIcon = "fal fa-file";
    }

    private loadSupplierInvoiceAccountingRows(params: any) {
        if (!params.data['rowsLoaded']) {
            this.progress.startLoadingProgress([() => {
                return this.supplierService.getSupplierInvoiceAccountingRows(params.data.supplierInvoiceId).then((x) => {
                    //if (row.dim1nr)
                    _.forEach(x, (y) => {
                        if (y.dim1Nr)
                            y.dim1Name = y.dim1Nr + " - " + y.dim1Name;
                        if (y.dim2Nr)
                            y.dim2Name = y.dim2Nr + " - " + y.dim2Name;
                        if (y.dim3Nr)
                            y.dim3Name = y.dim3Nr + " - " + y.dim3Name;
                        if (y.dim4Nr)
                            y.dim4Name = y.dim4Nr + " - " + y.dim4Name;
                        if (y.dim5Nr)
                            y.dim5Name = y.dim5Nr + " - " + y.dim5Name;
                        if (y.dim6Nr)
                            y.dim6Name = y.dim6Nr + " - " + y.dim6Name;
                    });
                    params.data['rows'] = x;
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

    public search() {
        const filterModels = this.gridAg.options.getFilterModels();
        if (filterModels)
            this.loadFilteredGridData(filterModels);
    }

    public loadFilteredGridData(filterModels: any) {
        //Selection values
        filterModels["loadopen"] = this.loadOpen;
        filterModels["loadclosed"] = this.loadClosed;

        //Collections
        if (filterModels["attestGroupName"]) {
            var filteredAttestGroups = [];
            _.forEach(filterModels["attestGroupName"], (value) => {
                var attestGroup = _.find(this.attestGroups, { value: value.toString() });
                if (attestGroup)
                    filteredAttestGroups.push(attestGroup.label);

            });
            filterModels["attestGroupName"] = filteredAttestGroups;
        }

        if (filterModels["attestStateName"]) {
            var filteredAttestStates = [];
            _.forEach(filterModels["attestStateName"], (value) => {
                var attestState = _.find(this.attestStates, { value: value.toString() });
                if (attestState)
                    filteredAttestStates.push(attestState.id);

            });
            filterModels["attestStateName"] = filteredAttestStates;
        }

        if (filterModels["billingTypeName"]) {
            var filteredBillingTypes = [];
            _.forEach(filterModels["billingTypeName"], (value) => {
                var billingType = _.find(this.invoiceBillingTypes, { value: value.toString() });
                if (billingType)
                    filteredBillingTypes.push(billingType.id);

            });
            filterModels["billingTypeName"] = filteredBillingTypes;
        }

        if (filterModels["currencyCode"]) {
            var filteredCurrencyCodes = [];
            _.forEach(filterModels["currencyCode"], (value) => {
                var currencyCode = _.find(this.currencyCodes, { value: value.toString() });
                if (currencyCode)
                    filteredCurrencyCodes.push(currencyCode.id);

            });
            filterModels["currencyCode"] = filteredCurrencyCodes;
        }

        if (filterModels["statusName"]) {
            var filteredStatusNames = [];
            _.forEach(filterModels["statusName"], (value) => {
                var statusName = _.find(this.originStatus, { value: value.toString() });
                if (statusName)
                    filteredStatusNames.push(statusName.label);

            });
            filterModels["statusName"] = filteredStatusNames;
        }

        if ( filterModels["typeName"] ) {
            var filteredTypeNames = [];
            _.forEach(filterModels["typeName"].values, (value) => {
                var typeName = _.find(this.invoiceTypes, { value: value.toString() });
                if (typeName)
                    filteredTypeNames.push(typeName.label);

            });
            filterModels["typeName"] = filteredTypeNames;
        }

        this.progress.startLoadingProgress([() => {
            return this.supplierService.getFilteredSupplierInvoices(filterModels).then((x) => {
                this.allItems = x;

                var date = new Date();
                var dateToday: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0);

                _.forEach(this.allItems, (y) => {
                    y.payDate = y.payDate ? new Date(<any>y.payDate).date() : null;
                    y.invoiceDate = y.invoiceDate ? new Date(<any>y.invoiceDate).date() : null;
                    y.dueDate = y.dueDate ? new Date(<any>y.dueDate).date() : null;
                    y.voucherDate = y.voucherDate ? new Date(<any>y.voucherDate).date() : null;

                    if (y.timeDiscountPercent != null && y.timeDiscountPercent != 0 && y.timeDiscountDate != null && dateToday.isSameOrBeforeOnDay(y.timeDiscountDate)) {
                        y.payAmount = ((100 - y.timeDiscountPercent) / 100 * y.payAmount);
                        y.payAmountCurrency = ((100 - y.timeDiscountPercent) / 100 * y.payAmountCurrency);
                    }

                    if (!y.attestStateName)
                        y.attestStateName = this.noAttestStateTerm;

                    if (y.isAttestRejected)
                        y.attestStateName = this.attestRejectedTerm;

                    y.expandableDataIsLoaded = false;

                    if (y.ediType && y.ediType > 0) {
                        if (y.ediType === TermGroup_EDISourceType.Scanning) {
                            y.showCreateInvoiceIcon = "fal fa-plus iconEdit";
                            y.showCreateInvoiceTooltip = this.terms["economy.supplier.invoice.createinvoice"];
                        }
                        else if (y.ediType == TermGroup_EDISourceType.Finvoice) {
                            y['interpretationStateName'] = this.terms["common.noone"];
                            y.showCreateInvoiceIcon = "fal fa-plus iconEdit";
                            y.showCreateInvoiceTooltip = this.terms["economy.supplier.invoice.createinvoice"];
                        }
                    }
                    else {
                        y['interpretationStateName'] = this.terms["common.noone"];
                        y.showCreateInvoiceIcon = "fal fa-pencil iconEdit";
                        y.showCreateInvoiceTooltip = this.terms["common.customer.invoices.showinvoice"];
                    }
                    this.setInformationIconAndTooltip(y);
                    this.setPaymentStatusIcon(y);
                });

                this.projectId = 0;
                this.includeChildProjects = false;

                this.summarize(this.items);
            });
        }]).then(() => {
            this.items = this.allItems;

            this.setData(this.items);
        });
    }

    public setInformationIconAndTooltip(item: SupplierInvoiceGridDTO) {
        var message: string = "";
        if (item.ediType && item.ediType > 0) {
            if (item.operatorMessage) {
                message += this.terms["economy.supplier.invoice.messagefromoperator"] + "<br/>";
                message += item.operatorMessage + "<br/>" + "<br/>";
            }

            if (item.status != TermGroup_EDIStatus.Processed && item.errorCode > 0) {

                message += this.terms["common.errormessage"] + "<br/>"; //
                message += this.getActionResultMessage(item.errorCode, item.errorMessage) + "<br/>" + "<br/>";
            }

            if (item.invoiceStatus == TermGroup_EDIInvoiceStatus.Error) {
                message += this.terms["economy.supplier.invoice.invoicestatus"] + ": " + "<br/>";
                if (!item.invoiceNr || item.invoiceNr === "") {
                    message += this.terms["economy.supplier.invoice.invoicenrmissing"] + "<br/>";
                }

                if (!item.supplierId || item.supplierId === 0) {
                    message += this.terms["economy.supplier.invoice.suppliermissing"] + "<br/>";
                }

                if ((item.totalAmount < 0 && item.billingTypeId != TermGroup_BillingType.Credit) ||
                    (item.totalAmount >= 0 && item.billingTypeId == TermGroup_BillingType.Credit)) {
                    message += this.terms["economy.supplier.invoice.falsebillingtypemessage"] + "<br/>";
                }

                message += "<br/>";
            }

            message += this.terms["economy.accounting.currency.source"] + ": " + item.sourceTypeName + "<br/>";
            message += this.terms["economy.supplier.invoice.documentprovider"] + ": " + (item.ediMessageProviderName || "") + "<br/>";
            message += this.terms["economy.supplier.invoice.currency"] + ": " + item.currencyCode + "<br/>";
            message += this.terms["economy.supplier.invoice.retrievedate"] + ": " + (item.created ? CalendarUtility.toFormattedDate(item.created) : "NULL") + "<br/>";
            message += this.terms["economy.supplier.invoice.messagetype"] + ": " + item.ediMessageTypeName + "<br/>";

            item.infoIconValue = "fal fa-info-circle infoColor";
            item.infoIconTooltip = this.terms["core.showinfo"];
            item.infoIconMessage = message;
        }
        else {
            //Supplier invoices
            var hasInfo: boolean = false;
            var hasError: boolean = false;
            var flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
            var statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(item.statusIcon);
            /*if (item.paidAmount != 0 && item.fullyPaid === false) {
                hasInfo = true;
            }*/
            if (this.supplierUseTimeDiscount === true && item.fullyPaid === false &&
                item.timeDiscountDate != null && item.timeDiscountDate.date >= CalendarUtility.getDateToday().date &&
                item.timeDiscountPercent != null && item.timeDiscountPercent != 0) {
                hasInfo = true;
            }

            if (statusIcons.contains(SoeStatusIcon.Imported)) {
                item.infoIconValue = "fal fa-download";
            }
            else if (hasInfo) {
                item.infoIconValue = "fal fa-info-circle infoColor";
                item.infoIconTooltip = this.terms["core.showinfo"];
            }
            else if (item.statusIcon != SoeStatusIcon.None) {
                if (!statusIcons.contains(SoeStatusIcon.Email) && !statusIcons.contains(SoeStatusIcon.ElectronicallyDistributed)) {
                    item.infoIconValue = "fal fa-paperclip";

                    if (statusIcons.contains(SoeStatusIcon.Imported))
                        item.infoIconTooltip = item.infoIconTooltip && item.infoIconTooltip != "" ? "<br/>" + this.terms["common.imported"] : this.terms["common.imported"];
                    if (statusIcons.contains(SoeStatusIcon.Attachment))
                        item.infoIconTooltip = item.infoIconTooltip && item.infoIconTooltip != "" ? "<br/>" + this.terms["common.hasaattachedfiles"] : this.terms["common.hasaattachedfiles"];
                    if (statusIcons.contains(SoeStatusIcon.Image))
                        item.infoIconTooltip = item.infoIconTooltip && item.infoIconTooltip != "" ? "<br/>" + this.terms["common.hasattachedimages"] : this.terms["common.hasattachedimages"];
                }
            }
        }
    }

    private getActionResultMessage(actionResultValue: number, message: string) {
        let errorMsg = "";
        switch (actionResultValue) {
            case ActionResultSave.EdiInvalidUri:
                errorMsg = "Felaktig FTP-adress";
                break;
            case ActionResultSave.EdiInvalidType:
                errorMsg = "Felaktig EDI-källa";
                break;
            case ActionResultSave.EdiFailedParse:
                errorMsg = "Kunde ej tolka XML-filen";
                break;
            case ActionResultSave.EdiFailedFileListing:
                errorMsg = "Kunde inte hämta filer från FTP";
                break;
            case ActionResultSave.EdiFailedUnknown:
                errorMsg = "Okänt fel uppstod när filen tolkades";
                break;
            case ActionResultSave.EdiFailedTransferToInvoiceInvalidData:
                errorMsg = message;
                break;
            default:
                errorMsg = "Okänt fel";
                break;
        }
        return errorMsg;
    }

    public updateItemsSelection() {
        // We do not save setting on custom, YES we do!
        //if (this.allItemsSelection === 999)
        //    return;

        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.SupplierInvoiceAllItemsSelection, this.allItemsSelection).then((x) => {
            this.gridAg.options.useExternalFiltering = this.gridAg.options.ignoreFiltering = false;
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
            this.selectedTotal = this.selectedTotalIncVat;
        else
            this.selectedTotal = this.selectedTotalExVat;
    }

    protected supplierChanged(row) {
        var supplier = _.find(this.suppliers, p => p.value === row.supplierId);
        row.supplierName = supplier ? supplier.label : '';
    }

    protected attestGroupChanged(row) {
        var attestGroup = _.find(this.attestGroups, p => p.value == row.data.attestGroupName);
        if (attestGroup) {
            row.attestGroupId = attestGroup.label;
            row.attestGroupName = attestGroup.value;
        }
        else {
            row.attestGroupId = 0;
            row.attestGroupName = '';
        }

        this.supplierService.saveSupplierInvoiceChangeAttestGroup(row.data.supplierInvoiceId, row.attestGroupId).then(() => {

        });

        this.gridAg.options.selectRow(row.data);

    }

    protected showScannedState(row) {
        if (row.typeName === "Scanning")
            return true;
        else
            return false;
    }

    protected showPdfIcon(row) {
        if (row.hasPDF === true || row.ediType === TermGroup_EDISourceType.Finvoice)
            return true;
        else
            return false;
    }

    public showInvoice(row: any) {
        if (row.type === TermGroup_SupplierInvoiceType.EDI)
            return;

        const invoices: any[] = [];
        _.forEach(this.gridAg.options.getFilteredRows(), row => {
            if (row.type === TermGroup_SupplierInvoiceType.Invoice ||
                row.type === TermGroup_SupplierInvoiceType.Uploaded)
                invoices.push({ id: row.supplierInvoiceId, type: row.type });
            else
                if (row.type === TermGroup_SupplierInvoiceType.Scanning ||
                    row.type === TermGroup_SupplierInvoiceType.Finvoice)
                    invoices.push({ id: row.ediEntryId, type: row.type });
        });
        const message = new TabMessage(
            `${this.terms["economy.supplier.invoice.invoice"]} ${row.invoiceNr}`,
            (row.type === TermGroup_SupplierInvoiceType.Scanning || row.type === TermGroup_SupplierInvoiceType.Finvoice ? row.ediEntryId : row.supplierInvoiceId),
            SupplierInvoicesEditController,
            { id: row.supplierInvoiceId, ediType: row.ediType, ediEntryId: row.ediEntryId, invoiceIds: invoices },
            this.urlHelperService.getGlobalUrl("/Shared/Economy/Supplier/Invoices/Views/edit.html")
        );
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }

    private showAttestCommentIcon(row: any) {
        return row.hasAttestComment;
    }

    private showAttestCommentDialog(row: any) {
        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowAttestCommentsAndAnswersDialog/ShowAttestCommentsAndAnswers.html"),
            controller: ShowAttestCommentsAndAnswersDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                supplierService: () => { return this.supplierService },
                invoiceId: () => { return row.supplierInvoiceId },
                registeredTerm: () => { return this.terms["core.attestflowregistered"]}
            }
        });
    }

    private showPdf(row) {
        //Show picture in new browser tab (not sure if PDF:s work same way)
        if (row.type === TermGroup_SupplierInvoiceType.Invoice && row.hasPDF) {
            var imageUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SupplierInvoiceImage + "&invoiceId=" + row.supplierInvoiceId + "&c=" + CoreUtility.actorCompanyId;
            window.open(imageUrl, '_blank');
        }
        else {
            if (row.ediEntryId > 0) {
                if (row.ediType == TermGroup_EDISourceType.Finvoice) {
                    var uri = window.location.protocol + "//" + window.location.host;
                    uri = uri + "/soe/common/xslt/" + "?templatetype=" + SoeReportTemplateType.FinvoiceEdiSupplierInvoice + "&id=" + row.ediEntryId + "&c=" + CoreUtility.actorCompanyId;
                    window.open(uri, '_blank');
                }
                else if (row.ediType == TermGroup_EDISourceType.Scanning) {
                    //var scannedPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.ReadSoftScanningSupplierInvoice + "&scanningentryid=" + row.scanningEntryId + "&edientryid=" + row.entryId;
                    const scannedPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.ReadSoftScanningSupplierInvoice + "&scanningentryid=" + row.scanningEntryId + "&edientryid=" + row.ediEntryId + "&c=" + CoreUtility.actorCompanyId;
                    window.open(scannedPdfReportUrl, '_blank');
                }
                else if (row.ediType == TermGroup_EDISourceType.EDI) {
                    var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + row.ediEntryId;
                    window.open(ediPdfReportUrl, '_blank');
                }
            }
        }
    }

    private downloadPdfs(rows: number[]) {
        const imageUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SupplierInvoiceImage + "&c=" + CoreUtility.actorCompanyId + "&invoiceIds=" + rows;
        window.open(imageUrl, '_blank');
    }

    private showPayments(row: SupplierInvoiceGridDTO) {
        this.supplierService.getPaymentRows(row.supplierInvoiceId).then((x) => {
            var message = "<strong>" + row['paymentStatusTooltip'] + "</strong><br/>";
            var totalPaidAmount = 0;
            if (x.length > 0) {
                //Create header
                message += "<br/><table><thead>" +
                    "<tr>" +
                    "<td><strong>" + this.terms["economy.supplier.invoice.liquidityplanning.sequencenr"] + "</strong></span></td>" +
                    "<td><span class='margin-small-left'><strong>" + this.terms["common.amount"] + "</strong></span></td>" +
                    "<td><span class='margin-small-left'><strong>" + this.terms["common.tracerows.status"] + "</strong></span></td>" +
                    "<td><span class='margin-small-left'><strong>" + this.terms["common.customer.invoices.paydate"] + "</strong></span></td>" +
                    "<td><span class='margin-small-left'><strong>" + this.terms["common.created"] + "</strong></span></td>" +
                    "</tr>" +
                    "</thead><tbody>";

                // List payments
                _.forEach(x, (y) => {
                    if (y.payDate)
                        y.payDate = new Date(<any>y.payDate);
                    if (y.created)
                        y.created = new Date(<any>y.created);

                    if (y.status !== SoePaymentStatus.Cancel)
                        totalPaidAmount += y.amountCurrency;

                    message += "<tr>" +
                        "<td>" + y.seqNr + "</span></td>" +
                        "<td><span class='margin-small-left'>" + y.amountCurrency + " " + row.currencyCode + "</span></td>" +
                        "<td><span class='margin-small-left'>" + y.statusName + "</span></td>" +
                        "<td><span class='margin-small-left'>" + CalendarUtility.toFormattedDate(y.payDate) + "</span></td>" +
                        "<td><span class='margin-small-left'>" + CalendarUtility.toFormattedDate(y.created) + "</span></td>" +
                        "</tr>";
                });
                message += "</tbody>" +
                    "</table>";
            }

            // Set totals
            message += "<br/><span><strong>" + this.terms["economy.supplier.payment.totalamount"] + ":" + "</strong>" + " " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency : row.totalAmount) + " " + row.currencyCode + "</span>" +
                "<span class='margin-small-left'><strong>" + this.terms["economy.supplier.payment.paymentamount"] + ":" + "</strong>" + " " + totalPaidAmount + " " + row.currencyCode + "</span>" +
                "<span class='margin-small-left'><strong>" + this.terms["economy.supplier.invoice.remainingamount"] + ":" + "</strong>" + " " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency : row.payAmount) + " " + row.currencyCode + "</span>";

            this.notificationService.showDialog(this.terms["core.information"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }

    private showInformationMessage(row: SupplierInvoiceGridDTO) {
        let message = "";
        if (!row.ediType || row.ediType === 0) {
            if (!row.fullyPaid) {
                let isTotalAmountPaid = false;
                let partlyPaid = false;
                let partlyPaidForeign = false;
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

                /*if (isTotalAmountPaid) {
                    message = message + this.terms["economy.supplier.invoice.paidlate"] + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
                }
                else if ((partlyPaid || partlyPaidForeign) && (this.supplierUseTimeDiscount == false || row.timeDiscountDate == null)) {
                    message = message + this.terms["economy.supplier.invoice.partlypaid"] + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
                }
                else*/ if (partlyPaid && this.supplierUseTimeDiscount == true && row.timeDiscountDate != null) {
                    message = message + this.terms["economy.supplier.invoice.hastimediscount"] + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.timediscount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
                }
                else if (!partlyPaid && this.supplierUseTimeDiscount == true && row.timeDiscountDate != null) {
                    message = message + this.terms["economy.supplier.invoice.hastimediscount"] + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
                    message = message + this.terms["economy.supplier.invoice.timediscount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? (row.totalAmountCurrency - row.payAmountCurrency).toString() : (row.totalAmount - row.payAmount).toString()) + "<br/>";
                }
            }


            if (row.multipleDebtRows) {
                if (message != "")
                    message = message + "---<br/>";

                message = message + this.terms["economy.supplier.invoice.multipledebtrows"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.manualadjustmentneeded"] + "<br/>";
            }

        }
        else {
            message = row.infoIconMessage;
        }

        if (message != "")
            this.notificationService.showDialog(this.terms["core.information"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    private showBlockReason(row: SupplierInvoiceGridDTO) {
        this.notificationService.showDialog(this.terms["common.reason"], row.blockReason, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    // Lookups
    lookupSupplierInvoice() {
        this.accountingService.getDistributionCodes(false);
        this.accountingService.getGrossProfitCodes();
        this.accountingService.getPaymentConditions();
        this.accountingService.getVatCodes(true);
        this.coreService.getCompCurrencies(false);
        this.supplierService.getSysWholesellersDict(true);
        this.supplierService.getAttestWorkFlowGroupsDict(true);
        this.supplierService.getProjectList(TermGroup_ProjectType.TimeProject, true, false, true, true);
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Supplier_Invoice_Invoices_All);
        featureIds.push(Feature.Economy_Supplier_Invoice_Invoices);
        featureIds.push(Feature.Economy_Supplier_Invoice_Status_Foreign);
        featureIds.push(Feature.Economy_Supplier_Invoice_Status_DraftToOrigin);
        featureIds.push(Feature.Economy_Supplier_Invoice_Status_OriginToVoucher);
        featureIds.push(Feature.Economy_Supplier_Invoice_Scanning);
        featureIds.push(Feature.Economy_Supplier_Invoice_AttestFlow_Admin);
        featureIds.push(Feature.Economy_Supplier_Invoice_AttestFlow_Add); //Billing_Import_EDI
        featureIds.push(Feature.Billing_Import_EDI);
        featureIds.push(Feature.Economy_Supplier_Invoice_Finvoice);
        featureIds.push(Feature.Economy_Supplier_Invoice_AddImage);                     // Add an invoice image

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.hasCurrencyPermission = x[Feature.Economy_Supplier_Invoice_Status_Foreign];
            this.hasDraftToOriginPermission = x[Feature.Economy_Supplier_Invoice_Status_DraftToOrigin];
            this.hasOriginToVoucherPermission = x[Feature.Economy_Supplier_Invoice_Status_OriginToVoucher];
            this.hasScanningPermission = x[Feature.Economy_Supplier_Invoice_Scanning];
            this.hasAttestAdminPermission = x[Feature.Economy_Supplier_Invoice_AttestFlow_Admin];
            this.hasAttestAddPermission = x[Feature.Economy_Supplier_Invoice_AttestFlow_Add];
            this.hasEDIPermission = x[Feature.Billing_Import_EDI];
            this.hasFinvoicePermission = x[Feature.Economy_Supplier_Invoice_Finvoice];

            this.uploadImagePermission = x[Feature.Economy_Supplier_Invoice_AddImage];


            if (x[Feature.Economy_Supplier_Invoice_Invoices_All]) {
                this.loadOpen = true;
                this.loadClosed = false;
                this.hasOpenPermission = true;
                this.hasClosedPermission = true;
            }
            else {
                if (x[Feature.Economy_Supplier_Invoice_Invoices] || this.hasDraftToOriginPermission || this.hasOriginToVoucherPermission) {
                    this.loadOpen = true;
                    this.hasOpenPermission = true;
                }
                if (x[Feature.Economy_Supplier_Invoice_Invoices] || this.hasOriginToVoucherPermission) {
                    this.loadClosed = false;
                    this.hasClosedPermission = true;
                }
            }
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.SupplierInvoiceAllItemsSelection];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.SupplierInvoiceAllItemsSelection, TermGroup_ChangeStatusGridAllItemsSelection.One_Month, false);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypeIds: number[] = [];
        settingTypeIds.push(CompanySettingType.SupplierInvoiceTransferToVoucher);
        settingTypeIds.push(CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer);
        settingTypeIds.push(CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher);
        settingTypeIds.push(CompanySettingType.SupplierDefaultBalanceList);
        settingTypeIds.push(CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired);
        settingTypeIds.push(CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired);
        settingTypeIds.push(CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup);
        settingTypeIds.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypeIds.push(CompanySettingType.SupplierUseTimeDiscount);
        settingTypeIds.push(CompanySettingType.CoreBaseCurrency);
        settingTypeIds.push(CompanySettingType.SupplierShowTransactionCurrency);
        settingTypeIds.push(CompanySettingType.SupplierShowEnterpriseCurrency);
        settingTypeIds.push(CompanySettingType.SupplierShowLedgerCurrency);
        settingTypeIds.push(CompanySettingType.InExchangeAPIReciveRegistered);
        settingTypeIds.push(CompanySettingType.ScanningUsesAzoraOne);

        return this.coreService.getCompanySettings(settingTypeIds).then(x => {
            if (x[CompanySettingType.SupplierInvoiceTransferToVoucher])
                this.supplierInvoiceTransferToVoucher = x[CompanySettingType.SupplierInvoiceTransferToVoucher];
            if (x[CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer])
                this.supplierInvoiceAskPrintVoucherOnTransfer = x[CompanySettingType.SupplierInvoiceAskPrintVoucherOnTransfer];
            if (x[CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher])
                this.supplierCloseInvoicesWhenTransferredToVoucher = x[CompanySettingType.SupplierCloseInvoicesWhenTransferredToVoucher];
            if (x[CompanySettingType.SupplierDefaultBalanceList])
                this.supplierBalanceListReportId = x[CompanySettingType.SupplierDefaultBalanceList];
            if (x[CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired] && x[CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired] != 0)
                this.userIdNeededWithTotalAmount = x[CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired];
            if (x[CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired] && x[CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired] != 0)
                this.totalAmountWhenUserReguired = x[CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired];
            if (x[CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup] && x[CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup] != 0)
                this.defaultAttestGroupId = x[CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestGroup];
            if (x[CompanySettingType.AccountingDefaultVoucherList] != null)
                this.supplierInvoiceVoucherReportId = x[CompanySettingType.AccountingDefaultVoucherList];
            if (x[CompanySettingType.SupplierUseTimeDiscount] != null)
                this.supplierUseTimeDiscount = x[CompanySettingType.SupplierUseTimeDiscount];
            if (x[CompanySettingType.CoreBaseCurrency] != null)
                this.coreBaseCurrency = x[CompanySettingType.CoreBaseCurrency];
            if (x[CompanySettingType.ScanningUsesAzoraOne] != null)
                this.usesAzoraOneScanning = x[CompanySettingType.ScanningUsesAzoraOne];

            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowLedgerCurrency);
            this.showGetEInvoices = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.InExchangeAPIReciveRegistered);
        });
    }

    private loadCurrentAccountYear(): ng.IPromise<any> {
        return this.coreService.getCurrentAccountYear().then((x) => {
            if (x) {
                this.currentAccountYearFromDate = x.from;
                this.currentAccountYearToDate = x.to;
            }
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
            //Add custom
            this.allItemsSelectionDict.push({ id: 999, name: this.terms["economy.supplier.invoice.custom"] })
        });
    }

    private loadInvoiceTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SupplierInvoiceType, false, false).then((x) => {
            this.invoiceTypes = [];
            _.forEach(x, (row) => {
                this.invoiceTypes.push({ value: row.name, label: row.id });
            });
        });
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then((x) => {
            this.invoiceBillingTypes = [];
            _.forEach(x, (row) => {
                if (row.id < 3)
                    this.invoiceBillingTypes.push({ value: row.name, label: row.id });
            });
        });
    }

    private loadOriginStatus(): ng.IPromise<any> {
        this.originStatus = [];
        return this.supplierService.getInvoiceAndPaymentStatus(SoeOriginType.SupplierInvoice, true).then((x) => {
            _.forEach(x, (row) => {
                this.originStatus.push({ value: row.name, label: row.id });
            });
        });
    }

    private loadEdiStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDIStatus, false, false).then((x) => {
            _.forEach(x, (row) => {
                if (_.filter(this.originStatus, { value: row.name }).length === 0)
                    this.originStatus.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadScanningStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScanningStatus, false, false).then((x) => {
            _.forEach(x, (row) => {
                if (_.filter(this.originStatus, { value: row.name }).length === 0)
                    this.originStatus.push({ value: row.name, label: row.name });
            });
        });
    }

    public loadAttestStates(): ng.IPromise<any> {
        return this.supplierService.getAttestStates(TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false).then((x) => {
            this.attestStates = [];
            this.hiddenAttestStates = [];
            this.attestStates.push({ value: this.noAttestStateTerm, label: -100 });
            this.attestStates.push({ value: this.attestRejectedTerm, label: -200 });
            _.forEach(x, (y: any) => {
                this.attestStates.push({ value: y.name, label: y.attestStateId });

                if (y.hidden === true)
                    this.hiddenAttestStates.push(y);
            });
        });
    }

    private getHasHiddenAttestState(): ng.IPromise<any> {
        return this.supplierService.hasHiddenAttestState().then(x => {
            this.hasHiddenAttestState = x;
        });
    }

    public loadAttestGroups(): ng.IPromise<any> {
        return this.supplierService.getAttestWorkFlowGroups().then((x) => {
            this.attestGroups = [];
            this.attestGroups.push({ value: ' ', label: 0 });
            _.forEach(x, (y: any) => {
                this.attestGroups.push({ value: y.name, label: y.attestWorkFlowHeadId });
            });
        });
    }

    public loadScanningUnprocessedCount(): ng.IPromise<any> {
        return this.supplierService.getScanningUnprocessedCount().then((x) => {
            this.scanningUnprocessedCount = x;
        });
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, true).then((x: ISmallGenericType[]) => {
            this.suppliers = [];
            _.forEach(x, (y: any) => {
                this.suppliers.push({ value: y.id, label: y.name });
            });
        });
    }

    public loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesGrid().then((x) => {
            this.currencyCodes = [];
            _.forEach(x, (y: any) => {
                this.currencyCodes.push({ value: y.code, label: y.currencyId });
            });
        });
    }

    private loadAccounts(useCache: boolean): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, false, true, true, useCache).then(x => {

            this.accountDims = x;

            /*this.accountDims.forEach(ad => {
                if (!ad.accounts)
                    ad.accounts = [];

                if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0)
                    (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: '', name: '', numberName: ' ' });
            });
            this.setRowItemAccountsOnAllRows(false);*/
        });
    }

    private loadInvoiceJournalReportId(): ng.IPromise<any> {
        return this.accountingService.getInvoiceJournalReportId(SoeReportTemplateType.SupplierInvoiceJournal).then(x => {
            this.invoiceJournalReportId = x;
        });
    }

    private executeButtonFunction(option) {
        let transfer: boolean = false;
        let removeDraft: boolean = false;
        let ediscanning: boolean = false;
        let ediScanningType: number = 0;
        let attestflow: boolean = false;
        
        this.buttonOption = option;
        let validatedItems: any = [];
        let validMessage: string = "";
        let invalidMessage: string = "";
        let successMessage: string = "";
        let errorMessage: string = "";
        let print: boolean = false;

        const selectedItems = this.gridAg.options.getSelectedRows();

        switch (option.id) {
            case SupplierGridButtonFunctions.SaveAsDefinitiv:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Draft) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.invoicesdrafttooriginvalid"] : this.terms["common.invoicedrafttooriginvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.invoicesdrafttoorigininvalid"] : this.terms["common.invoicedrafttoorigininvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasoriginseqnr"] : this.terms["common.invoicesavedasoriginseqnr"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.invoicessavedasoriginfailed"] : this.terms["common.invoicesavedasoriginfailed"];

                this.originStatusChange = SoeOriginStatusChange.DraftToOrigin;
                transfer = true;
                break;
            case SupplierGridButtonFunctions.TransferToVoucher:
                _.forEach(selectedItems, (row) => {
                    if (row.status === SoeOriginStatus.Origin && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.invoicesorigintovouchervalid"] : this.terms["common.invoiceorigintovouchervalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.invoicesorigintovoucherinvalid"] : this.terms["common.invoiceorigintovoucherinvalid"];
                successMessage = this.terms["economy.supplier.invoice.voucherscreated"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.invoicestransfertovoucherfailed"] : this.terms["common.invoicetransfertovoucherfailed"];

                this.originStatusChange = SoeOriginStatusChange.OriginToVoucher;
                transfer = true;
                break;
            case SupplierGridButtonFunctions.CreateInvoiceOutOfEdi:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    //Own validation for Finvoice
                    if (row.ediType > 0 && row.ediType == TermGroup_EDISourceType.Finvoice) {
                        if (row.invoiceNr && row.invoiceNr != "" &&
                            !!(row.supplierId && row.supplierId > 0) &&
                            row.supplierId && row.supplierId > 0 &&
                            row.invoiceStatus === TermGroup_EDIInvoiceStatus.Unprocessed)
                            validatedItems.push(row);
                    }
                    else {
                        if (row.invoiceNr && row.invoiceNr != "" &&
                            !!(row.supplierId && row.supplierId > 0) &&
                            row.supplierId && row.supplierId > 0 &&
                            row.invoiceStatus === TermGroup_EDIInvoiceStatus.Unprocessed &&
                            (row.status === TermGroup_EDIStatus.UnderProcessing || row.status === TermGroup_EDIStatus.Processed))
                            validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.edistransfertoinvoicevalid"] : this.terms["common.editransfertoinvoicevalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.edistransfertoinvoiceinvalid"] : this.terms["common.editransfertoinvoiceinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.invoiceswascreated"] : this.terms["common.invoicewascreated"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.edistransfertoinvoicefailed"] : this.terms["common.editransfertoinvoicefailed"];

                this.ignoreDateValidation = true;
                ediscanning = true;
                ediScanningType = 0;
                break;
            case SupplierGridButtonFunctions.Save:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.ediType && row.ediType > 0) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.invoice.saveedischangesvalid"] : this.terms["economy.supplier.invoice.saveedichangesvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.invoice.saveedischangesinvalid"] : this.terms["economy.supplier.invoice.saveedichangesinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["economy.supplier.invoice.edischangessaved"] : this.terms["economy.supplier.invoice.edichangessaved"];
                errorMessage = validatedItems.length > 1 ? this.terms["economy.supplier.invoice.edischangessavedfailed"] : this.terms["economy.supplier.invoice.edichangessavedfailed"];

                this.ignoreDateValidation = true;
                ediscanning = true;
                ediScanningType = 3;
                break;
            case SupplierGridButtonFunctions.CloseEdi:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.ediType && row.ediType > 0) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.edisclosevalid"] : this.terms["common.ediclosevalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.ediscloseinvalid"] : this.terms["common.edicloseinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.edisclosed"] : this.terms["common.ediclosed"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.edisclosefailed"] : this.terms["common.ediclosefailed"];

                this.ignoreDateValidation = true;
                ediscanning = true;
                ediScanningType = 1;
                break;
            case SupplierGridButtonFunctions.RemoveDraftOrEdi:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.ediType && row.ediType > 0) {
                        validatedItems.push(row);
                        ediscanning = true;
                        ediScanningType = 2;
                    }
                    else
                        if (row.status == SoeOriginStatus.Draft || row.type == TermGroup_SupplierInvoiceType.Uploaded ) {
                            validatedItems.push(row);
                            removeDraft = true;
                        }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.edisdeletevalid"] : this.terms["common.edideletevalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.edisdeleteinvalid"] : this.terms["common.edideleteinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.edisdeleted"] : this.terms["common.edideleted"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.edisdeletefailed"] : this.terms["common.edideletefailed"];

                this.ignoreDateValidation = true;

                break;
            case SupplierGridButtonFunctions.AddToAttestFlow:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.ediType || row.ediType === 0) {
                        validatedItems.push(row);
                    }
                });
                attestflow = true;
                this.addToAttestFlow(validatedItems);
                break;
            case SupplierGridButtonFunctions.StartAttestFlow:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.ediType || row.ediType === 0) {
                        validatedItems.push(row);
                    }
                });
                attestflow = true;
                //console.log("start attestflow", selectedItems, validatedItems);
                //this.startAttestFlow(validatedItems);
                this.initStartAttestFlow(validatedItems);
                break;
            case SupplierGridButtonFunctions.SendAttestReminder:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.ediType || row.ediType === 0) {
                        validatedItems.push(row);
                    }
                });
                attestflow = true;
                this.sendAttestReminder(validatedItems);
                break;
            case SupplierGridButtonFunctions.CreatePDF:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (!row.hasPDF && row.ediType && row.ediType === TermGroup_EDISourceType.EDI) { //(row.ediType === TermGroup_EDISourceType.EDI || row.ediType === TermGroup_EDISourceType.Finvoice)
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.createpdfsvalid"] : this.terms["common.createpdfvalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.postsinvalid"] : this.terms["common.postinvalid"];
                successMessage = validatedItems.length > 1 ? this.terms["common.pdfscreated"] : this.terms["common.pdfcreated"];
                errorMessage = validatedItems.length > 1 ? this.terms["common.pdfserror"] : this.terms["common.pdferror"];

                this.ignoreDateValidation = true;
                ediscanning = true;
                ediScanningType = 4;
                break;
            case SupplierGridButtonFunctions.HideUnhandled:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (!row.attestStateId) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.invoice.hideunhandled.askinvoices"] : this.terms["economy.supplier.invoice.hideunhandled.askinvoice"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.invoice.hideunhandled.invalidinvoices"] : this.terms["economy.supplier.invoice.hideunhandled.invalidinvoice"];

                this.ignoreDateValidation = true;
                break;
            case SupplierGridButtonFunctions.PrintInvoiceImages:
                _.forEach(selectedItems, (row: SupplierInvoiceGridDTO) => {
                    if (row.type === TermGroup_SupplierInvoiceType.Invoice && row.hasPDF) {
                        validatedItems.push(row.supplierInvoiceId);
                    }
                });

                if (validatedItems.length > 0) {
                    this.downloadPdfs(validatedItems);
                }

                print = true;
                break;
        }

        if (!attestflow && !print) {
            const noOfValid: number = validatedItems.length;
            const noOfInvalid = selectedItems.length - validatedItems.length;

            // Items to transfer
            var itemsToTransfer: SupplierInvoiceGridDTO[] = validatedItems;

            var title: string = "";
            var text: string = "";
            var doTransfer: boolean = false;
            var yesButtonText: string = "";
            var noButtonText: string = "";
            var cancelButtonText: string = "";
            var image: SOEMessageBoxImage = SOEMessageBoxImage.None;
            var buttons: SOEMessageBoxButtons = SOEMessageBoxButtons.None;

            if (selectedItems.length === validatedItems.length) {
                if (this.ignoreDateValidation || this.validateInvoiceDates()) {
                    title = this.terms["core.verifyquestion"];

                    text = "";
                    text += noOfValid.toString() + " " + validMessage + "<br\>";
                    text += this.terms["core.continue"];

                    image = SOEMessageBoxImage.Question;
                    buttons = SOEMessageBoxButtons.OKCancel;

                    doTransfer = true;
                }
                else {
                    this.selectedItems = [];
                    return;
                }
            }
            else if (selectedItems.length > validatedItems.length) {
                if (noOfValid === 0) {
                    title = this.terms["core.warning"];

                    text = "";
                    text += (selectedItems.length - validatedItems.length).toString() + " " + invalidMessage + "<br\>";

                    image = SOEMessageBoxImage.Warning;
                    buttons = SOEMessageBoxButtons.OK;

                    doTransfer = false;
                }
                else {
                    if (this.ignoreDateValidation || this.validateInvoiceDates()) {
                        title = this.terms["core.verifyquestion"];

                        text = "";
                        text += (selectedItems.length - validatedItems.length).toString() + " " + invalidMessage + "<br\>";
                        text += noOfValid.toString() + " " + validMessage + "<br\>";
                        text += this.terms["core.continue"];

                        image = SOEMessageBoxImage.Question;
                        buttons = SOEMessageBoxButtons.OKCancel;

                        doTransfer = true;
                    }
                    else {
                        this.selectedItems = [];
                        return;
                    }
                }
            }
            const modal = this.notificationService.showDialog(title, text, image, buttons);
            modal.result.then(val => {
                if (val != null && val === true) {
                    if (option.id == SupplierGridButtonFunctions.HideUnhandled) {
                        this.hideUnhandled(validatedItems);
                    } else {
                        if (doTransfer === true) {
                            if (transfer) {
                                // Get ids of items to transfer
                                var dict: any = _.map(itemsToTransfer, "supplierInvoiceId");
                                this.transfer(dict, successMessage, errorMessage);
                            } else if (ediscanning) {
                                if (ediScanningType === 3) {
                                    this.updateEdi(itemsToTransfer, successMessage, errorMessage);
                                } else if (ediScanningType === 4) {
                                    var dict: any = _.map(itemsToTransfer, "ediEntryId");
                                    this.createEdiPDFs(dict, successMessage, errorMessage);
                                } else {
                                    var dict: any = _.map(itemsToTransfer, "ediEntryId");
                                    this.transferEdi(dict, ediScanningType, successMessage, errorMessage);
                                }
                            } else if (removeDraft) {
                                var dict: any = _.map(itemsToTransfer, "supplierInvoiceId");
                                this.removeDraft(dict, successMessage, errorMessage);
                            }
                        }
                    }
                } else {
                    this.ignoreDateValidation = false;
                }

                this.ignoreDateValidation = false;
            });
        }
    }

    private transfer(dict: any[], successMessage: string, errorMessage: string) {
        switch (this.originStatusChange) {
            case SoeOriginStatusChange.DraftToOrigin:
                this.progress.startSaveProgress((completion) => {
                    this.supplierService.TransferSupplierInvoicesToDefinitive(dict).then((result) => {
                        if (result.success) {
                            let hideDialog: boolean = false;
                            if (result.stringValue)
                                completion.failed(result.stringValue);
                            if (this.supplierInvoiceTransferToVoucher) {
                                this.accountingService.calculateAccountBalanceForAccountsFromVoucher(this.currentAccountYearId).then((result) => {
                                    if (result.success) {
                                        //Do something?
                                    }
                                });

                                if (result.idDict && this.supplierInvoiceAskPrintVoucherOnTransfer) {
                                    hideDialog = true;
                                    // Get keys
                                    var voucherIds: number[] = []
                                    _.forEach(Object.keys(result.idDict), (key) => {
                                        voucherIds.push(Number(key));
                                    });

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

                                    const message = dict.length + " " + successMessage + "<br/>" + result.stringValue;
                                    const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OK);
                                    modal.result.then(val => {
                                        const message2 = this.terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + this.terms["economy.supplier.payment.askPrintVoucher"];
                                        const modal2 = this.notificationService.showDialog(this.terms["core.information"], message2, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                                        modal2.result.then(val => {
                                            if (val) {
                                                // Get id's from dictionary
                                                // Id's are stored as the keys
                                                var ids: number[] = [];
                                                _.forEach(Object.keys(result.idDict), (key) => {
                                                    ids.push(Number(key));
                                                });
                                                this.printVouchers(ids);
                                            }
                                        });
                                    });
                                }
                            }
                            completion.completed(null, null, hideDialog, (hideDialog === true ? "" : dict.length + " " + successMessage + "<br/>" + result.stringValue));
                            this.loadGridData();
                        }
                        else {
                            completion.failed(dict.length + " " + errorMessage + ".< br />" + result.errorMessage);
                        }
                    }, error => {
                        completion.failed(errorMessage);
                    });
                }, null);
                break;
            case SoeOriginStatusChange.OriginToVoucher:
                this.progress.showProgressDialog(this.terms["common.dailyrecurrencepattern.startdate"] + "...");

                this.currentGuid = Guid.newGuid();
                this.supplierService.transferSupplierInvoicesToVouchers(dict, this.currentGuid).then((result) => {
                    this.timerToken = setInterval(() => this.getTransferToVoucerProgress(dict, successMessage, errorMessage), 500);
                });
                break;
        }
    }

    private getTransferToVoucerProgress(dict: any[], successMessage: string, errorMessage: string) {
        this.coreService.getProgressInfo(this.currentGuid.toString()).then((x) => {
            this.progress.updateProgressDialogMessage(x.message + "...");
            if (x.abort == true)
                this.getTransferToVoucherProcessedResult(dict, successMessage, errorMessage);
        });
    }

    private getTransferToVoucherProcessedResult(dict: any[], successMessage: string, errorMessage: string) {
        clearInterval(this.timerToken);
        this.supplierService.getTransferSupplierInvoicesToVoucherResult(this.currentGuid).then((result) => {
            if (result.success) {
                if (result.stringValue) {
                    this.notificationService.showDialog(this.terms["core.warning"], (this.terms["economy.supplier.invoice.failedtransfertovoucher1"] + "\r\n" + result.stringValue + this.terms["economy.supplier.invoice.failedtransfertovoucher2"]), SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                }
                else {
                    if (result.intDict) {
                        _.forEach(Object.keys(result.intDict), (key) => {
                            this.accountingService.calculateAccountBalanceForAccountsFromVoucher(Number(key));
                        });
                    }
                    else {
                        this.accountingService.calculateAccountBalanceForAccountsFromVoucher(this.currentAccountYearId);
                    }

                    if (result.idDict && this.supplierInvoiceAskPrintVoucherOnTransfer) {

                        // Get values
                        let first: boolean = true;
                        let voucherNrs: string = "";
                        let voucherCount = 0;
                        _.forEach(result.idDict, (pair) => {
                            if (!first)
                                voucherNrs = voucherNrs + ", ";
                            else
                                first = false;
                            voucherNrs = voucherNrs + pair;
                            voucherCount = voucherCount + 1;
                        });

                        const message = voucherCount + " " + successMessage;
                        const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OK);
                        modal.result.then(val => {
                            const message2 = this.terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + this.terms["economy.supplier.payment.askPrintVoucher"];
                            const modal2 = this.notificationService.showDialog(this.terms["core.information"], message2, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                            modal2.result.then(val => {
                                if (val) {
                                    // Get id's from dictionary
                                    // Id's are stored as the keys
                                    const ids: number[] = [];
                                    _.forEach(Object.keys(result.idDict), (key) => {
                                        ids.push(Number(key));
                                    });
                                    this.printVouchers(ids);
                                }
                            });
                        });
                    }

                    this.loadGridData();
                }
            }
            else {
                this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            }

            this.progress.hideProgressDialog();
        });
    }

    private transferEdi(dict: any, state: number, successMessage: string, errorMessage: string) {
        if (state === 0) {
            this.progress.startSaveProgress((completion) => {
                this.supplierService.TransferEdiToInvoices(dict).then((result) => {
                    if (result.success) {
                        completion.completed(null, null, null, dict.length + " " + successMessage);
                    }
                    else {
                        completion.failed(dict.length + " " + errorMessage);
                    }
                }, error => {
                    completion.failed(dict.length + " " + errorMessage);
                });
            }, null).then(data => {
                this.loadGridData();
            }, error => {
            });
        }
        if (state === 1) {
            this.progress.startSaveProgress((completion) => {
                this.supplierService.TransferEdiState(dict, 1).then((result) => {
                    if (result.success) {
                        completion.completed(null, null, null, dict.length + " " + successMessage);
                    }
                    else {
                        completion.failed(dict.length + " " + errorMessage);
                    }
                }, error => {
                    completion.failed(dict.length + " " + errorMessage);
                });
            }, null).then(data => {
                this.loadGridData();
            }, error => {
            });
        }
        if (state === 2) {
            this.progress.startSaveProgress((completion) => {
                this.supplierService.TransferEdiState(dict, 2).then((result) => {
                    if (result.success) {
                        completion.completed(null, null, null, dict.length + " " + successMessage);
                    }
                    else {
                        completion.failed(dict.length + " " + errorMessage);
                    }
                }, error => {
                    completion.failed(dict.length + " " + errorMessage);
                });
            }, null).then(data => {
                this.loadGridData();
            }, error => {
            });
        }
    }

    private updateEdi(ediEntries: SupplierInvoiceGridDTO[], successMessage: string, errorMessage: string) {
        var items: UpdateEdiEntryDTO[] = [];
        _.forEach(ediEntries, (e: SupplierInvoiceGridDTO) => {
            var item = new UpdateEdiEntryDTO();
            item.ediEntryId = e.ediEntryId;
            item.supplierId = e.supplierId;
            item.attestGroupId = e.attestGroupId;
            items.push(item);
        });

        this.progress.startSaveProgress((completion) => {
            this.supplierService.updateEdiEntries(items).then((result) => {
                if (result.success) {
                    completion.completed(null, null, null, successMessage);
                }
                else {
                    completion.failed(errorMessage);
                }
            }, error => {
                completion.failed(errorMessage);
            });
        }, null).then(data => {
            this.loadGridData();
        }, error => {
        });
    }

    private createEdiPDFs(dict: any, successMessage: string, errorMessage: string) {
        this.progress.startSaveProgress((completion) => {
            this.supplierService.generateReportForEdi(dict).then((result) => {
                if (result.success) {
                    completion.completed(null, null, null, dict.length + " " + successMessage);
                }
                else {
                    completion.failed(dict.length + " " + errorMessage);
                }
            }, error => {
                completion.failed(dict.length + " " + errorMessage);
            });
        }, null).then(data => {
            this.loadGridData();
        }, error => {
        });
    }

    private removeDraft(dict: any, successMessage: string, errorMessage: string) {
        return this.progress.startDeleteProgress((completion) => {
            this.supplierService.deleteDraftInvoices(dict).then((result) => {
                if (result.integerValue2 > 0)
                    completion.failed(result.integerValue2 + " " + errorMessage);
                else {
                    completion.completed(null, false, dict.length + " " + successMessage);
                }
            });

        }, null).then(data => {
            this.loadGridData();
        }, error => {
        });
    }

    private validateInvoiceDates() {

        let message: string = "";
        let showMessageBox = false;
        const rows = this.gridAg.options.getSelectedRows();

        //Check if invoicedate is entered
        if (!InvoiceUtility.IsInvoiceDatesEntered(rows)) {
            message += this.terms["common.customer.invoices.invoicedatesnotentered"] + "<br>";
            showMessageBox = true;
        }

        //Check if duedate is entered
        if (!InvoiceUtility.IsDueDatesEntered(rows)) {
            message += this.terms["common.customer.invoices.duedatesnotentered"] + "<br>";
            showMessageBox = true;
        }

        if (this.supplierInvoiceTransferToVoucher) {
            message += this.terms["common.customer.invoices.autotovoucher"] + "<br>";
            showMessageBox = true;
        }

        if (showMessageBox) {
            message += this.terms["core.continue"];
            const modal = this.notificationService.showDialog(this.terms["core.controlquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.ignoreDateValidation = val;
                this.executeButtonFunction(this.buttonOption);
            });
        }

        return !showMessageBox;
    }

    private summarize(x) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        _.forEach(x, (y: any) => {
            this.filteredTotalIncVat += y.totalAmount;
            this.filteredTotalExVat += y.totalAmountExVat;
        });
        if (this.showVatFree)
            this.filteredTotal = this.filteredTotalIncVat;
        else
            this.filteredTotal = this.filteredTotalExVat;
    }

    private summarizeFiltered(x) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        _.forEach(x, (y: any) => {
            this.filteredTotalIncVat += y.totalAmount;
            this.filteredTotalExVat += y.totalAmountExVat;
        });
        if (this.showVatFree)
            this.filteredTotal = this.filteredTotalIncVat;
        else
            this.filteredTotal = this.filteredTotalExVat;
    }

    private summarizeSelected() {
        this.selectedTotal = 0;
        this.selectedTotalIncVat = 0;
        this.selectedTotalExVat = 0;
        const rows = this.gridAg.options.getSelectedRows();
        rows.forEach(r => {
            this.selectedTotalIncVat += r.totalAmount;
            this.selectedTotalExVat += r.totalAmountExVat;
        })
        this.$timeout(() => {
            if (this.showVatFree)
                this.selectedTotal = this.selectedTotalIncVat;
            else
                this.selectedTotal = this.selectedTotalExVat;
        });
    }

    private showVatFreeChanged() {
        this.$timeout(() => {
            if (this.showVatFree) {
                this.filteredTotal = this.filteredTotalIncVat;
                this.selectedTotal = this.selectedTotalIncVat;
            } else {
                this.filteredTotal = this.filteredTotalExVat;
                this.selectedTotal = this.selectedTotalExVat;
            }
        });
    }

    private printVouchers(ids: number[]) {
        
        if (this.supplierInvoiceVoucherReportId) {

            this.requestReportService.printVoucherList(ids);

        } else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private printSelectedInvoices(templateType: SoeReportTemplateType) {
        if (templateType === SoeReportTemplateType.SupplierBalanceList) {
            if (this.supplierBalanceListReportId) {
                const ids = [];
                _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                    ids.push(row.supplierInvoiceId);
                });

                this.isSupplierBalanceListPrinting = true;

                const reportItem = new BalanceListPrintDTO(ids);
                reportItem.companySettingType = CompanySettingType.SupplierDefaultBalanceList;

                this.requestReportService.printSupplierBalanceList(reportItem)
                .then(() => {
                    this.isSupplierBalanceListPrinting = false;
                });

            }
            else {
                this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            }
        }
        else if (templateType === SoeReportTemplateType.SupplierInvoiceJournal) {
            if (this.invoiceJournalReportId) {
                const ids = [];
                _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                    ids.push(row.supplierInvoiceId);
                });

                this.isSupplierInvoiceJournalPrinting = true;
                this.requestReportService.printInvoicesJournal(this.invoiceJournalReportId, ids)
                .then(() => {
                    this.isSupplierInvoiceJournalPrinting = false;

                });

            }
        }
    }

    private getEInvoices(ediSourceType: number) {
        if (!this.showGetEInvoices) {
            this.notificationService.showDialog(this.terms["core.error"], this.terms["common.customer.invoices.inexchangevalidation"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        return this.progress.startWorkProgress((completion) => {
            this.supplierService.addScanningEntrys(ediSourceType).then((result: IActionResult) => {
                if (result.success) {
                    completion.completed(null, false, result.infoMessage);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(null);
            });

        }, null).then(data => {
            this.loadGridData();
        }, error => {
        });
    }

    private showScanningMessage() {
        if (this.scanningUnprocessedCount > 0) {
            let message = this.scanningUnprocessedCount.toString();
            if (this.scanningUnprocessedCount === 1)
                message = message + " " + this.terms["economy.supplier.invoice.invoice"] + " ";
            else
                message = message + " " + this.terms["economy.supplier.invoice.invoices"] + " ";
            message = message + this.terms["economy.supplier.invoice.missinginterpretation"];
            this.notificationService.showDialog(this.terms["core.information"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        }
    }

    private uploadImages() {
        this.translationService.translate("core.fileupload.choosefiletoimport").then((term) => {
            const url = CoreUtility.apiPrefix + Constants.WEBAPI_CORE_FILES_UPLOAD_INVOICE + SoeEntityType.SupplierInvoice;
            const modal = this.notificationService.showFileUpload(url, term, true, true, true);
            modal.result.then(res => {
                var successCount: number = 0;
                var failedCount: number = 0;
                var dataStorageIds: number[] = [];

                _.forEach(res.result, (file) => {
                    if (file.success) {
                        dataStorageIds.push(Number(file.integerValue2));
                        successCount++;
                    }
                    else
                        failedCount++;
                });

                return this.progress.startWorkProgress((completion) => {
                    this.supplierService.saveInvoicesForUploadedImages(dataStorageIds).then((result) => {
                        if (result.success) {
                            this.isImagesUploaded = true;
                            completion.completed(null);
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null).then(data => {
                    this.loadGridData();
                }, error => {
                });

            }, error => {
                //this.failedWork(error.message)
            });
        });
    }

    private addToAttestFlow(rows: any[]) {
        var dict: any = [];
        if (rows) {
            let highestAmount: number = 0;
            _.forEach(rows, (y: any) => {
                if (y.supplierInvoiceId > 0) {
                    dict.push(y.supplierInvoiceId);
                    highestAmount = y.totalAmount >= highestAmount ? y.totalAmount : highestAmount;
                }
            });
            this.showAttestDialog(dict, highestAmount)
        }
    }

    //    var keys: string[] = [
    //        "core.warning",
    //        "economy.supplier.invoice.itemsunderlimitwarning",
    //        "economy.supplier.invoice.addedtoattestflowsuccess",
    //    ];

    //    //Create a collection of entries to move to attestflow
    //    this.translationService.translateMany(keys).then((terms) => {


    //            if (this.userIdNeededWithTotalAmount > 0 && this.totalAmountWhenUserReguired > 0) {
    //                _.forEach(rows, (y: any) => {
    //                    if (y.supplierInvoiceId > 0 && y.totalAmount <= this.totalAmountWhenUserReguired)
    //                        dictUnderLimit.push(y.supplierInvoiceId);
    //                });
    //            }

    //                if (dict.length != dictUnderLimit.length && dict.length > 1) {
    //                    var modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.invoice.itemsunderlimitwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
    //                    modal.result.then(val => {
    //                        this.showAttestDialog(dictUnderLimit);
    //                    }, (reason) => {
    //                        // User cancelled, do nothing
    //                    });
    //                }
    //                else {
    //                    this.showAttestDialog(dict);
    //                }
    //            }
    //            else {
    //                this.showAttestDialog(dict);
    //            }
    //        }
    //    });
    //}

    private showAttestDialog(dict: any[], highestAmount: number) {

        this.translationService.translate("economy.supplier.invoice.addedtoattestflowsuccess").then((term) => {
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
                    defaultAttestGroupId: () => { return this.defaultAttestGroupId },
                    highestAmount: () => { return highestAmount }
                }
            });

            modal.result.then(numberOfAffected => {
                if (numberOfAffected) {
                    this.notificationService.showDialog("", term.format(numberOfAffected), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    this.loadGridData();
                }
            }, function () {
            });
        });
    }

    private initStartAttestFlow(rows: any[]) {

        var rowsToAttest: any = [];
        var dictUnderLimit: any = [];
        var promises = [];

        const keys: string[] = [
            "core.verifyquestion",
            "core.warning",
            "economy.supplier.invoice.existingattestflowmessage",
            "economy.supplier.invoice.itemsunderlimitwarning"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            const deferral = this.$q.defer();
            promises.push(deferral.promise);
            if (this.userIdNeededWithTotalAmount > 0 && this.totalAmountWhenUserReguired > 0) {
                _.forEach(rows, (y: any) => {
                    if (y.totalAmount <= this.totalAmountWhenUserReguired)
                        rowsToAttest.push(y);
                });

                if (rows.length != rowsToAttest.length) {
                    const modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.supplier.invoice.itemsunderlimitwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        deferral.resolve();
                    }, (reason) => {
                        // User cancelled, do nothing
                        rowsToAttest = [];
                        deferral.resolve();
                    });
                }
                else {
                    rowsToAttest = rows;
                    deferral.resolve();
                }
            }
            else {
                rowsToAttest = rows;
                deferral.resolve();
            }

            var attestWorkFlowHeadIds: any = [];
            _.forEach(rowsToAttest, (y: any) => {
                var deferral = this.$q.defer();
                promises.push(deferral.promise);
                this.supplierService.getAttestWorkFlowHeadFromInvoiceId(y.supplierInvoiceId, false, false, false, false).then((attestWorkFlowHead) => {
                    if (attestWorkFlowHead) {
                        attestWorkFlowHeadIds.push(attestWorkFlowHead.attestWorkFlowHeadId);
                    }
                    deferral.resolve();
                })
            });

            this.$q.all(promises).then(() => {
                if (attestWorkFlowHeadIds.length > 0) {
                    const modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["economy.supplier.invoice.existingattestflowmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                    modal.result.then(val => {
                        if (val != null && val === true) {
                            this.supplierService.deleteAttestWorkFlows(attestWorkFlowHeadIds).then(() => {
                                this.startAttestFlow(rowsToAttest);
                            });
                        }
                    });
                }
                else
                    this.startAttestFlow(rowsToAttest);
            });


        });

    }

    private startAttestFlow(rows: any[]) {
        const keys: string[] = [
            "core.warning",
            "economy.supplier.invoice.addedtoattestflowsuccess",
            "economy.supplier.invoice.addedtoattestflownotsuccess",
            "economy.supplier.invoice.startattestflowinvalid",
            "economy.supplier.invoice.sendattestmessage",
            "core.startattestflow",
        ];

        var ids: any = [];
        if (rows) {
            _.forEach(rows, (y: any) => {
                ids.push(y.supplierInvoiceId);
            });

            this.translationService.translateMany(keys).then((terms) => {

                if (ids.length > 0) {

                    const modal = this.notificationService.showDialog(terms["core.startattestflow"], terms["economy.supplier.invoice.sendattestmessage"] + "?", SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                    modal.result.then(val => {
                        var sendMessage = (val);
                        this.progress.startSaveProgress((completion) => {
                            this.supplierService.saveAttestWorkFlowForInvoices(ids, sendMessage).then((result) => {
                                if (result.success === false) {
                                    completion.failed(result.errorMessage);
                                }
                                else {
                                    completion.completed(null);
                                    var message: string = "";
                                    if (result.integerValue > 0)
                                        message = message + terms["economy.supplier.invoice.addedtoattestflowsuccess"].format(result.integerValue.toString()) + ".\\n";
                                    if (result.integerValue2 > 0)
                                        message = message + terms["economy.supplier.invoice.addedtoattestflownotsuccess"].format(result.integerValue2.toString()) + ".\\n";
                                    this.notificationService.showDialog("", message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                                }

                            }, error => {
                                completion.failed(error.message);
                            });
                        }, null).then(data => {
                            this.loadGridData();
                        }, error => {
                        });
                    });
                }
            });
        }
    }

    private startAttestWorkFlowForInvoice(invoiceId: number) {

    }


    private sendAttestReminder(rows: any[]) {
        var Ids: any = [];
        if (rows) {
            //Gather only rows that are under attest
            _.forEach(rows, (y: any) => {
                if (y.attestStateId > 0)
                    Ids.push(y.supplierInvoiceId);
            });
            if (Ids) {
                this.translationService.translate("economy.supplier.invoice.sendattestflowreminderssuccess").then((term) => {
                    this.progress.startWorkProgress((completion) => {
                        this.supplierService.SendAttestReminders(Ids).then((result) => {
                            if (result.success) {
                                completion.completed(null, true);
                                this.notificationService.showDialog("", term, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    }, null).then(data => {
                        this.loadGridData();
                    }, error => {
                    });
                });
            }
        }
    }

    private hideUnhandled(rows: any[]) {
        this.progress.startWorkProgress((completion) => {
            this.supplierService.hideUnhandledInvoices(_.map(rows, r => r.supplierInvoiceId)).then((result) => {
                if (result.success) {
                    completion.completed(null);
                    this.loadGridData();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null).then(data => {
            this.loadGridData();
        }, error => {
        });
    }

    public edit(row: any) {
        if (this.modifyPermission)
            this.showInvoice(row);
    }

}