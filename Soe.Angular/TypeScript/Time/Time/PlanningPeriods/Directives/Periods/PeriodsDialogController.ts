import { TimePeriodDTO } from "../../../../../Common/Models/TimePeriodDTO";

export class PeriodsDialogController {

    private timePeriod: TimePeriodDTO;
    private isNew: boolean;

    //private startDateOptions = {
    //    dateDisabled: this.disabledStartDates,
    //    customClass: this.getStartDayClass
    //};

    //private stopDateOptions = {
    //    dateDisabled: this.disabledStopDates,
    //    customClass: this.getStopDayClass
    //};

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        timePeriod: TimePeriodDTO,
        lastDate: Date) {

        this.isNew = !timePeriod;

        this.timePeriod = new TimePeriodDTO();
        angular.merge(this.timePeriod, timePeriod);

        if (this.isNew && lastDate)
            this.timePeriod.startDate = lastDate.addDays(1).beginningOfWeek();
    }

    // HELP-METHODS

    //private disabledStartDates(data) {
    //    // Only mondays are valid
    //    return (data.mode === 'day' && data.date.getDay() !== 1);
    //}

    //private getStartDayClass(data) {
    //    // Only mondays are valid
    //    return (data.mode === 'day' && data.date.getDay() !== 1) ? 'disabledDate' : '';
    //}

    //private disabledStopDates(data) {
    //    // Only sundays are valid
    //    return (data.mode === 'day' && data.date.getDay() !== 0);
    //}

    //private getStopDayClass(data) {
    //    // Only sundays are valid
    //    return (data.mode === 'day' && data.date.getDay() !== 0) ? 'disabledDate' : '';
    //}

    private isSaveDisabled(): boolean {
        if (!this.timePeriod.name || !this.timePeriod.startDate || !this.timePeriod.stopDate)
            return true;

        if (this.timePeriod.startDate.isAfterOnDay(this.timePeriod.stopDate))
            return true;

        return false;
    }

    // EVENTS

    //private startDateChanged() {
    //    this.$timeout(() => {
    //        this.timePeriod.startDate = (this.timePeriod.startDate ? this.timePeriod.startDate : new Date()).beginningOfWeek();
    //    });
    //}

    //private stopDateChanged() {
    //    this.$timeout(() => {
    //        this.timePeriod.stopDate = (this.timePeriod.stopDate ? this.timePeriod.stopDate : new Date()).endOfWeek().date();
    //    });
    //}

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ timePeriod: this.timePeriod });
    }
}
