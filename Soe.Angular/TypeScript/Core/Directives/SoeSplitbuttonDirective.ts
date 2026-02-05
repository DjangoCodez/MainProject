import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeSplitbuttonDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<button type="button" class="btn btn-sm btn-default ngSoeSplitButton" data-ng-class="{\'ngSoeMainButton\': directiveCtrl.mainButton}" data-ng-disabled="directiveCtrl.disabled() || directiveCtrl.options[0].disabled()"';

                if (attrs['buttonName'])
                    tmplString += ' name="' + attrs['buttonName'] + '"';

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{directiveCtrl.tabIndex}}"';

                var classStr = "";
                if (attrs['lastButtonClass']) {
                    classStr = attrs['lastButtonClass'];
                }
                //var lastButtonStatement = "";
                //if (attrs['extrastatementonlastbutton']) {
                //    lastButtonStatement = ", " + attrs['extrastatementonlastbutton'];
                //}

                tmplString += ' data-ng-click="directiveCtrl.click()">{{directiveCtrl.optionName}}</button>' +
                    '<button type="button" class="btn btn-sm btn-default ngSoeSplitButton dropdown-toggle ' + classStr + '" data-ng-class="{\'ngSoeMainButton\': directiveCtrl.mainButton}" data-ng-disabled="directiveCtrl.disabled()" data-toggle="dropdown">' +
                    '<span class="caret"></span>' +
                    '</button>' +
                    '<ul class="dropdown-menu ngSoeMainDropdownMenu">' +
                    '<li data-ng-repeat="option in directiveCtrl.options">' +
                    '<a data-ng-if="option.name" href="" data-ng-class="{\'disabled-link\': option.disabled()}" data-ng-hide="option.hidden()" data-ng-click="directiveCtrl.menuClick(option)"><i class="{{option.icon}}"></i>&nbsp; {{option.name}}</a>' +
                    '<div data-ng-if="!option.name" data-ng-hide="option.hidden()" role="separator" class="divider"></div>' +
                    '</li>' +
                    '</ul>' +
                    '</div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                elem.setAttribute("data-ng-class", "{\'btn-group\':true, \'dropup\': directiveCtrl.settings.dropUp,\'dropdown\': !directiveCtrl.settings.dropUp, \'pull-right\': !directiveCtrl.settings.pullLeft, \'pull-left\': directiveCtrl.settings.pullLeft }");

                DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                buttonName: '=',
                labelKey: '@',
                options: '=',
                selectedOption: '=?',
                mainButton: '@',
                disabled: '&isDisabled',
                tabIndex: '=',
                optionSelected: '&',
                dropUp: '@',
                pullLeft: '@',
                lastButtonClass: "=",
                extraStatementOnLastButton: "=",
                invalid: '@',
                firstOptionAsLabel: "="
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            controller: SplitbuttonController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class SplitbuttonController {

    options: any[];
    selectedOption: any;
    optionSelected: (option: any) => void;
    optionName: any;
    disabled: Function;
    labelKey: any;
    firstOptionAsLabel: boolean;

    settings = {
        dropUp: false,
        pullLeft: false
    }

    dropUp: any;
    pullLeft: any;

    //@ngInject
    constructor(private $scope: ng.IScope, private translationService: ITranslationService) {
        
    }

    $onInit() {
        if (this.dropUp) {
            this.settings.dropUp = this.dropUp != 'false';
        }
        if (this.pullLeft) {
            this.settings.pullLeft = this.pullLeft != 'false';
        }
        
        this.selectedOption = _.find(this.options, (o) => { return !(o.hidden && o.hidden()) && !(o.disabled && o.disabled()) });

        if (!this.labelKey && (!this.firstOptionAsLabel || !this.selectedOption)) {
            this.labelKey = "core.choose";
        }

        if (this.labelKey) {
            this.translationService.translate(this.labelKey).then((text) => {
                this.optionName = text;
            });
        }
        else if (this.firstOptionAsLabel && this.selectedOption) {
            this.optionName = this.selectedOption.name;
        }

        if (!this.disabled)
            this.disabled = () => false;

        
        
        this.$scope.$watch(() => this.selectedOption, (newValue, oldValue, scope) => {
            if (newValue === oldValue)
                return;
            this.setSelectedOption(this.selectedOption);
        });
    }

    private click() {
        // First item in list must be the default item
        if (!this.selectedOption)
            this.selectedOption = _.find(this.options, (o) => { return (!o.hidden || !o.hidden()) && (!o.disabled || !o.disabled()) });

        this.notifyParent();
    }

    private menuClick(option) {
        this.setSelectedOption(option);
        this.notifyParent();
    }

    private setSelectedOption(option) {
        if (option) {
            this.selectedOption = option;
            this.optionName = this.selectedOption.name;
        }
    }

    private notifyParent() {
        if (this.selectedOption && (!this.selectedOption.disabled || !this.selectedOption.disabled()))
            this.optionSelected({ option: this.selectedOption }); //notify parent
    }
}