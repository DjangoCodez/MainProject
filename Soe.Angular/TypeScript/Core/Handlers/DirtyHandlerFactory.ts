import { IDirtyHandler, DirtyHandler } from "./DirtyHandler";
import { IMessagingService } from "../Services/MessagingService";
import { Guid } from "../../Util/StringUtility";

export interface IDirtyHandlerFactory {
    create(guid: Guid): IDirtyHandler;
}

export class DirtyHandlerFactory implements IDirtyHandlerFactory {
    //@ngInject
    constructor(private messagingService: IMessagingService) {
    }

    create(guid: Guid): IDirtyHandler {
        return new DirtyHandler(this.messagingService, guid);
    }
}
