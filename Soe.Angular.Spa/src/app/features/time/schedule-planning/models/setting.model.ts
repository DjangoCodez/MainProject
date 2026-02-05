import {
  TermGroup_StaffingNeedsHeadInterval,
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningScheduleViewSortBy,
  TermGroup_TimeSchedulePlanningViews,
  TermGroup_TimeSchedulePlanningVisibleDays,
} from '@shared/models/generated-interfaces/Enumerations';

export class SchedulePlanningSetting {
  // Company settings
  dayViewStartTime = 0;
  dayViewEndTime = 0;
  dayViewMinorTickLength = TermGroup_StaffingNeedsHeadInterval.FifteenMinutes;
  shiftRequestPreventTooEarly = false;
  shiftRequestPreventTooEarlyWarnHoursBefore = 0;
  shiftRequestPreventTooEarlyStopHoursBefore = 0;

  // User settings
  defaultView = TermGroup_TimeSchedulePlanningViews.Schedule;
  defaultInterval = TermGroup_TimeSchedulePlanningVisibleDays.Week;
  dayViewDefaultSortBy = TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime;
  scheduleViewDefaultSortBy =
    TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr;
  startWeek = 0;
  disableAutoLoad = true;

  // Selectable information settings
  showEmployeeGroup = false;
  showCyclePlannedTime = false;
  showScheduleTypeFactorTime = false;
  showGrossTime = false;
  showTotalCost = false;
  showTotalCostIncEmpTaxAndSuppCharge = false;
  showAvailability = false;
  skipXEMailOnChanges = false;
  skipWorkRules = false;

  summaryInFooter = false;
}

export class SchedulePlanningSettingsDTO {
  // doNotSearchOnFilter: boolean;
  // showHiddenShifts: boolean;
  showInactiveEmployees = false;
  showUnemployedEmployees = false;
  // showFullyLendedEmployees: boolean;
  defaultUserSelectionId?: number;

  showEmployeeGroup = false;

  showCyclePlannedTime = false;
  showScheduleTypeFactorTime = false;
  showGrossTime = false;
  showTotalCost = false;
  showTotalCostIncEmpTaxAndSuppCharge = false;
  // showWeekendSalary: boolean;
  // showPlanningPeriodSummary: boolean;
  // planningPeriodHeadId: number;

  // useShiftTypeCode: boolean;
  // showWeekNumber: boolean;
  // shiftTypePosition: TermGroup_TimeSchedulePlanningShiftTypePosition;
  // timePosition: TermGroup_TimeSchedulePlanningTimePosition;
  // hideTimeOnShiftShorterThanMinutes: number;
  // breakVisibility: TermGroup_TimeSchedulePlanningBreakVisibility;

  showAvailability = false;

  // showAbsenceRequests: boolean;

  skipXEMailOnChanges = false;
  skipWorkRules = false;

  summaryInFooter = false;
}
