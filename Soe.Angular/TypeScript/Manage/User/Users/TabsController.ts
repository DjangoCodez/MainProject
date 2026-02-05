import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { Guid } from "../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private $timeout: ng.ITimeoutService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.user.user.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.userId)
            .onGetRowEditName(row => '{0} - {1}'.format(row.loginName, row.name))
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "user", part + "users", part + "new");
    }

    private $onInit() {
        var selectedUserId = soeConfig.selectedUserId;
        if (selectedUserId && selectedUserId > 0) {
            var row = { userId: selectedUserId };
            this.$timeout(() => { this.edit(row) });
        }
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController);
    }

    private add(): Guid {
        return this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}