// https://github.com/bennadel/JavaScript-Demos/blob/master/demos/window-blur-focus-angularjs/index.htm

export class WindowFocusDirectiveFactory implements ng.IDirective {
    //@ngInject
    static create($window) {
        return {
            restrict: 'A',
            link: ($scope, $element, attrs) => {
                // Hook up blur-handler.
                var win = angular.element($window).on("focus", handleFocus);

                // When the scope is destroyed, we have to make sure to teardown
                // the event binding so we don't get a leak.
                $scope.$on("$destroy", handleDestroy);

                // Handle the focus event on the Window.
                function handleFocus(event) {
                    $scope.$apply(attrs.windowFocus);
                }

                // Teardown the directive.
                function handleDestroy() {
                    win.off("focus", handleFocus);
                }
            }
        }
    }
}