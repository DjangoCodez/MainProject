import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        const part = "common.customer.customer.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => this.getRowIdentifier(row))
            .onGetRowEditName(row => this.getRowEditName(row))
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "customer", part + "customers", part + "new");
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    protected getEditIdentifier(row: any): any {
        return row.actorCustomerId;
    }

    protected edit(rowAndIds: any) {
        this.tabs.addEditTab(rowAndIds.row, EditController, { id: rowAndIds.row.actorCustomerId, ids: rowAndIds.ids }, this.urlHelperService.getViewUrl("edit.html"));
    }

    private getRowIdentifier(row: any): string {
        return row.row ? row.row.actorCustomerId : row.actorCustomerId;
    }

    private getRowEditName(row: any): string {
        return row.customerNr ? row.customerNr : "";
    }

    public tabs: ITabHandler;
}