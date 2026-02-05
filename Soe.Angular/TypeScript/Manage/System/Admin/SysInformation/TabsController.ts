import { ICompositionTabsController } from "../../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ILazyLoadService } from "../../../../Core/Services/LazyLoadService";
import { ITabHandlerFactory } from "../../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { Guid } from "../../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {
    private htmlEditorLoaderPromise: Promise<any>;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private lazyLoadService: ILazyLoadService) {

        var part: string = "manage.system.admin.sysinformation.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.informationId)
            .onGetRowEditName(row => row.subject)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "item", part + "items", part + "new");
    }

    $onInit() {
        this.htmlEditorLoaderPromise = this.lazyLoadService.loadBundle("Soe.Common.HtmlEditor.Bundle");
    }

    private edit(row: any) {
        this.htmlEditorLoaderPromise.then(() => {
            // Open edit page
            this.tabs.addEditTab(row, EditController);
        });
    }

    private add(): Guid {
        return this.tabs.addCreateNewTab(EditController);
    }

    public tabs: ITabHandler;
}