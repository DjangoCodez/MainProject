import { DateHelperService } from "../Services/datehelperservice";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class ParseTimeboxDirectiveFactory {
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
                    if (text) {
                        var date: Date;
                        if (CalendarUtility.isValidDate(text))
                            date = new Date(text);
                        else {
                            let minutes = CalendarUtility.timeSpanToMinutes(text);
                            date = CalendarUtility.getDateToday().addMinutes(minutes);
                        }

                        if (attributes['isTime'])
                            text = CalendarUtility.toFormattedTime(date);
                        else
                            text = CalendarUtility.parseTimeSpan(text, false, false);
                    }

                    element.val(text);
                    ngModelController.$viewValue = text;
                };

                // Parse the value when it is being set from the view to the model
                ngModelController.$parsers.unshift(function (value) {
                    if (attributes['allowEmpty'] && !value) {
                        element.val(null);
                    } else {
                        var timeSpan: string;

                        if (attributes['isTime']) {
                            let parts: string[] = [];
                            let isHundreds: boolean = false;

                            if (value.contains(':')) {
                                parts = value.split(':');
                            } else if (value.contains(',')) {
                                parts = value.split(',');
                                isHundreds = true;
                            } else if (value.contains('.')) {
                                parts = value.split('.');
                                isHundreds = true;
                            } else {
                                timeSpan = CalendarUtility.parseTimeSpan(value, false, true);
                            }

                            if (parts.length > 0) {
                                let hours = parseInt(parts[0], 10);
                                let minutes = 0;
                                if (parts.length > 1) {
                                    minutes = parseInt(parts[1], 10);
                                    if (isHundreds) {
                                        // Make sure minute part is two digits (eg: 0,25 = 25, 0,5 == 50, 0,750 = 75).
                                        // Otherwise convertion from hundreds to minutes below will be wrong.
                                        minutes = parseInt(minutes.toString().substring(0, 2).padRight(2, '0'), 10);
                                        minutes = (minutes / 100 * 60).round(0);
                                    }
                                }
                                while (minutes > 59) {
                                    minutes -= 60;
                                    hours++;
                                }
                                while (hours > 23) {
                                    hours -= 24;
                                }

                                timeSpan = CalendarUtility.minutesToTimeSpan((hours * 60) + minutes, false, false, true);
                            }
                        } else {
                            timeSpan = CalendarUtility.parseTimeSpan(value, false, false);
                        }

                        var date: Date = CalendarUtility.convertToDate(ngModelController.$modelValue);
                       
                        if (!date || !CalendarUtility.isValidDate(date))
                            date = CalendarUtility.getDateToday();
                        date = date.mergeTimeSpan(timeSpan);

                        if (attributes['isTime'])
                            element.val(date.toFormattedTime());
                        else
                            element.val(timeSpan);

                        return date;
                    }
                });

                // Used by ngModel to display to render the directive initially; we'll just reformat
                ngModelController.$render = formatter;

                // Trigger the formatter on blur
                //element.bind('blur', formatter);

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