
//NOTE: this is mostly a copy paste of uiGridEditor from ui-grid. My changes are noted below with //NOTE
export class uiGridTypeaheadEditor {
    //@ngInject
    public static create(gridUtil, uiGridConstants, uiGridEditConstants, $timeout, uiGridEditService) {
        return {
            scope: true,
            require: ['?^uiGrid', '?^uiGridRenderContainer', 'ngModel'],
            compile: function () {
                return {
                    pre: function ($scope, $elm, $attrs) {

                    },
                    post: function ($scope, $elm, $attrs, controllers) {
                        var uiGridCtrl, renderContainerCtrl, ngModel;
                        if (controllers[0]) {
                            uiGridCtrl = controllers[0];
                        }
                        if (controllers[1]) {
                            renderContainerCtrl = controllers[1];
                        }
                        if (controllers[2]) {
                            ngModel = controllers[2];
                        }

                        //set focus at start of edit
                        $scope.$on(uiGridEditConstants.events.BEGIN_CELL_EDIT,
                            function (evt, triggerEvent) {
                                $timeout(function () {
                                    $elm[0].focus();
                                    //only select text if it is not being replaced below in the cellNav viewPortKeyPress
                                    if ($elm[0].select && $scope.col.colDef.enableCellEditOnFocus ||
                                        !(uiGridCtrl && uiGridCtrl.grid.api.cellNav)) {
                                        $elm[0].select();
                                    } else {
                                        //some browsers (Chrome) stupidly, imo, support the w3 standard that number, email, ...
                                        //fields should not allow setSelectionRange.  We ignore the error for those browsers
                                        //https://www.w3.org/Bugs/Public/show_bug.cgi?id=24796
                                        try {
                                            $elm[0].setSelectionRange($elm[0].value.length, $elm[0].value.length);
                                        } catch (ex) {
                                            //ignore
                                        }
                                    }
                                });

                                //set the keystroke that started the edit event
                                //we must do this because the BeginEdit is done in a different event loop than the intitial
                                //keydown event
                                //fire this event for the keypress that is received
                                if (uiGridCtrl && uiGridCtrl.grid.api.cellNav) {
                                    var viewPortKeyDownUnregister = uiGridCtrl.grid.api.cellNav.on
                                        .viewPortKeyPress($scope,
                                            function (evt, rowCol) {
                                                if (uiGridEditService.isStartEditKey(evt)) {
                                                    ngModel
                                                        .$setViewValue(String
                                                            .fromCharCode(typeof evt.which === 'number'
                                                                ? evt.which
                                                                : evt.keyCode),
                                                            evt);
                                                    ngModel.$render();
                                                }
                                                viewPortKeyDownUnregister();
                                            });
                                }

                                //NOTE: Here are my changes, we need to know if the typeahead was clicked and then prevent the blur handler from stopping edit on itself. The edit is later stopped when the typeahead selected-item callback triggers. 
                                var typeAheadWasClicked = false;
                                var somethingThatWantsToNotAffectTheTypeaheadWasClicked = false;
                                function handleMouseDownSinceIEDoesNotFollowTheEventSpec(e) {
                                    typeAheadWasClicked = e.target && e.target.parentElement && e.target.parentElement.id && e.target.parentElement.id.startsWithCaseInsensitive('typeahead') || false;//totally foolproof
                                    somethingThatWantsToNotAffectTheTypeaheadWasClicked = $(e.target).hasClass('leave-typeahead-alone');
                                }

                                $(document).on('mousedown', handleMouseDownSinceIEDoesNotFollowTheEventSpec);

                                $elm.on('blur',
                                    function (evt) {//evt.relatedTarget would have been nice.. but IE 11 has bugs with that. and so does firefox apparently. So hence, we created the mousedown handler.
                                        if (typeAheadWasClicked) {
                                            typeAheadWasClicked = false;
                                            return;
                                        }

                                        if (somethingThatWantsToNotAffectTheTypeaheadWasClicked) {
                                            somethingThatWantsToNotAffectTheTypeaheadWasClicked = false;
                                            return;
                                        }

                                        $timeout(function () {
                                            $scope.stopEdit(evt);
                                        });
                                    });

                                $scope.$on("$destroy", function () {
                                    $(document).off('mousedown', handleMouseDownSinceIEDoesNotFollowTheEventSpec);
                                });

                                //NOTE: end my stuff
                            });


                        $scope.deepEdit = false;

                        $scope.stopEdit = function (evt) {
                            if ($scope.inputForm && !$scope.inputForm.$valid) {
                                evt.stopPropagation();
                                $scope.$emit(uiGridEditConstants.events.CANCEL_CELL_EDIT);
                            } else {
                                $scope.$emit(uiGridEditConstants.events.END_CELL_EDIT);
                            }
                            $scope.deepEdit = false;
                        };


                        $elm.on('click',
                            function (evt) {
                                if ($elm[0].type !== 'checkbox') {
                                    $scope.deepEdit = true;
                                    $timeout(function () {
                                        $scope.grid.disableScrolling = true;
                                    });
                                }
                            });

                        $elm.on('keydown',
                            function (evt) {
                                if (evt.keyCode == uiGridConstants.keymap.ESC) {
                                    evt.stopPropagation();
                                    $scope.$emit(uiGridEditConstants.events.CANCEL_CELL_EDIT);
                                }

                                if ($scope.deepEdit &&
                                    (evt.keyCode === uiGridConstants.keymap.LEFT ||
                                        evt.keyCode === uiGridConstants.keymap.RIGHT ||
                                        evt.keyCode === uiGridConstants.keymap.UP ||
                                        evt.keyCode === uiGridConstants.keymap.DOWN)) {
                                    evt.stopPropagation();
                                } else if (uiGridCtrl && uiGridCtrl.grid.api.cellNav) {
                                    evt.uiGridTargetRenderContainerId = renderContainerCtrl.containerId;
                                    if (uiGridCtrl.cellNav.handleKeyDown(evt) !== null) {
                                        $scope.stopEdit(evt);
                                    }
                                } else if (evt.keyCode == uiGridConstants.keymap.ENTER || evt.keyCode == uiGridConstants.keymap.TAB) {
                                    evt.stopPropagation();
                                    evt.preventDefault();
                                    $scope.stopEdit(evt);
                                }
                                return true;
                            });
                    }
                };
            }
        };
    }
}
