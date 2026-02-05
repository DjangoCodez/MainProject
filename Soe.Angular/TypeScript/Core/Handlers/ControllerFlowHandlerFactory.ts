import { IGridControllerFlowHandler, GridControllerFlowHandler, IEditControllerFlowHandler, EditControllerFlowHandler } from "./ControllerFlowHandler";
import { ICoreService } from "../Services/CoreService";
import { IToolbarFactory } from "./ToolbarFactory";

export interface IControllerFlowHandlerFactory {
    createForEdit(): IEditControllerFlowHandler;
    createForGrid(): IGridControllerFlowHandler;
}

export class ControllerFlowHandlerFactory implements IControllerFlowHandlerFactory {
    //@ngInject
    constructor(private $q: ng.IQService, private coreService: ICoreService, private toolbarFactory: IToolbarFactory, private $timeout: ng.ITimeoutService) {
    }

    createForEdit(): IEditControllerFlowHandler {
        return new EditControllerFlowHandler(this.$q, this.coreService, this.toolbarFactory);
    }
    createForGrid(): IGridControllerFlowHandler {
        return new GridControllerFlowHandler(this.$q, this.coreService, this.toolbarFactory, this.$timeout);
    }
}
