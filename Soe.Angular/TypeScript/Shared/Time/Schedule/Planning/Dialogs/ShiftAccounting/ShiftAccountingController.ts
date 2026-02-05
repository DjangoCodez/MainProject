import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { IShiftAccountingDTO } from "../../../../../../Scripts/TypeLite.Net4";

export class ShiftAccountingController {

    private accounting: IShiftAccountingDTO;

    //@ngInject
    constructor(private $uibModalInstance,
        private sharedScheduleService: ISharedScheduleService,
        private shifts: ShiftDTO[],
        private selectedShift: ShiftDTO) {

        // Set description to be used in the navigator
        _.forEach(this.shifts, shift => {
            shift['accountingDescription'] = "{0}-{1} {2}".format(shift.actualStartTime.toFormattedTime(), shift.actualStopTime.toFormattedTime(), shift.shiftTypeName);
        });

        if (this.shifts.length === 1)
            this.setSelectedShift(this.shifts[0]);
    }

    private setSelectedShift(shift) {
        this.selectedShift = shift;
        this.loadAccounting();
    }

    private loadAccounting() {
        this.sharedScheduleService.getShiftAccounting(this.selectedShift.timeScheduleTemplateBlockId).then(x => {
            this.accounting = x;
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}