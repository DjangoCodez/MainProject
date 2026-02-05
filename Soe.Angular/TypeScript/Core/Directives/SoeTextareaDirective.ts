import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeTextareaDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-l10n-bind="labelKey" data-ng-hide="hidelabel"></label>';

                if (attrs['labelValue']) {
                    tmplString += '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="!hideLabelValue && labelValue && labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="!hideLabelValue && labelPrefixValue" style="padding-right: 3px;">{{labelPrefixValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="!hideLabelValue && labelValue">{{labelValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="!hideLabelValue && labelPostfixValue" style="padding-left: 3px;">{{labelPostfixValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="!hideLabelValue && labelValue && labelValueInParentheses">)</label>';
                }

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel">*</label>';
                else if (attrs['isRequired'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel || !isRequired">*</label>';

                if (attrs['showLength']) {
                    tmplString += '<span class="pull-right margin-small-top">{{model.length || 0}}';
                    if (attrs['maxLength'])
                        tmplString += '/' + attrs['maxLength'];
                    tmplString += '</span>';
                }

                // If onButtonClick function is specified, add button for it
                if (attrs['onButtonClick']) {
                    tmplString += '<div class="input-group">';
                }

                tmplString += '<textarea class="form-control" ';
                if (attrs['inputClass'])
                    tmplString += 'data-ng-class="{\'' + attrs['inputClass'] + '\': inputClassCondition} ';
                if (attrs['autoSize']) {
                    tmplString += 'textarea-auto-size ';
                    if (attrs['shrinkOnBlur'])
                        tmplString += 'shrink-on-blur="true"';
                }
                tmplString += 'data-ng-model="model" data-l10n-bind data-l10n-bind-placeholder="placeholderKey" rows="{{rows}}" data-ng-change="onChange({item: model})" data-ng-focus="onFocus()" data-ng-blur="onBlur()" data-ng-disabled="disabled"';

                if (attrs['isReadonly'])
                    tmplString += ' data-ng-readonly="isReadonly"';

                if (attrs['setFocus'])
                    tmplString += ' set-focus="{{setFocus}}"';

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{tabIndex}}"';

                if (attrs['useIgnoreDirty'])
                    tmplString += ' ignore-dirty';

                // ngModelOptions
                if (attrs['updateOn'])
                    tmplString += ' data-ng-model-options="{updateOn: \'' + attrs['updateOn'] + '\'' + '}"';

                if (attrs['noTrim'])
                    tmplString += ' ng-trim="false"';

                tmplString += '></textarea>';

                if (attrs['onButtonClick']) {
                    var classStr = "";
                    if (attrs['buttonClass'])
                        classStr += attrs['buttonClass'] + ' ';
                    tmplString += '<span class="' + classStr + 'input-group-addon" data-ng-class="{\'disabled\': buttonDisabled || disabled || isReadonly}" data-l10n-bind data-l10n-bind-title="buttonTooltipKey" data-ng-click="!buttonDisabled && !disabled && !isReadonly && onButtonClick()" data-ng-disabled="buttonDisabled || disabled || isReadonly">';
                    tmplString += '<i class="fal ' + attrs['buttonIcon'] + '" data-ng-disabled="buttonDisabled || disabled || isReadonly"></i>';
                    tmplString += '</span></div>';
                }

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

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
                placeholderKey: '@',
                model: '=',
                rows: '=',
                autoSize: '@',
                shrinkOnBlur: '@',
                inputClass: '@',
                inputClassCondition: '=',
                updateOn: '@',
                noTrim: '@',
                disabled: '=isDisabled',
                isReadonly: '=',
                isRequired: '=',
                onChange: '&',
                onFocus: '&',
                onBlur: '&',
                onButtonClick: '&',
                buttonIcon: '=',
                buttonTooltipKey: '@',
                buttonDisabled: '=',
                maxLength: '=',
                showLength: '@',
                setFocus: '=',
                tabIndex: '=?',
                buttonClass: '@'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
                if (attrs['maxLength']) {
                    element[0].getElementsByTagName("textarea")[0].setAttribute('maxlength', scope['maxLength']);
                }
            },
            restrict: 'E',
            replace: true,
        };
    }
}