import { DirectiveHelper } from "./DirectiveHelper";

// https://github.com/kaihenzler/angular-minicolors

export class SoeColorpickerDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-hide="directiveCtrl.hidelabel"></label>';
                tmplString += '<input minicolors="directiveCtrl.customSettings" class="form-control input-sm" data-ng-model="directiveCtrl.model" options="directiveCtrl.options">';

                    var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);
                    DirectiveHelper.applyAttributes([elem], attrs, 'directiveCtrl');

                return elem.outerHTML;
            },
            scope: {
                labelKey: '@',
                hidelabel: '=',
                model: '=',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: ColorpickerController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class ColorpickerController {

    customSettings: any;

    //@ngInject
    constructor() {
    }

    public $onInit() {
        this.customSettings = {
            position: 'bottom right',
            letterCase: 'uppercase'
        }
    }
}