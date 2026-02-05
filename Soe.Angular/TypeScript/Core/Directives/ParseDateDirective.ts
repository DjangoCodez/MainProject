import { DateHelperService } from "../Services/datehelperservice";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class ParseDateDirectiveFactory {
    //@ngInject
    public static create(dateHelperService: DateHelperService, $filter, $timeout): ng.IDirective {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: false,
            link(scope: any, element: JQuery, attributes: any, ngModelController: ng.INgModelController) {
                // Called by the directive to render the first time, and also on events when the value is changed 
                var formatter = function () {
                    var text: string = ngModelController.$viewValue;
                    var date = CalendarUtility.parseDate(text, dateHelperService);
                    if (date)
                        text = $filter('date')(date, dateHelperService.getShortDateFormat());
                    element.val(text);
                    ngModelController.$viewValue = text;
                };
                
                // Parse the value when it is being set from the view to the model
                ngModelController.$parsers.unshift(function (value) {
                    var date = CalendarUtility.parseDate(value, dateHelperService);
                    /*
                    if (date)
                        date = date.addMinutes(date.localTimeZoneOffsetFromDefault());
                    */
                    return date;
                });
                
                // Used by ngModel to display to render the directive initially; we'll just reformat
                ngModelController.$render = formatter;

                // Trigger the formatter on blur
                element.bind('blur', formatter);

                // Trigger the formatter on paste
                var handle;
                element.bind('paste cut', function () {
                    if (handle)
                        $timeout.cancel(handle);
                    handle = $timeout(formatter, 0); // Needs to break out of the current context to work
                });
            }
        }
    }
}