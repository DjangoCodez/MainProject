import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SearchAnnualLeaveTransactionModel } from './annual-leave-balance.model';

interface IAnnualLeaveBalanceFilterForm {
  validationHandler: ValidationHandler;
  element: SearchAnnualLeaveTransactionModel | undefined;
}
export class AnnualLeaveBalanceFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAnnualLeaveBalanceFilterForm) {
    super(validationHandler, {
      employeeIds: new SoeSelectFormControl(element?.employeeIds || []),
      dateFrom: new SoeDateFormControl(element?.dateFrom || new Date()),
      dateTo: new SoeDateFormControl(element?.dateTo || new Date()),
    });
  }

  get employeeIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeIds;
  }

  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }

  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }
}
