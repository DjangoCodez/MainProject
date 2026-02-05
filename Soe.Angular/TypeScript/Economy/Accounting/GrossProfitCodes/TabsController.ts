import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "economy.accounting.grossprofitcode.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.grossProfitCodeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "grossprofitcode", part + "grossprofitcodes", part + "new_grossprofitcode");
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