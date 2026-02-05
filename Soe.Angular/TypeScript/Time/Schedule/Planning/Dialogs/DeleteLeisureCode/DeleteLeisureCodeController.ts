import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";

export class DeleteLeisureCodeController {

    // Terms
    private shiftDate: string;
    private shiftEmployee: string;

    // Properties

    private get multipleDates(): boolean {
        return this.shifts.length > 0 && !this.firstShift.date.isSameDayAs(this.lastShift.date);
    }

    private get firstShift(): ShiftDTO {
        return _.head(_.orderBy(this.shifts, 'actualStartTime'));
    }

    private get lastShift(): ShiftDTO {
        return _.head(_.orderBy(this.shifts, 'actualStopTime', 'desc'));
    }

    private get nbrOfSelectedShifts(): number {
        return _.filter(this.shifts, s => s.selected).length;
    }

    // Flags
    private executing = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $timeout: ng.ITimeoutService,
        private shifts: ShiftDTO[]) {

        if (this.shifts.length > 0) {
            _.forEach(this.shifts, shift => {
                shift.selected = true;
            });
            this.shiftDate = this.multipleDates ? "{0} - {1}".format(this.firstShift.actualStartDate.toFormattedDate(), this.lastShift.actualStopTime.toFormattedDate()) : this.firstShift.actualStartDate.toFormattedDate();
            this.shiftEmployee = this.firstShift.employeeName;
        } else {
            // No shifts, close dialog
            $timeout(() => {
                this.cancel();
            }, 2000);
        }
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.$uibModalInstance.close({ selectedShifts: _.filter(this.shifts, s => s.selected) });
    }
}
