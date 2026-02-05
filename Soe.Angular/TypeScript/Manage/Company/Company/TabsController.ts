import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
                private translationService: ITranslationService,
                tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.actorCompanyId)
            .onGetRowEditName(row => row.CompanyName)
            .onSetupTabs(() => { this.setupTabs(); })
            .onEdit(row => this.edit(row))
            .initialize("dummy", "dummy","dummy");
    }

    private setupTabs() {
        const keys: string[] = [
            "manage.company.editcompany",
            "manage.company.templatecompany",
            "manage.company.activeateedi"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.tabs.addNewTab(terms["manage.company.editcompany"], null, EditController, this.urlHelperService.getViewUrl("edit.html"), { isHomeTab: true, setup: true }, false, true);
            /*this.tabs.addNewTab(terms["manage.company.templatecompany"], null, EditController, this.urlHelperService.getViewUrl("edit.html"), { isHomeTab: true, setup: true }, false, false);
            this.tabs.addNewTab(terms["manage.company.activeateedi"], null, EditController, this.urlHelperService.getViewUrl("edit.html"), { isHomeTab: true, setup: true }, false, false);*/
        });
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    protected edit(rowAndIds: any) {
        this.tabs.addEditTab(rowAndIds.row, EditController, { id: rowAndIds.row.actorCustomerId, ids: rowAndIds.ids }, this.urlHelperService.getViewUrl("edit.html"));
    }
    
    public tabs: ITabHandler;
}