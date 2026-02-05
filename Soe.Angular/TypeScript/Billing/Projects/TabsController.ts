import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "../../Shared/Billing/Projects/EditController";
import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../Core/Handlers/tabhandlerfactory";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        const part = "billing.projects.list.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.projectId)
            .onGetRowEditName(row => row.number ? row.number : "")
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "project", part + "project", part + "new_project");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController);
    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}