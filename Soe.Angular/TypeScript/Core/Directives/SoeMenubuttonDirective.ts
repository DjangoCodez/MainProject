import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeMenubuttonDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var elem = DirectiveHelper.createTemplateElement(
                    '<button type="button" class="btn btn-sm btn-default dropdown-toggle ngSoeMenuButton" data-ng-class="{\'ngSoeMainButton\': directiveCtrl.mainButton}" data-ng-disabled="directiveCtrl.disabled()" data-toggle="dropdown">' +
                    '<span>{{directiveCtrl.optionName}}&nbsp;</span>' +
                    '<span class="caret"></span>' +
                    '</button>' +
                    '<ul class="dropdown-menu ngSoeMainDropdownMenu">' +
                    '<li ng-repeat="option in directiveCtrl.options">' +
                    '<a data-ng-if="option.name" href="" data-ng-class="{\'disabled-link\': option.disabled()}" data-ng-hide="option.hidden()" data-ng-click="directiveCtrl.click(option)"><i class="fa-fw {{option.icon}}"></i>&nbsp; {{option.name}}</a>' +
                    '<div data-ng-if="!option.name" data-ng-hide="option.hidden()" role="separator" class="divider"></div>' +
                    '</li>' +
                    '</ul>',
                    attrs);

                elem.setAttribute("data-ng-class", "{\'dropup\': directiveCtrl.settings.dropUp,\'dropdown\': !directiveCtrl.settings.dropUp, \'pull-right\': !directiveCtrl.settings.pullLeft, \'pull-left\': directiveCtrl.settings.pullLeft }");

                DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                labelKey: '@',
                options: '=',
                mainButton: '@',
                disabled: '&isDisabled',
                optionSelected: '&',
                dropUp: '@',
                pullLeft: '@',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: MenubuttonController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class MenubuttonController {

    options: any[];
    optionSelected: (option: any) => void;
    optionName: any;
    disabled: Function;
    labelKey: any;

    settings = {
        dropUp: false,
        pullLeft: false
    }
    dropUp: any;
    pullLeft: any;

    //@ngInject
    constructor(private translationService: ITranslationService) {
    }

    public $onInit() {
        if (this.dropUp) {
            this.settings.dropUp = this.dropUp != 'false';
        }

        if (this.pullLeft) {
            this.settings.pullLeft = this.pullLeft != 'false';
        }

        if (!this.labelKey) {
            this.labelKey = "core.choose";
        }

        this.translationService.translate(this.labelKey).then((term) => {
            this.optionName = term;
        });

        if (!this.disabled)
            this.disabled = () => false;
    }

    private click(option) {
        if (!option.disabled || !option.disabled())
            this.optionSelected({ option: option }); //notify parent
    }
}