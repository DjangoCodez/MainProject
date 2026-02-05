import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseSetPurchaseDateDTO } from './purchase.model';

interface IPurchaseSetPurchaseDateForm {
  validationHandler: ValidationHandler;
  element: PurchaseSetPurchaseDateDTO | undefined;
}
export class PurchaseSetPurchaseDateForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPurchaseSetPurchaseDateForm) {
    super(validationHandler, {
      purchaseDate: new SoeDateFormControl(element?.purchaseDate || undefined),
    });
  }

  get purchaseDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.purchaseDate;
  }
}
