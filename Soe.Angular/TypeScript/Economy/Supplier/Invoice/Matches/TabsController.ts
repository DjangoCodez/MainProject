import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private $window, private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "economy.supplier.invoice.matches.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                //tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            //.onEdit(row => this.edit(row))
            .initialize(part + "match", part + "matches", part + "new");
    }

    public tabs: ITabHandler;
}