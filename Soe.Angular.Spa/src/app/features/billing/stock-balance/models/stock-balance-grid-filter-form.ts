import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

interface IStockBalanceFilterForm {
  validationHandler: ValidationHandler;
  element: boolean;
}

export class StockBalanceFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStockBalanceFilterForm) {
    super(validationHandler, {
      showInactive: new SoeSelectFormControl(element || false),
    });
  }

  get showInactive() {
    return <SoeSelectFormControl>this.controls.showInactive;
  }
}
