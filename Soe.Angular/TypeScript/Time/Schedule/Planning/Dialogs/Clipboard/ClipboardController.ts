import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";

export class ClipboardController {

    // Properties

    private get allItemsSelected(): boolean {
        var selected = true;
        _.forEach(this.shifts, shift => {
            if (!shift['selectedForPaste']) {
                selected = false;
                return false;
            }
        });

        return selected;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private employeeName: string,
        private shifts: ShiftDTO[],
        private isCut: boolean) {
    }

    // EVENTS

    private selectAllItems() {
        var selected: boolean = this.allItemsSelected;
        _.forEach(this.shifts, shift => {
            shift['selectedForPaste'] = !selected;
        });
    }

    private deleteItem(shift) {
        _.pull(this.shifts, shift);
    }

    private deleteAllItems() {
        // Can't just clear the collection.
        // Then it will not reflect back to parent controller.
        while (this.shifts.length > 0) {
            _.forEach(this.shifts, shift => {
                _.pull(this.shifts, shift);
            });
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
