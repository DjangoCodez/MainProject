import { BillingInvoiceDTO } from "../../../Common/Models/InvoiceDTO";
import { StringUtility } from "../../../Util/StringUtility";

export class InvoiceValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                isLocked: '=',
                editPermission: '=',
                accountYearOpen: '=',
                saveAsTemplate: '=',
                standardVoucherSeriesId: '=',
                customerBlocked: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var invoice: BillingInvoiceDTO = ngModelController.$modelValue;
                        var editPermission: boolean = scope["editPermission"];
                        var yearIsOpen: boolean = scope["accountYearOpen"];
                        var saveAsTemplate: boolean = scope["saveAsTemplate"];
                        var validCustomer: boolean = true;
                        var validTemplateDescription: boolean = true;
                        if (editPermission && (yearIsOpen || saveAsTemplate)) {
                            if (invoice) {
                                var customerBlocked = scope["customerBlocked"];
                                ngModelController.$setValidity("customerBlocked", customerBlocked ? !customerBlocked : true);
                            }

                            if (scope["isLocked"] && invoice) {
                                ngModelController.$setValidity("locked", true);
                            }
                            else {
                                if (saveAsTemplate) {
                                    validTemplateDescription = (invoice && !StringUtility.isEmpty(invoice.originDescription));
                                }
                                else {

                                    validCustomer = !!(invoice && (invoice.actorId && invoice.actorId > 0));

                                    var validPriceList: boolean = !!(invoice && (invoice.priceListTypeId && invoice.priceListTypeId > 0));
                                    ngModelController.$setValidity("priceList", validPriceList);
                                }
                            }

                            ngModelController.$setValidity("customer", validCustomer);
                            ngModelController.$setValidity("templateDescription", validTemplateDescription);
                        }
                        else {
                            ngModelController.$setValidity("editPermission", editPermission);
                            ngModelController.$setValidity("yearOpen", yearIsOpen);
                        }
                    }
                }, true);
                scope.$watchGroup(['saveAsTemplate'], (newValues, oldValues, scope) => {
                    var invoice: BillingInvoiceDTO = ngModelController.$modelValue;
                    var validCustomer = !!(invoice && (invoice.actorId && invoice.actorId > 0));
                    var validTemplateDescription = (invoice && !StringUtility.isEmpty(invoice.originDescription));
                    if (scope["saveAsTemplate"])
                        validCustomer = true;
                    else
                        validTemplateDescription = true;

                    ngModelController.$setValidity("customer", validCustomer);
                    ngModelController.$setValidity("templateDescription", validTemplateDescription);
                });
            }
        }
    }
}


