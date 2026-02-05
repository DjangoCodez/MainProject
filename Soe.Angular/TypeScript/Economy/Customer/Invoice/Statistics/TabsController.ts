import { TabsControllerBase1 } from "../../../../Core/Controllers/TabsControllerBase1";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridController } from "./GridController";

export class TabsController extends TabsControllerBase1 {

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private $http,
        private $templateCache,
        private $uibModal,
        private $filter,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope);

        // Setup base class
        var part: string = "economy.customer.invoice.statistics.";
        super.initialize(part + "statistics", part + "statistics", part + "new");
    }

    protected setupTabs() {
        this.enableRemoveAll();
        this.addHomeTab(GridController);
    }
}
