import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { SoeOriginType } from "../../../Util/CommonEnumerations";

export class TabsController implements ICompositionTabsController {
    private terms: any;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private tabHandlerFactory: ITabHandlerFactory,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private $window: ng.IWindowService) {

        const part = "common.intrastat.reportingandexport";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productUnitId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .initialize(part, part, part);

        const keys = [
            "billing.project.central.supplierinvoice",
            "billing.project.central.customerinvoice"
        ]
        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        })

        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            if (x.originType === SoeOriginType.SupplierInvoice)
                this.editSupplierInvoice(x);
            if (x.originType === SoeOriginType.CustomerInvoice)
                this.editCustomerInvoice(x);
        });
    }

    private edit(row: any) {
    }

    private add() {
    }

    protected editSupplierInvoice(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("supplierInvoice_" + row.associatedId);
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, SupplierInvoicesEditController, { id: row.associatedId }, this.urlHelperService.getGlobalUrl("shared/economy/supplier/invoices/views/edit.html"), this.terms["billing.project.central.supplierinvoice"] + " " + row.tabSuffix, true);
        }
    }

    protected editCustomerInvoice(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("customerInvoice_" + row.associatedId);
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, BillingInvoicesEditController, { id: row.associatedId }, this.urlHelperService.getGlobalUrl("shared/billing/invoices/views/edit.html"), this.terms["billing.project.central.customerinvoice"] + " " + row.tabSuffix, true);
        }
    }

    public tabs: ITabHandler;
}