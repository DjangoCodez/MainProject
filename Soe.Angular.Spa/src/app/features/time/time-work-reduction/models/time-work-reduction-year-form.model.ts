import { FormArray } from '@angular/forms';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ITimeWorkReductionReconciliationEmployeeDTO,
  ITimeWorkReductionReconciliationYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeWorkReductionYearEmployeeForm } from './time-work-reduction-year-employee-form.model';

interface ITimeWorkReductionYearForm {
  validationHandler: ValidationHandler;
  element: ITimeWorkReductionReconciliationYearDTO | undefined;
}

export class TimeWorkReductionYearForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeWorkReductionYearForm) {
    super(validationHandler, {
      timeWorkReductionReconciliationYearId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationYearId || 0,
        { isIdField: true }
      ),
      timeWorkReductionReconciliationId: new SoeTextFormControl(
        element?.timeWorkReductionReconciliationId || 0,
        {}
      ),
      stop: new SoeDateFormControl(
        element?.stop || '',
        {},
        'time.time.timeworkreduction.stopdate'
      ),

      employeeLastDecidedDate: new SoeDateFormControl(
        element?.employeeLastDecidedDate || '',
        {},
        'time.workreduction.employeeLastDecidedDate'
      ),

      pensionDepositPayrollProductId: new SoeSelectFormControl(
        element?.pensionDepositPayrollProductId || undefined,
        {},

        'time.workreduction.pensionDepositPayrollProduct'
      ),
      directPaymentPayrollProductId: new SoeSelectFormControl(
        element?.directPaymentPayrollProductId || undefined,
        {},
        'time.workreduction.directPaymentPayrollProduct'
      ),

      timeWorkReductionReconciliationEmployeeDTO: new FormArray([]),
      timeWorkReductionReconciliationYearDTO: new FormArray([]),
    });
  }

  set timeWorkReductionReconciliationYearDTO(formArray: FormArray) {
    this.controls.timeWorkReductionReconciliationYearDTO = formArray;
  }
  get timeWorkReductionReconciliationYearDTO(): FormArray {
    return <FormArray>this.controls.timeWorkReductionReconciliationYearDTO;
  }
  set timeWorkReductionReconciliationEmployeeDTO(formArray: FormArray) {
    this.controls.timeWorkReductionReconciliationEmployeeDTO = formArray;
  }
  get timeWorkReductionReconciliationEmployeeDTO(): FormArray {
    return <FormArray>this.controls.timeWorkReductionReconciliationEmployeeDTO;
  }
  customPatchValue(element: ITimeWorkReductionReconciliationYearDTO) {
    if (element.timeWorkReductionReconciliationEmployeeDTO != null) {
      this.patchYearEmployees(
        element.timeWorkReductionReconciliationEmployeeDTO
      );
    }

    this.patchValue(element);
  }

  patchYearEmployees(
    employees: ITimeWorkReductionReconciliationEmployeeDTO[]
  ): void {
    this.timeWorkReductionReconciliationEmployeeDTO.clear();
    for (const timeWorkAccountYearEmployee of employees) {
      const employee = new TimeWorkReductionYearEmployeeForm({
        validationHandler: undefined as any,
        element: timeWorkAccountYearEmployee,
      });
      this.timeWorkReductionReconciliationEmployeeDTO.push(employee);
    }
  }
}
