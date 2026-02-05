import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";

export class WhenRenderedDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            scope: {
                whenRendered: '&'
            },
            restrict: 'A',
            controller: WhenRenderedController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class WhenRenderedController {
    private whenRendered: Function;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService) {
        $timeout(() => {
            this.whenRendered();
        }, 100);
    }
}