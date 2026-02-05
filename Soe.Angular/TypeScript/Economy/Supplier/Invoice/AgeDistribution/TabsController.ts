import { TabsControllerBase1 } from "../../../../Core/Controllers/TabsControllerBase1";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { GridController } from "./GridController";

export class TabsController extends TabsControllerBase1 {

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope);

        // Setup base class
        var part: string = "economy.supplier.invoice.agedistribution.";
        super.initialize(part + "agedistribution", part + "agedistribution", part + "new");
    }

    protected setupTabs() {
        this.enableRemoveAll();
        this.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridWithGrouping.html"));
    }
}