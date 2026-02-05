import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeTextDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label ngSoeLabel {0}"'.format((attrs['bold'] && attrs['bold'] === 'true') ? "indiscreet" : "discreet");

                if (attrs['textClass'])
                    tmplString += 'data-ng-class="{\'' + attrs['textClass'] + '\': textClassCondition}"';

                tmplString += '>{{model}}</label>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                model: '=?',
                bold: '@',
                textClass: '@',
                textClassCondition: '=?'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
        };
    }
}