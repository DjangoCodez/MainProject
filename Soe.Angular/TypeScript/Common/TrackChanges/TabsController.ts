import { TabsControllerBase } from "../../Core/Controllers/TabsControllerBase";
import { IGridControllerBase } from "../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../Core/Services/MessagingService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IReportService } from "../../Core/Services/ReportService";
import { CoreUtility } from "../../Util/CoreUtility";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { GridController } from "./GridController";
import { SoeCategoryType, SoeEntityType } from "../../Util/CommonEnumerations";
import { ITabHandlerFactory } from "../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { EditController as SupplierEditController } from "../../Shared/Economy/Supplier/Suppliers/EditController";

export class TabsController implements ICompositionTabsController {

    private type: SoeCategoryType;
    private typeName: string;
    private module: string;

    //@ngInject
    constructor(private $window, private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.enableRemoveAll();
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .onEdit(row => this.edit(row))
            .initialize("common.trackchanges.trackchanges", "common.trackchanges.trackchanges", "common.trackchanges.trackchanges");
    }

    private edit(row: any) {
        if (row.id && row.id > 0) {
            if (row.entityType === SoeEntityType.Supplier)
                this.tabs.addEditTab(row, SupplierEditController, { id: row.id, updateCaption: true }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html"), "");
        }
    }

    private add() {
    }

    public tabs: ITabHandler;
}