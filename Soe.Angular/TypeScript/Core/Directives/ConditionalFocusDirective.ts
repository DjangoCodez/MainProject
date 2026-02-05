export class ConditionalFocusDirective implements ng.IDirective {
    //@ngInject
    static create($timeout: ng.ITimeoutService) {
        return {
            restrict: 'A',
            scope: {
                focus: "=conditionalFocus",
                focusValue: "@conditionalFocusValue"
            },
            link: ($scope, $element, attrs) => {
                $scope.$watch("focus", (currentValue, previousValue) => {
                    if (currentValue === null)
                        return;

                    var delay: number = 0;
                    if (attrs['delay'])
                        delay = Number(attrs['delay']);

                    if (currentValue === $scope['focusValue']) {
                        $timeout(() => {
                            $element[0].focus();
                        }, delay);
                    }
                    //else {
                    //    $element[0].blur();
                    //}

                    //$scope['focus'] = null;
                });
            }
        }
    }
}