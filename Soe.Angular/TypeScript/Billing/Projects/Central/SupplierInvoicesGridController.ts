import angular from "angular";
import { ShowAttestCommentsAndAnswersDialogController } from "../../../Common/Dialogs/ShowAttestCommentsAndAnswersDialog/ShowAttestCommentsAndAnswersDialogController";
import { ShowPdfController } from "../../../Common/Dialogs/ShowPdf/ShowPdfController";
import { SupplierInvoiceGridDTO } from "../../../Common/Models/InvoiceDTO";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { Feature, SoeDataStorageRecordType, SoeModule, SoeOriginType, SoeReportTemplateType, SoeStatusIcon, TermGroup_AttestEntity, TermGroup_BillingType, TermGroup_EDISourceType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups
    items: SupplierInvoiceGridDTO[];

    //Project central
    projectId: number;
    includeChildProjects: boolean;
    invoices: number[];
    fromDate: Date;
    toDate: Date;

    // Grid header and footer
    gridFooterComponentUrl: any;

    //modal
    private modalInstance: any;
    hasCurrencyPermission: boolean;
    activated: boolean;
    doReload: boolean;
    hasAttestFlowPermission: boolean;


    //Terms
    terms: { [index: string]: string; };
    noAttestStateTerm: string;
    attestRejectedTerm: string;
    attestStates: any[];
    invoiceJournalReportId = 0;

    filteredTotal = 0;
    filteredTotalLinkedToProject = 0;
    filteredTotalLinkedToOrder = 0;
    filteredTotalLinkedToOrderSale = 0;
    filteredTotalLinkedCost = 0;
    surchargePercentage = 0;

    private isSupplierInvoiceJournalPrinting = false;

    //@ngInject
    constructor(
        private $window,
        $uibModal,
        private coreService: ICoreService,
        private supplierService: ISupplierService,
        private accountingService: IAccountingService,
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

        super(gridHandlerFactory, "Billing.Project.Central.SupplierInvoices", progressHandlerFactory, messagingHandlerFactory);
        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.initSetupGrid())
            .onLoadGridData(() => this.loadGridDataForProjectCentral());

        this.onTabActivetedAndModified(() => this.loadGridDataForProjectCentral());
        this.messagingHandler.onTabActivated((tabGuid) => this.onControlActivated(tabGuid));
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;
        this.init();
    }

    private getPermissions(): any[] {
        const features: any[] = [];
        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow, loadReadPermissions: true, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: true });
        return features;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Supplier_Invoice_Invoices].readPermission || response[Feature.Economy_Supplier_Invoice_Invoices].modifyPermission;
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission || response[Feature.Economy_Supplier_Invoice_Invoices_Edit].readPermission;
        this.hasAttestFlowPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow].readPermission || response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(
            <IGridHandler>this.gridAg,
            this.loadGridDataForProjectCentral.bind(this));
    }

    public onControlActivated(tabGuid: any) {
        if (tabGuid !== this.guid)
            return;

        if (!this.activated) {
            this.flowHandler.start(this.getPermissions());
            this.activated = true;
        }
        else if (this.doReload) {
            this.loadGridDataForProjectCentral();
            this.doReload = false;
        }
    }

    protected init() {
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("supplierInvoiceGridFooter.html");

        this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
            this.projectId = x.projectId;
            this.includeChildProjects = x.includeChildProjects;
            this.fromDate = x.fromDate;
            this.toDate = x.toDate;

            if (this.activated)
                this.doReload = true;
        });
    }

    protected initSetupGrid() {
        this.setupToolbar();
        this.setupInvoiceGrid();
    }

    private getSelectedIds() {
        return this.gridAg.options
            .getSelectedRows()
            .filter(r => r.hasPDF)
            .map(r => r.supplierInvoiceId);
    }
    protected setupToolbar() {
        if (this.toolbar) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.downloadinvoiceimages", "economy.supplier.invoice.downloadinvoiceimages", IconLibrary.FontAwesome, "fa-download",
                () => {
                    this.downloadPdfs(this.getSelectedIds());
                }, () => {
                    return this.gridAg.options.getSelectedCount() === 0;
                })));
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "economy.supplier.invoice.liquidityplanning.sequencenr",
            "core.edit",
            "common.imported",
            "economy.supplier.invoice.seqnr",
            "economy.supplier.invoice.invoicenr",
            "economy.supplier.invoice.invoicetype",
            "common.tracerows.status",
            "economy.supplier.supplier.suppliernr.grid",
            "economy.supplier.supplier.suppliername.grid",
            "economy.supplier.invoice.amountexvat",
            "economy.supplier.invoice.amountincvat",
            "economy.supplier.invoice.remainingamount",
            "economy.supplier.invoice.invoicedate",
            "economy.supplier.invoice.duedate",
            "common.customer.invoices.paydate",
            "economy.supplier.invoice.attest",
            "economy.supplier.invoice.attestname",
            "economy.supplier.invoice.noatteststate",
            "economy.supplier.invoice.attestrejected",
            "economy.supplier.invoice.sumlinkedtoorder",
            "economy.supplier.invoice.sumlinkedtoproject",
            "economy.supplier.invoice.openpdf",
            "common.reason",
            "economy.supplier.invoice.paidshort",
            "economy.supplier.invoice.paidbutnotcheckedshort",
            "economy.supplier.invoice.partlypaidshort",
            "economy.supplier.invoice.unpaidshort",
            "core.attestflowregistered",
            "economy.supplier.invoice.sumlinkedtoordercost",
            "economy.supplier.invoice.description",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.noAttestStateTerm = this.terms["economy.supplier.invoice.noatteststate"];
            this.attestRejectedTerm = this.terms["economy.supplier.invoice.attestrejected"];
        });
    }

    public setupInvoiceGrid() {
        this.gridAg.addColumnNumber("seqNr", this.terms["economy.supplier.invoice.liquidityplanning.sequencenr"], null, { alignLeft: true, formatAsText: true });
        this.gridAg.addColumnText("invoiceNr", this.terms["economy.supplier.invoice.invoicenr"], null);
        this.gridAg.addColumnSelect("billingTypeName", this.terms["economy.supplier.invoice.invoicetype"], null, { enableHiding: true, displayField: "billingTypeName", selectOptions: [], hide: true });

        this.gridAg.addColumnSelect("statusName", this.terms["common.tracerows.status"], null, { enableHiding: true, displayField: "statusName", selectOptions: [], hide: true });

        this.gridAg.addColumnText("supplierNr", this.terms["economy.supplier.supplier.suppliernr.grid"], null, true, { enableHiding: true, hide: true, enableRowGrouping: true });
        this.gridAg.addColumnText("supplierName", this.terms["economy.supplier.supplier.suppliername.grid"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("internalText", this.terms["economy.supplier.invoice.description"], null, true, { enableHiding: true });
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["economy.supplier.invoice.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("totalAmount", this.terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: true, decimals: 2, hide: true });
        this.gridAg.addColumnNumber("payAmount", this.terms["economy.supplier.invoice.remainingamount"], null, { enableHiding: true, decimals: 2, hide: true });

        this.gridAg.addColumnDate("invoiceDate", this.terms["economy.supplier.invoice.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["economy.supplier.invoice.duedate"], null, true);
        this.gridAg.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null, true, null, { hide: true });

        if (this.hasAttestFlowPermission) {
            this.gridAg.addColumnSelect("attestStateName", this.terms["economy.supplier.invoice.attest"], null, { enableHiding: true, displayField: "attestStateName", selectOptions: this.attestStates, hide: true });
            this.gridAg.addColumnText("currentAttestUserName", this.terms["economy.supplier.invoice.attestname"], null, true, { hide: true });
        }

        this.gridAg.addColumnNumber("projectInvoicedAmount", this.terms["economy.supplier.invoice.sumlinkedtoordercost"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("projectInvoicedSalesAmount", this.terms["economy.supplier.invoice.sumlinkedtoorder"], null, { enableHiding: true, decimals: 2, hide: true });
        this.gridAg.addColumnNumber("projectAmount", this.terms["economy.supplier.invoice.sumlinkedtoproject"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-comment-dots", showIcon: this.showAttestCommentIcon.bind(this), onClick: this.showAttestCommentDialog.bind(this) });
        this.gridAg.addColumnIcon("blockIcon", null, null, { onClick: this.showBlockReason.bind(this), toolTipField: "blockReason" });
        this.gridAg.addColumnIcon("paymentStatusIcon", null, null, { suppressSorting: false, enableHiding: true, toolTipField: "paymentStatusTooltip", showTooltipFieldInFilter: true });
        this.gridAg.addColumnIcon("pdfIcon", null, null, { icon: "fal fa-file-search", onClick: this.openPicture.bind(this), showIcon: this.showPdfIcon.bind(this), toolTip: this.terms["economy.supplier.invoice.openpdf"] });

        if (this.modifyPermission) {
            this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));
        }

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
        });

        // Subscribe to grid events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: SupplierInvoiceGridDTO[]) => { this.summarizeFiltered(rows); }));
        this.gridAg.options.subscribe(events);

        /**/

        this.gridAg.finalizeInitGrid("economy.supplier.invoice.invoice", true);
    }

    public loadGridDataForProjectCentral() {
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getSupplierInvoicesForProjectCentral(this.projectId, this.includeChildProjects, this.fromDate, this.toDate, this.invoices)
                .then((data) => {
                    this.items = data;

                    for (const y of this.items) {
                        y.payDate = y.payDate ? new Date(<any>y.payDate).date() : null;
                        y.invoiceDate = y.invoiceDate ? new Date(<any>y.invoiceDate).date() : null;
                        y.dueDate = y.dueDate ? new Date(<any>y.dueDate).date() : null;
                        y.voucherDate = y.voucherDate ? new Date(<any>y.voucherDate).date() : null;

                        if (!y.attestStateName)
                            y.attestStateName = this.noAttestStateTerm;

                        if (y.isAttestRejected)
                            y.attestStateName = this.attestRejectedTerm;

                        y.expandableDataIsLoaded = false;
                        this.setInformationIconAndTooltip(y);
                    }
                });
        }]).then(() => {
            this.setData(this.items);
        });
    }

    public loadAttestStates(): ng.IPromise<any> {
        return this.supplierService.getAttestStates(TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false).then((attestStates) => {
            this.attestStates = [];
            this.attestStates.push({ value: this.noAttestStateTerm, label: -100 });
            this.attestStates.push({ value: this.attestRejectedTerm, label: -200 });
            attestStates.forEach((state: any) => {
                this.attestStates.push({ value: state.name, label: state.attestStateId });
            });
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
            invoice.blockIcon = "fal fa-lock-alt errorColor";

        if (invoice.hasPDF)
            invoice.pdfIcon = "fal fa-file-pdf";
    }


    public setInformationIconAndTooltip(item: SupplierInvoiceGridDTO) {
        this.setPaymentStatusIcon(item);
        const flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
        const statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(item.statusIcon);

        if (statusIcons.contains(SoeStatusIcon.Imported)) {
            item.infoIconValue = "fal fa-download";
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

    protected showPdfIcon(row) {
        if (row.hasPDF === true || row.ediType === TermGroup_EDISourceType.Finvoice)
            return true;
        else
            return false;
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
                registeredTerm: () => { return this.terms["core.attestflowregistered"] }
            }
        });
    }


    public openPicture(row: SupplierInvoiceGridDTO) {

        this.supplierService.getSupplierInvoiceImage(row.supplierInvoiceId).then((invoiceImage) => {
            if (invoiceImage) {
                if (invoiceImage.imageFormatType === SoeDataStorageRecordType.InvoicePdf) {
                    var modal = this.modalInstance.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowPdf/ShowPdf.html"),
                        controller: ShowPdfController,
                        controllerAs: 'ctrl',
                        backdrop: 'static',
                        size: 'lg',
                        resolve: {
                            pdf: () => { return invoiceImage.image },
                            storageRecordId: () => { return undefined },
                            invoiceId: () => { return row.supplierInvoiceId },
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
                            image: () => invoiceImage,
                            ediType: () => null,
                            ediEntryId: () => null,
                            scanningEntryId: () => null
                        }
                    }
                    this.modalInstance.open(options);

                }
            }
        });
    }

    private downloadPdfs(rows: number[]) {
        const imageUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SupplierInvoiceImage + "&c=" + CoreUtility.actorCompanyId + "&invoiceIds=" + rows;
        window.open(imageUrl, '_blank');
    }

    private showBlockReason(row: SupplierInvoiceGridDTO) {
        this.notificationService.showDialog(this.terms["common.reason"], row.blockReason, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private summarizeFiltered(rows: SupplierInvoiceGridDTO[]) {
        this.filteredTotal = 0;
        this.filteredTotalLinkedToProject = 0;
        this.filteredTotalLinkedToOrder = 0;
        this.filteredTotalLinkedToOrderSale = 0;

        rows.forEach(r => {
            this.filteredTotal += r.totalAmountExVat;
            this.filteredTotalLinkedToProject += r.projectAmount;
            this.filteredTotalLinkedToOrder += r.projectInvoicedAmount;
            this.filteredTotalLinkedToOrderSale += r.projectInvoicedSalesAmount;
        })

        this.surchargePercentage = this.filteredTotalLinkedToOrder > 0 ?
            (this.filteredTotalLinkedToOrderSale / this.filteredTotalLinkedToOrder * 100) - 100 : 0;
        this.surchargePercentage = Math.round(this.surchargePercentage * 100) / 100;
        this.filteredTotalLinkedCost = this.filteredTotalLinkedToOrder + this.filteredTotalLinkedToProject;
    }

    public edit(row: SupplierInvoiceGridDTO) {
        if (this.modifyPermission)
            this.messagingService.publish(Constants.EVENT_OPEN_PROJECTCENTRAL, {
                row: {
                    associatedId: row.supplierInvoiceId,
                    invoiceNr: row.invoiceNr,
                    originType: SoeOriginType.SupplierInvoice
                }
            });
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