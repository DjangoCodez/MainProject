import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { GridController } from "./GridController";
import { SoeModule } from "../../../Util/CommonEnumerations";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private $window, private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "common.report.reportgroup.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.edit(null));
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "reportgroup", part + "reportgroups", part + "new");
    }

    protected edit(row: any) {
        // Open edit page
        if(row)
            HtmlUtility.openInSameTab(this.$window, "/soe/" + SoeModule[soeConfig.module].toLowerCase() + "/distribution/groups/edit/?group=" + row.reportGroupId + "&company=" + CoreUtility.actorCompanyId);
        else
            HtmlUtility.openInSameTab(this.$window, "/soe/" + SoeModule[soeConfig.module].toLowerCase() + "/distribution/groups/edit/?company=" + CoreUtility.actorCompanyId);
    }

    public tabs: ITabHandler;
}