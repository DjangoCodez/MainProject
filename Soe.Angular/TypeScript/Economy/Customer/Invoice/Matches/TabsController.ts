import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridController } from "./GridController";
import { EditController as InvoicesEditController } from "../../../../Common/Customer/Invoices/EditController";
import { EditController as PaymentsEditController } from "../../../../Common/Customer/Payments/EditController";
import { Constants } from "../../../../Util/Constants";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private $window, private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory, private messagingService: IMessagingService,) {

        // Setup base class
        var part: string = "economy.customer.invoice.matches.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                //tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            //.onEdit(row => this.edit(row))
            .initialize(part + "match", part + "matches", part + "new");



        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITCUSTOMERINVOICE, (x) => {
            this.openEditCustomerInvoice(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITPAYMENT, (x) => {
            this.openEditPayment(x);
        });
    }

    protected getEditIdentifier(row: any): any {
        return row.matchesId;
    }

    protected getEditName(data: any): string {
        return data.name;
    }

    protected openEditCustomerInvoice(row) {
        this.tabs.addNewTab(row.name, row.id, InvoicesEditController, this.urlHelperService.getGlobalUrl("Common/Customer/Invoices/Views/edit.html"), { id: row.id }, true, true);
    }

    protected openEditPayment(row) {
        this.tabs.addNewTab(row.name, row.id, PaymentsEditController, this.urlHelperService.getGlobalUrl('Common/Customer/Payments/Views/edit.html'), { id: row.id }, true, true);
    }

    public tabs: ITabHandler;
}