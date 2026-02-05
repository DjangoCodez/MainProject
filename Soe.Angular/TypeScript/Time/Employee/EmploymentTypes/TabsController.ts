
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
        var part: string = "time.employee.employmenttype.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.employmentTypeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "employmenttype", part + "employmenttypes", part + "new_employmenttype");

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