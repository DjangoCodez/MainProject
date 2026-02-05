import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TimeWorkAccountYearDTO } from '../../models/timeworkaccount.model';
import { FormArray } from '@angular/forms';
import { TimeWorkAccountYearEmployeeForm } from './time-work-account-year-employee-form.model';
import { TimeWorkAccountWorkTimeWeekForm } from './time-work-account-year-worktimeweek-form.model';
import { ITimeWorkAccountWorkTimeWeekDTO, ITimeWorkAccountYearEmployeeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeWorkAccountYearForm {
  validationHandler: ValidationHandler;
  element: TimeWorkAccountYearDTO | undefined;
}
export class TimeWorkAccountYearForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ITimeWorkAccountYearForm) {
    super(validationHandler, {
      timeWorkAccountYearId: new SoeTextFormControl(
        element?.timeWorkAccountYearId || 0,
        { isIdField: true }
      ),
      timeWorkAccountId: new SoeTextFormControl(
        element?.timeWorkAccountId || 0,
        { isIdField: true }
      ),
      earningStart: new SoeDateFormControl(
        element?.earningStart || '',
        { required: true },
        'time.payroll.worktimeaccount.earningstart'
      ),
      withdrawalStart: new SoeDateFormControl(
        element?.withdrawalStart || '',
        { required: true },
        'time.payroll.worktimeaccount.withdrawalstart'
      ),
      employeeLastDecidedDate: new SoeDateFormControl(
        element?.employeeLastDecidedDate || '',
        { required: true },
        'time.payroll.worktimeaccount.employeelastdecideddate'
      ),
      earningStop: new SoeDateFormControl(
        element?.earningStop || '',
        { required: true },
        'time.payroll.worktimeaccount.earningstop'
      ),
      withdrawalStop: new SoeDateFormControl(
        element?.withdrawalStop || '',
        { required: true },
        'time.payroll.worktimeaccount.withdrawalstop'
      ),
      paidAbsenceStopDate: new SoeDateFormControl(
        element?.paidAbsenceStopDate || '',
        { required: true },
        'time.payroll.worktimeaccount.paidabsencestopdate'
      ),
      directPaymentLastDate: new SoeDateFormControl(
        element?.directPaymentLastDate || '',
        { required: true },
        'time.payroll.worktimeaccount.directpaymentlastdate'
      ),
      pensionDepositPercent: new SoeNumberFormControl(
        element?.pensionDepositPercent || 0.0,
        { maxValue: 100, minDecimals: 0, maxDecimals: 2, required: true },
        'time.payroll.worktimeaccount.pensiondepositpercent'
      ),
      paidLeavePercent: new SoeNumberFormControl(
        element?.paidLeavePercent || 0.0,
        { maxValue: 100, minDecimals: 0, maxDecimals: 2, required: true },
        'time.payroll.worktimeaccount.paidleavepercent'
      ),
      directPaymentPercent: new SoeNumberFormControl(
        element?.directPaymentPercent || 0.0,
        { maxValue: 100, minDecimals: 0, maxDecimals: 2, required: true },
        'time.payroll.worktimeaccount.directpaymentpercent'
      ),
      pensionDepositPayrollProductId: new SoeSelectFormControl(
        element?.pensionDepositPayrollProductId || 0,
        { required: false },
        'time.payroll.worktimeaccount.pensionproduct'
      ),
      directPaymentPayrollProductId: new SoeSelectFormControl(
        element?.directPaymentPayrollProductId || 0,
        { required: false },
        'time.payroll.worktimeaccount.directpaymentproduct'
      ),
      timeAccumulatorId: new SoeSelectFormControl(
        element?.timeAccumulatorId || 0,
        { required: false },
        'time.time.timeaccumulators.timeaccumulator'
      ),

      timeWorkAccountYearEmployees: new FormArray([]),
      timeWorkAccountWorkTimeWeeks: new FormArray([]),
    });
    this.thisValidationHandler = validationHandler;
  }

  get timeWorkAccountId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeWorkAccountId;
  }
  get earningStart(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.earningStart;
  }

  get withdrawalStart(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.withdrawalStart;
  }

  get employeeLastDecidedDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.employeeLastDecidedDate;
  }

  get earningStop(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.earningStop;
  }

  get withdrawalStop(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.withdrawalStop;
  }

  get paidAbsenceStopDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.paidAbsenceStopDate;
  }

  get directPaymentLastDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.directPaymentLastDate;
  }

  set timeWorkAccountYearEmployees(formArray: FormArray) {
    this.controls.timeWorkAccountYearEmployees = formArray;
  }
  get timeWorkAccountYearEmployees(): FormArray {
    return <FormArray>this.controls.timeWorkAccountYearEmployees;
  }

  set timeWorkAccountWorkTimeWeeks(formArray: FormArray) {
    this.controls.timeWorkAccountWorkTimeWeeks = formArray;
  }
  get timeWorkAccountWorkTimeWeeks(): FormArray {
    return <FormArray>this.controls.timeWorkAccountWorkTimeWeeks;
  }

  get pensionDepositPercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.pensionDepositPercent;
  }

  get paidLeavePercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.paidLeavePercent;
  }

  get directPaymentPercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.directPaymentPercent;
  }
  get pensionDepositPayrollProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.pensionDepositPayrollProductId;
  }
  get timeAccumulatorId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeAccumulatorId;
  }
  get directPaymentPayrollProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.directPaymentPayrollProductId;
  }
  customPatchValue(element: TimeWorkAccountYearDTO) {
    
    if(element.timeWorkAccountYearEmployees != null) {
      this.patchTimeWorkAccountYearEmployees(
        element.timeWorkAccountYearEmployees
      );
    }
    if(element.timeWorkAccountWorkTimeWeeks != null){
      this.patchTimeWorkAccountWorkTimeWeeks(
        element.timeWorkAccountWorkTimeWeeks
      );
    }
    this.patchValue(element);
  }

  patchTimeWorkAccountYearEmployees(
    employees: ITimeWorkAccountYearEmployeeDTO[]
  ): void {
    (this.controls.timeWorkAccountYearEmployees as FormArray).clear();
    for (const timeWorkAccountYearEmployee of employees) {
      const employee = new TimeWorkAccountYearEmployeeForm({
        validationHandler: this.thisValidationHandler,
        element: timeWorkAccountYearEmployee,
      });
      this.timeWorkAccountYearEmployees.push(employee);
    }
  }

  patchTimeWorkAccountWorkTimeWeeks(
    workTimeWeeks: ITimeWorkAccountWorkTimeWeekDTO[]
  ): void {
    (this.controls.timeWorkAccountWorkTimeWeeks as FormArray).clear();
    for (const workTimeWeek of workTimeWeeks) {
      const weekForm = new TimeWorkAccountWorkTimeWeekForm({
        validationHandler: this.thisValidationHandler,
        element: workTimeWeek,
      });
      this.timeWorkAccountWorkTimeWeeks.push(weekForm);
    }
  }
}
