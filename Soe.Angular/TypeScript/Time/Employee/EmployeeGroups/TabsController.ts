import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private $window: ng.IWindowService) {

        // Setup base class
        var part: string = "time.employee.employeegroup.";


        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.employeeGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "employeegroup", part + "employeegroups", "newemployeegroup");
    }

    protected edit(row: any) {
        // Open edit page
        HtmlUtility.openInSameTab(this.$window, "/soe/time/employee/groups/edit/?group=" + row.employeeGroupId);
    }
    private add() {
        HtmlUtility.openInSameTab(this.$window, "/soe/time/employee/groups/edit/");
    }

    public tabs: ITabHandler;
}
