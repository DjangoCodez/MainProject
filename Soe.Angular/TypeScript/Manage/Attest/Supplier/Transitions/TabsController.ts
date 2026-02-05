import { IUrlHelperService} from "../../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { ITabHandlerFactory } from "../../../../Core/Handlers/tabhandlerfactory";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private $window: ng.IWindowService,private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        // Setup base class
        var part: string = "manage.attest.transition.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.attestTransitionId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "role", part + "transitions", part + "new");

    }

    tabs: ITabHandler;
    private url: string = "/soe/manage/attest/supplier/transition/edit/";

    private add() {
        HtmlUtility.openInSameTab(this.$window, this.url);
    }

    private edit(row: any) {
        // Open edit page
        if (row.attestTransitionId !== 0)
            this.url += "?transition=" + row.attestTransitionId;

        HtmlUtility.openInSameTab(this.$window, this.url);
    }
}
