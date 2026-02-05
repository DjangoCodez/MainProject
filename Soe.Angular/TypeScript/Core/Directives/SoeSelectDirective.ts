import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeSelectDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-ng-class="{\'discreet\': labelDiscreet, \'margin-small-right\': inline}" data-l10n-bind="labelKey" data-ng-hide="hidelabel"></label>';

                if (attrs['labelValue']) {
                    tmplString += '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="labelValue && labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-style="{\'margin-left\': labelValueInParentheses ? \'0px\' : \'3px\'}" data-ng-show="labelValue">{{labelValue}}</label>' +
                        '<label class="control-label" data-ng-class="{\'discreet\': !labelValueIndiscreet}" data-ng-show="labelValue && labelValueInParentheses">)</label>';
                }

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel">*</label>';
                else if (attrs['isRequired'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel || !isRequired">*</label>';

                // If onButtonClick function is specified, add button for it
                if (attrs['onButtonClick'] || attrs['onEditClick']) {
                    tmplString += '<div class="input-group">';
                }
                var selectClasses = "form-control input-sm";
                if (attrs['selectClass']) {
                    selectClasses += " " + attrs['selectClass'];
                }
                tmplString += '<select class="' + selectClasses + '" data-ng-class="{\'ngSoeMainSelect\': mainButton';
                if (attrs['inputClass'])
                    tmplString += ', \'' + attrs['inputClass'] + '\': inputClassCondition';
                tmplString += '}" data-ng-model="model" data-ng-options="{{options}}"';

                if (attrs['inputWidth'])
                    tmplString += ' style="width:' + attrs['inputWidth'] + ';"';

                if (attrs['isReadonly'])
                    tmplString += ' data-ng-readonly="isReadonly" data-ng-disabled="isReadonly"';
                else
                    tmplString += ' data-ng-disabled="disabled"';

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{tabIndex}}"';

                if (attrs['useIgnoreDirty'])
                    tmplString += ' ignore-dirty';

                tmplString += ' data-ng-change="onChanging({item: model})" data-ng-click="onClicking({item: model})"></select>';

                if (attrs['onButtonClick']) {
                    tmplString += '<span class="input-group-btn">';
                    tmplString += '<button type="button" class="btn btn-sm btn-default fal {{buttonIcon}}';
                    if (attrs['buttonClass'])
                        tmplString += ' ' + attrs['buttonClass'];
                    if (attrs['clickButtonClass'])
                        tmplString += ' ' + attrs['clickButtonClass'];
                    tmplString += '" data-ng-class="{\'ngSoeMainSelectButton\': mainButton}" style="margin-left: -1px;" data-l10n-bind data-l10n-bind-title="buttonTooltipKey" data-ng-click="onButtonClick()" data-ng-disabled="buttonDisabled || disabled || isReadonly"></button>';
                    tmplString += '</span>';
                }

                if (attrs['onEditClick']) {
                    tmplString += '<span class="input-group-btn">';
                    tmplString += '<button type="button" class="btn btn-sm btn-default fal ' + attrs['editIcon'];
                    if (attrs['buttonClass'])
                        tmplString += ' ' + attrs['buttonClass'];
                    if (attrs['editButtonClass'])
                        tmplString += ' ' + attrs['editButtonClass'];
                    tmplString += '" data-ng-class="{\'ngSoeMainSelectButton\': mainButton}" style="margin-left: -1px;" data-l10n-bind data-l10n-bind-title="editTooltipKey" data-ng-click="onEditClick()" data-ng-disabled="editDisabled || disabled || isReadonly"></button>';
                    tmplString += '</span>';
                }

                if (attrs['onButtonClick'] || attrs['onEditClick']) {
                    tmplString += '</div>';
                }

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

                if (attrs['inlineBlock'])
                    elem.style.display = 'inline-block';

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                model: '=?',
                items: '=?',
                setselectedindex: '=?',
                options: '@',
                mainButton: '@',
                labelKey: '@',
                labelDiscreet: '@',
                hidelabel: '=?',
                labelValue: '=?',
                labelValueInParentheses: '@',
                labelValueIndiscreet: '@',
                inline: '@',
                inlineBlock: '@',
                disabled: '=?isDisabled',
                inputClass: '@',
                inputClassCondition: '=?',
                inputWidth: '@',
                isReadonly: '=?',
                tabIndex: '=?',
                onChanging: '&',
                onClicking: '&',
                onButtonClick: '&',
                buttonIcon: '=?',
                buttonTooltipKey: '@',
                buttonClass: '@',
                selectClass: '@',
                clickButtonClass: '@',
                editButtonClass: '@',
                buttonDisabled: '=?',
                onEditClick: '&',
                editIcon: '=?',
                editTooltipKey: '@',
                editDisabled: '=?',
                isRequired: '=?',
                useIgnoreDirty: '=?',
                autoFocus: '@',
            },
            link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: SelectController,
        };
    }
}

class SelectController {

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
        const elem = this.$element.find('select')[0];

        // This is needed in case we show/hide the element during load
        if (this.autoFocus === 'true') {
            elem.focus();
        }

        $(this.$element).on('focus', 'select', e => {
            $(e.currentTarget).select();
        });
    }
}