import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { EditController } from "./EditController";
import { Guid } from "../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService) {

        var part: string = "time.time.timecalendar.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
            })
            .initialize(part + "timecalendar", part + "timecalendar", "");
    }

    //private edit(row: any) {
    //    this.tabs.addEditTab(row, EditController);
    //}

    //private add(): Guid {
    //    return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    //}

    public tabs: ITabHandler;
}