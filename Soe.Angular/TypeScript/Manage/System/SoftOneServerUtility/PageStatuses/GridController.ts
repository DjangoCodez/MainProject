import { ICoreService } from "../../../../Core/Services/CoreService";
import { ISystemService } from "../../SystemService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase {

    statusTypes: any[] = [];

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

        super("Manage.System.SoftOneServerUtility.PageStatuses", "manage.system.softoneserverutility.pagestatuses", Feature.Manage_System, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.loadStatusTypes();
    }

    protected setupGrid() {

        // Columns
        var keys: string[] = [
            "manage.system.softoneserverutility.pagestatuses.pagename",
            "manage.system.softoneserverutility.pagestatuses.betastatus",
            "manage.system.softoneserverutility.pagestatuses.livestatus",
            "common.created",
            "common.createdby",
            "common.modified",
            "common.modifiedby",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            super.addColumnText("pageName", terms["manage.system.softoneserverutility.pagestatuses.pagename"], null);
            super.addColumnSelect("betaStatusName", terms["manage.system.softoneserverutility.pagestatuses.betastatus"], null, this.statusTypes);
            super.addColumnSelect("liveStatusName", terms["manage.system.softoneserverutility.pagestatuses.livestatus"], null, this.statusTypes);
            super.addColumnDateTime("created", terms["common.created"], null);
            super.addColumnText("createdBy", terms["common.createdby"], null);
            super.addColumnDateTime("modified", terms["common.modified"], null);
            super.addColumnText("modifiedBy", terms["common.modifiedby"], null);
        });
    }

    private loadStatusTypes() {
        this.coreService.getTermGroupContent(TermGroup.SysPageStatusStatusType, true, false).then((x) => {
            _.forEach(x, (y: any) => {
                if (y['id'] > -1)
                    this.statusTypes.push({ value: y.name, label: y.name })
            });
        });
    }

    public loadGridData() {
        this.systemService.getSoftOneServerUtilityPageStatuses().then((x) => {
            super.gridDataLoaded(x);
        });
    }
}


