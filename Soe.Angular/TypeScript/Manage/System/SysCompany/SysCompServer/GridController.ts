import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ISystemService } from "../../SystemService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { Feature } from "../../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase {
    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        super("Soe.Manage.System.SysCompany.SysCompServer", "Soe.Manage.System.SysCompany.SysCompServer", Feature.Manage_System, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "core.edit",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            super.addColumnText("name", terms["common.name"], null);
            super.addColumnEdit(terms["core.edit"]);
        });

    }

    public loadGridData() {
        // Load data
        this.systemService.getSysCompServers().then((x) => {
            super.gridDataLoaded(x);
        });
    }
}
