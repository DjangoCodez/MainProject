import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { SoeLogType } from "../../../Util/CommonEnumerations";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private $timeout: ng.ITimeoutService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.support.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.sysLogId)
            .onGetRowEditName(row => row.sysLogId)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "log", this.getTabName(part + "logs"), part + "new");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController);
    }

    private getTabName(part: string): string {
        var key = part + ".";
        var logType = soeConfig.logType;
        if (logType == SoeLogType.System_All_Today)
            key += 'all';
        else if (logType == SoeLogType.System_Error_Today)
            key += 'error';
        else if (logType == SoeLogType.System_Warning_Today)
            key += 'warning';
        else if (logType == SoeLogType.System_Information_Today)
            key += 'information';
        else if (logType == SoeLogType.System_Search)
            key += 'search';
        return key;
    }

    public tabs: ITabHandler;
}