import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "time.schedule.daytype.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.holidayId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize(part + "holiday", part + "holidays", part + "newholiday");
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