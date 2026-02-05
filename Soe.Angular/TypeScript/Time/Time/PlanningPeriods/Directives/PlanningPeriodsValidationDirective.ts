import { TimePeriodHeadDTO } from "../../../../Common/Models/TimePeriodHeadDTO";

export class PlanningPeriodsValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        let timePeriodHead: TimePeriodHeadDTO = ngModelController.$modelValue;

                        let invalidDates: boolean = false;
                        let prevStopDate: Date;
                        _.forEach(_.sortBy(timePeriodHead.timePeriods, p => p.startDate), period => {
                            if (prevStopDate && period.startDate.isBeforeOnDay(prevStopDate)) {
                                invalidDates = true;
                                return false;
                            }

                            prevStopDate = period.stopDate;
                        });

                        ngModelController.$setValidity("periodDatesValid", !invalidDates);
                    }
                }, true);
            }
        }
    }
}