import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeRadiobuttonDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label>' +
                    '<input type="radio" data-ng-class="{\'inline\': inline}" data-ng-model="model" data-ng-disabled="disabled || isReadonly" data-ng-change="onChanging({item: model})">' +
                    '<span data-l10n-bind="labelKey"></span>' +
                    '</label>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                elem.className = "radio";

                DirectiveHelper.applyAttributes([elem], attrs, null);

                DirectiveHelper.getInputElement([elem]).setAttribute("name", "{{group}}");
                DirectiveHelper.getInputElement([elem]).setAttribute("ng-value", "{{value}}");

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                labelKey: '@',
                hidelabel: '=',
                model: '=',
                group: '@',
                value: '=',
                inline: '=?',
                disabled: '=isDisabled',
                isReadonly: '=',
                onChanging: '&'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
        };
    }
}