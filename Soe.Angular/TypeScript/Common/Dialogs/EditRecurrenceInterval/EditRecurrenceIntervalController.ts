import { ICoreService } from "../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CronExpressionPart, CronExpressionType, CronIntervalItem, SchedulerUtility } from "../../../Util/SchedulerUtility";
import { SmallGenericType } from "../../Models/SmallGenericType";

export class EditRecurrenceIntervalController {

    private monthTerms: SmallGenericType[] = [];
    private weekdayTerms: SmallGenericType[] = [];

    private minutes: CronIntervalItem[][] = [];
    private hours: CronIntervalItem[][] = [];
    private days: CronIntervalItem[][] = [];
    private weekdays: CronIntervalItem[][] = [];
    private months: CronIntervalItem[][] = [];

    private selectedMinutes: number[] = [];
    private selectedHours: number[] = [];
    private selectedDays: number[] = [];
    private selectedWeekdays: number[] = [];
    private selectedMonths: number[] = [];

    private parts: CronExpressionPart[];
    private intervalText: string;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private singleSelectTime: boolean,
        private interval: string) {

        this.setup();
    }

    // SETUP

    private setup() {
        this.monthTerms = _.range(12).map(m => new SmallGenericType(m + 1, CalendarUtility.getMonthName(m).toUpperCaseFirstLetter()));
        _.range(1, 8).forEach(w => this.weekdayTerms.push(new SmallGenericType(w, CalendarUtility.getDayName(w > 6 ? 0 : w).toUpperCaseFirstLetter())));

        this.parseInterval();
        this.setupIntervals();
    }

    private setupIntervals() {
        this.minutes = this.setupInterval(SchedulerUtility.CRONTAB_MINUTES_LOWER, SchedulerUtility.CRONTAB_MINUTES_UPPER, 10, null, this.selectedMinutes);
        this.hours = this.setupInterval(SchedulerUtility.CRONTAB_HOURS_LOWER, SchedulerUtility.CRONTAB_HOURS_UPPER, 12, null, this.selectedHours);
        this.days = this.setupInterval(SchedulerUtility.CRONTAB_DAYS_LOWER, SchedulerUtility.CRONTAB_DAYS_UPPER, 7, null, this.selectedDays);
        this.weekdays = this.setupInterval(SchedulerUtility.CRONTAB_WEEKDAYS_LOWER, SchedulerUtility.CRONTAB_WEEKDAYS_UPPER, 7, this.weekdayTerms, this.selectedWeekdays);
        this.months = this.setupInterval(SchedulerUtility.CRONTAB_MONTHS_LOWER, SchedulerUtility.CRONTAB_MONTHS_UPPER, 6, this.monthTerms, this.selectedMonths);
    }

    private setupInterval(lower: number, upper: number, itemsPerRow: number, names: ISmallGenericType[], selectedItems: number[]): CronIntervalItem[][] {
        let collection: CronIntervalItem[][] = [];
        let i: number = lower;
        let rows: CronIntervalItem[] = [];
        while (i <= upper) {
            let name: string = names ? _.find(names, { 'id': i }).name : i.toString();
            rows.push(new CronIntervalItem(i, name, _.includes(selectedItems, i)));
            if ((i + 1 - lower) % itemsPerRow === 0) {
                collection.push(rows);
                rows = [];
            }
            i++;
        }
        if (rows.length > 0)
            collection.push(rows);

        return collection;
    }

    // SERVICE CALLS

    private getIntervalText(): ng.IPromise<any> {
        return this.coreService.getRecurrenceIntervalText(this.interval).then(x => {
            this.intervalText = x;
        });
    }

    // EVENTS

    private minuteSelected(item: CronIntervalItem) {
        if (this.singleSelectTime) {
            if (item.selected)
                return;

            this.clearCollection(this.minutes);
        }

        item.selected = !item.selected;
        this.selectedMinutes = this.setSelected(this.minutes);
        this.setInterval();
    }

    private hourSelected(item: CronIntervalItem) {
        if (this.singleSelectTime) {
            if (item.selected)
                return;

            this.clearCollection(this.hours);
        }

        item.selected = !item.selected;
        this.selectedHours = this.setSelected(this.hours);
        this.setInterval();
    }

    private daySelected(item: CronIntervalItem) {
        item.selected = !item.selected;
        this.selectedDays = this.setSelected(this.days);
        this.setInterval();
    }

    private weekdaySelected(item: CronIntervalItem) {
        item.selected = !item.selected;
        this.selectedWeekdays = this.setSelected(this.weekdays);
        this.setInterval();
    }

    private monthSelected(item: CronIntervalItem) {
        item.selected = !item.selected;
        this.selectedMonths = this.setSelected(this.months);
        this.setInterval();
    }

    private invertMinutes() {
        if (!this.singleSelectTime) {
            this.invertCollection(this.minutes);
            this.selectedMinutes = this.setSelected(this.minutes);
            this.setInterval();
        }
    }

    private invertHours() {
        if (!this.singleSelectTime) {
            this.invertCollection(this.hours);
            this.selectedHours = this.setSelected(this.hours);
            this.setInterval();
        }
    }

    private invertDays() {
        this.invertCollection(this.days);
        this.selectedDays = this.setSelected(this.days);
        this.setInterval();
    }

    private invertWeekdays() {
        this.invertCollection(this.weekdays);
        this.selectedWeekdays = this.setSelected(this.weekdays);
        this.setInterval();
    }

    private invertMonths() {
        this.invertCollection(this.months);
        this.selectedMonths = this.setSelected(this.months);
        this.setInterval();
    }

    private intervalChanged() {
        this.$timeout(() => {
            this.parseInterval();
            this.setupIntervals();
        });
    }

    private cancel() {
        this.$uibModalInstance.close(null);
    }

    private ok() {
        this.$uibModalInstance.close({ interval: this.interval, intervalText: this.intervalText });
    }

    // HELP-METHODS

    private parseInterval() {
        this.parts = SchedulerUtility.parseCrontabExpression(this.interval);
        this.selectedMinutes = SchedulerUtility.getCrontabPartValue(this.parts, CronExpressionType.Minute);
        this.selectedHours = SchedulerUtility.getCrontabPartValue(this.parts, CronExpressionType.Hour);
        this.selectedDays = SchedulerUtility.getCrontabPartValue(this.parts, CronExpressionType.Day);
        this.selectedWeekdays = SchedulerUtility.getCrontabPartValue(this.parts, CronExpressionType.Weekday);
        this.selectedMonths = SchedulerUtility.getCrontabPartValue(this.parts, CronExpressionType.Month);

        if (this.singleSelectTime) {
            // If no minute selected, select 0
            // If more than one minute is selected, only select the first one
            if (this.selectedMinutes.length === 0)
                this.selectedMinutes.push(0);
            else if (this.selectedMinutes.length > 1)
                this.selectedMinutes = [this.selectedMinutes[0]];

            // If no hour selected, select 0
            // If more than one hour is selected, only select the first one
            if (this.selectedHours.length === 0)
                this.selectedHours.push(0);
            else if (this.selectedHours.length > 1)
                this.selectedHours = [this.selectedHours[0]];
        }

        this.setInterval();
    }

    private setInterval() {
        this.interval = SchedulerUtility.getCrontabExpression(this.selectedMinutes, this.selectedHours, this.selectedDays, this.selectedMonths, this.selectedWeekdays);
        this.getIntervalText();
    }

    private setSelected(collection: CronIntervalItem[][]): number[] {
        let selected: number[] = [];
        _.forEach(collection, row => {
            _.forEach(row, item => {
                if (item.selected)
                    selected.push(item.id);
            });
        });

        return selected;
    }

    private clearCollection(collection: CronIntervalItem[][]) {
        _.forEach(collection, row => {
            _.forEach(row, item => {
                item.selected = false;
            });
        });
    }

    private invertCollection(collection: CronIntervalItem[][]) {
        _.forEach(collection, row => {
            _.forEach(row, item => {
                item.selected = !item.selected;
            });
        });
    }
}

