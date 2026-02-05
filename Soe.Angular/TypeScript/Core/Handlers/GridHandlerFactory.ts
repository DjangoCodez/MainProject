import { ICoreService } from "../Services/CoreService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { ITranslationService } from "../Services/TranslationService";
import { IGridHandler } from "./GridHandler";
import { GridHandlerAg } from "./GridHandlerAg";
import { GridHandlerUiGrid } from "./GridHandlerUiGrid";

export interface IGridHandlerFactory {
    create(name: string, type?: string): IGridHandler;
}

export class GridHandlerFactory implements IGridHandlerFactory {
    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants, private coreService: ICoreService,
        private translationService: ITranslationService, private notificationService: INotificationService, private messagingService: IMessagingService) {
    }

    create(name: string, type?: string): IGridHandler {
        if (type === "agGrid")
            return new GridHandlerAg(name, this.$timeout, this.uiGridConstants, this.coreService, this.translationService, this.notificationService, this.messagingService) as any;
        else
            return new GridHandlerUiGrid(name, this.$timeout, this.uiGridConstants, this.coreService, this.translationService, this.notificationService) as any;
    }
}
