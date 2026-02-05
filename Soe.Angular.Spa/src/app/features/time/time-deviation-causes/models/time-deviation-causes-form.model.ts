import { AbstractControl } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeDeviationCauseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';

interface ITimeDeviationCausesForm {
  validationHandler: ValidationHandler;
  element: ITimeDeviationCauseDTO | undefined;
}
export class TimeDeviationCausesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeDeviationCausesForm) {
    super(validationHandler, {
      timeDeviationCauseId: new SoeTextFormControl(
        element?.timeDeviationCauseId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      extCode: new SoeTextFormControl(
        element?.extCode || '',
        { maxLength: 50 },
        'time.time.timedeviationcause.extcode'
      ),
      type: new SoeNumberFormControl(
        element?.type || 3,
        { required: true },
        'common.type'
      ),
      typeName: new SoeTextFormControl(
        element?.typeName || '',
        { maxLength: 100 }, // Arbitrary value, since it doesn't exist in DB
        'common.type'
      ),
      timeCodeId: new SoeSelectFormControl(
        element?.timeCodeId || 0,
        {},
        'time.time.timedeviationcause.timecodeid'
      ),
      timeCodeName: new SoeTextFormControl(
        element?.timeCodeName || '',
        { maxLength: 100 },
        'time.time.timedeviationcause.timecode'
      ),
      validForStandby: new SoeCheckboxFormControl(
        element?.validForStandby || false,
        {},
        'time.time.timedeviationcause.validforstandby'
      ),
      validForHibernating: new SoeCheckboxFormControl(
        element?.validForHibernating || false,
        {},
        'time.time.timedeviationcause.validforhibernating'
      ),
      candidateForOvertime: new SoeCheckboxFormControl(
        element?.candidateForOvertime || false,
        {},
        'time.time.timedeviationcause.candidateforovertime'
      ),
      employeeRequestPolicyNbrOfDaysBefore: new SoeNumberFormControl(
        element?.employeeRequestPolicyNbrOfDaysBefore || 0,
        {},
        'time.time.timedeviationcause.employeerequestpolicynbrofdaysbefore'
      ),
      employeeRequestPolicyNbrOfDaysBeforeCanOverride:
        new SoeCheckboxFormControl(
          element?.employeeRequestPolicyNbrOfDaysBeforeCanOverride || false,
          {},
          'time.time.timedeviationcause.employeeRequestPolicyNbrOfDaysBeforeCanOverride'
        ),
      onlyWholeDay: new SoeCheckboxFormControl(
        element?.onlyWholeDay || false,
        {},
        'time.time.timedeviationcause.onlyWholeDay'
      ),
      showZeroDaysInAbsencePlanning: new SoeCheckboxFormControl(
        element?.showZeroDaysInAbsencePlanning || false,
        {},
        'time.time.timedeviationcause.showZeroDaysInAbsencePlanning'
      ),
      attachZeroDaysNbrOfDaysBefore: new SoeNumberFormControl(
        element?.attachZeroDaysNbrOfDaysBefore || 0,
        {},
        'time.time.timedeviationcause.attachZeroDaysNbrOfDaysBefore'
      ),
      attachZeroDaysNbrOfDaysAfter: new SoeNumberFormControl(
        element?.attachZeroDaysNbrOfDaysAfter || 0,
        {},
        'time.time.timedeviationcause.attachZeroDaysNbrOfDaysAfter'
      ),
      changeDeviationCauseAccordingToPlannedAbsence: new SoeCheckboxFormControl(
        element?.changeDeviationCauseAccordingToPlannedAbsence || false,
        {},
        'time.time.timedeviationcause.changeDeviationCauseAccordingToPlannedAbsence'
      ),
      changeCauseOutsideOfPlannedAbsence: new SoeNumberFormControl( // Minutes, DB field is int
        element?.changeCauseOutsideOfPlannedAbsence || 0,
        {},
        'time.time.timedeviationcause.changeCauseOutsideOfPlannedAbsence'
      ),
      changeCauseOutsideOfPlannedAbsenceFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(
          element?.changeCauseOutsideOfPlannedAbsence || 0
        ),
        {}
      ),
      changeCauseInsideOfPlannedAbsence: new SoeNumberFormControl( // Minutes, DB field is int
        element?.changeCauseInsideOfPlannedAbsence || 0,
        {},
        'time.time.timedeviationcause.changeCauseInsideOfPlannedAbsence'
      ),
      changeCauseInsideOfPlannedAbsenceFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(
          element?.changeCauseInsideOfPlannedAbsence || 0
        ),
        {}
      ),
      adjustTimeOutsideOfPlannedAbsence: new SoeNumberFormControl( // Minutes, DB field is int
        element?.adjustTimeOutsideOfPlannedAbsence || 0,
        {},
        'time.time.timedeviationcause.adjustTimeOutsideOfPlannedAbsence'
      ),
      adjustTimeOutsideOfPlannedAbsenceFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(
          element?.adjustTimeOutsideOfPlannedAbsence || 0
        ),
        {}
      ),
      adjustTimeInsideOfPlannedAbsence: new SoeNumberFormControl( // Minutes, DB field is int
        element?.adjustTimeInsideOfPlannedAbsence || 0,
        {},
        'time.time.timedeviationcause.adjustTimeInsideOfPlannedAbsence'
      ),
      adjustTimeInsideOfPlannedAbsenceFormatted: new SoeTextFormControl( // HH:MM
        DateUtil.minutesToTimeSpan(
          element?.adjustTimeInsideOfPlannedAbsence || 0
        ),
        {}
      ),
      allowGapToPlannedAbsence: new SoeCheckboxFormControl(
        element?.allowGapToPlannedAbsence || false,
        {},
        'time.time.timedeviationcause.allowGapToPlannedAbsence'
      ),
      isVacation: new SoeCheckboxFormControl(
        element?.isVacation || false,
        {},
        'time.time.timedeviationcause.isVacation'
      ),
      specifyChild: new SoeCheckboxFormControl(
        element?.specifyChild || false,
        {},
        'time.time.timedeviationcause.specifyChild'
      ),
      payed: new SoeCheckboxFormControl(
        element?.payed || false,
        {},
        'time.time.timedeviationcause.payed'
      ),
      notChargeable: new SoeCheckboxFormControl(
        element?.notChargeable || false,
        {},
        'time.time.timedeviationcause.notChargeable'
      ),
      mandatoryNote: new SoeCheckboxFormControl(
        element?.mandatoryNote || false,
        {},
        'time.time.timedeviationcause.mandatoryNote'
      ),
      mandatoryTime: new SoeCheckboxFormControl(
        element?.mandatoryTime || false,
        {},
        'time.time.timedeviationcause.mandatoryTime'
      ),
      excludeFromPresenceWorkRules: new SoeCheckboxFormControl(
        element?.excludeFromPresenceWorkRules || false,
        {},
        'time.time.timedeviationcause.excludeFromPresenceWorkRules'
      ),
      excludeFromScheduleWorkRules: new SoeCheckboxFormControl(
        element?.excludeFromScheduleWorkRules || false,
        {},
        'time.time.timedeviationcause.excludeFromScheduleWorkRules'
      ),
      calculateAsOtherTimeInSales: new SoeCheckboxFormControl(
        element?.calculateAsOtherTimeInSales || false,
        {},
        'time.time.timedeviationcause.calculateasothertimeinsales'
      ),
    });
  }
  get name() {
    return <SoeTextFormControl>this.controls.name;
  }
  get type() {
    return <SoeNumberFormControl>this.controls.type;
  }
  get changeDeviationCauseAccordingToPlannedAbsence() {
    return <SoeCheckboxFormControl>(
      this.controls.changeDeviationCauseAccordingToPlannedAbsence
    );
  }
  get changeCauseOutsideOfPlannedAbsence() {
    return <SoeNumberFormControl>(
      this.controls.changeCauseOutsideOfPlannedAbsence
    );
  }
  // get changeCauseOutsideOfPlannedAbsenceFormatted() {
  //   return <SoeTextFormControl>this.controls.changeCauseOutsideOfPlannedAbsenceFormatted;
  // }
  get changeCauseInsideOfPlannedAbsence() {
    return <SoeNumberFormControl>(
      this.controls.changeCauseInsideOfPlannedAbsence
    );
  }
  get adjustTimeOutsideOfPlannedAbsence() {
    return <SoeNumberFormControl>(
      this.controls.adjustTimeOutsideOfPlannedAbsence
    );
  }
  get adjustTimeInsideOfPlannedAbsence() {
    return <SoeNumberFormControl>this.controls.adjustTimeInsideOfPlannedAbsence;
  }

  setInitialFormattedValues() {
    this.controls.changeCauseOutsideOfPlannedAbsenceFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.changeCauseOutsideOfPlannedAbsence.value
      )
    );

    this.controls.changeCauseInsideOfPlannedAbsenceFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.changeCauseInsideOfPlannedAbsence.value
      )
    );

    this.controls.adjustTimeOutsideOfPlannedAbsenceFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.adjustTimeOutsideOfPlannedAbsence.value
      )
    );

    this.controls.adjustTimeInsideOfPlannedAbsenceFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.adjustTimeInsideOfPlannedAbsence.value
      )
    );

    // this.controls.minSplitLengthFormatted.setValue(
    //   DateUtil.minutesToTimeSpan(this.controls.minSplitLength.value)
    // );
  }

  disableFieldsAccordingToPlannedAbsenceCheckbox(
    event: boolean,
    modifyPermission: boolean
  ) {
    if (event) {
      if (modifyPermission) {
        this.controls.changeCauseOutsideOfPlannedAbsenceFormatted.enable();
        this.controls.changeCauseInsideOfPlannedAbsenceFormatted.enable();
        this.controls.adjustTimeOutsideOfPlannedAbsenceFormatted.enable();
        this.controls.adjustTimeInsideOfPlannedAbsenceFormatted.enable();
        this.controls.allowGapToPlannedAbsence.enable();
      }
    } else {
      this.controls.changeCauseOutsideOfPlannedAbsenceFormatted.disable();
      this.controls.changeCauseInsideOfPlannedAbsenceFormatted.disable();
      this.controls.adjustTimeOutsideOfPlannedAbsenceFormatted.disable();
      this.controls.adjustTimeInsideOfPlannedAbsenceFormatted.disable();
      this.controls.allowGapToPlannedAbsence.disable();
    }
  }

  setupPlannedAbsenceCheckboxesLogic() {
    // Sets state once.
    if (!this.controls.changeDeviationCauseAccordingToPlannedAbsence.value) {
      this.controls.changeCauseOutsideOfPlannedAbsenceFormatted.disable({
        emitEvent: false,
      });
      this.controls.changeCauseInsideOfPlannedAbsenceFormatted.disable({
        emitEvent: false,
      });
      this.controls.adjustTimeOutsideOfPlannedAbsenceFormatted.disable({
        emitEvent: false,
      });
      this.controls.adjustTimeInsideOfPlannedAbsenceFormatted.disable({
        emitEvent: false,
      });
      this.controls.allowGapToPlannedAbsence.disable({ emitEvent: false });
    }
  }

  changeCauseOutsideOfPlannedAbsenceChanged() {
    this.formatTimeboxValue(
      this.controls.changeCauseOutsideOfPlannedAbsence,
      this.controls.changeCauseOutsideOfPlannedAbsenceFormatted
    );
  }

  changeCauseInsideOfPlannedAbsenceChanged() {
    this.formatTimeboxValue(
      this.controls.changeCauseInsideOfPlannedAbsence,
      this.controls.changeCauseInsideOfPlannedAbsenceFormatted
    );
  }

  adjustTimeOutsideOfPlannedAbsenceChanged() {
    this.formatTimeboxValue(
      this.controls.adjustTimeOutsideOfPlannedAbsence,
      this.controls.adjustTimeOutsideOfPlannedAbsenceFormatted
    );
  }

  adjustTimeInsideOfPlannedAbsenceChanged() {
    this.formatTimeboxValue(
      this.controls.adjustTimeInsideOfPlannedAbsence,
      this.controls.adjustTimeInsideOfPlannedAbsenceFormatted
    );
  }

  formatTimeboxValue(field: AbstractControl, formattedField: AbstractControl) {
    const newValue = DateUtil.timeSpanToMinutes(formattedField.value);
    field.setValue(newValue);

    if (newValue <= 0) {
      field.setValue(0);
    }

    const newFormattedValue = DateUtil.minutesToTimeSpan(field.value);

    if (newFormattedValue !== formattedField.value) {
      formattedField.setValue(newFormattedValue);
    }
  }
}
