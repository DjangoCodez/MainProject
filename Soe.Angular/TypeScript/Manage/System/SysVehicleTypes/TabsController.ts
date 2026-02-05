import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { GridController } from "./GridController";
import { ISystemService } from "../SystemService";

export class TabsController extends TabsControllerBase {

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private $http,
        private $templateCache,
        private $uibModal,
        private $filter,
        private coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        super.initialize("sysVehicleTypeId", "name", "manage.system.sysvehicletype", "manage.system.sysvehicletypes", "");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeTab(this.createGridController()),
        ];
    }

    private createGridController(): IGridControllerBase {
        return new GridController(this.$http, this.$templateCache, this.$timeout, this.$uibModal, this.$filter, this.coreService, this.systemService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }
}
