import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Feature, SoeOriginStatusClassification, SoeOriginStatus } from "../../../Util/CommonEnumerations";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingProjectsEditController } from "../../../Shared/Billing/Projects/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { Constants } from "../../../Util/Constants";
import { GridController as InvoicesGridController } from "../../../Common/Customer/Invoices/GridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private messagingService: IMessagingService,
        private translationService: ITranslationService) {

        const part = "billing.order.handlebilling.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true, activeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAgContext.html"));
                translationService.translate("common.customer.invoices.customerinvoices").then((term) => {
                    tabHandler.addHomeTab(InvoicesGridController, { classification: SoeOriginStatusClassification.CustomerInvoicesAll, setup: true, handleBilling: true, activeTab: false }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), term);
                });
            })
            .initialize(part + "periodinvoicing", part + "periodinvoicing", part + "periodinvoicing");

        messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x);
        });
        messagingService.subscribe(Constants.EVENT_OPEN_EDITPROJECT, (x) => {
            this.editProject(x);
        });
        messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            if (x)
                this.editInvoice(x.row, x.ids);
        });
    }

    private edit(row: any) {
    }
    private add() {
    }

    protected editOrder(row: any, useAssociated: boolean = false) {
        const activeTab = this.tabs.getTabByIdentifier("order_" + row.id);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "order_" + row.id, BillingOrdersEditController, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), { id: row.id, feature: Feature.Billing_Order_Status }, true, true);
        }
    }

    protected editProject(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("project_" + row.id);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "project_" + row.id, BillingProjectsEditController, this.urlHelperService.getGlobalUrl("billing/projects/views/edit.html"), { id: row.id }, true, true);
        }
    }

    protected editInvoice(row: any, ids: number[] = []) {
        const activeTab = this.tabs.getTabByIdentifier("invoice_" + row.customerInvoiceId);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.translationService.translateMany(["common.customerinvoice", "billing.order.preliminary"]).then((terms) => {
                this.tabs.addNewTab(row.status === SoeOriginStatus.Draft ? terms["common.customerinvoice"] + " " + terms["billing.order.preliminary"] : terms["common.customerinvoice"] + " " + row.seqNr, "invoice_" + row.customerInvoiceId, BillingInvoicesEditController, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), { id: row.customerInvoiceId, ids: ids }, true, true);
            });
        }
    }

    public tabs: ITabHandler;
}