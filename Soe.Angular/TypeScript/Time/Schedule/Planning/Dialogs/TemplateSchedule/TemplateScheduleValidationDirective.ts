import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/timescheduletemplatedtos";
import { TermGroup_TemplateScheduleActivateFunctions } from "../../../../../Util/CommonEnumerations";

export class TemplateScheduleValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                placementExists: '=',
                placementFunction: '=',
                placementStartDate: '=',
                placementStopDate: '=',
                employeeScheduleStartDate: '=',
                templateStartDate: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => scope['ngModel'], (newValues, oldValues) => {
                    // Init parameters
                    var templateHead: TimeScheduleTemplateHeadSmallDTO = ngModelController.$modelValue;
                    var placementExists: boolean = scope["placementExists"];
                    var placementStopDate: Date = scope["placementStopDate"];

                    // Template
                    var stopDateBeforeStartDate: boolean = false;
                    var stopDateBeforePlacementStopDate: boolean = false;

                    if (templateHead && templateHead.stopDate) {
                        // Stop date must be after start date
                        stopDateBeforeStartDate = (templateHead.startDate && templateHead.stopDate.isBeforeOnDay(templateHead.startDate));
                        // Stop date must be after placement end date
                        stopDateBeforePlacementStopDate = (placementExists && placementStopDate && templateHead.stopDate.isBeforeOnDay(placementStopDate));
                    }

                    ngModelController.$setValidity("locked", !(templateHead && templateHead.locked));
                    ngModelController.$setValidity("stopDateBeforeStartDate", !stopDateBeforeStartDate);
                    ngModelController.$setValidity("stopDateBeforePlacementStopDate", !stopDateBeforePlacementStopDate);
                }, true);

                scope.$watchGroup(['placementFunction', 'placementStartDate', 'placementStopDate', 'employeeScheduleStartDate'], (newValues, oldValues) => {
                    // Init parameters
                    var placementExists: boolean = scope["placementExists"];
                    var placementFunction: TermGroup_TemplateScheduleActivateFunctions = scope["placementFunction"];
                    var placementStartDate: Date = scope["placementStartDate"];
                    var placementStopDate: Date = scope["placementStopDate"];
                    var employeeScheduleStartDate: Date = scope["employeeScheduleStartDate"];                    

                    // Placement
                    var bothPlacementDates: boolean = true;
                    var placementStopDateBeforePlacementStartDate: boolean = false;
                    var missingPlacementStopDate: boolean = false;
                    var placementStopDateBeforeEmployeeScheduleStartDate: boolean = false;

                    if (placementFunction == TermGroup_TemplateScheduleActivateFunctions.NewPlacement) {
                        // If new placement, both dates must be specified
                        bothPlacementDates = !!(placementStartDate && placementStopDate);
                        if (bothPlacementDates) {
                            // Stop date must be same or after start date
                            placementStopDateBeforePlacementStartDate = placementStopDate.isBeforeOnDay(placementStartDate);
                        }
                    } else if (placementFunction == TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate) {
                        if (!placementStopDate) {
                            // If change stop date, stop date must be specified
                            missingPlacementStopDate = true;
                        } else if (placementExists && placementStopDate.isBeforeOnDay(employeeScheduleStartDate)) {
                            // If change stop date, stop date must not be before last placement's start date
                            placementStopDateBeforeEmployeeScheduleStartDate = true;
                        }
                    }

                    ngModelController.$setValidity("missingPlacementDates", bothPlacementDates);
                    ngModelController.$setValidity("placementStopDateBeforePlacementStartDate", !placementStopDateBeforePlacementStartDate);
                    ngModelController.$setValidity("missingPlacementStopDate", !missingPlacementStopDate);
                    ngModelController.$setValidity("placementStopDateBeforeEmployeeScheduleStartDate", !placementStopDateBeforeEmployeeScheduleStartDate);
                });
            }
        }
    }
}
