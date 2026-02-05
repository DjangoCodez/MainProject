import { RightMenuControllerBase, RightMenuType } from "../RightMenuControllerBase";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { IMessagingService } from "../../Services/MessagingService";
import { ITranslationService } from "../../Services/TranslationService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { Constants } from "../../../Util/Constants";

export class AcademyMenuDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('/Core/RightMenu/AcademyMenu/AcademyMenu.html'),
            scope: {
                positionIndex: "@"
            },
            restrict: 'E',
            replace: true,
            controller: AcademyMenuController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AcademyMenuController extends RightMenuControllerBase {

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        messagingService: IMessagingService,
        private $window,
        private translationService: ITranslationService) {
        super($timeout, messagingService, RightMenuType.Academy);
    }

    public $onInit() {
        this.setTopPosition();

        this.messagingService.subscribe(Constants.EVENT_SHOW_ACADEMY_MENU, (data: any) => {
            this.openSoftOneAcademy();
        });

    }

    private openSoftOneAcademy() {
        this.translationService.translate("core.help.softoneacademy.url").then(url => {
            HtmlUtility.openInNewTab(this.$window, url);
        });
    }
}
