export class SetFocusDirectiveFactory {
    //@ngInject
    public static create($timeout: ng.ITimeoutService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                scope.$watch(attrs['setFocus'], function (newValue, oldValue) {
                    if (newValue) {
                        var delay: number = 100;
                        if (attrs['delay'])
                            delay = Number(attrs['delay']);
                        $timeout(() => {
                            if ($(element[0]).is('input') || $(element[0]).is('textarea')) {
                                element[0].focus();
                            } else {
                                let inputElem: JQuery<unknown> = $(element[0]).find('input');
                                if (inputElem)
                                    inputElem.focus();
                                else {
                                    inputElem = $(element[0]).find('textarea');
                                    if (inputElem)
                                        inputElem.focus();
                                }
                            }
                        }, delay);
                    }
                });
            }
        }
    }
}