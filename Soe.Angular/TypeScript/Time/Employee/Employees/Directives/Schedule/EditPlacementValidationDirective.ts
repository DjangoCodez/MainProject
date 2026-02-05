import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EditPlacementMode } from "./ScheduleDirective";

export class EditPlacementValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                hasInitialAttestState: '=',
                mode: '=',
                templateType: '=',
                templateHead: '=',
                periodId: '=',
                startDate: '=',
                stopDate: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['hasInitialAttestState', 'mode', 'templateType', 'templateHead', 'periodId', 'startDate', 'stopDate'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("initialAttestState", scope['hasInitialAttestState']);
                    ngModelController.$setValidity("templateHead", (!!scope['templateHead'] || scope['mode'] != EditPlacementMode.New || scope['templateType'] == 0));

                    // Check dates
                    let startDate: Date = CalendarUtility.convertToDate(scope['startDate']);
                    let stopDate: Date = CalendarUtility.convertToDate(scope['stopDate']);

                    let validStartDate = true;
                    let templateHead: TimeScheduleTemplateHeadSmallDTO = scope['templateHead'];
                    if (templateHead?.startDate.isAfterOnDay(startDate))
                        validStartDate = false;

                    ngModelController.$setValidity("selectedPeriod", (scope['templateType'] == 0 || scope['periodId'] > 0) && validStartDate);

                    ngModelController.$setValidity("startDate", (!!startDate || scope['mode'] != EditPlacementMode.New));
                    ngModelController.$setValidity("stopDate", !!stopDate);
                    ngModelController.$setValidity("stopDateMaxTwoYears", stopDate?.isBeforeOnDay(CalendarUtility.getDateToday().addYears(2)));
                    ngModelController.$setValidity("stopDateAfterStartDate", !startDate || stopDate?.isAfterOnDay(startDate));
                });
            }
        }
    }
}