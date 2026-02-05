import { DirectiveHelper } from "./DirectiveHelper";

export class FocusOnShowDirectiveFactory {
    //@ngInject
    public static create($timeout: ng.ITimeoutService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                if (attrs.ngIf) {
                    scope.$watch(attrs.ngIf, function (newValue) {
                        if (newValue) {
                            $timeout(function () {
                                let elem = DirectiveHelper.getInputElement(element);
                                if (elem)
                                    elem.focus();
                            }, 0);
                        }
                    })
                }
                if (attrs.ngShow) {
                    scope.$watch(attrs.ngShow, function (newValue) {
                        if (newValue) {
                            $timeout(function () {
                                let elem = DirectiveHelper.getInputElement(element);
                                if (elem)
                                    elem.focus();
                            }, 0);
                        }
                    })
                }
                if (attrs.ngHide) {
                    scope.$watch(attrs.ngHide, function (newValue) {
                        if (!newValue) {
                            $timeout(function () {
                                let elem = DirectiveHelper.getInputElement(element);
                                if (elem)
                                    elem.focus();
                            }, 0);
                        }
                    })
                }

                // Removes bound events in the element itself
                // when the scope is destroyed
                scope.$on('$destroy', function () {
                    element.off(attrs['focusOnShow']);
                });
            }
        }
    }
}