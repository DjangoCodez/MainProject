import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IVoucherTraceViewDTO, IInvoiceTraceViewDTO, IOrderTraceViewDTO, IPaymentTraceViewDTO, IProjectTraceViewDTO, IAccountDistributionTraceViewDTO, IOfferTraceViewDTO, IContractTraceViewDTO } from "../../../Scripts/TypeLite.Net4";
import { EditController as CustomerInvoicesEditController } from "../../../Common/Customer/Invoices/EditController";
import { EditController as CustomerPaymentsEditController } from "../../../Common/Customer/Payments/EditController";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { Feature, SoeOriginType, SoeEntityState, OrderInvoiceRegistrationType, SoeReportTemplateType, ImportPaymentType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as AccountDistributionEditController } from "../../../Shared/Economy/Accounting/AccountDistribution/EditController";
import { EditController as SupplierPaymentsEditController } from "../../../Shared/Economy/Supplier/Payments/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as BillingProjectsEditController } from "../../../Shared/Billing/Projects/EditController";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingPurchaseEditController } from "../../../Shared/Billing/Purchase/Purchase/EditController";
import { EditController as BillingDeliveryEditController } from "../../../Shared/Billing/Purchase/Delivery/EditController";
import { EditController as InventoryEditController } from "../../../Shared/Economy/Inventory/Inventories/EditController";
import { EditController as CustomerPaymentImportEditController } from "../../../Shared/Economy/Import/Payments/EditController";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../Util/SoeGridOptions";

class TraceRowController {
    traceId: number;    // Common id eg. invoiceId, voucherheadId
    pageName: string;
    deleted: string;
    editPageName: string;
    editSupplierInvoicePermission: boolean;
    editCustomerInvoicePermission: boolean;
    editBillingInvoicePermission: boolean;
    editCustomerPaymentPermission: boolean;
    editSupplierPaymentPermission: boolean;
    editInventoryPermission: boolean;
    editVoucherPermission: boolean;
    editProjectPermission: boolean;
    editImportCustomerPaymentPermission: boolean;
    editOrderPermission: boolean;
    editAccountDistributionHeadPermission: boolean;
    editOfferPermission: boolean;
    editContractsPermission: boolean;
    editPurchasePermission: boolean;
    editPurchaseDeliveryPermission: boolean;
    terms: { [index: string]: string; };

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    // Flags
    progressBusy = false;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService) {

        const featureIds: number[] = [];
        featureIds.push(Feature.Economy_Supplier_Invoice_Invoices_Edit);
        featureIds.push(Feature.Billing_Invoice_Invoices_Edit);
        featureIds.push(Feature.Economy_Customer_Invoice_Invoices_Edit);
        featureIds.push(Feature.Economy_Customer_Payment_Payments_Edit);
        featureIds.push(Feature.Economy_Import_Payments);
        featureIds.push(Feature.Economy_Supplier_Payment_Payments_Edit);
        featureIds.push(Feature.Economy_Accounting_Vouchers_Edit);
        featureIds.push(Feature.Billing_Project_Edit);
        featureIds.push(Feature.Billing_Order_Orders_Edit);
        featureIds.push(Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod);
        featureIds.push(Feature.Billing_Offer_Offers_Edit);
        featureIds.push(Feature.Billing_Contract_Contracts_Edit);
        featureIds.push(Feature.Billing_Purchase_Purchase_Edit);
        featureIds.push(Feature.Billing_Purchase_Delivery_Edit);
        featureIds.push(Feature.Economy_Inventory_Inventories_Edit);

        this.coreService.hasReadOnlyPermissions(featureIds).then((data: number[]) => {
            if (data[Feature.Economy_Supplier_Invoice_Invoices_Edit]) {
                this.editSupplierInvoicePermission = true;
            }
            if (data[Feature.Economy_Customer_Invoice_Invoices_Edit]) {
                this.editCustomerInvoicePermission = true;
            }
            if (data[Feature.Billing_Invoice_Invoices_Edit]) {
                this.editBillingInvoicePermission = true;
            }
            if (data[Feature.Economy_Customer_Payment_Payments_Edit]) {
                this.editCustomerPaymentPermission = true;
            }
            if (data[Feature.Economy_Supplier_Payment_Payments_Edit]) {
                this.editSupplierPaymentPermission = true;
            }
            if (data[Feature.Economy_Inventory_Inventories_Edit]) {
                this.editInventoryPermission = true;
            }
            if (data[Feature.Economy_Accounting_Vouchers_Edit]) {
                this.editVoucherPermission = true;
            }
            if (data[Feature.Billing_Project_Edit]) {
                this.editProjectPermission = true;
            }
            if (data[Feature.Billing_Order_Orders_Edit]) {
                this.editOrderPermission = true;
            }
            if (data[Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod]) {
                this.editAccountDistributionHeadPermission = true;
            }
            if (data[Feature.Billing_Offer_Offers_Edit]) {
                this.editOfferPermission = true;
            }
            if (data[Feature.Billing_Contract_Contracts_Edit]) {
                this.editContractsPermission = true;
            }
            if (data[Feature.Billing_Purchase_Purchase_Edit]) {
                this.editPurchasePermission = true;
            }
            if (data[Feature.Billing_Purchase_Delivery_Edit]) {
                this.editPurchaseDeliveryPermission = true;
            }
            if (data[Feature.Economy_Import_Payments]) {
                this.editImportCustomerPaymentPermission = true;
            }
        });

        this.$scope.$on('reloadTracingData', (e, a) => {
            this.loadGridData();
        });
    }    

    public $onInit() {
        // Setup grid
        this.soeGridOptions = new SoeGridOptionsAg("TraceGrid", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;

        this.setupGrid();
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.traceId, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.loadGridData();
            }
        });
    }

    public setupGrid(): void {
        const keys: string[] = [
            "core.deleted",
            "core.warning",
            "common.type",
            "common.tracerows.status",
            "common.tracerows.invoicetype",
            "common.description",
            "common.number",
            "common.date",
            "common.tracerows.invoiceshow",
            "common.tracerows.invoiceshort",
            "common.tracerows.paymentshow",
            "common.tracerows.paymentshort",
            "common.tracerows.inventoryshow",
            "common.tracerows.inventoryshort",
            "economy.supplier.invoice.invoice",
            "economy.accounting.voucher.voucher",
            "economy.supplier.payment.payment",
            "common.showpdf",
            "common.pdferror",
            "economy.accounting.voucher.voucher",
            "economy.accounting.accountdistribution.accountdistribution"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.deleted = terms["core.deleted"];
            this.soeGridOptions.addColumnText(this.interfacePropertyToString((o: IVoucherTraceViewDTO) => o.originTypeName), terms["common.type"], null, { enableHiding: true });
            this.soeGridOptions.addColumnText(this.interfacePropertyToString((o: IVoucherTraceViewDTO) => o.originStatusName), terms["common.tracerows.status"], null, { enableHiding: true });
            this.soeGridOptions.addColumnText(this.interfacePropertyToString((o: IVoucherTraceViewDTO) => o.description), terms["common.description"], null, { enableHiding: true });
            this.soeGridOptions.addColumnText(this.interfacePropertyToString((o: IVoucherTraceViewDTO) => o.number), terms["common.number"], null, { enableHiding: true });
            this.soeGridOptions.addColumnDate(this.interfacePropertyToString((o: IVoucherTraceViewDTO) => o.date), terms["common.date"], null, true);

            //Add only one column for opening link
            //this.soeGridOptions.addColumnIcon(null, "fal fa-pencil iconEdit", terms["common.tracerows.show"], "openEdit", null, "showEdit", terms["common.tracerows.openEdit"], "45");
            this.soeGridOptions.addColumnIcon(null, " ", null, { onClick: this.openEdit.bind(this), icon: "fal fa-pencil iconEdit", showIcon: this.showEdit.bind(this), toolTip: terms["common.tracerows.openEdit"], suppressFilter: true });

            //Document kolumn - EDI
            //this.soeGridOptions.addColumnIcon(null, "fal fa-file-pdf", this.terms["common.showpdf"], "showPdf", "showPdfIcon", null, "", null, false);
            this.soeGridOptions.addColumnIcon(null, " ", null, { onClick: this.showPdf.bind(this), icon: "fal fa-file-pdf", showIcon: (row) => row.showPdfIcon, toolTip: terms["common.showpdf"], suppressFilter: true });

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.openEditDoubleClick(row); }));
            this.soeGridOptions.subscribe(events);

            this.soeGridOptions.finalizeInitGrid();

            //Load data
            this.loadGridData();
        });
    }

    showEdit(row) {
        if (row.state === SoeEntityState.Deleted) {
            this.setRowAsDeleted(row);
        } else if (row.state === SoeEntityState.Active && (row.isCustomerInvoice || (row.isInvoice && row.originType === SoeOriginType.CustomerInvoice))) {
            if ((this.editCustomerInvoicePermission || this.editBillingInvoicePermission)) {
                return true;
            }
        }
        else if (row.state === SoeEntityState.Active && (row.isSupplierInvoice || (row.isInvoice && row.originType === SoeOriginType.SupplierInvoice))) {
            if (this.editSupplierInvoicePermission)
                return true;
        }
        else if (row.state === SoeEntityState.Active && row.isPayment) {
            if ((row.originType === SoeOriginType.CustomerPayment && this.editCustomerPaymentPermission) || (row.originType === SoeOriginType.SupplierPayment && this.editSupplierPaymentPermission)) {
                return true;
            }
        } else if (row.state === SoeEntityState.Active && row.isInventory) {
            if (this.editInventoryPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && (row.isVoucher || row.isStockVoucher)) {
            if (this.editVoucherPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isProject) {
            if (this.editProjectPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isOrder) {
            if (this.editOrderPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isAccountDistribution) {
            if (this.editAccountDistributionHeadPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isOffer) {
            if (this.editOfferPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isContract) {
            if (this.editContractsPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isPurchase) {
            if (this.editPurchasePermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isDelivery) {
            if (this.editPurchaseDeliveryPermission)
                return true;
        } else if (row.state === SoeEntityState.Active && row.isImport) {
            if (this.editImportCustomerPaymentPermission)
                return true;
        }   else
            return false;
    }

    protected showPayment(row) {
        if (row.state === SoeEntityState.Deleted) {
            this.setRowAsDeleted(row);
        } else if (row.state === SoeEntityState.Active && row.isPayment) {
            if ((row.originType === SoeOriginType.CustomerPayment && this.editCustomerPaymentPermission) || (row.originType === SoeOriginType.SupplierPayment && this.editSupplierPaymentPermission)) {
                return true;
            }
        }
        return false;
    }

    protected openEditDoubleClick(row) {
        if (this.showEdit(row))
            this.openEdit(row)
    }

    protected openEdit(row) {
        if (row.isVoucher || row.isStockVoucher) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["economy.accounting.voucher.voucher"] + " " + row.number, row.voucherHeadId, VouchersEditController, { id: row.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
        } else if (row.isSupplierInvoice) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.supplierInvoiceId, SupplierInvoicesEditController, { id: row.supplierInvoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
        } else if (row.isInvoice && row.originType === SoeOriginType.SupplierInvoice) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.mappedInvoiceId ? row.mappedInvoiceId : row.invoiceId, SupplierInvoicesEditController, { id: row.mappedInvoiceId ? row.mappedInvoiceId : row.invoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
        }
        else if (row.isCustomerInvoice || (row.isInvoice && row.originType === SoeOriginType.CustomerInvoice)) {
            if (row.registrationType === OrderInvoiceRegistrationType.Ledger) {
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.isCustomerInvoice ? row.customerInvoicId : row.invoiceId, CustomerInvoicesEditController, { id: row.invoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html')));
            }
            else {
                let invoiceId = row.mappedInvoiceId ?? row.invoiceId ?? row.customerInvoiceId;
                var message = new TabMessage(
                    row.originTypeName + " " + row.number,
                    invoiceId,
                    BillingInvoicesEditController,
                    { id: invoiceId },
                    this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
            }
        }
        else if (row.isPayment && row.originType === SoeOriginType.SupplierPayment) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.paymentRowId, SupplierPaymentsEditController, { paymentId: row.paymentRowId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Payments/Views/edit.html')));
        } else if (row.isPayment && row.originType === SoeOriginType.CustomerPayment) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.paymentRowId, CustomerPaymentsEditController, { paymentId: row.paymentRowId }, this.urlHelperService.getGlobalUrl('Common/Customer/Payments/Views/edit.html')));
        } else if (row.isProject) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.projectId, BillingProjectsEditController, { id: row.projectId }, this.urlHelperService.getGlobalUrl('Billing/Projects/Views/edit.html')));
        } else if (row.isOrder) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.orderId, BillingOrdersEditController, { id: row.orderId, originType: SoeOriginType.Order, feature: Feature.Billing_Order_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
        } else if (row.isContract) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.contractId, BillingOrdersEditController, { id: row.contractId, originType: SoeOriginType.Contract, feature: Feature.Billing_Contract_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editContract.html')));
        } else if (row.isOffer) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.offerId, BillingOrdersEditController, { id: row.offerId, originType: SoeOriginType.Offer, feature: Feature.Billing_Offer_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html')));
        } else if (row.isAccountDistribution) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.accountDistributionName, row.accountDistributionHeadId, AccountDistributionEditController, { id: row.accountDistributionHeadId, accountDistributionType: 'Period' }, this.urlHelperService.getGlobalUrl('Shared/Economy/Accounting/AccountDistribution/Views/edit.html')));
        } else if (row.isPurchase) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.purchaseId, BillingPurchaseEditController, { id: row.purchaseId }, this.urlHelperService.getGlobalUrl('Billing/Purchase/Purchase/Views/edit.html')));
        } else if (row.isDelivery) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.purchaseDeliveryId, BillingDeliveryEditController, { id: row.purchaseDeliveryId }, this.urlHelperService.getGlobalUrl('Billing/Purchase/Delivery/Views/edit.html')));
        } else if (row.isInventory) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.purchaseDeliveryId, InventoryEditController, { id: row.inventoryId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Inventory/Inventories/Views/edit.html')));
        } else if (row.isImport) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.number, CustomerPaymentImportEditController, { id: row.paymentImportId, importType: ImportPaymentType.CustomerPayment, feature: Feature.Economy_Import_Payments }, this.urlHelperService.getGlobalUrl('Shared/Economy/Import/Payments/Views/edit.html')));
        }
    }

    protected showPdf(row) {
        if (row.ediHasPdf) {
            var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + row.ediEntryId;
            window.open(ediPdfReportUrl, '_blank');
        }
        else {
            this.coreService.generateReportForEdi([row.ediEntryId]).then((result) => {
                if (result.success) {
                    var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + row.ediEntryId;
                    window.open(ediPdfReportUrl, '_blank');
                }
            }, error => {
                this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["common.pdferror"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }
    }

    protected showInventory(row) {
        if (row.state === SoeEntityState.Deleted) {
            this.setRowAsDeleted(row);
        } else if (row.state === SoeEntityState.Active && row.isInventory) {
            if (this.editInventoryPermission)
                return true;
        }
        return false;
    }

    public edit(row) { }

    public loadGridData() {
        if (!this.traceId)
            return;

        this.progressBusy = true;

        //Depending where we are, different data should be loaded
        if (this.pageName === "voucherEdit")
            this.coreService.getVoucherTraceViews(this.traceId).then((data) => {
                _.forEach(data, (y) => {
                    if (y.isAccountDistribution)
                        y.originTypeName = this.terms["economy.accounting.accountdistribution.accountdistribution"];
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data);
            });
        else if (this.pageName === "supplierInvoiceEdit" || this.pageName === "customerInvoiceEdit")
            this.coreService.getInvoiceTraceViews(this.traceId).then((data: IInvoiceTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = CalendarUtility.convertToDate(y.date);

                    if (y.isInventory)
                        y.originStatusName = y.inventoryStatusName;
                    else if (y.isPayment)
                        y.originStatusName = y.paymentStatusName;
                    else if (y.isAccountDistribution) {
                        y.originTypeName = this.terms["economy.accounting.accountdistribution.accountdistribution"];
                    }

                    if (y.ediEntryId)
                        y['showPdfIcon'] = true;
                    else
                        y['showPdfIcon'] = false;
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data);
            });
        else if (this.pageName === "orderEdit")
            this.coreService.getOrderTraceViews(this.traceId).then((data: IOrderTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = CalendarUtility.convertToDate(y.date);

                    // TODO: Fix any cast... Missing in export interface declaration
                    if ((<any>y).isInventory)
                        y.originStatusName = (<any>y).inventoryStatusName;
                    else if ((<any>y).isPayment)
                        y.originStatusName = (<any>y).paymentStatusName;

                    if (y.ediEntryId)
                        y['showPdfIcon'] = true;
                    else
                        y['showPdfIcon'] = false;
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data);
            });
        else if (this.pageName === "paymentEdit")
            this.coreService.getPaymentTraceViews(this.traceId).then((data: IPaymentTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = <any>CalendarUtility.toFormattedDate(y.date);
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data);
            });
        else if (this.pageName === "projectEdit") {
            this.coreService.getProjectTraceViews(this.traceId).then((data: IProjectTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = <any>CalendarUtility.toFormattedDate(y.date);
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data);
            });
        }
        else if (this.pageName === "accountDistributionEdit")
            this.accountingService.getAccountDistributionTraceViews(this.traceId).then((data: IAccountDistributionTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = <any>CalendarUtility.toFormattedDate(y.date);
                    if (y.isVoucher)
                        y.originTypeName = this.terms["economy.accounting.voucher.voucher"];
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data); 
            });
        else if (this.pageName === "offerEdit")
            this.coreService.getOfferTraceViews(this.traceId).then((data: IOfferTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = CalendarUtility.convertToDate(y.date);

                    // TODO: Fix any cast... Missing in export interface declaration
                    if ((<any>y).isInventory)
                        y.originStatusName = (<any>y).inventoryStatusName;
                    else if ((<any>y).isPayment)
                        y.originStatusName = (<any>y).paymentStatusName;
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data); 
            });
        else if (this.pageName === "contractEdit")
            this.coreService.getContractTraceViews(this.traceId).then((data: IContractTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = CalendarUtility.convertToDate(y.date);

                    // TODO: Fix any cast... Missing in export interface declaration
                    if ((<any>y).isInventory)
                        y.originStatusName = (<any>y).inventoryStatusName;
                    else if ((<any>y).isPayment)
                        y.originStatusName = (<any>y).paymentStatusName;
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data); 
            });
        else if (this.pageName === "purchaseEdit")
            this.coreService.getPurchaseTraceViews(this.traceId).then((data: IContractTraceViewDTO[]) => {
                _.forEach(data, (y) => {
                    y.date = CalendarUtility.convertToDate(y.date);
                });

                this.progressBusy = false;
                this.soeGridOptions.setData(data);
            });
    }

    interfacePropertyToString = (property: (object: any) => void) => {
        var chaine = property.toString();
        var arr = chaine.match(/[\s\S]*{[\s\S]*\.([^\.; ]*)[ ;\n]*}/);
        return arr[1];
    };

    setRowAsDeleted(row: IInvoiceTraceViewDTO) {
        if (row.originTypeName.indexOf("*") === -1) {
            row.originTypeName += " *";
        }
    }
}

//@ngInject
export function traceRowsDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: 'E',
        templateUrl: urlHelperService.getCommonDirectiveUrl('TraceRows', 'TraceRows.html'),
        scope: {
            traceId: "=",
            pageName: "=",
        },
        replace: true,
        controller: TraceRowController,
        controllerAs: "ctrl",
        bindToController: true
    }
}