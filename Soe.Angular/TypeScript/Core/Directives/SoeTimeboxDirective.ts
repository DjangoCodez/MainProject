import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeTimeboxDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-hide="directiveCtrl.hidelabel"></label>';

                if (attrs['labelValue']) {
                    tmplString += '<label class="control-label discreet" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                        '<label class="control-label discreet" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelPrefixValue" style="padding-right: 3px;">{{directiveCtrl.labelPrefixValue}}</label>' +
                        '<label class="control-label discreet" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue">{{directiveCtrl.labelValue}}</label>' +
                        '<label class="control-label discreet" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelPostfixValue" style="padding-left: 3px;">{{directiveCtrl.labelPostfixValue}}</label>' +
                        '<label class="control-label discreet" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">)</label>';
                }

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="directiveCtrl.hidelabel">*</label>';

                tmplString += '<input class="form-control input-sm" data-l10n-bind data-l10n-bind-placeholder="\'core.time.placeholder.hoursminutes\'" data-ng-model="directiveCtrl.model" data-ng-model-options="{updateOn: \'blur\'}" data-ng-disabled="directiveCtrl.disabled"';

                if (attrs['inputClass'])
                    tmplString += ' data-ng-class="{\'' + attrs['inputClass'] + '\': directiveCtrl.inputClassCondition}"';

                tmplString += ' parse-timebox';

                if (attrs['tooltipKey'])
                    tmplString += ' data-l10n-bind-title="directiveCtrl.tooltipKey"';
                else if (attrs['showDateAsTooltip'])
                    tmplString += ' data-ng-attr-title="{{directiveCtrl.model|date:\'shortDate\'}}"';

                if (attrs['allowEmpty'])
                    tmplString += ' allow-empty="true"';

                if (attrs['isTime'])
                    tmplString += ' is-time="true"';

                if (attrs['isReadonly'])
                    tmplString += ' data-ng-readonly="directiveCtrl.isReadonly"';

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{directiveCtrl.tabIndex}}"';

                if (attrs['setFocus'])
                    tmplString += ' set-focus="{{directiveCtrl.setFocus}}"';

                if (attrs['useIgnoreDirty'])
                    tmplString += ' ignore-dirty';

                tmplString += ' data-ng-change="directiveCtrl.onChange({model: directiveCtrl.model})" data-ng-focus="directiveCtrl.onFocus()" data-ng-blur="directiveCtrl.onBlur()"> ';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                inputid: '@',
                labelKey: '@',
                hidelabel: '=?',
                labelValue: '=?',
                labelPrefixValue: '=?',
                labelPostfixValue: '=?',
                labelValueInParentheses: '@',
                hideLabelValue: '=?',
                tooltipKey: '@',
                showDateAsTooltip: '@',
                model: '=?',
                allowEmpty: '=?',
                isTime: '=?',
                disabled: '=?isDisabled',
                inputClass: '@',
                inputClassCondition: '=?',
                isReadonly: '=?',
                tabIndex: '=?',
                autoFocus: '@',
                setFocus: '=?',
                onChange: '&',
                onFocus: '&',
                onBlur: '&',
                useIgnoreDirty: '=?'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: TimeboxController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class TimeboxController {

    form: string;
    inputid: string;
    labelKey: string;
    hidelabel: boolean;
    placeholderKey: string;
    model: any;
    autoFocus: string;
    setFocus: any;

    //@ngInject
    constructor(private $element) {
    }

    public $onInit() {
        // This is needed in case we show/hide the element during load
        if (this.autoFocus === 'true') {
            this.$element.find('input')[0].focus();
        }

        $(this.$element).on('focus', 'input', e => {
            $(e.currentTarget).select();
        });
    }
}