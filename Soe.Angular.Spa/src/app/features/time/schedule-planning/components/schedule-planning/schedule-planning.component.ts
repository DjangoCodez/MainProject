import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
  ViewContainerRef,
} from '@angular/core';
import { SpHeaderComponent } from '../sp-header/sp-header.component';
import { SpContentComponent } from '../sp-content/sp-content.component';
import { SpFooterComponent } from '../sp-footer/sp-footer.component';
import {
  DateRangeChangedValue,
  SpFilterService,
  ViewDefinitionChangedValue,
} from '../../services/sp-filter.service';
import { SharedModule } from '@shared/shared.module';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { SpShiftService } from '../../services/sp-shift.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { Observable, of, Subscription, tap, switchMap, catchError } from 'rxjs';
import { SpSettingService } from '../../services/sp-setting.service';
import { SpSlotService } from '../../services/sp-slot.service';
import {
  AddShiftEvent,
  DeleteShiftsEvent,
  EditShiftEvent,
  EmployeesAndShiftsRecalculatedEvent,
  ShiftAbsenceEvent,
  ShiftRequestEvent,
  SpEventService,
  SplitShiftEvent,
} from '../../services/sp-event.service';
import { SpDialogService } from '../../services/sp-dialog.service';
import { SpShiftDialogResult } from '../../dialogs/sp-shift-dialog/sp-shift-dialog.component';
import { SpShiftDeleteDialogResult } from '../../dialogs/sp-shift-delete-dialog/sp-shift-delete-dialog.component';
import {
  PlanningShiftDTO,
  PlanningShiftDayDTO,
} from '../../models/shift.model';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { SpGraphicsService } from '../../services/sp-graphics.service';
import {
  CompanySettingType,
  SettingMainType,
  TermGroup_ShiftHistoryType,
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningViews,
} from '@shared/models/generated-interfaces/Enumerations';
import { SplitContainerComponent } from '@ui/split-container/split-container.component';
import { SpTimeSlotHeaderComponent } from '../sp-time-slot-header/sp-time-slot-header.component';
import { SpTimeSlotSumComponent } from '../sp-time-slot-sum/sp-time-slot-sum.component';
import { SpRightContentComponent } from '../sp-right-content/sp-right-content.component';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import {
  SpEmployeeDaySlot,
  SpEmployeeHourSlot,
} from '../../models/time-slot.model';
import { SpShiftSplitDialogResult } from '../../dialogs/sp-shift-split-dialog/sp-shift-split-dialog.component';
import { SpSettingDialogResult } from '../../dialogs/sp-setting-dialog/sp-setting-dialog.component';
import { CommonModule } from '@angular/common';
import { SpShiftRequestDialogResult } from '../../dialogs/sp-shift-request-dialog/sp-shift-request-dialog.component';
import { SpShiftRequestService } from '../../services/sp-shift-request.service';
import { SpWorkRuleService } from '../../services/sp-work-rule.service';
import { IShiftRequestStatusDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { IAbsenceQuickDialogResult } from '@features/time/absence-requests/dialogs/absence-quick-dialog/absence-quick-dialog.component';

@Component({
  selector: 'soe-schedule-planning',
  imports: [
    CdkScrollable,
    CommonModule,
    SharedModule,
    SplitContainerComponent,
    SpHeaderComponent,
    SpTimeSlotHeaderComponent,
    SpTimeSlotSumComponent,
    SpContentComponent,
    SpFooterComponent,
    SpRightContentComponent,
  ],
  templateUrl: './schedule-planning.component.html',
  styleUrl: './schedule-planning.component.scss',
})
export class SchedulePlanningComponent
  implements OnInit, AfterViewInit, OnDestroy
{
  private readonly service = inject(SchedulePlanningService);
  private readonly dialogService = inject(SpDialogService);
  private readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);
  private readonly graphicsService = inject(SpGraphicsService);
  private readonly progressService = inject(ProgressService);
  readonly settingService = inject(SpSettingService);
  private readonly shiftService = inject(SpShiftService);
  private readonly shiftRequestService = inject(SpShiftRequestService);
  private readonly slotService = inject(SpSlotService);
  private readonly workRuleService = inject(SpWorkRuleService);

  private performLoad = new Perform<any>(this.progressService);

  @ViewChild(SpContentComponent, { static: true, read: ViewContainerRef })
  content!: ViewContainerRef;
  @ViewChild('contentContainer', { static: false, read: ElementRef })
  contentContainer!: ElementRef;
  @ViewChild('spContent', { static: false, read: ElementRef })
  spContent!: ElementRef;
  contentHeight = '';

  hasScrollbar = signal(false);

  // INIT

  private settingsLoadedSubscription?: Subscription;
  private viewDefinitionChangedSubscription?: Subscription;
  private dateRangeChangedSubscription?: Subscription;
  private shiftsLoadedSubscription?: Subscription;
  private employeesAndShiftsRecalculatedSubscription?: Subscription;

  // Employee context menu
  private reloadEmployeeSubscription?: Subscription;
  private reloadEmployeesSubscription?: Subscription;

  // Shift/slot context menu
  private addShiftEventSubscription?: Subscription;
  private editShiftEventSubscription?: Subscription;
  private deleteShiftEventSubscription?: Subscription;
  private splitShiftEventSubscription?: Subscription;
  private shiftRequestEventSubscription?: Subscription;
  private shiftAbsenceEventSubscription?: Subscription;

  private selectedShiftsChangedSubscription?: Subscription;
  private selectedEmployeeChangedSubscription?: Subscription;
  private selectedSlotChangedSubscription?: Subscription;

  ngOnInit(): void {
    this.settingsLoadedSubscription =
      this.settingService.settingsLoaded.subscribe(value => {
        // When BehaviorSubject is initialized, this will be called with the value = false, ignore that.
        // We only want to react when the settings are actually loaded.
        if (value) this.onSettingsLoaded(value);
      });

    this.viewDefinitionChangedSubscription =
      this.filterService.viewDefinitionChanged.subscribe(
        (value: ViewDefinitionChangedValue | undefined) => {
          if (value) this.onViewDefinitionChanged(value);
        }
      );

    this.dateRangeChangedSubscription =
      this.filterService.dateRangeChanged.subscribe(
        (value: DateRangeChangedValue | undefined) => {
          if (value) this.onDateRangeChanged(value);
        }
      );

    this.shiftsLoadedSubscription = this.shiftService.shiftsLoaded.subscribe(
      (shifts: PlanningShiftDTO[] | undefined) => {
        if (shifts) this.onShiftsLoaded(shifts);
      }
    );

    this.employeesAndShiftsRecalculatedSubscription =
      this.eventService.employeesAndShiftsRecalculated.subscribe(
        (event: EmployeesAndShiftsRecalculatedEvent | undefined) => {
          if (event) this.onEmployeesAndShiftsRecalculated(event);
        }
      );

    this.reloadEmployeeSubscription =
      this.eventService.reloadEmployeeEvent.subscribe(event => {
        if (event) this.loadEmployees([event.employeeId], false);
      });

    this.reloadEmployeesSubscription =
      this.eventService.reloadAllEmployeesEvent.subscribe(event => {
        if (event) this.loadEmployees([], true);
      });

    this.addShiftEventSubscription = this.eventService.addShiftEvent.subscribe(
      (event?: AddShiftEvent) => {
        this.openShiftAddDialog(event);
      }
    );

    this.editShiftEventSubscription =
      this.eventService.editShiftEvent.subscribe((event?: EditShiftEvent) => {
        this.openShiftEditDialog(event);
      });

    this.deleteShiftEventSubscription =
      this.eventService.deleteShiftEvent.subscribe(
        (event?: DeleteShiftsEvent) => {
          this.openShiftDeleteDialog(event);
        }
      );

    this.splitShiftEventSubscription =
      this.eventService.splitShiftEvent.subscribe((event?: SplitShiftEvent) => {
        this.openShiftSplitDialog(event);
      });

    this.shiftRequestEventSubscription =
      this.eventService.shiftRequestEvent.subscribe(
        (event?: ShiftRequestEvent) => {
          this.initOpenShiftRequestDialog(event);
        }
      );

    this.shiftAbsenceEventSubscription =
      this.eventService.shiftAbsenceEvent.subscribe(
        (event?: ShiftAbsenceEvent) => {
          this.openShiftAbsenceDialog(event);
        }
      );

    this.selectedShiftsChangedSubscription =
      this.shiftService.selectedShiftsChanged.subscribe(
        (shifts: PlanningShiftDTO[] | undefined) => {
          // If selecting shifts, check if an employee is selected and deselect it
          if ((shifts || []).length > 0) {
            if (this.employeeService.selectedEmployeeChanged.value) {
              this.employeeService.selectedEmployeeChanged.next(undefined);
            }
            // Also, check if a slot is selected and deselect it
            if (this.employeeService.selectedSlotChanged.value) {
              this.employeeService.selectedSlotChanged.next(undefined);
            }
          }
        }
      );

    this.selectedEmployeeChangedSubscription =
      this.employeeService.selectedEmployeeChanged.subscribe(
        (employee: PlanningEmployeeDTO | undefined) => {
          // If selecting an employee, check if shifts are selected and deselect them
          if (employee) {
            if (this.shiftService.selectedShiftsChanged.value.length > 0) {
              this.shiftService.selectedShiftsChanged.next([]);
            }
            // Also, check if a slot is selected and deselect it
            if (this.employeeService.selectedSlotChanged.value) {
              this.employeeService.selectedSlotChanged.next(undefined);
            }
          }
        }
      );

    this.selectedSlotChangedSubscription =
      this.employeeService.selectedSlotChanged.subscribe(
        (slot: SpEmployeeDaySlot | SpEmployeeHourSlot | undefined) => {
          // If selecting a slot, check if an employee is selected and deselect it
          if (slot) {
            if (this.employeeService.selectedEmployeeChanged.value) {
              this.employeeService.selectedEmployeeChanged.next(undefined);
            }
            // Also, check if shifts are selected and deselect them
            if (this.shiftService.selectedShiftsChanged.value.length > 0) {
              this.shiftService.selectedShiftsChanged.next([]);
            }
          }
        }
      );

    window.addEventListener('resize', () => {
      this.onWindowResize();
    });

    // Initially load all employees
    // TODO: Do we actually want to do this?
    this.loadEmployees();
  }

  ngAfterViewInit(): void {
    this.setContentWidth();
    this.setContentHeight();
  }

  ngOnDestroy(): void {
    this.settingsLoadedSubscription?.unsubscribe();
    this.viewDefinitionChangedSubscription?.unsubscribe();
    this.dateRangeChangedSubscription?.unsubscribe();
    this.shiftsLoadedSubscription?.unsubscribe();
    this.employeesAndShiftsRecalculatedSubscription?.unsubscribe();
    this.reloadEmployeeSubscription?.unsubscribe();
    this.reloadEmployeesSubscription?.unsubscribe();
    this.addShiftEventSubscription?.unsubscribe();
    this.editShiftEventSubscription?.unsubscribe();
    this.deleteShiftEventSubscription?.unsubscribe();
    this.splitShiftEventSubscription?.unsubscribe();
    this.shiftRequestEventSubscription?.unsubscribe();
    this.shiftAbsenceEventSubscription?.unsubscribe();
    this.selectedShiftsChangedSubscription?.unsubscribe();
    this.selectedEmployeeChangedSubscription?.unsubscribe();
    this.selectedSlotChangedSubscription?.unsubscribe();

    window.removeEventListener('resize', () => {
      this.onWindowResize();
    });
  }

  // SERVICE CALLS

  loadEmployees(employeeIds: number[] = [], loadShifts = false): void {
    this.performLoad.load(
      this.employeeService
        .loadEmployees(employeeIds)
        .pipe(tap(() => this.onEmployeesLoaded(employeeIds, loadShifts))),
      { showDialog: employeeIds.length === 0 || employeeIds.length > 2 }
    );
  }

  loadShifts(employeeIds: number[] = []): void {
    this.performLoad.load(this.shiftService.loadShifts(employeeIds));
  }

  // EVENTS

  private onSettingsLoaded(value: boolean) {
    if (value) {
      this.filterService.setupBlockTypes();

      this.filterService.setViewDefinition(
        this.settingService.defaultView(),
        undefined,
        true
      );
      this.filterService.setScheduleViewInterval(
        this.settingService.defaultInterval()
      );

      // Settings loaded, check if we should load shifts
      if (!this.settingService.disableAutoLoad()) {
        if (this.employeeService.employees().length > 0) {
          this.loadShifts();
        }
      }
    }
  }

  onSettingsChanged(event: SpSettingDialogResult) {
    let loadShifts = false;
    let recreateEmployees = false;
    let recalculateEmployees = false;
    let updateContentHeight = false;
    let updateEmployeeTooltip = false;

    if (
      event.changedSettings?.find(
        s =>
          s.settingMainType === SettingMainType.Company &&
          s.settingType ===
            CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength
      )
    ) {
      // Day view time slot interval changed, need to recreate employee slots
      recreateEmployees = true;
    }

    if (event.loadGrossNetAndCost) {
      // Show gross time and/or cost has been turned on, need to reload shifts
      loadShifts = true;
    } else if (event.calculateTimes) {
      // Show gross time and/or cost has been turned off, need to recalculate shifts
      recalculateEmployees = true;
    }

    // Some setting that affects employee tooltips was either turned on or off
    if (event.updateEmployeeTooltip) {
      updateEmployeeTooltip = true;
    }

    if (event.updateContentHeight) {
      // Summary row turned on or off, content height needs to be updated
      updateContentHeight = true;
    }

    // Do actions based on flags above
    if (loadShifts) {
      this.loadShifts();
    } else if (recreateEmployees) {
      this.slotService.createTimeSlots();
      const employees = this.employeeService.createPlanningEmployees(
        this.employeeService.employees()
      );
      this.employeeService.employees.set(employees);
      // this.employeeService.recalculateEmployeesAndShifts();
      // TODO: For now we reload shifts to recreate them correctly, shouldn't be necessary
      this.loadShifts();
    } else if (recalculateEmployees) {
      this.shiftService.clearShiftTooltips();
      this.employeeService.recalculateEmployeesAndShifts();
    } else if (updateEmployeeTooltip) {
      this.employeeService.createEmployeeToolTips();
    }

    if (updateContentHeight) {
      this.setContentHeight();
    }
  }

  private onViewDefinitionChanged(value: ViewDefinitionChangedValue) {
    const fromAnyDay =
      value.from === TermGroup_TimeSchedulePlanningViews.Day ||
      value.from === TermGroup_TimeSchedulePlanningViews.TemplateDay ||
      value.from === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay ||
      value.from === TermGroup_TimeSchedulePlanningViews.ScenarioDay ||
      value.from === TermGroup_TimeSchedulePlanningViews.StandbyDay;
    const toAnyDay =
      value.to === TermGroup_TimeSchedulePlanningViews.Day ||
      value.to === TermGroup_TimeSchedulePlanningViews.TemplateDay ||
      value.to === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay ||
      value.to === TermGroup_TimeSchedulePlanningViews.ScenarioDay ||
      value.to === TermGroup_TimeSchedulePlanningViews.StandbyDay;
    const fromAnyWeek =
      value.from === TermGroup_TimeSchedulePlanningViews.Schedule ||
      value.from === TermGroup_TimeSchedulePlanningViews.TemplateSchedule ||
      value.from ===
        TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule ||
      value.from === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule ||
      value.from === TermGroup_TimeSchedulePlanningViews.StandbySchedule;
    const toAnyWeek =
      value.to === TermGroup_TimeSchedulePlanningViews.Schedule ||
      value.to === TermGroup_TimeSchedulePlanningViews.TemplateSchedule ||
      value.to === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule ||
      value.to === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule ||
      value.to === TermGroup_TimeSchedulePlanningViews.StandbySchedule;

    // Hide right content when switching view
    this.settingService.showRightContent.set(false);

    // Set initial date(s)
    // Use today unless specified in the event
    const date = value.targetDate ? value.targetDate : new Date();

    if (this.filterService.isCommonDayView()) {
      // Current day
      this.filterService.setDateRange(
        'onViewDefinitionChanged',
        date
          .beginningOfDay()
          .addMinutes(this.settingService.dayViewStartTime()),
        date.beginningOfDay().addMinutes(this.settingService.dayViewEndTime()),
        true
      );
    } else if (this.filterService.isCommonScheduleView()) {
      // Current week
      this.filterService.setDateRange(
        'onViewDefinitionChanged',
        date.beginningOfWeek(),
        date.endOfWeek(),
        true
      );
    }

    // Switching between day view and schedule view (week)
    if ((fromAnyDay && !toAnyDay) || (fromAnyWeek && !toAnyWeek)) {
      // Recalculate pixels per time unit
      this.graphicsService.calculatePixelsPerTimeUnit();
      this.setContentHeight();

      // Resort employees since the views have different sorting types
      this.employeeService.sortEmployees();
    }
  }

  private onDateRangeChanged(value: DateRangeChangedValue) {
    // Date range changed, create new slots
    this.slotService.createTimeSlots();
    this.setContentWidth(true);

    const employees = this.employeeService.createPlanningEmployees(
      this.employeeService.employees()
    );
    this.employeeService.employees.set(employees);
    this.employeeService.setEmployedEmployees();

    // If shifts were loaded before, reload them
    if (value.reloadShifts && this.shiftService.initialShiftsLoaded()) {
      this.loadShifts();
    }
  }

  private onShiftsLoaded(shifts: PlanningShiftDTO[]): void {
    this.employeeService.recalculateEmployeesAndShifts();

    // If employees are sorted by start time, sort them again
    // This is needed because the default sorting on the employees are set before the shifts are loaded
    if (
      this.filterService.isCommonDayView() &&
      this.filterService.sortEmployeesByForDayView() ===
        TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime
    ) {
      this.employeeService.sortEmployees();
    }
  }

  onFilterChanged(): void {
    this.employeeService.recalculateEmployeesAndShifts();
  }

  private onEmployeesAndShiftsRecalculated(
    event: EmployeesAndShiftsRecalculatedEvent
  ): void {
    this.checkScrollbar();
  }

  private onEmployeesLoaded(
    employeeIds: number[] = [],
    loadShifts = false
  ): void {
    if (
      loadShifts ||
      (this.settingService.settingsLoaded.getValue() &&
        !this.settingService.disableAutoLoad())
    ) {
      this.loadShifts(employeeIds);
    } else {
      this.employeeService.recalculateEmployeesAndShifts();
      this.checkScrollbar();
    }
  }

  onRightContentWidthChanged(ratio: number): void {
    this.checkScrollbar(true);
  }

  private onWindowResize(): void {
    this.checkScrollbar();
  }

  private setContentWidthIntervalId: NodeJS.Timeout | undefined = undefined;
  private setContentWidth(forceCalculatePixelsPerTimeUnit = false) {
    if (this.setContentWidthIntervalId)
      clearInterval(this.setContentWidthIntervalId);

    const element = this.content?.element?.nativeElement;
    if (element) {
      const width = BrowserUtil.getElementWidth(element);
      if (width > 0)
        this.graphicsService.setContentWidth(
          width,
          forceCalculatePixelsPerTimeUnit
        );
    }

    // Loop until the content width is set
    if (this.graphicsService.contentWidth === 0) {
      this.setContentWidthIntervalId = setInterval(() => {
        this.setContentWidth(forceCalculatePixelsPerTimeUnit);
      }, 500);
    }
  }

  private setContentHeight() {
    // Get the top position of the content area and set the height to fill the rest of the screen
    setTimeout(() => {
      // Wait for dynamic heights to settle
      let top = this.spContent.nativeElement.getBoundingClientRect().top;
      if (top === 0) {
        console.warn(
          'setContentHeight()',
          'top is 0, trying again in 1 second'
        );
        setTimeout(() => {
          this.setContentHeight();
        }, 1000);
        return;
      }

      top += 32; // Add bottom margin
      this.contentHeight = `calc(100vh - ${top}px)`;
    }, 500);
  }

  // DIALOGS

  private openShiftAddDialog(event?: AddShiftEvent) {
    if (event) {
      const day: PlanningShiftDayDTO = new PlanningShiftDayDTO(
        event.date,
        event.employeeId
      );
      this.dialogService
        .openShiftDialog(day, event.selectedShiftId, true)
        .subscribe((result: SpShiftDialogResult) => {});
    }
  }

  private openShiftEditDialog(event?: EditShiftEvent) {
    if (event) {
      const day: PlanningShiftDayDTO = new PlanningShiftDayDTO(
        event.shift.actualStartDate,
        event.shift.employeeId,
        [event.shift]
      );

      this.dialogService
        .openShiftDialog(day, event.shift.timeScheduleTemplateBlockId, false)
        .subscribe((result: SpShiftDialogResult) => {});
    }
  }

  private openShiftDeleteDialog(event?: DeleteShiftsEvent) {
    if (event)
      this.dialogService
        .openShiftDeleteDialog(event.employee, event.shifts, event.onDutyShifts)
        .subscribe((result: SpShiftDeleteDialogResult) => {});
  }

  private openShiftSplitDialog(event?: SplitShiftEvent) {
    if (event) {
      this.dialogService
        .openShiftSplitDialog(event.dialogTitle, event.shift)
        .subscribe((result: SpShiftSplitDialogResult) => {
          const employeeIds: number[] = [];
          if (result.employeeId1) employeeIds.push(result.employeeId1);
          if (result.employeeId2 && result.employeeId2 !== result.employeeId1)
            employeeIds.push(result.employeeId2);

          if (employeeIds.length > 0) this.loadEmployees(employeeIds, true);
        });
    }
  }

  private initOpenShiftRequestDialog(event?: ShiftRequestEvent) {
    if (event?.employee && event?.shift) {
      const shift = event.shift;

      this.performLoad.load(
        // Validate that we can send shift request depending on company settings
        this.validateSendShiftRequest(event.employee, shift).pipe(
          // Always emit a value so performLoad can close the progress dialog
          // Carry the validation flag forward and pair it with the status
          switchMap(valid =>
            (valid
              ? this.shiftRequestService
                  .getShiftRequestStatus(shift.timeScheduleTemplateBlockId)
                  .pipe(catchError(() => of(null)))
              : of(null)
            ).pipe(
              // Map to a tuple [valid, status]
              switchMap(status =>
                of([valid, status] as [boolean, IShiftRequestStatusDTO | null])
              )
            )
          ),
          tap(([valid, status]: [boolean, IShiftRequestStatusDTO | null]) => {
            // If validation failed, do not open dialog (but progress will complete)
            if (!valid) return;

            let excludeEmployeeIds: number[] = [];

            // If there is an existing request with existing recipients, exclude them from possible recipients
            if (status?.recipients)
              excludeEmployeeIds = status.recipients.map(r => r.employeeId);

            // Do some filtering on valid accounts for the date of the selected shift
            let validEmployees: PlanningEmployeeDTO[] = [];

            if (this.settingService.useAccountHierarchy()) {
              // TODO: Implement account hierarchy
              // this.employeeService.employedEmployees().forEach(employee => {
              //   if (!employee.hidden && employee.accounts) {
              //     for (const empAccount of employee.accounts) {
              //       if (
              //         this.validAccountIds.includes(empAccount.accountId) &&
              //         shift.actualStartDate.isSameOrAfterOnDay(
              //           empAccount.dateFrom
              //         ) &&
              //         ((empAccount.dateTo !== null &&
              //           shift.actualStartDate.isSameOrBeforeOnDay(
              //             empAccount.dateTo
              //           )) ||
              //           CalendarUtility.isEmptyDate(empAccount.dateTo))
              //       ) {
              //         validEmployees.push(employee);
              //         break;
              //       }
              //     }
              //   }
              // });
            } else {
              validEmployees = this.employeeService.employedEmployees();
            }

            this.openShiftRequestDialog(
              event.employee,
              event.shift,
              validEmployees.filter(
                e =>
                  e.employeeId &&
                  !e.hidden &&
                  !e.vacant &&
                  !excludeEmployeeIds.includes(e.employeeId)
              )
            );
          })
        ),
        {
          // TODO: New term
          title: 'time.schedule.planning.contextmenu.sendshiftrequest',
          message: 'Kontrollerar inställningar för passförfrågan...',
        }
      );
    }
  }

  private openShiftRequestDialog(
    employee: PlanningEmployeeDTO,
    shift: PlanningShiftDTO,
    possibleEmployees: PlanningEmployeeDTO[]
  ) {
    this.dialogService
      .openShiftRequestDialog(employee, shift, possibleEmployees)
      .subscribe((result: SpShiftRequestDialogResult) => {
        if (result?.requestSent) {
          // If request was sent, reload shifts for the source employee to make icon appear
          this.loadShifts([employee.employeeId]);
        }
      });
  }

  private validateSendShiftRequest(
    employee: PlanningEmployeeDTO,
    shift: PlanningShiftDTO
  ): Observable<boolean> {
    if (!this.settingService.shiftRequestPreventTooEarly()) return of(true);

    return this.shiftRequestService
      .checkIfTooEarlyToSendShiftRequest(shift.actualStartTime)
      .pipe(
        switchMap(result =>
          this.workRuleService.showValidateWorkRulesResult(
            TermGroup_ShiftHistoryType.ShiftRequest,
            result,
            employee.employeeId,
            false,
            'time.schedule.planning.contextmenu.sendshiftrequest'
          )
        )
      );
  }

  private getShiftRequestStatus(
    timeScheduleTemplateBlockId: number
  ): Observable<IShiftRequestStatusDTO> {
    return this.shiftRequestService.getShiftRequestStatus(
      timeScheduleTemplateBlockId
    );
  }

  private openShiftAbsenceDialog(event?: ShiftAbsenceEvent) {
    // TODO: Add subscribe and handle result
    if (event)
      this.dialogService
        .openShiftAbsenceDialog(event.employee, event.shifts)
        .subscribe((result: IAbsenceQuickDialogResult) => {
          if (result.affectedEmployeeIds) {
            this.shiftService.reloadShiftsForEmployeeIds =
              result.affectedEmployeeIds;
            this.performLoad.load(
              this.shiftService.loadShifts(
                this.shiftService.reloadShiftsForEmployeeIds
              ),
              { showDialogDelay: 2000 } //TODO: Use Delay, show load-progress at all? IDK
            );
          }
        });
  }

  // HELP METHODS

  private checkScrollbar(forceSetContentWidth = false): void {
    setTimeout(() => {
      const element = this.content?.element?.nativeElement;
      if (element) {
        const showingScrollbar = element.scrollHeight > element.clientHeight;
        if (this.hasScrollbar() !== showingScrollbar || forceSetContentWidth) {
          this.hasScrollbar.set(showingScrollbar);
          this.setContentWidth(true);
        }
      }
    });
  }
}
