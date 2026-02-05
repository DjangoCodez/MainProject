import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        // Setup base class
        var part: string = "time.employee.csr.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.employeeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .initialize(part + "export", part + "exports", "");
    }      
    public tabs: ITabHandler;
}
