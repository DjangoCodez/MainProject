import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TimeWorkAccountYearEmployeeDTO } from '../../models/timeworkaccount.model';

interface ITimeWorkAccountYearEmployeeForm {
  validationHandler: ValidationHandler;
  element: TimeWorkAccountYearEmployeeDTO | undefined;
}
export class TimeWorkAccountYearEmployeeForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: ITimeWorkAccountYearEmployeeForm) {
    super(validationHandler, {
      timeWorkAccountYearEmployeeId: new SoeTextFormControl(
        element?.timeWorkAccountYearEmployeeId || 0,
        { isIdField: true }
      ),
      employeeId: new SoeTextFormControl(element?.employeeId || 0, {}),
      employeeName: new SoeTextFormControl(element?.employeeName || '', {}),
      employeeNumber: new SoeTextFormControl(element?.employeeNumber || '', {}),
      status: new SoeTextFormControl(element?.status || '', {}),
      earningStart: new SoeDateFormControl(
        element?.earningStart || '',
        { required: true },
        'time.payroll.worktimeaccount.earningstart'
      ),
      earningStop: new SoeDateFormControl(
        element?.earningStop || '',
        { required: true },
        'time.payroll.worktimeaccount.earningstop'
      ),
      selectedWithdrawalMethod: new SoeSelectFormControl(
        element?.selectedWithdrawalMethod || 0,
        {}
      ),
      selectedDate: new SoeDateFormControl(element?.selectedDate || undefined, {}),
      calculatedPaidLeaveMinutes: new SoeNumberFormControl(
        element?.calculatedPaidLeaveMinutes || 0,
        {}
      ),
      calculatedPaidLeaveAmount: new SoeNumberFormControl(
        element?.calculatedPaidLeaveAmount || 0,
        {}
      ),
      calculatedPensionDepositAmount: new SoeNumberFormControl(
        element?.calculatedPaidLeaveMinutes || 0,
        {}
      ),
      calculatedDirectPaymentAmount: new SoeNumberFormControl(
        element?.calculatedDirectPaymentAmount || 0,
        {}
      ),
      calculatedWorkingTimePromoted: new SoeNumberFormControl(
        element?.calculatedWorkingTimePromoted || 0,
        {}
      ),
      specifiedWorkingTimePromoted: new SoeNumberFormControl(
        element?.specifiedWorkingTimePromoted || null,
        {}
      ),
    });
  }

  get earningStart(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.earningStart;
  }

  get earningStop(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.earningStop;
  }

  get employeeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeName;
  }

  get employeeNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeNumber;
  }

  get statusValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.status;
  }

  get selectedWithdrawalMethod(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedWithdrawalMethod;
  }

  get selectedDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.selectedDate;
  }

  get calculatedPaidLeaveMinutes(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.calculatedPaidLeaveMinutes;
  }

  get calculatedPaidLeaveAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.calculatedPaidLeaveAmount;
  }

  get calculatedPensionDepositAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.calculatedPensionDepositAmount;
  }

  get calculatedDirectPaymentAmount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.calculatedDirectPaymentAmount;
  }
  get calculatedWorkingTimePromoted(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.calculatedWorkingTimePromoted;
  }
  get specifiedWorkingTimePromoted(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.specifiedWorkingTimePromoted;
  }
}
