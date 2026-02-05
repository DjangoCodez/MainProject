import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as BillingPurchaseEditController } from "../../../Shared/Billing/Purchase/Purchase/EditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private messagingService: IMessagingService, private translationService: ITranslationService,) {

        const part: string = "billing.distribution.edistribution.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.vatCodeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            //.onEdit(row => this.edit(row))
            .initialize(part + "distribution", part + "distribution", part);

        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            this.editInvoice(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_OFFER, (x) => {
            this.editOffer(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_PURCHASE, (x) => {
            this.editPurchase(x);
        });
    }
    protected editPurchase(row: any, useAssociated: boolean = false) {
        const activeTab = this.tabs.getTabByIdentifier("purchase_" + row.originId);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "purchase_" + row.originId, BillingPurchaseEditController, this.urlHelperService.getGlobalUrl("billing/purchase/purchase/views/edit.html"), { id: row.originId }, true, true);
        }
    }
    protected editOrder(row: any, useAssociated: boolean = false) {
        const activeTab = this.tabs.getTabByIdentifier("order_" + row.originId);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {

            this.tabs.addNewTab(row.name, "order_" + row.originId, BillingOrdersEditController, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), { id: row.originId, feature: Feature.Billing_Order_Status }, true, true);
        }
    }
    protected editInvoice(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("invoice_" + row.originId);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "invoice_" + row.originId, BillingInvoicesEditController, this.urlHelperService.getGlobalUrl("Shared/Billing/Invoices/Views/edit.html"), { id: row.originId, createEInvoicePermissionFromEDI: true }, true, true);
        }
    }
    protected editOffer(row: any) {
        const activeTab = this.tabs.getTabByIdentifier(row.originId);
        if (activeTab) {
            this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
        } else {
            this.tabs.addNewTab(row.name, "offer_" + row.originId, BillingOrdersEditController, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/editOffer.html"), { id: row.originId, feature: Feature.Billing_Offer_Status }, true, true);
        }
    }
    private edit(row: any) {
        //this.tabs.addEditTab(row, EditController);
    }

    public tabs: ITabHandler;
}