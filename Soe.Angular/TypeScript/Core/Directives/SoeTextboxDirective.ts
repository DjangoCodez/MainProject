import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeTextboxDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                let tmplString: string = '<label class="control-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-hide="directiveCtrl.hidelabel"></label>';
                if (attrs['labelValue']) {
                    tmplString += '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelPrefixValue" style="padding-right: 3px;">{{directiveCtrl.labelPrefixValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue">{{directiveCtrl.labelValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelPostfixValue" style="padding-left: 3px;">{{directiveCtrl.labelPostfixValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !directiveCtrl.labelValueIndiscreet}" data-ng-show="!directiveCtrl.hideLabelValue && directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">)</label>';
                }

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="directiveCtrl.hidelabel">*</label>';
                else if (attrs['isRequired'])
                    tmplString += '<label class="required-label" data-ng-hide="directiveCtrl.hidelabel || !directiveCtrl.isRequired">*</label>';

                // If onButtonClick function is specified, add button for it
                if (attrs['onPrefixButtonClick'] || attrs['onButtonClick']) {
                    tmplString += '<div class="input-group">';

                    if (attrs['onPrefixButtonClick']) { 
                        tmplString += '<span class="input-group-addon" data-ng-style="{\'background-color\': directiveCtrl.prefixButtonDisabled ? \'rgb(247, 244, 240)\' : \'#FFFFFF\', \'color\': directiveCtrl.prefixButtonDisabled ? \'rgb(182, 182, 182)\' : \'#00000\'}">';
                        tmplString += '<span class="fal ' + attrs['prefixButtonIcon'];
                        if (attrs['prefixButtonClass'])
                            tmplString += ' ' + attrs['prefixButtonClass'];
                        tmplString += '" data-ng-class="{\'link\': !directiveCtrl.prefixButtonDisabled}"';
                        tmplString += ' data-l10n-bind data-l10n-bind-title="directiveCtrl.prefixButtonTooltipKey"';
                        tmplString += ' data-ng-click="directiveCtrl.prefixButtonDisabled ? false : directiveCtrl.onPrefixButtonClick()"';
                        
                        tmplString +=' data-ng-disabled="directiveCtrl.prefixButtonDisabled || directiveCtrl.disabled"></span></span>';
                    }
                }

                tmplString += '<input class="form-control input-sm';
                if (attrs['inputClassNoCondition'])
                    tmplString += ' ' + attrs['inputClassNoCondition'];
                tmplString += '" data-l10n-bind-title="directiveCtrl.tooltipKey" data-ng-class="{\'numeric\': directiveCtrl.numeric || directiveCtrl.numericNotZero';
                if (attrs['inputClass'])
                    tmplString += ', \'' + attrs['inputClass'] + '\': directiveCtrl.inputClassCondition';
                tmplString += '}" type="{{directiveCtrl.inputType}}" data-l10n-bind data-l10n-bind-placeholder="directiveCtrl.placeholderKey" data-ng-model="directiveCtrl.model" data-ng-disabled="directiveCtrl.disabled"';

                // ngModelOptions
                if (attrs['allowInvalid'] || attrs['updateOn']) {
                    tmplString += ' data-ng-model-options="{';
                    if (attrs['allowInvalid'])
                        tmplString += 'allowInvalid: ' + attrs['allowInvalid'];
                    if (attrs['updateOn']) {
                        if (attrs['allowInvalid'])
                            tmplString += ', ';
                        tmplString += 'updateOn: \'' + attrs['updateOn'] + '\'';
                    }
                    tmplString += '}"';
                }

                if ((attrs['numeric'] || attrs['numericNotZero']) && attrs['decimals']) {
                    let decimals: number = 0;
                    if (attrs['decimals'])
                        decimals = Number(attrs['decimals']);
                    tmplString += ' decimal="{0}"'.format(decimals.toString());

                    if (attrs['maxDecimals']) {
                        const maxDecimals: number = Number(attrs['maxDecimals']);
                        tmplString += ' max-decimals="{0}"'.format(maxDecimals.toString());
                    }
                }

                if (attrs['isTime'])
                    tmplString += ' parse-time';

                if (attrs['inputType'] && attrs['inputType'] == 'number' && attrs['min']) {
                    tmplString += ' min="' + attrs['min'] + '"';
                }

                if (attrs['inputType'] && attrs['inputType'] == 'number' && attrs['max']) {
                    tmplString += ' max="' + attrs['max'] + '"';
                }

                if (attrs['inputType'] && attrs['inputType'] == 'number' && (attrs['min'] || attrs['max']) && attrs['onInput']) {
                    tmplString += ' oninput="' + attrs['onInput'] + '"';
                }

                if (attrs['inputWidth'])
                    tmplString += ' style="width:' + attrs['inputWidth'] + ';"';

                if (attrs['isReadonly'])
                    tmplString += ' data-ng-readonly="directiveCtrl.isReadonly"';

                if (attrs['skipNotZeroValidation'])
                    tmplString += ' skip-not-zero-validation="directiveCtrl.skipNotZeroValidation"';

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{directiveCtrl.tabIndex}}"';

                if (attrs['setFocus'])
                    tmplString += ' set-focus="{{directiveCtrl.setFocus}}"';

                if (attrs['useIgnoreDirty'])
                    tmplString += ' ignore-dirty';

                tmplString += ' data-ng-change="directiveCtrl.onChange({model: directiveCtrl.model})" data-ng-focus="directiveCtrl.onFocus()" data-ng-blur="directiveCtrl.onBlur()" enter="directiveCtrl.onEnter()"> ';

                if (attrs['onButtonClick'] || attrs['onSecondButtonClick']) {
                    tmplString += '<span class="input-group-btn">';

                    if (attrs['onButtonClick']) {
                        tmplString += '<button type="button" class="btn btn-sm btn-default fal ' + attrs['buttonIcon'] + ' ';
                        if (attrs['buttonClass'])
                            tmplString += attrs['buttonClass'];
                        tmplString += '" ';

                        if (attrs['setFocusOnButton'])
                            tmplString += 'autofocus ';
                        tmplString += 'data-l10n-bind data-l10n-bind-title="directiveCtrl.buttonTooltipKey" data-ng-click="directiveCtrl.onButtonClick()" data-ng-disabled="directiveCtrl.buttonDisabled || directiveCtrl.disabled"></button>';
                    }
                    if (attrs['onSecondButtonClick']) {
                        tmplString += '<button type="button" class="btn btn-sm btn-default fal ' + attrs['secondButtonIcon'] + ' ';
                        if (attrs['secondButtonClass'])
                            tmplString += attrs['secondButtonClass'] + ' ';
                        tmplString += '"data-l10n-bind data-l10n-bind-title="directiveCtrl.secondButtonTooltipKey" data-ng-click="directiveCtrl.onSecondButtonClick()" data-ng-disabled="directiveCtrl.secondButtonDisabled || directiveCtrl.disabled"></button>';
                    }
                    else if (attrs['link']) {
                        tmplString += '<a href="{{directiveCtrl.link}}" data-ng-disabled="directiveCtrl.secondButtonDisabled || directiveCtrl.disabled"><button type="button" class="no-left-border-radius btn btn-sm btn-default fal ' + attrs['secondButtonIcon'] + ' ';
                        if (attrs['secondButtonClass'])
                            tmplString += attrs['secondButtonClass'] + ' ';
                        tmplString += '"></button></a>';
                    }

                    tmplString += '</span></div>';
                }

                if (attrs['onPrefixButtonClick'])
                    tmplString += '</div>';

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
                labelValueIndiscreet: '@',
                hideLabelValue: '=?',
                tooltipKey: '@',
                inputType: '@',
                placeholderKey: '@',
                model: '=?',
                numeric: '@',
                numericNotZero: '@',
                clearZero: '@',
                decimals: '@',
                isTime: '@',
                min: '@',
                max: '@',
                onInput: '@',
                skipNotZeroValidation: '=?',
                allowInvalid: '@',
                updateOn: '@',
                disabled: '=?isDisabled',
                inputClass: '@',
                inputClassCondition: '=?',
                inputClassNoCondition: '@',
                inputWidth: '@',
                isReadonly: '=?',
                tabIndex: '=?',
                autoFocus: '@',
                setFocus: '=?',
                setFocusOnButton: '@',
                onChange: '&',
                onFocus: '&',
                onBlur: '&',
                onEnter: '&',
                onPrefixButtonClick: '&',
                prefixButtonIcon: '=?',
                prefixButtonTooltipKey: '@',
                prefixButtonClass: '@',
                prefixButtonDisabled: '=?',
                onButtonClick: '&',
                buttonIcon: '=?',
                buttonTooltipKey: '@',
                buttonClass: '@',
                buttonDisabled: '=?',
                onSecondButtonClick: '&',
                secondButtonIcon: '=?',
                secondButtonTooltipKey: '@',
                secondButtonClass: '@',
                secondButtonDisabled: '=?',
                isRequired: '=',
                useIgnoreDirty: '=?',
                maxDecimals: '=?',
                link: '=?',
                maxLength: '=?',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: TextboxController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class TextboxController {

    form: string;
    inputid: string;
    labelKey: string;
    hidelabel: boolean;
    placeholderKey: string;
    inputType: string;
    model: any;
    allowInvalid: boolean;
    updateOn: string;
    autoFocus: string;
    setFocus: any;
    link: string;
    maxLength: number;

    //@ngInject
    constructor(private $element, private $timeout: ng.ITimeoutService) {
    }

    public $onInit() {
        // Default input type is text if not set
        if (!this.inputType) {
            this.inputType = "text";
        }

        const elem = this.$element.find('input')[0];

        if (this.maxLength) {
            $(elem).attr("maxlength", this.maxLength);
        } else {
            $(elem).removeAttr("maxlength");
        }

        // This is needed in case we show/hide the element during load
        if (this.autoFocus === 'true') {
            elem.focus();
        }

        $(this.$element).on('focus', 'input', e => {
            $(e.currentTarget).select();
        });
    }
}