import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController } from "./EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "time.schedule.shifttypelink.";
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "shifftypelink", part + "shifttypelinks", part + "new");
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        this.tabs.addEditTab(row, EditController);
    }

    public tabs: ITabHandler;
}