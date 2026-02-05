import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        var part: string = "manage.admin.language.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.fieldId)
            .onGetRowEditName(row => row.fieldName)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, { isHomeTab: true }, this.urlHelperService.getViewUrl("edit.html"));
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "translation", part + "translations", part + "newtranslation");
    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController, { dto: row });
    }

    public tabs: ITabHandler;
}