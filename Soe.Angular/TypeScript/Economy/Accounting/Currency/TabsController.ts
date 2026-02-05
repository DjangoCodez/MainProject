import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { GridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $window: ng.IWindowService) {

        // Setup base class
        var part: string = "economy.accounting.currency.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.currencyId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "currency", part + "currencies", part + "newcurrency");
    }

    private edit(row: any) {
        // Open edit page
        HtmlUtility.openInSameTab(this.$window, "edit/?currencyId=" + row.currencyId);
        //this.tabs.addEditTab(row, EditController);
    }

    private add() {
        // this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
        HtmlUtility.openInSameTab(this.$window, "edit/")
    }

    public tabs: ITabHandler;
}