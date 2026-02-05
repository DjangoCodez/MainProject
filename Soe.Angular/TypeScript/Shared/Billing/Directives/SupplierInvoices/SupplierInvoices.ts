import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { Feature, TermGroup, TermGroup_BillingType, SupplierInvoiceOrderLinkType, SoeDataStorageRecordType, TermGroup_AttestEntity, SoeModule } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { Guid } from "../../../../Util/StringUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IOrderService } from "../../Orders/OrderService";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IColumnAggregations } from "../../../../Util/SoeGridOptionsAg";
import { SupplierInvoiceOrderGridDTO } from "../../../../Common/Models/SupplierInvoiceOrderGridDTO";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { EditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { AttestStateDTO } from "../../../../Common/Models/AttestStateDTO";

class OrderSupplierInvoicesController extends GridControllerBase2Ag implements ICompositionGridController {
    guid: Guid;
    readOnly: boolean;
    projectId?: number;
    customerInvoiceId: number;   

    // Lookups
    terms: { [index: string]: string; };
    invoiceEditPermission: boolean;
    invoiceBillingTypes: any[];
    attestStates: AttestStateDTO[];

    // Rows
    supplierInvoices: SupplierInvoiceOrderGridDTO[];

    // Flags
    progressBusy: boolean = true;

    //@ngInject
    constructor(
        private $window,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private orderService: IOrderService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Common.Directives.SupplierInvoices", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))  
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        this.flowHandler.start([
            { feature: Feature.Billing_Order_SupplierInvoices, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Order_SupplierInvoices].readPermission;
        this.modifyPermission = response[Feature.Billing_Order_SupplierInvoices].modifyPermission;
        this.invoiceEditPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].readPermission || response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission;
        if (this.modifyPermission) {
            // Send messages to TabsController
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private doLookups(): ng.IPromise<any> {
        this.supplierInvoices = [];
        return this.$q.all([
            this.loadInvoiceBillingTypes(),
            this.loadSupplierInvoicesForOrder(),
            this.loadAttestStates()
        ]).then(() => {
            this.handleRows();
        });
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then((x) => {
            this.invoiceBillingTypes = [];
            _.forEach(x, (row) => {
                if (row.id < 3)
                    this.invoiceBillingTypes.push({ id: row.id, value: row.name });
            });
        });
    }

    private loadAttestStates() {
        return this.coreService.getAttestStates(TermGroup_AttestEntity.Order, SoeModule.Billing, false).then((x) => {
            this.attestStates = x;
        })
    }

    private loadSupplierInvoicesForOrder(): ng.IPromise<any> {
        return this.orderService.getSupplierInvoicesItemsForOrder(this.customerInvoiceId, !this.projectId ? 0 : this.projectId).then((x) => {
            this.supplierInvoices = x.map(supp => {
                const attestState = this.attestStates.find(a => a.attestStateId === supp.customerInvoiceRowAttestStateId);
                if (attestState) {
                    supp.customerInvoiceRowAttestStateColor = attestState.color;
                    supp.customerInvoiceRowAttestStateName = attestState.name;
                }
                return supp;
            })
        });
    }

    public setupGrid(): void {
        const translationKeys: string[] = [
            "common.customer.invoices.seqnr",
            "economy.supplier.supplier.suppliernrshort",
            "economy.supplier.supplier.supplier",
            "economy.supplier.invoice.liquidityplanning.sequencenr",
            "economy.supplier.invoice.invoicenr",
            "economy.supplier.invoice.invoicetype",
            "economy.supplier.invoice.amountexvat",
            "economy.supplier.invoice.amountincvat",
            "common.openinvoice",
            "economy.supplier.invoice.includeimage",
            "common.supplierinvoice",
            "billing.order.linkedtoorder",
            "billing.order.linkedtoproject",
            "common.customer.invoices.transferedfromedi",
            "billing.order.transferedtotalamount",
            "billing.order.supplierinvoiceamountexvat",
            "common.download",
            "common.status",
            "common.customer.invoices.invoicedate",
            "billing.order.targetcustomerinvoicenr",
            "billing.order.targetcustomerinvoicedate"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.options.setMinRowsToShow(10);

            this.gridAg.addColumnIcon("icon", "...", null, { toolTipField: "iconToolTip" });
            this.gridAg.addColumnText("supplierNr", this.terms["economy.supplier.supplier.suppliernrshort"], null, true);
            this.gridAg.addColumnText("supplierName", this.terms["economy.supplier.supplier.supplier"], null, true);
            this.gridAg.addColumnText("seqNr", this.terms["common.customer.invoices.seqnr"], null, true);
            this.gridAg.addColumnText("invoiceNr", this.terms["economy.supplier.invoice.invoicenr"], null);
            this.gridAg.addColumnSelect("billingTypeName", this.terms["economy.supplier.invoice.invoicetype"], null, { enableHiding: true, displayField: "billingTypeName", selectOptions: this.invoiceBillingTypes });
            this.gridAg.addColumnNumber("amount", this.terms["billing.order.transferedtotalamount"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("invoiceAmountExVat", this.terms["billing.order.supplierinvoiceamountexvat"], null, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null, true, null, { enableHiding: true, hide: true })
            this.gridAg.addColumnBoolEx("includeImageOnInvoice", this.terms["economy.supplier.invoice.includeimage"], 100, { disabledField: "checkBoxLocked", enableEdit: true, onChanged: this.updateSupplierInvoiceOrderImage.bind(this), toolTip: this.terms["economy.supplier.invoice.includeimage"] });
            this.gridAg.addColumnText("targetCustomerInvoiceNr", this.terms["billing.order.targetcustomerinvoicenr"], null, true, { enableHiding: true, hide: true });
            this.gridAg.addColumnDate("targetCustomerInvoiceDate", terms["billing.order.targetcustomerinvoicedate"], null, true, null, { enableHiding: true, hide: false })
            this.gridAg.addColumnShape("customerInvoiceRowAttestStateColor", null, 30, { shape: Constants.SHAPE_CIRCLE, toolTipField: "customerInvoiceRowAttestStateName", showIconField: "customerInvoiceRowAttestStateColor", enableHiding: true });

            this.gridAg.addColumnIcon("documentIcon", null, null, { toolTip: this.terms["common.download"], onClick: this.showImage.bind(this), showIcon: (row) => row.hasImage });
            this.gridAg.addColumnEdit(this.terms["common.openinvoice"], this.edit.bind(this));

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));

            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("billing.project.central.supplierinvoices", true);

            this.$timeout(() => {
                this.gridAg.options.addFooterRow("#expense-row-grid-sum-footer", {
                    "amount": "sum",
                    "invoiceAmountExVat": "sum",
                } as IColumnAggregations);
            });
        });
    }

    private handleRows() {
        _.forEach(this.supplierInvoices, (y: SupplierInvoiceOrderGridDTO) => {
            if (y.billingType === TermGroup_BillingType.Credit)
                y['billingTypeName'] = _.find(this.invoiceBillingTypes, { 'id': 2 }).value;
            else if (y.billingType === TermGroup_BillingType.Debit)
                y['billingTypeName'] = _.find(this.invoiceBillingTypes, { 'id': 1 }).value;

            switch (y.supplierInvoiceOrderLinkType) {
                case SupplierInvoiceOrderLinkType.LinkToOrder:
                    y["icon"] = "fal fa-file-import";
                    y["iconToolTip"] = this.terms["billing.order.linkedtoorder"];
                    break;
                case SupplierInvoiceOrderLinkType.LinkToProject:
                    y["icon"] = "fal fa-link";
                    y["iconToolTip"] = this.terms["billing.order.linkedtoproject"];
                    break;
                case SupplierInvoiceOrderLinkType.Transfered:
                    y["icon"] = "fal fa-file-invoice-dollar";
                    y["iconToolTip"] = this.terms["common.customer.invoices.transferedfromedi"];
                    break;
            }
            y['checkBoxLocked'] = !y.hasImage;
            y['documentIcon'] = y.hasImage ? "fal fa-file-pdf" : "";
            if (!y.hasImage)
                y.includeImageOnInvoice = false;
        }); 

        this.gridAg.setData(this.supplierInvoices);
        this.progressBusy = false;
    }

    private showImage(row) {
        HtmlUtility.openInSameTab(this.$window,`/ajax/downloadTextFile.aspx?table=invoiceimage&id=${row.supplierInvoiceId}&cid=${CoreUtility.actorCompanyId}&nr=${row.invoiceNr}&type=${SoeDataStorageRecordType.InvoicePdf}`);
    }

    public updateSupplierInvoiceOrderImage(row) {
        var item: SupplierInvoiceOrderGridDTO = row.data;
        this.orderService.updateOrderSupplierInvoiceImage(item.supplierInvoiceOrderLinkType === SupplierInvoiceOrderLinkType.LinkToProject ? item.timeCodeTransactionId : item.customerInvoiceRowId, item.supplierInvoiceOrderLinkType, item.includeImageOnInvoice).then((x) => {
            if (x.success)
                this.messagingService.publish(Constants.EVENT_RELOAD_ORDER_IMAGES, { guid: this.guid });
        });
    }

    public edit(row: SupplierInvoiceOrderGridDTO) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.supplierinvoice"] + " " + row.invoiceNr, row.supplierInvoiceId, EditController, { id: row.supplierInvoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
    }

    private reloadData() {
        this.progressBusy = true;
        this.supplierInvoices = [];
        this.gridAg.setData(null);

        return this.$q.all([
            this.loadSupplierInvoicesForOrder()
        ]).then(() => {
            this.handleRows();
        });
    }
}

export class OrderSupplierInvoicesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Billing/Directives/SupplierInvoices/SupplierInvoices.html"),
            scope: {
                guid: "=",
                readOnly: "=",
                projectId: "=?",
                customerInvoiceId: "=",
            },
            restrict: 'E',
            replace: true,
            controller: OrderSupplierInvoicesController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}