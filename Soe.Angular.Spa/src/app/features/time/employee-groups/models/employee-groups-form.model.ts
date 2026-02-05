import { AbstractControl, FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IEmployeeGroupAttestTransitionDTO,
  IEmployeeGroupDayTypeDTO,
  IEmployeeGroupDTO,
  IEmployeeGroupRuleWorkTimePeriodDTO,
  IEmployeeGroupTimeDeviationCauseDTO,
  IEmployeeGroupTimeDeviationCauseTimeCodeDTO,
  ITimeAccumulatorEmployeeGroupRuleDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';
import { arrayToFormArray } from '@shared/util/form-util';
import { IdForm, IId } from '../../../../shared/models/id.form.model';
import { EgAttestTransitionsForm } from './eg-attest-transitions-form.model';
import { EmployeeGroupDayTypeForm } from './eg-daytypes-weekendpay-form.model';
import { EmployeeGroupsTimeDeviationCauseTimeCodeForm } from './eg-deviation-cause-time-code-form.model';
import { EmployeeGroupRuleWorkTimePeriodsForm } from './eg-rule-work-time-periods-form.model';
import { EgTimeAccumulatorsRulesForm } from './eg-time-accumulators-rules-form.model';
import { EmployeeGroupTimeDeviationCauseForm } from './eg-timedeviationcauses-form.model';
interface IEmployeeGroupsForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupDTO | undefined;
}
export class EmployeeGroupsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IEmployeeGroupsForm) {
    super(validationHandler, {
      employeeGroupId: new SoeTextFormControl(element?.employeeGroupId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      externalCodesString: new SoeTextFormControl(
        element?.externalCodesString || '',
        { maxLength: 255 },
        'common.externalcodes'
      ),
      alsoAttestAdditionsFromTime: new SoeCheckboxFormControl(
        element?.alsoAttestAdditionsFromTime || false,
        {},
        'time.employee.employeegroup.alsoattestadditionsfromtime'
      ),

      // SETTINGS
      //TIME AGREEMENT IS SCHEDULE ON THE FOLLOWING DAYTYPES
      dayTypeIds: arrayToFormArray(element?.dayTypeIds || []),

      //CALCULATE WEEKEND PAY ON THE FOLLOWING DAYTYPES..
      employeeGroupDayType: arrayToFormArray(
        element?.employeeGroupDayType || []
      ),

      // LINKED TO FOLLOWING CAUSES
      timeDeviationCauseId: new SoeSelectFormControl(
        element?.timeDeviationCauseId || null,
        { required: true },
        'time.employee.employeegroup.deviationcauses.standard'
      ),

      timeDeviationCauses: arrayToFormArray(element?.timeDeviationCauses || []),

      // Time agreement must apply for the following reasons for absence
      timeDeviationCauseRequestIds: arrayToFormArray(
        element?.timeDeviationCauseRequestIds || []
      ),

      // Time agreement can call in sick with the following reasons for absence
      timeDeviationCauseAbsenceAnnouncementIds: arrayToFormArray(
        element?.timeDeviationCauseAbsenceAnnouncementIds || []
      ),

      //Time agreement is linked to the following time codes..
      timeCodeIds: arrayToFormArray(element?.timeCodeIds || []),

      // VALID ATTEST TRANSITIONS
      attestTransition: arrayToFormArray(element?.attestTransition || []),

      // SETTINGS FOR FINALIZATION REMINDERS
      reminderAttestStateId: new SoeSelectFormControl(
        element?.reminderAttestStateId || 0,
        {},
        'time.employee.employeegroup.attest.finalizationreminders.attestlevel'
      ),
      reminderNoOfDays: new SoeNumberFormControl(
        element?.reminderNoOfDays || 0,
        {},
        'time.employee.employeegroup.attest.finalizationreminders.amountdays'
      ),
      reminderPeriodType: new SoeSelectFormControl(
        element?.reminderPeriodType || 0,
        {},
        'reminderPeritime.employee.employeegroup.attest.finalizationreminders.after'
      ),

      // SETTINGS / FINANCIAL AFFILIATION SETTINGS
      allowShiftsWithoutAccount: new SoeCheckboxFormControl(
        element?.allowShiftsWithoutAccount || false,
        {},
        'time.employee.employeegroup.allowshiftswithoutaccount'
      ),

      // SETTINGS / PLANNING
      extraShiftAsDefault: new SoeCheckboxFormControl(
        element?.extraShiftAsDefault || false,
        {},
        'time.employee.employeegroup.extrashiftasdefault'
      ),

      // TIME
      // TYPE OF TIME REPORTING
      timeReportType: new SoeSelectFormControl(
        element?.timeReportType || 0,
        {},
        'time.employee.employeegroup.timereporttype'
      ),
      autogenTimeblocks: new SoeCheckboxFormControl(
        element?.autogenTimeblocks || false,
        { disabled: true },
        'autogenTimeblocks'
      ),
      notifyChangeOfDeviations: new SoeCheckboxFormControl(
        element?.notifyChangeOfDeviations || false,
        {},
        'time.employee.employeegroup.notifychangeofdeviations'
      ),
      autoGenTimeAndBreakForProject: new SoeCheckboxFormControl(
        element?.autoGenTimeAndBreakForProject || false,
        {},
        'time.employee.employeegroup.autogentimeandbreakforproject'
      ),

      // PERIOD CALCULATION
      candidateForOvertimeOnZeroDayExcluded: new SoeCheckboxFormControl(
        element?.candidateForOvertimeOnZeroDayExcluded || false,
        {},
        'time.employee.employeegroup.candidateforovertimeonzerodayexcluded'
      ),
      // QUALIFYING DEDUCTION
      qualifyingDayCalculationRule: new SoeSelectFormControl(
        element?.qualifyingDayCalculationRule || 0,
        {},
        'time.employee.employeegroup.qualifyingdeductioncalculationrule'
      ),
      qualifyingDayCalculationRuleLimitFirstDay: new SoeCheckboxFormControl(
        element?.qualifyingDayCalculationRuleLimitFirstDay || false,
        {},
        'time.employee.employeegroup.qualifyingdeductionrulelimitfirstday'
      ),

      // WORKING TIME REGULATIONS

      ruleWorkTimeWeek: new SoeNumberFormControl( // Minutes
        element?.ruleWorkTimeWeek || 0,
        {},
        'time.employee.employeegroup.ruleworktimeweek'
      ),
      ruleWorkTimeWeekFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.ruleWorkTimeWeek || 0),
        {}
      ),

      ruleWorkTimeDayMinimum: new SoeNumberFormControl( // Minutes
        element?.ruleWorkTimeDayMinimum || 0,
        {},
        'time.employee.employeegroup.ruleworktimedayminimum'
      ),
      ruleWorkTimeDayMinimumFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.ruleWorkTimeDayMinimum || 0),
        {},
        'time.employee.employeegroup.ruleworktimedayminimum'
      ),

      ruleWorkTimeDayMaximumWorkDay: new SoeNumberFormControl(
        element?.ruleWorkTimeDayMaximumWorkDay || 0,
        {},
        'time.employee.employeegroup.ruleworktimedaymaximumworkday'
      ),
      ruleWorkTimeDayMaximumWorkDayFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.ruleWorkTimeDayMaximumWorkDay || 0),
        {},
        'time.employee.employeegroup.ruleworktimedaymaximumworkday'
      ),

      ruleWorkTimeDayMaximumWeekend: new SoeNumberFormControl(
        element?.ruleWorkTimeDayMaximumWeekend || 0,
        {},
        'time.employee.employeegroup.ruleworktimedaymaximumweekend'
      ),
      ruleWorkTimeDayMaximumWeekendFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.ruleWorkTimeDayMaximumWeekend || 0),
        {},
        'time.employee.employeegroup.ruleworktimedaymaximumweekend'
      ),

      maxScheduleTimeFullTime: new SoeNumberFormControl(
        element?.maxScheduleTimeFullTime || 0,
        {},
        'time.employee.employeegroup.maxscheduletimefulltime'
      ),
      maxScheduleTimeFullTimeFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.maxScheduleTimeFullTime || 0),
        {},
        'time.employee.employeegroup.maxscheduletimefulltime'
      ),

      minScheduleTimeFullTime: new SoeNumberFormControl(
        element?.minScheduleTimeFullTime || 0,
        {},
        'time.employee.employeegroup.minscheduletimefulltime'
      ),
      minScheduleTimeFullTimeFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.minScheduleTimeFullTime || 0),
        {},
        'time.employee.employeegroup.minscheduletimefulltime'
      ),

      maxScheduleTimePartTime: new SoeNumberFormControl(
        element?.maxScheduleTimePartTime || 0,
        {},
        'time.employee.employeegroup.maxscheduletimeparttime'
      ),
      maxScheduleTimePartTimeFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.maxScheduleTimePartTime || 0),
        {},
        'time.employee.employeegroup.maxscheduletimeparttime'
      ),

      minScheduleTimePartTime: new SoeNumberFormControl(
        element?.minScheduleTimePartTime || 0,
        {},
        'time.employee.employeegroup.minscheduletimeparttime'
      ),
      minScheduleTimePartTimeFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.minScheduleTimePartTime || 0),
        {},
        'time.employee.employeegroup.minscheduletimeparttime'
      ),

      maxScheduleTimeWithoutBreaks: new SoeNumberFormControl(
        element?.maxScheduleTimeWithoutBreaks || 300,
        {},
        'time.employee.employeegroup.maxscheduletimewithoutbreaks'
      ),
      maxScheduleTimeWithoutBreaksFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(
          element?.maxScheduleTimeWithoutBreaks || 300
        ),
        {},
        'time.employee.employeegroup.maxscheduletimewithoutbreaks'
      ),

      ruleRestTimeDay: new SoeNumberFormControl(
        element?.ruleRestTimeDay || 0,
        {},
        'time.employee.employeegroup.ruleresttimeday'
      ),
      ruleRestTimeDayFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.ruleRestTimeDay || 0),
        {},
        'time.employee.employeegroup.ruleresttimeday'
      ),
      ruleRestTimeDayStartTime: new SoeDateFormControl(
        element?.ruleRestTimeDayStartTime ||
          DateUtil.defaultDateTime().addHours(12),
        {},
        'time.employee.employeegroup.ruleresttimedaystarttime'
      ),
      ruleRestTimeWeekStartTime: new SoeDateFormControl(
        element?.ruleRestTimeWeekStartTime || DateUtil.defaultDateTime(),
        {},
        'time.employee.employeegroup.ruleresttimeweekstarttime'
      ),

      ruleRestTimeWeek: new SoeNumberFormControl(
        element?.ruleRestTimeWeek || 0,
        {},
        'time.employee.employeegroup.ruleresttimeweek'
      ),
      ruleRestTimeWeekFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(element?.ruleRestTimeWeek || 0),
        {}
      ),

      ruleRestTimeWeekStartDayNumber: new SoeSelectFormControl(
        element?.ruleRestTimeWeekStartDayNumber || 1,
        {},
        'time.employee.employeegroup.ruleresttimeweekstartday'
      ),

      ruleScheduleFreeWeekendsMinimumYear: new SoeNumberFormControl(
        element?.ruleScheduleFreeWeekendsMinimumYear || 0,
        {},
        'time.employee.employeegroup.ruleschedulefreeweekendsminimumyear'
      ),
      ruleScheduledDaysMaximumWeek: new SoeNumberFormControl(
        element?.ruleScheduledDaysMaximumWeek || 0,
        {},
        'time.employee.employeegroup.rulescheduleddaysmaximumweek'
      ),

      ruleRestDayIncludePresence: new SoeCheckboxFormControl(
        element?.ruleRestDayIncludePresence || false,
        {},
        'common.time.employee.employeegroup.rulerestdayincludepresence'
      ),
      ruleRestWeekIncludePresence: new SoeCheckboxFormControl(
        element?.ruleRestWeekIncludePresence || false,
        {},
        'time.employee.employeegroup.ruleresttimeweekincludepresence'
      ),

      // PLANNING PERIODS
      ruleWorkTimePeriods: arrayToFormArray(element?.ruleWorkTimePeriods || []),

      // SHIFTSWAP
      swapShiftToShorterText: new SoeTextFormControl(
        element?.swapShiftToShorterText || null,
        { maxLength: 512 },
        'time.employee.employeegroup.shiftswap.toshorter'
      ),
      swapShiftToLongerText: new SoeTextFormControl(
        element?.swapShiftToLongerText || null,
        { maxLength: 512 },
        'time.employee.employeegroup.shiftswap.tolonger'
      ),

      // DEVIATIONS
      deviationAxelStartHours: new SoeNumberFormControl(
        element?.deviationAxelStartHours || 0,
        {},
        'time.employee.employeegroup.deviationaxelstarthours'
      ),
      deviationAxelStopHours: new SoeNumberFormControl(
        element?.deviationAxelStopHours || 0,
        {},
        'time.employee.employeegroup.deviationaxelstophours'
      ),

      // PROJECTS
      timeCodeId: new SoeSelectFormControl(
        element?.timeCodeId || 0,
        {},
        'time.employee.employeegroup.timecodes.projecttimecode'
      ),

      // LINK BETWEEN TIMECODE AND CAUSE
      employeeGroupTimeDeviationCauseTimeCode: arrayToFormArray(
        element?.employeeGroupTimeDeviationCauseTimeCode || []
      ),

      //ACCOUNTS
      //Accounts Extensions
      defaultDim1CostAccountId: new SoeSelectFormControl(
        element?.defaultDim1CostAccountId || 0,
        {},
        'defaultDim1CostAccountId'
      ),
      defaultDim2CostAccountId: new SoeSelectFormControl(
        element?.defaultDim2CostAccountId || 0,
        {},
        'defaultDim2CostAccountId'
      ),
      defaultDim3CostAccountId: new SoeSelectFormControl(
        element?.defaultDim3CostAccountId || 0,
        {},
        'defaultDim3CostAccountId'
      ),
      defaultDim4CostAccountId: new SoeSelectFormControl(
        element?.defaultDim4CostAccountId || 0,
        {},
        'defaultDim4CostAccountId'
      ),
      defaultDim5CostAccountId: new SoeSelectFormControl(
        element?.defaultDim5CostAccountId || 0,
        {},
        'defaultDim5CostAccountId'
      ),
      defaultDim6CostAccountId: new SoeSelectFormControl(
        element?.defaultDim6CostAccountId || 0,
        {},
        'defaultDim6CostAccountId'
      ),
      defaultDim1IncomeAccountId: new SoeSelectFormControl(
        element?.defaultDim1IncomeAccountId || 0,
        {},
        'defaultDim1IncomeAccountId'
      ),
      defaultDim2IncomeAccountId: new SoeSelectFormControl(
        element?.defaultDim2IncomeAccountId || 0,
        {},
        'defaultDim2IncomeAccountId'
      ),
      defaultDim3IncomeAccountId: new SoeSelectFormControl(
        element?.defaultDim3IncomeAccountId || 0,
        {},
        'defaultDim3IncomeAccountId'
      ),
      defaultDim4IncomeAccountId: new SoeSelectFormControl(
        element?.defaultDim4IncomeAccountId || 0,
        {},
        'defaultDim4IncomeAccountId'
      ),
      defaultDim5IncomeAccountId: new SoeSelectFormControl(
        element?.defaultDim5IncomeAccountId || 0,
        {},
        'defaultDim5IncomeAccountId'
      ),
      defaultDim6IncomeAccountId: new SoeSelectFormControl(
        element?.defaultDim6IncomeAccountId || 0,
        {},
        'defaultDim6IncomeAccountId'
      ),

      // BALANCE RULES
      timeAccumulatorEmployeeGroupRules: arrayToFormArray(
        element?.timeAccumulatorEmployeeGroupRules || []
      ),

      // PUNCHING SETTINGS
      // BREAK HANDLING
      autogenBreakOnStamping: new SoeCheckboxFormControl(
        element?.autogenBreakOnStamping || false,
        {},
        'time.employee.employeegroup.autogenbreakonstamping'
      ),
      alwaysDiscardBreakEvaluation: new SoeCheckboxFormControl(
        element?.alwaysDiscardBreakEvaluation || false,
        {},
        'time.employee.employeegroup.alwaysdiscardbreakevaluation'
      ),
      mergeScheduleBreaksOnDay: new SoeCheckboxFormControl(
        element?.mergeScheduleBreaksOnDay || false,
        {},
        'time.employee.employeegroup.mergeschedulebreaksonday'
      ),
      breakRoundingUp: new SoeNumberFormControl(
        element?.breakRoundingUp || 0,
        {},
        'time.employee.employeegroup.breakrounding.roundingup'
      ),
      breakRoundingDown: new SoeNumberFormControl(
        element?.breakRoundingDown || 0,
        {},
        'time.employee.employeegroup.breakrounding.roundingdown'
      ),

      // Punching Rounding
      roundInNeg: new SoeNumberFormControl(
        element?.roundInNeg || 0,
        {},
        'time.employee.employeegroup.punchingrounding.beforeschedulein'
      ),
      roundInPos: new SoeNumberFormControl(
        element?.roundInPos || 0,
        {},
        'time.employee.employeegroup.punchingrounding.beforescheduleout'
      ),
      roundOutNeg: new SoeNumberFormControl(
        element?.roundOutNeg || 0,
        {},
        'time.employee.employeegroup.punchingrounding.afterschedulein'
      ),
      roundOutPos: new SoeNumberFormControl(
        element?.roundOutPos || 0,
        {},
        'time.employee.employeegroup.punchingrounding.afterscheduleout'
      ),

      // DAY BREAK
      breakDayMinutesAfterMidnight: new SoeNumberFormControl(
        element?.breakDayMinutesAfterMidnight || 180,
        {},
        'time.employee.employeegroup.breakdayminutesaftermidnight'
      ),
      keepStampsTogetherWithinMinutes: new SoeNumberFormControl(
        element?.keepStampsTogetherWithinMinutes || 0,
        {},
        'time.employee.employeegroup.keepstampstogetherwithinminutes'
      ),

      // CODING SETTINGS
      payrollProductAccountingPrio: new SoeTextFormControl(
        element?.payrollProductAccountingPrio || '0,0,0,0,0',
        { maxLength: 50 },
        'payrollProductAccountingPrio'
      ),
      invoiceProductAccountingPrio: new SoeTextFormControl(
        element?.invoiceProductAccountingPrio || '0,0,0,0,0',
        {
          maxLength: 50,
        },
        'invoiceProductAccountingPrio'
      ),

      //TIME WORK REDUCTION (ATF)
      timeWorkReductionCalculationRule: new SoeSelectFormControl(
        element?.timeWorkReductionCalculationRule || 0,
        {},
        'time.employeegroup.worktimereduction.calculation'
      ),
    });

    this.thisValidationHandler = validationHandler;
  }

  get ruleWorkTimeWeek() {
    return <SoeNumberFormControl>this.controls.ruleWorkTimeWeek;
  }

  get employeeGroupTimeDeviationCauseTimeCode(): FormArray<EmployeeGroupsTimeDeviationCauseTimeCodeForm> {
    return <FormArray>this.controls.employeeGroupTimeDeviationCauseTimeCode;
  }

  get dayTypeIds(): FormArray<IdForm> {
    return <FormArray>this.controls.dayTypeIds;
  }

  get employeeGroupDayType(): FormArray<EmployeeGroupDayTypeForm> {
    return <FormArray>this.controls.employeeGroupDayType;
  }

  // get timeAccumulatorIds(): FormArray<IdForm> {
  //   return <FormArray>this.controls.timeAccumulatorIds;
  // }

  get timeAccumulatorEmployeeGroupRules(): FormArray<EgTimeAccumulatorsRulesForm> {
    return <FormArray>this.controls.timeAccumulatorEmployeeGroupRules;
  }

  get timeDeviationCauses(): FormArray<EmployeeGroupTimeDeviationCauseForm> {
    return <FormArray>this.controls.timeDeviationCauses;
  }

  get timeDeviationCauseRequestIds(): FormArray<IdForm> {
    return <FormArray>this.controls.timeDeviationCauseRequestIds;
  }

  get timeDeviationCauseAbsenceAnnouncementIds(): FormArray<IdForm> {
    return <FormArray>this.controls.timeDeviationCauseAbsenceAnnouncementIds;
  }

  get timeCodeIds(): FormArray<IdForm> {
    return <FormArray>this.controls.timeCodeIds;
  }

  get attestTransition(): FormArray<EgAttestTransitionsForm> {
    return <FormArray>this.controls.attestTransition;
  }

  get ruleWorkTimePeriods(): FormArray<EmployeeGroupRuleWorkTimePeriodsForm> {
    return <FormArray>this.controls.ruleWorkTimePeriods;
  }

  get defaultDim1CostAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim1CostAccountId;
  }

  get defaultDim2CostAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim2CostAccountId;
  }

  get defaultDim3CostAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim3CostAccountId;
  }

  get defaultDim4CostAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim4CostAccountId;
  }

  get defaultDim5CostAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim5CostAccountId;
  }

  get defaultDim6CostAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim6CostAccountId;
  }

  get defaultDim1IncomeAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim1IncomeAccountId;
  }

  get defaultDim2IncomeAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim2IncomeAccountId;
  }

  get defaultDim3IncomeAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim3IncomeAccountId;
  }

  get defaultDim4IncomeAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim4IncomeAccountId;
  }

  get defaultDim5IncomeAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim5IncomeAccountId;
  }

  get defaultDim6IncomeAccountId() {
    return <SoeSelectFormControl>this.controls.defaultDim6IncomeAccountId;
  }

  get alwaysDiscardBreakEvaluation() {
    return <SoeCheckboxFormControl>this.controls.alwaysDiscardBreakEvaluation;
  }

  get autogenBreakOnStamping() {
    return <SoeCheckboxFormControl>this.controls.autogenBreakOnStamping;
  }

  get mergeScheduleBreaksOnDay() {
    return <SoeCheckboxFormControl>this.controls.mergeScheduleBreaksOnDay;
  }

  get timeReportType() {
    return <SoeSelectFormControl>this.controls.timeReportType;
  }

  get notifyChangeOfDeviations() {
    return <SoeCheckboxFormControl>this.controls.notifyChangeOfDeviations;
  }

  get autoGenTimeAndBreakForProject() {
    return <SoeCheckboxFormControl>this.controls.autoGenTimeAndBreakForProject;
  }

  // SET FORMATTED VALUES

  setInitialFormattedTimeboxValues() {
    this.controls.ruleWorkTimeWeekFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.ruleWorkTimeWeek.value),
      { emitEvent: true }
    );
    this.controls.ruleWorkTimeDayMinimumFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.ruleWorkTimeDayMinimum.value),
      { emitEvent: true }
    );
    this.controls.ruleWorkTimeDayMaximumWorkDayFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.ruleWorkTimeDayMaximumWorkDay.value
      ),
      { emitEvent: true }
    );
    this.controls.ruleWorkTimeDayMaximumWeekendFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.ruleWorkTimeDayMaximumWeekend.value
      ),
      { emitEvent: true }
    );
    this.controls.maxScheduleTimeFullTimeFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.maxScheduleTimeFullTime.value),
      { emitEvent: true }
    );
    this.controls.minScheduleTimeFullTimeFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.minScheduleTimeFullTime.value),
      { emitEvent: true }
    );
    this.controls.maxScheduleTimePartTimeFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.maxScheduleTimePartTime.value),
      { emitEvent: true }
    );
    this.controls.minScheduleTimePartTimeFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.minScheduleTimePartTime.value),
      { emitEvent: true }
    );
    this.controls.maxScheduleTimeWithoutBreaksFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.maxScheduleTimeWithoutBreaks.value
      ),
      { emitEvent: true }
    );
    this.controls.ruleRestTimeDayFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.ruleRestTimeDay.value),
      { emitEvent: true }
    );
    this.controls.ruleRestTimeWeekFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.ruleRestTimeWeek.value),
      { emitEvent: true }
    );
  }

  timeboxFieldChanged(fieldName: string) {
    const raw = this.controls[fieldName];
    const formatted = this.controls[`${fieldName}Formatted`];
    this.formatTimeboxValue(raw, formatted);
  }

  formatTimeboxValue(field: AbstractControl, formattedField: AbstractControl) {
    const newValue = DateUtil.timeSpanToMinutes(formattedField.value);
    field.setValue(newValue, { emitEvent: false });

    const newFormattedValue = DateUtil.minutesToTimeSpan(field.value);

    if (newFormattedValue !== formattedField.value) {
      formattedField.setValue(newFormattedValue, { emitEvent: false });
    }
  }

  // GRID PATCH VALUES

  customPatchValue(element: IEmployeeGroupDTO) {
    this.reset();
    this.patchValue(element);
    this.customTimeDeviationCauseTimeCodePatchValue(
      element?.employeeGroupTimeDeviationCauseTimeCode ?? []
    );
    this.customAttestTransitionsPatchValue(element?.attestTransition ?? []);
    this.customDayTypesPatchValue(
      (element?.dayTypeIds ?? []).map(id => ({ id }))
    );
    this.customTimeAccumulatorsPatchValue(
      element?.timeAccumulatorEmployeeGroupRules ?? []
    );
    this.customTimeDeviationCauseRequestIdsPatchValue(
      (element?.timeDeviationCauseRequestIds ?? []).map(id => ({ id }))
    );
    this.customTimeDeviationCauseAbsenceAnnouncementIdsPatchValue(
      (element?.timeDeviationCauseAbsenceAnnouncementIds ?? []).map(id => ({
        id,
      }))
    );
    this.customTimeCodeIdsPatchValue(
      (element?.timeCodeIds ?? []).map(id => ({ id }))
    );
    this.customDayTypesWeekendSalaryPatchValue(
      element?.employeeGroupDayType ?? []
    );
    this.customTimeDeviationCausesPatchValue(
      element?.timeDeviationCauses ?? []
    );
    this.customRuleWorkTimePeriodsPatchValue(
      element?.ruleWorkTimePeriods ?? []
    );

    this.markAsUntouched({ onlySelf: true });
    this.markAsPristine({ onlySelf: true });
  }

  customTimeDeviationCauseTimeCodePatchValue(
    employeeGroupTimeDeviationCauseTimeCode: IEmployeeGroupTimeDeviationCauseTimeCodeDTO[]
  ) {
    this.employeeGroupTimeDeviationCauseTimeCode.clear({ emitEvent: false });
    employeeGroupTimeDeviationCauseTimeCode.forEach(tdc => {
      const timeDeviationCauseTimeCodeForm =
        new EmployeeGroupsTimeDeviationCauseTimeCodeForm({
          validationHandler: this.thisValidationHandler,
          element: tdc,
        });
      timeDeviationCauseTimeCodeForm.customPatchValue(tdc);
      this.employeeGroupTimeDeviationCauseTimeCode.push(
        timeDeviationCauseTimeCodeForm
      );
    });
    this.employeeGroupTimeDeviationCauseTimeCode.markAsUntouched({
      onlySelf: true,
    });
    this.employeeGroupTimeDeviationCauseTimeCode.markAsPristine({
      onlySelf: true,
    });
    this.employeeGroupTimeDeviationCauseTimeCode.updateValueAndValidity();
  }

  customTimeDeviationCausesPatchValue(
    timeDeviationCauses: IEmployeeGroupTimeDeviationCauseDTO[]
  ) {
    this.timeDeviationCauses.clear({ emitEvent: false });
    timeDeviationCauses.forEach(tdc => {
      const timeDeviationCauseForm = new EmployeeGroupTimeDeviationCauseForm({
        validationHandler: this.thisValidationHandler,
        element: tdc,
      });
      timeDeviationCauseForm.customPatchValue(tdc);
      this.timeDeviationCauses.push(timeDeviationCauseForm);
    });
    this.timeDeviationCauses.markAsUntouched({
      onlySelf: true,
    });
    this.timeDeviationCauses.markAsPristine({
      onlySelf: true,
    });
    this.timeDeviationCauses.updateValueAndValidity();
  }

  customTimeAccumulatorsPatchValue(
    timeAccumulatorEmployeeGroupRules: ITimeAccumulatorEmployeeGroupRuleDTO[]
  ) {
    this.timeAccumulatorEmployeeGroupRules.clear({ emitEvent: false });
    timeAccumulatorEmployeeGroupRules.forEach(x => {
      const form = new EgTimeAccumulatorsRulesForm({
        validationHandler: this.thisValidationHandler,
        element: x,
      });
      form.customPatchValue(x);
      this.timeAccumulatorEmployeeGroupRules.push(form);
    });
    this.timeAccumulatorEmployeeGroupRules.markAsUntouched({
      onlySelf: true,
    });
    this.timeAccumulatorEmployeeGroupRules.markAsPristine({
      onlySelf: true,
    });
    this.timeAccumulatorEmployeeGroupRules.updateValueAndValidity();
  }

  customTimeDeviationCauseRequestIdsPatchValue(
    timeDeviationCauseRequestIds: IId[]
  ) {
    this.timeDeviationCauseRequestIds.clear({ emitEvent: false });
    timeDeviationCauseRequestIds.forEach(dt => {
      const form = new IdForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.timeDeviationCauseRequestIds.push(form);
    });
    this.timeDeviationCauseRequestIds.markAsUntouched({
      onlySelf: true,
    });
    this.timeDeviationCauseRequestIds.markAsPristine({
      onlySelf: true,
    });
    this.timeDeviationCauseRequestIds.updateValueAndValidity();
  }

  customTimeDeviationCauseAbsenceAnnouncementIdsPatchValue(
    timeDeviationCauseAbsenceAnnouncementIds: IId[]
  ) {
    this.timeDeviationCauseAbsenceAnnouncementIds.clear({ emitEvent: false });
    timeDeviationCauseAbsenceAnnouncementIds.forEach(dt => {
      const form = new IdForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.timeDeviationCauseAbsenceAnnouncementIds.push(form);
    });
    this.timeDeviationCauseAbsenceAnnouncementIds.markAsUntouched({
      onlySelf: true,
    });
    this.timeDeviationCauseAbsenceAnnouncementIds.markAsPristine({
      onlySelf: true,
    });
    this.timeDeviationCauseAbsenceAnnouncementIds.updateValueAndValidity();
  }

  customTimeCodeIdsPatchValue(timeCodeIds: IId[]) {
    this.timeCodeIds.clear({ emitEvent: false });
    timeCodeIds.forEach(dt => {
      const form = new IdForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.timeCodeIds.push(form);
    });
    this.timeCodeIds.markAsUntouched({
      onlySelf: true,
    });
    this.timeCodeIds.markAsPristine({
      onlySelf: true,
    });
    this.timeCodeIds.updateValueAndValidity();
  }

  customAttestTransitionsPatchValue(
    attestTransition: IEmployeeGroupAttestTransitionDTO[]
  ) {
    this.attestTransition.clear({ emitEvent: false });
    attestTransition.forEach(x => {
      const attestTransitionForm = new EgAttestTransitionsForm({
        validationHandler: this.thisValidationHandler,
        element: x,
      });
      attestTransitionForm.customPatchValue(x);
      this.attestTransition.push(attestTransitionForm);
    });
    this.attestTransition.markAsUntouched({
      onlySelf: true,
    });
    this.attestTransition.markAsPristine({
      onlySelf: true,
    });
    this.attestTransition.updateValueAndValidity();
  }

  customDayTypesPatchValue(dayTypeIds: IId[]) {
    this.dayTypeIds.clear({ emitEvent: false });
    dayTypeIds.forEach(dt => {
      const form = new IdForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.dayTypeIds.push(form);
    });
    this.dayTypeIds.markAsUntouched({
      onlySelf: true,
    });
    this.dayTypeIds.markAsPristine({
      onlySelf: true,
    });
    this.dayTypeIds.updateValueAndValidity();
  }

  customDayTypesWeekendSalaryPatchValue(dayTypes: IEmployeeGroupDayTypeDTO[]) {
    this.employeeGroupDayType.clear({ emitEvent: false });
    dayTypes.forEach(dt => {
      const form = new EmployeeGroupDayTypeForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.employeeGroupDayType.push(form);
    });
    this.employeeGroupDayType.markAsUntouched({
      onlySelf: true,
    });
    this.employeeGroupDayType.markAsPristine({
      onlySelf: true,
    });
    this.employeeGroupDayType.updateValueAndValidity();
  }

  customRuleWorkTimePeriodsPatchValue(
    ruleWorkTimePeriods: IEmployeeGroupRuleWorkTimePeriodDTO[]
  ) {
    this.ruleWorkTimePeriods.clear({ emitEvent: false });
    ruleWorkTimePeriods.forEach(x => {
      const form = new EmployeeGroupRuleWorkTimePeriodsForm({
        validationHandler: this.thisValidationHandler,
        element: x,
      });
      form.customPatchValue(x);
      this.ruleWorkTimePeriods.push(form);
    });
    this.ruleWorkTimePeriods.markAsUntouched({
      onlySelf: true,
    });
    this.ruleWorkTimePeriods.markAsPristine({
      onlySelf: true,
    });
    this.ruleWorkTimePeriods.updateValueAndValidity();
  }

  //FORM LOGIC
  setInitialFormLogic() {
    this.setBreakSettingsFormLogic();
  }

  setBreakSettingsFormLogic() {
    if (
      this.autogenBreakOnStamping.value === true ||
      this.mergeScheduleBreaksOnDay.value === true
    ) {
      this.alwaysDiscardBreakEvaluation.disable();
    } else {
      this.alwaysDiscardBreakEvaluation.enable();
    }

    if (this.alwaysDiscardBreakEvaluation.value === true) {
      this.autogenBreakOnStamping.disable();
      this.mergeScheduleBreaksOnDay.disable();
    } else {
      this.autogenBreakOnStamping.enable();
      this.mergeScheduleBreaksOnDay.enable();
    }
  }
}
