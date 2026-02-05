import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { MassUpdateEmployeeGridController } from "./GridController";

export class TabsController implements ICompositionTabsController {

    terms: any;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $q: ng.IQService,
        private translationService: ITranslationService) {

        // Setup base class
        var part: string = "time.employee.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.importId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                this.setupTabs();
            })
            .initialize("", part + "massupdateemployeefields", "");
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
        ]).then(() => {
            this.tabs.addHomeTab(MassUpdateEmployeeGridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
        });
    }

    //LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public tabs: ITabHandler;
}