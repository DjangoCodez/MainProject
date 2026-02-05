import { INotificationService } from "../../Core/Services/NotificationService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";

//TODO: Remove this, its only here to display the dashboard, but that should be done in billing (that does not exist yet?)

export class MainController {
    private widgets: any[];
    private showWidgets: boolean;
    private sortableOptions: any;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private $scope: any) {
    }
}