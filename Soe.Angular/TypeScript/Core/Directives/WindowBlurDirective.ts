// https://github.com/bennadel/JavaScript-Demos/blob/master/demos/window-blur-focus-angularjs/index.htm

export class WindowBlurDirectiveFactory implements ng.IDirective {
    //@ngInject
    static create($window) {
        return {
            restrict: 'A',
            link: ($scope, $element, attrs) => {
                // Hook up blur-handler.
                var win = angular.element($window).on("blur", handleBlur);

                // When the scope is destroyed, we have to make sure to teardown
                // the event binding so we don't get a leak.
                $scope.$on("$destroy", handleDestroy);

                // Handle the blur event on the Window.
                function handleBlur(event) {
                    $scope.$apply(attrs.windowBlur);
                }

                // Teardown the directive.
                function handleDestroy() {
                    win.off("blur", handleBlur);
                }
            }
        }
    }
}