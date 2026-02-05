import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Billing/Products/Products/EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { EditController as PurchaseProductEditController } from "../../Purchase/Products/Products/EditController"
import { Constants } from "../../../Util/Constants";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        messagingService: IMessagingService,
        private translationService: ITranslationService) {

        // Setup base class
        const part = "billing.products.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => this.getRowIdentifier(row))
            .onGetRowEditName(row => row.number ? row.number : "")
            .onSetupTabs((tabHandler) => { this.setupTabs(tabHandler); })
            .onEdit(row => this.edit(row))
            .initialize(part + "product", part + "products", part + "newproduct");

        messagingService.subscribe(Constants.EVENT_OPEN_PURCHASE_PRODUCT, (x) => {
            this.openPurchaseProduct(x);
        });
    }

    protected setupTabs(tabHandler: ITabHandler) {
        tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
        tabHandler.enableAddTab(() => this.add());
        tabHandler.enableRemoveAll();
        if (soeConfig.productId > 0) {
            this.translationService.translateMany(['billing.products.product']).then((terms) => {
                const params = { id: soeConfig.productId }
                const templateUrl = this.urlHelperService.getViewUrl("edit.html");
                const title = `${terms['billing.products.product']} ${soeConfig.productNr}`;
                this.tabs.addEditTab({ productId: soeConfig.productId }, EditController, params, templateUrl, title, true);
            });
        }
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController);
    }

    private getRowIdentifier(row: any): string {
        return row.row ? row.row.productId : row.productId;
    }

    protected edit(row: any) {
        this.tabs.addEditTab(row.row, EditController, { id: this.getRowIdentifier(row), ids: row.ids }, this.urlHelperService.getViewUrl("edit.html"));
    }

    private openPurchaseProduct(row) {
        const activeTab = this.tabs.getTabByIdentifier(row.id);
        if (activeTab && !row.createNew) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, PurchaseProductEditController, { id: row.id, productId: row.productId, createNew: row.createNew }, this.urlHelperService.getGlobalUrl("billing/purchase/products/products/views/edit.html"), row.name, true);
        }
    }

    public tabs: ITabHandler;
}