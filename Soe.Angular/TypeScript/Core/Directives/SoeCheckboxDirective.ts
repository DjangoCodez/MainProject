import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeCheckboxDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label data-ng-class="{\'inline\': inline, \'indiscreet\': indiscreet';
                if (attrs['textClass'])
                    tmplString += ', \'' + attrs['textClass'] + '\': textClassCondition';
                tmplString += ' }" data-l10n-bind data-l10n-bind-title="tooltipKey"';

                if (attrs['textWidth'])
                    tmplString += ' style="width:' + attrs['textWidth'] + ';"';

                tmplString += '>';


                tmplString += '<input type="checkbox" class="form-check-input" data-ng-model="model" data-ng-disabled="disabled || isReadonly" data-ng-change="onChanging({item: model})"';

                if (attrs['useIgnoreDirty'])
                    tmplString += ' ignore-dirty';

                tmplString += '>';

                tmplString += '<span class="form-check-label" data-ng-class="{\'mark-checked\': markChecked}" data-ng-if="labelKey" data-l10n-bind="labelKey"></span>';

                if (attrs['labelValue'] !== undefined) {
                    tmplString += ' ';
                    tmplString += '<span data-ng-show="labelValueInParentheses && labelValue !== undefined">(</span>';
                    tmplString += '<span>{{labelValue}}</span>';
                    tmplString += '<span data-ng-show="labelValueInParentheses && labelValue !== undefined">)</span>';
                }

                tmplString += '<span data-ng-class="{\'mark-checked\': markChecked}" data-ng-if="!labelKey">{{label}}</span></label>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                elem.className = "checkbox ngSoeCheckBox";
                if (attrs['condensed'])
                    elem.className += " condensed";

                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                labelKey: '@',
                hidelabel: '=?',
                labelValue: '=?',
                labelValueInParentheses: '@',
                label: '@',
                indiscreet: '@',
                tooltipKey: '@',
                textClass: '@',
                textClassCondition: '=?',
                textWidth: '@',
                inline: '=?',
                condensed: '=?',
                model: '=?',
                markChecked: '@',
                disabled: '=?isDisabled',
                isReadonly: '=?',
                onChanging: '&',
                useIgnoreDirty: '=?'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
        };
    }
}