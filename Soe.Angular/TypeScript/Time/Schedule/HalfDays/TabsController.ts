import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IScheduleService } from "../ScheduleService";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { IEditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private $window: ng.IWindowService, private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "time.schedule.daytype.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.timeHalfdayId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row.timeHalfdayId))
            .initialize(part + "halfday", part + "halfdays", part + "newhalfday");
    }

    private add() {
        HtmlUtility.openInSameTab(this.$window, "/soe/time/preferences/schedulesettings/halfdays/edit/");
    }

    protected edit(timeHalfdayId: number) {
        HtmlUtility.openInSameTab(this.$window, "/soe/time/preferences/schedulesettings/halfdays/edit/?halfday=" + timeHalfdayId);
    }

    public tabs: ITabHandler;
}