import { EmployeeAvailabilitySortOrder } from "../../Util/Enumerations";
import { IEmployeeListEmploymentDTO, IEmployeeListDTO, IEmployeeListSmallDTO, IEmployeeSmallDTO, IEmployeeListAvailabilityDTO } from "../../Scripts/TypeLite.Net4";
import { DateRangeDTO } from "./DateRangeDTO";
import { TimeScheduleTemplateHeadSmallDTO } from "./TimeScheduleTemplateDTOs";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEmployeePostStatus, TermGroup_Sex, TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat } from "../../Util/CommonEnumerations";
import { EmployeeAccountDTO, EmployeeSkillDTO } from "./EmployeeUserDTO";
import { CompanyCategoryRecordDTO } from "./Category";
import { TimeScheduleScenarioEmployeeDTO } from "./TimeSchedulePlanningDTOs";

export class EmployeeListDTO implements IEmployeeListDTO {
    absenceApproved: DateRangeDTO[];
    absenceRequest: DateRangeDTO[];
    accounts: EmployeeAccountDTO[];
    active: boolean;
    annualScheduledTimeMinutes: number;
    annualWorkTimeMinutes: number;
    available: EmployeeListAvailabilityDTO[];
    categoryRecords: CompanyCategoryRecordDTO[];
    childPeriodBalanceTimeMinutes: number;
    childRuleWorkedTimeMinutes: number;
    currentDate: Date;
    description: string;
    employeeId: number;
    employeeNr: string;
    employeeNrSort: string;
    employeePostId: number;
    employeePostStatus: SoeEmployeePostStatus;
    employeeSchedules: DateRangeDTO[];
    employeeSkills: EmployeeSkillDTO[];
    employments: EmployeeListEmploymentDTO[];
    firstName: string;
    groupName: string;
    hasAbsenceApproved: boolean;
    hasAbsenceRequest: boolean;
    hibernatingText: string;
    hidden: boolean;
    image: number[];
    imageSource: string;
    isGroupHeader: boolean;
    isSelected: boolean;
    lastName: string;
    name: string;
    parentPeriodBalanceTimeMinutes: number;
    parentRuleWorkedTimeMinutes: number;
    parentScheduledTimeMinutes: number;
    parentWorkedTimeMinutes: number;
    sex: TermGroup_Sex;
    templateSchedules: TimeScheduleTemplateHeadSmallDTO[];
    unavailable: EmployeeListAvailabilityDTO[];
    vacant: boolean;

    // Extensions
    isVisible: boolean;
    isModified: boolean;
    selected: boolean;
    toolTip: string;
    plannedMinutes = 0;
    cyclePlannedMinutes = 0;
    cyclePlannedAverageMinutes = 0;
    annualLeaveBalanceDays = 0;
    annualLeaveBalanceMinutes = 0;
    workTimeMinutes = 0;
    oneWeekWorkTimeMinutes = 0;
    minScheduleTime = 0;
    maxScheduleTime = 0;
    timeScheduleTypeFactorMinutes = 0;
    grossMinutes = 0;
    totalCost = 0;
    totalCostIncEmpTaxAndSuppCharge = 0;
    hasAbsence = false;
    hasTimeScheduleTypeIsNotScheduleTime = false;

    public get identifier(): number {
        if (this['isAccount'])
            return parseInt(this['accountId'], 10);
        else if (this['isCategory'])
            return parseInt(this['categoryId'], 10);
        else if (this['isShiftType'])
            return parseInt(this['shiftTypeId'], 10);
        else
            return this.employeePostId ? this.employeePostId : this.employeeId;
    }

    public get numberAndName(): string {
        return this.employeeNr && this.employeeNr !== '0' ? "({0}) {1}".format(this.employeeNr, this.name) : this.name;
    }

    public fixDates() {
        if (this.employeeSchedules) {
            this.employeeSchedules.forEach(s => {
                s.start = CalendarUtility.convertToDate(s.start);
                s.stop = CalendarUtility.convertToDate(s.stop);
            });
        }

        if (this.templateSchedules) {
            this.templateSchedules.forEach(t => {
                t.startDate = CalendarUtility.convertToDate(t.startDate);
                t.stopDate = CalendarUtility.convertToDate(t.stopDate);
                t.firstMondayOfCycle = CalendarUtility.convertToDate(t.firstMondayOfCycle);
            });
        }
    }

    public setTypes() {
        this.employeeNrSort = _.padStart(this.employeeNr, 50, '0');

        if (this.employments) {
            this.employments = this.employments.map(e => {
                let eObj = new EmployeeListEmploymentDTO();
                angular.extend(eObj, e);
                eObj.fixDates();
                return eObj;
            });
        }

        if (this.accounts) {
            this.accounts = this.accounts.map(a => {
                let aObj = new EmployeeAccountDTO();
                angular.extend(aObj, a);
                aObj.fixDates();

                if (a.children) {
                    aObj.children = aObj.children.map(c => {
                        let cObj = new EmployeeAccountDTO();
                        angular.extend(cObj, c);
                        cObj.fixDates();

                        if (c.children) {
                            cObj.children = cObj.children.map(sc => {
                                let scObj = new EmployeeAccountDTO();
                                angular.extend(scObj, sc);
                                scObj.fixDates();
                                return scObj;
                            });
                        }

                        return cObj;
                    });
                }

                return aObj;
            });
        }

        if (this.categoryRecords) {
            this.categoryRecords = this.categoryRecords.map(c => {
                let cObj = new CompanyCategoryRecordDTO();
                angular.extend(cObj, c);
                cObj.fixDates();
                return cObj;
            });
        }

        if (this.available) {
            this.available = this.available.map(a => {
                let aObj = new EmployeeListAvailabilityDTO;
                angular.extend(aObj, a);
                aObj.fixDates();
                return aObj;
            });
        }

        if (this.unavailable) {
            this.unavailable = this.unavailable.map(u => {
                let uObj = new EmployeeListAvailabilityDTO;
                angular.extend(uObj, u);
                uObj.fixDates();
                return uObj;
            });
        }

        if (this.templateSchedules) {
            this.templateSchedules = this.templateSchedules.map(t => {
                let tObj = new TimeScheduleTemplateHeadSmallDTO();
                angular.extend(tObj, t);
                tObj.fixDates();
                return tObj;
            });
        }

        if (this.employeeSkills) {
            this.employeeSkills = this.employeeSkills.map(t => {
                let tObj = new EmployeeSkillDTO();
                angular.extend(tObj, t);
                tObj.fixDates();
                return tObj;
            });
        }
    }

    public getEmployment(dateFrom: Date, dateTo: Date): EmployeeListEmploymentDTO {
        let result: EmployeeListEmploymentDTO;

        let dateFromDay = dateFrom.beginningOfDay();
        let dateToDay = dateTo.beginningOfDay();

        while (dateFromDay <= dateToDay) {
            result = EmployeeListEmploymentDTO.getEmployment(this.employments, dateFromDay);
            if (result)
                break;

            dateFromDay = dateFromDay.addDays(1);
        }

        return result;
    }

    public hasEmployment(dateFrom: Date, dateTo: Date, wholePeriod = false): boolean {
        let result = true;

        if (wholePeriod) {
            while (dateFrom <= dateTo) {
                if (!this.getEmployment(dateFrom, dateTo)) {
                    result = false;
                    break;
                }

                dateFrom = dateFrom.addDays(1);
            }
        } else {
            if (!this.getEmployment(dateFrom, dateTo))
                result = false;
        }

        return result;
    }

    public get hasHibernatingEmployment(): boolean {
        return !!this.hibernatingText;
    }

    public getBreakDayMinutesAfterMidnight(dateFrom: Date, dateTo: Date): number {
        let employment = this.getEmployment(dateFrom, dateTo);
        return employment?.breakDayMinutesAfterMidnight || 0;
    }

    public get hasEmployeeSchedules(): boolean {
        return this.employeeSchedules && this.employeeSchedules.length > 0;
    }

    public hasEmployeeSchedule(date: Date): boolean {
        return !!this.getEmployeeSchedule(date);
    }

    public getEmployeeSchedule(date: Date): DateRangeDTO {
        let employeeSchedule: DateRangeDTO;

        if (this.hasEmployeeSchedules) {
            _.forEach(this.employeeSchedules, schedule => {
                if (!schedule.start || schedule.start.isSameOrBeforeOnDay(date) && (!schedule.stop || schedule.stop.isSameOrAfterOnDay(date))) {
                    employeeSchedule = schedule;
                    return false;
                }
            });
        }

        return employeeSchedule;
    }

    public get hasTemplateSchedules(): boolean {
        return this.templateSchedules && this.templateSchedules.length > 0;
    }

    public hasTemplateSchedule(date: Date): boolean {
        return !!this.getTemplateSchedule(date);
    }

    public getTemplateSchedulesForDate(date: Date): TimeScheduleTemplateHeadSmallDTO[] {
        return this.hasTemplateSchedules ? this.templateSchedules.filter(t => t.startDate && t.startDate.isSameOrBeforeOnDay(date) && (!t.stopDate || t.stopDate.isSameOrAfterOnDay(date))) : [];
    }

    public getTemplateSchedule(date: Date): TimeScheduleTemplateHeadSmallDTO {
        let schedules = this.getTemplateSchedulesForDate(date);
        if (schedules?.length > 0)
            return schedules[0];

        return undefined;
    }

    public hasTemplateScheduleByStartDate(date: Date): boolean {
        return this.templateSchedules && !!this.templateSchedules.find(t => t.startDate && t.startDate.isSameDayAs(date));
    }

    public isFirstDayOfTemplate(date: Date): boolean {
        return this.templateSchedules && !!this.templateSchedules.find(t => t.startDate && t.startDate.isSameDayAs(date));
    }

    public isLastDayOfTemplate(date: Date): boolean {
        return this.templateSchedules && !!this.templateSchedules.find(t => t.virtualStopDate && t.virtualStopDate.isSameDayAs(date));
    }

    public get hasAvailability(): boolean {
        return this.available && this.available.length > 0;
    }

    public getAvailableInRange(dateFrom: Date, dateTo: Date): EmployeeListAvailabilityDTO[] {
        return this.hasAvailability ? _.sortBy(_.filter(this.available, a => a.isOverlapping(dateFrom, dateTo)), 'start') : [];
    }

    public isAvailableInRange(dateFrom: Date, dateTo: Date): boolean {
        return this.hasAvailability && _.filter(this.available, a => a.isOverlapping(dateFrom, dateTo)).length > 0;
    }

    public hasAvailabilityCommentInRange(dateFrom: Date, dateTo: Date): boolean {
        return (this.hasAvailability && _.filter(this.available, a => a.comment && a.isOverlapping(dateFrom, dateTo)).length > 0) ||
            (this.hasUnavailability && _.filter(this.unavailable, a => a.comment && a.isOverlapping(dateFrom, dateTo)).length > 0);
    }

    public isLastSlotOnAvailability(dateTo: Date): boolean {
        if (dateTo.getSeconds() === 59)
            dateTo = dateTo.addSeconds(1);

        return this.hasAvailability && _.filter(this.available, a => a.stop.isSameMinuteAs(dateTo)).length > 0 ||
            this.hasUnavailability && _.filter(this.unavailable, a => a.stop.isSameMinuteAs(dateTo)).length > 0;
    }

    public isFullyAvailableInRange(dateFrom: Date, dateTo: Date): boolean {
        return this.hasAvailability && !!_.find(this.available, a => a.isFullyOverlapping(dateFrom, dateTo)) && !this.isUnavailableInRange(dateFrom, dateTo);
    }

    public isPartlyAvailableInRange(dateFrom: Date, dateTo: Date): boolean {
        return this.hasAvailability && !!_.find(this.available, a => a.isPartlyOverlapping(dateFrom, dateTo));
    }

    public get hasUnavailability(): boolean {
        return this.unavailable && this.unavailable.length > 0;
    }

    public getUnavailableInRange(dateFrom: Date, dateTo: Date): EmployeeListAvailabilityDTO[] {
        return this.hasUnavailability ? _.sortBy(_.filter(this.unavailable, a => a.isOverlapping(dateFrom, dateTo)), 'start') : [];
    }

    public isUnavailableInRange(dateFrom: Date, dateTo: Date): boolean {
        return this.hasUnavailability && _.filter(this.unavailable, a => a.isOverlapping(dateFrom, dateTo)).length > 0;
    }

    public isFullyUnavailableInRange(dateFrom: Date, dateTo: Date): boolean {
        return this.hasUnavailability && !!_.find(this.unavailable, a => a.isFullyOverlapping(dateFrom, dateTo)) && !this.isAvailableInRange(dateFrom, dateTo);
    }

    public isPartlyUnavailableInRange(dateFrom: Date, dateTo: Date): boolean {
        return this.hasUnavailability && !!_.find(this.unavailable, a => a.isPartlyOverlapping(dateFrom, dateTo));
    }

    public getAnnualLeaveBalanceValue(format: TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat, dayLabel = '', daysLabel = ''): string {
        switch (format) {
            case TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days:
                const days: number = this.annualLeaveBalanceDays.round(2) ?? 0;
                return days.toLocaleString() + ' ' + (days === 1 ? dayLabel : daysLabel);
            case TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours:
                return CalendarUtility.minutesToTimeSpan(this.annualLeaveBalanceMinutes ?? 0);
            default:
                return '';
        }
    }

    public convertToTimeScheduleScenarioEmployeeDTO(): TimeScheduleScenarioEmployeeDTO {
        let clone: TimeScheduleScenarioEmployeeDTO = new TimeScheduleScenarioEmployeeDTO();
        clone.employeeId = this.employeeId;
        clone.employeeName = this.name;
        clone.employeeNumberAndName = this.numberAndName;

        return clone;
    }
}

export class EmployeeListEmploymentDTO implements IEmployeeListEmploymentDTO {
    allowShiftsWithoutAccount: boolean;
    breakDayMinutesAfterMidnight: number;
    dateFrom: Date;
    dateTo: Date;
    employeeGroupId: number;
    employeeGroupName: string;
    isTemporaryPrimary: boolean;
    maxScheduleTime: number;
    minScheduleTime: number;
    percent: number;
    workTimeWeekMinutes: number;
    extraShiftAsDefault: boolean;
    annualLeaveGroupId: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }

    // Get last employment where specified date is in the employments date range
    public static getEmployment(employments: EmployeeListEmploymentDTO[], date: Date): EmployeeListEmploymentDTO {
        if (!employments)
            return null;

        let beginningOfDay = date.beginningOfDay();
        let filteredList: EmployeeListEmploymentDTO[] = new Array();
        let employmentsTempPrimary = employments.filter(e => e.isTemporaryPrimary);
        let employmentsRegular = employments.filter(e => !e.isTemporaryPrimary);

        for (let employment of employmentsTempPrimary) {
            if ((!employment.dateFrom || employment.dateFrom.beginningOfDay() <= beginningOfDay) && (!employment.dateTo || employment.dateTo.endOfDay() >= beginningOfDay)) {
                filteredList.push(employment);
            }
        }
        if (filteredList.length == 0) {
            for (let employment of employmentsRegular) {
                if ((!employment.dateFrom || employment.dateFrom.beginningOfDay() <= beginningOfDay) && (!employment.dateTo || employment.dateTo.endOfDay() >= beginningOfDay)) {
                    filteredList.push(employment);
                }
            }
        }

        return filteredList.sort(this.descDateFromSort)[0];
    }

    private static descDateFromSort(a, b) {
        if (a['dateFrom'] < b['dateFrom'])
            return 1;
        else if (a['dateFrom'] > b['dateFrom'])
            return -1;
        else
            return 0;
    }
}

export class EmployeeListAvailabilityDTO implements IEmployeeListAvailabilityDTO {
    comment: string;
    minutes: number;
    start: Date;
    stop: Date;

    constructor(start?: Date, stop?: Date) {
        if (start)
            this.start = start;
        if (stop)
            this.stop = stop;
    }

    public fixDates() {
        if (this.start)
            this.start = CalendarUtility.convertToDate(this.start);
        if (this.stop)
            this.stop = CalendarUtility.convertToDate(this.stop);
    }

    public get duration(): number {
        return this.stop.diffMinutes(this.start);
    }

    public isOverlapping(dateFrom: Date, dateTo: Date): boolean {
        return CalendarUtility.isRangesOverlapping(this.start, this.stop, dateFrom, dateTo);
    }

    public isFullyOverlapping(dateFrom: Date, dateTo: Date): boolean {
        const duration = CalendarUtility.getIntersectingDuration(this.start, this.stop, dateFrom, dateTo);
        return (duration > 0 && duration === dateTo.diffMinutes(dateFrom));
    }

    public isPartlyOverlapping(dateFrom: Date, dateTo: Date): boolean {
        const duration = CalendarUtility.getIntersectingDuration(this.start, this.stop, dateFrom, dateTo);
        return (duration > 0 && duration < dateTo.diffMinutes(dateFrom));
    }
}

export class EmployeeListSmallDTO implements IEmployeeListSmallDTO {
    accounts: EmployeeAccountDTO[];
    employeeId: number;
    employeeNr: string;
    employeeNrSort: string;
    firstName: string;
    hidden: boolean;
    lastName: string;
    name: string;
    userId: number;
    vacant: boolean;
    //Extension for selecting
    selected: boolean;

    public get numberAndName(): string {
        return this.employeeNr && this.employeeNr !== '0' ? "({0}) {1}".format(this.employeeNr, this.name) : this.name;
    }
}

export class EmployeeRightListDTO {
    employeeId: number;
    employeeNr: string;
    employeeNrSort: string;
    firstName: string;
    lastName: string;
    name: string;
    imageSource: string;

    wantsExtraShifts: boolean = false;
    workTimeMinutes: number = 0;

    toolTip: string;

    employeePostId: number;

    isFullyAvailable: boolean = false;
    isPartlyAvailable: boolean = false;
    isMixedAvailable: boolean = false;
    isFullyUnavailable: boolean = false;
    isPartlyUnavailable: boolean = false;
    availabilitySort: EmployeeAvailabilitySortOrder = EmployeeAvailabilitySortOrder.NotSpecified;

    employeeSkills: EmployeeSkillDTO[];
    employments: EmployeeListEmploymentDTO[];

    hasValidatedPercent: boolean = false;
    percentDiff: number;

    public setTypes() {
        if (this.employeeSkills) {
            this.employeeSkills = this.employeeSkills.map(t => {
                let tObj = new EmployeeSkillDTO();
                angular.extend(tObj, t);
                tObj.fixDates();
                return tObj;
            });
        }
    }

    public getEmployment(dateFrom: Date, dateTo: Date): EmployeeListEmploymentDTO {
        let result: EmployeeListEmploymentDTO;

        let dateFromDay = dateFrom.beginningOfDay();
        let dateToDay = dateTo.beginningOfDay();

        while (dateFromDay <= dateToDay) {
            result = EmployeeListEmploymentDTO.getEmployment(this.employments, dateFromDay);
            if (result)
                break;

            dateFromDay = dateFromDay.addDays(1);
        }

        return result;
    }
}

export class EmployeeSmallDTO implements IEmployeeSmallDTO {
    employeeId: number;
    employeeNr: string;
    name: string;

    // Extensions
    public get numberAndName(): string {
        return "({0}) {1}".format(this.employeeNr, this.name);
    }
}