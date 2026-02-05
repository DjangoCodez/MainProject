import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITimeService } from "../TimeService";
import { ITimeService as ISharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { IEditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { EditController } from "./EditController";
import { TimeAttestMode, Feature } from "../../../Util/CommonEnumerations";
import { IScheduleService } from "../../Schedule/ScheduleService";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";

export class TabsController extends TabsControllerBase {
    mode: TimeAttestMode;

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
        private reportDataService: IReportDataService,
        private timeService: ITimeService,
        private sharedTimeService: ISharedTimeService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        this.mode = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;

        // Setup base class
        var part: string = "time.time.attest.";
        var name: string =  part + (this.mode === TimeAttestMode.TimeUser ? "attestemployeeuser" : "attestemployee");
        super.initialize("timeattest", "", name, name, "");
       
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeEditTab(this.createEditController()),
        ];
    }

    private createEditController(): IEditControllerBase {
        return new EditController(0, this.$timeout, this.$window, this.$uibModal, this.$http, this.$templateCache, this.$filter, this.coreService, this.timeService, this.sharedTimeService, this.reportService, this.reportDataService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants, this.$q, this.$scope, this.GetFeature());
    }

    private GetFeature(): Feature {
        if (this.mode === TimeAttestMode.Time)
            return Feature.Time_Time_Attest;
        else if (this.mode === TimeAttestMode.TimeUser)
            return Feature.Time_Time_AttestUser;
        else
            return Feature.Billing_Project_Attest
    }
}