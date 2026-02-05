import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Guid } from "../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private messagingService: IMessagingService,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService) {

        var part: string = "time.time.timeterminal.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.timeTerminalId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize("timeterminal", part + "timeterminals", part + "newtimeterminal");
    }

    private edit(row: any, data: any) {
        let activateTab: boolean = true;
        if (data && data.doNotActivateTab)
            activateTab = false;

        this.tabs.addEditTab(row, EditController, { navigatorRecords: data }, this.urlHelperService.getViewUrl("edit.html"), null, activateTab);
    }

    private add(): Guid {
        return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}