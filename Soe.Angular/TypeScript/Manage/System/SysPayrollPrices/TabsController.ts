import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.system.syspayrollprices.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.sysPayrollPriceId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                //tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "syspayrollprice", part + "syspayrollprices", part + "new");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController);
    }

    //private add() {
    //    return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    //}

    public tabs: ITabHandler;
}