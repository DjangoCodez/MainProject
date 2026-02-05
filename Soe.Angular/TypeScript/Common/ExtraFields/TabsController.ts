import { TabsControllerBase } from "../../Core/Controllers/TabsControllerBase";
import { IGridControllerBase } from "../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../Core/Services/MessagingService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IReportService } from "../../Core/Services/ReportService";
import { CoreUtility } from "../../Util/CoreUtility";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { GridController } from "./GridController";
import { SoeModule, SoeCategoryType } from "../../Util/CommonEnumerations";
import { ITabHandlerFactory } from "../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { ITabHandler } from "../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    private type: SoeCategoryType;
    private typeName: string;
    private module: string;

    //@ngInject
    constructor(private $window, private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.extraFieldId)
            .onGetRowEditName(row => row.text)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .initialize("common.extrafields.extrafields", "common.extrafields.extrafields", "common.extrafields.extrafields");
    }

    private edit(row: any) {
    }

    private add() {
    }

    public tabs: ITabHandler;
}