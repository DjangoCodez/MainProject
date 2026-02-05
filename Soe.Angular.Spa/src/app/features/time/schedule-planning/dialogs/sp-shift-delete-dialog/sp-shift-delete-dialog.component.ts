import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { IconModule } from '@ui/icon/icon.module';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import {
  PlanningShiftBreakDTO,
  PlanningShiftDTO,
} from '../../models/shift.model';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { ShiftUtil } from '../../util/shift-util';
import { SpShiftService } from '../../services/sp-shift.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { SpTranslateService } from '../../services/sp-translate.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Observable, tap } from 'rxjs';
import { TermCollection } from '@shared/localization/term-types';
import { ValidationHandler } from '@shared/handlers';
import { SpShiftDeleteDialogForm } from './sp-shift-delete-dialog-form.model';
import { ReactiveFormsModule } from '@angular/forms';
import { SpSettingService } from '../../services/sp-setting.service';
import { SpFilterService } from '../../services/sp-filter.service';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import { SpWorkRuleService } from '../../services/sp-work-rule.service';
import {
  DragShiftAction,
  SoeScheduleWorkRules,
  TermGroup_ShiftHistoryType,
} from '@shared/models/generated-interfaces/Enumerations';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SpToolbarEmployeeDate } from '../../toolbar/sp-toolbar-employee-date/sp-toolbar-employee-date';
import { SpShiftSimpleComponent } from '../../components/sp-shift-simple/sp-shift-simple.component';
import { SpShiftDeleteDialogShiftForm } from './sp-shift-delete-dialog-shift-form.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export class SpShiftDeleteDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  employee!: PlanningEmployeeDTO;
  shifts!: PlanningShiftDTO[];
  onDutyShifts: PlanningShiftDTO[] = [];
}

export class SpShiftDeleteDialogResult {
  shiftDeleted = false;
}

@Component({
  selector: 'sp-shift-delete-dialog',
  imports: [
    ButtonComponent,
    CheckboxComponent,
    DatePipe,
    DialogComponent,
    IconModule,
    InstructionComponent,
    ReactiveFormsModule,
    SpShiftSimpleComponent,
    SpToolbarEmployeeDate,
    ToolbarComponent,
    TranslatePipe,
    LowerCasePipe,
  ],
  templateUrl: './sp-shift-delete-dialog.component.html',
  styleUrl: './sp-shift-delete-dialog.component.scss',
})
export class SpShiftDeleteDialogComponent
  extends DialogComponent<SpShiftDeleteDialogData>
  implements OnInit
{
  private readonly service = inject(SchedulePlanningService);
  readonly filterService = inject(SpFilterService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly progressService = inject(ProgressService);
  readonly settingService = inject(SpSettingService);
  private readonly shiftService = inject(SpShiftService);
  private readonly spTranslateService = inject(SpTranslateService);
  private readonly translate = inject(TranslateService);
  private readonly workRuleService = inject(SpWorkRuleService);

  validationHandler = inject(ValidationHandler);
  form: SpShiftDeleteDialogForm = new SpShiftDeleteDialogForm({
    validationHandler: this.validationHandler,
    element: undefined,
  });

  private perform = new Perform<any>(this.progressService);
  executing = signal(false);

  private shifts: PlanningShiftDTO[] = [];
  firstShift = signal<PlanningShiftDTO | undefined>(undefined);
  lastShift = signal<PlanningShiftDTO | undefined>(undefined);
  multipleDates = computed((): boolean => {
    return (
      !!this.firstShift() &&
      !!this.lastShift() &&
      !this.firstShift()!.actualStartDate.isSameDay(
        this.lastShift()!.actualStartDate
      )
    );
  });

  onlyAbsence = signal(false);
  selectInfo = signal('');
  nbrOfSelectedShifts = signal(0);

  ngOnInit(): void {
    let shifts = this.data.shifts;
    // Keep reference to original shifts to be able to extract break information for validation
    this.shifts = this.data.shifts;
    const onDutyShifts = this.data.onDutyShifts;

    // Exclude repeating shifts (in template view)
    shifts = shifts.filter(s => !s.originalBlockId);

    if (!this.data.employee || shifts.length === 0) this.cancel();

    const nbrOfAbsenceShifts = shifts.filter(
      s => s.timeDeviationCauseId
    ).length;
    const nbrOfRegularShifts = shifts.filter(
      s => !s.timeDeviationCauseId
    ).length;

    // Approved absence can't be deleted
    // If only absence shifts, show error
    this.onlyAbsence.set(nbrOfAbsenceShifts > 0 && nbrOfRegularShifts === 0);
    // Otherwise just filter absence away and show other shifts
    shifts = shifts.filter(s => !s.timeDeviationCauseId);

    if (shifts.length > 0) {
      ShiftUtil.sortShifts(shifts);

      this.firstShift.set(shifts[0]);
      this.lastShift.set(shifts[shifts.length - 1]);
    }

    this.form.reset({
      employeeId: this.data.employee.employeeId,
      employeeName: this.data.employee.name,
      firstDate: this.firstShift()?.actualStartDate,
      lastDate: this.lastShift()?.actualStopTime,
    });
    this.form.patchShifts(shifts);
    this.form.shiftForms.controls.forEach(shiftCtrl => {
      shiftCtrl.patchValue({ selected: true }, { emitEvent: false });
      if (this.onlyAbsence()) {
        shiftCtrl.controls.selected.disable({ emitEvent: false });
      }
    });
    this.form.patchOnDutyShifts(onDutyShifts);
    this.form.onDutyShiftForms.controls.forEach(shiftCtrl => {
      shiftCtrl.patchValue({ selected: true }, { emitEvent: false });
    });
    this.setNbrOfSelectedShifts();

    if (
      this.settingService.onDutyShiftsModifyPermission() &&
      this.filterService.isScheduleView() &&
      this.form.onDutyShiftForms.length > 0
    )
      this.form.patchValue({ includeOnDutyShifts: true });

    this.setupTerms().subscribe();
  }

  private setupTerms(): Observable<TermCollection> {
    return this.translate
      .get(['time.schedule.planning.deleteshift.selectshifts'])
      .pipe(
        tap(terms => {
          this.selectInfo.set(
            terms['time.schedule.planning.deleteshift.selectshifts'].format(
              this.spTranslateService.shiftsUndefined()
            )
          );
        })
      );
  }

  selectShift(shiftCtrl: SpShiftDeleteDialogShiftForm) {
    shiftCtrl.patchValue(
      { selected: !shiftCtrl.controls.selected.value },
      { emitEvent: false }
    );
    this.setNbrOfSelectedShifts();
  }

  shiftSelected() {
    this.setNbrOfSelectedShifts();
  }

  private setNbrOfSelectedShifts() {
    this.nbrOfSelectedShifts.set(
      this.form.shiftForms.value.filter(s => s.selected).length
    );
  }

  private validateBreaks(): boolean {
    // If all shifts are selected, no need to validate breaks
    if (this.nbrOfSelectedShifts() === this.form.shiftForms.length) return true;

    // If not all shifts are selected, check if a break overlaps any shifts
    const breaks = this.shifts.map(s => s.breaks).flat();
    // No breaks exists, no need to validate
    if (breaks.length === 0) return true;

    // It's OK if all overlapping shifts are either selected or not selected.
    // But it's not OK if one shift is selected and another is not.
    let invalidBreak = false;
    breaks.forEach((brk: PlanningShiftBreakDTO) => {
      // Get shifts that overlaps current break
      const overlappingShifts = ShiftUtil.getShiftsOverlappedByBreak(
        this.form.shiftForms.value,
        brk
      );
      if (overlappingShifts.length > 0) {
        const selectedOverlappingShifts = overlappingShifts.filter(
          s => (<any>s).selected
        );

        if (
          overlappingShifts.length !== selectedOverlappingShifts.length &&
          selectedOverlappingShifts.length !== 0
        ) {
          invalidBreak = true;
        }
      }
    });

    if (invalidBreak) {
      // TODO: New terms
      this.messageboxService.show(
        this.translate
          .instant('time.schedule.planning.deleteshift.cantsplitshifts.title')
          .format(this.spTranslateService.shiftsDefined()),
        this.translate
          .instant('time.schedule.planning.deleteshift.cantsplitshifts.message')
          .format(
            this.spTranslateService.shiftsDefined().toUpperCaseFirstLetter()
          ),
        { type: 'forbidden' }
      );
      return false;
    }

    return true;
  }

  private validateWorkRules(
    shiftIds: number[]
  ): Observable<IEvaluateWorkRulesActionResult> {
    let rules: SoeScheduleWorkRules[] | null = null;
    if (this.settingService.skipWorkRules()) {
      // The following rules should always be evaluated
      rules = [];
      rules.push(SoeScheduleWorkRules.OverlappingShifts);
      if (!this.filterService.isTemplateView())
        rules.push(SoeScheduleWorkRules.AttestedDay);
    }

    return this.workRuleService.evaluateDragShiftsAgainstWorkRules(
      DragShiftAction.Delete,
      this.form.controls.employeeId.value,
      shiftIds,
      rules
    );
  }

  private performDeleteShifts(shiftIds: number[]) {
    this.executing.set(true);

    const selectedShifts = this.data.shifts.filter(s =>
      shiftIds.includes(s.timeScheduleTemplateBlockId)
    );

    const selectedOnDutyShiftIds = this.form.onDutyShiftForms.value
      .filter(s => s.selected)
      .map(s => s.timeScheduleTemplateBlockId);

    // TODO: Scenario not implemented
    setTimeout(() => {
      // A timeout is needed for the validateWorkRules dialog to close before starting this new one.
      // Otherwise the validateWorkRules completed will close the new crud dialog.
      this.perform.crud(
        CrudActionTypeEnum.Delete,
        this.shiftService.deleteShifts(
          selectedShifts,
          undefined,
          selectedOnDutyShiftIds
        ),
        (result: BackendResponse) => {
          if (result.success) {
            this.dialogRef.close({
              shiftDeleted: true,
            } as SpShiftDeleteDialogResult);
          } else {
            this.executing.set(false);
          }
        }
      );
    }, 0);
  }

  cancel() {
    this.dialogRef.close({ shiftDeleted: false } as SpShiftDeleteDialogResult);
  }

  ok() {
    this.executing.set(true);

    const isValid = this.validateBreaks();
    if (!isValid) return;

    const selectedShiftIds = this.form.shiftForms.value
      .filter(s => s.selected)
      .map(s => s.timeScheduleTemplateBlockId);

    if (
      this.filterService.isTemplateView() ||
      this.filterService.isEmployeePostView()
    ) {
      // TODO: Template shifts have their own validate work rules and save method
    } else {
      this.perform.load(
        this.validateWorkRules(selectedShiftIds).pipe(
          tap(result => {
            this.executing.set(false);

            this.workRuleService
              .showValidateWorkRulesResult(
                TermGroup_ShiftHistoryType.TaskDeleteTimeScheduleShift,
                result,
                this.form.controls.employeeId.value
              )
              .subscribe(passed => {
                if (passed) this.performDeleteShifts(selectedShiftIds);
              });
          })
        ),
        { message: 'time.schedule.planning.evaluateworkrules.executing' }
      );
    }
  }
}
