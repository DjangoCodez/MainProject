import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SoeGridOptionsEvent, SupplierInvoiceAttestFlowButtonFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IAttestWorkFlowOverviewGridDTO, IAttestWorkFlowHeadDTO, IAttestWorkFlowRowDTO, IActionResult, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { ShowPdfController } from "../../../Common/Dialogs/ShowPdf/ShowPdfController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { AttestWorkFlowUserSelectorController } from "../../../Shared/Economy/Supplier/Invoices/Dialogs/AttestWorkFlowUserSelector/AttestWorkFlowUserSelectorController";
import { TermGroup_Languages, SoeEntityType, SoeEntityImageType, SoeStatusIcon, SimpleTextEditorDialogMode, TextBlockType, SoeOriginStatusClassification, Feature, UserSettingType, SoeReportTemplateType, TermGroup_ChangeStatusGridAllItemsSelection, TermGroup, TermGroup_AttestEntity, SoeModule, SettingMainType, SoeDataStorageRecordType, AttestFlow_ReplaceUserReason, TermGroup_ProjectType, TermGroup_SupplierInvoiceType, TermGroup_ForeignPaymentBankCode, CompanySettingType, AccountingRowType, SoeEntityState, SupplierInvoiceAccountRowAttestStatus, SoeOriginStatus, TermGroup_InvoiceVatType, TermGroup_BillingType, SoeOriginType, SoeTimeCodeType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SupplierInvoiceDTO } from "../../../Common/Models/InvoiceDTO";
import { SupplierInvoiceRowDTO } from "../../../Common/Models/SupplierInvoiceRowDTO";
import { SupplierInvoiceAccountDimHelper } from "../../../Shared/Economy/Supplier/Invoices/Helpers/SupplierInvoiceAccountDimHelper";
import { AccordionSettingsController } from "../../../Common/Dialogs/AccordionSettings/AccordionSettingsController";
import { TextBlockDialogController } from "../../../Common/Dialogs/TextBlock/TextBlockDialogController";
import { SelectCustomerInvoiceController } from "../../../Common/Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { SelectProjectController } from "../../../Common/Dialogs/SelectProject/SelectProjectController";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { FilesHelper } from "../../../Common/Files/FilesHelper";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { FileUploadDTO } from "../../../Common/Models/FileUploadDTO";


type IProjectItem = {
    number: string,
    name: string,
    id: number,
    label: string
}
type IOrderItem = {
    number: number,
    id: number,
    label: string
    projectId: number,
    customerName: string,
}

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Config
    private currentAccountYearId: number = 0;
    private classification: SoeOriginStatusClassification;

    // Data
    private myActiveInvoices: IAttestWorkFlowOverviewGridDTO[];
    private myClosedInvoices: IAttestWorkFlowOverviewGridDTO[];
    private showInputFieldsWhenImage: boolean = false;

    // Functions
    private buttonFunctions: any = [];

    //FileUpload
    private filesHelper: FilesHelper;
    private loadingFiles = false;
    private newAttachedFiles: FileUploadDTO[] = [];

    // Flags        
    private isOverViewPage: boolean = true;
    private setupComplete: boolean;
    private hasSelectedInvoices: boolean = false;
    private showAttestHistory: boolean = false;

    private showTransactionCurrency: boolean = false;
    private showEnterpriseCurrency: boolean = false;
    private showLedgerCurrency: boolean = false;
    private isLockedAccountingRows: boolean = false;

    //currency
    currencyRate: number = 1;
    currencyDate: Date;
    currencyCode: string;
    baseCurrencyCode: string;
    ledgerCurrencyCode: string;
    isBaseCurrency: boolean;
    isLedgerCurrency: boolean;

    // Lookup
    attestStates: any[];
    allItemsSelectionDict: any[];
    billingTypes: ISmallGenericType[];
    vatTypes: ISmallGenericType[];
    vatCodes: any[];
    timecodes: any[] = [];

    // Totals
    filteredTotal = 0;
    selectedTotal = 0;
    filteredTotalExVat = 0;
    selectedTotalIncVat = 0;
    filteredTotalIncVat = 0;
    selectedTotalExVat = 0;
    showVatFree = true;

    // Permissions
    private multiSelectPermission: boolean;
    private imageOnlyPermission: boolean;
    private allowEditOrigin: boolean;
    private attestcomment: string = '';
    private finvoicePermission = false;
    private projectPermission = false;
    private ordersPermission = false;
    private fileUploadPermission = false;

    // Terms
    private terms: { [index: string]: string; };
    private dimName: string;
    private strSelectInvoicesToAcceptOrReject: string;

    // Collections
    private accountDims: any;
    private previousSelection: number[];

    private currentSelection: any;

    // Flags
    private invoiceHasBeenSelected = false;
    private invoicesHasBeenModified = false;
    private fullHeight = true;
    private accountingRowsExpanderOpen = false;
    private costAllocationExpanderOpen = false;

    private isAccountingrowsDirty = false;


    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        this.updateItemsSelection();
    }

    private widthRatio: any = 6;
    private gridWidthClass: any;
    private invoiceWidthClass: any;
    private gridIsHidden = false;


    private gridFooterComponentUrl: any;

    private invoiceId: number;
    private invoiceImage: any;
    private selectedInvoice: SupplierInvoiceDTO;
    private selectedAttestWorkFlowHead: IAttestWorkFlowHeadDTO;
    private accountDimHelper: SupplierInvoiceAccountDimHelper;
    private ediEntryId: number;

    // Cost allocation
    private defaultTimeCodeId = 0;
    private _selectedCustomerInvoice: IOrderItem;
    get selectedCustomerInvoice() {
        return this._selectedCustomerInvoice;
    }
    set selectedCustomerInvoice(item: IOrderItem | null) {
        this._selectedCustomerInvoice = item;
        if (this.selectedInvoice) {
            this.orderNrChanging(item);
        }
    }

    get selectedCustomerInvoiceName() {
        return this._selectedCustomerInvoice ? this._selectedCustomerInvoice.label : " ";
    }

    private _selectedProject: IProjectItem;
    get selectedProject() {
        return this._selectedProject;
    }
    set selectedProject(item: IProjectItem) {
        this._selectedProject = item;
        if (this.selectedInvoice) {
            this.projectChanging(item);
        }
    }

    get selectedProjectName() {
        return this._selectedProject ? this._selectedProject.label : " ";
    }

    private modalInstance: any;

    private modal: angular.ui.bootstrap.IModalService;

    //@ngInject
    constructor(
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private shortCutService: IShortCutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Economy.Supplier.AttestFlowOverview", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => this.onPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.doubleClickToEdit = false;
        this.setupComplete = false;
        this.modalInstance = $uibModal;

        // Config parameters
        this.currentAccountYearId = soeConfig.accountYearId;

        this.modal = $uibModal;

        this.messagingService.subscribe(Constants.EVENT_RELOAD_ATTEST_FLOW_OVERVIEW, () => {
            if (this.setupComplete && (this.classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive)) {
                this.attestcomment = '';
                this.loadGridData();
            }
        }, $scope);
        this.messagingService.subscribe(Constants.EVENT_INVOICE_CHANGED, (invoiceId) => {
            this.changeInvoiceById(invoiceId, false);
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_INVOICE_MODIFIED, (item) => {
            if (item) {
                this.invoicesHasBeenModified = item.dirty;
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_SET_DIRTY, (item) => {
            if (item) {
                //From accountingrows...
                this.isDirty = true;
                this.isAccountingrowsDirty = true;
            }
        }, $scope);

        this.messagingService.subscribe(Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_DIALOG, (parentGuid) => {
            if (parentGuid == this.guid) {
                if (this.currentSelection.seqNr)
                    this.$scope.$broadcast('accountDistributionName', this.currentSelection.seqNr + ", " + this.currentSelection.supplierNr + " " + this.currentSelection.supplierName);
                else {
                    this.translationService.translate("economy.supplier.invoice.seqnr").then(label => {
                        this.$scope.$broadcast('accountDistributionName', "[" + label + "]" + ", " + this.currentSelection.supplierNr + " " + this.currentSelection.supplierName);
                    });
                }

            }
        }, $scope);

        shortCutService.bindSave($scope, () => {
            if (this.isDirty) {
                this.saveCostAllocationRows();
                if (this.isAccountingrowsDirty && !this.isLockedAccountingRows) {
                    this.saveAccountingRows();
                }
            }
        });

        this.onTabActivated(() => this.localOnTabActivated());

        this.accountDimHelper = new SupplierInvoiceAccountDimHelper(this.$scope, () => { return this.selectedInvoice }, null, () => { return this.isLockedAccountingRows }, () => { return false });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Overview].readPermission;
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Overview].modifyPermission;
        this.multiSelectPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Overview_Multiselect].modifyPermission;
        this.imageOnlyPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_ImageOnly].modifyPermission;
        this.finvoicePermission = response[Feature.Economy_Supplier_Invoice_Finvoice].modifyPermission;
        this.projectPermission = response[Feature.Economy_Supplier_Invoice_Project].modifyPermission;
        this.ordersPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.fileUploadPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow_Upload_Documents].modifyPermission;
    }

    // SETUP
    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        this.classification = this.parameters.classification;

        this.isOverViewPage = (this.classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive);

        this.filesHelper = new FilesHelper(this.coreService, this.$q, undefined, false, SoeEntityType.SupplierInvoice, SoeEntityImageType.SupplierInvoice, () => this.invoiceId);
    }

    private localOnTabActivated() {
        if (!this.setupComplete) {
            this.flowHandler.start(this.getPermissions());
            this.setupComplete = true;
        }
    }

    private getPermissions(): any[] {
        var features: any[] = [];

        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_Overview, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_Overview_Multiselect, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_ImageOnly, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Finvoice, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Project, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow_Upload_Documents, loadReadPermissions: true, loadModifyPermissions: true });

        return features;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("attestOverViewHeader.html"));

        if (this.classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyClosed) {
            this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));

            // Footer
            this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
        }

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "common.settings", IconLibrary.FontAwesome, "fa-cog", () => {
            this.updateAccordionSettings();
        }, null, null
        )));

        var sizeGroup = ToolBarUtility.createGroup();

        sizeGroup.buttons.push(new ToolBarButton("", "", IconLibrary.FontAwesome, "fa-arrow-to-left",
            () => { this.setWidth(0); },
            null,
            null
        ))

        sizeGroup.buttons.push(new ToolBarButton("", "", IconLibrary.FontAwesome, "fa-columns",
            () => { this.setWidth(); },
            null,
            null
        ))
        sizeGroup.buttons.push(new ToolBarButton("", "", IconLibrary.FontAwesome, "fa-arrow-to-right",
            () => { this.setWidth(12); },
            null,
            null
        ))
        this.toolbar.addButtonGroup(sizeGroup);
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadAttestStates(),
            this.loadSelectionTypes(),
            this.loadUserSettings(),
            this.loadAccountDimName(),
            this.loadBillingTypes(),
            this.loadVatTypes(),
            this.loadVatCodes()
        ]).then(() => {
            this.lookupSupplierInvoice();
        });
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "core.edit",
            "core.warning",
            "core.attestflowaccept",
            "core.attestflowreject",
            "core.attestflowtransfertoother",
            "core.attestflowtransfertootherwithreturn",
            "common.customer.invoices.projectnr",
            "common.customer.invoices.ordernr",
            "economy.supplier.attestflowoverview.invoicenr",
            "economy.supplier.attestflowoverview.supplier",
            "economy.supplier.attestflowoverview.amount",
            "economy.supplier.attestflowoverview.duedate",
            "economy.supplier.attestflowoverview.fullypaid",
            "economy.supplier.attestflowoverview.selectinvoices",
            "economy.supplier.invoice.ourreference",
            "economy.supplier.attestflowoverview.invoiceid",
            "economy.supplier.attestflowoverview.atteststatename",
            "economy.supplier.attestflowoverview.currency",
            "economy.supplier.attestflowoverview.internaldescription",
            "economy.supplier.attestflowoverview.invoicedate",
            "economy.supplier.attestflowoverview.openinvoice",
            "economy.supplier.invoice.invoice",
            "economy.supplier.invoice.seqnr",
            "economy.supplier.invoice.openpdf",
            "economy.supplier.invoice.unpaid",
            "economy.supplier.invoice.paid2",
            "economy.supplier.invoice.partlypaid",
            "economy.supplier.attestflowoverview.unsavedchanges",
            "economy.supplier.attestflowoverview.atteststategreen",
            "economy.supplier.attestflowoverview.atteststatered",
            "economy.supplier.invoice.unblockforpayment",
            "economy.supplier.invoice.blockforpayment",
            "economy.supplier.invoice.matches.payment",
            "common.customer.invoices.seqnr",
            "economy.supplier.invoice.voucherdate",
            "common.reason",
            "economy.supplier.payment.paymentdate",
            "economy.accounting.accountdistribution.diffinrows"
        ];

        var exportFileName;
        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            switch (this.classification) {
                case SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive:
                    this.strSelectInvoicesToAcceptOrReject = terms["economy.supplier.attestflowoverview.selectinvoices"];
                    //, null, null, "modifiedCell", "markAsSelected"
                    this.gridAg.addColumnText("invoiceNr", terms["economy.supplier.attestflowoverview.invoicenr"], null, true);
                    this.gridAg.addColumnText("supplierName", terms["economy.supplier.attestflowoverview.supplier"], null, true);
                    this.gridAg.addColumnText("projectNr", terms["common.customer.invoices.projectnr"], null, true, { enableHiding: true, hide: true });
                    this.gridAg.addColumnText("orderNr", terms["common.customer.invoices.ordernr"], null, true, { enableHiding: true, hide: true });
                    this.gridAg.addColumnText("referenceOur", terms["economy.supplier.invoice.ourreference"], null, true, { enableHiding: true, hide: true });
                    this.gridAg.addColumnNumber("totalAmount", terms["economy.supplier.attestflowoverview.amount"], null, { enableHiding: true, decimals: 2 });
                    this.gridAg.addColumnText("seqNr", terms["common.customer.invoices.seqnr"], null, true, { enableHiding: true, hide: true });
                    this.gridAg.addColumnDate("voucherDate", terms["economy.supplier.invoice.voucherdate"], null, true, null, { enableHiding: true, hide: true });

                    this.gridAg.addColumnDate("dueDate", terms["economy.supplier.attestflowoverview.duedate"], null, true);

                    this.gridAg.addColumnDate("payDate", terms["economy.supplier.payment.paymentdate"], null, true, null, { hide: true });

                    if (this.dimName)
                        this.gridAg.addColumnText("costCentreName", this.dimName, null, true, { enableHiding: true, hide: true });

                    //this.gridAg.addColumnIcon(null, this.terms["economy.supplier.payment.registerpayment"], null, { icon: "fal fa-comment-dots", onClick: this.createPayment.bind(this) });

                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-comment-dots", showIcon: this.showAttestCommentIcon.bind(this), toolTipField: "attestComments" });//null, "showAttestCommentIcon", null, null, null, false, false, null, null, "attestComments");
                    this.gridAg.addColumnIcon("paymentStatusIcon", terms["economy.supplier.invoice.matches.payment"], null, { suppressSorting: false, enableHiding: true, toolTipField: "attestStateColorText", onClick: this.showBlockReason.bind(this) });
                    this.gridAg.addColumnEdit(terms["economy.supplier.attestflowoverview.openinvoice"], this.openInvoice.bind(this));

                    // Grid events
                    this.setupGridEvents();

                    exportFileName = "economy.supplier.attestflowoverview.overview";

                    break;
                case SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyClosed:
                    this.gridAg.addColumnText("seqNr", terms["economy.supplier.invoice.seqnr"], null, true);
                    this.gridAg.addColumnText("invoiceNr", terms["economy.supplier.attestflowoverview.invoicenr"], null, true);
                    this.gridAg.addColumnSelect("attestStateName", this.terms["economy.supplier.attestflowoverview.atteststatename"], null, { displayField: "attestStateName", selectOptions: this.attestStates });
                    this.gridAg.addColumnText("supplierName", terms["economy.supplier.attestflowoverview.supplier"], null, true);
                    this.gridAg.addColumnText("currency", terms["economy.supplier.attestflowoverview.currency"], null, true);
                    this.gridAg.addColumnText("projectNr", terms["common.customer.invoices.projectnr"], null, true);
                    this.gridAg.addColumnText("orderNr", terms["common.customer.invoices.ordernr"], null, true);
                    this.gridAg.addColumnText("internalDescription", terms["economy.supplier.attestflowoverview.internaldescription"], null, true);
                    this.gridAg.addColumnNumber("totalAmount", terms["economy.supplier.attestflowoverview.amount"], null, { enableHiding: true, decimals: 2 });
                    this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.attestflowoverview.invoicedate"], null, true, null, { sort: 'desc' });
                    let colDueDate2 = this.gridAg.addColumnDate("dueDate", terms["economy.supplier.attestflowoverview.duedate"], null, true);
                    colDueDate2.cellClass = (params) => { return (params.data.attestFlowOverdued ? " errorRow" : ""); };

                    var iterator: number = 2;
                    _.forEach(this.accountDims, (dim) => {
                        this.gridAg.addColumnText("defaultDim" + iterator.toString() + "Name", dim.name, null, true);
                        iterator++;
                    });

                    this.gridAg.addColumnShape("attestStateColor", null, 55, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestComments", showIconField: "attestStateColor", shapeField: "attestStateColor", colorField: "attestStateColor", attestGradient: true, gradientField: "useGradient" });

                    this.gridAg.addColumnIcon(null, this.terms["economy.supplier.invoice.openpdf"], null, { icon: "fal fa-file-search", showIcon: this.showOpenPictureIcon.bind(this), onClick: this.openPicture.bind(this) });
                    //this.gridAg.addColumnIcon(null, "fal fa-file-search", this.terms["economy.supplier.invoice.openpdf"], "openPicture", null, "showOpenPictureIcon", "", null, false);
                    this.gridAg.addColumnEdit(terms["economy.supplier.attestflowoverview.openinvoice"], this.openInvoice.bind(this));

                    // Grid events
                    var events: GridEvent[] = [];
                    events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
                    events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
                    events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => { this.summarizeFiltered(rows); }));
                    this.gridAg.options.subscribe(events);

                    exportFileName = "economy.supplier.attestflowoverview.myattested";
                    break;
            }

            if (!this.multiSelectPermission)
                this.gridAg.options.enableRowSelection = false;

            this.gridAg.options.getColumnDefs().forEach(f => {
                // Append closedRow to cellClass
                var cellcls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any) => {
                    if (grid.data.blockPayment) {
                        if (f.field === "dueDate") {
                            if (grid.data.attestFlowOverdued)
                                return cellcls + " errorRow";
                            else
                                return cellcls + " warningRow";
                        }
                        else {
                            return cellcls + " warningRow";
                        }
                    }
                    else {
                        if (f.field === "dueDate") {
                            if (grid.data.attestFlowOverdued)
                                return cellcls + " errorRow";
                            else
                                return cellcls;
                        }
                        else {
                            return cellcls;
                        }
                    }
                };
            });

            var gridOptions = (this.gridAg.options as any).gridOptions;
            gridOptions.getRowClass = (params) => {
                //console.log("getRowClass", params.data.invoiceId, this.invoiceId, params);
                return (params.data.invoiceId === this.invoiceId ? " activeRow" : "");
            };

            this.gridAg.finalizeInitGrid(exportFileName, true);

            this.messagingHandler.publishEvent(Constants.EVENT_TOGGLE_INVOICE_EDIT_PARAMS, { showInputFieldsWhenImage: this.showInputFieldsWhenImage, showBlockPaymentButton: false });
        });
    }

    private setupGridEvents() {
        var events: GridEvent[] = [];

        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: uiGrid.IGridRow) => {
            this.hasSelectedInvoices = (Array.isArray(rows) && rows.length > 0);
            this.changeInvoice(rows);
            this.updateButtonFunctions();
        }));

        events.push(new GridEvent(SoeGridOptionsEvent.RowClicked, (row: any) => {
            this.changeInvoice(row);

            if (!this.multiSelectPermission) {
                this.hasSelectedInvoices = true;
                this.updateButtonFunctions();
            }
        }));
        this.gridAg.options.subscribe(events);
    }

    private changeInvoiceById = _.debounce((invoiceId: number, sendReload: boolean) => {
        if (this.invoiceId === invoiceId) {
            return;
        }

        this.captureUploadedFiles();

        this.invoiceId = invoiceId;
        this.filesHelper.nbrOfFiles = '';
        this.filesHelper.reset();
        if (sendReload) {
            this.loadInvoice(invoiceId);
            if (this.imageOnlyPermission) {
                this.loadInvoiceImage(invoiceId);
                if (this.finvoicePermission) {
                    this.initShowFinvoice();
                }
            }
            else {
                this.messagingHandler.publishEvent(Constants.EVENT_RELOAD_INVOICE, this.invoiceId);
            }
        }

        //for the activeGridRow class to be changed...
        this.gridAg.options.refreshGrid();
    }, 700, { leading: true, trailing: false });

    private changeInvoice(row: any) {
        var invoiceId = -1;
        if (Array.isArray(row) && (row.length > 0)) {
            if (this.previousSelection) {
                _.forEach(row, (r) => {
                    if (invoiceId === -1 && !_.includes(this.previousSelection, r.invoiceId)) {
                        invoiceId = r.invoiceId;
                        this.currentSelection = r;
                    }
                });
            }
            else {
                this.currentSelection = row[row.length - 1];
                invoiceId = row[row.length - 1].invoiceId;
            }
            this.previousSelection = _.map(row, 'invoiceId');
        }
        else if (row && !Array.isArray(row)) {
            this.currentSelection = row;
            invoiceId = row.invoiceId;
        }
        this.changeInvoiceById(invoiceId, true);
    }

    //Capture uploaded files before resetting
    private captureUploadedFiles() {
        const files = this.filesHelper.getAsDTOs(undefined, undefined, true) ?? [];
        if (files.length > 0) {
            this.newAttachedFiles.push(...files);
        }
    }

    private loadFiles() {
        if (!this.currentSelection || !this.fileUploadPermission) {
            return;
        }

        this.loadingFiles = true;
        this.filesHelper.loadFiles()
            .finally(() => this.loadingFiles = false);
    }

    // SERVICE CALLS
    private loadCompanySettings() {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.SupplierInvoiceAllowEditOrigin);
        settingTypes.push(CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts);
        settingTypes.push(CompanySettingType.ProjectDefaultTimeCodeId);
        settingTypes.push(CompanySettingType.SupplierShowTransactionCurrency);
        settingTypes.push(CompanySettingType.SupplierShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.SupplierShowLedgerCurrency);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.allowEditOrigin = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAllowEditOrigin);
            this.accountDimHelper.useInternalAccountWithBalanceSheetAccounts = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseInternalAccountsWithBalanceSheetAccounts);
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProjectDefaultTimeCodeId);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierShowLedgerCurrency);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(UserSettingType.SupplierInvoicesAttestFlowMyClosed);
        settingTypes.push(UserSettingType.SupplierInvoiceShowInputFieldsWhenImage);
        settingTypes.push(UserSettingType.BillingSupplierAttestSlider);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this._allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.SupplierInvoicesAttestFlowMyClosed, TermGroup_ChangeStatusGridAllItemsSelection.Tree_Months, false);
            this.showInputFieldsWhenImage = SettingsUtility.getBoolUserSetting(x, UserSettingType.SupplierInvoiceShowInputFieldsWhenImage, false);
            if (x[UserSettingType.BillingSupplierAttestSlider])
                this.widthRatio = x[UserSettingType.BillingSupplierAttestSlider]
            this.setWidth();
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadAccountDimName(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, true, false, false, true).then(dims => {
            this.accountDims = dims;
        });
    }

    public loadAttestStates(): ng.IPromise<any> {
        return this.supplierService.getAttestStates(TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false).then((x) => {
            this.attestStates = [];
            _.forEach(x, (y: any) => {
                this.attestStates.push({ value: y.name, label: y.name, attestStateId: y.attestStateId });
            });
        });
    }

    private loadVatCodes(): ng.IPromise<any> {
        return this.accountingService.getVatCodes(true).then(x => {
            this.vatCodes = x;
        });
    }

    private loadVatTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, false, false).then(x => {
            this.vatTypes = _.filter(x, (y) => y.id < 7);
        });
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then(x => {
            this.billingTypes = x;
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

    public loadGridData() {
        if (this.classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive)
            this.loadMyActiveInvoicesGridData();
        else if (this.classification == SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyClosed)
            this.loadMyClosedInvoicesGridData();
    }

    private setPaymentStatusIcon(invoice: IAttestWorkFlowOverviewGridDTO) {

        if (invoice.blockPayment) {
            invoice["paymentStatusIcon"] = "fal fa-lock-alt errorColor";
            invoice["attestStateColorText"] = invoice.blockReason;
        }
        else if (invoice.fullyPaid) {
            invoice["paymentStatusIcon"] = "fas fa-circle okColor";
            invoice["attestStateColorText"] = this.terms["economy.supplier.attestflowoverview.atteststategreen"];
        }
        else {
            invoice["attestStateColorText"] = this.terms["economy.supplier.attestflowoverview.atteststatered"];
            invoice["paymentStatusIcon"] = "fas fa-circle errorColor";
        }

    }

    public loadMyActiveInvoicesGridData() {
        this.changeInvoiceById(-1, true);

        // Load data
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getAttestWorkFlowOverview(this.classification, TermGroup_ChangeStatusGridAllItemsSelection.All).then((x) => {
                this.myActiveInvoices = x;
                let invoiceIds: any[] = [];
                for (var i = 0; i < this.myActiveInvoices.length; i++) {
                    let invoice: IAttestWorkFlowOverviewGridDTO = this.myActiveInvoices[i];
                    var attestState = _.find(this.attestStates, i => i.attestStateId == invoice.attestStateId);
                    invoice.attestStateName = attestState.label;
                    this.setPaymentStatusIcon(invoice);
                    invoiceIds.push({ id: invoice.invoiceId, type: TermGroup_SupplierInvoiceType.Invoice });
                }

                this.setData(this.myActiveInvoices);
                this.messagingHandler.publishEvent(Constants.EVENT_SET_INVOICE_IDS, invoiceIds)
            })
        }]);
    }

    public loadMyClosedInvoicesGridData() {
        this.changeInvoiceById(-1, true);

        // Load data           
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getAttestWorkFlowOverview(this.classification, this.allItemsSelection).then((x) => {
                this.myClosedInvoices = x;
                _.forEach(this.myClosedInvoices, (invoice: IAttestWorkFlowOverviewGridDTO) => {
                    var attestState = _.find(this.attestStates, i => i.attestStateId == invoice.attestStateId);
                    invoice.attestStateName = (attestState) ? attestState.label : "";
                    if (invoice.lastPaymentDate && invoice.fullyPaid) {
                        invoice.attestStateColor = "#98EF5D";
                        invoice.attestComments = this.terms["economy.supplier.invoice.paid2"] + "\n" + CalendarUtility.toFormattedDate(CalendarUtility.convertToDate(invoice.lastPaymentDate));
                    }
                    else if (invoice.lastPaymentDate) {
                        invoice.attestStateColor = undefined;
                        invoice.attestComments = this.terms["economy.supplier.invoice.partlypaid"] + "\n" + CalendarUtility.toFormattedDate(CalendarUtility.convertToDate(invoice.lastPaymentDate));
                        invoice["useGradient"] = true;
                    }
                    else {
                        invoice.attestStateColor = "#FF3D3D";
                        invoice.attestComments = this.terms["economy.supplier.invoice.unpaid"];
                    }
                });

                this.setData(this.myClosedInvoices);

                // Calculate summaries
                this.summarize(this.myClosedInvoices);
            });
        }]);
    }

    public loadInvoiceImage(invoiceId: number): ng.IPromise<any> {
        return this.supplierService.getSupplierInvoiceImage(invoiceId).then(result => {
            this.invoiceImage = result;
        });
    }


    // EVENTS
    public openInvoice(row: any) {
        var message = new TabMessage(
            `${this.terms["economy.supplier.invoice.invoice"]} ${row.invoiceNr}`,
            row.invoiceId,
            EditController,
            { id: row.invoiceId },
            this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
        );

        this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.SupplierInvoicesAttestFlowMyClosed, this.allItemsSelection).then((x) => {
            this.loadGridData();
        });
    }

    private showOpenPictureIcon(row) {
        return row.hasPicture;
    }

    public openPicture(row: any) {

        this.loadInvoiceImage(row.invoiceId).then(() => {
            if (this.invoiceImage) {

                if (this.invoiceImage.imageFormatType === SoeDataStorageRecordType.InvoicePdf) {
                    var modal = this.$uibModal.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowPdf/ShowPdf.html"),
                        controller: ShowPdfController,
                        controllerAs: 'ctrl',
                        backdrop: 'static',
                        size: 'lg',
                        resolve: {
                            pdf: () => { return this.invoiceImage.image },
                            storageRecordId: () => { return undefined },
                            invoiceId: () => { return row.invoiceId },
                            invoiceNr: () => { return row.invoiceNr },
                            companyId: () => { return soeConfig.actorCompanyId }
                        }
                    });
                }
                else {
                    var options: angular.ui.bootstrap.IModalSettings = {
                        template: `<div class="messagebox">
                                <div class="modal-header">
                                    <button type="button" class="close" data-ng-click="ctrl.cancel()">&times;</button>                                    
                                    <h6 class="modal-title">{{ctrl.image.description || ''}}</h6>
                                </div>
                                <div class="modal-body" style="text-align:center">
                                    <img ng-if="ctrl.image" style="max-width: 100%;" data-ng-src="data:image/jpg;base64,{{ctrl.image.image}}" />
                                </div>
                            </div>`,
                        controller: ImageController,
                        controllerAs: "ctrl",
                        size: 'lg',
                        resolve: {
                            image: () => this.invoiceImage,
                            ediType: () => null,
                            ediEntryId: () => null,
                            scanningEntryId: () => null
                        }
                    }
                    this.$uibModal.open(options);

                }
            }
        });
    }

    private updateButtonFunctions() {
        this.buttonFunctions = [];

        if (this.hasSelectedInvoices) {
            this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.Accept, name: this.terms["core.attestflowaccept"] });
            if (this.getSelectedInvoiceIds().length === 1) {
                this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.TransferToOther, name: this.terms["core.attestflowtransfertoother"] });
                this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.TransferToOtherWithReturn, name: this.terms["core.attestflowtransfertootherwithreturn"] });
            }
            this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.Reject, name: this.terms["core.attestflowreject"] });

            this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.BlockPayment, name: this.terms["economy.supplier.invoice.blockforpayment"] });
            this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.UnBlockPayment, name: this.terms["economy.supplier.invoice.unblockforpayment"] });
        }
        this.$scope.$applyAsync();
    }

    private showAttestCommentIcon(row: any) {
        return row.showAttestCommentIcon;
    }

    private showBlockReason(row: IAttestWorkFlowOverviewGridDTO) {
        if (!row.blockPayment)
            return;

        this.notificationService.showDialog(this.terms["common.reason"], row.blockReason, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private initExecuteButtonFunction(option) {
        if (this.invoicesHasBeenModified) {
            var modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.attestflowoverview.unsavedchanges"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.executeButtonFunction(option);
            });
        }
        else {
            this.executeButtonFunction(option);
        }
    }

    protected initShowFinvoice() {
        this.supplierService.getEdiEntryFromInvoice(this.invoiceId).then((x) => {
            if (x)
                this.ediEntryId = x.ediEntryId;
            else
                this.ediEntryId = 0;
        });
    }

    protected showFinvoice() {
        // Show finvoice picture        
        var uri = window.location.protocol + "//" + window.location.host;
        uri = uri + "/soe/common/xslt/" + "?templatetype=" + SoeReportTemplateType.FinvoiceEdiSupplierInvoice + "&id=" + this.ediEntryId + "&c=" + soeConfig.actorCompanyId;
        window.open(uri, '_blank');
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case SupplierInvoiceAttestFlowButtonFunctions.Accept:
                this.accept();
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.TransferToOther:
                this.transferToOther(false);
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.TransferToOtherWithReturn:
                this.transferToOther(true);
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.Reject:
                this.reject();
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.BlockPayment:
                this.blockPaymentDialog(true, this.attestcomment);
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.UnBlockPayment:
                this.blockPaymentDialog(false, this.attestcomment);
                break;
        }
    }

    private loadInvoice(invoiceId: number) {
        if (invoiceId > 0 && this.imageOnlyPermission) {
            this.supplierService.getInvoice(invoiceId, false, false, true).then((invoice: SupplierInvoiceDTO) => {
                var accountingRows = SupplierInvoiceRowDTO.toAccountingRowDTOs(invoice.supplierInvoiceRows);
                invoice.accountingRows = _.orderBy(accountingRows.filter(x => x.type === AccountingRowType.AccountingRow && x.state !== SoeEntityState.Deleted), 'rowNr');
                this.selectedInvoice = invoice;
                this.selectedInvoice["billingTypeName"] = invoice.billingType ? this.billingTypes.filter(x => x.id === invoice.billingType)[0].name : "";
                this.selectedInvoice["vatTypeName"] = invoice.vatType ? this.vatTypes.filter(x => x.id === invoice.vatType)[0].name : "";
                this.selectedInvoice["vatCodeName"] = invoice.vatCodeId ? this.vatCodes.filter(x => x.vatCodeId === invoice.vatCodeId)[0].name : "";
                this.accountDimHelper.setDimIdValues(invoice);

                if (this.costAllocationExpanderOpen) {
                    this.loadCostAllocationRows(null, null);
                }
                else if (this.selectedInvoice.originStatus === SoeOriginStatus.Draft && this.selectedProject && !this.selectedInvoice.hasOrderRows && !this.selectedInvoice.hasProjectRows) {
                    this.selectedInvoice.supplierInvoiceCostAllocationRows = [];
                    this.selectedInvoice['costAllocationRowsLoaded'] = true;
                    this.costAllocationExpanderOpen = true;

                    this.$timeout(() => {
                        this.setOrderAndProject();
                    });

                    this.openTabAccordion();
                }

                if (!this.invoiceHasBeenSelected) {
                    this.accountingRowsExpanderOpen = true;
                    this.invoiceHasBeenSelected = true;

                    this.openTabAccordion();
                }

                // Mark image gallery with an asterix if any images or attachments are on the invoice
                var flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
                var statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(this.selectedInvoice.statusIcon);

                if (statusIcons.contains(SoeStatusIcon.Attachment) || statusIcons.contains(SoeStatusIcon.Image))
                    this.filesHelper.nbrOfFiles = '*';

                //Load files
                this.loadFiles();
            });

            this.getAttestWorkFlowHead(invoiceId).then((head) => {
                this.selectedAttestWorkFlowHead = head;
            });
        }
        else {
            this.selectedInvoice = null;
            this.selectedAttestWorkFlowHead = null;
            this.accountingRowsExpanderOpen = false;
            this.invoiceHasBeenSelected = false;
        }
    }

    private setIsLockedAccountingRows() {
        this.isLockedAccountingRows = !((this.selectedInvoice.originStatus === SoeOriginStatus.Draft) ||
            (this.selectedInvoice.originStatus === SoeOriginStatus.Origin && this.allowEditOrigin));
    }

    private saveAccountingRows() {
        if (this.selectedInvoice) {
            this.$scope.$broadcast('stopEditing', {
                functionComplete: () => {
                    if (this.askSaveUnbalanced()) {
                        var currentDimIds = this.accountDimHelper.getdDimIdValues();
                        this.progress.startSaveProgress((completion) => {
                            this.supplierService.saveSupplierInvoiceAccountingRows(this.selectedInvoice.invoiceId, this.selectedInvoice.accountingRows, currentDimIds).then((result: IActionResult) => {
                                if (!result.success) {
                                    var keys: string[] = [
                                        "core.unabletosave",
                                    ];
                                    this.translationService.translateMany(keys).then((terms) => {
                                        this.notificationService.showDialog(terms["core.unabletosave"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                    });
                                }
                                this.isDirty = false;
                                this.isAccountingrowsDirty = false;

                                completion.completed(Constants.EVENT_EDIT_SAVED);
                            });
                        }, this.guid);
                    };
                }
            });
        }
    }

    private askSaveUnbalanced(): boolean {
        let accountingRowsTotalAmount = _.sumBy(_.filter(this.selectedInvoice.accountingRows, x => !x.isDeleted), r => r.isDebitRow === true ? (r.debitAmountCurrency ? r.debitAmountCurrency : 0) : r.creditAmountCurrency ? (r.creditAmountCurrency * -1) : 0).round(2);

        if (accountingRowsTotalAmount !== 0) {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.accounting.accountdistribution.diffinrows"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            return false;
        }
        else {
            return true;
        }
    }

    private historyTabSelected() {
        this.showAttestHistory = true;
    }

    private closeTabAccordion() {
        var elem = document.getElementById("invoice-grid");
        if (elem) {
            $(elem).height(800);
        }
        this.$timeout(() => { this.messagingHandler.publishResizeWindow(); }, 250);
    }

    private openTabAccordion() {
        this.$timeout(() => { this.messagingHandler.publishResizeWindow(); }, 250);
    }

    // ACTIONS

    private blockPaymentDialog(block: boolean, text: string = "") {
        this.translationService.translateMany(["common.statereason", "economy.supplier.invoice.blockforpayment"]).then((terms) => {
            var options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
                controller: TextBlockDialogController,
                controllerAs: "ctrl",
                size: 'lg',
                resolve: {
                    text: () => { return text },
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
                    this.blockPayment(block, result.text);
                    this.attestcomment = "";
                }
            });
        });
    }


    private blockPayment(block: boolean, text: string) {
        var invoiceList = this.getSelectedInvoiceIds();

        _.forEach(invoiceList, (invoiceId: number) => {
            this.supplierService.blockSupplierInvoicePayment(invoiceId, block, text).then((result: IActionResult) => {// FIX BLOCK REASON
                if (result.success) {
                    this.loadGridData();
                }
                else {
                    console.log("failed", result.errorMessage);
                }
            });
        })
    }

    private accept() {
        // Save answers to attestflow with true flag to accept
        this.saveAnswersToAttestFlow(true);
    }

    private reject() {
        if (!this.attestcomment) {
            // Must have comment if reject
            var keys: string[] = [
                "core.unabletosave",
                "common.commentrequired"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.unabletosave"], terms["common.commentrequired"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            });
        } else {
            // Save answers to attestflow with false flag to reject
            this.saveAnswersToAttestFlow(false);
        }
    }

    private saveAnswersToAttestFlow(answer: boolean) {
        if (!this.hasSelectedInvoices)
            return;

		this.captureUploadedFiles();

        this.progress.startSaveProgress((completion) => {
            this.supplierService.saveAttestWorkFlowRowAnswers(this.getSelectedInvoiceIds(), this.attestcomment, answer, this.currentAccountYearId, this.newAttachedFiles).then((result) => {
                if (result.success) {
                    completion.completed(null);
                    this.attestcomment = '';
					this.newAttachedFiles = [];
                    this.changeInvoice(null);
                } else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid)
            .then(data => {
                this.loadGridData();
            }, error => {

            });
    }

    private getCurrentAttestRow(attestWorkFlowHeadId: number): ng.IPromise<IAttestWorkFlowRowDTO> {

        var deferral = this.$q.defer<IAttestWorkFlowRowDTO>();
        this.supplierService.getAttestWorkFlowTemplateHeadRowsUser(attestWorkFlowHeadId).then((rows: IAttestWorkFlowRowDTO[]) => {
            var selectedRows = rows.filter(x => x.isCurrentUser && !x.isDeleted);
            if (selectedRows && selectedRows.length > 0)
                deferral.resolve(selectedRows[0]);
            else
                deferral.resolve(null);
        });

        return deferral.promise;
    }

    private getAttestWorkFlowHeadId(invoiceId: number): ng.IPromise<number> {

        var deferral = this.$q.defer<number>();

        this.getAttestWorkFlowHead(invoiceId).then((head: IAttestWorkFlowHeadDTO) => {
            if (head) {
                deferral.resolve(head.attestWorkFlowHeadId);
            }
            else {
                deferral.resolve(0)
            }
        });

        return deferral.promise;
    }

    private getAttestWorkFlowHead(invoiceId: number): ng.IPromise<IAttestWorkFlowHeadDTO> {

        var deferral = this.$q.defer<IAttestWorkFlowHeadDTO>();

        this.supplierService.getAttestWorkFlowHeadFromInvoiceId(invoiceId, false, false, true, true).then((head: IAttestWorkFlowHeadDTO) => {
            if (head) {
                deferral.resolve(head);
            }
            else {
                deferral.resolve(undefined)
            }
        });

        return deferral.promise;
    }

    private transferToOther(withReturn: boolean) {

        if (!this.invoiceId) {
            return;
        }

        this.getAttestWorkFlowHeadId(this.invoiceId).then((attestWorkFlowHeadId: number) => {

            this.getCurrentAttestRow(attestWorkFlowHeadId).then((selectedAttestRow) => {

                // Show dialog to transfer to other
                if (!selectedAttestRow || !selectedAttestRow.attestWorkFlowRowId)
                    return;

                var result: any = [];
                var reason: AttestFlow_ReplaceUserReason = withReturn ? AttestFlow_ReplaceUserReason.TransferWithReturn : AttestFlow_ReplaceUserReason.Transfer;

                var options: angular.ui.bootstrap.IModalSettings = {
                    templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Dialogs/AttestWorkFlowUserSelector/Views/attestWorkFlowUserSelector.html"),
                    controller: AttestWorkFlowUserSelectorController,
                    controllerAs: "ctrl",
                    resolve: {
                        result: () => result,
                        row: () => selectedAttestRow,
                        reason: () => reason
                    }
                }
                this.modal.open(options).result.then((result: any) => {
                    if (result && result.selectedUser) {
                        this.replaceUser(reason, selectedAttestRow, result.selectedUser.userId, result.sendMessage);
                    }
                });
            });
        });
    }

    private replaceUser(reason: AttestFlow_ReplaceUserReason, row: any, replacementUserId: number, sendMessage: boolean) {
        this.supplierService.replaceAttestWorkFlowUser(reason, row.attestWorkFlowRowId, this.attestcomment, replacementUserId, this.invoiceId, sendMessage).then((result) => {
            if (result.success) {
                this.attestcomment = '';
                this.loadGridData();
            }
        });
    }


    // LOOKUPS

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

    // HELP-METHODS

    private getSelectedInvoiceIds() {
        var invoiceIds: number[] = [];

        if (!this.multiSelectPermission)
            invoiceIds.push(this.invoiceId);
        else
            invoiceIds = this.gridAg.options.getSelectedIds('invoiceId');

        return invoiceIds;
    }

    private markAsSelected(invoice) {
        return invoice.invoiceId === this.invoiceId;
    }

    // Cost allocation
    private setOrderAndProject() {
        if (this.selectedInvoice.orderNr)
            this.updateCustomerInvoice(this.selectedInvoice.orderCustomerInvoiceId, this.selectedInvoice.orderNr, this.selectedInvoice.orderProjectId, this.selectedInvoice.orderCustomerName);
        else
            this.selectedCustomerInvoice = null;
        if (this.selectedInvoice.projectId)
            this.updateProject(this.selectedInvoice.projectId, this.selectedInvoice.projectNr, this.selectedInvoice.projectName);
        else
            this.selectedProject = null;
    }

    private loadCostAllocationRows(selectedProject, selectedOrder, resizeGrid = false) {
        if (!this.selectedInvoice)
            return;

        if (!this.selectedInvoice['costAllocationRowsLoaded']) {
            this.setOrderAndProject();

            this.progress.startLoadingProgress([() => {
                this.selectedInvoice.supplierInvoiceCostAllocationRows = [];
                return this.supplierService.getSupplierInvoiceOrderProjectRows(this.selectedInvoice.invoiceId).then((x: any[]) => {
                    this.selectedInvoice.supplierInvoiceCostAllocationRows = x;

                    if (selectedOrder)
                        this.setOrderSelected(selectedOrder, selectedProject ? false : true);
                    else if (selectedProject)
                        this.setProjectSelected(selectedProject);

                    this.selectedInvoice['costAllocationRowsLoaded'] = true;

                });
            }]);
        }

        if (resizeGrid)
            this.openTabAccordion();
    }

    private orderNrChanging(item: IOrderItem | null) {
        this.selectedInvoice.orderNr = item?.number ?? null;
        this.selectedInvoice.orderCustomerName = item?.customerName ?? null;
        this.selectedInvoice.orderProjectId = item?.projectId ?? null;
    }

    private projectChanging(item: IProjectItem | null) {

        this.selectedInvoice.projectId = item?.id ?? null;
        this.selectedInvoice.projectName = item?.name ?? null;
        this.selectedInvoice.projectNr = item?.number ?? null;
    }

    private showOrderDialog() {
        if (!this.selectedInvoice)
            return;
        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomerInvoice", "selectcustomerinvoice.html"),
                controller: SelectCustomerInvoiceController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => { return term },
                    isNew: () => { return false },
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
                        this.selectedCustomerInvoice = null;
                    }
                    else if (result.invoice) {
                        const currentOrderId = this.selectedCustomerInvoice ? this.selectedCustomerInvoice.id : null;
                        const currentProjectId = this.selectedProject ? this.selectedProject.id : null;
                        this.setOrderSelected(result, false);
                        this.handleOrderProjectChange(currentOrderId, currentProjectId, this.selectedInvoice.supplierInvoiceCostAllocationRows);
                    }

                    this.isDirty = true;
                }
            });
        });
    }

    private setOrderSelected(result, ignoreSetProject) {
        this.updateCustomerInvoice(result.invoice.customerInvoiceId, result.invoice.number, result.invoice.projectId, result.invoice.customerName);
        if (!ignoreSetProject) {
            this.updateProject(result.invoice.projectId, result.invoice.projectNr, result.invoice.projectName);
        }
    }

    private showProjectDialog() {
        if (!this.selectedInvoice)
            return;

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
                    if (!this.selectedInvoice['costAllocationRowsLoaded']) {
                        this.loadCostAllocationRows(result, undefined);
                    }
                    else {
                        const currentOrderId = this.selectedCustomerInvoice ? this.selectedCustomerInvoice.id : null;
                        const currentProjectId = this.selectedProject ? this.selectedProject.id : null;
                        this.setProjectSelected(result);
                        this.handleOrderProjectChange(currentOrderId, currentProjectId, this.selectedInvoice.supplierInvoiceCostAllocationRows);
                    }
                }

                this.isDirty = true;
            }
        });
    }

    private setProjectSelected(result) {
        this.updateProject(result.projectId, result.number, result.name);
        if (this.selectedCustomerInvoice && this.selectedCustomerInvoice.projectId !== result.projectId)
            this.selectedCustomerInvoice = undefined;
    }

    private tryAddProjectRow() {
        if (this.selectedInvoice.projectId && (!this.selectedInvoice.supplierInvoiceCostAllocationRows || this.selectedInvoice.supplierInvoiceCostAllocationRows.length === 0)) {
            this.$timeout(() => {
                var timeCode = this.defaultTimeCodeId ? this.timecodes.find(x => x.value === this.defaultTimeCodeId) : undefined;
                this.$scope.$broadcast('addProjectRow', { guid: this.guid, project: this.selectedProject, timeCode: timeCode, amount: +(this.selectedInvoice.totalAmountCurrency - this.selectedInvoice.vatAmountCurrency) - _.sumBy(this.selectedInvoice.supplierInvoiceProjectRows, function (o) { return +o.amount; }) });
            });
        }
    }

    private saveCostAllocationRows() {
        if (this.selectedInvoice) {
            this.$scope.$broadcast('stopEditingCost', {
                functionComplete: () => {
                    this.progress.startSaveProgress((completion) => {
                        this.supplierService.saveSupplierInvoiceCostAllocationRows(
                            this.selectedInvoice.invoiceId,
                            this.selectedInvoice.supplierInvoiceCostAllocationRows,
                            this.selectedProject ? this.selectedProject.id : 0,
                            this.selectedCustomerInvoice ? this.selectedCustomerInvoice.id : 0,
                            this.selectedCustomerInvoice ? this.selectedCustomerInvoice.number : 0
                        ).then((result: IActionResult) => {
                            if (!result.success) {
                                var keys: string[] = [
                                    "core.unabletosave",
                                ];
                                this.translationService.translateMany(keys).then((terms) => {
                                    this.notificationService.showDialog(terms["core.unabletosave"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                });
                            }
                            else {
                                this.selectedInvoice['costAllocationRowsLoaded'] = undefined;
                                this.loadCostAllocationRows(null, null);
                            }
                            this.isDirty = false;
                            completion.completed(Constants.EVENT_EDIT_SAVED);
                        });
                    }, this.guid);
                }
            });
        }
    }

    // Totals
    private showVatFreeChanged() {
        if (this.showVatFree) {
            this.filteredTotal = this.filteredTotalIncVat;
            this.selectedTotal = this.selectedTotalIncVat;
        } else {
            this.filteredTotal = this.filteredTotalExVat;
            this.selectedTotal = this.selectedTotalExVat;
        }
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
        var rows = this.gridAg.options.getSelectedRows();
        _.forEach(rows, (y: any) => {
            this.selectedTotalIncVat += y.totalAmount;
            this.selectedTotalExVat += y.totalAmountExVat;
        });
        this.$timeout(() => {
            if (this.showVatFree)
                this.selectedTotal = this.selectedTotalIncVat;
            else
                this.selectedTotal = this.selectedTotalExVat;
        });
    }

    private setWidth(width = this.widthRatio) {
        this.gridIsHidden = false;
        if (width > 12) {
            width = 12;
        }
        else if (width < 0) {
            width = 0;
        }

        if (width == 0) {
            this.gridIsHidden = true;
            this.gridWidthClass = "hide"
        } else {
            this.gridWidthClass = "col-sm-" + width
        }

        if (width == 12)
            this.invoiceWidthClass = "hide"
        else
            this.invoiceWidthClass = "col-sm-" + (12 - width)

        this.updateHeight();
    }

    private updateHeight(timeout = 0) {
        this.$timeout(() => {
            this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW,
                { id: 'supplier_invoice_edit' });
        }, timeout)

    }

    private updateAccordionSettings() {

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AccordionSettings/Views/accordionsettings.html"),
            controller: AccordionSettingsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                coreService: () => { return this.coreService },
                userSettingType: () => { return null },
                accordionList: () => { return null },
                userSliderSettingType: () => { return UserSettingType.BillingSupplierAttestSlider }
            }
        });

        modal.result.then(ids => {
            this.loadUserSettings().then(() => this.setWidth())
        }, function () {
            //Cancelled
        });
    }


    private updateCustomerInvoice(orderId: number, orderNr: number, projectId: number, customerName: string) {
        this.selectedCustomerInvoice = {
            id: orderId,
            projectId: projectId,
            number: orderNr,
            label: orderNr + " " + customerName,
            customerName: customerName
        };
    }
    private updateProject(projectId: number, number: string, name: string) {
        this.selectedProject = {
            id: projectId,
            name: name,
            number: number,
            label: number + " " + name
        };
    }

    private handleOrderProjectChange(previousOrderId, previousProjectId, costAllocationRows: any[]) {
        if (!costAllocationRows || costAllocationRows.length === 0) this.tryAddProjectRow();
        else this.changeCostAllocationRows(previousOrderId, previousProjectId, costAllocationRows);
    }


    private changeCostAllocationRows(previousOrderId, previousProjectId, costAllocationRows: any[]) {
        if (!costAllocationRows || costAllocationRows.length === 0) return;
        costAllocationRows.forEach(row => {
            if (previousOrderId != row.orderId || previousProjectId != row.projectId)
                return;

            row.orderId = this.selectedCustomerInvoice?.id ?? row.orderId;
            row.orderNr = this.selectedCustomerInvoice?.number ?? row.orderNr;
            row.customerInvoiceNumberName = this.selectedCustomerInvoice?.label ?? row.customerInvoiceNumberName;

            row.projectId = this.selectedProject?.id ?? row.projectId;
            row.projectNr = this.selectedProject.number ?? row.ProjectNr;
            row.projectName = this.selectedProject.label ?? row.ProjectName;
            row.isModified = true;
        })
        this.selectedInvoice.supplierInvoiceCostAllocationRows = [...costAllocationRows];
    }
}

class ImageController {
    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private image: any, private ediType: number, private ediEntryId: number, private scanningEntryId: number) {
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

}

