import { computed, inject, Injectable, signal } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  SettingMainType,
  TermGroup,
  TermGroup_OrderPlanningShiftInfo,
  TermGroup_StaffingNeedsDayViewGroupBy,
  TermGroup_StaffingNeedsDayViewSortBy,
  TermGroup_StaffingNeedsHeadInterval,
  TermGroup_StaffingNeedsScheduleViewGroupBy,
  TermGroup_TimeSchedulePlanningDayViewGroupBy,
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningScheduleViewGroupBy,
  TermGroup_TimeSchedulePlanningScheduleViewSortBy,
  TermGroup_TimeSchedulePlanningShiftStyle,
  TermGroup_TimeSchedulePlanningViews,
  TermGroup_TimeSchedulePlanningVisibleDays,
  UserSelectionType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISaveUserCompanySettingModel } from '@shared/models/generated-interfaces/TimeModels';
import { CoreService } from '@shared/services/core.service';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { BehaviorSubject, forkJoin, Observable, of, tap } from 'rxjs';
import { SchedulePlanningSettingsDTO } from '../models/setting.model';
import { TranslateService } from '@ngx-translate/core';
import { TermCollection } from '@shared/localization/term-types';
import { SchedulePlanningService } from './schedule-planning.service';
import {
  ITimeCodeBreakSmallDTO,
  IUserSelectionDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { UserSelectionService } from '@shared/components/user-selection/services/user-selection.service';

@Injectable({
  providedIn: 'root',
})
export class SpSettingService {
  service = inject(SchedulePlanningService);
  coreService = inject(CoreService);
  translate = inject(TranslateService);
  userSelectionService = inject(UserSelectionService);

  settingsLoaded = new BehaviorSubject<boolean>(false);

  // PERMISSIONS
  // Read
  bookingReadPermission = signal(false);
  standbyShiftsReadPermission = signal(false);
  onDutyShiftsReadPermission = signal(false);
  showCostReadPermission = signal(false);
  // Modify
  onDutyShiftsModifyPermission = signal(false);
  savePublicSelectionPermission = signal(false);

  // COMPANY SETTINGS

  dayViewStartTime = signal(0); // Minutes from midnight
  dayViewEndTime = signal(0); // Minutes from midnight
  originalDayViewStartTime = 0;
  originalDayViewEndTime = 0;
  useAccountHierarchy = signal(false);
  defaultEmployeeAccountDimIdEmployee = signal(0);
  useLeisureCodes = signal(false);

  useVacant = signal(false);
  skillCantBeOverridden = signal(false);
  // showSummaryInCalendarView = signal(false);
  // originalDayViewStartTime = signal(0);
  // originalDayViewEndTime = signal(0);
  dayViewMinorTickLength = signal(
    TermGroup_StaffingNeedsHeadInterval.FifteenMinutes
  );

  // clockRounding = signal(0);
  shiftTypeMandatory = signal(false);
  allowHolesWithoutBreaks = signal(false);
  // keepShiftsTogether = signal(false);
  sendXEMailOnChange = signal(false);
  // possibleToSkipWorkRules = signal(false);
  // dayScheduleReportId = signal(0);
  // weekScheduleReportId = signal(0);
  // dayTemplateScheduleReportId = signal(0);
  // weekTemplateScheduleReportId = signal(0);
  // dayEmployeePostTemplateScheduleReportId = signal(0);
  // weekEmployeePostTemplateScheduleReportId = signal(0);
  // dayScenarioScheduleReportId = signal(0);
  // weekScenarioScheduleReportId = signal(0);
  // employmentContractShortSubstituteReportId = signal(0);
  // employmentContractShortSubstituteReportName = signal('');
  // tasksAndDeliveriesDayReportId = signal(0);
  // tasksAndDeliveriesWeekReportId = signal(0);
  // hasEmployeeTemplates = signal(false);
  maxNbrOfBreaks = signal(1);
  // useTemplateScheduleStopDate = signal(false);
  // placementDefaultPreliminary = signal(false);
  // placementHidePreliminary = signal(false);
  // calculatePlanningPeriodScheduledTime = signal(false);
  // calculatePlanningPeriodScheduledTimeUseAveragingPeriod = signal(false);
  // planningPeriodColors = signal(['da1e28', '24a148', '0565c9']); // @soe-color-semantic-error, @soe-color-semantic-success, @soe-color-semantic-information
  // planningPeriodColorOver = computed(() => {
  //   return `#${this.planningPeriodColors()[0]}`;
  // });
  // planningPeriodColorEqual = computed(() => {
  //   return `#${this.planningPeriodColors()[1]}`;
  // });
  // planningPeriodColorUnder = computed(() => {
  //   return `#${this.planningPeriodColors()[2]}`;
  // });
  showGrossTimeSetting = signal(false);
  // showExtraShift = signal(false);
  // showSubstitute = signal(false);
  useMultipleScheduleTypes = signal(false);
  // orderPlanningIgnoreScheduledBreaksOnAssignment = signal(false);
  shiftRequestPreventTooEarly = signal(false);
  shiftRequestPreventTooEarlyWarnHoursBefore = signal(0);
  shiftRequestPreventTooEarlyStopHoursBefore = signal(0);
  // inactivateLending = signal(false);
  // extraShiftAsDefaultOnHidden = signal(false);

  isDayViewTickLengthHour = computed(() => {
    return (
      this.dayViewMinorTickLength() ===
      TermGroup_StaffingNeedsHeadInterval.SixtyMinutes
    );
  });

  isDayViewTickLengthHalfHour = computed(() => {
    return (
      this.dayViewMinorTickLength() ===
      TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes
    );
  });

  isDayViewTickLengthQuarterHour = computed(() => {
    return (
      this.dayViewMinorTickLength() ===
      TermGroup_StaffingNeedsHeadInterval.FifteenMinutes
    );
  });

  // USER SETTINGS

  defaultView = signal(TermGroup_TimeSchedulePlanningViews.Schedule);
  defaultInterval = signal(TermGroup_TimeSchedulePlanningVisibleDays.Week);

  dayViewDefaultSortBy = signal(
    TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime
  );
  dayViewDefaultSortByChanged =
    new BehaviorSubject<TermGroup_TimeSchedulePlanningDayViewSortBy>(
      this.dayViewDefaultSortBy()
    );
  scheduleViewDefaultSortBy = signal(
    TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr
  );
  scheduleViewDefaultSortByChanged =
    new BehaviorSubject<TermGroup_TimeSchedulePlanningScheduleViewSortBy>(
      this.scheduleViewDefaultSortBy()
    );

  disableAutoLoad = signal(true);
  disableAutoLoadChanged = new BehaviorSubject<boolean>(this.disableAutoLoad());

  disableBreaksWithinHolesWarning = signal(false);

  // SELECTABLE INFORMATION SETTINGS

  // This object will keep the loaded settings until the user saves them.
  // The reason for this is that all settings are not migrated yet.
  // So if we would only save the settings we have as signals, we would lose the settings that are not migrated yet
  selectableInformationSettings?: SchedulePlanningSettingsDTO;

  // TODO: Add GUI for showInactiveEmployees and showUnemployedEmployees
  // TODO: Make sure changes of the settings are reloading relevant things (AngularJS)
  showInactiveEmployees = signal(false);
  showUnemployedEmployees = signal(false);
  defaultUserSelectionId = signal<number>(0);
  showEmployeeGroup = signal(false);
  showCyclePlannedTime = signal(false);
  showScheduleTypeFactorTime = signal(false);
  showGrossTime = signal(false);
  showTotalCost = signal(false);
  showTotalCostIncEmpTaxAndSuppCharge = signal(false);
  showWeekendSalary = signal(false);

  showAvailability = signal(false);
  skipXEMailOnChanges = signal(false);
  skipWorkRules = signal(false);
  summaryInFooter = signal(false);

  // TODO: Hard coded so far, just to be able to use setting in total summary
  showFollowUpOnNeed = signal(true);

  // TEST
  showTimesOnShift = signal(true);
  showRightContent = signal(false);

  //startWeek = signal(0);  // Only in calendar view

  //selectableInformationSettings: SchedulePlanningSettingsDTO;
  // accountHierarchyId = signal('');
  // allAccountsSelected = signal(false);
  // isDefaultAccountDimLevel = signal(false);
  // userAccountId = signal(0);
  //defaultShiftStyle = signal(TermGroup_TimeSchedulePlanningShiftStyle.Detailed);
  // dayViewDefaultGroupBy = signal(
  //   TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee
  // );
  // scheduleViewDefaultGroupBy = signal(
  //   TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee
  // );
  // tadDayViewDefaultGroupBy = signal(TermGroup_StaffingNeedsDayViewGroupBy.None);
  // tadDayViewDefaultSortBy = signal(
  //   TermGroup_StaffingNeedsDayViewSortBy.StartTime
  // );
  // tadScheduleViewDefaultGroupBy = signal(
  //   TermGroup_StaffingNeedsScheduleViewGroupBy.None
  // );
  // orderPlanningShiftInfoTopRight = signal(
  //   TermGroup_OrderPlanningShiftInfo.NoInfo
  // );
  // orderPlanningShiftInfoBottomLeft = signal(
  //   TermGroup_OrderPlanningShiftInfo.NoInfo
  // );
  // orderPlanningShiftInfoBottomRight = signal(
  //   TermGroup_OrderPlanningShiftInfo.NoInfo
  // );

  //hasStaffingByEmployeeAccount = signal(false);

  // calendarViewCountByEmployee = signal(false);
  // defaultShowEmployeeList = signal(false);
  // disableCheckBreakTimesWarning = signal(false);
  // disableBreaksWithinHolesWarning = signal(false);
  // disableSaveAndActivateCheck = signal(false);
  // autoSaveAndActivate = signal(false);
  // disableTemplateScheduleWarning = signal(false);
  // doNotShowTemplateScheduleWarningAgain = signal(false);
  // setInitialHiddenEmployeeFilter = signal(false);
  // firstLoadHasOccurred = signal(false);

  // showShiftTypeSum = signal(false);
  // staffingNeedsDayViewShowDiagram = signal(false);
  // staffingNeedsDayViewShowDetailedSummary = signal(false);
  // staffingNeedsScheduleViewShowDetailedSummary = signal(false);

  // LOOKUPS

  staffingNeedsHeadIntervals: SmallGenericType[] = [];

  defaultViews: SmallGenericType[] = [];
  scheduleIntervals: SmallGenericType[] = [];
  scheduleIntervalsForSetting: SmallGenericType[] = [];
  dayViewSortBys: SmallGenericType[] = [];
  scheduleViewSortBys: SmallGenericType[] = [];

  constructor() {
    forkJoin([
      this.setDefaultViews(),
      this.loadTimeCodeBreaks(),
      this.loadHiddenEmployeeId(),
      this.loadStaffingNeedsHeadIntervals(),
      this.loadScheduleIntervals(),
      this.loadDayViewSortBys(),
      this.loadScheduleViewSortBys(),
      this.loadReadOnlyPermissions(),
      this.loadModifyPermissions(),
      this.loadCompanySettings(),
      this.loadUserSettings(),
    ])
      .pipe(
        tap(() => {
          // Tell other components/services that settings are initially loaded
          this.settingsLoaded.next(true);
        })
      )
      .subscribe();
  }

  private setDefaultViews(): Observable<TermCollection> {
    return this.translate
      .get([
        'time.schedule.planning.viewdefinition.schedule',
        'time.schedule.planning.viewdefinition.day',
      ])
      .pipe(
        tap(terms => {
          this.defaultViews = [
            {
              id: TermGroup_TimeSchedulePlanningViews.Schedule,
              name: terms['time.schedule.planning.viewdefinition.schedule'],
            },
            {
              id: TermGroup_TimeSchedulePlanningViews.Day,
              name: terms['time.schedule.planning.viewdefinition.day'],
            },
          ];
        })
      );
  }

  private loadTimeCodeBreaks(): Observable<ITimeCodeBreakSmallDTO[]> {
    return this.service.getTimeCodeBreaks();
  }

  private loadHiddenEmployeeId(): Observable<number> {
    return this.service.getHiddenEmployeeId();
  }

  private loadStaffingNeedsHeadIntervals(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.StaffingNeedsHeadInterval,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.staffingNeedsHeadIntervals = x.filter(
            y =>
              y.id === TermGroup_StaffingNeedsHeadInterval.FifteenMinutes ||
              y.id === TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes ||
              y.id === TermGroup_StaffingNeedsHeadInterval.SixtyMinutes
          );
        })
      );
  }

  private loadScheduleIntervals(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeSchedulePlanningVisibleDays,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.scheduleIntervals = x;
          this.scheduleIntervalsForSetting = x.filter(
            y =>
              y.id !== TermGroup_TimeSchedulePlanningVisibleDays.Custom &&
              y.id !== TermGroup_TimeSchedulePlanningVisibleDays.Year
          );
        })
      );
  }

  private loadDayViewSortBys(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeSchedulePlanningDayViewSortBy,
        false,
        false,
        false
      )
      .pipe(
        tap(x => {
          // Change sort order
          // Should be fixed, not by id or name
          if (x.length > 0) {
            this.addDayViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr
            );
            this.addDayViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname
            );
            this.addDayViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname
            );
            this.addDayViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime
            );
          }
        })
      );
  }

  private addDayViewSortBy(
    list: SmallGenericType[],
    sortBy: TermGroup_TimeSchedulePlanningDayViewSortBy
  ): void {
    const item = list.find(i => i.id === sortBy);
    if (item) this.dayViewSortBys.push(item);
  }

  private loadScheduleViewSortBys(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeSchedulePlanningScheduleViewSortBy,
        false,
        false,
        false
      )
      .pipe(
        tap(x => {
          // Change sort order
          // Should be fixed, not by id or name
          if (x.length > 0) {
            this.addScheduleViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr
            );
            this.addScheduleViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningScheduleViewSortBy.Firstname
            );
            this.addScheduleViewSortBy(
              x,
              TermGroup_TimeSchedulePlanningScheduleViewSortBy.Lastname
            );
          }
        })
      );
  }

  private addScheduleViewSortBy(
    list: SmallGenericType[],
    sortBy: TermGroup_TimeSchedulePlanningScheduleViewSortBy
  ): void {
    const item = list.find(i => i.id === sortBy);
    if (item) this.scheduleViewSortBys.push(item);
  }

  private loadReadOnlyPermissions(): Observable<Record<number, boolean>> {
    const featureIds: number[] = [];
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_Bookings);
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_OnDutyShifts);
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_ShowCosts);

    return this.coreService.hasReadOnlyPermissions(featureIds).pipe(
      tap(x => {
        this.bookingReadPermission.set(
          x[Feature.Time_Schedule_SchedulePlanning_Bookings]
        );
        this.standbyShiftsReadPermission.set(
          x[Feature.Time_Schedule_SchedulePlanning_StandbyShifts]
        );
        this.onDutyShiftsReadPermission.set(
          x[Feature.Time_Schedule_SchedulePlanning_OnDutyShifts]
        );
        this.showCostReadPermission.set(
          x[Feature.Time_Schedule_SchedulePlanning_ShowCosts]
        );
      })
    );
  }

  private loadModifyPermissions(): Observable<Record<number, boolean>> {
    const featureIds: number[] = [];
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_OnDutyShifts);
    featureIds.push(Feature.Time_Distribution_Reports_SavePublicSelections);

    return this.coreService.hasModifyPermissions(featureIds).pipe(
      tap(x => {
        this.onDutyShiftsModifyPermission.set(
          x[Feature.Time_Schedule_SchedulePlanning_OnDutyShifts]
        );
        this.savePublicSelectionPermission.set(
          x[Feature.Time_Distribution_Reports_SavePublicSelections]
        );
      })
    );
  }

  private loadCompanySettings(): Observable<UserCompanySettingCollection> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.TimeSchedulePlanningDayViewStartTime,
        CompanySettingType.TimeSchedulePlanningDayViewEndTime,
        CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength,
        CompanySettingType.TimeShiftTypeMandatory,
        CompanySettingType.TimeEditShiftAllowHoles,
        CompanySettingType.TimeSchedulePlanningSendXEMailOnChange,
        CompanySettingType.TimeMaxNoOfBrakes,
        CompanySettingType.UseMultipleScheduleTypes,
        CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly,
        CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore,
        CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore,
        CompanySettingType.UseAccountHierarchy,
        CompanySettingType.DefaultEmployeeAccountDimEmployee,
        CompanySettingType.UseLeisureCodes,
        CompanySettingType.TimeUseVacant,
        CompanySettingType.TimeSkillCantBeOverridden,
      ])
      .pipe(
        tap(x => {
          this.dayViewStartTime.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningDayViewStartTime
            )
          );
          this.originalDayViewStartTime = this.dayViewStartTime();
          this.dayViewEndTime.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningDayViewEndTime
            )
          );
          this.originalDayViewEndTime = this.dayViewEndTime();
          this.dayViewMinorTickLength.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength
            )
          );
          this.shiftTypeMandatory.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimeShiftTypeMandatory
            )
          );
          this.allowHolesWithoutBreaks.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimeEditShiftAllowHoles
            )
          );
          this.sendXEMailOnChange.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningSendXEMailOnChange
            )
          );
          this.maxNbrOfBreaks.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeMaxNoOfBrakes
            )
          );
          this.showGrossTimeSetting.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing
            )
          );
          this.useMultipleScheduleTypes.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseMultipleScheduleTypes
            )
          );
          this.shiftRequestPreventTooEarly.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly
            )
          );
          this.shiftRequestPreventTooEarlyWarnHoursBefore.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore
            )
          );
          this.shiftRequestPreventTooEarlyStopHoursBefore.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore
            )
          );
          this.useAccountHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
          this.defaultEmployeeAccountDimIdEmployee.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.DefaultEmployeeAccountDimEmployee
            )
          );
          this.useLeisureCodes.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseLeisureCodes
            )
          );
          this.useVacant.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimeUseVacant
            )
          );
          this.skillCantBeOverridden.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.TimeSkillCantBeOverridden
            )
          );
        })
      );
  }

  private loadUserSettings(): Observable<UserCompanySettingCollection> {
    return this.coreService
      .getUserSettings([
        UserSettingType.TimeSchedulePlanningDefaultView,
        UserSettingType.TimeSchedulePlanningDefaultInterval,
        UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy,
        UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy,
        UserSettingType.TimeSchedulePlanningStartWeek,
        UserSettingType.TimeSchedulePlanningDisableAutoLoad,
      ])
      .pipe(
        tap(x => {
          const defaultView = SettingsUtil.getIntUserSetting(
            x,
            UserSettingType.TimeSchedulePlanningDefaultView
          );
          if (this.defaultView() !== defaultView) {
            this.defaultView.set(defaultView);
          }

          const defaultIntervalValue = SettingsUtil.getIntUserSetting(
            x,
            UserSettingType.TimeSchedulePlanningDefaultInterval
          );
          if (this.defaultInterval() !== defaultIntervalValue) {
            this.defaultInterval.set(defaultIntervalValue);
          }

          const dayViewDefaultSortByValue = SettingsUtil.getIntUserSetting(
            x,
            UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy
          );
          if (this.dayViewDefaultSortBy() !== dayViewDefaultSortByValue) {
            this.dayViewDefaultSortBy.set(dayViewDefaultSortByValue);
            this.dayViewDefaultSortByChanged.next(dayViewDefaultSortByValue);
          }

          const scheduleViewDefaultSortByValue = SettingsUtil.getIntUserSetting(
            x,
            UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy
          );
          if (
            this.scheduleViewDefaultSortBy() !== scheduleViewDefaultSortByValue
          ) {
            this.scheduleViewDefaultSortBy.set(scheduleViewDefaultSortByValue);
            this.scheduleViewDefaultSortByChanged.next(
              scheduleViewDefaultSortByValue
            );
          }

          const disableAutoLoadValue = SettingsUtil.getBoolUserSetting(
            x,
            UserSettingType.TimeSchedulePlanningDisableAutoLoad
          );
          if (this.disableAutoLoad() !== disableAutoLoadValue) {
            this.disableAutoLoad.set(disableAutoLoadValue);
            this.disableAutoLoadChanged.next(true);
          }

          const disableBreaksWithinHolesWarningValue =
            SettingsUtil.getBoolUserSetting(
              x,
              UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning
            );
          if (
            this.disableBreaksWithinHolesWarning() !==
            disableBreaksWithinHolesWarningValue
          ) {
            this.disableBreaksWithinHolesWarning.set(
              disableBreaksWithinHolesWarningValue
            );
          }
        })
      );
  }

  loadSelectableInformationSettings(
    viewDefinition: TermGroup_TimeSchedulePlanningViews
  ): Observable<UserCompanySettingCollection | undefined> {
    const type =
      this.getUserSelectionTypeForSelectableInformationSettings(viewDefinition);
    if (type) {
      return this.userSelectionService.getUserSelections(type).pipe(
        tap(x => {
          if (x) {
            this.setSelectableInformationSettingsFromServer(
              x.find(s => s.default)?.selection || ''
            );
          }
        })
      );
    } else {
      return of(undefined);
    }
  }

  private setSelectableInformationSettingsFromServer(
    settingsString: string
  ): void {
    // Set default
    this.selectableInformationSettings = new SchedulePlanningSettingsDTO();

    if (!settingsString) return;

    // Override default with loaded settings
    Object.assign(
      this.selectableInformationSettings,
      JSON.parse(settingsString)
    );

    // Set signal values from DTO values
    if (
      this.showInactiveEmployees() !==
      this.selectableInformationSettings.showInactiveEmployees
    ) {
      this.showInactiveEmployees.set(
        this.selectableInformationSettings.showInactiveEmployees
      );
    }

    if (
      this.showUnemployedEmployees() !==
      this.selectableInformationSettings.showUnemployedEmployees
    ) {
      this.showUnemployedEmployees.set(
        this.selectableInformationSettings.showUnemployedEmployees
      );
    }

    if (
      this.defaultUserSelectionId() !==
      this.selectableInformationSettings.defaultUserSelectionId
    ) {
      this.defaultUserSelectionId.set(
        this.selectableInformationSettings.defaultUserSelectionId ?? 0
      );
    }

    if (
      this.showEmployeeGroup() !==
      this.selectableInformationSettings.showEmployeeGroup
    ) {
      this.showEmployeeGroup.set(
        this.selectableInformationSettings.showEmployeeGroup
      );
    }

    if (
      this.showCyclePlannedTime() !==
      this.selectableInformationSettings.showCyclePlannedTime
    ) {
      this.showCyclePlannedTime.set(
        this.selectableInformationSettings.showCyclePlannedTime
      );
    }

    if (
      this.showScheduleTypeFactorTime() !==
      this.selectableInformationSettings.showScheduleTypeFactorTime
    ) {
      this.showScheduleTypeFactorTime.set(
        this.selectableInformationSettings.showScheduleTypeFactorTime
      );
    }

    if (
      this.showGrossTime() !== this.selectableInformationSettings.showGrossTime
    ) {
      this.showGrossTime.set(this.selectableInformationSettings.showGrossTime);
    }

    if (
      this.showTotalCost() !== this.selectableInformationSettings.showTotalCost
    ) {
      this.showTotalCost.set(this.selectableInformationSettings.showTotalCost);
    }

    if (
      this.showTotalCostIncEmpTaxAndSuppCharge() !==
      this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge
    ) {
      this.showTotalCostIncEmpTaxAndSuppCharge.set(
        this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge
      );
    }

    if (
      this.showAvailability() !==
      this.selectableInformationSettings.showAvailability
    ) {
      this.showAvailability.set(
        this.selectableInformationSettings.showAvailability
      );
    }

    if (
      this.skipXEMailOnChanges() !==
      this.selectableInformationSettings.skipXEMailOnChanges
    ) {
      this.skipXEMailOnChanges.set(
        this.selectableInformationSettings.skipXEMailOnChanges
      );
    }

    // Skip work rules should always be false initially
    // if (
    //   this.skipWorkRules() !== this.selectableInformationSettings.skipWorkRules
    // ) {
    //   this.skipWorkRules.set(this.selectableInformationSettings.skipWorkRules);
    // }

    if (
      this.summaryInFooter() !==
      this.selectableInformationSettings.summaryInFooter
    ) {
      this.summaryInFooter.set(
        this.selectableInformationSettings.summaryInFooter
      );
    }
  }

  saveSelectableInformationSettings(
    viewDefinition: TermGroup_TimeSchedulePlanningViews
  ): Observable<any> {
    this.setSelectableInformationSettingsFromSignals();

    const type =
      this.getUserSelectionTypeForSelectableInformationSettings(viewDefinition);

    if (type) {
      return this.saveStringUserSelection(
        type,
        'MySelection', // TODO: Pass in name of the selection
        JSON.stringify(this.selectableInformationSettings)
      );
    } else {
      return of(undefined);
    }
  }

  private setSelectableInformationSettingsFromSignals(): void {
    if (!this.selectableInformationSettings)
      this.selectableInformationSettings = new SchedulePlanningSettingsDTO();

    // Set DTO values from signal values
    this.selectableInformationSettings.showInactiveEmployees =
      this.showInactiveEmployees();
    this.selectableInformationSettings.showUnemployedEmployees =
      this.showUnemployedEmployees();
    this.selectableInformationSettings.defaultUserSelectionId =
      this.defaultUserSelectionId();
    this.selectableInformationSettings.showEmployeeGroup =
      this.showEmployeeGroup();
    this.selectableInformationSettings.showCyclePlannedTime =
      this.showCyclePlannedTime();
    this.selectableInformationSettings.showScheduleTypeFactorTime =
      this.showScheduleTypeFactorTime();
    this.selectableInformationSettings.showGrossTime = this.showGrossTime();
    this.selectableInformationSettings.showTotalCost = this.showTotalCost();
    this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge =
      this.showTotalCostIncEmpTaxAndSuppCharge();
    this.selectableInformationSettings.showAvailability =
      this.showAvailability();
    this.selectableInformationSettings.skipXEMailOnChanges =
      this.skipXEMailOnChanges();
    this.selectableInformationSettings.skipWorkRules = this.skipWorkRules();
    this.selectableInformationSettings.summaryInFooter = this.summaryInFooter();
  }

  getUserSelectionTypeForSelectableInformationSettings(
    viewDefinition: TermGroup_TimeSchedulePlanningViews
  ): UserSelectionType | undefined {
    switch (viewDefinition) {
      case TermGroup_TimeSchedulePlanningViews.Day:
        return UserSelectionType.SchedulePlanningView_Day;
      case TermGroup_TimeSchedulePlanningViews.Schedule:
        return UserSelectionType.SchedulePlanningView_Schedule;
      default:
        return undefined;
    }
  }

  saveBoolCompanySetting(
    settingType: CompanySettingType,
    value: boolean
  ): Observable<any> {
    const model = {
      settingMainType: SettingMainType.Company,
      settingTypeId: settingType,
      boolValue: value,
    } as ISaveUserCompanySettingModel;

    return of(this.coreService.saveBoolSetting(model).subscribe());
  }

  saveIntCompanySetting(
    settingType: CompanySettingType,
    value: number
  ): Observable<any> {
    const model = {
      settingMainType: SettingMainType.Company,
      settingTypeId: settingType,
      intValue: value,
    } as ISaveUserCompanySettingModel;

    return of(this.coreService.saveIntSetting(model).subscribe());
  }

  saveBoolUserSetting(
    settingType: UserSettingType,
    value: boolean
  ): Observable<any> {
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: settingType,
      boolValue: value,
    } as ISaveUserCompanySettingModel;

    return of(this.coreService.saveBoolSetting(model).subscribe());
  }

  saveIntUserSetting(
    settingType: UserSettingType,
    value: number
  ): Observable<any> {
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: settingType,
      intValue: value,
    } as ISaveUserCompanySettingModel;

    return of(this.coreService.saveIntSetting(model).subscribe());
  }

  saveStringUserSetting(
    settingType: UserSettingType,
    value: string
  ): Observable<any> {
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: settingType,
      stringValue: value,
    } as ISaveUserCompanySettingModel;

    return of(this.coreService.saveStringSetting(model).subscribe());
  }

  saveStringUserSelection(
    settingType: UserSelectionType,
    name: string,
    value: string
  ): Observable<any> {
    const model = {
      type: settingType,
      name: name,
      selection: value,
    } as IUserSelectionDTO;

    return of(this.userSelectionService.saveUserSelection(model).subscribe());
  }
}
