import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { GridController } from "./GridController";
import { GDPRService } from "../../GDPRService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(tabHandlerFactory: ITabHandlerFactory, protected urlHelperService: IUrlHelperService, protected translationService: ITranslationService, protected gdprService: GDPRService) {

        var part: string = "manage.gdpr.registry.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.id)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {

                return this.translationService.translate("manage.gdpr.registry.handleinfo").then((term) => {
                    this.tabs.addNewTab(term, null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), null, false, true)
                });
            })
            .initialize(part + "handleinfo", part + "handleinfo", "");
    }

    public tabs: ITabHandler;
}