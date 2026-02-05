import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { SoeOriginType, Feature } from "../../../Util/CommonEnumerations";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private messagingService: IMessagingService, private translationService: ITranslationService) {
        const label = "common.customer.customer.templates";
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.invoiceId)
            .onGetRowEditName(row => row.invoiceNr)
            .onSetupTabs((tabHandler) => {
                this.tabs.enableRemoveAll();
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));                
            })    
            .initialize(label, label, label);

        this.messagingService.subscribe(Constants.EVENT_OPEN_CUSTOMERINVOICE, (x) => {
            if (x) {
                if(x.row)
                    this.edit(x.row);
                else
                    this.add(x.originType);
            }
        });
    }

    protected add(originType: SoeOriginType) {
        const keys: string[] = [
            "common.new",
            "common.customer.customer.offertemplate",
            "common.customer.customer.ordertemplate",
            "common.customer.customer.billingtemplate",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            switch (originType) {
                case SoeOriginType.Offer:
                    this.tabs.addCreateNewTab(BillingOrdersEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html'), { id: 0, originType: SoeOriginType.Offer, feature: Feature.Billing_Offer_Status, isTemplate: true }, terms["common.new"] + " " + terms["common.customer.customer.offertemplate"]);
                    break;
                case SoeOriginType.Order:
                    this.tabs.addCreateNewTab(BillingOrdersEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html'), { id: 0, originType: SoeOriginType.Order, feature: Feature.Billing_Order_Status, isTemplate: true }, terms["common.new"] + " " + terms["common.customer.customer.ordertemplate"]);
                    break;
                case SoeOriginType.CustomerInvoice:
                    this.tabs.addCreateNewTab(BillingInvoicesEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), { id: 0, isTemplate: true }, terms["common.new"] + " " + terms["common.customer.customer.billingtemplate"]);
                    break;
            }
        });
    }

    protected edit(row: any) {
        switch (row.originType) {
            case SoeOriginType.Offer:
                // Offer
                var offerTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (offerTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(offerTab));
                } else {
                    this.tabs.addEditTab(row, BillingOrdersEditController, { id: this.getEditIdentifier(row), feature: Feature.Billing_Offer_Status, isTemplate: true, ids: [] }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html'), row.name);
                }
                break;
            case SoeOriginType.Order: {
                // Order
                var orderTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (orderTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(orderTab));
                } else {
                    this.tabs.addEditTab(row, BillingOrdersEditController, { id: this.getEditIdentifier(row), feature: Feature.Billing_Order_Status, isTemplate: true, ids: [] }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html'), row.name);
                }
                break;
            }
            case SoeOriginType.CustomerInvoice: {
                // Invoice
                var invoiceTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row));
                if (invoiceTab) {
                    this.tabs.setActiveTabIndex(this.tabs.getIndexOf(invoiceTab));
                } else {
                    this.tabs.addEditTab(row, BillingInvoicesEditController, { id: this.getEditIdentifier(row), isTemplate: true, ids: [] }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), row.name);
                }
                break;
            }
        }
    }

    protected getEditIdentifier(row: any): any {
        return row.invoiceId;
    }

    public tabs: ITabHandler;
}