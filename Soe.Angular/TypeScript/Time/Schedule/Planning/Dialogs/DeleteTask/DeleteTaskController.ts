import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IScheduleService } from "../../../ScheduleService";
import { StaffingNeedsTaskDTO } from "../../../../../Common/Models/StaffingNeedsDTOs";
import { DailyRecurrenceRangeDTO } from "../../../../../Common/Models/DailyRecurrencePatternDTOs";

export class DeleteTaskController {

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        private task: StaffingNeedsTaskDTO) {

        DailyRecurrenceRangeDTO.setRecurrenceInfo(this.task, this.translationService);
        this.scheduleService.getRecurrenceDescription(this.task.recurrencePattern).then((x) => {
            this.task["patternDescription"] = x;
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

