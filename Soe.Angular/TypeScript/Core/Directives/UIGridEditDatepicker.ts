import { ITranslationService } from "../Services/TranslationService";

// https://github.com/Joiler/ui-grid-edit-datepicker

export class UIGridEditDatepickerFactory {
    //@ngInject
    public static create(translationService: ITranslationService, uibDatepickerConfig: angular.ui.bootstrap.IDatepickerConfig, uibDatepickerPopupConfig: angular.ui.bootstrap.IDatepickerPopupConfig, $locale: any, $timeout: ng.ITimeoutService, $document, uiGridConstants, uiGridEditConstants): ng.IDirective {
        return {
            template: (element: any, attrs: ng.IAttributes) => {
                return '<div class="datepicker-wrapper"><input type="text" uib-datepicker-popup parse-date datepicker-options="datepickerOptions" datepicker-append-to-body="true" is-open="isOpen" ng-model="datePickerValue" ng-change="changeDate($event)" ng-keydown="editDate($event)" popup-placement="auto top"/></div>';
            },
            require: ['?^uiGrid', '?^uiGridRenderContainer'],
            scope: true,
            compile: function () {
                return {
                    pre: function (scope: any, element: any, attrs: any) {
                        if (attrs.datepickerOptions) {
                            if (scope.col.grid.appScope[attrs.datepickerOptions]) {
                                scope.datepickerOptions = scope.col.grid.appScope[attrs.datepickerOptions];
                            }
                        }

                        scope['datepickerConfig'] = uibDatepickerConfig;
                        scope['datepickerPopupConfig'] = uibDatepickerPopupConfig;
                        scope["format"] = $locale.DATETIME_FORMATS.shortDate;

                        var keys: string[] = [
                            "core.datepicker.current",
                            "core.datepicker.clear",
                            "core.datepicker.close"
                        ];

                        translationService.translateMany(keys).then((terms) => {
                            (<any>uibDatepickerPopupConfig).currentText = terms["core.datepicker.current"];
                            (<any>uibDatepickerPopupConfig).clearText = terms["core.datepicker.clear"];
                            (<any>uibDatepickerPopupConfig).closeText = terms["core.datepicker.close"];
                        });
                    },
                    post: function (scope: any, element: any, attrs: any, controllers) {
                        var setCorrectPosition = function () {
                            var gridElement = $('.ui-grid-viewport');
                            var gridPosition = {
                                width: gridElement.outerWidth(),
                                height: gridElement.outerHeight(),
                                offset: gridElement.offset()
                            };

                            var cellElement = $(element);
                            var cellPosition = {
                                width: cellElement.outerWidth(),
                                height: cellElement.outerHeight(),
                                offset: cellElement.offset()
                            };

                            var datepickerElement = $('body > .dropdown-menu, body > div > .dropdown-menu');
                            var datepickerPosition = {
                                width: datepickerElement.outerWidth(),
                                height: datepickerElement.outerHeight()
                            };

                            var setCorrectTopPositionInGrid = function () {
                                var topPosition;
                                var freePixelsOnBottom = gridPosition.height - (cellPosition.offset.top - gridPosition.offset.top) - cellPosition.height;
                                var freePixelsOnTop = gridPosition.height - freePixelsOnBottom - cellPosition.height;
                                var requiredPixels = (datepickerPosition.height - cellPosition.height) / 2;
                                if (freePixelsOnBottom >= requiredPixels && freePixelsOnTop >= requiredPixels) {
                                    topPosition = cellPosition.offset.top - requiredPixels + 10;
                                } else if (freePixelsOnBottom >= requiredPixels && freePixelsOnTop < requiredPixels) {
                                    topPosition = cellPosition.offset.top - freePixelsOnTop + 10;
                                } else {
                                    topPosition = gridPosition.height - datepickerPosition.height + gridPosition.offset.top - 20;
                                }
                                return topPosition;
                            };

                            var setCorrectTopPositionInWindow = function () {
                                var topPosition;
                                var windowHeight = window.innerHeight - 10;

                                var freePixelsOnBottom = windowHeight - cellPosition.offset.top;
                                var freePixelsOnTop = windowHeight - freePixelsOnBottom - cellPosition.height;
                                var requiredPixels = (datepickerPosition.height - cellPosition.height) / 2;
                                if (freePixelsOnBottom >= requiredPixels && freePixelsOnTop >= requiredPixels) {
                                    topPosition = cellPosition.offset.top - requiredPixels;
                                } else if (freePixelsOnBottom >= requiredPixels && freePixelsOnTop < requiredPixels) {
                                    topPosition = cellPosition.offset.top - freePixelsOnTop;
                                } else {
                                    topPosition = windowHeight - datepickerPosition.height - 10;
                                }
                                return topPosition;
                            };

                            var newOffsetValues = {};

                            var isFreeOnRight = (gridPosition.width - (cellPosition.offset.left - gridPosition.offset.left) - cellPosition.width) > datepickerPosition.width;
                            if (isFreeOnRight) {
                                newOffsetValues['left'] = cellPosition.offset.left + cellPosition.width;
                            } else {
                                newOffsetValues['left'] = cellPosition.offset.left - datepickerPosition.width;
                            }

                            if (datepickerPosition.height < gridPosition.height) {
                                newOffsetValues['top'] = setCorrectTopPositionInGrid();
                            } else {
                                newOffsetValues['top'] = setCorrectTopPositionInWindow();
                            }

                            datepickerElement.offset(<any>newOffsetValues);
                            datepickerElement.css("visibility", "visible");
                        };

                        $timeout(function () {
                            setCorrectPosition();
                        }, 0);

                        scope.datePickerValue = scope.row.entity[scope.col.field] ? new Date(scope.row.entity[scope.col.field]) : null;
                        scope.isOpen = true;
                        var uiGridCtrl = controllers[0];
                        var renderContainerCtrl = controllers[1];

                        var onWindowClick = function (evt) {
                            var classNamed = angular.element(evt.target).attr('class');
                            if (classNamed) {
                                var inDatepicker = (classNamed.indexOf('datepicker-calendar') > -1);
                                if (!inDatepicker && evt.target.nodeName !== "INPUT") {
                                    scope.stopEdit(evt);
                                }
                            }
                            else {
                                scope.stopEdit(evt);
                            }
                        };

                        var onCellClick = function (evt) {
                            angular.element(document.querySelectorAll('.ui-grid-cell-contents')).off('click', onCellClick);
                            scope.stopEdit(evt);
                        };

                        scope.editDate = function (evt) {
                        };

                        scope.changeDate = function (evt) {
                        };

                        scope.$on(uiGridEditConstants.events.BEGIN_CELL_EDIT, function () {
                            if (uiGridCtrl.grid.api.cellNav) {
                                uiGridCtrl.grid.api.cellNav.on.navigate(scope, function (newRowCol, oldRowCol) {
                                    scope.stopEdit();
                                });
                            } else {
                                angular.element(document.querySelectorAll('.ui-grid-cell-contents')).on('click', onCellClick);
                            }
                            angular.element(window).on('click', onWindowClick);
                        });

                        scope.$on('$destroy', function () {
                            angular.element(window).off('click', onWindowClick);
                            $('body > .dropdown-menu, body > div > .dropdown-menu').remove();
                        });

                        scope.stopEdit = function (evt) {
                            if (angular.isDate(scope.datePickerValue) || !scope.datePickerValue) {
                                scope.row.entity[scope.col.field] = scope.datePickerValue;
                            } else {
                                scope.datePickerValue = scope.row.entity[scope.col.field];
                            }
                            scope.$emit(uiGridEditConstants.events.END_CELL_EDIT);
                        };

                        element.on('keydown', function (evt) {
                            if (evt.keyCode == uiGridConstants.keymap.ESC) {
                                evt.stopPropagation();
                                scope.$emit(uiGridEditConstants.events.CANCEL_CELL_EDIT);
                            }

                            if (uiGridCtrl && uiGridCtrl.grid.api.cellNav) {
                                evt.uiGridTargetRenderContainerId = renderContainerCtrl.containerId;
                                if (uiGridCtrl.cellNav.handleKeyDown(evt) !== null) {
                                    scope.stopEdit(evt);
                                }
                            } else if (evt.keyCode == uiGridConstants.keymap.ENTER || evt.keyCode == uiGridConstants.keymap.TAB) {
                                evt.stopPropagation();
                                evt.preventDefault();
                                scope.stopEdit(evt);
                            }
                            return true;
                        });
                    }
                };
            }
        };
    }
}