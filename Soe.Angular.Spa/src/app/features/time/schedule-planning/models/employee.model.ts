import { IDateRangeDTO } from '@shared/models/generated-interfaces/DateRangeDTO';
import { IEmployeeAccountDTO } from '@shared/models/generated-interfaces/EmployeeUserDTO';
import {
  TermGroup_Sex,
  SoeEmployeePostStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ICompanyCategoryRecordDTO,
  IEmployeeSkillDTO,
  ITimeScheduleTemplateHeadSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  IEmployeeListAvailabilityDTO,
  IEmployeeListDTO,
  IEmployeeListEmploymentDTO,
} from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { PlanningShiftDTO } from './shift.model';
import { SpEmployeeDaySlot } from './time-slot.model';
import { DateUtil } from '@shared/util/date-util';

export type DateValueType = { date: Date; value: number };

export interface IPlanningEmployeeDTO extends IEmployeeListDTO {
  daySlots: SpEmployeeDaySlot[];
}

export class PlanningEmployeeDTO implements IPlanningEmployeeDTO {
  employeeId = 0;
  employeeNr = '';
  employeeNrSort = '';
  firstName = '';
  lastName = '';
  sex: TermGroup_Sex = TermGroup_Sex.Unknown;
  annualWorkTimeMinutes = 0;
  annualScheduledTimeMinutes = 0;
  childRuleWorkedTimeMinutes = 0;
  childPeriodBalanceTimeMinutes = 0;
  parentWorkedTimeMinutes = 0;
  parentScheduledTimeMinutes = 0;
  parentRuleWorkedTimeMinutes = 0;
  parentPeriodBalanceTimeMinutes = 0;
  active = false;
  hidden = false;
  vacant = false;
  isSelected = false;
  isGroupHeader = false;
  groupName = '';
  employments: EmployeeListEmploymentDTO[] = [];
  accounts: IEmployeeAccountDTO[] = [];
  categoryRecords: ICompanyCategoryRecordDTO[] = [];
  employeeSkills: IEmployeeSkillDTO[] = [];
  templateSchedules: ITimeScheduleTemplateHeadSmallDTO[] = [];
  employeeSchedules: IDateRangeDTO[] = [];
  absenceRequest: IDateRangeDTO[] = [];
  absenceApproved: IDateRangeDTO[] = [];
  hasAbsenceRequest = false;
  hasAbsenceApproved = false;
  currentDate?: Date;
  available: EmployeeListAvailabilityDTO[] = [];
  unavailable: EmployeeListAvailabilityDTO[] = [];
  employeePostId = 0;
  description = '';
  employeePostStatus = SoeEmployeePostStatus.None;
  name = '';
  hibernatingText = '';

  // Extentions
  daySlots: SpEmployeeDaySlot[] = [];
  isVisible = false;
  toolTip = '';
  employeeGroupName?: string;
  workTimeMinutes = 0;
  oneWeekWorkTimeMinutes = 0;
  cyclePlannedMinutes = 0;
  cyclePlannedAverageMinutes = 0;
  isUnderTime = false;
  isOverTime = false;
  minScheduleTime = 0;
  maxScheduleTime = 0;
  startTime = 0; // Used for sorting
  stopTime = 0; // Used for sorting

  constructor() {
    this.clearTimeAndCosts();
  }

  setEmployeeNrSort(): void {
    this.employeeNrSort = this.employeeNr.padStart(50, '0');
  }

  setTypes() {
    // Map employments to correct type
    this.employments = this.employments.map(ep => {
      const empl = new EmployeeListEmploymentDTO();
      Object.assign(empl, ep);
      return empl;
    });

    // Map availability to correct type
    if (!this.available) this.available = [];
    this.available = this.available.map(a => {
      const avail = new EmployeeListAvailabilityDTO(a.start, a.stop);
      Object.assign(avail, a);
      return avail;
    });
    if (!this.unavailable) this.unavailable = [];
    this.unavailable = this.unavailable.map(a => {
      const unavail = new EmployeeListAvailabilityDTO(a.start, a.stop);
      Object.assign(unavail, a);
      return unavail;
    });
  }

  get numberAndName(): string {
    return this.hidden ? this.name : `(${this.employeeNr}) ${this.name}`;
  }

  // Slots

  getDaySlot(date: Date): SpEmployeeDaySlot | undefined {
    return this.daySlots.find(daySlot => daySlot.start.isSameDay(date));
  }

  // Shifts

  get hasShifts(): boolean {
    return this.daySlots.some(daySlot => daySlot.shifts.length > 0);
  }

  get hasVisibleShifts(): boolean {
    return this.daySlots.some(daySlot => daySlot.shifts.some(s => s.isVisible));
  }

  getShifts(date?: Date, maxDateInView?: Date): PlanningShiftDTO[] {
    if (!date) {
      return this.daySlots.reduce(
        (acc, daySlot) => acc.concat(<any>daySlot.shifts),
        []
      );
    } else {
      const daySlot = this.getDaySlot(date);
      if (daySlot) {
        return daySlot.shifts.filter(shift => {
          const dateRangeStart =
            shift.actualStartDate.isBeforeOnDay(shift.actualStartTime) &&
            shift.actualStartTime.isAfterOnDay(maxDateInView ?? date)
              ? shift.actualStartDate
              : shift.actualStartTime.beginningOfDay();
          const dateRangeEnd = shift.actualStartDate.isAfterOnDay(
            shift.actualStopTime
          )
            ? shift.actualStartDate
            : shift.actualStopTime.beginningOfDay();

          return (
            date.beginningOfDay().isSameOrBeforeOnMinute(dateRangeStart) &&
            date.endOfDay().isSameOrAfterOnMinute(dateRangeEnd)
          );
        });
      } else {
        return [];
      }
    }
  }

  // Employment

  getEmployment(
    dateFrom: Date,
    dateTo: Date
  ): EmployeeListEmploymentDTO | undefined {
    let result: EmployeeListEmploymentDTO | undefined = undefined;

    let dateFromDay = dateFrom.beginningOfDay();
    const dateToDay = dateTo.beginningOfDay();

    while (dateFromDay <= dateToDay) {
      result = EmployeeListEmploymentDTO.getEmployment(
        this.employments,
        dateFromDay
      );
      if (result) break;

      dateFromDay = dateFromDay.addDays(1);
    }

    return result;
  }

  hasEmployment(dateFrom: Date, dateTo: Date, wholePeriod = false): boolean {
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
      if (!this.getEmployment(dateFrom, dateTo)) result = false;
    }

    return result;
  }

  get hasHibernatingEmployment(): boolean {
    return !!this.hibernatingText;
  }

  getEmploymentMinScheduleTime(dateFrom: Date, dateTo: Date): number {
    const employment = this.getEmployment(
      dateFrom.beginningOfDay(),
      dateTo.endOfDay()
    );
    return employment?.minScheduleTime || 0;
  }

  getEmploymentMaxScheduleTime(dateFrom: Date, dateTo: Date): number {
    const employment = this.getEmployment(
      dateFrom.beginningOfDay(),
      dateTo.endOfDay()
    );
    return employment?.maxScheduleTime || 0;
  }

  // Time and cost

  get totalNetTime(): number {
    return this.daySlots.reduce((sum, item) => sum + item.netTime, 0);
  }

  get totalAbsenceTime(): number {
    return this.daySlots.reduce((sum, item) => sum + item.absenceTime, 0);
  }

  get totalFactorTime(): number {
    return this.daySlots.reduce((sum, item) => sum + item.factorTime, 0);
  }

  get totalGrossTime(): number {
    return this.daySlots.reduce((sum, item) => sum + item.grossTime, 0);
  }

  get totalCost(): number {
    return this.daySlots.reduce((sum, item) => sum + item.cost, 0);
  }

  get totalCostIncEmpTaxAndSuppCharge(): number {
    return this.daySlots.reduce(
      (sum, item) => sum + item.costIncEmpTaxAndSuppCharge,
      0
    );
  }

  clearTimeAndCosts(): void {
    this.daySlots.forEach(daySlot => {
      daySlot.clearTimeAndCosts();
      daySlot.hourSlots.forEach(hourSlot => {
        hourSlot.clearTimeAndCosts();
      });
      daySlot.halfHourSlots.forEach(halfHourSlot => {
        halfHourSlot.clearTimeAndCosts();
      });
      daySlot.quarterHourSlots.forEach(quarterHourSlot => {
        quarterHourSlot.clearTimeAndCosts();
      });
    });
    this.isUnderTime = false;
    this.isOverTime = false;
  }

  // Availability

  get hasAvailability(): boolean {
    return this.available && this.available.length > 0;
  }

  getAvailableInRange(
    dateFrom: Date,
    dateTo: Date
  ): EmployeeListAvailabilityDTO[] {
    // TODO: Replace .slice().sort with toSorted after updating to ES2023 in tsconfig
    return this.hasAvailability
      ? this.available
          .filter(a =>
            DateUtil.isRangesOverlapping(a.start, a.stop, dateFrom, dateTo)
          )
          .slice() // Create a shallow copy for immutability
          .sort((a, b) => a.start.getTime() - b.start.getTime())
      : [];
  }

  isAvailableInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      this.hasAvailability &&
      this.available.some(a =>
        DateUtil.isRangesOverlapping(a.start, a.stop, dateFrom, dateTo)
      )
    );
  }

  isFullyAvailableInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      this.hasAvailability &&
      this.available.some(a => a.isFullyOverlapping(dateFrom, dateTo)) &&
      !this.isUnavailableInRange(dateFrom, dateTo)
    );
  }

  isPartiallyAvailableInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      this.hasAvailability &&
      this.available.some(a => a.isPartiallyOverlapping(dateFrom, dateTo))
    );
  }

  get hasUnavailability(): boolean {
    return this.unavailable && this.unavailable.length > 0;
  }

  getUnavailableInRange(
    dateFrom: Date,
    dateTo: Date
  ): EmployeeListAvailabilityDTO[] {
    // TODO: Replace .slice().sort with toSorted after updating to ES2023 in tsconfig
    return this.hasUnavailability
      ? this.unavailable
          .filter(a =>
            DateUtil.isRangesOverlapping(a.start, a.stop, dateFrom, dateTo)
          )
          .slice() // Create a shallow copy for immutability
          .sort((a, b) => a.start.getTime() - b.start.getTime())
      : [];
  }

  isUnavailableInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      this.hasUnavailability &&
      this.unavailable.some(a =>
        DateUtil.isRangesOverlapping(a.start, a.stop, dateFrom, dateTo)
      )
    );
  }

  isFullyUnavailableInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      this.hasUnavailability &&
      this.unavailable.some(a => a.isFullyOverlapping(dateFrom, dateTo)) &&
      !this.isAvailableInRange(dateFrom, dateTo)
    );
  }

  isPartiallyUnavailableInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      this.hasUnavailability &&
      this.unavailable.some(a => a.isPartiallyOverlapping(dateFrom, dateTo))
    );
  }

  hasAvailabilityCommentInRange(dateFrom: Date, dateTo: Date): boolean {
    return (
      (this.hasAvailability &&
        this.available.some(
          a =>
            a.comment &&
            DateUtil.isRangesOverlapping(a.start, a.stop, dateFrom, dateTo)
        )) ||
      (this.hasUnavailability &&
        this.unavailable.some(
          a =>
            a.comment &&
            DateUtil.isRangesOverlapping(a.start, a.stop, dateFrom, dateTo)
        ))
    );
  }

  // Template schedules

  get hasTemplateSchedules(): boolean {
    return this.templateSchedules && this.templateSchedules.length > 0;
  }
}

export class EmployeeListEmploymentDTO implements IEmployeeListEmploymentDTO {
  dateFrom?: Date;
  dateTo?: Date;
  isSecondaryEmployment = false;
  workTimeWeekMinutes = 0;
  percent = 0;
  minScheduleTime = 0;
  maxScheduleTime = 0;
  employeeGroupId?: number;
  employeeGroupName = '';
  breakDayMinutesAfterMidnight = 0;
  allowShiftsWithoutAccount = false;
  isTemporaryPrimary = false;
  extraShiftAsDefault = false;

  // Get last employment where specified date is in the employments date range
  static getEmployment(
    employments: EmployeeListEmploymentDTO[],
    date: Date
  ): EmployeeListEmploymentDTO | undefined {
    if (!employments) return undefined;

    const beginningOfDay = date.beginningOfDay();
    const filteredList: EmployeeListEmploymentDTO[] = [];
    const employmentsTempPrimary = employments.filter(
      e => e.isTemporaryPrimary
    );
    const employmentsRegular = employments.filter(e => !e.isTemporaryPrimary);

    for (const employment of employmentsTempPrimary) {
      if (
        (!employment.dateFrom ||
          employment.dateFrom.beginningOfDay() <= beginningOfDay) &&
        (!employment.dateTo || employment.dateTo.endOfDay() >= beginningOfDay)
      ) {
        filteredList.push(employment);
      }
    }
    if (filteredList.length == 0) {
      for (const employment of employmentsRegular) {
        if (
          (!employment.dateFrom ||
            employment.dateFrom.beginningOfDay() <= beginningOfDay) &&
          (!employment.dateTo || employment.dateTo.endOfDay() >= beginningOfDay)
        ) {
          filteredList.push(employment);
        }
      }
    }

    filteredList.sort(this.descDateFromSort);

    return filteredList[0];
  }

  private static descDateFromSort(
    a: EmployeeListEmploymentDTO,
    b: EmployeeListEmploymentDTO
  ) {
    if (a.dateFrom && b.dateFrom && a.dateFrom < b.dateFrom) return 1;
    else if (a.dateFrom && b.dateFrom && a.dateFrom > b.dateFrom) return -1;
    else return 0;
  }
}

export class EmployeeListAvailabilityDTO
  implements IEmployeeListAvailabilityDTO
{
  start: Date;
  stop: Date;
  comment = '';
  minutes = 0;

  constructor(start: Date, stop: Date) {
    this.start = start;
    this.stop = stop;
  }

  get isWholeDay(): boolean {
    return (
      this.start.isBeginningOfDay() &&
      this.stop.isEndOfDay() &&
      this.start.isSameDay(this.stop)
    );
  }

  get isMultipleDays(): boolean {
    return this.start.isBeforeOnDay(this.stop) && !this.isWholeDay;
  }

  isOverlapping(dateFrom: Date, dateTo: Date): boolean {
    return DateUtil.isRangesOverlapping(
      this.start,
      this.stop,
      dateFrom,
      dateTo
    );
  }

  isFullyOverlapping(dateFrom: Date, dateTo: Date): boolean {
    const duration = DateUtil.getIntersectingMinutes(
      this.start,
      this.stop,
      dateFrom,
      dateTo
    );
    return duration > 0 && duration === dateTo.diffMinutes(dateFrom);
  }

  isPartiallyOverlapping(dateFrom: Date, dateTo: Date): boolean {
    const duration = DateUtil.getIntersectingMinutes(
      this.start,
      this.stop,
      dateFrom,
      dateTo
    );
    return duration > 0 && duration < dateTo.diffMinutes(dateFrom);
  }
}
