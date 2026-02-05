import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { GridController } from "../Invoices/GridController";
import { EditController } from "./EditController";
import { EditController as InvoiceEditController } from "../Invoices/EditController";
import { EditController as BillingInvoiceEditController } from "../../../Shared/Billing/Invoices/EditController";
import { Feature, SoeModule, SoeOriginStatusClassification, OrderInvoiceRegistrationType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { EditController as CustomerPaymentsEditController } from "../../../Common/Customer/Payments/EditController";
import { PaymentRowDTO } from "../../Models/PaymentRowDTO";
export class TabsController implements ICompositionTabsController {

    protected terms: any;
    protected hasUnpaidPermission: boolean;
    protected hasIntrestReminderPermission: boolean;
    protected hasPaidPermission: boolean;
    protected hasVoucherPermission: boolean;

    //@ngInject
    constructor(tabHandlerFactory: ITabHandlerFactory,
        protected urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private coreService: ICoreService, private $timeout: ng.ITimeoutService, private $window) {

        const part: string = "common.customer.payment.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.customerInvoiceId)
            .onGetRowEditName(row => {
                if (row instanceof PaymentRowDTO) {
                    if (!row.isSeqNrTabLabelDisplay) {
                        return row.seqNr;
                    }
                    return "";
                }
                return row.invoiceNr ? row.invoiceNr : this.terms["economy.supplier.invoice.preliminary"];
            })
            .onSetupTabs(() => { this.setupTabs(); })
            .onEdit(row => this.edit(row))
            .initialize(part + "payment", part + "payments", part + "newpayment");

        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            if (x)
                this.editInvoice(x.row);
        });
    }

    private $onInit() {
        const paymentIdStr = HtmlUtility.getQueryParameterByName(this.$window.location, "paymentId");
        const seqNr = HtmlUtility.getQueryParameterByName(this.$window.location, "seqNr");
        var paymentId = parseInt(paymentIdStr);
        if (paymentIdStr && paymentId && seqNr) {            
            this.$timeout(() => {               
                const row = new PaymentRowDTO();
                row.paymentId = paymentId;
                row.isSeqNrTabLabelDisplay = false;
               
                const templateUrl = this.urlHelperService.getGlobalUrl("Common/Customer/Payments/Views/edit.html");
                this.tabs.addEditTab(row, CustomerPaymentsEditController, { paymentId: paymentId }, templateUrl, this.terms['common.customer.payment.payment'] + ' ' + seqNr, true);
            }, 1000);
        }
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.tabs.enableRemoveAll();
            this.tabs.enableAddTab(() => this.add());
            // NOTICE! The gridcontroller that are used is the invoices gridcontroller (and not the payments gridcontroller)
            var activateTab = true;
            if (this.hasUnpaidPermission) {
                this.tabs.addNewTab(this.terms["common.customer.invoices.unpaid"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerPaymentsUnpayed, isHomeTab: true, setup: true }, false, activateTab);
                activateTab = false;
            }
            if (this.hasIntrestReminderPermission && soeConfig.module === SoeModule.Economy) {
                this.tabs.addNewTab(this.terms["common.customer.invoices.reminder"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerInvoicesReminder, isHomeTab: true }, false, activateTab);
                this.tabs.addNewTab(this.terms["common.customer.invoices.intrest"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerInvoicesInterest, isHomeTab: true }, false, activateTab);
                activateTab = false;
            }
            if (this.hasPaidPermission && soeConfig.module === SoeModule.Economy) {
                this.tabs.addNewTab(this.terms["common.customer.payment.paid"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerPaymentsPayed, isHomeTab: true }, false, activateTab);
                activateTab = false;
            }
            if (this.hasVoucherPermission && soeConfig.module === SoeModule.Economy) {
                this.tabs.addNewTab(this.terms["common.customer.payment.paidvoucher"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerPaymentsVoucher, isHomeTab: true }, false, activateTab);
                activateTab = false;
            }
        });
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        this.tabs.addEditTab(row, EditController );
    }

    protected editInvoice(row: any) {
        if (row.registrationType === OrderInvoiceRegistrationType.Ledger) {
            var activeLedgerTab = this.tabs.getTabByIdentifier(row.customerInvoiceId); 
            if (activeLedgerTab) {
                this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeLedgerTab));
            } else {
                this.tabs.addEditTab(row, InvoiceEditController, { id: row.customerInvoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html'), this.terms["common.customer.invoices.invoice"]);
            }
        }
        else {
            var activeInvoiceTab = this.tabs.getTabByIdentifier(row.customerInvoiceId); 
            if (activeInvoiceTab) {
                this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeInvoiceTab));
            } else {
                this.tabs.addEditTab(row, BillingInvoiceEditController, { id: row.customerInvoiceId }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), this.terms["common.customer.invoices.invoice"]);
            }
        }
    }

    protected getEditIdentifier(row: any): any {
        return row.customerInvoiceId;
    }

    protected getEditName(data: any): string {
        return data.seqNr ? data.seqNr : "";
    }


    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.customer.invoices.invoice",
            "common.customer.invoices.unpaid",
            "common.customer.invoices.reminder",
            "common.customer.invoices.intrest",
            "common.customer.payment.paid",
            "common.customer.payment.paidvoucher",
            "common.customer.invoices.customerinvoice",
            "common.customer.payment.payment"];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions() {
        const featureIds: number[] = [];
        featureIds.push(Feature.Economy_Customer_Invoice_Status_OriginToPayment);
        featureIds.push(Feature.Billing_Invoice_Status_OriginToPayment);
        featureIds.push(Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest);
        featureIds.push(Feature.Economy_Customer_Invoice_Status_PayedToVoucher);
        featureIds.push(Feature.Economy_Customer_Invoice_Status_PaymentVoucher);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if ((x[Feature.Economy_Customer_Invoice_Status_OriginToPayment] && soeConfig.module === SoeModule.Economy) || (x[Feature.Billing_Invoice_Status_OriginToPayment] && soeConfig.module === SoeModule.Billing))
                this.hasUnpaidPermission = true;
            if (x[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest])
                this.hasIntrestReminderPermission = true;
            if (x[Feature.Economy_Customer_Invoice_Status_PayedToVoucher])
                this.hasPaidPermission = true;
            if (x[Feature.Economy_Customer_Invoice_Status_PaymentVoucher])
                this.hasVoucherPermission = true;
        });
    }

    public tabs: ITabHandler;
}