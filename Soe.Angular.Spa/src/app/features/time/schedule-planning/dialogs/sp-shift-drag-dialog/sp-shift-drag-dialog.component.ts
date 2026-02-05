import {
  Component,
  computed,
  effect,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { IconModule } from '@ui/icon/icon.module';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { PlanningShiftDTO } from '../../models/shift.model';
import { DatePipe } from '@angular/common';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { TranslateService } from '@ngx-translate/core';
import {
  DragShiftAction,
  SoeScheduleWorkRules,
  TermGroup_ShiftHistoryType,
  TermGroup_TimeScheduleTemplateBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { RadioComponent } from '@ui/forms/radio/radio.component';
import { ValidationHandler } from '@shared/handlers';
import { SpShiftDragDialogForm } from './sp-shift-drag-dialog-form.model';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { SpTranslateService } from '../../services/sp-translate.service';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { DateUtil } from '@shared/util/date-util';
import { SpFilterService } from '../../services/sp-filter.service';
import { SpSettingService } from '../../services/sp-setting.service';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { TimeDeviationCausesService } from '@features/time/time-deviation-causes/services/time-deviation-causes.service';
import { Observable, of, tap, forkJoin, map } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SelectComponent } from '@ui/forms/select/select.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { ITimeDeviationCauseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { EmployeeService } from '@features/time/services/employee.service';
import { TermCollection } from '@shared/localization/term-types';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SkillService } from '@shared/services/time/skill.service';
import { SpWorkRuleService } from '../../services/sp-work-rule.service';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import { Perform } from '@shared/util/perform.class';
import { ProgressService } from '@shared/services/progress';
import { Guid } from '@shared/util/string-util';
import { CrudActionTypeEnum } from '@shared/enums';
import { SpShiftSimpleComponent } from '../../components/sp-shift-simple/sp-shift-simple.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export class SpShiftDragDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  sourceDate!: Date;
  sourceEmployee!: PlanningEmployeeDTO;
  sourceShifts: PlanningShiftDTO[] = [];
  onDutyShifts: PlanningShiftDTO[] = [];
  targetDate!: Date;
  targetEmployee!: PlanningEmployeeDTO;
  targetShifts: PlanningShiftDTO[] = [];
  defaultAction = DragShiftAction.Move;
  executeDefaultAction = false;
  moveOffsetDays = 0;
}

export class SpShiftDragDialogResult {
  shiftModified: boolean = false;
}

@Component({
  selector: 'sp-shift-drag-dialog',
  imports: [
    DialogComponent,
    DatePipe,
    ReactiveFormsModule,
    ButtonComponent,
    SaveButtonComponent,
    CheckboxComponent,
    ExpansionPanelComponent,
    IconModule,
    InstructionComponent,
    RadioComponent,
    SelectComponent,
    SpShiftSimpleComponent,
  ],
  templateUrl: './sp-shift-drag-dialog.component.html',
  styleUrl: './sp-shift-drag-dialog.component.scss',
})
export class SpShiftDragDialogComponent
  extends DialogComponent<SpShiftDragDialogData>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  form: SpShiftDragDialogForm = new SpShiftDragDialogForm({
    validationHandler: this.validationHandler,
    element: undefined,
  });

  private readonly deviationCauseService = inject(TimeDeviationCausesService);
  private readonly employeeService = inject(EmployeeService);
  private readonly filterService = inject(SpFilterService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly progressService = inject(ProgressService);
  private readonly service = inject(SchedulePlanningService);
  private readonly settingService = inject(SpSettingService);
  private readonly skillService = inject(SkillService);
  private readonly translate = inject(TranslateService);
  private readonly spTranslate = inject(SpTranslateService);
  private readonly workRuleService = inject(SpWorkRuleService);

  // Lookups
  timeDeviationCauses: ITimeDeviationCauseDTO[] = [];
  employeeChilds: SmallGenericType[] = [];

  // Actions
  DragShiftAction = DragShiftAction;

  selectedAction = toSignal(this.form.get('action')!.valueChanges, {
    initialValue: this.form.action,
  });

  selectedActionIsMove = computed(
    () => this.selectedAction() === DragShiftAction.Move
  );
  selectedActionIsCopy = computed(
    () => this.selectedAction() === DragShiftAction.Copy
  );
  selectedActionIsAbsence = computed(
    () => this.selectedAction() === DragShiftAction.Absence
  );

  selectedDeviationCause = toSignal(
    this.form.get('timeDeviationCauseId')!.valueChanges,
    {
      initialValue: this.form.controls.timeDeviationCauseId.value,
    }
  );

  selectedDeviationCauseRequiresChild = computed(() => {
    const timeDeviationCauseId = this.selectedDeviationCause();
    const deviationCause = this.timeDeviationCauses.find(
      c => c.timeDeviationCauseId === timeDeviationCauseId
    );
    return deviationCause ? deviationCause.specifyChild : false;
  });

  // Terms
  private terms!: TermCollection;

  private fullyUnavailableLabel = '';
  private partiallyUnavailableLabel = '';
  targetEmployeeUnavailableText = computed(() => {
    return `${this.form.targetEmployeeName} ${
      this.form.targetEmployeeIsFullyUnavailable()
        ? this.fullyUnavailableLabel
        : this.form.targetEmployeeIsPartiallyUnavailable()
          ? this.partiallyUnavailableLabel
          : ''
    }`;
  });

  private shiftExtraLabel = '';
  private shiftSubstituteLabel = '';
  private shiftExtraAndSubstituteLabel = '';
  sourceShiftExtraAndSubstituteText = computed(() => {
    if (this.form.sourceShiftIsExtra() && this.form.sourceShiftIsSubstitute()) {
      return this.shiftExtraAndSubstituteLabel;
    } else if (this.form.sourceShiftIsExtra()) {
      return this.shiftExtraLabel;
    } else if (this.form.sourceShiftIsSubstitute()) {
      return this.shiftSubstituteLabel;
    }
    return '';
  });

  informationMove = signal('');
  informationCopy = signal('');
  informationReplace = signal('');
  informationReplaceAndFree = signal('');
  informationSwapEmployee = signal('');
  informationAbsence = signal('');
  informationWholeDayAbsence = signal('');

  // Flags
  multipleSourceShifts = signal(false);
  multipleSourceDates = signal(false);

  showMove = signal(false);
  showCopy = signal(false);
  showReplace = signal(false);
  showReplaceAndFree = signal(false);
  showSwapEmployee = signal(false);
  showAbsence = signal(false);

  showOnDutyShifts = signal(false);

  private perform = new Perform<any>(this.progressService);
  executing = signal(false);

  private isOverlappingForCopy = false;
  private isOverlappingForMove = false;
  private isOverlappingLended = false;

  // TODO: Implement?
  // Not used in AngularJS either?
  private copyTaskWithShift = false;

  constructor() {
    super();

    effect(() => {
      const action = this.selectedAction();
      this.onActionChanged(action);
    });

    effect(() => {
      const timeDeviationCauseId = this.selectedDeviationCause();
      this.onDeviationCauseChanged(timeDeviationCauseId);
    });
  }

  ngOnInit(): void {
    this.patchForm();
    this.multipleSourceShifts.set(this.form.sourceShifts.length > 1);
    this.multipleSourceDates.set(this.form.multipleSourceDates);

    this.setupLabels();
    this.showActions();
    this.setDefaultAction();

    // TODO: Check if ok to execute default action immediately
    if (this.data.executeDefaultAction) {
      this.initSave();
    }
  }

  private patchForm() {
    this.form.reset({
      sourceDate: this.data.sourceDate,
      sourceEmployee: this.data.sourceEmployee,
      targetDate: this.data.targetDate,
      targetEmployee: this.data.targetEmployee,
      action: this.data.defaultAction || DragShiftAction.Move,
      moveOffsetDays: this.data.moveOffsetDays || 0,
    });
    this.form.patchSourceShifts(this.data.sourceShifts);
    this.form.patchOnDutyShifts(this.data.onDutyShifts);
    this.form.patchTargetShifts(this.data.targetShifts);

    this.form.setHiddenEmployeeId(this.service.hiddenEmployeeId);
    this.form.setSourceEmployee(this.data.sourceEmployee);
    this.form.setTargetEmployee(this.data.targetEmployee);

    if (
      this.settingService.onDutyShiftsModifyPermission() &&
      this.filterService.isScheduleView() &&
      this.form.onDutyShiftForms.length > 0
    )
      this.form.patchValue({ includeOnDutyShifts: true });
  }

  private setupLabels() {
    this.translate
      .get([
        'common.obs',
        'time.schedule.planning.editshift.fullyunavailable',
        'time.schedule.planning.editshift.partlyunavailable',
        'time.schedule.planning.dragshift.extrashiftinfo',
        'time.schedule.planning.dragshift.substituteinfo',
        'time.schedule.planning.dragshift.extrashiftandsubstituteinfo',
        'time.schedule.planning.dragshift.information.move',
        'time.schedule.planning.dragshift.information.copy',
        'time.schedule.planning.dragshift.information.copymultiple',
        'time.schedule.planning.dragshift.information.replace',
        'time.schedule.planning.dragshift.information.replaceandfree',
        'time.schedule.planning.dragshift.information.swapemployee',
        'time.schedule.planning.dragshift.information.absence',
        'time.schedule.planning.dragshift.information.absencemultiple',
        'time.schedule.planning.dragshift.information.wholedayabsence',
        'time.schedule.planning.editshift.missingskills',
        'time.schedule.planning.editshift.missingskillsoverride',
      ])
      .subscribe(terms => {
        this.terms = terms;

        this.fullyUnavailableLabel =
          terms['time.schedule.planning.editshift.fullyunavailable'];
        this.partiallyUnavailableLabel =
          terms['time.schedule.planning.editshift.partlyunavailable'];

        this.shiftExtraLabel =
          terms['time.schedule.planning.dragshift.extrashiftinfo'];
        this.shiftSubstituteLabel =
          terms['time.schedule.planning.dragshift.substituteinfo'];
        this.shiftExtraAndSubstituteLabel =
          terms['time.schedule.planning.dragshift.extrashiftandsubstituteinfo'];

        this.informationMove.set(
          terms['time.schedule.planning.dragshift.information.move'].format(
            (this.multipleSourceShifts()
              ? this.spTranslate.shiftsDefined()
              : this.spTranslate.shiftDefined()
            ).toUpperCaseFirstLetter()
          )
        );
        this.informationCopy.set(
          (this.multipleSourceShifts()
            ? terms['time.schedule.planning.dragshift.information.copymultiple']
            : terms['time.schedule.planning.dragshift.information.copy']
          ).format(
            (this.multipleSourceShifts()
              ? this.spTranslate.shiftsDefined()
              : this.spTranslate.shiftDefined()
            ).toUpperCaseFirstLetter()
          )
        );
        this.informationReplace.set(
          terms['time.schedule.planning.dragshift.information.replace'].format(
            (this.multipleSourceShifts()
              ? this.spTranslate.shiftsDefined()
              : this.spTranslate.shiftDefined()
            ).toUpperCaseFirstLetter()
          )
        );
        this.informationReplaceAndFree.set(
          terms[
            'time.schedule.planning.dragshift.information.replaceandfree'
          ].format(
            (this.multipleSourceShifts()
              ? this.spTranslate.shiftsDefined()
              : this.spTranslate.shiftDefined()
            ).toUpperCaseFirstLetter()
          )
        );
        this.informationSwapEmployee.set(
          terms[
            'time.schedule.planning.dragshift.information.swapemployee'
          ].format(this.spTranslate.shiftsDefined().toUpperCaseFirstLetter())
        );
        this.informationAbsence.set(
          (this.multipleSourceShifts()
            ? terms[
                'time.schedule.planning.dragshift.information.absencemultiple'
              ]
            : terms['time.schedule.planning.dragshift.information.absence']
          ).format(
            (this.multipleSourceShifts()
              ? this.spTranslate.shiftsDefined()
              : this.spTranslate.shiftDefined()
            ).toUpperCaseFirstLetter()
          )
        );
        this.informationWholeDayAbsence.set(
          terms[
            'time.schedule.planning.dragshift.information.wholedayabsence'
          ].format(this.spTranslate.shiftsUndefined())
        );
      });
  }

  // SERVICE CALLS

  private loadTimeDeviationCauses() {
    if (this.timeDeviationCauses.length > 0) return;

    this.deviationCauseService
      .getTimeDeviationCausesAbsenceFromEmployeeId(
        this.form.sourceEmployeeId,
        this.form.sourceDate,
        false
      )
      .pipe(
        tap(x => {
          this.timeDeviationCauses = x;
        })
      )
      .subscribe();
  }

  private loadChildren() {
    if (this.employeeChilds.length > 0) return;

    this.employeeService
      .getEmployeeChildsDict(this.form.sourceEmployeeId, false)
      .pipe(
        tap(x => {
          this.employeeChilds = x;
          // If only one child, select it
          if (this.employeeChilds.length === 1) {
            this.form.patchValue(
              { employeeChildId: this.employeeChilds[0].id },
              { emitEvent: false }
            );
          }
        })
      )
      .subscribe();
  }

  // HELP-METHODS

  private showActions() {
    // Same account is mandatory for all actions
    if (!this.form.sameAccount) return;

    this.setOverlapping();

    // Move
    this.showMove.set(
      !this.isOverlappingForMove && !this.form.sourceShiftIsAbsence
    );

    // Copy
    this.showCopy.set(!this.isOverlappingForCopy);

    // Advanced actions
    const validForAdvancedActions =
      !this.form.sourceShiftIsStandby &&
      !this.form.sourceShiftIsOnDuty &&
      !this.form.sourceShiftIsBooking &&
      !this.form.multipleSourceGuids &&
      !this.form.targetShiftIsAbsence;
    if (!validForAdvancedActions) return;

    const validForReplaceAndSwap =
      !this.form.targetSlotIsEmpty &&
      !this.form.targetShiftIsLended &&
      !this.isOverlappingLended;

    const isTemplateOrEmployeePostView =
      this.filterService.isTemplateView() ||
      this.filterService.isEmployeePostView();

    if (validForReplaceAndSwap) {
      // Replace
      this.showReplace.set(!isTemplateOrEmployeePostView);

      // Replace and free
      this.showReplaceAndFree.set(
        !isTemplateOrEmployeePostView &&
          !this.filterService.isScenarioView() &&
          !this.settingService.useVacant()
      );

      // Swap employee
      this.showSwapEmployee.set(!this.form.sourceAndTargetIsSameEmployee);
    }

    // Absence
    this.showAbsence.set(
      !this.isOverlappingForCopy &&
        !this.isOverlappingForMove &&
        !isTemplateOrEmployeePostView &&
        !this.filterService.isScenarioView() &&
        !this.form.sourceAndTargetIsSameEmployee
    );
  }

  private setOverlapping() {
    if (this.form.targetShifts.length === 0) return;

    // Validate that source and target times do not overlap (except for hidden employee or on duty shifts)
    if (!this.form.targetEmployeeIsHidden && !this.form.sourceShiftIsOnDuty) {
      // Check all source shifts against all target shifts
      this.form.sourceShifts.forEach(source => {
        this.isOverlappingTargets(source, false);
        this.isOverlappingTargets(source, true);
      });
    }
  }

  private isOverlappingTargets(
    source: PlanningShiftDTO,
    checkForMove: boolean
  ) {
    this.form.targetShifts.forEach(target => {
      if (
        target.timeScheduleTemplateBlockId !==
        source.timeScheduleTemplateBlockId
      ) {
        const sourceStart: Date = source.actualStartTime.addDays(
          this.form.moveOffsetDays
        );
        const sourceStop: Date = source.actualStopTime.addDays(
          this.form.moveOffsetDays
        );
        let targetStart: Date = target.actualStartTime;
        let targetStop: Date = target.actualStopTime;

        if (
          checkForMove &&
          this.form.sourceShifts
            .map(s => s.timeScheduleTemplateBlockId)
            .includes(target.timeScheduleTemplateBlockId)
        ) {
          targetStart = targetStart.addDays(this.form.moveOffsetDays);
          targetStop = targetStop.addDays(this.form.moveOffsetDays);
        }

        if (
          DateUtil.getIntersectingMinutes(
            sourceStart,
            sourceStop,
            targetStart,
            targetStop
          ) > 0
        ) {
          if (this.filterService.isSchedulePlanningMode()) {
            if (checkForMove) {
              this.isOverlappingForMove = true;
            } else {
              this.isOverlappingForCopy = true;
            }
            this.isOverlappingLended = target.isLended;
          } else if (this.filterService.isOrderPlanningMode()) {
            switch (source.type) {
              case TermGroup_TimeScheduleTemplateBlockType.Schedule:
                if (target.isSchedule || target.isStandby) {
                  if (checkForMove) {
                    this.isOverlappingForMove = true;
                  } else {
                    this.isOverlappingForCopy = true;
                  }
                }
                break;
              case TermGroup_TimeScheduleTemplateBlockType.Order:
              case TermGroup_TimeScheduleTemplateBlockType.Booking:
                if (target.isOrder || target.isBooking) {
                  if (checkForMove) {
                    this.isOverlappingForMove = true;
                  } else {
                    this.isOverlappingForCopy = true;
                  }
                }
                break;
            }
          }
        }
      }
    });
  }

  private setDefaultAction() {
    // Check if default action is possible, otherwise set first available action

    let action = this.form.action;
    if (action === DragShiftAction.Move && !this.showMove())
      action = DragShiftAction.Cancel;
    if (action === DragShiftAction.Copy && !this.showCopy())
      action = DragShiftAction.Cancel;
    if (action === DragShiftAction.Replace && !this.showReplace())
      action = DragShiftAction.Cancel;
    if (action === DragShiftAction.ReplaceAndFree && !this.showReplaceAndFree())
      action = DragShiftAction.Cancel;
    if (action === DragShiftAction.SwapEmployee && !this.showSwapEmployee())
      action = DragShiftAction.Cancel;
    if (action === DragShiftAction.Absence && !this.showAbsence())
      action = DragShiftAction.Cancel;

    if (action === DragShiftAction.Cancel) {
      this.data.executeDefaultAction = false; // Prevent auto execution if default action is not possible

      if (this.showMove()) action = DragShiftAction.Move;
      else if (this.showCopy()) action = DragShiftAction.Copy;
      else if (this.showReplace()) action = DragShiftAction.Replace;
      else if (this.showReplaceAndFree())
        action = DragShiftAction.ReplaceAndFree;
      else if (this.showSwapEmployee()) action = DragShiftAction.SwapEmployee;
      else if (this.showAbsence()) action = DragShiftAction.Absence;
    }

    this.form.patchValue({ action: action }, { emitEvent: false });
    this.form.markAsDirty(); // Mark form as dirty to enable save button
  }

  private setShowOnDutyShifts() {
    if (
      this.settingService.onDutyShiftsModifyPermission() &&
      this.filterService.isScheduleView() &&
      (this.form.onDutyShifts.length > 0 ||
        this.form.targetShiftsOnDuty.length > 0)
    ) {
      this.form.patchValue({ includeOnDutyShifts: true }, { emitEvent: false });
      this.showOnDutyShifts.set(true);

      if (
        this.form.onDutyShifts.length === 0 &&
        (this.selectedActionIsCopy() ||
          this.selectedActionIsMove() ||
          this.selectedActionIsAbsence())
      ) {
        this.form.patchValue(
          { includeOnDutyShifts: false },
          { emitEvent: false }
        );
        this.showOnDutyShifts.set(false);
      }
    }
  }

  private getSourceEmployeeIdentifier(): number {
    return this.filterService.isEmployeePostView()
      ? this.form.controls.sourceEmployeePostId.value
      : this.form.sourceEmployeeId;
  }

  private getTargetEmployeeIdentifier(): number {
    return this.filterService.isEmployeePostView()
      ? this.form.controls.targetEmployeePostId.value
      : this.form.targetEmployeeId;
  }

  // EVENTS

  private onActionChanged(action: DragShiftAction) {
    // If selected action is absence, load deviation causes
    if (action === DragShiftAction.Absence) {
      this.loadTimeDeviationCauses();
    }

    // Deviation cause is mandatory for absence
    const causeCtrl = this.form.controls.timeDeviationCauseId;
    const childCtrl = this.form.controls.employeeChildId;
    if (action === DragShiftAction.Absence) {
      causeCtrl.setValidators([Validators.required]);
    } else {
      causeCtrl.clearValidators();
      childCtrl.clearValidators();

      // Clear deviation cause selection
      this.form.clearTimeDeviationCause();
    }
    causeCtrl.updateValueAndValidity({ emitEvent: false });
    childCtrl.updateValueAndValidity({ emitEvent: false });

    this.setShowOnDutyShifts();
  }

  private onDeviationCauseChanged(timeDeviationCauseId: number) {
    const deviationCause = this.timeDeviationCauses.find(
      c => c.timeDeviationCauseId === timeDeviationCauseId
    );

    if (deviationCause?.specifyChild) {
      // If selected deviation cause requires child, load children
      this.loadChildren();
    } else if (this.form.controls.employeeChildId.value) {
      // Clear child selection
      this.form.clearChild();
    }

    // Set child mandatory if deviation cause requires it
    const childCtrl = this.form.controls.employeeChildId;
    if (deviationCause?.specifyChild) {
      childCtrl.setValidators([Validators.required]);
    } else {
      childCtrl.clearValidators();
    }
    childCtrl.updateValueAndValidity({ emitEvent: false });
  }

  // VALIDATION

  private validateSkills(): Observable<boolean> {
    const checks: Observable<boolean>[] = [];

    // Source shifts checked against target employee
    // Skip validation when target employee is hidden
    if (!this.form.targetEmployeeIsHidden) {
      for (const shift of this.form.sourceShifts) {
        checks.push(
          this.employeeHasSkill(this.getTargetEmployeeIdentifier(), shift)
        );
      }
    }

    // Target shifts checked against source employee when swapping employees
    if (this.selectedAction() === DragShiftAction.SwapEmployee) {
      for (const shift of this.form.targetShifts) {
        checks.push(
          this.employeeHasSkill(this.getSourceEmployeeIdentifier(), shift)
        );
      }
    }

    if (checks.length === 0) return of(true);

    return forkJoin(checks).pipe(
      map(results => {
        // Return true if all checks passed
        return results.every(r => r);
      })
    );
  }

  private employeeHasSkill(
    employeeIdentifier: number,
    shift: PlanningShiftDTO
  ): Observable<boolean> {
    if (this.filterService.isEmployeePostView()) {
      return this.skillService.employeePostHasShiftTypeSkills(
        employeeIdentifier,
        shift.shiftTypeId,
        shift.actualStartTime.addDays(this.form.moveOffsetDays)
      );
    } else {
      return this.skillService.employeeHasShiftTypeSkills(
        employeeIdentifier,
        shift.shiftTypeId,
        shift.actualStartTime.addDays(this.form.moveOffsetDays)
      );
    }
  }

  private validateWorkRules(): Observable<IEvaluateWorkRulesActionResult> {
    let rules: SoeScheduleWorkRules[] | null = null;
    if (this.settingService.skipWorkRules()) {
      // The following rules should always be evaluated
      rules = [];
      rules.push(SoeScheduleWorkRules.OverlappingShifts);
      if (!this.filterService.isTemplateView())
        rules.push(SoeScheduleWorkRules.AttestedDay);
    }

    if (!this.form.multipleSourceGuids) {
      // Single shift drag
      const targetShiftId: number =
        this.form.firstTargetShift?.timeScheduleTemplateBlockId ?? 0;
      const start = this.form.targetDate.mergeTime(
        this.form.firstSourceShift!.actualStartTime
      );
      const end = this.form.targetDate.mergeTime(
        this.form.firstSourceShift!.actualStopTime
      );

      return this.workRuleService.evaluateDragShiftAgainstWorkRules(
        this.selectedAction(),
        this.form.firstSourceShift!.timeScheduleTemplateBlockId,
        targetShiftId,
        start,
        end,
        this.getTargetEmployeeIdentifier(),
        this.filterService.isTemplateView(),
        this.form.isWholeDayAbsence,
        rules,
        this.filterService.isStandbyView()
      );
    } else {
      // Multiple shifts drag
      return this.workRuleService.evaluateDragShiftsAgainstWorkRules(
        this.selectedAction(),
        this.getTargetEmployeeIdentifier(),
        this.form.sourceShifts.map(s => s.timeScheduleTemplateBlockId),
        rules,
        this.form.moveOffsetDays,
        this.filterService.isTemplateView(),
        this.filterService.isStandbyView()
      );
    }
  }

  private getWorkruleActionFromDragAction(): TermGroup_ShiftHistoryType {
    switch (this.selectedAction()) {
      case DragShiftAction.Move:
        return TermGroup_ShiftHistoryType.DragShiftActionMove;
      case DragShiftAction.Copy:
        return TermGroup_ShiftHistoryType.DragShiftActionCopy;
      case DragShiftAction.Replace:
        return TermGroup_ShiftHistoryType.DragShiftActionReplace;
      case DragShiftAction.ReplaceAndFree:
        return TermGroup_ShiftHistoryType.DragShiftActionReplaceAndFree;
      case DragShiftAction.SwapEmployee:
        return TermGroup_ShiftHistoryType.DragShiftActionSwapEmployee;
      case DragShiftAction.Absence:
        return TermGroup_ShiftHistoryType.DragShiftActionAbsence;
      case DragShiftAction.Delete:
        return TermGroup_ShiftHistoryType.DragShiftActionDelete;
      default:
        return TermGroup_ShiftHistoryType.Unknown;
    }
  }

  cancel() {
    this.dialogRef.close({ shiftModified: false } as SpShiftDragDialogResult);
  }

  initSave() {
    this.executing.set(true);

    // Skills
    this.validateSkills().subscribe(skillsPassed => {
      this.executing.set(false);
      this.skillService
        .showValidateSkillsResult(
          skillsPassed,
          this.spTranslate.shiftUndefined()
        )
        .subscribe(proceed => {
          if (proceed) {
            // Work rules
            this.perform.load(
              this.validateWorkRules().pipe(
                tap(result => {
                  this.executing.set(false);

                  this.workRuleService
                    .showValidateWorkRulesResult(
                      this.getWorkruleActionFromDragAction(),
                      result,
                      this.form.targetEmployeeId
                    )
                    .subscribe(rulesPassed => {
                      if (rulesPassed) this.performSave();
                    });
                })
              ),
              { message: 'time.schedule.planning.evaluateworkrules.executing' }
            );
          }
        });
    });
  }

  private performSave() {
    this.executing.set(true);

    if (!this.form.multipleSourceGuids) {
      // Single shift drag

      // Get target link (Guid on target shift) or create new if target shift's link is empty (old data)
      let targetLink: string | undefined = undefined;
      let updateLinkOnTarget = false;
      // Target shifts with no account or same account as source shift
      const target: PlanningShiftDTO | undefined = this.form.targetShifts
        .filter(
          t =>
            !this.form.firstSourceShift!.accountId ||
            t.accountId === this.form.firstSourceShift!.accountId
        )
        .find(s => s.link && s.type === this.form.firstSourceShift!.type);

      if (target) {
        targetLink = target.link;
        if (!targetLink) {
          // If no Guid exists on target (old data or no shifts in target slot),
          // create a new Guid and set 'updateLinkOnTarget' flag so the target shift(s) will be updated with the new link on the server
          targetLink = Guid.newGuid();
          updateLinkOnTarget = true;
        }
      }

      const targetShiftId: number =
        this.form.firstTargetShift?.timeScheduleTemplateBlockId ?? 0;
      const start = this.form.targetDate.mergeTime(
        this.form.firstSourceShift!.actualStartTime
      );
      const end = this.form.targetDate.mergeTime(
        this.form.firstSourceShift!.actualStopTime
      );

      this.perform.crud(
        CrudActionTypeEnum.Save,
        this.service
          .dragShift(
            this.selectedAction(),
            this.form.firstSourceShift!.timeScheduleTemplateBlockId,
            targetShiftId,
            start,
            end,
            this.getTargetEmployeeIdentifier(),
            targetLink!,
            updateLinkOnTarget,
            this.form.controls.timeDeviationCauseId.value ?? 0,
            this.form.controls.employeeChildId.value ?? null,
            this.form.isWholeDayAbsence,
            this.settingService.skipXEMailOnChanges(),
            this.copyTaskWithShift,
            this.filterService.isStandbyView()
          )
          .pipe(
            tap((res: BackendResponse) => {
              this.executing.set(false);
              if (res.success)
                this.dialogRef.close({
                  shiftModified: true,
                } as SpShiftDragDialogResult);
            })
          )
      );
    } else {
      // Multiple shifts drag
      this.perform.crud(
        CrudActionTypeEnum.Save,
        this.service
          .dragShifts(
            this.selectedAction(),
            this.form.sourceShifts.map(s => s.timeScheduleTemplateBlockId),
            this.form.moveOffsetDays,
            this.getTargetEmployeeIdentifier(),
            this.settingService.skipXEMailOnChanges(),
            this.copyTaskWithShift,
            this.filterService.isStandbyView()
          )
          .pipe(
            tap((res: BackendResponse) => {
              this.executing.set(false);
              if (res.success)
                this.dialogRef.close({
                  shiftModified: true,
                } as SpShiftDragDialogResult);
            })
          )
      );
    }
  }
}
