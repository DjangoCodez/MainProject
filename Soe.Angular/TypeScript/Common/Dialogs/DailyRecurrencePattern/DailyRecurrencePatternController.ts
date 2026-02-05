import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SmallGenericType } from "../../Models/smallgenerictype";
import { DailyRecurrencePatternDTO, DailyRecurrenceRangeDTO } from "../../Models/DailyRecurrencePatternDTOs";
import { DayOfWeek } from "../../../Util/Enumerations";
import { DailyRecurrenceRangeType, TermGroup, DailyRecurrencePatternType } from "../../../Util/CommonEnumerations";

export class DailyRecurrencePatternController {

    // Terms
    private terms: any;
    private suggestionInfo: string;

    // Lookups
    private types: SmallGenericType[] = [];
    private sysHolidayTypes: any[] = [];
    private weekIndexes: SmallGenericType[] = [];
    private dayOfWeeks: SmallGenericType[] = [];
    private months: SmallGenericType[] = [];
    private rangeTypes: SmallGenericType[] = [];

    // Properties
    private _mondaySelected: boolean = false;
    private get mondaySelected(): boolean {
        return this._mondaySelected;
    }
    private set mondaySelected(value: boolean) {
        this._mondaySelected = value;
        this.setDaysOfWeek();
    }

    private _tuesdaySelected: boolean = false;
    private get tuesdaySelected(): boolean {
        return this._tuesdaySelected;
    }
    private set tuesdaySelected(value: boolean) {
        this._tuesdaySelected = value;
        this.setDaysOfWeek();
    }

    private _wednesdaySelected: boolean = false;
    private get wednesdaySelected(): boolean {
        return this._wednesdaySelected;
    }
    private set wednesdaySelected(value: boolean) {
        this._wednesdaySelected = value;
        this.setDaysOfWeek();
    }

    private _thursdaySelected: boolean = false;
    private get thursdaySelected(): boolean {
        return this._thursdaySelected;
    }
    private set thursdaySelected(value: boolean) {
        this._thursdaySelected = value;
        this.setDaysOfWeek();
    }

    private _fridaySelected: boolean = false;
    private get fridaySelected(): boolean {
        return this._fridaySelected;
    }
    private set fridaySelected(value: boolean) {
        this._fridaySelected = value;
        this.setDaysOfWeek();
    }

    private _saturdaySelected: boolean = false;
    private get saturdaySelected(): boolean {
        return this._saturdaySelected;
    }
    private set saturdaySelected(value: boolean) {
        this._saturdaySelected = value;
        this.setDaysOfWeek();
    }

    private _sundaySelected: boolean = false;
    private get sundaySelected(): boolean {
        return this._sundaySelected;
    }
    private set sundaySelected(value: boolean) {
        this._sundaySelected = value;
        this.setDaysOfWeek();
    }

    private get allSysHolidayTypesSelected(): boolean {
        var selected = true;
        _.forEach(this.sysHolidayTypes, item => {
            if (!item.selected) {
                selected = false;
                return false;
            }
        });

        return selected;
    }

    private isNew: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private pattern: DailyRecurrencePatternDTO,
        private range: DailyRecurrenceRangeDTO,
        private excludedDates: Date[],
        private date: Date,
        private hideRange: boolean) {

        this.$q.all([
            this.loadTerms(),
            this.loadTypes(),
            this.loadSysHolidayTypes(),
            this.loadWeekIndexes(),
            this.loadRangeTypes()]).then(() => {
                this.setupDayOfWeeks();
                this.setupMonths();

                if (!pattern) {
                    this.isNew = true;
                    this.pattern = new DailyRecurrencePatternDTO();
                    this.typeChanged();
                }
                else {
                    this.setSelectedDaysOfWeek();
                    this.setSelectedSysHolidayTypes();
                }
                if (!range) {
                    this.range = new DailyRecurrenceRangeDTO();
                    this.range.startDate = date;
                    this.range.type = DailyRecurrenceRangeType.NoEnd;
                    this.rangeTypeChanged();
                }
            });
    }

    // LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.dailyrecurrencepattern.suggestinfo",
            "common.dailyrecurrencepattern.type",
            "common.dailyrecurrencepattern.interval",
            "common.dailyrecurrencepattern.dayofmonth",
            "common.dailyrecurrencepattern.month",
            "common.dailyrecurrencepattern.daysofweek",
            "common.dailyrecurrencepattern.weekindex",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.suggestionInfo = this.terms["common.dailyrecurrencepattern.suggestinfo"].format(this.date.toLocaleDateString());
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.DailyRecurrencePatternType, false, false).then(x => {
            this.types = x;
        });
    }

    private loadSysHolidayTypes(): ng.IPromise<any> {
        this.sysHolidayTypes = [];
        return this.coreService.getSysHolidayTypes().then(x => {
            _.forEach(x, y => {
                this.sysHolidayTypes.push({ id: y.sysHolidayTypeId, name: y.name })
            });
        });
    }

    private loadWeekIndexes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.DailyRecurrencePatternWeekIndex, false, false).then(x => {
            this.weekIndexes = [];
            _.forEach(x, y => {
                this.weekIndexes.push({ id: y.id, name: y.name.toLocaleLowerCase() });
            });
        });
    }

    private loadRangeTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.DailyRecurrenceRangeType, false, false).then(x => {
            // Different sort
            this.rangeTypes.push(x[1]);
            this.rangeTypes.push(x[0]);
            this.rangeTypes.push(x[2]);
        });
    }

    private setupDayOfWeeks() {
        this.dayOfWeeks = CalendarUtility.getDayOfWeekNames(true);
    }

    private setupMonths() {
        for (var i = 1; i <= 12; i++) {
            this.months.push({ id: i, name: this.getMonthName(i) });
        }
    }

    protected isPatternTypeNone(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.None;
    }

    protected isPatternTypeDaily(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.Daily;
    }

    protected isPatternTypeWeekly(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.Weekly;
    }

    protected isPatternTypeAbsoluteMonthly(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.AbsoluteMonthly;
    }

    protected isPatternTypeRelativeMonthly(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.RelativeMonthly;
    }

    protected isPatternTypeAbsoluteYearly(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.AbsoluteYearly;
    }

    protected isPatternTypeRelativeYearly(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.RelativeYearly;
    }

    protected isPatternTypeSysHoliday(): boolean {
        return this.pattern && this.pattern.type === DailyRecurrencePatternType.SysHoliday;
    }

    // HELP-METHODS

    private getDayName(day: number): string {
        return CalendarUtility.getDayName(day).toUpperCaseFirstLetter();
    }

    private getMonthName(month: number): string {
        return CalendarUtility.getMonthName(month - 1);
    }

    private setDaysOfWeek() {
        if (!this.pattern)
            return;

        this.pattern.daysOfWeek = [];
        if (this.mondaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Monday);
        if (this.tuesdaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Tuesday);
        if (this.wednesdaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Wednesday);
        if (this.thursdaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Thursday);
        if (this.fridaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Friday);
        if (this.saturdaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Saturday);
        if (this.sundaySelected)
            this.pattern.daysOfWeek.push(DayOfWeek.Sunday);
    }

    private setSelectedDaysOfWeek() {
        if (!this.pattern)
            return;

        if (!this.pattern.daysOfWeek)
            this.pattern.daysOfWeek = [];

        _.forEach(this.pattern.daysOfWeek, day => {
            if (day === DayOfWeek.Monday)
                this.mondaySelected = true;
            if (day === DayOfWeek.Tuesday)
                this.tuesdaySelected = true;
            if (day === DayOfWeek.Wednesday)
                this.wednesdaySelected = true;
            if (day === DayOfWeek.Thursday)
                this.thursdaySelected = true;
            if (day === DayOfWeek.Friday)
                this.fridaySelected = true;
            if (day === DayOfWeek.Saturday)
                this.saturdaySelected = true;
            if (day === DayOfWeek.Sunday)
                this.sundaySelected = true;
        });
    }

    private setSelectedSysHolidayTypes() {
        if (!this.pattern)
            return;

        if (!this.pattern.sysHolidayTypeIds)
            this.pattern.sysHolidayTypeIds = [];

        _.forEach(this.pattern.sysHolidayTypeIds, sysHolidayTypeId => {
            var type = _.find(this.sysHolidayTypes, t => t.id === sysHolidayTypeId);
            if (type)
                type.selected = true;
        });
    }

    private clearPattern() {
        this.pattern.interval = 0;
        this.pattern.dayOfMonth = 0;
        this.pattern.month = 0;
        this.pattern.daysOfWeek = [];
        this.pattern.firstDayOfWeek = undefined;
        this.pattern.weekIndex = 0;
        this.pattern.sysHolidayTypeIds = [];
    }

    private setDefaultInterval() {
        this.pattern.interval = 1;
    }

    private setDefaultDayOfMonth() {
        this.pattern.dayOfMonth = this.date.getDate();
    }

    private setDefaultMonth() {
        this.pattern.month = this.date.getMonth() + 1;
    }

    private setDefaultDaysOfWeek() {
        if (this.isNew)
            return;

        var dayOfWeek: DayOfWeek = this.date.dayOfWeek();
        this.mondaySelected = (dayOfWeek === DayOfWeek.Monday);
        this.tuesdaySelected = (dayOfWeek === DayOfWeek.Tuesday);
        this.wednesdaySelected = (dayOfWeek === DayOfWeek.Wednesday);
        this.thursdaySelected = (dayOfWeek === DayOfWeek.Thursday);
        this.fridaySelected = (dayOfWeek === DayOfWeek.Friday);
        this.saturdaySelected = (dayOfWeek === DayOfWeek.Saturday);
        this.sundaySelected = (dayOfWeek === DayOfWeek.Sunday);
    }

    private setDefaultFirstDayOfWeek() {
        this.pattern.firstDayOfWeek = this.date.dayOfWeek();
    }

    private setDefaultWeekIndex() {
        var dayOfMonth = this.date.getDate();
        var week = Math.floor(dayOfMonth / 7);
        this.pattern.weekIndex = week;
    }

    private getSelectedSysHolidayTypeIds(): any[] {
        return _.map(_.filter(this.sysHolidayTypes, t => t.selected), t => t.id);
    }

    // EVENTS

    private typeChanged() {
        // Set suggestions based on specified date
        this.$timeout(() => {
            this.clearPattern();
            switch (this.pattern.type) {
                case DailyRecurrencePatternType.None:
                    break;
                case DailyRecurrencePatternType.Daily:
                    this.setDefaultInterval();
                    break;
                case DailyRecurrencePatternType.Weekly:
                case DailyRecurrencePatternType.AbsoluteMonthly:
                    this.setDefaultInterval();
                    this.setDefaultDayOfMonth();
                    break;
                case DailyRecurrencePatternType.RelativeMonthly:
                    this.setDefaultInterval();
                    this.setDefaultWeekIndex();
                    this.setDefaultFirstDayOfWeek();
                    break;
                case DailyRecurrencePatternType.AbsoluteYearly:
                    this.setDefaultDayOfMonth();
                    this.setDefaultMonth();
                    break;
                case DailyRecurrencePatternType.RelativeYearly:
                    this.setDefaultWeekIndex();
                    this.setDefaultFirstDayOfWeek();
                    this.setDefaultMonth();
                    break;
                case DailyRecurrencePatternType.SysHoliday:
                    this.setDefaultInterval();
                    break;
            }
        });
    }

    private rangeTypeChanged() {
        this.$timeout(() => {
            switch (this.range.type) {
                case DailyRecurrenceRangeType.NoEnd:
                    this.range.endDate = null;
                    this.range.numberOfOccurrences = 0;
                    break;
                case DailyRecurrenceRangeType.EndDate:
                    this.range.endDate = this.date;
                    this.range.numberOfOccurrences = 0;
                    break;
                case DailyRecurrenceRangeType.Numbered:
                    this.range.endDate = null;
                    this.range.numberOfOccurrences = 1;
                    break;
            }
        });
    }

    private monthChanged() {
        this.$timeout(() => {
            if (this.pattern.type === DailyRecurrencePatternType.AbsoluteYearly) {
                //Max day depending on delected month
                var maxDay = new Date(this.date.year(), this.pattern.month, 0).getDate();
                if (this.pattern.dayOfMonth > maxDay)
                    this.pattern.dayOfMonth = maxDay;
            }
        });
    }

    private selectAllSysHolidayTypes() {
        var selected: boolean = this.allSysHolidayTypesSelected;
        _.forEach(this.sysHolidayTypes, type => {
            type.selected = !selected;
        });
    }

    private numberChanged(id: string) {
        this.$timeout(() => {
            if (id === 'interval') {
                if (this.pattern.interval < 1)
                    this.pattern.interval = 1;
            }
            if (id === 'dayOfMonth') {
                if (this.pattern.dayOfMonth < 1)
                    this.pattern.dayOfMonth = 1;
                if (this.pattern.type == DailyRecurrencePatternType.AbsoluteYearly) {
                    //Max day depending on delected month
                    var maxDay = new Date(this.date.year(), this.pattern.month, 0).getDate();
                    if (this.pattern.dayOfMonth > maxDay)
                        this.pattern.dayOfMonth = maxDay;
                }
                else {
                    if (this.pattern.dayOfMonth > 31)
                        this.pattern.dayOfMonth = 31;
                }
            }
            if (id === 'numberOfOccurrences') {
                if (this.range.numberOfOccurrences < 1)
                    this.range.numberOfOccurrences = 1;
            }
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.pattern.sysHolidayTypeIds = (this.pattern.type === DailyRecurrencePatternType.SysHoliday) ? this.getSelectedSysHolidayTypeIds() : [];
        this.$uibModalInstance.close({ pattern: this.pattern, range: this.range, excludedDates: this.excludedDates });
    }
}