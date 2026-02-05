import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { Guid } from "../../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.logs.changelogs.search.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableRemoveAll();
            })
            .initialize(part + "log", part + "logs", part + "new");
    }

    private $onInit() {
    }

    public tabs: ITabHandler;
}