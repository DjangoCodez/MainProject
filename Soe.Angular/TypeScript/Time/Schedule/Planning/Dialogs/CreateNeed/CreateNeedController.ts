import { IScheduleService } from "../../../ScheduleService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class CreateNeedController {

    // Properties
    private selectedTimeScheduleTaskId: number;
    private selectedFromDate: Date;
    private selectedWeekday: any;
    private selectedDate: Date;
    private wholeWeek: boolean = false;

    private intervalDateFrom: Date = undefined;
    private intervalDateTo: Date = undefined;
    private dayOfWeeks: any[] = [];
    private adjustPercent: number = 0;

    private fromDateOptions = {
        dateDisabled: this.disabledFromDates,
        customClass: this.getDayClass
    };

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private scheduleService: IScheduleService,
        translationService: ITranslationService,
        private weekdays: any[],
        private date: Date,
        private frequencyTasks: ISmallGenericType[]) {

        if (this.frequencyTasks.length > 0)
            this.selectedTimeScheduleTaskId = this.frequencyTasks[0].id;

        // Set weekday from date
        this.selectedWeekday = date.dayOfWeek();
        this.selectedDate = undefined;
        this.selectedFromDate = date;
    }

    // Events

    private weekdayChanged() {
        this.selectedDate = undefined;
    }

    private dateChanged() {
        this.selectedWeekday = undefined;
    }

    private disabledFromDates(data) {
        return (data.mode === 'day' && data.date.getDay() !== 1);
    }

    private getDayClass(data) {
        return (data.mode === 'day' && data.date.getDay() !== 1) ? 'disabledDate' : '';
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.$uibModalInstance.close({
            timeScheduleTaskId: this.selectedTimeScheduleTaskId,
            fromDate: this.selectedFromDate,
            weekday: this.selectedWeekday,
            date: this.selectedDate,
            wholeWeek: this.wholeWeek,
            intervalDateFrom: this.intervalDateFrom,
            intervalDateTo: this.intervalDateTo,
            dayOfWeeks: _.map(this.dayOfWeeks, d => d.id),
            adjustPercent: this.adjustPercent,
        });
    }
}
