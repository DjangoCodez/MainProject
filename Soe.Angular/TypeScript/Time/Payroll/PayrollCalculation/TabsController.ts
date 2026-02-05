import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IReportService } from "../../../Core/Services/ReportService";
import { IPayrollService } from "../PayrollService";
import { IEditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { EditController } from "./EditController";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";

export class TabsController extends TabsControllerBase {

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private $http,
        private $templateCache,
        private $uibModal,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private reportDataService : IReportDataService,
        private payrollService: IPayrollService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        var part: string = "time.payroll.payrollcalculation.";
        super.initialize("payrollcalculation", "", part + "calculation", part + "calculation", "");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeEditTab(this.createEditController()),
        ];
    }

    private createEditController(): IEditControllerBase {
        return new EditController(0, this.$timeout, this.$window, this.$uibModal, this.$http, this.$templateCache, this.coreService, this.payrollService, this.reportService, this.reportDataService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants, this.$q, this.$scope);
    }
}