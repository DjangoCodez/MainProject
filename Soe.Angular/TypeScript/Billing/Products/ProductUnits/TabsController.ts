import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private tabHandlerFactory: ITabHandlerFactory,
        private $window: ng.IWindowService) {

        const part = "billing.product.productunit.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productUnitId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "productunit", part + "productunits", part + "new");
    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController);
    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}