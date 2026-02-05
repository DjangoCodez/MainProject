import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { EditController as SupplierInvoiceEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as CustomerInvoiceEditController } from "../../../Shared/Billing/Invoices/EditController";
import { SoeAccountDistributionType, Feature, SoeEntityState, TermGroup_AccountDistributionRegistrationType, TermGroup_AccountDistributionTriggerType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { IActionResult } from "../../../Scripts/TypeLite.Net4";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { EditNoteController } from "./EditNoteController";
import { AccountDistributionEntryDTO } from "../../../Common/Models/AccountDistributionEntryDTO";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private distributionEntries: any[];
    private subGridRows: any;
    private toolbarInclude: any;
    private currentMonth: Date;
    private supplierInvoiceEditPrefixTerm: string;
    private customerInvoiceEditPrefixTerm: string;
    private voucherEditPrefixTerm: string;
    private gridHeaderComponentUrl: any;
    private btnLabel: string;
    private terms: { [index: string]: string; };
    private accountDim1Name: string = "";
    private accountDim2Name: string = "";
    private accountDim3Name: string = "";
    private accountDim4Name: string = "";
    private accountDim5Name: string = "";
    private accountDim6Name: string = "";

    // Flags
    private buttonDisabled = true;

    //config
    private distributionType: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Economy.Accounting.AccountDistributionEntry", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('accountDistributionHeadId', 'name');
        this.gridHeaderComponentUrl = urlHelperService.getViewUrl("filterHeader.html");

        this.distributionType = soeConfig.accountDistributionType;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onLoadSettings(() => this.doLoadSettings())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupToolbar())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData());


        //Set the current month as default at startup
        this.currentMonth = new Date().beginningOfMonth();

        //Check disabled selection
        //this.grid.options.rowSelectDisabledProperty = "isSelectEnable";
        //this.grid.options.rowSelectDisabledPropertyInvert = true;        

        this.subGridRows = {
            dim1Name: "",
            debet: 0,
            credit: 0,
        }
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.distributionType == SoeAccountDistributionType.Period)
            this.flowHandler.start({ feature: Feature.Economy_Accounting_AccountDistributionEntry, loadReadPermissions: true, loadModifyPermissions: true });
        else
            this.flowHandler.start({ feature: Feature.Economy_Inventory_WriteOffs, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public doLoadSettings(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.getDimLabels()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.rownumber",
            "common.name",
            'economy.inventory.writeoffs.date',
            "common.type",
            "common.description",
            "economy.accounting.accountdistributionentry.status",
            "economy.inventory.writeoffs.periodamount",
            "economy.accounting.voucher.voucher",
            "economy.accounting.accountdistributionentry.accounting",
            "economy.accounting.accountdistributionentry.debet",
            "economy.accounting.accountdistributionentry.credit",
            "economy.accounting.vatverification.vouchernumber",
            "economy.inventory.writeoffs.inventoryname",
            "economy.inventory.writeoffs.writeoffamount",
            "economy.inventory.writeoffs.writeoffyearamount",
            "economy.inventory.writeoffs.writeofftotalamount",
            "economy.inventory.inventories.writeoffsum",
            "economy.inventory.writeoffs.currentamount",
            "economy.accounting.accountdistributionentry.sourcetype",
            "economy.accounting.accountdistributionentry.source",
            "common.categories",
            "common.supplierinvoice",
            "common.customerinvoice",
            "core.warning",
            "core.info",
            "economy.accounting.accountdistributionentry.deletepermantentlywarning",
            "economy.accounting.accountdistributionentry.noentriesselected",
            "economy.supplier.invoice.preliminary",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "economy.accounting.accountdistributionentry.nowrowstodelete",
            "economy.accounting.accountdistributionentry.nowrowstotransfer",
            "economy.inventory.inventories.writeoff",
            "economy.accounting.accountdistributionentry.accountyearmissingmessage",
            "economy.accounting.accountdistributionentry.accountperiodclosed",
            "economy.inventory.writeoffs.perioderror",
            "economy.inventory.writeoffs.perioderrorinfosingle",
            "economy.inventory.writeoffs.perioderrorinfomulti",
            "economy.inventory.inventories.writeoffdate",
            "economy.inventory.inventories.purchasedate",
            "economy.accounting.accountdistributionentry.getdetails.information",
            "economy.accounting.inventory.writeoffs.getdetails.information",
            "economy.inventory.writeoffs.noentriesselected",
            "economy.inventory.writeoffs.nowrowstodelete",
            "economy.accounting.accountdistributionentry.periodamount",
            "economy.inventory.writeoffs.deletewarning"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.voucherEditPrefixTerm = this.terms["economy.accounting.voucher.voucher"];
            this.customerInvoiceEditPrefixTerm = this.terms["common.customerinvoice"];
            this.supplierInvoiceEditPrefixTerm = this.terms["common.supplierinvoice"];
        });
    }

    private getDimLabels(): ng.IPromise<any> {
        return this.accountingService.getAccountDims(false, false, false, false, false).then((x: any[]) => {
            const dim1 = _.find(x, { accountDimNr: 1 });
            if (dim1) { this.accountDim1Name = dim1.name }

            const dim2 = _.find(x, { accountDimNr: 2 });
            if (dim2) { this.accountDim2Name = dim2.name }

            const dim3 = _.find(x, { accountDimNr: 3 });
            if (dim3) { this.accountDim3Name = dim3.name }

            const dim4 = _.find(x, { accountDimNr: 4 });
            if (dim4) { this.accountDim4Name = dim4.name }

            const dim5 = _.find(x, { accountDimNr: 5 });
            if (dim5) { this.accountDim5Name = dim5.name }

            const dim6 = _.find(x, { accountDimNr: 6 });
            if (dim6) { this.accountDim6Name = dim6.name }
        });
    }

    public loadGridData() {

        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccountDistributionEntries(this.currentMonth, this.distributionType, false, true).then((x: any[]) => {
                this.distributionEntries = x;
                this.distributionEntries.forEach((item) => {
                    if (item.inventoryNr) {
                        item.inventoryName = item.inventoryNr + ' - ' + item.inventoryName
                    }

                    if (item.state === SoeEntityState.Deleted)
                        item.editIcon = "fal fa-undo";
                    if (item.voucherHeadId && item.voucherHeadId != null && item.state === SoeEntityState.Active) {
                        item.editIcon = "fal fa-pencil iconEdit";
                    }
                    if (item.inventoryNotes != undefined &&
                        item.inventoryNotes != null &&
                        item.inventoryNotes.length > 0
                    )
                        item.notesIcon = "fal fa-file-alt";
                    else
                        item.notesIcon = "fal fa-file";
                    if (item.registrationType == TermGroup_AccountDistributionRegistrationType.CustomerInvoice) {
                        if (item.sourceCustomerInvoiceSeqNr)
                            item.sourceSeqNr = item.sourceCustomerInvoiceSeqNr;
                        else
                            item.sourceSeqNr = this.terms["economy.supplier.invoice.preliminary"];
                    }
                    else if (item.registrationType == TermGroup_AccountDistributionRegistrationType.SupplierInvoice) {
                        if (item.sourceSupplierInvoiceSeqNr)
                            item.sourceSeqNr = item.sourceSupplierInvoiceSeqNr;
                        else
                            item.sourceSeqNr = this.terms["economy.supplier.invoice.preliminary"];
                    }
                    else if (item.registrationType == TermGroup_AccountDistributionRegistrationType.Voucher)
                        item.sourceSeqNr = item.sourceVoucherNr;
                    else if (item.supplierInvoiceId != null) {
                        if (item.sourceSupplierInvoiceSeqNr)
                            item.sourceSeqNr = item.sourceSupplierInvoiceSeqNr;
                        else
                            item.sourceSeqNr = this.terms["economy.supplier.invoice.preliminary"];
                    }

                    if (item.date) {
                        item.date = new Date(<any>item.date).date()
                    }

                    item.editSourceIcon = "fal fa-pencil iconEdit";

                    item.expander = "";

                });

                this.setData(this.distributionEntries);
            });

        }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
    }

    protected setupToolbar() {
        if (this.toolbar) {
            if (this.distributionType == SoeAccountDistributionType.Period) {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.delete", "core.delete", IconLibrary.FontAwesome, "fa-times", () => { this.deleteSelectedEntrys(); })));
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.accounting.accountdistributionentry.accrual", "economy.accounting.accountdistributionentry.accrual", IconLibrary.FontAwesome, "fa-calendar", () => { this.initTransferSelectedItemsToVoucher(); }, () => this.buttonDisabled)));
            }
            else {
                this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.inventory.writeoffs.transfertovoucher", "economy.inventory.writeoffs.transfertovoucher", IconLibrary.FontAwesome, "fa-calendar", () => { this.initTransferSelectedItemsToVoucher(); }, () => this.buttonDisabled)));
            }

            const groupLoadDetails = ToolBarUtility.createGroup(
                new ToolBarButton("economy.accounting.accountdistributionentry.getdetails", "economy.accounting.accountdistributionentry.getdetails", IconLibrary.FontAwesome, "fa-upload", () => { this.getDetails() })
            );

            groupLoadDetails.buttons.push(
                new ToolBarButton("", "core.showinfo", IconLibrary.FontAwesome, "fal fa-info-square iconEdit", () => {
                    this.notificationService.showDialog(
                        this.terms["core.info"],
                        (this.distributionType == SoeAccountDistributionType.Inventory_WriteOff) ? this.terms["economy.accounting.inventory.writeoffs.getdetails.information"] : this.terms["economy.accounting.accountdistributionentry.getdetails.information"],
                        SOEMessageBoxImage.Information,
                        SOEMessageBoxButtons.OK
                    )
                })
            );
            this.toolbar.addButtonGroup(groupLoadDetails);

            if (this.distributionType == SoeAccountDistributionType.Inventory_WriteOff) {
                const writeOffDeleteButtonGroup = ToolBarUtility.createGroup(
                    new ToolBarButton("core.delete", "core.delete", IconLibrary.FontAwesome, "fa-times", () => { this.deleteSelectedEntrys(); }, () => { return this.gridAg.options.getSelectedCount() === 0; })
                );
                this.toolbar.addButtonGroup(writeOffDeleteButtonGroup);
            }
        }
    }

    private setUpGrid() {

        this.gridAg.options.clearColumnDefs();

        this.gridAg.options.setName("Economy.Accounting.AccountDistributionEntry" + "_" + this.distributionType);

        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadEntryRows(params);
        });

        this.gridAg.detailOptions.addColumnText("dim1Name", this.accountDim1Name, null);
        if (this.accountDim2Name)
            this.gridAg.detailOptions.addColumnText("dim2Name", this.accountDim2Name, null);
        if (this.accountDim3Name)
            this.gridAg.detailOptions.addColumnText("dim3Name", this.accountDim3Name, null);
        if (this.accountDim4Name)
            this.gridAg.detailOptions.addColumnText("dim4Name", this.accountDim4Name, null);
        if (this.accountDim5Name)
            this.gridAg.detailOptions.addColumnText("dim5Name", this.accountDim5Name, null);
        if (this.accountDim6Name)
            this.gridAg.detailOptions.addColumnText("dim6Name", this.accountDim6Name, null);
        this.gridAg.detailOptions.addColumnNumber("debet", this.terms["economy.accounting.accountdistributionentry.debet"], null, { decimals: 2 });
        this.gridAg.detailOptions.addColumnNumber("credit", this.terms["economy.accounting.accountdistributionentry.credit"], null, { decimals: 2 });

        this.gridAg.detailOptions.finalizeInitGrid();

        // Master
        if (this.distributionType == SoeAccountDistributionType.Period) {
            this.gridAg.addColumnNumber("rowId", this.terms["common.rownumber"], null);
            this.gridAg.addColumnText("accountDistributionHeadName", this.terms["common.name"], null, true, { buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: () => true, callback: this.edit.bind(this) } });
            this.gridAg.addColumnDate("date", this.terms["economy.inventory.writeoffs.date"], null);
            this.gridAg.addColumnText("typeName", this.terms["economy.accounting.accountdistributionentry.sourcetype"], null);
            this.gridAg.addColumnText("sourceSeqNr", this.terms["economy.accounting.accountdistributionentry.source"], null);
            this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-pencil iconEdit", onClick: this.handleEditSource.bind(this), showIcon: this.showEditSource.bind(this) });
            this.gridAg.addColumnText("status", this.terms["economy.accounting.accountdistributionentry.status"], null);
            this.gridAg.addColumnNumber("amount", this.terms["economy.accounting.accountdistributionentry.periodamount"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("voucherNr", this.terms["economy.accounting.voucher.voucher"], null);
            this.gridAg.addColumnIcon(null, this.terms["economy.accounting.accountdistributionentry.restore"], null, { icon: "fal fa-pencil iconEdit", onClick: this.handleEdit.bind(this), showIcon: this.showEdit.bind(this) });
            this.gridAg.setExporterFilenamesAndHeader("economy.accounting.accountdistributionentry.entries");
        }
        else {
            this.gridAg.addColumnNumber("rowId", this.terms["common.rownumber"], null);
            this.gridAg.addColumnDate("inventoryPurchaseDate", this.terms["economy.inventory.inventories.purchasedate"], null, true, null, { hide: true });
            this.gridAg.addColumnDate("inventoryWriteOffDate", this.terms["economy.inventory.inventories.writeoffdate"], null, true, null, { hide: true });
            this.gridAg.addColumnText("inventoryName", this.terms["economy.inventory.writeoffs.inventoryname"], null);
            this.gridAg.addColumnText("inventoryDescription", this.terms["common.description"], null, true, { hide: true });
            this.gridAg.addColumnIcon("notesIcon", null, null, { onClick: this.showNote.bind(this), showIcon: () => true, enableHiding: true, hide: true });
            this.gridAg.addColumnDate("date", this.terms["economy.inventory.writeoffs.date"], null, true, null, { hide: true });
            this.gridAg.addColumnText("typeName", this.terms["common.type"], null);
            this.gridAg.addColumnText("categories", this.terms["common.categories"], null, true);
            this.gridAg.addColumnText("status", this.terms["economy.accounting.accountdistributionentry.status"], null);
            this.gridAg.addColumnNumber("amount", this.terms["economy.inventory.writeoffs.periodamount"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("writeOffAmount", this.terms["economy.inventory.writeoffs.writeoffamount"], null, { decimals: 2, enableHiding: true });
            this.gridAg.addColumnNumber("writeOffYear", this.terms["economy.inventory.writeoffs.writeoffyearamount"], null, { decimals: 2, enableHiding: true });
            this.gridAg.addColumnNumber("writeOffTotal", this.terms["economy.inventory.writeoffs.writeofftotalamount"], null, { decimals: 2, enableHiding: true });
            this.gridAg.addColumnNumber("writeOffSum", this.terms["economy.inventory.inventories.writeoffsum"], null, { decimals: 2, enableHiding: true });
            this.gridAg.addColumnNumber("currentAmount", this.terms["economy.inventory.writeoffs.currentamount"], null, { decimals: 2, enableHiding: true});
            this.gridAg.addColumnNumber("voucherNr", this.terms["economy.accounting.voucher.voucher"], null);
            this.gridAg.addColumnIcon(null, this.terms["economy.accounting.accountdistributionentry.restore"], null, { icon: "fal fa-pencil iconEdit", onClick: this.handleEdit.bind(this), showIcon: this.showEdit.bind(this) });
            this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-exclamation-triangle warningColor", showIcon: (row) => row.periodError, toolTip: this.terms["economy.inventory.writeoffs.perioderror"] });
            this.gridAg.setExporterFilenamesAndHeader("economy.inventory.inventories.writeoff");
        }

        //Set up totals row
        this.gridAg.options.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });

        //Set up summarizing
        this.gridAg.options.addFooterRow("#sum-footer-grid", {
            "amount": "sum",
            "writeOffAmount": "sum",
            "writeOffYear": "sum",
            "writeOffTotal": "sum",
            "currentAmount": "sum",
        } as IColumnAggregations);

        // Events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
            this.$timeout(() => {
                this.buttonDisabled = this.gridAg.options.getSelectedCount() === 0;
            });
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows) => {
            this.$timeout(() => {
                this.buttonDisabled = this.gridAg.options.getSelectedCount() === 0;
            });
        }));

        this.gridAg.options.subscribe(events);

        this.gridAg.addStandardMenuItems();
        this.gridAg.options.finalizeInitGrid();
    }

    private loadEntryRows(params: any) {
        if (!params.data['rowsLoaded']) {

            var rows = [];
            for (let i = 0; i < params.data.accountDistributionEntryRowDTO.length; i++) {
                const row = params.data.accountDistributionEntryRowDTO[i];

                rows.push({
                    dim1Name: row.dim1Nr + " " + row.dim1Name,
                    dim2Name: ((row.dim2Nr != null) ? row.dim2Nr.toString() : "") + " " + ((row.dim2Name != null) ? row.dim2Name.toString() : ""),
                    dim3Name: ((row.dim3Nr != null) ? row.dim3Nr.toString() : "") + " " + ((row.dim3Name != null) ? row.dim3Name.toString() : ""),
                    dim4Name: ((row.dim4Nr != null) ? row.dim4Nr.toString() : "") + " " + ((row.dim4Name != null) ? row.dim4Name.toString() : ""),
                    dim5Name: ((row.dim5Nr != null) ? row.dim5Nr.toString() : "") + " " + ((row.dim5Name != null) ? row.dim5Name.toString() : ""),
                    dim6Name: ((row.dim6Nr != null) ? row.dim6Nr.toString() : "") + " " + ((row.dim6Name != null) ? row.dim6Name.toString() : ""),
                    debet: (row.sameBalance != null) ? row.sameBalance.toString() : "",
                    credit: (row.oppositeBalance != null) ? row.oppositeBalance.toString() : "",
                });
            }

            params.data['rows'] = rows;
            params.data['rowsLoaded'] = true;
            params.successCallback(params.data['rows']);

        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private getPrevMonth() {
        this.currentMonth.setMonth(this.currentMonth.getMonth() - 1);
        this.reloadFromDateChange();
    }

    private getNextMonth() {
        this.currentMonth.setMonth(this.currentMonth.getMonth() + 1);
        this.reloadFromDateChange();
    }

    public reloadFromDateChange = _.debounce(() => {
        this.loadGridData();
    }, 700, { leading: false, trailing: true });

    private getDetails() {
        this.progress.startWorkProgress((completion) => {
            const pvm: Date = new Date(<any>this.currentMonth);

            this.accountingService.transferToAccountDistributionEntry(pvm, this.distributionType).then((result) => {
                if (result.success) {
                    completion.completed(this.distributionEntries, true);
                } else {
                    let message: string = result.errorMessage;
                    if (result.errorMessage == "AccountYear")
                        message = this.terms["economy.accounting.accountdistributionentry.accountyearmissingmessage"];
                    else if (result.errorMessage == "AccountPeriod")
                        message = this.terms["economy.accounting.accountdistributionentry.accountperiodclosed"];

                    completion.failed(message);
                }

            });
        }).then(data => {
            this.loadGridData();

        }, error => { });
    }

    private showEdit(row) {
        if (row.state === SoeEntityState.Deleted || (row.voucherHeadId && row.voucherHeadId != null && row.state === SoeEntityState.Active))
            return true;

        return false;
    }

    private showEditSource(row) {
        if (row.sourceSeqNr && row.sourceSeqNr != null)
            return true;

        return false;
    }

    private showDeletePermanently(row) {
        if (row.state === SoeEntityState.Deleted)
            return true;

        return false;
    }

    private handleEdit(row) {
        if (row.voucherHeadId && row.voucherHeadId != null && row.state === SoeEntityState.Active)
            this.showVoucher(row);
    }

    private handleEditSource(row) {
        if (row.sourceVoucherHeadId && row.sourceVoucherHeadId != null)
            this.showSourceVoucher(row);
        else if ((row.sourceSupplierInvoiceId && row.sourceSupplierInvoiceId != null) ||
            (row.supplierInvoiceId && row.supplierInvoiceId != null))
            this.showSourceSupplierInvoice(row);
        else if (row.sourceCustomerInvoiceId && row.sourceCustomerInvoiceId != null)
            this.showSourceCustomerInvoice(row);
    }

    private initTransferSelectedItemsToVoucher = _.debounce(() => {
        if (this.gridAg.options.getSelectedCount() === 0) {
            this.notificationService.showDialog(this.terms["core.info"], this.terms["economy.accounting.accountdistributionentry.noentriesselected"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        }
        else {
            const rows = [];
            let invalidCount = 0;
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                if (row.voucherHeadId == null && row.state === SoeEntityState.Active && !row.periodError)
                    rows.push(row);
                else
                    invalidCount++;
            });

            //clear selectedrows to prevent double transfers
            this.gridAg.options.clearSelectedRows();

            if (invalidCount > 0 && this.distributionType === SoeAccountDistributionType.Inventory_WriteOff) {
                const modal = this.notificationService.showDialog(this.terms["core.warning"], invalidCount === 1 ? this.terms["economy.inventory.writeoffs.perioderrorinfosingle"] : invalidCount + " " + this.terms["economy.inventory.writeoffs.perioderrorinfomulti"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if(rows.length > 0)
                        this.transferSelectedItemsToVoucher(rows);
                });
            }
            else {
                if(rows.length > 0)
                    this.transferSelectedItemsToVoucher(rows);
                else
                    this.notificationService.showDialog(this.terms["core.info"], this.terms["economy.accounting.accountdistributionentry.nowrowstotransfer"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            }
        }
    }, 500, { leading: false, trailing: true });

    private transferSelectedItemsToVoucher(rows) {
        this.progress.startWorkProgress((completion) => {
            if (rows.length > 0) {
                this.accountingService.transferAccountDistributionEntryToVoucher(rows, this.currentMonth, this.distributionType).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed(null);
                        this.loadGridData();
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                });
            }
            else {
                completion.completed(null, null, this.terms["economy.accounting.accountdistributionentry.nowrowstotransfer"]);
            }
        });
    }

    private deleteSelectedEntrys(ignoreDeleteWarning: boolean = false) {
        if (this.gridAg.options.getSelectedCount() === 0) {
            this.notificationService.showDialog(this.terms["core.info"], this.distributionType == SoeAccountDistributionType.Inventory_WriteOff ? this.terms["economy.inventory.writeoffs.noentriesselected"] : this.terms["economy.accounting.accountdistributionentry.noentriesselected"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        }
        else {
            const rows = [];
            let warning = false;

            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                if (
                    row.triggerType !=
                    TermGroup_AccountDistributionTriggerType.Distribution
                ) {
                    warning = true;
                }

                if (row.voucherHeadId == null && row.state === SoeEntityState.Active)
                    rows.push(row);
            });

            if (rows.length > 0) {
                if (warning && !ignoreDeleteWarning) {
                    const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.inventory.writeoffs.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(res => {
                        if (res)
                            this.deleteSelectedEntrys(true);
                        return;
                    });
                }

                if (!warning || ignoreDeleteWarning) {
                    this.accountingService.deleteAccountDistributionEntries(rows, this.distributionType).then((x) => {
                        this.loadGridData();
                    });
                }
            }
            else {
                this.notificationService.showDialog(this.terms["core.info"], this.distributionType == SoeAccountDistributionType.Inventory_WriteOff ? this.terms["economy.inventory.writeoffs.nowrowstodelete"] : this.terms["economy.accounting.accountdistributionentry.nowrowstodelete"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            }
        }
    }    

    private showVoucher(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.voucherEditPrefixTerm + " " + row.voucherNr, row.voucherHeadId, VouchersEditController, { id: row.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
    }

    private showSourceVoucher(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.voucherEditPrefixTerm + " " + row.sourceVoucherNr, row.sourceVoucherHeadId, VouchersEditController, { id: row.sourceVoucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
    }

    private showSourceSupplierInvoice(row) {
        let prefixNr: string = row.invoiceNr;
        if (row.sourceSupplierInvoiceSeqNr)
            prefixNr = row.sourceSupplierInvoiceSeqNr;

        if (row.sourceSupplierInvoiceId != null)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.supplierInvoiceEditPrefixTerm + " " + prefixNr, row.sourceSupplierInvoiceId, SupplierInvoiceEditController, { id: row.sourceSupplierInvoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
        else
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.supplierInvoiceEditPrefixTerm + " " + prefixNr, row.supplierInvoiceId, SupplierInvoiceEditController, { id: row.supplierInvoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
    }

    private showSourceCustomerInvoice(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.customerInvoiceEditPrefixTerm + " " + row.sourceCustomerInvoiceSeqNr != null ? row.sourceCustomerInvoiceSeqNr : row.invoiceNr, row.sourceCustomerInvoiceId, CustomerInvoiceEditController, { id: row.sourceCustomerInvoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html')));
    }

    private showNote(row: AccountDistributionEntryDTO) {
        // Show edit note dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Economy/Accounting/AccountDistributionEntry/Views/editNoteDialog.html"),
            controller: EditNoteController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: '',
            resolve: {
                rows: () => { return this.distributionEntries },
                row: () => { return row },
                isReadonly: () => { return false }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.rowIsModified) {
                let inventory = this.distributionEntries.filter(w => w.inventoryId == result.inventoryId)[0] as AccountDistributionEntryDTO;
                inventory.inventoryNotes = result.notes;
                inventory.inventoryDescription = result.description;
                this.isDirty = true;
                this.gridAg.options.refreshRows();
            }
        });
    }
}
