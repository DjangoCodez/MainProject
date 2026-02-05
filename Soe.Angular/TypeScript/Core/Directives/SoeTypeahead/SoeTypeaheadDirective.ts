import { ITranslationService } from "../../Services/TranslationService";
import { DirectiveHelper } from "./../DirectiveHelper";

export class SoeTypeaheadDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                //if (!attrs['placeholderKey'])
                //    attrs['placeholderKey'] = 'core.typetofilter';
                if (!attrs['limit'])
                    attrs['limit'] = 20;
                if (!attrs['filterProperty'])
                    attrs['filterProperty'] = 'name';
                if (!attrs['customPipe'])
                    attrs['customPipe'] = '';
                else 
                    attrs['customPipe'] = ' | ' + attrs['customPipe'];

                var tmplString: string = '<label class="control-label" data-l10n-bind="labelKey" data-ng-hide="hidelabel"></label>';

                if (attrs['labelValue']) {
                    if (attrs['notDiscreet']) {
                        tmplString += '<label class="control-label" data-ng-show="labelValue && labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                            '<label class="control-label" data-ng-show="labelValue">{{labelValue}}</label>' +
                            '<label class="control-label" data-ng-show="labelValue && labelValueInParentheses">)</label>';
                    }
                    else {
                        tmplString += '<label class="control-label discreet" data-ng-show="labelValue && labelValueInParentheses" style="margin-left: 3px;">(</label>' +
                            '<label class="control-label discreet" data-ng-show="labelValue">{{labelValue}}</label>' +
                            '<label class="control-label discreet" data-ng-show="labelValue && labelValueInParentheses">)</label>';
                    }
                }

                if (attrs['required'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel">*</label>';
                else if (attrs['isRequired'])
                    tmplString += '<label class="required-label" data-ng-hide="hidelabel || !isRequired">*</label>';

                // If onEdit and/or onInfo functions are specified, add edit buttons for them
                if (attrs['onEdit'] || attrs['onInfo']) {
                    tmplString += '<div class="input-group">';
                }

                tmplString += '<input class="form-control input-sm ngSoeTypeahead" type="text" autocomplete="off" data-ng-model="model" typeahead-input-formatter="formatInput($model)" data-l10n-bind data-l10n-bind-placeholder="placeholderKey" data-ng-focus="focus($event)" data-ng-blur="blur($event)" typeahead-on-select="onSelect($item, $model, $label, $event)" uib-typeahead="' 
                + attrs['options'] 
                    + ' | filter:{' + attrs['filterProperty'] + ': $viewValue}'
                    + attrs['customPipe']
                    + ' | limitTo:' + attrs['limit'] 
                    + '" typeahead-append-to-body="true" typeahead-min-length="0" typeahead-popup-template-url="soeTypeahead/soeTypeaheadPopup.html"';

                if (attrs['inputWidth'])
                    tmplString += ' style="width:' + attrs['inputWidth'] + ';"';


                if (attrs['editable'] && attrs['editable'] == 'true')
                    tmplString += 'typeahead-editable="true"';
                else {
                    tmplString += 'typeahead-editable="false"';
                }

                if (attrs['isReadonly']) {
                    tmplString += ' data-ng-readonly="isReadonly" data-ng-disabled="isReadonly"';
                }
                else {
                    tmplString += ' data-ng-disabled="disabled"';
                }

                if (attrs['tabIndex'])
                    tmplString += ' tabindex="{{tabIndex}}"';

                if (attrs['setFocus'])
                    tmplString += ' set-focus="{{setFocus}}"';

                if (attrs['eventFocusName'])
                    tmplString += ' event-focus event-focus-name="' + attrs['eventFocusName'] + '"';

                tmplString += ' />';

                if (attrs['onEdit'] || attrs['onInfo'] || attrs['onSearch']) {
                    tmplString += '<span class="input-group-btn">';

                    if (attrs['onEdit']) {
                        var classes = "btn btn-sm btn-default iconEdit fal";
                        if (attrs["editClass"]) {
                            classes += " " + attrs["editClass"] + " ";
                        }
                        if (attrs['editIcon'])
                            classes += " " + attrs['editIcon'];
                        else
                            classes += " fa-pencil";
                        tmplString += '<button type="button" class="' + classes + '" data-l10n-bind data-l10n-bind-title="editTooltipKey" data-ng-click="onEdit()" data-ng-disabled="' + (attrs['useOnlyEditDisabled'] ? 'editDisabled"></button>' : 'editDisabled || disabled || isReadonly"></button>');

                    }
                    if (attrs['onSearch']) {
                        var classes = "btn btn-sm btn-default iconEdit fal";
                        if (attrs['searchIcon'])
                            classes += " " + attrs['searchIcon'];
                        else
                            classes += " fa-search";
                        tmplString += '<button type="button" class="' + classes + '" data-l10n-bind data-l10n-bind-title="searchTooltipKey" data-ng-click="$event.stopPropagation();onSearch()" data-ng-disabled="searchDisabled"></button>';
                    }
                    if (attrs['onInfo'])
                        tmplString += '<button type="button" class="btn btn-sm btn-default fal fa-info-circle" data-l10n-bind data-l10n-bind-title="infoTooltipKey" data-ng-click="$event.stopPropagation();onInfo()" data-ng-disabled="infoDisabled"></button>';

                    tmplString += '</span></div>';
                }

                var elem = DirectiveHelper.createTemplateElement(tmplString, attrs);

                DirectiveHelper.applyAttributes([elem], attrs, null);

                return elem.outerHTML;
            },
            scope: {
                form: '=?',
                model: '=?',
                items: '=?',
                options: '@',
                labelKey: '@',
                hidelabel: '=?',
                labelValue: '=?',
                labelValueInParentheses: '@',
                placeholderKey: '@',
                limit: '=?',
                filterProperty: '=?',
                editable: '@',
                inputWidth: '@',
                disabled: '=?isDisabled',
                isReadonly: '=?',
                tabIndex: '=?',
                autoFocus: '@',
                setFocus: '=?',
                onFocus: '&',
                onSelectionChanged: '&',
                onFocusLost: '&',
                onEdit: '&',
                editIcon: '=?',
                editTooltipKey: '@',
                editDisabled: '=?',
                useOnlyEditDisabled: '=?',
                onSearch: '&',
                serchIcon: '=?',
                searchTooltipKey: '@',
                searchDisabled: '=?',
                onInfo: '&',
                infoTooltipKey: '@',
                infoDisabled: '=?',
                isRequired: '=?',
                notDiscreet: '=?',
                customPipe: '@'
            },
            link: (scope: any, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
                DirectiveHelper.removeAttributes(element, attrs);
            },
            restrict: 'E',
            replace: true,
            //@ngInject
            controller: function ($timeout: ng.ITimeoutService, $scope) {
                $scope.focus = function (event) {
                    if (event && event.currentTarget)
                        $(event.currentTarget).select();
                    if ($scope.onFocus())
                        $scope.onFocus();
                };

                $scope.blur = function (event) {
                    if ($scope.onFocusLost) {
                        if (typeof $scope.model === 'string') {
                            const value = $scope.model;
                            $scope.onFocusLost({value: value});
                        }
                    }
                };

                $scope.formatInput = function (model) {
                    // If model is string (typeahead is editable), just use model as label
                    var label = model;

                    if (model) {
                        if (typeof model !== 'string') {
                            if (model.displayValue) {
                                label = model.displayValue;
                            }
                            else {
                                // Get which name that is bound to the label value
                                // Format: 'item.id as item.name for item in items', will return 'name'
                                var optionParts: string[] = $scope.options.split(' ');
                                if (optionParts.length > 2) {
                                    var labelParts: string[] = optionParts[2].split('.');
                                    label = model[labelParts[1]];
                                }
                            }
                        }

                        // If empty value is selected in dropdown (also happens on tabbing passed field), clear value
                        if (label && label.trim().length === 0)
                            label = undefined;
                    }

                    return label;
                };

                $scope.onSelect = function (item, model, label, event) {
                    if ($scope.onSelectionChanged)
                        $scope.onSelectionChanged({ item: item });
                };
            }
        };
    }
}