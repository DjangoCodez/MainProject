import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeTimepickerDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, $timeout: ng.ITimeoutService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-l10n-bind="labelKey" data-ng-hide="hidelabel"></label>';

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel">*</label>';

                tmplString += '<div uib-timepicker pad-hours="true" data-ng-model="model" hour-step="hourstep" minute-step="minutestep" arrowkeys="arrowkeys && !isReadonly" mousewheel="mousewheel && !isReadonly" show-meridian="false" show-spinners="false" readonly-input="isReadonly" data-ng-disabled="disabled" data-ng-change="changed()"';

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{tabIndex}}"';

                tmplString += ' /></div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);
                //attrs.$set("date", true);
                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                labelKey: '@',
                hidelabel: '=?',
                model: '=?',
                hourstep: '@',
                minutestep: '@',
                arrowkeys: '=?',
                mousewheel: '=?',
                disabled: '=?isDisabled',
                isReadonly: '=?',
                tabIndex: '=?',
                setFocus: '=?',
                onFocus: '&',
                changed: '&',
                timeChanged: '&',
                hoursChanged: '&',
                minutesChanged: '&',
                secondsChanged: '&',
            },
            controller: TimePickerController,
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);

                // Default values
                if (!scope['hourstep'])
                    scope['hourstep'] = 1;
                if (!scope['minutestep'])
                    scope['minutestep'] = 1;
                if (!scope['arrowkeys'])
                    scope['arrowkeys'] = true;
                if (!scope['mousewheel'])
                    scope['mousewheel'] = false;
            },
            restrict: 'E',
            replace: true,
        };
    }
}

class TimePickerController {

    //@ngInject
    constructor($element, $timeout: ng.ITimeoutService, $scope, private translationService: ITranslationService) {

        this.setPlaceholders($element);

        if ($scope.setFocus) {
            $timeout(() => {
                $element.find('.hours input').select();
            });
        }

        $($element).on('focus', 'input', e => {
            $(e.currentTarget).select();
            if ($scope.onFocus())
                $scope.onFocus();
        });

        $($element).on('blur', 'input', e => {
            // IE needs a timeout
            $timeout(() => {
                var target = $(e.currentTarget);
                if (target && target.length) {
                    var parent = target[0].parentNode;
                    if (parent) {
                        if ($scope.timeChanged())
                            $scope.timeChanged();
                        if ($(parent).hasClass('hours') && $scope.hoursChanged())
                            $scope.hoursChanged();
                        if ($(parent).hasClass('minutes') && $scope.minutesChanged())
                            $scope.minutesChanged();
                        if ($(parent).hasClass('seconds') && $scope.secondsChanged())
                            $scope.secondsChanged();
                    }
                }
            });
        });

        var prev;
        $($element).on('keydown', 'input', e => {
            if ((e.keyCode >= 48 && e.keyCode <= 57) || (e.keyCode >= 96 && e.keyCode <= 105)) {//number
                var target = $(e.currentTarget);
                var prevprev = prev;
                prev = e.keyCode;

                $timeout(() => {//timeout to let the keydown update, we cant have keyup since we can push multiple buttons at the same time if typing quickly.
                    if ((<string>target.val()).length === 2 || prevprev === 48 || prevprev === 96) {//the timepicker removes leading 0 from hours during input, so we need to remember the 0 ourselves to make the focus work correctly on eg. 08
                        var inputs = $(':focusable'); //yes this is not optimal
                        inputs.eq(inputs.index(target) + 1).focus();
                        prev = null;
                        prevprev = null;
                    }
                });
            }
        });

        $scope.$on('$destroy', function () {
            $($element).off('keydown', 'input');
            $($element).off('focus', 'input');
        });
    }

    private setPlaceholders($element) {
        var keys: string[] = [
            "core.time.placeholder.hours",
            "core.time.placeholder.minutes",
            "core.time.placeholder.seconds"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var hours = $element.find('.hours input');
            if (hours && hours.length)
                $(hours[0]).attr('placeholder', terms["core.time.placeholder.hours"]);
            var minutes = $element.find('.minutes input');
            if (minutes && minutes.length)
                $(minutes[0]).attr('placeholder', terms["core.time.placeholder.minutes"]);
            var seconds = $element.find('.seconds input');
            if (seconds && seconds.length)
                $(seconds[0]).attr('placeholder', terms["core.time.placeholder.seconds"]);
        });
    }
}