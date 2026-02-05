import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "billing.stock.purchase.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.stockId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getViewUrl("grid.html"));
            })
            .initialize(part + "suggestion", part + "suggestion", part + "suggestion");
    }

    public tabs: ITabHandler;
}