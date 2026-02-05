import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { PaymentImportIODTO } from "../../../../Common/Models/PaymentImportIODTO";
import { CustomerInvoiceDTO } from "../../../../Common/Models/InvoiceDTO";
import { PaymentImportDTO } from "../../../../Common/Models/PaymentImportDTO";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { ImportPaymentType, Feature, ImportPaymentIOState, SoeOriginType, TermGroup_SysPaymentType, SoeInvoiceMatchingType, CompanySettingType, TermGroup_SysPaymentMethod, ImportPaymentIOStatus, TermGroup_BillingType, SoeEntityState, CustomerAccountType, AccountingRowType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { StringUtility } from "../../../../Util/StringUtility";
import { IColumnAggregations } from "../../../../Util/SoeGridOptionsAg";
import { EmbeddedGridController } from "../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IActionResult } from "../../../../Scripts/TypeLite.Net4";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../../../Util/ToolBarUtility";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { SelectCustomerInvoiceController } from "../../../../Common/Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { EditController as CustomerPaymentsEditController } from "../../../../Common/Customer/Payments/EditController";

enum PaymentImportUpdateFunctions {
    UpdatePayment = 1,
    UpdateStatus = 2,
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private importFile: any;

    private currentAccountYearId = 0;

    private paymentImportIO: PaymentImportIODTO;
    private paymentImportIODTOs: PaymentImportIODTO[];
    private paymentImportIODTOsToUpdate: PaymentImportIODTO[];
    private terms: { [index: string]: string; };
    private invoice: CustomerInvoiceDTO;

    private gridpaymentButtonGroups = new Array<ToolBarButtonGroup>();
    private gridpaymentOptions: EmbeddedGridController;

    private gridAccordionIsOpen = false;

    private mergeInvoices = false;
    private reportPermission = false;
    private customerPaymentEditPermission = false;

    private manualCustomerPaymentTransferToVoucher = false;
    private supplierInvoiceAskPrintVoucherOnTransfer = false;
    private defaultVoucherSeriesTypeId = 0;
    private defaultPaymentConditionId = 0;
    private defaultPaymentMethodId = 0;
    private voucherListReportId: number = null;
    private useExternalInvoiceNr = false;

    private defaultCreditAccountId = 0;
    private defaultDebitAccountId = 0;

    private isChooseFileButtonVisible = false;
    private isUploadFileButtonVisible = false;

    private paymentImportId: number;
    private paymentImport: PaymentImportDTO;
    private importPaymentTypeId: number;
    private isCustomerPayment: boolean;
    private isSupplierPayment: boolean;

    private paymentMethod: any;
    private paymentMethods: any[] = [];
    private paymentMethodsDict: any[] = [];
    private paymentMethodsAccounts: any[] = [];
    private paymentTypes: any[] = [];
    private matchCodesDict: any[] = [];

    private prevInvoiceNr: string = "";
    private prevPaidAmount = 0;
    private prevMatchCodeId = 0;

    private updateButtonDisabled = true;

    private filteredPaid = 0;
    private selectedPaid = 0;

    // Functions
    buttonFunctions: any = [];

    private _selectedSysPaymentType: any;
    get selectedSysPaymentType() {
        return this._selectedSysPaymentType;
    }
    set selectedSysPaymentType(item: any) {
        this._selectedSysPaymentType = item;
    }

    private _selectedPaymentMethod: any;
    get selectedPaymentMethod() {
        return this._selectedPaymentMethod;
    }
    set selectedPaymentMethod(item: any) {
        this._selectedPaymentMethod = item;

        if (item) {
            this.loadPaymentMethodsAccounts(item.id);
        }
    }

    private _selectedImportDate: Date;
    get selectedImportDate() {
        return this._selectedImportDate;
    }
    set selectedImportDate(date: any) {
        if (date instanceof Date) {
            this._selectedImportDate = date;
        }
        else {
            this._selectedImportDate = date ? new Date(<string>date.toString()) : undefined;
        }

        if (this.paymentImport) {
            this.paymentImport.importDate = this.selectedImportDate;
        }
    }

    private get showDeleteButton() {
        return this.paymentImportId > 0 && this.paymentImportIODTOs && this.paymentImportIODTOs.length > 0 && !_.find(this.paymentImportIODTOs, (p) => p.state === ImportPaymentIOState.Closed);
    }

    private importDateLabel;

    // Id for trace rows
    selectedInvoiceId: number

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private supplierService: ISupplierService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private gridHandlerFactory: IGridHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private translationService: ITranslationService,
        private commonCustomerService: ICommonCustomerService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $uibModal: any) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())

        this.currentAccountYearId = soeConfig.accountYearId;

        this.selectedImportDate = new Date();
    }

    public onInit(parameters: any) {
        this.importPaymentTypeId = parameters.importType;
        if (this.importPaymentTypeId === ImportPaymentType.SupplierPayment) {
            this.isSupplierPayment = true;
            this.isCustomerPayment = false;
        }
        else {
            this.isSupplierPayment = false;
            this.isCustomerPayment = true;
        }

        this.paymentImportId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Import_Payments, loadReadPermissions: true, loadModifyPermissions: true }, { feature: Feature.Economy_Customer_Payment_Payments_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);

        this.gridpaymentOptions = new EmbeddedGridController(this.gridHandlerFactory, "Economy.Import.Payment.Edit.Details" + (this.importPaymentTypeId === ImportPaymentType.SupplierPayment ? ".Supplier" : ".Customer"));
        this.gridpaymentOptions.gridAg.options.enableRowSelection = true;
        this.gridpaymentOptions.gridAg.options.setMinRowsToShow(15);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Import_Payments].readPermission;
        this.modifyPermission = response[Feature.Economy_Import_Payments].modifyPermission;
        this.customerPaymentEditPermission = response[Feature.Economy_Customer_Payment_Payments_Edit].readPermission || response[Feature.Economy_Customer_Payment_Payments_Edit].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            //() => this.setupPaymentGrid(),
            () => this.loadPaymentMethods(),
            () => this.loadPaymentTypes(),
            () => this.loadMatchCodes(),
            () => this.loadCompanySettings(),
            () => this.loadPermissions()
        ]).then(() => this.setupPaymentGrid()).then(() => this.onLoadData());
    }

    private onLoadData(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.paymentImportId > 0) {
            return this.$q.all([this.getPaymentImport()]).then(() => this.loadImportedIoInvoices());
        } else {
            this.createNew();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private paymentMethodDisabled(): boolean {
        let result = (this.paymentImportId > 0)
        if (result && !this.selectedPaymentMethod) {
            result = false;
        }
        return result;
    }
    private createNew() {
        this.isNew = true;
        this.paymentImportId = 0;
        this.paymentImport = new PaymentImportDTO();
        this.paymentImport.paymentImportId = 0;
        this.paymentImport.batchId = 0;

        if (this.importPaymentTypeId == ImportPaymentType.CustomerPayment || this.importPaymentTypeId == ImportPaymentType.SupplierPayment)
            this.selectedPaymentMethod = _.find(this.paymentMethodsDict, { id: this.defaultPaymentMethodId });

        this.isChooseFileButtonVisible = true;
    }

    private getPaymentImport(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.paymentImportId > 0) {
            return this.accountingService.getPaymentImport(this.paymentImportId).then((x) => {
                this.isNew = false;
                this.paymentImport = x;
                this.selectedImportDate = x.importDate;
                this.selectedPaymentMethod = _.find(this.paymentMethodsDict, { id: x.type });
            });
        }

        deferral.resolve();

        return deferral.promise;
    }

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.paymentImport) {
                if (!this.selectedPaymentMethod) {
                    mandatoryFieldKeys.push("economy.import.payment.paymentmethod");
                }
                if (!this.selectedImportDate) {
                    mandatoryFieldKeys.push("economy.import.payment.importdate");
                }
            }
        });
    }

    private intPaymentGrid() {
        // Toolbar
        if (this.isCustomerPayment) {
            this.gridpaymentButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.newrow", "core.newrow", IconLibrary.FontAwesome, "fal fa-plus", () => {
                this.addRow();
            }, () => (!this.paymentImport || this.paymentImport.paymentImportId === 0))));
        }

        // Set up events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.beginCellEdit(entity, colDef); }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: PaymentImportIODTO[]) => {
            this.summarizeFiltered(rows);
        }));

        const eventRowBeforeSelectionChanged: GridEvent = new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => {
            this.gridRowSelected(row);

            //Validate button
            this.$timeout(() => {
                this.updateButtonDisabled = this.gridpaymentOptions.gridAg.options.getSelectedCount() === 0;
                this.summarizeSelected();
            });
        });

        const eventRowBeforeSelectionChangedBatch: GridEvent = new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: any[]) => {
            this.gridRowSelected(rows[0]);
            //Validate button
            this.$timeout(() => {
                this.updateButtonDisabled = this.gridpaymentOptions.gridAg.options.getSelectedCount() === 0;
                this.summarizeSelected();
            });
        });

        const eventRowClicked: GridEvent = new GridEvent(SoeGridOptionsEvent.RowClicked, (row) => {
            if (!row)
                return;

            this.gridRowSelected(row);
        });

        const eventIsRowSelectable: GridEvent = new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (row: any) => {
            return row && row.data && row.data.state !== ImportPaymentIOState.Closed && row.data.status !== 6;
        });

        events.push(eventRowBeforeSelectionChanged);
        events.push(eventRowBeforeSelectionChangedBatch);
        events.push(eventRowClicked);
        events.push(eventIsRowSelectable);

        this.gridpaymentOptions.gridAg.options.subscribe(events);
    }

    private setupPaymentGrid(): ng.IPromise<any> {
        const defferal = this.$q.defer();

        const translationKeys: string[] = [
            "core.warning",
            "common.type",
            "economy.import.payment.customer",
            "economy.import.payment.invoiceseries",
            "economy.import.payment.invoicenr",
            "economy.import.payment.paymentdate",
            "economy.import.payment.invoicetotalamount",
            "economy.import.payment.duedate",
            "economy.import.payment.matchcode",
            "economy.import.payment.paidamount",
            "economy.import.payment.paiddate",
            "economy.import.payment.paymenttypename",
            "economy.import.payment.currency",
            "economy.supplier.invoice.remainingamount",
            "economy.import.payment.status",
            "economy.import.payment.supplier",
            "economy.import.payment.debit",
            "economy.import.payment.credit",
            "economy.import.payment.fullypaid",
            "economy.import.payment.matched",
            "economy.import.payment.paid",
            "economy.import.payment.partly_paid",
            "economy.import.payment.rest",
            "economy.import.payment.unknown",
            "economy.import.payment.error",
            "economy.supplier.invoice.duedate",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "economy.import.payment.error.paymentrowerror",
            "core.manual",
            "economy.import.payment.amountvalidationfailed",
            "common.customer.payment.paymentseqnr",
            "common.customer.payment.payment",
            "economy.import.payment.deleted",
            "economy.import.payments.importdate",
            "economy.supplier.invoice.ocr",
            "economy.import.payment.updatepaymentsbutton",
            "economy.import.payment.updatestatusbutton",
            "core.comment",
            "economy.import.payment.manualstatus",
            "core.missingcomment",
            "economy.import.payment.updatedstatusmessage"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.terms = terms;

            this.importDateLabel = this.importPaymentTypeId === ImportPaymentType.CustomerPayment ? this.terms["economy.import.payment.paiddate"] : this.terms["economy.import.payments.importdate"];

            if (this.isCustomerPayment) {
                this.gridpaymentOptions.gridAg.addColumnIsModified();
            }

            this.gridpaymentOptions.gridAg.addColumnText("typeName", terms["common.type"], null);


            if (this.importPaymentTypeId === ImportPaymentType.SupplierPayment) {
                this.gridpaymentOptions.gridAg.addColumnText("customer", terms["economy.import.payment.supplier"], null);
                this.gridpaymentOptions.gridAg.addColumnText("invoiceSeqnr", terms["economy.import.payment.invoiceseries"], null);
            }
            else {
                this.gridpaymentOptions.gridAg.addColumnText("customer", terms["economy.import.payment.customer"], null);
                this.gridpaymentOptions.gridAg.addColumnText("ocr", terms["economy.supplier.invoice.ocr"], null, true, { hide: true });
            }
            this.gridpaymentOptions.gridAg.addColumnText("invoiceNr", terms["economy.import.payment.invoicenr"], null, false, { editable: this.isCellEditable.bind(this), buttonConfiguration: { iconClass: "iconEdit fal fa-search", show: () => (this.isCellEditable.bind(this) && this.importPaymentTypeId === ImportPaymentType.CustomerPayment), callback: this.openSearchInvoiceDialog.bind(this) } });
            this.gridpaymentOptions.gridAg.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null, this.importPaymentTypeId === ImportPaymentType.CustomerPayment ? true : false);
            this.gridpaymentOptions.gridAg.addColumnDate("paidDate", terms["economy.import.payment.paiddate"], null, false, null, { editable: this.isDateEditable.bind(this) });
            this.gridpaymentOptions.gridAg.addColumnNumber("invoiceAmount", terms["economy.import.payment.invoicetotalamount"], null, { decimals: 2 });
            this.gridpaymentOptions.gridAg.addColumnNumber("paidAmount", terms["economy.import.payment.paidamount"], null, { decimals: 2, editable: this.isCellEditable.bind(this) });
            this.gridpaymentOptions.gridAg.addColumnNumber("restAmount", terms["economy.supplier.invoice.remainingamount"], null, { decimals: 2 });
            //this.gridpaymentOptions.gridAg.addColumnNumber("currency", terms["economy.import.payment.currency"], null, { decimals: 2 });
            this.gridpaymentOptions.gridAg.addColumnText("statusName", terms["economy.import.payment.status"], null);
            if (this.isCustomerPayment) {
                this.gridpaymentOptions.gridAg.addColumnText("paymentRowSeqNr", terms["common.customer.payment.paymentseqnr"], null, false, { editable: false, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.paymentRowId && row.paymentRowId > 0 && this.customerPaymentEditPermission, callback: this.openPayment.bind(this) } });
                this.gridpaymentOptions.gridAg.addColumnSelect("matchCodeId", this.terms["economy.import.payment.matchcode"], null, {
                    selectOptions: this.matchCodesDict,
                    enableHiding: false,
                    editable: this.isCellEditable.bind(this),
                    displayField: "matchCodeName",
                    dropdownIdLabel: "matchCodeId",
                    dropdownValueLabel: "name",
                });

                this.gridpaymentOptions.gridAg.addColumnDelete(null, this.deleteRow.bind(this), false, this.showDeleteRow.bind(this));
            }
            this.gridpaymentOptions.gridAg.addColumnText("comment", terms["core.comment"], null, true, { hide: true, toolTipField: "comment" });

            this.gridpaymentOptions.gridAg.options.getColumnDefs().forEach(col => {
                // Append closedRow to cellClass
                var cellcls: string = col.cellClass ? col.cellClass.toString() : "";
                col.cellClass = (grid: any) => {
                    if (grid.data['status'] === ImportPaymentIOStatus.PartlyPaid && grid.data['state'] === ImportPaymentIOState.Open) {
                        return cellcls + (col.field === 'paidAmount' ? " errorRow" : (grid.data['isSelectDisabled'] ? " closedRow" : ""))
                    }
                    else if (grid.data['status'] === ImportPaymentIOStatus.Unknown && grid.data['invoiceAmount'] === 0) {
                        return cellcls + (col.field === 'invoiceAmount' ? " errorRow" : "");
                    }
                    else
                        return cellcls + (grid.data['isSelectDisabled'] ? " closedRow" : "");
                };
            });

            this.gridpaymentOptions.gridAg.finalizeInitGrid("economy.import.payment.payments", true);

            // Add sum and totals grids
            this.$timeout(() => {
                this.gridpaymentOptions.gridAg.options.addFooterRow("#sum-footer-grid", {
                    "invoiceAmount": "sum",
                    "paidAmount": "sum",
                    "restAmount": "sum",
                    "currency": "sum",
                } as IColumnAggregations);
            });

            this.intPaymentGrid();

            // Set up functions
            this.buttonFunctions.push({ id: PaymentImportUpdateFunctions.UpdatePayment, name: terms["economy.import.payment.updatepaymentsbutton"] });
            this.buttonFunctions.push({ id: PaymentImportUpdateFunctions.UpdateStatus, name: terms["economy.import.payment.updatestatusbutton"] });

            defferal.resolve();
        });

        return defferal.promise;
    }

    private isCellEditable(row) {
        return (row.state !== ImportPaymentIOState.Closed);
    }

    private isDateEditable(row) {
        return ((row.state !== ImportPaymentIOState.Closed) || (row.invoiceId !== 0 && row.status === ImportPaymentIOStatus.Manual)) && !row['isSelectDisabled'];
    }

    private showDeleteRow(row: PaymentImportIODTO): boolean {
        let allowDelete: boolean = (row.status === ImportPaymentIOStatus.Manual && (row.paymentImportIOId === undefined || row.paymentImportIOId === 0));
        if (!allowDelete) {
            allowDelete = (row.status === ImportPaymentIOStatus.Unknown && row.paidAmount === 0);
        }

        return allowDelete;
    }

    private summarizeFiltered(invoices: PaymentImportIODTO[]) {
        this.filteredPaid = 0;
        this.filteredPaid = _.sum(_.map(invoices, i => i.paidAmount ? i.paidAmount : 0));
    }

    private summarizeSelected() {
        this.selectedPaid = 0;
        this.selectedPaid = _.sum(_.map(this.gridpaymentOptions.gridAg.options.getSelectedRows(), i => i.paidAmount ? i.paidAmount : 0));
    }

    private performFunction(option) {
        switch (option.id) {
            case PaymentImportUpdateFunctions.UpdatePayment:
                this.saveBulkPaymentImportIOInvoices();
                break;
            case PaymentImportUpdateFunctions.UpdateStatus:
                this.initUpdatePaymentImportIOStatus();
                break;
        }
    }

    private openSearchInvoiceDialog(row) {
        this.translationService.translate("common.customer.invoices.chooseinvoice").then(term => {
            var oldNr = row.invoiceNr;
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/selectcustomerinvoice", "selectcustomerinvoice.html"),
                controller: SelectCustomerInvoiceController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => { return term },
                    isNew: () => { return true },
                    ignoreChildren: () => { return false },
                    originType: () => { return SoeOriginType.CustomerInvoice },
                    customerId: () => { return null },
                    projectId: () => { return null },
                    invoiceId: () => { return null },
                    currentMainInvoiceId: () => { return null },
                    selectedProjectName: () => { return "" },
                    userId: () => { return null },
                    includePreliminary: () => { return false },
                    includeVoucher: () => { return true },
                    fullyPaid: () => { return false },
                    useExternalInvoiceNr: () => { return this.useExternalInvoiceNr },
                    importRow: () => { return row },
                }
            });

            modal.result.then(result => {
                if (result && result.invoice) {
                    row.invoiceNr = result.invoice.number;
                    this.updatePaymentImportIO(row, oldNr);
                    this.dirtyHandler.setDirty();
                }
            });
        })
    }

    private openEdit(paymentImportId: number, batchId: string) {
        this.messagingHandler.publishEditRow({ paymentImportId: paymentImportId, batchId: batchId });
    }

    private openPayment(row) {
        const message = new TabMessage(this.terms["common.customer.payment.payment"] + " " + row.paymentRowSeqNr.toString(), "pay_" + row.paymentRowId, CustomerPaymentsEditController, { paymentId: row.paymentRowId }, this.urlHelperService.getGlobalUrl("Common/Customer/Payments/Views/edit.html"));
        this.messagingHandler.publishOpenTab(message)
    }

    private addRow() {
        const newRow = new PaymentImportIODTO();

        newRow.tempRowId = this.paymentImportIODTOs.length + 1;
        newRow.status = ImportPaymentIOStatus.Manual;
        newRow.importType = ImportPaymentType.CustomerPayment;
        newRow.isModified = true;
        newRow.paidDate = this.selectedImportDate ? this.selectedImportDate : CalendarUtility.getDateToday();

        this.setStatus(newRow);

        const currRow = this.gridpaymentOptions.gridAg.options.getCurrentRow();
        const insertAt = currRow ? this.gridpaymentOptions.gridAg.options.getRowIndex(currRow) + 1 : this.paymentImportIODTOs.length + 1;

        this.paymentImportIODTOs.splice(insertAt, 0, newRow);

        this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);

        this.dirtyHandler.setDirty();
    }

    private deleteRow(row: PaymentImportIODTO) {
        if (row.paymentImportIOId) {
            this.progress.startWorkProgress((completion) => {
                this.accountingService.deletePaymentImportIORow(row.paymentImportIOId).then(result => {
                    if (result.success) {
                        completion.completed(null);
                        _.remove(this.paymentImportIODTOs, (r) => r.tempRowId === row.tempRowId);
                        this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            });
        }
        else {
            _.remove(this.paymentImportIODTOs, (r) => r.tempRowId === row.tempRowId);
            this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);
            
        }
        this.dirtyHandler.setDirty();
    }

    /*private addSumAggregationFooterToColumns(...args: uiGrid.IColumnDef[]) { //TODO: WHAT DOTS DOES???
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }*/

    private loadImportedIoInvoices(): ng.IPromise<any> {
        this.paymentImportIODTOs = [];
        const deferral = this.$q.defer();

        if (!this.paymentImport) {
            deferral.resolve();
            return deferral.promise;
        }

        const importPaymentType = this.importPaymentTypeId == ImportPaymentType.CustomerPayment
            ? ImportPaymentType.CustomerPayment
            : ImportPaymentType.SupplierPayment;

        return this.accountingService.getImportedIoInvoices(this.paymentImport.batchId, importPaymentType).then((result: PaymentImportIODTO[]) => {
            let counter = 1;

            result.forEach(x => {
                x.tempRowId = counter;
                counter++;
                this.setStatusTexts(x);
            });
            this.paymentImportIODTOs = result;

            this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);

            this.gridAccordionIsOpen = true;
        });
    }

    private setStatusTexts(r: PaymentImportIODTO) {
        this.setBillingType(r);
        this.setStatus(r);
        this.setMatchCode(r);
        r.paymentTypeName = "Girering";
    }

    private loadPaymentMethodsAccounts(paymentMethodId: number) {
        if (paymentMethodId > 0 && this.paymentMethodsAccounts.length > 0) {
            const isExists: boolean = _.includes(this.paymentMethodsAccounts, { id: paymentMethodId });
            if (isExists) {
                return;
            }
        }

        this.commonCustomerService.getPaymentMethod(paymentMethodId, true, false).then(method => {
            this.paymentMethodsAccounts.push({
                id: method.paymentMethodId,
                accountId: method.accountId
            });
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        const originType = this.importPaymentTypeId == ImportPaymentType.CustomerPayment
            ? SoeOriginType.CustomerPayment
            : SoeOriginType.SupplierPayment;

        return this.accountingService.getPaymentMethodsForImport(originType).then((data: any[]) => {
            this.paymentMethods = data;
            data.forEach(x => {
                this.paymentMethodsDict.push({ id: x.paymentMethodId, name: x.name });
            })
        });
    }

    private loadPaymentTypes(): ng.IPromise<any> {
        return this.accountingService.getSysPaymentTypeDict().then((x: any[]) => {
            //Only supported is bg and pg and sepa
            x = x.filter(y => y.id === TermGroup_SysPaymentType.BG || y.id === TermGroup_SysPaymentType.PG || y.id === TermGroup_SysPaymentType.SEPA)
            x.forEach((y: any) => {
                this.paymentTypes.push({ id: y.id, name: y.name });
            });

            this.selectedSysPaymentType = this.paymentTypes.length > 0 ? this.paymentTypes[0] : 0;
        });
    }

    private loadMatchCodes(): ng.IPromise<any> {
        return this.accountingService.getMatchCodes(SoeInvoiceMatchingType.CustomerInvoiceMatching, true).then((x) => {
            this.matchCodesDict = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountingVoucherImportVoucherSerie);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentMethod);
        settingTypes.push(CompanySettingType.SupplierPaymentDefaultPaymentMethod);
        settingTypes.push(CompanySettingType.CustomerPaymentManualTransferToVoucher);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.CustomerPaymentVoucherSeriesType);
        settingTypes.push(CompanySettingType.AccountCustomerSalesVat);
        settingTypes.push(CompanySettingType.AccountCustomerClaim);
        settingTypes.push(CompanySettingType.BillingUseExternalInvoiceNr);

        settingTypes.push(CompanySettingType.AccountCommonCheck);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim); //TODO: CHECK THIS
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonCheck);
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentVoucherSeriesType);
            this.manualCustomerPaymentTransferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentManualTransferToVoucher);
            this.defaultPaymentConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentCondition);

            if (this.isSupplierPayment)
                this.defaultPaymentMethodId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierPaymentDefaultPaymentMethod);
            else if (this.isCustomerPayment)
                this.defaultPaymentMethodId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentMethod);

            this.voucherListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVoucherList);
            this.supplierInvoiceAskPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer);
            this.useExternalInvoiceNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseExternalInvoiceNr);
        });
    }

    private loadPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];

        featureIds.push(Feature.Billing_Distribution_Reports_Selection);            // Invoice report
        featureIds.push(Feature.Billing_Distribution_Reports_Selection_Download);   // Invoice report
        featureIds.push(Feature.Billing_Order_Status_OrderToInvoice);               // Merge invoices

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.mergeInvoices = x[Feature.Billing_Order_Status_OrderToInvoice];
            this.reportPermission = x[Feature.Economy_Distribution_Reports_Selection] && x[Feature.Economy_Distribution_Reports_Selection_Download];
        });
    }

    private saveBulkPaymentImportIOInvoices(bypassValidation = false) {
        this.paymentImportIODTOsToUpdate = _.filter(this.gridpaymentOptions.gridAg.options.getSelectedRows(), (r) => r.paidAmount && r.paidAmount !== 0 && r.invoiceId && r.invoiceId !== 0 && r.status !== ImportPaymentIOStatus.Unknown);

        if (this.importPaymentTypeId === ImportPaymentType.CustomerPayment && !bypassValidation) {
            this.validatePayments(this.paymentImportIODTOsToUpdate);
            return;
        }

        //Set diff
        _.forEach(this.paymentImportIODTOsToUpdate, (p) => {
            p.amountDiff = NumberUtility.parseDecimal((p.paidAmount - p.invoiceAmount).toFixed(2));
        });

        this.progress.startSaveProgress((completion) => {

            if (this.importPaymentTypeId === ImportPaymentType.CustomerPayment) {
                this.accountingService.updateCustomerPaymentImportIODTOS(this.paymentImportIODTOsToUpdate,
                    this.selectedImportDate,
                    this.currentAccountYearId,
                    this.selectedPaymentMethod.id).then((result) => {
                        if (result.success) {

                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentImport);
                        } else {

                            completion.failed(result.errorMessage);
                        }
                    }, error => { completion.failed(error.message); });

            } else {
                this.accountingService.updatePaymentImportIODTOS(this.paymentImportIODTOsToUpdate,
                    this.selectedImportDate,
                    this.currentAccountYearId).then((result) => {
                        if (result.success) {
                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentImport);
                        } else {
                            completion.failed((result.errorMessage) ? result.errorMessage : this.terms["economy.import.payment.error.paymentrowerror"]);
                        }
                    }, error => { completion.failed(error.errorMessage); });
            }

        }, this.guid).then(data => {
            this.dirtyHandler.clean();
            this.onLoadData();

        }, error => { });
    }

    private initUpdatePaymentImportIOStatus() {
        let modal = this.notificationService.showDialogEx(this.terms["core.comment"], "", SOEMessageBoxImage.None, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxRows: 3, useTextValidation: true });
        modal.result.then(val => {
            modal.result.then(result => {
                if (result.result) {
                    if (result.textBoxValue)
                        this.updatePaymentImportIOStatus(result.textBoxValue);
                    else
                        this.notificationService.showDialog(this.terms["core.warning"], this.terms["core.missingcomment"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                }
            });
        });
    }

    private updatePaymentImportIOStatus(comment: string) {
        const selectedRows = this.gridpaymentOptions.gridAg.options.getSelectedRows() as PaymentImportIODTO[];
        const rowsToUpdate = selectedRows.filter((r) => r.paymentImportIOId && (r.status === ImportPaymentIOStatus.Unknown || r.status === ImportPaymentIOStatus.ManuallyHandled));
        rowsToUpdate.forEach((r) => {
            r.comment = comment;
        })

        this.progress.startSaveProgress((completion) => {
            this.accountingService.updatePaymentImportIODTOSStatus(rowsToUpdate).then((result) => {
                    if (result.success) {
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentImport, false, this.terms["economy.import.payment.updatedstatusmessage"].format(rowsToUpdate.length.toString(), this.gridpaymentOptions.gridAg.options.getSelectedRows().length.toString()));
                    } else {

                        completion.failed(result.errorMessage);
                    }
                }, error => { completion.failed(error.message); });

        }, this.guid).then(data => {
            this.dirtyHandler.clean();
            this.onLoadData();

        });
    }

    private validatePayments(itemsToUpdate: PaymentImportIODTO[]) {
        const totalPaidAmount =  _.sum(_.map(this.paymentImportIODTOs, r => r.paidAmount)).round(4);

        if (this.paymentImport.totalAmount !== totalPaidAmount) {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.import.payment.amountvalidationfailed"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        }
        else {
            this.saveBulkPaymentImportIOInvoices(true);
        }
    }

    private statusChanged(row) {
        const obj = _.find(this.loadStatuses(), { value: row.status });
        if (obj) {
            row.status = obj["value"];
            row.statusName = obj["label"];
        }
    }

    private addFile() {
        this.translationService.translate("core.fileupload.choosefiletoimport").then((term) => {
            var url = CoreUtility.apiPrefix + Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTFILEIMPORT;
            const modal = this.notificationService.showFileUpload(url, term, true, true, false);
            modal.result.then(res => {
                let result: IActionResult = res.result;
                if (result.success) {
                    this.importFile = result.value;
                    this.paymentImport.filename = result.value2;

                    this.isChooseFileButtonVisible = false;
                    this.isUploadFileButtonVisible = true;
                } else {
                    //this.failedWork(result.errorMessage);
                }

            }, error => {
                //this.failedWork(error.message)
            });
        });
    }

    private startImportFile() {
        this.setPaymentMetod();

        if (this.importPaymentTypeId == ImportPaymentType.CustomerPayment)
            this.startCustomerImportFile();
        else
            this.startSupplierImportFile();

    }

    private setPaymentMetod() {
        if (this.selectedPaymentMethod) {
            this.paymentMethod = this.paymentMethods.find(x => x.paymentMethodId === this.selectedPaymentMethod.id);
        }
    }
    private setPaymentMetodToImportDTO() {
        this.paymentImport.type = this.paymentMethod.paymentMethodId;
        this.paymentImport.sysPaymentTypeId = this.selectedSysPaymentType.id;
    }

    private startCustomerImportFile() {

        this.setPaymentMetodToImportDTO();
        this.paymentImport.importDate = this.selectedImportDate;
        this.paymentImport.importType = ImportPaymentType.CustomerPayment;

        this.progress.startSaveProgress((completion) => {
            this.accountingService.savePaymentImportHeader(this.paymentImport)
                .then(result => {
                    if (result.success) {
                        this.paymentImportId = result.value2;
                        this.paymentImport.batchId = result.value;

                        var paymentImportRows: any = {};

                        paymentImportRows.paymentIOType = TermGroup_SysPaymentMethod.BGMax;
                        if (this.paymentMethod != null) {
                            paymentImportRows.paymentIOType = this.paymentMethod.sysPaymentMethodId;
                        }

                        paymentImportRows.type = this.selectedSysPaymentType.id;
                        paymentImportRows.paymentMethodId = this.selectedPaymentMethod.id;
                        paymentImportRows.contents = this.importFile;
                        paymentImportRows.fileName = this.paymentImport.filename;
                        paymentImportRows.batchId = result.value;
                        paymentImportRows.paymentImportId = this.paymentImportId;
                        paymentImportRows.importType = ImportPaymentType.CustomerPayment;

                        this.savePaymentImportRows(paymentImportRows, completion);

                    } else {
                        this.isChooseFileButtonVisible = true;
                        this.isUploadFileButtonVisible = false;

                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    this.isChooseFileButtonVisible = true;
                    this.isUploadFileButtonVisible = false;

                    completion.failed(error.message);
                });

        }, this.guid).then(() => {

            this.isChooseFileButtonVisible = false;
            this.isUploadFileButtonVisible = false;

            this.dirtyHandler.clean();
            this.onLoadData();
        });

    }

    private startSupplierImportFile() {

        this.setPaymentMetodToImportDTO();
        this.paymentImport.importDate = this.selectedImportDate;
        this.paymentImport.importType = ImportPaymentType.SupplierPayment;

        this.progress.startSaveProgress((completion) => {
            this.accountingService.savePaymentImportHeader(this.paymentImport)
                .then(result => {
                    if (result.success) {
                        this.paymentImportId = result.value2;
                        this.paymentImport.batchId = result.value;

                        let paymentImportRows: any = {};

                        if (this.paymentMethod != null) {
                            paymentImportRows.paymentIOType = this.paymentMethod.sysPaymentMethodId;
                        }

                        paymentImportRows.type = this.selectedSysPaymentType.id;
                        paymentImportRows.paymentMethodId = this.selectedPaymentMethod.id;
                        paymentImportRows.contents = this.importFile;
                        paymentImportRows.fileName = this.paymentImport.filename;
                        paymentImportRows.batchId = result.value;
                        paymentImportRows.paymentImportId = this.paymentImportId;
                        paymentImportRows.importType = ImportPaymentType.SupplierPayment;

                        this.savePaymentImportRows(paymentImportRows, completion);
                    } else {
                        this.isChooseFileButtonVisible = true;
                        this.isUploadFileButtonVisible = false;

                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    this.isChooseFileButtonVisible = true;
                    this.isUploadFileButtonVisible = false;

                    completion.failed(error.message);
                });

        }, this.guid).then(() => {

            this.isChooseFileButtonVisible = false;
            this.isUploadFileButtonVisible = false;

            this.dirtyHandler.clean();
            this.onLoadData();
        });
    }

    private savePaymentImportRowsAndReload(rows: PaymentImportIODTO[], completion: any) {
        this.accountingService.updatePaymentImportIOs(rows).then((result) => {
                this.accountingService.getImportedIoInvoices(this.paymentImport.batchId, this.importPaymentTypeId).then((result: PaymentImportIODTO[]) => {
                    this.paymentImportIODTOs = result;
                    let counter = 1;
                    result.forEach(x => {
                        x.tempRowId = counter;
                        counter++;
                        this.setStatusTexts(x);
                    });
                    this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentImport);
                })
        });
    }

    private savePaymentImportRows(paymentImportRows: any, completion: any) {

        this.accountingService.savePaymentImportRow(paymentImportRows).then((result: IActionResult) => {
            if (result.success) {
                completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.paymentImport);
            } else {

                this.isChooseFileButtonVisible = true;
                this.isUploadFileButtonVisible = false;

                completion.failed(result.errorMessage);
            }

            if (result.keys && result.keys.length > 1) {
                let pos = 0;
                result.keys.forEach((k) => {
                    if (this.paymentImportId !== k) {
                        this.openEdit(k, result.strings[pos]);
                    }
                    pos++;
                })
            }
        }, error => {

            this.isChooseFileButtonVisible = true;
            this.isUploadFileButtonVisible = false;

            completion.failed(error.message);
        });
    }

    private loadStatuses() {
        return [
            { value: ImportPaymentIOStatus.FullyPaid, label: this.terms["economy.import.payment.fullypaid"] },
            { value: ImportPaymentIOStatus.Match, label: this.terms["economy.import.payment.matched"] },
            { value: ImportPaymentIOStatus.Paid, label: this.terms["economy.import.payment.paid"] },
            { value: ImportPaymentIOStatus.PartlyPaid, label: this.terms["economy.import.payment.partly_paid"] },
            { value: ImportPaymentIOStatus.Rest, label: this.terms["economy.import.payment.rest"] },
            { value: ImportPaymentIOStatus.Unknown, label: this.terms["economy.import.payment.unknown"] },
            { value: ImportPaymentIOStatus.Error, label: this.terms["economy.import.payment.error"] }
        ];
    }

    private setBillingType(x: PaymentImportIODTO) {
        switch (x.type) {
            case TermGroup_BillingType.Debit:
                x.typeName = this.terms["economy.import.payment.debit"];
                break;
            case TermGroup_BillingType.Credit:
                x.typeName = this.terms["economy.import.payment.credit"];
                break;
            default:
        }
    }

    private setStatus(x: PaymentImportIODTO) {
        switch (x.status) {
            case ImportPaymentIOStatus.FullyPaid:
                x.statusName = this.terms["economy.import.payment.fullypaid"];
                break;
            case ImportPaymentIOStatus.Match:
                x.statusName = this.terms["economy.import.payment.matched"];
                break;
            case ImportPaymentIOStatus.Paid:
                x.statusName = this.terms["economy.import.payment.paid"];
                x['isSelectDisabled'] = true;
                break;
            case ImportPaymentIOStatus.PartlyPaid:
                x.statusName = this.terms["economy.import.payment.partly_paid"];
                break;
            case ImportPaymentIOStatus.Rest:
                x.statusName = this.terms["economy.import.payment.rest"];
                break;
            case ImportPaymentIOStatus.Unknown:
                x.statusName = this.terms["economy.import.payment.unknown"];
                break;
            case ImportPaymentIOStatus.Error:
                x.statusName = this.terms["economy.import.payment.error"];
                break;
            case ImportPaymentIOStatus.Manual:
                x.statusName = this.terms["core.manual"];
                break;
            case ImportPaymentIOStatus.Deleted:
                x.statusName = this.terms["economy.import.payment.deleted"];
                break;
            case ImportPaymentIOStatus.ManuallyHandled:
                x.statusName = this.terms["economy.import.payment.manualstatus"];
                break;
            default: 
        }

        if (x.state == ImportPaymentIOState.Closed)
            x['isSelectDisabled'] = true;
    }

    private getMatchCode(matchCodeId: number) {
        return _.find(this.matchCodesDict, p => p.matchCodeId == matchCodeId);
    }

    private setMatchCode(x: PaymentImportIODTO) {
        const matchCode = this.getMatchCode(x.matchCodeId);
        x.matchCodeName = matchCode?.name || "";
    }

    private gridRowSelected(row: any) {
        if (row) {
            this.selectedInvoiceId = row.invoiceId;
            if (this.isSupplierPayment) {
                this.supplierService.getInvoice(row.invoiceId, false, false, false).then((x) => {
                    this.invoice = x;
                });

                this.paymentImportIO = row;
            }
            else {
                this.paymentImportIO = row;
            }
        }
        else {
            this.selectedInvoiceId = undefined;
            this.invoice = null;
            if (this.isSupplierPayment) {
                this.paymentImportIO = null;
            }
        }
    }

    private beginCellEdit(row: PaymentImportIODTO, colDef: uiGrid.IColumnDef) {
        switch (colDef.field) {
            case 'invoiceNr':
                this.prevInvoiceNr = row.invoiceNr;
                break;
            case 'paidAmount':
                this.prevPaidAmount = row.paidAmount;
                break;
            case 'matchCodeId':
                this.prevMatchCodeId = row.matchCodeId;
                break;
        }
    }

    private afterCellEdit(row: PaymentImportIODTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;
        switch (colDef.field) {
            case 'invoiceNr':
                row.batchNr = this.paymentImport.batchId;
                this.updatePaymentImportIO(row, oldValue);
                break;
            case 'paidAmount':
                row.restAmount = row.invoiceAmount < 0 ? row.invoiceAmount + (row.paidAmount * -1) : row.invoiceAmount - row.paidAmount;
                this.gridpaymentOptions.gridAg.options.refreshRows(row);
                row.isModified = true;
                break;
            case 'matchCodeName':
                var id = Number(row.matchCodeName);
                var matchCode = this.getMatchCode(id);
                row.matchCodeId = matchCode.matchCodeId;
                row.matchCodeName = matchCode.name;
                row.isModified = true;
                //this.initChangeAmount(row);
                break;
            case 'paidDate':
                row.isModified = true;
                break;
        }

        row.isModified && this.dirtyHandler.setDirty();
    }

    private save() {
        if (!this.paymentImport.sysPaymentTypeId && this.selectedSysPaymentType) {
            this.setPaymentMetod();
            this.setPaymentMetodToImportDTO();
        }
        this.progress.startSaveProgress((completion) => {
            this.accountingService.savePaymentImportHeader(this.paymentImport).then(result => {
                if (result.success) {
                    const modifiedRows = this.paymentImportIODTOs.filter(x => x.isModified);

                    if (modifiedRows.length > 0) {
                        modifiedRows.forEach(r => {
                            r.batchNr = this.paymentImport.batchId;
                        })
                        this.savePaymentImportRowsAndReload(modifiedRows, completion);
                    } else {
                        completion.completed(Constants.EVENT_EDIT_SAVED, this.paymentImport);
                    }
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(() => {
            this.dirtyHandler.clean();
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deletePaymentImportIOInvoices(this.paymentImport.batchId, this.importPaymentTypeId == ImportPaymentType.CustomerPayment ? ImportPaymentType.CustomerPayment : ImportPaymentType.SupplierPayment).then(result => {
                if (result.success) {
                    completion.completed(null);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(() => {
            this.onLoadData();
            this.dirtyHandler.clean();
        });
    }

    private updatePaymentImportIO(row: PaymentImportIODTO, oldNumber: string) {
        this.accountingService.updatePaymentImportIO(row).then((result: IActionResult) => {
            if (row.paymentImportIOId === undefined || row.paymentImportIOId === 0) {
                if (result.value) {
                    const originalRowS = _.find(this.paymentImportIODTOs, r => r === row);
                    if (originalRowS) {
                        angular.extend(originalRowS, result.value);

                        //Set batchnr
                        originalRowS.batchNr = this.paymentImport.batchId;

                        this.setBillingType(originalRowS);
                    }
                    this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);
                }
                else if (result.errorMessage) {
                    this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);

                    const originalRowE = _.find(this.paymentImportIODTOs, r => r === row);
                    if (originalRowE) {
                        originalRowE.invoiceNr = oldNumber;
                        this.gridpaymentOptions.gridAg.setData(this.paymentImportIODTOs);
                    }
                }
            } else {
                let reloadAll = true;
                if (result.value) {
                    const originalRowS = _.find(this.paymentImportIODTOs, r => r === row);
                    if (originalRowS) {
                        angular.extend(originalRowS, result.value);
                        this.setStatusTexts(originalRowS);
                        this.gridpaymentOptions.gridAg.options.refreshRows(originalRowS);
                        reloadAll = false;
                    }
                }
                if (reloadAll) {
                    this.loadImportedIoInvoices();
                }
            }

        });
    }
}