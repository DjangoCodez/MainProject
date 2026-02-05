import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        const part: string = "billing.stock.stocksaldo.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.stockProductId ?? 0)
            .onGetRowEditName(row => row.name ?? "")
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "stocksaldo", part + "stocksaldo", part + "stocksaldo");
    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController);
    }

    private add() {
        const existingTab = this.tabs.getTabByParameters(0,"stockproduct");
        if (existingTab)
        {
            this.tabs.setActiveTab(existingTab);
        }
        else {
            this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"), { id: 0, type: "stockproduct" });
        }
    }

    public tabs: ITabHandler;
}