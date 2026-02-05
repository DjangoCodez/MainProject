import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        var part: string = "time.schedule.staffingneedslocation.";
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.staffingNeedsLocationId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize(part + "staffingneedslocation", part + "staffingneedslocations", part + "new");
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        // Open edit page
        this.tabs.addEditTab(row, EditController, { navigatorRecords: data });
    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}