import {
  TermGroup_TimeScheduleTemplateBlockShiftUserStatus,
  TermGroup_TimeScheduleTemplateBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from '@shared/util/date-util';
import { GraphicsUtil } from '@shared/util/graphics-util';
import { PlanningShiftBreakDTO, PlanningShiftDTO } from '../models/shift.model';
import { ITimeScheduleTypeFactorSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ValidationHandler } from '@shared/handlers';

export class ShiftUtil {
  // Constants
  static readonly SHIFT_FULL_HEIGHT = 48; // Shift height in px
  static readonly SHIFT_FULL_HEIGHT_MARGIN = 4; // Vertical gap between shifts
  static readonly SHIFT_FULL_HEIGHT_PADDING = 6; // Top and bottom padding of first and last shift inside cell
  static readonly SHIFT_HALF_HEIGHT = 27;
  static readonly SHIFT_HALF_HEIGHT_MARGIN = 1;
  static readonly SHIFT_HALF_HEIGHT_PADDING = 0;

  // Type of shift
  static isSchedule(shift: {
    type: TermGroup_TimeScheduleTemplateBlockType;
  }): boolean {
    return shift.type === TermGroup_TimeScheduleTemplateBlockType.Schedule;
  }

  static isOrder(shift: {
    type: TermGroup_TimeScheduleTemplateBlockType;
  }): boolean {
    return shift.type === TermGroup_TimeScheduleTemplateBlockType.Order;
  }

  static isBooking(shift: {
    type: TermGroup_TimeScheduleTemplateBlockType;
  }): boolean {
    return shift.type === TermGroup_TimeScheduleTemplateBlockType.Booking;
  }

  static isStandby(shift: {
    type: TermGroup_TimeScheduleTemplateBlockType;
  }): boolean {
    return shift.type === TermGroup_TimeScheduleTemplateBlockType.Standby;
  }

  static isOnDuty(shift: {
    type: TermGroup_TimeScheduleTemplateBlockType;
  }): boolean {
    return shift.type === TermGroup_TimeScheduleTemplateBlockType.OnDuty;
  }

  static isLeisureCode(shift: { timeLeisureCodeId?: number }): boolean {
    return !!shift.timeLeisureCodeId;
  }

  // Absence and absence request
  static isAbsence(shift: {
    timeDeviationCauseId?: number;
    isAbsenceRequest?: boolean;
  }): boolean {
    return (
      shift.timeDeviationCauseId !== undefined &&
      shift.timeDeviationCauseId !== 0 &&
      !shift.isAbsenceRequest
    );
  }

  static hasAbsenceRequest(shift: {
    shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus;
    isAbsenceRequest: boolean;
  }): boolean {
    return (
      shift.shiftUserStatus ===
        TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested &&
      !shift.isAbsenceRequest
    );
  }

  static isWholeDay(shift: {
    actualStartTime: Date;
    actualStopTime: Date;
  }): boolean {
    return (
      shift.actualStartTime &&
      shift.actualStartTime.isBeginningOfDay() &&
      shift.actualStopTime &&
      shift.actualStopTime.isEndOfDay()
    );
  }

  static isWholeDayAbsence(shift: {
    actualStartTime: Date;
    actualStopTime: Date;
    isAbsence: boolean;
  }): boolean {
    return this.isWholeDay(shift) && shift.isAbsence;
  }

  // Shift times
  static shiftLength(shift: {
    actualStartTime: Date;
    actualStopTime: Date;
    isAbsence: boolean;
  }): number {
    return !this.isWholeDayAbsence(shift) &&
      shift.actualStartTime &&
      shift.actualStopTime
      ? shift.actualStopTime.diffMinutes(shift.actualStartTime)
      : 0;
  }

  static shiftBreaksLength(
    shift: {
      type: TermGroup_TimeScheduleTemplateBlockType;
      actualStartTime: Date;
      actualStopTime: Date;
      startTime: Date;
    },
    dayShifts: {
      breaks: { startTime: Date; stopTime: Date; isIntersecting: boolean }[];
    }[]
  ): number {
    return this.getBreakTimeWithinShift(shift, dayShifts);
  }

  static shiftLengthExcludingBreaks(
    shift: {
      type: TermGroup_TimeScheduleTemplateBlockType;
      actualStartTime: Date;
      actualStopTime: Date;
      isAbsence: boolean;
      startTime: Date;
    },
    dayShifts: {
      breaks: { startTime: Date; stopTime: Date; isIntersecting: boolean }[];
    }[]
  ): number {
    return (
      this.shiftLength(shift) - this.getBreakTimeWithinShift(shift, dayShifts)
    );
  }

  static getBreakTimeWithinShift(
    shift: {
      actualStartTime: Date;
      actualStopTime: Date;
    },
    dayShifts: {
      breaks: { startTime: Date; stopTime: Date; isIntersecting: boolean }[];
    }[],
    shiftStart?: Date,
    shiftStop?: Date
  ): number {
    if (!shiftStart) shiftStart = shift.actualStartTime;
    if (!shiftStop) shiftStop = shift.actualStopTime;

    // Need to loop over all breaks, since breaks can span over multiple shifts.
    let breakTime = 0;
    dayShifts.forEach(s => {
      s.breaks
        .filter(b => !b.isIntersecting)
        .forEach(b => {
          breakTime += DateUtil.getIntersectingMinutes(
            shiftStart,
            shiftStop,
            b.startTime,
            b.stopTime
          );
        });
    });
    return breakTime;
  }

  static getBreakTimeOutsideShift(
    shift: {
      actualStartTime: Date;
      actualStopTime: Date;
    },
    dayShifts: { breaks: { startTime: Date; stopTime: Date }[] }[]
  ): number {
    if (!shift || !dayShifts || dayShifts.length === 0) return 0;

    let breakTime = 0;

    dayShifts.forEach(s => {
      s.breaks.forEach(b => {
        // Only consider breaks that overlap the shift
        const overlapMinutes = DateUtil.getIntersectingMinutes(
          shift.actualStartTime,
          shift.actualStopTime,
          b.startTime,
          b.stopTime
        );
        if (overlapMinutes > 0) {
          const totalBreakMinutes = b.stopTime.diffMinutes(b.startTime);
          breakTime += totalBreakMinutes - overlapMinutes;
        }
      });
    });
    return breakTime;
  }

  static getFactorMinutesWithinShift(
    shift: {
      type: TermGroup_TimeScheduleTemplateBlockType;
      actualStartTime: Date;
      actualStopTime: Date;
      startTime: Date;
      isAbsence: boolean;
      timeScheduleTypeIsNotScheduleTime: boolean;
      timeScheduleTypeFactors: ITimeScheduleTypeFactorSmallDTO[];
    },
    dayShifts: {
      breaks: { startTime: Date; stopTime: Date; isIntersecting: boolean }[];
    }[]
  ): number {
    if (
      !shift.timeScheduleTypeIsNotScheduleTime &&
      (!shift.timeScheduleTypeFactors ||
        shift.timeScheduleTypeFactors.length === 0)
    )
      return 0;

    let totalMinutes = 0;

    if (shift.timeScheduleTypeIsNotScheduleTime) {
      // Subtract the whole shift time
      totalMinutes =
        this.getBreakTimeWithinShift(shift, dayShifts) -
        this.shiftLength(shift);
    } else {
      // Calculate time for each factor
      shift.timeScheduleTypeFactors.forEach(factor => {
        factor.fromTime =
          DateUtil.parseDateOrJson(factor.fromTime) || DateUtil.getToday();
        factor.toTime =
          DateUtil.parseDateOrJson(factor.toTime) || DateUtil.getToday();

        const fromTime: Date = shift.actualStartTime
          .beginningOfDay()
          .mergeTime(factor.fromTime);
        const toTime: Date = fromTime.addMinutes(
          factor.toTime.diffMinutes(factor.fromTime)
        );

        const maxStart: Date = DateUtil.getMaxDate(
          shift.actualStartTime,
          fromTime
        );
        const minStop: Date = DateUtil.getMinDate(shift.actualStopTime, toTime);
        let factorMinutes: number = DateUtil.getIntersectingMinutes(
          shift.actualStartTime,
          shift.actualStopTime,
          fromTime,
          toTime
        );
        factorMinutes -= ShiftUtil.getBreakTimeWithinShift(
          shift,
          dayShifts,
          maxStart,
          minStop
        );

        // Reduce factor with 1 to get value that should be added to original time.
        // E.g: 60 minutes with factor 2 will return  60 minutes (to be added to the original 60 minutes = 120).
        //      60 minutes with factor 4 will return 180 minutes (to be added to the original 60 minutes = 240).
        let factorValue: number = factor.factor;
        if (factor.factor >= 0) factorValue -= 1;

        totalMinutes += factorMinutes * factorValue;
      });
    }

    return totalMinutes;
  }

  // Shift type
  static shiftTypeColor(shift: {
    shiftTypeColor?: string;
    isAbsenceRequest?: boolean;
    isReadOnly?: boolean;
    isLended?: boolean;
    timeLeisureCodeId?: number;
  }): string {
    let color = shift.shiftTypeColor || '';

    if (shift.isAbsenceRequest) {
      color = '#ff832b'; // $shift-absence-request-color
      shift.isReadOnly = true;
    } else if (shift.isLended) {
      color = '#dfdfdf'; // $shift-lended-color
    } else if (this.isLeisureCode(shift)) {
      color = '#dfdfdf'; // $shift-leisure-code-color
    } else {
      color = GraphicsUtil.removeAlphaValue(color, '#b6b6b6'); // $shift-no-shift-type-color
    }

    return color;
  }

  static textColor(shift: { shiftTypeColor?: string }): string {
    return GraphicsUtil.foregroundColorByBackgroundBrightness(
      shift.shiftTypeColor || ''
    );
  }

  static getShiftsOverlappedByBreak(
    shifts: PlanningShiftDTO[],
    brk: PlanningShiftBreakDTO
  ): PlanningShiftDTO[] {
    // From the specified list of shifts, return only shifts that are overlapped by the specified other shift.
    return shifts.filter(s =>
      DateUtil.isRangesOverlapping(
        s.actualStartTime,
        s.actualStopTime,
        brk.startTime,
        brk.stopTime
      )
    );
  }

  static sortShifts(shifts: PlanningShiftDTO[]): PlanningShiftDTO[] {
    return shifts.sort((a, b) => {
      const startTimeComparison =
        a.actualStartTime.getTime() - b.actualStartTime.getTime();
      if (startTimeComparison !== 0) return startTimeComparison;

      const stopTimeComparison =
        a.actualStopTime.getTime() - b.actualStopTime.getTime();
      if (stopTimeComparison !== 0) return stopTimeComparison;

      return a.shiftTypeName.localeCompare(b.shiftTypeName);
    });
  }

  static sortShiftsByStartThenStop(
    a: PlanningShiftDTO,
    b: PlanningShiftDTO
  ): number {
    return (
      a.actualStartTime.getTime() - b.actualStartTime.getTime() ||
      a.actualStopTime.getTime() - b.actualStopTime.getTime()
    );
  }

  static sortBreaksByStartThenStop(
    a: PlanningShiftBreakDTO,
    b: PlanningShiftBreakDTO
  ): number {
    return (
      a.startTime.getTime() - b.startTime.getTime() ||
      a.stopTime.getTime() - b.stopTime.getTime()
    );
  }

  static createShiftFormFromDTO<CtorArgs, T>(
    ctor: new (args: CtorArgs) => T,
    args: CtorArgs
  ): T {
    return new ctor(args);
  }
}
