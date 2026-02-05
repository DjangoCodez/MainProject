import { TabsControllerBase1 } from "../../../../Core/Controllers/TabsControllerBase1";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

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
        private coreService: ICoreService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope);

        // Setup base class
        const part = "manage.system.syscompany.";
        super.initialize(part + "syscompany", part + "syscompany", part + "new");
    }
    protected setupTabs() {
        this.enableRemoveAll();
        this.enableAddTab();
        this.addHomeTab(GridController);
    }

    protected add() {
        this.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        this.addEditTab(this.getEditName(row), this.getEditIdentifier(row), EditController, { id: this.getEditIdentifier(row) });
    }

    protected getEditIdentifier(row: any): any {
        return row.sysCompanyId;
    }

    protected getEditName(row: any): string {
        return row.name;
    }
}
