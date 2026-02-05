import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Constants } from "../../../../Util/Constants";
import { EditController } from "./EditController";
import { GridController } from "./GridController";
import { EditController as ProductEditController } from "../Products/EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private messagingService: IMessagingService) {

        const part = "billing.purchase.pricelists.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(rowAndIds => this.getRowIdentifier(rowAndIds))
            .onGetRowEditName(rowAndIds => this.getRowEditName(rowAndIds))
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "pricelist", part + "pricelists", part + "new");

        this.messagingService.subscribe(Constants.EVENT_OPEN_PURCHASE_PRODUCT, (x) => {
            this.openPurchaseProduct(x);
        });
    }
    
    private edit(row: any) {
        this.tabs.addEditTab(row, EditController, { id: this.getRowIdentifier(row) }, this.urlHelperService.getViewUrl("edit.html"));

    }
    
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    private openPurchaseProduct(row) {
        const activeTab = this.tabs.getTabByIdentifier(row.id);
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, ProductEditController, { id: row.id }, this.urlHelperService.getGlobalUrl("billing/purchase/products/products/views/edit.html"), row.name, true);
        }
    }

    private getRowIdentifier(row): string {
        return row.supplierProductPriceListId ? row.supplierProductPriceListId : row.id;
    }

    private getRowEditName(row) {
        return row.supplierName ? row.supplierName : "";
    }

    public tabs: ITabHandler;
}