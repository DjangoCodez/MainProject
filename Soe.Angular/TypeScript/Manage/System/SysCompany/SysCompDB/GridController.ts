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

        super("Soe.Manage.System.SysCompany.SysCompDB", "Soe.Manage.System.SysCompany.SysCompDB", Feature.Manage_System, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.server",
            "core.edit",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            super.addColumnText("name", terms["common.name"], null);
            super.addColumnText("serverName", terms["common.server"], null);
            super.addColumnEdit(terms["core.edit"]);
        });

    }

    public loadGridData() {
        // Load data
        this.systemService.getSysCompDBs().then((x) => {
            _.forEach(x, y => {
                y.serverName = y.sysCompServerDTO.name;
            });
            super.gridDataLoaded(x);
        });
    }
}