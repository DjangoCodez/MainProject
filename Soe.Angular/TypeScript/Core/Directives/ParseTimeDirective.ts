import { DateHelperService } from "../Services/datehelperservice";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class ParseTimeDirectiveFactory {
    //@ngInject
    public static create(dateHelperService: DateHelperService, $filter, $timeout): ng.IDirective {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: false,
            link(scope: any, element: JQuery, attributes: any, ngModelController: ng.INgModelController) {

                let allowEmpty: boolean = attributes['allowEmpty'];

                // Called by the directive to render the first time, and also on events when the value is changed 
                var formatter = _.debounce(() => {
                    var text: string = ngModelController.$viewValue;
                    if (!text && allowEmpty)
                        return;

                    var time = CalendarUtility.parseTimeSpan(text);
                    if (time)
                        text = $filter('minutesToTimeSpan')(time);
                    element.val(text);
                    ngModelController.$viewValue = text;
                }, 200, { leading: false, trailing: true });

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