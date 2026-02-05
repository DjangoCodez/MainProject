import {
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions/soe-formgroup.extension';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { ICardNumberGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeeCardNumberForm {
  validationHandler: ValidationHandler;
  element: ICardNumberGridDTO | undefined;
}
export class EmployeeCardNumberForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmployeeCardNumberForm) {
    super(validationHandler, {
      employeeId: new SoeTextFormControl(element?.cardNumber || 0, {
        isIdField: true,
      }),
      cardNumber: new SoeTextFormControl(
        element?.employeeId || '',
        { },
        'time.employee.cardnumber.number'
      ),
      employeeName: new SoeTextFormControl(
        element?.employeeName || '',
        { isNameField: true },
        'time.employee.name'
      ),
      employeeNrSort: new SoeTextFormControl(element?.employeeNrSort || '', 
        { }
      ),
      employeeNumber: new SoeTextFormControl(
        element?.employeeNumber || '',
        { },
        'time.employee.employeenumber'
      ),
    });
  }

  get cardNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.cardNumber;
  }

  get employeeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeId;
  }

  get employeeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeName;
  }

  get employeeNrSort(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeNrSort;
  }
  get employeeNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeNumber;
  }
}
