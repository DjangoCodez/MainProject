import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { Guid } from "../../../Util/StringUtility";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    reconciliationPerAccountTabGuid: Guid;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "economy.accounting.reconciliation.reconciliation";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.accountId)
            .onGetRowEditName(row => row.accountNr + " " + row.account)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part, part, part + ".new_reconciliation");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, GridController);
    }

    private add() {
        this.tabs.addCreateNewTab(GridController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}