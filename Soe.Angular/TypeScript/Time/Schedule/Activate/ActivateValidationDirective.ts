import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TermGroup_TemplateScheduleActivateFunctions } from "../../../Util/CommonEnumerations";

export class ActivateValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                hasInitialAttestState: '=',
                hasSelectedEmployees: '=',
                selectedFunction: '=',
                startDate: '=',
                stopDate: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['hasInitialAttestState', 'hasSelectedEmployees', 'selectedFunction', 'startDate', 'stopDate'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("initialAttestState", scope['hasInitialAttestState']);
                    ngModelController.$setValidity("selectedEmployees", scope['hasSelectedEmployees']);

                    // Check dates
                    let startDate: Date = CalendarUtility.convertToDate(scope['startDate']);
                    let stopDate: Date = CalendarUtility.convertToDate(scope['stopDate']);
                    ngModelController.$setValidity("startDate", (!!startDate || scope['selectedFunction'] == TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate));
                    ngModelController.$setValidity("stopDate", !!stopDate);
                    //ngModelController.$setValidity("startDateOnMonday", startDate && startDate.getDay() === 1);
                    ngModelController.$setValidity("stopDateMaxTwoYears", stopDate && stopDate.isBeforeOnDay(CalendarUtility.getDateToday().addYears(2)));
                    ngModelController.$setValidity("stopDateAfterStartDate", !startDate || (stopDate && stopDate.isAfterOnDay(startDate)));
                });
            }
        }
    }
}