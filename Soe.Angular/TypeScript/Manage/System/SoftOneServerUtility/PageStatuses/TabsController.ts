import { ICoreService } from "../../../../Core/Services/CoreService";
import { ISystemService } from "../../SystemService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { TabsControllerBase } from "../../../../Core/Controllers/TabsControllerBase";
import { IGridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { GridController } from "./GridController";

export class TabsController extends TabsControllerBase {

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private $http,
        private $templateCache,
        private $uibModal,
        private coreService: ICoreService,
        private systemService: ISystemService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        super.initialize("sysFeatureId", "", "", "manage.system.softoneserverutility.pagestatuses", "");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeTab(this.createGridController()),
        ];
    }

    private createGridController(): IGridControllerBase {
        return new GridController(this.$http, this.$templateCache, this.$timeout, this.$uibModal, this.coreService, this.systemService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }
}
