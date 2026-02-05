
// Original code from:
// https://gist.github.com/thomseddon/4703968
// Added functonality for shringOnBlur and maxHeight etc.

export class TextareaAutoSizeDirectiveFactory {
    //@ngInject
    public static create($timeout: ng.ITimeoutService): ng.IDirective {
        return {
            link: (scope: ng.IScope, element: any, attrs: ng.IAttributes) => {
                var minHeight;
                var maxHeight = 180;
                var $shadowtx = angular.element('<textarea class="skip-tab-with-enter ' + attrs['class'] + '" tabIndex="-1"></textarea>');
                //add to element parent
                angular.element(element.parent()[0]).append($shadowtx);

                var update = function (addtext?) {
                    $shadowtx.val(element.val() + (addtext ? addtext : ''));
                    var height = Math.min(Math.max($shadowtx[0].scrollHeight, minHeight), maxHeight);
                    element.css('height', height + 'px');
                    element.css('overflow', height < maxHeight ? 'hidden' : 'auto');
                };

                element.bind('keydown', function (event) {
                    var keyc = event.keyCode || event.which;
                    if (keyc == 13) {
                        update("\n");
                    }
                }).bind('keyup', function (event) {
                    var keyc = event.keyCode || event.which;
                    if ((keyc == 46) || (keyc == 8)) { //delete, backspace, not fired by scope.$watch
                        update();
                    }
                });

                if (attrs['shrinkOnBlur']) {
                    element.bind('focus', function (event) {
                        update();
                    }).bind('blur', function (event) {
                        // TODO: Hard coded default height (currently used in repeater-condensed)
                        $timeout(() => {
                            element.css('height', '22px');
                        }, 150);
                    })
                }
                
                $timeout(function () {
                    minHeight = element[0].offsetHeight;
                    $shadowtx.css({
                        position: 'absolute',
                        top: -10000,
                        left: -10000,
                        width: element.width(),
                        height: element.height()
                    });
                    update();
                    element.css('height', '22px');
                }, 0);
                
                scope.$watch(attrs['ngModel'], function (v) { update(); });
            },
        }
    }
}