import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private $window: ng.IWindowService,
        tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "manage.attest.role.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.attestRoleId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "role", part + "roles", part + "new");
    }

    private edit(row: any) {
        var url: string = "/soe/manage/attest/customer/role/edit/";
        if (row.attestRoleId !== 0)
            url += "?role=" + row.attestRoleId;

        HtmlUtility.openInSameTab(this.$window, url);
    }

    private add() {
        var url: string = "/soe/manage/attest/customer/role/edit/";
        HtmlUtility.openInSameTab(this.$window, url);
    }

    tabs: ITabHandler;
}
