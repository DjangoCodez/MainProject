import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService} from "../../../../Core/Services/UrlHelperService";
import { ImportService } from "../ImportService";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { Feature, CompanySettingType, SoeEntityState, SoeOriginType, TermGroup_EDIStatus, TermGroup, TermGroup_EDIOrderStatus, TermGroup_EDIInvoiceStatus, TermGroup_EdiMessageType, ActionResultSave, SoeReportTemplateType, SettingMainType, TermGroup_EDISourceType, UserSettingType, TermGroup_Country } from "../../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../../Util/Constants";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../../Util/Enumerations";
import { EdiEntryViewDTO, UpdateEdiEntryDTO } from "../../../../Common/Models/InvoiceDTO";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { TypeAheadOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ISmallGenericType, IActionResult } from "../../../../Scripts/TypeLite.Net4";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Modal
    modal: any;

    // Functions
    buttonFunctions: any = [];

    //Inparams
    private originType: SoeOriginType;
    private ediStatus: TermGroup_EDIStatus;
    private classification: SoeEntityState;

    // Permissions
    private payrollGroupPermission = false;
    private socialSecPermission = false;

    // Company settings
    private ediReportTemplateId: number;
    private createAutoAttestOnEdi = false;
    private disableAutoLoad = false;
    private allItemsSelectionSettingType = 0;

    // Collections
    private ediStatuses: any[] = [];
    private orderStatuses: any[] = [];
    private invoiceStatuses: any[] = [];
    private billingTypes: any[] = [];
    private suppliers: ISmallGenericType[];
    private allItemsSelectionDict: any[];

    // Flags
    public isModal = false;
    private setupComplete = false;
    private showSearchButton = false;
    private showSave = false;
    private showTransferOrderRows = false;
    private showCreateInvoice = false;
    private showCreatePdf = false;
    private saveReadOnly = true;
    private transferOrderRowsReadOnly = true;
    private createInvoiceReadOnly = true;
    private createPdfReadOnly = true;
    private callForReload = true;

    // Gui
    private timeout = null;
    toolbarInclude: any;
    gridFooterComponentUrl: any;
    private isOkToSave = false;
    private isOkToGeneratePdf = false;
    private isOkToTransferToSupplierInvoice = false;
    private isOkToTransferToOrder = false;

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete)
            this.updateItemsSelection();
    }

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private reportService: IReportService,
        private importService: ImportService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Billing.Import.Edi", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadUserSettings())
            .onBeforeSetUpGrid(() => this.loadSelectionTypes())
            .onBeforeSetUpGrid(() => this.loadEdiReportTemplateId())
            .onBeforeSetUpGrid(() => this.loadEdiStatuses())
            .onBeforeSetUpGrid(() => this.loadEdiOrderStatuses())
            .onBeforeSetUpGrid(() => this.loadEdiInvoicesStatuses())
            .onBeforeSetUpGrid(() => this.loadBillingTypes())
            .onBeforeSetUpGrid(() => this.loadSuppliers())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Billing/Import/Edi/Views/gridFooter.html");

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
        });
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.originType = parameters.originType;
        this.ediStatus = parameters.status;
        this.classification = this.ediStatus === TermGroup_EDIStatus.Unprocessed ? SoeEntityState.Active : SoeEntityState.Inactive;

        if (this.classification !== SoeEntityState.Active) {
            this.showSearchButton = true;
            this.toolbarInclude = this.urlHelperService.getViewUrl("gridHeader.html");
            if (this.originType === SoeOriginType.Order)
                this.allItemsSelectionSettingType = UserSettingType.EdiOrdersAllItemsSelection;
            else
                this.allItemsSelectionSettingType = UserSettingType.EdiSupplierInvoicesAllItemsSelection;
        }

        this.flowHandler.start([
            { feature: Feature.Billing_Import_EDI_All, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Import_XEEdi, loadReadPermissions: true, loadModifyPermissions: false },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Import_EDI_All].readPermission || response[Feature.Billing_Import_XEEdi].readPermission;
        this.modifyPermission = response[Feature.Billing_Import_EDI_All].modifyPermission || response[Feature.Billing_Import_XEEdi].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.CreateAutoAttestFromSupplierOnEDI];
        
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.createAutoAttestOnEdi = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CreateAutoAttestFromSupplierOnEDI);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {

        if (this.allItemsSelectionSettingType === 0)
            return;

        const settingTypes: number[] = [this.allItemsSelectionSettingType];
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, this.allItemsSelectionSettingType, 1, false);
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadEdiReportTemplateId(): ng.IPromise<any> {
        return this.reportService.getStandardReportId(SettingMainType.Company, CompanySettingType.AccountingDefaultAccountingOrder, SoeReportTemplateType.VoucherList).then((x) => {
            this.ediReportTemplateId = x;
        });
    }

    private loadEdiStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDIStatus, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.ediStatuses.push({ "id": y.id, "value": y.name });
            });
        });
    }

    private loadEdiOrderStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDIOrderStatus, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.orderStatuses.push({ "id": y.id, "value": y.name });
            });
        });
    }

    private loadEdiInvoicesStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDIInvoiceStatus, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.invoiceStatuses.push({ "id": y.id, "value": y.name });
            });
        });
    }

    private loadBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.billingTypes.push({ "id": y.id, "value": y.name });
            });
        });
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.importService.getSuppliersDict(true, true, true).then((x: ISmallGenericType[]) => {
            this.suppliers = x;
        });
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "core.open",
            "core.close",
            "core.delete",
            "common.amount",
            "common.customer.invoices.invoicedate",
            "common.customer.invoices.duedate",
            "billing.import.edi.downloadstatus",
            "billing.import.edi.orderstatus",
            "billing.import.edi.invoicestatus",
            "billing.import.edi.type",
            "billing.import.edi.invoicenr",
            "billing.import.edi.ordernr",
            "billing.order.syswholeseller",
            "billing.import.edi.supplier",
            "billing.import.edi.supplierordernr",
            "billing.import.edi.customernr",
            "billing.import.edi.showmoreinfo",
            "billing.import.edi.retrieveposts",
            "billing.import.edi.retrievepoststooltip",
            "common.date"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (this.originType === SoeOriginType.Order) {
                if (this.ediStatus === TermGroup_EDIStatus.Unprocessed) {
                    //Setup buttons
                    this.showSave = true;
                    this.showTransferOrderRows = true;
                    this.showCreatePdf = true;
                    this.buttonFunctions.push({ id: SoeEntityState.Inactive, name: terms["core.close"] });
                    this.buttonFunctions.push({ id: SoeEntityState.Deleted, name: terms["core.delete"] });

                    //Setup grid
                    this.gridAg.addColumnIsModified("isModified", "", null);
                    this.gridAg.addColumnSelect("statusName", terms["billing.import.edi.downloadstatus"], null, { displayField: "statusName", selectOptions: this.ediStatuses });
                    this.gridAg.addColumnSelect("orderStatusName", terms["billing.import.edi.orderstatus"], null, { displayField: "orderStatusName", selectOptions: this.orderStatuses });
                    this.gridAg.addColumnSelect("billingTypeName", terms["billing.import.edi.type"], null, { displayField: "billingTypeName", selectOptions: this.billingTypes });

                    this.gridAg.addColumnText("orderNr", terms["billing.import.edi.ordernr"], null, false, { editable: true });

                    var options = new TypeAheadOptionsAg();
                    options.source = (filter) => this.filterSuppliers(filter);
                    options.displayField = "name"
                    options.dataField = "name";
                    options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

                    this.gridAg.addColumnTypeAhead("supplierNrName", terms["billing.import.edi.supplier"], null, { typeAheadOptions: options, editable: true, cellClassRules: { "errorRow": (data) => { return data.data["hasInvalidSupplier"]; } }});

                    this.gridAg.addColumnText("sellerOrderNr", terms["billing.import.edi.supplierordernr"], null);
                    this.gridAg.addColumnText("wholesellerName", terms["billing.order.syswholeseller"], null);
                    this.gridAg.addColumnText("buyerId", terms["billing.import.edi.customernr"], null);
                    this.gridAg.addColumnNumber("sum", terms["common.amount"], null, null);
                    this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["common.customer.invoices.duedate"], null);
                    this.gridAg.addColumnDate("date", terms["common.date"], null);
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-alt", onClick: this.showOrder.bind(this), showIcon: this.showOrderIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-pdf", onClick: this.showPdf.bind(this), showIcon: this.showPdfIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-info-circle infoColor", onClick: this.showInfo.bind(this), toolTip: terms["billing.import.edi.showmoreinfo"] });
                }
                else {
                    //Set delta mode
                    this.gridAg.options.immutableData = true;
                    this.gridAg.options.useGetRowNodeId((data) => {
                        return data.ediEntryId;
                    }); 

                    //Setup buttons
                    this.showCreatePdf = true;
                    this.buttonFunctions.push({ id: SoeEntityState.Active, name: terms["core.open"] });
                    this.buttonFunctions.push({ id: SoeEntityState.Inactive, name: terms["core.close"] });
                    this.buttonFunctions.push({ id: SoeEntityState.Deleted, name: terms["core.delete"] });

                    //Setup grid
                    this.gridAg.addColumnIsModified("isModified", "", null);
                    this.gridAg.addColumnSelect("statusName", terms["billing.import.edi.downloadstatus"], null, { displayField: "statusName", selectOptions: this.ediStatuses });
                    this.gridAg.addColumnSelect("orderStatusName", terms["billing.import.edi.orderstatus"], null, { displayField: "orderStatusName", selectOptions: this.invoiceStatuses });
                    this.gridAg.addColumnSelect("billingTypeName", terms["billing.import.edi.type"], null, { displayField: "billingTypeName", selectOptions: this.billingTypes });

                    this.gridAg.addColumnText("orderNr", terms["billing.import.edi.ordernr"], null, false, { editable: true });

                    const options = new TypeAheadOptionsAg();
                    options.source = (filter) => this.filterSuppliers(filter);
                    options.displayField = "name"
                    options.dataField = "name";
                    options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

                    this.gridAg.addColumnText("supplierNrName", terms["billing.import.edi.supplier"], null);

                    this.gridAg.addColumnText("sellerOrderNr", terms["billing.import.edi.supplierordernr"], null);
                    this.gridAg.addColumnText("wholesellerName", terms["billing.order.syswholeseller"], null);
                    this.gridAg.addColumnText("buyerId", terms["billing.import.edi.customernr"], null);
                    this.gridAg.addColumnNumber("sum", terms["common.amount"], null, null);
                    this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["common.customer.invoices.duedate"], null);
                    this.gridAg.addColumnDate("date", terms["common.date"], null);
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-alt", onClick: this.showInvoice.bind(this), showIcon: this.showInvoiceIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-pdf", onClick: this.showPdf.bind(this), showIcon: this.showPdfIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-info-circle infoColor", onClick: this.showInfo.bind(this), toolTip: terms["billing.import.edi.showmoreinfo"] });
                }
            }
            else {
                if (this.ediStatus === TermGroup_EDIStatus.Unprocessed) {
                    //Setup buttons
                    this.showSave = true;
                    this.showTransferOrderRows = true;
                    this.showCreateInvoice = true;
                    this.showCreatePdf = true;
                    this.buttonFunctions.push({ id: SoeEntityState.Inactive, name: terms["core.close"] });
                    this.buttonFunctions.push({ id: SoeEntityState.Deleted, name: terms["core.delete"] });

                    //Setup grid
                    this.gridAg.addColumnIsModified("isModified", "", null);
                    this.gridAg.addColumnSelect("statusName", terms["billing.import.edi.downloadstatus"], null, { displayField: "statusName", selectOptions: this.ediStatuses });
                    this.gridAg.addColumnSelect("invoiceStatusName", terms["billing.import.edi.invoicestatus"], null, { displayField: "invoiceStatusName", selectOptions: this.invoiceStatuses });
                    this.gridAg.addColumnSelect("orderStatusName", terms["billing.import.edi.orderstatus"], null, { displayField: "orderStatusName", selectOptions: this.orderStatuses });
                    this.gridAg.addColumnSelect("billingTypeName", terms["billing.import.edi.type"], null, { displayField: "billingTypeName", selectOptions: this.billingTypes });
                    this.gridAg.addColumnText("invoiceNr", terms["billing.import.edi.invoicenr"], null, false);
                    this.gridAg.addColumnText("orderNr", terms["billing.import.edi.ordernr"], null, false, { editable: true });

                    var options = new TypeAheadOptionsAg();
                    options.source = (filter) => this.filterSuppliers(filter);
                    options.displayField = "name"
                    options.dataField = "name";
                    options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

                    //this.gridAg.addColumnTypeAhead("supplierNrName", terms["billing.import.edi.supplier"], null, { typeAheadOptions: options, editable: true });
                    this.gridAg.addColumnTypeAhead("supplierNrName", terms["billing.import.edi.supplier"], null, { typeAheadOptions: options, editable: true, cellClassRules: { "errorRow": (data) => { return data.data["hasInvalidSupplier"]; } } });
                    //this.gridAg.addColumnText("supplierNrName", terms["billing.import.edi.supplier"], null);

                    this.gridAg.addColumnText("sellerOrderNr", terms["billing.import.edi.supplierordernr"], null);
                    this.gridAg.addColumnText("wholesellerName", terms["billing.order.syswholeseller"], null);
                    this.gridAg.addColumnText("buyerId", terms["billing.import.edi.customernr"], null);
                    this.gridAg.addColumnNumber("sum", terms["common.amount"], null, null);
                    this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["common.customer.invoices.duedate"], null);
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-alt", onClick: this.showInvoice.bind(this), showIcon: this.showInvoiceIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-pdf", onClick: this.showPdf.bind(this), showIcon: this.showPdfIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-info-circle infoColor", onClick: this.showInfo.bind(this), toolTip: terms["billing.import.edi.showmoreinfo"] });
                }
                else {
                    //Set delta mode
                    this.gridAg.options.immutableData = true;
                    this.gridAg.options.useGetRowNodeId((data) => {
                        return data.ediEntryId;
                    });

                    //Setup buttons
                    this.showCreatePdf = true;
                    this.buttonFunctions.push({ id: SoeEntityState.Active, name: terms["core.open"] });
                    this.buttonFunctions.push({ id: SoeEntityState.Inactive, name: terms["core.close"] });
                    this.buttonFunctions.push({ id: SoeEntityState.Deleted, name: terms["core.delete"] });

                    //Setup grid
                    this.gridAg.addColumnIsModified("isModified", "", null);
                    this.gridAg.addColumnSelect("statusName", terms["billing.import.edi.downloadstatus"], null, { displayField: "statusName", selectOptions: this.ediStatuses });
                    this.gridAg.addColumnSelect("invoiceStatusName", terms["billing.import.edi.invoicestatus"], null, { displayField: "invoiceStatusName", selectOptions: this.invoiceStatuses });
                    this.gridAg.addColumnSelect("orderStatusName", terms["billing.import.edi.orderstatus"], null, { displayField: "orderStatusName", selectOptions: this.orderStatuses });
                    this.gridAg.addColumnSelect("billingTypeName", terms["billing.import.edi.type"], null, { displayField: "billingTypeName", selectOptions: this.billingTypes });
                    this.gridAg.addColumnText("invoiceNr", terms["billing.import.edi.invoicenr"], null);
                    this.gridAg.addColumnText("orderNr", terms["billing.import.edi.ordernr"], null);

                    var options = new TypeAheadOptionsAg();
                    options.source = (filter) => this.filterSuppliers(filter);
                    options.displayField = "name"
                    options.dataField = "id";
                    options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

                    this.gridAg.addColumnTypeAhead("supplierNrName", terms["billing.import.edi.supplier"], null, { typeAheadOptions: options, editable: true });
                    //this.gridAg.addColumnText("supplierNrName", terms["billing.import.edi.supplier"], null);

                    this.gridAg.addColumnText("sellerOrderNr", terms["billing.import.edi.supplierordernr"], null);
                    this.gridAg.addColumnText("wholesellerName", terms["billing.order.syswholeseller"], null);
                    this.gridAg.addColumnText("buyerId", terms["billing.import.edi.customernr"], null);
                    this.gridAg.addColumnNumber("sum", terms["common.amount"], null, null);
                    this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["common.customer.invoices.duedate"], null);
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-alt infoColor", onClick: this.showInvoice.bind(this), showIcon: this.showInvoiceIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-pdf infoColor", onClick: this.showPdf.bind(this), showIcon: this.showPdfIcon.bind(this) });
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-info-circle infoColor", onClick: this.showInfo.bind(this), toolTip: terms["billing.import.edi.showmoreinfo"] });
                }
            }

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.beginCellEdit(entity, colDef); }));
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("common.customer.invoices.edi", true);

            this.setupComplete = true;
        });
    }

    private filterSuppliers(filter) {
        return this.suppliers.filter(supplier => {
                return supplier.name.contains(filter);
        });
    }

    protected allowNavigationFromTypeAhead(value, entity, colDef) {
        if (!value)  // If no value, allow it.
            return true;

        const matched = _.some(this.suppliers, (p) => p.name === value);
        if (matched) {
            return true;
        }
        else {
            return false;
        }
    }

    private beginCellEdit(row: EdiEntryViewDTO, colDef: uiGrid.IColumnDef) {
        if (this.originType === SoeOriginType.Order && this.showOrderIcon(row)) {
            const keys: string[] = [
                "core.info",
                "billing.import.edi.transferredtoorderwarning",
            ];

            this.translationService.translateMany(keys).then((terms) => {

                this.notificationService.showDialog(terms["core.info"], terms["billing.import.edi.transferredtoorderwarning"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                this.gridAg.options.stopEditing(true);
            });
        }
    }

    private afterCellEdit(row: EdiEntryViewDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'supplierNrName':
                const supplier = _.find(this.suppliers, { name: newValue });
                if (supplier) {
                    row.supplierId = supplier.id;
                    row.supplierName = "";
                    row.supplierNr = "";
                    row["hasInvalidSupplier"] = false;
                }
                else {
                    row.supplierNrName = oldValue;
                }
                break;
        }

        row.isModified = true;
        this.gridAg.options.selectRow(row);
    }

    private clearFlags() {
        this.isOkToSave = false;
        this.isOkToGeneratePdf = false;
        this.isOkToTransferToSupplierInvoice = false;
        this.isOkToTransferToOrder = false;
    }

    private gridSelectionChanged() {
        this.clearFlags();
        this.$scope.$applyAsync(() => {
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                if (this.isOkToSaveRow(row))
                    this.isOkToSave = true;
                if (this.isOkTransferToSupplierInvoice(row))
                    this.isOkToTransferToSupplierInvoice = true;
                if (this.isOkTransferToOrder(row))
                    this.isOkToTransferToOrder = true;
                if (this.okToGeneratePdf(row))
                    this.isOkToGeneratePdf = true;
            });
        });
    }

    private isOkToSaveRow(item: EdiEntryViewDTO) {
        return item.isModified;
    }

    private isOkTransferToSupplierInvoice(item: EdiEntryViewDTO)
    {
        return (item.invoiceNr && item.invoiceNr.length > 0) &&
                !item.invoiceId &&
                (item.supplierId) &&
                item.ediMessageType == TermGroup_EdiMessageType.SupplierInvoice &&
                item.invoiceStatus == TermGroup_EDIInvoiceStatus.Unprocessed &&
                (item.status == TermGroup_EDIStatus.UnderProcessing || item.status == TermGroup_EDIStatus.Processed);
    }

    private isOkTransferToOrder(item: EdiEntryViewDTO)
    {
        return ((item.orderNr && item.orderNr.length > 0) &&
            (item.orderStatus == TermGroup_EDIOrderStatus.Unprocessed) &&
            (item.status == TermGroup_EDIStatus.Processed || item.status == TermGroup_EDIStatus.UnderProcessing)); 
    }

    private okToGeneratePdf(item: EdiEntryViewDTO)
    {
        return (item.status == TermGroup_EDIStatus.UnderProcessing || item.status == TermGroup_EDIStatus.Processed) && !item.hasPdf && this.ediReportTemplateId > 0;
    }

    private showOrder(row: any) {
        this.translationService.translate("common.order").then((term) => {
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, {
                id: row.orderId,
                name: term + " " + row.orderNr,
            });
        });
    }

    private showOrderIcon(item: any) {
        return ((item.orderId && item.orderId > 0) &&
            (item.orderStatus === TermGroup_EDIOrderStatus.Processed) &&
            (item.status === TermGroup_EDIStatus.UnderProcessing || item.status === TermGroup_EDIStatus.Processed));
    }

    private showInvoice(row: any) {
        this.translationService.translate("common.supplierinvoice").then((term) => {
            this.messagingService.publish(Constants.EVENT_OPEN_EDITSUPPLIERINVOICE, {
                id: row.invoiceId,
                name: term + " " + row.invoiceNr,
            });
        });
    }

    private showInvoiceIcon(item: any) {
        return ((item.invoiceId && item.invoiceId > 0) &&
            (item.invoiceStatus === TermGroup_EDIInvoiceStatus.Processed) &&
            (item.status === TermGroup_EDIStatus.UnderProcessing || item.status === TermGroup_EDIStatus.Processed));
    }

    private showPdf(row: any) {
        const ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + row.ediEntryId;
        window.open(ediPdfReportUrl, '_blank');
    }

    private showPdfIcon(item: any) {
        return item.hasPdf;
    }

    private showInfo(item: EdiEntryViewDTO) {
        if (!item)
            return;

        // Columns
        const keys: string[] = [
            "core.info",
            "core.source",
            "common.currency",
            "common.errormessage",
            "billing.import.edi.messagefromoperator",
            "billing.import.edi.errorftp",
            "billing.import.edi.erroredi",
            "billing.import.edi.errorxml",
            "billing.import.edi.errordownload",
            "billing.import.edi.errorinterpretation",
            "billing.import.edi.errorunknown",
            "billing.import.edi.invoicestatus",
            "billing.import.edi.invoicenrmissing",
            "billing.import.edi.suppliermissing",
            "billing.import.edi.orderstatus",
            "billing.import.edi.ordernrmissing",
            "billing.import.edi.wholesellermissing",
            "billing.import.edi.downloaddate",
            "billing.import.edi.messagetype",
        ];

        this.translationService.translateMany(keys).then((terms) => {

            var message: string = "";

            if (item.operatorMessage && item.operatorMessage.length > 0) {
                message += terms["billing.import.edi.messagefromoperator"] + "\r\n";
                message += item.operatorMessage + "\r\n" + "\r\n";
            }

            if (item.status !== TermGroup_EDIStatus.Processed && (item.errorCode && item.errorCode > 0)) {

                message += terms["common.errormessage"] + ":\r\n";
                switch (item.errorCode) {
                    case ActionResultSave.EdiInvalidUri:
                        message += terms["billing.import.edi.errorftp"];
                        break;
                    case ActionResultSave.EdiInvalidType:
                        message += terms["billing.import.edi.erroredi"];
                        break;
                    case ActionResultSave.EdiFailedParse:
                        message += terms["billing.import.edi.errorxml"];
                        break;
                    case ActionResultSave.EdiFailedFileListing:
                        message += terms["billing.import.edi.errordownload"];
                        break;
                    case ActionResultSave.EdiFailedUnknown:
                        message += terms["billing.import.edi.errorinterpretation"];
                        break;
                    default:
                        message += terms["billing.imprt.edi.errorunknown"];
                        break;
                }
                message += "\r\n" + "\r\n";
            }

            if (item.invoiceStatus === TermGroup_EDIInvoiceStatus.Error) {
                message += terms["billing.import.edi.invoicestatus"] + ": " + "\r\n";
                if (!item.invoiceNr || item.invoiceNr.length === 0) {
                    message += terms["billing.import.edi.invoicenrmissing"] + "\r\n";
                }

                if (!item.supplierId) {
                    message += terms["billing.import.edi.suppliermissing"] + "\r\n";
                }
                message += "\r\n";
            }

            if (item.orderStatus == TermGroup_EDIOrderStatus.Error) {
                message += terms["billing.import.edi.orderstatus"] + ": " + "\r\n";
                if (!item.orderNr || item.orderNr.length === 0) {
                    message += terms["billing.import.edi.ordernrmissing"] + "\r\n";
                }

                if (!item.wholesellerName || item.wholesellerName.length === 0|| item.wholesellerId == 0) {
                    message += terms["billing.import.edi.wholesellermissing"] + "\r\n";
                }
                message += "\r\n";
            }

            message += terms["core.source"] + ": " + item.sourceTypeName + "\r\n";
            message += terms["common.currency"] + ": " + item.currencyCode + "\r\n";
            message += terms["billing.import.edi.downloaddate"] + ": " + (item.created ? CalendarUtility.toFormattedDateAndTime(item.created) : "NULL") + "\r\n";
            message += terms["billing.import.edi.messagetype"] + ": " + (item.ediMessageTypeName) + "\r\n";

            this.notificationService.showDialog(terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());

        // Commented to remove as per PBI #82267
        //Setup toolbar
        //if (soeConfig.sysCountryId === TermGroup_Country.FI) {
        //    this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.import.edi.retrieveposts", "billing.import.edi.retrievepoststooltip", IconLibrary.FontAwesome, "fa-download",
        //        () => { this.addEdiEntries(); }
        //    )));
        //}

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.search", "core.search", IconLibrary.FontAwesome, "fa-search", () => {
            this.search();
        }, null, () => { return !this.showSearchButton })));

        this.toolbar.addInclude(this.toolbarInclude);
    }

    public loadGridData() {
        if (this.classification === SoeEntityState.Active) {
            this.clearFlags();
            this.progress.startLoadingProgress([() => {
                return this.importService.getEdiEntryViews(this.classification, this.originType).then(x => {
                    _.forEach(x, (row: EdiEntryViewDTO) => {
                        //Fix dates
                        if (row.date)
                            row.date = new Date(<any>row.date).date();
                        if (row.dueDate)
                            row.dueDate = new Date(<any>row.dueDate).date();
                        if (row.invoiceDate)
                            row.invoiceDate = new Date(<any>row.invoiceDate).date();
                        
                        row.supplierNrName = row.supplierNr + " " + row.supplierName;
                        if (!row.supplierNr)
                            row.supplierNrName = "";

                        row["hasInvalidSupplier"] = this.suppliers.filter(r => r.id === row.supplierId).length === 0 ? true : false;
                    });
                    return x;
                }).then(data => {
                    this.setData(data);
                });
            }]);
        }
    }

    public search() {
        var filterModels = this.gridAg.options.getFilterModels();
        if (filterModels)
            this.loadFilteredGridData(filterModels);
    }

    public loadFilteredGridData(filterModels: any) {
        var filterValues: any[] = [];
        var billingTypes = [];
        if (filterModels["billingTypeName"]) {
            _.forEach(filterModels["billingTypeName"], (value) => {
                var billingType = _.find(this.billingTypes, { value: value.toString() });
                if (billingType)
                    billingTypes.push(billingType.id);
                
            });
        }
        var buyerId: string = filterModels["buyerId"] ? filterModels["buyerId"].filter : "";
        var dueDate: Date = filterModels["dueDate"] ? new Date(<any>filterModels["dueDate"].dateFrom) : null;
        var invoiceDate: Date = filterModels["invoiceDate"] ? new Date(<any>filterModels["invoiceDate"].dateFrom) : null;
        var orderNr: string = filterModels["orderNr"] ? filterModels["orderNr"].filter : "";
        var orderStatuses = [];
        if (filterModels["orderStatusName"]) {
            _.forEach(filterModels["orderStatusName"], (value) => {
                var orderStatus = _.find(this.orderStatuses, { value: value.toString() });
                if (orderStatus)
                    orderStatuses.push(orderStatus.id);

            });
        }
        var sellerOrderNr: string = filterModels["sellerOrderNr"] ? filterModels["sellerOrderNr"].filter : "";
        var ediStatuses = [];
        if (filterModels["statusName"]) {
            _.forEach(filterModels["statusName"], (value) => {
                var ediStatus = _.find(this.ediStatuses, { value: value.toString() });
                if (ediStatus)
                    ediStatuses.push(ediStatus.id);

            });
        }
 
        var sum: number = filterModels["sum"] ? filterModels["sum"].filter : 0;
        var supplierNrName: string = filterModels["supplierNrName"] ? filterModels["supplierNrName"].filter : "";
        this.progress.startLoadingProgress([() => {
            return this.importService.getFilteredEdiEntryViews(this.classification, this.originType, billingTypes, buyerId, dueDate, invoiceDate, orderNr, orderStatuses, sellerOrderNr, ediStatuses, sum, supplierNrName, this.allItemsSelection).then(x => {
                _.forEach(x, (row: EdiEntryViewDTO) => {
                    //Fix dates
                    if(row.date)
                        row.date = new Date(<any>row.date).date();
                    if(row.dueDate)
                        row.dueDate = new Date(<any>row.dueDate).date();
                    if(row.invoiceDate)
                        row.invoiceDate = new Date(<any>row.invoiceDate).date();

                    row.supplierNrName = row.supplierNr + " " + row.supplierName;
                    if (!row.supplierNr)
                        row.supplierNrName = "";

                    row["hasInvalidSupplier"] = _.find(this.suppliers, { id: row.supplierId }) ? false : true;
                });
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private save() {
        var items: UpdateEdiEntryDTO[] = [];
        _.forEach(this.gridAg.options.getSelectedRows(), (e: EdiEntryViewDTO) => {
            var item = new UpdateEdiEntryDTO();
            item.ediEntryId = e.ediEntryId;
            item.supplierId = e.supplierId;
            item.orderNr = e.orderNr;
            items.push(item);
        });

        if (items.length > 0) {
            
            this.progress.startSaveProgress((completion) => {
                this.importService.updateEdiEntries(items).then((result) => {
                    if (result.success) {
                        this.loadGridData();
                        completion.completed();
                    } else {
                        completion.failed(result.errorMessage);
                    }
                });
            }, null);
        }
    }

    private initTransferToOrder() {
        const keys: string[] = [
            "core.warning",
            "core.verifyquestion",
            "billing.import.edi.transferposttoorder",
            "billing.import.edi.transferpoststoorder",
            "billing.import.edi.invalidtransferposttoorder",
            "billing.import.edi.invalidtransferpoststoorder",
            "billing.import.edi.posttransferedtoorder",
            "billing.import.edi.poststransferedtoorder",
            "billing.import.edi.transferposttoorderfailed",
            "billing.import.edi.transferpoststoorderfailed",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var nbrOfValid: number = 0;
            var nbrOfInvalid: number = 0;
            var dict: any[] = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                if (this.isOkTransferToOrder(row)) {
                    dict.push(row.ediEntryId);
                    nbrOfValid = nbrOfValid + 1;
                }
                else {
                    nbrOfInvalid = nbrOfInvalid + 1;
                }
            });

            var title = "";
            var message = "";
            var invalidMessage = "";
            var successMessage = "";
            var errorMessage = "";
            var image = null;
            if (nbrOfInvalid === 0) {
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["billing.import.edi.transferpoststoorder"] : terms["billing.import.edi.transferposttoorder"]);
                successMessage = nbrOfValid > 1 ? terms["billing.import.edi.poststransferedtoorder"] : terms["billing.import.edi.posttransferedtoorder"];
                errorMessage = nbrOfValid > 1 ? terms["billing.import.edi.transferpoststoorderfailed"] : terms["billing.import.edi.transferposttoorderfailed"];
            }
            else {
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["billing.import.edi.transferpoststoorder"] : terms["billing.import.edi.transferposttoorder"]);
                message += "\n" + nbrOfInvalid.toString() + " " + (nbrOfInvalid > 1 ? terms["billing.import.edi.invalidtransferpoststoorder"] : terms["billing.import.edi.invalidtransferposttoorder"]);
                successMessage = nbrOfValid > 1 ? terms["billing.import.edi.poststransferedtoorder"] : terms["billing.import.edi.posttransferedtoorder"];
                errorMessage = nbrOfValid > 1 ? terms["billing.import.edi.transferpoststoorderfailed"] : terms["billing.import.edi.transferposttoorderfailed"];
            }

            const modal = this.notificationService.showDialog(title, message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.transferToOrder(dict, errorMessage);
            });
        });
    }

    private transferToOrder(dict: any[], errorMessage: string) {
        this.callForReload = true;
        this.progress.startSaveProgress((completion) => {
            this.importService.transferEdiToOrders(dict).then((result: IActionResult) => {
                if (result.success) {
                    this.loadGridData();
                    completion.completed();
                } else {
                    if (result.errorMessage)
                        completion.failed(errorMessage + "\n" + result.errorMessage);
                    else
                        completion.failed(errorMessage);
                }
            });
        }, null);
    }

    private initCreateInvoice() {
        const keys: string[] = [
            "core.warning",
            "core.verifyquestion",
            "common.edistransfertoinvoicevalid",
            "common.editransfertoinvoicevalid",
            "common.edistransfertoinvoiceinvalid",
            "common.editransfertoinvoiceinvalid",
            "common.invoiceswascreated",
            "common.invoicewascreated",
            "common.edistransfertoinvoicefailed",
            "common.editransfertoinvoicefailed",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var nbrOfValid: number = 0;
            var nbrOfInvalid: number = 0;
            var dict: any[] = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                if (this.isOkTransferToSupplierInvoice(row)) {
                    dict.push(row.ediEntryId);
                    nbrOfValid = nbrOfValid + 1;
                }
                else {
                    nbrOfInvalid = nbrOfInvalid + 1;
                }
            });

            var title = "";
            var message = "";
            var invalidMessage = "";
            var successMessage = "";
            var errorMessage = "";
            var image = null;
            if (nbrOfInvalid === 0) {
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.edistransfertoinvoicevalid"] : terms["common.editransfertoinvoicevalid"]);
                successMessage = nbrOfValid > 1 ? terms["common.invoiceswascreated"] : terms["common.invoicewascreated"];
                errorMessage = nbrOfValid > 1 ? terms["common.edistransfertoinvoicefailed"] : terms["common.editransfertoinvoicefailed"];
            }
            else {
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.edistransfertoinvoicevalid"] : terms["common.editransfertoinvoicevalid"]);
                message += "\n" + nbrOfInvalid.toString() + " " + (nbrOfInvalid > 1 ? terms["common.edistransfertoinvoiceinvalid"] : terms["common.editransfertoinvoiceinvalid"]);
                successMessage = nbrOfValid > 1 ? terms["common.invoiceswascreated"] : terms["common.invoicewascreated"];
                errorMessage = nbrOfValid > 1 ? terms["common.edistransfertoinvoicefailed"] : terms["common.editransfertoinvoicefailed"];
            }

            const modal = this.notificationService.showDialog(title, message, image, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.createInvoice(dict, errorMessage);
            });
        });
    }

    private createInvoice(dict: any[], errorMessage: string) {
        this.progress.startSaveProgress((completion) => {
            this.importService.transferEdiToInvoices(dict).then((result:IActionResult) => {
                if (result.success) {
                    this.loadGridData();
                    completion.completed();
                } else {
                    if (result.errorNumber && result.errorMessage)
                        completion.failed(errorMessage + "\n" + result.errorMessage);
                    else
                        completion.failed(errorMessage);
                }
            });
        }, null);
    }

    private createPdf() {// Columns
        const keys: string[] = [
            "core.warning",
            "core.verifyquestion",
            "common.createpdfsvalid",
            "common.createpdfvalid",
            "common.postsinvalid",
            "common.postinvalid",
            "common.pdfscreated",
            "common.pdfcreated",
            "common.pdfserror",
            "common.pdferror",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var nbrOfValid: number = 0;
            var nbrOfInvalid: number = 0;
            var dict: any[] = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                if (this.okToGeneratePdf(row)) {
                    dict.push(row.ediEntryId);
                    nbrOfValid = nbrOfValid + 1;
                }
                else {
                    nbrOfInvalid = nbrOfInvalid + 1;
                }
            });

            var title = "";
            var message = "";
            var invalidMessage = "";
            var successMessage = "";
            var errorMessage = "";
            var image = null;
            if (nbrOfInvalid === 0) {
                title = terms["core.verifyquestion"];
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.createpdfsvalid"] : terms["common.createpdfvalid"]);
                successMessage = nbrOfValid > 1 ? terms["common.pdfscreated"] : terms["common.pdfcreated"];
                errorMessage = nbrOfValid > 1 ? terms["common.pdfserror"] : terms["common.pdferror"];
                image = SOEMessageBoxImage.Question;
            }
            else {
                title = terms["core.warning"];
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.createpdfsvalid"] : terms["common.createpdfvalid"]);
                message += "\n" + nbrOfInvalid.toString() + " " + (nbrOfInvalid ? terms["common.postsinvalid"] : terms["common.postinvalid"]);
                successMessage = nbrOfValid > 1 ? terms["common.pdfscreated"] : terms["common.pdfcreated"];
                errorMessage = nbrOfValid > 1 ? terms["common.pdfserror"] : terms["common.pdferror"];
                image = SOEMessageBoxImage.Warning;
            }

            var modal = this.notificationService.showDialog(title, message, image, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.generatePdfs(dict, errorMessage);
            });
        });
    }

    private generatePdfs(dict: any[], errorMessage: string){
        this.progress.startSaveProgress((completion) => {
            this.importService.generateReportForEdi(dict).then((result) => {
                if (result.success) {
                    this.loadGridData();
                    completion.completed();
                } else {
                    completion.failed(errorMessage);
                }
            });
        }, null);
    }
    
    private initChangeEdiState(stateTo: any) {
        var keys: string[] = [
            "core.warning",
            "core.verifyquestion",
            "common.ediclosevalid",
            "common.edisclosevalid",            
            "common.ediclosefailed",
            "common.edisclosefailed",
            "common.edideletevalid",
            "common.edisdeletevalid",            
            "common.edideletefailed",
            "common.edisdeletefailed",
            "common.edisopenvalid",
            "common.ediopenvalid",
            "common.edisopenfailed",
            "common.ediopenfailed"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            var nbrOfValid: number = 0;
            
            var dict: any[] = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                dict.push(row.ediEntryId);
                nbrOfValid = nbrOfValid + 1;                
            });

            var title = "";
            var message = "";            
            var errorMessage = "";

            if (stateTo.id === SoeEntityState.Active) {
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.edisopenvalid"] : terms["common.ediopenvalid"]);
                errorMessage = nbrOfValid > 1 ? terms["common.edisopenfailed"] : terms["common.ediopenfailed"];
            }
            else
            if (stateTo.id === SoeEntityState.Inactive) {
                
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.edisclosevalid"] : terms["common.ediclosevalid"]);
                errorMessage = nbrOfValid > 1 ? terms["common.edisclosefailed"] : terms["common.ediclosefailed"];
                
            }
            else {
                
                message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.edisdeletevalid"] : terms["common.edideletevalid"]);                
                errorMessage = nbrOfValid > 1 ? terms["common.edisdeletefailed"] : terms["common.common.edideletefailed"];
                
            }

            var modal = this.notificationService.showDialog(title, message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.changeEdiState(dict, stateTo.id, errorMessage);
            });
        });
    }

    private changeEdiState(dict: any[], stateTo: SoeEntityState, errorMessage: string) {
        this.progress.startSaveProgress((completion) => {
            this.importService.changeEdiState(dict, stateTo).then((result) => {
                if (result.success) {
                    this.loadGridData();
                    completion.completed();
                } else {
                    completion.failed(errorMessage);
                }
            });
        }, null);
    }

    private addEdiEntries() {
        this.progress.startSaveProgress((completion) => {
            this.importService.addEdiEntrys(TermGroup_EDISourceType.EDI).then((result) => {
                if (result.success) {
                    if (result.keys && result.keys.length > 0)
                        this.generatePdfs(result.keys, "");
                    this.loadGridData();
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            });
        }, null);
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, this.allItemsSelectionSettingType, this.allItemsSelection)
        this.loadGridData();
    }

    private reloadData() {
        if (this.classification === SoeEntityState.Active)
            this.loadGridData();
        else
            this.search();
    }

    public closeModal() {
        if (this.isModal && this.modal) {
            this.modal.dismiss();
        }
    }
}