import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class CreatedModifiedDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var elem = DirectiveHelper.createTemplateElement(
                    '<div data-ng-if="model.created">' +
                    '<label data-l10n-bind="\'common.created\'"></label> <label class="discreet">{{model.created | date : \'shortDate\'}} {{model.created | date : \'shortTime\'}}</label> ' +
                    '<label data-ng-if="model.createdBy" data-l10n-bind="\'common.by\'"></label> <label class="discreet">{{model.createdBy}}</label>' +
                    '<br />' +
                    '</div>' +
                    '<div data-ng-if="model.modified">' +
                    '<label data-l10n-bind="\'common.modified\'"></label> <label class="discreet">{{model.modified | date : \'shortDate\'}} {{model.modified | date : \'shortTime\'}}</label> ' +
                    '<label data-ng-if="model.modifiedBy" data-l10n-bind="\'common.by\'"></label> <label class="discreet">{{model.modifiedBy}}</label>' +
                    '</div>',
                    attrs);

                elem.className = "createdModified";

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                model: '=',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
            },
            restrict: 'E',
            replace: true,
        };
    }
}