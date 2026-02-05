import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IScheduleService } from "../../../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { StaffingNeedsTaskDTO } from "../../../../../Common/Models/StaffingNeedsDTOs";
import { EditControllerBase } from "../../../../../Core/Controllers/EditControllerBase";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { StringUtility } from "../../../../../Util/StringUtility";
import { Feature, TermGroup_TimeScheduleTemplateBlockType, SoeScheduleWorkRules, TermGroup_ShiftHistoryType } from "../../../../../Util/CommonEnumerations";

export class AssignTaskController extends EditControllerBase {

    // Terms
    private notSameDateInfo: string;

    // Data
    public targetShifts: ShiftDTO[] = [];

    // Flags
    private notSameDate: boolean = false;
    private hasInvalidSkills: boolean = false;
    private executing: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        coreService: ICoreService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private $q: ng.IQService,
        private tasks: StaffingNeedsTaskDTO[],
        private targetEmployeeId: number,
        private targetEmployeeName: string,
        private targetDate: Date,
        private skillCantBeOverridden: boolean,
        private skipWorkRules: boolean,
        private skipXEMailOnChanges: boolean) {

        super("Time.Schedule.Planning.AssignTask",
            Feature.Time_Schedule_SchedulePlanning,
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
    }

    // SETUP

    protected setupLookups() {
        super.setupLookups();

        this.$q.all([
            this.loadTargetShifts()
        ]).then(() => {
            this.stopProgress();
            this.isDirty = true; // Enable Save button

            // Check that tasks are dropped on correct date, otherwise show warning message
            _.forEach(this.tasks, task => {
                if (!task.startTime.isSameDayAs(this.targetDate))
                    this.notSameDate = true;
            });

            if (this.notSameDate) {
                this.translationService.translate("time.schedule.planning.assigntask.notsamedate").then(term => {
                    this.notSameDateInfo = StringUtility.ToBr(term);
                });
            }
        });
    }

    // LOOKUPS

    private loadTargetShifts(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftsForDay(this.targetEmployeeId, this.targetDate, [TermGroup_TimeScheduleTemplateBlockType.Schedule], false, false, null, false, false, true, true).then(x => {
            this.targetShifts = _.orderBy(x, 'actualStartTime');
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close() {
        this.$uibModalInstance.close({ success: true });
    }

    private save() {
        this.executing = true;

        this.initSave().then(save => {
            if (save) {
                this.assign();
            } else {
                this.executing = false;
            }
        });
    }

    private initSave(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.validateSkills().then(passedSkills => {
            this.showValidateSkillsResult(passedSkills).then(proceed => {
                if (proceed) {
                    // Validate work rules
                    this.validateWorkRules().then(passedWorkRules => {
                        deferral.resolve(passedWorkRules);
                    });
                } else {
                    deferral.resolve(false);
                }
            });
        });

        return deferral.promise;
    }

    private assign() {
        this.scheduleService.assignTaskToEmployee(this.targetEmployeeId, this.targetDate, this.tasks, this.skipXEMailOnChanges).then(result => {
            if (result.success) {
                // Success
                this.close();
            } else {
                // Failure
                this.translationService.translate("time.schedule.planning.assigntask.failed").then(term => {
                    this.notificationService.showDialog(term, result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                });
                this.executing = false;
            }
        });
    }

    // VALIDATION

    private validateSkills(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var nbrOfTasksChecked: number = 0;
        var nbrOfTasksToCheck: number = 0;
        this.hasInvalidSkills = false;

        nbrOfTasksToCheck = this.tasks.length;

        // Source tasks
        _.forEach(this.tasks, task => {
            this.employeeHasSkill(this.targetEmployeeId, task).then(() => {
                nbrOfTasksChecked++;
                if (nbrOfTasksToCheck === nbrOfTasksChecked)
                    deferral.resolve(!this.hasInvalidSkills);
            });
        });

        return deferral.promise;
    }

    private employeeHasSkill(employeeId: number, task: StaffingNeedsTaskDTO): ng.IPromise<any> {
        return this.scheduleService.employeeHasSkill(employeeId, task.shiftTypeId, task.startTime).then(hasSkill => {
            if (!hasSkill)
                this.hasInvalidSkills = true;
        });
    }

    private showValidateSkillsResult(passed: boolean): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (!passed) {
            var keys: string[] = [
                "common.obs",
                "time.schedule.planning.assigntask.missingskills",
                "time.schedule.planning.editshift.missingskillsoverride"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var message = terms["time.schedule.planning.assigntask.missingskills"];
                if (!this.skillCantBeOverridden)
                    message += "\n" + terms["time.schedule.planning.editshift.missingskillsoverride"];

                var modal = this.notificationService.showDialog(terms["common.obs"], message, SOEMessageBoxImage.Forbidden, this.skillCantBeOverridden ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(val && !this.skillCantBeOverridden);
                }, (reason) => {
                    deferral.resolve(false);
                });
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
            rules.push(SoeScheduleWorkRules.AttestedDay);
        }

        this.scheduleService.evaluateAssignTaskToEmployeeAgainstWorkRules(this.targetEmployeeId, this.targetDate, this.tasks, rules).then(result => {
            this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.AssignTaskToEmployee, result, this.targetEmployeeId).then(passed => {
                deferral.resolve(passed);
            });
        });

        return deferral.promise;
    }
}
