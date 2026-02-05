import { TabsControllerBase } from "../../../../Core/Controllers/TabsControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { IPayrollService } from "../../PayrollService";
import { IEditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
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
        private reportService: IReportService,
        private payrollService: IPayrollService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        var part: string = "time.payroll.accountprovision.";
        super.initialize("id", "name", part + "accountprovisiontransaction", part + "accountprovisiontransaction", "");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeEditTab(this.createEditController()),
        ];
    }

    private createEditController(): IEditControllerBase {
        return new EditController(0, this.$timeout, this.$window, this.$uibModal, this.coreService, this.payrollService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }
}