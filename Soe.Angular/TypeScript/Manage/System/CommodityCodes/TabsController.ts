import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        var part: string = "manage.system.commoditycode.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.skillTypeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);               
            })           
            .initialize(part + "commoditycode", part + "commoditycode", part + "commoditycode");
    }


    tabs: ITabHandler;
}