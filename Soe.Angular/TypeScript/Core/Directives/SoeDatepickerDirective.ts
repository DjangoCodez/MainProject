import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class SoeDatepickerDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, uibDatepickerConfig: angular.ui.bootstrap.IDatepickerConfig, uibDatepickerPopupConfig: angular.ui.bootstrap.IDatepickerPopupConfig, $locale: any): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-hide="directiveCtrl.hidelabel"></label>';

                if (attrs['labelValue']) {
                    tmplString += '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelPrefixValue" style="padding-right: 3px;">{{directiveCtrl.labelPrefixValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue">{{directiveCtrl.labelValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelPostfixValue" style="padding-left: 3px;">{{directiveCtrl.labelPostfixValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">)</label>';
                } else if (attrs['showWeekDay']) {
                    tmplString += '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" style="margin-left: 3px;" data-ng-show="directiveCtrl.hasDate">(</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="directiveCtrl.hasDate">{{directiveCtrl.weekDay}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="directiveCtrl.hasDate">)</label>';
                }

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="directiveCtrl.hidelabel">*</label>';
                else if (attrs['isRequired'])
                    tmplString += '<label class="required-label" data-ng-hide="directiveCtrl.hidelabel || !directiveCtrl.isRequired">*</label>';

                tmplString += '<form name="dateForm" class="ngSoeDatePickerForm"><div class="ngSoeDatePicker" data-ng-class="{\'hide-today-button\': directiveCtrl.hideTodayButton, \'hide-clear-button\': directiveCtrl.hideClearButton, \'input-group input-group-sm \': (!directiveCtrl.disabled && !directiveCtrl.isReadonly)}">';
                tmplString += '<input class="form-control input-sm" type="text" data-l10n-bind data-l10n-bind-placeholder="directiveCtrl.placeholderKey" data-ng-model="directiveCtrl.model" parse-date uib-datepicker-popup="{{directiveCtrl.format}}" datepicker-options="directiveCtrl.options" data-ng-hide="directiveCtrl.hidevalue" data-ng-disabled="directiveCtrl.disabled" is-open="directiveCtrl.isOpen"';
                if (!attrs['hidevalue'])
                    tmplString += ' data-ng-model-options="{updateOn: \'blur\'}"';

                if (attrs['inputWidth'])
                    tmplString += ' style="width:' + attrs['inputWidth'] + ';"';

                if (attrs['isReadonly'])
                    tmplString += ' data-ng-readonly="directiveCtrl.isReadonly"';
                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{directiveCtrl.tabIndex}}"';

                if (attrs['useIgnoreDirty']) {
                    tmplString += ' ignore-dirty ng-change="directiveCtrl.restoreDirtyState();directiveCtrl.onChange({ date: directiveCtrl.model});"';
                } else {
                    tmplString += ' ng-change="directiveCtrl.onChange({date: directiveCtrl.model});"';
                }

                tmplString += ' />';

                if (attrs["description"])
                    tmplString += '<span class="description">{{directiveCtrl.description}}</span>'
                var classString: string = '';
                if (attrs['buttonClass']) {
                    classString = attrs['buttonClass'];
                }
                tmplString += '<span class="input-group-btn datepicker-button">' +
                    '<button type="button" tabindex="-1" class="btn btn-default skip-tab-with-enter ' + classString + '" data-ng-hide="directiveCtrl.disabled || directiveCtrl.isReadonly" data-ng-click="directiveCtrl.checkDirtyState();directiveCtrl.isOpen=true;"><i class="fal fa-calendar-alt"></i></button>' +
                    '</span>';

                tmplString += '</div></form>';
                tmplString += '<span data-ng-if="directiveCtrl.showWeek">{{directiveCtrl.model.week()}}</span>'

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);
                elem.setAttribute('parentdateform', '');
                attrs.$set("date", true);

                DirectiveHelper.applyAttributes([elem], attrs, 'directiveCtrl');

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                labelKey: '@',
                hidelabel: '=',
                labelValue: '=?',
                labelPrefixValue: '=?',
                labelPostfixValue: '=?',
                labelValueInParentheses: '@',
                labelValueIndiscreet: '@',
                hideLabelValue: '=?',
                description: '=',
                hidevalue: '=',
                placeholderKey: '@',
                model: '=',
                secondModel: '=?',
                validateDateRange: '@',
                isStartInRange: '@',
                allowInvalid: '@',
                showWeek: '@',
                showWeekDay: '@',
                disabled: '=isDisabled',
                inputWidth: '@',
                options: '=?',
                isReadonly: '=',
                tabIndex: '=?',
                onChange: '&',
                isRequired: '=?',
                useIgnoreDirty: '=?',
                hideTodayButton: '=?',
                hideClearButton: '=?',
                noBorderRadius: "=",
                minDate: '=?',
                maxDate: '=?',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);

                scope['isOpen'] = false;
                scope['datepickerConfig'] = uibDatepickerConfig;
                scope['datepickerPopupConfig'] = uibDatepickerPopupConfig;
                scope["format"] = $locale.DATETIME_FORMATS.shortDate;

                const keys: string[] = [
                    "core.datepicker.current",
                    "core.datepicker.clear",
                    "core.datepicker.close"
                ];

                translationService.translateMany(keys).then((terms) => {
                    (<any>uibDatepickerPopupConfig).currentText = terms["core.datepicker.current"];
                    (<any>uibDatepickerPopupConfig).clearText = terms["core.datepicker.clear"];
                    (<any>uibDatepickerPopupConfig).closeText = terms["core.datepicker.close"];
                });
            },
            restrict: 'E',
            replace: true,
            controller: DatepickerController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class DatepickerController {

    model: any;
    secondModel: any;
    validateDateRange: boolean;
    isStartInRange: boolean;
    minDate: Date | undefined;
    maxDate: Date | undefined;

    wasDirty: boolean = false;

    options: any;

    private get modelDate(): Date {
        return CalendarUtility.convertToDate(this.model);
    }

    private get modelSecondDate(): Date {
        return CalendarUtility.convertToDate(this.secondModel);
    }

    private get hasDate(): boolean {
        return !!this.model;
    }

    private get weekDay(): string {
        return this.modelDate ? this.modelDate.format('dddd') : '';
    }

    //@ngInject
    constructor(translationService: ITranslationService, private $element, private $timeout: ng.ITimeoutService, private $scope) {
        $($element).on('focus', 'input', e => {
            this.checkDirtyState();
            $(e.currentTarget).select();
        });
    }

    public $onInit() {
        // Check if model is a valid date but not actually a Date type.
        // In that case convert to Date type.
        if (this.model && !(this.model instanceof Date) && CalendarUtility.isValidDate(this.model))
            this.model = CalendarUtility.convertToDate(this.model);

        if (this.validateDateRange) {
            $(this.$element).on('blur', 'input', e => {
                this.$timeout(() => {
                    let isValid: boolean = true;
                    if (this.modelDate && this.modelSecondDate && ((this.isStartInRange && this.modelDate.isAfterOnDay(this.modelSecondDate)) || (!this.isStartInRange && this.modelSecondDate.isAfterOnDay(this.modelDate))))
                        isValid = false;

                    let ngModelController: ng.INgModelController = $(e.currentTarget).controller('ngModel');
                    if (ngModelController)
                        ngModelController.$setValidity("dateRange", isValid);

                    let parentdateform = $(e.currentTarget).closest("div:has([parentdateform])").children('[parentdateform]');
                    if (parentdateform) {
                        if (!isValid)
                            parentdateform.addClass('has-error');
                        else
                            parentdateform.removeClass('has-error');
                    }
                });
            });
        }

        this.options = {
            ...this.options,
            minDate: this.minDate,
            maxDate: this.maxDate,
        }
    }

    public checkDirtyState() {
        if (this.$scope.dateForm && !this.$scope.directiveCtrl.useIgnoreDirty)
            this.wasDirty = this.$scope.dateForm.$$parentForm.$dirty;
    }

    public restoreDirtyState() {
        if (!this.wasDirty && this.$scope.dateForm) {
            this.$scope.dateForm.$$parentForm.$setPristine();
        }
    }
}
