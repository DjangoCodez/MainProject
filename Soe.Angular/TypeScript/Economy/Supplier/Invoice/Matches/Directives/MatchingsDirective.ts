import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { Feature, SoeEntityType } from "../../../../../Util/CommonEnumerations";

//@ngInject
export function invoiceMatchMatchingsDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        templateUrl: urlHelperService.getViewUrl("invoiceMatchMatchings.html"),
        replace: true,
        restrict: "E",
        controller: InvoiceMatchMatchingController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            matching: "="
        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.matching), () => {
                ngModelController.updateData();
            }, true);
        }
    }
}

export class InvoiceMatchMatchingController extends GridControllerBase {
    public matching: any;
    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("economy.supplier.invoice.attestaccountingrows",
            "economy.supplier.invoice.attestaccountingrows",
            Feature.Economy_Supplier_Invoice_Invoices_Edit,
            $http,
            $templateCache,
            $timeout,
            $uibModal,
            coreService,
            translationService,
            urlHelperService,
            messagingService,
            notificationService,
            uiGridConstants);
    }

    public updateData() {
        if (this.matching) {
            var id = (this.matching.type === SoeEntityType.SupplierInvoice || this.matching.type === SoeEntityType.CustomerInvoice) ? this.matching.invoiceId : this.matching.paymentRowId;
            this.supplierService.getInvoicesMatches(id, this.matching.actorId, this.matching.type).then(x => {
            });
        }
    }
}