import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IImportService } from "../../../Shared/Billing/Import/ImportService";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { IGridHandler } from "../../../Core/Handlers/GridHandler"
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, CompanySettingType, SoeEntityType, SoeEntityState, SoeReportTemplateType, TermGroup, TermGroup_EDIOrderStatus, TermGroup_EDIStatus, TermGroup_EdiMessageType, TermGroup_EDIInvoiceStatus, UserSettingType, SettingMainType, EdiImportSource } from "../../../Util/CommonEnumerations";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage, IconLibrary, FinvoiceGridFunctions } from "../../../Util/Enumerations";
import { Constants } from "../../../Util/Constants";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { EditController as SupplierInvoiceEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { IActionResult, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TypeAheadOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { EdiEntryViewDTO, UpdateEdiEntryDTO } from "../../../Common/Models/InvoiceDTO";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class GridController 
    extends GridControllerBase2Ag 
    implements ICompositionGridController {

    //Terms
    terms: { [index: string]: string; };

    // Company settings
    private ediReportTemplateId: number;
    private useOrder: boolean;

    public files: any[] = [];
    private suppliers: ISmallGenericType[];
    customerInvoices: any[] = [];
    filteredCustomerInvoices: any[] = [];    

    gridFooterComponentUrl: any;
    private isOkToSave = false;
    private isOkToGeneratePdf = false;
    private isOkToTransferToSupplierInvoice = false;
    private isOkToTransferToOrder = false;

    //Collections
    private ediStatuses: any[] = [];
    private invoiceStatuses: any[] = [];
    private orderStatuses: any[] = [];
    private billingTypes: any[] = [];
    protected allItemsSelectionDict: any[];

    // Filtering
    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        this.reloadGridFromFilter();
    }

    private _showOnlyUnHandled = false;
    get showOnlyUnHandled() {
        return this._showOnlyUnHandled;
    }
    set showOnlyUnHandled(item: any) {
        if (item !== this._showOnlyUnHandled) {
            this._showOnlyUnHandled = item;
            this.updateUnhandledSelection();
        }
    }

    // Functions
    buttonFunctions: any = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private importService: IImportService,        
        private supplierService: ISupplierService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Economy.Import.Finvoice", progressHandlerFactory, messagingHandlerFactory);

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onAllPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))            
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
    }

    public onInit(parameters: any) {

        this.guid = parameters.guid;

        this.flowHandler.start([{ feature: Feature.Economy_Import_Invoices_Finvoice, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Import_Invoices_Finvoice].readPermission;
        this.modifyPermission = response[Feature.Economy_Import_Invoices_Finvoice].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));

        //Setup toolbar
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.import.finvoice.selectfiles", "economy.import.finvoice.selectfiles", IconLibrary.FontAwesome, "fa-download",
            () => { this.uploadFiles(); }
        )));

        //Setup toolbar
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.import.finvoice.selectattachments", "economy.import.finvoice.selectattachments", IconLibrary.FontAwesome, "fa-download",
            () => { this.uploadAttachments(); }
        )));
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadTerms(),
            this.loadSuppliers(),
            this.loadEdiStatuses(),
            this.loadEdiInvoicesStatuses(),
            this.loadEdiOrderStatuses(),
            this.loadBillingTypes(),
            this.loadSuppliers(),
            this.loadCustomerInvoices(),
            this.loadSelectionTypes(),
            this.loadUserSettings()
        ]).then(() => {
            this.buttonFunctions.push({ id: FinvoiceGridFunctions.Save, name: this.terms["core.save"] });
            this.buttonFunctions.push({ id: FinvoiceGridFunctions.CreateSupplierInvoice, name: this.terms["billing.import.edi.createinvoice"] });
            if (this.useOrder)
                this.buttonFunctions.push({ id: FinvoiceGridFunctions.TransferToOrder, name: this.terms["billing.import.edi.transferorderrows"] });
            this.buttonFunctions.push({ id: FinvoiceGridFunctions.Delete, name: this.terms["core.delete"] });
        });
    }

    private loadEdiStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDIStatus, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.ediStatuses.push({ "id": y.id, "value": y.name });
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

    private loadEdiOrderStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EDIOrderStatus, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.orderStatuses.push({ "id": y.id, "value": y.name });
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

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.GridDateSelectionType, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
            this._allItemsSelection = 1;
        });
    }     

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.SupplierInvoiceFinvoiceUnhandledSelection];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this._showOnlyUnHandled = SettingsUtility.getBoolUserSetting(x, UserSettingType.SupplierInvoiceFinvoiceUnhandledSelection);
        });
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.importService.getSuppliersDict(true, true, true).then((x: ISmallGenericType[]) => {
            this.suppliers = x;
        });
    }

    private loadCustomerInvoices(): ng.IPromise<any> {
        return this.supplierService.getOrdersForSupplierInvoiceEdit(true).then((x) => {
            this.customerInvoices = x;
        });
    }

    public updateUnhandledSelection() {
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.SupplierInvoiceFinvoiceUnhandledSelection, this.showOnlyUnHandled)
        this.reloadGridFromFilter();
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 500, { leading: false, trailing: true });

    public loadGridData() {
        this.progress.startLoadingProgress([
            () => this.load()
        ]);
    }

    private load(): ng.IPromise<any> {
        return this.importService.getFinvoiceEntryViews(SoeEntityState.Active, this.allItemsSelection, this.showOnlyUnHandled).then(x => {
            _.forEach(x, (row: any) => {
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

                if (row.orderNr) {
                    const customerInvoice = _.find(this.customerInvoices, { invoiceNr: row.orderNr });
                    if (customerInvoice)
                        row.customerInvoiceNumberName =customerInvoice.customerInvoiceNumberName;
                }

                row["hasInvalidSupplier"] = _.find(this.suppliers, { id: row.supplierId }) ? false : true;

                if (!row.invoiceNr)
                    row.errorMessage = row.errorMessage != null ? row.errorMessage + ", " + this.terms["billing.import.edi.invoicenrmissing"] : this.terms["billing.import.edi.invoicenrmissing"];

                if (!row.supplierId)
                    row.errorMessage = row.errorMessage != null ? row.errorMessage + ", " + this.terms["billing.import.edi.suppliermissing"] : this.terms["billing.import.edi.suppliermissing"];

                if (row.seqNr)
                    row.editIcon = "fal fa-pencil iconEdit";

                if (row.importSource === EdiImportSource.BankIntegration) {
                    row.sourceIcon = "fal fa-cloud-download okColor";
                    row.sourceTooltip = this.terms["economy.import.finvoice.bankinttooltip"];
                }
            });

            this.setData(x);
        });

    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.save",
            "billing.import.edi.createinvoice",
            "billing.import.edi.transferorderrows",
            "core.fileupload.choosefiletoimport",
            "common.connect.fileuploadnotsuccess",
            "core.open",
            "core.close",
            "core.delete",
            "core.error",
            "common.status",
            "core.verifyquestion",
            "economy.import.finvoice.importdate",
            "economy.supplier.invoice.billingtype",
            "billing.import.edi.downloadstatus",
            "billing.import.edi.invoicestatus",
            "billing.import.edi.orderstatus",
            "billing.import.edi.type",
            "billing.import.edi.invoicenr",
            "economy.supplier.invoice.seqnr",
            "common.customer.invoices.invoicedate",
            "common.customer.invoices.duedate",
            "billing.import.edi.supplier",
            "common.amount",
            "common.errormessage",
            "economy.import.finvoice.showfinvoice",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "billing.import.edi.invoicenrmissing",
            "billing.import.edi.suppliermissing",
            "economy.import.finvoice.importsuccess",
            "economy.import.finvoice.importnotsuccess",
            "economy.import.finvoice.fileuploadnotsuccess",
            "billing.import.edi.ordernr",
            "economy.import.finvoice.createsupplierquestion",
            "economy.import.finvoice.createsuppliertooltip",
            "billing.import.edi.seqnr",
            "common.supplierinvoice",
            "economy.import.finvoice.bankinttooltip"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setupGrid() {

        this.gridAg.addColumnIsModified("isModified", "", null);
        this.gridAg.addColumnDate("date", this.terms["economy.import.finvoice.importdate"], null);
        this.gridAg.addColumnSelect("statusName", this.terms["billing.import.edi.downloadstatus"], null, { displayField: "statusName", selectOptions: this.ediStatuses });
        this.gridAg.addColumnSelect("invoiceStatusName", this.terms["billing.import.edi.invoicestatus"], null, { displayField: "invoiceStatusName", selectOptions: this.invoiceStatuses });
        if (this.useOrder) 
            this.gridAg.addColumnSelect("orderStatusName", this.terms["billing.import.edi.orderstatus"], null, { displayField: "orderStatusName", selectOptions: this.invoiceStatuses, enableHiding: true });
        this.gridAg.addColumnSelect("billingTypeName", this.terms["billing.import.edi.type"], null, { displayField: "billingTypeName", selectOptions: this.billingTypes });        
        this.gridAg.addColumnText("seqNr", this.terms["billing.import.edi.seqnr"], null, false);        
        this.gridAg.addColumnText("invoiceNr", this.terms["billing.import.edi.invoicenr"], null, false);
        this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-pencil iconEdit", onClick: this.handleEditSource.bind(this), showIcon: this.showEditSource.bind(this) });            
        this.gridAg.addColumnDate("invoiceDate", this.terms["common.customer.invoices.invoicedate"], null);
        this.gridAg.addColumnDate("dueDate", this.terms["common.customer.invoices.duedate"], null);        
        
        const optionsSupplier = new TypeAheadOptionsAg();
        optionsSupplier.source = (filter) => this.filterSuppliers(filter);
        optionsSupplier.displayField = "name"
        optionsSupplier.dataField = "name";
        optionsSupplier.allowNavigationFromTypeAhead = () => { return false };

        this.gridAg.addColumnTypeAhead("supplierNrName", this.terms["billing.import.edi.supplier"], null, { typeAheadOptions: optionsSupplier, editable: true });

        this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-plus", onClick: this.createNewSupplier.bind(this), showIcon: this.showAddSupplier.bind(this), toolTip: this.terms["economy.import.finvoice.createsuppliertooltip"] });            

        this.gridAg.addColumnNumber("sum", this.terms["common.amount"], null, { decimals: 2 });

        if (this.useOrder) {
            const optionsOrder = new TypeAheadOptionsAg();
            optionsOrder.source = (filter) => this.filterCustomerInvoices(filter);
            optionsOrder.displayField = "customerInvoiceNumberName";
            optionsOrder.dataField = "customerInvoiceNumberName";
            optionsOrder.allowNavigationFromTypeAhead = () => { return false };

            this.gridAg.addColumnTypeAhead("customerInvoiceNumberName", this.terms["billing.import.edi.ordernr"], null, { typeAheadOptions: optionsOrder, editable: true });
        }
        this.gridAg.addColumnText("errorMessage", this.terms["common.errormessage"], null, false, { toolTipField: "errorMessage" });
        this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-file-pdf", onClick: this.showPdf.bind(this), showIcon: this.showPdfIcon.bind(this) });
        this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-file-alt", onClick: this.showFinvoice.bind(this), toolTip: this.terms["economy.import.finvoice.showfinvoice"] });
        this.gridAg.addColumnIcon("sourceIcon", null, null, { toolTipField: "sourceTooltip" });

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.beginCellEdit(entity, colDef); }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("economy.import.finvoice", true);
    }

    private beginCellEdit(row: EdiEntryViewDTO, colDef: uiGrid.IColumnDef) {
        if (colDef.field == 'customerInvoiceNumberName' && row.orderStatus == TermGroup_EDIOrderStatus.Processed) {
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
                }
                else {
                    row.supplierNrName = oldValue;
                }
                break;
            case 'customerInvoiceNumberName':
                const invoice = _.find(this.customerInvoices, { customerInvoiceNumberName: newValue });
                if (invoice) {
                    row.orderId = invoice.invoiceId;
                    row.orderNr = invoice.invoiceNr;                   
                }
                else {
                    row.orderNr = oldValue;
                }
                break;
        }

        row.isModified = true;
        this.gridAg.options.refreshRows(row);
        this.gridAg.options.selectRow(row);
    }

    private showEditSource(row) {
        if (row.invoiceId && row.invoiceId != null)
            return true;

        return false;
    }

    private handleEditSource(row) {
        if (row.invoiceId && row.invoiceId != null)                    
            this.showSourceSupplierInvoice(row);        
    }

    private showAddSupplier(row) {
        if (row.supplierId && row.supplierId != null)
            return false;

        return true;
    }    

    private filterSuppliers(filter) {
        return this.suppliers.filter(supplier => {
            return supplier.name.contains(filter);
        });
    }

    private filterCustomerInvoices(filter) {
        return this.customerInvoices.filter(invoice => {
            return invoice.customerInvoiceNumberName.contains(filter);
        });
    }

    private showSourceSupplierInvoice(row) {
        let prefixNr: string = row.invoiceNr;
        if (row.seqNr)
            prefixNr = row.seqNr;

        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.supplierinvoice"] + " " + prefixNr, row.invoiceId, SupplierInvoiceEditController, { id: row.invoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
    }

    private showFinvoice(row) {
        let uri = window.location.protocol + "//" + window.location.host;
        uri = uri + "/soe/common/xslt/" + "?templatetype=" + SoeReportTemplateType.FinvoiceEdiSupplierInvoice + "&id=" + row.ediEntryId + "&c=" + CoreUtility.actorCompanyId;
        window.open(uri, '_blank');
    }

    private showPdf(row: any) {
        const ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + row.ediEntryId;
        window.open(ediPdfReportUrl, '_blank');
    }

    private showPdfIcon(item: any) {
        return item.hasPdf;
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.FinvoiceUseTransferToOrder];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useOrder = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.FinvoiceUseTransferToOrder);
        });
    }

    private uploadAttachments() {
        const url = CoreUtility.apiPrefix + Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORT_FINVOICE_ATTACHMENTS; 
        const modal = this.notificationService.showFileUpload(url, this.terms["core.fileupload.choosefiletoimport"], true, true, true, true);
        
        modal.result.then(res => {

            modal.result.then(res => {
                _.forEach(res.result, result => {
                    if (result.success) {
                        this.notificationService.showDialog(this.terms["common.status"], result.infoMessage, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    } else {
                        this.notificationService.showDialog(this.terms["common.status"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                });
            });
        }, error => {

        });
    }

    private uploadFiles() {
        this.files = [];
        const url = CoreUtility.apiPrefix + Constants.WEBAPI_CORE_FILES_UPLOAD_INVOICE + SoeEntityType.None;
        const modal = this.notificationService.showFileUpload(url, this.terms["core.fileupload.choosefiletoimport"], true, true, true, true);
        modal.result.then(res => {

            let filesNotUploaded: string = "";

            _.forEach(res.result, result => {
                if (result.success)
                    this.files.push(result);
                else
                    filesNotUploaded += filesNotUploaded.length > 0 ? ", " + result.value : result.value;
            });

            this.importFiles();

            if (filesNotUploaded.length > 0)
                this.notificationService.showDialog(this.terms["core.error"], this.terms["economy.import.finvoice.fileuploadnotsuccess"] + "\n" + filesNotUploaded, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);            
                
        }, error => {

        });
    }

    private importFiles() {
        this.progress.startWorkProgress((completion) => {
            const dataStorageIds: number[] = [];

            _.forEach(this.files, (file) => {
                if (file.success) {
                    dataStorageIds.push(Number(file.integerValue2));
                }
            });

            this.importService.importFinvoiceItems(dataStorageIds).then((result) => {
                if (result.success) {
                    this.load();
                    completion.completed(null, false, this.terms["economy.import.finvoice.importsuccess"].format(result.integerValue.toString(), result.integerValue2.toString()));
                }
                else {
                    completion.failed(this.terms["economy.import.finvoice.importnotsuccess"]);
                }
            });
        });
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case FinvoiceGridFunctions.Save:
                this.save();
                break;
            case FinvoiceGridFunctions.CreatePdf:
                this.createEdiPDFs();
                break;
            case FinvoiceGridFunctions.CreateSupplierInvoice:
                this.initCreateInvoice();
                break;
            case FinvoiceGridFunctions.TransferToOrder:
                this.updateEdiEntryAndTransferToOrder();
                break;
            case FinvoiceGridFunctions.Delete:
                this.deleteFinvoice();
                break;
        }
    }

    private createEdiPDFs() {

        const dict: any[] = this.gridAg.options.getSelectedIds("ediEntryId")

        this.progress.startSaveProgress((completion) => {
            this.coreService.generateReportForFinvoice(dict).then((result) => {
                if (result.success) {
                    completion.completed();
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
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                if (this.isOkToSaveRow(row))
                    this.isOkToSave = true;
                if (this.isOkTransferToSupplierInvoice(row))
                    this.isOkToTransferToSupplierInvoice = true;
                if (this.isOkTransferToOrder(row))
                    this.isOkToTransferToOrder = true;                
            });
        });
    }

    private isOkToSaveRow(item: EdiEntryViewDTO) {
        return item.isModified;
    }

    private isOkTransferToSupplierInvoice(item: EdiEntryViewDTO) {
        return (item.invoiceNr && item.invoiceNr.length > 0) &&
            !item.invoiceId &&
            item.supplierId && item.supplierId > 0 &&            
            item.ediMessageType == TermGroup_EdiMessageType.SupplierInvoice &&
            item.invoiceStatus == TermGroup_EDIInvoiceStatus.Unprocessed &&
            (item.status == TermGroup_EDIStatus.UnderProcessing || item.status == TermGroup_EDIStatus.Processed);
    }

    private isOkTransferToOrder(item: EdiEntryViewDTO) {
        return ((item.orderNr && item.orderNr.length > 0) &&
            (item.orderStatus == TermGroup_EDIOrderStatus.Unprocessed) &&
            (item.status == TermGroup_EDIStatus.Processed || item.status == TermGroup_EDIStatus.UnderProcessing));
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

    private updateEdiEntryAndTransferToOrder() {
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
                        this.initTransferToOrder();
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
        this.progress.startSaveProgress((completion) => {
            this.importService.transferEdiToOrders(dict).then((result) => {
                if (result.success) {
                    this.loadGridData();
                    completion.completed();
                } else {
                    if (result.errorNr && result.errorNr > 0 && result.errorMessage)
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
            this.importService.transferEdiToInvoices(dict).then((result) => {
                if (result.success) {
                    this.loadGridData();
                    completion.completed();
                } else {
                    if (result.errorNr && result.errorNr > 0 && result.errorMessage)
                        completion.failed(errorMessage + "\n" + result.errorMessage);
                    else
                        completion.failed(errorMessage);
                }
            });
        }, null);
    }

    private deleteFinvoice() {
        const keys: string[] = [
            "core.warning",
            "core.verifyquestion",
            "common.edideletevalid",
            "common.edisdeletevalid",
            "common.edideletefailed",
            "common.edisdeletefailed",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var nbrOfValid: number = 0;

            var dict: any[] = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row: EdiEntryViewDTO) => {
                if (row.invoiceStatus == TermGroup_EDIInvoiceStatus.Unprocessed || row.invoiceStatus == TermGroup_EDIInvoiceStatus.Error) {
                    dict.push(row.ediEntryId);
                    nbrOfValid = nbrOfValid + 1;
                }
            });

            if (nbrOfValid === 0)
                return;

            var title = "";
            var message = "";
            var errorMessage = "";

            message = nbrOfValid.toString() + " " + (nbrOfValid > 1 ? terms["common.edisdeletevalid"] : terms["common.edideletevalid"]);
            errorMessage = nbrOfValid > 1 ? terms["common.edisdeletefailed"] : terms["common.common.edideletefailed"];

            var modal = this.notificationService.showDialog(title, message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.progress.startSaveProgress((completion) => {
                    this.importService.changeEdiState(dict, SoeEntityState.Deleted).then((result) => {
                        if (result.success) {
                            this.loadGridData();
                            completion.completed();
                        } else {
                            completion.failed(errorMessage);
                        }
                    });
                }, null);
            });
        });
    }

    private createNewSupplier(row) {
        const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], this.terms["economy.import.finvoice.createsupplierquestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
                this.supplierService.saveSupplierFromFinvoice(row.ediEntryId).then((result) => {
                    if (result.success) {
                        this.loadGridData();
                    }
                    if (result.errorMessage) {
                        this.notificationService.showDialog(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                });
            }
        });

    }    
}