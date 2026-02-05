import { IFocusService } from "../Services/focusservice";

export class EnterFocusDirectiveFactory {
    //@ngInject
    public static create(focusService: IFocusService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                element.bind("keydown keypress", function (event) {
                    if (event.which === 13) {
                        focusService.focusByName(attrs['enterFocus']);

                        event.preventDefault();
                    }
                });
            }
        }
    }
}
