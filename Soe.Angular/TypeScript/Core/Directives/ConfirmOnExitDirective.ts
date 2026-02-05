import { ITranslationService } from "../Services/TranslationService";

export class ConfirmOnExitDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            link: function ($scope, elem, attrs, form: ng.IFormController) {
                var message;
                translationService.translate("core.confirmonexit").then((term) => {
                    message = term;
                });

                window.onbeforeunload = function () {
                    if (form.$dirty) {
                        return message;
                    }
                }
                var o = $scope.$on('$locationChangeStart', function (event, next, current) {
                    var sameUrl: boolean = (trimUrl(next) === trimUrl(current));
                    if (form.$dirty && !sameUrl) {
                        if (!confirm(message)) {
                            event.preventDefault();
                        }
                    }
                });

                var trimUrl = (url: string) => {
                    if (url.endsWithCaseInsensitive('/'))
                        url = url.left(url.length - 1);
                    if (url.endsWithCaseInsensitive('#!'))
                        url = url.left(url.length - 2);
                    return url;
                }

                var o2 = $scope.$on("$destroy", () => {
                    window.onbeforeunload = null;
                    o();
                    o2();
                });
            },
            restrict: 'A',
            require: 'form'
        };
    }
}