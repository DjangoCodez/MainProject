import {
  TermGroup_TimeScheduleTemplateBlockShiftStatus,
  TermGroup_TimeScheduleTemplateBlockShiftUserStatus,
  TermGroup_TimeScheduleTemplateBlockType,
  TermGroup_YesNo,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IShiftDTO,
  IShiftBreakDTO,
} from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { DateUtil } from '@shared/util/date-util';
import { Guid } from '@shared/util/string-util';
import { ShiftUtil } from '../util/shift-util';
import { ITimeScheduleTypeFactorSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

// DAY

export interface IPlanningShiftDayDTO {
  date: Date;
  employeeId: number;
  shifts: PlanningShiftDTO[];
}

export class PlanningShiftDayDTO implements IPlanningShiftDayDTO {
  date: Date;
  employeeId: number;
  shifts: PlanningShiftDTO[];

  constructor(date: Date, employeeId: number, shifts?: PlanningShiftDTO[]) {
    this.date = date;
    this.employeeId = employeeId;
    this.shifts = shifts || [];
  }
}

// SHIFT

export interface IPlanningShiftDTO extends IShiftDTO {
  actualStartTime: Date;
  actualStopTime: Date;
}

export class PlanningShiftDTO implements IPlanningShiftDTO {
  type: TermGroup_TimeScheduleTemplateBlockType =
    TermGroup_TimeScheduleTemplateBlockType.Schedule;
  timeScheduleTemplateBlockId = 0;
  timeScheduleEmployeePeriodId = 0;
  timeCodeId = 0; // Default time code (company setting) will be set on server side if 0
  description = '';
  absenceStartTime: Date = new Date();
  absenceStopTime: Date = new Date();
  startTime: Date = new Date();
  stopTime: Date = new Date();
  belongsToPreviousDay = false;
  belongsToNextDay = false;
  timeScheduleTypeId = 0;
  timeScheduleTypeCode = '';
  timeScheduleTypeName = '';
  timeScheduleTypeIsNotScheduleTime = false;
  timeScheduleTypeFactors: ITimeScheduleTypeFactorSmallDTO[] = [];
  shiftTypeTimeScheduleTypeId = 0;
  shiftTypeTimeScheduleTypeCode = '';
  shiftTypeTimeScheduleTypeName = '';
  employeeId = 0;
  employeeName = '';
  grossTime = 0;
  totalCost = 0;
  totalCostIncEmpTaxAndSuppCharge = 0;
  breaks: PlanningShiftBreakDTO[] = [];
  timeDeviationCauseId?: number;
  timeDeviationCauseName = '';
  shiftTypeId = 0;
  shiftTypeCode = '';
  shiftTypeName = '';
  shiftTypeDesc = '';
  shiftTypeColor = '';
  shiftStatus = TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned;
  shiftUserStatus = TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted;
  isPreliminary = false;
  extraShift = false;
  substituteShift = false;
  isDeleted = false;
  accountId?: number;
  accountName = '';
  nbrOfWantedInQueue = 0;
  timeScheduleTemplateHeadId?: number;
  nbrOfWeeks = 0;
  dayNumber = 0;
  originalBlockId = 0;
  link?: string;
  isAbsenceRequest = false;
  timeLeisureCodeId?: number;
  approvalTypeId: number = TermGroup_YesNo.Unknown;

  // Extensions
  tempTimeScheduleTemplateBlockId = 0;
  actualStartTime = new Date();
  actualStopTime = new Date();
  originalStartTime?: Date; // Temporary placeholder for starttime used when saving
  originalStopTime?: Date; // Temporary placeholder for stoptime used when saving
  startTimeStartsOn = 0; // 0 = current day, 1 = previous day, 2 = next day
  stopTimeStartsOn = 0; // 0 = current day, 1 = previous day, 2 = next day
  textColor?: string;
  toolTip?: string;

  isBreak = false;
  isVisible = true;
  isReadOnly = false;
  isLended = false;
  isOtherAccount = false;

  isSelected = false;

  // Used for drawing the shift in the day view
  rowNbr = 0;
  positionTop = 0;
  positionLeft = 0;
  positionWidth = 0;

  fixDates() {
    this.startTime = DateUtil.parseDateOrJson(this.startTime)!;
    this.stopTime = DateUtil.parseDateOrJson(this.stopTime)!;

    this.actualStartTime = this.startTime;
    this.actualStopTime = this.stopTime;

    this.breaks.forEach(brk => {
      brk.startTime = DateUtil.parseDateOrJson(brk.startTime)!;
      brk.stopTime = brk.startTime.addMinutes(brk.minutes);
    });
  }

  setProperties() {
    this.shiftTypeColor = ShiftUtil.shiftTypeColor(this);
    this.textColor = ShiftUtil.textColor(this);
  }

  setTypes() {
    this.breaks = (this.breaks || []).map(brk => {
      const b = new PlanningShiftBreakDTO();
      Object.assign(b, brk);
      return b;
    });
  }

  setIntersectingBreaks(breaks: PlanningShiftBreakDTO[]) {
    // Mark all incoming breaks as intersecting
    const intersectingBreaks = (breaks || []).map(brk => {
      const b = new PlanningShiftBreakDTO();
      Object.assign(b, brk);
      b.isIntersecting = true;
      return b;
    });

    // Combine with existing breaks
    const allBreaks = this.breaks.concat(intersectingBreaks);

    // Sort by startTime
    allBreaks.sort((a, b) => a.startTime.getTime() - b.startTime.getTime());

    this.breaks = allBreaks;
  }

  get actualStartDate(): Date {
    if (this.belongsToPreviousDay)
      return this.actualStartTime.addDays(-1).beginningOfDay();
    else if (this.belongsToNextDay)
      return this.actualStartTime.addDays(1).beginningOfDay();
    return this.actualStartTime.beginningOfDay();
  }

  get isSchedule(): boolean {
    return ShiftUtil.isSchedule(this);
  }

  get isOrder(): boolean {
    return ShiftUtil.isOrder(this); // && !!this.order;
  }

  get isBooking(): boolean {
    return ShiftUtil.isBooking(this);
  }

  get isStandby(): boolean {
    return ShiftUtil.isStandby(this);
  }

  get isOnDuty(): boolean {
    return ShiftUtil.isOnDuty(this);
  }

  get isLeisureCode(): boolean {
    return ShiftUtil.isLeisureCode(this);
  }

  public get isNeed(): boolean {
    return false; // this.type === TermGroup_TimeScheduleTemplateBlockType.Need || (this.staffingNeedsRowId && this.staffingNeedsRowId !== 0);
  }

  get isAbsence(): boolean {
    return ShiftUtil.isAbsence(this);
  }

  get hasAbsenceRequest(): boolean {
    return ShiftUtil.hasAbsenceRequest(this);
  }

  get isWholeDay(): boolean {
    return ShiftUtil.isWholeDay(this);
  }

  get isWholeDayAbsence(): boolean {
    return ShiftUtil.isWholeDayAbsence(this);
  }

  get isWanted(): boolean {
    return false; // this.nbrOfWantedInQueue > 0;
  }

  get isUnwanted(): boolean {
    return false; // this.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted;
  }

  get isZeroShift(): boolean {
    return this.actualStartTime.isSameMinute(this.actualStopTime);
  }

  get shiftLength(): number {
    return ShiftUtil.shiftLength(this);
  }

  get hasMultipleScheduleTypes(): boolean {
    return (
      !!this.timeScheduleTypeId &&
      !!this.shiftTypeTimeScheduleTypeId &&
      this.timeScheduleTypeId !== this.shiftTypeTimeScheduleTypeId
    );
  }

  getTimeScheduleTypeCodes(includeShiftTypeTimeScheduleType: boolean): string {
    const shiftTypes: string[] = [];
    if (
      includeShiftTypeTimeScheduleType &&
      this.hasMultipleScheduleTypes &&
      this.shiftTypeTimeScheduleTypeCode
    )
      shiftTypes.push(this.shiftTypeTimeScheduleTypeCode);
    if (this.timeScheduleTypeCode) shiftTypes.push(this.timeScheduleTypeCode);

    return shiftTypes.join(', ');
  }

  getTimeScheduleTypeNames(includeShiftTypeTimeScheduleType: boolean): string {
    const shiftTypes: string[] = [];
    if (
      includeShiftTypeTimeScheduleType &&
      this.hasMultipleScheduleTypes &&
      this.shiftTypeTimeScheduleTypeName
    )
      shiftTypes.push(this.shiftTypeTimeScheduleTypeName);
    if (this.timeScheduleTypeName) shiftTypes.push(this.timeScheduleTypeName);

    return shiftTypes.join(', ');
  }

  // Breaks

  // createBreaksFromShift(selectedDate?: Date): PlanningShiftDTO[] {
  //   if (!selectedDate) selectedDate = this.actualStartTime.beginningOfDay();

  //   const breaks: PlanningShiftDTO[] = [];
  //   const brk1 = this.createBreakFromShift(1, selectedDate);
  //   if (brk1) breaks.push(brk1);
  //   const brk2 = this.createBreakFromShift(2, selectedDate);
  //   if (brk2) breaks.push(brk2);
  //   const brk3 = this.createBreakFromShift(3, selectedDate);
  //   if (brk3) breaks.push(brk3);
  //   const brk4 = this.createBreakFromShift(4, selectedDate);
  //   if (brk4) breaks.push(brk4);

  //   return breaks;
  // }

  // createBreakFromShift(
  //   breakNbr: number,
  //   selectedDate?: Date
  // ): PlanningShiftDTO | undefined {
  //   if (!selectedDate) selectedDate = this.actualStartTime.beginningOfDay();

  //   const breakId = this[`break${breakNbr}Id` as keyof typeof this] as number;

  //   const breakTimeCodeId =
  //     this[`break${breakNbr}TimeCodeId` as keyof typeof this];
  //   if (!breakTimeCodeId) return undefined;

  //   const breakStartTime = this[
  //     `break${breakNbr}StartTime` as keyof typeof this
  //   ] as Date | undefined;
  //   if (!breakStartTime) return undefined;

  //   const breakMinutes =
  //     (this[`break${breakNbr}Minutes` as keyof typeof this] as number) || 0;
  //   if (breakMinutes === 0) return undefined;

  //   const brk: PlanningShiftDTO = this.copy(true, false);
  //   (<any>brk)[`break${breakNbr}Id`] =
  //     brk.timeScheduleTemplateBlockId =
  //     brk.tempTimeScheduleTemplateBlockId =
  //       breakId;

  //   brk.timeCodeId = <number>breakTimeCodeId;

  //   if (
  //     breakStartTime
  //       .beginningOfDay()
  //       .isAfterOnDay(DateUtil.defaultDateTime().addDays(2))
  //   ) {
  //     brk.actualStartTime = breakStartTime;
  //   } else {
  //     brk.actualStartTime = this.startTime
  //       .mergeTime(breakStartTime)
  //       .addDays(
  //         breakStartTime.beginningOfDay().diffDays(DateUtil.defaultDateTime())
  //       );
  //   }

  //   brk.actualStopTime = brk.actualStartTime.addMinutes(breakMinutes);
  //   brk.isPreliminary = this[
  //     `break${breakNbr}IsPreliminary` as keyof typeof this
  //   ] as boolean;
  //   this.setCommonBreakInfo(brk, selectedDate);
  //   return brk;
  // }

  // private setCommonBreakInfo(brk: PlanningShiftDTO, selectedDate: Date) {
  //   brk.isBreak = true;
  //   brk.belongsToPreviousDay = brk.actualStartTime.isAfterOnDay(selectedDate);
  //   brk.belongsToNextDay = brk.actualStartTime.isBeforeOnDay(selectedDate);
  //   //brk.employeeChildId = 0;
  //   brk.timeDeviationCauseId = 0;
  //   brk.timeDeviationCauseName = '';
  //   brk.timeScheduleTypeId = 0;
  //   brk.timeScheduleTypeCode = '';
  //   brk.timeScheduleTypeName = '';
  //   brk.shiftTypeTimeScheduleTypeId = 0;
  //   brk.shiftTypeTimeScheduleTypeCode = '';
  //   brk.shiftTypeTimeScheduleTypeName = '';
  //   //brk.isModified = false;
  // }

  prepareShiftForSave() {
    this.setTimesForSave();
  }

  setTimesForSave() {
    this.clearSeconds();
    this.originalStartTime = this.startTime;
    this.originalStopTime = this.stopTime;
    this.startTime = this.actualStartTime;
    this.stopTime = this.actualStopTime;

    // Whole day absence stop time must be restored back before saved
    // When loaded it set to stretch over the whole day
    if (this.isWholeDayAbsence) this.stopTime = this.stopTime.beginningOfDay();
  }

  resetTimesForSave() {
    if (this.originalStartTime) this.startTime = this.originalStartTime;
    if (this.originalStopTime) this.stopTime = this.originalStopTime;
  }

  // setDayNumber(periodStart: Date) {
  //   this.dayNumber =
  //     this.actualStartDate
  //       .beginningOfDay()
  //       .diffDays(periodStart.beginningOfDay()) + 1;
  // }

  clearSeconds() {
    if (this.startTime) this.startTime.clearSeconds();
    if (this.stopTime) this.stopTime.clearSeconds();
    if (this.actualStartTime) this.actualStartTime.clearSeconds();
    if (this.actualStopTime) this.actualStopTime.clearSeconds();
  }

  copy(keepLink: boolean, keepTasks: boolean): PlanningShiftDTO {
    const dto = structuredClone(this);
    dto.timeScheduleTemplateBlockId = 0;
    dto.tempTimeScheduleTemplateBlockId = 0;
    if (!keepLink) dto.link = Guid.newGuid();
    // TODO: Implement tasks
    // if (!keepTasks) dto.tasks = [];
    // TODO: Implement queue
    // dto.nbrOfWantedInQueue = 0;
    // dto.isModified = true;

    return dto;
  }
}

// BREAK

export interface IPlanningShiftBreakDTO extends IShiftBreakDTO {
  startTimeStartsOn: number;
  stopTime?: Date | undefined;
}

export class PlanningShiftBreakDTO implements IPlanningShiftBreakDTO {
  breakId = 0;
  tempBreakId = 0; // Temporary id used for tracking in the UI
  timeCodeId = 0;
  startTime: Date = new Date();
  belongsToPreviousDay = false;
  belongsToNextDay = false;
  minutes = 0;
  link?: string | undefined;
  isPreliminary = false;

  // Extensions
  startTimeStartsOn = 0; // 0 = current day, 1 = previous day, 2 = next day
  stopTime: Date = new Date();
  stopTimeStartsOn = 0; // 0 = current day, 1 = previous day, 2 = next day

  isIntersecting = false;

  positionTop = 0;
  positionLeft = 0;
  positionWidth = 0;

  get actualStartDate(): Date {
    if (this.belongsToPreviousDay)
      return this.startTime.addDays(-1).beginningOfDay();
    else if (this.belongsToNextDay)
      return this.startTime.addDays(1).beginningOfDay();
    return this.startTime.beginningOfDay();
  }
}
