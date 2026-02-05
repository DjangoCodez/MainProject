import { inject, Injectable } from '@angular/core';
import {
  PlanningShiftBreakDTO,
  PlanningShiftDTO,
} from '../../models/shift.model';
import { SpSettingService } from '../../services/sp-setting.service';
import { SpTranslateService } from '../../services/sp-translate.service';
import { TranslateService } from '@ngx-translate/core';
import { SpWorkRuleService } from '../../services/sp-work-rule.service';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { ShiftUtil } from '../../util/shift-util';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { DateUtil } from '@shared/util/date-util';
import { map, Observable, of, finalize } from 'rxjs';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import {
  SoeScheduleWorkRules,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import { SpFilterService } from '../../services/sp-filter.service';

export type validateHolesResult = {
  passed: boolean;
  hasHole?: boolean;
  hasBreakInsideHole?: boolean;
  adjustedShifts?: {
    shift: PlanningShiftDTO;
    newStopTime?: Date;
    addedBreaks?: PlanningShiftBreakDTO[];
  }[];
};

@Injectable()
export class SpShiftDialogValidationService {
  private readonly service = inject(SchedulePlanningService);
  private readonly employeeService = inject(SpEmployeeService);
  private readonly filterService = inject(SpFilterService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly settingService = inject(SpSettingService);
  private readonly spTranslateService = inject(SpTranslateService);
  private readonly translate = inject(TranslateService);
  private readonly workRuleService = inject(SpWorkRuleService);

  validate(
    employeeId: number,
    date: Date,
    shifts: PlanningShiftDTO[]
  ): boolean {
    // Validation for all shifts
    const validationErrors: string[] = [];
    let isValid = true;

    // AccountId must be set on all shifts if using account hierarchy,
    // unless setting on employee group allows it
    // TODO: Account hierarchy not implemented
    if (
      this.settingService.useAccountHierarchy() &&
      !this.allowShiftsWithoutAccount(employeeId, date)
    ) {
      // if (shifts.some(s => !s.isBreak && !s.accountId)) {
      //   isValid = false;
      //   validationErrors.push(
      //     this.translate
      //       .instant('time.schedule.planning.editshift.accountmandatory')
      //       .format(this.accountDim.name.toLocaleLowerCase()) + '\n'
      //   );
      // }
    }

    // ShiftType mandatory (company setting)
    // Not mandatory on absence
    if (
      this.settingService.shiftTypeMandatory() &&
      shifts.some(s => !s.shiftTypeId && !s.timeDeviationCauseId)
    ) {
      isValid = false;
      validationErrors.push(
        this.translate.instant(
          'time.schedule.planning.editshift.shifttypemandatory'
        )
      );
    }

    // Validation for each shift
    // Group shifts by date
    const dates: Date[] = this.getUniqueDates(shifts);
    dates.forEach(date => {
      // Create new sorted collection of shifts for the date
      const dayShifts = shifts
        .filter(s => !s.isOnDuty && s.actualStartDate.isSameDay(date))
        .slice()
        .sort(ShiftUtil.sortShiftsByStartThenStop);
      // Create new sorted collection of breaks for the date
      const dayBreaks = dayShifts
        .map(shift => shift.breaks)
        .flat()
        .sort(ShiftUtil.sortBreaksByStartThenStop);

      // Validate shifts for the day
      const shiftValidationErrors = this.validateShifts(dayShifts);
      if (shiftValidationErrors.length > 0) {
        isValid = false;
        validationErrors.push(...shiftValidationErrors);
      }

      // Validate breaks for the day
      const breakValidationErrors = this.validateBreaks(
        dayShifts,
        dayBreaks,
        date
      );
      if (breakValidationErrors.length > 0) {
        isValid = false;
        validationErrors.push(...breakValidationErrors);
      }
    });

    // Show validation errors
    if (!isValid && validationErrors.length > 0) {
      this.messageboxService.error(
        this.translate.instant('core.unabletosave'),
        validationErrors.join('\n')
      );
    }

    return isValid;
  }

  private validateShifts(dayShifts: PlanningShiftDTO[]): string[] {
    const validationErrors: string[] = [];
    let isValid = true;

    let shiftsOverlapping = false;
    let tasksOutsideConnectedShift = false;
    let prevShift: PlanningShiftDTO | undefined = undefined;
    dayShifts.forEach(shift => {
      // Check for overlapping shifts
      if (prevShift) {
        // If hidden employee only validate if shifts are linked
        if (
          shift.employeeId !== this.service.hiddenEmployeeId ||
          (prevShift.link && prevShift.link === shift.link)
        ) {
          // Overlapping
          if (shift.actualStartTime.isBeforeOnMinute(prevShift.actualStopTime))
            shiftsOverlapping = true;
        }
      }

      tasksOutsideConnectedShift = this.validateTasks(shift);

      prevShift = shift;
    });

    if (shiftsOverlapping) {
      isValid = false;
      validationErrors.push(
        this.translate
          .instant('time.schedule.planning.editshift.overlappingshifts')
          .format(this.spTranslateService.shiftsUndefined())
      );
    }

    if (tasksOutsideConnectedShift) {
      isValid = false;
      validationErrors.push(
        this.translate.instant(
          'time.schedule.planning.editshift.tasksoutsideconnectedshift'
        )
      );
    }

    return validationErrors;
  }

  private validateTasks(shift: PlanningShiftDTO): boolean {
    const tasksOutsideConnectedShift = false;

    // TODO: Tasks not implemented

    //Check for tasks outside its connected shift
    // const tasks: TimeScheduleTemplateBlockTaskDTO[] = this.allTasks.filter(
    //   t =>
    //     t.tempTimeScheduleTemplateBlockId ===
    //     shift.tempTimeScheduleTemplateBlockId
    // );
    // tasks.forEach((task: TimeScheduleTemplateBlockTaskDTO) => {
    //   task.startTime = CalendarUtility.convertToDate(task.startTime);
    //   task.stopTime = CalendarUtility.convertToDate(task.stopTime);
    //   const taskStartTime =
    //     this.isTemplateView || this.isEmployeePostView
    //       ? shift.date.mergeTime(task.startTime)
    //       : task.startTime;
    //   const taskStopTime =
    //     this.isTemplateView || this.isEmployeePostView
    //       ? taskStartTime.addMinutes(
    //           task.stopTime.diffMinutes(task.startTime)
    //         )
    //       : task.stopTime;
    //   if (
    //     taskStartTime.isBeforeOnMinute(shift.actualStartTime) ||
    //     taskStopTime.isAfterOnMinute(shift.actualStopTime)
    //   )
    //     tasksOutsideConnectedShift = true;
    // });

    return tasksOutsideConnectedShift;
  }

  private validateBreaks(
    dayShifts: PlanningShiftDTO[],
    dayBreaks: PlanningShiftBreakDTO[],
    date: Date
  ): string[] {
    const validationErrors: string[] = [];
    let isValid = true;

    // Check max number of breaks per day against setting
    if (dayBreaks.length > this.settingService.maxNbrOfBreaks()) {
      isValid = false;
      validationErrors.push(
        this.translate
          .instant('time.schedule.planning.editshift.maxnbrofbreakspassed')
          .format(
            date.toFormattedDate(),
            dayBreaks.length,
            this.settingService.maxNbrOfBreaks()
          )
      );
    }

    let breaksOverlapping = false;
    let prevBreak: PlanningShiftBreakDTO | undefined = undefined;
    dayBreaks.forEach(brk => {
      // Verify that break is within start and end time of day.
      // Make sure a break is not spanning over two shifts with different links.
      let breakTimeIsValid = true;
      let dayStartsWithBreak = false;
      let dayEndsWithBreak = false;
      let prevShift: PlanningShiftDTO | undefined = undefined;
      for (const shift of dayShifts) {
        // Break starts before first shift start
        if (!prevShift) {
          if (brk.startTime.isSameOrBeforeOnMinute(shift.actualStartTime)) {
            breakTimeIsValid = false;
            if (brk.startTime.isSameMinute(shift.actualStartTime))
              dayStartsWithBreak = true;
            break;
          }
        }

        if (prevShift && prevShift.link !== shift.link) {
          // Shift is not linked with previous shifts, check intersecting breaks
          const prevIntersect = DateUtil.getIntersectingMinutes(
            prevShift.actualStartTime,
            prevShift.actualStopTime,
            brk.startTime,
            brk.stopTime
          );
          const currentIntersect = DateUtil.getIntersectingMinutes(
            shift.actualStartTime,
            shift.actualStopTime,
            brk.startTime,
            brk.stopTime
          );
          // Current break intersects with both previous shift and current shift
          if (prevIntersect > 0 && currentIntersect > 0) {
            breakTimeIsValid = false;
            break;
          }
        }

        prevShift = shift;
      }

      // Break ends after last shift ends
      if (
        breakTimeIsValid &&
        prevShift &&
        brk.stopTime.isSameOrAfterOnMinute(prevShift.actualStopTime)
      ) {
        breakTimeIsValid = false;
        if (brk.stopTime.isSameMinute(prevShift.actualStopTime))
          dayEndsWithBreak = true;
      }

      if (!breakTimeIsValid) {
        isValid = false;
        if (dayStartsWithBreak)
          validationErrors.push(
            this.translate.instant(
              'time.schedule.planning.editshift.daystartswithbreak'
            )
          );
        else if (dayEndsWithBreak)
          validationErrors.push(
            this.translate.instant(
              'time.schedule.planning.editshift.dayendswithbreak'
            )
          );
        else
          validationErrors.push(
            this.translate.instant(
              'time.schedule.planning.editshift.breakoutsideworktime'
            )
          );
      }

      // Verify that break start/end times corresponds with break type length
      const timeCodeBreak = this.service.getTimeCodeBreak(brk.timeCodeId, true);
      const breakLength: number = brk.minutes;
      if (!timeCodeBreak && breakLength > 0) {
        isValid = false;
        validationErrors.push(
          this.translate
            .instant('time.schedule.planning.editshift.breaktypemissing')
            .format(
              brk.startTime.toFormattedTime(),
              brk.stopTime.toFormattedTime()
            )
        );
      } else if (
        timeCodeBreak &&
        breakLength !== timeCodeBreak.defaultMinutes
      ) {
        isValid = false;
        validationErrors.push(
          this.translate
            .instant('time.schedule.planning.editshift.breaktypelengthmismatch')
            .format(
              brk.startTime.toFormattedTime(),
              brk.stopTime.toFormattedTime(),
              breakLength,
              timeCodeBreak.defaultMinutes
            )
        );
      }

      // Overlapping
      if (prevBreak && brk.startTime.isBeforeOnMinute(prevBreak.stopTime))
        breaksOverlapping = true;

      prevBreak = brk;
    });

    if (breaksOverlapping) {
      isValid = false;
      validationErrors.push(
        this.translate.instant(
          'time.schedule.planning.editshift.overlappingbreaks'
        )
      );
    }

    return validationErrors;
  }

  validateHolesWithBreaks(
    dayShifts: PlanningShiftDTO[],
    dayBreaks: PlanningShiftBreakDTO[]
  ): Observable<validateHolesResult> {
    const result = this.hasHolesWithBreaksInside(dayShifts, dayBreaks, false);
    if (!(result.hasHole && result.hasBreakInsideHole)) {
      // No holes with breaks inside exists
      return of({ passed: true });
    }

    if (this.settingService.disableBreaksWithinHolesWarning()) {
      // Warning disabled, automatically adjust break to fill hole
      return of(this.hasHolesWithBreaksInside(dayShifts, dayBreaks, true));
    }

    // Ask user to adjust breaks within holes
    const mb = this.messageboxService.questionAbort(
      this.translate
        .instant('core.unabletosave')
        .format(this.spTranslateService.shiftsUndefined()),
      this.translate
        .instant('time.schedule.planning.editshift.askadjustholes')
        .format(this.spTranslateService.shiftsDefined()),
      {
        customIconName: 'ban',
        iconClass: 'warning-color',
        showInputCheckbox: true,
        inputCheckboxLabel: this.translate.instant('core.donotshowagain'),
      }
    );
    return mb.afterClosed().pipe(
      map((response: IMessageboxComponentResponse) => {
        // Save user setting
        if (response.checkboxValue) {
          this.settingService.disableBreaksWithinHolesWarning.set(true);
          this.settingService.saveBoolUserSetting(
            UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning,
            this.settingService.disableBreaksWithinHolesWarning()
          );
        }

        if (!response.result) return { passed: false };

        // Adjust break to fill hole
        return this.hasHolesWithBreaksInside(dayShifts, dayBreaks, true);
      })
    );
  }

  private hasHolesWithBreaksInside(
    scheduleShifts: PlanningShiftDTO[],
    breakShifts: PlanningShiftBreakDTO[],
    adjustShifts: boolean
  ): validateHolesResult {
    const result: validateHolesResult = { passed: false };

    let prevShift: PlanningShiftDTO | undefined = undefined;

    scheduleShifts.forEach(shift => {
      // Check for holes
      if (prevShift?.actualStopTime.isBeforeOnMinute(shift.actualStartTime)) {
        result.hasHole = true;

        // A hole is found, check if a break is within the hole
        if (
          breakShifts.filter(
            s =>
              (s.startTime.isSameOrAfterOnMinute(prevShift!.actualStopTime) &&
                s.startTime.isBeforeOnMinute(shift.actualStartTime)) ||
              (s.stopTime.isAfterOnMinute(prevShift!.actualStopTime) &&
                s.stopTime.isSameOrBeforeOnMinute(shift.actualStartTime))
          ).length > 0
        ) {
          result.hasBreakInsideHole = true;

          if (adjustShifts) {
            // Return result with adjusted shift to fill hole
            if (!result.adjustedShifts) result.adjustedShifts = [];
            result.adjustedShifts.push({
              shift: prevShift,
              newStopTime: shift.actualStartTime,
            });
          }
        }
      }
      prevShift = shift;
    });

    result.passed = !result.hasHole;

    return result;
  }

  validateHolesWithoutBreaks(
    scheduleShifts: PlanningShiftDTO[],
    breakShifts: PlanningShiftBreakDTO[]
  ): Observable<validateHolesResult> {
    const result: validateHolesResult = { passed: false };

    const hasHoles = this.hasHolesWithoutBreaks(scheduleShifts, breakShifts);

    if (!hasHoles) {
      // No holes without breaks exists
      return of({ passed: true });
    }

    result.hasHole = true;

    if (this.settingService.allowHolesWithoutBreaks()) {
      // Holes are allowed, ask user to save with holes
      const mb = this.messageboxService.questionAbort(
        'core.warning',
        'time.schedule.planning.editshift.asksavewithholes',
        {
          customIconName: 'warning',
          iconClass: 'warning-color',
          buttonYesLabelKey: 'core.save',
          buttonNoLabelKey: 'time.schedule.planning.editshift.fillholes',
        }
      );
      return mb.afterClosed().pipe(
        map((response: IMessageboxComponentResponse) => {
          if (response.result === true) {
            // Save with holes
            return { passed: true, hasHole: true };
          } else if (response.result === false) {
            // Fill holes with breaks
            return this.fillHolesWithBreaks(scheduleShifts, breakShifts);
          } else {
            // Abort
            return { passed: false, hasHole: true };
          }
        })
      );
    } else {
      // Holes are not allowed, ask user to fill holes with breaks
      const mb = this.messageboxService.question(
        'core.warning',
        'time.schedule.planning.editshift.askfillholeswithbreaks',
        {
          customIconName: 'ban',
          iconClass: 'warning-color',
          buttonYesLabelKey: 'time.schedule.planning.editshift.fillholes',
          buttonNoLabelKey: 'core.cancel',
        }
      );
      return mb.afterClosed().pipe(
        map((response: IMessageboxComponentResponse) => {
          if (response.result === true) {
            // Fill holes with breaks
            return this.fillHolesWithBreaks(scheduleShifts, breakShifts);
          } else {
            // Abort
            return { passed: false, hasHole: true };
          }
        })
      );
    }
  }

  private hasHolesWithoutBreaks(
    scheduleShifts: PlanningShiftDTO[],
    breakShifts: PlanningShiftBreakDTO[]
  ): boolean {
    let hasHoleWithoutBreak = false;

    let prevShift: PlanningShiftDTO | undefined = undefined;

    scheduleShifts.forEach(shift => {
      // Check for holes
      if (prevShift?.actualStopTime.isBeforeOnMinute(shift.actualStartTime)) {
        // A hole is found, check if a break is within the hole
        if (
          breakShifts.filter(
            s =>
              (s.startTime.isSameOrAfterOnMinute(prevShift!.actualStopTime) &&
                s.startTime.isBeforeOnMinute(shift.actualStartTime)) ||
              (s.stopTime.isAfterOnMinute(prevShift!.actualStopTime) &&
                s.stopTime.isSameOrBeforeOnMinute(shift.actualStartTime))
          ).length === 0
        ) {
          hasHoleWithoutBreak = true;
        }
      }
      prevShift = shift;
    });

    return hasHoleWithoutBreak;
  }

  private fillHolesWithBreaks(
    scheduleShifts: PlanningShiftDTO[],
    breakShifts: PlanningShiftBreakDTO[]
  ): validateHolesResult {
    const result: validateHolesResult = { passed: false, adjustedShifts: [] };

    let prevShift: PlanningShiftDTO | undefined = undefined;

    const dates: Date[] = this.getUniqueDates(scheduleShifts);
    dates.forEach((date: Date) => {
      const dayShifts = scheduleShifts.filter(
        s => s.startTime.isSameDay(date) && !s.isStandby && !s.isOnDuty
      );
      prevShift = undefined;
      dayShifts.forEach(shift => {
        // Check for holes
        if (prevShift?.actualStopTime.isBeforeOnMinute(shift.actualStartTime)) {
          // A hole found, check if a break is within the hole
          if (
            breakShifts.filter(
              s =>
                (s.startTime.isSameOrAfterOnMinute(prevShift!.actualStopTime) &&
                  s.startTime.isBeforeOnMinute(shift.actualStartTime)) ||
                (s.stopTime.isAfterOnMinute(prevShift!.actualStopTime) &&
                  s.stopTime.isSameOrBeforeOnMinute(shift.actualStartTime))
            ).length === 0
          ) {
            // Create break inside hole
            const brk: PlanningShiftBreakDTO = new PlanningShiftBreakDTO();
            brk.startTime = prevShift.actualStopTime;
            brk.stopTime = shift.actualStartTime;
            brk.minutes = brk.stopTime.diffMinutes(brk.startTime);

            result.hasHole = true;
            result.adjustedShifts!.push({
              shift: prevShift,
              newStopTime: shift.actualStartTime,
              addedBreaks: [brk],
            });
          }
        }
        prevShift = shift;
      });
    });

    result.passed = true;
    return result;
  }

  // private validateSkills(): ng.IPromise<boolean> {
  //     let deferral = this.$q.defer<boolean>();

  //     if (!this.isHiddenOrVacant && this.invalidSkills) {
  //         let message = this.terms["time.schedule.planning.editshift.missingskills"].format(this.shiftUndefined);
  //         if (!this.skillCantBeOverridden)
  //             message += "\n" + this.terms["time.schedule.planning.editshift.missingskillsoverride"];

  //         this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Forbidden, this.skillCantBeOverridden ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.OKCancel).result.then(val => {
  //             deferral.resolve(val && !this.skillCantBeOverridden);
  //         }, (reason) => {
  //             deferral.resolve(false);
  //         });
  //     } else
  //         deferral.resolve(true);

  //     return deferral.promise;
  // }

  // private validateOnDutyShifts(): ng.IPromise<boolean> {
  //     let deferral = this.$q.defer<boolean>();

  //     let shiftsOutside = false;

  //     if (this.isHidden) {
  //         deferral.resolve(true);
  //     } else {
  //         let onDutyShifts = _.filter(this.shifts, s => s.isOnDuty);
  //         if (onDutyShifts.length === 0) {
  //             // No on duty shifts
  //             deferral.resolve(true);
  //         } else {
  //             const firstShift = this.getFirstShift();
  //             const lastShift = this.getLastShift();
  //             if (!firstShift || !lastShift) {
  //                 // No regular shifts
  //                 shiftsOutside = true;
  //             } else {
  //                 _.forEach(onDutyShifts, s => {
  //                     if (s.actualStartTime.isBeforeOnMinute(firstShift.actualStartTime) || s.actualStopTime.isAfterOnMinute(lastShift.actualStopTime)) {
  //                         shiftsOutside = true;
  //                         return false;
  //                     }
  //                 });
  //             }

  //             if (shiftsOutside) {
  //                 this.notificationService.showDialog(this.terms["common.obs"], this.terms["time.schedule.planning.editshift.ondutyoutsideschedule"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OKCancel).result.then(val => {
  //                     deferral.resolve(true);
  //                 }, (reason) => {
  //                     deferral.resolve(false);
  //                 });
  //             } else {
  //                 deferral.resolve(true);
  //             }
  //         }
  //     }

  //     return deferral.promise;
  // }

  validateWorkRules(
    employeeId: number,
    shifts: PlanningShiftDTO[]
  ): Observable<IEvaluateWorkRulesActionResult> {
    let rules: SoeScheduleWorkRules[] | null = null;
    if (this.settingService.skipWorkRules()) {
      // The following rules should always be evaluated
      rules = [];
      rules.push(SoeScheduleWorkRules.OverlappingShifts);
      if (!this.filterService.isTemplateView())
        rules.push(SoeScheduleWorkRules.AttestedDay);
    }

    // Set times used for evaluation
    shifts.forEach(s => {
      s.setTimesForSave();
    });

    return this.workRuleService
      .evaluatePlannedShiftsAgainstWorkRules(
        shifts,
        rules,
        employeeId,
        false,
        undefined,
        undefined,
        undefined
      )
      .pipe(
        finalize(() => {
          // Reset the times used for evaluation after it has completed
          shifts.forEach(s => s.resetTimesForSave());
        })
      );
  }

  private allowShiftsWithoutAccount(employeeId: number, date: Date): boolean {
    let allow = false;

    if (this.settingService.useAccountHierarchy()) {
      // Check setting on employment (employee group)
      if (employeeId && date) {
        const employee = this.employeeService.getEmployee(employeeId);
        if (employee) {
          const employment = employee.getEmployment(date, date);
          if (employment) {
            allow = employment.allowShiftsWithoutAccount;
          }
        }
      }
    } else {
      allow = true;
    }

    return allow;
  }

  private getUniqueDates(shifts: PlanningShiftDTO[]): Date[] {
    const daySet = new Set<string>();
    const uniqueDates: Date[] = [];

    shifts.forEach(shift => {
      const dayStr = shift.startTime.beginningOfDay().toISOString();
      if (!daySet.has(dayStr)) {
        daySet.add(dayStr);
        uniqueDates.push(shift.startTime.beginningOfDay());
      }
    });

    return uniqueDates;
  }
}
