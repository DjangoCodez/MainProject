import { IMessagingService } from "../Services/MessagingService";
import { Constants } from "../../Util/Constants";

export class HeightCheckerFactory {
    //@ngInject
    public static create(messagingService: IMessagingService): ng.IDirective {
        return {
            restrict: 'A',
            link: function (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
                let sub = messagingService.subscribe(Constants.EVENT_RESIZE_WINDOW, (params) => {
                    let useWindowHeightMinus = 0;
                    if (attrs['useWindowHeightMinus'])
                        useWindowHeightMinus = attrs['useWindowHeightMinus'];
                    resize(element, params, useWindowHeightMinus)
                });

                scope.$on("$destroy", (x) => sub.unsubscribe());
            }
        }

        function resize(element: any, params: any, useWindowHeightMinus: number) {
            if (params && params["id"] !== $(element).attr("id")) {
                return;
            }

            let isVisible = $(element).is(':visible');
            if (!isVisible) {
                return;
            }

            let height = 0;
            const windowHeight = $(window).height() - 15;   // Add bottom margin

            if (useWindowHeightMinus > 0) {
                height = windowHeight - useWindowHeightMinus;
            } else {
                const documentHeight = $(document).height();
                const elementHeight = $(element).height();

                // On first load, calculate height of everything else but the element
                // Store it as an attribute on the element
                // Next time when resized (without reloaded) use value from attribute
                let otherHeight = 0;
                if ($(element).attr('other-height')) {
                    otherHeight = parseInt($(element).attr('other-height'));
                } else {
                    otherHeight = documentHeight - elementHeight;
                    $(element).attr('other-height', otherHeight);
                }

                if (otherHeight > 0)
                    height = windowHeight - otherHeight;
                else {
                    const diff = documentHeight - windowHeight;
                    height = elementHeight - diff;
                }
            }

            height = Math.max(200, height);//dont make it too small?

            $(element).height(height);
        }
    }
}