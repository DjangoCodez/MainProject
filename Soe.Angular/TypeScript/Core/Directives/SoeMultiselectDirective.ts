import { ITranslationService } from "../Services/TranslationService";
import { DirectiveHelper } from "./DirectiveHelper";

// http://dotansimha.github.io/angularjs-dropdown-multiselect/

export class SoeMultiselectDirectiveFactory {
    private static hasValidation(attrs: ng.IAttributes): boolean {
        return attrs['required'] && attrs['required'] === 'true';
    }

    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {

        return {
            template: (element: any, attrs: ng.IAttributes) => {
                var tmplString: string = '<label class="control-label" data-l10n-bind="directiveCtrl.labelKey" data-ng-hide="directiveCtrl.hidelabel">{{directiveCtrl.label}}</label>' +
                    (SoeMultiselectDirectiveFactory.hasValidation(attrs) ? '<label class="required-label" data-ng-hide="directiveCtrl.hidelabel">*</label><input type="text" class="form-control input-sm" ng-model="directiveCtrl.model" style="display:none" />' : "") +
                    '<div ng-dropdown-multiselect=""' +
                    'data-options="directiveCtrl.options"' +
                    'data-selected-model="directiveCtrl.model"' +
                    'data-extra-settings="directiveCtrl.multiSelectSettings"' +
                    'data-translation-texts="directiveCtrl.multiSelectTerms"' +
                    'data-checkboxes="true"' +
                    'data-disabled="directiveCtrl.disabled"' +
                    'data-events="directiveCtrl.events"';

                if (attrs['limitTo'])
                    tmplString += 'data-limit-to="' + attrs['limitTo'] + '"';

                if (attrs['inputClass'])
                    tmplString += 'data-ng-class="{\'' + attrs['inputClass'] + '\': directiveCtrl.inputClassCondition}"';

                tmplString += '></div>';

                tmplString += '<div class="margin-small-top" data-ng-if="directiveCtrl.showSelected"><span class="margin-small-left">{{directiveCtrl.model.length||0}} </span><span data-l10n-bind="\'common.of\'"></span><span> {{directiveCtrl.options.length}} </span><span data-l10n-bind="\'core.multiselect.selection_count\'"></span></div>';

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);
                elem.classList.add('ngSoeMultiselect');

                var id = DirectiveHelper.applyAttributes([elem], attrs, "directiveCtrl");
                attrs["input-element-id"] = id;
                return elem.outerHTML;
            },
            scope: {
                form: '=',
                labelKey: '@',
                hidelabel: '=',
                label: '=',
                dynamicTitle: '=',
                showCheckAll: '=?',
                showUncheckAll: '=?',
                smartButtonMaxItems: '@',
                options: '=',
                model: '=',
                idProp: '@',
                displayProp: '@',
                showSelected: '=?',
                showSelectedItems: '=?',
                search: '@',
                button: '@',
                disabled: '=isDisabled',
                inputClass: '@',
                inputClassCondition: '=?',
                keepOpen: '=?',
                onItemSelect: '&',
                onItemDeselect: '&',
                onSelectionComplete: '&',
                onSelectionOpened: '&',
                limitTo: '=',
            },
            link: (scope: ng.IScope, element: JQLite, attrs: ng.IAttributes, ctrl: MultiselectController) => {
                DirectiveHelper.removeAttributes(element, attrs);

                const id = attrs["input-element-id"];

                if (SoeMultiselectDirectiveFactory.hasValidation(attrs)) {
                    ctrl.underlyingNgModelController = <ng.INgModelController>element.find('#' + id).controller('ngModel');

                    ctrl.underlyingNgModelController.$formatters.push((value: Array<any>) => {
                        return value && value.length ? value.length : '';
                    });
                }

                delete attrs["input-element-id"];
            },
            restrict: 'E',
            replace: true,
            controller: MultiselectController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class MultiselectController {
    form: ng.IFormController;
    labelKey: any;
    hidelabel: any;
    label: any;
    dynamicTitle: any;
    showCheckAll: boolean;
    showUncheckAll: boolean;
    smartButtonMaxItems: any;
    keepOpen: boolean;

    multiSelectTerms: any;
    multiSelectSettings: any;

    model: any;
    idProp: string;
    displayProp: string;
    showSelectedItems: boolean;
    search: any;
    events: any;
    button: any;
    underlyingNgModelController: ng.INgModelController;

    hasChanged: boolean = false;    // Keep track of selections, preventing onSelectionComplete if nothing has been selected/deselected
    onItemSelect: (item: any) => any;
    onItemDeselect: (item: any) => any;
    onSelectionComplete: (items: any) => any;
    onSelectionOpened: () => any;

    private itemSelect(item: any) {
        this.hasChanged = true;
        if (this.onItemSelect)
            this.onItemSelect({ item });
    }

    private itemDeselect(item: any) {
        this.hasChanged = true;
        if (this.onItemDeselect)
            this.onItemDeselect({ item });
    }

    private onOpen() {
        this.hasChanged = false;
        (this.onSelectionOpened || angular.noop)();
    }

    private onClose() {
        this.$timeout(() => {
            this.applyValidationPropegationIfNeeded();

            if (this.hasChanged && this.onSelectionComplete) {
                this.onSelectionComplete({ items: this.model });
            }
        });
    }

    private applyValidationPropegationIfNeeded() {
        if (this.hasChanged && this.underlyingNgModelController) {
            this.underlyingNgModelController.$modelValue = this.model;
            this.underlyingNgModelController.$setTouched();
            this.underlyingNgModelController.$processModelValue();
        }
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService) {
    }

    public $onInit() {
        if (this.showCheckAll === undefined)
            this.showCheckAll = true;
        if (this.showUncheckAll === undefined)
            this.showUncheckAll = true;

        this.multiSelectSettings = {
            dynamicTitle: this.dynamicTitle,
            showCheckAll: this.showCheckAll,
            showUncheckAll: this.showUncheckAll,
            smartButtonMaxItems: this.smartButtonMaxItems || 0,
            showSelectedItems: this.showSelectedItems,
            enableSearch: this.search !== 'false',
            showButton: this.button !== 'false',
            closeOnBlur: !this.keepOpen,
            buttonClasses: 'btn btn-default'
        };

        if (this.idProp) {
            this.multiSelectSettings['idProp'] = this.idProp;
            this.multiSelectSettings['externalIdProp'] = this.idProp;
        }
        if (this.displayProp)
            this.multiSelectSettings['displayProp'] = this.displayProp;

        var keys: string[] = [
            "core.multiselect.check_all",
            "core.multiselect.uncheck_all",
            "core.multiselect.selection_count",
            "core.multiselect.dynamic_button_text_suffix",
            "core.multiselect.search_placeholder",
            "core.multiselect.button_default_text_prefix",
            "core.multiselect.close",
            "common.all",
            "common.of"
        ];

        if (this.labelKey && this.hidelabel)
            keys.push(this.labelKey);

        this.translationService.translateMany(keys).then((terms) => {
            this.multiSelectTerms = {
                checkAll: terms["core.multiselect.check_all"],
                uncheckAll: terms["core.multiselect.uncheck_all"],
                selectionCount: terms["core.multiselect.selection_count"],
                dynamicButtonTextSuffix: terms["core.multiselect.dynamic_button_text_suffix"],
                searchPlaceholder: terms["core.multiselect.search_placeholder"],
                buttonDefaultText: this.labelKey && this.hidelabel ? terms[this.labelKey] : this.label && this.hidelabel ? this.label : terms["core.multiselect.button_default_text_prefix"],
                close: terms["core.multiselect.close"],
                all: terms["common.all"],
                of: terms["common.of"]
            }
        });

        this.events = {
            onItemSelect: this.itemSelect.bind(this),
            onItemDeselect: this.itemDeselect.bind(this),
            onOpen: this.onOpen.bind(this),
            onClose: this.onClose.bind(this)
        };
    }
}