import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        const part: string = "time.payroll.payrollimport.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.payrollImportHeadId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .onEdit(row => this.edit(row))
            .initialize("", part + "imports", "");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController, { id: row.payrollImportHeadId, name: row.employeeInfo }, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}