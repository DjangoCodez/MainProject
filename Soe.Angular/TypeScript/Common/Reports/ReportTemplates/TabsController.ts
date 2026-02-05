import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SoeModule } from "../../../Util/CommonEnumerations";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { Guid } from "../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private $timeout: ng.ITimeoutService,
        private $window: ng.IWindowService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = soeConfig.isSys === true ? "common.report.sysreporttemplate." : "common.report.userreporttemplate.";
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => soeConfig.isSys === true ? row.sysReportTemplateId : row.reportTemplateId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "reporttemplate", part + "reporttemplates", part + "new");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController, null, this.urlHelperService.getViewUrl("edit.html"));
    }

    private add(): Guid {
        return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}
