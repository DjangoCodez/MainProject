import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeInstructionDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="info ngSoeInstructionLabel';
                if (!attrs['inline'])
                    tmplString += ' control-label';

                if (attrs['fullWidth'] && attrs['fullWidth'] == 'true')
                    tmplString += ' full-width';

                if (attrs['labelClass'])
                    tmplString += ' ' + attrs['labelClass'];

                tmplString += '" data-ng-class="{\'warning\': isWarning}"';

                if (attrs['inline'])
                    tmplString += ' style="margin: 0px;"';

                if (attrs['labelKey'])
                    tmplString += ' data-l10n-bind="labelKey"';

                tmplString += ' data-ng-hide="hidelabel">{{model}}</label>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                labelKey: '@',
                hidelabel: '=?',
                model: '=?',
                inline: '@',
                fullWidth: '@',
                labelClass: '@',
                isWarning: '=?'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
        };
    }
}