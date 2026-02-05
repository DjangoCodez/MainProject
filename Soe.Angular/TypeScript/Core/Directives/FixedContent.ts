export class FixedContentFactory {
    //@ngInject
    public static create($interval: ng.IIntervalService): ng.IDirective {
        return {
            restrict: 'A',
            scope: {
                fixedActivate:'=?',
                fixedTopDefault: '@?',
                fixedScrollSpeed: '@?'
            },
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                angular.element($(window)).on('scroll', function () {
                    let activate = scope["fixedActivate"];
                    if (!!activate) {
                        let cancel = $interval(() => {
                            $interval.cancel(cancel);

                            let scrollTop = $(window).scrollTop() || $(document.documentElement).scrollTop();
                            let fixedTopDefault = !isNaN(scope["fixedTopDefault"]) ? parseInt(scope["fixedTopDefault"]) : 0;
                            let fixedScrollSpeed = !isNaN(scope["fixedScrollSpeed"]) ? parseInt(scope["fixedScrollSpeed"]) : 1;

                            let top = scrollTop == 0 ? fixedTopDefault : scrollTop;
                            let transitionSpeed = scrollTop == 0 ? '0s' : (fixedScrollSpeed + 's');

                            element.css('top', top + 'px');
                            element.css('transition', transitionSpeed);

                            scope.$applyAsync();

                        }, 100, 8);
                    }
                });

                // Clean up the event listener
                scope.$on('$destroy', function () {
                    angular.element($(window)).off('scroll');
                });
            }
        };
    }
}