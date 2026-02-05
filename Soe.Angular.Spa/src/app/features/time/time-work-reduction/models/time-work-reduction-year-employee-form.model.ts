import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

import { ITimeWorkReductionReconciliationEmployeeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeWorkReductionYearEmployeeForm {
  validationHandler: ValidationHandler;
  element: ITimeWorkReductionReconciliationEmployeeDTO | undefined;
}

export class TimeWorkReductionYearEmployeeForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: ITimeWorkReductionYearEmployeeForm) {
    super(validationHandler, {
      timeWorkReductionReconciliationEmployeeId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationEmployeeId || 0,
        { isIdField: true }
      ),
      timeWorkReductionReconciliationYearId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationYearId || 0,
        {}
      ),
      timeWorkReductionReconciliationId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationId || 0,
        {}
      ),
      employeeNrAndName: new SoeTextFormControl(
        element?.employeeNrAndName || '',
        {},
        'time.workreduction.employee'
      ),
      employeeNr: new SoeNumberFormControl(
        element?.employeeNr || 0,
        {},
        'time.workreduction.employeenr'
      ),
      employeeName: new SoeTextFormControl(
        element?.employeeName || '',
        {},
        'time.workreduction.employeename'
      ),
      employeeId: new SoeNumberFormControl(
        element?.employeeId || 0,
        {},
        'time.workreduction.employee'
      ),
      selectedWithdrawalMethod: new SoeSelectFormControl(
        element?.selectedWithdrawalMethod || 0,
        {},
        'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod'
      ),
      selectedDate: new SoeTextFormControl(
        element?.selectedDate || '',
        {
          required: true,
        },
        'time.payroll.worktimeaccount.employee.selecteddate'
      ),
      minutesOverThreshold: new SoeNumberFormControl(
        element?.minutesOverThreshold || 0,
        {},

        'time.time.timeworkreduction.minutesoverthreshold'
      ),
      accEarningMinutes: new SoeNumberFormControl(
        element?.accEarningMinutes || 0,
        {},
        'time.time.timeworkreduction.accearningminutes'
      ),
      threshold: new SoeNumberFormControl(
        element?.threshold || 0,
        {},
        'time.time.timeaccumulator.employeegrouprule.thresholdMinutes'
      ),
      status: new SoeSelectFormControl(
        element?.status || 0,
        {},
        'core.fileupload.status'
      ),
    });
  }

  customPatchValue(element: ITimeWorkReductionReconciliationEmployeeDTO) {
    this.patchValue(element);
    this.updateValueAndValidity();
  }
}
