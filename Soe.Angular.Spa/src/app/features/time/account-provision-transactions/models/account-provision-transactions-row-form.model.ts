import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

interface IAccountProvisionTransactionsRowForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}
export class AccountProvisionTransactionsRowForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IAccountProvisionTransactionsRowForm) {
    super(validationHandler, {
      timePayrollTransactionId: new SoeNumberFormControl(
        element?.timePayrollTransactionId || 0,
        {},
        ''
      ),
      employeeId: new SoeNumberFormControl(element?.employeeId || 0, {}, ''),
      employeeNr: new SoeTextFormControl(element?.employeeNr || '', {}, ''),
      employeeFirstName: new SoeTextFormControl(
        element?.employeeFirstName || '',
        {},
        ''
      ),
      employeeLastName: new SoeTextFormControl(
        element?.employeeLastName || '',
        {},
        ''
      ),
      accountNr: new SoeTextFormControl(element?.accountNr || '', {}, ''),
      accountName: new SoeTextFormControl(element?.accountName || '', {}, ''),

      workTime: new SoeNumberFormControl(element?.workTime || 0, {}, ''),
      comment: new SoeTextFormControl(element?.comment || '', {}, ''),
      amount: new SoeNumberFormControl(
        element?.amount || 0,
        { decimals: 2 },
        ''
      ),
      quantity: new SoeNumberFormControl(
        element?.quantity || 0,
        { decimals: 2 },
        ''
      ),
      formulaPlain: new SoeTextFormControl(element?.formulaPlain || '', {}, ''),
      employmentStartDate: new SoeDateFormControl(
        element?.employmentStartDate || undefined,
        {},
        ''
      ),
      attestStateId: new SoeNumberFormControl(
        element?.attestStateId || 0,
        {},
        ''
      ),
      attestStateColor: new SoeTextFormControl(
        element?.attestStateColor || '',
        {},
        ''
      ),
    });
  }
}
