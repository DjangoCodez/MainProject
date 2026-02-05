import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService) {

        var part: string = "time.employee.statistics.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
            })
            .initialize(part + "statistics", part + "statistics", "");
    }

    public tabs: ITabHandler;
}