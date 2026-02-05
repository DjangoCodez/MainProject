import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IScheduleService } from "../../../ScheduleService";
import { StaffingNeedsTaskDTO } from "../../../../../Common/Models/StaffingNeedsDTOs";
import { DailyRecurrenceRangeDTO } from "../../../../../Common/Models/DailyRecurrencePatternDTOs";

export class DeleteDeliveryController {

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        private deliveries: StaffingNeedsTaskDTO[]) {

        DailyRecurrenceRangeDTO.setRecurrenceInfo(this.deliveries[0], this.translationService);
        this.scheduleService.getRecurrenceDescription(this.deliveries[0].recurrencePattern).then((x) => {
            this.deliveries[0]["patternDescription"] = x;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.$uibModalInstance.close({ success: true });
    }
}

