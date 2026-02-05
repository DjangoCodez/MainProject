import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import {
  TimeWorkAccountDTO,
  TimeWorkAccountYearDTO,
} from '../../models/timeworkaccount.model';
import { FormArray } from '@angular/forms';
import { TimeWorkAccountYearForm } from './time-work-account-year-form.model';
import { ValidationHandler } from '@shared/handlers';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

interface ITimeWorkAccountForm {
  validationHandler: ValidationHandler;
  element: TimeWorkAccountDTO | undefined;
}
export class TimeWorkAccountForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ITimeWorkAccountForm) {
    super(validationHandler, {
      timeWorkAccountId: new SoeNumberFormControl(
        element?.timeWorkAccountId || 0,
        { isIdField: true }
      ),
      actorCompanyId: new SoeNumberFormControl(
        element?.actorCompanyId || SoeConfigUtil.actorCompanyId,
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 100, minLength: 1 },
        'common.code'
      ),
      defaultPaidLeaveNotUsed: new SoeSelectFormControl(
        element?.defaultPaidLeaveNotUsed || 0,
        { required: true, zeroNotAllowed: true },
        'time.payroll.worktimeaccount.defaultpaidleavenotused'
      ),
      defaultWithdrawalMethod: new SoeSelectFormControl(
        element?.defaultWithdrawalMethod || 0,
        { required: true, zeroNotAllowed: true },
        'time.payroll.worktimeaccount.defaultwithdrawalmethod'
      ),
      usePensionDeposit: new SoeCheckboxFormControl(
        element?.usePensionDeposit || false,
        {},
        'time.payroll.worktimeaccount.usepensiondeposit'
      ),
      usePaidLeave: new SoeCheckboxFormControl(
        element?.usePaidLeave || false,
        {},
        'time.payroll.worktimeaccount.usepaidleave'
      ),
      useDirectPayment: new SoeCheckboxFormControl(
        element?.useDirectPayment || false,
        {},
        'time.payroll.worktimeaccount.usedirectpayment'
      ),
      
      timeWorkAccountYears: new FormArray([]),
      addCommonControls: false,
    });
    this.thisValidationHandler = validationHandler;
  }

    get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get usePensionDeposit(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.usePensionDeposit;
  }

  get usePaidLeave(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.usePaidLeave;
  }

  get useDirectPayment(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.useDirectPayment;
  }

  get defaultPaidLeaveNotUsed(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.defaultPaidLeaveNotUsed;
  }

  get defaultWithdrawalMethod(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.defaultWithdrawalMethod;
  }

  get timeWorkAccountYears(): FormArray {
    return <FormArray>this.controls.timeWorkAccountYears;
  }

  customPatchValue(element: TimeWorkAccountDTO) {
    (this.controls.timeWorkAccountYears as FormArray).clear();
    for (const timeWorkAccountYear of element.timeWorkAccountYears) {
      const year = new TimeWorkAccountYearForm({
        validationHandler: this.thisValidationHandler,
        element: timeWorkAccountYear,
      });
      this.timeWorkAccountYears.push(year);
    }
    this.patchValue(element);
  }
  
  removeYear(yearForm: TimeWorkAccountYearForm | undefined){
    const index = this.timeWorkAccountYears.value.findIndex((f: { timeWorkAccountYearId: number; }) => f.timeWorkAccountYearId === yearForm?.value.timeWorkAccountYearId)
    if(index !== -1) this.timeWorkAccountYears.removeAt(index)    
  }
  
  addTimeWorkAccountYear(yearForm: TimeWorkAccountYearForm | undefined) {
    this.timeWorkAccountYears.push(
      yearForm ??
        new TimeWorkAccountYearForm({
          validationHandler: this.thisValidationHandler,
          element: new TimeWorkAccountYearDTO(),
        })
    );
  }

  updateYear(yearForm: TimeWorkAccountYearForm | undefined){
    const index = this.timeWorkAccountYears.value.findIndex((f: { timeWorkAccountYearId: number; }) => f.timeWorkAccountYearId === yearForm?.value.timeWorkAccountYearId)
    if(index !== -1) {
      this.timeWorkAccountYears.removeAt(index);
      this.timeWorkAccountYears.insert(index, yearForm);
    }
  }
}
