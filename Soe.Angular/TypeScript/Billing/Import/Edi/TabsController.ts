import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { TabsControllerBase1 } from "../../../Core/Controllers/TabsControllerBase1";
import { GridController } from "../../../Shared/Billing/Import/Edi/GridController";
import { EditController } from "../../../Shared/Billing/Products/Products/EditController";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SoeOriginType, TermGroup_EDIStatus } from "../../../Util/CommonEnumerations";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as SupplierInvoiceEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { Constants } from "../../../Util/Constants";

export class TabsController extends TabsControllerBase1 {

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private coreService: ICoreService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope)

        // Setup base class
        const part: string = "billing.imports.edi";
        super.initialize(part + "edi", part + "edi", part + "edi");

        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITSUPPLIERINVOICE, (x) => {
            this.editSupplierInvoice(x);
        });
    }

    protected setupTabs() {
        var keys: string[] = ["billing.import.edi.openorders", "billing.import.edi.openinvoices", "billing.import.edi.closedorders", "billing.import.edi.closedinvoices"];
        return this.translationService.translateMany(keys).then((terms) => {
            this.addNewTab(terms["billing.import.edi.openorders"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { originType: SoeOriginType.Order, status: TermGroup_EDIStatus.Unprocessed }, false, true)
            this.addNewTab(terms["billing.import.edi.openinvoices"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { originType: SoeOriginType.SupplierInvoice, status: TermGroup_EDIStatus.Unprocessed }, false, false)
            this.addNewTab(terms["billing.import.edi.closedorders"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { originType: SoeOriginType.Order, status: TermGroup_EDIStatus.Processed }, false, false)
            this.addNewTab(terms["billing.import.edi.closedinvoices"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { originType: SoeOriginType.SupplierInvoice, status: TermGroup_EDIStatus.Processed }, false, false)
            this.enableRemoveAll();
        });
    }

    protected add() {
        this.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        //Edit is taken care of in gridcontroller
    }

    protected getEditIdentifier(row: any): any {
        return null;
    }

    protected getEditName(data: any): string {
        return "";
    }

    protected editOrder(item: any) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === "order_" + item.id);
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            this.addEditTab(item.name, "order_" + item.id, BillingOrdersEditController, { id: item.id }, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), true);
        }
    }

    protected editSupplierInvoice(item: any) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === "invoice_" + item.id);
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            this.addEditTab(item.name, "invoice_" + item.id, SupplierInvoiceEditController, { id: item.id }, this.urlHelperService.getGlobalUrl("shared/economy/supplier/invoices/views/edit.html"), true);
        }
    }
}