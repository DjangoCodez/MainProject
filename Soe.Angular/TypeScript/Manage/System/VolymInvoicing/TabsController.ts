import { GridController } from "./GridController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.system.volyminvoiceing.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.volymInvoiceId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .initialize(part + "volyminvoiceing", part + "volyminvoiceing", "");
    }

    public tabs: ITabHandler;
}