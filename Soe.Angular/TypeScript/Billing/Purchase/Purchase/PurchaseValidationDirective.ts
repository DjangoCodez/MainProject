import { PurchaseDTO } from "../../../Common/Models/PurchaseDTO";

export class PurchaseValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                editPermission: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        const purchase: PurchaseDTO = ngModelController.$modelValue;
                        const editPermission: boolean = scope["editPermission"];
                        let validSupplier = true;

                        if (editPermission) {

                            validSupplier = !!(purchase && (purchase.supplierId && purchase.supplierId > 0));
                            //var validPriceList: boolean = !!(order && (order.priceListTypeId && order.priceListTypeId > 0));
                            //ngModelController.$setValidity("priceList", validPriceList);

                            /*var validOrderDate: boolean = !!(order && order.orderDate);
                            ngModelController.$setValidity("orderDate", validOrderDate);*/

                            ngModelController.$setValidity("supplier", validSupplier);
                        }
                    }
                }, true);
            }
        }
    }
}