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
        var part: string = "time.employee.payrolllevel.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.payrollLevelId)
            .onGetRowEditName(row => row.name ? row.name : "")
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "payrolllevel", part + "payrolllevels", part + "new");
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        this.tabs.addEditTab(row, EditController, { navigatorRecords: data });
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}