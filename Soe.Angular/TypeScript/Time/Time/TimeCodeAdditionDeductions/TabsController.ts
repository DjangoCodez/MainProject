import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "time.time.timecodeadditiondeductions.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.timeCodeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize(part + "timecodeadditiondeduction", part + "timecodeadditiondeductions", part + "new");
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        // Open edit page
        this.tabs.addEditTab(row, EditController, { navigatorRecords: data });
    }

    private add() {
        return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}