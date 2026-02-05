import { ITranslationService } from "../Services/TranslationService";

export class L10NBindDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService): ng.IDirective {
        return {
            restrict: 'A',
            link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
                var resourceKey = attrs['l10nBind'];

                if (resourceKey) {
                    let key = scope.$eval(resourceKey);
                    translationService.translate(key).then(text => {
                        element.text(text);
                    });
                }

                _.forEach(element[0].attributes, x => {
                    if (!x.value)
                        return;

                    if (x.name.indexOf("data-l10n-bind") >= 0 || x.name.indexOf("l10n-bind") >= 0) {
                        var name = x.name;
                        name = name.substr(name.lastIndexOf('-') + 1);
                        if (name === "bind")
                            return;

                        let key = scope.$eval(x.value);
                        translationService.translate(key).then(text => {
                            if (attrs['lowercase'] && text)
                                text = text.toLocaleLowerCase();
                            element.attr(name, text);
                        });
                    }
                });
            }
        };
    }
}