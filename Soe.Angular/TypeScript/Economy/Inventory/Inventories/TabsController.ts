import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Economy/Inventory/Inventories/EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        // Setup base class
        var part: string = "economy.inventory.inventories.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => { if (row.row) return row.row.inventoryId; else return row.inventoryId; })
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "inventory", part + "inventory", part + "new_inventory");
    }

    protected getEditIdentifier(row: any): any {
        return row.inventoryId;
    }

    private edit(rowAndIds: any) {
        // Open edit page
        //this.tabs.addEditTab(row, EditController, null, this.urlHelperService.getGlobalUrl("Shared/Economy/Inventory/Inventories/Views/edit.html"));
        this.tabs.addEditTab(rowAndIds.row, EditController, { id: this.getEditIdentifier(rowAndIds), ids: rowAndIds.ids }, this.urlHelperService.getGlobalUrl("Shared/Economy/Inventory/Inventories/Views/edit.html"));

    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Inventory/Inventories/Views/edit.html"));
    }

    public tabs: ITabHandler;
}