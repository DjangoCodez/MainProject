import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export interface IAccountingPrio {
  accountingPrio1: number;
  accountingPrio2: number;
  accountingPrio3: number;
  accountingPrio4: number;
  accountingPrio5: number;
}
interface IAccountingPrioForm {
  validationHandler: ValidationHandler;
  element: IAccountingPrio | undefined;
}
export class AccountingPrioForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IAccountingPrioForm) {
    super(validationHandler, {
      accountingPrio1: new SoeSelectFormControl(0, {}, 'accountingPrio1'),
      accountingPrio2: new SoeSelectFormControl(0, {}, 'accountingPrio2'),
      accountingPrio3: new SoeSelectFormControl(0, {}, 'accountingPrio3'),
      accountingPrio4: new SoeSelectFormControl(0, {}, 'accountingPrio4'),
      accountingPrio5: new SoeSelectFormControl(0, {}, 'accountingPrio5'),
    });

    this.thisValidationHandler = validationHandler;
  }
  get accountingPrio1() {
    return <SoeSelectFormControl>this.controls.accountingPrio1;
  }
  get accountingPrio2() {
    return <SoeSelectFormControl>this.controls.accountingPrio2;
  }
  get accountingPrio3() {
    return <SoeSelectFormControl>this.controls.accountingPrio3;
  }
  get accountingPrio4() {
    return <SoeSelectFormControl>this.controls.accountingPrio4;
  }
  get accountingPrio5() {
    return <SoeSelectFormControl>this.controls.accountingPrio5;
  }
  get accountingPrio6() {
    return <SoeSelectFormControl>this.controls.accountingPrio6;
  }
}
