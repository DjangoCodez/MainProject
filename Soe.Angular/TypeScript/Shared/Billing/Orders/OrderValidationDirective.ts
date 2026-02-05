import { OrderDTO } from "../../../Common/Models/InvoiceDTO";
import { StringUtility } from "../../../Util/StringUtility";
import { TermGroup_ContractGroupPeriod } from "../../../Util/CommonEnumerations";

export class OrderValidationDirectiveFactory {
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
                isContract: '=',
                period: "=",
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var order: OrderDTO = ngModelController.$modelValue;
                        var editPermission: boolean = scope["editPermission"];
                        var yearIsOpen: boolean = scope["accountYearOpen"];
                        var saveAsTemplate: boolean = scope["saveAsTemplate"];
                        var validCustomer: boolean = true;
                        var validTemplateDescription: boolean = true;

                        if (editPermission && (yearIsOpen || saveAsTemplate)) {
                            if (order) {
                                var customerBlocked = scope["customerBlocked"];
                                ngModelController.$setValidity("customerBlocked", customerBlocked ? !customerBlocked : true);
                            }

                            if (scope["isLocked"] && order) {
                                ngModelController.$setValidity("locked", true);
                            }
                            else {
                                if (saveAsTemplate) {
                                    validTemplateDescription = (order && !StringUtility.isEmpty(order.originDescription));
                                }
                                else {
                                    validCustomer = !!(order && (order.actorId && order.actorId > 0));
                                    var validPriceList: boolean = !!(order && (order.priceListTypeId && order.priceListTypeId > 0));
                                    ngModelController.$setValidity("priceList", validPriceList);

                                    /*var validOrderDate: boolean = !!(order && order.orderDate);
                                    ngModelController.$setValidity("orderDate", validOrderDate);*/

                                    if (!scope["isContract"]) {
                                        var seriesId = scope["standardVoucherSeriesId"];
                                        if (seriesId && seriesId > 0) {
                                            var validVoucherSeries: boolean = !!(order && (order.voucherSeriesId && order.voucherSeriesId > 0));
                                            ngModelController.$setValidity("defaultVoucherSeries", validVoucherSeries);
                                        }
                                        else {
                                            ngModelController.$setValidity("standardVoucherSeries", false);
                                        }
                                    }

                                    if (scope["isContract"]) {
                                        var validContractGroup: boolean = !!(order && (order.contractGroupId && order.contractGroupId > 0));
                                        ngModelController.$setValidity("contractGroup", validContractGroup);

                                        var validContractInterval: boolean = true;
                                        
                                        if (!order.nextContractPeriodYear || order.nextContractPeriodYear < 1) //|| !CalendarUtility.IsDateTimeSqlServerValid(new DateTime(contractPeriodYear, 1, 1)))
                                            validContractInterval = false;
                                        
                                        if (!order.nextContractPeriodValue || order.nextContractPeriodValue < 1)
                                            validContractInterval = false;
                                        var period = scope["period"] || TermGroup_ContractGroupPeriod.Week;
                                        if (validContractInterval && period) {
                                            switch (period) {
                                                case TermGroup_ContractGroupPeriod.Week:
                                                    if (order.nextContractPeriodValue > 53)
                                                        validContractInterval = false;
                                                    break;
                                                case TermGroup_ContractGroupPeriod.Month:
                                                    if (order.nextContractPeriodValue > 12)
                                                        validContractInterval = false;
                                                    break;
                                                case TermGroup_ContractGroupPeriod.Quarter:
                                                    if (order.nextContractPeriodValue > 4)
                                                        validContractInterval = false;
                                                    break;
                                                case TermGroup_ContractGroupPeriod.Year:
                                                    break;
                                                case TermGroup_ContractGroupPeriod.CalendarYear:
                                                    break;
                                            }
                                        }

                                        ngModelController.$setValidity("contractInterval", validContractInterval);
                                    }
                                    else {
                                        ngModelController.$setValidity("contractInterval", true);
                                        ngModelController.$setValidity("contractGroup", true);
                                    }
                                }
                            }

                            ngModelController.$setValidity("customer", validCustomer);
                            ngModelController.$setValidity("templateDescription", validTemplateDescription);
                        }
                        else {
                            ngModelController.$setValidity("customer", validCustomer);
                            ngModelController.$setValidity("editPermission", editPermission);
                            ngModelController.$setValidity("yearOpen", yearIsOpen);
                        }
                    }
                }, true);
                scope.$watchGroup(['saveAsTemplate'], (newValues, oldValues, scope) => {

                    var order: OrderDTO = ngModelController.$modelValue;
                    var validCustomer = !!(order && (order.actorId && order.actorId > 0));
                    var validTemplateDescription = (order && !StringUtility.isEmpty(order.originDescription));
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