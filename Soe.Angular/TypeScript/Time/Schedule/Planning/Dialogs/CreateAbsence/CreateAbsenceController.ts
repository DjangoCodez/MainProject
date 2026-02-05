import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SoeScheduleWorkRules, TermGroup_TimeScheduleTemplateBlockAbsenceType, TermGroup_ShiftHistoryType } from "../../../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { IScheduleService } from "../../../ScheduleService";

export class CreateAbsenceController {
    // Flags
    private executing: boolean = false;

    selectedAbsenceType: number;
    length: number;

    //@ngInject
    constructor(private readonly $timeout: ng.ITimeoutService,
        private readonly $uibModalInstance,
        private readonly scheduleService: IScheduleService,
        private readonly sharedScheduleService: ISharedScheduleService,
        private readonly translationService: ITranslationService,
        private readonly notificationService: INotificationService,
        private readonly $q: ng.IQService,
        private readonly date: Date,
        private readonly employeeId: number,
        private readonly absenceTypes: ISmallGenericType[],
        private readonly skipWorkRules: boolean
    ) {
        this.selectedAbsenceType = TermGroup_TimeScheduleTemplateBlockAbsenceType.AnnualLeave;
        this.getAnnualLeaveShiftLength();
    }

    // LOOKUPS

    private getAnnualLeaveShiftLength() {
        this.scheduleService.getAnnualLeaveShiftLength(this.date, this.employeeId).then(minutes => {
            this.length = minutes;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private save() {
        this.executing = true;

        this.initSave().then(passed => {
            if (passed) {
                this.doSave();
            } else {
                this.executing = false;
            }
        });
    }

    private initSave(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Validate work rules
        this.validateWorkRules().then(passedWorkRules => {
            deferral.resolve(passedWorkRules);
        });

        return deferral.promise;
    }

    private doSave() {
        this.scheduleService.createAnnualLeaveShift(this.date, this.employeeId).then(result => {
            this.executing = false;
            if (result.success) {
                this.$uibModalInstance.close({ reload: true });
            } else {
                this.notificationService.showErrorDialog('', result.errorMessage, '');
            }
        });
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        let rules: SoeScheduleWorkRules[] = [];

        // The following rules should always be evaluated
        rules.push(SoeScheduleWorkRules.AnnualLeave);

        if (this.skipWorkRules) {
            rules = [];
            rules.push(SoeScheduleWorkRules.None);
        }

        let shift = new ShiftDTO();
        shift.startTime = this.date; // The time part will be set on server side
        shift.stopTime = this.date; // The time part will be set on server side
        shift.employeeId = this.employeeId;
        shift.absenceType = this.selectedAbsenceType;

        this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules([shift], rules, this.employeeId, false, null).then(result => {
            this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.Unknown, result, this.employeeId).then(passed => {
                deferral.resolve(passed);
            });
        });

        return deferral.promise;
    }
}