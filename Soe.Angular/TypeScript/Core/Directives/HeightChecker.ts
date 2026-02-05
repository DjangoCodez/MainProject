export class HeightCheckerFactory {
    //@ngInject
    public static create($interval: ng.IIntervalService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                var oldHeight = 0;
                var cancel = $interval(() => {
                    var newHeight = $(document).height();
                    if (newHeight !== oldHeight) {
                        oldHeight = newHeight;
                        return;
                    }

                    $interval.cancel(cancel);

                    var wh = $(window).height();
                    var dh = $(document).height();
                    var eh = $(element).height();

                    var diff = dh - wh;

                    var height = eh - diff;
                    height = Math.max(200, height);//dont make it too small?
                    $(element).height(height);
                }, 100, 8);
            }
        }
    }
}