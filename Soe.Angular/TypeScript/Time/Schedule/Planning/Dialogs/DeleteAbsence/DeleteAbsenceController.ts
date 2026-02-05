import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";

export class DeleteAbsenceController {

    // Terms
    private shiftDate: string;
    private shiftEmployee: string;

    // Flags
    private executing = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private readonly scheduleService: IScheduleService,
        private readonly notificationService: INotificationService,
        private shift: ShiftDTO) {

        this.shiftDate = this.shift.actualStartDate.toFormattedDate();
        this.shiftEmployee = this.shift.employeeName;
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.executing = true;

        this.scheduleService.deleteAnnualLeaveShift(this.shift.timeScheduleTemplateBlockId).then(result => {
            this.executing = false;
            if (result.success) {
                this.$uibModalInstance.close({ reload: true });
            } else {
                this.notificationService.showErrorDialog('', result.errorMessage, '');
            }
        });
    }
}
