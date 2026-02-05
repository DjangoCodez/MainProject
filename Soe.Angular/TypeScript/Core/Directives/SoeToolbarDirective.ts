import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

export class SoeToolbarDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {

                const elem = DirectiveHelper.createTemplateElement(
                    '<div data-ng-if="(directiveCtrl.navigatorRecords && directiveCtrl.navigatorRecords.length>0) || directiveCtrl.navigatorShowAlways"><record-navigator records="directiveCtrl.navigatorRecords" selected-record="directiveCtrl.navigatorSelectedRecord" show-always="directiveCtrl.navigatorShowAlways" show-position="directiveCtrl.navigatorShowPosition" show-record-name="directiveCtrl.navigatorShowRecordName" show-dropdown="directiveCtrl.navigatorShowDropdown" dropdown-text-property="{{directiveCtrl.navigatorDropdownTextProperty}}" is-date="directiveCtrl.navigatorRecordIsDate" on-selection-changed="directiveCtrl.navigatorRecordSelected(record);"></record-navigator></div>' +
                    '<div class="btn-group pull-right" data-ng-repeat="group in directiveCtrl.buttons">' +
                    '<a href="{{button.options.newTabLink}}" data-ng-repeat="button in group.buttons" class="{{::\'btn \' + button.options.buttonClass + \' \' + button.library + \' \' + button.iconClass}}"' +
                    'data-ng-class="{disabled: button.disabled()}" data-ng-if="!button.hidden()"' +
                    'data-ng-hide="button.hidden()"' +
                    'data-l10n-bind data-l10n-bind-title="button.titleKey" data-ng-click="$event.stopPropagation();!button.disabled() && button.click()">' +
                    '<span data-ng-if="button.labelKey" data-l10n-bind="button.labelKey"></span>' +
                    '<span data-ng-if="!button.labelKey && button.options.labelValue">{{button.options.labelValue}}</span>' +
                    '</a>' +
                    '</div>' +
                    '<label class="toolbar-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-if="directiveCtrl.labelKey"></label>' +
                    '<label class="toolbar-label" data-ng-if="directiveCtrl.label">{{directiveCtrl.label}}</label>' +
                    '<span class="toolbar-spinner far fa-spinner fa-pulse fa-fw" data-ng-if="directiveCtrl.isBusy"></span>' +
                    '<span class="toolbar-spinner fal fa-sync fa-spin fa-fw" data-ng-if="directiveCtrl.isRefreshing"></span>' +
                    '<div class="pull-left" ng-transclude></div>',
                    attrs);

                elem.className = "btn-toolbar";
                elem.setAttribute("role", "toolbar");

                // Only show toolbar if it contains any buttons
                if (!attrs['alwaysShow'])
                    elem.setAttribute("data-ng-hide", "!directiveCtrl.buttons || directiveCtrl.buttons.length === 0");

                // Hide border
                elem.setAttribute("data-ng-class", "{'btn-toolbar-no-border': directiveCtrl.hideBorder, 'btn-toolbar-no-margin': directiveCtrl.noMargin, 'btn-toolbar-modal-margin': directiveCtrl.modalMargin}");

                DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                buttons: '=?',
                labelKey: '@',
                label: '=?',
                hideBorder: '=?',
                noMargin: '=?',
                modalMargin: '=?',
                isBusy: '=?',
                isRefreshing: '=?',
                alwaysShow: '=?',
                navigatorRecords: '=?',
                navigatorSelectedRecord: '=?',
                navigatorShowAlways: '=?',
                navigatorShowPosition: '=?',
                navigatorShowRecordName: '=?',
                navigatorShowDropdown: '=?',
                navigatorDropdownTextProperty: '@',
                navigatorRecordIsDate: '@',
                navigatorOnSelectionChanged: '&'
            },
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            transclude: true,
            controller: ToolbarController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class ToolbarController {

    private navigatorDropdownTextProperty: string;
    private navigatorOnSelectionChanged: (record) => void;

    //@ngInject
    constructor(private $element) {
    }

    public $onInit() {
        if (!this.navigatorDropdownTextProperty)
            this.navigatorDropdownTextProperty = 'name';
    }

    private navigatorRecordSelected(record) {
        if (this.navigatorOnSelectionChanged && record)
            this.navigatorOnSelectionChanged({ record: record })
    }
}