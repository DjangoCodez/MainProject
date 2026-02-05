import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeProgressbarDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<div class="edit-padding">';
                tmplString += '<uib-progressbar class="progress-striped active';

                if (attrs['customClass'])
                    tmplString += ' ' + attrs['customClass'];

                tmplString += '" type="info">';
                tmplString += '<span>{{message}}</span>' +
                    '</uib-progressbar>' +
                    '</div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                elem.setAttribute("data-ng-hide", "!isBusy");

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                isBusy: '=?',
                message: '=?',
                customClass: '=?'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
            },
            restrict: 'E',
            replace: true,
        };
    }
}