import { computed, effect, inject, Injectable, signal } from '@angular/core';
import {
  TermGroup,
  TermGroup_TimeSchedulePlanningDayViewGroupBy,
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningScheduleViewGroupBy,
  TermGroup_TimeSchedulePlanningScheduleViewSortBy,
  TermGroup_TimeSchedulePlanningViews,
  TermGroup_TimeSchedulePlanningVisibleDays,
  TermGroup_TimeScheduleTemplateBlockType,
  TimeSchedulePlanningMode,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { SpSettingService } from './sp-setting.service';
import { ShiftTypeService } from '@shared/features/shift-type/services/shift-type.service';
import {
  IShiftTypeDTO,
  ITimeScheduleTypeSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TimeScheduleTypeService } from '@features/time/time-schedule-type/services/time-schedule-type.service';
import { TranslateService } from '@ngx-translate/core';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { Constants } from '@shared/util/client-constants';
import { DateUtil } from '@shared/util/date-util';

export type ViewDefinitionChangedValue = {
  from: TermGroup_TimeSchedulePlanningViews;
  to: TermGroup_TimeSchedulePlanningViews;
  targetDate?: Date;
};

export type NbrOfDaysChangedValue = {
  from: number;
  to: number;
  isCustom: boolean;
};
export type DateRangeChangedValue = {
  from: Date;
  to: Date;
  reloadShifts: boolean;
};

@Injectable({
  providedIn: 'root',
})
export class SpFilterService {
  coreService = inject(CoreService);
  settingService = inject(SpSettingService);
  shiftTypeService = inject(ShiftTypeService);
  timeScheduleTypeService = inject(TimeScheduleTypeService);
  translate = inject(TranslateService);

  // DATE & TIMES
  dateFrom = signal<Date>(new Date());
  dateTo = signal<Date>(new Date());
  nbrOfDays = computed(() => {
    if (
      this.isCustomSelected() ||
      this.isMonthSelected() ||
      this.isYearSelected()
    )
      return (
        this.dateTo()
          .beginningOfDay()
          .diffDays(this.dateFrom().beginningOfDay()) + 1
      );
    else return <number>this.nbrOfDaysSelector();
  });

  nbrOfWeeks = computed(() => {
    return this.nbrOfDays() / 7;
  });

  nbrOfHours = computed(() => {
    return (
      (this.settingService.dayViewEndTime() -
        this.settingService.dayViewStartTime()) /
      60
    );
  });

  get startHour(): number {
    let hour = this.settingService.dayViewStartTime() / 60;
    if (this.isBecomingDST) hour--;
    else if (this.isLeavingDST) hour++;

    return hour;
  }

  get endHour(): number {
    let hour = this.settingService.dayViewEndTime() / 60;
    if (this.isBecomingDST) hour--;
    else if (this.isLeavingDST) hour++;

    return hour;
  }

  private get isBecomingDST(): boolean {
    return false;
    // TODO: Implement DST with date-fns-tz
    // return (
    //   !this.dateFrom().beginningOfDay().isDST() &&
    //   this.dateFrom().beginningOfDay().addMinutes(this.settingService.dayViewStartTime()).isDST()
    // );
  }

  private get isLeavingDST(): boolean {
    return false;
    // TODO: Implement DST with date-fns-tz
    // return (
    //   this.dateFrom().beginningOfDay().isDST() &&
    //   !this.dateFrom().beginningOfDay().addMinutes(this.settingService.dayViewStartTime()).isDST()
    // );
  }

  nbrOfDaysSelector = signal<TermGroup_TimeSchedulePlanningVisibleDays>(
    TermGroup_TimeSchedulePlanningVisibleDays.Week
  );
  nbrOfDaysChanged = new BehaviorSubject<NbrOfDaysChangedValue | undefined>(
    undefined
  );
  dateRangeChanged = new BehaviorSubject<DateRangeChangedValue | undefined>(
    undefined
  );

  isCustomSelected = computed(() => {
    return (
      this.nbrOfDaysSelector() ===
      TermGroup_TimeSchedulePlanningVisibleDays.Custom
    );
  });
  isWorkWeekSelected = computed(() => {
    return (
      this.nbrOfDaysSelector() ===
      TermGroup_TimeSchedulePlanningVisibleDays.WorkWeek
    );
  });
  isMonthSelected = computed(() => {
    return (
      this.nbrOfDaysSelector() ===
      TermGroup_TimeSchedulePlanningVisibleDays.Month
    );
  });
  isYearSelected = computed(() => {
    return (
      this.nbrOfDaysSelector() ===
      TermGroup_TimeSchedulePlanningVisibleDays.Year
    );
  });

  visibleDays = signal<ISmallGenericType[]>([]);

  // MODES
  planningMode = signal<TimeSchedulePlanningMode>(
    TimeSchedulePlanningMode.SchedulePlanning
  );
  isSchedulePlanningMode = computed(() => {
    return this.planningMode() === TimeSchedulePlanningMode.SchedulePlanning;
  });
  isOrderPlanningMode = computed(() => {
    return this.planningMode() === TimeSchedulePlanningMode.OrderPlanning;
  });

  // VIEWS
  viewDefinition = signal<TermGroup_TimeSchedulePlanningViews>(
    TermGroup_TimeSchedulePlanningViews.Schedule
  );
  isDayView = computed(() => {
    return this.viewDefinition() === TermGroup_TimeSchedulePlanningViews.Day;
  });
  isScheduleView = computed(() => {
    return (
      this.viewDefinition() === TermGroup_TimeSchedulePlanningViews.Schedule
    );
  });
  isTemplateDayView = computed(() => {
    return (
      this.viewDefinition() === TermGroup_TimeSchedulePlanningViews.TemplateDay
    );
  });
  isTemplateScheduleView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.TemplateSchedule
    );
  });
  isTemplateView = computed(() => {
    return this.isTemplateDayView() || this.isTemplateScheduleView();
  });
  isEmployeePostDayView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.EmployeePostsDay
    );
  });
  isEmployeePostScheduleView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule
    );
  });
  isEmployeePostView = computed(() => {
    return this.isEmployeePostDayView() || this.isEmployeePostScheduleView();
  });
  isScenarioDayView = computed(() => {
    return (
      this.viewDefinition() === TermGroup_TimeSchedulePlanningViews.ScenarioDay
    );
  });
  isScenarioScheduleView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.ScenarioSchedule
    );
  });
  isScenarioCompleteView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.ScenarioComplete
    );
  });
  isScenarioView = computed(() => {
    return (
      this.isScenarioDayView() ||
      this.isScenarioScheduleView() ||
      this.isScenarioCompleteView()
    );
  });
  isStandbyDayView = computed(() => {
    return (
      this.viewDefinition() === TermGroup_TimeSchedulePlanningViews.StandbyDay
    );
  });
  isStandbyScheduleView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.StandbySchedule
    );
  });
  isStandbyView = computed(() => {
    return this.isStandbyDayView() || this.isStandbyScheduleView();
  });
  isTasksAndDeliveriesDayView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay
    );
  });
  isTasksAndDeliveriesScheduleView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule
    );
  });
  isTasksAndDeliveriesView = computed(() => {
    return (
      this.isTasksAndDeliveriesDayView() ||
      this.isTasksAndDeliveriesScheduleView()
    );
  });
  isStaffingNeedsDayView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay
    );
  });
  isStaffingNeedsScheduleView = computed(() => {
    return (
      this.viewDefinition() ===
      TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule
    );
  });
  isStaffingNeedsView = computed(() => {
    return this.isStaffingNeedsDayView() || this.isStaffingNeedsScheduleView();
  });
  isCommonDayView = computed(() => {
    return (
      this.isDayView() ||
      this.isTemplateDayView() ||
      this.isEmployeePostDayView() ||
      this.isScenarioDayView() ||
      this.isStandbyDayView() ||
      this.isTasksAndDeliveriesDayView() ||
      this.isStaffingNeedsDayView()
    );
  });
  isCommonScheduleView = computed(() => {
    return (
      this.isScheduleView() ||
      this.isTemplateScheduleView() ||
      this.isEmployeePostScheduleView() ||
      this.isScenarioScheduleView() ||
      this.isScenarioCompleteView() ||
      this.isStandbyScheduleView() ||
      this.isTasksAndDeliveriesScheduleView() ||
      this.isStaffingNeedsScheduleView()
    );
  });

  viewDefinitionChanged = new BehaviorSubject<
    ViewDefinitionChangedValue | undefined
  >(undefined);

  scheduleViewInterval = signal(TermGroup_TimeSchedulePlanningVisibleDays.Week);
  scheduleViewIntervalChanged =
    new BehaviorSubject<TermGroup_TimeSchedulePlanningVisibleDays>(
      this.scheduleViewInterval()
    );

  // SHOW FLAGS
  showAllEmployees = signal(false);
  showSecondaryCategories = signal(false);
  showHiddenShifts = signal(false);

  // SORTING
  sortEmployeesByForDayView =
    signal<TermGroup_TimeSchedulePlanningDayViewSortBy>(
      TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime
    );
  sortEmployeesByForScheduleView =
    signal<TermGroup_TimeSchedulePlanningScheduleViewSortBy>(
      TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr
    );

  sortEmployeesBy = computed<
    | TermGroup_TimeSchedulePlanningDayViewSortBy
    | TermGroup_TimeSchedulePlanningScheduleViewSortBy
  >(() => {
    return this.isCommonDayView()
      ? this.sortEmployeesByForDayView()
      : this.sortEmployeesByForScheduleView();
  });

  accountIds = signal<number[]>([]);
  employeeIds = signal<number[]>([]);
  shiftTypeIds = signal<number[]>([]);
  blockTypes = signal<TermGroup_TimeScheduleTemplateBlockType[]>([]);

  isFilteredOnAccountDim = computed(() => {
    // TODO: Account hierarchy not implemented
    return false;
  });

  isFilteredOnEmployee = computed(() => {
    return this.employeeIds().length > 0;
  });

  isFilteredOnShiftType = computed(() => {
    return this.shiftTypeIds().length > 0;
  });

  isFilteredOnBlockType = computed(() => {
    return this.blockTypes().length > 0;
  });

  isFiltered = computed(() => {
    return (
      this.isFilteredOnEmployee() ||
      this.isFilteredOnShiftType() ||
      this.isFilteredOnBlockType()
    );
  });

  isFilteredChanged = new BehaviorSubject<boolean>(false);

  // GROUPING

  isGrouped = signal(false);
  isGroupedByAccount = signal(false);
  isGroupedByCategory = signal(false);
  isGroupedByShiftType = signal(false);

  groupEmployeesByForDayView =
    signal<TermGroup_TimeSchedulePlanningDayViewGroupBy>(
      TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee
    );

  groupEmployeesByForScheduleView =
    signal<TermGroup_TimeSchedulePlanningScheduleViewGroupBy>(
      TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee
    );

  // public get isGrouped(): boolean {
  //   return (
  //     this.isGroupedByAccount ||
  //     this.isGroupedByCategory ||
  //     this.isGroupedByShiftType
  //   );
  // }
  // public get isGroupedByAccount(): boolean {
  //   return (
  //     (this.isCommonDayView && this.dayViewGroupBy > 10) ||
  //     (this.isCommonScheduleView && this.scheduleViewGroupBy > 10)
  //   );
  // }
  // public get isGroupedByCategory(): boolean {
  //   return (
  //     (this.isCommonDayView &&
  //       this.dayViewGroupBy ===
  //         TermGroup_TimeSchedulePlanningDayViewGroupBy.Category) ||
  //     (this.isCommonScheduleView &&
  //       this.scheduleViewGroupBy ===
  //         TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Category)
  //   );
  // }
  // public get isGroupedByShiftType(): boolean {
  //   return (
  //     (this.isCommonDayView &&
  //       this.dayViewGroupBy ===
  //         TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType) ||
  //     (this.isCommonScheduleView &&
  //       this.scheduleViewGroupBy ===
  //         TermGroup_TimeSchedulePlanningScheduleViewGroupBy.ShiftType)
  //   );
  // }

  // LOOKUPS

  employeesDict: SmallGenericType[] = [];
  blockTypesDict: SmallGenericType[] = [];
  blockTypeItems: MenuButtonItem[] = [];
  shiftTypes: IShiftTypeDTO[] = [];
  shiftTypesDict: SmallGenericType[] = [];
  timeScheduleTypes: ITimeScheduleTypeSmallDTO[] = [];
  timeScheduleTypesDict: SmallGenericType[] = [];

  // INIT

  constructor() {
    this.settingService.dayViewDefaultSortByChanged.subscribe(
      (value: TermGroup_TimeSchedulePlanningDayViewSortBy) => {
        this.sortEmployeesByForDayView.set(value);
      }
    );
    this.settingService.scheduleViewDefaultSortByChanged.subscribe(
      (value: TermGroup_TimeSchedulePlanningScheduleViewSortBy) => {
        this.sortEmployeesByForScheduleView.set(value);
      }
    );

    effect(() => {
      // Detect changes in filters and notify subscribers.
      // For some reason, the effect is called twice when the signal is updated,
      // so we need to check if the value has actually changed before passing it on.
      if (this.isFilteredChanged.value !== this.isFiltered())
        this.isFilteredChanged.next(this.isFiltered());
    });

    // TODO: Should this be here or moved higher up so we know what is loaded?
    this.loadShiftTypes().subscribe();
    this.loadTimeScheduleTypes().subscribe();
  }

  setupBlockTypes() {
    this.blockTypesDict = [];
    this.blockTypeItems = [];

    // Schedule
    this.addBlockType(
      TermGroup_TimeScheduleTemplateBlockType.Schedule,
      'schedule'
    );

    // Order
    // if (this.isOrderPlanningMode()) {
    //   this.addBlockType(TermGroup_TimeScheduleTemplateBlockType.Order, 'order');
    // }

    // Booking
    if (this.settingService.bookingReadPermission()) {
      this.addBlockType(
        TermGroup_TimeScheduleTemplateBlockType.Booking,
        'booking'
      );
    }

    // Standby
    if (
      this.settingService.standbyShiftsReadPermission() &&
      !this.isOrderPlanningMode()
    ) {
      this.addBlockType(
        TermGroup_TimeScheduleTemplateBlockType.Standby,
        'standby'
      );
    }

    // On Duty
    if (
      this.settingService.onDutyShiftsReadPermission() &&
      !this.isOrderPlanningMode()
    ) {
      this.addBlockType(
        TermGroup_TimeScheduleTemplateBlockType.OnDuty,
        'onduty'
      );
    }
  }

  private addBlockType(
    type: TermGroup_TimeScheduleTemplateBlockType,
    labelKey: string
  ) {
    const label = this.translate.instant(
      `time.schedule.planning.blocktype.${labelKey}`
    );
    this.blockTypesDict.push({
      id: type,
      name: label,
    });
    this.blockTypeItems.push({
      id: type,
      label: label,
      icon: ['fas', 'square'],
      iconClass: `${labelKey.toLowerCase()}-color`,
    });
  }

  setViewDefinition(
    view: TermGroup_TimeSchedulePlanningViews,
    targetDate?: Date,
    forceSet = false
  ) {
    const from = this.viewDefinition();
    if (!forceSet && from === view) return;

    // if (targetDate) {
    //   this.dateFrom.set(targetDate.beginningOfDay());
    //   this.dateTo.set(targetDate.endOfDay());
    // }

    this.viewDefinition.set(view);
    this.settingService.loadSelectableInformationSettings(view).subscribe();
    this.viewDefinitionChanged.next({
      from: from,
      to: view,
      targetDate: targetDate,
    });
  }

  setScheduleViewInterval(interval: TermGroup_TimeSchedulePlanningVisibleDays) {
    this.scheduleViewInterval.set(interval);
    this.scheduleViewIntervalChanged.next(interval);
  }

  loadVisibleDays(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeSchedulePlanningVisibleDays,
        false,
        false,
        true
      )
      .pipe(
        tap(data => {
          // Filter out values over six weeks except for year
          data = data.filter(d => d.id <= 42 || d.id === 365);
          this.visibleDays.set(data);
        })
      );
  }

  setSelectedNbrOfDays(days: number) {
    const prevDays = this.nbrOfDays();
    if (prevDays === days) return;

    let dateFrom = this.dateFrom();
    let dateTo = this.dateTo();

    if (days === TermGroup_TimeSchedulePlanningVisibleDays.Month) {
      // Month view (current month)
      dateFrom = DateUtil.getDateFirstInMonth(new Date());
      dateTo = DateUtil.getDateLastInMonth(dateFrom);
    } else if (days === TermGroup_TimeSchedulePlanningVisibleDays.WorkWeek) {
      // Work week (Mon-Fri) - set dateFrom to Monday of selected week
      dateFrom = DateUtil.getDateFirstInWeek(dateFrom);
      dateTo = dateFrom.addDays(4);
    } else {
      // If switching from year, set start to first day of current week
      if (
        this.isYearSelected() &&
        days !== TermGroup_TimeSchedulePlanningVisibleDays.Year
      ) {
        dateFrom = new Date().beginningOfWeek();
      }

      dateTo = dateFrom.addDays(days - 1);
    }

    const isCustom = !Object.values(
      TermGroup_TimeSchedulePlanningVisibleDays
    ).includes(days);

    // Number of days was changed, update the selector
    if (isCustom) {
      this.nbrOfDaysSelector.set(
        DateUtil.isFullMonth(dateFrom, dateTo)
          ? TermGroup_TimeSchedulePlanningVisibleDays.Month
          : TermGroup_TimeSchedulePlanningVisibleDays.Custom
      );
    } else {
      this.nbrOfDaysSelector.set(days);
    }

    this.nbrOfDaysChanged.next({
      from: prevDays,
      to: days,
      isCustom: isCustom,
    });

    this.setDateRange('setSelectedNbrOfDays', dateFrom, dateTo, true);
  }

  setDateRange(source: string, from: Date, to: Date, reloadShifts: boolean) {
    const prevNbrOfDays = this.nbrOfDays();
    let dateChanged = false;

    if (this.isCommonDayView()) {
      if (this.nbrOfHours() > 0) {
        const fromWithStartHour = from
          .beginningOfDay()
          .addHours(this.startHour);

        if (!this.dateFrom().isSameHourAs(fromWithStartHour)) {
          this.dateFrom.set(fromWithStartHour);
          dateChanged = true;
        }

        const toWithEndHour = this.dateFrom()
          .addHours(this.nbrOfHours())
          .addSeconds(-1)
          .endOfHour();

        if (!this.dateTo().isSameHourAs(toWithEndHour)) {
          this.dateTo.set(toWithEndHour);
          dateChanged = true;
        }
      }
    } else if (this.isCommonScheduleView()) {
      if (!from.isSameDay(this.dateFrom())) {
        this.dateFrom.set(from.beginningOfDay());
        dateChanged = true;
      }
      if (!to.isSameDay(this.dateTo())) {
        this.dateTo.set(to.endOfDay());
        dateChanged = true;
      }

      if (dateChanged && !this.isMonthSelected()) {
        const days =
          this.dateTo()
            .beginningOfDay()
            .diffDays(this.dateFrom().beginningOfDay()) + 1;

        if (days !== prevNbrOfDays) {
          // Number of days was changed, update the selector
          this.setSelectedNbrOfDays(days);
        }
      }
    }

    if (dateChanged) {
      this.dateRangeChanged.next({
        from: this.dateFrom(),
        to: this.dateTo(),
        reloadShifts: reloadShifts,
      });
    }
  }

  loadShiftTypes(): Observable<IShiftTypeDTO[]> {
    return this.shiftTypeService
      .getShiftTypes(
        false,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        this.settingService.useAccountHierarchy()
      )
      .pipe(
        tap(data => {
          // Insert empty shift type
          const shiftType: Partial<IShiftTypeDTO> = {
            shiftTypeId: 0,
            name: this.translate.instant('core.notselected'),
            color: Constants.SHIFT_TYPE_UNSPECIFIED_COLOR,
          };
          data.splice(0, 0, shiftType as IShiftTypeDTO);

          this.shiftTypes = data;
          this.shiftTypesDict = data.map(x => ({
            id: x.shiftTypeId,
            name: x.name,
          }));
        })
      );
  }

  loadTimeScheduleTypes(): Observable<ITimeScheduleTypeSmallDTO[]> {
    return this.timeScheduleTypeService
      .getTimeScheduleTypesSmall(true, true, true)
      .pipe(
        tap(data => {
          this.timeScheduleTypes = data;
          this.timeScheduleTypesDict = data.map(x => ({
            id: x.timeScheduleTypeId,
            name: x.name,
          }));
        })
      );
  }
}
