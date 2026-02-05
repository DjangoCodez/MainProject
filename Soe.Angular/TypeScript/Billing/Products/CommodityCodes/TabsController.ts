import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private tabHandlerFactory: ITabHandlerFactory,
        private $window: ng.IWindowService) {

        const part = "common.commoditycodes.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productUnitId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .initialize(part + "code", part + "codes", part + "code");
    }

    private edit(row: any) {
    }
    private add() {
    }

    public tabs: ITabHandler;
}