import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IScheduleService as ISharedScheduleService } from "../../Schedule/ScheduleService";
import { TimeScheduleShiftQueueDTO } from "../../../../Common/Models/TimeSchedulePlanningDTOs";
import { Constants } from "../../../../Util/Constants";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { CompanySettingType } from "../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";

export class ShiftQueueDirectiveFactory {

    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Time/Directives/ShiftQueue/Views/ShiftQueue.html'),
            scope: {
                shiftId: '=',
                nbrOfQueues: '=',
                selectedQueue: '=',
                isOrderPlanning: '@',
                isReadonly: '='
            },
            restrict: 'E',
            replace: true,
            controller: ShiftQueueController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class ShiftQueueController {

    // Terms
    private terms: any;
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;

    // Init parameters
    private shiftId: number;
    private employeeId: number;
    private isOrderPlanning: boolean;
    private isReadonly: boolean;

    // Company settings
    private sortByLas: boolean;

    // Data
    private queues: TimeScheduleShiftQueueDTO[] = [];
    private selectedQueue: TimeScheduleShiftQueueDTO;

    public get nbrOfQueues(): number {
        return this.queues.length;
    }
    public set nbrOfQueues(nbr: number) { /* Not actually a setter, just to make binding work */ }

    //@ngInject
    constructor(
        private sharedScheduleService: ISharedScheduleService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
    }

    // SETUP

    public $onInit() {
        this.setup();
    }

    private setup() {
        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings()
        ]).then(() => {
            this.loadQueue()
        }).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.shiftId, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.loadQueue();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [];

        if (this.isOrderPlanning) {
            keys.push("time.schedule.planning.assignmentdefined");
            keys.push("time.schedule.planning.assignmentundefined");
            keys.push("time.schedule.planning.assignmentsdefined");
            keys.push("time.schedule.planning.assignmentsundefined");
        } else {
            keys.push("time.schedule.planning.shiftdefined");
            keys.push("time.schedule.planning.shiftundefined");
            keys.push("time.schedule.planning.shiftsundefined");
            keys.push("time.schedule.planning.shiftsdefined");
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            if (this.isOrderPlanning) {
                this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            } else {
                this.shiftDefined = this.terms["time.schedule.planning.shiftdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.shiftundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.shiftsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.shiftsundefined"];
            }
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSortQueueByLas);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.sortByLas = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSortQueueByLas);
        });
    }

    private loadQueue(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftQueue(this.shiftId).then(x => {
            this.queues = x;

            if (this.queues.length > 0)
                this.selectedQueue = this.queues[0];
        });
    }

    // ACTIONS

    private assignEmployeeFromQueue(queue: TimeScheduleShiftQueueDTO) {
        var keys: string[] = [
            "time.schedule.planning.shiftqueue.assignfromqueue.asktitle",
            "time.schedule.planning.shiftqueue.assignfromqueue.askmessage"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.schedule.planning.shiftqueue.assignfromqueue.asktitle"].format(this.shiftUndefined), terms["time.schedule.planning.shiftqueue.assignfromqueue.askmessage"].format(this.shiftDefined.toUpperCaseFirstLetter(), queue.employeeName), SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(result => {
                this.messagingService.publish(Constants.EVENT_ASSIGN_EMPLOYEE_FROM_QUEUE, { timeScheduleTemplateBlockId: queue.timeScheduleTemplateBlockId, employeeId: queue.employeeId });
            });
        });
    }

    private removeEmployeeFromQueue(queue: TimeScheduleShiftQueueDTO) {
        var keys: string[] = [
            "time.schedule.planning.shiftqueue.removefromqueue.warningtitle",
            "time.schedule.planning.shiftqueue.removefromqueue.warningmessage"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.schedule.planning.shiftqueue.removefromqueue.warningtitle"], terms["time.schedule.planning.shiftqueue.removefromqueue.warningmessage"].format(queue.employeeName), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.sharedScheduleService.removeEmployeeFromShiftQueue(queue.type, queue.timeScheduleTemplateBlockId, queue.employeeId).then(result => {
                        if (result.success) {
                            this.loadQueue();
                            this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE_BY_SHIFT_ID, queue.timeScheduleTemplateBlockId);
                        }
                    });
                }
            });
        });
    }
}
