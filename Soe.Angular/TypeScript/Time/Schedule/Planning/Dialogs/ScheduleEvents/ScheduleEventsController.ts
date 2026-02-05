import { ITimeScheduleEventForPlanningDTO } from "../../../../../Scripts/TypeLite.Net4";

export class ScheduleEventsController {

    // Properties
    private get openingHours(): ITimeScheduleEventForPlanningDTO[] {
        return _.filter(this.scheduleEvents, e => e.openingHoursId);
    }
    private get events(): ITimeScheduleEventForPlanningDTO[] {
        return _.filter(this.scheduleEvents, e => e.timeScheduleEventId);
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private date: Date,
        private scheduleEvents: ITimeScheduleEventForPlanningDTO[]) {
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
