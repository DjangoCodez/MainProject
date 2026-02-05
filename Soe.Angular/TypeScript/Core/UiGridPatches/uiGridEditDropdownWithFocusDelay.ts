

//NOTE: For some reason there is no timeout before $elm[0].focus() in the uiGridEditDropdown directive, which causes problems. This solves them. Will submitt a pull-req later to hopefully get it into the normal code.
//this is a complete copy paste of uiGridEditDropdown, but with $elm[0].focus() wrapped in a timeout. (and ofc changes to make typescript happy)
//When this can be removed, please remember to find and replace all ui-grid-edit-dropdown-with-focus-delay with ui-grid-edit-dropdown.

export class uiGridEditDropdownWithFocusDelay {
    //@ngInject
    public static create(uiGridConstants: uiGrid.IUiGridConstants, uiGridEditConstants: uiGrid.edit.IUiGridEditConstants, $timeout: ng.ITimeoutService): ng.IDirective {
        return {
            require: ['?^uiGrid', '?^uiGridRenderContainer'],
            scope: true,
            compile: function () {
                return {
                    pre: function ($scope, $elm, $attrs) {

                    },
                    post: function ($scope, $elm, $attrs, controllers) {
                        var uiGridCtrl = controllers[0];
                        var renderContainerCtrl = controllers[1];

                        //set focus at start of edit
                        $scope.$on(uiGridEditConstants.events.BEGIN_CELL_EDIT, function () {
                            $timeout(function () {
                                $elm[0].focus();
                            });
                            $elm[0].style.width = ($elm[0].parentElement.offsetWidth - 1) + 'px';
                            $elm.on('blur', function (evt) {

                                (<any>$scope).stopEdit(evt);
                            });
                        });


                        (<any>$scope).stopEdit = function (evt) {
                            // no need to validate a dropdown - invalid values shouldn't be
                            // available in the list
                            $scope.$emit(uiGridEditConstants.events.END_CELL_EDIT);
                        };

                        $elm.on('keydown', function (evt) {
                            if (evt.keyCode == uiGridConstants.keymap.ESC) {
                                evt.stopPropagation();
                                $scope.$emit(uiGridEditConstants.events.CANCEL_CELL_EDIT);
                            }
                            if (uiGridCtrl && uiGridCtrl.grid.api.cellNav) {
                                (<any>evt).uiGridTargetRenderContainerId = renderContainerCtrl.containerId;
                                if (uiGridCtrl.cellNav.handleKeyDown(evt) !== null)
                                    (<any>$scope).stopEdit(evt);
                            } else if (evt.keyCode == uiGridConstants.keymap.ENTER || evt.keyCode == uiGridConstants.keymap.TAB) {
                                evt.stopPropagation();
                                evt.preventDefault();
                                (<any>$scope).stopEdit(evt);
                            }
                            return true;
                        });
                    }
                };
            }
        }
    }
}

