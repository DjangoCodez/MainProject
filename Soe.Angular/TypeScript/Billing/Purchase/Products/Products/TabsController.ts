import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditController } from "./EditController";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        const part = "billing.purchase.product.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => this.getRowIdentifier(row))
            .onGetRowEditName(row => row.supplierProductNr ? row.supplierProductNr : "")
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "product", part + "products", part + "new_product");
    }

    private getRowIdentifier(row: any): string {
        return row.row ? row.row.supplierProductId : row.supplierProductId;
    }
    
    private edit(rowAndIds: any) {
        this.tabs.addEditTab(rowAndIds.row, EditController, { id: this.getRowIdentifier(rowAndIds), ids: rowAndIds.ids }, this.urlHelperService.getViewUrl("edit.html"));
    }
    
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}