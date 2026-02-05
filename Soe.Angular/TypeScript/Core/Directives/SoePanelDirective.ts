import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoePanelDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<div class="panel-heading">' +
                    '<label class="control-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-hide="directiveCtrl.hidelabel"></label>';

                if (attrs['labelValue']) {
                    tmplString += '<span style="margin-left: 5px;" data-ng-show="directiveCtrl.hidelabel"></span>';

                    tmplString += '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass" data-ng-show="directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">(</label>' +
                        '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass" data-ng-show="directiveCtrl.labelValue">{{directiveCtrl.labelValue}}</label>' +
                        '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass" data-ng-show="directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">)</label>';
                }

                if (attrs['buttonIcon']) {
                    tmplString += '<span class="panel-button pull-right" data-l10n-bind data-l10n-bind-title="directiveCtrl.buttonTooltipKey" data-ng-hide="directiveCtrl.buttonHidden" data-ng-disabled="directiveCtrl.buttonDisabled" data-ng-click="directiveCtrl.onButtonClick()">';
                    tmplString += '<i class="fal ' + attrs['buttonIcon'] + '"></i>';
                    if (attrs['buttonLabelKey'])
                        tmplString += '<label class="control-label link" data-l10n-bind="directiveCtrl.buttonLabelKey"></label>';
                    tmplString += '</span>';
                }

                if (attrs['icon']) {
                    tmplString += '<span class="panel-icon pull-right">';
                    tmplString += '<i class="fas ' + attrs['icon'] + '" data-ng-class="directiveCtrl.iconClass"';
                    if (attrs['iconColor'])
                        tmplString += ' data-ng-style="{\'color\': directiveCtrl.iconColor}"';
                    tmplString += '> </i>';
                    tmplString += '</span>';
                }

                tmplString += '</div>' +
                    '<div class="panel-body" data-ng-class="{\'panel-body-condensed\': directiveCtrl.condensed}" data-ng-transclude></div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                elem.className = "panel panel-default";

                return elem.outerHTML;
            },
            scope: {
                form: '=',
                labelKey: '@',
                labelValue: '=',
                labelValueInParentheses: '@',
                labelValueClass: '=?',
                hidelabel: '=',
                onButtonClick: '&',
                buttonIcon: '@',
                buttonLabelKey: '@',
                buttonTooltipKey: '@',
                buttonHidden: '=?',
                buttonDisabled: '=?',
                icon: '=?',
                iconClass: '=?',
                iconColor: '=?',
                condensed: '@'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
            },
            restrict: 'E',
            transclude: true,
            controller: PanelController,
            controllerAs: 'directiveCtrl',
            bindToController: true,
        };
    }
}

class PanelController {
    onOpen: Function;
    onClose: Function;

    status = {
        open: false,
        disabled: false
    }

    isOpen: any;
    isDisabled: any;

    //@ngInject
    constructor(translationService: ITranslationService, private $scope: ng.IScope) {

    }

    $onInit() {
        if (this.isOpen) {
            this.status.open = this.isOpen != 'false';
        }
        if (this.isDisabled) {
            this.status.disabled = this.isDisabled != 'false';
        }

        if (this.onOpen || this.onClose) {
            var hasBeenOpened = false;//we dont want to trigger onClose when the accordion first loads in closed mode, so we use this to make sure it has been opened at least once before starting to trigger the closed event.
            this.$scope.$watch(() => this.isOpen, (isOpen) => {

                if (isOpen && this.onOpen) {
                    hasBeenOpened = true;
                    this.onOpen();
                }

                if (!isOpen && this.onClose && hasBeenOpened) {
                    this.onClose();
                }
            });
        }
    }
}