import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
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
        coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        super("Soe.Manage.System.SysCompany.SysCompany", "Soe.Manage.System.SysCompany.SysCompany", Feature.Manage_System, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.database",
            "common.server",
            "core.edit",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            super.addColumnText("name", terms["common.name"], null);
            super.addColumnText("dbName", terms["common.database"], null);
            super.addColumnText("serverName", terms["common.server"], null);
            super.addColumnEdit(terms["core.edit"]);
        });

    }

    public loadGridData() {
        // Load data
        this.systemService.getSysCompanies().then((x) => {
            _.forEach(x, y => {
                y.dbName = y.sysCompDBDTO.name;
                y.serverName = y.sysCompDBDTO.sysCompServerDTO.name;
            });
            super.gridDataLoaded(x);
        });
    }
}
