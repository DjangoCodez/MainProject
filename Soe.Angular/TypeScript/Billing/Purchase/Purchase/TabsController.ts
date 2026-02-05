import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Billing/Purchase/Purchase/EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private translationService: ITranslationService) {

        const part = "billing.purchase.list.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => this.getRowIdentifier(row))
            .onGetRowEditName(row => row.purchaseNr ? row.purchaseNr : "")
            .onSetupTabs((tabHandler) => { this.setupTabs(tabHandler);  })
            .onEdit(row => this.edit(row))
            .initialize(part + "purchase", part + "purchases", part + "new_purchase");
    }

    protected setupTabs(tabHandler: ITabHandler) {
        tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
        tabHandler.enableAddTab(() => this.add());
        tabHandler.enableRemoveAll();
        if (soeConfig.purchaseId > 0) {
            this.translationService.translateMany(['billing.purchase.list.purchase']).then((terms) => {
                const params = { id: soeConfig.purchaseId }
                const templateUrl = this.urlHelperService.getViewUrl("edit.html");
                const title = `${terms['billing.purchase.list.purchase']} ${soeConfig.purchaseNr}`;
                this.tabs.addEditTab({ purchaseId: soeConfig.purchaseId }, EditController, params, templateUrl, title, true);
            });
        }
    }

    private getRowIdentifier(row: any): string {
        return row.row ? row.row.purchaseId : row.purchaseId;
    }

    private edit(rowAndIds: any) {
        this.tabs.addEditTab(rowAndIds.row, EditController, { id: this.getRowIdentifier(rowAndIds), ids: rowAndIds.ids }, this.urlHelperService.getViewUrl("edit.html"));
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}