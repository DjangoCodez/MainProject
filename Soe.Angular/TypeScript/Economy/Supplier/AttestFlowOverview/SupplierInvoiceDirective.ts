import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { Guid } from "../../../Util/StringUtility";

//@ngInject
export function supplierInvoiceDirective(
    urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        templateUrl: urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"),
        replace: false,
        restrict: "E",
        controller: SupplierInvoicesEditController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            
        },
        link(scope: any, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            ngModelController.onInit({ guid: Guid.newGuid(), noTabs: true, secondaryDirtyEvent: true });
            /*
            scope.$watch(() => ngModelController.invoiceId,
                (newVAlue, oldvalue) => {
                    if (newVAlue) {
                        //console.log("reloadSupplierInvoice", ngModelController.invoiceId);
                        //ngModelController.reloadSupplierInvoice(ngModelController.invoiceId, true);
                    }
                }, true);
            */
        }
    }
}