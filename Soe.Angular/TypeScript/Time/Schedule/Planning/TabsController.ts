import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { TabsControllerBase1 } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController } from "./EditController";
import { Feature } from "../../../Util/CommonEnumerations";

export class TabsController extends TabsControllerBase1 {

    // Terms
    private terms: any;

    // Permissions
    private schedulePermission: boolean = false;
    private scheduleReadPermission: boolean = false;

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        $scope: ng.IScope,
        private $q: ng.IQService) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope)

        // Setup base class
        var part: string = "time.schedule.planning";
        super.initialize(part, part, part + ".new");
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadReadOnlyPermissions(),
            this.loadModifyPermissions()
        ]).then(() => {
            if (this.scheduleReadPermission || this.schedulePermission)
                this.addNewTab(soeConfig.planningMode === 'order' ? this.terms["time.schedule.planning.orderplanning"] : this.terms["time.schedule.planning"], null, EditController, this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Views/edit.html"), {}, false, true);
        })
    }

    protected getEditIdentifier(row: any): any {
        return 0;
    }

    protected getEditName(data: any): string {
        return "";
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "time.schedule.planning",
            "time.schedule.planning.orderplanning"];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadReadOnlyPermissions() {
        let featureIds: number[] = [];
        featureIds.push(Feature.Time_Schedule_SchedulePlanning);
        featureIds.push(Feature.Billing_Order_Planning);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.scheduleReadPermission = x[Feature.Time_Schedule_SchedulePlanning] || x[Feature.Billing_Order_Planning];
        });
    }

    private loadModifyPermissions() {
        let featureIds: number[] = [];
        featureIds.push(Feature.Time_Schedule_SchedulePlanning);
        featureIds.push(Feature.Billing_Order_Planning);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.schedulePermission = x[Feature.Time_Schedule_SchedulePlanning] || x[Feature.Billing_Order_Planning];
        });
    }
}