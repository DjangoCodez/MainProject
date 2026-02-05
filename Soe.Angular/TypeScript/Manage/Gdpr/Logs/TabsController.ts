import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(tabHandlerFactory: ITabHandlerFactory, protected urlHelperService: IUrlHelperService) {

        var part: string = "manage.gdpr.logs.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
            })
            .initialize(part + "logs", part + "logs", "");
    }

    public tabs: ITabHandler;
}