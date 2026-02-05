import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        var part: string = "time.employee.vacationdebt.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(item => item.row.employeeId + item.row.employeeCalculateVacationResultHeadId)
            .onGetRowEditName(item => item.row.employeeName + " " + item.row.dateStr)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));    
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "vacationdebt", part + "vacationdebt", "");
    }

    private edit(item: any) {
        
        this.tabs.addEditTab(item, EditController, { resultHeadId: item.row.employeeCalculateVacationResultHeadId, employeeId: item.row.employeeId, employeeNr: item.row.employeeNr, employeeName: item.row.employeeName, dateStr: item.row.dateStr });
    }

    public tabs: ITabHandler;
}