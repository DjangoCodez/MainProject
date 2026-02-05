import { TabsControllerBase1 } from "../../../Core/Controllers/TabsControllerBase1";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { EditController as SupplierCentral } from "./EditController";
import { Constants } from "../../../Util/Constants";
import { EditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";

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
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope);

        const part = "economy.supplier.suppliercentral.";
        super.initialize(part + "suppliercentral", part + "suppliercentrals", "economy.supplier.invoice.new");
    }

    protected setupTabs() {
        this.addHomeTab(SupplierCentral, null, this.urlHelperService.getViewUrl("edit.html"));
        this.messagingService.subscribe(Constants.EVENT_EDIT_NEW, (params) => {
            this.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"), params);
        }, this.$scope);
    }

    protected add() {
        this.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        this.addEditTab(this.getEditName(row), this.getEditIdentifier(row), EditController, { id: this.getEditIdentifier(row) });
    }
    protected getEditIdentifier(row: any): any {
        return row.supplierInvoiceId;
    }

    protected getEditName(data: any): string {
        return data.invoiceNr ? data.invoiceNr : "";
    }
}