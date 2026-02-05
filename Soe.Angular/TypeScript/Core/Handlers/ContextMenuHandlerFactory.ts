import { IContextMenuHandler, ContextMenuHandler } from "./ContextMenuHandler";

export interface IContextMenuHandlerFactory {
    create(): IContextMenuHandler;
}

export class ContextMenuHandlerFactory implements IContextMenuHandlerFactory {
    //@ngInject
    constructor() {
    }

    create(): IContextMenuHandler {
        return new ContextMenuHandler();
    }
}
