import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { Constants } from "../../../Util/Constants";
import { SoeModule, SoeOriginStatusClassification, Feature, OrderInvoiceRegistrationType, SoeOriginType } from "../../../Util/CommonEnumerations";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { add } from "lodash";

export class TabsController implements ICompositionTabsController {
    private singular: string;
    private plural: string;
    private new: string;

    protected terms: any;
    protected hasOpenPermission: boolean;
    protected hasIntrestReminderPermission: boolean;

    //@ngInject
    constructor(tabHandlerFactory: ITabHandlerFactory,
        protected urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService, private $window) {

        this.setTypeStrings(soeConfig.feature);
        this.messagingService.subscribe(Constants.EVENT_OPEN_OFFER, (x) => {
            if (x)
                this.edit(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_CONTRACT, (x) => {
            if (x)
                this.edit(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.edit(x.row, x.ids);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            if (x)
                this.edit(x.row, x.ids);
        });

        const part = "common.customer.invoices.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.customerInvoiceId)
            .onGetRowEditName(row => row.invoiceNr ? row.invoiceNr : this.terms["economy.supplier.invoice.preliminary"])
            .onSetupTabs(() => { this.setupTabs(); })
            .onEdit(row => this.edit(row))
            .initialize(part + this.singular, part + this.plural, part + this.new);
    }

    protected setupTabs() {
        //debugger;
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.tabs.enableAddTab(() => this.add());
            this.tabs.enableRemoveAll();
            if (soeConfig.module === SoeModule.Economy) {
                let activateTab = true;
                if (this.hasOpenPermission) {
                    this.tabs.addNewTab(this.terms["common.customer.invoices.customerinvoices"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerInvoicesAll, isHomeTab: true, setup: true }, false, activateTab);
                    activateTab = false;
                }
                if (this.hasIntrestReminderPermission) {
                    //this.addNewTab(this.terms["common.customer.invoices.reminder"], null, GridController, this.urlHelperService.getCoreViewUrl("grid1.html"), { classification: SoeOriginStatusClassification.CustomerInvoicesReminder, isHomeTab: true }, false, activateTab);
                    this.tabs.addNewTab(this.terms["common.customer.invoices.intrest"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerInvoicesInterest, isHomeTab: true, setup: activateTab }, false, activateTab);
                }
            }
            else {
                switch (soeConfig.feature) {
                    case Feature.Billing_Offer_Status:
                        this.tabs.addHomeTab(GridController, { classification: SoeOriginStatusClassification.OffersAll, isHomeTab: true, setup: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                        break;
                    case Feature.Billing_Order_Status:
                        this.tabs.addHomeTab(GridController, { classification: SoeOriginStatusClassification.OrdersAll, isHomeTab: true, setup: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                        break;
                    case Feature.Billing_Invoice_Status:
                        this.tabs.addHomeTab(GridController, { classification: SoeOriginStatusClassification.CustomerInvoicesAll, isHomeTab: true, setup: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                        break;
                    case Feature.Billing_Contract_Status:
                        this.tabs.addHomeTab(GridController, { classification: SoeOriginStatusClassification.ContractsRunning, activeTab: true, setup: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), this.terms["common.customer.contracts.running"]);
                        this.tabs.addHomeTab(GridController, { classification: SoeOriginStatusClassification.ContractsAll, activeTab: false, setup: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                        break;
                    default:
                        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
                        break;
                }
            }

            // Navigate to invoice from new Angular
            const invIdStr = HtmlUtility.getQueryParameterByName(this.$window.location, "invoiceId");
            if (invIdStr && soeConfig.invoiceId == 0) {
                this.add();
            }
            else if (soeConfig.invoiceId > 0) {
                const invoiceIdStr = soeConfig.invoiceId;
                if (invoiceIdStr) {
                    var invoiceNr = soeConfig.invoiceNr;
                    const invoiceId = parseInt(invoiceIdStr);
                    const row = { customerInvoiceId: invoiceId, invoiceNr: invoiceNr > 0 ? invoiceNr : "" };
                    let ids: number[] = [];
                    ids.push(invoiceId);
                    this.navigateToLagacyEdit(row, ids);
                }
            }
        });
    }

    protected getEditIdentifier(row: any): any {
        return row.customerInvoiceId;
    }

    protected add() {
        switch (soeConfig.feature) {
            case Feature.Billing_Offer_Status:
                this.tabs.addCreateNewTab(BillingOrdersEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html'), { id: 0, originType: SoeOriginType.Offer });
                break;
            case Feature.Billing_Order_Status:
                this.tabs.addCreateNewTab(BillingOrdersEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html'), { id: 0, originType: SoeOriginType.Order });
                break;
            case Feature.Billing_Invoice_Status:
                this.tabs.addCreateNewTab(BillingInvoicesEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), { id: 0 });
                break;
            case Feature.Billing_Contract_Status:
                this.tabs.addCreateNewTab(BillingOrdersEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editContract.html'), { id: 0, originType: SoeOriginType.Contract });
                break;
            default:
                this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
                break;
        }
    }

    protected navigateToLagacyEdit(row: any, ids: number[] = []) {
        const params = { id: row.customerInvoiceId, ids: ids };
        this.$timeout(() => {
            switch (soeConfig.feature) {
                case Feature.Billing_Contract_Status: {
                    this.tabs.addEditTab(row, BillingOrdersEditController, params, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editContract.html'));
                    break;
                }
                case Feature.Billing_Invoice_Status: {
                    //const params = { id: soeConfig.invoiceId }
                    const templateUrl = this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html');
                    this.tabs.addEditTab(row, BillingInvoicesEditController, params, templateUrl, null, true);
                    break;
                }
                case Feature.Billing_Offer_Status: {
                    this.tabs.addEditTab(row, BillingOrdersEditController, params, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html'));
                    break;
                }
                case Feature.Billing_Order_Status: {
                    this.tabs.addEditTab(row, BillingOrdersEditController, params, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html'));
                    break;
                } case Feature.Economy_Customer_Invoice_Status: {
                    this.tabs.addEditTab(row, EditController, params, this.urlHelperService.getViewUrl("edit.html"));
                    break;
                }
            }
        }, 1000);
    }

    protected edit(row: any, ids: number[] = []) {
        switch (soeConfig.feature) {
            case Feature.Billing_Contract_Status: {
                // Invoice
                const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (activeTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
                } else {
                    this.tabs.addEditTab(row, BillingOrdersEditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editContract.html'));
                }
                break;
            }
            case Feature.Billing_Offer_Status:
                // Offer
                const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (activeTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
                } else {
                    this.tabs.addEditTab(row, BillingOrdersEditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html'));
                }
                break;
            case Feature.Billing_Order_Status: {
                // Order
                const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (activeTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
                } else {
                    this.tabs.addEditTab(row, BillingOrdersEditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html'));
                }
                break;
            }
            case Feature.Billing_Invoice_Status: {
                // Invoice
                const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (activeTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
                } else {
                    if (row.registrationType === OrderInvoiceRegistrationType.Ledger)
                        this.tabs.addEditTab(row, EditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html'), undefined, row.createCopy ? false : true);
                    else
                        this.tabs.addEditTab(row, BillingInvoicesEditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), undefined, row.createCopy ? false : true);
                }
                break;
            }
            default: {
                if (row.registrationType === OrderInvoiceRegistrationType.Ledger) {
                    const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                    if (activeTab) {
                        this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
                    } else {
                        this.tabs.addEditTab(row, EditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html'));
                    }
                }
                else {
                    const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                    if (activeTab) {
                        this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
                    } else {
                        this.tabs.addEditTab(row, BillingInvoicesEditController, { id: this.getEditIdentifier(row), ids: ids }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'));
                    }
                }
                break;
            }
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.customer.invoices.customerinvoices",
            "common.customer.invoices.reminder",
            "common.customer.invoices.intrest",
            "economy.supplier.invoice.preliminary",
            "common.customer.contracts.running",
            "billing.invoices.invoice",
            "billing.order.order"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions() {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Customer_Invoice_Invoices);
        featureIds.push(Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (x[Feature.Economy_Customer_Invoice_Invoices])
                this.hasOpenPermission = true;
            if (x[Feature.Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest])
                this.hasIntrestReminderPermission = true;
        });
    }

    private setTypeStrings(feature: Feature) {
        switch (feature) {
            case Feature.Billing_Offer_Status:
                this.singular = "offer";
                this.plural = "offers";
                this.new = "newoffer";
                break;
            case Feature.Billing_Order_Status:
                this.singular = "order";
                this.plural = "orders";
                this.new = "neworder";
                break;
            case Feature.Billing_Invoice_Status:
                this.singular = "customerinvoice";
                this.plural = "customerinvoices";
                this.new = "newcustomerinvoice";
                break;
            case Feature.Billing_Contract_Status:
                this.singular = "contract";
                this.plural = "contract";
                this.new = "newcontract";
                break;
            case Feature.Economy_Customer_Invoice_Status:
                this.singular = "customerinvoice";
                this.plural = "customerinvoices";
                this.new = "newcustomerinvoice";
                break;
            default:
                this.singular = "unknown";
                this.plural = "unknown";
                this.new = "unknown";
                break;
        }
    }

    public tabs: ITabHandler;
}