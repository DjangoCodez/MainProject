import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IImportService } from "../ImportService";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { IEditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

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
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private coreService: ICoreService,
        private importService: IImportService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        var part: string = "economy.import.";
        super.initialize("importId", "", part + "automaster", part + "automaster", "automaster");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeEditTab(this.createEditController()),
        ];
    }

    private createGridController(): IGridControllerBase {
        return new GridController(this.$http, this.$templateCache, this.$timeout, this.$uibModal, this.$filter, this.coreService, this.importService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }

    private createEditController(): IEditControllerBase {
        return new EditController(this.$uibModal, this.coreService, this.importService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService);
    }
}