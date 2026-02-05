import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "common.usercompanysettings.licensesettings";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, { isHomeTab: true }, this.urlHelperService.getViewUrl("edit.html"));
            })
            .initialize(part, part, part);
    }

    public tabs: ITabHandler;
}