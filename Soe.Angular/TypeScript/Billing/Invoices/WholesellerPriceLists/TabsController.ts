import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {       

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.sysPriceListHeadId)
            .onGetRowEditName(row => row.sysWholesellerName)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));                
                //tabHandler.addHomeTab(GridController);                
                //tabHandler.enableRemoveAll();
            })            
            .initialize("billing.invoices.wholesellerpricelists", "billing.invoices.wholesellerpricelists", "billing.invoices.wholesellerpricelists");
    }
    

    public tabs: ITabHandler;
}