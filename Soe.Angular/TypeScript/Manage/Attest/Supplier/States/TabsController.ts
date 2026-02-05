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

        var part: string = "manage.attest.state.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.attestStateId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "state", part + "states", part + "new");
    }

    private edit(row: any) {
        var url: string = "/soe/manage/attest/supplier/state/edit/";
        if (row.attestStateId !== 0)
            url += "?state=" + row.attestStateId;

        HtmlUtility.openInSameTab(this.$window, url);
    }

    private add() {
        var url: string = "/soe/manage/attest/supplier/state/edit/";
        HtmlUtility.openInSameTab(this.$window, url);
    }

    public tabs: ITabHandler;
}
