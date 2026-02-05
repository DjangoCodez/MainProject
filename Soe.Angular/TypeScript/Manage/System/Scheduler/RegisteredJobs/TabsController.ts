import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.system.scheduler."; 
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.sysScheduledJobId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));      
            })
            .initialize(part + "registeredjob", part + "registeredjobs", part + "newregisteredjob");
    }

    public tabs: ITabHandler;
}