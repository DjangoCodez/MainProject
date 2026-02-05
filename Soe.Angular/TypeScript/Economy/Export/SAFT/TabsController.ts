import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        private tabHandlerFactory: ITabHandlerFactory,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private $window: ng.IWindowService) {

        const part = "economy.export.saft";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productUnitId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .initialize(part, part, part);
    }

    private edit(row: any) {
    }

    private add() {
    }


    public tabs: ITabHandler;
}