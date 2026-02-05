import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        const part = "economy.accounting.vatverification";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.accountId)
            .onGetRowEditName(row => row.accountNr + " " + row.account)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"));   
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part, part, part + ".new_vatverification");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, GridController);
    }

    private add() {
        this.tabs.addCreateNewTab(GridController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}