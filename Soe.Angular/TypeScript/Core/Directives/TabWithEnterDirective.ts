
export class TabWithEnterDirectiveFactory {
    //@ngInject
    public static create($timeout: ng.ITimeoutService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                element.bind('keydown', function (e) {
                    var code = e.keyCode || e.which;
                    if (code === 13) {
                        e.preventDefault();

                        // Get next input element
                        var next: HTMLElement = getNextElement(document.activeElement);

                        // Skip some elements
                        var i = 0;
                        while (i < 20 && (next.tabIndex < 0 || hasClass(next, 'skip-tab-with-enter'))) {
                            next = getNextElement(next);
                            i++;    // Just to prevent infinite loop
                        }

                        $timeout(() => {
                            next.focus();
                        });
                    }
                });

                var getNextElement = function (elem) {
                    if ($(elem).is('textarea')) {
                        if (elem.value) {
                            // If typing text, enter will add new line
                            elem.value += '\r\n';
                            return elem;
                        } else {
                            // If no text, use enter as tab (increase by 2 to skip shadow-textarea)
                            return $(":input")[$(":input").index(elem) + 2];
                        }
                    } else {
                        return $(":input")[$(":input").index(elem) + 1];
                    }
                };

                var hasClass = function (elem, className) {
                    return _.includes(elem.classList, className)
                }
            }
        }
    }
}