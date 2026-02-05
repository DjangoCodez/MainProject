import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeWorkReductionReconciliationGenerateOutcomeModel } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface TimeWorkReductionReconcilationGenerateOutcomeForm {
  validationHandler: ValidationHandler;
  element: ITimeWorkReductionReconciliationGenerateOutcomeModel | undefined;
}
export class TimeWorkReducionGenerateOutcomeForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({
    validationHandler,
    element,
  }: TimeWorkReductionReconcilationGenerateOutcomeForm) {
    super(validationHandler, {
      timeWorkReductionReconciliationYearId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationYearId || 0,
        { isIdField: true }
      ),
      timeWorkReductionReconciliationId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationId || 0,
        { isIdField: true }
      ),
      paymentDateId: new SoeSelectFormControl(
        element?.paymentDateId || 0,
        {},
        'time.time.timeperiod.paymentdate'
      ),
      paymentDate: new SoeDateFormControl(
        element?.paymentDate || undefined,
        { required: false },
        'time.time.timeperiod.paymentdate'
      ),
      overrideChoosen: new SoeCheckboxFormControl(
        element?.overrideChoosen || false,
        {},
        'time.payroll.worktimeaccount.overridechoosen'
      ),
      timeWorkReductionReconciliationEmployeeIds: new SoeSelectFormControl(
        element?.timeWorkReductionReconciliationEmployeeIds || null
      ),
    });
    this.thisValidationHandler = validationHandler;
  }

  get TimeWorkReductionReconciliationId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.TimeWorkReductionReconciliationId;
  }
  get PaymentDateId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.paymentDateId;
  }
  get PaymentDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.paymentDate;
  }
  get overrideChoosen(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.overrideChoosen;
  }
}
