import { ITabHandler, TabHandler } from "./TabHandler";
import { ITranslationService } from "../Services/TranslationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { INotificationService } from "../Services/NotificationService";
import { IMessagingService } from "../Services/MessagingService";

export interface ITabHandlerFactory {
        create(): ITabHandler;
    }

    export class TabHandlerFactory implements ITabHandlerFactory {
        //@ngInject
        constructor(private translationService: ITranslationService,
            private $stateParams: angular.ui.IStateParamsService, private notificationService: INotificationService,
            private urlHelperService: IUrlHelperService, private messagingService: IMessagingService,
            private $timeout: ng.ITimeoutService, private $window: ng.IWindowService) {
        }

        create(): ITabHandler {
            return new TabHandler(this.translationService, this.$stateParams, this.notificationService, this.urlHelperService, this.messagingService, this.$timeout, this.$window);
        }
    }
