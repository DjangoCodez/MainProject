import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITimeService } from "../TimeService";
import { IEditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { EditController } from "./EditController";
import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";

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
        private reportService: IReportService,
        private timeService: ITimeService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        var part: string = "time.time.timeearnedholiday.";
        super.initialize("id", "name", part + "timeearnedholiday", part + "timeearnedholiday", "");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeEditTab(this.createEditController()),
        ];
    }

    private createEditController(): IEditControllerBase {
        return new EditController(0, this.$timeout, this.$window, this.$uibModal, this.coreService, this.timeService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }
}