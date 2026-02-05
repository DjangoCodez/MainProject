import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.system.test.";
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.testCaseGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));      
            })
            .initialize(part + "testgroup", part + "testgroups", part + "newtestgroup");
    }

    //private edit(row: any) {
    //    this.tabs.addEditTab(row, EditController, { id: row.testCaseGroupId });
    //}
    //private add() {
    //    this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    //}

    public tabs: ITabHandler;
}