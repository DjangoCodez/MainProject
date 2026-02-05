import { TabsControllerBase } from "../../../../Core/Controllers/TabsControllerBase";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IEmployeeService } from "../../EmployeeService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { IGridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { IEditControllerBase } from "../../../../Core/Controllers/EditControllerBase";

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
        private employeeService: IEmployeeService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        var part: string = "time.employee.csr.";
        super.initialize("importId", "import", part + "import", part + "imports", "import");
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeEditTab(this.createEditController()),
        ];
    }

    private createGridController(): IGridControllerBase {
        return new GridController(this.$http, this.$templateCache, this.$timeout, this.$uibModal, this.$filter, this.coreService, this.employeeService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }

    private createEditController(): IEditControllerBase {
        return new EditController(this.$uibModal, this.coreService, this.employeeService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService);
    }
}
