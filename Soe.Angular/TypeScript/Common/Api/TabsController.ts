import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { ITabHandlerFactory } from "../../Core/Handlers/TabHandlerFactory";
import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { DeviationsGridController } from "./DeviationsGridController";

export class TabsController implements ICompositionTabsController {

    terms: any;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $q: ng.IQService,
        private translationService: ITranslationService) {

        // Setup base class
        var part: string = "common.api.";
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.importId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                this.setupTabs();
            })
            .initialize("", part + "messages", "");
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
        ]).then(() => {

            this.tabs.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            this.tabs.addNewTab(this.terms["common.api.settings"], null, EditController, this.urlHelperService.getViewUrl("edit.html"), { isHomeTab: true, setup: true }, false, false);
            this.tabs.addNewTab(this.terms["common.api.deviations"], null, DeviationsGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, setup: true }, false, false);
        });
    }

    //LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.api.settings",
            "common.api.deviations",
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public tabs: ITabHandler;
}