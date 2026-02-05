import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "core.eventhistory.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.eventHistoryId)
            .onGetRowEditName(row => "#" + row.batchId + ": " + row.typeName + ", " + row.recordName)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize("", part + "records", "");
    }

    protected edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController);
    }

    public tabs: ITabHandler;
}