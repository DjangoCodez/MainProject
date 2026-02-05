import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.preferences.checksettings.checksettings";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row)
            .onGetRowEditName(row => row)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));    
            })
            .onEdit(row => this.edit(row))
            .initialize(part, part, part);
    }

    private edit(row: any) {
        // Open edit page
        //this.tabs.addEditTab(row, EditController);
    }
    private add() {
        //this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}