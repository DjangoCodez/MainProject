import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../Core/Services/MessagingService";
import { IReportService } from "../../Core/Services/ReportService";
import { ITabHandlerFactory } from "../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { EditController } from "./Editcontroller";
import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { CoreUtility } from "../../Util/CoreUtility";
import { Constants } from "../../Util/Constants";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "common.connect.";
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.importId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {                
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "import", part + "imports", part + "new_import");

        this.messagingService.subscribe(Constants.EVENT_OPEN_IMPORT, x => {
            this.editWithTitle(x.data, x.title, x.data.files);
        });
    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController, { feature: soeConfig.feature, module: soeConfig.module });
    }
    private editWithTitle(row, title: string, files: any) {
        this.tabs.addEditTab(row, EditController, { feature: soeConfig.feature, module: soeConfig.module, files: files }, undefined, title);
    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"), { feature: soeConfig.feature, module: soeConfig.module });
    }

    public tabs: ITabHandler;
}