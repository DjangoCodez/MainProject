import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeInfobarDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var elem = DirectiveHelper.createTemplateElement(
                    '<div class="info form-group form-group-sm" data-ng-if="showInfo">' +
                    '<div class="margin-none-left row">' +
                    '<label class="margin-large-right" data-l10n-bind="infoMessage"></label>' +
                    '<i data-ng-repeat="button in buttons" class="{{::\'btn btn-default \' + button.library + \' \' + button.iconClass}}"' +
                    'data-ng-class="{disabled: button.disabled()}"' +
                    'data-ng-hide="button.hidden()"' +
                    'data-l10n-bind data-l10n-bind-title="button.titleKey" data-ng-click="$event.stopPropagation();!button.disabled() && button.click()">' +
                    '<span data-ng-if="button.labelKey" data-l10n-bind="button.labelKey"></span>' +
                    '<span data-ng-if="!button.labelKey && button.labelValue">{{button.labelValue}}</span>' +
                    '</i>' +
                    '</div></div>',
                    attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                showInfo: '=?',
                infoMessage: '=?',
                buttons: '=?',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            transclude: true
        };
    }
}