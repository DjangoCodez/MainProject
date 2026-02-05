import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridController } from "./GridController";
import { EditController as InvoicesEditController } from "../../../../Common/Customer/Invoices/EditController";
import { EditController as BillingInvoicesEditController } from "../../../../Shared/Billing/Invoices/EditController";
import { EditController as SupplierInvoiceEditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { Constants } from "../../../../Util/Constants";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { Feature, SoeInvoiceType } from "../../../../Util/CommonEnumerations";
import { HtmlUtility } from "../../../../Util/HtmlUtility";

export class TabsController implements ICompositionTabsController {

    private invoiceType: SoeInvoiceType;

    //@ngInject
    constructor(
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private messagingService: IMessagingService,
        $scope: ng.IScope,
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "economy.supplier.invoice.agedistribution.";

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITCUSTOMERINVOICE, (x) => {
            if (this.invoiceType === SoeInvoiceType.CustomerInvoice)
                this.openEditCustomerInvoice(x);
            else
                this.openEditSupplierInvoice(x);

        }, $scope);

        this.invoiceType = soeConfig.invoiceType;

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.ageDistributionId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true, invoiceType: this.invoiceType }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "agedistribution", part + "agedistribution", part + "new");
    }

    protected openEditCustomerInvoice(row) {
        if (row.registrationType == 1) {
            this.tabs.addNewTab(row.name, "Customer Invoice" + row.id, BillingInvoicesEditController, this.urlHelperService.getGlobalUrl("Shared/Billing/Invoices/Views/edit.html"), { id: row.id, feature: Feature.Economy_Customer_Invoice_AgeDistribution }, true, true);
            }
        else {
            this.tabs.addNewTab(row.name, "Customer Invoice" + row.id, InvoicesEditController, this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/edit.html"), { id: row.id, feature: Feature.Economy_Customer_Invoice_AgeDistribution }, true, true);      
        }
    }

    protected openEditSupplierInvoice(row) {
        this.tabs.addNewTab(row.name, "Supplier Invoice" + row.id, SupplierInvoiceEditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"), { id: row.id, feature: Feature.Economy_Supplier_Invoice_AgeDistribution }, true, true);
    }

    private edit(row: any) {
        // Open edit page
    }
    private add() {
        //this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/AccountDistribution/Views/edit.html"));
    }

    public tabs: ITabHandler;
}