import { IMessagingHandler, MessagingHandler } from "./MessagingHandler";
import { IMessagingService } from "../Services/MessagingService";

export interface IMessagingHandlerFactory {
    create(): IMessagingHandler;
}

export class MessagingHandlerFactory implements IMessagingHandlerFactory {
    //@ngInject
    constructor(private messagingService: IMessagingService) {
    }

    create() {
        return new MessagingHandler(this.messagingService);
    }
}
