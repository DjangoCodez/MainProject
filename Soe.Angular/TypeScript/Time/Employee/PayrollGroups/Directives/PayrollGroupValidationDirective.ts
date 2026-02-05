import { EmployeeUserDTO, UserRolesDTO } from "../../../../Common/Models/EmployeeUserDTO";
import { TermGroup_EmployeeDisbursementMethod } from "../../../../Util/CommonEnumerations";
import { PayrollGroupDTO } from "../../../../Common/Models/PayrollGroupDTOs";

export class PayrollGroupValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                usePayroll: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    var payrollGroup: PayrollGroupDTO = ngModelController.$modelValue;
                    if (payrollGroup) {
                        // If using payroll, vacation group is mandatory
                        let vacationGroupSet: boolean = !scope['usePayroll'] || (payrollGroup.vacations && payrollGroup.vacations.length > 0);
                        ngModelController.$setValidity("vacationGroup", vacationGroupSet);

                        // Check dates on price types
                        let priceTypePeriodValidFromDate: boolean = true;
                        _.forEach(payrollGroup.priceTypes, priceType => {
                            let uniqDates = _.uniq(_.map(priceType.periods, p => p.fromDate.date().toFormattedDate()));
                            if (priceType.periods.length !== uniqDates.length) {
                                priceTypePeriodValidFromDate = false;
                                return;
                            }
                        });
                        ngModelController.$setValidity("priceTypePeriodFromDate", priceTypePeriodValidFromDate);

                        let uniquePriceTypesValid: boolean = true;
                        let uniquePriceTypesValidWithLevel: boolean = true;
                        let haslevels: boolean = false;
                        let group = _.groupBy(payrollGroup.priceTypes, pgpt => pgpt.priceTypeNameAndLevelName);
                        let payrollPriceTypeIds: string[] = Object.keys(group);                        
                        for (let i = 0, j = payrollPriceTypeIds.length; i < j; i++) {
                            let groupedPriceTypes = group[payrollPriceTypeIds[i]];
                            if (_.size(groupedPriceTypes) > 1) {
                                if (groupedPriceTypes[0].payrollLevelId && groupedPriceTypes[0].payrollLevelId > 0) {
                                    uniquePriceTypesValidWithLevel = false;
                                } else {
                                    uniquePriceTypesValid = false;
                                }
                            }                          
                        };
                       
                        ngModelController.$setValidity("uniquePriceTypesWithLevel", uniquePriceTypesValidWithLevel);                       
                        ngModelController.$setValidity("uniquePriceTypes", uniquePriceTypesValid);

                    }
                }, true);
            }
        }
    }
}