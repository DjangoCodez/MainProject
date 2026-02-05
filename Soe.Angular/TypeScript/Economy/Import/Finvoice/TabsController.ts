import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {
    private type: number;
    //@ngInject
    constructor(private $window: ng.IWindowService,
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "economy.import.finvoice.";
        var suffix: string = "";        

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.ediEntryId)            
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })            
            .initialize(part + "importinvoice" + suffix, part + "importinvoices" + suffix, part + "new_importinvoice" + suffix);
    }    

    public tabs: ITabHandler;
}