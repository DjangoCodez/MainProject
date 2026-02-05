import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { PlacementsService } from '@features/time/placements/services/placements.service';
import { PlanningEmployeeDTO } from '@features/time/schedule-planning/models/employee.model';
import { SpWorkRuleService } from '@features/time/schedule-planning/services/sp-work-rule.service';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import {
  CompanySettingType,
  SoeScheduleWorkRules,
  TermGroup,
  TermGroup_ShiftHistoryType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import {
  IEmployeeRequestDTO,
  IExtendedAbsenceSettingDTO,
  ITimeDeviationCauseDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { ButtonComponent } from '@ui/button/button/button.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { forkJoin, Observable, tap } from 'rxjs';
import { AbsenceShiftsComponent } from '../../components/absence-shifts/absence-shifts.component';
import { EmployeeRequestsDTO } from '../../models/employee-request.model';
import { AbsenceService } from '../../services/absence.service';
import {
  AbsenceQuickDialogForm,
  createFullDayValidator,
} from './absence-quick-dialog-form.model';

export interface IAbsenceQuickDialogData extends DialogData {
  employeeId: number;
  employeeName: string;
  dateFrom: Date; // string instead? or skip
  dateTo: Date; // string instead? or skip
  shiftIds: number[];
  timeScheduleScenarioHeadId?: number;
}

export interface IAbsenceQuickDialogResult {
  affectedEmployeeIds?: number[];
}
@Component({
  selector: 'soe-absence-quick-dialog',
  imports: [
    DialogComponent,
    ReactiveFormsModule,
    SelectComponent,
    AbsenceShiftsComponent,
    ButtonComponent,
    SaveButtonComponent,
    AutocompleteComponent,
    CheckboxComponent,
    DatePipe,
  ],
  templateUrl: './absence-quick-dialog.component.html',
  styleUrl: './absence-quick-dialog.component.scss',
  providers: [FlowHandlerService],
})
export class AbsenceQuickDialogComponent
  extends DialogComponent<IAbsenceQuickDialogData>
  implements OnInit
{
  readonly service = inject(AbsenceService);
  private readonly placementsService = inject(PlacementsService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly toasterService = inject(ToasterService);
  private readonly coreService = inject(CoreService);
  private readonly workRuleService = inject(SpWorkRuleService);
  private readonly progressService = inject(ProgressService);
  private readonly translateService = inject(TranslateService);
  performAction = new Perform<any>(this.progressService);

  public form: AbsenceQuickDialogForm = this.createForm();

  shifts: IShiftDTO[] = [];
  shiftsRestOfDay: IShiftDTO[] = [];
  terms: TermCollection = {};

  //DialogData
  private readonly employeeId = this.data.employeeId;
  readonly employeeName = this.data.employeeName;
  readonly dateFrom = this.data.dateFrom;
  readonly dateTo = this.data.dateTo;
  private readonly shiftIds = this.data.shiftIds;
  private readonly timeScheduleScenarioHeadId =
    this.data.timeScheduleScenarioHeadId;

  // Flags
  readonly hiddenEmployeeId = signal(0);
  readonly sendXEMailOnChangeDefault = signal(false);
  // readonly setApprovedYesAsDefault = signal(false);
  readonly onlyNoReplacementIsSelectable = signal(false);
  readonly isScenario = signal(false);

  readonly timeDeviationCauseId = toSignal(
    this.form.timeDeviationCauseId.valueChanges,
    { initialValue: this.form.timeDeviationCauseId.value }
  );

  readonly showSkipSendXEMail = computed(
    () => !this.isScenario() && this.sendXEMailOnChangeDefault()
  );
  readonly showEmployeeChild = computed(() => {
    const tdcId = this.timeDeviationCauseId() ?? 0;
    const cause = this.timeDeviationCauses.find(
      c => c.timeDeviationCauseId === tdcId
    );
    return cause?.specifyChild ?? false;
  });

  // Data
  timeDeviationCauses: ITimeDeviationCauseDTO[] = [];
  employeeList: PlanningEmployeeDTO[] = [];
  replaceWithAllEmployees: SmallGenericType[] = [];
  approvalTypes: SmallGenericType[] = [];
  employeeChilds: SmallGenericType[] = [];

  ngOnInit(): void {
    this.performAction.load(
      forkJoin([
        this.loadShiftsForAbsence(),
        this.loadTimeDeviationCausesAbsenceFromEmployeeId(),
        this.loadEmployeesForAbsencePlanning(),
        this.loadHiddenEmployeeId(),
        this.loadCompanySettings(),
        this.loadApprovalTypes(),
        this.loadEmployeeChildSmall(),
        this.loadTerms(),
      ]).pipe(tap(() => this.setInitialFormValues()))
    );

    this.form.timeDeviationCauseId.valueChanges.subscribe(causeId => {
      // Set employeeChild required only when cause requires it.
      const cause = this.timeDeviationCauses.find(
        c => c.timeDeviationCauseId === causeId
      );
      if (cause?.onlyWholeDay) {
        // Load shifts for whole day to compare
        if (this.shiftsRestOfDay.length > 0) {
          // If already loaded
          this.applyFullDayValidator(cause, this.shiftsRestOfDay);
        } else {
          this.loadAbsenceAffectedShiftsRestOfDay()
            .pipe(
              tap(shiftsRestOfDay => {
                this.applyFullDayValidator(cause, shiftsRestOfDay);
                this.form.timeDeviationCauseId.updateValueAndValidity({
                  emitEvent: false,
                });
              })
            )
            .subscribe();
        }
      } else {
        this.form.timeDeviationCauseId.setValidators([Validators.required]);
      }
      if (cause?.specifyChild) {
        this.form.employeeChildId.setValidators([Validators.required]);
      } else {
        this.form.employeeChildId.clearValidators();
        this.form.employeeChildId.setValue(null);
      }

      this.form.timeDeviationCauseId.updateValueAndValidity({
        emitEvent: false,
      });
      this.form.employeeChildId.updateValueAndValidity({ emitEvent: false });
    });
  }

  //#region Load Data
  private loadEmployeeChildSmall(): Observable<SmallGenericType[]> {
    return this.service
      .GetEmployeeChildsSmall(this.employeeId, false)
      .pipe(tap(childs => (this.employeeChilds = childs)));
  }

  private loadApprovalTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.YesNo, true, false)
      .pipe(tap(x => (this.approvalTypes = x)));
  }

  private loadShiftsForAbsence(): Observable<IShiftDTO[]> {
    return this.service
      .getShiftsForQuickAbsence({
        employeeId: this.employeeId,
        shiftIds: this.shiftIds,
        includeLinkedShifts: true,
        timeScheduleScenarioHeadId: 0,
      })
      .pipe(
        tap(shifts => {
          this.shifts = shifts;
          this.form?.populateShiftsForm(shifts);
        })
      );
  }

  // Used when selecting a timedeviationcause with only full day
  private loadAbsenceAffectedShiftsRestOfDay(): Observable<IShiftDTO[]> {
    return this.service
      .getAbsenceAffectedShifts(
        this.employeeId,
        this.dateFrom,
        this.dateTo,
        this.form.timeDeviationCauseId.value,
        null as unknown as IExtendedAbsenceSettingDTO, //TODO: ???
        false,
        this.timeScheduleScenarioHeadId
      )
      .pipe(
        tap(shifts => {
          this.shiftsRestOfDay = shifts;
        })
      );
  }

  private loadTimeDeviationCausesAbsenceFromEmployeeId(): Observable<
    ITimeDeviationCauseDTO[]
  > {
    const onlyUseInTerminal = false;
    return this.service
      .getTimeDeviationCausesAbsenceFromEmployeeId(
        this.employeeId,
        this.dateFrom ?? DateUtil.getToday(),
        onlyUseInTerminal
      )
      .pipe(
        tap(causes => {
          this.timeDeviationCauses = causes;
        })
      );
  }

  private loadEmployeesForAbsencePlanning(): Observable<PlanningEmployeeDTO[]> {
    return this.service
      .getEmployeesForAbsencePlanning(
        DateUtil.toDateString(this.dateFrom),
        DateUtil.toDateString(this.dateTo),
        this.employeeId,
        true
      )
      .pipe(
        tap(x => {
          // this.employeeList = this.service.getFilteredReplacementEmployees(
          //   x,
          //   this.onlyNoReplacementIsSelectable(),
          //   this.employeeId,
          //   this.hiddenEmployeeId(),
          //   this.dateFrom,
          //   this.dateTo
          // );

          this.replaceWithAllEmployees =
            this.service.formatEmployeeListForDisplay(
              x,
              this.hiddenEmployeeId()
            );
        })
      );
  }

  private loadHiddenEmployeeId(): Observable<number> {
    return this.placementsService.getHiddenEmployeeId().pipe(
      tap(x => {
        this.hiddenEmployeeId.set(x);
      })
    );
  }

  private loadCompanySettings(): Observable<number[]> {
    const settingTypes: number[] = [
      // CompanySettingType.TimeSetApprovedYesAsDefault,
      CompanySettingType.TimeOnlyNoReplacementIsSelectable,
      CompanySettingType.TimeSchedulePlanningSendXEMailOnChange,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        // this.setApprovedYesAsDefault.set(
        //   SettingsUtil.getBoolCompanySetting(
        //     setting,
        //     CompanySettingType.TimeSetApprovedYesAsDefault
        //   )
        // );
        this.onlyNoReplacementIsSelectable.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.TimeOnlyNoReplacementIsSelectable
          )
        );
        this.sendXEMailOnChangeDefault.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.TimeSchedulePlanningSendXEMailOnChange
          )
        );
      })
    );
  }

  private loadTerms(): Observable<TermCollection> {
    const translationKeys = ['time.schedule.absencerequests.onlyfullday'];
    return this.translateService
      .get(translationKeys)
      .pipe(tap(terms => [(this.terms = terms)]));
  }

  //#region Events
  save() {
    const employeeRequest: EmployeeRequestsDTO = this.form?.toDTO();
    const shifts: IShiftDTO[] = this.form?.getShiftDTOs();
    this.performAction.load(
      this.validateAndSaveShifts(employeeRequest, shifts, this.employeeId)
    );
  }

  cancel() {
    this.dialogRef.close({} as IAbsenceQuickDialogResult);
  }

  //#region Helper Methods
  private validateAndSaveShifts(
    employeeRequest: EmployeeRequestsDTO,
    shifts: IShiftDTO[],
    employeeId: number
  ): Observable<IEvaluateWorkRulesActionResult> {
    return this.validateWorkRules(shifts).pipe(
      tap(result => {
        // this.toasterService.info('Arbetstidsregler validerade'); //TODO: Fix term

        this.workRuleService
          .showValidateWorkRulesResult(
            TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift,
            result,
            employeeId
          )
          .subscribe(passed => {
            if (passed) {
              setTimeout(() => {
                this.performSave(employeeRequest, shifts);
              }, 100);
            }
          });
      })
    );
  }
  private validateWorkRules(
    shifts: any
  ): Observable<IEvaluateWorkRulesActionResult> {
    let rules: SoeScheduleWorkRules[] | null = null;
    return this.service.evaluateAbsenceRequestPlannedShiftsAgainstWorkRules({
      employeeId: this.employeeId,
      shifts: shifts,
      rules: rules,
      timeScheduleScenarioHeadId: this.timeScheduleScenarioHeadId,
    });
  }

  performSave(employeeRequest: IEmployeeRequestDTO, shifts: IShiftDTO[]) {
    return this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .performAbsencePlanningAction({
          employeeRequest: employeeRequest as unknown as IEmployeeRequestDTO,
          shifts: shifts,
          isScheduledAbsence: true,
          skipGOMailOnShiftChanges: this.form?.skipSendXEMail.value,
        })
        .pipe(
          tap(res => {
            if (res.success) {
              const affectedEmployeeIds = this.form?.getAffectedEmployeeIds();
              this.dialogRef.close({
                affectedEmployeeIds,
              } as IAbsenceQuickDialogResult);
            }
          })
        )
    );
  }

  //#region Form
  private setInitialFormValues() {
    // if (this.setApprovedYesAsDefault()) {
    //   this.form.patchValue(
    //     {
    //       approveAllTypeId: TermGroup_YesNo.Yes,
    //     },
    //     { emitEvent: false }
    //   );
    //   this.form.approveAll(TermGroup_YesNo.Yes);
    // }
    if (this.onlyNoReplacementIsSelectable()) {
      // this.form.patchValue(
      //   {
      //     replaceWithEmployeeId: this.service.NO_REPLACEMENT_EMPLOYEEID,
      //   },
      //   { emitEvent: false }
      // );
      this.form.replaceAll(this.service.NO_REPLACEMENT_EMPLOYEEID);
    }
    this.form.markAsPristine();
  }

  public onReplaceAll(employeeId: number) {
    this.form.replaceAll(employeeId);
  }

  // public onApproveAll(approvalTypeId: TermGroup_YesNo) {
  //   this.form.approveAll(approvalTypeId);
  // }

  private createForm() {
    return new AbsenceQuickDialogForm({
      validationHandler: this.validationHandler,
      element: {
        employeeId: this.data.employeeId,
        employeeName: this.data.employeeName,
        dateFrom: this.data.dateFrom,
        dateTo: this.data.dateTo,
        timeScheduleScenarioHeadId: this.data.timeScheduleScenarioHeadId,
        timeDeviationCauseId: 0,
        shifts: [],
      },
    });
  }

  public get saveIsDisabled() {
    return !this.form?.dirty || this.form?.invalid;
  }

  private applyFullDayValidator(
    cause: ITimeDeviationCauseDTO,
    shiftsRestOfDay: IShiftDTO[]
  ) {
    this.form.timeDeviationCauseId.setValidators([
      Validators.required,
      createFullDayValidator(
        this.terms['time.schedule.absencerequests.onlyfullday'],
        cause,
        this.shifts,
        shiftsRestOfDay
      ),
    ]);
  }
}
