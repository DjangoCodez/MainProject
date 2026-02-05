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
import { SoeModule, SoeCategoryType } from "../../Util/CommonEnumerations";
import { ITabHandlerFactory } from "../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { ITabHandler } from "../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    private type: SoeCategoryType;
    private typeName: string;
    private module: string;

    //@ngInject
    constructor(private $window, private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "common.categories.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.productGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "category", part + "categories", part + "new_category");

        this.setTypeAndModule();
    }

    private edit(row: any) {
        // Open edit page
        //super.addNewTab(id, label, this.createEditController(id));
        HtmlUtility.openInSameTab(this.$window, "/soe/" + this.module + this.typeName + "/categories/edit/?company=" + CoreUtility.actorCompanyId + "&category=" + row.categoryId + "&type=" + this.type);
    }

    private add() {
        HtmlUtility.openInSameTab(this.$window, "/soe/" + this.module + this.typeName + "/categories/edit/?company=" + CoreUtility.actorCompanyId + "&type=" + this.type);
        //this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    private setTypeAndModule() {
        this.type = CoreUtility.getCategoryType(soeConfig.feature);
        this.typeName = "";
        if (this.type === 1)
            this.typeName = "product";
        if (this.type === 2)
            this.typeName = "customer";
        if (this.type === 3)
            this.typeName = "supplier";
        if (this.type === 4)
            this.typeName = "contactpersons";
        if (this.type === 5)
            this.typeName = ""; //todo: AttestRole
        if (this.type === 6)
            this.typeName = "employee";
        if (this.type === 7)
            this.typeName = "project";
        if (this.type === 8)
            this.typeName = "contract";
        if (this.type === 9)
            this.typeName = "inventory";
        if (this.type === 10)
            this.typeName = "order";
        if (this.type === 11)
            this.typeName = ""; //todo: PayrollProduct

        this.module = "billing/";
        if ((this.type === 2 && soeConfig.module === SoeModule.Economy) || this.type === 3 || this.type === 9)
            this.module = "economy/";
        if (this.type === 4)
            this.module = "manage/";
        if (this.type === 6)
            this.module = "time/";
    }

    public tabs: ITabHandler;
}