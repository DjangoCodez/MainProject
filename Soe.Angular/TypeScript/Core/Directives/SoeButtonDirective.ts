import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeButtonDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template:
                '<button type="button" class="ngSoeButton btn-sm btn-default" data-l10n-bind="labelKey"></button>',
            scope: {
                form: '=',
                labelKey: '@',
                click: '&',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.addEvent(scope, element, null, "click");
            },
            restrict: 'E',
            replace: true,
        };
    }
}