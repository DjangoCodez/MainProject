import { IProgressHandler, ProgressHandler } from "./ProgressHandler";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { ITranslationService } from "../Services/TranslationService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";

export interface IProgressHandlerFactory {
    create(): IProgressHandler;
}

export class ProgressHandlerFactory implements IProgressHandlerFactory {
    //@ngInject
    constructor(private $uibModal, private translationService: ITranslationService, private $q: ng.IQService,
        protected messagingService: IMessagingService, private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService) {
    }

    create(): IProgressHandler {
        return new ProgressHandler(this.$uibModal, this.translationService, this.$q, this.messagingService, this.urlHelperService, this.notificationService);
    }
}
