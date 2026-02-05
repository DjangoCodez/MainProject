import { IFocusService } from "../Services/focusservice";

export class EventFocusDirectiveFactory {
    //@ngInject
    public static create(focusService: IFocusService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                if (!attrs['eventFocus']) {
                    if (attrs['eventFocusId'])
                        focusService.focusById(attrs['eventFocusId']);

                    if (attrs['eventFocusName'])
                        focusService.focusByName(attrs['eventFocusName']);
                    return;
                }

                element.on(attrs['eventFocus'], function () {
                    if (attrs['eventFocusId'])
                        focusService.focusById(attrs['eventFocusId']);

                    if (attrs['eventFocusName'])
                        focusService.focusByName(attrs['eventFocusName']);
                });

                // Removes bound events in the element itself
                // when the scope is destroyed
                scope.$on('$destroy', function () {
                    element.off(attrs['eventFocus']);
                });
            }
        }
    }
}