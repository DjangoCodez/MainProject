import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./ProjectTransactions/EditController";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $window: ng.IWindowService) {

        // Setup base class
        var part: string = "common.report.report.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.reportId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "report", part + "reports", part + "new_report");

    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController, null, this.urlHelperService.getGlobalUrl("/Common/Reports/ReportGrid/ProjectTransactions/Views/edit.html"));
    }
    private add() {
        HtmlUtility.openInSameTab(this.$window, "edit/")
    }

    public tabs: ITabHandler;
}