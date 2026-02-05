import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeAccordionDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, $timeout: ng.ITimeoutService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '';

                tmplString = '<div uib-accordion-group data-ng-class="{\'soe-accordion-condensed\': directiveCtrl.condensed, \'soe-accordion-more-condensed\': directiveCtrl.moreCondensed}" is-open="directiveCtrl.isOpen" ';

                tmplString += 'class="soe-accordion {0}" '.format(attrs['accordionClass']);

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{directiveCtrl.tabIndex}}"';

                if (attrs['isDisabledBinding'])
                    tmplString += 'is-disabled="directiveCtrl.isDisabledBinding">';
                else
                    tmplString += 'is-disabled="directiveCtrl.status.disabled">';

                tmplString += '<uib-accordion-heading>' +
                    '<div class="soe-accordion-progressbar" data-ng-if="directiveCtrl.isBusy">' +
                    //'<soe-progressbar is-busy="directiveCtrl.isBusy" message="directiveCtrl.busyMessage"></soe-progressbar>' +
                    '<progressbar class="progress-striped active" type="warning">' +
                    '<span data-l10n-bind="directiveCtrl.busyMessage"></span>' +
                    '</progressbar>' +
                    '</div>' +
                    '<div class="soe-accordion-heading" data-ng-if="!directiveCtrl.isBusy">';

                tmplString += '<label class="control-label" data-ng-class="directiveCtrl.labelClass"';
                if (attrs['labelModel'])
                    tmplString += '>{{directiveCtrl.labelModel}}';
                else
                    tmplString += ' data-l10n-bind="directiveCtrl.labelKey">';
                tmplString += '</label>';

                tmplString += '<span class="panel-button toggle pull-right"><i class="fal" data-ng-class="{\'fa-chevron-up\': directiveCtrl.isOpen, \'fa-chevron-down\': !directiveCtrl.isOpen}"></i></span>';

                if (attrs['showPrintIcon']) {
                    tmplString += '<i class="pull-right fal fa-print" data-l10n-bind data-l10n-bind-title="directiveCtrl.printTooltipKey" data-ng-click="directiveCtrl.print()" data-ng-hide="!directiveCtrl.showPrintIcon" style="margin: 3px;"></i>';
                }

                if (attrs['buttonIcon']) {
                    tmplString += '<span class="panel-button pull-right" data-l10n-bind data-l10n-bind-title="directiveCtrl.buttonTooltipKey" data-ng-hide="directiveCtrl.buttonHidden" data-ng-click="$event.stopPropagation();$event.preventDefault();directiveCtrl.onButtonClick();">';
                    tmplString += '<i class="fal ' + attrs['buttonIcon'] + '"></i>';
                    if (attrs['buttonLabelKey'])
                        tmplString += '<label class="control-label link" data-l10n-bind="directiveCtrl.buttonLabelKey"></label>';
                    tmplString += '</span>';
                }

                if (attrs['statusIcon']) {
                    tmplString += '<span class="panel-button pull-right" data-l10n-bind data-l10n-bind-title="directiveCtrl.statusTooltipKey" data-ng-hide="directiveCtrl.hideSignatureIcon"><i class="fal ' + attrs['statusIcon'] + '" style="margin: 3px;"></i></span>';
                }

                if (attrs['labelValue']) {
                    tmplString += '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass" data-ng-show="directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">(</label>' +
                        '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass" data-ng-show="directiveCtrl.labelValue">{{directiveCtrl.labelValue}}</label>' +
                    '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass" data-ng-show="directiveCtrl.labelValue && directiveCtrl.labelValueInParentheses">)</label>';
                }
                if (attrs['warningValue']) {
                    tmplString += '<span class="panel-button toggle" data-ng-show="directiveCtrl.warningValue > 0"><i class="fal fa-exclamation-circle warningColor"></i>' +
                        '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass">{{directiveCtrl.warningValue}}</label>' +
                        '</span>';
                }
                if (attrs['warningValue2']) {
                    tmplString += '<span class="panel-button toggle" data-ng-show="directiveCtrl.warningValue2 > 0"><i class="fal fa-exclamation-square warningColor"></i>' +
                        '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass">{{directiveCtrl.warningValue2}}</label>' +
                        '</span>';
                }
                if (attrs['errorValue']) {
                    tmplString += '<span class="panel-button toggle" data-ng-show="directiveCtrl.errorValue > 0"><i class="fal fa-exclamation-triangle errorColor"></i>' +
                        '<label class="control-label value-label" data-ng-class="directiveCtrl.labelValueClass">{{directiveCtrl.errorValue}}</label>' +
                        '</span>';
                }

                tmplString += '</div>' +
                    '</uib-accordion-heading>' +
                    '<div data-ng-transclude />' +
                    '</div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                accordionClass: '=?',
                labelKey: '@',
                labelModel: '=?',
                labelValue: '=?',
                labelValueInParentheses: '@',
                labelValueClass: '=?',
                titleClass: '@',
                labelClass: '=?',
                isOpen: '=?',
                isDisabled: '@',
                isDisabledBinding: '=?',
                isBusy: '=?',
                busyMessage: '@',
                onOpen: '&',
                onClose: '&',
                onButtonClick: '&',
                buttonIcon: '@',
                buttonLabelKey: '@',
                buttonTooltipKey: '@',
                buttonHidden: '=?',
                print: '&',
                showPrintIcon: '@',
                printTooltipKey: '@',
                printHidden: '=?',
                condensed: '@',
                moreCondensed: '@',
                tabIndex: '=',
                warningValue: '=?',
                warningValue2: '=?',
                errorValue: '=?',
                statusIcon: '@',
                statusTooltipKey: '@',
                hideSignatureIcon: '=?',
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                if (attrs['titleClass']) {
                    $timeout(() => {
                        var titles = element[0].getElementsByClassName("panel-title");
                        if (titles.length > 0)
                            titles[0].className += (" " + attrs['titleClass']);
                    });
                }
            },
            restrict: 'E',
            transclude: true,
            controller: AccordionController,
            controllerAs: 'directiveCtrl',
            bindToController: true,
        };
    }
}

class AccordionController {
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