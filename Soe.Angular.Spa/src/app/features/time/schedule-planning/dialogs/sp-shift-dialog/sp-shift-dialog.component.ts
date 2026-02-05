import {
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import {
  TermGroup_ShiftHistoryType,
  TermGroup_TimeScheduleTemplateBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SpShiftEditOverviewComponent } from '../../components/sp-shift-edit/sp-shift-edit-overview/sp-shift-edit-overview.component';
import { SpShiftEditComponent } from '../../components/sp-shift-edit/sp-shift-edit.component';
import {
  PlanningShiftBreakDTO,
  PlanningShiftDayDTO,
  PlanningShiftDTO,
} from '../../models/shift.model';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { SpShiftService } from '../../services/sp-shift.service';
import { SpShiftDialogForm } from './sp-shift-dialog-form.model';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SpTranslateService } from '../../services/sp-translate.service';
import { TermCollection } from '@shared/localization/term-types';
import { Observable, tap, map, Subscription } from 'rxjs';
import { SpShiftEditForm } from '../../components/sp-shift-edit/sp-shift-edit-form.model';
import { SpSettingService } from '../../services/sp-setting.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { SpEventService } from '../../services/sp-event.service';
import { SpFilterService } from '../../services/sp-filter.service';
import { SpWorkRuleService } from '../../services/sp-work-rule.service';
import { Perform } from '@shared/util/perform.class';
import { DateUtil } from '@shared/util/date-util';
import { Guid } from '@shared/util/string-util';
import { SpDialogService } from '../../services/sp-dialog.service';
import { SpToolbarEmployeeDate } from '../../toolbar/sp-toolbar-employee-date/sp-toolbar-employee-date';
import { ShiftUtil } from '../../util/shift-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { SpShiftDialogValidationService } from './sp-shift-dialog-validation.service';

export class SpShiftDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  disableClose?: boolean;
  disableContentScroll?: boolean;
  day!: PlanningShiftDayDTO;
  selectedShiftId?: number;
  addShift: boolean = false;
}

export class SpShiftDialogResult {
  shiftModified = false;
}

enum EditShiftFunctions {
  SplitShift = 1,
  ShiftRequest = 2,
  Absence = 3,
  RestoreToSchedule = 4,
  History = 5,
  Accounting = 6,
}

@Component({
  selector: 'sp-shift-dialog',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    DialogComponent,
    InstructionComponent,
    SpShiftEditComponent,
    SpShiftEditOverviewComponent,
    SpToolbarEmployeeDate,
    ToolbarComponent,
    TranslatePipe,
  ],
  providers: [ToolbarService, SpShiftDialogValidationService],
  templateUrl: './sp-shift-dialog.component.html',
  styleUrl: './sp-shift-dialog.component.scss',
})
export class SpShiftDialogComponent
  extends DialogComponent<SpShiftDialogData>
  implements OnInit, OnDestroy
{
  private readonly service = inject(SchedulePlanningService);
  private readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  private readonly filterService = inject(SpFilterService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly progressService = inject(ProgressService);
  readonly settingService = inject(SpSettingService);
  private readonly shiftService = inject(SpShiftService);
  private readonly spTranslateService = inject(SpTranslateService);
  readonly toolbarService = inject(ToolbarService);
  private readonly translate = inject(TranslateService);
  private readonly workRuleService = inject(SpWorkRuleService);
  private readonly spDialogService = inject(SpDialogService);
  private readonly validationService = inject(SpShiftDialogValidationService);

  validationHandler = inject(ValidationHandler);
  form: SpShiftDialogForm = new SpShiftDialogForm({
    validationHandler: this.validationHandler,
    element: { day: new PlanningShiftDayDTO(new Date(), 0) },
  });

  private perform = new Perform<any>(this.progressService);
  executing = signal(false);

  // Terms
  splitShiftLabel = '';
  deletedShiftsLabel = signal('');

  functions: MenuButtonItem[] = [];

  loadingShifts = signal(false);

  private selectedShiftId = 0;
  private selectedShiftLink = 'null';
  selectedShiftIndex = signal<number | undefined>(undefined);
  noSelectedShift = computed(() => this.selectedShiftIndex() === undefined);

  private shiftSummaryNeedsUpdateSubscription?: Subscription;
  private deleteCurrentShiftSubscription?: Subscription;

  ngOnInit(): void {
    // Get data passed in to the dialog
    if (
      !this.data ||
      !this.data.day ||
      !this.data.day.date ||
      !this.data.day.employeeId
    ) {
      this.cancel();
      return;
    }

    const day: PlanningShiftDayDTO = this.data.day;
    const employee = this.employeeService.getEmployee(day.employeeId);

    this.form.reset({
      date: day.date,
      employeeId: day.employeeId,
      employeeName: employee?.name,
    });

    // Remember selected shift
    if (this.data.selectedShiftId) {
      this.selectedShiftId = this.data.selectedShiftId;

      // If the selected shift is on the hidden employee, we also need to remember the link,
      // so we can load the shifts on the hidden employee using the link.
      if (
        this.data.day.shifts &&
        this.data.day.shifts.length > 0 &&
        day.employeeId === this.service.hiddenEmployeeId
      ) {
        this.selectedShiftLink =
          this.data.day.shifts.find(
            shift => shift.timeScheduleTemplateBlockId === this.selectedShiftId
          )?.link || '';
      }
    }

    this.setupTerms().subscribe(() => {
      this.setupToolbar();
    });

    this.shiftSummaryNeedsUpdateSubscription =
      this.eventService.shiftSummaryNeedsUpdate.subscribe(
        (event: any | undefined) => {
          if (event) this.setSummaryRows();
        }
      );

    this.deleteCurrentShiftSubscription =
      this.eventService.deleteCurrentShiftEvent.subscribe(
        (event: any | undefined) => {
          if (event) this.deleteShift();
        }
      );

    this.loadShiftsForDay(this.data.addShift);
  }

  ngOnDestroy(): void {
    this.shiftSummaryNeedsUpdateSubscription?.unsubscribe();
    this.deleteCurrentShiftSubscription?.unsubscribe();
  }

  private setupTerms(): Observable<TermCollection> {
    return this.translate
      .get(['time.schedule.planning.contextmenu.splitshift'])
      .pipe(
        tap(terms => {
          this.splitShiftLabel = terms[
            'time.schedule.planning.contextmenu.splitshift'
          ].format(this.spTranslateService.shiftUndefined());
        })
      );
  }

  private setupToolbar() {
    this.setupFunctions();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarMenuButton('functions', {
          caption: signal('core.functions'),
          dropLeft: signal(true),
          unselectItemAfterSelect: signal(true),
          list: signal(this.functions),
          onItemSelected: event => this.onToolbarFunctionsSelected(event),
        }),
      ],
    });

    // TODO: New terms
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarButton('movePrevDay', {
          iconName: signal('chevron-left'),
          tooltip: signal('Föregående dag'),
          onAction: () => this.onToolbarMovePrevDay(),
        }),
        this.toolbarService.createToolbarButton('moveNextDay', {
          iconName: signal('chevron-right'),
          tooltip: signal('Nästa dag'),
          onAction: () => this.onToolbarMoveNextDay(),
        }),
      ],
    });
  }

  private setupFunctions() {
    this.functions.push({
      id: EditShiftFunctions.SplitShift,
      label: this.splitShiftLabel,
      icon: ['fal', 'scissors'],
      disabled: this.noSelectedShift,
    });
    this.functions.push({ type: 'divider' });
    this.functions.push({
      id: EditShiftFunctions.ShiftRequest,
      label: 'time.schedule.planning.editshift.functions.shiftrequest',
      icon: ['fal', 'envelope'],
      disabled: this.noSelectedShift,
    });
    this.functions.push({ type: 'divider' });
    this.functions.push({
      id: EditShiftFunctions.Absence,
      label: 'time.schedule.planning.editshift.functions.absence',
      icon: ['fal', 'briefcase-medical'],
      iconClass: 'color-error',
      disabled: this.noSelectedShift,
    });
    this.functions.push({
      id: EditShiftFunctions.RestoreToSchedule,
      label: 'time.schedule.planning.editshift.functions.restoretoschedule',
      icon: ['fal', 'arrow-rotate-left'],
      iconClass: 'color-warning',
      disabled: this.noSelectedShift,
    });
    this.functions.push({ type: 'divider' });
    this.functions.push({
      id: EditShiftFunctions.History,
      label: 'time.schedule.planning.editshift.functions.history',
      icon: ['fal', 'clock-rotate-left'],
      disabled: this.noSelectedShift,
    });
    this.functions.push({
      id: EditShiftFunctions.Accounting,
      label: 'common.accounting',
      icon: ['fal', 'table-columns'],
      disabled: this.noSelectedShift,
    });
  }

  private loadShiftsForDay(addShift: boolean) {
    this.loadingShifts.set(true);

    // If new shift on hidden employee, do not load existing shifts on the day
    if (
      this.selectedShiftId === 0 &&
      this.form.employeeId.value === this.service.hiddenEmployeeId
    ) {
      this.shiftsLoaded([], addShift);
      return;
    }

    const includeGrossNetAndCost =
      this.settingService.showGrossTime() ||
      this.settingService.showTotalCost();
    const timeScheduleScenarioHeadId = 0;

    this.perform
      .load$(
        this.shiftService.loadShiftsForEmployeeAndDate(
          this.form.employeeId.value,
          this.form.date.value,
          [
            TermGroup_TimeScheduleTemplateBlockType.Schedule,
            TermGroup_TimeScheduleTemplateBlockType.Standby,
            TermGroup_TimeScheduleTemplateBlockType.OnDuty,
          ],
          true,
          includeGrossNetAndCost,
          this.selectedShiftLink,
          false,
          true,
          true,
          true,
          timeScheduleScenarioHeadId
        )
      )
      .subscribe((shifts: PlanningShiftDTO[]) => {
        this.shiftsLoaded(shifts, addShift);
      });
  }

  private shiftsLoaded(shifts: PlanningShiftDTO[], addShift: boolean) {
    shifts.forEach(shift => {
      // Find breaks that belong to other shifts but overlap this shift
      this.setIntersectingBreaks(shift, shifts);
    });
    this.form.patchShifts(shifts);
    this.form.markAsPristine();

    // Select the shift that was selected in the main view, if any.
    // Pass along the addShift flag because the addShift() method must be called after the timeout inside selectShift()
    // to ensure the UI has time to render the selected shift first.
    // It's the selected shift that should be used as source for the new shift.
    this.selectInitialShift(addShift);
    if (shifts.length === 0 && addShift) {
      this.addShift();
    }

    this.setSummaryRows();
    this.loadingShifts.set(false);
  }

  private setIntersectingBreaks(
    shift: PlanningShiftDTO,
    dayShifts: PlanningShiftDTO[]
  ) {
    // For each shift, find breaks that belong to other shifts but overlap this shift
    const intersectingBreaks = dayShifts
      .filter(s => s !== shift)
      .flatMap(s => s.breaks)
      .filter(
        b =>
          DateUtil.getIntersectingMinutes(
            shift.actualStartTime,
            shift.actualStopTime,
            b.startTime,
            b.stopTime
          ) > 0
      );

    shift.setIntersectingBreaks(intersectingBreaks);
  }

  private selectInitialShift(addShift: boolean = false) {
    if (this.form.shifts.controls.length > 0) {
      if (this.selectedShiftId !== 0) {
        // If a specific shift is selected (from the main view), find it in the list.
        let shiftIndex = this.form.shifts.controls.findIndex(
          s => s.value.timeScheduleTemplateBlockId === this.selectedShiftId
        );
        if (shiftIndex < 0) {
          // If not found, select the first one.
          this.selectedShiftId = 0;
          shiftIndex = 0;
        }
        this.selectShift(shiftIndex, addShift);
      } else if (this.noSelectedShift()) {
        // If no shift is selected, select the first one.
        this.selectShift(0, addShift);
      }
    } else {
      // No shifts available, ensure no selection.
      this.selectedShiftIndex.set(undefined);
    }
  }

  private setSummaryRows() {
    this.form.setSummaryRows(
      this.settingService.showGrossTime(),
      this.settingService.showTotalCost(),
      this.settingService.showTotalCostIncEmpTaxAndSuppCharge()
    );
  }

  // EVENTS

  selectShift(idx: number, addShift: boolean = false) {
    if (this.selectedShiftIndex() !== idx) {
      if (this.noSelectedShift()) {
        this.selectedShiftIndex.set(idx);
        if (addShift) this.addShift();
      } else {
        // Deselect previous shift.
        // This will destroy the previous shifts edit component,
        // since we need a new one to be created to ensure the form is reset correctly.
        this.selectedShiftIndex.set(undefined);
        // Wait for the next change detection cycle to ensure the UI updates before selecting the new shift.
        setTimeout(() => {
          this.selectedShiftIndex.set(idx);
          if (addShift) this.addShift();
        }, 200);
      }
    } else {
      if (addShift) this.addShift();
    }
  }

  addShift() {
    // Use selected shift as source if available.
    // If no selected shift, use last shift as source (can happen if right clicking a slot and adding a new shift on an existing day).
    // Always use last shift's stopTime as startTime for the new shift.

    const selectedShift = this.selectedShiftForm;
    const lastShift = this.lastShiftForm;
    const sourceShift = selectedShift || lastShift;

    const keepLink: boolean = false;
    // (this.keepShiftsTogether || this.isHidden) &&
    // !standby &&
    // !createFromStandby &&
    // !onDuty &&
    // !createFromOnDuty;

    // Create a new empty shift form
    const emptyShift = new PlanningShiftDTO();
    emptyShift.type = TermGroup_TimeScheduleTemplateBlockType.Schedule;
    emptyShift.actualStartTime = this.form.date.value;
    emptyShift.actualStopTime = this.form.date.value;
    const shiftForm = ShiftUtil.createShiftFormFromDTO(SpShiftEditForm, {
      validationHandler: this.validationHandler,
      element: emptyShift,
    });

    //const shift: PlanningShiftDTO = new PlanningShiftDTO();
    if (sourceShift) {
      // Copy values from the source
      shiftForm.patchValue(sourceShift.value, { emitEvent: false });

      // Clear values that should not be copied
      shiftForm.patchValue(
        {
          timeScheduleTemplateBlockId: 0,
          tempTimeScheduleTemplateBlockId: 0,
          break1Id: 0,
          break2Id: 0,
          break3Id: 0,
          break4Id: 0,
          nbrOfWantedInQueue: 0,
          isLinked: keepLink,
          isCreatedAsFirstOnDay: false,
        },
        { emitEvent: false }
      );

      if (!keepLink) shiftForm.patchValue({ link: Guid.newGuid() });
      // if (!keepTasks) dto.tasks = [];
    } else {
      // If no source shift, create a new empty shift
      shiftForm.patchValue(
        {
          type: emptyShift.type,
          actualStartDate: emptyShift.actualStartDate,
          actualStartTime: emptyShift.actualStartTime,
          actualStopTime: emptyShift.actualStopTime,
          link: Guid.newGuid(),
          isCreatedAsFirstOnDay: true,
        },
        { emitEvent: false }
      );
      //shift.dayNumber
    }

    // Set properties that are always set for a new shift.
    // Need to create new dates,
    // otherwise original form.date will be changed when changing start date,
    // and changing start time will also change stop time.
    const start: Date = new Date(
      lastShift ? lastShift.value.actualStopTime : this.form.date.value
    );
    const stop: Date = new Date(start);
    shiftForm.patchValue(
      {
        startTime: start,
        actualStartTime: start,
        stopTime: stop,
        actualStopTime: stop,
        employeeId: this.form.employeeId.value,
        employeeName: this.form.controls.employeeName.value,
        isModified: true,
      },
      { emitEvent: false }
    );

    this.form.addShift(shiftForm);
    this.selectShift(this.form.shifts.controls.length - 1);
  }

  deleteShift(shiftForm: SpShiftEditForm | undefined = undefined) {
    if (!shiftForm && this.selectedShiftIndex() === undefined) return;

    if (!shiftForm)
      shiftForm = this.form.shifts.controls[this.selectedShiftIndex()!];
    if (!shiftForm) return;

    this.form.deleteShift(shiftForm);
    //this.setShiftTypeIds();

    // if (shiftForm.isBreak) {
    //   this.setBreaksOnShifts();

    //   // Set shift which break belongs to as modified
    //   this.form.shifts.value
    //     .filter(s => !s.isBreak && !s.isStandby && !s.isOnDuty)
    //     .forEach(shift => {
    //       const duration = DateUtil.getIntersectingMinutes(
    //         shift.actualStartTime,
    //         shift.actualStopTime,
    //         shiftForm.actualStartTime,
    //         shiftForm.actualStopTime
    //       );
    //       if (duration > 0) this.setModified(shift);
    //     });
    // }

    if (this.form.shifts.controls.length > 0) this.selectShift(0);
    else this.selectedShiftIndex.set(undefined);

    this.setDeletedShiftsLabel();
  }

  private onToolbarFunctionsSelected(event: any) {
    if (event) {
      switch (event.value.id) {
        case EditShiftFunctions.Absence:
          const emp = this.employeeService.getEmployee(
            this.form.employeeId.value
          );
          const shifts = this.form.toShiftsDTOs();
          if (emp && shifts) {
            this.eventService.shiftAbsence(emp, shifts);
          }
          break;
        case EditShiftFunctions.History:
          const shiftIds = this.form.shifts.value.flatMap(shift => [
            shift.timeScheduleTemplateBlockId,
            ...(shift.breaks?.map((b: any) => b.breakId) || []),
          ]);
          this.spDialogService.openShiftHistoryDialog(shiftIds).subscribe();
          break;
        case EditShiftFunctions.Accounting:
          const shiftIdsForAccounting = this.form.shifts.value.flatMap(
            shift => [
              shift.timeScheduleTemplateBlockId,
              ...(shift.breaks?.map((b: any) => b.breakId) || []),
            ]
          );
          this.spDialogService
            .openShiftAccountingDialog(shiftIdsForAccounting)
            .subscribe();
          break;
      }
    }
  }

  private onToolbarMovePrevDay() {
    this.initMoveToDate(this.form.date.value.addDays(-1));
  }

  private onToolbarMoveNextDay() {
    this.initMoveToDate(this.form.date.value.addDays(1));
  }

  private initMoveToDate(date: Date) {
    if (this.form.dirty) {
      this.askDiscardChanges().subscribe(proceed => {
        if (proceed) this.moveToDate(date);
      });
    } else {
      this.moveToDate(date);
    }
  }

  private moveToDate(date: Date) {
    // Unselect shift to hide edit component
    this.selectedShiftIndex.set(undefined);

    // Reset form with new date
    this.form.reset({
      date: date,
      employeeId: this.form.employeeId.value,
      employeeName: this.form.employeeName.value,
    });
    this.form.patchShifts([]);

    // Load shifts for new date
    this.loadShiftsForDay(false);
  }

  private askDiscardChanges(): Observable<boolean> {
    const dialog = this.messageboxService.question(
      'core.warning',
      'common.unsavedchanges',
      { type: 'warning' }
    );
    return dialog
      .afterClosed()
      .pipe(map((response: IMessageboxComponentResponse) => !!response.result));
  }

  cancel() {
    this.dialogRef.close({ shiftModified: false } as SpShiftDialogResult);
  }

  save() {
    this.executing.set(true);

    // Remove any empty shifts
    this.form.shifts.controls.forEach(shiftForm => {
      if (shiftForm.shiftLength === 0) {
        this.deleteShift(shiftForm);
      } else {
        // Remove any empty breaks
        let breakIndex = 0;
        shiftForm.breaks.controls.forEach(breakForm => {
          if (breakForm.minutes === 0) {
            shiftForm.deleteBreak(breakIndex);
          }
          breakIndex++;
        });
      }
    });

    const shiftDTOs: PlanningShiftDTO[] = this.form.toShiftsDTOs();

    // Common validations that does not require user interaction or modifications of shifts or breaks
    if (
      !this.validationService.validate(
        this.form.employeeId.value,
        this.form.date.value,
        shiftDTOs
      )
    ) {
      // Did not pass validation, abort save
      this.executing.set(false);
      return;
    }

    // Validate holes, with or without breaks

    const scheduleShifts = shiftDTOs
      .filter(s => !s.isOnDuty && !s.isStandby)
      .sort(ShiftUtil.sortShiftsByStartThenStop);
    const breakShifts = scheduleShifts
      .map(shift => shift.breaks)
      .flat()
      .sort(ShiftUtil.sortBreaksByStartThenStop);

    // Validate holes between shifts that have breaks inside
    this.validationService
      .validateHolesWithBreaks(scheduleShifts, breakShifts)
      .subscribe(holesWithBreaksResult => {
        if (holesWithBreaksResult.adjustedShifts?.length || 0 > 0) {
          // If any shifts were adjusted, update the form's shifts accordingly
          holesWithBreaksResult.adjustedShifts!.forEach(adjustedShift => {
            const shiftForm = this.form.getShiftEditFormById(
              adjustedShift.shift.timeScheduleTemplateBlockId,
              adjustedShift.shift.tempTimeScheduleTemplateBlockId
            );
            if (shiftForm) {
              shiftForm.patchValue({
                actualStopTime: adjustedShift.newStopTime,
              });
            }
          });

          // Retry save after adjustments
          this.executing.set(false);
          this.save();
        } else {
          // No shifts were adjusted
          // Validate holes between shifts without breaks inside
          this.validationService
            .validateHolesWithoutBreaks(scheduleShifts, breakShifts)
            .subscribe(holesWithoutBreaksResult => {
              if (!holesWithoutBreaksResult.passed) {
                // Did not pass validation, abort save
                this.executing.set(false);
                return;
              }

              if (holesWithoutBreaksResult.adjustedShifts?.length || 0 > 0) {
                // If any shifts were adjusted, update the form's shifts accordingly
                holesWithoutBreaksResult.adjustedShifts!.forEach(
                  adjustedShift => {
                    const shiftForm = this.form.getShiftEditFormById(
                      adjustedShift.shift.timeScheduleTemplateBlockId,
                      adjustedShift.shift.tempTimeScheduleTemplateBlockId
                    );
                    if (shiftForm) {
                      shiftForm.patchValue({
                        actualStopTime: adjustedShift.newStopTime,
                      });
                      if (adjustedShift.addedBreaks?.length || 0 > 0) {
                        // If any breaks were added, add them to the shift form
                        adjustedShift.addedBreaks!.forEach(addedBreak => {
                          const breakForm =
                            this.form.createShiftBreakFormFromDTO(
                              new PlanningShiftBreakDTO()
                            );

                          // Get max tempBreakId to ensure unique tempBreakId for new breaks
                          const tempBreakId =
                            breakShifts.reduce((max, b) => {
                              const id = b.tempBreakId ?? 0;
                              return id > max ? id : max;
                            }, 0) + 1;

                          const timeCode =
                            this.service.getTimeCodeBreakFromLength(
                              addedBreak.minutes
                            );

                          breakForm.patchValue({
                            tempBreakId: tempBreakId,
                            timeCodeId: timeCode?.timeCodeId || 0,
                            startTime: addedBreak.startTime,
                            stopTime: addedBreak.stopTime,
                            actualStartDate: new Date(addedBreak.startTime),
                            minutes: addedBreak.minutes,
                          });

                          shiftForm.addBreak(breakForm);
                        });
                      }
                    }
                  }
                );

                // Retry save after adjustments
                this.executing.set(false);
                this.save();
              } else {
                // All hole validations passed, continue with work rules
                if (
                  this.filterService.isTemplateView() ||
                  this.filterService.isEmployeePostView()
                ) {
                  // TODO: Template shifts have their own validate work rules and save method
                } else {
                  this.perform.load(
                    this.validationService
                      .validateWorkRules(this.form.employeeId.value, shiftDTOs)
                      .pipe(
                        tap(result => {
                          this.executing.set(false);

                          this.workRuleService
                            .showValidateWorkRulesResult(
                              TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift,
                              result,
                              this.form.employeeId.value
                            )
                            .subscribe(passed => {
                              if (passed) {
                                setTimeout(() => {
                                  // Wait for work rules progress dialog to close before saving shifts.
                                  // Otherwise the save progress dialog will not show.
                                  const deletedShiftDTOs: PlanningShiftDTO[] =
                                    this.form.toDeletedShiftsDTOs();

                                  this.performSaveShifts(
                                    shiftDTOs,
                                    deletedShiftDTOs
                                  );
                                }, 100);
                              }
                            });
                        })
                      ),
                    {
                      message:
                        'time.schedule.planning.evaluateworkrules.executing',
                    }
                  );
                }
              }
            });
        }
      });
  }

  private performSaveShifts(
    shiftDTOs: PlanningShiftDTO[],
    deletedShiftDTOs: PlanningShiftDTO[]
  ) {
    this.executing.set(true);

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.shiftService.saveShifts(
        'SpShiftDialog',
        shiftDTOs.concat(deletedShiftDTOs)
      ),
      (result: BackendResponse) => {
        if (result.success) {
          this.dialogRef.close({
            shiftModified: true,
          } as SpShiftDialogResult);
        } else {
          this.executing.set(false);
        }
      }
    );
  }

  // HELP-METHODS

  private get selectedShiftForm(): SpShiftEditForm | undefined {
    // Get selected shift from the form's shifts array.
    if (this.noSelectedShift()) return undefined;

    return this.form.shifts.controls[
      this.selectedShiftIndex()!
    ] as SpShiftEditForm;
  }

  private get lastShiftForm(): SpShiftEditForm | undefined {
    // Get last shift from the form's shifts array sorted by stopTime.
    if (this.form.shifts.controls.length === 0) return undefined;

    return this.form.shifts.controls.reduce(
      (latest, current) => {
        return !latest || current.value.stopTime > latest.value.stopTime
          ? current
          : latest;
      },
      undefined as SpShiftEditForm | undefined
    );
  }

  private setDeletedShiftsLabel() {
    if (this.form.deletedShifts.length > 0) {
      // TODO: New terms
      this.deletedShiftsLabel.set(
        `${this.form.deletedShifts.length} ${this.form.deletedShifts.length > 1 ? 'pass borttagna' : 'pass borttaget'}`
      );
    } else {
      this.deletedShiftsLabel.set('');
    }
  }
}
