import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeLabelDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                
                var tmplString: string = '<label class="control-label" data-ng-class="{\'discreet\': discreet, \'lowercase\': lowercase, \'smallFont\': smallFont, \'mediumFont\': mediumFont';

                if (attrs['textClass'])
                    tmplString += ', \'' + attrs['textClass'] + '\': textClassCondition';

                tmplString += '}" data-l10n-bind-title="tooltipKey" data-ng-hide="hidelabel"';
                if (attrs['labelKey']) {
                    tmplString += 'data-l10n-bind="labelKey"';
                }

                if (attrs['textWidth'])
                    tmplString += ' style="width:' + attrs['textWidth'] + ';"';

                tmplString += '>{{model}}</label>';

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

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                labelKey: '@',
                tooltipKey: '@',
                hidelabel: '=?',
                labelValue: '=?',
                labelPrefixValue: '=?',
                labelPostfixValue: '=?',
                labelValueInParentheses: '@',
                labelValueIndiscreet: '@',
                lowercase: '@',
                discreet: '@',
                smallFont: '@',
                mediumFont: '@',
                model: '=?',
                textClass: '@',
                textClassCondition: '=?',
                textWidth: '@'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
        };
    }
}