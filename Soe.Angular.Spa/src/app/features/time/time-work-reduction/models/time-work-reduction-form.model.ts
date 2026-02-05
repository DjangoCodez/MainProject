import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ITimeWorkReductionReconciliationDTO,
  ITimeWorkReductionReconciliationYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeWorkReductionYearForm } from './time-work-reduction-year-form.model';
import { arrayToFormArray } from '@shared/util/form-util';

interface ITimeWorkReductionForm {
  validationHandler: ValidationHandler;
  element: ITimeWorkReductionReconciliationDTO | undefined;
}

export class TimeWorkReductionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeWorkReductionForm) {
    super(validationHandler, {
      timeWorkReductionReconciliationId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationId || 0,
        { isIdField: true }
      ),
      timeAccumulatorId: new SoeSelectFormControl(
        element?.timeAccumulatorId || undefined,
        { required: true },
        'time.time.timeaccumulators.timeaccumulator'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { required: true, maxLength: 100 },
        'common.description'
      ),
      usePensionDeposit: new SoeCheckboxFormControl(
        element?.usePensionDeposit || false,
        {},
        'time.payroll.worktimeaccount.usepensiondeposit'
      ),
      useDirectPayment: new SoeCheckboxFormControl(
        element?.useDirectPayment || false,
        {},
        'time.payroll.worktimeaccount.usedirectpayment'
      ),
      defaultWithdrawalMethod: new SoeSelectFormControl(
        element?.defaultWithdrawalMethod || undefined,
        { required: true },
        'time.payroll.worktimeaccount.defaultwithdrawalmethod'
      ),

      timeWorkReductionReconciliationYearDTO: arrayToFormArray(
        element?.timeWorkReductionReconciliationYearDTO || []
      ),
    });
  }
  get timeWorkReductionReconciliationYearDTO(): FormArray<TimeWorkReductionYearForm> {
    return <FormArray>this.controls.timeWorkReductionReconciliationYearDTO;
  }

  customPatchValue(element: ITimeWorkReductionReconciliationDTO) {
    this.patchValue(element);
    this.patchYear(element.timeWorkReductionReconciliationYearDTO);
  }

  patchYear(years: ITimeWorkReductionReconciliationYearDTO[]) {
    this.timeWorkReductionReconciliationYearDTO.clear();
    years.forEach(twy => {
      const year = new TimeWorkReductionYearForm({
        validationHandler: this.formValidationHandler,
        element: twy,
      });
      year.customPatchValue(twy);

      this.timeWorkReductionReconciliationYearDTO.push(year, {
        emitEvent: false,
      });
    });
    this.timeWorkReductionReconciliationYearDTO.markAsUntouched({
      onlySelf: true,
    });
    this.timeWorkReductionReconciliationYearDTO.markAsPristine({
      onlySelf: true,
    });
    this.timeWorkReductionReconciliationYearDTO.updateValueAndValidity();
  }
}
