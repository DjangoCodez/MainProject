import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AttestEmployeeDayDTO, AttestEmployeeDayTimeCodeTransactionDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { Feature, TimeAttestMode } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { EditController as TimeRuleEditController } from "../../Time/TimeRules/EditController";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";

export class TimeCodeTransactionDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/TimeCodeTransaction.html'),
            scope: {
                model: '=',
                selectedTimeCodeTransactionId: '=',
                onResizeNeeded: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TimeCodeTransactionController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class TimeCodeTransactionController {

    // Init parameters
    private model: AttestEmployeeDayDTO;
    private selectedTimeCodeTransactionId: number;
    private onResizeNeeded: Function;
    private attestMode: TimeAttestMode;
    private get isMyTime(): boolean {
        return this.attestMode == TimeAttestMode.TimeUser;
    }

    //Permission
    private showTimeCodeTransactionsPermission: boolean = false;
    private showTimeRulePermission: boolean = false;

    // Flags
    private expanded: boolean = true;

    // Properties
    private selectedTransaction: AttestEmployeeDayTimeCodeTransactionDTO;
    private sortBy: string = 'startTime';
    private sortByReverse: boolean = false;

    private modalInstance: any;

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private coreService: ICoreService) {

        // Config parameters
        this.attestMode = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;

        this.modalInstance = $uibModal;
    }

    public $onInit() {
        this.$q.all([
            this.loadReadPermissions()
        ]);
    }

    // SERVICE CALLS

    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        if (this.isMyTime) {
            featureIds.push(Feature.Time_Time_AttestUser_ShowTimeCodeTransactions);
        } else {
            featureIds.push(Feature.Time_Time_Attest_ShowTimeCodeTransactions);
            featureIds.push(Feature.Time_Preferences_TimeSettings_TimeRule_Edit);
        }

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (this.isMyTime) {
                this.showTimeCodeTransactionsPermission = x[Feature.Time_Time_AttestUser_ShowTimeCodeTransactions];
            } else {
                this.showTimeCodeTransactionsPermission = x[Feature.Time_Time_Attest_ShowTimeCodeTransactions];
                this.showTimeRulePermission = x[Feature.Time_Preferences_TimeSettings_TimeRule_Edit];
            }
        });
    }

    // EVENTS

    private toggleExpanded() {
        this.expanded = !this.expanded;

        if (this.onResizeNeeded) {
            this.$timeout(() => {
                this.onResizeNeeded();
            });
        }
    }

    private sort(column: string) {
        this.sortByReverse = !this.sortByReverse && this.sortBy === column;
        this.sortBy = column;
    }

    private transactionSelected(trans: AttestEmployeeDayTimeCodeTransactionDTO) {
        if (!trans)
            return;
        this.selectedTransaction = trans;
        this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_TIMECODETRANSACTION_SELECTED, trans.timeCodeTransactionId);
    }

    private editTimeRule(timeRuleId: number) {
        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeRules/Views/edit.html"),
            controller: TimeRuleEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: timeRuleId,
            });
        });
    }
}
