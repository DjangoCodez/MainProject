import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService"
import { TimeScheduleTemplateBlockTaskDTO } from "../../../../Common/Models/StaffingNeedsDTOs";
import { IScheduleService as ISharedScheduleService } from "../../Schedule/ScheduleService";
import { SoeEntityState } from "../../../../Util/CommonEnumerations";

export class ShiftTasksDirectiveFactory {

    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Time/Directives/ShiftTasks/Views/ShiftTasks.html'),
            scope: {
                tasks: '=',
                showDesc: '=',
                isReadonly: '=',
                shiftIds: '=',
                taskExists: '='
            },
            restrict: 'E',
            replace: true,
            controller: ShiftTasksController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class ShiftTasksController {

    // Terms
    private terms: any;

    // Init parameters
    private showDesc: boolean;
    private isReadonly: boolean;
    private shiftIds: number[];
    private tasks: TimeScheduleTemplateBlockTaskDTO[]; //not always set

    //Flags to parent
    private taskExists: boolean;

    // Data
    private allTasks: TimeScheduleTemplateBlockTaskDTO[];

    get activeTasks(): TimeScheduleTemplateBlockTaskDTO[] {
        return _.filter(this.allTasks, s => s.state == SoeEntityState.Active);
    }

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
        this.allTasks = this.tasks ? this.tasks : [];
        this.sortTasks();
        this.$q.all([
            this.loadTerms(),
            this.loadTasks()
        ]).then(() => {
            this.setupCompleted();
        });
    }

    private setupCompleted() {
        this.taskExists = this.allTasks.length > 0;
    }

    // SERVICE CALLS

    private loadTasks(): ng.IPromise<any> {
        if (this.shiftIds && this.shiftIds.length > 0) {
            return this.sharedScheduleService.getShiftTasks(this.shiftIds).then(x => {
                this.allTasks = x;
                this.sortTasks();
            });
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [

        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    // ACTIONS

    private freeTask(task: TimeScheduleTemplateBlockTaskDTO) {
        var originalTask = _.find(this.allTasks, s => s.timeScheduleTemplateBlockTaskId === task.timeScheduleTemplateBlockTaskId);
        if (originalTask)
            originalTask.state = SoeEntityState.Deleted;
    }

    // HELP-METHODS
    private sortTasks() {
        if (this.allTasks)
            this.allTasks = _.orderBy(this.allTasks, 'startTime');
    }
}
