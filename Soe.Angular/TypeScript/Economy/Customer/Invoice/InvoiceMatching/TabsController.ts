import { TabsControllerBase1 } from "../../../../Core/Controllers/TabsControllerBase1";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

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

        var part: string = "common.customer.customer.";
        super.initialize(part + "customer", part + "customers", part + "new");
    }

    protected setupTabs() {
        this.enableRemoveAll();
        this.addHomeTab(GridController);
    }

    protected getEditIdentifier(row: any): any {
        return row.liquidityPlanningId;
    }

    protected getEditName(data: any): string {
        return data.name;
    }

    protected edit(row: any) {
        this.addEditTab(row.name, row.id, EditController, { id: row.id });
    }
}