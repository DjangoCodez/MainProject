import { TimePeriodDTO } from "../../../../Common/Models/TimePeriodDTO";
import { ITimePeriodDTO } from "../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../Util/CalendarUtility";

export class TimePeriodValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                usePayroll: '=',
                payrollStartDate: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var timePeriodRows: ITimePeriodDTO[] = ngModelController.$modelValue;
                        var periodsValid: boolean = true;
                        if (scope['usePayroll']) {
                            var date = new Date(2014, 12, 1);
                            if (scope['payrollStartDate'])
                                date = CalendarUtility.convertToDate(scope['payrollStartDate'])

                            _.forEach(timePeriodRows, (timePeriod: TimePeriodDTO) => {
                                
                                if (timePeriod.startDate >= date) {
                                    if (!timePeriod.paymentDate || !timePeriod.payrollStartDate || !timePeriod.payrollStopDate)
                                        periodsValid = false;
                                }
                            });
                        }
                        ngModelController.$setValidity("periodsValid", periodsValid);
                    }
                },true);
            }
        }
    }
}